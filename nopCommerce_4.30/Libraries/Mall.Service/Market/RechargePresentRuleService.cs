using Mall.CommonModel;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Mall.Service
{
    /// <summary>
    /// 充值赠送
    /// </summary>
    public class RechargePresentRuleService : ServiceBase, IRechargePresentRuleService
    {
        /// <summary>
        /// 新增修改充值赠送规则
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rules"></param>
        /// <param name="products"></param>
        public void SetRules(IEnumerable<RechargePresentRuleInfo> rules)
        {
            var result = DbFactory.Default.Get<RechargePresentRuleInfo>().OrderBy(d => d.Id).ToList();
            List<long> upids = new List<long>();
            var datalist = rules.OrderBy(d=>d.ChargeAmount).ToList();
            var upcount = (result.Count > datalist.Count) ? datalist.Count() : result.Count;
            //修改
            if (result.Count > 0)
            {
                for (var i = 0; i < upcount; i++)
                {
                    var item = result[i];
                    var ditem = datalist[i];
                    item.ChargeAmount = ditem.ChargeAmount;
                    item.PresentAmount = ditem.PresentAmount;
                    upids.Add(item.Id);
                    DbFactory.Default.Update(item);
                }
            }
            //删除
            DbFactory.Default.Del<RechargePresentRuleInfo>(d => d.Id.ExNotIn(upids));
            //添加
            if (datalist.Count > result.Count)
            {

                for (var i = upcount; i < datalist.Count; i++)
                {
                    var ditem = datalist[i];
                    DbFactory.Default.Add(ditem);
                }
            }
            /*
            //清理所有
            DbFactory.Default.Del<RechargePresentRuleInfo>(d => d.Id > 0);
            //重新入库
            DbFactory.Default.Add(rules);
            */

        }
        /// <summary>
        /// 获取充值赠送规则
        /// </summary>
        /// <returns></returns>
        public List<RechargePresentRuleInfo> GetRules()
        {
            var result = DbFactory.Default.Get<RechargePresentRuleInfo>().OrderBy(d => d.ChargeAmount).ToList();
            return result;
        }
    }
}
