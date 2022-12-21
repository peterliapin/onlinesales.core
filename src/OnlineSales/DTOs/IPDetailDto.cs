// <copyright file="IPDetailDto.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json.Serialization;

namespace OnlineSales.DTOs
{
    public class IPDetailDto
    {
        [JsonPropertyName("ip")]
        public string IP { get; set; } = string.Empty;

        [JsonPropertyName("country_name")]
        public string Country { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("continent_name")]
        public string Continent { get; set; } = string.Empty;

        [JsonPropertyName("latitude")]
        public string Latitude { get; set; } = string.Empty;

        [JsonPropertyName("longitude")]
        public string Longitude { get; set; } = string.Empty;
    }
}
