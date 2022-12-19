// <copyright file="ISwaggerConfigurator.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OnlineSales.Interfaces;

public interface ISwaggerConfigurator
{
    void ConfigureSwagger(SwaggerGenOptions options, OpenApiInfo settings);
}