// <copyright file="ErrorMessageGenerator.cs" company="WavePoint Co. Ltd.">
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
    public class ErrorMessageGenerator : IErrorMessageGenerator
    {
        private static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions
        {
        };

        private readonly ControllerBase controller;

        public ErrorMessageGenerator(ControllerBase controller)
        {
            this.controller = controller;
        }

        public ActionResult CreateBadRequestResponce((string, string) innerErrorMessage, params string[] arguments)
        {
            var allErrors = new List<string>();
            foreach (var keyModelStatePair in controller.ModelState)
            {
                var errors = keyModelStatePair.Value.Errors;
                if (errors != null && errors.Count > 0)
                {
                    foreach (var error in errors)
                    {
                        allErrors.Add(error.ErrorMessage);
                    }
                }
            }

            var em = new ErrorMessage()
            {
                Status = StatusCodes.Status400BadRequest,
                Code = "Status400." + innerErrorMessage.Item1,
                Message = CreateMessage(innerErrorMessage.Item2, arguments),
                Arguments = arguments,
                Details = allErrors,
            };

            return controller.BadRequest(JsonSerializer.Serialize(em, SerializeOptions));
        }

        public ActionResult CreateNotFoundResponce((string, string) innerErrorMessage, params string[] arguments)
        {
            var em = new ErrorMessage()
            {
                Status = StatusCodes.Status404NotFound,
                Code = "Status404." + innerErrorMessage.Item1,
                Message = CreateMessage(innerErrorMessage.Item2, arguments),
                Arguments = arguments,
            };

            return controller.NotFound(JsonSerializer.Serialize(em, SerializeOptions));
        }

        public ActionResult CreateUnprocessableEntityResponce((string, string) innerErrorMessage, params string[] arguments)
        {
            var em = new ErrorMessage()
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Code = "Status422." + innerErrorMessage.Item1,
                Message = CreateMessage(innerErrorMessage.Item2, arguments),
                Arguments = arguments,
            };

            return controller.UnprocessableEntity(JsonSerializer.Serialize(em, SerializeOptions));
        }

        private static string CreateMessage(string formattedMessage, params string[] arguments)
        {
            try
            {
                return string.Format(formattedMessage, arguments);
            }
            catch (Exception)
            {
                return string.Format(formattedMessage);
            }
        }
    }
}
