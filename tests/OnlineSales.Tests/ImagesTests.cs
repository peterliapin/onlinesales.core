// <copyright file="ImagesTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.OData.Query;
using OnlineSales.Entities;

namespace OnlineSales.Tests;

public class ImagesTests : BaseTest
{
    private readonly string fileName = "wavepoint.png";
    private readonly string fileNameModified = "wavepointModified.png";

    [Fact]
    public async Task CreateAndGetImageTest()
    {
        var stream = CreateImageStream(fileName);
        var testImage = new TestImage(stream!, fileName);
        var newPostUrl = await PostTest("/api/images", testImage);
        var imageStream = await GetImageTest(newPostUrl);
        imageStream.Should().NotBeNull();
        CompareStreams(stream!, imageStream!).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateImageTest()
    {
        await CreateAndGetImageTest();

        var nonModifiedStream = CreateImageStream(fileName);

        var stream = CreateImageStream(fileNameModified);
        var testImage = new TestImage(stream!, fileName);
        var newPostUrl = await PostTest("/api/images", testImage);
        var imageStream = await GetImageTest(newPostUrl);
        imageStream.Should().NotBeNull();
        CompareStreams(nonModifiedStream!, imageStream!).Should().BeFalse();
        CompareStreams(stream!, imageStream!).Should().BeTrue();
    }

    [Fact]
    public async Task CreateImageAnonymousTest()
    {
        var stream = CreateImageStream(fileName);
        var testImage = new TestImage(stream!, fileName);
        await PostTest("/api/images", testImage, HttpStatusCode.Unauthorized, "NonSuccessAuthentification");
    }
  
    [Fact]
    public async Task GetImageAnonymousTest()
    {
        var stream = CreateImageStream(fileName);
        var testImage = new TestImage(stream!, fileName);
        var newPostUrl = await PostTest("/api/images", testImage);
        var imageStream = await GetImageTest(newPostUrl, HttpStatusCode.OK, "NonSuccessAuthentification");
        imageStream.Should().NotBeNull();
        CompareStreams(stream!, imageStream!).Should().BeTrue();
    }

    private Stream CreateImageStream(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        assembly.Should().NotBeNull();
        var resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(filename));
        resourcePath.Should().NotBeNull();
        var stream = assembly!.GetManifestResourceStream(resourcePath);
        stream.Should().NotBeNull();
        return stream!;
    }

    private bool CompareStreams(Stream s1, Stream s2)
    {
        if (s1.Length != s2.Length)
        {
            return false;
        }
        else
        {
            s1.Position = 0;
            s2.Position = 0;
            int data = 0;
            while (data != -1)
            {
                data = s1.ReadByte();
                if (data != s2.ReadByte())
                {
                    return false;
                }
            }

            return true;
        }
    }

    private async Task<string> PostTest(string url, TestImage payload, HttpStatusCode expectedCode = HttpStatusCode.Created, string authToken = "Success")
    {
        var response = await Request(HttpMethod.Post, url, payload, authToken);

        return CheckImagePostResponce(url, response, expectedCode);
    }

    private Task<HttpResponseMessage> Request(HttpMethod method, string url, TestImage? payload, string authToken = "Success")
    {
        var request = new HttpRequestMessage(method, url);

        if (payload != null)
        {
            var stream = new FileStream(payload.FilePath, FileMode.Open);

            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(stream), "Image", payload.Image!.Name);
            content.Add(new StringContent(payload.ScopeUid), "ScopeUid");

            request.Content = content;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        return Client.SendAsync(request);
    }

    private async Task<Stream?> GetImageTest(string url, HttpStatusCode expectedCode = HttpStatusCode.OK, string authToken = "Success")
    {
        var response = await GetTest(url, expectedCode, authToken);

        var content = await response.Content.ReadAsStreamAsync();

        if (expectedCode == HttpStatusCode.OK)
        {
            return content;
        }
        else
        {
            return null;
        }
    }

    private string CheckImagePostResponce(string url, HttpResponseMessage response, HttpStatusCode expectedCode)
    {
        var location = string.Empty;
        if (expectedCode == HttpStatusCode.Created)
        {
            location = response.Headers?.Location?.LocalPath ?? string.Empty;
            location.Should().StartWith(url);
        }

        return location;
    }
}