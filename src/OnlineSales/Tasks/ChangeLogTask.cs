// <copyright file="ChangeLogTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Tasks;

public abstract class ChangeLogTask : ITask
{
    protected readonly ApiDbContext dbContext;

    protected ChangeLogTask(ApiDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public virtual int LogTaskRetryCount { get; set; } = 0;

    public virtual int ChangeLogBatchSize { get; set; } = 50;

    public abstract string Name { get; }

    public abstract string CronSchedule { get; }

    public abstract int RetryCount { get; }

    public abstract int RetryInterval { get; }

    public Task<bool> Execute(TaskExecutionLog currentJob)
    {
        if (IsPreviousTaskInProgress(Name))
        {
            return Task.FromResult(true);
        }

        var changeLogBatch = GetNextOrFailedChangeLogBatch(Name);

        if (changeLogBatch is not null)
        {
            var taskLog = AddChangeLogTaskLogRecord(Name, changeLogBatch.First().Id, changeLogBatch.Last().Id);

            try
            {
                ExecuteLogTask(changeLogBatch);

                UpdateChangeLogTaskLogRecord(taskLog, changeLogBatch.Count, TaskExecutionState.Completed);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error occurred when executing task {Name}");

                UpdateChangeLogTaskLogRecord(taskLog, 0, TaskExecutionState.Failed);
                throw;
            }
        }

        return Task.FromResult(true);
    }

    internal abstract void ExecuteLogTask(List<ChangeLog> nextBatch);

    private bool IsPreviousTaskInProgress(string name)
    {
        var inProgressCount = dbContext.ChangeLogTaskLog!.Where(c => c.TaskName == name && c.State == TaskExecutionState.InProgress).Count();

        return inProgressCount > 0;
    }

    private void UpdateChangeLogTaskLogRecord(ChangeLogTaskLog taskLog, int changesProcessed, TaskExecutionState state)
    {
        taskLog.ChangesProcessed = changesProcessed;
        taskLog.State = state;
        taskLog.End = DateTime.UtcNow;

        dbContext.SaveChanges();
    }

    private ChangeLogTaskLog AddChangeLogTaskLogRecord(string taskName, int minLogId, int maxLogId)
    {
        var changeLogTaskLogEntry = new ChangeLogTaskLog()
        {
            TaskName = taskName,
            Start = DateTime.UtcNow,
            State = TaskExecutionState.InProgress,
            ChangeLogIdMin = minLogId,
            ChangeLogIdMax = maxLogId,
        };

        dbContext.ChangeLogTaskLog!.Add(changeLogTaskLogEntry);
        dbContext.SaveChanges();

        return changeLogTaskLogEntry;
    }

    private List<ChangeLog> GetNextOrFailedChangeLogBatch(string taskName)
    {
        int minLogId = 1;

        var lastProcessedTask = dbContext.ChangeLogTaskLog!.Where(c => c.TaskName == taskName).OrderByDescending(t => t.Id).FirstOrDefault();

        if (lastProcessedTask is not null && lastProcessedTask.State == TaskExecutionState.Failed)
        {
            var failedTaskCount = dbContext.ChangeLogTaskLog!.Where(c => c.TaskName == taskName && c.ChangeLogIdMin == lastProcessedTask.ChangeLogIdMin).Count();
            if (failedTaskCount > 0 && failedTaskCount <= LogTaskRetryCount)
            {
                // If this is a retry, get the same minId of last processed task to re-execute the same batch.
                minLogId = lastProcessedTask.ChangeLogIdMin;
            }
            else
            {
                // If all retries are completed get the next batch.
                minLogId = lastProcessedTask.ChangeLogIdMax + 1;
            }
        }
        else if (lastProcessedTask is not null && lastProcessedTask.State == TaskExecutionState.Completed)
        {
            minLogId = lastProcessedTask.ChangeLogIdMax + 1;
        }

        var changeLogList = dbContext.ChangeLog!.Where(c => c.Id >= minLogId && c.Id < minLogId + ChangeLogBatchSize).OrderBy(b => b.Id).ToList();

        return changeLogList;
    }
}
