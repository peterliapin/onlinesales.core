// <copyright file="ImagesTests.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;
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
}