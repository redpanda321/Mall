
using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Areas.Admin.Models.Market;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Senparc.NeuChar;
using Senparc.NeuChar.Entities;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.MessageContexts;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;

namespace Mall.Web.Areas.Mobile.Controllers
{

    public class WXApiController : BaseMobileController
    {
        private IWXCardService _iWXCardService;
        private IBonusService _iBonusService;
        private IVShopService _iVShopService;
        private IWXApiService _iWXApiService;
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private IWXMsgTemplateService _iWXMsgTemplateService;
        public WXApiController(
            IWXCardService iWXCardService,
            IBonusService iBonusService,
            IVShopService iVShopService,
            IWXApiService iWXApiService,
            ILimitTimeBuyService iLimitTimeBuyService,
            IWXMsgTemplateService iWXMsgTemplateService
            )
        {
            _iWXCardService = iWXCardService;
            _iBonusService = iBonusService;
            _iVShopService = iVShopService;
            _iWXApiService = iWXApiService;
            _iLimitTimeBuyService = iLimitTimeBuyService;
            _iWXMsgTemplateService = iWXMsgTemplateService;

        }
        // GET: Mobile/Weixin/WXApi
    
        [AllowAnonymous]
        public ActionResult Index(long id = 0)
        {
            Log.Info("进入微信API");

            var siteSetting = SiteSettingApplication.SiteSettings;
            //string token = "weixin_test";
            string token = "", signature = "", nonce = "", timestamp = "", echostr = "";
            if (id == 0)
            {
                token = siteSetting.WeixinToken;
            }
            else
            {
                var _tmp = _iVShopService.GetVShopSetting(id);
                if (_tmp == null)
                {
                    throw new MallException("错误的商铺编号");
                }
                token = _tmp.Token;
            }
            signature = Request.Query["signature"];
            nonce = Request.Query["nonce"];
            timestamp = Request.Query["timestamp"];
            echostr = Request.Query["echostr"];
            ActionResult actionResult = Content("");

            Log.Info(string.Format("传入信息：signature = {0} , nonce = {1} , timestamp = {2} , echostr = {3} , id = {4}", signature, nonce, timestamp, echostr, id));
            if (Request.Method == "GET")
            {
                if (CheckSignature.Check(signature, timestamp, nonce, token))
                {
                    actionResult = Content(echostr);
                }
            }
            else
            {
                //TODO:[lly] 微信消息处理
                if (!CheckSignature.Check(signature, timestamp, nonce, token))
                {
                    Log.Info("验证不通过");
                }

                XDocument requestDoc = XDocument.Load(Request.Body);

#if DEBUG
                //调试时记录数据日志
                Core.Log.Debug(requestDoc);
#endif

                var requestBaseMsg = RequestMessageFactory.GetRequestEntity(new DefaultMpMessageContext(),requestDoc);
                SceneHelper helper = new SceneHelper();

                //卡券服务
                var cardser = _iWXCardService;

                switch (requestBaseMsg.MsgType)
                {
                    #region Event处理
                    case RequestMsgType.Event:
                        var requestMsg = requestBaseMsg as Senparc.Weixin.MP.Entities.RequestMessageEventBase;
                        Log.Info("进入RequestMsgType - Event：" + requestMsg.Event.ToString());
                        switch (requestMsg.Event)
                        {
                            //取消公众号订阅事件
                            case Event.unsubscribe:
                                UnSubscribe(requestMsg.FromUserName);
                                break;

                            //订阅公众号事件
                            case Event.subscribe:

                                //关注红包
                                actionResult = SendAttentionToUser(requestBaseMsg);


                                #region 关注公众号，欢迎语！
                                int nowtime = ConvertDateTimeInt(DateTime.Now);

                                //string msg = "";//关注时，需要发送的内容
                                var data = WeixinAutoReplyApplication.GetAutoReplyByKey(CommonModel.ReplyType.Follow, "", false, true, true);
                                //string resxml = "<xml><ToUserName><![CDATA[" + requestBaseMsg.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + requestBaseMsg.ToUserName + "]]></FromUserName><CreateTime>" + nowtime + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[" + msg + "]]></Content><FuncFlag>0</FuncFlag></xml>";
                                if (data == null)
                                    Log.Info("请设置关注后自动回复消息内容！");
                                string resxml = GetResxml(data, requestBaseMsg);
                                if (!string.IsNullOrEmpty(resxml))
                                {
                                    Response.WriteAsync(resxml).Wait();
                                }
                                //Response.Write(resxml);
                                #endregion

                                var requestSubscribe = requestMsg as Senparc.Weixin.MP.Entities.RequestMessageEvent_Subscribe;
#if DEBUG
                                Log.Debug(requestSubscribe, new Exception("进入订阅事件"));
#endif
                                bool isActivityBonus = false;
                                SceneModel sceneModel = null;
#if DEBUG
                                Log.Info("进入订阅事件requestSubscribe.EventKey =" + requestSubscribe.EventKey);
#endif
                                #region 场景二维码

                                if (requestSubscribe.EventKey != string.Empty)
                                {
                                    var scene = requestSubscribe.EventKey.Replace("qrscene_", string.Empty);
                                    sceneModel = helper.GetModel(scene);
                                    if (sceneModel != null)
                                    {
#if DEBUG
                                        Log.Info("sceneModel.Type = " + sceneModel.SceneType.ToString());
#endif
                                        if (sceneModel.SceneType == QR_SCENE_Type.WithDraw)
                                        {//提现场景（未关注时）
                                            echostr = ProcessWithDrawScene(requestSubscribe, scene, sceneModel);
                                            actionResult = Content(echostr);
                                        }
                                        else if (sceneModel.SceneType == QR_SCENE_Type.Bonus)
                                        {
                                            //TODO:ZJT 用户通过场景二维码扫描关注后 推送消息给用户
                                            //isActivityBonus用于判断激发关注事件的是活动红包还是其他
                                            isActivityBonus = true;
                                            actionResult = SendActivityToUser(sceneModel.Object, requestBaseMsg);
                                        }
                                        else if (sceneModel.SceneType == QR_SCENE_Type.FlashSaleRemind)
                                        {
#if DEBUG
                                            Log.Info("进入限时购场景二维码事件");
#endif
                                            long flashSaleId = (long)sceneModel.Object;
                                            SendFlashSaleRemindMessage(flashSaleId, requestBaseMsg.FromUserName);
                                        }
                                        else if (sceneModel.SceneType == QR_SCENE_Type.Binding)//绑定微信
                                        {
                                            echostr = Binding(requestSubscribe, scene, sceneModel);
                                            actionResult = Content(echostr);
                                        }
                                    }
                                }
                                #endregion
                                Log.Info("开始isActivityBonus=" + isActivityBonus);

                                //通过场景二维码进来，并且二维码不是活动红包，则进行关注送红包事件
                                //if (!isActivityBonus && sceneModel != null && sceneModel.SceneType == QR_SCENE_Type.Bonus)
                                //{
                                // actionResult = SendAttentionToUser(requestBaseMsg);
                                //}

#if DEBUG
                                Log.Info("开始Subscribe requestBaseMsg.FromUserName=" + requestBaseMsg.FromUserName);
#endif
                                Subscribe(requestBaseMsg.FromUserName);

                                break;

                            case Event.scan:
                                var requestScan = requestMsg as Senparc.Weixin.MP.Entities.RequestMessageEvent_Scan;
                                if (!string.IsNullOrWhiteSpace(requestScan.EventKey))
                                {
                                    sceneModel = helper.GetModel(requestScan.EventKey);
                                    if (sceneModel != null)
                                    {
                                        //提现场景(之前已关注)
                                        if (sceneModel.SceneType == QR_SCENE_Type.WithDraw)
                                        {
                                            echostr = ProcessWithDrawScene(requestScan, requestScan.EventKey, sceneModel);
                                            actionResult = Content(echostr);
                                        }
                                        else if (sceneModel.SceneType == QR_SCENE_Type.FlashSaleRemind)
                                        {
                                            Log.Info("进入限时购场景二维码事件");
                                            long flashSaleId = (long)sceneModel.Object;
                                            SendFlashSaleRemindMessage(flashSaleId, requestBaseMsg.FromUserName);
                                        }
                                        else if (sceneModel.SceneType == QR_SCENE_Type.Binding)//绑定微信
                                        {
                                            echostr = Binding(requestScan, requestScan.EventKey, sceneModel);
                                            actionResult = Content(echostr);
                                        }
                                        else if (sceneModel.SceneType == QR_SCENE_Type.ShopShipper)//绑定收发货
                                        {
                                            echostr = BindingByShopShipper(requestScan, requestScan.EventKey, sceneModel);
                                            actionResult = Content(echostr);
                                        }
                                    }
                                }
                                break;
                            case Event.card_pass_check:
                                //TODO:DZY[150907] 卡券审核通过
                                var reqpasscard = requestMsg as Senparc.Weixin.MP.Entities.RequestMessageEvent_Card_Pass_Check;
                                if (!string.IsNullOrWhiteSpace(reqpasscard.CardId))
                                {
                                    cardser.Event_Audit(reqpasscard.CardId, Entities.WXCardLogInfo.AuditStatusEnum.Audited);
                                }
                                break;
                            case Event.card_not_pass_check:
                                //TODO:DZY[150907] 卡券审核失败
                                var reqnopasscard = requestMsg as Senparc.Weixin.MP.Entities.RequestMessageEvent_Card_Pass_Check;
                                if (!string.IsNullOrWhiteSpace(reqnopasscard.CardId))
                                {
                                    cardser.Event_Audit(reqnopasscard.CardId, Entities.WXCardLogInfo.AuditStatusEnum.AuditNot);
                                }
                                break;
                            case Event.user_del_card:
                                //TODO:DZY[150907] 删除卡包内优惠券
                                var requdelcard = requestMsg as Senparc.Weixin.MP.Entities.RequestMessageEvent_User_Del_Card;
                                if (!string.IsNullOrWhiteSpace(requdelcard.CardId) && !string.IsNullOrWhiteSpace(requdelcard.UserCardCode))
                                {
                                    cardser.Event_Unavailable(requdelcard.CardId, requdelcard.UserCardCode);
                                }
                                break;
                            case Event.user_get_card:
                                //TODO:DZY[150907] 用户获取优惠券
                                var requgetcard = requestMsg as Senparc.Weixin.MP.Entities.RequestMessageEvent_User_Get_Card;
                                Log.Debug("WXCard:" + requgetcard.CardId + "_" + requgetcard.UserCardCode);
                                if (!string.IsNullOrWhiteSpace(requgetcard.CardId) && !string.IsNullOrWhiteSpace(requgetcard.UserCardCode))
                                {
                                    cardser.Event_Send(requgetcard.CardId, requgetcard.UserCardCode, requgetcard.FromUserName, requgetcard.OuterId);
                                }
                                break;
                        }
                        break;
                    #endregion
                    case RequestMsgType.Text:
                        textCase(requestBaseMsg);
                        break;
                }
             //   Response.End();
            }
            return actionResult;
        }
        private void textCase(IRequestMessageBase requestBaseMsg)
        {
            var siteSetting = SiteSettingApplication.SiteSettings;
            int nowtime = ConvertDateTimeInt(DateTime.Now);
            //string msg = "";
            //用户发送过来的消息内容
            string content = (requestBaseMsg as RequestMessageText).Content;
            //msg = getText(content);
            Log.Info("微信用户输入内容：" + content);
            var data = WeixinAutoReplyApplication.GetAutoReplyByKey(CommonModel.ReplyType.Keyword, content, false, true, false);
            if (data == null)
            {
                Log.Info("请设置自动回复内容！");
            }
            else
            {
                string resxml = GetResxml(data, requestBaseMsg);
                if (!string.IsNullOrEmpty(resxml))
                {
                    Response.WriteAsync(resxml).Wait();
                }
            }
        }

        private string GetResxml(Entities.AutoReplyInfo data, IRequestMessageBase requestBaseMsg)
        {
            string resxml = "";
            try
            {
                var siteSetting = SiteSettingApplication.SiteSettings;
                int nowtime = ConvertDateTimeInt(DateTime.Now);
                if (data.ReplyContentType == 1)
                {
                    if (!string.IsNullOrEmpty(data.TextReply))
                        resxml = "<xml><ToUserName><![CDATA[" + requestBaseMsg.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + requestBaseMsg.ToUserName + "]]></FromUserName><CreateTime>" + nowtime + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[" + data.TextReply + "]]></Content><FuncFlag>0</FuncFlag></xml>";
                }
                else if (data.ReplyContentType == 2)
                {
                    if (string.IsNullOrWhiteSpace(siteSetting.WeixinAppId))
                    {
                        Log.Info("未配置微信公众号！");
                    }
                    if (string.IsNullOrEmpty(data.MediaId))
                    {
                        Log.Info("素材ID不能为空！");
                    }
                    var artList = _iWXMsgTemplateService.GetMedia(data.MediaId, siteSetting.WeixinAppId, siteSetting.WeixinAppSecret);
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<xml>");
                    sb.AppendFormat("<ToUserName><![CDATA[{0}]]></ToUserName>", requestBaseMsg.FromUserName);
                    sb.AppendFormat("<FromUserName><![CDATA[{0}]]></FromUserName>", requestBaseMsg.ToUserName);
                    sb.AppendFormat("<CreateTime>{0}</CreateTime>", nowtime);
                    sb.Append("<MsgType><![CDATA[news]]></MsgType>");
                    sb.AppendFormat("<ArticleCount>{0}</ArticleCount>", artList.Count());
                    sb.Append("<Articles>");
                    foreach (var item in artList)
                    {
                        Log.Debug("微信自动回复图文消息的图片media_id：" + item.thumb_media_id);
                        var ImageDir = Core.Helper.IOHelper.GetMapPath("/Storage/Weixin/AutoReply");
                        if (!System.IO.Directory.Exists(ImageDir))
                            System.IO.Directory.CreateDirectory(ImageDir);
                        string filepath = Path.Combine(ImageDir, item.thumb_media_id + ".png");
                        if (!System.IO.File.Exists(filepath))
                        {
                            MemoryStream stream = new MemoryStream();
                            _iWXMsgTemplateService.GetMedia(item.thumb_media_id, siteSetting.WeixinAppId, siteSetting.WeixinAppSecret, stream);
                            System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
                            img.Save(filepath);
                        }
                        Log.Debug("微信自动回复图文消息的图片Url:" + filepath);
                        var repUrl = CurrentUrlHelper.CurrentUrlNoPort() + "/Storage/Weixin/AutoReply/" + item.thumb_media_id + ".png";
                        sb.Append("<item>");
                        sb.AppendFormat("<Title><![CDATA[{0}]]></Title>", item.title);
                        sb.AppendFormat("<Description><![CDATA[{0}]]></Description>", item.digest);
                        sb.AppendFormat("<PicUrl><![CDATA[{0}]]></PicUrl>", repUrl);
                        sb.AppendFormat("<Url><![CDATA[{0}]]></Url>", item.url);
                        sb.Append("</item>");
                    }
                    sb.Append("</Articles>");
                    sb.Append("</xml>");
                    resxml = sb.ToString();
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
            return resxml;
        }
        //private string getText(string content)
        //{
        //    Log.Info("微信用户输入内容：" + content);
        //    var item = WeixinAutoReplyApplication.GetAutoReplyByKey(CommonModel.ReplyType.Keyword, content, false, true, false);
        //    if (item == null)
        //    {
        //        Log.Info("请设置自动回复内容！");
        //        return "";
        //    }
        //    //Log.Info("根据关键字取到的数据：" + item.Id);
        //    return item.TextReply;
        //}
        private int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }
        /// <summary>
        /// 提现
        /// </summary>
        /// <param name="weixinMsg"></param>
        /// <param name="sceneid"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private string ProcessWithDrawScene(RequestMessageEventBase weixinMsg, string sceneid, SceneModel model)
        {
            try
            {
                var key = CacheKeyCollection.SceneReturn(sceneid);
                var sceneResult = Core.Cache.Get<ApplyWithdrawInfo>(key);
                if (sceneResult == null)
                {
                    var siteSetting = SiteSettingApplication.SiteSettings;
                    if (!(string.IsNullOrWhiteSpace(siteSetting.WeixinAppId) || string.IsNullOrWhiteSpace(siteSetting.WeixinAppSecret)))
                    {
                        string token = AccessTokenContainer.TryGetAccessToken(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret);
                        var userinfo = Senparc.Weixin.MP.CommonAPIs.CommonApi.GetUserInfo(token, weixinMsg.FromUserName);
                        var withDraw = new ApplyWithdrawInfo
                        {
                            MemId = long.Parse(model.Object.ToString()),
                            OpenId = weixinMsg.FromUserName,
                            ApplyStatus = ApplyWithdrawInfo.ApplyWithdrawStatus.WaitConfirm,
                            NickName = userinfo.nickname
                        };
                        if (Core.Cache.Get<ApplyWithdrawInfo>(key) != null)
                            Core.Cache.Remove(key);
                        Core.Cache.Insert(key, withDraw, 300);
                    }
                    else
                    {
                        Core.Log.Error("微信事件回调：未设置公众号配置参数！");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("ProcessWithDrawScene:" + ex.Message);
            }
            return string.Empty;
        }


        /// <summary>
        /// 绑定微信
        /// </summary>
        /// <param name="weixinMsg"></param>
        /// <param name="sceneid"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private string Binding(RequestMessageEventBase weixinMsg, string sceneid, SceneModel model)
        {
            try
            {
                var key = CacheKeyCollection.BindingReturn(sceneid);
                var sceneResult = Core.Cache.Get<Mall.DTO.WeiXinInfo>(key);
                if (sceneResult == null)
                {
                    var siteSetting = SiteSettingApplication.SiteSettings;
                    if (!(string.IsNullOrWhiteSpace(siteSetting.WeixinAppId) || string.IsNullOrWhiteSpace(siteSetting.WeixinAppSecret)))
                    {
                        string token = AccessTokenContainer.TryGetAccessToken(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret);
                        var userinfo = Senparc.Weixin.MP.CommonAPIs.CommonApi.GetUserInfo(token, weixinMsg.FromUserName);
                        string sex = "未知";
                        switch (userinfo.sex)
                        {
                            case 1: sex = "男"; break;
                            case 2: sex = "女"; break;
                        }
                        var WeiXinInfo = new Mall.DTO.WeiXinInfo
                        {
                            OpenId = weixinMsg.FromUserName,
                            NickName = userinfo.nickname,
                            city = userinfo.city,
                            province = userinfo.province,
                            sex = sex,
                            headimgurl = userinfo.headimgurl
                        };
                        if (Core.Cache.Get<WeiXinInfo>(key) != null)
                            Core.Cache.Remove(key);
                        Core.Cache.Insert(key, WeiXinInfo, 300);
                    }
                    else
                    {
                        Core.Log.Error("微信事件回调：未设置公众号配置参数！");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Binding:" + ex.Message);
            }
            return string.Empty;
        }
        /// <summary>
        /// 绑定微信到商家收退款地址
        /// </summary>
        /// <param name="weixinMsg"></param>
        /// <param name="sceneid"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private string BindingByShopShipper(RequestMessageEventBase weixinMsg, string sceneid, SceneModel model)
        {
            try
            {
                var key = CacheKeyCollection.SceneReturn(sceneid);
                var sceneResult = Core.Cache.Get<Mall.DTO.WeiXinInfo>(key);
                if (sceneResult == null)
                {
                    var siteSetting = SiteSettingApplication.SiteSettings;
                    if (!(string.IsNullOrWhiteSpace(siteSetting.WeixinAppId) || string.IsNullOrWhiteSpace(siteSetting.WeixinAppSecret)))
                    {
                        string token = AccessTokenContainer.TryGetAccessToken(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret);
                        var userinfo = Senparc.Weixin.MP.CommonAPIs.CommonApi.GetUserInfo(token, weixinMsg.FromUserName);
                        string sex = "未知";
                        switch (userinfo.sex)
                        {
                            case 1: sex = "男"; break;
                            case 2: sex = "女"; break;
                        }
                        var WeiXinInfo = new Mall.DTO.WeiXinInfo
                        {
                            OpenId = weixinMsg.FromUserName,
                            NickName = userinfo.nickname,
                            city = userinfo.city,
                            province = userinfo.province,
                            sex = sex,
                            headimgurl = userinfo.headimgurl
                        };
                        if (Core.Cache.Get<WeiXinInfo>(key) != null)
                            Core.Cache.Remove(key);
                        Core.Cache.Insert(key, WeiXinInfo, 300);
                    }
                    else
                    {
                        Core.Log.Error("微信事件回调：未设置公众号配置参数！");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Binding:" + ex.Message);
            }
            return string.Empty;
        }

        private ActionResult SendActivityToUser(BonusModel bonusModel, IRequestMessageBase requestBaseMsg)
        {
            string url = CurrentUrlHelper.CurrentUrlNoPort() + "/m-weixin/Bonus/Index/" + bonusModel.Id;
            string content = string.Format("<a href='" + url + "'>您参加{0}，成功获得{1}赠送的红包</a>，点击去拆红包", bonusModel.Name, bonusModel.MerchantsName);
            string result = DealTextMsg(requestBaseMsg, content);
            return new XmlResult(result);
        }

        /// <summary>
        /// 增加一条需要发送的开团提醒记录
        /// </summary>
        public void SendFlashSaleRemindMessage(long flashSaleId, string openid)
        {
            _iLimitTimeBuyService.AddRemind(flashSaleId, openid);
        }

        /// <summary>
        /// 发送活动红包
        /// </summary>
        private ActionResult SendActivityToUser(object sceneObj, IRequestMessageBase requestBaseMsg)
        {
            var bonusModel = sceneObj as BonusModel;
            if (bonusModel.Type == Entities.BonusInfo.BonusType.Activity)
            {
                try
                {
                    return SendActivityToUser(bonusModel, requestBaseMsg);
                }
                catch (Exception e)
                {
                    Log.Info("活动红包出错：", e);
                }
            }
            return Content("");
        }

        private string DealTextMsg(IRequestMessageBase doc, string msg)
        {
            string result = string.Empty;
            string fromUserName = doc.FromUserName;
            string myWeixinId = doc.ToUserName;
            result = "";
            result += "<xml>";
            result += "<ToUserName><![CDATA[" + fromUserName + "]]></ToUserName>";
            result += "<FromUserName><![CDATA[" + myWeixinId + "]]></FromUserName>";
            result += "<CreateTime>" + GetWeixinDateTime(DateTime.Now) + "</CreateTime>";
            result += "<MsgType><![CDATA[text]]></MsgType>";
            result += "<Content><![CDATA[" + msg + "]]></Content>";
            result += "</xml>";
            return result;
        }

        /// <summary>
        /// 发送关注红包
        /// </summary>
        private ActionResult SendAttentionToUser(IRequestMessageBase requestBaseMsg)
        {
            string msg = "";
            try
            {
                IBonusService bonusService = _iBonusService;
                Log.Debug("关注红包openId：" + requestBaseMsg.FromUserName);
                msg = bonusService.Receive(requestBaseMsg.FromUserName);
                if (!string.IsNullOrEmpty(msg))
                {
                    string result = DealTextMsg(requestBaseMsg, msg);
                    return new XmlResult(result);
                }
            }
            catch (Exception e)
            {
                Log.Info("关注红包出错：", e);
            }
            return Content("");
        }

        /// <summary>
        /// 获取微信DateTime（UNIX时间戳）
        /// </summary>
        public static long GetWeixinDateTime(DateTime dateTime)
        {
            DateTime BaseTime = new DateTime(1970, 1, 1);
            return (dateTime.Ticks - BaseTime.Ticks) / 10000000 - 8 * 60 * 60;
        }

        public ContentResult VShopApi(long vshopId)
        {
            var vshopSetting = _iVShopService.GetVShopSetting(vshopId);
            string token = vshopSetting.Token;
            string signature = Request.Query["signature"];
            string nonce = Request.Query["nonce"];
            string timestamp = Request.Query["timestamp"];
            string echostr = Request.Query["echostr"];

            if (Request.Method == "GET")
            {
                //get method - 仅在微信后台填写URL验证时触发
                if (!Hishop.Weixin.MP.Util.CheckSignature.Check(signature, timestamp, nonce, token))
                    echostr = string.Empty;
            }

            return Content(echostr);
        }


        /// <summary>
        /// 取消订阅
        /// </summary>
        private void UnSubscribe(string openId)
        {
            Task.Factory.StartNew(() => _iWXApiService.UnSubscribe(openId));
        }

        /// <summary>
        /// 订阅
        /// </summary>
        private void Subscribe(string openId)
        {
            Task.Factory.StartNew(() => _iWXApiService.Subscribe(openId));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult WXAuthorize(string returnUrl = "")
        {
            var settings = SiteSettingApplication.SiteSettings;
            var redirectUrl = string.Empty;

            if (!string.IsNullOrEmpty(settings.WeixinAppId))
            {
                string code = HttpContext.Request.Query["code"];
                if (!string.IsNullOrEmpty(code)) // 如果用户同意授权
                {
                    string result = GetResponseResult(string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", settings.WeixinAppId, settings.WeixinAppSecret, code));
                    if (result.Contains("access_token"))
                    {
                        var resultObj = JsonConvert.DeserializeObject(result) as JObject;
                        string userStr = GetResponseResult("https://api.weixin.qq.com/sns/userinfo?access_token=" + resultObj["access_token"].ToString() + "&openid=" + resultObj["openid"].ToString() + "&lang=zh_CN");
                        if (userStr.Contains("nickname"))
                        {
                            var userObj = JsonConvert.DeserializeObject(userStr) as JObject;
                            if (Request.Query["returnUrl"].ToString() != null)
                            {
                                var url = Request.Query["returnUrl"].ToString();
                                if (url.Contains("?"))
                                {
                                    url = "{0}&openId={1}&serviceProvider={2}&nickName={3}&headimgurl={4}&unionid={5}";
                                }
                                else
                                {
                                    url = "{0}?openId={1}&serviceProvider={2}&nickName={3}&headimgurl={4}&unionid={5}";
                                }
                                url = string.Format(url
                                    , returnUrl
                                    , System.Web.HttpUtility.UrlEncode(userObj["openid"].ToString())
                                    , System.Web.HttpUtility.UrlEncode("Mall.Plugin.OAuth.WeiXin")
                                    , System.Web.HttpUtility.UrlEncode(userObj["nickname"].ToString())
                                    , System.Web.HttpUtility.UrlEncode(userObj["headimgurl"].ToString())
                                    , userObj["unionid"] != null ? userObj["unionid"].ToString() : userObj["openid"].ToString()
                                    );
                                //Log.Debug("return Url:" + url);
                                return Redirect(url);
                            }
                        }
                    }
                }
                else //还没有到用户授权页面
                {
                    string url = HttpContext.Request.GetDisplayUrl();
                    //scope=snsapi_base,静默方法
                    url = string.Format("https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope=snsapi_userinfo&state=STATE#wechat_redirect"
                                , settings.WeixinAppId, System.Web.HttpUtility.UrlEncode(url));
                    redirectUrl = url;//指定跳转授权页面 
                    return Redirect(url);
                }
            }
            return Content("授权异常");
        }
        string GetResponseResult(string url)
        {
            string result;
            WebRequest req = WebRequest.Create(url);

            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                using (Stream receiveStream = response.GetResponseStream())
                {

                    using (StreamReader readerOfStream = new StreamReader(receiveStream, System.Text.Encoding.UTF8))
                    {
                        result = readerOfStream.ReadToEnd();
                    }
                }
            }
            return result;
        }
    }
}