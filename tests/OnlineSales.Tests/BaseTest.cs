// <copyright file="BaseTest.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;

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

    protected async Task<HttpResponseMessage> GetTest(string url, HttpStatusCode expectedCode = HttpStatusCode.OK)
    {
        var response = await Client.GetAsync(url);

        response.StatusCode.Should().Be(expectedCode);

        return response;
    }

    protected async Task<T?> GetTest<T>(string url, HttpStatusCode expectedCode = HttpStatusCode.OK)
        where T : class
    {
        var response = await GetTest(url, expectedCode);

        var content = await response.Content.ReadAsStringAsync();

        return DeserializePayload<T>(content);
    }

    protected async Task<HttpResponseMessage> UnsuccessfulPostTest(string url, object payload, HttpStatusCode expectedCode = HttpStatusCode.InternalServerError)
    {
        var content = PayloadToStringContent(payload);

        var response = await Client.PostAsync(url, content);

        response.StatusCode.Should().Be(expectedCode);

        return response;
    }

    protected async Task<string> PostTest(string url, object payload, HttpStatusCode expectedCode = HttpStatusCode.Created)
    {
        var content = PayloadToStringContent(payload);

        var response = await Client.PostAsync(url, content);

        response.StatusCode.Should().Be(expectedCode);

        var location = response.Headers?.Location?.LocalPath ?? string.Empty;

        location.Should().StartWith(url);

        return location;
    }

    protected async Task<HttpResponseMessage> PatchTest(string url, object payload, HttpStatusCode expectedCode = HttpStatusCode.OK)
    {
        var content = PayloadToStringContent(payload);

        var response = await Client.PatchAsync(url, content);

        response.StatusCode.Should().Be(expectedCode);

        return response;
    }

    protected async Task<HttpResponseMessage> DeleteTest(string url, HttpStatusCode expectedCode = HttpStatusCode.NoContent)
    {
        var response = await Client.DeleteAsync(url);

        response.StatusCode.Should().Be(expectedCode);

        return response;
    }
}
