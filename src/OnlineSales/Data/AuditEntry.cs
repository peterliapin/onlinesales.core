// <copyright file="AuditEntry.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace OnlineSales.Data
{
    internal class AuditEntry
    {
        public EntityEntry? EntityEntry { get; set; }

        public EntityState EntityState { get; set; }
    }
}
