using AutoMapper;
using Mall.CommonModel;
using Mall.CommonModel.WeiXin;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Nop.Core.Infrastructure;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mall.Application
{
    public class WXMsgTemplateApplication
    {

        private static IWXMsgTemplateService _iWXMsgTemplateService =  EngineContext.Current.Resolve<IWXMsgTemplateService>();

        /// <summary>
        /// 新增图文
        /// </summary>
        /// <param name="info"></param>
        /// 
        public static Entities.WXUploadNewsResult Add(IEnumerable<Entities.WXMaterialInfo> info, string appid, string appsecret)
        {
            return _iWXMsgTemplateService.Add(info, appid, appsecret);
        }
        /// <summary>
        /// 更新单条图文消息
        /// </summary>
        /// <param name="mediaid"></param>
        /// <param name="news"></param>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <returns></returns>
        public static IEnumerable<Entities.WxJsonResult> UpdateMedia(string mediaid, IEnumerable<Entities.WXMaterialInfo> news, string appid, string appsecret)
        {
            return _iWXMsgTemplateService.UpdateMedia(mediaid, news, appid, appsecret);
        }
        /// <summary>
        /// 删除素材
        /// </summary>
        /// <param name="mediaid"></param>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <returns></returns>
        public static Entities.WxJsonResult DeleteMedia(string mediaid, string appid, string appsecret)
        {
            return _iWXMsgTemplateService.DeleteMedia(mediaid, appid, appsecret);
        }
        /// <summary>
        /// 添加图片
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <returns>media_id</returns>
        public static string AddImage(string filename, string appid, string appsecret)
        {
            return _iWXMsgTemplateService.AddImage(filename, appid, appsecret);
        }
        /// <summary>
        /// 获取图文素材列表
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Entities.MediaNewsItemList GetMediaMsgTemplateList(string appid, string appsecret, int offset, int count)
        {
            return _iWXMsgTemplateService.GetMediaMsgTemplateList(appid, appsecret, offset, count);
        }
        /// <summary>
        /// 取素材总数
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <returns></returns>
        public static Entities.MediaItemCount GetMediaItemCount(string appid, string appsecret)
        {
            return _iWXMsgTemplateService.GetMediaItemCount(appid, appsecret);
        }
        /// <summary>
        /// 群发送消息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static Entities.SendInfoResult SendWXMsg(Entities.SendMsgInfo info)
        {
            return _iWXMsgTemplateService.SendWXMsg(info);
        }
        /// <summary>
        /// 取图文素材
        /// </summary>
        /// <param name="mediaid"></param>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <returns></returns>
        public static IEnumerable<Entities.WXMaterialInfo> GetMedia(string mediaid, string appid, string appsecret)
        {
            return _iWXMsgTemplateService.GetMedia(mediaid, appid, appsecret);
        }
        /// <summary>
        /// 取非图文素材
        /// </summary>
        /// <param name="mediaid"></param>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <param name="stream"></param>
        public static void GetMedia(string mediaid, string appid, string appsecret, Stream stream)
        {
            _iWXMsgTemplateService.GetMedia(mediaid, appid, appsecret, stream);
        }
        /// <summary>
        /// 添加发送记录
        /// </summary>
        /// <param name="info"></param>
        public static void AddSendRecord(SendMessageRecordInfo info, List<SendmessagerecordCouponInfo> coupons = null)
        {
            _iWXMsgTemplateService.AddSendRecord(info, coupons);
        }
        /// <summary>
        /// 取发送记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<SendMessageRecord> GetSendRecords(SendRecordQuery query)
        {
            var data = _iWXMsgTemplateService.GetSendRecords(query);
            QueryPageModel<SendMessageRecord> item = new QueryPageModel<SendMessageRecord>();
            item.Total = data.Total;
            var list = data.Models.ToList();
            //var dataList = Mapper.Map<List<SendMessageRecord>>(list);
            var dataList = list.Map<List<SendMessageRecord>>();


            foreach (var info in dataList)
            {
                var record = _iWXMsgTemplateService.GetSendrecordCouponSnById(info.Id);
                info.CurrentCouponCount = record.Count;
                foreach (var items in record)
                {
                    var result = _iWXMsgTemplateService.GetCouponRecordBySn(items.CouponSN);
                    var orderResult = result.OrderId == null ? null : OrderApplication.GetOrder(result.OrderId.Value);
                    if (result != null && orderResult != null)
                        info.CurrentUseCouponCount++;
                }
            }
            item.Models = dataList;
            return item;
        }
        public static List<Entities.SendmessagerecordCouponSNInfo> GetSendrecordCouponSnById(long id)
        {
            return _iWXMsgTemplateService.GetSendrecordCouponSnById(id);
        }
        /// <summary>
        /// 指定openIds发送微信消息
        /// </summary>
        /// <param name="openIds">发送openId集合</param>
        /// <param name="msgType">类型</param>
        /// <param name="content">文本内容</param>
        /// <param name="mediaId">模板ID</param>
        /// <param name="appId"></param>
        /// <param name="appSecret"></param>
        /// <returns></returns>
        public static Entities.SendInfoResult SendWXMsg(IEnumerable<string> openIds, WXMsgType msgType, string content, string mediaId, string appId, string appSecret)
        {
            return _iWXMsgTemplateService.SendWXMsg(openIds, msgType, content, mediaId, appId, appSecret);
        }



        #region 模板消息
        /// <summary>
        /// 获取微信模板消息列表
        /// </summary>
        /// <returns></returns>
        public static List<Entities.WeiXinMsgTemplateInfo> GetWeiXinMsgTemplateList()
        {
            return _iWXMsgTemplateService.GetWeiXinMsgTemplateList();
        }
        /// <summary>
        /// 获取微信模板信息
        /// </summary>
        /// <returns></returns>
        public static Entities.WeiXinMsgTemplateInfo GetWeiXinMsgTemplate(Mall.Core.Plugins.Message.MessageTypeEnum type)
        {
            return _iWXMsgTemplateService.GetWeiXinMsgTemplate(type);
        }
        /// <summary>
        /// 设置微信模板消息配置
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static Entities.WeiXinMsgTemplateInfo UpdateWeiXinMsgTemplate(Entities.WeiXinMsgTemplateInfo info)
        {
            return _iWXMsgTemplateService.UpdateWeiXinMsgTemplate(info);
        }
        /// <summary>
        /// 设置微信消息开启状态
        /// </summary>
        /// <param name="type"></param>
        /// <param name="isOpen"></param>
        public static void UpdateWeiXinMsgOpenState(Mall.Core.Plugins.Message.MessageTypeEnum type, bool isOpen)
        {
            _iWXMsgTemplateService.UpdateWeiXinMsgOpenState(type, isOpen);
        }
        /// <summary>
        /// 发送模板消息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="userId">为0时使用openid</param>
        /// <param name="data">信息数据</param>
        /// <param name="url"></param>
        /// <param name="openid">与userid配合使用，userid为0时使用此字段</param>
        public static void SendMessageByTemplate(Mall.Core.Plugins.Message.MessageTypeEnum type, long userId, WX_MsgTemplateSendDataModel data, string url = "", string wxopenid = "")
        {
            _iWXMsgTemplateService.SendMessageByTemplate(type, userId, data, url, wxopenid);
        }
        /// <summary>
        /// 获取模板消息跳转URL
        /// </summary>
        /// <param name="type"></param>
        public static string GetMessageTemplateShowUrl(Mall.Core.Plugins.Message.MessageTypeEnum type)
        {
            return _iWXMsgTemplateService.GetMessageTemplateShowUrl(type);
        }
        /// <summary>
        /// 添加消息模板
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="appsecret"></param>
        /// <param name="type">null表示所有都重置</param>
        public static void AddMessageTemplate(Mall.Core.Plugins.Message.MessageTypeEnum? type = null)
        {
            _iWXMsgTemplateService.AddMessageTemplate(type);
        }
        #endregion

        /// <summary>
        /// 新增小程序表单提交数据
        /// </summary>
        /// <param name="mWXSmallChoiceProductsInfo"></param>
        public static void AddWXAppletFromData(Entities.WXAppletFormDataInfo wxapplet)
        {
            _iWXMsgTemplateService.AddWXAppletFromData(wxapplet);
        }

        /// <summary>
        /// 获取发送消息记录
        /// </summary>
        /// <param name="messageId">消息记录Id</param>
        public static SendMessageRecordModel GetSendMessageRecordById(long messageId)
        {
            var record = _iWXMsgTemplateService.GetSendMessageRecordById(messageId);
            //  Mapper.CreateMap<Entities.SendMessageRecordInfo, SendMessageRecordModel>();
            //SendMessageRecordModel sendModel = Mapper.Map<SendMessageRecordInfo, SendMessageRecordModel>(record);

            SendMessageRecordModel sendModel = record.Map<SendMessageRecordModel>();


            if (sendModel == null)
                return sendModel;

            #region 群发对象拆简化（之前数据已保存了，则下面拆简）
            if (!string.IsNullOrEmpty(sendModel.ToUserLabel))
            {
                sendModel.ToUserObject = sendModel.ToUserLabel;
                string[] s1 = sendModel.ToUserLabel.Trim().Split(new string[] { "性别" }, System.StringSplitOptions.RemoveEmptyEntries);
                if(s1!=null && s1.Length > 1)
                {
                    sendModel.ToUserObject= s1[0];
                    string[] s2 = s1[1].Split(' ');
                    sendModel.ToUserSex = s2[0].Replace("：", "").Replace(":", "");
                }

                string[] sq = sendModel.ToUserLabel.Trim().Split(new string[] { "地区" }, System.StringSplitOptions.RemoveEmptyEntries);
                if (sq != null && sq.Length > 1)
                {
                    string[] s2 = sq[1].Split(' ');
                    sendModel.ToUserRegion = s2[0].Replace("：", "").Replace(":", "");
                }
                sendModel.ToUserObject = sendModel.ToUserObject.Replace("标签：", "").Replace("标签:", "");
            }
            #endregion

            #region 群发是内容是存在一起的拆简分出来
            if (!string.IsNullOrEmpty(sendModel.SendContent))
            {
                if (sendModel.MessageType == MsgType.Email)
                {
                    #region //邮件内容和标题是存在一个字段，现拆分出来
                    string[] s1 = sendModel.SendContent.Trim().Split(new string[] { "####" }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (string str in s1)
                    {
                        if (string.IsNullOrEmpty(str.Trim()))
                            continue;
                        string[] s2 = str.Split(new string[] { "■■" }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (s2.Length > 1)
                        {
                            sendModel.SendEmailTitle = s2[1];
                            break;
                        }
                    }
                    sendModel.SendContent = s1[s1.Length - 1];//内容是最后
                    #endregion
                }
                else if (sendModel.MessageType == MsgType.WeiXin && sendModel.ContentType == WXMsgType.mpnews)
                {
                    #region 微信群发图文，链接MediaId与内容是存在一个字段，现拆分出来
                    string[] s1 = sendModel.SendContent.Trim().Split(new string[] { "####" }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (string str in s1)
                    {
                        if (string.IsNullOrEmpty(str.Trim()))
                            continue;
                        string[] s2 = str.Split(new string[] { "■■" }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (s2.Length > 1)
                        {
                            sendModel.SendWXMediaId = s2[1];
                            break;
                        }
                    }
                    sendModel.SendContent = s1[s1.Length - 1];//内容是最后
                    #endregion
                }
            }
            #endregion

            if (sendModel.MessageType == MsgType.Coupon)
            {
                #region 加载它优惠列表
                ICouponService _iCouponService =  EngineContext.Current.Resolve<ICouponService>();
                List<Entities.CouponInfo> CList = _iCouponService.GetCouponBySendmessagerecordId(sendModel.Id);

                if (CList != null && CList.Count() > 0)
                {
                    sendModel.CouponList = new List<CouponModel>();
                    foreach (var cinfo in CList)
                    {
                        // Mapper.CreateMap<Entities.CouponInfo, CouponModel>();
                        // CouponModel couponModel = Mapper.Map<CouponInfo, CouponModel>(cinfo);
                        CouponModel couponModel = cinfo.Map< CouponModel>();


                        sendModel.CouponList.Add(couponModel);
                    }
                }
                #endregion
            }

            return sendModel;
        }
    }
}
