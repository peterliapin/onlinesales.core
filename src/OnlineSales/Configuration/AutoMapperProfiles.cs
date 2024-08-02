// <copyright file="AutoMapperProfiles.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using OnlineSales.DTOs;
using OnlineSales.Entities;

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
        CreateMap<CommentStatus?, CommentStatus>().ConvertUsing((src, dest) => src ?? dest);

        CreateMap<DateTime, DateTime>().ConvertUsing(new DateTimeToUtcConverter());
        CreateMap<DateTime?, DateTime?>().ConvertUsing(new DateTimeToUtcConverter());
        CreateMap<DateTime?, DateTime>().ConvertUsing(new DateTimeToUtcConverter());

        CreateMap<Comment, CommentCreateDto>().ReverseMap();
        CreateMap<Comment, CommentCreateBaseDto>().ReverseMap();
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

        CreateMap<DealPipelineCreateDto, DealPipeline>().ReverseMap();
        CreateMap<DealPipelineUpdateDto, DealPipeline>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<DealPipeline, DealPipelineUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<DealPipeline, DealPipelineDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<DealCreateDto, Deal>().ReverseMap();
        CreateMap<DealUpdateDto, Deal>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<Deal, DealUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<Deal, DealDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<PromotionCreateDto, Promotion>().ReverseMap();
        CreateMap<PromotionUpdateDto, Promotion>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<Promotion, PromotionUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<Promotion, PromotionDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<DealPipelineStageCreateDto, DealPipelineStage>().ReverseMap();
        CreateMap<DealPipelineStageUpdateDto, DealPipelineStage>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<DealPipelineStage, DealPipelineStageUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<DealPipelineStage, DealPipelineStageDetailsDto>()
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
        CreateMap<LinkImportDto, Link>()
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

        CreateMap<User, UserCreateDto>().ReverseMap();
        CreateMap<User, UserUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<UserUpdateDto, User>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<User, UserDetailsDto>();

        CreateMap<ActivityLog, ActivityLogDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<Unsubscribe, UnsubscribeDto>().ReverseMap();
        CreateMap<Unsubscribe, UnsubscribeDetailsDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<UnsubscribeImportDto, Unsubscribe>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
    }

    private static bool PropertyNeedsMapping(object source, object target, object sourceValue, object targetValue)
    {
        return sourceValue != null;
    }
}

public class DateTimeToUtcConverter : ITypeConverter<DateTime, DateTime>, ITypeConverter<DateTime?, DateTime?>, ITypeConverter<DateTime?, DateTime>
{
    public DateTime Convert(DateTime source, DateTime destination, ResolutionContext context)
    {
        if (source.Kind == DateTimeKind.Unspecified/* || source.Kind == DateTimeKind.Local*/)
        {
            return DateTime.SpecifyKind(source, DateTimeKind.Utc);
        }

        return source;
    }

    public DateTime? Convert(DateTime? source, DateTime? destination, ResolutionContext context)
    {
        if (source == null)
        {
            return destination;
        }

        return Convert(source.Value, destination ?? DateTime.MinValue, context);
    }

    public DateTime Convert(DateTime? source, DateTime destination, ResolutionContext context)
    {
        if (source == null)
        {
            return destination;
        }

        return Convert(source.Value, destination, context);
    }
}