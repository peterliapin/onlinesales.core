// <copyright file="TaskDtos.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration.Attributes;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OnlineSales.Entities;

namespace OnlineSales.DTOs;
public class TaskDetailsDto
{
    public string Name { get; set; } = string.Empty;

    public string CronSchedule { get; set; } = string.Empty;

    public int RetryCount { get; set; }

    public int RetryInterval { get; set; }

    public bool IsRunning { get; set; }
}

public class TaskExecutionDto
{
    public string Name { get; set; } = string.Empty;

    public bool Completed { get; set; } = false;
}