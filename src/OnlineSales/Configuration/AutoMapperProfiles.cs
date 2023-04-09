// <copyright file="AutoMapperProfiles.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Interfaces;

namespace OnlineSales.Configuration;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<bool?, bool>().ConvertUsing((src, dest) => src ?? dest);
        CreateMap<int?, int>().ConvertUsing((src, dest) => src ?? dest);
        CreateMap<decimal?, decimal>().ConvertUsing((src, dest) => src ?? dest);
        CreateMap<List<DnsRecord>?, List<DnsRecord>>().ConvertUsing((src, dest) => src ?? dest);
        CreateMap<Dictionary<string, string>?, Dictionary<string, string>>().ConvertUsing((src, dest) => src ?? dest);
        CreateMap<string?[], string?[]>().ConvertUsing((src, dest) => src ?? dest);

        CreateMap<Comment, CommentCreateDto>().ReverseMap();
        CreateMap<Comment, CommentUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<CommentUpdateDto, Comment>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<Comment, CommentDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<CommentImportDto, Comment>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<ContentCreateDto, Content>().ReverseMap();
        CreateMap<ContentUpdateDto, Content>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<Content, ContentUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<Content, ContentDetailsDto>()
           .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<ContentImportDto, Content>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<OrderCreateDto, Order>().ReverseMap();
        CreateMap<OrderUpdateDto, Order>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<Order, OrderUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<Order, OrderDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<OrderImportDto, Order>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<OrderItemCreateDto, OrderItem>().ReverseMap();
        CreateMap<OrderItemUpdateDto, OrderItem>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<OrderItem, OrderItemUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<OrderItem, OrderItemDetailsDto>()
           .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<OrderItemImportDto, OrderItem>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<EmailTemplateCreateDto, EmailTemplate>().ReverseMap();
        CreateMap<EmailTemplateUpdateDto, EmailTemplate>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<EmailTemplate, EmailTemplateUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<EmailTemplate, EmailTemplateDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<ContactCreateDto, Contact>().ReverseMap();
        CreateMap<ContactUpdateDto, Contact>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<Contact, ContactUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<Contact, ContactDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<ContactImportDto, Contact>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<EmailGroupCreateDto, EmailGroup>().ReverseMap();
        CreateMap<EmailGroupUpdateDto, EmailGroup>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<EmailGroup, EmailGroupUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<EmailGroup, EmailGroupDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<OrderItem, OrderItemDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<Order, OrderDetailsDto>()
           .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<Link, LinkCreateDto>().ReverseMap();
        CreateMap<Link, LinkUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<LinkUpdateDto, Link>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<Link, LinkDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<Domain, DomainCreateDto>().ReverseMap();
        CreateMap<Domain, DomainUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<DomainUpdateDto, Domain>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<Domain, DomainDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<DomainImportDto, Domain>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<Domain, EmailVerifyDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<Account, AccountCreateDto>().ReverseMap();
        CreateMap<Account, AccountUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<AccountUpdateDto, Account>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<Account, AccountDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<AccountImportDto, Account>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<AccountDetailsInfo, Account>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
    }

    private static bool PropertyNeedsMapping(object source, object target, object sourceValue, object targetValue)
    {
        return sourceValue != null;
    }
}