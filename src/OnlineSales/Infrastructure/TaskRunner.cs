// <copyright file="TaskRunner.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;
using Quartz;

namespace OnlineSales.Infrastructure
{
    public class TaskRunner : IJob
    {
        private const string TaskRunnerNodeLockKey = "TaskRunnerPrimaryNodeLock";

        private readonly IEnumerable<ITask> tasks;
        private readonly ApiDbContext dbContext;

        public TaskRunner(IEnumerable<ITask> tasks, ApiDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.tasks = tasks;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (!CheckPrimaryNode())
                {
                    return;
                }                    

                foreach (var task in tasks)
                {
                    await ExecuteTask(task, false);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error executing task runner");
            }
        }             

        public async Task ExecuteTask(ITask task)
        {
            if (!CheckPrimaryNode())
            {
                throw new NonPrimaryNodeException();
            }

            await ExecuteTask(task, true);
        }

        private bool CheckPrimaryNode()
        {
            var nodeLockInstance = LockManager.GetInstanceWithNoWaitLock(TaskRunnerNodeLockKey);

            if (nodeLockInstance is null)
            {
                Log.Information("This is not the current primary node for task execution");
                return false;
            }

            return true;
        }

        private async Task ExecuteTask(ITask task, bool onDemand)
        {
            var taskLock = LockManager.GetNoWaitLock(task.Name);

            if (taskLock is null)
            {
                if (onDemand)
                {
                    throw new TaskNotCompletedException();
                }
                else
                {
                    Log.Error($"Skipping the task {task.Name} as the previous run is not completed yet.");
                    return;
                }
            }

            using (taskLock)
            {
                var currentJob = await AddOrGetPendingTaskExecutionLog(task);

                if (IsRightTimeToExecute(currentJob, task, onDemand))
                {
                    var isCompleted = await task.Execute(currentJob);

                    await UpdateTaskExecutionLog(currentJob, isCompleted ? TaskExecutionStatus.Completed : TaskExecutionStatus.Pending);
                }
            }
        }

        private async Task<TaskExecutionLog> AddOrGetPendingTaskExecutionLog(ITask task)
        {
            var pendingTask = await dbContext.TaskExecutionLogs!.
                FirstOrDefaultAsync(taskLog => taskLog.Status == TaskExecutionStatus.Pending && taskLog.TaskName == task.Name);

            if (pendingTask is not null)
            {
                return pendingTask;
            }

            pendingTask = new TaskExecutionLog()
            {
                TaskName = task.Name,
                ScheduledExecutionTime = GetExecutionTimeByCronSchedule(task.CronSchedule, DateTime.UtcNow),
                Status = TaskExecutionStatus.Pending,
                RetryCount = null,
            };

            await dbContext.TaskExecutionLogs!.AddAsync(pendingTask);
            await dbContext.SaveChangesAsync();

            return pendingTask;
        }

        private async Task UpdateTaskExecutionLog(TaskExecutionLog job, TaskExecutionStatus status)
        {
            job.Status = status;
            job.ActualExecutionTime = DateTime.UtcNow;

            if (status == TaskExecutionStatus.Pending)
            {
                job.RetryCount = ++job.RetryCount;
            }

            dbContext!.TaskExecutionLogs!.Update(job);
            await dbContext.SaveChangesAsync();
        }

        private bool IsRightTimeToExecute(TaskExecutionLog job, ITask task, bool onDemand)
        {
            if (onDemand)
            {
                return true;
            }

            if (!task.IsRunning || job.RetryCount >= task.RetryCount)
            {
                return false;
            }

            if (job.RetryCount > 0)
            {
                return job.ActualExecutionTime.AddMinutes(task.RetryInterval) <= DateTime.UtcNow;
            }

            return job.ScheduledExecutionTime <= DateTime.UtcNow;
        }

        private DateTime GetExecutionTimeByCronSchedule(string cronSchedule, DateTime baseExecutionTime)
        {
            var expression = new CronExpression(cronSchedule);

            var nextRunTime = expression.GetNextValidTimeAfter(baseExecutionTime);

            return nextRunTime!.Value.UtcDateTime;
        }
    }
}