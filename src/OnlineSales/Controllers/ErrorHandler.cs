// <copyright file="ErrorHandler.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

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
            return controller.BadRequest(controller.ModelState);
        }

        public ActionResult CreateNotFoundResponce(string message = "")
        {
            return controller.NotFound(message);
        }

        public ActionResult CreateUnprocessableEntityResponce(string message = "")
        {
            return controller.UnprocessableEntity(message);
        }

        public ActionResult CreateInternalServerErrorResponce(string message = "")
        {
            return controller.Problem(statusCode: StatusCodes.Status500InternalServerError, title: message);
        }
    }
}
