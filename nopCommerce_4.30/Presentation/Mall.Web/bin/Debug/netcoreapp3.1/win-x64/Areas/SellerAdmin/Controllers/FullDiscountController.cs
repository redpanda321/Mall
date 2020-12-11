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
using Mall.Web.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    [MarketingAuthorization]
    /// <summary>
    /// 满减
    /// </summary>
    public class FullDiscountController : BaseSellerController
    {
        private long CurShopId { get; set; }
        public FullDiscountController()
        {
            //退出登录后，直接进入controller异常处理
            if (CurrentSellerManager != null)
            {
                CurShopId = CurrentSellerManager.ShopId;
            }
        }

        #region 活动列表
        /// <summary>
        /// 满减列表管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var settings = MarketApplication.GetServiceSetting(MarketType.FullDiscount);
            if (settings == null)
                return View("Nosetting");

            var market = MarketApplication.GetMarketService(CurShopId, MarketType.FullDiscount);
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
        public JsonResult PostActiveList(string activeName, FullDiscountStatus? status, DateTime? startTime, DateTime? endTime, int page, int rows)
        {
            if (startTime.HasValue)
            {
                startTime = startTime.Value.Date;
            }
            if (endTime.HasValue)
            {
                endTime = endTime.Value.Date;
            }
            var query = new FullDiscountActiveQuery();
            query.ShopId = CurShopId;
            query.ActiveName = activeName;
            query.Status = status;
            query.StartTime = startTime;
            query.EndTime = endTime;
            query.PageNo = page;
            query.PageSize = rows;
            var data = FullDiscountApplication.GetActives(query);
            var datalist = data.Models.ToList();
            return Json(new { rows = datalist, total = data.Total });
        }
        /// <summary>
        /// 删除活动
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult PostActiveDelete(long id)
        {
            FullDiscountApplication.DeleteActive(id);
            return Json(new Result { success = true, msg = "删除成功" });
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

        #region 信息操作

        #region 添加满减活动
        /// <summary>
        /// 添加满减活动
        /// </summary>
        /// <returns></returns>
        public ActionResult AddActive()
        {
            FullDiscountActiveModel model = new FullDiscountActiveModel();
            model.ShopId = CurShopId;
            model.StartTime = DateTime.Now;
            model.EndTime = DateTime.Now.AddMonths(1);
            model.IsAllProduct = true;
            model.RuleJSON = "";

            var sermodel = FullDiscountApplication.GetMarketService(CurShopId);
            model.EndServerTime = sermodel.EndTime.Value.Date.AddDays(1).AddMinutes(-1);

            return View(model);
        }
        /// <summary>
        /// 保存满减活动添加
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddActive(FullDiscountActiveModel model)
        {
            var result = new Result { success = false, msg = "未知错误", status = -1 };
            if (string.IsNullOrWhiteSpace(model.ActiveName))
            {
                model.ActiveName = model.ActiveName.Trim();
            }
            FullDiscountActive data = new FullDiscountActive();
            if (model.EndTime.Date < model.StartTime.Date)
            {
                throw new MallException("错误的结束时间");
            }
            //数据有效性验证
            model.CheckValidation();
            List<FullDiscountRules> rules = JsonConvert.DeserializeObject<List<FullDiscountRules>>(model.RuleJSON);
            if (rules == null)
            {
                throw new MallException("优惠规则异常");
            }
            rules = rules.OrderBy(d => d.Quota).ToList();
            CheckRules(rules);
            List<long> proids = new List<long>();
            if (model.IsAllProduct)
            {
                proids.Add(-1);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(model.ProductIds))
                {
                    proids = model.ProductIds.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(d => long.Parse(d)).ToList();
                    List<long> cannotjoin = CheckCanNotJoinProduct(proids, 0);
                    if (cannotjoin.Count > 0)
                    {
                        result = new Result { success = false, msg = string.Join(",", cannotjoin.ToArray()), status = -2 };
                        return Json(result);
                    }
                }
            }
            var sermodel = FullDiscountApplication.GetMarketService(CurShopId);
            var newEnd = DateTime.Parse(sermodel.EndTime.Value.ToString("yyyy-MM-dd") + " 23:59:59");
            if (model.EndTime > newEnd)
            {
                throw new MallException(string.Format("活动结束时间不得超过服务到期时间，<br/>您的服务到期时间为{0}", newEnd.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            if (ModelState.IsValid)
            {
                List<FullDiscountActiveProduct> products = proids.Select(d => new FullDiscountActiveProduct { ProductId = d }).ToList();

                data.ActiveName = model.ActiveName;
                data.StartTime = model.StartTime;
                data.EndTime = model.EndTime;
                data.Id = model.Id;
                data.ShopId = CurShopId;
                data.IsAllProduct = model.IsAllProduct;
                data.Rules = rules;
                data.Products = products;
                FullDiscountApplication.AddActive(data);

                result = new Result { success = true, msg = "操作成功", status = 1 };
            }
            else
            {
                result = new Result { success = false, msg = "数据异常，请检查数据有效性", status = -1 };
            }
            return Json(result);
        }
        #endregion

        #region 修改满减活动
        /// <summary>
        /// 修改满减活动
        /// </summary>
        /// <returns></returns>
        public ActionResult EditActive(long id)
        {
            FullDiscountActive data = FullDiscountApplication.GetActive(id);
            FullDiscountActiveModel model = new FullDiscountActiveModel();
            if (data == null || data.ShopId != CurShopId)
            {
                throw new MallException("错误的活动编号");
            }
            model.Id = data.Id;
            model.ActiveName = data.ActiveName;
            model.StartTime = data.StartTime;
            model.EndTime = data.EndTime;
            model.IsAllProduct = data.IsAllProduct;
            model.RuleJSON = JsonConvert.SerializeObject(data.Rules);
            var proids = data.Products.Select(d => d.ProductId).ToArray();
            model.ProductIds = string.Join(",", proids);

            var sermodel = FullDiscountApplication.GetMarketService(CurShopId);
            model.EndServerTime = sermodel.EndTime.Value.Date.AddDays(1).AddMinutes(-1);

            return View(model);
        }
        /// <summary>
        /// 保存满减活动修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditActive(FullDiscountActiveModel model)
        {
            var result = new Result { success = false, msg = "未知错误", status = -1 };
            if (string.IsNullOrWhiteSpace(model.ActiveName))
            {
                model.ActiveName = model.ActiveName.Trim();
            }
            FullDiscountActive data = FullDiscountApplication.GetActive(model.Id);
            if (data == null || data.ShopId != CurShopId)
            {
                throw new MallException("错误的活动编号");
            }
            if (model.EndTime.Date < model.StartTime.Date)
            {
                throw new MallException("错误的结束时间");
            }
            //数据有效性验证
            model.CheckValidation();
            List<FullDiscountRules> rules = JsonConvert.DeserializeObject<List<FullDiscountRules>>(model.RuleJSON);
            if (rules == null)
            {
                throw new MallException("优惠规则异常");
            }
            rules = rules.OrderBy(d => d.Quota).ToList();
            CheckRules(rules);
            List<long> proids = new List<long>();
            if (model.IsAllProduct)
            {
                proids.Add(-1);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(model.ProductIds))
                {
                    proids = model.ProductIds.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(d => long.Parse(d)).ToList();
                    List<long> cannotjoin = CheckCanNotJoinProduct(proids, data.Id);
                    if (cannotjoin.Count > 0)
                    {
                        result = new Result { success = false, msg = string.Join(",", cannotjoin.ToArray()), status = -2 };
                        return Json(result);
                    }
                }
            }
            var sermodel = FullDiscountApplication.GetMarketService(CurShopId);
            if (model.EndTime > sermodel.EndTime.Value.Date.AddDays(1))
            {
                throw new MallException("活动结束时间不可超过服务时间");
            }

            if (ModelState.IsValid)
            {
                List<FullDiscountActiveProduct> products = proids.Select(d => new FullDiscountActiveProduct { ProductId = d }).ToList();

                data.Id = model.Id;
                data.ActiveName = model.ActiveName;
                data.EndTime = model.EndTime;
                data.ShopId = CurShopId;
                data.IsAllProduct = model.IsAllProduct;
                data.Rules = rules;
                data.Products = products;
                FullDiscountApplication.UpdateActive(data);

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
        /// 获取可以加入活动的商品列表
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="activeStatus"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCanJoinProducts(string name, string code, string selectedProductId, int activeId, int page, int rows, long? categoryId = null)
        {
            List<long> selproids = new List<long>();
            if (!string.IsNullOrWhiteSpace(selectedProductId))
            {
                selproids = selectedProductId.Split(',')
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .Select(d => long.Parse(d))
                    .ToList();
            }
            if (name != null)
            {
                name = name.Trim();
            }
            if (code != null)
            {
                code = code.Trim();
            }
            var products = FullDiscountApplication.GetCanJoinProducts(CurShopId, name, code, categoryId, true, selproids, activeId, page, rows);
            DataGridModel<ActiveProductModel> dataGrid = new DataGridModel<ActiveProductModel>()
            {
                rows = products.Models
                .Select(item => new ActiveProductModel()
                {
                    Name = item.ProductName,
                    Id = item.Id,
                    Image = item.GetImage(ImageSize.Size_50),
                    Price = item.MinSalePrice,
                    ProductCode = item.ProductCode
                }),
                total = products.Total
            };
            return Json(dataGrid);
        }

        [HttpPost]
        public JsonResult GetProductsByIds(string productids)
        {
            if (string.IsNullOrWhiteSpace(productids))
            {
                throw new MallException("错误的参数");
            }

            var selproids = productids.Split(',')
                 .Where(d => !string.IsNullOrWhiteSpace(d))
                 .Select(d => long.Parse(d))
                 .ToList();

            var products = ProductManagerApplication.GetProductByIds(selproids);
            var temp = products.Select(p => p.Id).ToList();
            var noproids = selproids.Where(p => !temp.Contains(p)).ToList();


            List<ActiveProductModel> result = products.Select(item =>
            {
                return new ActiveProductModel()
                {
                    Name = item.ProductName,
                    Id = item.Id,
                    Image = item.GetImage(ImageSize.Size_50),
                    Price = item.MinSalePrice,
                    ProductCode = item.ProductCode,
                    IsException = noproids.Any(d => d == item.Id)
                };
            }).ToList();
            return Json(result);
        }

        #endregion

        #region 营销系统
        
        /// <summary>
        /// 购买营销服务
        /// </summary>
        /// <returns></returns>
        public ActionResult BuyMarketService()
        {
            var model = FullDiscountApplication.GetMarketService(CurShopId);
            return View(model);
        }
        [HttpPost]
        public JsonResult BuyMarketService(int month)
        {
            Result result = new Result();
            FullDiscountApplication.BuyMarketService(month, CurrentSellerManager.ShopId);
            result.success = true;
            result.msg = "购买服务成功";
            return Json(result);
        }

        /// <summary>
        /// 未购买或已过期
        /// </summary>
        /// <returns></returns>
        public ActionResult NoBuyTips()
        {
            return View();
        }
        /// <summary>
        /// 营销功能未开放
        /// </summary>
        /// <returns></returns>
        public ActionResult Nosetting()
        {
            if (FullDiscountApplication.IsOpenMarketService())
            {
                return View("NoBuyTips");
            }
            return View("Nosetting");
        }
        #endregion

        #region 私有
        /// <summary>
        /// 检测优惠规则
        /// </summary>
        /// <param name="rules"></param>
        private void CheckRules(IEnumerable<FullDiscountRules> rules)
        {
            if (rules == null || rules.Count() < 1)
            {
                throw new MallException("优惠规则异常");
            }
            foreach (var item in rules)
            {
                if (rules.Count(d => d.Quota == item.Quota) > 1)
                {
                    throw new MallException("优惠规则重复");
                }
                if (item.Quota <= 0)
                {
                    throw new MallException("优惠规则异常：优惠门槛必须大于0");
                }
                if (item.Discount < 0)
                {
                    throw new MallException("优惠规则异常：优惠方式不可以小于0");
                }
                if (item.Discount > item.Quota)
                {
                    throw new MallException("优惠规则异常：优惠方式不可以大于门槛");
                }
                if (!Regex.IsMatch(item.Quota.ToString(), @"^\d+(\.\d{1,2})?$"))
                {
                    throw new MallException("优惠规则异常：门槛只可以保留二位小数");
                }
                if (!Regex.IsMatch(item.Discount.ToString(), @"^\d+(\.\d{1,2})?$"))
                {
                    throw new MallException("优惠规则异常：优惠方式只可以保留二位小数");
                }
            }
        }
        /// <summary>
        /// 检测不可以参加活动的商品
        /// </summary>
        /// <param name="productIds"></param>
        private List<long> CheckCanNotJoinProduct(IEnumerable<long> products, long activeId)
        {
            if (products == null || products.Count() < 1)
            {
                throw new MallException("请选择参与活动的商品");
            }
            List<long> result = products.ToList();
            var canjoin = FullDiscountApplication.FilterActiveProductId(products, activeId, CurrentShop.Id);
            result = result.Where(d => !canjoin.Contains(d)).ToList();
            return result;
        }
        #endregion
    }
}