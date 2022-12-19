// <copyright file="IHttpContextHelper.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Interfaces
{
    public interface IHttpContextHelper
    {
        public HttpRequest Request { get; }

        public string? IpAddress { get; }

        public string? UserAgent { get; }
    }
}