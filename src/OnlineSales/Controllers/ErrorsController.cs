// <copyright file="ErrorsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OnlineSales.Controllers;

[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorsController : Controller
{
    [Route("/error")]
    public IActionResult HandleError()
    {
        var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var error = exceptionHandlerFeature!.Error;

        ProblemDetails problemDetails;

        Log.Error(error, $"Exception catched by the error controller.");

        switch (error)
        {
            case InvalidModelStateException exception:
                problemDetails = ProblemDetailsFactory.CreateValidationProblemDetails(
                    HttpContext,
                    exception.ModelState!,
                    StatusCodes.Status422UnprocessableEntity);

                break;

            case TaskNotFoundException taskNotFoundException:

                problemDetails = ProblemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status404NotFound);

                problemDetails.Extensions["taskName"] = taskNotFoundException.TaskName;

                break;

            case EntityNotFoundException entityNotFoundError:

                problemDetails = ProblemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status404NotFound);

                problemDetails.Extensions["entityType"] = entityNotFoundError.EntityType;
                problemDetails.Extensions["entityUid"] = entityNotFoundError.EntityUid;

                break;
            case QueryException queryException:
                problemDetails = ProblemDetailsFactory.CreateProblemDetails(
                HttpContext,
                StatusCodes.Status400BadRequest);
                queryException.FailedCommands.ForEach(cmd =>
                {
                    problemDetails.Extensions[cmd.Key] = cmd.Value;
                });
                break;

            case DbUpdateException dbUpdateException:
                var dbError = dbUpdateException.InnerException ?? dbUpdateException;

                problemDetails = ProblemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status422UnprocessableEntity,
                    dbError.Message);

                break;
            case IdentityException identityException:
                problemDetails = ProblemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status400BadRequest,
                    identityException.ErrorMessage);
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