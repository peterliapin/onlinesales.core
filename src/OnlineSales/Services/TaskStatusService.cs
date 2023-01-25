// <copyright file="TaskStatusService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using DnsClient;
using DnsClient.Protocol;
using HtmlAgilityPack;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Services;

public class TaskStatusService 
{
    private readonly Dictionary<string, bool> data = new Dictionary<string, bool>();

    private readonly bool valueOnStartUp;

    public TaskStatusService(IConfiguration configuration)
    {
        valueOnStartUp = configuration.GetValue<bool>("TaskRunner:Enable");
    }

    public bool IsRunning(string name)
    {
        if (data.ContainsKey(name))
        {
            return data[name];
        }
        else
        {
            return valueOnStartUp;
        }
    }

    public void SetRunning(string name, bool running)
    {
        if (data.ContainsKey(name))
        {
            data[name] = running;
        }
        else
        {
            data.Add(name, running);
        }        
    }
}

