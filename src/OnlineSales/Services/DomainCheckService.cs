// <copyright file="DomainCheckService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text;
using DnsClient;
using HtmlAgilityPack;
using Microsoft.AspNetCore.OData.Formatter;
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
                    var htmlDoc = await web.LoadFromWebAsync(d.Url, Encoding.UTF8);

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

            return GetNodeContentByAttr(htmlDoc, "title");
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