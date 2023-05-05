// <copyright file="InvalidModelStateException.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OnlineSales.Exceptions;

public class InvalidModelStateException : Exception
{
    public InvalidModelStateException(ModelStateDictionary modelState)
    {
        this.ModelState = modelState;
    }

    public ModelStateDictionary? ModelState { get; init; }
}

