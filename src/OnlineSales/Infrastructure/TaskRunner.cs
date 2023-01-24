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
                    Log.Information("This is not the current primary node for task execution");
                    return;
                }                    

                foreach (var task in tasks.Where(t => t.IsRunning))
                {
                    var taskLock = LockManager.GetNoWaitLock(task.Name);

                    if (taskLock is null)
                    {
                        Log.Error($"Skipping the task {task.Name} as the previous run is not completed yet.");
                        continue;
                    }

                    using (taskLock)
                    {
                        var currentJob = await AddOrGetPendingTaskExecutionLog(task);

                        if (IsRightTimeToExecute(currentJob, task))
                        {
                            var isCompleted = await task.Execute(currentJob);

                            await UpdateTaskExecutionLog(currentJob, isCompleted ? TaskExecutionStatus.Completed : TaskExecutionStatus.Pending);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error executing task runner");
            }
        }             

        public async Task<bool> ExecuteTask(ITask task)
        {
            if (!CheckPrimaryNode())
            {
                throw new NonPrimaryNodeException();
            }

            var taskLock = LockManager.GetNoWaitLock(task.Name);

            if (taskLock is null)
            {
                throw new TaskNotCompletedException();
            }

            using (taskLock)
            {
                var currentJob = await AddOrGetPendingTaskExecutionLog(task);

                var isCompleted = await task.Execute(currentJob);

                await UpdateTaskExecutionLog(currentJob, isCompleted ? TaskExecutionStatus.Completed : TaskExecutionStatus.Pending);

                return isCompleted;
            }
        }

        public void StartOrStopTask(ITask task, bool start)
        {
            if (!CheckPrimaryNode())
            {
                throw new NonPrimaryNodeException();
            }

            task.SetRunning(start);
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

        private bool IsRightTimeToExecute(TaskExecutionLog job, ITask task)
        {
            if (job.RetryCount >= task.RetryCount)
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