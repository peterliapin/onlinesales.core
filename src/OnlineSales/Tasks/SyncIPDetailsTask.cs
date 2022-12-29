// <copyright file="SyncIPDetailsTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Geography;
using OnlineSales.Services;

namespace OnlineSales.Tasks;

public class SyncIPDetailsTask : ChangeLogTask
{
    private readonly TaskConfig? taskConfig = new TaskConfig();
    private readonly IOptions<GeolocationApiConfig> options;

    public SyncIPDetailsTask(IConfiguration configuration, ApiDbContext dbContext, IOptions<GeolocationApiConfig> options)
        : base(dbContext)
    {
        var config = configuration.GetSection("Tasks:SyncIPDetailsTask") !.Get<TaskConfig>();
        if (config is not null)
        {
            taskConfig = config;
        }

        this.options = options;
    }

    public override string Name => "SyncIPDetailsTask";

    public override string CronSchedule => taskConfig!.CronSchedule;

    public override int RetryCount => taskConfig!.RetryCount;

    public override int RetryInterval => taskConfig!.RetryInterval;

    internal override void ExecuteLogTask(List<ChangeLog> nextBatch)
    {
        foreach (var changeLogData in nextBatch)
        {
            if (string.IsNullOrEmpty(changeLogData.Data))
            {
                continue;
            }

            var newIp = GetIpIfNotExist(changeLogData);

            if (!string.IsNullOrEmpty(newIp))
            {
                var geoIpDetails = new IpDetailsService(options).GetIPDetail(newIp).Result;

                if (geoIpDetails == null)
                {
                    Log.Information("Ip {0} does not have any information", newIp);
                    continue;
                }

                var ipDetails = new IpDetails()
                {
                    Ip = newIp,
                    CityName = geoIpDetails!.City,
                    CountryCode = Enum.TryParse<Country>(geoIpDetails!.CountryCode2, out var countryCode) ? countryCode : Country.ZZ,
                    ContinentCode = Enum.TryParse<Continent>(geoIpDetails!.ContinentCode, out var continentCode) ? continentCode : Continent.ZZ,
                    Latitude = double.TryParse(geoIpDetails!.Latitude, out double resultLatitiude) ? resultLatitiude : 0,
                    Longitude = double.TryParse(geoIpDetails!.Longitude, out double resultLongitude) ? resultLongitude : 0,
                };

                dbContext.IpDetails!.Add(ipDetails);
                dbContext.SaveChanges();
            }
        }
    }

    private string GetIpIfNotExist(ChangeLog changeLogData)
    {
        var newIp = string.Empty;
        IpDetails? existingIpDetails;

        var ipObject = JsonSerializer.Deserialize<IpObject>(changeLogData.Data!) !;

        if (changeLogData.EntityState == EntityState.Added)
        {
            existingIpDetails = dbContext.IpDetails!.FirstOrDefault(i => i.Ip == ipObject.CreatedByIp);
            if (existingIpDetails is null)
            {
                newIp = ipObject.CreatedByIp!;
            }
        }
        else if (changeLogData.EntityState == EntityState.Modified)
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
}
