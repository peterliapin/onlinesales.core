// <copyright file="ControllerBaseEH.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Mvc;

namespace OnlineSales.Controllers;

public class ControllerBaseEH : ControllerBase
{
    protected readonly ErrorHandler errorHandler;

    public ControllerBaseEH()
        : base()
    {
        errorHandler = new ErrorHandler(this);
    }
}