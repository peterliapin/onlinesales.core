// <copyright file="ChangeLogTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Tasks;

public abstract class ChangeLogTask : ITask
{
    public virtual int LogTaskRetryCount { get; set; } = 0;

    public abstract string Name { get; }

    public abstract string CronSchedule { get; }

    public abstract int RetryCount { get; }

    public abstract int RetryInterval { get; }

    public Task<bool> Execute(TaskExecutionLog currentJob)
    {
        var changeLogBatch = GetNextOrFailedChangeLogBatch(Name);

        if (changeLogBatch is not null)
        {
            var taskLog = AddChangeLogTaskLogRecord(Name);

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

    private void UpdateChangeLogTaskLogRecord(ChangeLogTaskLog taskLog, int changesProcessed, TaskExecutionState state)
    {
        // TODO: Update ChangeLogTaskLog record with the state and change count.
        throw new NotImplementedException();
    }

    private ChangeLogTaskLog AddChangeLogTaskLogRecord(string taskName)
    {
        // TODO: Add a new ChangeLogTaskLog record and return.
        throw new NotImplementedException();
    }

    private List<ChangeLog> GetNextOrFailedChangeLogBatch(string taskName)
    {
        // TODO: Get any "state = Failed" batch or get next batch to execute.
        // Handle LogTaskRetryCount logic also here. If retrycount exceeded then get the next batch without retrying.
        throw new NotImplementedException();
    }
}
