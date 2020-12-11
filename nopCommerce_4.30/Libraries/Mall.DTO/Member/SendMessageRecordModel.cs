using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.CommonModel;

namespace Mall.DTO
{
    public class SendMessageRecordModel
    {
        public long Id { get; set; }

        /// <summary>
		/// 消息类别
		/// </summary>
        public MsgType MessageType { get; set; }

        /// <summary>
        /// 内容类型
        /// </summary>
        public WXMsgType ContentType { get; set; }

        /// <summary>
		/// 发送内容
		/// </summary>
        public string SendContent { get; set; }

        /// <summary>
		/// 发送对象
		/// </summary>
        public string ToUserLabel { get; set; }

        /// <summary>
		/// 发送状态
		/// </summary>
        public int? SendState { get; set; }

        /// <summary>
		/// 发送时间
		/// </summary>
        public DateTime SendTime { get; set; }

        public string SendTimeStr { get { return SendTime.ToString("yyyy-MM-dd HH:mm:ss"); } }

        #region 群发对象内容拆分
        /// <summary>
        /// 群发送对象
        /// </summary>
        public string ToUserObject { get; set; }

        /// <summary>
        /// 群发送对象性别
        /// </summary>
        public string ToUserSex { get; set; }

        /// <summary>
        /// 群发送对象地区
        /// </summary>
        public string ToUserRegion { get; set; }
        #endregion

        /// <summary>
        /// 群发微信图文时它素材模板MediaId（群发微信消息）
        /// </summary>
        public string SendWXMediaId { get; set; }

        /// <summary>
        /// 群发送标题（群发邮件--邮件标题）
        /// </summary>
        public string SendEmailTitle { get; set; }

        /// <summary>
        /// 群发发送的优惠券列表(群发优惠券)
        /// </summary>
        public List<CouponModel> CouponList { get; set; }
    }
}
