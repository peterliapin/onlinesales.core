// <copyright file="IErrorMessageGenerator.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineSales.ErrorHandling
{
    public interface IErrorMessageGenerator
    {
        public static readonly JsonSerializerOptions ErrorHandlingSerializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public ActionResult CreateBadRequestResponce(ControllerBase controller, ErrorDescription innerErrorMessage, params string[] arguments);

        public ActionResult CreateNotFoundResponce(ErrorDescription innerErrorMessage, params string[] arguments);

        public ActionResult CreateUnprocessableEntityResponce(ErrorDescription innerErrorMessage, params string[] arguments);

        public ErrorMessage CreateErrorMessage<T>(int status, ErrorDescription innerErrorMessage, params string[] arguments);
    }
}
