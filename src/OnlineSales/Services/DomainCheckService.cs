// <copyright file="DomainCheckService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using ARSoft.Tools.Net.Dns;
using DnsClient;
using HtmlAgilityPack;
using Nest;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Helpers;

namespace OnlineSales.Interfaces
{
    public class DomainCheckService : IDomainCheckService
    {
        public async Task HttpCheck(Domain d)
        {
            await HttpCheck("http://" + d.Name, d);
            if (d.HttpCheck == false)
            {
                await HttpCheck("https://" + d.Name, d);
            }
        }

        public async Task DnsCheck(Domain d)
        {
            d.DnsRecords = null;
            d.DnsCheck = false;

            try
            {               
                var lookup = new LookupClient();
                var result = await lookup.QueryAsync(d.Name, QueryType.ANY);
                if (result.Answers.Any())
                {
                    d.DnsRecords = JsonHelper.Serialize(result.Answers);
                    d.DnsCheck = true;
                }
            }
            catch
            {
                // do nothing
            }
        }

        private async Task HttpCheck(string httpUrl, Domain d)
        {
            d.HttpCheck = false;
            var responce = await RequestGetUrl(httpUrl);
            if (responce != null)
            {
                d.HttpCheck = true;
                if (responce.RequestMessage != null && responce.RequestMessage.RequestUri != null)
                {
                    d.Url = responce.RequestMessage.RequestUri.ToString();
                    HtmlWeb web = new HtmlWeb();
                    var htmlDoc = web.Load(d.Url);

                    if (htmlDoc != null)
                    {
                        d.Title = GetTitle(htmlDoc);
                        d.Description = GetDescription(htmlDoc);
                    }
                }
            }
        }

        private async Task<HttpResponseMessage?> RequestGetUrl(string url)
        {
            HttpClient client = new HttpClient();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                return await client.SendAsync(request);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string? GetTitle(HtmlDocument htmlDoc)
        {
            var htmlNode = htmlDoc.DocumentNode.SelectSingleNode("//title");
            if (htmlNode != null)
            {
                return htmlNode.InnerText;
            }

            return GetNodeContentByTag(htmlDoc, "title");
        }

        private string? GetDescription(HtmlDocument htmlDoc)
        {
            return GetNodeContentByTag(htmlDoc, "description");
        }

        private string? GetNodeContentByTag(HtmlDocument htmlDoc, string value)
        {
            var htmlNode = htmlDoc.DocumentNode.SelectSingleNode(string.Format("//meta[@name='{0}']", value));
            if (htmlNode != null)
            {
                return htmlNode.GetAttributeValue("content", null);
            }

            htmlNode = htmlDoc.DocumentNode.SelectSingleNode(string.Format("//meta[@name='og:{0}']", value));
            if (htmlNode != null)
            {
                return htmlNode.GetAttributeValue("content", null);
            }

            return null;
        }
    }
}