using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.Core;
using Mall.IServices;
using Mall.CommonModel;
using Mall.Entities;
using Mall.DTO.QueryModel;

namespace Mall.Application
{
   public class BonusApplication:BaseApplicaion<IBonusService>
    {
        /// <summary>
        ///  获取红包列表
        /// </summary>
        public static QueryPageModel<BonusInfo> Get(BonusQuery query)
        {
            return Service.Get(query);
        }

        /// <summary>
        /// 获取红包
        /// </summary>
        public static BonusInfo Get(long id)
        {
            return Service.Get(id);
        }

        /// <summary>
        ///  获取红包详情
        /// </summary>
        public static QueryPageModel<BonusReceiveInfo> GetDetail(long bonusId, int pageIndex, int pageSize)
        {
            return Service.GetDetail(bonusId, pageIndex, pageSize);
        }

        /// <summary>
        ///  添加红包
        /// </summary>
        public static void Add(BonusInfo model, string baseAddress)
        {
            Service.Add(model, baseAddress);
        }

        /// <summary>
        ///  修改红包
        /// </summary>
        public static void Update(BonusInfo model)
        {
            Service.Update(model);
        }

        /// <summary>
        ///  红包失效
        /// </summary>
        public static void Invalid(long id)
        {
            Service.Invalid(id);
        }

        /// <summary>
        /// 领取活动红包
        /// </summary>
        public static object Receive(long id, string openId, string unionId)
        {
            return Service.Receive(id, openId, unionId);
        }


        /// <summary>
        /// 关注送红包
        /// </summary>
        public static string Receive(string openId)
        {
            return Service.Receive(openId);
        }

        /// <summary>
        /// 获取某用户领取的金额
        /// </summary>
        public static decimal GetReceivePriceByOpendId(long id, string openId)
        {
            return Service.GetReceivePriceByOpendId(id, openId);
        }

        /// <summary>
        /// 能否添加红包
        /// </summary>
        public static bool CanAddBonus()
        {
           return  Service.CanAddBonus();
        }


        public static void SetShare(long id, string openId)
        {
            Service.SetShare(id, openId);
        }
        /// <summary>
        /// 刮刮卡新增红包
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isShare"></param>
        /// <param name="userId"></param>
        /// <param name="openId"></param>
        public static void SetShareByUserId(long id, bool isShare, long userId)
        {
            Service.SetShareByUserId(id, isShare, userId);
        }
        public static void DepositToRegister(long userid)
        {
            Service.DepositToRegister(userid);
        }
        /// <summary>
        /// 获取红包集合 
        /// </summary>
        public static IEnumerable<BonusInfo> GetBonusByType(BonusInfo.BonusType bonusType)
        {
           return  Service.GetBonusByType(bonusType);
        }

        /// <summary>
        /// 获取红包剩余数量
        /// </summary>
        public static string GetBonusSurplus(long bonusId)
        {
            return Service.GetBonusSurplus(bonusId);
        }

        public static decimal GetReceivePriceByUserId(long id, long userId)
        {
           return  Service.GetReceivePriceByUserId(id, userId);
        }

        public static decimal GetFirstReceivePriceByBonus(long bonus)
        {
            //TODO:FG 业务实现有疑问？
            return Service.GetFirstReceivePriceByBonus(bonus);
            
        }
        /// <summary>
        /// 增加红包增加个数
        /// </summary>
        /// <param name="id">红包Id</param>
        public static void IncrReceiveCount(long id)
        {
            Service.IncrReceiveCount(id);
        }
    }
}
