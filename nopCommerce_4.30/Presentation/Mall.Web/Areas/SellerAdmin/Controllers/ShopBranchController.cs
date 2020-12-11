using System;
using System.Collections.Generic;
using System.Linq;


using Mall.Web.Framework;
using Mall.Application;
using Mall.DTO;
using Mall.Core;
using Mall.CommonModel;
using Mall.Web.Models;
using Mall.IServices;

using System.Drawing;
using System.IO;
using Mall.DTO.QueryModel;
using Mall.Web.Areas.SellerAdmin.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Mall.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http.Features;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    [StoreAuthorization]
    public class ShopBranchController : BaseSellerController
    {
        private const string DADA_STORE_PREFIX = "st_";

        // GET: SellerAdmin/ShopBranch
        public ActionResult Add()
        {
            //门店标签
            var shopBranchTagInfos = ShopBranchApplication.GetAllShopBranchTagInfos();
            List<SelectListItem> tagList = new List<SelectListItem>();
            foreach (var item in shopBranchTagInfos)
            {
                tagList.Add(new SelectListItem
                {
                    Selected = false,
                    Value = item.Id.ToString(),
                    Text = item.Title
                });
            }
            ViewBag.ShopBranchTags = tagList;
            return View(new ShopBranch { IsStoreDelive = true, ServeRadius = 0, DeliveFee = 0, DeliveTotalFee = 0, FreeMailFee = 0 });
        }
        [HttpPost]
        public ActionResult Add(ShopBranch shopBranch)
        {
            try
            {
                if (!string.Equals(shopBranch.PasswordOne, shopBranch.PasswordTwo))
                {
                    throw new MallException("两次密码输入不一致！");
                }
                if (string.IsNullOrWhiteSpace(shopBranch.PasswordOne) || string.IsNullOrWhiteSpace(shopBranch.PasswordTwo))
                {
                    throw new MallException("密码不能为空！");
                }
                if (shopBranch.ShopBranchName.Length > 15)
                {
                    throw new MallException("门店名称不能超过15个字！");
                }
                if (shopBranch.AddressDetail.Length > 50)
                {
                    throw new MallException("详细地址不能超过50个字！");
                }
                if (shopBranch.Latitude <= 0 || shopBranch.Longitude <= 0)
                {
                    throw new MallException("请搜索地址地图定位！");
                }
                if (!shopBranch.IsAboveSelf && !shopBranch.IsStoreDelive)
                {
                    throw new MallException("至少需要选择一种配送方式！");
                }
                if(shopBranch.IsStoreDelive && shopBranch.IsFreeMail && shopBranch.FreeMailFee <= 0)
                {
                    throw new MallException("满额包邮金额必须大于0！");
                }
                if (!shopBranch.IsStoreDelive)
                {
                    shopBranch.IsFreeMail = false;
                }
                if (!shopBranch.IsFreeMail)
                {
                    shopBranch.FreeMailFee = 0;
                }
                shopBranch.ShopId = CurrentSellerManager.ShopId;
                shopBranch.CreateDate = DateTime.Now;
                long shopBranchId;
                ShopBranchApplication.AddShopBranch(shopBranch, out shopBranchId);

                if (!string.IsNullOrWhiteSpace(shopBranch.ShopBranchTagId))
                {
                    var tags = shopBranch.ShopBranchTagId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => long.Parse(p)).ToList();
                    ShopBranchApplication.SetShopBrandTagInfos(new List<long> { shopBranch.Id }, tags);
                }

                //门店标签
                var shopBranchTagInfos = ShopBranchApplication.GetAllShopBranchTagInfos();
                List<SelectListItem> tagList = new List<SelectListItem>();
                foreach (var item in shopBranchTagInfos)
                {
                    tagList.Add(new SelectListItem
                    {
                        Selected = (shopBranch.ShopBranchTagId == null ? false : shopBranch.ShopBranchTagId.Split(',').Contains(item.Id.ToString()) ? true : false),
                        Value = item.Id.ToString(),
                        Text = item.Title
                    });
                }
                ViewBag.ShopBranchTags = tagList;

                if (CityExpressConfigApplication.GetDaDaCityExpressConfig(CurrentShop.Id).IsEnable)
                {
                    var dada_shop_id = GetNewDadaStoreId(CurrentShop.Id, shopBranch.Id);
                    var _area = RegionApplication.GetRegion(shopBranch.AddressId);
                    var _city = GetCity(_area);
                    var json = ExpressDaDaHelper.shopAdd(CurrentShop.Id, shopBranch.ShopBranchName, 5, _city.ShortName, _area.Parent.Name, shopBranch.AddressDetail, shopBranch.Longitude, shopBranch.Latitude, shopBranch.ContactUser, shopBranch.ContactPhone, dada_shop_id);
                    var resultObj = JsonConvert.DeserializeObject(json) as JObject;
                    string status = resultObj["status"].ToString();
                    int code = int.Parse(resultObj["code"].ToString());
                    if (status == "fail" && code != 7718)
                    {
                        return Json(new Result() { success = true, msg = "但同步门店至达达物流失败，可能所在城市达达不支持" });
                    }
                    if (string.IsNullOrWhiteSpace(shopBranch.DaDaShopId) && (status == "success" || code == 7718))
                    {
                        shopBranch.DaDaShopId = dada_shop_id;
                        ShopBranchApplication.UpdateShopBranch(shopBranch);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new Result() { success = false, msg = ex.Message });
            }
            return Json(new Result() { success = true });
        }
        public ActionResult Edit(long id)
        {
            var shopBranch = ShopBranchApplication.GetShopBranchById(id);

            //门店标签
            var shopBranchTagInfos = ShopBranchApplication.GetAllShopBranchTagInfos();
            List<SelectListItem> tagList = new List<SelectListItem>();
            foreach (var item in shopBranchTagInfos)
            {
                tagList.Add(new SelectListItem
                {
                    Selected = (shopBranch.ShopBranchTagId == null ? false : shopBranch.ShopBranchTagId.Split(',').Contains(item.Id.ToString()) ? true : false),
                    Value = item.Id.ToString(),
                    Text = item.Title
                });
            }
            ViewBag.ShopBranchTags = tagList;
            return View(shopBranch);
        }
        [HttpPost]
        public ActionResult Edit(ShopBranch shopBranch)
        {
            try
            {
                if (!string.Equals(shopBranch.PasswordOne, shopBranch.PasswordTwo))
                {
                    throw new MallException("两次密码输入不一致！");
                }
                if (shopBranch.ShopBranchName.Length > 15)
                {
                    throw new MallException("门店名称不能超过15个字！");
                }
                if (shopBranch.AddressDetail.Length > 50)
                {
                    throw new MallException("详细地址不能超过50个字！");
                }
                if (shopBranch.Latitude <= 0 || shopBranch.Longitude <= 0)
                {
                    throw new MallException("请搜索地址地图定位！");
                }
                if (!shopBranch.IsAboveSelf && !shopBranch.IsStoreDelive)
                {
                    throw new MallException("至少需要选择一种配送方式！");
                }
                if (shopBranch.IsStoreDelive && shopBranch.IsFreeMail && shopBranch.FreeMailFee <= 0)
                {
                    throw new MallException("满额包邮金额必须大于0！");
                }
                if (!shopBranch.IsStoreDelive)
                {
                    shopBranch.IsFreeMail = false;
                }
                if (!shopBranch.IsFreeMail)
                {
                    shopBranch.FreeMailFee = 0;
                }
                //判断是否编辑自己的门店
                shopBranch.ShopId = CurrentSellerManager.ShopId;//当前登录商家
                //门店所属商家
                var oldBranch = ShopBranchApplication.GetShopBranchById(shopBranch.Id);
                if (oldBranch != null && oldBranch.ShopId != shopBranch.ShopId)
                    throw new MallException("不能修改其他商家的门店！");

                if (!shopBranch.IsFreeMail)
                {
                    shopBranch.FreeMailFee = 0;
                }

                if (!string.IsNullOrWhiteSpace(shopBranch.ShopBranchTagId))
                {
                    var tags = shopBranch.ShopBranchTagId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => long.Parse(p)).ToList();
                    ShopBranchApplication.SetShopBrandTagInfos(new List<long> { shopBranch.Id }, tags);
                }

                ShopBranchApplication.UpdateShopBranch(shopBranch);

                if (CityExpressConfigApplication.GetDaDaCityExpressConfig(CurrentShop.Id).IsEnable)
                {
                    var _area = RegionApplication.GetRegion(shopBranch.AddressId);
                    var _city = GetCity(_area);
                    string json = "";
                    var dada_shop_id = GetNewDadaStoreId(CurrentShop.Id, shopBranch.Id);
                    if (string.IsNullOrWhiteSpace(shopBranch.DaDaShopId))
                    {
                        json = ExpressDaDaHelper.shopAdd(CurrentShop.Id, shopBranch.ShopBranchName, 5, _city.ShortName, _area.Parent.Name, shopBranch.AddressDetail, shopBranch.Longitude, shopBranch.Latitude, shopBranch.ContactUser, shopBranch.ContactPhone, dada_shop_id);
                    }
                    else
                    {
                        json = ExpressDaDaHelper.shopUpdate(CurrentShop.Id, shopBranch.DaDaShopId, shopBranch.ShopBranchName, 5, _city.ShortName, _area.Parent.Name, shopBranch.AddressDetail, shopBranch.Longitude, shopBranch.Latitude, shopBranch.ContactUser, shopBranch.ContactPhone);
                    }
                    var resultObj = JsonConvert.DeserializeObject(json) as JObject;
                    string status = resultObj["status"].ToString();
                    int code = int.Parse(resultObj["code"].ToString());
                    if (status == "fail" && code != 7718)
                    {
                        return Json(new Result() { success = true, msg = "但同步门店至达达物流失败，可能所在城市达达不支持" });
                    }
                    if (string.IsNullOrWhiteSpace(shopBranch.DaDaShopId) && (status == "success" || code == 7718))
                    {
                        shopBranch.DaDaShopId = dada_shop_id;
                        ShopBranchApplication.UpdateShopBranch(shopBranch);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new Result() { success = false, msg = ex.Message });
            }
            return Json(new Result() { success = true });
        }
        long[] convertLongs(string[] strs)
        {
            List<long> list = new List<long>();
            foreach (string str in strs)
            {
                long info = 0;
                long.TryParse(str, out info);
                list.Add(info);
            }
            return list.ToArray();
        }

        public ActionResult Management()
        {
            return View();
        }
        public JsonResult List(ShopBranchQuery query)
        {
            query.ShopId = (int)CurrentSellerManager.ShopId;
            if (query.AddressId.HasValue)
                query.AddressPath = RegionApplication.GetRegionPath(query.AddressId.Value);
            var shopBranchs = ShopBranchApplication.GetShopBranchs(query);
            var dataGrid = new DataGridModel<ShopBranch>()
            {
                rows = shopBranchs.Models,
                total = shopBranchs.Total
            };
            return Json(dataGrid);
        }

        public JsonResult Freeze(long shopBranchId)
        {
            ShopBranchApplication.Freeze(shopBranchId);
            return Json(new { success = true, msg = "冻结成功！" });
        }
        public JsonResult UnFreeze(long shopBranchId)
        {
            ShopBranchApplication.UnFreeze(shopBranchId);
            return Json(new { success = true, msg = "解冻成功！" });
        }

        /// <summary>
        /// 门店设置
        /// </summary>
        /// <returns></returns>
        public ActionResult Setting()
        {
            var shopInfo = ShopApplication.GetShop(CurrentSellerManager.ShopId);
            if (shopInfo != null)
                ViewBag.AutoAllotOrder = shopInfo.AutoAllotOrder;
            return View();
        }

        [HttpPost]
        public JsonResult Setting(bool autoAllotOrder)
        {
            try
            {
                ShopApplication.SetAutoAllotOrder(CurrentSellerManager.ShopId, autoAllotOrder);
                ServiceApplication.Create<IOperationLogService>().AddSellerOperationLog(new Entities.LogInfo
                {
                    Date = DateTime.Now,
                    Description = string.Format("{0}:订单自动分配到门店", autoAllotOrder ? "开启" : "关闭"),
                    IPAddress =  base.Request.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress.ToString(),
                    PageUrl = "/ShopBranch/Setting",
                    UserName = CurrentSellerManager.UserName,
                    ShopId = CurrentSellerManager.ShopId
                });
                return Json(new
                {
                    success = true
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    msg = e.Message
                });
            }
        }

        /// <summary>
        /// 门店链接二维码
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public JsonResult StoresLink(string vshopUrl)
        {
            string qrCodeImagePath = string.Empty;
            if (!string.IsNullOrWhiteSpace(vshopUrl))
            {
                Bitmap map;
                map = Core.Helper.QRCodeHelper.Create(vshopUrl);
                MemoryStream ms = new MemoryStream();
                map.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                qrCodeImagePath = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray()); // 将图片内存流转成base64,图片以DataURI形式显示  
                ms.Dispose();
            }
            return Json(new { success = true, qrCodeImagePath = qrCodeImagePath });
        }
        /// <summary>
        /// 达达物流配置
        /// </summary>
        /// <returns></returns>
        public ActionResult DaDaConfig()
        {
            var data = CityExpressConfigApplication.GetDaDaCityExpressConfig(CurrentShop.Id);
            var result = new ShopBranchDaDaConfigModel
            {
                IsEnable = data.IsEnable,
                app_key = data.app_key,
                app_secret = data.app_secret,
                source_id = data.source_id
            };
            return View(result);
        }
        [HttpPost]
        public JsonResult DaDaConfig(ShopBranchDaDaConfigModel model)
        {
            long shopId = CurrentShop.Id;
            Result result = new Result
            {
                success = false,
                msg = "未知错误"
            };
            if (ModelState.IsValid)
            {
                if (model.IsEnable)
                {
                    if (string.IsNullOrWhiteSpace(model.app_key) || string.IsNullOrWhiteSpace(model.app_secret) || string.IsNullOrWhiteSpace(model.source_id))
                    {
                        result.success = false;
                        result.msg = "数据错误，请填写必填信息";
                        return Json(result);
                    }
                }
                var data = CityExpressConfigApplication.GetDaDaCityExpressConfig(shopId);
                data.IsEnable = model.IsEnable;
                data.app_key = model.app_key;
                data.app_secret = model.app_secret;
                data.source_id = model.source_id;
                CityExpressConfigApplication.Update(CurrentShop.Id, data);
                result.msg = "";
                //同步开通达达门店
                var sblist = ShopBranchApplication.GetShopBranchByShopId(shopId).Where(d => string.IsNullOrWhiteSpace(d.DaDaShopId));
                foreach (var item in sblist)
                {
                    var dada_shop_id = GetNewDadaStoreId(CurrentShop.Id, item.Id);
                    var _area = RegionApplication.GetRegion(item.AddressId);
                    var _city = GetCity(_area);
                    var json = ExpressDaDaHelper.shopAdd(shopId, item.ShopBranchName, 5, _city.ShortName, _area.Parent.Name, item.AddressDetail, item.Longitude, item.Latitude, item.ContactUser, item.ContactPhone, dada_shop_id);
                    var resultObj = JsonConvert.DeserializeObject(json) as JObject;
                    string status = resultObj["status"].ToString();
                    int code = int.Parse(resultObj["code"].ToString());
                    if (status == "fail" && code != 7718)
                    {
                        result.msg = "但部份同步门店失败，可能所在城市达达不支持";
                    }
                    if (string.IsNullOrWhiteSpace(item.DaDaShopId) && (status == "success" || code == 7718))
                    {
                        item.DaDaShopId = dada_shop_id;
                        ShopBranchApplication.UpdateShopBranch(item);
                    }
                }
                result.success = true;
            }
            else
            {
                result.success = false;
                result.msg = "数据错误，请填写必填信息";
            }

            return Json(result);
        }
        /// <summary>
        /// 获取市级地区
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        private CommonModel.Region GetCity(CommonModel.Region region)
        {
            CommonModel.Region _city = region;
            if (_city.Level == CommonModel.Region.RegionLevel.City || _city.Level == CommonModel.Region.RegionLevel.Province || _city.Parent == null)
            {
                return _city;
            }
            _city = _city.Parent;
            return GetCity(_city);
        }
        /// <summary>
        /// 获取达达门店编号
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="shopbranchId"></param>
        /// <returns></returns>
        private string GetNewDadaStoreId(long shopId, long shopbranchId)
        {
            return DADA_STORE_PREFIX + shopId + "_" + shopbranchId;
        }

        /// <summary>
        /// 门店权限管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Privilege(long id)
        {
            var shopBranch = ShopBranchApplication.GetShopBranchById(id);
            return View(shopBranch);
        }

        [HttpPost]
        public JsonResult Privilege(long Id, bool IsShelvesProduct)
        {
            Result result = new Result
            {
                success = false,
                msg = "未知错误"
            };
            var shopBranch = ShopBranchApplication.GetShopBranchById(Id);
            if (shopBranch == null)
            {
                result.success = false;
                result.msg = "错误的门店";
            }
            else
            {
                shopBranch.IsShelvesProduct = IsShelvesProduct;
                ShopBranchApplication.UpdateShopBranch(shopBranch);
                result.success = true;
                result.msg = "";
            }
            return Json(result);
        }
        /// <summary>
        /// 门店商品管理
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public ActionResult ProductManagement(long shopBranchId)
        {
            var shopBranch = ShopBranchApplication.GetShopBranchById(shopBranchId);
            var shopBranchName = string.Empty;
            if (shopBranch != null)
                shopBranchName = shopBranch.ShopBranchName;
            ViewBag.ShopBranchName = shopBranchName;
            ViewBag.ShopCategorys = ShopCategoryApplication.GetShopCategory(CurrentSellerManager.ShopId);
            return View();
        }

        /// <summary>
        /// 门店商品列表
        /// </summary>
        /// <param name="query"></param>
        /// <param name="rows"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ProductList(ShopBranchProductQuery query)
        {
            query.ShopBranchProductStatus = 0;
            query.OrderKey = 2;
            //查询商品
            var pageModel = ShopBranchApplication.GetShopBranchProducts(query);

            //查询门店SKU库存
            var allSKU = ProductManagerApplication.GetSKUByProducts(pageModel.Models.Select(p => p.Id));
            List<string> skuids = allSKU.Select(p => p.Id).ToList();
            var shopBranchSkus = ShopBranchApplication.GetSkusByIds(query.ShopBranchId.Value, skuids);

            var dataGrid = new DataGridModel<ProductModel>();
            dataGrid.total = pageModel.Total;
            dataGrid.rows= pageModel.Models.Select(item =>
            {
                var cate = ShopCategoryApplication.GetCategoryByProductId(item.Id);
                return new ProductModel()
                {
                    Name = item.ProductName,
                    Id = item.Id,
                    Image = item.GetImage(ImageSize.Size_50),
                    CategoryName = cate == null ? "" : cate.Name,
                    SaleCount = item.SaleCounts,
                    Stock = shopBranchSkus.Where(e => e.ProductId == item.Id).Sum(s => s.Stock),
                    Price = item.MinSalePrice,
                    MinPrice = allSKU.Where(s => s.ProductId == item.Id).Min(s => s.SalePrice),
                    MaxPrice = allSKU.Where(s => s.ProductId == item.Id).Max(s => s.SalePrice),
                    Url = "",
                    PublishTime = item.AddedDate.ToString("yyyy-MM-dd HH:mm"),
                    SaleState = (int)item.SaleStatus,
                    CategoryId = item.CategoryId,
                    ProductCode = item.ProductCode,
                    ProductType = item.ProductType
                };
            }).ToList();
            return Json(dataGrid);
        }

        /// <summary>
        /// 下架商品
        /// </summary>
        /// <param name="pids"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetUnSaleProduct(string pids,long shopbranchId)
        {
            if (string.IsNullOrWhiteSpace(pids))
                return  Json(new { success = false, msg = "参数异常" });
            var ids = ConvertToIEnumerable(pids);
            ShopBranchApplication.UnSaleProduct(shopbranchId, ids);
            return Json(new { success = true, msg = "已下架" });
        }

        private List<long> ConvertToIEnumerable(string str, char sp = ',')
        {
            var ids = str.Split(sp).Select(e =>
            {
                long id = 0;
                if (!long.TryParse(e, out id))
                {
                    id = 0;
                }
                return id;
            }).ToList();
            return ids;
        }

        /// <summary>
        /// 加载门店编辑商品列表
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="shopbranchId"></param>
        /// <returns></returns>
        public JsonResult GetProductsByIds(string ids,long shopbranchId)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return Json(new { success = false, msg = "参数传递错误" });

            var pIds = ConvertToIEnumerable(ids);
            var products = ProductManagerApplication.GetAllProductByIds(pIds);
            
            //查询门店SKU库存
            var allSKU = ProductManagerApplication.GetSKUByProducts(products.Select(p => p.Id));
            List<string> skuids = allSKU.Select(p => p.Id).ToList();
            var shopBranchSkus = ShopBranchApplication.GetSkusByIds(shopbranchId, skuids);

            List<ProductModel> productlist = new List<ProductModel>();
            foreach (var item in products)
            {
                var skus = ProductManagerApplication.GetSKUs(item.Id);
                ProductModel product = new ProductModel()
                {
                    Name = item.ProductName,
                    Id = item.Id,
                    Image = item.GetImage(ImageSize.Size_50),
                    Price = item.MinSalePrice
                };
                List<SKuModel> skulist = new List<SKuModel>();
                foreach (var skuInfo in skus)
                {
                    var shopbranckSku = shopBranchSkus.Where(s => s.SkuId == skuInfo.Id).FirstOrDefault();
                    var str = string.Empty;
                    if (!string.IsNullOrWhiteSpace(skuInfo.Color))
                    {
                        str += item.ColorAlias + "：" + skuInfo.Color;
                    }
                    else
                    {
                        str += " " + item.ColorAlias + "；";
                    }
                    if (!string.IsNullOrWhiteSpace(skuInfo.Size))
                    {
                        str += " " + item.SizeAlias + "：" + skuInfo.Size;
                    }
                    else
                    {
                        str += " " + item.SizeAlias + "；";
                    }
                    if (!string.IsNullOrWhiteSpace(skuInfo.Version))
                    {
                        str += " " + item.VersionAlias + "：" + skuInfo.Version;
                    }
                    else
                    {
                        str += " " + item.VersionAlias + "；";
                    }
                    var model = new SKuModel()
                    {
                        ProductName = (item.HasSKU ? str.TrimEnd('；') : ""),
                        Sku = item.HasSKU ? (skuInfo.Sku ?? string.Empty) : item.ProductCode,
                        Id = skuInfo.Id,
                        AutoId = shopbranckSku.Id,
                        Stock = shopbranckSku.Stock,
                        MarketPrice = item.MarketPrice,
                        SalePrice = item.HasSKU ? skuInfo.SalePrice : item.MinSalePrice,//有规格则取规格商城价，否则取商品本身商城价
                        ProductId = item.Id,
                        HasSKU = item.HasSKU ? 1 : 0
                    };
                    if (skus.IndexOf(skuInfo) == 0)
                    {
                        model.IsFirst = 1;
                    }
                    skulist.Add(model);
                }
                product.Skus = skulist;
                productlist.Add(product);
            }
            return Json(new
            {
                success = true,
                model = productlist
            });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult BatchSettingStock( List<StockModel> stocks, long shopbranchId)
        {
            if (stocks == null)
            {
                return Json(new { success = false, msg = "参数传递错误！" });
            }
            Dictionary<string, int> dics = stocks.ToDictionary(key => key.SkuId, value => int.Parse(value.Stock.ToString()));
            ShopBranchApplication.SetSkuStock(shopbranchId, StockOptionType.Normal, dics);
            return Json(new
            {
                success = true,
                msg = "操作成功"
            });
        }


        [UnAuthorize]
        [HttpPost]        
        public JsonResult Browse(long? categoryId, int? auditStatus, string ids, int page, string keyWords, string shopName,
            int? saleStatus, bool? isShopCategory, int rows = 10, bool isLimitTimeBuy = false, bool showSku = false, long[] exceptProductIds = null)
        {
            var query = new ProductQuery()
            {
                PageSize = rows,
                PageNo = page,
                KeyWords = keyWords,
                ShopName = shopName,
                CategoryId = isShopCategory.GetValueOrDefault() ? null : categoryId,
                ShopCategoryId = isShopCategory.GetValueOrDefault() ? categoryId : null,
                Ids = string.IsNullOrWhiteSpace(ids) ? null : ids.Split(',').Select(item => long.Parse(item)),
                ShopId = CurrentSellerManager.ShopId,
                IsLimitTimeBuy = isLimitTimeBuy,
                ExceptIds = exceptProductIds,
                IsFilterStock = false,
                HasLadderProduct = false,
                AuditStatus = new[] { ProductInfo.ProductAuditStatus.Audited },
                SaleStatus = ProductInfo.ProductSaleStatus.OnSale
            };

            var data = ProductManagerApplication.GetProducts(query);
            var shops = ShopApplication.GetShops(data.Models.Select(p => p.ShopId));
            var brands = BrandApplication.GetBrands(data.Models.Select(p => p.BrandId));
            var skus = ProductManagerApplication.GetSKUByProducts(data.Models.Select(p => p.Id));

            var products = data.Models.Select(item =>
            {
                var brand = brands.FirstOrDefault(p => p.Id == item.BrandId);
                var shop = shops.FirstOrDefault(p => p.Id == item.ShopId);
                var cate = CategoryApplication.GetCategory(item.CategoryId);
                var sku = skus.Where(p => p.ProductId == item.Id);
                var limitAdd = LimitTimeApplication.IsAdd(item.Id);
                return new
                {
                    name = item.ProductName,
                    brandName = brand?.Name ?? string.Empty,
                    categoryName = brand == null ? "" : cate.Name,
                    id = item.Id,
                    imgUrl = item.GetImage(ImageSize.Size_50),
                    price = item.MinSalePrice,
                    skus = !showSku ? null : sku.Select(a => new SKUModel()
                    {
                        Id = a.Id,
                        SalePrice = a.SalePrice,
                        Size = a.Size,
                        Stock = a.Stock,
                        Version = a.Version,
                        Color = a.Color,
                        Sku = a.Sku,
                        AutoId = a.AutoId,
                        ProductId = a.ProductId
                    }),
                    shopName = shop.ShopName,
                    isOpenLadder = item.IsOpenLadder,
                    isLimit = limitAdd
                };
            });

            var dataGrid = new
            {
                rows = products,
                total = data.Total
            };
            return Json(dataGrid);
        }

        /// <summary>
        /// 获取门店上架商品ID集合
        /// </summary>
        /// <param name="shopbranchId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ShopBranchProductIds(long shopbranchId)
        {
            ShopBranchProductQuery query = new ShopBranchProductQuery()
            {
                ShopBranchId = shopbranchId,
                ShopBranchProductStatus = 0,
                PageSize = 10000
            };
            //查询商品
            var pageModel = ShopBranchApplication.GetShopBranchProducts(query);

            var ids = pageModel.Models.Select(item => item.Id);

            return Json(ids);
        }

        /// <summary>
        /// 上架商品
        /// </summary>
        /// <param name="shopbranchId"></param>
        /// <param name="pids"></param>
        /// <returns></returns>
        [UnAuthorize]
        [HttpPost]
        public JsonResult OnSaleProduct(long shopbranchId,string pids)
        {
            if (string.IsNullOrWhiteSpace(pids))
                return Json(new { success = false, msg = "参数异常" });
            var ids = ConvertToIEnumerable(pids);
            if (!ShopBranchApplication.CanOnSaleProduct(ids))
            {
                return Json(new { success = false, msg = "有不在销售状态的商品存在，不可执行上架操作" });
            }
            if (ShopBranchApplication.IsOpenLadderInProducts(ids))
            {
                return Json(new { success = false, msg = "有商品为阶梯批发商品，不能上架到门店" });
            }
            ShopBranchApplication.AddProductSkus(ids, shopbranchId, CurrentSellerManager.ShopId);
            ShopBranchApplication.OnSaleProduct(shopbranchId, ids);
            return Json(new { success = true, msg = "已上架" });
        }
    }
}