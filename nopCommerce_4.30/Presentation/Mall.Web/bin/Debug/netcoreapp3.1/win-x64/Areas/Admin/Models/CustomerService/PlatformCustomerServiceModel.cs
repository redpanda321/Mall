using System;
using Mall.DTO;

namespace Mall.Web.Areas.Admin.Models
{
    public class PlatformCustomerServiceModel:CustomerService
    {
		static PlatformCustomerServiceModel()
		{
		//	AutoMapper.Mapper.CreateMap<CustomerService, PlatformCustomerServiceModel>();
		//	AutoMapper.Mapper.CreateMap<PlatformCustomerServiceModel, CustomerService>();
		}

		/// <summary>
		/// 准备创建时的id
		/// </summary>
		public Guid CreateId { get; set; }
    }
}