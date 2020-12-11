using Mall.Core;
using Mall.Core.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using System.Text;
using Mall.IServices;
using Mall.DTO.QueryModel;

using Mall.Web.Framework;
using LumenWorks.Framework.IO.Csv;
using Mall.CommonModel;
using Mall.Application;
using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Entities;
using Mall.DTO;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Himall.TaoBaoSDK;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class ProductImportController : BaseSellerController
    {
        private ICategoryService _iCategoryService;
        private IShopService _iShopService;
        private IBrandService _iBrandService;
        private IProductService _iProductService;
        public ProductImportController(
            ICategoryService iCategoryService,
            IShopService iShopService,
            IBrandService iBrandService, IProductService iProductService
            )
            : this()
        {
            _iBrandService = iBrandService;
            _iShopService = iShopService;
            _iCategoryService = iCategoryService;
            _iProductService = iProductService;
        }
        private long _shopid = 0;
        private long _userid = 0;
        public ProductImportController()
            : base()
        {
            //退出登录后，直接访问页面异常处理
            if (CurrentSellerManager != null)
            {
                _shopid = CurrentSellerManager.ShopId;
                _userid = CurrentSellerManager.Id;
            }
        }
        public ActionResult ImportManage()
        {
            string message = "";
            ViewBag.CanCreate = 1;
            ViewBag.CanNotCreateMessage = "";
            if (!CanCreate(out message))
            {
                ViewBag.CanCreate = 0;
                ViewBag.CanNotCreateMessage = message;
            }
            int lngCount = 0, lngTotal = 0;
            int intSuccess = 0;
            //从缓存取用户导入商品数量
            GetImportCountFromCache(out lngCount, out lngTotal);

            if (lngTotal == lngCount && lngTotal > 0)
            {
                intSuccess = 1;
            }
            var freightTemplates = ServiceApplication.Create<IFreightTemplateService>().GetShopFreightTemplate(CurrentSellerManager.ShopId);
            List<SelectListItem> freightList = new List<SelectListItem> { new SelectListItem
                {
                    Selected = false,
                    Text ="请选择运费模板...",
                    Value = "0"
                }};
            foreach (var item in freightTemplates)
            {
                freightList.Add(new SelectListItem
                {
                    Text = item.Name + "【" + item.ValuationMethod.ToDescription() + "】",
                    Value = item.Id.ToString()
                });
            }
            ViewBag.FreightTemplates = freightList;
            ViewBag.Count = lngCount;
            ViewBag.Total = lngTotal;
            ViewBag.Success = intSuccess;
            ViewBag.shopid = _shopid;
            ViewBag.userid = _userid;
            return View();
        }

        /// <summary>
        /// 抓取淘宝/天猫数据导入
        /// </summary>
        /// <returns></returns>
        public ActionResult SpiderManage()
        {
            int lngCount = 0, lngTotal = 0;
            int intSuccess = 0;
            //从缓存取用户导入商品数量
            GetImportCountFromCache(out lngCount, out lngTotal);

            if (lngTotal == lngCount && lngTotal > 0)
            {
                intSuccess = 1;
            }
            var freightTemplates = ServiceApplication.Create<IFreightTemplateService>().GetShopFreightTemplate(CurrentSellerManager.ShopId);
            List<SelectListItem> freightList = new List<SelectListItem> { new SelectListItem
                {
                    Selected = false,
                    Text ="请选择运费模板...",
                    Value = "0"
                }};
            foreach (var item in freightTemplates)
            {
                freightList.Add(new SelectListItem
                {
                    Text = item.Name + "【" + item.ValuationMethod.ToDescription() + "】",
                    Value = item.Id.ToString()
                });
            }
            ViewBag.FreightTemplates = freightList;
            ViewBag.Count = lngCount;
            ViewBag.Total = lngTotal;
            ViewBag.Success = intSuccess;
            ViewBag.shopid = _shopid;
            ViewBag.userid = _userid;
            return View();
        }

        public ActionResult SpiderSuccess(string guid)
        {
            SpiderProductResult resultInfo = new Models.SpiderProductResult();
            var result = Cache.Get<object>(CacheKeyCollection.CACHE_IMPORTPRODUCTRESULT(CurrentShop.Id + guid));
            if (result != null)
            {
                Cache.Remove(CacheKeyCollection.CACHE_IMPORTPRODUCTRESULT(CurrentShop.Id + guid));
                resultInfo = result as SpiderProductResult;
            }
            return View(resultInfo);
        }
        private bool CanCreate(out string message, int productCount = 0)
        {
            if (ServiceApplication.Create<IShopService>().GetShopSurplusSpace(CurrentSellerManager.ShopId) <= 0)
            {
                message = "存储图片空间不足,不能发布商品!";
                return false;
            }

            var grade = ServiceApplication.Create<IShopService>().GetShopGrade(CurrentShop.GradeId);
            if (grade != null)
            {
                int count = _iProductService.GetShopAllProducts(CurrentSellerManager.ShopId);
                if (productCount > 0)
                {
                    count += productCount;
                }
                if (count >= grade.ProductLimit)
                {
                    message = "此店铺等级最多只能发布" + grade.ProductLimit + "件商品";
                    return false;
                }
            }
            message = "";
            return true;
        }

        [HttpPost]
        public JsonResult SaveProducts(SpiderProductModel model)
        {
            string message = "";
            if (!CanCreate(out message))
            {
                return Json(new { success = false, msg= message });
            }
            DateTime currentTime = DateTime.Now;
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            if (!string.IsNullOrWhiteSpace(model.Guid))
            {
                guid = model.Guid;
            }
            var percent = Cache.Get<object>(CacheKeyCollection.CACHE_IMPORTPRODUCTPERCENT(this.CurrentShop.Id + guid));
            if (percent != null && TypeHelper.StringToInt(percent.ToString().Split('@')[0]) != 100)
            {
                return Json(new { success = false, msg = "上一批还没导入完成", guid = guid });
            }

            if (model.SmallCategoryId <= 0)
            {
                return Json(new { success = false, msg = "请选择完整平台分类" });
            }
            if (model.SellerBigCategoryId <= 0)
            {
                return Json(new { success = false, msg = "请选择商家分类" });
            }
            if (model.FreightTemplateId <= 0)
            {
                return Json(new { success = false, msg = "请选择运费模板" });
            }
            var isEmptyUrl = true;
            if (model.GrabUrl != null)
            {
                isEmptyUrl = model.GrabUrl.Exists(a => string.IsNullOrWhiteSpace(a));
            }
            if (isEmptyUrl)
            {
                return Json(new { success = false, msg = "抓取地址中含有空链接" });
            }

            if (!CanCreate(out message, model.GrabUrl == null ? 0 : model.GrabUrl.Count()))
            {
                return Json(new { success = false, msg = message });
            }
            string ip = Mall.Core.Helper.WebHelper.GetIP();
            List<long> ids = new List<long>();
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (ComfirmCancle(currentTime, guid)) return;
                    Cache.Insert(CacheKeyCollection.CACHE_IMPORTPRODUCTPERCENT(CurrentShop.Id + guid), "@");
                    SpiderProductResult result = new Models.SpiderProductResult();
                    List<string> colors = new List<string>() { "颜色", "色", "色彩" };
                    List<string> sizes = new List<string>() { "尺码", "尺寸", "码" };
                    ProductCreateModel createModel = null;
                    TaoBaoSpider spider = new TaoBaoSpider();
                    var list = spider.GetProductDetailsByUrl(model.GrabUrl);
                    string imageRealtivePath = "/temp/" + Guid.NewGuid().ToString();//图片宕下后存储到临时目录
                    int spiderNum = model.GrabUrl.Count;//总共要处理的链接数量
                    decimal percenet = 0, singlePercenet = 0, infoPercenet = 0;
                    if (spiderNum > 0)
                    {
                        singlePercenet = decimal.Round(100 / spiderNum, 2);//单个URL百分均比
                    }
                    infoPercenet = decimal.Round(singlePercenet / 4, 2);//单个内信息百分比:分为基本信息+详情+sku处理+入库
                    int successNum = 0, failNum = 0;
                    int index = 0;
                    result.FailDataModel = new List<FailDataModel>();
                    if (list == null || list.Count <= 0)
                    {
                        percenet = 100;
                        Cache.Insert(CacheKeyCollection.CACHE_IMPORTPRODUCTPERCENT(CurrentShop.Id + guid), percenet + "@" + index);
                        return;
                    }
                    foreach (var item in list)
                    {
                        index = list.IndexOf(item) + 1;
                        if (item.ProductInfo == null || (item.ProductInfo != null && item.ProductInfo.itemDO != null && (string.IsNullOrWhiteSpace(item.ProductInfo.itemDO.title))))
                        {
                            failNum++;
                            percenet += singlePercenet;
                            result.FailDataModel.Add(new Models.FailDataModel()
                            {
                                GrabUrl = item.GrabUrl,
                                Remark = "链接无法解析"
                            });

                            if (ComfirmCancle(currentTime, guid)) return;
                            Cache.Insert(CacheKeyCollection.CACHE_IMPORTPRODUCTPERCENT(CurrentShop.Id + guid), percenet + "@" + index);
                            continue;
                        }
                        //重复链接判断暂时去掉
                        //if (OperationLogApplication.ExistUrl(item.GrabUrl.Trim(), CurrentShop.Id))
                        //{
                        //    failNum++;
                        //    percenet += singlePercenet;
                        //    result.FailDataModel.Add(new Models.FailDataModel()
                        //    {
                        //        GrabUrl = item.GrabUrl,
                        //        Remark = "重复链接"
                        //    });
                        //    Cache.Insert(CacheKeyCollection.CACHE_IMPORTPRODUCTPERCENT(CurrentShop.Id), percenet + "@" + index);
                        //    continue;
                        //}
                        createModel = new ProductCreateModel();
                        createModel.GrabUrl = item.GrabUrl;
                        createModel.SaleStatus = Entities.ProductInfo.ProductSaleStatus.InDraft;//采集过来的商品状态为草稿箱
                        createModel.CategoryId = model.SmallCategoryId;
                        createModel.CategoryPath = string.Format("{0}|{1}|{2}", model.BigCategoryId, model.MidCategoryId, model.SmallCategoryId);
                        createModel.FreightTemplateId = model.FreightTemplateId;
                        createModel.GoodsCategory = new long[] { model.SellerMidcategoryId > 0 ? model.SellerMidcategoryId : model.SellerBigCategoryId };//这里可传大类也可传小类，优先小类
                        createModel.AuditStatus = ProductInfo.ProductAuditStatus.UnAudit;
                        createModel.BrandId = model.BrandId;
                        createModel.MeasureUnit = "件";
                        createModel.ProductCode = new Random().Next(10000000, 99999999).ToString();
                        createModel.ShortDescription = item.ProductInfo.ShortDescription;//广告词

                        var categoryInfo = CategoryApplication.GetCategory(model.SmallCategoryId);
                        if (categoryInfo != null)
                        {
                            createModel.TypeId = categoryInfo.TypeId;//根据三级分类反查所属类型
                        }
                        TypeInfo typeInfo = TypeApplication.GetType(createModel.TypeId);
                        if (item != null && item.ProductInfo != null)
                        {
                            if (item.ProductInfo.NoSkuInfo != null && item.ProductInfo.NoSkuInfo.data != null && item.ProductInfo.NoSkuInfo.data.dynStock != null)
                            {
                                createModel.Quantity = Mall.Core.Helper.TypeHelper.StringToInt(item.ProductInfo.NoSkuInfo.data.dynStock.stock);
                            }
                            if (item.ProductInfo.itemDO != null)
                            {
                                createModel.ProductName = item.ProductInfo.itemDO.title;//商品标题
                            }
                            if (item.ProductInfo.detail != null)
                            {
                                if (item.ProductInfo.detail.defaultItemPrice.Contains("-"))
                                {
                                    createModel.MinSalePrice = TypeHelper.StringToDecimal(item.ProductInfo.detail.defaultItemPrice.Trim().Split('-')[0]);//默认售价
                                }
                                else
                                {
                                    createModel.MinSalePrice = TypeHelper.StringToDecimal(item.ProductInfo.detail.defaultItemPrice);//默认售价
                                }
                                //createModel.MarketPrice = createModel.MinSalePrice;//市场价现在可为空
                            }
                            if (item.ProductInfo.propertyPics != null)
                            {
                                var imagesPics = item.ProductInfo.propertyPics.ImagesPics;//主图地址
                                                                                          //要转换到本地
                                for (int i = 0; i < imagesPics.Count; i++)
                                {
                                    imagesPics[i] = imagesPics[i].Replace("\"", "").Trim();//去掉两边引号和空格
                                    imagesPics[i] = "http://" + imagesPics[i].Replace("//", "").Replace("http://", "").Replace("https://", "");
                                    imagesPics[i] = Core.Helper.HtmlContentHelper.TransferNetworkImageToLocal(imagesPics[i], "/", imageRealtivePath, Core.MallIO.GetImagePath(imageRealtivePath) + "/");
                                }
                                createModel.Pics = imagesPics.ToArray();//主图地址
                            }
                            if (!string.IsNullOrWhiteSpace(createModel.ProductName) || createModel.Pics.Count() > 0)
                            {
                                percenet = percenet + infoPercenet;
                                if (ComfirmCancle(currentTime, guid)) return;
                                Cache.Insert(CacheKeyCollection.CACHE_IMPORTPRODUCTPERCENT(CurrentShop.Id + guid), percenet + "@" + index);

                            }
                            if (item.ProductInfo.api != null)//商品详情内容
                            {
                                createModel.Description = new DTO.ProductDescription();
                                createModel.Description.Description = item.ProductInfo.api.productdescriptions;
                                createModel.Description.Description = createModel.Description.Description.Replace("\\", "");
                                //要移除空标签和行内样式
                                Regex regex = new Regex(@"<table [\s]*>[\s]*(<br[\s]?/>)?[\s]*</table>", RegexOptions.IgnoreCase);
                                createModel.Description.Description = regex.Replace(createModel.Description.Description, "");

                                regex = new Regex("width\\s*=\\s*\\S+ height\\s*=\\s*\\S+");
                                createModel.Description.Description = regex.Replace(createModel.Description.Description, "");

                                //createModel.Description.Description = Regex.Replace(createModel.Description.Description, @"style\s*=(['""\s]?)[^'""]*?\1", "");
                                //createModel.Description.Description = Regex.Replace(createModel.Description.Description, @"(?i)font-[^\-]+:((?!pt|;).)+(pt|;)?", "");

                                //移除外链
                                createModel.Description.Description = Regex.Replace(createModel.Description.Description, "<a[^>]+>", "");
                                createModel.Description.Description = Regex.Replace(createModel.Description.Description, "<area[^>]+>", "");

                                createModel.Description.Description = Core.Helper.HtmlContentHelper.TransferToLocalImage(createModel.Description.Description, "/", imageRealtivePath, Core.MallIO.GetImagePath(imageRealtivePath) + "/", true);//处理详情图片
                                createModel.Description.MobileDescription = createModel.Description.Description;//移动端详情读取PC端的详情
                            }
                            if (createModel.Description != null && !string.IsNullOrWhiteSpace(createModel.Description.Description))
                            {
                                percenet = percenet + infoPercenet;
                                if (ComfirmCancle(currentTime, guid)) return;
                                Cache.Insert(CacheKeyCollection.CACHE_IMPORTPRODUCTPERCENT(CurrentShop.Id + guid), percenet + "@" + index);
                            }
                            var aLias = new List<SkuProperty>();
                            #region 读取规格，规格值，规格值ID
                            string hasAddSku = "";
                            var skuPropertys = item.ProductInfo.skuProperty;//规格名，规格值，规格图片要处理
                            List<ProductImportSKU> listSku = new List<ProductImportSKU>();
                            if (skuPropertys != null && skuPropertys.Count > 0 && typeInfo != null)
                            {
                                //List<SpecificationValueInfo> specifications =  EngineContext.Current.Resolve<ISpecificationService>().GetSpecification(model.SmallCategoryId, this.CurrentShop.Id);

                                List<SpecificationValueInfo> specifications =  EngineContext.Current.Resolve<ISpecificationService>().GetSpecification(model.SmallCategoryId, this.CurrentShop.Id);

                                if (typeInfo.IsSupportColor)
                                {
                                    SkuProperty colorItem = skuPropertys.Where(p => !string.IsNullOrEmpty(p.SkuPropertyValues.FirstOrDefault().PropertyValueIcon)).FirstOrDefault();
                                    if (colorItem == null)
                                    {
                                        colorItem = skuPropertys.Where(p => p.PropertyName.Contains("颜色") || p.PropertyName.Contains("色") || p.PropertyName.Contains("色彩")).FirstOrDefault();
                                    }
                                    if (colorItem == null)
                                    {
                                        colorItem = skuPropertys.FirstOrDefault();
                                    }
                                   if (colorItem != null)
                                    {
                                        colorItem.Type = 1;
                                        var spitems = specifications.Where(a => a.Specification == SpecificationType.Color).ToList();
                                        var skuPropertyValues = colorItem.SkuPropertyValues.Take(spitems.Count).ToList();
                                        for (int j = 0; j < skuPropertyValues.Count; j++)
                                        {
                                            skuPropertyValues[j].ValueId = spitems[j].Id;
                                            if (skuPropertyValues[j].PropertyValueIcon != "htt")
                                                skuPropertyValues[j].PropertyValueIcon = Core.Helper.HtmlContentHelper.TransferNetworkImageToLocal(skuPropertyValues[j].PropertyValueIcon, "/", imageRealtivePath, Core.MallIO.GetImagePath(imageRealtivePath) + "/");//处理规格图片
                                        }
                                        colorItem.SkuPropertyValues = skuPropertyValues;
                                        listSku.AddRange(colorItem.SkuPropertyValues.Select(s => new ProductImportSKU
                                        {
                                            Type = colorItem.Type,
                                            PropertyCode = colorItem.PropertyCode,
                                            PropertyName = colorItem.PropertyName,
                                            PropertyValueCode = s.PropertyValueCode,
                                            PropertyValueIcon = s.PropertyValueIcon,
                                            PropertyValueName = s.PropertyValueName,
                                            ValueId = s.ValueId,
                                        }));
                                        aLias.Add(colorItem);
                                        createModel.ColorAlias = colorItem.PropertyName;
                                        if (!string.IsNullOrEmpty(hasAddSku)) hasAddSku += ",";
                                        hasAddSku += colorItem.PropertyCode;
                                    }
                                }
                                 if (typeInfo.IsSupportSize)
                                {
                                    var sizeItem = skuPropertys.Where(p => !hasAddSku.Contains(p.PropertyCode) && (p.PropertyName.Contains("尺码") || p.PropertyName.Contains("尺寸") || p.PropertyName.Contains("码"))).FirstOrDefault();
                                    if (sizeItem == null)
                                    {
                                        if (aLias != null && aLias.Count > 0)
                                        {
                                            sizeItem = skuPropertys.Where(p => !hasAddSku.Contains(p.PropertyCode)).FirstOrDefault();
                                        }
                                        else
                                        {
                                            sizeItem = skuPropertys.FirstOrDefault();
                                        }
                                    }
                                    if (sizeItem != null)
                                    {
                                        sizeItem.Type = 2;
                                        var spitems = specifications.Where(a => a.Specification == SpecificationType.Size).ToList();
                                        var skuPropertyValues = sizeItem.SkuPropertyValues.Take(spitems.Count).ToList();
                                        for (int j = 0; j < skuPropertyValues.Count; j++)
                                        {
                                            skuPropertyValues[j].ValueId = spitems[j].Id;
                                        }
                                        sizeItem.SkuPropertyValues = skuPropertyValues;
                                        listSku.AddRange(sizeItem.SkuPropertyValues.Select(s => new ProductImportSKU
                                        {
                                            Type = sizeItem.Type,
                                            PropertyCode = sizeItem.PropertyCode,
                                            PropertyName = sizeItem.PropertyName,
                                            PropertyValueCode = s.PropertyValueCode,
                                            PropertyValueIcon = s.PropertyValueIcon,
                                            PropertyValueName = s.PropertyValueName,
                                            ValueId = s.ValueId,
                                        }));
                                        aLias.Add(sizeItem);
                                        createModel.SizeAlias = sizeItem.PropertyName;
                                        if (!string.IsNullOrEmpty(hasAddSku)) hasAddSku += ",";
                                        hasAddSku += sizeItem.PropertyCode;
                                    }
                                }
                                if (typeInfo.IsSupportVersion)
                                {
                                    SkuProperty versionItem = null;
                                    if (aLias != null && aLias.Count > 0)
                                    {
                                        versionItem = skuPropertys.Where(p => !hasAddSku.Contains(p.PropertyCode)).FirstOrDefault();
                                    }
                                    else
                                    {
                                        versionItem = skuPropertys.FirstOrDefault();
                                    }
                                    if (versionItem != null)
                                    {
                                        versionItem.Type = 3;
                                        var spitems = specifications.Where(a => a.Specification == SpecificationType.Version).ToList();
                                        var skuPropertyValues = versionItem.SkuPropertyValues.Take(spitems.Count).ToList();
                                        for (int j = 0; j < skuPropertyValues.Count; j++)
                                        {
                                            skuPropertyValues[j].ValueId = spitems[j].Id;
                                        }
                                        versionItem.SkuPropertyValues = skuPropertyValues;
                                        listSku.AddRange(versionItem.SkuPropertyValues.Select(s => new ProductImportSKU
                                        {
                                            Type = versionItem.Type,
                                            PropertyCode = versionItem.PropertyCode,
                                            PropertyName = versionItem.PropertyName,
                                            PropertyValueCode = s.PropertyValueCode,
                                            PropertyValueIcon = s.PropertyValueIcon,
                                            PropertyValueName = s.PropertyValueName,
                                            ValueId = s.ValueId,
                                        }));
                                        aLias.Add(versionItem);
                                        createModel.VersionAlias = versionItem.PropertyName;
                                    }
                                }

                                //商家修改的规格值
                                List<SpecificationValue> updateSpecs = listSku.Select(ls => new SpecificationValue
                                {
                                    Id = ls.ValueId,
                                    Specification = ls.Type == 1 ? SpecificationType.Color : (ls.Type == 2 ? SpecificationType.Size : SpecificationType.Version),
                                    TypeId = createModel.TypeId,
                                    Value = ls.PropertyValueName
                                }).ToList();

                                createModel.UpdateSpecs = updateSpecs.ToArray();

                                if (item.ProductInfo.valItemInfo != null && item.ProductInfo.valItemInfo.skuMap != null && item.ProductInfo.valItemInfo.skuMap.Count > 0)
                                {
                                    var skuList = item.ProductInfo.valItemInfo.skuList;
                                    var skuMap = item.ProductInfo.valItemInfo.skuMap;
                                    var skuColor = listSku.Where(p => p.Type == 1).Select(p => p.PropertyValueCode);
                                    if (skuColor != null && skuColor.Count() > 0)
                                    {
                                        skuMap = skuMap.Where(p => skuColor.Any(s => p.skuId.Contains(s))).ToList();
                                    }
                                    var skuSize = listSku.Where(p => p.Type == 2).Select(p => p.PropertyValueCode);
                                    if (skuSize != null && skuSize.Count() > 0)
                                    {
                                        skuMap = skuMap.Where(p => skuSize.Any(s => p.skuId.Contains(s))).ToList();
                                    }
                                    var skuVersion = listSku.Where(p => p.Type == 3).Select(p => p.PropertyValueCode);
                                    if (skuVersion != null && skuVersion.Count() > 0)
                                    {
                                        skuMap = skuMap.Where(p => skuVersion.Any(s => p.skuId.Contains(s))).ToList();
                                    }
                                    createModel.HasSKU = true;
                                    List<SKUEx> skuExs = new List<SKUEx>();//组装SKUEx
                                    SKUEx skuInfo = null;
                                    foreach (var skuMapItem in skuMap)
                                    {
                                        skuInfo = new SKUEx();
                                        long stock = 0;
                                        long.TryParse(skuMapItem.stock, out stock);
                                        skuInfo.CostPrice = TypeHelper.StringToDecimal(skuMapItem.price);
                                        skuInfo.SalePrice = skuInfo.CostPrice;
                                        skuInfo.Stock = stock;
                                        if (!string.IsNullOrWhiteSpace(skuMapItem.sku))
                                        {
                                            skuInfo.Sku = skuMapItem.sku;
                                        }
                                        else
                                        {
                                            skuInfo.Sku = createModel.ProductCode + "_" + (skuMap.IndexOf(skuMapItem) + 1);
                                        }
                                        var skuids = skuMapItem.skuId;
                                        var a = skuids.Split(';');
                                        foreach (var b in a)
                                        {
                                            var c = b.Split(':');
                                            var ci = listSku.Where(p => p.PropertyValueCode == c[1]).FirstOrDefault();
                                            if (ci != null)
                                            {
                                                if (ci.Type == 1)
                                                {
                                                    skuInfo.Color = ci.PropertyValueName;
                                                    skuInfo.ColorId = ci.ValueId;
                                                    skuInfo.ShowPic = ci.PropertyValueIcon;
                                                }
                                                else if (ci.Type == 2)
                                                {
                                                    skuInfo.Size = string.IsNullOrWhiteSpace(ci.PropertyValueName) ? "" : HttpUtility.HtmlDecode(ci.PropertyValueName);
                                                    skuInfo.SizeId = ci.ValueId;
                                                }
                                                else if (ci.Type == 3)
                                                {
                                                    skuInfo.Version = ci.PropertyValueName;
                                                    skuInfo.VersionId = ci.ValueId;
                                                }
                                            }
                                            else{ continue; }
                                        }
                                        if (skuInfo.SizeId == 0 && skuInfo.ColorId == 0 && skuInfo.VersionId == 0)
                                        {
                                            continue;
                                        }
                                        skuInfo.Id = skuInfo.CreateId(null);
                                        if (!skuExs.Exists(p => p.Id == skuInfo.Id))
                                            skuExs.Add(skuInfo);
                                    }
                                    createModel.SKUExs = skuExs.Where(a => a.ColorId > 0 || a.SizeId > 0 || a.VersionId > 0).ToArray();
                                    if (createModel.SKUExs == null || (createModel.SKUExs != null && createModel.SKUExs.Count() == 0))
                                    {
                                        if (skuExs != null && skuExs.Count > 0)
                                        {
                                            createModel.Quantity = (int)skuExs.Max(a => a.Stock);
                                        }
                                    }
                                    percenet = percenet + infoPercenet;
                                    if (ComfirmCancle(currentTime, guid)) return;
                                    Cache.Insert(CacheKeyCollection.CACHE_IMPORTPRODUCTPERCENT(CurrentShop.Id + guid), percenet + "@" + index);
                                }
                            }
                            #endregion
                            
                        }
                        //1、淘宝商品本身没规格，无论平台类型是否勾选规格，都取页面上有的库存值
                        if (createModel.Quantity == null && item.ProductInfo.NoSkuInfo != null && item.ProductInfo.NoSkuInfo.data != null && item.ProductInfo.NoSkuInfo.data.dynStock != null)//则导入的是无规格的商品
                        {
                            createModel.Quantity = Mall.Core.Helper.TypeHelper.StringToInt(item.ProductInfo.NoSkuInfo.data.dynStock.stock);
                        }
                        //2、淘宝商品本身有规格，Mall平台的类型没勾选规格，则取规格中库存最大的项
                        if (createModel.Quantity == null && item.ProductInfo.valItemInfo != null && item.ProductInfo.valItemInfo.skuMap != null && item.ProductInfo.valItemInfo.skuMap.Count > 0)
                        {
                            createModel.Quantity = item.ProductInfo.valItemInfo.skuMap.Select(a => Mall.Core.Helper.TypeHelper.StringToInt(a.stock)).Max();
                        }

                        var product = AutoMapper.Mapper.DynamicMap<DTO.Product.Product>(createModel);
                        ProductLadderPrice[] prices = new ProductLadderPrice[] { };
                        ProductAttribute[] attributes = new ProductAttribute[] { };
                        var sellerSpecs = createModel.GetSellerSpecification(this.CurrentSellerManager.ShopId, product.TypeId);
                        if (createModel.SKUExs != null && createModel.SKUExs.Count() == 0 || (createModel.SKUExs == null))
                        {
                            List<SKUEx> exList = new List<SKUEx>();
                            SKUEx ex = new SKUEx()
                            {
                                Sku = createModel.ProductCode,
                                Stock = createModel.Quantity.HasValue ? createModel.Quantity.Value : 0,
                                SalePrice = createModel.MinSalePrice
                            };
                            ex.Id = ex.CreateId(null);
                            exList.Add(ex);
                            createModel.SKUExs = exList.ToArray();
                        }
                        var skus = new SKUEx[] { };
                        if (createModel.SKUExs != null)
                        {
                            skus = createModel.SKUExs.ToArray();
                        }

                        if (ComfirmCancle(currentTime, guid)) return;

                        bool success = false;
                        var productDto = Mall.Application.ProductManagerApplication.AddProduct(this.CurrentShop.Id, product,
                           createModel.Pics, skus, createModel.Description, attributes, createModel.GoodsCategory, sellerSpecs, prices);
                        success = true;
                        ids.Add(productDto.Id);
                        Cache.Insert(CacheKeyCollection.CACHE_CACHEIMPORT(CurrentShop.Id + guid), ids);
                        //添加商家操作日志
                        LogInfo logInfo = new Entities.LogInfo
                        {
                            Date = DateTime.Now,
                            Description = string.Format("商家导入淘宝/天猫商品数据, [{0}]", success ? "成功" : "失败"),
                            PageUrl = createModel.GrabUrl,
                            UserName = CurrentSellerManager.UserName,
                            ShopId = CurrentSellerManager.ShopId,
                            IPAddress = ip
                        };
                        OperationLogApplication.AddSellerOperationLog(logInfo);
                        successNum++;
                        if (list.IndexOf(item) + 1 == list.Count)//如果导入完最后一个，则100%
                        {
                            percenet = 100;
                        }
                        Cache.Insert(CacheKeyCollection.CACHE_IMPORTPRODUCTPERCENT(CurrentShop.Id + guid), percenet + "@" + index);
                    }

                    result.SuccessNum = successNum;
                    result.FailNum = failNum;
                    Cache.Insert(CacheKeyCollection.CACHE_IMPORTPRODUCTRESULT(CurrentShop.Id + guid), result);//处理后将结果写入缓存
                }
                catch (Exception e)
                {
                    Core.Log.Error("导入淘宝/天猫商品异常", e);
                }
            });
            return Json(new { success = true, total = model.GrabUrl.Count, guid = guid });
        }
        private bool ComfirmCancle(DateTime current, string guid)
        {
            //如果确认取消
            var cancleImport = Cache.Get<object>(CacheKeyCollection.CACHE_IMPORTCANCLE(CurrentShop.Id + guid));
            if (cancleImport != null)
            {
                var flag = int.Parse(cancleImport.ToString().Split('=')[0]);
                var time = DateTime.Parse(cancleImport.ToString().Split('=')[1]);
                if (flag == 1 && time >= current)
                {
                    Cache.Remove(CacheKeyCollection.CACHE_IMPORTPRODUCTPERCENT(this.CurrentShop.Id + guid));//当确认离开后，清除导入数据和百分比
                    Cache.Remove(CacheKeyCollection.CACHE_IMPORTPRODUCTRESULT(this.CurrentShop.Id + guid));
                    Cache.Remove(CacheKeyCollection.CACHE_CACHEIMPORT(this.CurrentShop.Id + guid));
                    Cache.Remove(CacheKeyCollection.CACHE_IMPORTCANCLE(this.CurrentShop.Id + guid));
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 取消导入商品
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CancleImport(string guid)
        {
            Cache.Insert(CacheKeyCollection.CACHE_IMPORTCANCLE(CurrentShop.Id + guid), 1 + "=" + DateTime.Now);
            Cache.Remove(CacheKeyCollection.CACHE_IMPORTPRODUCTPERCENT(this.CurrentShop.Id + guid));//当确认离开后，清除导入数据和百分比
            Cache.Remove(CacheKeyCollection.CACHE_IMPORTPRODUCTRESULT(this.CurrentShop.Id + guid));
            Cache.Remove(CacheKeyCollection.CACHE_CACHEIMPORT(this.CurrentShop.Id + guid));
            //Cache.Remove(CacheKeyCollection.CACHE_IMPORTCANCLE(this.CurrentShop.Id));
            //cancelTokenSource.Cancel();
            return Json(new
            {
                success = true
            });
        }

        [HttpPost]
        public JsonResult RefreshImportProduct(string guid)
        {
            var importCache = Cache.Get<object>(CacheKeyCollection.CACHE_IMPORTPRODUCTPERCENT(CurrentShop.Id + guid));

            decimal value = 0; int index = 0;
            if (importCache != null)
            {
                var percent = importCache.ToString().Split('@');
                if (percent.Length == 2)
                {
                    value = TypeHelper.StringToDecimal(percent[0]);
                    index = TypeHelper.StringToInt(percent[1]);
                }
            }

            if (value == 100)
            {
                //var result = Cache.Get<object>(CacheKeyCollection.CACHE_IMPORTPRODUCTRESULT(CurrentShop.Id));
                Cache.Remove(CacheKeyCollection.CACHE_IMPORTPRODUCTPERCENT(CurrentShop.Id + guid));
                Cache.Remove(CacheKeyCollection.CACHE_CACHEIMPORT(CurrentShop.Id + guid));
                //Cache.Remove(CacheKeyCollection.CACHE_IMPORTPRODUCTRESULT(CurrentShop.Id));
                return Json(new
                {
                    success = true,
                    value = value,
                    index = index
                    //result = result as SpiderProductResult
                });
            }
            return Json(new
            {
                success = false,
                value = value,
                index = index
            });
        }

        /// <summary>
        /// 规格名称检查
        /// </summary>
        /// <param name="str"></param>
        /// <param name="excludeWordList"></param>
        /// <returns></returns>
        static bool CheckSkuName(string str, ICollection<string> excludeWordList = null)
        {
            if (str.Trim().Length <= 0 || excludeWordList == null || excludeWordList.Count <= 0)
            {
                return false;
            }
            return excludeWordList.Any(s => str.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// 取平台类目
        /// </summary>
        /// lly 2015-02-06
        /// <param name="key"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        [UnAuthorize]
        [HttpPost]
        public JsonResult GetPlatFormCategory(long? key = null, int? level = -1)
        {
            if (level == -1)
                key = 0;

            if (key.HasValue)
            {
                //var shopcategories =  EngineContext.Current.Resolve<IShopCategoryService>().GetBusinessCategory(CurrentSellerManager.ShopId, this.CurrentShop.IsSelf).Select(e => e.Id);

                var shopcategories =  EngineContext.Current.Resolve<IShopCategoryService>().GetBusinessCategory(CurrentSellerManager.ShopId, this.CurrentShop.IsSelf).Select(e => e.Id);

                var categories = _iCategoryService.GetCategoryByParentId(key.Value, false);
                categories = categories.Where(a => shopcategories.Contains(a.Id));

                var cateoriesPair = categories.Select(item => new KeyValuePair<long, string>(item.Id, item.Name));
                return Json(cateoriesPair);
            }
            else
                return Json(new object[] { });
        }
        /// <summary>
        /// 取店铺品牌
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [UnAuthorize]
        [HttpPost]
        public JsonResult GetShopBrand(long categoryId)
        {
            var brands = _iBrandService.GetBrandsByCategoryIds(CurrentSellerManager.ShopId, categoryId);
            var brandsPair = brands.Select(item => new KeyValuePair<long, string>(item.Id, item.Name));
            return Json(brandsPair);
        }

        /// <summary>
        /// 取导入记录条数
        /// </summary>
        /// <returns></returns>
        public JsonResult GetImportCount()
        {
            int lngCount = 0, lngTotal = 0;
            int intSuccess = 0;
            //从缓存取用户导入商品数量
            GetImportCountFromCache(out lngCount, out lngTotal);

            if (lngTotal == lngCount && lngTotal > 0)
            {
                intSuccess = 1;
            }
            return Json(new
            {
                Count = lngCount,
                Total = lngTotal,
                Success = intSuccess
            });
        }
        /// <summary>
        /// 取正在进行导入的人数
        /// </summary>
        /// <returns></returns>
        public JsonResult GetImportOpCount()
        {
            //long lngCount = 0;
            //
            var opcount = Core.Cache.Get<int>(CacheKeyCollection.UserImportOpCount);
            //if (opcount != 0)
            //{
            //    lngCount = string.IsNullOrEmpty(opcount.ToString()) ? 0 : long.Parse(opcount.ToString());
            //}

            return Json(new
            {
                Count = opcount
            });
        }
        /// <summary>
        /// 从缓存读取导入记录数
        /// </summary>
        /// <param name="count"></param>
        /// <param name="total"></param>
        private void GetImportCountFromCache(out int count, out int total)
        {
            count = Core.Cache.Get<int>(CacheKeyCollection.UserImportProductCount(_userid));
            total = Core.Cache.Get<int>(CacheKeyCollection.UserImportProductTotal(_userid));
            //count = objCount == null ? 0 : long.Parse(objCount.ToString());
            //total = objTotal == null ? 0 : long.Parse(objTotal.ToString());
            if (count == total && total > 0)
            {
                Core.Cache.Remove(CacheKeyCollection.UserImportProductCount(_userid));
                Core.Cache.Remove(CacheKeyCollection.UserImportProductTotal(_userid));
            }
        }
    }

    public class AsyncProductImportController : BaseAsyncController
    {
        private ICategoryService _iCategoryService;
        private IProductService _iProductService;
        private ISearchProductService _iSearchProductService;
        public AsyncProductImportController(ICategoryService iCategoryService, IProductService iProductService,
            ISearchProductService iSearchProductService)
        {
            _iCategoryService = iCategoryService;
            _iProductService = iProductService;
            _iSearchProductService = iSearchProductService;
        }

        [UnAuthorize]
        [HttpGet]
        public JsonResult ImportProductJson(long paraCategory, long paraShopCategory, long? paraBrand, int paraSaleStatus, long _shopid, long _userid, long freightId, string file)
        {
            /*
             产品ID/主图
             产品ID/Details/明细图片
            */
            string filePath = Server.MapPath("/temp/" + file);
            string imgpath1 = string.Format(@"/Storage/Shop/{0}/Products", _shopid);
            string imgpath2 = Server.MapPath(imgpath1);
            long brand = 0;
            if (paraBrand.HasValue)
                brand = paraBrand.Value;
            JsonResult result = new JsonResult();
            if (System.IO.File.Exists(filePath))
            {
                ZipHelper.ZipInfo zipinfo = ZipHelper.UnZipFile(filePath);
                if (zipinfo.Success)
                {
                    try
                    {
                        int intCnt = ProcessProduct(paraCategory, paraShopCategory, brand, paraSaleStatus, _shopid, _userid, freightId, zipinfo.UnZipPath, imgpath1, imgpath2);
                        if (intCnt > 0)
                        {
                            result = Json(new { success = true, message = "成功导入【" + intCnt.ToString() + "】件商品" });
                        }
                        else
                        {
                            result = Json(new { success = false, message = "导入【0】件商品，请检查数据包，是否是重复导入" });
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Error("导入商品异常：" + ex.Message);
                        Core.Cache.Remove(CacheKeyCollection.UserImportProductCount(_userid));
                        Core.Cache.Remove(CacheKeyCollection.UserImportProductTotal(_userid));
                        result = Json(new { success = false, message = "导入商品异常:" + ex.Message });
                    }
                }
                else
                {
                    Core.Log.Error("解压文件异常：" + zipinfo.InfoMessage);
                    result = Json(new { success = false, message = "解压出现异常,请检查压缩文件格式" });
                }
            }
            else
            {
                result = Json(new { success = false, message = "上传文件不存在" });
            }
            var opcount = Core.Cache.Get<int>(CacheKeyCollection.UserImportOpCount);
            if (opcount != 0)
            {
                Core.Cache.Insert(CacheKeyCollection.UserImportOpCount, opcount - 1);
            }
            return result;
        }
        /// <summary>
        /// 异步导入商品
        /// </summary>
        /// <param name="paraCategory"></param>
        /// <param name="paraShopCategory"></param>
        /// <param name="paraBrand"></param>
        /// <param name="paraSaleStatus"></param>
        /// <param name="_shopid"></param>
        /// <param name="_userid"></param>
        /// <param name="file">压缩文件名</param>
        /// <returns></returns>
        public void ImportProductAsync(long paraCategory, long paraShopCategory, long? paraBrand, int paraSaleStatus, long _shopid, long _userid, long freightId, string file)
        {
            /*
             产品ID/主图
             产品ID/Details/明细图片
            */
            AsyncManager.OutstandingOperations.Increment();
            Task.Factory.StartNew(() =>
            {
                string filePath = Server.MapPath("/temp/" + file);
                string imgpath1 = string.Format(@"/Storage/Shop/{0}/Products", _shopid);
                string imgpath2 = Server.MapPath(imgpath1);
                long brand = 0;
                if (paraBrand.HasValue)
                    brand = paraBrand.Value;
                JsonResult result = new JsonResult();
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        ZipHelper.ZipInfo zipinfo = ZipHelper.UnZipFile(filePath);
                        if (zipinfo.Success)
                        {

                            int intCnt = ProcessProduct(paraCategory, paraShopCategory, brand, paraSaleStatus, _shopid, _userid, freightId, zipinfo.UnZipPath, imgpath1, imgpath2);
                            if (intCnt > 0)
                            {
                                AsyncManager.Parameters["success"] = true;
                                AsyncManager.Parameters["message"] = "成功导入【" + intCnt.ToString() + "】件商品";
                            }
                            else
                            {
                                Core.Cache.Remove(CacheKeyCollection.UserImportProductCount(_userid));
                                Core.Cache.Remove(CacheKeyCollection.UserImportProductTotal(_userid));
                                AsyncManager.Parameters["success"] = false;
                                AsyncManager.Parameters["message"] = "导入【0】件商品，请检查数据包格式，或是否重复导入";
                            }

                        }
                        else
                        {
                            Core.Log.Error("解压文件异常：" + zipinfo.InfoMessage);
                            AsyncManager.Parameters["success"] = false;
                            AsyncManager.Parameters["message"] = "解压出现异常,请检查压缩文件格式";
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Log.Error("导入商品异常：" + ex.Message);
                        Core.Cache.Remove(CacheKeyCollection.UserImportProductCount(_userid));
                        Core.Cache.Remove(CacheKeyCollection.UserImportProductTotal(_userid));
                        AsyncManager.Parameters["success"] = false;
                        AsyncManager.Parameters["message"] = "导入商品异常:" + ex.Message;
                    }
                }
                else
                {
                    AsyncManager.Parameters["success"] = false;
                    AsyncManager.Parameters["message"] = "上传文件不存在";
                }
                AsyncManager.OutstandingOperations.Decrement();
                var opcount = Core.Cache.Get<int>(CacheKeyCollection.UserImportOpCount);
                if (opcount != 0)
                {
                    Core.Cache.Insert(CacheKeyCollection.UserImportOpCount, opcount - 1);
                }
            });
        }
        public JsonResult ImportProductCompleted(bool success, string message)
        {
            return Json(new { success = success, message = message });
        }
        /// <summary>
        /// 商品明细处理
        /// </summary>
        /// <param name="paraCategory"></param>
        /// <param name="paraShopCategory"></param>
        /// <param name="paraBrand"></param>
        /// <param name="paraSaleStatus"></param>
        /// <param name="_shopid"></param>
        /// <param name="_userid"></param>
        /// <param name="mainpath">压缩文件的路径</param>
        /// <param name="imgpath1">虚拟相对路径</param>
        /// <param name="imgpath2">绝对路径(mappath)包含</param>
        /// <returns></returns>
        private int ProcessProduct(long paraCategory, long paraShopCategory, long paraBrand, int paraSaleStatus, long _shopid, long _userid, long freightId, string mainpath, string imgpath1, string imgpath2)
        {
            int result = 0;
            string strPath = mainpath;
            var category = _iCategoryService.GetCategory(paraCategory);

            if (Directory.Exists(strPath))
            {
                string[] csvfiles = Directory.GetFiles(strPath, "*.csv", SearchOption.AllDirectories);
                string line = string.Empty;
                List<string> cells = new List<string>();
                for (int i = 0; i < csvfiles.Length; i++)
                {
                    StreamReader reader = new StreamReader(csvfiles[i], Encoding.Unicode);
                    string str2 = reader.ReadToEnd();
                    reader.Close();
                    str2 = str2.Substring(str2.IndexOf('\n') + 1);
                    str2 = str2.Substring(str2.IndexOf('\n') + 1);
                    StreamWriter writer = new StreamWriter(csvfiles[i], false, Encoding.Unicode);
                    writer.Write(str2);
                    writer.Close();
                    using (CsvReader reader2 = new CsvReader(new StreamReader(csvfiles[i], Encoding.UTF8), true, '\t'))
                    {
                        int num = 0;
                        while (reader2.ReadNextRecord())
                        {
                            num++;
                            int columnCount = reader2.FieldCount;
                            //string[] heads = reader2.GetFieldHeaders();
                            string strProductName = reader2["宝贝名称"].Replace("\"", "");
                            ProductQuery productQuery = new ProductQuery();
                            productQuery.CategoryId = category.Id;
                            productQuery.ShopId = _shopid;
                            productQuery.KeyWords = strProductName;
                            var iProcudt = _iProductService;

                            var proCount = ProductManagerApplication.GetProductCount(productQuery);
                            if (proCount > 0)
                            {//当前店铺、分类已经存在相同编码的商品
                                result++;
                                Core.Log.Debug(strProductName + " : 商品不能重复导入");
                                Core.Cache.Insert<int>(CacheKeyCollection.UserImportProductCount(_userid), result);
                                continue;
                            }
                            long pid = iProcudt.GetNextProductId();
                            long lngStock = 0;
                            decimal price = decimal.Parse(reader2["宝贝价格"] == string.Empty ? "0" : reader2["宝贝价格"]);
                            var product = new ProductInfo()
                            {
                                Id = pid,
                                TypeId = category.TypeId,
                                AddedDate = DateTime.Now,
                                BrandId = paraBrand,
                                CategoryId = category.Id,
                                CategoryPath = category.Path,
                                MarketPrice = price,
                                ShortDescription = string.Empty,
                                ProductCode = reader2["商家编码"].Replace("\"", ""),
                                ImagePath = "",
                                DisplaySequence = 0,//默认的序号都为0
                                ProductName = strProductName,
                                MinSalePrice = price,
                                ShopId = _shopid,
                                HasSKU = false,//判断是否有多规格才能赋值
                                SaleStatus = paraSaleStatus == 1 ? Entities.ProductInfo.ProductSaleStatus.OnSale : Entities.ProductInfo.ProductSaleStatus.InStock,
                                AuditStatus = Entities.ProductInfo.ProductAuditStatus.WaitForAuditing,
                                FreightTemplateId = freightId,
                                MeasureUnit = "件"
                            };
                            var skus = new List<SKUInfo>() { new SKUInfo()
                                        { Id=string.Format("{0}_{1}_{2}_{3}" , pid , "0" , "0" , "0"),
                                          Stock=long.TryParse(reader2["宝贝数量"],out lngStock)?lngStock:0,
                                          SalePrice=price,
                                          CostPrice=price
                                        }};
                            var description = new ProductDescriptionInfo
                            {
                                AuditReason = "",
                                Description = reader2["宝贝描述"],//.Replace("\"", ""),//不能纯去除
                                DescriptiondSuffixId = 0,
                                DescriptionPrefixId = 0,
                                Meta_Description = string.Empty,
                                Meta_Keywords = string.Empty,
                                Meta_Title = string.Empty,
                                ProductId = pid
                            };
                            //图片处理
                            product.ImagePath = imgpath1 + "//" + product.Id.ToString();
                            if (reader2["新图片"] != string.Empty)
                            {
                                ImportProductImg(product.Id, _shopid, csvfiles[i], reader2["新图片"]);
                            }
                            iProcudt.AddProduct(_shopid, product, null, skus.ToArray(), description, null, new long[] { paraShopCategory }, null, null);
                            //iProcudt.AddProduct(product, description, skus);
                            //_iSearchProductService.AddSearchProduct(product.Id);
                            result++;
                            Core.Log.Debug(strProductName);
                            Core.Cache.Insert<int>(CacheKeyCollection.UserImportProductCount(_userid), result);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 导入主图
        /// </summary>
        /// <param name="pid">产品ID</param>
        /// <param name="path">主图目录</param>
        /// <param name="filenames">主图文件信息</param>
        private void ImportProductImg(long pid, long _shopid, string path, string filenames)
        {
            //ff50d4ebbbe59def9faa2672d7538f44:1:0:|;1cd65d2a2d6b8c5bf1818151e5c982e6:1:1:|;54845d82ec3db3731fd63ebf5568b82d:2:0:1627207:107121|;
            path = path.Replace(Path.GetExtension(path), string.Empty);
            filenames = filenames.Replace("\"", string.Empty);
            string despath = string.Format(@"/Storage/Shop/{0}/Products/{1}", _shopid, pid);
            string[] arrFiles = new string[] { };
            string strDesfilename = string.Empty;
            int intImgCnt = 0;
            filenames.Split(';').ToList().ForEach(item =>
            {
                if (item != string.Empty)
                {
                    string[] strArray = item.Split(':');
                    if (strArray.Length > 0)
                    {
                        arrFiles = Directory.GetFiles(path, strArray[0] + ".*", SearchOption.AllDirectories);

                        intImgCnt += 1;

                        try
                        {
                            string dest = string.Format("{0}\\{1}.png", despath, intImgCnt);

                            //读取文件流
                            FileStream fileStream = new FileStream(arrFiles[0], FileMode.Open, FileAccess.Read);

                            int byteLength = (int)fileStream.Length;
                            byte[] fileBytes = new byte[byteLength];
                            fileStream.Read(fileBytes, 0, byteLength);

                            //文件流关閉,文件解除锁定
                            fileStream.Close();

                            MemoryStream stream = new MemoryStream(fileBytes);

                            //using (Image image = Image.FromFile(arrFiles[0]))
                            //{
                            //    MemoryStream stream = new MemoryStream();

                            //    image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                            Core.MallIO.CreateFile(dest, stream, FileCreateType.Create);
                            //}

                            var imageSizes = EnumHelper.ToDictionary<ImageSize>().Select(t => t.Key);
                            foreach (var imageSize in imageSizes)
                            {
                                string size = string.Format("{0}/{1}_{2}.png", despath, intImgCnt, imageSize);
                                Core.MallIO.CreateThumbnail(dest, size, imageSize, imageSize);
                            }
                        }
                        catch (FileNotFoundException fex)
                        {
                            Core.Log.Error("导入商品处理图片时，没有找到文件", fex);
                        }
                        catch (System.Runtime.InteropServices.ExternalException eex)
                        {
                            Core.Log.Error("导入商品处理图片时，ExternalException异常", eex);
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Error("导入商品处理图片时，Exception异常", ex);
                        }
                        //IOHelper.CopyFile(source, Server.MapPath(dest), true);

                    }
                }
            });

        }
    }
}