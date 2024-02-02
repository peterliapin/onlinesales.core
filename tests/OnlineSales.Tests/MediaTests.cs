// <copyright file="MediaTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace OnlineSales.Tests;

public class MediaTests : BaseTest
{
    [Theory]
    [InlineData("test1.png", 1024000, false)]
    [InlineData("test2.png", 1024, true)]
    [InlineData("test3.jpeg", 1024000, false)]
    [InlineData("test4.jpeg", 1024, true)]
    [InlineData("test5.mp4", 5024000, false)]
    [InlineData("test6.mp4", 1024, true)]
    public async Task CreateAndGetMediaTest(string fileName, int fileSize, bool shouldBePositive)
    {
        var result = await CreateAndGetMedia(fileName, fileSize);
        result.Should().Be(shouldBePositive);
    }

    [Theory]
    [InlineData("HelloWorld-ThisIs---     ...DotNet.png", "helloworld-thisis---...dotnet.png", 1024)]
    public async Task TransliterationAndSlugifyTest(string fileName, string expectedTransliteratedName, int fileSize)
    {
        var testImage = new TestMedia(fileName, fileSize);

        var postResult = await PostTest("/api/media", testImage);
        postResult.Item2.Should().BeTrue();
        var convertedFileName = Regex.Match(postResult.Item1, @"\/api\/media\/\S+\/(\S+.\S+)").Groups[1].Value;
        convertedFileName.Should().Match(expectedTransliteratedName);
        var imageStream = await GetImageTest(postResult.Item1);
        imageStream.Should().NotBeNull();
        CompareStreams(imageStream!, imageStream!).Should().BeTrue();
    }

    [Theory]
    [InlineData("test2.png", 1024)]
    [InlineData("test4.jpeg", 1024)]
    [InlineData("test6.mp4", 1024)]
    public async Task UpdateImageTest(string fileName, int fileSize)
    {
        await CreateAndGetMedia(fileName, fileSize);
        var nonModifiedStream = await GetImageTest($"/api/media/{TestMedia.Scope}/{fileName}");

        var testImage = new TestMedia(fileName, fileSize);

        var postResult = await PostTest("/api/media", testImage);
        postResult.Item2.Should().BeTrue();
        var imageStream = await GetImageTest(postResult.Item1);
        imageStream.Should().NotBeNull();
        CompareStreams(nonModifiedStream!, imageStream!).Should().BeTrue();
        CompareStreams(testImage.DataBuffer, imageStream!).Should().BeTrue();
    }

    [Fact]
    public async Task CreateImageAnonymousTest()
    {
        var testMedia = new TestMedia("test1.png", 1024);
        await PostTest("/api/media", testMedia, HttpStatusCode.Unauthorized, "Anonymous");
    }

    [Fact]
    public async Task GetImageAnonymousTest()
    {
        var testMedia = new TestMedia("test1.png", 1024);
        var postResult = await PostTest("/api/media", testMedia);
        postResult.Item2.Should().BeTrue();
        var imageStream = await GetImageTest(postResult.Item1, HttpStatusCode.OK, "Anonymous");
        imageStream.Should().NotBeNull();
        CompareStreams(testMedia.DataBuffer, imageStream!).Should().BeTrue();
    }

    public async Task<bool> CreateAndGetMedia(string fileName, int fileSize)
    {
        var testMedia = new TestMedia(fileName, fileSize);
        var postResult = await PostTest("/api/media", testMedia);
        if (!postResult.Item2)
        {
            return false;
        }

        var imageStream = await GetImageTest(postResult.Item1);
        if (imageStream == null)
        {
            return false;
        }

        return CompareStreams(testMedia.DataBuffer, imageStream!);
    }

    private bool CompareStreams(Stream s1, Stream s2)
    {
        if (s1.Length != s2.Length)
        {
            return false;
        }

        var s1Hash = string.Concat(SHA1.HashData(((MemoryStream)s1).ToArray()).Select(b => b.ToString("x2")));
        var s2Hash = string.Concat(SHA1.HashData(((MemoryStream)s2).ToArray()).Select(b => b.ToString("x2")));

        return string.Equals(s1Hash, s2Hash, StringComparison.Ordinal);
    }

    private async Task<(string, bool)> PostTest(string url, TestMedia payload)
    {
        var response = await Request(HttpMethod.Post, url, payload);
        if (response.StatusCode != HttpStatusCode.Created)
        {
            return (string.Empty, false);
        }

        var json = response.Content.ReadAsStringAsync().Result;
        if (json.Contains("location"))
        {
            json = json.Split(':').Last().Replace("}", string.Empty).Replace("\"", string.Empty).Trim();
            return (json, true);
        }

        return (string.Empty, false);
    }

    private Task<HttpResponseMessage> Request(HttpMethod method, string url, TestMedia? payload, string authToken = "Admin")
    {
        var request = new HttpRequestMessage(method, url);

        if (payload != null)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(payload.DataBuffer), "Image", payload.Image!.Name);
            content.Add(new StringContent(payload.ScopeUid), "ScopeUid");

            request.Content = content;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        return client.SendAsync(request);
    }

    private async Task<Stream?> GetImageTest(string url, HttpStatusCode expectedCode = HttpStatusCode.OK, string authToken = "Admin")
    {
        var response = await GetTest(url, expectedCode, authToken);

        if (expectedCode == HttpStatusCode.OK)
        {
            return await response.Content.ReadAsStreamAsync();
        }
        else
        {
            return null;
        }
    }
}