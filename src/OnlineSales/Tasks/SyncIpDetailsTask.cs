// <copyright file="SyncIpDetailsTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Geography;
using OnlineSales.Helpers;
using OnlineSales.Services;

namespace OnlineSales.Tasks;

public class SyncIpDetailsTask : ChangeLogTask
{
    private readonly IpDetailsService ipDetailsService;

    public SyncIpDetailsTask(IConfiguration configuration, PgDbContext dbContext, IEnumerable<PluginDbContextBase> pluginDbContexts, IpDetailsService ipDetailsService, TaskStatusService taskStatusService)
        : base("Tasks:SyncIpDetailsTask", configuration, dbContext, pluginDbContexts, taskStatusService)
    {
        this.ipDetailsService = ipDetailsService;
    }

    internal override void ExecuteLogTask(List<ChangeLog> nextBatch)
    {
        var ipDetailsCollection = new List<IpDetails>();
        var ipList = GetDistinctIps(nextBatch);
        var newIpCollection = GetNewIps(ipList!);

        foreach (var ip in newIpCollection)
        {
            if (string.IsNullOrEmpty(ip))
            {
                continue;
            }

            var geoIpDetails = ipDetailsService.GetIpDetails(ip).Result;

            if (geoIpDetails == null)
            {
                Log.Information("Ip {0} does not have any information", ip);
                continue;
            }

            var ipDetails = new IpDetails()
            {
                Ip = ip,
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

    protected override bool IsTypeSupported(Type type)
    {
        return typeof(IHasCreatedBy).IsAssignableFrom(type) || typeof(IHasUpdatedBy).IsAssignableFrom(type);
    }

    private List<string> GetDistinctIps(List<ChangeLog> changeLogs)
    {
        List<string> distinctIps;

        var ipObjects = (from cL in changeLogs select JsonHelper.Deserialize<IpObject>(cL.Data)).ToList();

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

        var newIps = (from dr in dbResults where dr.Ip == string.Empty select dr.NewIp).ToList();

        return newIps;
    }
}

public class IpObject
{
    public string? CreatedByIp { get; set; }

    public string? UpdatedByIp { get; set; }
}
