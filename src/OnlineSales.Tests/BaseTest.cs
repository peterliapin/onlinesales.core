// <copyright file="BaseTest.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace OnlineSales.Tests;

public class BaseTest : IDisposable
{
    protected static WebApplicationFactory<Program> application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                ConfigureBuilder(builder);
            });

    protected readonly HttpClient client;

    public BaseTest()
    {
        client = application.CreateClient();
    }

    public virtual void Dispose()
    {
        client.Dispose();
    }

    private static void ConfigureBuilder(IWebHostBuilder builder)
    {
        // nothing here yet   
    }
}

