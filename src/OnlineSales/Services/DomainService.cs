// <copyright file="DomainService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Net.Mail;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using ARSoft.Tools.Net.Dns;
using HtmlAgilityPack;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Interfaces; 

namespace OnlineSales.Services
{
    public class DomainService : IDomainService
    {
        private static HashSet<string> freeDomains = InitDomainsList("free_domains.txt");

        private static HashSet<string> disposableDomains = InitDomainsList("disposable_domains.txt");

        private readonly PgDbContext pgDbContext;

        private readonly DnsStubResolver lookupClient;
        private readonly IMxVerifyService mxVerifyService;

        public DomainService(PgDbContext pgDbContext, IMxVerifyService mxVerifyService)
        {
            this.mxVerifyService = mxVerifyService;

            this.pgDbContext = pgDbContext;

            lookupClient = new ();
        }

        public async Task Verify(Domain domain)
        {
            if (domain.DnsCheck == null)
            {
                VerifyDns(domain);
            }            

            if (domain.DnsCheck is true)
            {
                if (domain.HttpCheck == null)
                {
                    await VerifyHttp(domain);
                }

                if (domain.MxCheck == null)
                {
                    await VerifyMX(domain);
                }
            }
            else
            {
                domain.HttpCheck = false;
                domain.MxCheck = false;
                domain.Url = null;
                domain.Title = null;
                domain.Description = null;
            }
        }

        public async Task SaveAsync(Domain domain)
        {
            VerifyFreeAndDisposable(domain);

            if (domain.Id > 0)
            {
                pgDbContext.Domains !.Update(domain);
            }
            else
            {
                await pgDbContext.Domains !.AddAsync(domain);
            }
        }

        public async Task SaveRangeAsync(List<Domain> domains)
        {
            domains.ForEach(d => VerifyFreeAndDisposable(d));

            var sortedDomains = domains.GroupBy(d => d.Id > 0);

            foreach (var group in sortedDomains)
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

        public string GetDomainNameByEmail(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return string.Empty;
                }

                var address = new MailAddress(email);
                return address.Host;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return string.Empty;
            }
        }

        private static HashSet<string> InitDomainsList(string filename)
        {
            var res = new HashSet<string>();

            var asm = Assembly.GetExecutingAssembly();
            var resourcePath = asm.GetManifestResourceNames().Single(str => str.EndsWith(filename));

            if (resourcePath == null)
            {
                throw new FileNotFoundException(filename);
            }

            using (var rsrcStream = asm.GetManifestResourceStream(resourcePath))
            {
                if (rsrcStream == null)
                {
                    throw new FileNotFoundException(filename);
                }
                else
                {
                    using (var sRdr = new StreamReader(rsrcStream))
                    {
                        string? line = null;
                        while ((line = sRdr.ReadLine()) != null)
                        {                            
                            res.Add(line);
                        }
                    }
                }
            }

            return res;
        }

        private void VerifyFreeAndDisposable(Domain domain)
        {
            if (domain.Free == null || domain.Disposable == null)
            {
                domain.Free = freeDomains.Contains(domain.Name);

                domain.Disposable = disposableDomains.Contains(domain.Name);
            }
        }

        private async Task VerifyHttp(Domain domain)
        {
            domain.HttpCheck = false;

            var urls = new string[]
            {
            "https://" + domain.Name,
            "https://www." + domain.Name,
            "http://" + domain.Name,
            "http://www." + domain.Name,
            };

            foreach (var url in urls)
            {
                var responce = await GetRequest(url);

                if (responce != null && responce.RequestMessage != null && responce.RequestMessage.RequestUri != null)
                {
                    domain.HttpCheck = true;

                    domain.Url = responce.RequestMessage.RequestUri.ToString();
                    var web = new HtmlWeb();
                    var htmlDoc = await web.LoadFromWebAsync(domain.Url, Encoding.UTF8);

                    if (htmlDoc != null)
                    {
                        domain.Title = GetTitle(htmlDoc);
                        domain.Description = GetDescription(htmlDoc);
                    }

                    break;
                }
            }
        }

        private async Task VerifyMX(Domain domain)
        {
            domain.MxCheck = false;

            var mxRecords = domain
                .DnsRecords
                ?.OfType<MxRecord>()
                .ToArray()
                ?? Array.Empty<MxRecord>();

            foreach (var mxRecordValue in mxRecords)
            {
                var mxHost = mxRecordValue.Name.ToString().TrimEnd('.');

                var mxVerify = await mxVerifyService.Verify(mxHost);

                if (mxVerify)
                {
                    domain.MxCheck = true;
                    break;
                }
            }
        }

        private void VerifyDns(Domain domain)
        {
            domain.DnsRecords = null;
            domain.DnsCheck = false;

            var listOfDnsRecords = new List<DnsRecordBase>();

            listOfDnsRecords.AddRange(lookupClient.Resolve<DnsRecordBase>(domain.Name, RecordType.A));
            listOfDnsRecords.AddRange(lookupClient.Resolve<DnsRecordBase>(domain.Name, RecordType.Ns));
            listOfDnsRecords.AddRange(lookupClient.Resolve<DnsRecordBase>(domain.Name, RecordType.Txt));

            // CName record it's not a best way to resolve domain IP
            // It works like  CName -> CName -> CName -> IP
            listOfDnsRecords.AddRange(lookupClient.Resolve<DnsRecordBase>(domain.Name, RecordType.CName));

            // Get all Mx records and order it by Preference (lowest - most prefered)
            listOfDnsRecords.AddRange(lookupClient.Resolve<DnsRecordBase>(domain.Name, RecordType.Mx).Cast<MxRecord>().OrderBy(record => record.Preference).Cast<DnsRecordBase>().ToList());

            var dnsRecords = GetDnsRecords(listOfDnsRecords.ToArray(), domain);

            if (dnsRecords.Count > 0)
            {
                domain.DnsCheck = true;
                domain.DnsRecords = dnsRecords;
            }
        }

        private List<DnsRecord> GetDnsRecords(DnsRecordBase[] records, Domain d)
        {
            var dnsRecords = new List<DnsRecord>();

            foreach (var dnsResponseRecord in records)
            {
                try
                {
                    var dnsRecord = new DnsRecord
                    {
                        DomainName = dnsResponseRecord.Name.ToString().TrimEnd('.'),
                        RecordClass = dnsResponseRecord.RecordClass.ToString(),
                        RecordType = dnsResponseRecord.RecordType.ToString(),
                        TimeToLive = dnsResponseRecord.TimeToLive,
                    };

                    switch (dnsResponseRecord)
                    {
                        case ARecord a:
                            if (dnsRecord.DomainName != d.Name + ".")
                            {
                                // we are only interesting in an A record for the main domain
                                continue;
                            }

                            dnsRecord.Value = a.Address.ToString().TrimEnd('.');
                            break;

                        case CNameRecord cname:
                            dnsRecord.Value = cname.CanonicalName.ToString();
                            break;

                        case MxRecord mx:
                            dnsRecord.Value = mx.ExchangeDomainName.ToString().TrimEnd('.');
                            break;

                        case TxtRecord txt:
                            dnsRecord.Value = txt.TextData;
                            break;

                        case NsRecord ns:
                            dnsRecord.Value = ns.Name.ToString().TrimEnd('.');
                            break;

                        default:
                            continue;
                    }

                    dnsRecords.Add(dnsRecord);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error reading DNS record.");
                }
            }

            return dnsRecords;
        }

        private async Task<HttpResponseMessage?> GetRequest(string url)
        {
            HttpClient client = new HttpClient();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                return await client.SendAsync(request);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to fetch url: {url}");
                return null;
            }
        }

        private string? GetTitle(HtmlDocument htmlDoc)
        {
            var htmlNode = htmlDoc.DocumentNode.SelectSingleNode("//title");

            if (htmlNode != null && !string.IsNullOrEmpty(htmlNode.InnerText))
            {
                return htmlNode.InnerText;
            }

            var title = GetNodeContentByAttr(htmlDoc, "title");

            if (!string.IsNullOrEmpty(title))
            {
                return title;
            }

            htmlNode = htmlDoc.DocumentNode.SelectSingleNode("//h1");

            if (htmlNode != null && !string.IsNullOrEmpty(htmlNode.InnerText))
            {
                return htmlNode.InnerText;
            }

            return null;
        }

        private string? GetDescription(HtmlDocument htmlDoc)
        {
            return GetNodeContentByAttr(htmlDoc, "description");
        }

        private string? GetNodeContentByAttr(HtmlDocument htmlDoc, string value)
        {
            var result = GetNodeContentByAttr(htmlDoc, "name", value);
            if (result == null)
            {
                result = GetNodeContentByAttr(htmlDoc, "property", value);
            }

            return result;
        }

        private string? GetNodeContentByAttr(HtmlDocument htmlDoc, string attrName, string value)
        {
            string? GetNodeContent(HtmlDocument htmlDoc, string attrName, string value)
            {
                var htmlNode = htmlDoc.DocumentNode.SelectSingleNode(string.Format("//meta[@{0}='{1}']", attrName, value));
                if (htmlNode != null && htmlNode.Attributes.Contains("content"))
                {
                    return htmlNode.GetAttributeValue("content", null);
                }

                return null;
            }

            var res = GetNodeContent(htmlDoc, attrName, value);
            if (res == null)
            {
                res = GetNodeContent(htmlDoc, attrName, "og:" + value);
            }

            return res;
        }
    }
}