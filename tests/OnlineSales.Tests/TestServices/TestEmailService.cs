// <copyright file="TestEmailService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlineSales.DTOs;
using OnlineSales.Interfaces;

namespace OnlineSales.Tests.TestServices
{
    public class TestEmailService : IEmailService
    {
        public Task SendAsync(string subject, string fromEmail, string fromName, string[] recipients, string body, List<AttachmentDto>? attachments)
        {
            // Test email sender method returns success.
            return Task.CompletedTask;
        }
    }
}
