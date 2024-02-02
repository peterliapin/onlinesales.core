// <copyright file="FilesTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace OnlineSales.Tests;

public class FilesTests : BaseTest
{
    [Theory]
    [InlineData("doc-2mb.doc", 1024 * 1024 * 2, false)]
    [InlineData("doc-500kb.doc", 1024 * 512, true)]
    [InlineData("mp4-5mb.mp4", 1024 * 1024 * 5, false)]
    [InlineData("txt-500kb.txt", 1024 * 512, true)]
    [InlineData("zip-1mb.zip", 1024 * 1024, true)]
    [InlineData("zip-5mb.zip", 1024 * 1024 * 5, false)]
    public async Task CreateAndGetFileTest(string fileName, int fileSize, bool shouldBePositive)
    {
        var result = await CreateAndGetFile(fileName, fileSize);
        result.Should().Be(shouldBePositive);
    }

    [Theory]
    [InlineData("HelloWorld-ThisIs---     ...DotNet.txt", "helloworld-thisis---...dotnet.txt", 1024)]
    public async Task TransliterationAndSlugifyTest(string fileName, string expectedTransliteratedName, int fileSize)
    {
        var testFile = new TestFile(fileName, fileSize);

        var postResult = await PostTest("/api/files", testFile);
        postResult.Item2.Should().BeTrue();

        var convertedFileName = Regex.Match(postResult.Item1, @"\/api\/files\/\S+\/(\S+.\S+)").Groups[1].Value;
        convertedFileName.Should().Match(expectedTransliteratedName);

        var fileStream = await GetFileTest(postResult.Item1);
        fileStream.Should().NotBeNull();

        CompareStreams(fileStream!, fileStream!).Should().BeTrue();
    }

    [Theory]
    [InlineData("doc-1mb.doc", 1024 * 1024)]
    [InlineData("txt-500kb.txt", 1024 * 512)]
    [InlineData("zip-3mb.zip", 1024 * 1024 * 3)]
    public async Task UpdateFileTest(string fileName, int fileSize)
    {
        await CreateAndGetFile(fileName, fileSize);
        var nonModifiedStream = await GetFileTest($"/api/files/{TestFile.Scope}/{fileName}");

        var testFile = new TestFile(fileName, fileSize);

        var postResult = await PostTest("/api/files", testFile);
        postResult.Item2.Should().BeTrue();

        var fileStream = await GetFileTest(postResult.Item1);
        fileStream.Should().NotBeNull();

        CompareStreams(nonModifiedStream!, fileStream!).Should().BeTrue();
        CompareStreams(testFile.DataBuffer, fileStream!).Should().BeTrue();
    }

    [Fact]
    public async Task CreateFileAnonymousTest()
    {
        var testFile = new TestFile("zip-1mb.zip", 1024 * 1024);

        await PostTest("/api/files", testFile, HttpStatusCode.Unauthorized, "Anonymous");
    }

    [Fact]
    public async Task GetFileAnonymousTest()
    {
        var testFile = new TestFile("zip-2mb.zip", 1024 * 1024 * 2);

        var postResult = await PostTest("/api/files", testFile);
        postResult.Item2.Should().BeTrue();

        var fileStream = await GetFileTest(postResult.Item1, HttpStatusCode.OK, "Anonymous");
        fileStream.Should().NotBeNull();

        CompareStreams(testFile.DataBuffer, fileStream!).Should().BeTrue();
    }

    public async Task<bool> CreateAndGetFile(string fileName, int fileSize)
    {
        var testFile = new TestFile(fileName, fileSize);

        var postResult = await PostTest("/api/files", testFile);
        if (!postResult.Item2)
        {
            return false;
        }

        var fileStream = await GetFileTest(postResult.Item1);
        if (fileStream == null)
        {
            return false;
        }

        return CompareStreams(testFile.DataBuffer, fileStream!);
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

    private async Task<(string, bool)> PostTest(string url, TestFile payload)
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

    private Task<HttpResponseMessage> Request(HttpMethod method, string url, TestFile? payload, string authToken = "Admin")
    {
        var request = new HttpRequestMessage(method, url);

        if (payload != null)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(payload.DataBuffer), "File", payload.File!.Name);
            content.Add(new StringContent(payload.ScopeUid), "ScopeUid");

            request.Content = content;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        return client.SendAsync(request);
    }

    private async Task<Stream?> GetFileTest(string url, HttpStatusCode expectedCode = HttpStatusCode.OK, string authToken = "Admin")
    {
        var response = await GetTest(url, expectedCode, authToken);

        if (expectedCode == HttpStatusCode.OK)
        {
            return await response.Content.ReadAsStreamAsync();
        }

        return null;
    }
}