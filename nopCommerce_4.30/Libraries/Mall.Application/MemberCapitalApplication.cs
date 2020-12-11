using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.Core;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.CommonModel;
using Mall.DTO;
using Mall.Entities;

namespace Mall.Application
{
    public class MemberCapitalApplication:BaseApplicaion<IMemberCapitalService>
    {
        /// <summary>
        /// 取多个会员资产信息
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static QueryPageModel<MemberCapital> GetCapitals(CapitalQuery query)
        {
            return Service.GetCapitals(query);
        }
        /// <summary>
        /// 取会员资产
        /// </summary>
        /// <param name="userid">会员ID</param>
        /// <returns></returns>
        public static CapitalInfo GetCapitalInfo(long userid)
        {
            return Service.GetCapitalInfo(userid);
        }
        /// <summary>
        /// 获取用户余额
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static decimal GetBalanceByUserId(long userId)
        {
            var info = Service.GetCapitalInfo(userId);
            if (info == null) return 0;
            return info.Balance;
        }
        /// <summary>
        /// 取多个会员资产明细
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<CapitalDetailInfo> GetCapitalDetails(CapitalDetailQuery query)
        {
            return Service.GetCapitalDetails(query);
        }

        /// <summary>
        /// 取资产明细
        /// </summary>
        /// <param name="id">明细ID</param>
        /// <returns></returns>
        public static CapitalDetailInfo GetCapitalDetailInfo(long id)
        {
           return  Service.GetCapitalDetailInfo(id);
        }

        public static List<CapitalDetailInfo> GetTopCapitalDetailList(long capitalId, int num)
        {
            return Service.GetTopCapitalDetailList(capitalId, num);
        }

        /// <summary>
        /// 取申请提现记录
        /// </summary>
        /// <param name="MemId"></param>
        /// <returns></returns>
        public static QueryPageModel<ApplyWithdrawInfo> GetApplyWithDraw(ApplyWithDrawQuery query)
        {
            return  Service.GetApplyWithDraw(query);
        }

        public static int GetApplyWithDrawCount(ApplyWithDrawQuery query)
        {
            return Service.GetApplyWithDrawCount(query);
        }
        /// <summary>
        /// 取申请提现记录
        /// </summary>
        /// <param name="MemId"></param>
        /// <returns></returns>
        public static ApplyWithdrawInfo GetApplyWithDrawInfo(long id)
        {
            return Service.GetApplyWithDrawInfo(id);
        }

        /// <summary>
        /// 设置支付密码
        /// </summary>
        /// <param name="memid"></param>
        /// <param name="pwd"></param>
        public static void SetPayPwd(long memid, string pwd)
        {
            Service.SetPayPwd(memid, pwd);
        }
        /// <summary>
        /// 审核会员申请提现
        /// </summary>
        /// <param name="id"></param>
        public static void ConfirmApplyWithDraw(ApplyWithdrawInfo info)
        {
            Service.ConfirmApplyWithDraw(info);
        }
        /// <summary>
        /// 拒绝会员提现申请
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="opuser"></param>
        /// <param name="remark"></param>
        public static void RefuseApplyWithDraw(long id, ApplyWithdrawInfo.ApplyWithdrawStatus status, string opuser, string remark)
        {
            Service.RefuseApplyWithDraw(id, status, opuser, remark);
        }
        /// <summary>
        /// 添加提现申请
        /// </summary>
        /// <param name="memid"></param>
        /// <param name="amount"></param>
        /// <param name="nickname"></param>
        public static void AddWithDrawApply(ApplyWithdrawInfo model)
        {
            Service.AddWithDrawApply(model);
        }
        /// <summary>
        /// 添加充值记录
        /// </summary>
        /// <param name="model"></param>
        public static long AddChargeApply(DTO.ChargeDetail model)
        {
            //return Service.AddChargeApply(AutoMapper.Mapper.Map<ChargeDetailInfo>(model));
            return Service.AddChargeApply(model.Map<ChargeDetailInfo>());

        }
        /// <summary>
        /// 取充值记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ChargeDetailInfo GetChargeDetail(long id)
        {
           return  Service.GetChargeDetail(id);
        }

		/// <summary>
		/// 充值成功
		/// </summary>
		/// <param name="chargeDetailId"></param>
		public static void ChargeSuccess(long chargeDetailId)
		{
			Service.ChargeSuccess(chargeDetailId);
		}

        /// <summary>
        /// 取充值列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ChargeDetailInfo> GetChargeLists(ChargeQuery query)
        {
          return   Service.GetChargeLists(query);
        }

        public static void UpdateChargeDetail(ChargeDetailInfo model)
        {
            Service.UpdateChargeDetail(model);
        }
        /// <summary>
        /// 添加会员资产
        /// </summary>
        /// <param name="capitalModel"></param>
        public static void AddCapital(CapitalDetailModel model)
        {
            Service.AddCapital(model);
        }
        /// <summary>
        /// 更新会员资产金额
        /// </summary>
        /// <param name="memid"></param>
        /// <param name="amount">可用金额</param>
        /// <param name="freezeAmount">冻结金额</param>
        /// <param name="chargeAmount">支付金额</param>
        public static void UpdateCapitalAmount(long memid, decimal amount, decimal freezeAmount, decimal chargeAmount)
        {
            Service.UpdateCapitalAmount(memid, amount, freezeAmount, chargeAmount);
        }

        /// <summary>
        /// 根据不同类型生成单号(充值单号、提现单号)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static long CreateCode(CapitalDetailInfo.CapitalDetailType type)
        {
          return   Service.CreateCode(type);
        }

        /// <summary>
        /// 添加店铺充值流水
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static long AddChargeDetailShop(ChargeDetailShopInfo model)
        {
          return   Service.AddChargeDetailShop(model);
        }

        /// <summary>
        /// 修改店铺充值流水
        /// </summary>
        /// <param name="model"></param>
        public static void UpdateChargeDetailShop(ChargeDetailShopInfo model)
        {
            Service.UpdateChargeDetailShop(model);
        }

        /// <summary>
        /// 获取店铺充值流水信息
        /// </summary>
        /// <param name="Id">流水ID</param>
        /// <returns></returns>
        public static ChargeDetailShopInfo GetChargeDetailShop(long Id)
        {
          return   Service.GetChargeDetailShop(Id);
        }

        /// <summary>
        /// 获取累计收到红包
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static decimal GetSumRedPacket(long Id)
        {
            return Service.GetSumRedPacket(Id);
        }

        /// <summary>
        /// 佣金转账
        /// </summary>
        /// <param name="member"></param>
        /// <param name="amount"></param>
        /// <param name="remark"></param>
        /// <param name="payNo"></param>
        public static long BrokerageTransfer(long member, decimal amount, string remark, string payNo)
        {
            var capital = Service.GetCapitalInfo(member);
            var detail = new CapitalDetailInfo
            {
                Amount = amount,
                CapitalID = capital.Id,
                PresentAmount = 0,
                Remark = remark,
                SourceData = payNo,
                SourceType = CapitalDetailInfo.CapitalDetailType.Brokerage,
            };
            Service.AddCapital(detail);
            return detail.Id;
        }
    }
}
