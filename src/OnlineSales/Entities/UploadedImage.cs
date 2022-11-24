// <copyright file="UploadedImage.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.Entities
{
    public class UploadedImage : BaseEntity
    {
        [Required]
        public string ScopeId { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public long FileSize { get; set; } = 0;

        public string FileExtension { get; set; } = string.Empty;

        public string ReturnedFileName { get; set; } = string.Empty;

        public string MimeType { get; set; } = string.Empty;

        public byte[] ImageBinaryData { get; set; } = new byte[0];
    }
}
