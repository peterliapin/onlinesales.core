// <copyright file="IPDetailsDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json.Serialization;

namespace OnlineSales.DTOs
{
    public class IPDetailsDto
    {
        public string Ip { get; set; } = string.Empty;

        public string CountryName { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string ContinentName { get; set; } = string.Empty;

        public string Latitude { get; set; } = string.Empty;

        public string Longitude { get; set; } = string.Empty;
    }
}
