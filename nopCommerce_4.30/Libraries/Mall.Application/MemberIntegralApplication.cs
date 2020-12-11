using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.Core;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.CommonModel;
using Mall.DTO;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    public class MemberIntegralApplication:BaseApplicaion
    {
     //   private static IMemberIntegralService _iMemberIntegralService =  EngineContext.Current.Resolve<IMemberIntegralService>();


        private static IMemberIntegralService _iMemberIntegralService =  EngineContext.Current.Resolve<IMemberIntegralService>();


        /// <summary>
        ///  //用户积分记录增加
        /// </summary>
        /// <param name="model"></param>
        /// <param name="conversionMemberIntegralEntity"></param>
        public static void AddMemberIntegral(MemberIntegralRecordInfo model, IConversionMemberIntegralBase conversionMemberIntegralEntity = null)
        {
            _iMemberIntegralService.AddMemberIntegral(model, conversionMemberIntegralEntity);
        }
        /// <summary>
        /// 通过多个RecordAction，增加用户积分
        /// </summary>
        /// <param name="model"></param>
        /// <param name="type"></param>
        public static void AddMemberIntegralByEnum(MemberIntegralRecordInfo model, MemberIntegralInfo.IntegralType type)
        {
            var conversionService= ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create;
            var conversionMemberIntegralEntity = conversionService.Create(MemberIntegralInfo.IntegralType.Share);
            _iMemberIntegralService.AddMemberIntegralByRecordAction(model, conversionMemberIntegralEntity);
        }
        /// <summary>
        /// 获取用户积分列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<MemberIntegralInfo> GetMemberIntegralList(IntegralQuery query)
        {
            return _iMemberIntegralService.GetMemberIntegralList(query);
        }


        /// <summary>
        /// 根据用户ID获取用户的积分信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static MemberIntegralInfo GetMemberIntegral(long userId)
        {
            var integral = _iMemberIntegralService.GetMemberIntegral(userId);
            if (integral == null) return new MemberIntegralInfo { MemberId = userId };
            return integral;
        }
        public static int GetAvailableIntegral(long userId)
        {
            var integral = _iMemberIntegralService.GetMemberIntegral(userId);
            if (integral == null) return 0;
            return integral.AvailableIntegrals;
        }

        /// <summary>
        /// 根据用户ID获取用户的积分信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<MemberIntegralInfo> GetMemberIntegrals(List<long> userIds)
        {
            return _iMemberIntegralService.GetMemberIntegrals(userIds);
        }

        /// <summary>
        /// 获取单个用户的积分记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<MemberIntegralRecordInfo> GetIntegralRecordList(IntegralRecordQuery query)
        {
            return _iMemberIntegralService.GetIntegralRecordList(query);
        }

      

        /// <summary>
        /// 获取积分兑换规则
        /// </summary>
        /// <returns></returns>
        public static MemberIntegralExchangeRuleInfo GetIntegralChangeRule()
        {
            return _iMemberIntegralService.GetIntegralChangeRule();
        }


        /// <summary>
        /// 订单是否已经分享
        /// </summary>
        /// <param name="orderid"></param>
        /// <returns>true:已经分享过</returns>
        public static bool OrderIsShared(IEnumerable<long> orderids)
        {
            var recordAction = _iMemberIntegralService.GetIntegralRecordAction(orderids, MemberIntegralInfo.VirtualItemType.ShareOrder);
            if (recordAction.Count > 0)//有分享记录，就认为已经分享过（不管分享的订单个数）
                return true;
            return false;
        }


        public static QueryPageModel<MemberIntegral> GetMemberIntegrals(IntegralQuery query)
        {
            var data = _iMemberIntegralService.GetMemberIntegralList(query);
            var members = GetService<IMemberService>().GetMembers(data.Models.Select(p => (long)p.MemberId).ToList());
            var grades = MemberGradeApplication.GetMemberGrades();
            var result = new List<MemberIntegral>();
            foreach (var item in data.Models)
            {
                var member = members.FirstOrDefault(p => p.Id == item.MemberId);
                result.Add(new MemberIntegral
                {
                    Id = item.Id,
                    AvailableIntegrals = item.AvailableIntegrals,
                    HistoryIntegrals = item.HistoryIntegrals,
                    MemberGrade = MemberGradeApplication.GetMemberGradeByIntegral(grades, item.HistoryIntegrals).GradeName,
                    UserName = member.UserName,
                    MemberId = member.Id,
                    CreateDate = member.CreateDate,
                });
            }

            return new QueryPageModel<MemberIntegral>
            {
                Models = result,
                Total = data.Total
            };
        }
    }
}
