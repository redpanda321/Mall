using Mall.Core.Plugins.Message;
using System.Collections.Generic;

namespace Mall.CommonModel.WeiXin
{
    public class WXApplet_MsgTemplateLinkData
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
        private static List<WXApplet_MsgTemplateLinkData> DataList { get; set; }
        /// <summary>
        /// 静态构造
        /// </summary>
        static WXApplet_MsgTemplateLinkData()
        {
            DataList = new List<WXApplet_MsgTemplateLinkData>();
            WXApplet_MsgTemplateLinkData _tmp;

            #region 订单创建时  	待付款提醒
            _tmp = new WXApplet_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderCreated;
            _tmp.MsgTemplateShortId = "AT0008";
            _tmp.ReturnUrl = "pages/orderdetails/orderdetails?orderid={id}";
            DataList.Add(_tmp);
            #endregion

            #region 订单付款时  订单支付成功通知
            _tmp = new WXApplet_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderPay;
            _tmp.MsgTemplateShortId = "AT0009";
            _tmp.ReturnUrl = "pages/orderdetails/orderdetails?orderid={id}";
            DataList.Add(_tmp);
            #endregion

            #region 订单发货  订单发货提醒
            _tmp = new WXApplet_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderShipping;
            _tmp.MsgTemplateShortId = "AT0007";
            _tmp.ReturnUrl = "pages/orderdetails/orderdetails?orderid={id}";
            DataList.Add(_tmp);
            #endregion

            #region 订单退款  退款通知
            _tmp = new WXApplet_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderRefundSuccess;
            _tmp.MsgTemplateShortId = "AT0036";
            _tmp.ReturnUrl = "pages/returndetail/returndetail?orderid={id}";
            DataList.Add(_tmp);
            #endregion

            #region 订单退款  退款失败通知
            _tmp = new WXApplet_MsgTemplateLinkData();
            _tmp.MsgType = MessageTypeEnum.OrderRefundFail;
            _tmp.MsgTemplateShortId = "AT0329";
            _tmp.ReturnUrl = "pages/returndetail/returndetail?orderid={id}";
            DataList.Add(_tmp);
            #endregion
        }
        /// <summary>
        /// 获取消息与微信短编号关联
        /// </summary>
        /// <returns></returns>
        public static List<WXApplet_MsgTemplateLinkData> GetList()
        {
            return DataList;
        }
        #endregion
    }
}
