// <copyright file="IErrorMessageGenerator.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel;
using System.Reflection;
using System.Resources;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OnlineSales.Interfaces;
using YamlDotNet.Core.Tokens;

namespace OnlineSales.ErrorHandling
{    
    public interface IErrorMessageGenerator
    {
        public ActionResult CreateBadRequestResponce(ControllerBase controller, (string, string) innerErrorMessage, params string[] arguments);

        public ActionResult CreateNotFoundResponce(ControllerBase controller, (string, string) innerErrorMessage, params string[] arguments);

        public ActionResult CreateUnprocessableEntityResponce(ControllerBase controller, (string, string) innerErrorMessage, params string[] arguments);
    }
}
