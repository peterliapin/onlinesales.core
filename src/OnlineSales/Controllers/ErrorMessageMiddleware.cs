// <copyright file="ErrorMessageMiddleware.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Buffers;
using System.Buffers.Text;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using AutoMapper;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OnlineSales.Data;
using OnlineSales.Entities;

namespace OnlineSales.Controllers
{
    public class ErrorMessageMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorMessageMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await next(context);
        }
    }
}