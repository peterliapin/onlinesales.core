// <copyright file="ITask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Entities;

namespace OnlineSales.Interfaces
{
    public interface ITask
    {
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets cron expression based schedule.
        /// </summary>
        public string CronSchedule { get; set; }

        public int RetryCount { get; set; }

        /// <summary>
        /// Gets or sets retry interval in minutes.
        /// </summary>
        public int RetryInterval { get; set; }

        Task<bool> Execute(TaskExecutionLog currentJob);
    }
}
