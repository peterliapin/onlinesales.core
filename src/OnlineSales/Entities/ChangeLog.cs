// <copyright file="ChangeLog.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineSales.Entities
{
    public enum Operation
    {
        Inserted = 0,
        Updated = 1,
        Deleted = 2,
    }

    [Table("change_log")]
    public class ChangeLog : BaseEntityWithId
    {
        public string ObjectType { get; set; } = string.Empty;

        public int ObjectId { get; set; }

        public Operation Operation { get; set; }

        [Column(TypeName = "jsonb")]
        public string Data { get; set; } = string.Empty;
    }
}

