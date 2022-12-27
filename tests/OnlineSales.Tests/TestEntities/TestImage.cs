// <copyright file="TestImage.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using Microsoft.AspNetCore.Http;
using OnlineSales.DTOs;

namespace OnlineSales.Tests.TestEntities;

public class TestImage : ImageCreateDto
{
    public TestImage(string imageResourceName)
    {
        Image = ReadResource(imageResourceName);
        ScopeUid = "test-scope-ui";
    }

    private static IFormFile? ReadResource(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var resourcePath = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(fileName));

        if (resourcePath is null)
        {
            return null;
        }

        using (Stream? stream = assembly!.GetManifestResourceStream(resourcePath))
        {
            return new FormFile(stream!, 0, stream!.Length, fileName, fileName);
        }
    }
}