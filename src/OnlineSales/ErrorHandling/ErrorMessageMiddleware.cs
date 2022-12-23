// <copyright file="ErrorMessageMiddleware.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json;
using OnlineSales.ErrorHandling;

namespace OnlineSales.Controllers
{
    public class ErrorMessageMiddleware
    {
        private readonly IErrorMessageGenerator errorMessageGenerator;

        private readonly RequestDelegate next;

        public ErrorMessageMiddleware(RequestDelegate next, IErrorMessageGenerator errorMessageGenerator)
        {
            this.next = next;
            this.errorMessageGenerator = errorMessageGenerator;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originBody = context.Response.Body;

            var memStream = new MemoryStream();
            context.Response.Body = memStream;

            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                var em = errorMessageGenerator.CreateErrorMessage<InnerErrorCodes.Status500>(StatusCodes.Status500InternalServerError, InnerErrorCodes.Status500.ExceptionCaught, ex.GetType().Name);
                em.ErrorDescription = ex.Message;
                await WriteToStream(JsonSerializer.Serialize(em, IErrorMessageGenerator.ErrorHandlingSerializerOptions), originBody);
                context.Response.Body = originBody;
                return;
            }

            memStream.Position = 0;
            var responseBody = new StreamReader(memStream).ReadToEnd();
                        
            if (context.Response.StatusCode < 200 || context.Response.StatusCode >= 300)
            {
                try
                {
                    var em = GetErrorMessageFromBody(responseBody);
                    if (em == null)
                    {
                        var standartErrorMessage = new ErrorMessage();
                        standartErrorMessage.Status = context.Response.StatusCode;
                        standartErrorMessage.Code = InnerErrorCodes.UnspecifiedError.Code;
                        standartErrorMessage.Title = InnerErrorCodes.UnspecifiedError.Title;
                        standartErrorMessage.Arguments = null;
                        standartErrorMessage.Instance = null;
                        standartErrorMessage.Extensions.Clear();
                        standartErrorMessage.Type = null;
                        responseBody = JsonSerializer.Serialize(standartErrorMessage, IErrorMessageGenerator.ErrorHandlingSerializerOptions);
                    }
                }
                catch (Exception)
                {
                    // do nothing
                }                
            }

            await WriteToStream(responseBody, originBody);

            context.Response.Body = originBody;
        }

        private async Task WriteToStream(string data, Stream dest)
        {
            var memoryStreamModified = new MemoryStream();
            var sw = new StreamWriter(memoryStreamModified);
            sw.Write(data);
            sw.Flush();
            memoryStreamModified.Position = 0;

            await memoryStreamModified.CopyToAsync(dest).ConfigureAwait(false);
        }

        private ErrorMessage? GetErrorMessageFromBody(string data)
        {
            try
            {
                var em = JsonSerializer.Deserialize<ErrorMessage>(data, IErrorMessageGenerator.ErrorHandlingSerializerOptions);
                return em;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}