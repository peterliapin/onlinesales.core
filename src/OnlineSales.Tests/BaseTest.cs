// <copyright file="BaseTest.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net;
using System.Text;
using System.Text.Json;

namespace OnlineSales.Tests;

public class BaseTest : IDisposable
{
    protected static readonly TestApplication App = new TestApplication();
    protected readonly HttpClient Client;

    public BaseTest()
    {
        Client = App.CreateClient();
        App.CleanDatabase();
    }

    public virtual void Dispose()
    {
        Client.Dispose();
    }

    protected async Task<HttpResponseMessage> GetTest(string url, HttpStatusCode expectedCode = HttpStatusCode.OK)
    {
        var response = await Client.GetAsync(url);

        Assert.Equal(expectedCode, response.StatusCode);

        return response;
    }

    protected async Task<HttpResponseMessage> PostTest(string url, object payload, HttpStatusCode expectedCode = HttpStatusCode.OK)
    {
        var payloadString = JsonSerializer.Serialize(payload);

        var content = new StringContent(payloadString, Encoding.UTF8, "application/json");

        var response = await Client.PostAsync(url, content);

        Assert.Equal(expectedCode, response.StatusCode);

        return response;
    }
}
