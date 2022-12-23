// <copyright file="ErrorMessageGenerator.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Mvc;

namespace OnlineSales.ErrorHandling
{    
    public class ErrorMessageGenerator : IErrorMessageGenerator
    {
        public ActionResult CreateBadRequestResponce(ControllerBase controller, ErrorDescription innerErrorMessage, params string[] arguments)
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

            var em = CreateErrorMessage<InnerErrorCodes.Status400>(StatusCodes.Status400BadRequest, innerErrorMessage, arguments);
            em.ErrorDescription = allErrors;

            return CreateResult(em);
        }

        public ActionResult CreateNotFoundResponce(ErrorDescription innerErrorMessage, params string[] arguments)
        {
            var em = CreateErrorMessage<InnerErrorCodes.Status404>(StatusCodes.Status404NotFound, innerErrorMessage, arguments);

            return CreateResult(em);
        }

        public ActionResult CreateUnprocessableEntityResponce(ErrorDescription innerErrorMessage, params string[] arguments)
        {
            var em = CreateErrorMessage<InnerErrorCodes.Status422>(StatusCodes.Status422UnprocessableEntity, innerErrorMessage, arguments);

            return CreateResult(em);
        }

        public ErrorMessage CreateErrorMessage<T>(int status, ErrorDescription innerErrorMessage, params string[] arguments)
        {
            var em = new ErrorMessage();
            em.Status = status;
            em.Code = typeof(T).Name + "." + innerErrorMessage.Code;
            em.Status = status;
            em.Title = innerErrorMessage.Title;
            if (innerErrorMessage.Details != null)
            {
                em.Detail = CreateDetail(innerErrorMessage.Details, arguments);
                if (arguments.Length > 0)
                {
                    em.Arguments = arguments.ToList();
                }
            }

            return em;
        }

        private static string CreateDetail(string formattedMessage, params string[] arguments)
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

        private JsonResult CreateResult(ErrorMessage em)
        {
            return new JsonResult(em, IErrorMessageGenerator.ErrorHandlingSerializerOptions);
        }
    }
}
