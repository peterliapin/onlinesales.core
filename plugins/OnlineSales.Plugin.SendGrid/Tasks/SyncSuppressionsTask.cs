// <copyright file="SyncSuppressionsTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using Microsoft.Extensions.Configuration;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Helpers;
using OnlineSales.Interfaces;
using OnlineSales.Plugin.SendGrid.DTOs;
using OnlineSales.Plugin.SendGrid.Exceptions;
using OnlineSales.Services;
using OnlineSales.Tasks;
using SendGrid;

namespace OnlineSales.Plugin.SendGrid.Tasks;

public class SyncSuppressionsTask : BaseTask
{
    protected readonly PgDbContext dbContext;
    protected readonly IContactService contactService;

    private readonly TaskConfig taskConfig = new TaskConfig();

    public SyncSuppressionsTask(TaskStatusService taskStatusService, PgDbContext dbContext, IContactService contactService, IConfiguration configuration)
        : base(taskStatusService)
    {
        this.dbContext = dbContext;
        this.contactService = contactService;

        var section = configuration.GetSection("Tasks:SyncSuppressionsTask");

        var config = section.Get<TaskConfig>();

        if (config is not null)
        {
            taskConfig = config;
        }
    }

    public override string CronSchedule => taskConfig.CronSchedule;

    public override int RetryCount => taskConfig.RetryCount;

    public override int RetryInterval => taskConfig.RetryInterval;

    public override async Task<bool> Execute(TaskExecutionLog currentJob)
    {
        var bounces = await GetLatestSuppressionsByType<BlockOrBounceDto>("bounces");

        foreach (var bounce in bounces)
        {
            await contactService.Unsubscribe(bounce.Email, $"{bounce.Status} - {bounce.Reason}", "SendGrid_bounces", bounce.CreatedAt, null);
        }

        var blocks = await GetLatestSuppressionsByType<BlockOrBounceDto>("blocks");

        foreach (var block in blocks)
        {
            await contactService.Unsubscribe(block.Email, $"{block.Status} - {block.Reason}", "SendGrid_blocks", block.CreatedAt, null);
        }

        var spamReports = await GetLatestSuppressionsByType<SpamReportDto>("spam_reports");

        foreach (var spamReport in spamReports)
        {
            await contactService.Unsubscribe(spamReport.Email, "Reported As Spam", "SendGrid_spam_reports", spamReport.CreatedAt, spamReport.Ip);
        }        

        return true;
    }

    private async Task<List<T>> GetLatestSuppressionsByType<T>(string suppressionType)
        where T : SuppressionDto
    {
        var query = from u in dbContext.Unsubscribes
                    where u.Source == $"SendGrid_{suppressionType}"
                    orderby u.CreatedAt descending
                    select u.CreatedAt;

        var lastCreateAt = query.FirstOrDefault();

        var lastSuppression = new SuppressionDto { CreatedAt = lastCreateAt };

        var apiKey = SendGridPlugin.Configuration.SendGridApi.ApiKey;

        var sendGridClient = new SendGridClient(apiKey);

        var response = await sendGridClient.RequestAsync(
            method: SendGridClient.Method.GET,
            // It is OK to only get 500 records even if there are more. We will recieve the rest next time task is being executed.
            // We may consider to implement more advanced version when it recieve all the data in scope of one task execution later.
            urlPath: $"suppression/{suppressionType}?limit=500&offset=0&start_time={lastSuppression.Created + 1}"); // +1 is required to avoid syncing the same last record multiple times

        var statusCode = response.StatusCode;
        var result = await response.Body.ReadAsStringAsync();

        if (statusCode != System.Net.HttpStatusCode.OK)
        {
            throw new SendGridApiException("Invalid result returned from SendGrid API: " + result);
        }

        return JsonHelper.Deserialize<List<T>>(result) !;
    }
}

