using Mall.Core;
using Mall.IServices;
using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.DTO;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    public class CustomerServiceApplication
    {
		//private static ICustomerService _customerService =  EngineContext.Current.Resolve<ICustomerService>();


		private static ICustomerService _customerService =  EngineContext.Current.Resolve<ICustomerService>();

		/// <summary>
		/// 获取平台的客服信息
		/// </summary>
		/// <param name="isOpen">是否开启</param>
		/// <param name="isMobile">是否适用于移动端</param>
		public static List<CustomerService> GetPlatformCustomerService(bool isOpen=false, bool isMobile=false)
		{
			return _customerService.GetPlatformCustomerService(isOpen, isMobile).Map<List<CustomerService>>();
		}

		/// <summary>
		/// 获取门店可用售前客服
		/// </summary>
		/// <param name="shopId"></param>
		/// <returns></returns>
		public static List<CustomerService> GetPreSaleByShopId(long shopId)
		{
			return _customerService.GetPreSaleByShopId(shopId).Map<List<CustomerService>>();
		}

        /// <summary>
        /// 获取门店可用售后客服(美洽客服不分售后、售前)
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static List<CustomerService> GetAfterSaleByShopId(long shopId)
		{
			return _customerService.GetAfterSaleByShopId(shopId).Map<List<CustomerService>>();
		}

		/// <summary>
		/// 获取移动端客服
		/// </summary>
		/// <param name="shopId"></param>
		/// <returns></returns>
		public static List<CustomerService> GetMobileCustomerService(long shopId)
		{
			return _customerService.GetMobileCustomerService(shopId).Map<List<CustomerService>>();
        }

        /// <summary>
        /// 获取移动端客服且包含美洽(如有美洽，美洽存放第一个位置)
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static List<CustomerService> GetMobileCustomerServiceAndMQ(long shopId)
        {
            var customerServices = _customerService.GetMobileCustomerService(shopId).Map<List<CustomerService>>();
            var meiqia = _customerService.GetPreSaleByShopId(shopId).Map<List<CustomerService>>().FirstOrDefault(p => p.Tool == Entities.CustomerServiceInfo.ServiceTool.MeiQia);
            if (meiqia != null)
            {
                if (customerServices == null)
                    customerServices = new List<CustomerService>();
                customerServices.Insert(0, meiqia);
            }
            return customerServices;
        }

        /// <summary>
        /// 更新平台客服信息
        /// </summary>
        /// <param name="models"></param>
        public static void UpdatePlatformService(IEnumerable<CustomerService> models)
		{
			var css = models.Map<List<CustomerServiceInfo>>();
			_customerService.UpdatePlatformService(css);
		}

		/// <summary>
		/// 添加客服
		/// </summary>
		/// <param name="customerService">客服信息</param>
		public static long AddCustomerService(CustomerService model)
		{
			var cs = model.Map<CustomerServiceInfo>();
			_customerService.AddCustomerService(cs);
			model.Id = cs.Id;
			return cs.Id;
		}

		/// <summary>
		/// 添加客服
		/// </summary>
		/// <param name="model">客服信息</param>
		public static long AddPlateCustomerService(CustomerService model)
		{
			var cs = model.Map<CustomerServiceInfo>();
			_customerService.AddPlateCustomerService(cs);
			model.Id = cs.Id;
			return cs.Id;
		}
    }
}
