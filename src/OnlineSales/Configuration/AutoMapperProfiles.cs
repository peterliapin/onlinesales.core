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
        this.CreateMap<bool?, bool>().ConvertUsing((src, dest) => src ?? dest);
        this.CreateMap<int?, int>().ConvertUsing((src, dest) => src ?? dest);
        this.CreateMap<decimal?, decimal>().ConvertUsing((src, dest) => src ?? dest);
        this.CreateMap<List<DnsRecord>?, List<DnsRecord>>().ConvertUsing((src, dest) => src ?? dest);
        this.CreateMap<Dictionary<string, string>?, Dictionary<string, string>>().ConvertUsing((src, dest) => src ?? dest);
        this.CreateMap<string?[], string?[]>().ConvertUsing((src, dest) => src ?? dest);
        this.CreateMap<DateTime?, DateTime>().ConvertUsing((src, dest) => src ?? dest);
        this.CreateMap<CommentStatus?, CommentStatus>().ConvertUsing((src, dest) => src ?? dest);

        this.CreateMap<Comment, CommentCreateDto>().ReverseMap();
        this.CreateMap<Comment, CommentUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<CommentUpdateDto, Comment>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<Comment, CommentDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<CommentImportDto, Comment>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        this.CreateMap<ContentCreateDto, Content>().ReverseMap();
        this.CreateMap<ContentUpdateDto, Content>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<Content, ContentUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<Content, ContentDetailsDto>()
           .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<ContentImportDto, Content>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        this.CreateMap<OrderCreateDto, Order>().ReverseMap();
        this.CreateMap<OrderUpdateDto, Order>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<Order, OrderUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<Order, OrderDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<OrderImportDto, Order>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        this.CreateMap<OrderItemCreateDto, OrderItem>().ReverseMap();
        this.CreateMap<OrderItemUpdateDto, OrderItem>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<OrderItem, OrderItemUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<OrderItem, OrderItemDetailsDto>()
           .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<OrderItemImportDto, OrderItem>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        this.CreateMap<EmailTemplateCreateDto, EmailTemplate>().ReverseMap();
        this.CreateMap<EmailTemplateUpdateDto, EmailTemplate>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<EmailTemplate, EmailTemplateUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<EmailTemplate, EmailTemplateDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        this.CreateMap<ContactCreateDto, Contact>().ReverseMap();
        this.CreateMap<ContactUpdateDto, Contact>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<Contact, ContactUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<Contact, ContactDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<ContactImportDto, Contact>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        this.CreateMap<EmailGroupCreateDto, EmailGroup>().ReverseMap();
        this.CreateMap<EmailGroupUpdateDto, EmailGroup>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<EmailGroup, EmailGroupUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<EmailGroup, EmailGroupDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        this.CreateMap<OrderItem, OrderItemDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        this.CreateMap<Order, OrderDetailsDto>()
           .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        this.CreateMap<Link, LinkCreateDto>().ReverseMap();
        this.CreateMap<Link, LinkUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<LinkUpdateDto, Link>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<Link, LinkDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        this.CreateMap<Domain, DomainCreateDto>().ReverseMap();
        this.CreateMap<Domain, DomainUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<DomainUpdateDto, Domain>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<Domain, DomainDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<DomainImportDto, Domain>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<Domain, EmailVerifyDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        this.CreateMap<Account, AccountCreateDto>().ReverseMap();
        this.CreateMap<Account, AccountUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<AccountUpdateDto, Account>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<Account, AccountDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<AccountImportDto, Account>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<AccountDetailsInfo, Account>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        this.CreateMap<User, UserCreateDto>().ReverseMap();
        this.CreateMap<User, UserUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<UserUpdateDto, User>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<User, UserDetailsDto>();
    }

    private static bool PropertyNeedsMapping(object source, object target, object sourceValue, object targetValue)
    {
        return sourceValue != null;
    }
}