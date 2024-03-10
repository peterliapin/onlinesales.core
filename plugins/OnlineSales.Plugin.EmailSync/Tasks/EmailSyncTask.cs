// <copyright file="EmailSyncTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using OnlineSales.Configuration;
using OnlineSales.Entities;
using OnlineSales.Exceptions;
using OnlineSales.Interfaces;
using OnlineSales.Plugin.EmailSync.Data;
using OnlineSales.Plugin.EmailSync.Entities;
using OnlineSales.Services;
using OnlineSales.Tasks;
using Serilog;

namespace OnlineSales.EmailSync.Tasks
{
    public class EmailSyncTask : BaseTask
    {
        private readonly EmailSyncDbContext dbContext;

        private readonly int batchSize;

        private readonly string[] internalDomains;

        private readonly string[] ignoredEmails;

        private readonly IDomainService domainService;
        private readonly IContactService contactsService;

        public EmailSyncTask(IConfiguration configuration, EmailSyncDbContext dbContext, TaskStatusService taskStatusService, IDomainService domainService, IContactService contactsService)
            : base("Tasks:EmailSyncTask", configuration, taskStatusService)
        {
            this.dbContext = dbContext;
            this.domainService = domainService;
            this.contactsService = contactsService;

            var config = configuration.GetSection(configKey)!.Get<TaskWithBatchConfig>();            
            if (config is not null)
            {
                batchSize = config.BatchSize;
            }
            else
            {
                throw new MissingConfigurationException($"The specified configuration section for the provided configKey {configKey} could not be found in the settings file.");
            }

            var domains = configuration.GetSection("EmailSync:InternalDomains")!.Get<string[]>();
            internalDomains = (domains != null) ? domains : new string[0];

            var ignored = configuration.GetSection("EmailSync:IgnoredEmails")!.Get<string[]>();
            ignoredEmails = (ignored != null) ? ignored : new string[0];

            domainService.SetDBContext(dbContext);
            contactsService.SetDBContext(dbContext);            
        }

        public override async Task<bool> Execute(TaskExecutionLog currentJob)
        {
            var accounts = dbContext.ImapAccounts!.OrderBy(ia => ia.Id).ToList();

            foreach (var imapAccount in accounts)
            {
                try
                {
                    using (var client = new ImapClient())
                    {
                        client.Connect(imapAccount.Host, imapAccount.Port, imapAccount.UseSsl);

                        client.Authenticate(imapAccount.UserName, imapAccount.Password);

                        foreach (var personalNamespace in client.PersonalNamespaces)
                        {
                            var folders = client.GetFolders(personalNamespace);

                            var imapAccountFolders = dbContext.ImapAccountFolders!.Where(f => f.ImapAccountId == imapAccount.Id).ToList();

                            foreach (var folder in folders)
                            {
                                await GetEmailLogsFromFolder(imapAccount.UserName, imapAccountFolders, folder, imapAccount);
                            }

                            await DeleteUnexistedFolders(imapAccountFolders, folders);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, $"Error occured during imap syncronization, imap: {imapAccount.Host}, userName: {imapAccount.UserName}");
                }
            }

            return true;            
        }

        private async Task DeleteUnexistedFolders(List<ImapAccountFolder> imapAccountFolders, IList<IMailFolder> folders)
        {
            var foldersToDelete = imapAccountFolders.Where(iaf => !folders.Select(f => f.FullName).Contains(iaf.FullName));
            dbContext.ImapAccountFolders!.RemoveRange(foldersToDelete);
            await dbContext.SaveChangesAsync();
        }

        private async Task GetEmailLogsFromFolder(string userName, List<ImapAccountFolder> imapAccountFolders, IMailFolder folder, ImapAccount imapAccount)
        {
            await folder.OpenAsync(FolderAccess.ReadOnly);

            var dbFolder = imapAccountFolders.FirstOrDefault(f => f.FullName == folder.FullName);

            if (dbFolder == null)
            {
                dbFolder = new ImapAccountFolder
                {
                    FullName = folder.FullName,
                    LastUid = 0,
                    ImapAccountId = imapAccount.Id,
                    Source = userName,
                };

                await dbContext.ImapAccountFolders!.AddAsync(dbFolder);
            }

            if (folder.UidNext.HasValue && folder.UidNext.Value.Id <= dbFolder.LastUid)
            {
                dbFolder.LastUid = 0;
            }

            var range = new UniqueIdRange(new UniqueId((uint)dbFolder.LastUid + 1), UniqueId.MaxValue);
            var uids = folder.Search(range, SearchQuery.All);
            var position = 0;
            while (position < uids.Count)
            {
                var batch = uids.Skip(position).Take(batchSize);
                await GetEmailLogs(userName, dbFolder, folder, batch);
                position += batchSize;
            }
        }

        private async Task GetEmailLogs(string userName, ImapAccountFolder dbFolder, IMailFolder folder, IEnumerable<UniqueId> uids)
        {
            var emailLogs = new List<EmailLog>();
            var resultLastId = dbFolder.LastUid;

            var messages = new List<MimeMessage>();
            foreach (var uid in uids)
            {
                if (uid.Id <= dbFolder.LastUid)
                {
                    continue;
                }

                messages.Add(folder.GetMessage(uid));
                resultLastId = (int)uid.Id;
            }

            var existedMessagesUids = dbContext.EmailLogs!.Where(el => messages.Select(m => m.MessageId).Contains(el.MessageId)).Select(m => m.MessageId).ToList();

            foreach (var message in messages)
            {
                if (string.IsNullOrEmpty(message.MessageId))
                {
                    continue;
                }

                if (!existedMessagesUids.Contains(message.MessageId))
                {
                    var fromMailbox = message.From.Mailboxes.FirstOrDefault();

                    if (fromMailbox == null)
                    {
                        continue;
                    }

                    var fromEmail = fromMailbox.Address;

                    if (!ignoredEmails.Contains(fromEmail))
                    {
                        var recipients = message.GetRecipients().Select(r => r.Address).ToList();

                        if (!IsInternalEmails(fromEmail, recipients))
                        {
                            var from = message.From.Mailboxes.Single().Address;
                            var status = IsInternalDomain(from) ? EmailStatus.Sent : EmailStatus.Received;

                            var emailLog = new EmailLog
                            {
                                Subject = message.Subject == null ? string.Empty : message.Subject,
                                Recipients = string.Join(";", recipients),
                                FromEmail = from,
                                HtmlBody = message.HtmlBody,
                                TextBody = message.TextBody,
                                MessageId = message.MessageId,
                                Source = userName + " - " + folder.FullName,
                                Status = status,
                                CreatedAt = message.Date.UtcDateTime,
                            };

                            emailLogs.Add(emailLog);
                        }
                    }                    
                }
            }

            if (emailLogs.Count > 0)
            {
                await EnrichWithContactIdAsync(emailLogs);

                await dbContext.EmailLogs!.AddRangeAsync(emailLogs);
            }

            dbFolder.LastUid = resultLastId;

            await dbContext.SaveChangesAsync();
        }

        private async Task EnrichWithContactIdAsync(List<EmailLog> emailLogs)
        {
            var emails = emailLogs
                .SelectMany(emailLog => new[] { emailLog.FromEmail }
                    .Concat(emailLog.Recipients.Split(';')))
                .Distinct()
                .Where(email => !IsInternalDomain(email) && !ignoredEmails.Contains(email))
                .ToList();

            var existingContacts = await dbContext.Contacts!
                                    .Where(contact => emails.Contains(contact.Email))
                                    .ToDictionaryAsync(contact => contact.Email, contact => contact);

            var newContacts = new List<Contact>();

            foreach (var emailLog in emailLogs)
            {
                if (emailLog.ContactId > 0)
                {
                    continue;
                }

                var participants = new List<string> { emailLog.FromEmail };
                participants.AddRange(emailLog.Recipients.Split(';'));
                var contactEmail = participants.FirstOrDefault(email => !IsInternalDomain(email) && !ignoredEmails.Contains(email));

                if (string.IsNullOrEmpty(contactEmail))
                {
                    continue;
                }

                Contact? contact;

                if (!existingContacts.TryGetValue(contactEmail, out contact))
                {
                    contact = new Contact { Email = contactEmail };
                    newContacts.Add(contact);
                    existingContacts[contact.Email] = contact;
                }

                emailLog.Contact = contact;
            }

            await contactsService.SaveRangeAsync(newContacts);
        }

        private bool IsInternalDomain(string email)
        {
            return internalDomains.Contains(domainService.GetDomainNameByEmail(email));
        }

        private bool IsInternalEmails(string fromEmail, List<string> toEmails)
        {
            return IsInternalDomain(fromEmail) && toEmails.TrueForAll(email => IsInternalDomain(email));
        }
    }
}

