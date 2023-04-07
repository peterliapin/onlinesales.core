// <copyright file="BaseTest.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Globalization;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using OnlineSales.Controllers;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Helpers;

namespace OnlineSales.Tests;

public class BaseTest : IDisposable
{
    protected static readonly TestApplication App = new TestApplication();

    protected readonly HttpClient client;
    protected readonly IMapper mapper;

    static BaseTest()
    {
        AssertionOptions.AssertEquivalencyUsing(e => e.Using(new RelaxedEnumEquivalencyStep()));
    }

    public BaseTest()
    {
        client = App.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });

        mapper = App.GetMapper();
        App.CleanDatabase();
    }

    public virtual void Dispose()
    {
        client.Dispose();
    }

    protected static StringContent PayloadToStringContent(object payload)
    {
        var payloadString = JsonHelper.Serialize(payload);

        return new StringContent(payloadString, Encoding.UTF8, "application/json");
    }

    protected async Task SyncElasticSearch()
    {
        var taskExecuteResponce = await GetRequest("/api/tasks/execute/SyncEsTask");
        taskExecuteResponce.Should().NotBeNull();
        taskExecuteResponce.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await taskExecuteResponce.Content.ReadAsStringAsync();
        var task = JsonHelper.Deserialize<TaskExecutionDto>(content);
        task!.Completed.Should().BeTrue();
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

        return client.SendAsync(request);
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
            CheckForRedundantProperties(content);

            return JsonHelper.Deserialize<T>(content);
        }
        else
        {
            return null;
        }
    }

    protected async Task<List<TI>?> GetTestCSV<TI>(string url, HttpStatusCode expectedCode = HttpStatusCode.OK, string authToken = "Success")
    where TI : class
    {
        var response = await GetTest(url, expectedCode, authToken);

        var content = await response.Content.ReadAsStringAsync();

        if (expectedCode == HttpStatusCode.OK)
        {
            TextReader tr = new StringReader(content);
            var reader = new CsvReader(tr, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = (args) => char.ToLower(args.Header[0]) + args.Header.Substring(1),
                MissingFieldFound = null,
                HeaderValidated = null,
            });

            var res = reader.GetRecords<TI>();
            return res.ToList();
        }
        else
        {
            return null;
        }
    }

    protected async Task<string> PostTest(string url, object payload, HttpStatusCode expectedCode = HttpStatusCode.Created, string authToken = "Success")
    {        
        var response = await Request(HttpMethod.Post, url, payload, authToken);

        response.StatusCode.Should().Be(expectedCode);

        var location = string.Empty;

        if (expectedCode == HttpStatusCode.Created)
        {
            location = response.Headers?.Location?.LocalPath ?? string.Empty;
            location.Should().StartWith(url);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonHelper.Deserialize<BaseEntityWithId>(content);
            result.Should().NotBeNull();
            result!.Id.Should().BePositive();
        }

        return location;
    }

    protected async Task<ImportResult> PostImportTest(string url, string importFileName, HttpStatusCode expectedCode = HttpStatusCode.OK, string authToken = "Success")
    {
        var response = await ImportRequest(HttpMethod.Post, $"{url}/import", importFileName, authToken);

        response.StatusCode.Should().Be(expectedCode);

        var content = await response.Content.ReadAsStringAsync();

        return JsonHelper.Deserialize<ImportResult>(content) !;
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

    private void CheckForRedundantProperties(string content)
    {
        bool isCollection = content.StartsWith("[");

        if (isCollection)
        {
            var resultCollection = JsonHelper.Deserialize<List<BaseEntity>>(content) !;
            resultCollection.Should().NotBeNull();
            if (resultCollection.Count > 0)
            {
                resultCollection[0].CreatedByIp.Should().BeNull();
                resultCollection[0].UpdatedByIp.Should().BeNull();
                resultCollection[0].CreatedByUserAgent.Should().BeNull();
                resultCollection[0].UpdatedByUserAgent.Should().BeNull(); 
            }
        }
        else
        {
            var result = JsonHelper.Deserialize<BaseEntity>(content) !;
            result.Should().NotBeNull();
            result.CreatedByIp.Should().BeNull();
            result.UpdatedByIp.Should().BeNull();
            result.CreatedByUserAgent.Should().BeNull();
            result.UpdatedByUserAgent.Should().BeNull();
        }
    }

    private Task<HttpResponseMessage> ImportRequest(HttpMethod method, string url, string importFileName, string authToken = "Success")
    {
        StringContent content;

        var request = new HttpRequestMessage(method, url);

        var file = GetCsvResouceContent(importFileName);

        if (Path.GetExtension(importFileName) !.ToLower() == ".csv")
        {
            content = new StringContent(file!, Encoding.UTF8, "text/csv"); 
        }
        else
        {
            content = new StringContent(file!, Encoding.UTF8, "application/json");
        }

        request.Content = content;

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        return client.SendAsync(request);
    }

    private string? GetCsvResouceContent(string fileName)
    {
        string? content = null;
        var assembly = Assembly.GetExecutingAssembly();

        var resourcePath = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(fileName));

        if (resourcePath is null)
        {
            return null;
        }

        var stream = assembly!.GetManifestResourceStream(resourcePath);
        if (stream != null)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                content = reader.ReadToEnd();
            }
        }

        return content;
    }
}