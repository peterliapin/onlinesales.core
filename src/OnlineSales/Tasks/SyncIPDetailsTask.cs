// <copyright file="SyncIPDetailsTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Entities;

namespace OnlineSales.Tasks;

public class SyncIPDetailsTask : ChangeLogTask
{
    private readonly TaskConfig? taskConfig = new TaskConfig();

    public SyncIPDetailsTask(IConfiguration configuration, ApiDbContext dbContext)
        : base(dbContext)
    {
        var config = configuration.GetSection("Tasks:SyncIPDetailsTask") !.Get<TaskConfig>();
        if (config is not null)
        {
            taskConfig = config;
        }
    }

    public override string Name => "SyncIPDetailsTask";

    public override string CronSchedule => taskConfig!.CronSchedule;

    public override int RetryCount => taskConfig!.RetryCount;

    public override int RetryInterval => taskConfig!.RetryInterval;

    internal override void ExecuteLogTask(List<ChangeLog> nextBatch)
    {
        foreach (var changeLogData in nextBatch.Select(b => b.Data))
        {
            if (string.IsNullOrEmpty(changeLogData))
            {
                continue;
            }

            var newIp = GetIpIfNotExist(changeLogData);

            if (!string.IsNullOrEmpty(newIp))
            {
                // TODO: Call external service and get IP related additional information.

                var ipDetails = new IpDetails()
                {
                    Ip = newIp,
                    // TODO: fill additional information here.
                };

                dbContext.IpDetails!.Add(ipDetails);
                dbContext.SaveChanges();
            }
        }
    }

    private string GetIpIfNotExist(string changeLogData)
    {
        var newIp = string.Empty;
        IpDetails? existingIpDetails;

        var settings = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        };

        var ipObject = JsonConvert.DeserializeObject<IpObject>(changeLogData!, settings) !;

        if (ipObject.EntityState == EntityState.Added)
        {
            existingIpDetails = dbContext.IpDetails!.FirstOrDefault(i => i.Ip == ipObject.CreatedByIp);
            if (existingIpDetails is null)
            {
                newIp = ipObject.CreatedByIp!;
            }
        }
        else if (ipObject.EntityState == EntityState.Modified)
        {
            existingIpDetails = dbContext.IpDetails!.FirstOrDefault(i => i.Ip == ipObject.UpdatedByIp);
            if (existingIpDetails is null)
            {
                newIp = ipObject.UpdatedByIp!;
            }
        }

        return newIp;
    }
}

public class IpObject
{
    public string? CreatedByIp { get; set; }

    public string? UpdatedByIp { get; set; }

    public EntityState EntityState { get; set; }

    public Dictionary<string, object>? AdditionalProperties { get; set; }
}
