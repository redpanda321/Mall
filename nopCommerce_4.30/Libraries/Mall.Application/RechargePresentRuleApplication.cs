using AutoMapper;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Nop.Core.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Application
{
    /// <summary>
    /// 拼团逻辑
    /// </summary>
    public class RechargePresentRuleApplication
    {

      //  private static IRechargePresentRuleService _iRechargePresentRuleService =  EngineContext.Current.Resolve<IRechargePresentRuleService>();


        private static IRechargePresentRuleService _iRechargePresentRuleService =  EngineContext.Current.Resolve<IRechargePresentRuleService>();


        /// <summary>
        /// 新增修改充值赠送规则
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rules"></param>
        /// <param name="products"></param>
        public static void SetRules(IEnumerable<RechargePresentRule> rules)
        {
            //  var data = Mapper.Map<IEnumerable<RechargePresentRule>, List<RechargePresentRuleInfo>>(rules);

            var data = rules.Map< List<RechargePresentRuleInfo>>();

            _iRechargePresentRuleService.SetRules(data);
        }
        /// <summary>
        /// 获取充值赠送规则
        /// </summary>
        /// <returns></returns>
        public static List<RechargePresentRule> GetRules()
        {
            var data = _iRechargePresentRuleService.GetRules();
            //var result = Mapper.Map<List<RechargePresentRuleInfo>, List<RechargePresentRule>>(data);
            var result = data.Map< List<RechargePresentRule>>();


            return result;
        }
    }
}
