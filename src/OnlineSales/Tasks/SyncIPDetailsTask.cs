// <copyright file="SyncIPDetailsTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Configuration;
using OnlineSales.Entities;

namespace OnlineSales.Tasks;

public class SyncIPDetailsTask : ChangeLogTask
{
    private readonly TaskConfig? taskConfig = new TaskConfig();

    public SyncIPDetailsTask(IConfiguration configuration)
    {
        var config = configuration.GetSection("Tasks:SyncIPDetailsTask") !.Get<TaskConfig>();
        if (config is not null)
        {
            taskConfig = config;
        }
    }

    public override string Name => "SyncIPDetailsTask";

    public override string CronSchedule => taskConfig!.CronSchedule;

    public override int RetryCount => taskConfig!.RetryCount;

    public override int RetryInterval => taskConfig!.RetryInterval;

    internal override void ExecuteLogTask(List<ChangeLog> nextBatch)
    {
        // TODO: Check IP address is already available in IPDetails table.
        // If not call external API to get IP details and add an entry in IPDetails.
        throw new NotImplementedException();
    }
}
