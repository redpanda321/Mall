using Mall.IServices;
using Mall.Entities;
using System;
using NetRube.Data;

namespace Mall.Service
{
    public class MemberIntegralConversionFactoryService : ServiceBase, IMemberIntegralConversionFactoryService
    {
        public IConversionMemberIntegralBase Create(MemberIntegralInfo.IntegralType type, int Integral = 0)
        {
            switch (type)
            {
                case MemberIntegralInfo.IntegralType.Reg: return new RegisteGenerateIntegral();
                case MemberIntegralInfo.IntegralType.BindWX: return new BindWXGenerateIntegral();
                case MemberIntegralInfo.IntegralType.Comment: return new CommentGenerateIntegral();
                case MemberIntegralInfo.IntegralType.InvitationMemberRegiste: return new InvitationMemberRegisteGenerateIntegral();
                //case MemberIntegral.IntegralType.ProportionRebate: return new ProportionRebateGenerateIntegral();
                case MemberIntegralInfo.IntegralType.Login: return new LoginGenerateIntegral();
                case MemberIntegralInfo.IntegralType.Exchange: return new ExchangeGenerateIntegral(Integral);
                case MemberIntegralInfo.IntegralType.Cancel:
                case MemberIntegralInfo.IntegralType.SystemOper:
                case MemberIntegralInfo.IntegralType.Consumption:
                case MemberIntegralInfo.IntegralType.Others:
                case MemberIntegralInfo.IntegralType.SignIn:
                case MemberIntegralInfo.IntegralType.WeiActivity:
                    return new GenralIntegral(Integral);
                case MemberIntegralInfo.IntegralType.Share:
                    return new OrderShareGenerateIntegral();
                default: return null;
            }
        }
    }

    #region 产生积分实体
    /// <summary>
    /// 获取注册产生的会员积分
    /// </summary>
    public class RegisteGenerateIntegral : ServiceBase, IConversionMemberIntegralBase
    {

        public int ConversionIntegral()
        {

            //var type = Context.MemberIntegralRule.FirstOrDefault(m => m.TypeId == (int)MemberIntegral.IntegralType.Reg);
            var type = DbFactory.Default.Get<MemberIntegralRuleInfo>().Where(m => m.TypeId == (int)MemberIntegralInfo.IntegralType.Reg).FirstOrDefault();
            if (null == type)
            {
                throw new Exception(string.Format("找不到因注册产生会员积分的规则"));
            }
            return type.Integral;
        }
    }

    public class GenralIntegral : ServiceBase, IConversionMemberIntegralBase
    {
        private int _Integral = 0;
        public GenralIntegral(int Integral)
        {
            _Integral = Integral;
        }
        public int ConversionIntegral()
        {
            return _Integral;
        }
    }



    /// <summary>
    /// 获取绑定微信产生的会员积分
    /// </summary>
    public class BindWXGenerateIntegral : ServiceBase, IConversionMemberIntegralBase
    {
        public int ConversionIntegral()
        {
            //var type = Context.MemberIntegralRule.FirstOrDefault(m => m.TypeId == (int)MemberIntegral.IntegralType.BindWX);
            var type = DbFactory.Default.Get<MemberIntegralRuleInfo>().Where(m => m.TypeId == (int)MemberIntegralInfo.IntegralType.BindWX).FirstOrDefault();
            if (null == type)
            {
                Core.Log.Info(string.Format("找不到绑定微信产生会员积分的规则"));
                return 0;
            }
            return type.Integral;
        }
    }

    /// <summary>
    /// 获取登录产生的会员积分
    /// </summary>
    public class LoginGenerateIntegral : ServiceBase, IConversionMemberIntegralBase
    {
        public int ConversionIntegral()
        {
            //var type = Context.MemberIntegralRule.FirstOrDefault(m => m.TypeId == (int)MemberIntegral.IntegralType.Login);
            var type = DbFactory.Default.Get<MemberIntegralRuleInfo>().Where(m => m.TypeId == (int)MemberIntegralInfo.IntegralType.Login).FirstOrDefault();
            if (null == type)
            {
                Core.Log.Info(string.Format("找不到登录产生会员积分的规则"));
                return 0;
            }
            return type.Integral;
        }
    }

    /// <summary>
    /// 获取评论订单产生的会员积分
    /// </summary>
    public class CommentGenerateIntegral : ServiceBase, IConversionMemberIntegralBase
    {
        public int ConversionIntegral()
        {
            //var type = Context.MemberIntegralRule.FirstOrDefault(m => m.TypeId == (int)MemberIntegral.IntegralType.Comment);
            var type = DbFactory.Default.Get<MemberIntegralRuleInfo>().Where(m => m.TypeId == (int)MemberIntegralInfo.IntegralType.Comment).FirstOrDefault();
            if (null == type)
            {
                Core.Log.Info(string.Format("找不到评论订单产生会员积分的规则"));
                return 0;
            }
            return type.Integral;
        }
    }

    /// <summary>
    /// 获取邀请会员注册产生的会员积分
    /// </summary>
    public class InvitationMemberRegisteGenerateIntegral : ServiceBase, IConversionMemberIntegralBase
    {
        public int ConversionIntegral()
        {
            //var type = Context.InviteRuleInfo.FirstOrDefault();
            var type = DbFactory.Default.Get<InviteRuleInfo>().FirstOrDefault();
            if (null == type)
            {
                throw new Exception(string.Format("找不到邀请会员注册产生会员积分的规则"));
            }
            return type.InviteIntegral;
        }
    }


    ///// <summary>
    ///// 获取返利比例产生的会员积分
    ///// </summary>
    //public class ProportionRebateGenerateIntegral : ServiceBase, IConversionMemberIntegralBase
    //{
    //    public int ConversionIntegral()
    //    {
    //        var type = context.MemberIntegralRuleInfo.FirstOrDefault(m => m.TypeId == (int)MemberIntegral.IntegralType.ProportionRebate);
    //        if (null == type)
    //        {
    //            throw new Exception(string.Format("找不到返利比例产生会员积分的规则"));
    //        }
    //        return type.Integral;
    //    }
    //}


    public class ExchangeGenerateIntegral : ServiceBase, IConversionMemberIntegralBase
    {
        private int _Integral = 0;
        public ExchangeGenerateIntegral(int Integral)
        {
            if (Integral > 0)
                _Integral = -Integral;
        }
        public int ConversionIntegral()
        {
            return _Integral;
        }

    }
    /// <summary>
    /// 晒单获取积分
    /// </summary>
    public class OrderShareGenerateIntegral : ServiceBase, IConversionMemberIntegralBase
    {
        public int ConversionIntegral()
        {
            //var type = Context.MemberIntegralRule.FirstOrDefault(m => m.TypeId == (int)MemberIntegral.IntegralType.Share);
            var type = DbFactory.Default.Get<MemberIntegralRuleInfo>().Where(m => m.TypeId == (int)MemberIntegralInfo.IntegralType.Share).FirstOrDefault();
            if (null == type)
            {
                Core.Log.Info(string.Format("找不到晒单积分的规则"));
                return 0;
            }
            return type.Integral;
        }
    }
    #endregion
}
