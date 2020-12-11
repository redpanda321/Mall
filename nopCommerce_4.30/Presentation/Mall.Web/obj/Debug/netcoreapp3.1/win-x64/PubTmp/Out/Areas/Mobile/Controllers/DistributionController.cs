using AutoMapper;
using Mall.Application;
using Mall.CommonModel;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Areas.Admin.Models.Market;
using Mall.Web.Areas.Mobile.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Containers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;


namespace Mall.Web.Areas.Mobile.Controllers
{
    public class DistributionController : BaseMobileMemberController
    {
        private const string DISTRIBUTOR_LOGO_PATH = "/Storage/Distribution/";
        public DistributorInfo CurrentDistributor;
        public DistributionController()
        {
            var siteSetting = SiteSettingApplication.SiteSettings;
            if (string.IsNullOrWhiteSpace(siteSetting.WeixinAppId) || string.IsNullOrWhiteSpace(siteSetting.WeixinAppSecret))
            {
                throw new MallException("未配置公众号参数");
            }
        }
        /// <summary>
        /// 前置处理
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (CurrentUser == null)
            {
                return;
            }
            CurrentDistributor = DistributionApplication.GetDistributor(CurrentUser.Id);
            string actionName = filterContext.RouteData.Values["action"].ToString().ToLower();
            if (!WebHelper.IsAjax())
            {
                if (actionName.IndexOf("tips") > -1)
                {
                    return;
                }
                if (!SiteSettings.DistributionIsEnable)
                {
                    Response.Clear();
                   // Response.BufferOutput = true;
                    Response.Redirect(Url.Action("TipsClose"));
                    //Response.Redirect(Url.Action("Center", "Member"));
                }

                if (PlatformType != PlatformType.WeiXin)
                {
                    Response.Clear();
                   // Response.BufferOutput = true;
                    Response.Redirect(Url.Action("TipsNoWeiXin"));

                }

                //处理其他统一情况
                if ((CurrentDistributor == null || CurrentDistributor.IsOnlyMember) && (actionName != "openmyshop" && actionName != "applydistributor"))
                {
                    Response.Clear();
                  //  Response.BufferOutput = true;
                    Response.Redirect(Url.Action("OpenMyShop"));
                }
                if (CurrentDistributor != null && (actionName != "openmyshop" && actionName != "applydistributor"))
                {
                    if (CurrentDistributor.DistributionStatus == (int)DistributorStatus.NotAvailable)
                    {
                        Response.Clear();
                       // Response.BufferOutput = true;
                        Response.Redirect(Url.Action("TipsNotAvailable"));
                    }
                    if (CurrentDistributor.DistributionStatus == (int)DistributorStatus.Refused)
                    {
                        Response.Clear();
                       // Response.BufferOutput = true;
                        Response.Redirect(Url.Action("TipsRefused"));
                    }
                    if (CurrentDistributor.DistributionStatus == (int)DistributorStatus.UnAudit)
                    {
                        Response.Clear();
                    //    Response.BufferOutput = true;
                        Response.Redirect(Url.Action("TipsUnAudit"));
                    }
                }
            }
        }

        /// <summary>
        /// 我要开店
        /// </summary>
        /// <returns></returns>
        public ActionResult OpenMyShop()
        {
            ViewBag.content = SiteSettings.DistributorPageContent;
            ViewBag.Title = SiteSettings.DistributorRenameOpenMyShop;
            return View();
        }
        /// <summary>
        /// 是否可以申请销售员
        /// </summary>
        /// <returns></returns>
        public JsonResult CanOpenMyShop()
        {
            if (SiteSettings.DistributorApplyNeedQuota > 0)
            {
                var _u = MemberApplication.GetMember(CurrentUser.Id);
                if (_u.NetAmount < SiteSettings.DistributorApplyNeedQuota)
                {
                    return Json(new Result { success = false, msg = "需要累计消费金额达到" + SiteSettings.DistributorApplyNeedQuota + "元才可申请哦" });
                }
            }
            if (CurrentDistributor != null)
            {
                if (CurrentDistributor.DistributionStatus == (int)DistributorStatus.NotAvailable)
                {
                    return Json(new Result { success = true, msg = "您己经被清退，不可以申请！", code = 12 });
                }
                if (CurrentDistributor.DistributionStatus == (int)DistributorStatus.Audited)
                {
                    return Json(new Result { success = true, msg = "您己经是销售员，不可以重复申请！", code = 11 });
                }
                if (CurrentDistributor.DistributionStatus == (int)DistributorStatus.UnAudit)
                {
                    return Json(new Result { success = true, msg = "您己经提交销售员申请，请耐心等待！", code = 13 });
                }
            }
            return Json(new Result { success = true });
        }
        /// <summary>
        /// 销售员申请
        /// </summary>
        /// <returns></returns>
        public ActionResult ApplyDistributor()
        {
            var result = CurrentDistributor;
            if (result == null)
            {
                result = new DistributorInfo();
                result.MemberId = CurrentUser.Id;
            }
            if (string.IsNullOrWhiteSpace(result.ShopLogo))
            {
                result.ShopLogo = CurrentUser.Photo;
            }
            ViewBag.Title = "提交申请";
            return View(result);
        }

        /// <summary>
        /// 提交申请销售员
        /// </summary>
        /// <returns></returns>
        public JsonResult PostApplyDistributor(string logoWXmediaId, string shopname)
        {
            if (SiteSettings.DistributorApplyNeedQuota > 0)
            {
                var _u = MemberApplication.GetMember(CurrentUser.Id);
                if (_u.NetAmount < SiteSettings.DistributorApplyNeedQuota)
                {
                    throw new MallException("需要累计消费金额达到" + SiteSettings.DistributorApplyNeedQuota + "元才可申请哦");
                }
            }
            if (CurrentDistributor == null)
            {
                CurrentDistributor = new DistributorInfo
                {
                    MemberId = CurrentUser.Id
                };
            }
            if (string.IsNullOrWhiteSpace(CurrentDistributor.ShopLogo))
            {
                CurrentDistributor.ShopLogo = CurrentUser.Photo;
            }
            if (string.IsNullOrWhiteSpace(logoWXmediaId))
            {
                if (CurrentDistributor == null || string.IsNullOrWhiteSpace(CurrentDistributor.ShopLogo))
                {
                    throw new MallException("请上传小店logo");
                }
            }
            if (string.IsNullOrWhiteSpace(shopname))
            {
                throw new MallException("请填写小店名称");
            }
            if (shopname.Length > 10)
            {
                throw new MallException("小店名称不能超过10个字符");
            }
            if (CurrentDistributor != null)
            {
                if (CurrentDistributor.DistributionStatus == (int)DistributorStatus.NotAvailable)
                {
                    return Json(new Result { success = true, msg = "您己经被清退，不可以申请！", code = 12 });
                }
                if (CurrentDistributor.DistributionStatus == (int)DistributorStatus.Audited)
                {
                    return Json(new Result { success = true, msg = "您己经是销售员，不可以重复申请！", code = 11 });
                }
                if (CurrentDistributor.DistributionStatus == (int)DistributorStatus.UnAudit)
                {
                    return Json(new Result { success = true, msg = "您己经提交销售员申请，请耐心等待！", code = 13 });
                }
            }

            string shoplogo = CurrentDistributor != null ? CurrentDistributor.ShopLogo : "";
            if (!string.IsNullOrWhiteSpace(logoWXmediaId))
            {
                shoplogo = DownloadWxImage(logoWXmediaId);
            }
            if (string.IsNullOrWhiteSpace(shoplogo))
            {
                throw new MallException("请上传小店logo");
            }
            var oldname = Path.GetFileName(shoplogo);
            string ImageDir = string.Empty;

            if (!string.IsNullOrWhiteSpace(shoplogo))
            {
                //转移图片
                string relativeDir = DISTRIBUTOR_LOGO_PATH;
                Random ra = new Random();
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + ra.Next(4) + ".jpg";
                if (shoplogo.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
                {
                    var de = shoplogo.Substring(shoplogo.LastIndexOf("/temp/"));
                    Core.MallIO.CopyFile(de, relativeDir + fileName, true);
                    shoplogo = relativeDir + fileName;
                }  //目标地址
                else if (shoplogo.Contains("/Storage"))
                {
                    shoplogo = shoplogo.Substring(shoplogo.LastIndexOf("/Storage"));
                }
            }

            var d = DistributionApplication.ApplyDistributor(CurrentUser.Id, shoplogo, shopname);
            if (d == null)
            {
                return Json(new Result { success = false, msg = "申请失败，系统异常！", code = 41 });

            }
            if (d.DistributionStatus == (int)DistributorStatus.Audited)
            {
                return Json(new Result { success = true, msg = "申请销售员成功！" });
            }
            return Json(new Result { success = true, msg = "提交销售员申请成功！", code = 21 });
        }

        /// <summary>
        /// 我的佣金
        /// </summary>
        /// <returns></returns>
        public ActionResult MyBrokerage()
        {
            var memberId = CurrentUser.Id;
            ViewBag.Title = SiteSettings.DistributorRenameMyBrokerage;
            var model = DistributionApplication.GetDistributorDTO(memberId);
            model.NoSettlementAmount = DistributionApplication.GetNoSettlementAmount(memberId);
            ViewBag.Distributor = model;


            return View();
        }

        public ActionResult GetRecords(DistributorRecordQuery query)
        {
            query.MemberId = CurrentUser.Id;
            var models = DistributionApplication.GetRecords(query);
            var list = models.Models.Select(item => new
            {
                Type = item.Type.ToDescription(),
                Time = item.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                Amount = item.Amount.ToString("n2"),
                item.Remark,
            });

            return Json(new { success = true, list }, true);
        }
        /// <summary>
        /// 申请提现
        /// </summary>
        /// <returns></returns>
        public ActionResult ApplyWithdraw()
        {
            ViewBag.Title = "申请提现";
            var distributor = DistributionApplication.GetDistributorDTO(CurrentUser.Id);
            ViewBag.Balance = distributor.Balance;//账户余额
            ViewBag.IsSetPassword = MemberApplication.HasPayPassword(CurrentUser.Id);//是否设置交易密码

            var settings = new DistributionWithdrawSettings
            {
                MinLimit = SiteSettings.DistributorWithdrawMinLimit,
                MaxLimit = SiteSettings.DistributorWithdrawMaxLimit,
                Types = SiteSettings.DistributorWithdrawTypes,
            };
            return View(settings);
        }

        public ActionResult PostWithdraw(DistributionApplyWithdraw post)
        {
            post.MemberId = CurrentUser.Id;
            if (post.Type == DistributionWithdrawType.WeChat) //获取用户微信账户
            {
                var openid = WebHelper.GetCookie(CookieKeysCollection.Mall_USER_OpenID);
                post.WithdrawAccount = openid = SecureHelper.AESDecrypt(openid, "Mobile");
                if (!(string.IsNullOrWhiteSpace(SiteSettings.WeixinAppId) || string.IsNullOrWhiteSpace(SiteSettings.WeixinAppSecret)))
                {
                    string token = AccessTokenContainer.TryGetAccessToken(SiteSettings.WeixinAppId, SiteSettings.WeixinAppSecret);
                    var user = CommonApi.GetUserInfo(token, openid);
                    post.WithdrawName = user?.nickname ?? string.Empty;
                }
            }
            DistributionApplication.ApplyWithdraw(post);
            return Json(new { success = true }, true);
        }
        /// <summary>
        /// 提现记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Withdraws()
        {
            ViewBag.Title = "提现记录";
            return View();
        }
        [HttpPost]
        public ActionResult Withdraws(DistributionWithdrawQuery query)
        {
            query.MemberId = CurrentUser.Id;
            var data = DistributionApplication.GetWithdraws(query);
            var list = data.Models.Select(p => new
            {
                p.Amount,
                Id = p.Id.ToString(),
                Time = p.ApplyTime,
                WithdrawType = p.WithdrawType.ToDescription(),
                Status = p.WithdrawStatus,
                WithdrawStatus = p.WithdrawStatus.ToDescription()
            });

            return Json(new { success = true, list }, true);
        }

        public ActionResult InitPayPassowrd(string password)
        {
            if (MemberApplication.HasPayPassword(CurrentUser.Id))
                throw new MallException("已设置过支付密码");
            MemberApplication.ChangePayPassword(CurrentUser.Id, password);
            return Json(new { success = true }, true);
        }
        /// <summary>
        /// 小店订单
        /// </summary>
        /// <returns></returns>
        public ActionResult ShopOrder()
        {
            ViewBag.Title = SiteSettings.DistributorRenameShopOrder;
            return View();
        }
        public JsonResult GetShopOrder(int page = 1)
        {
            DistributionBrokerageQuery query = new DistributionBrokerageQuery
            {
                PageNo = page,
                PageSize = 5,
                DistributorId = CurrentDistributor.MemberId
            };
            var data = DistributionApplication.GetDistributorBrokerageOrderList(query);
            var result = new { rows = data.Models, total = data.Total };
            return Json(result);
        }
        /// <summary>
        /// 我的下级
        /// </summary>
        /// <returns></returns>
        public ActionResult MySubordinate()
        {
            var settings = SiteSettingApplication.SiteSettings;
            ViewBag.Title = SiteSettings.DistributorRenameMySubordinate;
            var subordinate = DistributionApplication.GetSubordinate(CurrentUser.Id);
            var model = new MySubordinateViewModel
            {
                MaxLevel = settings.DistributionMaxLevel,
                Levels = new Dictionary<int, MySubordinateLevelViewModel>()
            };
            if (model.MaxLevel >= 1)
            {
                model.Levels.Add(1, new MySubordinateLevelViewModel
                {
                    Name = settings.DistributorRenameMemberLevel1,
                    Count = subordinate[1].Count
                });
            }
            if (model.MaxLevel >= 2)
            {
                model.Levels.Add(2, new MySubordinateLevelViewModel
                {
                    Name = settings.DistributorRenameMemberLevel2,
                    Count = subordinate[2].Count
                });
            }
            if (model.MaxLevel >= 3)
            {
                model.Levels.Add(3, new MySubordinateLevelViewModel
                {
                    Name = settings.DistributorRenameMemberLevel3,
                    Count = subordinate[3].Count
                });
            }
            ViewBag.Title = SiteSettings.DistributorRenameMySubordinate;
            return View(model);
        }
        [HttpPost]
        public ActionResult MySubordinate(DistributorSubQuery query)
        {
            query.SuperiorId = CurrentUser.Id;
            query.IsAll = true;
            var data = DistributionApplication.GetDistributors(query);
            var list = data.Models.Select(item => new
            {
                MemberName = item.Member.UserName,
                TotalCount = item.Achievement?.TotalCount ?? 0,
                TotalAmount = item.Achievement?.TotalAmount ?? 0,
                RegTime = item.Member.CreateDate.ToString("yyyy-MM-dd"),
                Photo = MallIO.GetRomoteImagePath(item.Member.Photo)
            }).ToList();
            return Json(list, true);
        }
        /// <summary>
        /// 分销市场
        /// </summary>
        /// <returns></returns>
        public ActionResult Market()
        {
            DistributionMarketViewModel result = new DistributionMarketViewModel();
            result.AllTopCategories = DistributionApplication.GetHaveDistributionProductTopCategory();
            result.ShareProductUrlTMP = Request.Scheme + "://" + Request.Host.ToString() + Url.Action("Detail", "Product", new { id = "#id#", SpreadId = "#uid#" });
            ViewBag.Title = SiteSettings.DistributorRenameMarket;
            var shareIcon = string.Empty;
            if (CurrentSpreadId.HasValue)
            {
                var distributor = DistributionApplication.GetDistributor(CurrentSpreadId.Value);
                if (distributor != null && !string.IsNullOrWhiteSpace(distributor.ShopLogo))
                {
                    shareIcon = MallIO.GetRomoteImagePath(distributor.ShopLogo);
                }
            }
            ViewBag.ShareIcon = shareIcon;
            
            return View(result);
        }

        /// <summary>
        /// 获取分销商品
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetProductList(string productName, long? categoryId = null, int page = 1)
        {
            //查询条件
            DistributionProductQuery query = new DistributionProductQuery();
            query.ProductName = productName;
            query.CategoryId = categoryId;
            query.Status = DistributionProductStatus.Normal;
            query.PageSize = 8;
            query.PageNo = page;

            query.Sort = "SaleCount";
            query.IsAsc = false;

            var data = DistributionApplication.GetProducts(query);
            foreach (var item in data.Models)
            {
                item.DefaultImage = MallIO.GetRomoteImagePath(item.DefaultImage);
            }
            var result = new { rows = data.Models, total = data.Total };
            return Json(result);
        }
        /// <summary>
        /// 我的小店
        /// </summary>
        /// <returns></returns>
        public ActionResult MyShop()
        {
            //Mapper.CreateMap<DistributorInfo, DistributionMyShopViewModel>();
            DistributionMyShopViewModel result = CurrentDistributor.Map<DistributionMyShopViewModel>();
            if (string.IsNullOrWhiteSpace(result.ShopLogo))
            {
                result.ShopLogo = CurrentUser.Photo;
            }
            result.ShopLogo = Mall.Core.MallIO.GetRomoteImagePath(result.ShopLogo);
            var grades = DistributionApplication.GetDistributorGrades();
            var nextgrade = grades.Where(d => d.Quota > result.SettlementAmount).OrderBy(d => d.Quota).FirstOrDefault();
            if (nextgrade != null)
            {
                result.NextGradeName = nextgrade.GradeName;
                result.UpgradeNeedAmount = nextgrade.Quota - result.SettlementAmount;
            }
            result.NoSettlementAmount = DistributionApplication.GetNoSettlementAmount(CurrentDistributor.MemberId);
            result.CurrentSiteSettings = SiteSettings;
            ViewBag.Title = SiteSettings.DistributorRenameMyShop;
            return View(result);
        }
        /// <summary>
        /// 小店设置
        /// </summary>
        /// <returns></returns>
        public ActionResult ShopConfig()
        {
            ViewBag.Title = SiteSettings.DistributorRenameShopConfig;
            //TODO:DZY[180412]需要前端协助弄一下前台的图片上传
            return View(CurrentDistributor);
        }

        /// <summary>
        /// 提交申请销售员
        /// </summary>
        /// <returns></returns>
        public JsonResult SaveShopConfig(string logoWXmediaId, string shopname, bool isShowLogo)
        {
            if (string.IsNullOrWhiteSpace(CurrentDistributor.ShopLogo) && string.IsNullOrWhiteSpace(logoWXmediaId))
            {
                throw new MallException("请上传小店logo");
            }
            if (string.IsNullOrWhiteSpace(shopname))
            {
                throw new MallException("请填写小店名称");
            }
            if (shopname.Length > 10)
            {
                throw new MallException("小店名称不能超过10个字符");
            }
            string shoplogo = CurrentDistributor.ShopLogo;
            if (!string.IsNullOrWhiteSpace(logoWXmediaId))
            {
                shoplogo = DownloadWxImage(logoWXmediaId);
            }
            if (string.IsNullOrWhiteSpace(shoplogo))
            {
                throw new MallException("请上传小店logo");
            }
            var oldname = Path.GetFileName(shoplogo);
            string ImageDir = string.Empty;

            if (!string.IsNullOrWhiteSpace(shoplogo))
            {
                //转移图片
                string relativeDir = DISTRIBUTOR_LOGO_PATH;
                Random ra = new Random();
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + ra.Next(4) + ".jpg";
                if (shoplogo.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
                {
                    var de = shoplogo.Substring(shoplogo.LastIndexOf("/temp/"));
                    Core.MallIO.CopyFile(de, relativeDir + fileName, true);
                    shoplogo = relativeDir + fileName;
                }  //目标地址
                else if (shoplogo.Contains("/Storage"))
                {
                    shoplogo = shoplogo.Substring(shoplogo.LastIndexOf("/Storage"));
                }
            }
            DistributionApplication.UpdateDistributorConfig(CurrentDistributor.MemberId, shoplogo, shopname, isShowLogo);
            return Json(new Result { success = true, msg = "小店设置修改成功！", code = 21 });
        }
        /// <summary>
        /// 下载微信图片
        /// </summary>
        /// <param name="link">下载地址</param>
        /// <param name="filePath">保存相对路径</param>
        /// <param name="fileName">保存地址</param>
        /// <returns></returns>
        public string DownloadWxImage(string mediaId)
        {
            var token = AccessTokenContainer.TryGetAccessToken(SiteSettings.WeixinAppId, SiteSettings.WeixinAppSecret);
            var address = string.Format("https://file.api.weixin.qq.com/cgi-bin/media/get?access_token={0}&media_id={1}", token, mediaId);
            Random ra = new Random();
            var fileName = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + ra.Next(4) + ".jpg";
            var ImageDir = DISTRIBUTOR_LOGO_PATH;
            WebClient wc = new WebClient();
            try
            {
                string fullPath = Path.Combine(ImageDir, fileName);
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
                var data = wc.DownloadData(address);
                MemoryStream stream = new MemoryStream(data);
                Core.MallIO.CreateFile(fullPath, stream, FileCreateType.Create);
                return DISTRIBUTOR_LOGO_PATH + fileName;
            }
            catch (Exception ex)
            {
                Log.Error("下载图片发生异常" + ex.Message);
                return string.Empty;
            }
        }
        public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {  // 总是接受  
            return true;
        }

        #region 提示
        /// <summary>
        /// 提示待审核
        /// </summary>
        /// <returns></returns>
        public ActionResult TipsUnAudit()
        {
            return View();
        }
        /// <summary>
        /// 提示未通过
        /// </summary>
        /// <returns></returns>
        public ActionResult TipsRefused()
        {
            ViewBag.remark = CurrentDistributor.Remark;
            return View();
        }
        /// <summary>
        /// 提示己清退
        /// </summary>
        /// <returns></returns>
        public ActionResult TipsNotAvailable()
        {
            return View();
        }
        /// <summary>
        /// 提示非微信端
        /// </summary>
        /// <returns></returns>
        public ActionResult TipsNoWeiXin()
        {
            return View();
        }
        /// <summary>
        /// 提示己关闭
        /// </summary>
        /// <returns></returns>
        public ActionResult TipsClose()
        {
            return View();
        }
        #endregion

    }
}