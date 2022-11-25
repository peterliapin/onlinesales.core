// <copyright file="Image.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace OnlineSales.Entities
{
    public class Image : BaseEntity
    {
        [Required]
        public string ScopeUId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public long Size { get; set; } = 0;

        public string Extension { get; set; } = string.Empty;

        public string MimeType { get; set; } = string.Empty;

        public byte[] Data { get; set; } = new byte[0];
    }
}
