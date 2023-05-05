// <copyright file="TestMedia.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace OnlineSales.Tests.TestEntities
{
    public class TestMedia : ImageCreateDto
    {
        public static string Scope = "test-scope-ui";

        public readonly string FilePath;

        public readonly Stream DataBuffer;

        public TestMedia(string fileName, int length)
        {
            var buffer = new byte[length];
            this.DataBuffer = new MemoryStream(buffer);

            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);

            this.Image = new FormFile(this.DataBuffer, 0, this.DataBuffer.Length, fileName, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType!,
            };
            this.FilePath = fileName;
            this.ScopeUid = Scope;
        }
    }
}
