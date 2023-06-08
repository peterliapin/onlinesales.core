// <copyright file="DealPipelineStageException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Exceptions;

public class DealPipelineStageException : Exception
{
    public DealPipelineStageException(string message)
        : base(message)
    {        
    }
}