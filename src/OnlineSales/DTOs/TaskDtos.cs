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
    public string Name { get; } = string.Empty;

    public string CronSchedule { get; } = string.Empty;

    public int RetryCount { get; }

    public int RetryInterval { get; }
}