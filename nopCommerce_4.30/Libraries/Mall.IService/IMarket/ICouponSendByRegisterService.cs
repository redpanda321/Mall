using Mall.Entities;
using System.Collections.Generic;

namespace Mall.IServices
{
    /// <summary>
    /// 注册有礼优惠券设置
    /// </summary>
    public interface ICouponSendByRegisterService : IService
    {
        /// <summary>
        /// 添加设置
        /// </summary>
        /// <param name="mCouponSendByRegister">数据实体</param>
        void AddCouponSendByRegister(CouponSendByRegisterInfo model, List<CouponSendByRegisterDetailedInfo> details);

        /// <summary>
        /// 修改设置
        /// </summary>
        /// <param name="mCouponSendByRegister">数据实体</param>
        void UpdateCouponSendByRegister(CouponSendByRegisterInfo model, List<CouponSendByRegisterDetailedInfo> details);

        /// <summary>
        /// 获取第一条设置数据
        /// </summary>
        /// <returns></returns>
        CouponSendByRegisterInfo GetCouponSendByRegister();
    }
}
