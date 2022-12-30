// <copyright file="TestImage.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using Microsoft.AspNetCore.Http;
using OnlineSales.DTOs;

namespace OnlineSales.Tests.TestEntities;

public class TestImage : ImageCreateDto
{
    public readonly string FilePath;

    public TestImage(Stream resourseStream, string imageResourceName)
    {
        var imageData = ReadResource(resourseStream, imageResourceName);
        Image = imageData.Item1;
        FilePath = imageData.Item2;
        ScopeUid = "test-scope-ui";
    }

    public TestImage(string imageResourceName)
    {
        var imageData = ReadResource(imageResourceName);
        Image = imageData.Item1;
        FilePath = imageData.Item2;
        ScopeUid = "test-scope-ui";
    }

    private static (IFormFile?, string) ReadResource(Stream resourceStream, string fileName)
    {
        var tempFile = Path.GetTempFileName();

        var tempStream = new FileStream(tempFile, FileMode.Create);

        resourceStream.CopyTo(tempStream);

        tempStream.Close();

        return (new FormFile(resourceStream!, 0, resourceStream!.Length, fileName, fileName), tempFile);
    }

    private static (IFormFile?, string) ReadResource(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var resourcePath = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(fileName));

        if (resourcePath is null)
        {
            return (null, string.Empty);
        }
        
        var stream = assembly!.GetManifestResourceStream(resourcePath);
        if (stream != null)
        {
            var tempFile = Path.Combine(Path.GetTempPath(), fileName);

            var tempStream = new FileStream(tempFile, FileMode.Create);

            stream.CopyTo(tempStream);

            tempStream.Close();

            return (new FormFile(stream!, 0, stream!.Length, fileName, fileName), tempFile);
        }
        else
        {
            return (null, string.Empty);
        }
    }
}