// <copyright file="ChangeLog.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineSales.Entities
{
    public enum Operation
    {
        NA = 0,
        Inserted = 1,
        Updated = 2,
        Deleted = 3,
    }

    [Table("change_log")]
    public class ChangeLog : BaseEntityWithId
    {
        public int ObjectType { get; set; }

        public int ObjectId { get; set; }

        public Operation Operation { get; set; } = Operation.NA;

        [Column(TypeName = "jsonb")]
        public string Data { get; set; } = string.Empty;
    }
}

