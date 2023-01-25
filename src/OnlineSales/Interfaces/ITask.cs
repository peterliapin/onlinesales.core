// <copyright file="ITask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Entities;

namespace OnlineSales.Interfaces
{
    public interface ITask
    {
        public string Name { get; }

        /// <summary>
        /// Gets cron expression based schedule.
        /// </summary>
        public string CronSchedule { get; }

        public int RetryCount { get; }

        /// <summary>
        /// Gets retry interval in minutes.
        /// </summary>
        public int RetryInterval { get; }

        public bool IsRunning { get; }

        public void SetRunning(bool running);

        Task<bool> Execute(TaskExecutionLog currentJob);
    }
}