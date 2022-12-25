// <copyright file="ErrorsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nest;

namespace OnlineSales.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorsController : Controller
{
    [Route("/error")]
    public IActionResult HandleError()
    {
        var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var error = exceptionHandlerFeature!.Error;

        ProblemDetails problemDetails;

        switch (error)
        {
            case InvalidModelStateException:
                problemDetails = ProblemDetailsFactory.CreateValidationProblemDetails(
                    HttpContext,
                    ((InvalidModelStateException)error).ModelState!,
                    StatusCodes.Status422UnprocessableEntity);

                break;

            case EntityNotFoundException:
                var entityNotFoundError = (EntityNotFoundException)error;

                problemDetails = ProblemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status404NotFound);

                problemDetails.Extensions["entityType"] = entityNotFoundError.EntityType;
                problemDetails.Extensions["entityUid"] = entityNotFoundError.EntityUid;

                break;

            default:
                problemDetails = ProblemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status500InternalServerError,
                    error.Message);

                break;
        }

        return new ObjectResult(problemDetails);
    }    
}