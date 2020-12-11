using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Mall.Entities;

namespace Mall.Application.Mappers.Profiles
{
    public class ShopShipperProfile : Profile
    {
        public ShopShipperProfile()
        {
         

            CreateMap<ShopShipperInfo, DTO.ShopShipper>();
            CreateMap<DTO.ShopShipper, ShopShipperInfo>();

        }
    }
}
