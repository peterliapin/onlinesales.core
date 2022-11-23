// <copyright file="IEmailService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string subject, string fromEmail, string fromName, string recipients, string body, List<IFormFile>? attachments);
    }
}
