using AutoMapper;
using Mall.DTO;
using Mall.Entities;

namespace Mall.Web.Mappers.Profiles
{
    public class CommonProfile : Profile
    {
      public CommonProfile()
        {


            CreateMap<Models.SiteSettingModel, SiteSettings>();
            CreateMap<SiteSettings, Models.SiteSettingModel>();

        }
    }
}
