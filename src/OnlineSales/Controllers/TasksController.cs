// <copyright file="TasksController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.DTOs;
using OnlineSales.Infrastructure;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IEnumerable<ITask> tasks;

    private readonly TaskRunner taskRunner;

    public TasksController(IEnumerable<ITask> tasks, TaskRunner taskRunner, IConfiguration configuration)
    {
        this.taskRunner = taskRunner;
        this.tasks = tasks;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public ActionResult Get()
    {
        return this.Ok(this.tasks.Select(t => this.CreateTaskDetailsDto(t)));
    }

    [HttpGet("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public TaskDetailsDto Get(string name)
    {
        var result = this.tasks.Where(t => t.Name == name);

        if (!result.Any())
        {
            throw new TaskNotFoundException(name);
        }
        else
        {
            return this.CreateTaskDetailsDto(result.First());
        }
    }

    [HttpGet("start/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public TaskDetailsDto Start(string name)
    {
        return this.StartOrStop(name, true);
    }

    [HttpGet("stop/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public TaskDetailsDto Stop(string name)
    {
        return this.StartOrStop(name, false);
    }

    [HttpGet("execute/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<TaskExecutionDto> Execute(string name)
    {
        var result = this.tasks.Where(t => t.Name == name);

        if (!result.Any())
        {
            throw new TaskNotFoundException(name);
        }
        else
        {
            var completed = await this.taskRunner.ExecuteTask(result.First());
            return new TaskExecutionDto
            {
                Name = name,
                Completed = completed,
            };
        }
    }

    private TaskDetailsDto StartOrStop(string name, bool start)
    {
        var result = this.tasks.Where(t => t.Name == name);

        if (!result.Any())
        {
            throw new TaskNotFoundException(name);
        }
        else
        {
            var task = result.First();
            this.taskRunner.StartOrStopTask(task, start);
            return this.CreateTaskDetailsDto(task);
        }
    }

    private TaskDetailsDto CreateTaskDetailsDto(ITask task)
    {
        return new TaskDetailsDto
        {
            Name = task.Name,
            CronSchedule = task.CronSchedule,
            RetryCount = task.RetryCount,
            RetryInterval = task.RetryInterval,
            IsRunning = task.IsRunning,
        };
    }
}

