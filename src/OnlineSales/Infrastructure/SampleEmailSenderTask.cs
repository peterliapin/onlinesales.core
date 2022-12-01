// <copyright file="SampleEmailSenderTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.IO;
using System.Net.Http.Headers;
using Nest;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Infrastructure;

public class SampleEmailSenderTask : ITask
{
    private readonly IEmailService emailService;
    private readonly IEmailWithLogService emailWithLogService;
    private readonly IEmailFromTemplateService emailFromTemplateService;

    public SampleEmailSenderTask(IEmailService emailService, IEmailWithLogService emailWithLogService, IEmailFromTemplateService emailFromTemplateService)
    {
        this.emailService = emailService;
        this.emailWithLogService = emailWithLogService;
        this.emailFromTemplateService = emailFromTemplateService;
    }

    public string Name => "SampleEmailSenderTask";

    public string CronSchedule => "0 0/1 * * * ?";

    public int RetryCount => 1;

    public int RetryInterval => 1;

    public async Task<bool> Execute(TaskExecutionLog currentJob)
    {
        Console.WriteLine($"Sending emails by {Name}");

        /* IFormFile? file = null;
        List<IFormFile> files = new List<IFormFile>();

        var stream = new MemoryStream(File.ReadAllBytes("test.doc"));
        file = new FormFile(stream, 0, stream.Length, "test", "test.doc");
        files.Add(file); */

        var file = File.ReadAllBytes("test.doc");
        List<AttachmentDto> files = new List<AttachmentDto>();
        files.Add(new AttachmentDto { File = file, FileName = "test.doc" });

        await emailService.SendAsync("Generic Email Service", "abc@test.com", "Lakmal P", "gsplakmal@gmail.com", "test body1", null);

        await emailService.SendAsync("Generic Email Service with attachment", "abc@test.com", "Lakmal P", "gsplakmal@gmail.com", "test body1", files);

        await emailWithLogService.SendAsync("Email with log service", "abcd@test.com", "Lakmal S", "gsplakmal@gmail.com", "test body2", files);

        await emailWithLogService.SendToCustomerAsync(1, "Email with log service to customer", "abcd@test.com", "Lakmal S", "test body3", files);

        Dictionary<string, string> args = new Dictionary<string, string>();
        args.Add("<%replacement1%>", "testA");
        args.Add("<%replacement2%>", "testB");

        await emailFromTemplateService.SendAsync("testtemplate3", "gsplakmal@gmail.com", args, files);

        await emailFromTemplateService.SendToCustomerAsync(1, "testtemplate4", args, files);

        await emailFromTemplateService.SendToCustomerAsync(1, "testtemplate5", args, files, 1);

        return true;
    }
}
