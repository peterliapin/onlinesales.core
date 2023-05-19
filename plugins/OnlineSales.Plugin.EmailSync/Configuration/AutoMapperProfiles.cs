// <copyright file="AutoMapperProfiles.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using AutoMapper;
using OnlineSales.Plugin.EmailSync.DTOs;
using OnlineSales.Plugin.EmailSync.Entities;

namespace OnlineSales.Plugin.EmailSync.Configuration;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        this.CreateMap<ImapAccount, ImapAccountCreateDto>().ReverseMap();
        this.CreateMap<ImapAccount, ImapAccountUpdateDto>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<ImapAccountUpdateDto, ImapAccount>()
            .ForAllMembers(m => m.Condition(PropertyNeedsMapping));
        this.CreateMap<ImapAccount, ImapAccountDetailsDto>();
    }

    private static bool PropertyNeedsMapping(object source, object target, object sourceValue, object targetValue)
    {
        return sourceValue != null;
    }
}