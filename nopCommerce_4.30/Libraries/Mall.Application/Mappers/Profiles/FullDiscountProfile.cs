using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Mall.Entities;

namespace Mall.Application.Mappers.Profiles
{
	public class FullDiscountProfile : Profile
	{
		public FullDiscountProfile()
		{


			CreateMap<ActiveProductInfo, DTO.FullDiscountActiveProduct>();
			CreateMap<DTO.FullDiscountActiveProduct, ActiveProductInfo>();

            CreateMap<FullDiscountRuleInfo, DTO.FullDiscountRules>();
            CreateMap<DTO.FullDiscountRules, FullDiscountRuleInfo>();

            CreateMap<ActiveInfo, DTO.FullDiscountActive>();
            CreateMap<DTO.FullDiscountActive, ActiveInfo>();
            CreateMap<ActiveInfo, DTO.FullDiscountActiveBase>();
            CreateMap<DTO.FullDiscountActiveBase, ActiveInfo>();
            CreateMap<ActiveInfo, DTO.FullDiscountActiveList>();
            CreateMap<DTO.FullDiscountActiveList, ActiveInfo>();

            //CreateMap<ShopAccountItemInfo, Mall.DTO.ShopAccountItem>().ForMember(p => p.ShopAccountType, options => options.MapFrom(p => p.TradeType));
        }
	}
}
