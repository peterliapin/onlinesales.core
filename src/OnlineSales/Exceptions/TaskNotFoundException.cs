// <copyright file="TaskNotFoundException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Runtime.Serialization;

namespace OnlineSales.Exceptions;

[Serializable]
public class TaskNotFoundException : Exception
{
    public TaskNotFoundException(string taskName)
    {
        TaskName = taskName;
    }

    public string TaskName { get; }
}

