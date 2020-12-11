using Mall.IServices;

using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Web.Framework;
using Mall.Web.Models;
using System;
using System.Collections.Generic;

using System.Linq;
using Mall.Core.Plugins.Payment;
using Mall.Entities;

using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Application;
using Mall.CommonModel;
using Mall.DTO;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class ShopController : BaseSellerController
    {
        private IShopService _iShopService;
        private ICategoryService _iCategoryService;
        private IRegionService _iRegionService;

        public ShopController(
            IShopService iShopService,
            ICategoryService iCategoryService,
            IRegionService iRegionService)
        {
            _iShopService = iShopService;
            _iCategoryService = iCategoryService;
            _iRegionService = iRegionService;
        }
        public ActionResult Invoice()
        {
            var shopinvoice = ShopApplication.GetShopInvoiceConfig(CurrentSellerManager.ShopId);
            if (shopinvoice == null)
            {
                shopinvoice = new ShopInvoiceConfigInfo();
                shopinvoice.ShopId = CurrentSellerManager.ShopId;
            }
            return View(shopinvoice);
        }

        /// <summary>
        /// 公司信息保存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Invoice(ShopInvoiceConfigInfo config)
        {
            var provideInvoice = Core.Helper.WebHelper.GetFormString("ProvideInvoice", "");
            if (provideInvoice.Equals("on"))
                config.IsInvoice = true;
            var plainInvoice = Core.Helper.WebHelper.GetFormString("IsPlainInvoice", "");
            if (plainInvoice.Equals("on"))
                config.IsPlainInvoice = true;
            var electronicInvoice = Core.Helper.WebHelper.GetFormString("IsElectronicInvoice", "");
            if (electronicInvoice.Equals("on"))
                config.IsElectronicInvoice = true;
            var vatInvoice = Core.Helper.WebHelper.GetFormString("IsVatInvoice", "");
            if (vatInvoice.Equals("on"))
                config.IsVatInvoice = true;
            config.ShopId = CurrentSellerManager.ShopId;
            ShopApplication.SetProvideInvoice(config);
            return Json(new { success = true });
        }

      
        public ActionResult FreightSetting()
        {
            ShopInfo shop = ShopApplication.GetShop(CurrentSellerManager.ShopId);
            var shopModel = new ShopFreightModel()
            {
                FreeFreight = shop.FreeFreight,
                Freight = shop.Freight,
            };
            return View(shopModel);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SaveFreightSetting(ShopFreightModel shopFreight)
        {
            ShopApplication.SetShopFreight(CurrentSellerManager.ShopId, shopFreight.Freight, shopFreight.FreeFreight);
            return Json(new { success = true });
        }

        public ActionResult ShopDetail()
        {
            //Note:DZY[151010] 有form数据返回，传参暂时不能改
            var shopid = CurrentSellerManager.ShopId;
            var shop = _iShopService.GetShop(shopid, true);
            var model = new ShopModel(shop);
            model.BusinessCategory = new List<CategoryKeyVal>();
            foreach (var key in shop.BusinessCategory.Keys)
            {
                var category = _iCategoryService.GetCategory(key);
                model.BusinessCategory.Add(new CategoryKeyVal
                {
                    CommisRate = shop.BusinessCategory[key],
                    Name = category != null ? category.Name : "分类不存在"
                });
            }
            ViewBag.CompanyRegionIds = _iRegionService.GetRegionPath(shop.CompanyRegionId);
            ViewBag.BusinessLicenseCert = shop.BusinessLicenseCert;
            //var model= _iShopService.GetShopBasicInfo(shopid);

            string businessLicenseCerts = "";
            string productCerts = "";
            string otherCerts = "";
            for (int i = 0; i < 3; i++)
            {
                if (MallIO.ExistFile(shop.BusinessLicenseCert + string.Format("{0}.png", i + 1)))
                    businessLicenseCerts += MallIO.GetImagePath(shop.BusinessLicenseCert + string.Format("{0}.png", i + 1)) + ",";
                else
                    businessLicenseCerts += "null,";
                if (MallIO.ExistFile(shop.ProductCert + string.Format("{0}.png", i + 1)))
                    productCerts += MallIO.GetImagePath(shop.ProductCert + string.Format("{0}.png", i + 1)) + ",";
                else
                    productCerts += "null,";
                if (MallIO.ExistFile(shop.OtherCert + string.Format("{0}.png", i + 1)))
                    otherCerts = MallIO.GetImagePath(shop.OtherCert + string.Format("{0}.png", i + 1)) + ",";
                else
                    otherCerts += "null,";
            }
            ViewBag.BusinessLicenseCerts = businessLicenseCerts.TrimEnd(',');
            ViewBag.ProductCerts = productCerts.TrimEnd(',');
            ViewBag.OtherCerts = otherCerts.TrimEnd(',');

            //管理员信息
            long uid = ShopApplication.GetShopManagers(CurrentSellerManager.ShopId);
            var mUser = MemberApplication.GetMembers(uid);
            ViewBag.RealName = mUser.RealName;
            var mMemberAccountSafety = MemberApplication.GetMemberAccountSafety(uid);
            ViewBag.MemberEmail = mMemberAccountSafety.Email;
            ViewBag.MemberPhone = mMemberAccountSafety.Phone;

            if (model.BusinessType.Equals(ShopBusinessType.Enterprise))
            {
                return View(model);
            }
            else
            {
                return View("ShopPersonalDetail", model);
            }
        }

        /// <summary>
        /// 公司信息保存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditProfile1()
        {

            string CompanyName = Request.Query["CompanyName"].ToString() ?? "";
            string CompanyAddress = Request.Query["CompanyAddress"].ToString() ?? "";
            string strCompanyRegionId = Request.Query["CompanyRegionId"].ToString() ?? "";
            strCompanyRegionId = string.IsNullOrWhiteSpace(strCompanyRegionId) ? Request.Query["NewCompanyRegionId"] .ToString(): strCompanyRegionId;
            int CompanyRegionId = 0;
            if (!int.TryParse(strCompanyRegionId, out CompanyRegionId))
                CompanyRegionId = 0;
    
            string CompanyEmployeeCount = Request.Query["CompanyEmployeeCount"].ToString()?? "";
            string BusinessLicenseCert = Request.Query["BusinessLicenseCert"].ToString() ?? "";
            string ProductCert = Request.Query["ProductCert"].ToString() ?? "";
            string OtherCert = Request.Query["OtherCert"].ToString() ?? "";

            var info = new ShopCompanyInfo
            {
                ShopId = CurrentSellerManager.ShopId,
                CompanyName = CompanyName,
                CompanyAddress = CompanyAddress,
                CompanyRegionId = CompanyRegionId,
                CompanyEmployeeCount = (CompanyEmployeeCount)int.Parse(CompanyEmployeeCount)
            };
            var cert = new ShopLicenseCert
            {
                ShopId= CurrentSellerManager.ShopId,
                BusinessLicenseCert = BusinessLicenseCert,
                ProductCert = ProductCert,
                OtherCert = OtherCert
            };
            ShopApplication.SetCompanyInfo(info);
            ShopApplication.SetLicenseCert(cert);
            return Json(new { success = true });
        }


        /// <summary>
        /// 公司信息保存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SetCompanyInfo(ShopCompanyInfo info)
        {
            info.ShopId = CurrentSellerManager.ShopId;
            ShopApplication.SetCompanyInfo(info);
            return Json(new { success = true });
        }

        /// <summary>
        /// 营业执照信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Edit2(ShopLicenseCert cert)
        {
            cert.ShopId = CurrentShop.Id;
            ShopApplication.SetLicenseCert(cert);
            return Json(new { success = true });
        }


        /// <summary>
        /// 修改真实姓名
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Edit5(string RealName)
        {
            if (!RealName.Equals(""))
            {
                long uid = ShopApplication.GetShopManagers(CurrentSellerManager.ShopId);
                var member = MemberApplication.GetMembers(uid);
                member.RealName = RealName;
                MemberApplication.UpdateMember(member);
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, msg = "真实姓名不能为空" });
            }
        }

        public ActionResult Renew()
        {
            //店铺当前信息
            ShopRenewModel model = new ShopRenewModel();
            model.ShopId = CurrentSellerManager.ShopId;
            
            var shop = _iShopService.GetShop(CurrentSellerManager.ShopId);
            model.ShopName = shop.ShopName;
            model.ShopCreateTime = shop.CreateDate.ToString("yyyy/MM/dd");
            model.ShopEndTime = shop.EndDate.ToString("yyyy/MM/dd");

            var grade = ShopApplication.GetShopGrade(shop.GradeId);
            model.GradeName = grade.Name;
            model.ImageLimit = grade.ImageLimit;
            model.ProductLimit = grade.ProductLimit;

            //续费时间
            List<SelectListItem> yearList = new List<SelectListItem> { };
            yearList.Add(new SelectListItem() { Selected = true, Text = "一年", Value = "1" });
            yearList.Add(new SelectListItem() { Selected = false, Text = "两年", Value = "2" });
            yearList.Add(new SelectListItem() { Selected = false, Text = "三年", Value = "3" });
            yearList.Add(new SelectListItem() { Selected = false, Text = "四年", Value = "4" });
            yearList.Add(new SelectListItem() { Selected = false, Text = "五年", Value = "5" });
            ViewBag.YearList = yearList;

            //可升级套餐
            List<SelectListItem> gradeList = new List<SelectListItem>() { new SelectListItem() { Selected = true, Text = "请选择升级套餐", Value = "0" } };
            var enableGrade = _iShopService.GetShopGrades().Where(c => c.ChargeStandard >= grade.ChargeStandard && c.Id != shop.GradeId);//要排除自己
            foreach (var item in enableGrade)
            {
                gradeList.Add(new SelectListItem() { Selected = false, Text = item.Name, Value = item.Id.ToString() });
            }

            ViewBag.GradeList = gradeList;
            ViewBag.HasOver = shop.EndDate <= DateTime.Now;
            var shopAccount = ShopApplication.GetShopAccount(CurrentSellerManager.ShopId);
            ViewBag.ShopAccountAmount = shopAccount.Balance.ToString("F2");

            return View(model);
        }

        [HttpPost]
        public JsonResult GetInfoAfterTimeSelect(int year)
        {
            var shopInfo = _iShopService.GetShop(CurrentSellerManager.ShopId);
            var shopGrade = _iShopService.GetShopGrades().Where(c => c.Id == shopInfo.GradeId).FirstOrDefault();
            DateTime endtime = shopInfo.EndDate;
            if (shopInfo.EndDate < DateTime.Now)
                endtime = DateTime.Now;
            string newEndTime = endtime.AddYears(year).ToString("yyyy/MM/dd");
            string amount = (shopGrade.ChargeStandard * year).ToString("F2");
            return Json(new { success = true, amount = amount, endtime = newEndTime });
        }

        [HttpPost]
        public JsonResult GetInfoAfterGradeSelect(int grade)
        {
            var shopInfo = _iShopService.GetShop(CurrentSellerManager.ShopId);
            var shopGrade = _iShopService.GetShopGrades().Where(c => c.Id == shopInfo.GradeId).FirstOrDefault();
            var newGrade = _iShopService.GetShopGrades().Where(c => c.Id == (long)grade).FirstOrDefault();

            //差价
            decimal spread = newGrade.ChargeStandard - shopGrade.ChargeStandard;
            DateTime endTime = shopInfo.EndDate;
            string amount = (endTime.Subtract(DateTime.Now).Days * spread / 365).ToString("F2");
            string newGradeInfo = "可发布商品 " + newGrade.ProductLimit + "个，使用图片空间 " + newGrade.ImageLimit + "M";

            return Json(new { success = true, amount = amount, gradeTip = newGradeInfo });
        }

        public JsonResult PaymentList(decimal balance, int type, int value)
        {
            string webRoot = CurrentUrlHelper.CurrentUrlNoPort();

            //获取同步返回地址
            string returnUrl = webRoot + "/SellerAdmin/Shop/ReNewPayReturn/{0}?balance={1}";

            //获取异步通知地址
            string payNotify = webRoot + "/pay/ReNewPayNotify/{0}/?str={1}";

            var payments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(PlatformType.PC));

            const string RELATEIVE_PATH = "/Plugins/Payment/";

            //不重复数字
            string ids = DateTime.Now.ToString("yyyyMMddmmss") + CurrentSellerManager.ShopId.ToString();

            var models = payments.Select(item =>
            {
                string requestUrl = string.Empty;
                try
                {
                    string strPaymentId = EncodePaymentId(item.PluginInfo.PluginId);//异步是Id参数
                    string strstr = balance + "-" + CurrentSellerManager.UserName + "-" + CurrentSellerManager.ShopId + "-" + type + "-" + value;//异步是str参数
                    string strreturnUrl = string.Format(returnUrl, strPaymentId, balance);
                    string strpayNotify = string.Format(payNotify, strPaymentId, strstr);

                    #region 当微信扫描支付特殊处理参数：//在微信扫描支付用“?”参数传不过来，则第二个参数用“~”隔开传
                    if (strPaymentId.ToLower().IndexOf("weixinpay_native") != -1)
                    {
                        //在微信扫描支付?后参数不传过来，则第二个参数用“~”分开传
                        strpayNotify = string.Format(webRoot + "/pay/ReNewPayNotify/{0}", strPaymentId + "~" + strstr);
                    }
                    //Log.Error("strreturnUrl:" + strreturnUrl);
                    //Log.Error("strpayNotify:" + strpayNotify);
                    #endregion

                    requestUrl = item.Biz.GetRequestUrl(strreturnUrl, strpayNotify, ids, balance, "店铺续费");

                    #region //微信扫描支付它原本requestUrl是作为url参数，在url参数：加上了orderids参数特殊处理下
                    if (strPaymentId.ToLower().IndexOf("weixinpay_native") != -1)
                    {
                        requestUrl = HttpUtility.UrlEncode(requestUrl) + "&orderIds=" + ids + "&type=shop";
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Core.Log.Error("支付页面加载支付插件出错", ex);
                }
                return new PaymentModel()
                {
                    Logo = RELATEIVE_PATH + item.PluginInfo.ClassFullName.Split(',')[1] + "/" + item.Biz.Logo,
                    RequestUrl = requestUrl,
                    UrlType = item.Biz.RequestUrlType,
                    Id = item.PluginInfo.PluginId
                };
            });
            models = models.OrderByDescending(d => d.Id).Where(item => !string.IsNullOrEmpty(item.RequestUrl) && item.Id != "Mall.Plugin.Payment.WeiXinPay");//只选择正常加载的插件 && item.Id != "Mall.Plugin.Payment.WeiXinPay_Native"
            return Json(models);
        }

        /// <summary>
        /// 检测公司名是否重复
        /// </summary>
        /// <param name="companyName"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult CheckCompanyName(string companyNameT)
        {
            var exist = ShopApplication.ExistCompanyName(companyNameT.Trim(), CurrentSellerManager.ShopId);
            return Json(!exist);
        }

        /// <summary>
        /// 检测营业执照号是否重复
        /// </summary>
        /// <param name="BusinessLicenceNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult CheckBusinessLicenceNumber(string BusinessLicenceNumberT)
        {
            var exist = ShopApplication.ExistBusinessLicenceNumber(BusinessLicenceNumberT.Trim(), CurrentSellerManager.ShopId);
            return Json(!exist);
        }

        /// <summary>
        /// 检测营业执照号是否重复
        /// </summary>
        /// <param name="BusinessLicenceNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult CheckBusinessLicenceNumbers(string BusinessLicenceNumber)
        {
            var exist = ShopApplication.ExistBusinessLicenceNumber(BusinessLicenceNumber.Trim(), CurrentSellerManager.ShopId);
            return Json(!exist);
        }

        string EncodePaymentId(string paymentId)
        {
            return paymentId.Replace(".", "-");
        }

        string DecodePaymentId(string paymentId)
        {
            return paymentId.Replace("-", ".");
        }

        public ActionResult ReNewPayReturn(string id, decimal balance)
        {
            id = DecodePaymentId(id);
            string errorMsg = string.Empty;

            try
            {
                var payment = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id);
                var payInfo = payment.Biz.ProcessReturn(httpContextAccessor);
                
                bool result = Cache.Get<bool>(CacheKeyCollection.PaymentState(string.Join(",", payInfo.OrderIds)));
                if (!result)
                {
                    throw new Exception("支付未成功");

                    #region  "废弃,因为参数不足，无法在这里处理续费逻辑"
                    ////添加店铺续费记录
                    //model.ShopId = CurrentSellerManager.ShopId;
                    //model.OperateDate = DateTime.Now;
                    //model.Operator = CurrentSellerManager.UserName;
                    //model.Amount = balance;
                    ////续费操作
                    //if (type == 1)
                    //{

                    //    model.OperateType = ShopRenewRecord.EnumOperateType.ReNew;
                    //    var shopInfo = _iShopService.GetShop(CurrentSellerManager.ShopId);
                    //    string strNewEndTime = shopInfo.EndDate.Value.AddYears(value).ToString("yyyy-MM-dd");
                    //    model.OperateContent = "续费 " + value + " 年至 " + strNewEndTime;
                    //    _iShopService.AddShopRenewRecord(model);

                    //    //店铺续费
                    //    _iShopService.ShopReNew(CurrentSellerManager.ShopId, value);
                    //}
                    ////升级操作
                    //else
                    //{
                    //    model.ShopId = CurrentSellerManager.ShopId;
                    //    model.OperateType = ShopRenewRecord.EnumOperateType.Upgrade;
                    //    var shopInfo = _iShopService.GetShop(CurrentSellerManager.ShopId);
                    //    var shopGrade = _iShopService.GetShopGrades().Where(c => c.Id == shopInfo.GradeId).FirstOrDefault();
                    //    var newshopGrade = _iShopService.GetShopGrades().Where(c => c.Id == (long)value).FirstOrDefault();
                    //    model.OperateContent = "将套餐‘" + shopGrade.Name + "'升级为套餐‘" + newshopGrade.Name + "'";
                    //    _iShopService.AddShopRenewRecord(model);

                    //    //店铺升级
                    //    _iShopService.ShopUpGrade(CurrentSellerManager.ShopId, (long)value);
                    //}



                    ////写入支付状态缓存
                    //string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", payInfo.OrderIds));//获取支付状态缓存键
                    //Cache.Insert(payStateKey, true);//标记为已支付
                    #endregion
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            ViewBag.Error = errorMsg;
            return View();
        }

        public ActionResult ShopRenewRecords()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Recordlist(int page, int rows)
        {
            ShopQuery query = new ShopQuery()
            {
                BrandId = CurrentSellerManager.ShopId,
                PageNo = page,
                PageSize = rows
            };

            QueryPageModel<ShopRenewRecordInfo> accounts = _iShopService.GetShopRenewRecords(query);
            IList<ShopRecordModel> models = new List<ShopRecordModel>();
            foreach (var item in accounts.Models.ToArray())
            {
                ShopRecordModel model = new ShopRecordModel();
                model.Id = (int)item.Id;
                model.OperateType = item.OperateType.ToDescription();
                model.OperateDate = item.OperateDate.ToString("yyyy-MM-dd HH:mm");
                model.Operate = item.Operator;
                model.Content = item.OperateContent;
                model.Amount = item.Amount;
                models.Add(model);
            }
            return Json(new { rows = models, total = accounts.Total });
        }

        public ActionResult ShopOverview()
        {
            Mall.Web.Models.ShopModel result = new ShopModel();
            return View(result);
        }

        [HttpPost]
        public ActionResult UpRenew(ShopRenewRecordInfo model)
        {
            if (model == null)
                return Json(new Result() { success = false, msg = "更新对象为空" });

            model.OperateType = ShopRenewRecordInfo.EnumOperateType.Upgrade;
            model.Operator = CurrentSellerManager.UserName;
            model.OperateDate = DateTime.Now;

            if (model.Type == ShopRenewRecordInfo.EnumOperateType.ReNew)
            {
                model.OperateType = ShopRenewRecordInfo.EnumOperateType.ReNew;
                var shopInfo = _iShopService.GetShop(model.ShopId);
                if (shopInfo != null)
                {
                    DateTime beginTime = shopInfo.EndDate;
                    if (beginTime < DateTime.Now)
                        beginTime = DateTime.Now;
                    string strNewEndTime = beginTime.AddYears(model.Year).ToString("yyyy-MM-dd");
                    model.OperateContent = "续费 " + model.Year + " 年至 " + strNewEndTime;
                    _iShopService.AddShopRenewRecord(model);
                    _iShopService.ShopReNew(model.ShopId, model.Year); //店铺续费
                    ShopApplication.ClearCacheShop(CurrentSellerManager.ShopId);
                    return Json(new Result() { success = true });
                }
            }
            else
            {
                var shopInfo = _iShopService.GetShop(model.ShopId);
                if (shopInfo != null)
                {
                    var shopGrade = _iShopService.GetShopGrades().Where(c => c.Id == shopInfo.GradeId).FirstOrDefault();
                    var newshopGrade = _iShopService.GetShopGrades().Where(c => c.Id == model.GradeId).FirstOrDefault();
                    model.OperateContent = "将套餐‘" + shopGrade.Name + "'升级为套餐‘" + newshopGrade.Name + "'";
                    _iShopService.AddShopRenewRecord(model);
                    _iShopService.ShopUpGrade(model.ShopId, model.GradeId); //店铺升级
                    ShopApplication.ClearCacheShop(CurrentSellerManager.ShopId);
                    return Json(new Result() { success = true });
                }
            }
            return Json(new Result() { success = false, msg = "操作失败" });
        }


        public JsonResult AccountBalancePay(decimal balance, int type, int value)
        {
            if(balance <=0)
                return Json(new Result() { success = false, msg = "应缴金额不正确" });

            var shopAccount = ShopApplication.GetShopAccount(CurrentSellerManager.ShopId);
            if (shopAccount == null)
                return Json(new Result() { success = false, msg = "店铺账户信息异常" });

            if (shopAccount.Balance < balance) //店铺余额不足以支付费用
            {
                return Json(new Result() { success = false, msg = "您的店铺余额为：" + shopAccount.Balance + "元,不足以支付此次费用，请先充值。" });
            }

            string trandNo = shopAccount.ShopId + "" + new Random().Next(1000000);
            ShopApplication.ShopReNewPayNotify(trandNo, CurrentSellerManager.ShopId, CurrentSellerManager.UserName, balance, type, value, true);
            ShopApplication.ClearCacheShop(CurrentSellerManager.ShopId);
            return Json(new Result() { success = true });
        }
    }
}