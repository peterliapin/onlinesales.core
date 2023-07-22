// <copyright file="IContactService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using OnlineSales.Entities;

namespace OnlineSales.Interfaces
{
    public interface IContactService : IEntityService<Contact>
    {
        Task Unsubscribe(string email, string reason, string source, DateTime createdAt, string? ip);
    }
}