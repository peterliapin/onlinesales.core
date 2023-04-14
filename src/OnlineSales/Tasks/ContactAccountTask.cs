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
            var domainsToHandle = dbContext.Domains!.Where(d => d.AccountStatus == AccountSyncStatus.NotInitialized);
            int totalSize = domainsToHandle.Count();
            for (int start = 0; start < totalSize; start += batchSize)
            {
                var batch = domainsToHandle.Skip(start).Take(batchSize).ToList();
                await SetDomainsAccounts(batch);
                var domainIdDictionary = batch.ToDictionary(d => d.Id, d => d);
                var contacts = dbContext.Contacts!.Where(c => domainIdDictionary.Keys.Contains(c.DomainId));
                foreach (var c in contacts)
                {
                    c.AccountId = null;
                    c.Account = domainIdDictionary[c.DomainId].Account;
                }

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

    private async Task SetDomainsAccounts(List<Domain> domains)
    {
        var newAccounts = new HashSet<Account>();
        foreach (var domain in domains)
        {
            try
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
                        domain.Account = existingAccount;
                    }
                    else
                    {
                        var account = mapper.Map<Account>(accInfo);
                        newAccounts.Add(account);
                        domain.Account = account;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Cannot set Account for Domain in ContactAccountTask. Domain name: " + domain.Name + ". Reason: " + e.Message);
            }
        }

        await dbContext.Accounts!.AddRangeAsync(newAccounts);
    }
}
