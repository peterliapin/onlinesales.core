// <copyright file="TaskNotCompletedException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Exceptions;

[Serializable]
public class TaskNotCompletedException : Exception
{
    public TaskNotCompletedException()
        : base("Another task is not comleted yet")
    {
    }
}