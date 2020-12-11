using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.IServices;
using System;
using System.Collections.Generic;

namespace Mall.Application
{
    public class VshopApplication:BaseApplicaion<IVShopService>
    {
        /// <summary>
        /// 根据条件查询所有微店
        /// </summary>
        /// <returns></returns>
        public static QueryPageModel<Entities.VShopInfo> GetVShopByParamete(VshopQuery vshopQuery)
        {
            return Service.GetVShopByParamete(vshopQuery);
        }

        /// <summary>
        /// 获取热门微店
        /// </summary>
        /// <returns></returns>
        public static List<Entities.VShopInfo> GetHotShop(VshopQuery vshopQuery, DateTime? startTime, DateTime? endTime, out int total)
        {
            return Service.GetHotShop(vshopQuery, startTime, endTime, out total);
        }

        /// <summary>
        /// 获取热门微店
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="total">热门微店总数</param>
        /// <returns></returns>
        public static List<Entities.VShopInfo> GetHotShops(int page, int pageSize, out int total)
        {
            return Service.GetHotShops(page, pageSize, out total);
        }

        /// <summary>
        /// 获取所有微店
        /// </summary>
        /// <returns></returns>
        public static List<Entities.VShopInfo> GetVShops()
        {
            return Service.GetVShops();
        }

        /// <summary>
        /// 根据商家id获取微店信息
        /// </summary>
        /// <param name="shopIds"></param>
        /// <returns></returns>
        public static List<DTO.VShop> GetVShopsByShopIds(IEnumerable<long> shopIds)
        {
            return Service.GetVShopsByShopIds(shopIds).Map<List<DTO.VShop>>();
        }

        /// <summary>
        /// 获取微店
        /// </summary>
        /// <param name="id">微店Id</param>
        /// <returns></returns>
        public static Entities.VShopInfo GetVShop(long id)
        {
            return Service.GetVShop(id);
        }

        /// <summary>
        /// 获取所有微店(分页)
        /// </summary>
        /// <param name="page">页号</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="total">微店总数</param>
        /// <returns></returns>
        public static List<Entities.VShopInfo> GetVShops(int page, int pageSize, out int total)
        {
            return Service.GetVShops(page, pageSize, out total);
        }


        /// <summary>
        /// 获取所有微店(分页)
        /// </summary>
        /// <param name="page">页号</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="total">微店总数</param>
        /// <param name="state">微店状态</param>
        /// <returns></returns>
        public static List<Entities.VShopInfo> GetVShops(int page, int pageSize, out int total, Entities.VShopInfo.VShopStates state, bool? IsOpenVshop)
        {
            return Service.GetVShops(page, pageSize, out total, state, IsOpenVshop);
        }


        /// <summary>
        /// 获取主推微店
        /// </summary>
        /// <returns></returns>
        public static Entities.VShopInfo GetTopShop()
        {
            return Service.GetTopShop();
        }


        /// <summary>
        /// 设为主推
        /// </summary>
        public static void SetTopShop(long vshopId)
        {
            Service.SetTopShop(vshopId);
        }

        /// <summary>
        /// 设为热门
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        public static void SetHotShop(long vshopId)
        {
            Service.SetHotShop(vshopId);
        }

        /// <summary>
        /// 下架微店
        /// </summary>
        public static void CloseShop(long vshopId)
        {
            Service.CloseShop(vshopId);
        }

        /// <summary>
        /// 上架微店
        /// </summary>
        public static void SetShopNormal(long vshopId)
        {
            Service.SetShopNormal(vshopId);
        }

        /// <summary>
        /// 删除热门微店
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        public static void DeleteHotShop(long vshopId)
        {
            Service.DeleteHotShop(vshopId);
        }

        /// <summary>
        /// 替换热门微店
        /// </summary>
        /// <param name="oldHotVShopId">替换前的热门微店ID</param>
        /// <param name="newHotVshopId">要替换的热门微店ID</param>
        public static void ReplaceHotShop(long oldHotVShopId, long newHotVshopId)
        {
            Service.ReplaceHotShop(oldHotVShopId, newHotVshopId);
        }

        /// <summary>
        /// 更改热门微店排序
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        /// <param name="sequence">排序数字</param>
        public static void UpdateSequence(long vshopId, int? sequence)
        {
            Service.UpdateSequence(vshopId, sequence);
        }

        /// <summary>
        /// 审核通过
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        public static void AuditThrough(long vshopId)
        {
            Service.AuditThrough(vshopId);
        }

        /// <summary>
        /// 审核拒绝
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        public static void AuditRefused(long vshopId)
        {
            Service.AuditRefused(vshopId);
        }

        /// <summary>
        /// 替换主推微店
        /// </summary>
        /// <param name="oldTopVshopId">替换前的主推微店ID</param>
        /// <param name="newTopVshopId">要替换的主推微店ID</param>
        public static void ReplaceTopShop(long oldTopVshopId, long newTopVshopId)
        {
            Service.ReplaceTopShop(oldTopVshopId, newTopVshopId);
        }

        /// <summary>
        /// 删除主推微店
        /// </summary>
        /// <param name="vshopId">微店ID</param>
        public static void DeleteTopShop(long vshopId)
        {
            Service.DeleteTopShop(vshopId);
        }

        /// <summary>
        /// 根据店铺Id获取微店
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <returns></returns>
        public static Entities.VShopInfo GetVShopByShopId(long shopId)
        {
            return Service.GetVShopByShopId(shopId);
        }

        /// <summary>
        /// 创建微店
        /// </summary>
        /// <param name="vshopInfo">微店信息</param>
        public static void CreateVshop(Entities.VShopInfo vshopInfo)
        {
            Service.CreateVshop(vshopInfo);
        }

        /// <summary>
        /// 更新微店
        /// </summary>
        /// <param name="vshopInfo">微店信息</param>
        public static void UpdateVShop(Entities.VShopInfo vshopInfo)
        {
            Service.UpdateVShop(vshopInfo);
        }

        /// <summary>
        /// 添加微店访问数量
        /// </summary>
        /// <param name="vshopId"></param>
        public static void AddVisitNumber(long vshopId)
        {
            Service.AddBuyNumber(vshopId);
        }

        /// <summary>
        /// 添加微店购买数量
        /// </summary>
        /// <param name="vshopId"></param>
        public static void AddBuyNumber(long vshopId)
        {
            Service.AddBuyNumber(vshopId);
        }

        /// <summary>
        /// 获取用户关注的微店
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static List<Entities.VShopInfo> GetUserConcernVShops(long userId, int pageNo, int pageSize)
        {
            return Service.GetUserConcernVShops(userId, pageNo, pageSize);
        }

        /// <summary>
        /// 获取微店的配置信息
        /// </summary>
        /// <param name="shopId">微店ID</param>
        /// <returns></returns>
        public static Entities.WXshopInfo GetVShopSetting(long shopId)
        {
            return Service.GetVShopSetting(shopId);
        }
        /// <summary>
        /// 获取微店优惠卷 设置 信息
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public static List<Entities.CouponSettingInfo> GetVShopCouponSetting(long shopid)
        {
            return Service.GetVShopCouponSetting(shopid);
        }
        /// <summary>
        /// 更新店铺优惠卷 设置 信息
        /// </summary>
        /// <param name="infolist"></param>
        public static void SaveVShopCouponSetting(IEnumerable<Entities.CouponSettingInfo> infolist)
        {
            Service.SaveVShopCouponSetting(infolist);
        }
      
        /// <summary>
        /// 增加访问记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static int LogVisit(long id)
        {
            return Service.LogVisit(id);
        }

      
      
    }
}
