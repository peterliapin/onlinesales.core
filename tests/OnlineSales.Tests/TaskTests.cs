// <copyright file="TaskTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Helpers;

namespace OnlineSales.Tests;

public class TaskTests : BaseTest
{   
    private readonly string tasksUrl = "api/tasks";

    [Fact]
    public async Task GetAllTasksTest()
    {
        var responce = await GetRequest(tasksUrl);

        var content = await responce.Content.ReadAsStringAsync();

        var tasks = JsonHelper.Deserialize<IList<TaskDetailsDto>>(content);

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

        var responce = await GetTest<TaskDetailsDto>(tasksUrl + "/" + name);

        responce.Should().NotBeNull();
        responce!.Name.Should().Contain("SyncEsTask");
    }

    [Fact]
    public async Task StartAndStopTaskTest()
    {
        var name = "SyncEsTask";

        var responce = await GetTest<TaskDetailsDto>(tasksUrl + "/" + name);
        responce.Should().NotBeNull();
        responce!.IsRunning.Should().BeFalse();

        responce = await GetTest<TaskDetailsDto>(tasksUrl + "/start/" + name);
        responce.Should().NotBeNull();
        responce!.IsRunning.Should().BeTrue();

        responce = await GetTest<TaskDetailsDto>(tasksUrl + "/stop/" + name);
        responce.Should().NotBeNull();
        responce!.IsRunning.Should().BeFalse();
    }
}