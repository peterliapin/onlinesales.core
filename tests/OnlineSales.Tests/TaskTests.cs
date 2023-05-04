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
        var responce = await this.GetRequest(this.tasksUrl);

        var content = await responce.Content.ReadAsStringAsync();

        var tasks = JsonHelper.Deserialize<IList<TaskDetailsDto>>(content);

        tasks.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByNameFailureTest()
    {
        await this.GetTest(this.tasksUrl + "/SomeUnexistedTask", HttpStatusCode.NotFound, "Success");
    }

    [Fact]
    public async Task GetByNameSuccesTest()
    {
        var name = "SyncEsTask";

        var responce = await this.GetTest<TaskDetailsDto>(this.tasksUrl + "/" + name);

        responce.Should().NotBeNull();
        responce!.Name.Should().Contain("SyncEsTask");
    }

    [Fact]
    public async Task StartAndStopTaskTest()
    {
        var name = "SyncEsTask";

        var responce = await this.GetTest<TaskDetailsDto>(this.tasksUrl + "/" + name);
        responce.Should().NotBeNull();
        responce!.IsRunning.Should().BeFalse();

        responce = await this.GetTest<TaskDetailsDto>(this.tasksUrl + "/start/" + name);
        responce.Should().NotBeNull();
        responce!.IsRunning.Should().BeTrue();

        responce = await this.GetTest<TaskDetailsDto>(this.tasksUrl + "/stop/" + name);
        responce.Should().NotBeNull();
        responce!.IsRunning.Should().BeFalse();
    }
}