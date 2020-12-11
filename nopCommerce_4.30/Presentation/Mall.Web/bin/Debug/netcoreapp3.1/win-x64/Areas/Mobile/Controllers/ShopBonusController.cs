using Mall.Core;
using Mall.DTO;
using Mall.IServices;
using Mall.Web.Areas.Mobile.Models;
using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Web.Framework;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using Senparc.Weixin.MP.AdvancedAPIs.User;
using Senparc.Weixin.MP.CommonAPIs;
using System;
using System.Collections.Generic;

using Mall.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.Containers;

namespace Mall.Web.Areas.Mobile.Controllers
{
    public class ShopBonusController : BaseMobileTemplatesController
    {
        private IShopBonusService _bonusService = null;
        private IWXCardService _iWXCardService = null;
        private IVShopService _iVShopService;
        private IShopService _iShopService;
        private Entities.WXCardLogInfo.CouponTypeEnum ThisCouponType = Entities.WXCardLogInfo.CouponTypeEnum.Bonus;
        public ShopBonusController(IShopBonusService iShopBonusService,
            IWXCardService iWXCardService,
            IVShopService iVShopService,
            IShopService iShopService
            )
        {
            var settings = SiteSettingApplication.SiteSettings;
            if (string.IsNullOrWhiteSpace(settings.WeixinAppId) || string.IsNullOrWhiteSpace(settings.WeixinAppSecret))
            {
                throw new MallException("未配置公众号参数");
            }
            this._bonusService = iShopBonusService;
            this._iWXCardService = iWXCardService;
            _iVShopService = iVShopService;
            _iShopService = iShopService;
        }


        public ActionResult Index(long id)
        {
            if (this.PlatformType != Core.PlatformType.WeiXin)
            {
                Log.Info(this.PlatformType.ToString());
                return Content("只能在微信端访问");
            }

            var bonus = this._bonusService.GetByGrantId(id);
            if (bonus == null)
            {
                Log.Info("红包失踪，id = " + id);
                return Redirect("/m-weixin/ShopBonus/Invalidtwo");
            }

            //授权获取openid相关
            string code = HttpContext.Request.Query["code"].ToString();
            OAuthAccessTokenResult wxOpenInfo = null;
            var settings = SiteSettingApplication.SiteSettings;
            if (string.IsNullOrEmpty(code))
            {
                string selfAddress = Request.GetDisplayUrl();
                string url = OAuthApi.GetAuthorizeUrl(settings.WeixinAppId.Trim(), selfAddress, "STATE", Senparc.Weixin.MP.OAuthScope.snsapi_userinfo, "code");
                return Redirect(url);
            }
            else
            {
                try
                {
                    wxOpenInfo = OAuthApi.GetAccessToken(settings.WeixinAppId.Trim(), settings.WeixinAppSecret.Trim(), code, "authorization_code");
                }
                catch (Exception e)
                {
                    Exception innerEx = e.InnerException == null ? e : e.InnerException;
                    Log.Error(innerEx.Message);
                    if (innerEx.Message.IndexOf("code been used") != -1)
                    {
                        return Content("该红包已被领取过");
                    }
                    else
                    {
                        return Content(innerEx.Message);
                    }
                }
            }

            OAuthUserInfo wxUserInfo = OAuthApi.GetUserInfo(wxOpenInfo.access_token, wxOpenInfo.openid);
            var openId = wxOpenInfo.openid;
            if (!string.IsNullOrEmpty(wxUserInfo.unionid))
            {
                var moInfo = MemberApplication.GetMemberOpenIdInfoByOpenIdOrUnionId(openId, wxUserInfo.unionid);
                if (moInfo != null && !string.IsNullOrEmpty(moInfo.OpenId))
                {
                    openId = moInfo.OpenId;
                }
            }
            ShopBonusModel model = new ShopBonusModel(bonus);
            if (model.DateEnd <= DateTime.Now || model.IsInvalid || model.BonusDateEnd <= DateTime.Now)  //过期、失效
            {
                return Redirect("/m-weixin/ShopBonus/Expired/" + model.Id + "?openId=" + openId + "&grantid=" + id + "&wxhead=" + wxUserInfo.headimgurl);
            }
            else if (model.DateStart > DateTime.Now) // 未开始
            {
                return Redirect("/m-weixin/ShopBonus/NotStart/" + model.Id);
            }
            ShopReceiveModel obj = (ShopReceiveModel)this._bonusService.Receive(id, openId, wxUserInfo.headimgurl, wxUserInfo.nickname);

            //防两次刷新
            string cacheDName = "WXSB_U" + openId + "_D" + id.ToString();
            if (obj.State == ShopReceiveStatus.Receive)
            {
                var _tmp = Cache.Get<ShopReceiveModel>(cacheDName);
                if (_tmp != null && _tmp.State == ShopReceiveStatus.CanReceive)
                {
                    obj = _tmp;
                }
            }

            var host = CurrentUrlHelper.CurrentUrl();
            if (obj.State == ShopReceiveStatus.CanReceive)
            {
                Cache.Insert<ShopReceiveModel>(cacheDName, obj, 2);
                return Redirect("/m-weixin/ShopBonus/Completed/" + model.Id + "?openId=" + openId + "&price=" + obj.Price + "&user=" + obj.UserName + "&grantid=" + id + "&rid=" + obj.Id + "&wxhead=" + wxUserInfo.headimgurl + "&host=" + host);
            }
            else if (obj.State == ShopReceiveStatus.CanReceiveNotUser)
            {
                return Redirect("/m-weixin/ShopBonus/CompletedNotUser/" + model.Id + "?openId=" + openId + "&price=" + obj.Price + "&grantid=" + id + "&rid=" + obj.Id + "&wxhead=" + wxUserInfo.headimgurl + "&host=" + host);
            }
            else if (obj.State == ShopReceiveStatus.Receive)
            {
                return Redirect("/m-weixin/ShopBonus/HasReceive/" + model.Id + "?openId=" + openId + "&grantid=" + id + "&wxhead=" + wxUserInfo.headimgurl + "&host=" + host);
            }
            else if (obj.State == ShopReceiveStatus.HaveNot)
            {
                return Redirect("/m-weixin/ShopBonus/HaveNot/" + model.Id + "?openId=" + openId + "&grantid=" + id + "&wxhead=" + wxUserInfo.headimgurl);
            }
            else if (obj.State == ShopReceiveStatus.Invalid)
            {
                return Redirect("/m-weixin/ShopBonus/Expired/" + model.Id + "?openId=" + openId + "&grantid=" + id + "&wxhead=" + wxUserInfo.headimgurl);
            }
            else
            {
                throw new Exception("领取发生异常");
            }
        }

        /// <summary>
        /// 取微信同步卡券信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetWXCardData(long id, long rid)
        {
            Entities.WXJSCardModel result = new Entities.WXJSCardModel();
            bool isdataok = true;
            #region 同步微信前信息准备
            if (isdataok)
            {
                if (this.PlatformType == PlatformType.WeiXin)
                {
                    result = _iWXCardService.GetJSWeiXinCard(id, rid, ThisCouponType);
                }
            }
            #endregion
            return Json<dynamic>(success: true, data: result);
        }

        public ActionResult Completed(long id, string openId = "", decimal price = 0, string user = "", long grantid = 0, long rid = 0, string wxhead = "", string host = "")
        {
            host = CurrentUrlHelper.CurrentUrl();
            ShopBonusDataModel dataModel = new ShopBonusDataModel();
            ShopBonusModel model = new ShopBonusModel(this._bonusService.GetShopBonus(id));
            var shop = ShopApplication.GetShop(model.ShopId);
            dataModel.GrantId = grantid;
            dataModel.ShareHref = host + "/m-weixin/shopbonus/index/" + grantid.ToString();
            dataModel.HeadImg = wxhead;
            //dataModel.HeadImg = Mall.Core.MallIO.GetImagePath(dataModel.HeadImg);
            dataModel.ShopAddress = GetShopAddress(model.ShopId);
            dataModel.UserName = user;
            dataModel.Price = price;
            dataModel.OpenId = openId;
            dataModel.ShopName = shop.ShopName;
            ViewBag.DataModel = dataModel;
            model.ShareImg = MallIO.GetRomoteImagePath(model.ShareImg);
            if (!string.IsNullOrWhiteSpace(model.ShareImg))
            {
                if (model.ShareImg.Substring(0, 4) != "http")
                {
                    model.ShareImg = host + model.ShareImg;
                }
            }
            model.ReceiveId = rid;

            #region 同步微信前信息准备
            if (model.SynchronizeCard == true && this.PlatformType == PlatformType.WeiXin)
            {
                model.WXJSInfo = _iWXCardService.GetSyncWeiXin(id, rid, ThisCouponType, Request.GetDisplayUrl());
                if (model.WXJSInfo != null)
                {
                    model.IsShowSyncWeiXin = true;
                    //model.WXJSCardInfo = ser_wxcard.GetJSWeiXinCard(id, rid, ThisCouponType);    //同步方式有重复留的Bug
                }
            }
            #endregion

            return View(model);
        }

        public ActionResult CompletedNotUser(long id, long rid, string openId = "", decimal price = 0, long grantid = 0, string wxhead = "", string host = "")
        {
            host = CurrentUrlHelper.CurrentUrl();
            ShopBonusDataModel dataModel = new ShopBonusDataModel();
            ShopBonusModel model = new ShopBonusModel(this._bonusService.GetShopBonus(id));
            var shop = ShopApplication.GetShop(model.ShopId);
            dataModel.GrantId = grantid;
            dataModel.ShareHref = host + "/m-weixin/shopbonus/index/" + grantid.ToString();
            dataModel.HeadImg = wxhead;
            //dataModel.HeadImg = Mall.Core.MallIO.GetImagePath(dataModel.HeadImg);
            dataModel.ShopAddress = GetShopAddress(model.ShopId);
            dataModel.Price = price;
            dataModel.OpenId = openId;
            dataModel.ShopName = shop.ShopName; 
            ViewBag.DataModel = dataModel;
            model.ShareImg = MallIO.GetImagePath(model.ShareImg);
            if (!string.IsNullOrWhiteSpace(model.ShareImg))
            {
                if (model.ShareImg.Substring(0, 4) != "http")
                {
                    model.ShareImg = host + model.ShareImg;
                }
            }
            model.ReceiveId = rid;

            #region 同步微信前信息准备
            if (model.SynchronizeCard == true && this.PlatformType == PlatformType.WeiXin)
            {
                model.WXJSInfo = _iWXCardService.GetSyncWeiXin(id, rid, ThisCouponType, Request.GetDisplayUrl());
                if (model.WXJSInfo != null)
                {
                    model.IsShowSyncWeiXin = true;
                    //model.WXJSCardInfo = ser_wxcard.GetJSWeiXinCard(id, rid, ThisCouponType);    //同步方式有重复留的Bug
                }
            }
            #endregion

            return View(model);
        }

        public ActionResult HaveNot(long id, string openId = "", long grantid = 0, string wxhead = "")  //已被领完
        {
            ShopBonusDataModel dataModel = new ShopBonusDataModel();
            dataModel.GrantId = grantid;
            dataModel.HeadImg = wxhead;
            ShopBonusModel model = new ShopBonusModel(this._bonusService.GetShopBonus(id));
            dataModel.ShopAddress = GetShopAddress(model.ShopId);
            ViewBag.DataModel = dataModel;
            model.ShareImg = CurrentUrlHelper.CurrentUrlNoPort() + model.ShareImg;
            return View(model);
        }

        public ActionResult HasReceive(long id, string openId = "", long grantid = 0, string wxhead = "", string host = "")  //已领取过
        {
            host = CurrentUrlHelper.CurrentUrl();
            ShopBonusDataModel dataModel = new ShopBonusDataModel();
            dataModel.GrantId = grantid;
            dataModel.ShareHref = host + "/m-weixin/shopbonus/index/" + grantid.ToString();
            dataModel.OpenId = openId;
            dataModel.HeadImg = wxhead;
            ShopBonusModel model = new ShopBonusModel(this._bonusService.GetShopBonus(id));
            dataModel.ShopAddress = GetShopAddress(model.ShopId);
            ViewBag.DataModel = dataModel;
            model.ShareImg = host + model.ShareImg;
            return View(model);
        }

        public ActionResult Expired(long id, string openId = "", long grantid = 0, string wxhead = "")  //已过期
        {
            ShopBonusDataModel dataModel = new ShopBonusDataModel();
            dataModel.GrantId = grantid;
            dataModel.HeadImg = wxhead;
            ShopBonusModel model = new ShopBonusModel(this._bonusService.GetShopBonus(id));
            dataModel.ShopAddress = GetShopAddress(model.ShopId);
            ViewBag.DataModel = dataModel;
            model.ShareImg = CurrentUrlHelper.CurrentUrlNoPort() + model.ShareImg;
            return View(model);
        }

        public ActionResult NotStart(long id)  //未开始
        {
            ShopBonusModel model = new ShopBonusModel(this._bonusService.GetShopBonus(id));
            model.ShareImg = CurrentUrlHelper.CurrentUrlNoPort() + model.ShareImg;
            return View(model);
        }

        public ActionResult Invalidtwo()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetOtherReceive(long id)
        {
            List<ShopBonusOtherReceiveModel> models = new List<ShopBonusOtherReceiveModel>();
            var result = this._bonusService.GetDetailByGrantId(id);
            int randomIndex = 0;
            foreach (var re in result)
            {
                models.Add(new ShopBonusOtherReceiveModel
                {
                    Name = re.WXName,
                    HeadImg = MallIO.GetImagePath(re.WXHead),
                    Copywriter = RandomStr(randomIndex),
                    Price = (decimal)re.Price,
                    ReceiveTime = ((DateTime)re.ReceiveTime).ToString("yyyy-MM-dd HH:mm")
                });
                randomIndex++;
                if (randomIndex > 4)
                {
                    randomIndex = 0;
                }
            }

            return SuccessResult<dynamic>(data: models);
        }

        /// <summary>
        /// 随机文字
        /// </summary>
        private string RandomStr(int index)
        {
            string[] strs =
            {
                "手气不错，以后购物就来这家店了",
                "抢红包，姿势我最帅",
                "人品攒的好，红包来的早",
                "这个发红包的老板好帅",
                "多谢，老板和宝贝一样靠谱"
            };
            return strs[index];
        }

        private string GetShopAddress(long shopid)
        {
            var result = _iVShopService.GetVShopByShopId(shopid);
            if (result == null)
            {
                return "/";
            }
            else
            {
                return "/m-weixin/vshop/Detail/" + result.Id;
            }
        }

        private UserInfoJson GetWXUserHead(string openid)
        {
            var siteSetting = SiteSettingApplication.SiteSettings;
            if (!string.IsNullOrEmpty(siteSetting.WeixinAppId) || !string.IsNullOrEmpty(siteSetting.WeixinAppSecret))
            {
                string accessToken = AccessTokenContainer.TryGetAccessToken(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret);
                UserInfoJson user = UserApi.Info(accessToken, openid);
                if (user.errcode == Senparc.Weixin.ReturnCode.请求成功)
                {
                    return user;
                }
                else
                {
                    throw new MallException(user.errmsg);
                }
            }
            throw new Exception("未配置公众号");
        }
    }
}