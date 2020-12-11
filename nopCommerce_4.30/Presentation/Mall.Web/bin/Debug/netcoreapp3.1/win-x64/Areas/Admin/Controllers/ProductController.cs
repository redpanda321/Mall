using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Web.Areas.Admin.Models.Product;
using Mall.Web.Framework;
using Mall.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mall.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class ProductController : BaseAdminController
    {
        private IProductService _iProductService;
        private IBrandService _iBrandService;
        private ICategoryService _iCategoryService;
        private IShopService _iShopService;
        private ISearchProductService _iSearchProductService;
        private IShopCategoryService _iShopCategoryService;
        public ProductController(
            IProductService iProductService,
            IBrandService iBrandService,
            ICategoryService iCategoryService,
            IShopService iShopService,
            ISearchProductService iSearchProductService,
            IShopCategoryService iShopCategoryService
            )
        {
            _iProductService = iProductService;
            _iBrandService = iBrandService;
            _iCategoryService = iCategoryService;
            _iShopService = iShopService;
            _iSearchProductService = iSearchProductService;
            _iShopCategoryService = iShopCategoryService;
        }
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Management()
        {
            return View();
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult List(long? categoryId, string brandName, string productCode, int? auditStatus, string ids, int page, int rows, string keyWords, string shopName, int? saleStatus, bool? isAsc, string sort = "", sbyte? productType = null)
        {
            var query = new ProductQuery()
            {
                PageSize = rows,
                PageNo = page,
                BrandName = brandName,
                KeyWords = keyWords,
                CategoryId = categoryId,
                Ids = string.IsNullOrWhiteSpace(ids) ? null : ids.Split(',').Select(item => long.Parse(item)),
                ShopName = shopName,
                ProductCode = productCode,
                IsAsc = isAsc.HasValue ? isAsc.Value : false,
                Sort = sort
            };
            if (productType.HasValue && productType.Value > -1)
            {
                query.ProductType = productType.Value;
            }
            if (isAsc.HasValue && string.IsNullOrEmpty(query.Sort))//如果按序号排序不为null
            {
                query.OrderKey = 5;//4商家，5平台
                query.OrderType = isAsc.Value;
            }
            if (auditStatus.HasValue)
            {
                query.AuditStatus = new ProductInfo.ProductAuditStatus[] { (ProductInfo.ProductAuditStatus)auditStatus };
                if (auditStatus == (int)ProductInfo.ProductAuditStatus.WaitForAuditing)
                    query.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
            }
            if (saleStatus.HasValue)
                query.SaleStatus = (ProductInfo.ProductSaleStatus)saleStatus;

            var products = ProductManagerApplication.GetProducts(query);

            var brands = BrandApplication.GetBrandsByIds(products.Models.Select(p => p.BrandId).ToList());
            var categories = CategoryApplication.GetCategories();
            var shops = ShopApplication.GetShops(products.Models.Select(p => p.ShopId).ToList());

            var list = products.Models.Select(item =>
           {
               var description = ProductManagerApplication.GetProductDescription(item.Id);
               return new ProductModel()
               {
                   //TODO:FG 循环内查询调用提取
                   name = item.ProductName,
                   brandName = brands.FirstOrDefault(p => p.Id == item.BrandId)?.Name ?? string.Empty,
                   categoryName = categories.FirstOrDefault(p => p.Id == item.CategoryId)?.Name ?? string.Empty,
                   id = item.Id,
                   imgUrl = item.GetImage(ImageSize.Size_50),
                   price = item.MinSalePrice,
                   state = item.ShowProductState,
                   auditStatus = (int)item.AuditStatus,
                   url = "",
                   auditReason = description?.AuditReason ?? string.Empty,
                   shopName = shops.FirstOrDefault(p => p.Id == item.ShopId)?.ShopName ?? string.Empty,
                   saleStatus = (int)item.SaleStatus,
                   productCode = item.ProductCode,
                   saleCounts = item.SaleCounts,
                   AddedDate = item.AddedDate.ToString("yyyy-MM-dd HH:mm"),
                   DisplaySequence = item.DisplaySequence,
                   VirtualSaleCounts = (int)item.VirtualSaleCounts,
                   ProductType = item.ProductType
               };
           });

            return Json(new DataGridModel<ProductModel>() { rows = list, total = products.Total });
        }

        public ActionResult ExportToExcel(long? categoryId = null, string brandName = "", string productCode = "", int? auditStatus = null, string ids = "", string keyWords = "", string shopName = "", int? saleStatus = null, sbyte? productType = null)
        {
            var query = new ProductQuery()
            {
                PageSize = int.MaxValue,
                PageNo = 1,
                BrandName = brandName,
                KeyWords = keyWords,
                CategoryId = categoryId,
                Ids = string.IsNullOrWhiteSpace(ids) ? null : ids.Split(',').Select(item => long.Parse(item)),
                ShopName = shopName,
                ProductCode = productCode,
                NotIncludedInDraft = true
            };
            if (productType.HasValue && productType.Value > -1)
            {
                query.ProductType = productType.Value;
            }
            if (auditStatus.HasValue)
            {
                query.AuditStatus = new ProductInfo.ProductAuditStatus[] { (ProductInfo.ProductAuditStatus)auditStatus };
                if (auditStatus == (int)ProductInfo.ProductAuditStatus.WaitForAuditing)
                    query.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
            }
            if (saleStatus.HasValue)
                query.SaleStatus = (ProductInfo.ProductSaleStatus)saleStatus;


            var data = ProductManagerApplication.GetProducts(query);

            #region 先查询到列表，便于下面循环读取
            List<CategoryInfo> listproductcate = new List<CategoryInfo>();//商品分类
            List<BrandInfo> listbrand = new List<BrandInfo>();//品牌
            List<FreightTemplateInfo> listtemplate = new List<FreightTemplateInfo>();//运费模板
            List<Mall.DTO.ProductShopCategory> listshopcate = new List<DTO.ProductShopCategory>();//商铺分类
            Dictionary<long, string> shopnames = new Dictionary<long, string>();
            if (data.Models != null)
            {
                List<long> cateids = data.Models.Select(p => p.CategoryId).ToList();
                listproductcate = CategoryApplication.GetCateogryListByIds(cateids);//平台分类
                if (listproductcate == null)
                    listproductcate = new List<CategoryInfo>();

                List<long> templateIds = data.Models.Select(p => p.FreightTemplateId).ToList();
                listtemplate = FreightTemplateApplication.GetFreightTemplateList(templateIds);//运费模板
                if (listtemplate == null)
                    listtemplate = new List<FreightTemplateInfo>();//实例下便免下面循环里判断空

                IEnumerable<long> brandIds = data.Models.Select(p => p.BrandId);//品牌
                listbrand = BrandApplication.GetBrands(brandIds);
                if (listbrand == null)
                    listbrand = new List<BrandInfo>();

                List<long> productids = data.Models.Select(p => p.Id).ToList();//商铺分类
                listshopcate = ShopCategoryApplication.GetCategorysByProduct(productids);
                if (listshopcate == null)
                    listshopcate = new List<DTO.ProductShopCategory>();

                List<long> shopids = data.Models.Select(p => p.ShopId).ToList();
                shopnames = ShopApplication.GetShopNames(shopids);
                if (shopnames == null)
                    shopnames = new Dictionary<long, string>();
            }
            #endregion

            var products = data.Models.Select(item =>
            {
                var desc = ProductManagerApplication.GetProductDescription(item.Id);
                var skus = _iProductService.GetSKUs(item.Id);

                var brand = (item.BrandId <= 0) ? null : listbrand.Where(p => p.Id == item.BrandId).FirstOrDefault();//品牌
                var freightTem = (item.FreightTemplateId <= 0) ? null : listtemplate.Where(p => p.Id == item.FreightTemplateId).FirstOrDefault();//运费模板
                var platFormCate = (item.CategoryId <= 0) ? null : listproductcate.Where(p => p.Id == item.CategoryId).FirstOrDefault();//平台分类
                var shopcate = listshopcate.Where(p => p.ProductId == item.Id).FirstOrDefault();//商铺分类

                return new ProductModel()
                {
                    //TODO:FG 循环内查询调用(大数据量)
                    name = item.ProductName,
                    brandName = brand==null ? "" : brand.Name,
                    id = item.Id,
                    imgUrl = item.GetImage(ImageSize.Size_50),
                    price = item.MinSalePrice,
                    state = item.ShowProductState,
                    auditStatus = (int)item.AuditStatus,
                    url = "",
                    auditReason = desc?.AuditReason ?? string.Empty,
                    //shopName = shopService.GetShopBasicInfo(item.ShopId) == null ? "" : shopService.GetShopBasicInfo(item.ShopId).ShopName,
                    shopName = shopnames[item.ShopId],
                    saleStatus = (int)item.SaleStatus,
                    productCode = item.ProductCode,
                    
                    categoryName = platFormCate == null ? "" : platFormCate.Name, //平台分类
                    ShopCategoryName = shopcate == null ? "" : shopcate.ShopCategoryName,//商家分类
                    AuditStatusText = (item.AuditStatus == ProductInfo.ProductAuditStatus.Audited ? "已审核" : "待审核"),
                    MeasureUnit = item.MeasureUnit,
                    HasSKU = item.HasSKU,
                    SKUInfo = skus,
                    FreightTemplateName = freightTem == null ? "" : freightTem.Name,   //运费模板名称
                    IsOpenLadder = item.IsOpenLadder,   //是否开启阶梯批发
                    ProductLadderPrice = ProductManagerApplication.GetLadderPriceInfosByProductId(item.Id, item.IsOpenLadder),
                    ProductTypeName = item.ProductType == 1 ? "虚拟商品" : "实物商品",   //商品类型(虚拟或实物)
                    VirtualProduct = (item.ProductType == 1 ? ProductManagerApplication.GetVirtualProductInfoByProductId(item.Id) : null),  //虚拟商品
                    AuditReason = desc==null?"":desc.AuditReason,//审核备注
                    AddedDate=item.AddedDate.ToString(),       //发布日期
                    MarketPrice=item.MarketPrice,   //市场价
                };
            });

            #region 构建Excel文档
            ViewData.Model = products;
            string viewHtml = RenderPartialViewToString(this, "ExportProductinfo");
            return File(System.Text.Encoding.UTF8.GetBytes(viewHtml), "application/ms-excel", string.Format("平台商品信息_{0}.xls", DateTime.Now.ToString("yyyy-MM-dd")));
            #endregion
        }

        [NonAction]
        protected string RenderPartialViewToString(Controller controller, string partialViewName)
        {

            var razorEngine = EngineContext.Current.Resolve<IRazorViewEngine>();

            IView view = razorEngine.FindView(controller.ControllerContext, partialViewName,false).View;
            using (StringWriter writer = new StringWriter())
            {
                ViewContext viewContext = new ViewContext(controller.ControllerContext, view, controller.ViewData, controller.TempData, writer,null);
                viewContext.View.RenderAsync(viewContext).Wait();
                return writer.ToString();
            }
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="auditState">审核状态</param>
        /// <param name="message">理由</param>
        /// <returns></returns>
        [OperationLog(Message = "审核商品状态")]
        [HttpPost]
        public JsonResult Audit(long productId, int auditState, string message)
        {
            Result result = new Result();
            try
            {
                var status = (Entities.ProductInfo.ProductAuditStatus)auditState;
                _iProductService.AuditProduct(productId, status, message);
                #region 修改搜索商品状态
                _iSearchProductService.UpdateSearchStatusByProduct(productId);
                #endregion
                if (status != Entities.ProductInfo.ProductAuditStatus.Audited)
                {
                    //处理门店
                    ShopBranchApplication.UnSaleProduct(productId);
                }
                result.success = true;
                result.msg = "审核成功！";
            }
            catch (MallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("审核出错", ex);
                result.msg = "审核出错！";
            }
            return Json(result);
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="auditState">审核状态</param>
        /// <param name="message">理由</param>
        /// <returns></returns>
        [OperationLog(Message = "批量审核商品状态")]
        [HttpPost]
        public JsonResult BatchAudit(string productIds, int auditState, string message)
        {
            var productIdsArr = productIds.Split(',').Select(item => long.Parse(item));

            Result result = new Result();
            try
            {
                Entities.ProductInfo.ProductAuditStatus status = (Entities.ProductInfo.ProductAuditStatus)auditState;
                _iProductService.AuditProducts(productIdsArr, status, message);
                _iSearchProductService.UpdateSearchStatusByProducts(productIdsArr.ToList());
                if (status != Entities.ProductInfo.ProductAuditStatus.Audited)
                {
                    foreach (var item in productIdsArr)
                    {
                        //处理门店
                        ShopBranchApplication.UnSaleProduct(item);
                    }
                }
                result.success = true;
                result.msg = "审核成功！";
            }
            catch (MallException ex)
            {
                result.msg = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error("审核出错", ex);
                result.msg = "审核出错！";
            }
            return Json(result);
        }
        [HttpPost]
        public JsonResult GetProductAuditOnOff()
        {
            var sitesetting = SiteSettingApplication.SiteSettings.ProdutAuditOnOff;
            var productSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff;
            return Json(new { value = sitesetting, value2 = productSaleCountOnOff });
        }
        [HttpPost]
        public JsonResult SaveProductAuditOnOff(int value, int value2)
        {
            SiteSettingApplication.SiteSettings.ProdutAuditOnOff = value;
            SiteSettingApplication.SiteSettings.ProductSaleCountOnOff = value2;
            SiteSettingApplication.SaveChanges();
            if (value == 0)
            {
                var count = ProductManagerApplication.GetProductCount(new ProductQuery
                {
                    //参数参照商品管理待审核列表功能
                    AuditStatus = new ProductInfo.ProductAuditStatus[] { ProductInfo.ProductAuditStatus.WaitForAuditing },
                    SaleStatus = ProductInfo.ProductSaleStatus.OnSale
                });
                if (count > 0)//有待审核的商品，提示用户
                    return SuccessResult<dynamic>(code: 1);
            }
            return SuccessResult<dynamic>(code: 0);
        }
        [HttpPost]
        public JsonResult UpdateDisplaySequence(long id, int order)
        {
            if (order < 0)
            {
                return Json(new { success = false, msg = "商品序号必须大于0！" });
            }
            var result = ProductManagerApplication.UpdateDisplaySequence(id, order);
            return Json(new { success = result, msg = "" });
        }

        [HttpPost]
        public JsonResult BtachUpdateSaleCount(string ids, long virtualSaleCounts, int virtualType = 1, int minSaleCount = 0, int maxSaleCount = 0)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return Json(new { success = false, msg = "所选商品不能为空" });
            }
            if (virtualSaleCounts < 0)
            {
                return Json(new { success = false, msg = "虚拟销量不可以小于0" });
            }
            if (virtualType != 2)
            {
                minSaleCount = 0;
                maxSaleCount = 0;
            }
            var productIds = ids.Split(',').Select(item => long.Parse(item)).ToList();
            var result = ProductManagerApplication.BtachUpdateSaleCount(productIds, virtualSaleCounts, minSaleCount, maxSaleCount);
            return Json(new
            {
                success = result,
                msg = ""
            });
        }


    }
}