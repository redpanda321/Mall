using Mall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;



using Mall.IServices;
using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Entities;
using Mall.DTO.QueryModel;
using System.Configuration;
using Mall.Core.Helper;
using Mall.Application;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class HomeController : BaseSellerController
    {
       private IShopService _iShopService;
       private IArticleService _iArticleService;
       private IBrandService _iBrandService;
       private ICommentService _iCommentService;
       private IProductService _iProductService;
       private IManagerService _iManagerService;
       private IStatisticsService _iStatisticsService;
        public HomeController(
            IShopService iShopService,
            IArticleService iArticleService,
            IBrandService iBrandService,
            ICommentService iCommentService,
            IProductService iProductService,
            IManagerService iManagerService,
            IStatisticsService iStatisticsService
            )
        {
            _iShopService = iShopService;
            _iArticleService = iArticleService;
            _iBrandService = iBrandService;
            _iCommentService = iCommentService;
            _iProductService = iProductService;
            _iManagerService = iManagerService;
            _iStatisticsService = iStatisticsService;

        }
        private bool IsInstalled()
        {
            // var t = ConfigurationManager.AppSettings["IsInstalled"];
            // return null == t || bool.Parse(t);

            bool t = configuration.GetValue<bool>("Mall:IsInstalled");
            return t;


        }

       

        [UnAuthorize]
        public ActionResult Index()
        {
          
            return RedirectToAction("Console");
        }
        [UnAuthorize]
        public ActionResult Console()
        {
            //新首页开始
            HomeModel model = new HomeModel();
            var shopId = CurrentSellerManager.ShopId;
            /*店铺信息*/
            var shop = ShopApplication.GetShop(shopId);
            if (shop != null)
            {
                model.SellerConsoleModel = StatisticApplication.GetSellerConsoleModel(shopId);
                /*公告*/
                model.Articles = _iArticleService.GetTopNArticle<ArticleInfo>(6, 4);
                ViewBag.Logo = SiteSettingApplication.SiteSettings.MemberLogo;
                model.ShopId = shop.Id;
                model.ShopLogo = shop.Logo;
                model.ShopName = shop.ShopName;
                model.ShopEndDate = shop.EndDate.HasValue ? shop.EndDate.Value.ToString("yyyy-MM-dd") : string.Empty;
                model.ShopGradeName = model.SellerConsoleModel.ShopGrade;

                var shopStatisticOrderComments = _iShopService.GetShopStatisticOrderComments(CurrentSellerManager.ShopId);
                var productAndDescription = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentInfo.EnumCommentKey.ProductAndDescription).FirstOrDefault();
                var sellerServiceAttitude = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentInfo.EnumCommentKey.SellerServiceAttitude).FirstOrDefault();
                var sellerDeliverySpeed = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentInfo.EnumCommentKey.SellerDeliverySpeed).FirstOrDefault();

                var defaultValue = "5";
                //宝贝与描述
                model.ProductAndDescription = productAndDescription != null ? string.Format("{0:F}", productAndDescription.CommentValue) : defaultValue;
                //卖家服务态度
                model.SellerServiceAttitude = sellerServiceAttitude != null ? string.Format("{0:F}", sellerServiceAttitude.CommentValue) : defaultValue;
                //卖家发货速度
                model.SellerDeliverySpeed = sellerDeliverySpeed != null ? string.Format("{0:F}", sellerDeliverySpeed.CommentValue) : defaultValue;
                //所有商品数
                model.ProductsNumberIng = model.SellerConsoleModel.ProductLimit.ToString();
                //发布商品数量
                model.ProductsNumber = model.SellerConsoleModel.ProductsCount.ToString();
                //使用空间
                model.UseSpace = model.SellerConsoleModel.ImageLimit.ToString();
                //正使用的空间
                model.UseSpaceing = model.SellerConsoleModel.ProductImages.ToString();
                //商品咨询
                model.OrderProductConsultation = model.SellerConsoleModel.ProductConsultations.ToString();
                //订单总数
                model.OrderCounts = model.SellerConsoleModel.OrderCounts.ToString();
                //待买家付款
                model.OrderWaitPay = model.SellerConsoleModel.WaitPayTrades.ToString();
                //待发货
                model.OrderWaitDelivery = model.SellerConsoleModel.WaitDeliveryTrades.ToString();
                //待回复评价
                model.OrderReplyComments = model.SellerConsoleModel.ProductComments.ToString();
                //待处理投诉
                model.OrderHandlingComplaints = model.SellerConsoleModel.Complaints.ToString();
                //待处理退款
                model.OrderWithRefund = model.SellerConsoleModel.RefundTrades.ToString();
                //待处理退货
                model.OrderWithRefundAndRGoods = model.SellerConsoleModel.RefundAndRGoodsTrades.ToString();
                //商品评价
                model.ProductsEvaluation = model.SellerConsoleModel.ProductsEvaluation.ToString();
                //授权品牌
                model.ProductsBrands =_iBrandService.GetShopBrandApplys(CurrentSellerManager.ShopId).Where(c => c.AuditStatus == 1).Count().ToString();
                //出售中
                model.ProductsOnSale = model.SellerConsoleModel.OnSaleProducts.ToString();
                //草稿箱
                model.ProductsInDraft = model.SellerConsoleModel.ProductsInDraft.ToString();
                //待审核
                model.ProductsWaitForAuditing = model.SellerConsoleModel.WaitForAuditingProducts.ToString();
                //审核未通过
                model.ProductsAuditFailed = model.SellerConsoleModel.AuditFailureProducts.ToString();
                //违规下架
                model.ProductsInfractionSaleOff = model.SellerConsoleModel.InfractionSaleOffProducts.ToString();
                //仓库中
                model.ProductsInStock = model.SellerConsoleModel.InStockProducts.ToString();
                //警戒库存数
                model.OverSafeStockProducts = ProductManagerApplication.GetOverSafeStockProducts(CurrentSellerManager.ShopId).ToString();
                DateTime startDate = DateTime.Now.AddDays(-1).Date;
                DateTime endDate = startDate.AddDays(1).AddMilliseconds(-1);
                var statistic = StatisticApplication.GetShopTradeStatistic(CurrentSellerManager.ShopId, null, startDate, endDate);
                var lstEchartsData = new List<EchartsData>();
                if (statistic != null)
                {
                    ViewBag.VistiCounts = statistic.VisitCounts;
                    ViewBag.OrderCounts = statistic.OrderCount;
                    ViewBag.OrderPayCount = statistic.OrderPayCount;
                }
                else
                {
                    string zero = decimal.Zero.ToString();
                    ViewBag.VistiCounts = zero;
                    ViewBag.OrderCounts = zero;
                    ViewBag.OrderPayCount = zero;
                }
            }
            //新首页结束
            return View(model);
        }

        [UnAuthorize]
        [HttpGet]
        public ActionResult ProductRecentMonthSaleRank()
        {
            var shop = CurrentSellerManager.ShopId;
            var end = DateTime.Now;
            var begin = end.AddMonths(-1);
            var model = StatisticApplication.GetProductSaleRankingChart(shop,begin,end,CommonModel.SaleDimension.Amount,15);
            return Json(new { success = true, chart = model });
        }

        [UnAuthorize]
        [HttpGet]
        public ActionResult AnalysisEffectShop()
        {
            DateTime startDate = DateTime.Now.AddDays(-1).Date;
            DateTime endDate = startDate.AddDays(1).AddMilliseconds(-1);
            ShopInfo.ShopVistis shopVistis = _iShopService.GetShopVistiInfo(startDate, endDate, CurrentSellerManager.ShopId);
            IList<EchartsData> lstEchartsData = new List<EchartsData>();
            if (shopVistis != null)
            {
                lstEchartsData.Add(new EchartsData { name = "访问次数", value = shopVistis.VistiCounts.ToString() });
                lstEchartsData.Add(new EchartsData { name = "下单次数", value = shopVistis.OrderCounts.ToString() });
                lstEchartsData.Add(new EchartsData { name = "成交笔数", value = shopVistis.OrderPayCount.ToString() });
            }
            else
            {
                string zero = decimal.Zero.ToString();
                lstEchartsData.Add(new EchartsData { name = "访问次数", value = zero });
                lstEchartsData.Add(new EchartsData { name = "下单次数", value = zero });
                lstEchartsData.Add(new EchartsData { name = "成交笔数", value = zero });
            }

            return Json(new { success = true, chart = lstEchartsData });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult ChangePassword(string oldpassword, string password)
        {
            if (string.IsNullOrWhiteSpace(oldpassword) || string.IsNullOrWhiteSpace(password))
            {
                return Json(new Result() { success = false, msg = "密码不能为空！" });
            }
            var model = CurrentSellerManager;
            var pwd = SecureHelper.MD5(SecureHelper.MD5(oldpassword) + model.PasswordSalt);
            if (pwd == model.Password)
            {
                _iManagerService.ChangeSellerManagerPassword(CurrentSellerManager.Id, CurrentSellerManager.ShopId, password, CurrentSellerManager.RoleId);
                return Json(new Result() { success = true, msg = "修改成功" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "旧密码错误" });
            }
        }

        [UnAuthorize]
        public JsonResult CheckOldPassword(string password)
        {
            var model = CurrentSellerManager;
            var pwd = SecureHelper.MD5(SecureHelper.MD5(password) + model.PasswordSalt);
            if (model.Password == pwd)
            {
                return Json(new Result() { success = true });
            }
            return Json(new Result() { success = false });
        }

        #region LOGO图片设置(home里与PageSettingsController写的SetLogo是一样，没共用是因为当没开通PC端时PageSettingsController是访问不了的)

        [HttpPost]
        [UnAuthorize]
        public JsonResult SetLogo(string logo)
        {
            var img = MoveImages(logo);
            _iShopService.UpdateLogo(CurrentSellerManager.ShopId, img);
            ClearCache();//Logo涉及店铺首页清下缓存
            return Json(new { success = true, logo = Mall.Core.MallIO.GetImagePath(img) });
        }


        /// <summary>
        /// 转移LOGO图片
        /// </summary>
        /// <param name="id"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        private string MoveImages(string image)
        {
            if (string.IsNullOrWhiteSpace(image))
            {
                return "";
            }
            string ImageDir = string.Empty;
            var dir = string.Format("/Storage/Shop/{0}/ImageAd/", CurrentSellerManager.ShopId);

            if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                var source = image.Substring(image.LastIndexOf("/temp/"));
                Core.MallIO.CopyFile(source, dir + "logo.png", true);
            }  //目标地址
            return dir + "logo.png";
        }

        public void ClearCache()
        {
         //   HttpResponse.RemoveOutputCacheItem(string.Format("/Shop/Home/{0}", this.CurrentShop.Id)); //移除页面缓存
          //  HttpResponse.RemoveOutputCacheItem(string.Format("/shop/home/{0}", this.CurrentShop.Id));
        }
        #endregion
    }
}