using Mall.Application;
using Mall.CommonModel;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    [MarketingAuthorization]
    public class DistributionProductController : BaseSellerController
    {
        /// <summary>
        /// 当前shopid
        /// </summary>
        private long curshopid = 0;
        /// <summary>
        /// 参数是否正确
        /// </summary>
        private bool isConfigRight = false;
        public DistributionProductController()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            bool isclose = !SiteSettingApplication.SiteSettings.DistributionIsEnable;
            if (isclose && RouteData.Values["action"].ToString().ToLower() != "nosetting")
            {
                Response.Clear();
                //Response.BufferOutput = true;
                Response.Redirect(Url.Action("NoSetting"));
            }

            curshopid = CurrentSellerManager.ShopId;
            var dbkdata = DistributionApplication.GetDefaultBrokerageRate(curshopid);
            isConfigRight = true;
            if (dbkdata.BrokerageRate1 < 0.1m || dbkdata.BrokerageRate1 > 100)
            {
                isConfigRight = false;
            }
            if (SiteSettings.DistributionMaxLevel > 1 && (dbkdata.BrokerageRate2 < 0.1m || dbkdata.BrokerageRate2 > 100))
            {
                isConfigRight = false;
            }
            if (SiteSettings.DistributionMaxLevel > 2 && (dbkdata.BrokerageRate3 < 0.1m || dbkdata.BrokerageRate3 > 100))
            {
                isConfigRight = false;
            }
            if (SiteSettings.DistributionMaxBrokerageRate < (dbkdata.BrokerageRate1 + dbkdata.BrokerageRate2 + dbkdata.BrokerageRate3))
            {
                isConfigRight = false;
            }
        }
        // GET: SellerAdmin/Coupon
        public ActionResult Management()
        {
            if (!isConfigRight)
            {
                return RedirectToAction("DefaultBrokerage");
            }
            return View(SiteSettingApplication.SiteSettings);
        }

        /// <summary>
        /// 获取分销商品
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetProductList(string skey, int rows, int page, long categoryId = 0)
        {
            //查询条件
            DistributionProductQuery query = new DistributionProductQuery();
            query.ProductName = skey;
            query.PageSize = rows;
            query.PageNo = page;
            query.Status = DistributionProductStatus.Normal;
            query.ShopId = curshopid;
            if (categoryId != 0)
            {
                query.CategoryId = categoryId;
            }
            query.isShopCategory = true;

            var data = DistributionApplication.GetProducts(query);
            var datalist = new List<DistributionProductListModel>();
            if (data.Models != null)
            {
                var products = ProductManagerApplication.GetProductsByIds(data.Models.Select(p => p.ProductId).ToList());
                var categories = CategoryApplication.GetCategories();
                foreach (var item in data.Models)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    var categorie = categories.FirstOrDefault(p => p.Id == product.Id);
                    datalist.Add(new DistributionProductListModel
                    {
                        BrokerageId = item.Id,
                        ProductId = item.ProductId,
                        ProductName = product.ProductName,
                        CategoryId = product.CategoryId,
                        CategoryName = GetProductShopCategorieName(product.Id),
                        ShopId = item.ShopId,
                        BrokerageRate1 = item.BrokerageRate1,
                        BrokerageRate2 = item.BrokerageRate2,
                        BrokerageRate3 = item.BrokerageRate3,
                        MaxBrokerage = item.MaxBrokerage,
                        MaxSalePrice = item.MaxSalePrice,
                        MinSalePrice = item.MinSalePrice,
                        NoSettlementAmount = item.NoSettlementAmount,
                        ProductStatus = item.ProductStatus,
                        SaleAmount = item.SaleAmount,
                        SaleCount = item.SaleCount,
                        SettlementAmount = item.SettlementAmount,
                        ShopName = item.ShopName
                    });
                }
            }
            var result = new { rows = datalist, total = data.Total };
            return Json(result);
        }

        private string GetProductShopCategorieName(long id)
        {
            var shopCategory = Mall.Application.ShopCategoryApplication.GetCategorysByProductId(id);

            var name = shopCategory.Select(s =>
            {
                return string.IsNullOrEmpty(s.ShopCategoryName) ? null : s.ShopCategoryName;
            }).ToArray();

            return string.Join(",", name);
        }

        /// <summary>
        /// 获取分销的商品编号
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetAllProductIds()
        {
            List<long> proids = new List<long>();
            proids = DistributionApplication.GetAllDistributionProductIds(curshopid);
            return Json(proids);
        }
        /// <summary>
        /// 增加分销商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddProducts(string ids)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            bool isDataOk = true;
            List<long> productids = new List<long>();
            if (!isConfigRight)
            {
                isDataOk = false;
                result = new Result { success = false, msg = "错误的默认佣金比，请设置默认佣金比" };
            }
            if (isDataOk)
            {
                if (!Regex.IsMatch(ids, @"^(\d|,)+?"))
                {
                    isDataOk = false;
                    result = new Result { success = false, msg = "错误的商品编号列表" };
                }
            }
            //组装id数据
            if (isDataOk)
            {
                string[] _arrid = ids.Split(',');
                foreach (string item in _arrid)
                {
                    long _proid = 0;
                    if (long.TryParse(item, out _proid))
                    {
                        if (_proid > 0)
                        {
                            productids.Add(_proid);
                        }
                    }
                }
                if (productids.Count < 1)
                {
                    isDataOk = false;
                    result = new Result { success = false, msg = "错误的商品编号列表" };
                }
                else
                {
                    DistributionApplication.AddSpreadProducts(productids, curshopid);
                    result = new Result { success = true, msg = "添加分销商品成功" };
                }
            }
            return Json(result);
        }
        /// <summary>
        /// 取消分销商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CancelProduct(long id)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            List<long> ids = new List<long>();
            if (id > 0)
                ids.Add(id);

            result = CancelProductCommon(ids);
            return Json(result);
        }

        /// <summary>
        /// 取消分销商品批量
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CancelProductList(string ids)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            if (string.IsNullOrEmpty(ids))
                return Json(result);

            List<long> productIds = new List<long>();
            foreach (string pid in ids.Split(','))
            {
                if (!string.IsNullOrEmpty(pid))
                    productIds.Add(long.Parse(pid));
            }

            result = CancelProductCommon(productIds);
            return Json(result);
        }

        /// <summary>
        /// 取消商品推广
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private Result CancelProductCommon(List<long> ids)
        {
            Result result = new Result { success = false, msg = "错误的商品编号" };
            if (ids.Count() > 0)
            {
                DistributionApplication.RemoveSpreadProducts(ids, curshopid);
                result = new Result { success = true, msg = "取消推广成功" };
            }

            return result;
        }

        /// <summary>
        /// 取消分销商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult updateRate(long id, decimal rate1, decimal? rate2 = null, decimal? rate3 = null)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            List<long> ids = new List<long>();
            if (id > 0)
            {
                ids.Add(id);
                bool isdataok = true;
                var sumrate = rate1;
                if (rate1 < 0.1m)
                {
                    result.success = false;
                    result.msg = "佣金比例不能为0，且总和需小于 " + SiteSettings.DistributionMaxBrokerageRate + "%";
                    isdataok = false;
                }
                if (rate2.HasValue)
                {
                    sumrate += rate2.Value;
                    if (rate2 < 0.1m)
                    {
                        result.success = false;
                        result.msg = "佣金比例不能为0，且总和需小于 " + SiteSettings.DistributionMaxBrokerageRate + "%";
                        isdataok = false;
                    }
                }
                if (rate3.HasValue)
                {
                    sumrate += rate3.Value;
                    if (rate3 < 0.1m)
                    {
                        result.success = false;
                        result.msg = "佣金比例不能为0，且总和需小于 " + SiteSettings.DistributionMaxBrokerageRate + "%";
                        isdataok = false;
                    }
                }
                if (sumrate < 0.1m || sumrate > SiteSettings.DistributionMaxBrokerageRate)
                {
                    result.success = false;
                    result.msg = "佣金比例不能为0，且总和需小于 " + SiteSettings.DistributionMaxBrokerageRate + "%";
                    isdataok = false;
                }
                if (isdataok)
                {
                    string _str = rate1.ToString();
                    //验证格式
                    if (!Regex.IsMatch(_str, @"^\d{1,2}(\.\d)?$"))
                    {
                        result.success = false;
                        result.msg = "错误的数据格式，只可以保留一位小数";
                        isdataok = false;
                    }
                    if (SiteSettings.DistributionMaxLevel > 1)
                    {
                        _str = rate2.ToString();
                        //验证格式
                        if (!Regex.IsMatch(_str, @"^\d{1,2}(\.\d)?$"))
                        {
                            result.success = false;
                            result.msg = "错误的数据格式，只可以保留一位小数";
                            isdataok = false;
                        }
                    }
                    if (SiteSettings.DistributionMaxLevel > 2)
                    {
                        _str = rate3.ToString();
                        //验证格式
                        if (!Regex.IsMatch(_str, @"^\d{1,2}(\.\d)?$"))
                        {
                            result.success = false;
                            result.msg = "错误的数据格式，只可以保留一位小数";
                            isdataok = false;
                        }
                    }
                }
                if (isdataok)
                {
                    DistributionApplication.SetProductBrokerageRate(id, rate1, rate2, rate3);
                    result.success = true;
                    result.msg = "修改佣金比例成功";
                }
            }
            else
            {
                result = new Result { success = false, msg = "错误的商品编号" };
            }
            return Json(result);
        }

        #region 默认佣金设置
        /// <summary>
        /// 默认佣金设置
        /// </summary>
        /// <returns></returns>
        public ActionResult DefaultBrokerage()
        {
            var data = DistributionApplication.GetDefaultBrokerageRate(curshopid);
            var model = new DistributionDefaultBrokerageModel()
            {
                BrokerageRate1 = data.BrokerageRate1,
                BrokerageRate2 = data.BrokerageRate2,
                BrokerageRate3 = data.BrokerageRate3,
                DistributionMaxBrokerageRate = SiteSettings.DistributionMaxBrokerageRate,
                DistributionMaxLevel = SiteSettings.DistributionMaxLevel
            };
            model.IsConfigRight = true;
            if (model.BrokerageRate1 + model.BrokerageRate2 + model.BrokerageRate3 > model.DistributionMaxBrokerageRate)
            {
                model.IsConfigRight = false;
            }
            if (model.BrokerageRate1 <= 0)
            {
                model.IsConfigRight = false;
            }
            if (model.DistributionMaxLevel > 1 && model.BrokerageRate2 <= 0)
            {
                model.IsConfigRight = false;
            }
            if (model.DistributionMaxLevel > 2 && model.BrokerageRate3 <= 0)
            {
                model.IsConfigRight = false;
            }
            return View(model);
        }
        [HttpPost]
        public JsonResult DefaultBrokerage(DistributionDefaultBrokerageModel model)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            bool isdataok = true;
            if (!ModelState.IsValid)
            {
                result.success = false;
                result.msg = "佣金比例不能为0，且总和需小于 " + SiteSettings.DistributionMaxBrokerageRate + "%";
                isdataok = false;
            }
            if (SiteSettings.DistributionMaxLevel > 1 && model.BrokerageRate2 <= 0)
            {
                result.success = false;
                result.msg = "佣金比例不能为0，且总和需小于 " + SiteSettings.DistributionMaxBrokerageRate + "%";
                isdataok = false;
            }
            if (SiteSettings.DistributionMaxLevel < 2)
            {
                model.BrokerageRate2 = 0;
            }
            if (SiteSettings.DistributionMaxLevel > 2 && model.BrokerageRate3 <= 0)
            {
                result.success = false;
                result.msg = "佣金比例不能为0，且总和需小于 " + SiteSettings.DistributionMaxBrokerageRate + "%";
                isdataok = false;
            }
            if (SiteSettings.DistributionMaxLevel < 3)
            {
                model.BrokerageRate3 = 0;
            }
            if (model.BrokerageRate1 + model.BrokerageRate2 + model.BrokerageRate3 > SiteSettings.DistributionMaxBrokerageRate)
            {
                result.success = false;
                result.msg = "佣金比例不能为0，且总和需小于 " + SiteSettings.DistributionMaxBrokerageRate + "%";
                isdataok = false;
            }
            if (isdataok)
            {
                string _str = model.BrokerageRate1.ToString();
                //验证格式
                if (!Regex.IsMatch(_str, @"^\d{1,2}(\.\d)?$"))
                {
                    result.success = false;
                    result.msg = "错误的数据格式，只可以保留一位小数";
                    isdataok = false;
                }
                _str = model.BrokerageRate2.ToString();
                //验证格式
                if (!Regex.IsMatch(_str, @"^\d{1,2}(\.\d)?$"))
                {
                    result.success = false;
                    result.msg = "错误的数据格式，只可以保留一位小数";
                    isdataok = false;
                }
                _str = model.BrokerageRate3.ToString();
                //验证格式
                if (!Regex.IsMatch(_str, @"^\d{1,2}(\.\d)?$"))
                {
                    result.success = false;
                    result.msg = "错误的数据格式，只可以保留一位小数";
                    isdataok = false;
                }
            }

            if (isdataok)
            {
                var data = DistributionApplication.GetDefaultBrokerageRate(curshopid);
                data.BrokerageRate1 = model.BrokerageRate1;
                data.BrokerageRate2 = model.BrokerageRate2;
                data.BrokerageRate3 = model.BrokerageRate3;
                DistributionApplication.UpdateDefaultBrokerageRate(curshopid, data);
                result.success = true;
                result.msg = "设置默认佣金比例成功！";
            }
            return Json(result);
        }
        #endregion
        public ActionResult NoSetting()
        {
            return View("Nosetting");
        }
    }
}