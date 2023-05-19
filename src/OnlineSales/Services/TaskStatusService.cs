// <copyright file="TaskStatusService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Services;

public class TaskStatusService
{
    private readonly Dictionary<string, bool> taskStatusByName = new Dictionary<string, bool>();

    public void SetInitialState(string name, bool running)
    {
        if (!taskStatusByName.ContainsKey(name))
        {
            taskStatusByName[name] = running;
        }
    }

    public bool IsRunning(string name)
    {
        if (taskStatusByName.ContainsKey(name))
        {
            return taskStatusByName[name];
        }
        else
        {
            return false;
        }
    }

    public void SetRunning(string name, bool running)
    {
        taskStatusByName[name] = running;
    }
}