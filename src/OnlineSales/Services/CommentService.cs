// <copyright file="CommentService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Services;

public class CommentService : ICommentService
{
    private readonly IContactService contactsService;
    private PgDbContext pgDbContext;    

    public CommentService(PgDbContext pgDbContext, IContactService contactsService)
    {
        this.pgDbContext = pgDbContext;
        this.contactsService = contactsService;
    }

    public async Task SaveAsync(Comment comment)
    {
        await EnrichWithContactId(comment);

        if (comment.Id > 0)
        {
            pgDbContext.Comments!.Update(comment);
        }
        else
        {
            await pgDbContext.Comments!.AddAsync(comment);
        }
    }

    public async Task SaveRangeAsync(List<Comment> comments)
    {
        await EnrichWithContactIdAsync(comments);

        var newAndExisting = comments.GroupBy(c => c.Id > 0);

        foreach (var group in newAndExisting)
        {
            if (group.Key)
            {
                pgDbContext.UpdateRange(group.ToList());
            }
            else
            {
                await pgDbContext.AddRangeAsync(group.ToList());
            }
        }
    }

    public void SetDBContext(PgDbContext pgDbContext)
    {
        this.pgDbContext = pgDbContext;
        contactsService.SetDBContext(pgDbContext);
    }

    private async Task EnrichWithContactId(Comment comment)
    {
        if (comment.ContactId == 0)
        {
            var contact = await pgDbContext.Contacts!.FirstOrDefaultAsync(contact => contact.Email == comment.AuthorEmail);

            if (contact == null)
            {
                contact = new Contact { Email = comment.AuthorEmail, FirstName = comment.AuthorName };

                await contactsService.SaveAsync(contact);
            }

            comment.Contact = contact;
        }
    }

    private async Task EnrichWithContactIdAsync(List<Comment> comments)
    {
        var emails = (from comment in comments
                      select comment.AuthorEmail).Distinct();

        var existingContacts = await pgDbContext.Contacts!
                                .Where(contact => emails.Contains(contact.Email))
                                .ToDictionaryAsync(contact => contact.Email, contact => contact);

        var newContacts = new List<Contact>();

        foreach (var comment in comments)
        {
            if (comment.ContactId > 0)
            {
                continue;
            }

            Contact? contact;

            if (!existingContacts.TryGetValue(comment.AuthorEmail, out contact))
            {
                contact = new Contact { Email = comment.AuthorEmail, FirstName = comment.AuthorName };
                newContacts.Add(contact);
                existingContacts[contact.Email] = contact;
            }

            comment.Contact = contact;
        }

        await contactsService.SaveRangeAsync(newContacts);
    }
}