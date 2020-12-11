using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

using System.Drawing;
using Newtonsoft.Json;
using Mall.Core;
using Mall.IServices;

using Mall.CommonModel;

using Mall.DTO;
using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Web.Framework;
using Mall.Application;
using Mall.DTO.QueryModel;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    [MarketingAuthorization]
    /// <summary>
    /// 拼团
    /// </summary>
    public class FightGroupController : BaseSellerController
    {
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private long CurShopId { get; set; }
        public FightGroupController(ILimitTimeBuyService iLimitTimeBuyService)
        {
            //退出登录后，直接进入controller异常处理
            if (CurrentSellerManager != null)
            {
                CurShopId = CurrentSellerManager.ShopId;
            }
            _iLimitTimeBuyService = iLimitTimeBuyService;
        }

        ///// <summary>
        ///// 重写验证，以过滤未购买服务或已过期情况
        ///// </summary>
        ///// <param name="filterContext"></param>
        //protected override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    //路由处理
        //    var route = filterContext.RouteData;
        //    //string controller = route.Values["controller"].ToString().ToLower();
        //    string action = route.Values["action"].ToString().ToLower();
        //    if (action.ToLower() != "nosetting")
        //    {
        //        bool iscanmarket = FightGroupApplication.IsCanUseMarketService(CurShopId);
        //        if (!iscanmarket && action.ToLower() != "buymarketservice")
        //        {
        //            if (FightGroupApplication.IsOpenMarketService())
        //            {
        //                filterContext.Result = RedirectToAction("BuyMarketService");
        //                return;
        //            }
        //            else
        //            {
        //                filterContext.Result = RedirectToAction("Nosetting");
        //                return;
        //            }
        //        }
        //    }
        //    base.OnActionExecuting(filterContext);

        //}

        #region 活动列表
        /// <summary>
        /// 拼团管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var settings = MarketApplication.GetServiceSetting(MarketType.FightGroup);
            if (settings == null)
                return View("Nosetting");

            var market = MarketApplication.GetMarketService(CurShopId, MarketType.FightGroup);
            //未购买服务且列表刚进来则让进入购买服务页
            if ((market == null || market.Id <= 0) && Request.Query["first"].ToString() == "1")
            {
                return RedirectToAction("BuyMarketService");
            }

            ViewBag.Available = false;
            if (market != null && MarketApplication.GetServiceEndTime(market.Id) > DateTime.Now)
                ViewBag.Available = true;

            return View();
        }
        /// <summary>
        /// 获取活动列表
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="activeStatus"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult PostActiveList(FightGroupActiveQuery query)
        {
            query.ShopId = CurrentShop.Id;
            var data = FightGroupApplication.GetActives(query);
            return Json(data);
        }

        private string GetActiveShowUrl(long id)
        {
            string result = CurrentUrlHelper.CurrentUrlNoPort() + "/m-" + PlatformType.Wap.ToString() + "/FightGroup/Detail/" + id.ToString();
            return result;
        }
        /// <summary>
        /// 显示二维码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ShowActiveQRCode(long id)
        {
            string showurl = GetActiveShowUrl(id);
            Image map;
            map = Core.Helper.QRCodeHelper.Create(showurl);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);

            return File(ms.ToArray(), "image/png");
        }
        #endregion

        #region 拼团信息操作

        #region 添加拼团活动
        /// <summary>
        /// 添加拼团活动
        /// </summary>
        /// <returns></returns>
        public ActionResult AddActive()
        {
            FightGroupActiveModel model = new FightGroupActiveModel();
            model.ShopId = CurShopId;
            model.StartTime = DateTime.Now;
            model.EndTime = DateTime.Now.AddMonths(1);

            return View(model);
        }
        /// <summary>
        /// 保存拼团活动修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddActive(FightGroupActiveModel model)
        {
            var result = new Result { success = false, msg = "未知错误", status = -1 };
            FightGroupActiveModel data = new FightGroupActiveModel();
            if (model.EndTime.Date < DateTime.Now.Date)
            {
                throw new MallException("错误的结束时间");
            }
            if (model.EndTime.Date < model.StartTime.Date)
            {
                throw new MallException("错误的结束时间");
            }
            var market = MarketApplication.GetMarketService(CurrentSellerManager.ShopId, MarketType.FightGroup);
            if (market != null)
            {
                var endtime = MarketApplication.GetServiceEndTime(market.Id);
                var newEnd = DateTime.Parse(endtime.ToString("yyyy-MM-dd") + " 23:59:59");
                if (newEnd < model.EndTime.Date)
                {
                    throw new MallException(string.Format("活动结束时间不得超过服务到期时间，<br/>您的服务到期时间为{0}", newEnd.ToString("yyyy-MM-dd HH:mm:ss")));
                }
            }
            else
            {
                throw new MallException("您没有订购此服务");
            }
            //数据有效性验证
            model.CheckValidation();
            if (!FightGroupApplication.ProductCanJoinActive(model.ProductId))
            {
                throw new MallException("该商品已参与拼团或其他营销活动，请重新选择");
            }

            var skudata = FightGroupApplication.GetNewActiveItems(model.ProductId).skulist;
            foreach (var item in model.ActiveItems)
            {
                var cursku = skudata.FirstOrDefault(d => d.SkuId == item.SkuId);
                if (cursku != null)
                {
                    if (item.ActiveStock > cursku.ProductStock)
                    {
                        throw new MallException(item.SkuName + "错误的活动库存");
                    }
                }
                else
                {
                    model.ActiveItems.Remove(item);
                }
            }
            if (ModelState.IsValid)
            {
                TryUpdateModelAsync(data);
                data.ShopId = CurShopId;
                data.IconUrl = SaveActiveIcon(data.IconUrl);
                model = data;
                FightGroupApplication.AddActive(data);

                ProductManagerApplication.SaveCaculateMinPrice(model.ProductId, CurrentShop.Id);
                result = new Result { success = true, msg = "操作成功", status = 1 };
            }
            else
            {
                result = new Result { success = false, msg = "数据异常，请检查数据有效性", status = -1 };
            }
            return Json(result);
        }
        /// <summary>
        /// 获取规格信息
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetSkuList(long productId)
        {
            FightGroupGetSkuListModel result = new FightGroupGetSkuListModel();
            result = FightGroupApplication.GetNewActiveItems(productId);
            return Json(result);
        }
        /// <summary>
        /// 商品是否可以参加拼团活动
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CanAdd(long productId)
        {
            Result result = new Result { success = false, msg = "该商品正在参加拼团活动，无法同时参加拼团活动" };
            if (FightGroupApplication.ProductCanJoinActive(productId))
            {
                if (_iLimitTimeBuyService.IsAdd(productId))
                {
                    result.success = true;
                    result.msg = "";
                }
                else
                {
                    result = new Result { success = false, msg = "该商品正在参加限时购活动，无法同时参加拼团活动" };
                }
            }
            return Json(result);
        }
        #endregion

        #region 修改拼团活动
        /// <summary>
        /// 修改拼团活动
        /// </summary>
        /// <returns></returns>
        public ActionResult EditActive(long id)
        {
            FightGroupActiveModel model = FightGroupApplication.GetActive(id);
            if (model == null)
            {
                throw new MallException("错误的活动编号");
            }
            if (model.EndTime < DateTime.Now)
            {
                throw new MallException("已结束的活动不可以编辑");
            }
            return View(model);
        }
        /// <summary>
        /// 保存拼团活动修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditActive(FightGroupActiveModel model)
        {
            var result = new Result { success = false, msg = "未知错误", status = -1 };
            FightGroupActiveModel data = FightGroupApplication.GetActive(model.Id);
            if (data == null)
            {
                throw new MallException("错误的活动编号");
            }
            if (model.EndTime < DateTime.Now)
            {
                throw new MallException("错误的结束时间");
            }
            if (model.EndTime < model.StartTime)
            {
                throw new MallException("错误的结束时间");
            }
            var market = MarketApplication.GetMarketService(CurrentSellerManager.ShopId, MarketType.FightGroup);
            if (market != null)
            {
                var endtime = MarketApplication.GetServiceEndTime(market.Id);
                var newEnd = DateTime.Parse(endtime.ToString("yyyy-MM-dd") + " 23:59:59");
                if (newEnd < model.EndTime.Date)
                {
                    throw new MallException(string.Format("活动结束时间不得超过服务到期时间，<br/>您的服务到期时间为{0}", newEnd.ToString("yyyy-MM-dd HH:mm:ss")));
                }
            }
            else
            {
                throw new MallException("您没有订购此服务");
            }
            if (ModelState.IsValid)
            {
                TryUpdateModelAsync(data);

                model = data;
                //数据有效性验证
                model.CheckValidation();

                data.IconUrl = SaveActiveIcon(data.IconUrl);


                FightGroupApplication.UpdateActive(data);
                ProductManagerApplication.SaveCaculateMinPrice(model.ProductId, CurrentShop.Id);
                result = new Result { success = true, msg = "操作成功", status = 1 };
            }
            else
            {
                result = new Result { success = false, msg = "数据异常，请检查数据有效性", status = -1 };
            }
            return Json(result);
        }
        #endregion

        /// <summary>
        /// 保存活动图标
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private string SaveActiveIcon(string filepath)
        {
            string result = filepath;
            if (!string.IsNullOrWhiteSpace(filepath))
            {
                string dest = string.Format(@"/Storage/Shop/{0}/Market/", CurShopId);

                if (result.Contains("/temp/"))
                {
                    var d = result.Substring(result.LastIndexOf("/temp/"));

                    var destimg = Path.Combine(dest, Path.GetFileName(result));
                    Core.MallIO.CopyFile(d, destimg, true);
                    result = destimg;
                }
                else if (result.Contains("/Storage/"))
                {
                    result = result.Substring(result.LastIndexOf("/Storage/"));
                }
                else
                {
                    result = "";
                }
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 查看活动
        /// </summary>
        /// <param name="id">活动编号</param>
        /// <returns></returns>
        public ActionResult ViewActive(long id)
        {
            FightGroupActiveModel model = FightGroupApplication.GetActive(id);
            if (model == null)
            {
                throw new MallException("错误的活动编号");
            }
            if (model.ShopId != CurShopId)
            {
                throw new MallException("错误的活动编号");
            }
            model.ProductDefaultImage = MallIO.GetProductSizeImage(model.ProductImgPath, 1, ImageSize.Size_150.GetHashCode());
            return View(model);
        }
        /// <summary>
        /// 查看拼团情况
        /// </summary>
        /// <param name="id">活动编号</param>
        /// <returns></returns>
        public ActionResult ViewGroupList(long id)
        {
            var actobj = FightGroupApplication.GetActive(id, false, false);
            if (actobj == null)
            {
                throw new MallException("错误的活动编号");
            }
            if (actobj.ShopId != CurShopId)
            {
                throw new MallException("错误的活动编号");
            }
            ViewBag.ActionId = id;
            return View();
        }

        [HttpPost]
        public JsonResult PostGroupList(long actionId, FightGroupBuildStatus? groupStatus, DateTime? startTime, DateTime? endTime, int page, int rows)
        {
            var actobj = FightGroupApplication.GetActive(actionId, false, false);
            if (actobj == null)
            {
                throw new MallException("错误的活动编号");
            }
            if (actobj.ShopId != CurShopId)
            {
                throw new MallException("错误的活动编号");
            }
            List<FightGroupBuildStatus> seastatus = new List<FightGroupBuildStatus>();
            if (groupStatus.HasValue)
            {
                seastatus.Add(groupStatus.Value);
            }
            var data = FightGroupApplication.GetGroups(actionId, seastatus, startTime, endTime, page, rows);
            return Json(new { rows = data.Models.ToList(), total = data.Total });
        }
        /// <summary>
        /// 购买营销服务
        /// </summary>
        /// <returns></returns>
        public ActionResult BuyMarketService()
        {
            var model = FightGroupApplication.GetMarketService(CurShopId);
            return View(model);
        }
        [HttpPost]
        public JsonResult BuyMarketService(int month)
        {
            Result result = new Result();
            FightGroupApplication.BuyMarketService(month, CurrentSellerManager.ShopId);
            result.success = true;
            result.msg = "购买服务成功";
            return Json(result);
        }
        /// <summary>
        /// 提前结束
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EndActive(long id)
        {
            Result result = new Result();
            var data = FightGroupApplication.GetActive(id, false, false);
            data.EndTime = DateTime.Now.AddMinutes(-1);
            FightGroupApplication.UpdateActive(data);
            ProductManagerApplication.SaveCaculateMinPrice(data.ProductId, CurrentShop.Id);
            result.success = true;
            result.msg = "操作成功";
            return Json(result);
        }

        /// <summary>
        /// 营销功能未开放
        /// </summary>
        /// <returns></returns>
        public ActionResult Nosetting()
        {
            return View("Nosetting");
        }
    }
}