// <copyright file="TaskExecutionLog.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Entities
{
    public enum TaskExecutionStatus
    {
        PENDING = 0,
        COMPLETED = 1,
    }

    public class TaskExecutionLog : BaseEntity
    {
        public string? TaskName { get; set; }

        public DateTime ScheduledExecutionTime { get; set; }

        public DateTime ActualExecutionTime { get; set; }

        public TaskExecutionStatus Status { get; set; }

        public int RetryCount { get; set; }

        public string? Comment { get; set; }
    }
}
