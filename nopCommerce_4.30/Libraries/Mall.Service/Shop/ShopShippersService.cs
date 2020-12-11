using Mall.Core;
using Mall.IServices;
using Mall.Entities;
using System.Collections.Generic;
using NetRube.Data;

namespace Mall.Service
{
    public class ShopShippersService : ServiceBase, IShopShippersServiceService
    {
        /// <summary>
        /// 获得默认发货地址信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public ShopShipperInfo GetDefaultSendGoodsShipper(long shopId)
        {
            //return Context.ShopShippersInfo.FirstOrDefault(d => d.ShopId == shopId && d.IsDefaultSendGoods == true);
            return DbFactory.Default.Get<ShopShipperInfo>().Where(d => d.ShopId == shopId && d.IsDefaultSendGoods == true).FirstOrDefault();
        }
        /// <summary>
        /// 获得默认收货地址信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public ShopShipperInfo GetDefaultGetGoodsShipper(long shopId)
        {
            //return Context.ShopShippersInfo.FirstOrDefault(d => d.ShopId == shopId && d.IsDefaultGetGoods == true);
            return DbFactory.Default.Get<ShopShipperInfo>().Where(d => d.ShopId == shopId && d.IsDefaultGetGoods == true).FirstOrDefault();
        }
        /// <summary>
        /// 获得默认核销地址信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public ShopShipperInfo GetDefaultVerificationShipper(long shopId)
        {
            return DbFactory.Default.Get<ShopShipperInfo>().Where(d => d.ShopId == shopId && d.IsDefaultVerification.ExIfNull(false) == true).FirstOrDefault();
        }
        /// <summary>
        /// 设置默认发货地址信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="id"></param>
        public void SetDefaultSendGoodsShipper(long shopId, long id)
        {
            //var datalist = Context.ShopShippersInfo.Where(d => d.ShopId == shopId && d.IsDefaultSendGoods == true).ToList();
            //var datalist = DbFactory.Default.Get<ShopShipperInfo>().Where(d => d.ShopId == shopId && d.IsDefaultSendGoods == true).ToList();
            ////var data = Context.ShopShippersInfo.FirstOrDefault(d => d.ShopId == shopId && d.Id == id);
            //var data = DbFactory.Default.Get<ShopShipperInfo>().Where(d => d.ShopId == shopId && d.Id == id).FirstOrDefault();
            //if (data == null)
            //{
            //    throw new MallException("错误的参数");
            //}
            //foreach (var item in datalist)
            //{
            //    //item.IsDefaultSendGoods = false;
            //    DbFactory.Default.Set<ShopShipperInfo>().Set(n => n.IsDefaultSendGoods, false).Where(p => p.Id == item.Id && p.ShopId == shopId).Succeed();
            //}
            //DbFactory.Default.Set<ShopShipperInfo>().Set(n => n.IsDefaultSendGoods, true).Where(p => p.Id == data.Id && p.ShopId == shopId).Succeed();
            DbFactory.Default.InTransaction(() =>
            {
                var flag = DbFactory.Default.Set<ShopShipperInfo>().Set(n => n.IsDefaultSendGoods, false).Where(d => d.ShopId == shopId && d.IsDefaultSendGoods == true).Succeed();
                if (!flag) throw new MallException("错误的参数");
                DbFactory.Default.Set<ShopShipperInfo>().Set(n => n.IsDefaultSendGoods, true).Where(p => p.Id == id && p.ShopId == shopId).Succeed();
            });
            //data.IsDefaultSendGoods = true;
            //Context.SaveChanges();
        }
        /// <summary>
        /// 设置默认收货地址信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="id"></param>
        public void SetDefaultGetGoodsShipper(long shopId, long id)
        {
            //var datalist = Context.ShopShippersInfo.Where(d => d.ShopId == shopId && d.IsDefaultGetGoods == true).ToList();
            //var datalist = DbFactory.Default.Get<ShopShipperInfo>().Where(d => d.ShopId == shopId && d.IsDefaultGetGoods == true).ToList();
            //var data = DbFactory.Default.Get<ShopShipperInfo>().Where(d => d.ShopId == shopId && d.Id == id).FirstOrDefault();
            //if (data == null)
            //{
            //    throw new MallException("错误的参数");
            //}
            //foreach (var item in datalist)
            //{
            //    //item.IsDefaultGetGoods = false;
            //    DbFactory.Default.Set<ShopShipperInfo>().Set(n => n.IsDefaultGetGoods, false).Where(p => p.Id == item.Id && p.ShopId == shopId).Succeed();
            //}
            //DbFactory.Default.Set<ShopShipperInfo>().Set(n => n.IsDefaultGetGoods, true).Where(p => p.Id == data.Id && p.ShopId == shopId).Succeed();
            //data.IsDefaultGetGoods = true;
            //Context.SaveChanges();
            DbFactory.Default.InTransaction(() =>
            {
                var flag = DbFactory.Default.Set<ShopShipperInfo>().Set(n => n.IsDefaultGetGoods, false).Where(p => p.IsDefaultGetGoods == true && p.ShopId == shopId).Succeed();
                if (!flag) throw new MallException("错误的参数");
                DbFactory.Default.Set<ShopShipperInfo>().Set(n => n.IsDefaultGetGoods, true).Where(p => p.Id == id && p.ShopId == shopId).Succeed();
            });
        }
        /// <summary>
        /// 设置默认核销地址信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="id"></param>
        public void SetDefaultVerificationShipper(long shopId, long id)
        {
            var result = DbFactory.Default.Get<ShopShipperInfo>().Where(d => d.ShopId == shopId && d.Id == id).FirstOrDefault();
            if (result == null) throw new MallException("错误的参数");
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Set<ShopShipperInfo>().Set(n => n.IsDefaultVerification, false).Where(p => p.IsDefaultVerification == true && p.ShopId == shopId).Succeed();
                DbFactory.Default.Set<ShopShipperInfo>().Set(n => n.IsDefaultVerification, true).Where(p => p.Id == id && p.ShopId == shopId).Succeed();
            });
        }

        /// <summary>
        /// 获取所有发收货地址
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public List<ShopShipperInfo> GetShopShippers(long shopId)
        {
            //return Context.ShopShippersInfo.Where(d => d.ShopId == shopId).ToList();
            return DbFactory.Default.Get<ShopShipperInfo>().Where(d => d.ShopId == shopId).ToList();
        }

        /// <summary>
        /// 添加发收货地址
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="data"></param>
        public void Add(long shopId, ShopShipperInfo data)
        {
            //if (Context.ShopShippersInfo.Count(d => d.ShopId == shopId) == 0)
            if (!DbFactory.Default.Get<ShopShipperInfo>().Where(d => d.ShopId == shopId).Exist())
            {
                data.IsDefaultGetGoods = true;
                data.IsDefaultSendGoods = true;
                data.IsDefaultVerification = true;
            }
            data.ShopId = shopId;
            //Context.ShopShippersInfo.Add(data);
            //Context.SaveChanges();
            DbFactory.Default.Add(data);
        }

        /// <summary>
        /// 修改发收货地址
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="data"></param>
        public void Update(long shopId, ShopShipperInfo data)
        {
            //var _d = Context.ShopShippersInfo.FirstOrDefault(d => d.Id == data.Id && d.ShopId == shopId);
            var _d = DbFactory.Default.Get<ShopShipperInfo>().Where(d => d.Id == data.Id && d.ShopId == shopId).FirstOrDefault();
            if (_d == null)
            {
                throw new MallException("错误的参数");
            }
            _d.ShipperTag = data.ShipperTag;
            _d.ShipperName = data.ShipperName;
            _d.TelPhone = data.TelPhone;
            _d.IsDefaultGetGoods = data.IsDefaultGetGoods;
            _d.IsDefaultSendGoods = data.IsDefaultSendGoods;
            _d.Latitude = data.Latitude;
            _d.Longitude = data.Longitude;
            _d.RegionId = data.RegionId;
            _d.Address = data.Address;
            _d.ShopId = shopId;
            _d.WxOpenId = data.WxOpenId;
            _d.Zipcode = data.Zipcode;
            //Context.SaveChanges();
            DbFactory.Default.Update(_d);
        }

        /// <summary>
        /// 删除发收货地址
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="id"></param>
        public void Delete(long shopId, long id)
        {
            //var _d = Context.ShopShippersInfo.FirstOrDefault(d => d.Id == id && d.ShopId == shopId);
            var _d = DbFactory.Default.Get<ShopShipperInfo>().Where(d => d.Id == id && d.ShopId == shopId).FirstOrDefault();
            if (_d == null)
            {
                throw new MallException("错误的参数");
            }
            if (_d.IsDefaultGetGoods || _d.IsDefaultSendGoods)
            {
                throw new MallException("不能删除默认的发货/收货信息");
            }
            //Context.ShopShippersInfo.Remove(_d);
            //Context.SaveChanges();
            DbFactory.Default.Del(_d);
        }

        public ShopShipperInfo GetShopShipper(long shopId, long id)
        {
            //return Context.ShopShippersInfo.FirstOrDefault(d => d.ShopId == shopId && d.Id == id);
            return DbFactory.Default.Get<ShopShipperInfo>().Where(d => d.ShopId == shopId && d.Id == id).FirstOrDefault();
        }
    }
}
