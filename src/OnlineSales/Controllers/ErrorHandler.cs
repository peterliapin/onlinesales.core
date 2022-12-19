// <copyright file="ErrorHandler.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Resources;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers
{    
    public class ErrorHandler
    {
        private readonly ControllerBase controller;

        public ErrorHandler(ControllerBase controller)
        {
            this.controller = controller;
        }

        public ActionResult CreateBadRequestResponce(string message = "")
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

            return controller.BadRequest(allErrors);
        }

        public ActionResult CreateNotFoundResponce(string message = "")
        {
            return controller.NotFound(new string[] { message });
        }

        public ActionResult CreateUnprocessableEntityResponce(string message = "")
        {
            return controller.UnprocessableEntity(new string[] { message });
        }

        public ActionResult CreateInternalServerErrorResponce(string message = "")
        {
            return controller.Problem(statusCode: StatusCodes.Status500InternalServerError, title: message);
        }
    }
}
