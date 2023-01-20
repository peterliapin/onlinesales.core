// <copyright file="TasksController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.DTOs;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IEnumerable<ITask> tasks;
    private readonly IMapper mapper;

    public TasksController(IEnumerable<ITask> tasks, IMapper mapper)
    {
        this.tasks = tasks;
        this.mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public ActionResult Get()
    {
        return Ok(tasks.Select(t => t.Name));
    }

    [HttpGet("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public TaskDetailsDto Get(string name)
    {
        var result = tasks.Where(t => t.Name == name);

        if (!result.Any())
        {
            throw new TaskNotFoundException(name);
        }
        else
        {
            var resultConverted = mapper.Map<TaskDetailsDto>(result.FirstOrDefault());

            return resultConverted;
        }
    }
}

