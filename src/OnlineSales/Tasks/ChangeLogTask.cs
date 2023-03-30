// <copyright file="ChangeLogTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;
using OnlineSales.Services;

namespace OnlineSales.Tasks;

public abstract class ChangeLogTask : BaseTask
{    
    protected readonly PgDbContext dbContext;

    protected readonly IEnumerable<PluginDbContextBase> pluginDbContexts;

    private readonly HashSet<Type> loggedTypes;

    protected ChangeLogTask(string configKey, IConfiguration configuration, PgDbContext dbContext, IEnumerable<PluginDbContextBase> pluginDbContexts, TaskStatusService taskStatusService)
        : base(configKey, configuration, taskStatusService)
    {
        this.dbContext = dbContext;
        this.pluginDbContexts = pluginDbContexts;
        this.loggedTypes = GetTypes(dbContext);

        var config = configuration.GetSection(configKey) !.Get<ChangeLogTaskConfig>();

        if (config is not null)
        {
            ChangeLogBatchSize = config.BatchSize;
        }
        else
        {
            throw new MissingConfigurationException($"The specified configuration section for the provided configKey {configKey} could not be found in the settings file.");
        }

        foreach (var pt in pluginDbContexts)
        {
            var lt = GetTypes(pt);
            this.loggedTypes.UnionWith(lt);
        }
    }    

    public int ChangeLogBatchSize { get; private set; }

    public override Task<bool> Execute(TaskExecutionLog currentJob)
    {
        foreach (var typeName in loggedTypes.Select(type => type.Name))
        {
            var taskAndEntity = Name + "_" + typeName;

            if (IsPreviousTaskInProgress(taskAndEntity))
            {
                return Task.FromResult(true);
            }

            var changeLogBatch = GetNextOrFailedChangeLogBatch(taskAndEntity, typeName);

            if (changeLogBatch is not null && changeLogBatch!.Any())
            {
                var taskLog = AddChangeLogTaskLogRecord(taskAndEntity, changeLogBatch!.First().Id, changeLogBatch!.Last().Id);

                try
                {
                    ExecuteLogTask(changeLogBatch!);

                    UpdateChangeLogTaskLogRecord(taskLog, changeLogBatch!.Count, TaskExecutionState.Completed);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error occurred when executing task {Name}");

                    UpdateChangeLogTaskLogRecord(taskLog, 0, TaskExecutionState.Failed);
                    throw;
                }
            }
        }

        return Task.FromResult(true);
    }

    internal abstract void ExecuteLogTask(List<ChangeLog> nextBatch);

    protected HashSet<Type> GetTypes(PgDbContext context)
    {
        var res = new HashSet<Type>();

        var types = context.Model.GetEntityTypes();
        
        foreach (var type in types.Select(type => type.ClrType))
        {
            if (type != null && IsChangeLogAttribute(type) && IsTypeSupported(type))
            {
                res.Add(type);
            }
        }

        return res;
    }

    protected abstract bool IsTypeSupported(Type type);

    private bool IsChangeLogAttribute(Type type)
    {
        return type.GetCustomAttributes<SupportsChangeLogAttribute>().Any();
    }

    private bool IsPreviousTaskInProgress(string name)
    {
        var inProgressCount = dbContext.ChangeLogTaskLogs!.Count(c => c.TaskName == name && c.State == TaskExecutionState.InProgress);

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

        dbContext.ChangeLogTaskLogs!.Add(changeLogTaskLogEntry);
        dbContext.SaveChanges();

        return changeLogTaskLogEntry;
    }

    private List<ChangeLog> GetNextOrFailedChangeLogBatch(string taskName, string entity)
    {
        var minLogId = 1;

        var lastProcessedTask = dbContext.ChangeLogTaskLogs!.Where(c => c.TaskName == taskName).OrderByDescending(t => t.Id).FirstOrDefault();

        if (lastProcessedTask is not null && lastProcessedTask.State == TaskExecutionState.Failed)
        {
            var failedTaskCount = dbContext.ChangeLogTaskLogs!.Count(c => c.TaskName == taskName && c.ChangeLogIdMin == lastProcessedTask.ChangeLogIdMin);
            if (failedTaskCount > 0 && failedTaskCount <= RetryCount)
            {
                // If this is a retry, get the same minId of last processed task to re-execute the same batch.
                minLogId = lastProcessedTask.ChangeLogIdMin;
            }
            else
            {
                // If all retries are completed then discontinue.
                Log.Error($"Error in executing task {taskName} for entity {entity} from Id {lastProcessedTask.ChangeLogIdMin} to {lastProcessedTask.ChangeLogIdMax}");

                return Enumerable.Empty<ChangeLog>().ToList();
            }
        }
        else if (lastProcessedTask is not null && lastProcessedTask.State == TaskExecutionState.Completed)
        {
            minLogId = lastProcessedTask.ChangeLogIdMax + 1;
        }

        // 3000000 - tests show that limit of 3 millions records is optimal to split change_log table into the parts with acceptable performance
        var changeLogList = dbContext.ChangeLogs!.Where(c => c.Id >= minLogId && c.Id < minLogId + 3000000 && c.ObjectType == entity).OrderBy(b => b.Id).Take(ChangeLogBatchSize).ToList();

        return changeLogList;
    }
}
