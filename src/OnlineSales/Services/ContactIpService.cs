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
            // Select all ContactIps as anonymous type { ip, id, contact } 
            var all = pgDbContext.ContactIp!
                .Select(c => new { ip = c.IpAddress, id = c.Id, contact = c.Contact! });

            var ex = contacts
                /* Skip cantacts with empty LastIp */
                .Where(c => !string.IsNullOrEmpty(c.LastIp))
                /* Select all contacts as anonymous type { ip, id, contact } */
                .Select(c => new { ip = c.LastIp, id = c.Id, contact = c })
                /* skip all exists in database */
                .Except(all!)
                /* Select anonymous type as ContactIp */
                .Select(a => new ContactIp { IpAddress = a.ip!, Contact = a.contact })
                /* as array of ContactIp */
                .ToArray();

            if (ex is not null && ex.Length > 0)
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