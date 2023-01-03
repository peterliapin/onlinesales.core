// <copyright file="BaseTest.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.OData.UriParser;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public class BaseTest : IDisposable
{
    protected static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    };

    protected static readonly TestApplication App = new TestApplication();

    protected readonly HttpClient Client;

    static BaseTest()
    {
        SerializeOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public BaseTest()
    {
        Client = App.CreateClient();
        App.CleanDatabase();
    }

    public virtual void Dispose()
    {
        Client.Dispose();
    }

    protected static string SerializePayload(object payload)
    {
        return JsonSerializer.Serialize(payload, SerializeOptions);
    }

    protected static T? DeserializePayload<T>(string content)
        where T : class
    {
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }
        else
        {
            var result = JsonSerializer.Deserialize<T>(content, SerializeOptions);

            return result;
        }
    }

    protected static StringContent PayloadToStringContent(object payload)
    {
        var payloadString = SerializePayload(payload);

        return new StringContent(payloadString, Encoding.UTF8, "application/json");
    }

    protected Task<HttpResponseMessage> GetRequest(string url, string authToken = "Success")
    {
        return Request(HttpMethod.Get, url, null, authToken);
    }

    protected Task<HttpResponseMessage> Request(HttpMethod method, string url, object? payload, string authToken = "Success")
    {
        var request = new HttpRequestMessage(method, url);

        if (payload != null)
        {
            request.Content = PayloadToStringContent(payload);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        return Client.SendAsync(request);
    }

    protected async Task<HttpResponseMessage> GetTest(string url, HttpStatusCode expectedCode = HttpStatusCode.OK, string authToken = "Success")
    {
        var response = await GetRequest(url, authToken);

        response.StatusCode.Should().Be(expectedCode);

        return response;
    }

    protected async Task<T?> GetTest<T>(string url, HttpStatusCode expectedCode = HttpStatusCode.OK, string authToken = "Success")
        where T : class
    {
        var response = await GetTest(url, expectedCode, authToken);

        var content = await response.Content.ReadAsStringAsync();

        if (expectedCode == HttpStatusCode.OK)
        {
            return DeserializePayload<T>(content);
        }
        else
        {
            return null;
        }
    }

    protected async Task<string> PostTest(string url, object payload, HttpStatusCode expectedCode = HttpStatusCode.Created, string authToken = "Success")
    {        
        var response = await Request(HttpMethod.Post, url, payload, authToken);

        return CheckPostResponce(url, response, expectedCode);
    }

    protected async Task<HttpResponseMessage> Patch(string url, object payload, string authToken = "Success")
    {
        var response = await Request(HttpMethod.Patch, url, payload, authToken);
        return response;
    }

    protected async Task<HttpResponseMessage> PatchTest(string url, object payload, HttpStatusCode expectedCode = HttpStatusCode.OK, string authToken = "Success")
    {
        var response = await Patch(url, payload, authToken);

        response.StatusCode.Should().Be(expectedCode);

        return response;
    }

    protected async Task<HttpResponseMessage> DeleteTest(string url, HttpStatusCode expectedCode = HttpStatusCode.NoContent, string authToken = "Success")
    {
        var response = await Request(HttpMethod.Delete, url, null, authToken);

        response.StatusCode.Should().Be(expectedCode);

        return response;
    }

    protected string CheckPostResponce(string url, HttpResponseMessage response, HttpStatusCode expectedCode)
    {
        var location = string.Empty;

        if (expectedCode == HttpStatusCode.Created)
        {
            location = response.Headers?.Location?.LocalPath ?? string.Empty;
            location.Should().StartWith(url);
        }

        return location;
    }

    protected void SaveBulkRecords(dynamic bulkItems)
    {
        App.PopulateBulkData(bulkItems);
    }
}