// <copyright file="IpDetails.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineSales.Geography;

namespace OnlineSales.Entities;

[Table("ip_details")]
public class IpDetails
{
    [Key]
    public string Ip { get; set; } = string.Empty;

    public Continent ContinentCode { get; set; } = Continent.ZZ;

    public Country CountryCode { get; set; } = Country.ZZ;

    public string CityName { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}

