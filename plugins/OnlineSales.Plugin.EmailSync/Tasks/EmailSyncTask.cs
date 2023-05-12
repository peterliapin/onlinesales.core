// <copyright file="EmailSyncTask.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Extensions.Configuration;
using OnlineSales.Configuration;
using OnlineSales.Entities;
using OnlineSales.Exceptions;
using OnlineSales.Interfaces;
using OnlineSales.Plugin.EmailSync.Data;
using OnlineSales.Services;
using OnlineSales.Tasks;

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

            var domains = configuration.GetSection("InternalDomains")!.Get<string[]>();
            this.internalDomains = (domains != null) ? domains : new string[0];

            var ignored = configuration.GetSection("IgnoredEmails")!.Get<string[]>();
            this.ignoredEmails = (ignored != null) ? ignored : new string[0];
        }

        public override async Task<bool> Execute(TaskExecutionLog currentJob)
        {
            try
            {
                var accounts = dbContext.ImapAccounts!.ToList();
                foreach (var imapAccount in accounts)
                {
                    using (var client = new ImapClient())
                    {
                        client.Connect(imapAccount.Host, imapAccount.Port, imapAccount.UseSsl);

                        client.Authenticate(imapAccount.UserName, imapAccount.Password);

                        var folders = client.GetFolders(client.PersonalNamespaces[0]);

                        var now = DateTime.UtcNow;

                        var contactsToAdd = new HashSet<string>();

                        await AddEmailLogs(imapAccount.LastDateTime, now, folders, contactsToAdd);

                        await contactService.SaveRangeAsync(contactsToAdd.Select(c => new Contact() { Email = c, }).ToList());

                        imapAccount.LastDateTime = now;
                    }
                }

                await dbContext.SaveChangesAsync();

                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
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

        private async Task AddEmailLogs(DateTime sinceDateTime, DateTime toDateTime, IList<IMailFolder> folders, HashSet<string> contactsToAdd)
        {
            if (toDateTime > sinceDateTime)
            {
                int uidsCount = 0;
                var folderData = new Dictionary<IMailFolder, List<IMessageSummary>>();
                foreach (var folder in folders)
                {
                    await folder.OpenAsync(FolderAccess.ReadOnly);
                    var uids = await folder.SearchAsync(SearchQuery.SentSince(sinceDateTime.Date.AddDays(-1)).And(SearchQuery.SentBefore(toDateTime.Date.AddDays(1))).And(SearchQuery.NotDeleted).And(SearchQuery.NotDraft));
                    var data = await folder.FetchAsync(uids, MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope);
                    var newestData = data.Where(d => d.Envelope.Date.HasValue && d.Envelope.Date.Value.UtcDateTime >= sinceDateTime && d.Envelope.Date.Value.UtcDateTime < toDateTime).ToList();
                    uidsCount += newestData.Count;
                    folderData.Add(folder, newestData);
                }

                if (uidsCount > batchSize)
                {
                    var diff = (toDateTime - sinceDateTime).Divide(2);
                    var middle = sinceDateTime + diff;
                    await AddEmailLogs(sinceDateTime, middle, folders, contactsToAdd);
                    await AddEmailLogs(middle, toDateTime, folders, contactsToAdd);
                }
                else
                {
                    foreach (var data in folderData)
                    {
                        foreach (var uid in data.Value.Select(d => d.UniqueId))
                        {
                            await data.Key.OpenAsync(FolderAccess.ReadOnly);
                            var message = await data.Key.GetMessageAsync(uid);
                            var fromEmail = message.From.Mailboxes.Single().Address;

                            if (!ignoredEmails.Contains(fromEmail))
                            {
                                var recipients = message.GetRecipients().Select(r => r.Address).ToList();

                                var contactsWithInternalFlag = HandleContactsAndDomains(fromEmail, recipients, contactsToAdd);

                                if (contactsWithInternalFlag.Any(c => !c.Value))
                                {
                                    var log = new EmailLog()
                                    {
                                        Subject = message.Subject,
                                        Recipient = string.Join(";", recipients),
                                        FromEmail = message.From.Mailboxes.Single().Address,
                                        Body = message.TextBody,
                                    };

                                    if (log.Body == null)
                                    {
                                        log.Body = message.HtmlBody;
                                    }

                                    await dbContext.EmailLogs!.AddAsync(log);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

