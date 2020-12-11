using AutoMapper;
using Mall.DTO;
using Mall.Entities;

namespace Mall.Application.Mappers.Profiles
{
    public class DistributionProfile : Profile
	{
        public DistributionProfile()
		{
			
			CreateMap<DistributorInfo, DTO.Distribution.DistributorListDTO>();
            CreateMap<DistributorInfo, Distributor>();
            CreateMap<DistributionWithdrawInfo, DistributionWithdraw>();
        }
	}
}
