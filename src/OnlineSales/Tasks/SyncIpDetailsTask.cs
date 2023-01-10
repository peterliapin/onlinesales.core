// <copyright file="SyncIpDetailsTask.cs" company="WavePoint Co. Ltd.">
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

public class SyncIpDetailsTask : ChangeLogTask
{
    private readonly TaskConfig? taskConfig = new TaskConfig();
    private readonly IOptions<GeolocationApiConfig> options;
    private readonly IpDetailsService ipDetailsService;

    public SyncIpDetailsTask(IConfiguration configuration, ApiDbContext dbContext, IOptions<GeolocationApiConfig> options, IpDetailsService ipDetailsService)
        : base(dbContext, configuration)
    {
        var config = configuration.GetSection("Tasks:SyncIPDetailsTask") !.Get<TaskConfig>();
        if (config is not null)
        {
            taskConfig = config;
        }

        this.options = options;
        this.ipDetailsService = ipDetailsService;
    }

    public override string Name => "SyncIPDetailsTask";

    public override string CronSchedule => taskConfig!.CronSchedule;

    public override int RetryCount => taskConfig!.RetryCount;

    public override int RetryInterval => taskConfig!.RetryInterval;

    public override string[] Entities => new[] { "Customer", "Post", "EmailGroup", "EmailLog", "Order", "OrderItem" };

    internal override void ExecuteLogTask(List<ChangeLog> nextBatch)
    {
        List<IpDetails> ipDetailsCollection = new ();

        List<string> ipList = GetDistinctIps(nextBatch);

        List<string> newIpCollection = GetNewIps(ipList!);

        foreach (var ipItem in newIpCollection)
        {
            if (string.IsNullOrEmpty(ipItem))
            {
                continue;
            }

            var geoIpDetails = ipDetailsService.GetIPDetail(ipItem).Result;

            if (geoIpDetails == null)
            {
                Log.Information("Ip {0} does not have any information", ipItem);
                continue;
            }

            var ipDetails = new IpDetails()
            {
                Ip = ipItem,
                CityName = geoIpDetails!.City,
                CountryCode = Enum.TryParse<Country>(geoIpDetails!.CountryCode2, out var countryCode) ? countryCode : Country.ZZ,
                ContinentCode = Enum.TryParse<Continent>(geoIpDetails!.ContinentCode, out var continentCode) ? continentCode : Continent.ZZ,
                Latitude = double.TryParse(geoIpDetails!.Latitude, out double resultLatitiude) ? resultLatitiude : 0,
                Longitude = double.TryParse(geoIpDetails!.Longitude, out double resultLongitude) ? resultLongitude : 0,
            };

            ipDetailsCollection.Add(ipDetails); 
        }

        if (ipDetailsCollection.Any())
        {
            dbContext.IpDetails!.AddRange(ipDetailsCollection);
            dbContext.SaveChanges(); 
        }
    }

    private List<string> GetDistinctIps(List<ChangeLog> changeLogs)
    {
        List<string> distinctIps;

        var ipObjects = (from cL in changeLogs select JsonSerializer.Deserialize<IpObject>(cL.Data)).ToList();

        var resultedIps = (from ip in ipObjects where !string.IsNullOrWhiteSpace(ip.CreatedByIp!) select ip.CreatedByIp).Union(from ip in ipObjects where !string.IsNullOrWhiteSpace(ip.UpdatedByIp!) select ip.UpdatedByIp).ToList();

        distinctIps = resultedIps.Distinct().ToList();

        return distinctIps;
    }

    private List<string> GetNewIps(List<string> ips)
    {
        var dbResults = (from i in ips
                 join di in dbContext.IpDetails! on i equals di.Ip into ps
                 from di in ps.DefaultIfEmpty()
                 select new { NewIp = i, Ip = di?.Ip ?? string.Empty }).ToList();

        List<string> newIps = (from dr in dbResults where dr.Ip == string.Empty select dr.NewIp).ToList();

        return newIps;
    }
}

public class IpObject
{
    public string? CreatedByIp { get; set; }

    public string? UpdatedByIp { get; set; }
}
