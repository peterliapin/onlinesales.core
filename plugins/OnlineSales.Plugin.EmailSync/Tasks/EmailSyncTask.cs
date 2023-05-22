// <copyright file="EmailSyncTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Security.Cryptography.X509Certificates;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.EntityFrameworkCore.Storage;
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

        private readonly IContactService contactService;

        public EmailSyncTask(IConfiguration configuration, EmailSyncDbContext dbContext, TaskStatusService taskStatusService, IDomainService domainService, IContactService contactService)
            : base("Tasks:EmailSyncTask", configuration, taskStatusService)
        {
            this.dbContext = dbContext;
            this.domainService = domainService;
            this.contactService = contactService;

            var config = configuration.GetSection(this.configKey)!.Get<TaskWithBatchConfig>();            
            if (config is not null)
            {
                this.batchSize = config.BatchSize;
            }
            else
            {
                throw new MissingConfigurationException($"The specified configuration section for the provided configKey {this.configKey} could not be found in the settings file.");
            }

            var domains = configuration.GetSection("EmailSync:InternalDomains")!.Get<string[]>();
            this.internalDomains = (domains != null) ? domains : new string[0];

            var ignored = configuration.GetSection("EmailSync:IgnoredEmails")!.Get<string[]>();
            this.ignoredEmails = (ignored != null) ? ignored : new string[0];
        }

        public override async Task<bool> Execute(TaskExecutionLog currentJob)
        {
            var accounts = dbContext.ImapAccounts!.ToList();
            foreach (var imapAccount in accounts)
            {
                try
                {
                    using (var client = new ImapClient())
                    {
                        client.Connect(imapAccount.Host, imapAccount.Port, imapAccount.UseSsl);

                        client.Authenticate(imapAccount.UserName, imapAccount.Password);

                        var folders = client.GetFolders(client.PersonalNamespaces[0]);

                        var dBFolders = dbContext.ImapAccountFolders!.Where(f => f.ImapAccountId == imapAccount.Id).ToDictionary(dbf => dbf, dbf => false);

                        foreach (var folder in folders)
                        {
                            await GetEmailLogsFromFolder(dBFolders, folder, imapAccount);
                        }

                        var deletedFolders = dBFolders.Where(dbf => !dbf.Value).Select(dbf => dbf.Key);
                        if (deletedFolders.Any())
                        {
                            dbContext.ImapAccountFolders!.RemoveRange(deletedFolders);
                            await dbContext.SaveChangesAsync();
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

        private async Task GetEmailLogsFromFolder(Dictionary<ImapAccountFolder, bool> dBFolders, IMailFolder folder, ImapAccount imapAccount)
        {
            await folder.OpenAsync(FolderAccess.ReadOnly);
            var dbFolder = dBFolders.FirstOrDefault(f => f.Key.FullName == folder.FullName).Key;
            if (dbFolder == null)
            {
                dbFolder = new ImapAccountFolder
                {
                    FullName = folder.FullName,
                    LastUid = 0,
                    ImapAccountId = imapAccount.Id,
                };

                await dbContext.ImapAccountFolders!.AddAsync(dbFolder);
                await dbContext.SaveChangesAsync();
            }
            else
            {
                dBFolders[dbFolder] = true;
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
                await GetEmailLogs(dbFolder, folder, batch);
                position += batchSize;
            }
        }

        private async Task GetEmailLogs(ImapAccountFolder dbFolder, IMailFolder folder, IEnumerable<UniqueId> uids)
        {
            var contactsToAdd = new HashSet<string>();
            var resultData = new List<EmailLog>();
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
                if (!existedMessagesUids.Contains(message.MessageId))
                {
                    var fromEmail = message.From.Mailboxes.Single().Address;

                    if (!ignoredEmails.Contains(fromEmail))
                    {
                        var recipients = message.GetRecipients().Select(r => r.Address).ToList();

                        var contactsWithInternalFlag = HandleContactsAndDomains(fromEmail, recipients, contactsToAdd);

                        if (contactsWithInternalFlag.Any(c => !c.Value))
                        {
                            var log = new EmailLog()
                            {
                                Subject = message.Subject == null ? string.Empty : message.Subject,
                                Recipient = string.Join(";", recipients),
                                FromEmail = message.From.Mailboxes.Single().Address,
                                Body = message.TextBody,
                                MessageId = message.MessageId,
                            };

                            if (log.Body == null)
                            {
                                log.Body = message.HtmlBody;
                            }

                            resultData.Add(log);
                        }
                    }                    
                }
            }

            if (resultData.Count > 0)
            {
                await dbContext.EmailLogs!.AddRangeAsync(resultData);
            }

            dbFolder.LastUid = resultLastId;

            await dbContext.SaveChangesAsync();

            if (contactsToAdd.Count > 0)
            {
                await contactService.SaveRangeAsync(contactsToAdd.Select(c => new Contact() { Email = c, }).ToList());
                await contactService.SaveChangesAsync();
            }
        }

        private bool IsInternalDomain(string email)
        {
            return internalDomains.Contains(domainService.GetDomainNameByEmail(email));
        }

        private Dictionary<string, bool> HandleContactsAndDomains(string fromEmail, List<string> toEmails, HashSet<string> contactsToAdd)
        {
            void AddToResult(string email, Dictionary<string, bool> res)
            {
                res.Add(email, IsInternalDomain(email));
            }

            var res = new Dictionary<string, bool>();
            AddToResult(fromEmail, res);
            foreach (var e in toEmails)
            {
                AddToResult(e, res);
            }

            var existedContacts = dbContext.Contacts!.Where(c => res.Keys.Contains(c.Email)).Select(c => c.Email).ToList();

            foreach (var r in res)
            {
                if (!r.Value && !existedContacts.Contains(r.Key))
                {
                    contactsToAdd.Add(r.Key);
                }
            }

            return res;
        }
    }
}

