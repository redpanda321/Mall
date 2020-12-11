using Mall.Core.Plugins.Message;
using System.Collections.Generic;


namespace Mall.CommonModel.WeiXin
{
    /// <summary>
    /// 微信与消息类型绑定
    /// </summary>
    public class WX_MsgTemplateLinkData
    {
        /// <summary>
        /// 信息类型
        /// </summary>
        public MessageTypeEnum MsgType { get; set; }
        /// <summary>
        /// 模板短ID
        /// </summary>
        public string MsgTemplateShortId { get; set; }
        /// <summary>
        /// 微信点击跳回网址
        /// </summary>
        public string ReturnUrl { get; set; }

        #region 静态
        /// <summary>
        /// 数据列表
        /// </summary>
        private static List<WX_MsgTemplateLinkData> DataList { get; set; }
        /// <summary>
        /// 静态构造
        /// </summary>
        static WX_MsgTemplateLinkData()
        {
            DataList = new List<WX_MsgTemplateLinkData>();
            WX_MsgTemplateLinkData _tmp;

            #region 订单创建时
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderCreated;
            _tmp.MsgTemplateShortId = "OPENTM410180382";
            _tmp.ReturnUrl = "/m-WeiXin/Member/Orders?orderStatus=1";//"/m-WeiXin/Order/Detail?id={id}";
            DataList.Add(_tmp);
            #endregion

            #region 订单付款时
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderPay;
            _tmp.MsgTemplateShortId = "OPENTM207791277";
            _tmp.ReturnUrl = "/m-WeiXin/Order/Detail/{id}";
            DataList.Add(_tmp);
            #endregion            

            #region 订单发货
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderShipping;
            _tmp.MsgTemplateShortId = "OPENTM200565259";
            _tmp.ReturnUrl = "/m-WeiXin/Order/Detail/{id}";
            DataList.Add(_tmp);
            #endregion

            #region 订单退款成功
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderRefundSuccess;
            _tmp.MsgTemplateShortId = "OPENTM411451917";
            _tmp.ReturnUrl = "/m-WeiXin/OrderRefund/RefundDetail/{id}";
            DataList.Add(_tmp);
            #endregion            

            #region 退款失败
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderRefundFail;
            _tmp.MsgTemplateShortId = "OPENTM411734082";
            DataList.Add(_tmp);
            #endregion

            #region 退货成功
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderReturnSuccess;
            _tmp.MsgTemplateShortId = "OPENTM412612111";
            _tmp.ReturnUrl = "/m-WeiXin/OrderRefund/RefundDetail/{id}";
            DataList.Add(_tmp);
            #endregion

            #region 退货失败
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderReturnFail;
            _tmp.MsgTemplateShortId = "OPENTM411328388";
            DataList.Add(_tmp);
            #endregion


            #region 拼团

            #region 开团成功
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.FightGroupOpenSuccess;
            _tmp.MsgTemplateShortId = "OPENTM400048565";
            _tmp.ReturnUrl = "/m-WeiXin/MyFightGroup/GroupDetail/{gid}?aid={aid}";
            DataList.Add(_tmp);
            #endregion

            #region 参团成功
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.FightGroupJoinSuccess;
            _tmp.MsgTemplateShortId = "OPENTM400048581";
            _tmp.ReturnUrl = "/m-WeiXin/MyFightGroup/GroupDetail/{gid}?aid={aid}";
            DataList.Add(_tmp);
            #endregion            

            #region 拼团失败
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.FightGroupFailed;
            _tmp.MsgTemplateShortId = "OPENTM400833482";
            _tmp.ReturnUrl = "/m-WeiXin/FightGroup/Detail/{aid}";
            DataList.Add(_tmp);
            #endregion

            #region 拼团成功
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.FightGroupSuccess;
            _tmp.MsgTemplateShortId = "OPENTM400341405";
            _tmp.ReturnUrl = "/m-WeiXin/MyFightGroup/GroupDetail/{gid}?aid={aid}";
            DataList.Add(_tmp);
            #endregion
            #endregion


            #region 自提：自提订单支付成功
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.SelfTakeOrderPay;
            _tmp.MsgTemplateShortId = "OPENTM411882270";
            DataList.Add(_tmp);
            #endregion

            #region 自提：核销成功
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.AlreadyVerification;
            _tmp.MsgTemplateShortId = "OPENTM409583769";
            _tmp.ReturnUrl = "/m-WeiXin/Order/Detail/{id}";
            DataList.Add(_tmp);
            #endregion

            #region 虚拟订单：虚拟订单支付成功
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.VirtualOrderPay;
            _tmp.MsgTemplateShortId = "OPENTM410677954";
            _tmp.ReturnUrl = "/m-WeiXin/Member/Orders?orderStatus=3";//待消费列表
            DataList.Add(_tmp);
            #endregion

            #region 虚拟订单：核销成功
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.VirtualAlreadyVerification;
            _tmp.MsgTemplateShortId = "OPENTM409583769";
            _tmp.ReturnUrl = "/m-WeiXin/Order/electroniccredentials?orderId={id}";//电子凭证列表
            DataList.Add(_tmp);
            #endregion

            #region 分销：申请审核通过
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.DistributorAuditSuccess;
            _tmp.MsgTemplateShortId = "OPENTM412091602";
            _tmp.ReturnUrl = "/m-WeiXin/Distribution/MyShop/";
            DataList.Add(_tmp);
            #endregion

            #region 分销：申请审核拒绝
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.DistributorAuditFail;
            _tmp.MsgTemplateShortId = "OPENTM413617235";
            _tmp.ReturnUrl = "/m-WeiXin/Distribution/MyShop/";
            DataList.Add(_tmp);
            #endregion

            #region 分销：会员发展成功
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.DistributorNewJoin;
            _tmp.MsgTemplateShortId = "OPENTM207679900";
            DataList.Add(_tmp);
            #endregion

            #region 分销：有已结算佣金时
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.DistributorCommissionSettled;
            _tmp.MsgTemplateShortId = "OPENTM201812627";
            _tmp.ReturnUrl = "/m-WeiXin/Distribution/MyBrokerage/";
            DataList.Add(_tmp);
            #endregion

            #region 提现成功
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.MemberWithDrawSuccess;
            _tmp.MsgTemplateShortId = "OPENTM411289880";
            DataList.Add(_tmp);
            #endregion

            #region 提现失败
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.MemberWithDrawFail;
            _tmp.MsgTemplateShortId = "OPENTM413491756";
            DataList.Add(_tmp);
            #endregion

            #region 发货提醒
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.ShopOrderShipping;
            _tmp.MsgTemplateShortId = "OPENTM409521500";
            DataList.Add(_tmp);
            #endregion

            #region 售后处理提醒
            _tmp = new WX_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.ShopDelivering;
            _tmp.MsgTemplateShortId = "OPENTM414191393";
            DataList.Add(_tmp);
            #endregion


            #region TDO:ZYF 去掉不需要的消息类型
            #region 订单申请退款
            //_tmp = new WX_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.OrderRefundApply;
            //_tmp.MsgTemplateShortId = "OPENTM411451917";
            //_tmp.ReturnUrl = "/m-WeiXin/OrderRefund/RefundDetail/{id}";
            //DataList.Add(_tmp);
            #endregion

            #region 申请退货
            //_tmp = new WX_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.OrderReturnApply;
            //_tmp.MsgTemplateShortId = "OPENTM412612111";
            //_tmp.ReturnUrl = "/m-WeiXin/OrderRefund/RefundDetail/{id}";
            //DataList.Add(_tmp);
            #endregion

            #region 会员修改交易密码成功
            //_tmp = new WX_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.EditPayPassWord;
            //_tmp.MsgTemplateShortId = "OPENTM408956269";
            //_tmp.ReturnUrl = "/m-WeiXin/Member/Center";
            //DataList.Add(_tmp);
            #endregion

            #region 会员修改登录密码成功
            //_tmp = new WX_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.EditLoginPassWord;
            //_tmp.MsgTemplateShortId = "OPENTM408956269";
            //_tmp.ReturnUrl = "/m-WeiXin/Member/Center";
            //DataList.Add(_tmp);
            #endregion

            #region 获得一笔佣金时
            //_tmp = new WX_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.GetBrokerage;
            //_tmp.MsgTemplateShortId = "OPENTM201812627";
            //_tmp.ReturnUrl = "/m-WeiXin/Member/Center";
            //DataList.Add(_tmp);
            #endregion
            #region 售后发货
            //_tmp = new WX_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.RefundDeliver;
            //_tmp.MsgTemplateShortId = "OPENTM203847595";
            //DataList.Add(_tmp);
            #endregion

            #region 店铺有新订单
            //_tmp = new WX_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.ShopHaveNewOrder;
            //_tmp.MsgTemplateShortId = "OPENTM200750297";
            //DataList.Add(_tmp);
            #endregion

            #region 领取红包通知
            //_tmp = new WX_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.ReceiveBonus;
            //_tmp.MsgTemplateShortId = "OPENTM411793530";
            //_tmp.ReturnUrl = "/m-WeiXin/Member/Center";
            //DataList.Add(_tmp);
            #endregion

            #region 限时购通知
            //_tmp = new WX_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.LimitTimeBuy;
            //_tmp.MsgTemplateShortId = "OPENTM205364973";
            //_tmp.ReturnUrl = "/m-wap/limittimebuy/detail/{id}";
            //DataList.Add(_tmp);
            #endregion

            #region 订阅限时购
            //_tmp = new WX_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.SubscribeLimitTimeBuy;
            //_tmp.MsgTemplateShortId = "OPENTM201272994";
            //DataList.Add(_tmp);
            #endregion

            #region 有新成员参团
            //_tmp = new WX_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.FightGroupNewJoin;
            //_tmp.MsgTemplateShortId = "OPENTM413232421";
            //_tmp.ReturnUrl = "/m-WeiXin/MyFightGroup/GroupDetail/{gid}?aid={aid}";
            //DataList.Add(_tmp);
            #endregion

            #region 申请提现
            //_tmp = new WX_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.MemberWithDrawApply;
            //_tmp.MsgTemplateShortId = "OPENTM410690719";
            //DataList.Add(_tmp);
            #endregion

            #region 分销：申请成为销售员
            //_tmp = new WX_MsgTemplateLinkData();
            //_tmp.MsgType = MessageTypeEnum.DistributorApply;
            //_tmp.MsgTemplateShortId = "OPENTM412091602";
            //DataList.Add(_tmp);
            #endregion
            #endregion

        }
        /// <summary>
        /// 获取消息与微信短编号关联
        /// </summary>
        /// <returns></returns>
        public static List<WX_MsgTemplateLinkData> GetList()
        {
            return DataList;
        }
        #endregion
    }
}
