// <copyright file="DomainTaskTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Security.Policy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData.UriParser;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Helpers;
using OnlineSales.Interfaces;
using OnlineSales.Tasks;

namespace OnlineSales.Tests;

public class DomainTaskTests : BaseTest
{   
    private readonly string tasksUrl = "api/tasks";

    [Fact]
    public async Task GetAllTasksTest()
    {
        var responce = await GetRequest(tasksUrl);

        var content = await responce.Content.ReadAsStringAsync();

        var tasks = JsonHelper.Deserialize<IList<string>>(content);

        tasks.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByNameFailureTest()
    {
        await GetTest(tasksUrl + "/SomeUnexistedTask", HttpStatusCode.NotFound, "Success");
    }

    [Fact]
    public async Task GetByNameSuccesTest()
    {
        var name = "SyncEsTask";

        var responce = await GetRequest(tasksUrl + "/" + name);

        var content = await responce.Content.ReadAsStringAsync();

        var task = JsonHelper.Deserialize<TaskDetailsDto>(content);

        task.Should().NotBeNull();
        task!.Name.Should().Contain("SyncEsTask");
    }
}