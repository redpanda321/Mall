using Mall.Core.Helper;
using Mall.IServices;
using Mall.Web.Framework;
using Mall.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;


using Mall.DTO;
using Senparc.Weixin.MP.CommonAPIs;
using Mall.Core;
using System.Threading.Tasks;
using Mall.Core.Plugins.Message;

using Mall.Application;
using Mall.CommonModel;
using Microsoft.AspNetCore.Mvc;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.MP.AdvancedAPIs;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    //门店基本信息

        [Area("SellerAdmin")]
    public class AccountSettingsController : BaseSellerController
    {
        private IMessageService _iMessageService;

        public AccountSettingsController(
            IMessageService iMessageService)
        {
            _iMessageService = iMessageService;
        }

        #region 银行相关


        /// <summary>
        /// 修改银行账户
        /// </summary>
        /// <returns></returns>
        public ActionResult Bank()
        {
            long uid = ShopApplication.GetShopManagers(CurrentSellerManager.ShopId);
            Mall.DTO.MemberAccountSafety mMemberAccountSafety = MemberApplication.GetMemberAccountSafety(uid);
            ViewBag.MemberEmail = mMemberAccountSafety.Email;
            ViewBag.MemberPhone = mMemberAccountSafety.Phone;
            return View();
        }

        /// <summary>
        /// 银行数据提交
        /// </summary>
        /// <param name="BankAccountName">开户行名称</param>
        /// <param name="BankAccountNumber">开户行号</param>
        /// <param name="BankName">支行名称</param>
        /// <param name="BankCode">支行号</param>
        /// <param name="BankRegionId">开户行所在地</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult setBank(string BankAccountName, string BankAccountNumber, string BankName, string BankCode, int BankRegionId)
        {
            Mall.DTO.BankAccount model = new BankAccount() {
                ShopId = CurrentSellerManager.ShopId,
                BankAccountName = BankAccountName,
                BankAccountNumber = BankAccountNumber,
                BankCode = BankCode,
                BankName = BankName,
                BankRegionId = BankRegionId
            };

            ShopApplication.SetBankAccount(model);
            return Json(new { success = true, msg = "成功" });
        }

        #endregion

        #region 微信相关

        /// <summary>
        /// 绑定微信
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Weixin()
        {
            long uid = ShopApplication.GetShopManagers(CurrentSellerManager.ShopId);
            Mall.DTO.MemberAccountSafety mMemberAccountSafety = MemberApplication.GetMemberAccountSafety(uid);
            ViewBag.MemberEmail = mMemberAccountSafety.Email;
            ViewBag.MemberPhone = mMemberAccountSafety.Phone;
            return View();
        }
        /// <summary>
        /// 获取微信二维码
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult getWinxin(string pluginId, string destination)
        {
            long uid = ShopApplication.GetShopManagers(CurrentSellerManager.ShopId);
            var siteSetting = SiteSettingApplication.SiteSettings;
            //微信二维码
            if (string.IsNullOrWhiteSpace(siteSetting.WeixinAppId) || string.IsNullOrWhiteSpace(siteSetting.WeixinAppSecret))
                throw new MallException("未配置公众号参数");
            var token = AccessTokenContainer.TryGetAccessToken(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret);

            SceneModel scene = new SceneModel(QR_SCENE_Type.Binding)
            {
                Object = uid.ToString()
            };
            SceneHelper helper = new SceneHelper();
            var sceneid = helper.SetModel(scene);
            var ticket = QrCodeApi.Create(token, 300, sceneid, Senparc.Weixin.MP.QrCode_ActionName.QR_LIMIT_SCENE, null);

            return Json(new { success = true, msg = "成功", ticket = ticket.ticket, Sceneid = sceneid });
        }


        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SendCode(string pluginId, string destination)
        {
            long uid = ShopApplication.GetShopManagers(CurrentSellerManager.ShopId);
            var model = MemberApplication.GetMembers(uid);
            if (!MemberApplication.SendCode(pluginId, destination, model.UserName, SiteSettings.SiteName))
            {
                return Json(new Result() { success = false, msg = "120秒内只允许请求一次，请稍后重试！" });
            }
            return Json(new Result() { success = true, msg = "发送成功" });
        }

        /// <summary>
        /// 绑定微信验证手机
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult verificationCode(string pluginId, string code)
        {
            long uid = ShopApplication.GetShopManagers(CurrentSellerManager.ShopId);
            Mall.DTO.MemberAccountSafety mMemberAccountSafety = MemberApplication.GetMemberAccountSafety(uid);
            string destination = "";
            if (pluginId.Equals("Mall.Plugin.Message.Email"))
                destination = mMemberAccountSafety.Email;
            else
                destination = mMemberAccountSafety.Phone;

            int result = MemberApplication.CheckCode(pluginId, code, destination, uid);
            if (result > 0)
                return Json(new Result() { success = true });
            else
                return Json(new Result() { success = false });
        }

        /// <summary>
        /// 绑定微信
        /// </summary>
        /// <param name="sceneid">微信识别码</param>
        /// <param name="trueName">真实姓名</param>
        /// <returns></returns>
        public JsonResult setShopWeiXin(string sceneid, string trueName)
        {
            if (!sceneid.Equals("") && !trueName.Equals(""))
            {
                var key = CacheKeyCollection.BindingReturn(sceneid);
                var obj = Core.Cache.Get<Mall.DTO.WeiXinInfo>(key);

                Mall.DTO.WeChatAccount model = new WeChatAccount()
                {
                    ShopId = CurrentSellerManager.ShopId,
                    Address = obj.province + obj.city,
                    Logo = string.IsNullOrEmpty(obj.headimgurl) ? "" : obj.headimgurl,
                    Sex = obj.sex,
                    WeiXinNickName =string.IsNullOrEmpty(obj.NickName)?"":obj.NickName,
                    WeiXinOpenId = obj.OpenId,
                    WeiXinRealName = trueName
                };

                ShopApplication.SetWeChatAccount(model);
                return Json(new Result() { success = true });
            }
            else
            {
                return Json(new Result() { success = false });
            }
        }

        public ActionResult Finish()
        {
            return View();
        }
        #endregion

        #region 账号认证



        /// <summary>
        /// 管理员手机认证
        /// </summary>
        /// <returns></returns>
        public ActionResult Authenticate()
        {
            long uid = ShopApplication.GetShopManagers(CurrentSellerManager.ShopId);
            Mall.DTO.MemberAccountSafety mMemberAccountSafety = MemberApplication.GetMemberAccountSafety(uid);
            ViewBag.MemberEmail = mMemberAccountSafety.Email;
            ViewBag.MemberPhone = mMemberAccountSafety.Phone;
            return View();
        }

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SendPhoneCode(string pluginId, string destination)
        {
            long uid = ShopApplication.GetShopManagers(CurrentSellerManager.ShopId);
            var model = MemberApplication.GetMembers(uid);
            Mall.CommonModel.SendMemberCodeReturn status=MemberApplication.SendMemberCode(pluginId, destination, model.UserName, SiteSettings.SiteName);

            bool bo = status.Equals(Mall.CommonModel.SendMemberCodeReturn.success);
            return Json(new Result() { success = bo, msg = status.ToDescription() });
        }

        /// <summary>
        /// 绑定管理员手机号
        /// </summary>
        /// <returns></returns>
        public JsonResult setPhone(string pluginId, string code, string phone)
        {
            long uid = ShopApplication.GetShopManagers(CurrentSellerManager.ShopId);
            int result = MemberApplication.CheckMemberCode(pluginId, code, phone, uid);

            if (result > 0)
            {
                return Json(new Result() { success = true, msg = "修改成功" });
            }
            else if (result == -1)
            {
                return Json(new Result() { success = false, msg = "此联系方式已被绑定!" });
            }
            else
                return Json(new Result() { success = false, msg = "验证码错误" });
        }

        #endregion
    }

    #region 异步获取微信回调信息

    public class ScanShopStateController : BaseAsyncController
    {
        public void GetStateAsync(string sceneid)
        {
            //AsyncManager.OutstandingOperations.Increment();
            int interval = 200;//定义刷新间隔为200ms
            int maxWaitingTime = 10 * 1000;//定义最大等待时间为10s
            Task.Factory.StartNew(() =>
            {
                int time = 0;
                while (true)
                {
                    var key = CacheKeyCollection.BindingReturn(sceneid);
                    var obj = Core.Cache.Get<Mall.DTO.WeiXinInfo>(key);
                    if (obj != null)
                    {
                        //AsyncManager.Parameters["state"] = true;
                        //AsyncManager.Parameters["model"] = obj;
                        break;
                    }
                    else
                    {
                        if (time >= maxWaitingTime)
                        {
                            //AsyncManager.Parameters["state"] = false;
                            //AsyncManager.Parameters["model"] = obj;
                            break;
                        }
                        else
                        {
                            time += interval;
                            System.Threading.Thread.Sleep(interval);
                        }
                    }
                }
                //AsyncManager.OutstandingOperations.Decrement();
            });
        }
        public JsonResult GetStateCompleted(bool state, Mall.DTO.WeiXinInfo model)
        {
            return Json(new { success = state, data = model });
        }
    }
    #endregion
}