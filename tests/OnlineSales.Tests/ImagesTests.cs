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
    [Fact]
    public async Task CreateAndGetImageTest()
    {
        var fileName = "wavepoint.png";

        var assembly = Assembly.GetExecutingAssembly();
        assembly.Should().NotBeNull();
        var resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
        resourcePath.Should().NotBeNull();
        var stream = assembly!.GetManifestResourceStream(resourcePath);
        stream.Should().NotBeNull();

        var testImage = new TestImage(stream!, fileName);
        var newPostUrl = await PostImageTest("/api/images", testImage);
        var imageStream = await GetImageTest(newPostUrl);
        imageStream.Should().NotBeNull();
        CompareStreams(stream!, imageStream!).Should().BeTrue();
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