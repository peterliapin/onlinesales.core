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
            if (string.IsNullOrEmpty(c.LastIp))
            {
                _ = UpdateContactIp(c);
            }

            if(c.LastIp is null)
            {
                return;
            }

            if (c.Id == 0 || pgDbContext.ContactIp!.Any(i => i.ContactId == c.Id && i.IpAddress == c.LastIp))
            {
                await pgDbContext.ContactIp!.AddAsync(new ContactIp { IpAddress = c.LastIp!, Contact = c });
            }
        }

        public async Task SaveRangeAsync(List<Contact> contacts)
        {
            var toAdd = contacts
                .Where(c =>
                {
                    if (c.Id == 0)
                    {
                        if (string.IsNullOrEmpty(c.LastIp))
                        {
                            _ = UpdateContactIp(c);
                        }

                        return c.LastIp is not null;
                    }
                    else
                    {
                        return !pgDbContext.ContactIp!.Any(i => i.ContactId == c.Id && i.IpAddress == c.LastIp);
                    }
                })
                .Select(c => new ContactIp { IpAddress = c.LastIp!, Contact = c })
                .ToList();

            await pgDbContext.ContactIp!.AddRangeAsync(toAdd);
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