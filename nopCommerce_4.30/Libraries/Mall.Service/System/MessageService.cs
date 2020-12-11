using Mall.CommonModel;
using Mall.CommonModel.WeiXin;
using Mall.Core;
using Mall.Core.Plugins.Message;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Service
{
    public class MessageService : ServiceBase, IMessageService
    {

        public void UpdateMemberContacts(MemberContactInfo info)
        {
            DbFactory.Default.InTransaction(() =>
            {
                var exist = DbFactory.Default.Get<MemberContactInfo>().Where(a => a.ServiceProvider == info.ServiceProvider && a.UserId == info.UserId && a.UserType == info.UserType).FirstOrDefault();
                if (exist != null)
                {
                    exist.Contact = info.Contact;
                    DbFactory.Default.Update(exist);
                }
                else
                {
                    DbFactory.Default.Add(info);
                }
                var user = DbFactory.Default.Get<MemberInfo>().Where(a => a.Id == info.UserId).FirstOrDefault();
                if (user != null)
                {
                    if (info.ServiceProvider == "Mall.Plugin.Message.SMS")
                    {
                        user.CellPhone = info.Contact;
                    }
                    else
                    {
                        user.Email = info.Contact;
                    }
                    DbFactory.Default.Update(user);
                }
            });
            Core.Cache.Remove(CacheKeyCollection.Member(info.UserId));//移除用户缓存
        }

        public string GetDestination(long userId, string pluginId, MemberContactInfo.UserTypes type)
        {
            var Destination = DbFactory.Default.Get<MemberContactInfo>().Where(a => a.UserId == userId && a.ServiceProvider == pluginId && a.UserType == type).FirstOrDefault();
            if (Destination != null)
            {
                return Destination.Contact;
            }
            return "";
        }

        public string GetDestination(long userId, string pluginId)
        {
            var Destination = DbFactory.Default.Get<MemberContactInfo>().Where(a => a.UserId == userId && a.ServiceProvider == pluginId).FirstOrDefault();
            if (Destination != null)
            {
                return Destination.Contact;
            }
            return "";
        }
        public MemberContactInfo GetMemberContactsInfo(string pluginId, string contact, MemberContactInfo.UserTypes type)
        {
            return DbFactory.Default.Get<MemberContactInfo>().Where(a => a.ServiceProvider == pluginId && a.UserType == type && a.Contact == contact).FirstOrDefault();
        }

        public List<MemberContactInfo> GetMemberContactsInfo(long UserId)
        {
            return DbFactory.Default.Get<MemberContactInfo>().Where(a => a.UserId == UserId).ToList();
        }

        public void SendMessageCode(string destination, string pluginId, Core.Plugins.Message.MessageUserInfo info)
        {
            var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>(pluginId);
            if (string.IsNullOrEmpty(destination) || !messagePlugin.Biz.CheckDestination(destination))
                throw new MallException(messagePlugin.Biz.ShortName + "错误");
            var content = messagePlugin.Biz.SendMessageCode(destination, info);
            if (messagePlugin.Biz.EnableLog)
            {
                DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
            }
        }

        public void SendMessageOnFindPassWord(long userId, Core.Plugins.Message.MessageUserInfo info)
        {
            //TDO:ZYF 3.2注释
            //var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            //foreach (var msg in message)
            //{
            //    if (msg.Biz.GetStatus(MessageTypeEnum.FindPassWord) == StatusEnum.Open)
            //    {
            //        string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
            //        if (!msg.Biz.CheckDestination(destination))
            //            throw new MallException(msg.Biz.ShortName + "错误");
            //        var content = msg.Biz.SendMessageOnFindPassWord(destination, info);
            //        if (msg.Biz.EnableLog)
            //        {
            //            DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
            //        }
            //    }
            //}
        }

        /// <summary>
        /// 创建订单通知(TDO:ZYF3.2)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnOrderCreate(long userId, MessageOrderInfo info)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();

            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.OrderCreated) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnOrderCreate(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = content, TypeId = "短信" });
                    }
                }
            }

            #region 发送模板消息
            if (info.MsgOrderType == MessageOrderType.Applet)
            {
                Log.Info("小程序创建订单，准备开始发送消息");
                var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();

                    msgdata.keyword1.value = info.OrderTime.ToString("yyyy-MM-dd HH:mm:ss");
                    msgdata.keyword1.color = "#173177";
                    msgdata.keyword2.value = info.TotalMoney.ToString("F2");
                    msgdata.keyword2.color = "#ff3300";
                    msgdata.keyword3.value = info.ProductName + " 等...";
                    msgdata.keyword3.color = "#173177";
                    msgdata.keyword4.value = info.OrderId;
                    msgdata.keyword4.color = "#173177";
                    msgdata.keyword5.value = "待支付";
                    msgdata.keyword5.color = "#173177";

                    //处理url
                    var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    var _iwxmember = Mall.ServiceProvider.Instance<IMemberService>.Create;
                    string page = _iwxtser.GetWXAppletMessageTemplateShowUrl(MessageTypeEnum.OrderCreated);//小程序跳转地址
                    page = page.Replace("{id}", info.OrderId.ToString());
                    Log.Info("小程序创建订单跳转地址：" + page);
                    string openId = "";
                    try
                    {
                        openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "WeiXinSmallProg").OpenId;//登录小程序的OpenId
                        Log.Info("拿到的小程序openid为：" + openId);
                    }
                    catch (Exception e)
                    {
                        Log.Error("小程序获取openid报错：" + e.Message);
                    }
                    string formid = "";
                    var formmodel = _iwxtser.GetWXAppletFromDataById(MessageTypeEnum.OrderCreated, info.OrderId.ToString());
                    if (formmodel != null)
                        formid = formmodel.FormId;//根据OrderId获取FormId
                    Log.Info("拿到的小程序formId：" + formid);
                    _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderCreated, userId, msgdata, page, openId, formid);
                }

            }
            else
            {
                var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.first.value = "尊敬的（" + userinfo.Nick + "），您还有订单未成功付款，尽快支付别让好货错过！";
                    msgdata.first.color = "#000000";
                    msgdata.keyword1.value = info.ProductName + " ..."; //info.OrderId.ToString();
                    msgdata.keyword1.color = "#FF0000";
                    msgdata.keyword2.value = info.TotalMoney.ToString();
                    msgdata.keyword2.color = "#FF0000";
                    msgdata.keyword3.value = info.OrderTime.ToString("yyyy-MM-dd HH:mm:ss");
                    msgdata.keyword3.color = "#000000";
                    msgdata.remark.value = "二话不说，点击买单>>";
                    msgdata.remark.color = "#000000";
                    //处理url
                    var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.OrderCreated);
                    //url = url.Replace("{id}", info.OrderId.ToString());
                    _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderCreated, userId, msgdata, url);
                }
            }

            #endregion
        }
        /// <summary>
        /// 支付通知(TDO:ZYF3.2)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnOrderPay(long userId, MessageOrderInfo info)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.OrderPay) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnOrderPay(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = content, TypeId = "短信" });
                    }
                }
            }

            #region 发送模板消息
            if (info.MsgOrderType == MessageOrderType.Applet)
            {
                Log.Info("小程序支付成功，准备开始发送消息");
                var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                var _iwxmember = Mall.ServiceProvider.Instance<IMemberService>.Create;
                string formid = "";
                var formmodel = _iwxtser.GetWXAppletFromDataById(MessageTypeEnum.OrderPay, info.OrderId.ToString());
                if (formmodel != null)
                {
                    formid = formmodel.FormId;//根据OrderId获取FormId
                }
                else
                {
                    Log.Info("小程序订单：" + info.OrderId + " FormId为空");
                }
                Log.Info("小程序支付，formid=" + formid);

                var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
                if (userinfo != null)
                {
                    Log.Info("小程序支付，userId=" + userId);
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.keyword1.value = info.TotalMoney.ToString("f2") + "元";
                    msgdata.keyword1.color = "#173177";
                    msgdata.keyword2.value = info.PayTime.ToString("yyyy-MM-dd HH:mm:ss");
                    msgdata.keyword2.color = "#ff3300";
                    msgdata.keyword3.value = info.ProductName.ToString() + "等...";
                    msgdata.keyword3.color = "#173177";
                    msgdata.keyword4.value = info.OrderId.ToString();
                    msgdata.keyword4.color = "#173177";
                    msgdata.keyword5.value = info.OrderTime.ToString("yyyy-MM-dd HH:mm:ss");
                    msgdata.keyword5.color = "#173177";
                    //处理url
                    string page = _iwxtser.GetWXAppletMessageTemplateShowUrl(MessageTypeEnum.OrderPay);//小程序跳转地址
                    page = page.Replace("{id}", info.OrderId.ToString());
                    Log.Info("小程序支付跳转url=" + page);
                    string openId = "";
                    try
                    {
                        openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "WeiXinSmallProg").OpenId;//登录小程序的OpenId
                    }
                    catch (Exception e)
                    {
                        Log.Error("小程序支付，获取openid出错：" + e.Message);
                    }
                    Log.Info("小程序openid=" + openId);
                     _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderPay, userId, msgdata, page, openId, formid);
                }
            }
            else
            {
                var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.first.value = "尊敬的（" + userinfo.Nick + "），您的订单已支付成功，我们会尽快为您发货。";
                    msgdata.first.color = "#000000";
                    msgdata.keyword1.value = info.OrderId.ToString();
                    msgdata.keyword2.value = info.TotalMoney.ToString();
                    msgdata.keyword2.color = "#FF0000";
                    msgdata.remark.value = "请耐心等待收货，收到货后记得回来确认哦。";
                    msgdata.remark.color = "#000000";
                    string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.OrderPay);
                    url = url.Replace("{id}", info.OrderId.ToString());
                    _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderPay, userId, msgdata, url);
                }                
            }
            #endregion
        }

        /// <summary>
        /// 店铺有新订单
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        public void SendMessageOnShopHasNewOrder(long shopId, MessageOrderInfo info)
        {
            #region TDO:ZYF 注释 发送模板消息
            //卖家收信息
            //            var shopinfo = DbFactory.Default.Get<ManagerInfo>().Where(d => d.ShopId == shopId).FirstOrDefault();
            //            if (shopinfo != null)
            //            {
            //                var sellerinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.UserName == shopinfo.UserName).FirstOrDefault();
            //                if (sellerinfo != null)
            //                {
            //                    var msgdata = new WX_MsgTemplateSendDataModel();

            //#if DEBUG
            //                    Core.Log.Info("[模板消息]卖家新订单用户编号：" + sellerinfo.Id.ToString());
            //#endif
            //                    msgdata = new WX_MsgTemplateSendDataModel();
            //                    msgdata.first.value = "您的店铺有新的订单生成。";
            //                    msgdata.first.color = "#000000";
            //                    msgdata.keyword1.value = info.ShopName;
            //                    msgdata.keyword1.color = "#000000";
            //                    msgdata.keyword2.value = info.ProductName + "等...";
            //                    msgdata.keyword2.color = "#000000";
            //                    msgdata.keyword3.value = info.OrderTime.ToString();
            //                    msgdata.keyword3.color = "#000000";
            //                    msgdata.keyword4.value = info.TotalMoney.ToString();
            //                    msgdata.keyword4.color = "#FF0000";
            //                    msgdata.keyword5.value = "已付款(" + info.PaymentType + ")";
            //                    msgdata.keyword5.color = "#000000";
            //                    msgdata.remark.value = "感谢您的使用,祝您生意兴隆。";
            //                    msgdata.remark.color = "#000000";

            //#if DEBUG
            //                    Core.Log.Info("[模板消息]卖家新订单开始前：" + sellerinfo.Id.ToString() + "_" + info.OrderId.ToString());
            //#endif
            //                    var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
            //                    _iwxtser.SendMessageByTemplate(MessageTypeEnum.ShopHaveNewOrder, sellerinfo.Id, msgdata);


            //#if DEBUG
            //                    Core.Log.Info("[模板消息]发送商家发货人：" + shopId.ToString() + "_" + info.OrderId.ToString());
            //#endif
            //                    //发送商家发货人
            //                    var shopsend = DbFactory.Default.Get<ShopShipperInfo>().Where(d => d.ShopId == shopId && d.IsDefaultSendGoods == true).FirstOrDefault();
            //                    if (shopsend != null && !string.IsNullOrWhiteSpace(shopsend.WxOpenId))
            //                    {
            //                        var sendmsgdata = new WX_MsgTemplateSendDataModel();
            //                        sendmsgdata.first.value = info.SiteName + "中有一个新的订单付款成功，请及时发货！";
            //                        sendmsgdata.first.color = "#000000";
            //                        sendmsgdata.keyword1.value = info.OrderId.ToString();
            //                        sendmsgdata.keyword1.color = "#000000";
            //                        sendmsgdata.keyword2.value = info.PayTime.ToString();
            //                        sendmsgdata.keyword2.color = "#000000";
            //                        sendmsgdata.keyword3.value = info.TotalMoney.ToString();
            //                        sendmsgdata.keyword3.color = "#FF0000";
            //                        sendmsgdata.keyword4.value = info.PaymentType.ToString();
            //                        sendmsgdata.keyword4.color = "#000000";
            //                        sendmsgdata.remark.value = info.ProductName + "等...";
            //                        sendmsgdata.remark.color = "#000000";
            //                        string url = "";
            //                        _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderPay, 0, sendmsgdata, url, shopsend.WxOpenId);
            //                    }
            //                }
            //            }
            #endregion
        }

        /// <summary>
        /// 退款成功通知
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        /// <param name="refundid"></param>
        public void SendMessageOnOrderRefund(long userId, MessageOrderInfo info, long refundid = 0, string refundPayTypeName = "")
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.OrderRefundSuccess) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnOrderRefund(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = content, TypeId = "短信" });
                    }
                }
            }

            #region 发送模板消息
            if (info.MsgOrderType == MessageOrderType.Applet)
            {
                Log.Info("小程序退款退货，准备开始发送消息");
                //小程序发送
                string status = "退货/退款成功";
                string remark = "您的订单已经完成退款,¥" + info.RefundMoney.ToString("F2") + "已经退回您的付款账户（或预存款账户），请留意查收.";

                var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).Exist();
                if (userinfo)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.keyword1.value = "¥" + info.RefundMoney.ToString("F2") + "元";
                    msgdata.keyword1.color = "#173177";
                    msgdata.keyword2.value = refundPayTypeName;
                    msgdata.keyword2.color = "#173177";
                    msgdata.keyword3.value = status;
                    msgdata.keyword3.color = "#173177";
                    msgdata.keyword4.value = remark;
                    msgdata.keyword4.color = "#173177";
                    msgdata.keyword5.value = info.RefundTime.ToString("yyyy-MM-dd HH:mm:ss");
                    msgdata.keyword5.color = "#173177";
                    msgdata.keyword6.value = info.OrderId;
                    msgdata.keyword6.color = "#173177";

                    //处理url
                    var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    var _iwxmember = Mall.ServiceProvider.Instance<IMemberService>.Create;
                    string page = _iwxtser.GetWXAppletMessageTemplateShowUrl(MessageTypeEnum.OrderRefundSuccess);//小程序跳转地址
                    page = page.Replace("{id}", info.OrderId.ToString());
                    if (refundid > 0)
                    {
                        page = page.Replace("{id}", refundid.ToString());
                    }
                    Log.Info("小程序退款退货url=" + page);
                    string openId = "";
                    try
                    {
                        openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "WeiXinSmallProg").OpenId;//登录小程序的OpenId
                    }
                    catch (Exception e)
                    {
                        Log.Info("小程序退款退货获取openId出错：" + e.Message);
                    }
                    string formid = "";
                    var formmodel = _iwxtser.GetWXAppletFromDataById(MessageTypeEnum.OrderRefundSuccess, info.OrderId.ToString());
                    if (formmodel != null)
                        formid = formmodel.FormId;//根据OrderId获取FormId
                    Log.Info("小程序退款退货formId=" + formid);
                    _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderRefundSuccess, userId, msgdata, page, openId, formid);
                }

            }
            #endregion
            #region 微信模版消息
            else
            {
                var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.first.value = "尊敬的" + userinfo.Nick + "，您的订单已完成退款，" + info.RefundMoney.ToString("F2") + "已经退回您的付款账户（或预存款账户），请留意查收。";
                    msgdata.first.color = "#000000";
                    msgdata.keyword1.value = info.OrderId.ToString();
                    msgdata.keyword1.color = "#000000";
                    msgdata.keyword2.value = info.RefundMoney.ToString("F2");
                    msgdata.keyword2.color = "#FF0000";
                    msgdata.keyword3.value = "退款成功";
                    msgdata.keyword3.color = "#000000";
                    //msgdata.remark.value = "请您耐心等候";
                    //msgdata.remark.color = "#000000";
                    var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    if (refundid > 0)
                    {
                        string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.OrderRefundSuccess);
                        url = url.Replace("{id}", refundid.ToString());
                    }
                    _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderRefundSuccess, userId, msgdata);
                }

            }
            #endregion
        }

        /// <summary>
        /// 退货/退款申请通知(TDO:ZYF3.2)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        /// <param name="refundMode"></param>
        /// <param name="refundid"></param>
        public void SendMessageOnRefundApply(long userId, MessageOrderInfo info, int refundMode, long refundid = 0)
        {
            MessageTypeEnum refundStatus = MessageTypeEnum.OrderRefundApply;
            if (refundMode == 3)
                refundStatus = MessageTypeEnum.OrderReturnApply;
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(refundStatus) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = string.Empty;
                    if (refundStatus == MessageTypeEnum.OrderRefundApply)
                    {
                        content = msg.Biz.SendMessageOnOrderRefundApply(destination, info);
                    }
                    else
                    {
                        content = msg.Biz.SendMessageOnOrderReturnApply(destination, info);
                    }
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                    }
                }
            }
            #region 发送模板消息
            //var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
            //if (userinfo != null)
            //{
            //    var msgdata = new WX_MsgTemplateSendDataModel();
            //    if (refundStatus == MessageTypeEnum.OrderRefundApply)
            //    {
            //        msgdata.first.value = "尊敬的" + userinfo.Nick + "，您的退款申请正在受理中，请至个人中心查看。";
            //        msgdata.first.color = "#000000";
            //        msgdata.keyword1.value = info.OrderId + "";
            //        msgdata.keyword1.color = "#FF0000";
            //        msgdata.keyword2.value = info.RefundMoney.ToString("F2");
            //        msgdata.keyword2.color = "#000000";
            //        msgdata.keyword3.value = "待审核";
            //        msgdata.keyword3.color = "#000000";
            //        msgdata.remark.value = "点击此处，查看订单退款详情";
            //        msgdata.remark.color = "#000000";
            //    }
            //    else
            //    {
            //        msgdata.first.value = "尊敬的" + userinfo.Nick + "，您的退货申请正在受理中，请至个人中心查看。";
            //        msgdata.first.color = "#000000";
            //        msgdata.keyword1.value = info.OrderId + "";
            //        msgdata.keyword1.color = "#FF0000";
            //        msgdata.keyword2.value = info.ProductName;
            //        msgdata.keyword2.color = "#000000";
            //        msgdata.keyword3.value = info.RefundQuantity.ToString();
            //        msgdata.keyword3.color = "#000000";
            //        msgdata.keyword4.value = info.RefundMoney.ToString("F2");
            //        msgdata.keyword4.color = "#000000";
            //        msgdata.remark.value = "点击此处，查看订单退款详情";
            //        msgdata.remark.color = "#000000";
            //    }
            //    var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
            //    if (refundid > 0)
            //    {
            //        string url = _iwxtser.GetMessageTemplateShowUrl(refundStatus);
            //        url = url.Replace("{id}", refundid.ToString());
            //    }
            //    _iwxtser.SendMessageByTemplate(refundStatus, userId, msgdata);
            //}
            //发送给商家
            SendMessageOnShopDelivering(info);
            #endregion
        }

        /// <summary>
        /// 退货成功通知(TDO:ZYF3.2)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        /// <param name="refundid"></param>
        public void SendMessageOnRefundDeliver(long userId, MessageOrderInfo info, long refundid = 0)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
#if DEBUG
            Core.Log.Info("[发送消息]数据：" + Newtonsoft.Json.JsonConvert.SerializeObject(info) + "[售后发货]");
#endif
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.OrderReturnSuccess) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                    {
#if DEBUG
                        Core.Log.Info("[发送消息]失败：" + msg.PluginInfo.PluginId + "未发送," + destination + "格式检测未通过[售后发货]");
#endif
                        throw new MallException(msg.Biz.ShortName + "错误：实例失败。");
                    }
                    try
                    {
                        var content = msg.Biz.SendMessageOnOrderReturn(destination, info);
#if DEBUG
                        Core.Log.Info("[发送消息]发送结束：" + destination + " : " + msg.PluginInfo.PluginId + "[售后发货]");
#endif
                        if (msg.Biz.EnableLog)
                        {
                            DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = content, TypeId = "短信" });
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Info("[发送消息]发送失败：" + msg.PluginInfo.PluginId + "[售后发货]", ex);
                    }
                }
            }

            #region 发送模板消息
            var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
            if (userinfo != null)
            {
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "尊敬的" + userinfo.Nick + "，您的订单(" + info.OrderId + ")退货已审核通过，请及时发货。";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = info.OrderId + "";
                msgdata.keyword1.color = "#FF0000";
                msgdata.keyword2.value = info.ProductName + "";
                msgdata.keyword2.color = "#000000";
                msgdata.keyword3.value = info.RefundQuantity + "";
                msgdata.keyword3.color = "#000000";
                msgdata.keyword4.value = info.RefundMoney.ToString("F2");
                msgdata.keyword4.color = "#000000";
                msgdata.remark.value = "点击此处，查看订单退货详情";
                msgdata.remark.color = "#000000";
                var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                string url = string.Empty;
                if (refundid > 0)
                {
                    url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.OrderReturnSuccess);
                    url = url.Replace("{id}", refundid.ToString());
                }
                _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderReturnSuccess, userId, msgdata, url);
            }
            #endregion
        }

        /// <summary>
        /// 退款/退货失败通知(TDO:ZYF3.2)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        /// <param name="refundid"></param>
        public void SendMessageOnOrderRefundFail(long userId, MessageOrderInfo info, int refundMode, long refundid = 0)
        {
            MessageTypeEnum refundStatus = MessageTypeEnum.OrderRefundFail;
            if (refundMode == 3)
                refundStatus = MessageTypeEnum.OrderReturnFail;
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(refundStatus) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = string.Empty;
                    if (refundStatus == MessageTypeEnum.OrderRefundFail)
                    {
                        content = msg.Biz.SendMessageOnOrderRefundFail(destination, info);
                    }
                    else
                    {
                        content = msg.Biz.SendMessageOnOrderReturnFail(destination, info);
                    }
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                    }
                }
            }
            #region 发送模板消息
            if (info.MsgOrderType == MessageOrderType.Applet)
            {
                //小程序发送
                var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).Exist();
                if (userinfo)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.keyword1.value = info.OrderId;
                    msgdata.keyword1.color = "#173177";
                    msgdata.keyword2.value = info.ProductName + "等...";
                    msgdata.keyword2.color = "#ff3300";
                    msgdata.keyword3.value = info.RefundMoney.ToString();
                    msgdata.keyword3.color = "#173177";
                    msgdata.keyword4.value = info.Remark;
                    msgdata.keyword4.color = "#173177";

                    //处理url
                    var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    var _iwxmember = Mall.ServiceProvider.Instance<IMemberService>.Create;
                    string page = _iwxtser.GetWXAppletMessageTemplateShowUrl(MessageTypeEnum.OrderRefundFail);//小程序跳转地址
                    page = page.Replace("{id}", info.OrderId.ToString());
                    if (refundid > 0)
                    {
                        page = page.Replace("{id}", refundid.ToString());
                    }
                    string openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "WeiXinSmallProg").OpenId;//登录小程序的OpenId
                    
                    string formid = "";
                    var formmodel = _iwxtser.GetWXAppletFromDataById(MessageTypeEnum.OrderPay, info.OrderId.ToString());
                    if (formmodel != null)
                        formid = formmodel.FormId;//根据OrderId获取FormId
                    _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderRefundFail, userId, msgdata, page, openId, formid);
                }
            }
            else
            {
                //暂时不支持sms、email
                //var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
                //foreach (var msg in message)
                //{
                //    if (msg.Biz.GetStatus(MessageTypeEnum.OrderRefundFail) == StatusEnum.Open)
                //    {
                //        string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.General);
                //        if (!msg.Biz.CheckDestination(destination))
                //            throw new MallException(msg.Biz.ShortName + "错误");
                //        var content = msg.Biz.SendMessageOnOrderRefund(destination, info);
                //        if (msg.Biz.EnableLog)
                //        {
                //            Context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = content, TypeId = "短信" });
                //            Context.SaveChanges();
                //        }
                //    }
                //}
                var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    if (refundStatus == MessageTypeEnum.OrderRefundFail)
                    {
                        msgdata.first.value = "尊敬的" + userinfo.Nick + "，您的订单退款申请被拒绝，请知悉。";
                        msgdata.first.color = "#000000";
                        msgdata.keyword1.value = info.OrderId.ToString();
                        msgdata.keyword1.color = "#000000";
                        msgdata.keyword2.value = info.ProductName;
                        msgdata.keyword2.color = "#000000";
                        msgdata.keyword3.value = info.TotalMoney.ToString();
                        msgdata.keyword3.color = "#FF0000";
                        msgdata.keyword4.value = info.Remark;
                        msgdata.keyword4.color = "#000000";
                    }
                    else
                    {
                        msgdata.first.value = "尊敬的" + userinfo.Nick + "，您的订单退货申请被拒绝，请知悉。";
                        msgdata.first.color = "#000000";
                        msgdata.keyword1.value = refundid + "";
                        msgdata.keyword1.color = "#000000";
                        msgdata.keyword2.value = info.OrderId + "";
                        msgdata.keyword2.color = "#000000";
                        msgdata.keyword3.value = info.ProductName;
                        msgdata.keyword3.color = "#000000";
                        msgdata.keyword4.value = info.Remark;
                        msgdata.keyword4.color = "#000000";
                        msgdata.remark.value = "如有疑问请联系卖家";
                        msgdata.remark.color = "#000000";
                    }
                    //处理url
                    var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    _iwxtser.SendMessageByTemplate(refundStatus, userId, msgdata);
                }
            }
            #endregion
        }

        /// <summary>
        /// 发货通知(TDO:ZYF3.2)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnOrderShipping(long userId, MessageOrderInfo info)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.OrderShipping) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnOrderShipping(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = content, TypeId = "短信" });
                    }
                }
            }
            #region 发送模板消息
            if (info.MsgOrderType == MessageOrderType.Applet)
            {
                Log.Info("小程序发货通知，准备开始发送消息");
                //小程序模版
                var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).Exist();
                if (userinfo)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.keyword1.value = info.ShippingCompany;//快递公司
                    msgdata.keyword1.color = "#173177";
                    msgdata.keyword2.value = info.ShippingNumber;//快递单号
                    msgdata.keyword2.color = "#173177";
                    msgdata.keyword3.value = info.OrderTime.ToString("yyyy-MM-dd HH:mm:ss");
                    msgdata.keyword3.color = "#173177";
                    msgdata.keyword4.value = info.ProductName + "等...";
                    msgdata.keyword4.color = "#173177";
                    msgdata.keyword5.value = info.ShipTo;
                    msgdata.keyword5.color = "#173177";
                    msgdata.keyword6.value = info.OrderId;
                    msgdata.keyword6.color = "#173177";

                    //处理url
                    var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    var _iwxmember = Mall.ServiceProvider.Instance<IMemberService>.Create;
                    string page = _iwxtser.GetWXAppletMessageTemplateShowUrl(MessageTypeEnum.OrderShipping);//小程序跳转地址
                    page = page.Replace("{id}", info.OrderId.ToString());
                    Log.Info("小程序发货通知url=" + page);
                    string openId = "";
                    try
                    {
                        openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "WeiXinSmallProg").OpenId;//登录小程序的OpenId
                    }
                    catch (Exception e)
                    {
                        Log.Info("小程序发货通知获取openid出错：" + e.Message);
                    }

                    string formid = "";
                    var formmodel = _iwxtser.GetWXAppletFromDataById(MessageTypeEnum.OrderPay, info.OrderId.ToString());
                    if (formmodel != null)
                        formid = formmodel.FormId;//根据OrderId获取FormId

                    Log.Info("小程序发货通知获取formId=" + formid);
                    _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderShipping, userId, msgdata, page, openId, formid);
                }

            }
            #endregion
            #region 
            else
            {
                var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.first.value = "尊敬的" + userinfo.Nick + "，宝贝已启程了，好像快点来到你身边。";
                    msgdata.first.color = "#000000";
                    msgdata.keyword1.value = info.OrderId.ToString();
                    msgdata.keyword1.color = "#000000";
                    msgdata.keyword2.value = info.ShippingCompany.ToString();
                    msgdata.keyword2.color = "#000000";
                    msgdata.keyword3.value = info.ShippingNumber.ToString();
                    msgdata.keyword3.color = "#FF0000";
                    msgdata.remark.value = "点击查看完整的物流信息";
                    msgdata.remark.color = "#000000";
                    var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.OrderShipping);
                    url = url.Replace("{id}", info.OrderId.ToString());
                    _iwxtser.SendMessageByTemplate(MessageTypeEnum.OrderShipping, userId, msgdata, url);
                }
            }
            #endregion
        }

        public void SendMessageOnShopAudited(long userId, MessageShopInfo info)
        {
            //TDO:ZYF 3.2注释
            //var messages = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            //foreach (var msg in messages)
            //{
            //    if (msg.Biz.GetStatus(MessageTypeEnum.ShopAudited) == StatusEnum.Open)
            //    {
            //        string destination = GetDestination(userId, msg.PluginInfo.PluginId);
            //        if (!msg.Biz.CheckDestination(destination))
            //            throw new MallException(msg.Biz.ShortName + "错误");
            //        var content = msg.Biz.SendMessageOnShopAudited(destination, info);
            //        if (msg.Biz.EnableLog)
            //        {
            //            DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
            //        }
            //    }
            //}
        }

        /// <summary>
        /// 发送优惠券成功时发送消息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnCouponSuccess(long userId, MessageCouponInfo info)
        {
            //TDO:ZYF 3.2注释
            //var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            //foreach (var msg in message)
            //{
            //    if (msg.Biz.GetStatus(MessageTypeEnum.SendCouponSuccess) == StatusEnum.Open)
            //    {
            //        string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
            //        if (!msg.Biz.CheckDestination(destination))
            //            throw new MallException(msg.Biz.ShortName + "错误");
            //        var content = msg.Biz.SendMessageOnCouponSuccess(destination, info);
            //        if (msg.Biz.EnableLog)
            //        {
            //            DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
            //        }
            //    }
            //}
        }


        //2.4去除
        //public void SendMessageOnShopSuccess(long userId, MessageShopInfo info)
        //{
        //    var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
        //    foreach (var msg in message)
        //    {
        //        if (msg.Biz.GetStatus(MessageTypeEnum.ShopSuccess) == StatusEnum.Open)
        //        {
        //            string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.ShopManager);
        //            if (!msg.Biz.CheckDestination(destination))
        //                throw new MallException(msg.Biz.ShortName + "错误");
        //            var content = msg.Biz.SendMessageOnShopSuccess(destination, info);
        //            if (msg.Biz.EnableLog)
        //            {
        //                context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
        //                context.SaveChanges();
        //            }
        //        }
        //    }
        //}

        //原结算发送消息 150624
        //public void SendMessageOnShopSettlement(long userId, MessageSettlementInfo info)
        //{
        //    var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
        //    foreach (var msg in message)
        //    {
        //        if (msg.Biz.GetStatus(MessageTypeEnum.ShopSettlement) == StatusEnum.Open)
        //        {
        //            string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactsInfo.UserTypes.ShopManager);
        //            if (!msg.Biz.CheckDestination(destination))
        //                throw new MallException(msg.Biz.ShortName + "错误");
        //            var content = msg.Biz.SendMessageOnShopSettlement(destination, info);
        //            if (msg.Biz.EnableLog)
        //            {
        //                context.MessageLog.Add(new MessageLog() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
        //                context.SaveChanges();
        //            }
        //        }
        //    }
        //}


        public void AddSendMessageRecord(dynamic model)
        {
            throw new NotImplementedException();
        }

        public QueryPageModel<object> GetSendMessageRecords(object querymodel)
        {
            throw new NotImplementedException();
        }

        #region 分销
        /// <summary>
        /// 分销：申请成功销售员
        /// </summary>
        /// <param name="userName"></param>
        public void SendMessageOnDistributorApply(long userId, string userName, string siteName)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.DistributorApply) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnDistributorApply(destination, userName, siteName);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                    }
                }
            }
            #region 发送模板消息
            //try
            //{
            //    var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
            //    var msgdata = new WX_MsgTemplateSendDataModel();
            //    msgdata.first.value = "尊敬的会员，您的销售员申请正在受理中。";
            //    msgdata.first.color = "#000000";
            //    msgdata.keyword1.value = "申请成为销售员";
            //    msgdata.keyword1.color = "#000000";
            //    msgdata.keyword2.value = "待审核";
            //    msgdata.keyword2.color = "#000000";
            //    msgdata.remark.value = "请耐心等待平台审核。";
            //    msgdata.remark.color = "#000000";
            //    string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.DistributorApply);
            //    _iwxtser.SendMessageByTemplate(MessageTypeEnum.DistributorApply, userId, msgdata, url);
            //}
            //catch (Exception e)
            //{
            //    Log.Debug("发送分销：申请成功销售员消息的错误：" + e.Message);
            //}
            #endregion
        }
        /// <summary>
        /// 分销：申请审核通过
        /// </summary>
        /// <param name="userName"></param>
        public void SendMessageOnDistributorAuditSuccess(long userId, string userName, string siteName)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.DistributorAuditSuccess) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnDistributorAuditSuccess(destination, userName, siteName);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                    }
                }
            }
            #region 发送模板消息
            try
            {
                var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "尊敬的会员，恭喜您申请销售员审核通过！";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = "申请成为销售员";
                msgdata.keyword1.color = "#000000";
                msgdata.keyword2.value = "审核通过";
                msgdata.keyword2.color = "#000000";
                msgdata.remark.value = "赶快分享商品给小伙伴吧。";
                msgdata.remark.color = "#000000";
                string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.DistributorAuditSuccess);
                _iwxtser.SendMessageByTemplate(MessageTypeEnum.DistributorAuditSuccess, userId, msgdata, url);
            }
            catch (Exception e)
            {
                Log.Debug("发送分销：申请审核通过消息的错误：" + e.Message);
            }
            #endregion
        }
        /// <summary>
        /// 分销：申请审核拒绝
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="remark"></param>
        /// <param name="applyTime"></param>
        public void SendMessageOnDistributorAuditFail(long userId, string userName, string remark, DateTime applyTime, string siteName)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.DistributorAuditFail) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnDistributorAuditFail(destination, userName, siteName);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                    }
                }
            }
            #region 发送模板消息
            try
            {
                var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "尊敬的会员，您的销售员申请被拒绝了。";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = remark;
                msgdata.keyword1.color = "#000000";
                msgdata.keyword2.value = applyTime.ToString("yyyy-MM-dd HH:mm:ss");
                msgdata.keyword2.color = "#000000";
                msgdata.remark.value = "请修改后重新提交申请。";
                msgdata.remark.color = "#000000";
                string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.DistributorAuditFail);
                _iwxtser.SendMessageByTemplate(MessageTypeEnum.DistributorAuditFail, userId, msgdata, url);
            }
            catch (Exception e)
            {
                Log.Debug("发送分销：申请审核拒绝消息的错误：" + e.Message);
            }
            #endregion
        }
        /// <summary>
        /// 分销：会员发展成功
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="subUserName"></param>
        /// <param name="subUserRegTime"></param>
        public void SendMessageOnDistributorNewJoin(long userId, string subUserName, DateTime subUserRegTime, string siteName)
        {
            #region 发送模板消息
            try
            {
                var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "尊敬的会员，恭喜您成功邀请了新会员加入。";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = subUserName;
                msgdata.keyword1.color = "#000000";
                msgdata.keyword2.value = subUserRegTime.ToString("yyyy-MM-dd HH:mm:ss");
                msgdata.keyword2.color = "#000000";
                //msgdata.remark.value = "";
                //msgdata.remark.color = "";
                string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.DistributorNewJoin);
                _iwxtser.SendMessageByTemplate(MessageTypeEnum.DistributorNewJoin, userId, msgdata, url);
            }
            catch (Exception e)
            {
                Log.Debug("发送分销：会员发展成功消息的错误：" + e.Message);
            }
            #endregion
        }
        /// <summary>
        /// 分销：有已结算佣金时
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnDistributorCommissionSettled(long userId, decimal amount, DateTime settlementDate, string siteName)
        {
            var uobj = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
            string userName = uobj != null ? uobj.UserName : "";
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.DistributorCommissionSettled) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnDistributorCommissionSettled(destination, userName, siteName);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                    }
                }
            }
            #region 发送模板消息
            try
            {
                var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "尊敬的会员，您有一笔新的佣金已经结算了。";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = amount.ToString("F2");
                msgdata.keyword1.color = "#FF0000";
                msgdata.keyword2.value = settlementDate.ToString("yyyy-MM-dd HH:mm:ss");
                msgdata.keyword2.color = "#000000";
                msgdata.remark.value = "分享越多，收获越多。";
                msgdata.remark.color = "#000000";
                string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.DistributorCommissionSettled);
                _iwxtser.SendMessageByTemplate(MessageTypeEnum.DistributorCommissionSettled, userId, msgdata, url);
            }
            catch (Exception e)
            {
                Log.Debug("发送分销：有已结算佣金时消息的错误：" + e.Message);
            }
            #endregion
        }
        #endregion

        /// <summary>
        /// 自提订单支付成功通知(TDO:ZYF3.2)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnSelfTakeOrderPay(long userId, MessageOrderInfo info)
        {
            var refundStatus = MessageTypeEnum.SelfTakeOrderPay;
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(refundStatus) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnSelfTakeOrderPay(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                    }
                }
            }
            #region 发送模板消息
            if (info.MsgOrderType == MessageOrderType.Applet)
            {
                Log.Info("小程序支付成功，准备开始发送消息");
                var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                var _iwxmember = Mall.ServiceProvider.Instance<IMemberService>.Create;
                string formid = "";
                var formmodel = _iwxtser.GetWXAppletFromDataById(MessageTypeEnum.OrderPay, info.OrderId.ToString());
                if (formmodel != null)
                {
                    formid = formmodel.FormId;//根据OrderId获取FormId
                }
                else
                {
                    Log.Info("小程序订单：" + info.OrderId + " FormId为空");
                }
                Log.Info("小程序支付，formid=" + formid);

                var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
                if (userinfo != null)
                {
                    Log.Info("小程序支付，userId=" + userId);
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.keyword1.value = info.TotalMoney.ToString("f2") + "元";
                    msgdata.keyword1.color = "#173177";
                    msgdata.keyword2.value = info.PayTime.ToString("yyyy-MM-dd HH:mm:ss");
                    msgdata.keyword2.color = "#ff3300";
                    msgdata.keyword3.value = info.ProductName.ToString() + "等...";
                    msgdata.keyword3.color = "#173177";
                    msgdata.keyword4.value = info.OrderId.ToString();
                    msgdata.keyword4.color = "#173177";
                    msgdata.keyword5.value = info.OrderTime.ToString("yyyy-MM-dd HH:mm:ss");
                    msgdata.keyword5.color = "#173177";
                    //处理url
                    string page = _iwxtser.GetWXAppletMessageTemplateShowUrl(MessageTypeEnum.OrderPay);//小程序跳转地址
                    page = page.Replace("{id}", info.OrderId.ToString());
                    Log.Info("小程序支付跳转url=" + page);
                    string openId = "";
                    try
                    {
                        openId = _iwxmember.GetMemberOpenIdInfoByuserIdAndType(userId, "WeiXinSmallProg").OpenId;//登录小程序的OpenId
                    }
                    catch (Exception e)
                    {
                        Log.Error("小程序支付，获取openid出错：" + e.Message);
                    }
                    Log.Info("小程序openid=" + openId);
                    _iwxtser.SendAppletMessageByTemplate(MessageTypeEnum.OrderPay, userId, msgdata, page, openId, formid);
                }
            }
            else
            {
                var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
                if (userinfo != null)
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.first.value = "尊敬的" + userinfo.Nick + "，您的自提订单已支付成功。";
                    msgdata.first.color = "#000000";
                    msgdata.keyword1.value = info.OrderId + "";
                    msgdata.keyword1.color = "#FF0000";
                    msgdata.keyword2.value = info.PickupCode;
                    msgdata.keyword2.color = "#000000";
                    msgdata.keyword3.value = info.TotalMoney.ToString("F2");
                    msgdata.keyword3.color = "#000000";
                    msgdata.keyword4.value = info.ShopBranchAddress;
                    msgdata.keyword4.color = "#000000";
                    msgdata.remark.value = "请您尽快到达门店提货，期待您的光临！";
                    msgdata.remark.color = "#000000";
                    var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    _iwxtser.SendMessageByTemplate(refundStatus, userId, msgdata);
                }
            }
            #endregion
        }

        /// <summary>
        /// 自提订单核销成功通知(TDO:ZYF3.2)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnAlreadyVerification(long userId, MessageOrderInfo info)
        {
            var refundStatus = MessageTypeEnum.AlreadyVerification;
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(refundStatus) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnAlreadyVerification(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                    }
                }
            }
            var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
            var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
            if (userinfo != null)
            {
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "尊敬的（" + userinfo.Nick + "），您的订单已成功核销。";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = info.ProductName + " ...";
                msgdata.keyword1.color = "#FF0000";
                msgdata.keyword2.value = info.PickupCode;
                msgdata.keyword2.color = "#000000";
                msgdata.keyword3.value = info.FinishDate.ToString("yyyy-MM-dd HH:mm:ss");
                msgdata.keyword3.color = "#000000";
                msgdata.keyword4.value = info.ShopName;
                msgdata.keyword4.color = "#000000";
                msgdata.remark.value = "感谢您的使用，点击查看详情。";
                msgdata.remark.color = "#000000";
                string url = _iwxtser.GetMessageTemplateShowUrl(refundStatus);
                url = url.Replace("{id}", info.OrderId.ToString());
                _iwxtser.SendMessageByTemplate(refundStatus, userId, msgdata, url);
            }
        }


        /// <summary>
        /// 会员提现申请(TDO:ZYF3.2)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnMemberWithDrawApply(long userId, MessageWithDrawInfo info)
        {
            var refundStatus = MessageTypeEnum.MemberWithDrawApply;
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(refundStatus) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnMemberWithDrawApply(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                    }
                }
            }
            #region 发送模板消息
            var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
            if (userinfo != null)
            {
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "尊敬的" + userinfo.Nick + "，您的提现申请正在受理中。";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = info.Amount.ToString("F2");
                msgdata.keyword1.color = "#FF0000";
                msgdata.keyword2.value = info.ApplyTime.ToString("yyyy-MM-dd HH:mm:ss");
                msgdata.keyword2.color = "#000000";
                msgdata.keyword3.value = ((UserWithdrawType)info.ApplyType).ToDescription();
                msgdata.keyword3.color = "#000000";
                msgdata.remark.value = "我们将尽快处理完毕，感谢您的支持！";
                msgdata.remark.color = "#000000";
                var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                _iwxtser.SendMessageByTemplate(refundStatus, userId, msgdata);
            }
            #endregion
        }

        /// <summary>
        /// 会员提现成功(TDO:ZYF3.2)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnMemberWithDrawSuccess(long userId, MessageWithDrawInfo info)
        {
            var refundStatus = MessageTypeEnum.MemberWithDrawSuccess;
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(refundStatus) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnMemberWithDrawSuccess(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                    }
                }
            }
            #region 发送模板消息
            var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
            if (userinfo != null)
            {
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "尊敬的" + userinfo.Nick + "，您申请的提现已经到帐了，请注意查收。";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = info.ApplyTime.ToString("yyyy-MM-dd HH:mm:ss");
                msgdata.keyword1.color = "#000000";
                msgdata.keyword2.value = ((UserWithdrawType)info.ApplyType).ToDescription();
                msgdata.keyword2.color = "#000000";
                msgdata.keyword3.value = info.Amount.ToString("F2");
                msgdata.keyword3.color = "#FF0000";
                var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                _iwxtser.SendMessageByTemplate(refundStatus, userId, msgdata);
            }
            #endregion
        }

        /// <summary>
        /// 会员提现失败(TDO:ZYF3.2)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnMemberWithDrawFail(long userId, MessageWithDrawInfo info)
        {
            var refundStatus = MessageTypeEnum.MemberWithDrawFail;
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(refundStatus) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnMemberWithDrawFail(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                    }
                }
            }
            #region 发送模板消息
            var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
            if (userinfo != null)
            {
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "尊敬的" + userinfo.Nick + "，您的提现申请被拒绝了。";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = info.Amount.ToString("F2");
                msgdata.keyword1.color = "#FF0000";
                msgdata.keyword2.value = info.ApplyTime.ToString("yyyy-MM-dd HH:mm:ss");
                msgdata.keyword2.color = "#000000";
                msgdata.keyword3.value = info.Remark;
                msgdata.keyword3.color = "#000000";
                var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                _iwxtser.SendMessageByTemplate(refundStatus, userId, msgdata);
            }
            #endregion
        }

        #region 分销提现
        /// <summary>
        /// 分销会员提现申请
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnDistributionMemberWithDrawApply(long userId, MessageWithDrawInfo info)
        {
            var refundStatus = MessageTypeEnum.MemberWithDrawApply;
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(refundStatus) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnMemberWithDrawApply(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                    }
                }
            }
            #region 发送模板消息
            var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
            if (userinfo != null)
            {
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "尊敬的" + userinfo.Nick + "，您的提现申请正在受理中。";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = info.Amount.ToString("F2");
                msgdata.keyword1.color = "#FF0000";
                msgdata.keyword2.value = info.ApplyTime.ToString("yyyy-MM-dd HH:mm:ss");
                msgdata.keyword2.color = "#000000";
                msgdata.keyword3.value = ((UserWithdrawType)info.ApplyType).ToDescription();
                msgdata.keyword3.color = "#000000";
                msgdata.remark.value = "我们将尽快处理完毕，感谢您的支持！";
                msgdata.remark.color = "#000000";
                var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                _iwxtser.SendMessageByTemplate(refundStatus, userId, msgdata);
            }
            #endregion
        }

        /// <summary>
        /// 分销会员提现成功
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnDistributionMemberWithDrawSuccess(long userId, MessageWithDrawInfo info)
        {
            var refundStatus = MessageTypeEnum.MemberWithDrawSuccess;
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(refundStatus) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                    {
                        continue;
                        //throw new MallException(msg.Biz.ShortName + "错误");
                    }                        
                    var content = msg.Biz.SendMessageOnMemberWithDrawSuccess(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                    }
                }
            }
            #region 发送模板消息
            var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
            if (userinfo != null)
            {
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "尊敬的" + userinfo.Nick + "，您申请的提现已经到帐了，请注意查收。";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = info.ApplyTime.ToString("yyyy-MM-dd HH:mm:ss");
                msgdata.keyword1.color = "#000000";
                msgdata.keyword2.value = ((DistributionWithdrawType)info.ApplyType).ToDescription();
                msgdata.keyword2.color = "#000000";
                msgdata.keyword3.value = info.Amount.ToString("F2");
                msgdata.keyword3.color = "#FF0000";
                var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                _iwxtser.SendMessageByTemplate(refundStatus, userId, msgdata);
            }
            #endregion
        }
        #endregion

        /// <summary>
        /// 商家售后处理提醒(TDO:ZYF3.2)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnShopDelivering(MessageOrderInfo info)
        {
            //发送商家
            var shopsend = DbFactory.Default.Get<ShopShipperInfo>().Where(d => d.ShopId == info.ShopId && d.IsDefaultSendGoods == true).FirstOrDefault();
            if (shopsend != null && !string.IsNullOrWhiteSpace(shopsend.WxOpenId))
            {
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "尊敬的" + info.ShopName + "，您有一个售后申请待处理。";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = info.OrderId.ToString();
                msgdata.keyword1.color = "#FF0000";
                msgdata.keyword2.value = info.ProductName + "...";
                msgdata.keyword2.color = "#FF0000";
                msgdata.remark.value = "亲，请尽快处理，感谢您的使用";
                msgdata.remark.color = "#000000";
                string url = "";
                var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                _iwxtser.SendMessageByTemplate(MessageTypeEnum.ShopDelivering, 0, msgdata, url, shopsend.WxOpenId);
            }
        }

        /// <summary>
        /// 商家发货提醒
        /// </summary>
        /// <param name="info"></param>
        public void SendMessageOnShopOrderShipping(MessageOrderInfo info)
        {
            var orderIds = info.OrderId.Split(',');
            var i = 0;
            //发送商家
            foreach (var shopId in info.ShopIds)
            {                
                var shop = DbFactory.Default.Get<ShopInfo>().Where(s => s.Id == shopId).FirstOrDefault();
                var shopsend = DbFactory.Default.Get<ShopShipperInfo>().Where(d => d.ShopId == shopId && d.IsDefaultSendGoods == true).FirstOrDefault();
                if (shop != null && shopsend != null && !string.IsNullOrWhiteSpace(shopsend.WxOpenId))
                {
                    var msgdata = new WX_MsgTemplateSendDataModel();
                    msgdata.first.value = "尊敬的" + shop.ShopName + "，您有一个新的待发货订单。";
                    msgdata.first.color = "#000000";
                    msgdata.keyword1.value = orderIds[i];
                    msgdata.keyword1.color = "#000000";
                    msgdata.keyword2.value = info.TotalMoney.ToString();
                    msgdata.keyword2.color = "#FF0000";
                    msgdata.keyword3.value = info.UserName;
                    msgdata.keyword3.color = "#FF0000";
                    msgdata.keyword4.value = "待发货";
                    msgdata.keyword4.color = "#000000";
                    msgdata.remark.value = "买家已付款，请尽快发货";
                    msgdata.remark.color = "#000000";
                    string url = "";
                    var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                    _iwxtser.SendMessageByTemplate(MessageTypeEnum.ShopOrderShipping, 0, msgdata, url, shopsend.WxOpenId);
                }
                i++;
            }
        }

        /// <summary>
        /// 修改支付密码提醒(TDO:ZYF3.2)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnEditPayPassWord(long userId, MessageUserInfo info)
        {
            var status = MessageTypeEnum.EditPayPassWord;
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(status) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnEditPayPassWord(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                    }
                }
            }

            #region 发送模板消息
            var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
            if (userinfo != null)
            {
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "尊敬的" + userinfo.Nick + "，您的帐号交易密码修改成功。";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = info.UserName;
                msgdata.keyword1.color = "#FF0000";
                msgdata.keyword2.value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                msgdata.keyword2.color = "#000000";
                msgdata.remark.value = "如非本人操作，请及时联系客服。";
                msgdata.remark.color = "#000000";
                var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                string url = _iwxtser.GetMessageTemplateShowUrl(status);
                _iwxtser.SendMessageByTemplate(status, userId, msgdata, url);
            }
            #endregion
        }

        /// <summary>
        /// 修改登录密码提醒(TDO:ZYF3.2)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnEditLoginPassWord(long userId, MessageUserInfo info)
        {
            var status = MessageTypeEnum.EditLoginPassWord;
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(status) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnEditLoginPassWord(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = 0, MessageContent = content, TypeId = "短信" });
                    }
                }
            }

            #region 发送模板消息
            var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
            if (userinfo != null)
            {
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "尊敬的" + userinfo.Nick + "，您的帐号登录密码修改成功。";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = info.UserName;
                msgdata.keyword1.color = "#FF0000";
                msgdata.keyword2.value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                msgdata.keyword2.color = "#000000";
                msgdata.remark.value = "如非本人操作，请及时联系客服。";
                msgdata.remark.color = "#000000";
                var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
                string url = _iwxtser.GetMessageTemplateShowUrl(status);
                _iwxtser.SendMessageByTemplate(status, userId, msgdata, url);
            }
            #endregion
        }

        /// <summary>
        /// 虚拟商品购买成功
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnVirtualOrderPay(long userId, MessageVirtualOrderInfo info)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.VirtualOrderPay) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnVirtualOrderPay(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = content, TypeId = "短信" });
                    }
                }
            }
            #region 发送模板消息
            var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
            var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
            if (userinfo != null)
            {
                var msgdata = new WX_MsgTemplateSendDataModel();
                string msg = "恭喜你！购买成功啦！";
                if (info.EffectiveType == 1)
                {
                    msg += "可以立即消费使用！";
                }
                else if (info.EffectiveType == 2)
                {
                    msg += string.Format("{0}小时后可以使用！", info.Hour);
                }
                else if (info.EffectiveType == 3)
                {
                    msg += "次日后可以使用！";
                }
                msgdata.first.value = msg;
                msgdata.first.color = "#000000";

                msgdata.keyword1.value = info.ShopName;
                msgdata.keyword1.color = "#000000";
                msgdata.keyword2.value = info.Phone;
                msgdata.keyword2.color = "#000000";
                msgdata.keyword3.value = info.Address;
                msgdata.keyword3.color = "#000000";
                msgdata.keyword4.value = info.DueTime;
                msgdata.keyword4.color = "#000000";
                msgdata.keyword5.value = info.VerificationCodes;
                msgdata.keyword5.color = "#000000";
                msgdata.remark.value = string.Format("留意到期时间哦！快快去消费吧！点击查看详情");
                msgdata.remark.color = "#000000";

                string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.VirtualOrderPay);
                url = url.Replace("{id}", info.OrderId.ToString());
                _iwxtser.SendMessageByTemplate(MessageTypeEnum.VirtualOrderPay, userId, msgdata, url);
            }
            #endregion
        }

        /// <summary>
        /// 虚拟订单核销成功
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        public void SendMessageOnVirtualOrderVerificationSuccess(long userId, MessageVirtualOrderVerificationInfo info)
        {
            var message = PluginsManagement.GetPlugins<IMessagePlugin>().ToList();
            foreach (var msg in message)
            {
                if (msg.Biz.GetStatus(MessageTypeEnum.VirtualAlreadyVerification) == StatusEnum.Open)
                {
                    string destination = GetDestination(userId, msg.PluginInfo.PluginId, MemberContactInfo.UserTypes.General);
                    if (!msg.Biz.CheckDestination(destination))
                        throw new MallException(msg.Biz.ShortName + "错误");
                    var content = msg.Biz.SendMessageOnVirtualOrderVerificationSuccess(destination, info);
                    if (msg.Biz.EnableLog)
                    {
                        DbFactory.Default.Add(new MessageLogInfo() { SendTime = DateTime.Now, ShopId = info.ShopId, MessageContent = content, TypeId = "短信" });
                    }
                }
            }

            #region 发送模板消息
            var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
            var userinfo = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == userId).FirstOrDefault();
            if (userinfo != null)
            {
                var msgdata = new WX_MsgTemplateSendDataModel();
                msgdata.first.value = "您的订单已成功核销。";
                msgdata.first.color = "#000000";
                msgdata.keyword1.value = info.ProductName;
                msgdata.keyword1.color = "#000000";
                msgdata.keyword2.value = info.VerificationCodes;
                msgdata.keyword2.color = "#000000";
                msgdata.keyword3.value = info.VerificationTime;
                msgdata.keyword3.color = "#000000";
                msgdata.keyword4.value = info.ShopBranchName;
                msgdata.keyword4.color = "#000000";
                msgdata.remark.value = "感谢您的使用，点击查看详情。";
                msgdata.remark.color = "#000000";
                string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.VirtualAlreadyVerification);
                url = url.Replace("{id}", info.OrderId.ToString());
                _iwxtser.SendMessageByTemplate(MessageTypeEnum.VirtualAlreadyVerification, userId, msgdata, url);
            }
            #endregion
        }
    }
}

