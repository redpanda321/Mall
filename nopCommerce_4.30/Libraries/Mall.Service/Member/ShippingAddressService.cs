using Mall.Core;
using Mall.IServices;
using Mall.Entities;
using Mall.CommonModel;
using NetRube.Data;
using System.Collections.Generic;
using Mall.Application;

namespace Mall.Service
{
    public class ShippingAddressService : ServiceBase, IShippingAddressService
    {
        public void AddShippingAddress(ShippingAddressInfo shipinfo)
        {
            shipinfo.IsDefault = false;
            shipinfo.IsQuick = false;
            //if (Context.ShippingAddressInfo.Where(a => a.UserId == shipinfo.UserId).Count() >= 20)
            if (DbFactory.Default.Get<ShippingAddressInfo>().Where(a => a.UserId == shipinfo.UserId).Count() >= 20)
            {
                throw new MallException("收货地址最多只能创建20个！");
            }
            //Context.ShippingAddressInfo.Add(shipinfo);
            //Context.SaveChanges();
            DbFactory.Default.Add(shipinfo);
            //SetDefaultShippingAddress(shipinfo.Id, shipinfo.UserId);
        }

        public void SetDefaultShippingAddress(long id, long userId)
        {
            //var model = Context.ShippingAddressInfo.Where(a => a.UserId == userId);
            var model = DbFactory.Default.Get<ShippingAddressInfo>().Where(a => a.UserId == userId).ToList();
            foreach (var m in model)
            {
                bool isdefault = false;
                if (m.Id == id)
                {
                    isdefault = true;
                }
                DbFactory.Default.Set<ShippingAddressInfo>().Set(n => n.IsDefault, isdefault).Where(p => p.Id == m.Id).Succeed();
            }
            //Context.SaveChanges();
        }


        public void SetQuickShippingAddress(long id, long userId)
        {
            //var model = Context.ShippingAddressInfo.Where(a => a.UserId == userId);
            var model = DbFactory.Default.Get<ShippingAddressInfo>().Where(a => a.UserId == userId).ToList();
            foreach (var m in model)
            {
                bool isquick = false;
                if (m.Id == id)
                {
                    isquick = true;
                }
                DbFactory.Default.Set<ShippingAddressInfo>().Set(n => n.IsQuick, isquick).Where(p => p.Id == m.Id).Succeed();
            }
            //Context.SaveChanges();
        }


        public void UpdateShippingAddress(ShippingAddressInfo shipinfo)
        {
            //var model = Context.ShippingAddressInfo.Where(a => a.Id == shipinfo.Id && a.UserId == shipinfo.UserId).FirstOrDefault();
            var model = DbFactory.Default.Get<ShippingAddressInfo>().Where(a => a.Id == shipinfo.Id && a.UserId == shipinfo.UserId).FirstOrDefault();
            if (model == null)
            {
                throw new Mall.Core.MallException("该收货地址不存在或已被删除！");
            }
            model.Phone = shipinfo.Phone;
            model.RegionId = shipinfo.RegionId;
            model.ShipTo = shipinfo.ShipTo;
            model.Address = shipinfo.Address;
            model.Latitude = shipinfo.Latitude;
            model.Longitude = shipinfo.Longitude;
            model.AddressDetail = shipinfo.AddressDetail;
            //Context.SaveChanges();
            DbFactory.Default.Update(model);
            Cache.Remove(CacheKeyCollection.CACHE_SHIPADDRESS(shipinfo.Id));
        }

        public void DeleteShippingAddress(long id, long userId)
        {
            //var model = Context.ShippingAddressInfo.Where(a => a.Id == id && a.UserId == userId).FirstOrDefault();
            var model = DbFactory.Default.Get<ShippingAddressInfo>().Where(a => a.Id == id && a.UserId == userId).FirstOrDefault();
            if (model == null)
            {
                throw new Mall.Core.MallException("该收货地址不存在或已被删除！");
            }
            bool isDefault = model.IsDefault;
            //Context.ShippingAddressInfo.Remove(model);
            DbFactory.Default.Del(model);

            if (isDefault)
            {
                //var newModel = Context.ShippingAddressInfo.FirstOrDefault();
                var newModel = DbFactory.Default.Get<ShippingAddressInfo>().FirstOrDefault();
                if (newModel != null)
                {
                    //newModel.IsDefault = true;
                    DbFactory.Default.Set<ShippingAddressInfo>().Set(n => n.IsDefault, true).Where(p => p.Id == newModel.Id).Succeed();
                }
            }
            //Context.SaveChanges();

        }

        public List<ShippingAddressInfo> GetUserShippingAddressByUserId(long userId)
        {
            var regionService = ServiceProvider.Instance<IRegionService>.Create;
            var siteSettingService = SiteSettingApplication.SiteSettings;
            var model = DbFactory.Default.Get<ShippingAddressInfo>().Where(a => a.UserId == userId).OrderByDescending(a => a.Id).ToList();
            foreach (var m in model)
            {
                m.RegionFullName = regionService.GetFullName(m.RegionId);
                m.RegionIdPath = regionService.GetRegionPath(m.RegionId);
                m.NeedUpdate = (m.Latitude == 0 || m.Longitude == 0) && siteSettingService.IsOpenStore;
            }
            return model;
        }


        public ShippingAddressInfo GetUserShippingAddress(long shippingAddressId)
        {
            string cacheKey = CacheKeyCollection.CACHE_SHIPADDRESS(shippingAddressId);
            if (Cache.Exists(cacheKey))
                return Cache.Get<ShippingAddressInfo>(cacheKey);
            var regionService = ServiceProvider.Instance<IRegionService>.Create;
            var address = DbFactory.Default.Get<ShippingAddressInfo>().Where(p => p.Id == shippingAddressId).FirstOrDefault();
            if (address == null)
            {
                throw new MallException("错误的收货地址！");
            }
            address.RegionFullName = regionService.GetFullName(address.RegionId);
            address.RegionIdPath = regionService.GetRegionPath(address.RegionId);
            address.NeedUpdate = (address.Latitude == 0 || address.Longitude == 0) && SiteSettingApplication.SiteSettings.IsOpenStore;
            Cache.Insert(cacheKey, address, 1800);
            return address;
        }


        public ShippingAddressInfo GetDefaultUserShippingAddressByUserId(long userId)
        {
            //优先选择默认地址
            //ShippingAddressInfo defaultShippingAddressInfo = Context.ShippingAddressInfo.FirstOrDefault(item => item.UserId == userId && item.IsDefault);
            ShippingAddressInfo address = DbFactory.Default.Get<ShippingAddressInfo>().Where(item => item.UserId == userId && item.IsDefault==true).FirstOrDefault();

            //默认地址不存在时，选择最后一个添加的地址
            if (address == null)
                address = DbFactory.Default.Get<ShippingAddressInfo>().Where(item => item.UserId == userId).OrderByDescending(item => item.Id).FirstOrDefault();

            if (address != null)
            {
                var regionService = ServiceProvider.Instance<IRegionService>.Create;

                // defaultShippingAddressInfo.RegionFullName = regionService.GetRegionFullName(defaultShippingAddressInfo.RegionId);
                // defaultShippingAddressInfo.RegionIdPath = regionService.GetRegionIdPath(defaultShippingAddressInfo.RegionId);
                address.RegionFullName = regionService.GetFullName(address.RegionId);
                address.RegionIdPath = regionService.GetRegionPath(address.RegionId);
                address.NeedUpdate = (address.Latitude == 0 || address.Longitude == 0) && SiteSettingApplication.SiteSettings.IsOpenStore;
            }
            return address;
        }
    }
}
