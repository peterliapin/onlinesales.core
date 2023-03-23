// <copyright file="SyncSuppressionsTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>
using System.Collections.Generic;
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
    private readonly string primaryApiKeyName = "PrimaryApiKey";

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
        await Unsubscribe<BlockOrBounceDto>("bounces");
        await Unsubscribe<BlockOrBounceDto>("blocks");
        await Unsubscribe<SpamReportDto>("spam_reports");
        await Unsubscribe<SuppressionDto>("unsubscribes");       
        return true;
    }

    private string GetSecondaryApiKeyName(int i)
    {
        return $"SecondaryApiKey{i}";
    }

    private async Task Unsubscribe<T>(string suppressionType)
        where T : SuppressionDto
    {
        var primaryKeyData = await GetLatestSuppressionsByType<T>(primaryApiKeyName, SendGridPlugin.Configuration.SendGridApi.PrimaryApiKey, suppressionType);

        foreach (var pkd in primaryKeyData)
        {
            await contactService.Unsubscribe(pkd.Email, pkd.GetReason(), $"SendGrid_{suppressionType}_{primaryApiKeyName}", pkd.CreatedAt, null);
        }

        for (int i = 0; i < SendGridPlugin.Configuration.SendGridApi.SecondaryApiKeys.Count; ++i)
        {
            var secondaryKeyData = await GetLatestSuppressionsByType<T>(GetSecondaryApiKeyName(i), SendGridPlugin.Configuration.SendGridApi.SecondaryApiKeys[i], suppressionType);

            foreach (var skd in secondaryKeyData)
            {
                await contactService.Unsubscribe(skd.Email, skd.GetReason(), $"SendGrid_{suppressionType}_{GetSecondaryApiKeyName(i)}", skd.CreatedAt, null);
            }
        }
    }

    private async Task<List<T>> GetLatestSuppressionsByType<T>(string apiKeyName, string apiKeyValue, string suppressionType)
    {
        var query = from u in dbContext.Unsubscribes
                    where u.Source == $"SendGrid_{suppressionType}_{apiKeyName}"
                    orderby u.CreatedAt descending
                    select u.CreatedAt;

        var lastCreateAt = query.FirstOrDefault();

        var lastSuppression = new SuppressionDto { CreatedAt = lastCreateAt };

        var sendGridClient = new SendGridClient(apiKeyValue);

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

