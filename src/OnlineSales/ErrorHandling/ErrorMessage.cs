// <copyright file="ErrorMessage.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineSales.ErrorHandling
{
    public class ErrorMessage : ProblemDetails
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = InnerErrorCodes.UnspecifiedError.Code;

        [JsonPropertyName("details_arguments")]
        public List<string>? Arguments { get; set; } = null;

        [JsonPropertyName("error_description")]
        public object? ErrorDescription { get; set; } = null;
    }
}
