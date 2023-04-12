// <copyright file="ContactAccountTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Interfaces;
using OnlineSales.Services;

namespace OnlineSales.Tasks;

public class ContactAccountTask : BaseTask
{
    private const string ConfigKey = "Tasks:ContactAccountTask";

    protected readonly PgDbContext dbContext;

    private readonly IAccountExternalService accountExternalService;

    private readonly IMapper mapper;

    private readonly int batchSize;

    public ContactAccountTask(IConfiguration configuration, TaskStatusService taskStatusService, PgDbContext dbContext, IAccountExternalService accountExternalService, IMapper mapper)
        : base(ConfigKey, configuration, taskStatusService)
    {
        this.dbContext = dbContext;
        this.accountExternalService = accountExternalService;
        this.mapper = mapper;

        var config = configuration.GetSection(ConfigKey) !.Get<TaskWithBatchConfig>();

        if (config is not null)
        {
            batchSize = config.BatchSize;
        }
        else
        {
            throw new MissingConfigurationException($"The specified configuration section for the provided ConfigKey {ConfigKey} could not be found in the settings file.");
        }
    }

    public override async Task<bool> Execute(TaskExecutionLog currentJob)
    {
        try   
        {
            var contactsToHandle = dbContext.Contacts !.Where(c => c.AccountId == null);

            int totalSize = contactsToHandle.Count();  
            for (int start = 0; start < totalSize; start += batchSize)
            {
                var batch = contactsToHandle.Skip(start).Take(batchSize);
                var domainIds = batch.Select(c => c.DomainId).ToHashSet();
                var domains = dbContext.Domains!.Where(d => domainIds.Contains(d.Id)).ToList();
                var domainAndContacts = batch.GroupBy(c => c.DomainId).ToDictionary(g => domains.FirstOrDefault(d => d.Id == g.Key) !, g => g.ToList());

                var newlyCreatedAccounts = new HashSet<Account>();
                foreach (var dc in domainAndContacts)
                {
                    if (dc.Key.AccountStatus == AccountSyncStatus.NotInitialized)
                    {
                        foreach (var contact in dc.Value)
                        {
                            await SetContactAccountFromDomainAccount(contact, dc.Key, newlyCreatedAccounts);
                        }
                    }
                }

                await dbContext.Accounts !.AddRangeAsync(newlyCreatedAccounts);
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error occurred when executing Domain Check task in task runner {currentJob.Id}");
            return false;
        }

        return true;
    }

    private async Task SetContactAccountFromDomainAccount(Contact contact, Domain domain, HashSet<Account> newAccounts)
    {
        try
        {
            if (domain != null)
            {
                if (domain.AccountId != null)
                {
                    contact.AccountId = domain.AccountId;
                }
                else if (domain.Account != null)
                {
                    contact.Account = domain.Account;
                }
                else
                {
                    if (domain.Free.HasValue && !domain.Free.Value && domain.Disposable.HasValue && !domain.Disposable.Value)
                    {
                        var accInfo = await accountExternalService.GetAccountDetails(domain.Name);
                        if (accInfo == null)
                        {
                            accInfo = new AccountDetailsInfo() { Name = domain.Name };
                            domain.AccountStatus = AccountSyncStatus.Failed;
                        }
                        else
                        {
                            domain.AccountStatus = AccountSyncStatus.Successful;
                        }

                        var existingAccount = dbContext.Accounts!.Where(a => a.Name == accInfo.Name).FirstOrDefault();
                        if (existingAccount == null)
                        {
                            existingAccount = newAccounts.FirstOrDefault(a => a.Name == accInfo.Name);
                        }

                        if (existingAccount != null)
                        {
                            contact.Account = existingAccount;
                            domain.Account = existingAccount;
                        }
                        else
                        {
                            var account = mapper.Map<Account>(accInfo);
                            newAccounts.Add(account);
                            contact.Account = account;
                            domain.Account = account;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Error("Cannot set Account for Contact in ContactAccountTask. Contact email: " + contact.Email + ". Reason: " + e.Message);
        }
    }
}
