// <copyright file="ControllerEH.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using OnlineSales.Data;
using OnlineSales.Entities;

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