using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Mall.Application
{
    public class ShopBonusApplication:BaseApplicaion<IShopBonusService>
    {
        /// <summary>
        ///  获取红包列表
        /// </summary>
        public static QueryPageModel<ShopBonusInfo> Get(long shopid, string name, int state, int pageIndex, int pageSize)
        {
            return Service.Get(shopid, name, state, pageIndex, pageSize);
        }

        public static List<ShopBonusReceiveInfo> GetCanUseDetailByUserId(long userid)
        {
            return Service.GetCanUseDetailByUserId(userid);
        }
        public static List<ShopBonusReceiveInfo> GetDetailByUserId(long userid)
        {
            return Service.GetDetailByUserId(userid);
        }
        public static ShopBonusInfo GetBonus(long id)
        {
            return Service.GetShopBonus(id);
        }
        public static ShopBonusReceiveInfo GetDetailById(long userid, long id)
        {
            return Service.GetDetailById(userid, id);
        }
        public static List<ShopBonusReceiveInfo> GetDetailToUse(long shopid, long userid, decimal sumprice)
        {
            return Service.GetDetailToUse(shopid, userid, sumprice);
        }
        public static QueryPageModel<ShopBonusReceiveInfo> GetDetailByQuery(CouponRecordQuery query)
        {
            return Service.GetDetailByQuery(query);
        }
        public static ShopBonusGrantInfo GetByOrderId(long orderid)
        {
            return Service.GetByOrderId(orderid);
        }
        public static decimal GetUsedPrice(long orderid, long userid)
        {
            return Service.GetUsedPrice(orderid, userid);
        }

        public static ShopBonusGrantInfo GetGrantByUserOrder(long orderid, long userid)
        {
            return Service.GetGrantByUserOrder(orderid, userid);
        }

        /// <summary>
        /// 获取红包
        /// </summary>
        public static ShopBonusInfo Get(long id)
        {
            return Service.GetShopBonus(id);
        }

        /// <summary>
        /// 根据grantid获取
        /// </summary>
        public static ShopBonusInfo GetByGrantId(long grantid)
        {
            return Service.GetByGrantId(grantid);
        }

        public static long GetGrantIdByOrderId(long orderid)
        {
            return Service.GetGrantIdByOrderId(orderid);
        }

        public static void SetBonusToUsed(long userid, List<OrderInfo> orders, long rid)
        {
            Service.SetBonusToUsed(userid, orders, rid);
        }

        /// <summary>
        /// 检查是否能添加
        /// </summary>
        public static bool IsAdd(long shopid)
        {
            return Service.IsAdd(shopid);
        }

        /// <summary>
        ///  获取红包详情
        /// </summary> 
        public static QueryPageModel<ShopBonusReceiveInfo> GetDetail(long bonusid, int pageIndex, int pageSize)
        {
            return Service.GetDetail(bonusid, pageIndex, pageSize);
        }

        public static List<ShopBonusReceiveInfo> GetDetailByGrantId(long grantid)
        {
            return Service.GetDetailByGrantId(grantid);
        }

        /// <summary>
        ///  添加红包 
        /// </summary>
        public static void Add(ShopBonusInfo model, long shopid)
        {
            Service.Add(model, shopid);
        }

        /// <summary>
        ///  修改红包
        /// </summary>
        public static void Update(ShopBonusInfo model)
        {
            Service.Update(model);
        }

        /// <summary>
        ///  失效
        /// </summary>
        public static void Invalid(long id)
        {
            Service.Invalid(id);
        }

        /// <summary>
        /// 领取
        /// </summary>
        public static object Receive(long grantid, string openId, string wxhead, string wxname)
        {
            return Service.Receive(grantid, openId, wxhead, wxname);
        }

        /// <summary>
        /// 新增红包时判断是否超出服务费用结束日期
        /// </summary>
        /// <returns></returns>
        public static bool IsOverDate(DateTime bonusDateEnd, DateTime dateEnd, long shopid)
        {
            return Service.IsOverDate(bonusDateEnd, dateEnd, shopid);
        }

        public static ActiveMarketServiceInfo GetShopBonusService(long shopId)
        {
            return Service.GetShopBonusService(shopId);
        }

        /// <summary>
        /// 订单支付完成时，生成红包详情
        /// </summary>
        public static long GenerateBonusDetail(ShopBonusInfo model, long orderid, string receiveurl)
        {
            return Service.GenerateBonusDetail(model, orderid, receiveurl);
        }

        public static ShopBonusInfo GetByShopId(long shopid)
        {
            return Service.GetByShopId(shopid);
        }

        public static ShopInfo GetShopByReceive(long receive)
        {
            return Service.GetShopByReceive(receive);
        }

        public static List<ShopBonus> GetShopBounsByUser(long user)
        {
            var receive = Service.GetDetailByUserId(user);
            return BuildShopBonus(receive);
        }

        public static QueryPageModel<ShopBonus> GetShopBouns(CouponRecordQuery query)
        {
            var data = Service.GetDetailByQuery(query);
            var list = BuildShopBonus(data.Models);
            return new QueryPageModel<ShopBonus>
            {
                Models = list,
                Total = data.Total
            };
        }


        public static List<ShopBonus> BuildShopBonus(List<ShopBonusReceiveInfo> receive)
        {
            var grands = Service.GetGrants(receive.Select(p => p.BonusGrantId).ToList());
            var bouns = Service.GetShopBonus(grands.Select(p => p.ShopBonusId).ToList());
            var shops = GetService<IShopService>().GetShops(bouns.Select(p => p.ShopId));
            var vshops = GetService<IVShopService>().GetVShopsByShopIds(shops.Select(p => p.Id));

            return receive.Select(item =>
            {
                var grand = grands.FirstOrDefault(p => p.Id == item.BonusGrantId);
                var boun = bouns.FirstOrDefault(p => p.Id == grand.ShopBonusId);
                var shop = shops.FirstOrDefault(p => p.Id == boun.ShopId);
                var vshop = vshops.FirstOrDefault(p => p.ShopId == shop.Id);
                return new ShopBonus
                {
                    Receive = item,
                    Bonus = boun,
                    Grant = grand,
                    Shop = shop,
                    VShop = vshop
                };
            }).ToList();
           
        }
    }
}
