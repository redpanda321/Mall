using AutoMapper;

namespace Mall.Application.Mappers.Profiles
{
    public class FreightTemplateProfile : Profile
    {
        public FreightTemplateProfile()
        {
       
            ////模板基本信息
            //CreateMap<Entities.FreightTemplateInfo, DTO.FreightTemplate>();

            //CreateMap<DTO.FreightTemplate, Entities.FreightTemplateInfo>().ForMember(e => e.FreightAreaContentInfo, (map) => map.MapFrom(m => m.FreightArea));
            ////模板地区

            //CreateMap<DTO.FreightArea, Entities.FreightAreaContentInfo>().ForMember(e => e.FreightAreaDetailInfo, (map) => map.MapFrom(m => m.FreightAreaDetail));

            ////模板地区详情
            //CreateMap<Entities.FreightAreaDetailInfo, DTO.FreightAreaDetail>();
            //CreateMap<DTO.FreightAreaDetail, Entities.FreightAreaDetailInfo>();

            //模板基本信息
            
            CreateMap<Entities.FreightTemplateInfo, DTO.FreightTemplate>();

            CreateMap<DTO.FreightTemplate, Entities.FreightTemplateInfo>().ForMember(e => e.FreightAreaContentInfo, (map) => map.MapFrom(m => m.FreightArea));
            //模板地区

            CreateMap<DTO.FreightArea, Entities.FreightAreaContentInfo>().ForMember(e => e.FreightAreaDetailInfo, (map) => map.MapFrom(m => m.FreightAreaDetail));
            CreateMap<Entities.FreightAreaContentInfo, DTO.FreightArea>();
            //模板地区详情
            CreateMap<Entities.FreightAreaDetailInfo, DTO.FreightAreaDetail>();
            CreateMap<DTO.FreightAreaDetail, Entities.FreightAreaDetailInfo>();
        }
    }
}
