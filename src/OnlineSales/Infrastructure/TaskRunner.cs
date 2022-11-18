// <copyright file="TaskRunner.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;
using Quartz;

namespace OnlineSales.Infrastructure
{
    public class TaskRunner : IJob
    {
        private readonly IEnumerable<ITask> tasks;
        private readonly ApiDbContext dbContext;

        public TaskRunner(IEnumerable<ITask> tasks, ApiDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.tasks = tasks;
        }

        public static TaskExecutionLog GetTestScheduleJob()
        {
            var job = new TaskExecutionLog()
            {
                Id = 1,

                ScheduledExecutionTime = DateTime.UtcNow,
            };

            return job;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Task runner started");

            foreach (var task in tasks)
            {
                var currentJob = await AddOrGetPendingTaskLog(task);

                if (!IsRightTimeToExecute(currentJob))
                {
                    return;
                }

                var isCompleted = await task.Execute(currentJob);

                UpdateTaskExecutionLog(currentJob, isCompleted ? TaskExecutionStatus.COMPLETED : TaskExecutionStatus.PENDING); 
            }
        }

        private async Task<TaskExecutionLog> AddOrGetPendingTaskLog(ITask task)
        {
            var pendingTask = await dbContext.TaskExecutionLogs!.
                FirstAsync(taskLog => taskLog.Status == TaskExecutionStatus.PENDING && taskLog.TaskName == task.Name);

            if (pendingTask is not null)
            {
                return pendingTask;
            }

            pendingTask = new TaskExecutionLog()
            {
                TaskName = task.Name,
                ScheduledExecutionTime = GetExecutionTimeByCronSchedule(task.CronSchedule),
                Status = TaskExecutionStatus.PENDING,
                RetryCount = 0,
            };

            await dbContext.TaskExecutionLogs!.AddAsync(pendingTask);
            await dbContext.SaveChangesAsync();

            return pendingTask;
        }

        private void UpdateTaskExecutionLog(TaskExecutionLog job, TaskExecutionStatus status)
        {
            // job status update
            job.Status = status;
            Console.WriteLine($"Task runner completed for job id {job.Id}");
        }

        private bool IsRightTimeToExecute(TaskExecutionLog job)
        {
            if (job.ScheduledExecutionTime <= DateTime.UtcNow)
            {
                return true;
            }

            return false;
        }

        private DateTime GetExecutionTimeByCronSchedule(string cronSchedule)
        {
            Debug.WriteLine($"{cronSchedule}");
            return DateTime.UtcNow;
        }
    }
}
