// <copyright file="BaseTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System;
using System.Reflection;
using Microsoft.AspNetCore.Components.Web;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;
using OnlineSales.Interfaces;
using OnlineSales.Services;

namespace OnlineSales.Tasks;

public abstract class BaseTask : ITask
{
    private readonly TaskStatusService taskStatusService;

    protected BaseTask(TaskStatusService taskStatusService)
    {
        this.taskStatusService = taskStatusService;
    }

    public string Name
    {
        get
        {
            return this.GetType().Name;
        }
    }

    public abstract string CronSchedule { get; }

    public abstract int RetryCount { get; }

    public abstract int RetryInterval { get; }

    public bool IsRunning
    {
        get
        {
            return taskStatusService.IsRunning(Name);
        }
    }

    public void SetRunning(bool running)
    {
        taskStatusService.SetRunning(Name, running);
    }

    public abstract Task<bool> Execute(TaskExecutionLog currentJob);
}
