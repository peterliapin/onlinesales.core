// <copyright file="CoreTaskScheduler.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Infrastructure
{
    public class CoreTaskScheduler : ITask
    {
        private readonly DbContext dbContext;

        public CoreTaskScheduler(ApiDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public string Name { get => "Core Scheduler"; set => throw new NotImplementedException(); }

        public string CronSchedule { get => "* * * * * ?"; set => throw new NotImplementedException(); }

        public int RetryCount { get => 0; set => throw new NotImplementedException(); }

        public int RetryInterval { get => 60; set => throw new NotImplementedException(); }

        public Task<bool> Execute(TaskExecutionLog currentJob)
        {
            Console.WriteLine($"Executing {Name}");

            return Task.FromResult(true);
        }
    }
}
