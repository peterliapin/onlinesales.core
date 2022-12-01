// <copyright file="IEmailFromTemplateService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Interfaces;

public interface IEmailFromTemplateService
{
    Task SendAsync(string templateName, string recipient, Dictionary<string, string> templateArguments);

    Task SendToCustomerAsync(int customerId, string templateName, Dictionary<string, string> templateArguments, int scheduleId = 0);
}
