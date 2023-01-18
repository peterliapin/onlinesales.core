// <copyright file="DomainsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using DnsClient;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineSales.Configuration;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Helpers;

namespace OnlineSales.Controllers;

[Authorize]
[Route("api/[controller]")]
public class DomainsController : BaseControllerWithImport<Domain, DomainCreateDto, DomainUpdateDto, DomainDetailsDto, DomainImportDto>
{
    public DomainsController(ApiDbContext dbContext, IMapper mapper, IOptions<ApiSettingsConfig> apiSettingsConfig)
        : base(dbContext, mapper, apiSettingsConfig)
    {
    }

    // GET api/domains/names/gmail.com
    [HttpGet("names/{name}")]    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DomainDetailsDto>> GetOne(string name)
    {
        var existingEntity = from d in this.dbSet where d.Name == name select d;

        if (existingEntity != null && existingEntity.Any())
        {
            var domain = await existingEntity.FirstAsync();
            return mapper.Map<DomainDetailsDto>(domain);
        }
        else
        {
            var domain = new Domain();
            domain.Name = name;

            await HttpCheck("http://" + name, domain);
            if (domain.HttpCheck == false)
            {
                await HttpCheck("https://" + name, domain);
            }

            await GetDnsRecords(name, domain);

            var result = await dbSet.AddAsync(domain);
            await dbContext.SaveChangesAsync();

            return await GetOne(result.Entity.Id);
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
                    d.Title = GetNodeContent(htmlDoc, "title");
                    d.Description = GetNodeContent(htmlDoc, "description");
                }
            }
        }
    }

    private async Task GetDnsRecords(string domainName, Domain d)
    {
        d.DnsRecords = null;
        d.DnsCheck = false;

        try
        {
            var lookup = new LookupClient();
            var result = await lookup.QueryAsync(domainName, QueryType.MX);
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

    private string? GetNodeContent(HtmlDocument htmlDoc, string node)
    {
        var htmlNode = htmlDoc.DocumentNode.SelectSingleNode(string.Format("//meta[@name='{0}']", node));
        if (htmlNode != null)
        {
            return htmlNode.GetAttributeValue("content", null);
        }

        return null;
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
}

