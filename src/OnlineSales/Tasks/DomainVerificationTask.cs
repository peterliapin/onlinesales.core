// <copyright file="DomainVerificationTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;
using OnlineSales.Services;

namespace OnlineSales.Tasks;

public class DomainVerificationTask : BaseTask
{
    private const string ConfigKey = "Tasks:DomainVerificationTask";

    protected readonly PgDbContext dbContext;
    private readonly IDomainService domainService;
    private readonly int batchSize;
    private readonly int batchInterval;

    public DomainVerificationTask(PgDbContext dbContext, IConfiguration configuration, IDomainService domainService, TaskStatusService taskStatusService)
        : base(ConfigKey, configuration, taskStatusService)
    {
        this.dbContext = dbContext;
        this.domainService = domainService;

        var config = configuration.GetSection(ConfigKey)!.Get<DomainVerificationTaskConfig>();

        if (config is not null)
        {
            batchSize = config.BatchSize;
            batchInterval = config.BatchInterval;
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
            var domains = dbContext.Domains!.Where(d => d.HttpCheck == null || d.DnsCheck == null /*|| d.MxCheck == null*/);
            var totalSize = domains.Count();

            for (var start = 0; start < totalSize; start += batchSize)
            {
                domains.Skip(start).Take(batchSize)/*.AsParallel()*/.ToList().ForEach(domain =>
                {
                    domainService.Verify(domain).Wait();
                    Thread.Sleep(new TimeSpan(0, 0, batchInterval));
                });

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
}