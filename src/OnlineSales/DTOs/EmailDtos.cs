// <copyright file="EmailDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineSales.Helpers;

namespace OnlineSales.DTOs;
public class EmailVerifyDetailsDto : DomainDetailsDto
{
    public string EmailAddress { get; set; } = string.Empty;
}

public class EmailVerifyInfoDto
{
    public string EmailAddress { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonHelper.BooleanConverter))]
    public bool FreeCheck { get; set; }

    [JsonConverter(typeof(JsonHelper.BooleanConverter))]
    public bool DisposableCheck { get; set; }

    [JsonConverter(typeof(JsonHelper.BooleanConverter))]
    public bool CatchAllCheck { get; set; }
}