// <copyright file="AutoMapperProfiles.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using OnlineSales.Entities;

namespace OnlineSales.DTOs;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<bool?, bool>().ConvertUsing((src, dest) => src ?? dest);
        CreateMap<int?, int>().ConvertUsing((src, dest) => src ?? dest);

        CreateMap<Comment, CommentCreateDto>().ReverseMap();
        CreateMap<Comment, CommentUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<CommentUpdateDto, Comment>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<PostCreateDto, Post>().ReverseMap();
        CreateMap<PostUpdateDto, Post>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<Post, PostUpdateDto>()            
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<OrderCreateDto, Order>().ReverseMap();
        CreateMap<OrderUpdateDto, Order>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<Order, OrderUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<OrderItemCreateDto, OrderItem>().ReverseMap();
        CreateMap<OrderItemUpdateDto, OrderItem>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<OrderItem, OrderItemUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<EmailTemplateCreateDto, EmailTemplate>().ReverseMap();
        CreateMap<EmailTemplateUpdateDto, EmailTemplate>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<EmailTemplate, EmailTemplateUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<CustomerCreateDto, Customer>().ReverseMap();
        CreateMap<CustomerUpdateDto, Customer>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));                   
        CreateMap<Customer, CustomerUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));                   

        CreateMap<EmailGroupCreateDto, EmailGroup>().ReverseMap();
        CreateMap<EmailGroupUpdateDto, EmailGroup>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        CreateMap<EmailGroup, EmailGroupUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
    }

    private static bool PropertyNeedsMapping(object source, object target, object sourceValue, object targetValue)
    {
        return sourceValue != null;
    }
}
