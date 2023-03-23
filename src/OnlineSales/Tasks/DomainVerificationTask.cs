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
    protected readonly PgDbContext dbContext;

    private readonly ChangeLogTaskConfig taskConfig = new ChangeLogTaskConfig();

    private readonly IDomainService domainService;

    public DomainVerificationTask(PgDbContext dbContext, IConfiguration configuration, IDomainService domainService, TaskStatusService taskStatusService)
        : base(taskStatusService)
    {
        this.dbContext = dbContext;
        this.domainService = domainService;

        var section = configuration.GetSection("Tasks:DomainVerificationTask");        
        var config = section.Get<ChangeLogTaskConfig>();
        if (config is not null)
        {
            taskConfig = config;
        }
    }

    public override string CronSchedule => taskConfig.CronSchedule;

    public override int RetryCount => taskConfig.RetryCount;

    public override int RetryInterval => taskConfig.RetryInterval;

    public override async Task<bool> Execute(TaskExecutionLog currentJob)
    {
        try
        {
            var domains = dbContext.Domains!.Where(d => d.HttpCheck == null || d.DnsCheck == null /*|| d.MxCheck == null*/);
            int totalSize = domains.Count();

            for (int start = 0; start < totalSize; start += taskConfig.BatchSize)
            {
                domains.Skip(start).Take(taskConfig.BatchSize).AsParallel().ForAll(domain =>
                {
                    domainService.Verify(domain).Wait();
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
