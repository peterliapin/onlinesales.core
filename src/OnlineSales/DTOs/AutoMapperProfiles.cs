// <copyright file="AutoMapperProfiles.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using OnlineSales.Models;

namespace OnlineSales.DTOs;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Comment, CommentCreateDto>().ReverseMap();
        CreateMap<Comment, CommentUpdateDto>().ReverseMap()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));

        CreateMap<Post, PostCreateDto>().ReverseMap();
        CreateMap<Post, PostUpdateDto>().ReverseMap()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
    }

    private static bool PropertyNeedsMapping(object source, object target, object sourceValue, object targetValue)
    {
        return sourceValue != null;
    }
}

