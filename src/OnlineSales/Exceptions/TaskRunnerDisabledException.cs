// <copyright file="TaskRunnerDisabledException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Exceptions;

[Serializable]
public class TaskRunnerDisabledException : Exception
{
    public TaskRunnerDisabledException()
        : base("Task Runner is not enabled")
    {
    }
}