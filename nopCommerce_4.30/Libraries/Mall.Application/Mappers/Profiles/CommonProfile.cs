using AutoMapper;
using Mall.DTO;
using Mall.Entities;


namespace Mall.Application.Mappers.Profiles
{
    public class CommonProfile : Profile
    {
      public CommonProfile()
        {
            

            CreateMap<CategoryInfo, DTO.Category>();
            CreateMap<DTO.Category, CategoryInfo>();

            CreateMap<ManagerInfo, DTO.Manager>();
            CreateMap<DTO.Manager, ManagerInfo>();

            CreateMap<DTO.ExpressCompany, Mall.Entities.ExpressInfoInfo>();
            CreateMap<Mall.Entities.ExpressInfoInfo, DTO.ExpressCompany>();


            CreateMap<DTO.SiteSettings, Mall.Entities.SiteSettingInfo>();
            CreateMap<Mall.Entities.SiteSettingInfo, DTO.SiteSettings>();


            CreateMap<DTO.Settled, Mall.Entities.SettledInfo>();
            CreateMap<Mall.Entities.SettledInfo, DTO.Settled>();

        }
    }
}
