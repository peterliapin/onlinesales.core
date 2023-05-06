// <copyright file="IpDetailsDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.DTOs
{
    public class IpDetailsDto
    {
        public string Ip { get; set; } = string.Empty;

        public string CountryName { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string ContinentName { get; set; } = string.Empty;

        public string Latitude { get; set; } = string.Empty;

        public string Longitude { get; set; } = string.Empty;

        public string ContinentCode { get; set; } = string.Empty;

        public string CountryCode2 { get; set; } = string.Empty;
    }
}