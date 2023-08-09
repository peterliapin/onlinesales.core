// <copyright file="ContactIpService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;

using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Services
{
    public class ContactIpService : IContactIpService
    {
        private readonly IHttpContextHelper httpContextHelper;
        private PgDbContext pgDbContext;

        public ContactIpService(PgDbContext pgDbContext, IHttpContextHelper httpContextHelper)
        {
            this.pgDbContext = pgDbContext;
            this.httpContextHelper = httpContextHelper;
        }

        public async Task SaveAsync(Contact c)
        {
            await SaveRangeAsync(new List<Contact>() { c });
        }

        public async Task SaveRangeAsync(List<Contact> contacts)
        {
            var ex = from contact in contacts
                                 where !string.IsNullOrEmpty(contact.LastIp)
                                 join cip in pgDbContext.ContactIp! on contact.Id equals cip.Contact?.Id ?? cip.ContactId into cIp
                                 where !cIp.Any(c => c.IpAddress == contact.LastIp)
                                 select new ContactIp { Contact = contact, IpAddress = contact.LastIp! };

            if (ex is not null && ex.Any())
            {
                await pgDbContext.ContactIp!.AddRangeAsync(ex);
            }
        }

        public void SetDBContext(PgDbContext pgDbContext)
        {
            this.pgDbContext = pgDbContext;
        }

        /// <summary>
        /// Update <see cref="Contact.LastIp"/> and return new <see  langword="IpAddress"/> back.
        /// </summary>
        /// <param name="contact">Entity to update.</param>
        public string? UpdateContactIp(Contact contact)
        {
            var ipAddress = httpContextHelper.IpAddressV4;
            contact.LastIp = ipAddress;
            return ipAddress;
        }
    }
}