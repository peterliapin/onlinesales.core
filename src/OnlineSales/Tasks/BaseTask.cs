// <copyright file="BaseTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Configuration;
using OnlineSales.Entities;
using OnlineSales.Interfaces;
using OnlineSales.Services;

namespace OnlineSales.Tasks;

public abstract class BaseTask : ITask
{
    protected readonly string configKey;

    private readonly TaskStatusService taskStatusService;

    protected BaseTask(string configKey, IConfiguration configuration, TaskStatusService taskStatusService)
    {
        this.configKey = configKey;
        this.taskStatusService = taskStatusService;

        var config = configuration.GetSection(configKey) !.Get<TaskConfig>();

        if (config is not null)
        {
            CronSchedule = config.CronSchedule;
            RetryCount = config.RetryCount;
            RetryInterval = config.RetryInterval;

            taskStatusService.SetInitialState(Name, config.Enable);
        }
        else
        {
            throw new MissingConfigurationException($"The specified configuration section for the provided configKey {configKey} could not be found in the settings file.");
        }
    }

    public string Name
    {
        get
        {
            return this.GetType().Name;
        }
    }

    public string CronSchedule { get; private set; }

    public int RetryCount { get; private set; }

    public int RetryInterval { get; private set; }

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
