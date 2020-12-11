using AutoMapper;
using Nop.Core.Infrastructure.Mapper;



namespace Mall.Web.Mappers
{
    public static class Configuration
    {
        public static void InitConfiguration()
        {
            
            /*
            Mapper.Initialize(cfg =>
            {
				cfg.AddProfile<Mall.Application.Mappers.Profiles.OrderProfile>();
                cfg.AddProfile<Mall.Application.Mappers.Profiles.ShopProfile>();
				cfg.AddProfile<Mall.Application.Mappers.Profiles.MemberProfile>();
				cfg.AddProfile<Mall.Application.Mappers.Profiles.ProductProfile>();
				cfg.AddProfile<Mall.Application.Mappers.Profiles.FreightTemplateProfile>();
				cfg.AddProfile<Mall.Application.Mappers.Profiles.CommonProfile>();
				cfg.AddProfile<Mall.Application.Mappers.Profiles.FullDiscountProfile>();
				cfg.AddProfile<Mall.Application.Mappers.Profiles.ShopShipperProfile>();
				cfg.AddProfile<Mall.Application.Mappers.Profiles.RechargePresentRuleProfile>();
				cfg.AddProfile<Mall.Application.Mappers.Profiles.DistributionProfile>();
            }); 
            */

            
            var config = new MapperConfiguration(cfg =>
            {
               
                cfg.AddProfile<Mall.Application.Mappers.Profiles.CommonProfile>();
               
            });

            AutoMapperConfiguration.Init(config);
            
        }

		public static TDestination Map<TDestination>(this object source)
		{
           
                return AutoMapperConfiguration.Mapper.Map<TDestination>(source);


            }

		public static void Map<TSource, TDestination>(this TSource source, TDestination target)
		{
            AutoMapperConfiguration.Mapper.Map(source, target);
		}

		public static void DynamicMap<TSource, TDestination>(this TSource source, TDestination target)
		{
            AutoMapperConfiguration.Mapper.Map(source, target);
		}

		public static TDestination DynamicMap<TDestination>(this object source)
		{
			return AutoMapperConfiguration.Mapper.Map<TDestination>(source);
		}
    }
}
