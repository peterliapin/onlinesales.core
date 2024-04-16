// <copyright file="NonPrimaryNodeException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Exceptions;

[Serializable]
public class NonPrimaryNodeException : Exception
{
    public NonPrimaryNodeException()
        : base("This is not the current primary node for task execution")
    {
    }
}