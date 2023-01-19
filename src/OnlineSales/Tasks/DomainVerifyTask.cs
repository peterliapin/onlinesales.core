// <copyright file="DomainVerifyTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System;
using System.Configuration;
using System.Reflection;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.OData.ModelBuilder;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Tasks;

public class DomainVerifyTask : ITask
{
    protected readonly ApiDbContext dbContext;

    private readonly DomainCheckTaskConfig taskConfig = new DomainCheckTaskConfig();

    private readonly IDomainVerifyService domainService;

    public DomainVerifyTask(ApiDbContext dbContext, IConfiguration configuration, IDomainVerifyService domainService)
    {
        this.dbContext = dbContext;
        this.domainService = domainService;

        var section = configuration.GetSection("Tasks:DomainCheckTask");        
        var config = section.Get<DomainCheckTaskConfig>();
        if (config is not null)
        {
            taskConfig = config;
        }
    }

    public string Name => this.GetType().Name;

    public string CronSchedule => taskConfig.CronSchedule;

    public int RetryCount => taskConfig.RetryCount;

    public int RetryInterval => taskConfig.RetryInterval;

    public async Task<bool> Execute(TaskExecutionLog currentJob)
    {
        try
        {
            var domainsToCheck = dbContext.Domains!.Where(d => d.HttpCheck == null || d.DnsCheck == null).Take(taskConfig.BatchSize);

            foreach (var domain in domainsToCheck)
            {
                if (domain.HttpCheck == null)
                {
                    await domainService.VerifyHttp(domain);
                }

                if (domain.DnsCheck == null)
                {
                    await domainService.VerifyDns(domain);
                }
            }

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error occurred when executing Domain Check task in task runner {currentJob.Id}");
            return false;
        }

        return true;
    }
}
