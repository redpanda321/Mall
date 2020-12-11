using Mall.Core;
using Mall.Core.Plugins.Message;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Web.Framework;
using System;
using System.IO;
using System.Linq;

using Mall.Application;
using Mall.CommonModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class MessageGroupController : BaseAdminController
    {
        IMemberLabelService _iMemberLabelService;
        IMemberService _iMemberService;
        IRegionService _iRegionService;
        IWXMsgTemplateService _iWXMsgTemplateService;

        public MessageGroupController(IMemberLabelService iMemberLabelService, IMemberService iMemberService, IRegionService iRegionService, IWXMsgTemplateService iWXMsgTemplateService)
        {
            _iMemberLabelService = iMemberLabelService;
            _iMemberService = iMemberService;
            _iRegionService = iRegionService;
            _iWXMsgTemplateService = iWXMsgTemplateService;
        }
        // GET: Admin/Message
        public ActionResult Management()
        {
            return View();
        }

        public JsonResult GetSendRecords(int page, int rows, SendRecordQuery query)
        {
            query.PageNo = page;
            query.PageSize = rows;
            var pageModel = WXMsgTemplateApplication.GetSendRecords(query);
            var model = pageModel.Models.ToList().Select(e => new
            {
                Id = e.Id,
                MessageType = e.MessageType.GetHashCode(),
                MsgType = e.MessageType.ToDescription(),
                SendTime = e.SendTime.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                SendToUser = e.ToUserLabel,
                CurrentCouponCount = e.CurrentCouponCount,
                CurrentUseCouponCount = e.CurrentUseCouponCount,
                SendState = e.SendState == 1 ? "发送成功" : "发送失败"
            });
            return Json(new { rows = model, total = pageModel.Total });
        }

        
        /// <summary>
        /// 获取消息记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetSendRecordDetail(long id)
        {
            var messageinfo = WXMsgTemplateApplication.GetSendMessageRecordById(id);
            if(messageinfo==null || messageinfo.Id <= 0)
                return Json(new { success = false, msg = "消息记录不存在" });
            
            return Json(new { success = true, model = messageinfo });
        }

        public ActionResult WXGroupMessage()
        {
            var pageModel = _iMemberLabelService.GetMemberLabelList(new LabelQuery() { });
            ViewBag.LabelInfos = pageModel.Models.ToList();
            //var topregion = _iMemberService.GetAllTopRegion().Select(e => new SelectListItem
            //{
            //    Text = _iRegionService.GetRegion(e).Name,
            //    Value = e.ToString()
            //}).Where(e => !string.IsNullOrWhiteSpace(e.Text)).ToList();
            //topregion.Insert(0, new SelectListItem { Value = "-1", Text = "全部" });

            var topregion = MemberApplication.GetAllTopRegion().Select(e =>
            {
                var region = RegionApplication.GetRegion(e);
                string shortName = "";
                if (region != null && region.Level == Region.RegionLevel.Province)
                    shortName = region.ShortName;
                return new SelectListItem
                {
                    Text = shortName,
                    Value = e.ToString()
                };
            }).Where(e => !string.IsNullOrWhiteSpace(e.Text)).ToList();
            topregion.Insert(0, new SelectListItem { Value = "-1", Text = "全部" });

            ViewBag.Regions = topregion;
            return View();
        }

        /// <summary>
        /// 群发微信消息
        /// </summary>
        /// <param name="msgtype"></param>
        /// <param name="userdesc"></param>
        /// <param name="mediaid"></param>
        /// <param name="labelids"></param>
        /// <param name="sex"></param>
        /// <param name="msgcontent"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        public JsonResult SendWXGroupMessage(string msgtype, string userdesc, string mediaid = "", string labelids = "", string sex = "", string msgcontent = "", string region = "")
        {
            WXMsgType type;
            if (Enum.TryParse<WXMsgType>(msgtype, out type))
            {
                Entities.SendToUserLabel query = new Entities.SendToUserLabel();
                string toUserDesc = string.Empty;
                if (!string.IsNullOrWhiteSpace(labelids))
                {
                    query.LabelIds = labelids.Split(',').Select(e => long.Parse(e)).ToArray();
                }
                if (!string.IsNullOrWhiteSpace(sex))
                {
                    query.Sex = (Mall.CommonModel.SexType)Convert.ToInt32(sex);
                }
                if (!string.IsNullOrWhiteSpace(region))
                {
                    query.ProvinceId = long.Parse(region);
                }
                Entities.SendMsgInfo model = new Entities.SendMsgInfo()
                {
                    AppId = this.SiteSettings.WeixinAppId,
                    AppSecret = this.SiteSettings.WeixinAppSecret,
                    Content = msgcontent,
                    MediaId = mediaid,
                    MsgType = type,
                    ToUserLabel = query,
                    ToUserDesc = userdesc
                };
                var result = _iWXMsgTemplateService.SendWXMsg(model);
                if (result.errCode == "0")
                {
                    return Json(new { success = true });
                }
                else
                {
                    if (result.errMsg.Contains("success"))
                    {
                        return Json(new { success = true });
                    }
                    else
                    {
                        Log.Error("SendWXGroupMessage: " + result.errCode);
                        return Json(new { success = false, msg = result.errCode });
                    }
                }
            }
            return Json(new { success = false, msg = "微信消息类型异常" });
        }
        public JsonResult GetWXMaterialList(int pageIdx, int pageSize)
        {
            var siteSetting = SiteSettingApplication.SiteSettings;
            if (string.IsNullOrWhiteSpace(siteSetting.WeixinAppId))
            {
                throw new MallException("未配置微信公众号");
            }
            var offset = (pageIdx - 1) * pageSize;
            var list = _iWXMsgTemplateService.GetMediaMsgTemplateList(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret, offset, pageSize);
            return Json(list);
        }
        public ActionResult GetMedia(string mediaid)
        {
            var siteSetting = SiteSettingApplication.SiteSettings;
            if (string.IsNullOrWhiteSpace(siteSetting.WeixinAppId))
            {
                throw new MallException("未配置微信公众号");
            }
            MemoryStream stream = new MemoryStream();
            _iWXMsgTemplateService.GetMedia(mediaid, siteSetting.WeixinAppId, siteSetting.WeixinAppSecret, stream);
            return File(stream.ToArray(), "Image/png");
        }
        public ActionResult EMailGroupMessage()
        {
            var pageModel = _iMemberLabelService.GetMemberLabelList(new LabelQuery() { });
            ViewBag.LabelInfos = pageModel.Models.ToList();
            return View();
        }
        public ActionResult PhoneGroupMessage()
        {
            var pageModel = _iMemberLabelService.GetMemberLabelList(new LabelQuery() { });
            var sms = PluginsManagement.GetPlugins<ISMSPlugin>().FirstOrDefault();
            if (sms != null && sms.PluginInfo.PluginId == "Mall.Plugin.Message.SMS")
            {
                ViewBag.LoginLink = sms.Biz.GetLoginLink();
            }
            ViewBag.LabelInfos = pageModel.Models.ToList();
            return View();
        }

        [HttpPost]
        [UnAuthorize]
       
        public JsonResult SendEmailMsg(string labelids, string title, string content, string labelinfos)
        {
            var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>("Mall.Plugin.Message.Email");
            var lids = string.IsNullOrWhiteSpace(labelids) ? null : labelids.Split(',').Select(s => long.Parse(s)).ToArray();
            int pageNo = 1, pageSize = 100;
            var pageMode = _iMemberService.GetMembers(new MemberQuery
            {
                IsHaveEmail = true,
                Labels = lids,
                PageNo = pageNo,
                PageSize = pageSize
            });
            if (pageMode.Total == 0)
            {
                return Json(new Result { success = false, msg = "未找到可发送邮件的会员信息！" });
            }
            while (pageMode.Models.Count() > 0)
            {//暂时循环处理
                string[] dests = pageMode.Models.Select(e => e.Email).ToArray();
                foreach (var dest in dests)
                {
                    if (!messagePlugin.Biz.CheckDestination(dest))
                    {
                        return Json(new Result { success = false, msg = "非法Email地址：" + dest });
                    }
                }
                var siteName = SiteSettingApplication.SiteSettings.SiteName;
                messagePlugin.Biz.SendMessages(dests, content, title);
                pageNo += 1;
                pageMode = _iMemberService.GetMembers(new MemberQuery
                {
                    IsHaveEmail = true,
                    Labels = lids,
                    PageNo = pageNo,
                    PageSize = pageSize
                });
            }
            if (!string.IsNullOrEmpty(title))
            {
                content = "####Title■■" + title.Trim() + "####" + content;//把邮件标题保存在内容里面，用5个特殊符号隔开
            }
            var sendRecord = new Entities.SendMessageRecordInfo
            {
                ContentType = WXMsgType.text,
                MessageType = MsgType.Email,
                SendContent = content == null ? "" : content,
                SendState = 1,
                SendTime = DateTime.Now,
                ToUserLabel = labelinfos == null ? "" : labelinfos
            };
            _iWXMsgTemplateService.AddSendRecord(sendRecord);
            return Json(new { success = true });
        }

        [HttpPost]
        [UnAuthorize]
 
        public JsonResult SendPhoneMsg(string labelids, string content, string labelinfos)
        {
            var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>("Mall.Plugin.Message.SMS");
            var lids = string.IsNullOrWhiteSpace(labelids) ? null : labelids.Split(',').Select(s => long.Parse(s)).ToArray();
            int pageNo = 1, pageSize = 100;
            var pageMode = _iMemberService.GetMembers(new MemberQuery
            {
                IsHavePhone = true,
                Labels = lids,
                PageNo = pageNo,
                PageSize = pageSize
            });
            if (pageMode.Total == 0)
            {
                return Json(new Result { success = false, msg = "未找到可发送短信的会员信息！" });
            }
            var siteName = SiteSettingApplication.SiteSettings.SiteName;
            content = content + "【" + siteName + "】";
            while (pageMode.Models.Count() > 0)
            {//暂时循环处理
                string[] dests = pageMode.Models.Select(e => e.CellPhone).ToArray();
                foreach (var phone in dests)
                {
                    if (!string.IsNullOrWhiteSpace(phone))
                        messagePlugin.Biz.SendTestMessage(phone, content);
                }
                pageNo += 1;
                pageMode = _iMemberService.GetMembers(new MemberQuery
                {
                    IsHavePhone = true,
                    Labels = lids,
                    PageNo = pageNo,
                    PageSize = pageSize
                });
            }
            var sendRecord = new Entities.SendMessageRecordInfo
            {
                ContentType = WXMsgType.text,
                MessageType = MsgType.SMS,
                SendContent = content == null ? "" : content,
                SendState = 1,
                SendTime = DateTime.Now,
                ToUserLabel = labelinfos == null ? "" : labelinfos
            };
            _iWXMsgTemplateService.AddSendRecord(sendRecord);
            return Json(new { success = true });
        }

        #region
        /// <summary>
        /// 发送优惠券
        /// </summary>
        /// <returns></returns>
        public ActionResult CouponGroupMessage()
        {
            var pageModel = _iMemberLabelService.GetMemberLabelList(new LabelQuery() { });
            ViewBag.LabelInfos = pageModel.Models.ToList();
            return View();
        }

        /// <summary>
        /// 发送优惠券
        /// </summary>
        /// <param name="labelids">标签ID</param>
        /// <param name="couponIds">优惠券ID</param>
        /// <param name="labelinfos">标签名称</param>
        /// <returns></returns>
        public JsonResult SendCouponMsg(string labelids, string couponIds, string labelinfos)
        {
            string msg = CouponApplication.SendCouponMsg(labelids, labelinfos, couponIds, Request.Path.ToString());
            bool success = false;
            if (msg.Equals(""))
            {
                success = true;
                msg = "发送成功！";
            }
            return Json(new { success = success, msg = msg });
        }
        #endregion
    }
}