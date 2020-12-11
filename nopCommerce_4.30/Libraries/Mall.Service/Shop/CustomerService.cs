using Mall.Core;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Service
{
    public class CustomerService : ServiceBase, ICustomerService
    {
        public void AddCustomerService(CustomerServiceInfo customerService)
        {
            CheckPropertyWhenAdd(customerService);
            customerService.Name = customerService.Name.Trim();//去除首尾空白
            //Context.CustomerServiceInfo.Add(customerService);
            //Context.SaveChanges();
            DbFactory.Default.Add(customerService);
        }

        /// <summary>
        /// 添加客服
        /// </summary>
        /// <param name="customerService">客服信息</param>
        public void AddPlateCustomerService(CustomerServiceInfo customerService)
        {
            CheckPlatformCustomerServiceWhenAdd(customerService);
            //Context.CustomerServiceInfo.Add(customerService);
            //Context.SaveChanges();
            DbFactory.Default.Add(customerService);
        }

        public List<CustomerServiceInfo> GetCustomerService(long shopId)
        {
            //return Context.CustomerServiceInfo.Where(item => item.ShopId == shopId && item.TerminalType == CustomerServiceInfo.ServiceTerminalType.PC);
            return DbFactory.Default.Get<CustomerServiceInfo>().Where(item => item.ShopId == shopId && item.TerminalType == CustomerServiceInfo.ServiceTerminalType.PC).ToList();
        }

        public void UpdateCustomerService(CustomerServiceInfo customerService)
        {
            //检查
            var ori = CheckPropertyWhenUpdate(customerService);

            //更新信息
            ori.Name = customerService.Name;
            ori.Type = customerService.Type;
            ori.Tool = customerService.Tool;
            ori.AccountCode = customerService.AccountCode;
            ori.TerminalType = customerService.TerminalType;
            ori.ServerStatus = customerService.ServerStatus;

            //保存更改
            //Context.SaveChanges();
            DbFactory.Default.Update(ori);
        }

        public void DeleteCustomerService(long shopId, params long[] ids)
        {
            //删除
            //Context.CustomerServiceInfo.Remove(item => item.ShopId == shopId && ids.Contains(item.Id));
            //Context.SaveChanges();
            DbFactory.Default.Del<CustomerServiceInfo>(item => item.ShopId == shopId && item.Id.ExIn(ids));
        }

        public void DeleteCustomerServiceForMobile(long shopId)
        {
            //Context.CustomerServiceInfo.Remove(item => item.ShopId == shopId && item.TerminalType == CustomerServiceInfo.ServiceTerminalType.Mobile);
            //Context.SaveChanges();
            DbFactory.Default.Del<CustomerServiceInfo>(item => item.ShopId == shopId && item.TerminalType == CustomerServiceInfo.ServiceTerminalType.Mobile);
        }


        /// <summary>
        /// 添加时检查属性
        /// </summary>
        /// <param name="customerService"></param>
        void CheckPropertyWhenAdd(CustomerServiceInfo customerService)
        {
            CheckPlatformCustomerServiceWhenAdd(customerService);

            if (customerService.ShopId == 0)
                throw new InvalidPropertyException("店铺id必须大于0");
        }

        /// <summary>
        /// 添加时检查属性
        /// </summary>
        /// <param name="customerService"></param>
		void CheckPlatformCustomerServiceWhenAdd(CustomerServiceInfo customerService)
        {
            if (string.IsNullOrWhiteSpace(customerService.Name))
                throw new InvalidPropertyException("客服名称不能为空");
            if (string.IsNullOrWhiteSpace(customerService.AccountCode))
                throw new InvalidPropertyException("沟通工具账号不能为空");
        }

        /// <summary>
        /// 更新时检查属性
        /// </summary>
        /// <param name="customerService"></param>
        /// <returns>返回原始客服信息</returns>
        CustomerServiceInfo CheckPropertyWhenUpdate(CustomerServiceInfo customerService)
        {
            if (customerService.ShopId == 0)
                throw new InvalidPropertyException("店铺id必须大于0");

            return CheckPlatformCustomerServiceWhenUpdate(customerService);
        }

        /// <summary>
        /// 更新时检查属性
        /// </summary>
        /// <param name="customerService"></param>
        /// <returns>返回原始客服信息</returns>
        CustomerServiceInfo CheckPlatformCustomerServiceWhenUpdate(CustomerServiceInfo customerService)
        {
            if (string.IsNullOrWhiteSpace(customerService.Name))
                throw new InvalidPropertyException("客服名称不能为空");
            if (customerService.Id == 0)
                throw new InvalidPropertyException("客服id必须大于0");
            if (string.IsNullOrWhiteSpace(customerService.AccountCode))
                throw new InvalidPropertyException("沟通工具账号不能为空");

            //var ori = Context.CustomerServiceInfo.FirstOrDefault(item => item.Id == customerService.Id && item.ShopId == customerService.ShopId);//查找指定店铺下指定id的客服
            var ori = DbFactory.Default.Get<CustomerServiceInfo>().Where(item => item.Id == customerService.Id && item.ShopId == customerService.ShopId).FirstOrDefault();//查找指定店铺下指定id的客服
            if (ori == null)//查询不到，说明店铺id与客服id不匹配或至少有一个不存在
                throw new InvalidPropertyException("不存在id为" + customerService.Id + "的客服信息");
            return ori;
        }

        public CustomerServiceInfo GetCustomerService(long shopId, long id)
        {
            //return Context.CustomerServiceInfo.FirstOrDefault(item => item.Id == id && item.ShopId == shopId);
            return DbFactory.Default.Get<CustomerServiceInfo>().Where(item => item.Id == id && item.ShopId == shopId).FirstOrDefault();
        }

        public CustomerServiceInfo GetCustomerServiceForMobile(long shopId)
        {
            //return Context.CustomerServiceInfo.FirstOrDefault(r => r.TerminalType == CustomerServiceInfo.ServiceTerminalType.Mobile && r.ShopId == shopId);
            return DbFactory.Default.Get<CustomerServiceInfo>().Where(r => r.TerminalType == CustomerServiceInfo.ServiceTerminalType.Mobile && r.ShopId == shopId).FirstOrDefault();
        }

        public List<CustomerServiceInfo> GetMobileCustomerService(long shopId)
        {
            //return Context.CustomerServiceInfo.Where(r => r.TerminalType == CustomerServiceInfo.ServiceTerminalType.Mobile && r.ShopId == shopId).AsQueryable();
            return DbFactory.Default.Get<CustomerServiceInfo>().Where(r => r.TerminalType == CustomerServiceInfo.ServiceTerminalType.Mobile && r.ShopId == shopId).ToList();
        }

        /// <summary>
        /// 获取门店可用售前客服
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public List<CustomerServiceInfo> GetPreSaleByShopId(long shopId)
        {
            //return Context.CustomerServiceInfo.Where(p => p.ServerStatus == CustomerServiceInfo.ServiceStatusType.Open && p.Type == CustomerServiceInfo.ServiceType.PreSale && p.ShopId == shopId).ToList();
            return DbFactory.Default.Get<CustomerServiceInfo>().Where(p => p.ServerStatus == CustomerServiceInfo.ServiceStatusType.Open && p.Type == CustomerServiceInfo.ServiceType.PreSale && p.ShopId == shopId).ToList();
        }

        /// <summary>
        /// 获取门店可用售后客服(美洽客服不分售后、售前)
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public List<CustomerServiceInfo> GetAfterSaleByShopId(long shopId)
        {
            //return Context.CustomerServiceInfo.Where(p => p.ServerStatus == CustomerServiceInfo.ServiceStatusType.Open && p.Type == CustomerServiceInfo.ServiceType.AfterSale && p.ShopId == shopId).ToList();
            return DbFactory.Default.Get<CustomerServiceInfo>().Where(p => p.ServerStatus == CustomerServiceInfo.ServiceStatusType.Open && (p.Type == CustomerServiceInfo.ServiceType.AfterSale || p.Tool== CustomerServiceInfo.ServiceTool.MeiQia) && p.ShopId == shopId).ToList();
        }

        public List<CustomerServiceInfo> GetPlatformCustomerService(bool isOpen = false, bool isMobile = false)
        {
            //var result = Context.CustomerServiceInfo.Where(r => r.ShopId == 0);
            var result = DbFactory.Default.Get<CustomerServiceInfo>().Where(r => r.ShopId == 0);
            if (isOpen)
                result.Where(r => r.ServerStatus == CustomerServiceInfo.ServiceStatusType.Open);
            if (isMobile)
                result.Where(r => r.TerminalType == CustomerServiceInfo.ServiceTerminalType.All);
            return result.ToList();
        }

        public void UpdatePlatformService(IEnumerable<CustomerServiceInfo> models)
        {
            DbFactory.Default.InTransaction(() =>
            {
                foreach (var item in models)
                {
                    var ori = CheckPlatformCustomerServiceWhenUpdate(item);

                    ori.AccountCode = item.AccountCode;
                    ori.Name = item.Name;
                    ori.ServerStatus = item.ServerStatus;
                    ori.TerminalType = item.TerminalType;
                    //base.UpdateData(item);
                    DbFactory.Default.Update(ori);
                }
            });
            //this.Context.SaveChanges();
        }
    }
}
