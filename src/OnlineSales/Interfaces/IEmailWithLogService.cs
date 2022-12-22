// <copyright file="IEmailWithLogService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.DTOs;

namespace OnlineSales.Interfaces;

public interface IEmailWithLogService
{
    Task SendAsync(string subject, string fromEmail, string fromName, string[] recipients, string body, List<AttachmentDto>? attachments, int templateId = 0);

    Task SendToCustomerAsync(int customerId, string subject, string fromEmail, string fromName, string body, List<AttachmentDto>? attachments, int scheduleId = 0, int templateId = 0);
}