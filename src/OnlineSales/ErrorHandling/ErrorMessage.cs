// <copyright file="ErrorMessage.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.ErrorHandling
{
    public class ErrorMessage
    {
        public int Status { get; set; } = StatusCodes.Status200OK;

        public string Code { get; set; } = InnerErrorCodes.UnspecifiedError;

        public string Message { get; set; } = string.Empty;

        public string[] Arguments { get; set; } = new string[0];

        public List<string> Details { get; set; } = new List<string>();
    }
}
