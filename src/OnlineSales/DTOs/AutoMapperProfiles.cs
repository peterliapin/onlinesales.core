﻿// <copyright file="AutoMapperProfiles.cs" company="WavePoint Co. Ltd.">
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
    }

    private static bool PropertyNeedsMapping(object source, object target, object sourceValue, object targetValue)
    {
        return sourceValue != null;
    }
}
