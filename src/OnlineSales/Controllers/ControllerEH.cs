// <copyright file="ControllerEH.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Mvc;

namespace OnlineSales.Controllers
{
    public class ControllerEH : Controller
    {
        protected readonly ErrorHandler errorHandler;

        public ControllerEH()
            : base()
        {
            errorHandler = new ErrorHandler(this);
        }
    }
}