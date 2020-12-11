
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Service
{
    /// <summary>
    /// 注册有奖 TODO:FG 业务实现待优化
    /// </summary>
    public class CouponSendByRegisterService : ServiceBase, ICouponSendByRegisterService
    {
        /// <summary>
        /// 新增设置
        /// </summary>
        /// <param name="model"></param>
        public void AddCouponSendByRegister(CouponSendByRegisterInfo model, List<CouponSendByRegisterDetailedInfo> details)
        {
            var flag = DbFactory.Default.Get<CouponSendByRegisterInfo>().Exist();
            if (!flag)
            {
                DbFactory.Default
                    .InTransaction(() =>
                    {
                        DbFactory.Default.Add(model);
                        details.ForEach(p => p.CouponRegisterId = model.Id);
                        DbFactory.Default.AddRange(details);
                    });
            }
        }

        /// <summary>
        /// 更新设置
        /// </summary>
        /// <param name="mCouponSendByRegister"></param>
        public void UpdateCouponSendByRegister(CouponSendByRegisterInfo model, List<CouponSendByRegisterDetailedInfo> details)
        {
            var data = DbFactory.Default.Get<CouponSendByRegisterInfo>().Where(s => s.Id == model.Id).FirstOrDefault();
            DbFactory.Default
                .InTransaction(() =>
                {
                    data.Status = model.Status;
                    data.Link = model.Link;
                    DbFactory.Default.Update(data);
                    //删除设置详情
                    DbFactory.Default
                        .Del<CouponSendByRegisterDetailedInfo>()
                        .Where(item => item.CouponRegisterId == model.Id)
                        .Succeed();
                    details.ForEach(p => p.CouponRegisterId = model.Id);
                    DbFactory.Default.AddRange(details);
                });
        }

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <returns></returns>
        public CouponSendByRegisterInfo GetCouponSendByRegister()
        {
            return DbFactory.Default.Get<CouponSendByRegisterInfo>().FirstOrDefault();
        }
    }
}
