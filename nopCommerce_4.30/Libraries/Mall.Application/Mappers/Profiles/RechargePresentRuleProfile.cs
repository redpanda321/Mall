using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Mall.Entities;

namespace Mall.Application.Mappers.Profiles
{
    public class RechargePresentRuleProfile : Profile
    {
        public RechargePresentRuleProfile()
        {
           

            CreateMap<RechargePresentRuleInfo, DTO.RechargePresentRule>();
            CreateMap<DTO.RechargePresentRule, RechargePresentRuleInfo>();
        }
    }
}
