using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Web.Areas.Web.Models;
using Mall.Web.Framework;
using Mall.Web.Models;
using Microsoft.AspNetCore.Mvc;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;


namespace Mall.Web.Areas.Web.Controllers
{
    public class ProductController : BaseWebController
    {
        #region 字段
        private IProductService _iProductService;
        private IFreightTemplateService _iFreightTemplateService;
        private ICommentService _iCommentService;
        private ITypeService _iTypeService;
        private IOrderService _iOrderService;
        #endregion

        public ProductController(IProductService iProductService, IFreightTemplateService iFreightTemplateService, ICommentService iCommentService,
            ITypeService iTypeService, IOrderService iOrderService)
        {
            _iProductService = iProductService;
            _iFreightTemplateService = iFreightTemplateService;
            _iCommentService = iCommentService;
            _iTypeService = iTypeService;
            _iOrderService = iOrderService;
        }

        #region Ajax加载商品信息

        #region 商品咨询
        public JsonResult GetConsultationByProduct(long pId, int pageNo, int pageSize = 3)
        {
            var data = ConsultationApplication.GetConsultations(new ConsultationQuery
            {
                ProductID = pId,
                PageNo = pageNo,
                PageSize = pageSize
            });

            var list = data.Models.Select(c => new
            {
                UserName = c.UserName.Substring(0, 1) + c.UserName.Substring(1, 1) + "***" + c.UserName.Substring(c.UserName.Length - 1, 1),
                ConsultationContent = c.ConsultationContent,
                ConsultationDate = c.ConsultationDate.ToString("yyyy-MM-dd HH:mm:ss"),
                ReplyContent = string.IsNullOrWhiteSpace(c.ReplyContent) ? "暂无回复" : c.ReplyContent,
                ReplyDate = c.ReplyDate.HasValue ? c.ReplyDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : " ",
            });

            return Json(new
            {
                success = true,
                consults = list,
                totalPage = (int)Math.Ceiling((decimal)data.Total / pageSize),
                currentPage = pageNo,
            });
        }

        #endregion

        #region 商品评价
        /// <summary>
        /// 获取商品评论
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pageNo"></param>
        /// <returns></returns>
        public JsonResult GetCommentByProduct(long pId, int pageNo, int commentType = 0, int pageSize = 3)
        {
            var query = new ProductCommentQuery
            {
                ProductId = pId,
                PageNo = pageNo,
                PageSize = pageSize,
            };
            switch (commentType)
            {
                case 1: query.CommentType = ProductCommentMarkType.High; break;
                case 2: query.CommentType = ProductCommentMarkType.Medium; break;
                case 3: query.CommentType = ProductCommentMarkType.Low; break;
                case 4: query.CommentType = ProductCommentMarkType.HasImage; break;
                case 5: query.CommentType = ProductCommentMarkType.Append; break;
            }
            var comments = CommentApplication.GetComments(query).Models;
            var orderitems = OrderApplication.GetOrderItems(comments.Select(p => (long)p.SubOrderId));
            var orders = OrderApplication.GetOrders(orderitems.Select(p => p.OrderId));
            var commentImages = CommentApplication.GetProductCommentImagesByCommentIds(comments.Select(p => p.Id));
            var members = MemberApplication.GetMembers(comments.Select(p => p.UserId).ToList());

            var data = comments.Select(item =>
            {
                var orderitem = orderitems.FirstOrDefault(p => p.Id == item.SubOrderId);
                var order = orders.FirstOrDefault(p => p.Id == orderitem.OrderId);
                var cimages = commentImages.Where(a => a.CommentId == item.Id);
                var typeInfo = ServiceApplication.Create<ITypeService>().GetTypeByProductId(item.ProductId);
                string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                var productInfo = ProductManagerApplication.GetProduct(item.ProductId);
                if (productInfo != null)
                {
                    colorAlias = !string.IsNullOrWhiteSpace(productInfo.ColorAlias) ? productInfo.ColorAlias : colorAlias;
                    sizeAlias = !string.IsNullOrWhiteSpace(productInfo.SizeAlias) ? productInfo.SizeAlias : sizeAlias;
                    versionAlias = !string.IsNullOrWhiteSpace(productInfo.VersionAlias) ? productInfo.VersionAlias : versionAlias;
                }
                string picture = MallIO.GetRomoteImagePath(members.FirstOrDefault(p => p.Id == item.UserId)?.Photo ?? string.Empty);
                return new
                {
                    Picture = picture,
                    UserName = item.UserName.Substring(0, 1) + item.UserName.Substring(1, 1) + "***" + item.UserName.Substring(item.UserName.Length - 1, 1),
                    ReviewContent = item.ReviewContent,
                    AppendContent = item.AppendContent,
                    AppendDate = item.AppendDate.HasValue ? item.AppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                    ReplyAppendContent = item.ReplyAppendContent,
                    ReplyAppendDate = item.ReplyAppendDate.HasValue ? item.ReplyAppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                    FinshDate = order.FinishDate.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                    Images = cimages.Where(a => a.CommentType == 0).Select(a => new { CommentImage = Core.MallIO.GetImagePath(a.CommentImage) }),
                    AppendImages = cimages.Where(a => a.CommentType == 1).Select(a => new { CommentImage = Core.MallIO.GetImagePath(a.CommentImage) }),
                    ReviewDate = item.ReviewDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    ReplyContent = string.IsNullOrWhiteSpace(item.ReplyContent) ? "暂无回复" : item.ReplyContent,
                    ReplyDate = item.ReplyDate.HasValue ? item.ReplyDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : " ",
                    ReviewMark = item.ReviewMark,
                    BuyDate = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    Color = orderitem?.Color ?? string.Empty,
                    Version = orderitem?.Version ?? string.Empty,
                    Size = orderitem?.Size ?? string.Empty,
                    ColorAlias = colorAlias,
                    SizeAlias = sizeAlias,
                    VersionAlias = versionAlias
                };
            });
            var statistic = CommentApplication.GetProductCommentStatistic(pId);
            return Json(new
            {
                success = true,
                comments = data,
                totalPage = (int)Math.Ceiling((decimal)statistic.AllComment / pageSize),
                pageSize = pageSize,
                currentPage = pageNo,
                goodComment = statistic.HighComment,
                badComment = statistic.LowComment,
                comment = statistic.MediumComment,
                hasAppend = statistic.AppendComment,
                hasImages = statistic.HasImageComment,
                commentType = commentType
            });
        }

        #endregion

        #region 店铺分类
        [HttpGet]
        public JsonResult GetShopCate(long gid)
        {
            var ShopCategory = new List<CategoryJsonModel>();
            var product = ServiceApplication.Create<IProductService>().GetProduct(gid);
            var categories = ServiceApplication.Create<IShopCategoryService>().GetShopCategory(product.ShopId).Where(a => a.IsShow).ToList();
            foreach (var main in categories.Where(s => s.ParentCategoryId == 0))
            {
                var topC = new CategoryJsonModel()
                {
                    Name = main.Name,
                    Id = main.Id.ToString(),
                    SubCategory = new List<SecondLevelCategory>()
                };
                foreach (var secondItem in categories.Where(s => s.ParentCategoryId == main.Id).ToList())
                {
                    var secondC = new SecondLevelCategory()
                    {
                        Name = secondItem.Name,
                        Id = secondItem.Id.ToString(),
                    };

                    topC.SubCategory.Add(secondC);
                }
                ShopCategory.Add(topC);
            }
            return Json(ShopCategory);
        }

        [HttpGet]
        public JsonResult GetShopCateOfficial(long gid)
        {
            var category = new List<CategoryJsonModel>();
            var product = ServiceApplication.Create<IProductService>().GetProduct(gid);
            var productCategory = ServiceApplication.Create<ICategoryService>().GetCategory(product.CategoryId);
            var secondCategory = ServiceApplication.Create<ICategoryService>().GetCategory(productCategory.ParentCategoryId);
            if (secondCategory != null)
            {
                var thridCategories = ServiceApplication.Create<ICategoryService>().GetCategoryByParentId(secondCategory.Id);
                foreach (var thrid in thridCategories)
                {
                    var topC = new CategoryJsonModel()
                    {
                        Name = thrid.Name,
                        Id = thrid.Id.ToString()
                    };
                    category.Add(topC);
                }

            }

            return Json(category);
        }

        [HttpGet]
        public JsonResult GetBrandOfficial(long gid)
        {
            var brands = new List<CategoryJsonModel>();
            var product = ServiceApplication.Create<IProductService>().GetProduct(gid);
            var productCategory = ServiceApplication.Create<ICategoryService>().GetCategory(product.CategoryId);
            var secondCategory = ServiceApplication.Create<ICategoryService>().GetCategory(productCategory.ParentCategoryId);
            if (secondCategory != null)
            {
                var thridCategories = ServiceApplication.Create<ICategoryService>().GetCategoryByParentId(secondCategory.Id).Select(C => C.Id).ToArray();
                var brandsSecond = ServiceApplication.Create<IBrandService>().GetBrandsByCategoryIds(thridCategories);
                foreach (var brand in brandsSecond)
                {
                    var topC = new CategoryJsonModel()
                    {
                        Name = brand.Name,
                        Id = brand.Id.ToString()
                    };
                    brands.Add(topC);
                }

            }

            return Json(brands);
        }
        #endregion

        #region  店铺信息
        public JsonResult GetShopInfo(long sid, long productId = 0)
        {
            string cacheKey = CacheKeyCollection.CACHE_SHOPINFO(sid, productId);
            if (Cache.Exists(cacheKey))
                return Cache.Get<JsonResult>(cacheKey);

            string brandLogo = string.Empty;
            long brandId = 0L;
            decimal cashDeposits = 0M;
            if (productId != 0)
            {
                var product = ServiceApplication.Create<IProductService>().GetProduct(productId);
                if (product != null)
                {
                    var brand = ServiceApplication.Create<IBrandService>().GetBrand(product.BrandId);
                    brandLogo = brand == null ? string.Empty : brand.Logo;
                    brandId = brand == null ? brandId : brand.Id;
                }
            }
            var shop = ServiceApplication.Create<IShopService>().GetShop(sid);
            var mark = Framework.ShopServiceMark.GetShopComprehensiveMark(sid);
            var cashDepositsInfo = ServiceApplication.Create<ICashDepositsService>().GetCashDepositByShopId(sid);
            if (cashDepositsInfo != null)
                cashDeposits = cashDepositsInfo.CurrentBalance;

            var needPay = CashDepositsApplication.GetNeedPayCashDepositByShopId(sid);

            var cashDepositModel = ServiceApplication.Create<ICashDepositsService>().GetCashDepositsObligation(productId);
            var model = new
            {
                CompanyName = shop.CompanyName,
                Id = sid,
                PackMark = mark.PackMark,
                ServiceMark = mark.ServiceMark,
                ComprehensiveMark = mark.ComprehensiveMark,
                Phone = shop.CompanyPhone,
                Name = shop.ShopName,
                Address = ServiceApplication.Create<IRegionService>().GetFullName(shop.CompanyRegionId),
                ProductMark = 3,
                IsSelf = shop.IsSelf,
                BrandLogo = Core.MallIO.GetImagePath(brandLogo),
                BrandId = brandId,
                CashDeposits = cashDeposits,
                CashDepositNeedPay = needPay,//店铺保证金欠费金额
                IsSevenDayNoReasonReturn = cashDepositModel.IsSevenDayNoReasonReturn,
                IsCustomerSecurity = cashDepositModel.IsCustomerSecurity,
                TimelyDelivery = cashDepositModel.IsTimelyShip
            };
            JsonResult result = Json(model, true);
            Cache.Insert(cacheKey, result, 600);
            return result;
        }
        #endregion

        #region 热门销售
        [HttpGet]
        public JsonResult GetHotSaleProduct(long sid)
        {
            var HotSaleProducts = new List<HotProductInfo>();
            var sale = ServiceApplication.Create<IProductService>().GetHotSaleProduct(sid, 5);
            var isSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1;
            if (sale != null)
            {
                decimal discount = 1M;
                long SelfShopId = 0;
                if (CurrentUser != null)
                {
                    discount = CurrentUser.MemberDiscount;
                    var shopInfo = ShopApplication.GetSelfShop();
                    SelfShopId = shopInfo.Id;
                }
                foreach (var item in sale.ToArray())
                {
                    var salePrice = item.MinSalePrice;
                    if (item.ShopId == SelfShopId)
                        salePrice = item.MinSalePrice * discount;
                    var limitBuy = LimitTimeApplication.GetLimitTimeMarketItemByProductId(item.Id);
                    if (limitBuy != null)
                    {
                        salePrice = limitBuy.MinPrice;
                    }
                    HotSaleProducts.Add(new HotProductInfo
                    {
                        ImgPath = Core.MallIO.GetProductSizeImage(item.RelativePath, 1, (int)ImageSize.Size_220),
                        Name = item.ProductName,
                        Price = Math.Round(salePrice, 2),
                        Id = item.Id,
                        SaleCount = (int)item.SaleCounts + Mall.Core.Helper.TypeHelper.ObjectToInt(item.VirtualSaleCounts),
                        IsSaleCountOnOff = isSaleCountOnOff
                    });
                }
            }
            return Json(HotSaleProducts.OrderByDescending(p => p.SaleCount).ToList());
        }
        #endregion

        #region 热门关注
        public JsonResult GetHotConcernedProduct(long sid)
        {
            var HotAttentionProducts = new List<HotProductInfo>();
            var hot = ServiceApplication.Create<IProductService>().GetHotConcernedProduct(sid, 5);
            if (hot != null)
            {
                decimal discount = 1M;
                long SelfShopId = 0;
                if (CurrentUser != null)
                {
                    discount = CurrentUser.MemberDiscount;
                    var shopInfo = ShopApplication.GetSelfShop();
                    SelfShopId = shopInfo.Id;
                }
                foreach (var item in hot.ToArray())
                {
                    var salePrice = item.MinSalePrice;
                    if (item.ShopId == SelfShopId)
                        salePrice = item.MinSalePrice * discount;
                    var limitBuy = LimitTimeApplication.GetLimitTimeMarketItemByProductId(item.Id);
                    if (limitBuy != null)
                    {
                        salePrice = limitBuy.MinPrice;
                    }
                    HotAttentionProducts.Add(new HotProductInfo
                    {
                        ImgPath = Core.MallIO.GetProductSizeImage(item.RelativePath, 1, (int)ImageSize.Size_220),
                        Name = item.ProductName,
                        Price = Math.Round(salePrice, 2),
                        Id = item.Id,
                        SaleCount = (int)item.ConcernedCount
                    });
                }
            }
            return Json(HotAttentionProducts);
        }
        #endregion

        #region 累加浏览次数、 加入历史记录
        [HttpPost]
        public JsonResult LogProduct(long pid)
        {
            if (CurrentUser != null)
            {
                BrowseHistrory.AddBrowsingProduct(pid, CurrentUser.Id);
            }
            else
            {
                BrowseHistrory.AddBrowsingProduct(pid);
            }
            //ServiceApplication.Create<IProductService>().LogProductVisti(pid);
            return Json(null);
        }


        #endregion

        #region 商品属性
        [HttpGet]
        public JsonResult GetProductAttr(long pid)
        {
            List<TypeAttributesModel> ProductAttrs = new List<TypeAttributesModel>();
            var prodAttrs = ProductManagerApplication.GetProductAttributes(pid);
            foreach (var attr in prodAttrs)
            {
                if (!ProductAttrs.Any(p => p.AttrId == attr.AttributeId))
                {
                    var attribute = _iTypeService.GetAttribute(attr.AttributeId);
                    var values = _iTypeService.GetAttributeValues(attr.AttributeId);
                    TypeAttributesModel attrModel = new TypeAttributesModel()
                    {
                        AttrId = attr.AttributeId,
                        AttrValues = new List<TypeAttrValue>(),
                        Name = attribute.Name
                    };
                    foreach (var attrV in values)
                    {
                        if (prodAttrs.Any(p => p.ValueId == attrV.Id))
                        {
                            attrModel.AttrValues.Add(new TypeAttrValue
                            {
                                Id = attrV.Id.ToString(),
                                Name = attrV.Value
                            });
                        }
                    }
                    ProductAttrs.Add(attrModel);
                }
                else
                {
                    var attrTemp = ProductAttrs.FirstOrDefault(p => p.AttrId == attr.AttributeId);
                    var values = _iTypeService.GetAttributeValues(attr.AttributeId);
                    if (!attrTemp.AttrValues.Any(p => p.Id == attr.ValueId.ToString()))
                    {
                        var avinfo = values.FirstOrDefault(a => a.Id == attr.ValueId);
                        if (null != avinfo)
                        {
                            attrTemp.AttrValues.Add(new TypeAttrValue
                            {
                                Id = attr.ValueId.ToString(),
                                Name = avinfo.Value
                            });
                        }
                    }
                }
            }
            return Json(ProductAttrs);
        }

        #endregion

        #region 商品描述
        [HttpGet]
        public JsonResult GetProductDesc(long pid)
        {
            string cacheKey = CacheKeyCollection.CACHE_PRODUCTDESC(pid);
            if (Cache.Exists(cacheKey))
                return Cache.Get<JsonResult>(cacheKey);
            JsonResult result = null;
            var product = ServiceApplication.Create<IProductService>().GetProduct(pid);
            var description = ProductManagerApplication.GetProductDescription(pid);
            if (description != null)
            {
                var productDescription = description.Description;
                string descriptionPrefix = "", descriptiondSuffix = "";
                if (description.DescriptionPrefixId != 0)
                {
                    var desc = ServiceApplication.Create<IProductDescriptionTemplateService>()
                        .GetTemplate(description.DescriptionPrefixId, product.ShopId);
                    descriptionPrefix = desc == null ? "" : desc.Content;
                }

                if (description.DescriptiondSuffixId != 0)
                {
                    var desc = ServiceApplication.Create<IProductDescriptionTemplateService>()
                        .GetTemplate(description.DescriptiondSuffixId, product.ShopId);
                    descriptiondSuffix = desc == null ? "" : desc.Content;
                }
                result = Json(new
                {
                    ProductDescription = productDescription,
                    DescriptionPrefix = descriptionPrefix,
                    DescriptiondSuffix = descriptiondSuffix
                });

                Cache.Insert(cacheKey, result, 600);
                return result;
            }
            result = Json(new
            {
                ProductDescription = "",
                DescriptionPrefix = "",
                DescriptiondSuffix = ""
            });

            Cache.Insert(cacheKey, result, 600);
            return result;
        }
        #endregion

        #region 获取能否购买信息
        [HttpGet]
        public JsonResult GetEnableBuyInfo(long gid)
        {

            var product = _iProductService.GetProduct(gid);
            var hasQuick = CurrentUser == null ? false :
                ServiceApplication.Create<IShippingAddressService>()
                .GetUserShippingAddressByUserId(CurrentUser.Id)
                .Any(s => s.IsQuick);
            var skus = _iProductService.GetSKUs(product.Id);
            return Json(new
            {
                hasQuick = hasQuick ? 1 : 0,
                Logined = (null != CurrentUser) ? 1 : 0,
                hasSKU = skus.Any(s => s.Stock > 0),
                IsOnSale = product.AuditStatus == Entities.ProductInfo.ProductAuditStatus.Audited &&
                product.SaleStatus == Entities.ProductInfo.ProductSaleStatus.OnSale
            });

        }
        #endregion

        #region 获取评论数、咨询数
        [HttpGet]
        public JsonResult GetCommentsNumber(long gid)
        {
            var count = ProductManagerApplication.GetCommentsNumber(gid);
            var commentCount = CommentApplication.GetCommentsByProduct(gid);

            return Json(new
            {
                Comments = commentCount.Count(),
                Consultations = count["consultations"]
            });
        }

        #endregion

        #endregion

        [HttpPost]
        public JsonResult AddFavorite(long shopId)
        {
            try
            {
                ServiceApplication.Create<IShopService>().AddFavoriteShop(CurrentUser.Id, shopId);
            }
            catch (MallException ex)
            {
                return Json(new
                {
                    success = false,
                    msg = ex.Message
                });
            }

            return Json(new
            {
                success = true
            });
        }

        #region 组合购      

        public ActionResult ProductColloCation(long productId)
        {
            var data = CollocationApplication.GetDisplayCollocation(productId);
            return PartialView(data);
        }
        #endregion

        /// <summary>
        /// 商品详情
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partnerid">代理用户编号</param>
        /// <returns></returns>
        //#if DEBUG
        //        [ActionPerformance]
        //#endif
        public ActionResult Detail(long id, long partnerid = 0)
        {
            var productservice = _iProductService;
            var product = productservice.GetProduct(id);
            if (product == null || product.IsDeleted)
                throw new MallException("很抱歉，您查看的商品不存在，可能被转移。");

            var iLimitService = ServiceApplication.Create<ILimitTimeBuyService>();

            var ltmbuy = iLimitService.GetLimitTimeMarketItemByProductId(id);
            if (ltmbuy != null)
            {
                return RedirectToAction("Detail", "LimitTimeBuy", new { id = ltmbuy.Id });
            }

            ProductManagerApplication.GetPCHtml(id);
            string urlHtml = "/Storage/Products/Statics/" + id + ".html";

            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);
            return File(urlHtml, "text/html");
        }


        /// <summary>
        /// 商品详情(用于生成html页面时调用)
        /// </summary>
        /// <param name="productId"></param>      
        /// <returns></returns>
        public ActionResult Details(long id = 0, long partnerid = 0)
        {
            string price = "";
            var productservice = _iProductService;
            var freightTemplateService = _iFreightTemplateService;
            var typeservice = _iTypeService;

            #region 定义Model和变量

            ProductDetailModelForWeb model = new ProductDetailModelForWeb
            {
                HotAttentionProducts = new List<HotProductInfo>(),
                HotSaleProducts = new List<HotProductInfo>(),
                Product = new Entities.ProductInfo(),
                Shop = new ShopInfoModel(),
                ShopCategory = new List<CategoryJsonModel>(),
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU(),
                BoughtProducts = new List<HotProductInfo>()
            };
            #endregion
            Entities.ProductInfo product = null;
            #region 商品Id不合法           
            product = productservice.GetProduct(id);

            if (product == null)
            {
                throw new MallException("很抱歉，您查看的商品不存在，可能被转移。");
            }

            if (product.IsDeleted)
            {
                throw new MallException("很抱歉，您查看的商品不存在，可能被转移。");
                ////跳转到404页面
                //return RedirectToAction("Error404", "Error", new
                //{
                //    area = "Web"
                //});
            }
            #endregion
            if (CurrentUser != null && CurrentUser.Id > 0)
                product.IsFavorite = productservice.IsFavorite(product.Id, CurrentUser.Id);
            Stopwatch watch = new Stopwatch();

            #region 初始化商品和店铺




            #endregion

            #region 店铺信息
            var _shop = ServiceApplication.Create<IShopService>().GetShop(product.ShopId);
            var mark = Framework.ShopServiceMark.GetShopComprehensiveMark(_shop.Id);
            model.Shop.Name = _shop.ShopName;
            model.Shop.CompanyName = _shop.CompanyName;
            model.Shop.Id = _shop.Id;
            model.Shop.PackMark = mark.PackMark;
            model.Shop.ServiceMark = mark.ServiceMark;
            model.Shop.ComprehensiveMark = mark.ComprehensiveMark;
            model.Shop.Phone = _shop.CompanyPhone;
            model.Shop.FreeFreight = _shop.FreeFreight;
            //model.Shop.Address = ServiceApplication.Create<IRegionService>().GetRegionFullName(template == null ? 0L : (template.SourceAddress ?? 0L));
            model.Shop.ProductMark = CommentApplication.GetProductAverageMark(id);

            #endregion

            #region 商品规格

            Entities.TypeInfo typeInfo = typeservice.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            if (product != null)
            {
                colorAlias = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias;
                sizeAlias = !string.IsNullOrWhiteSpace(product.SizeAlias) ? product.SizeAlias : sizeAlias;
                versionAlias = !string.IsNullOrWhiteSpace(product.VersionAlias) ? product.VersionAlias : versionAlias;
            }
            model.ColorAlias = colorAlias;
            model.SizeAlias = sizeAlias;
            model.VersionAlias = versionAlias;
            var skus = ProductManagerApplication.GetSKUs(product.Id);
            if (skus.Count > 0)
            {
                long colorId = 0, sizeId = 0, versionId = 0;
                foreach (var sku in skus.OrderBy(s => s.AutoId).ToList())
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 0 && !string.IsNullOrEmpty(sku.Color))
                    {
                        if (long.TryParse(specs[1], out colorId))
                        {
                        }
                        if (colorId != 0)
                        {
                            if (sku.Color != null && !model.Color.Any(v => v.Value.Equals(sku.Color)))
                            {
                                var c = skus.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.Stock);
                                model.Color.Add(new ProductSKU
                                {
                                    //Name = "选择颜色",
                                    Name = "选择" + colorAlias,
                                    EnabledClass = c != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Color.Any(c1 => c1.SelectedClass.Equals("selected")) && c != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = colorId,
                                    Value = sku.Color,
                                    Img = sku.ShowPic
                                });
                            }
                        }
                    }
                    if (specs.Count() > 1 && !string.IsNullOrEmpty(sku.Size))
                    {
                        if (long.TryParse(specs[2], out sizeId))
                        {
                        }
                        if (sizeId != 0)
                        {
                            if (sku.Size != null && !model.Size.Any(v => v.Value.Equals(sku.Size)))
                            {
                                var ss = skus.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.Stock);
                                model.Size.Add(new ProductSKU
                                {
                                    //Name = "选择尺码",
                                    Name = "选择" + sizeAlias,
                                    EnabledClass = ss != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Size.Any(s1 => s1.SelectedClass.Equals("selected")) && ss != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = sizeId,
                                    Value = sku.Size

                                });
                            }
                        }
                    }

                    if (specs.Count() > 2 && !string.IsNullOrEmpty(sku.Version))
                    {
                        if (long.TryParse(specs[3], out versionId))
                        {
                        }
                        if (versionId != 0)
                        {
                            if (sku.Version != null && !model.Version.Any(v => v.Value.Equals(sku.Version)))
                            {
                                var v = skus.Where(s => s.Version.Equals(sku.Version)).Sum(s => s.Stock);
                                model.Version.Add(new ProductSKU
                                {
                                    //Name = "选择版本",
                                    Name = "选择" + versionAlias,
                                    EnabledClass = v != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Version.Any(v1 => v1.SelectedClass.Equals("selected")) && v != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = versionId,
                                    Value = sku.Version
                                });
                            }
                        }
                    }

                }
                decimal min = 0, max = 0;
                var skusql = skus.Where(s => s.Stock >= 0);
                if (skusql.Count() > 0)
                {
                    min = skusql.Min(s => s.SalePrice);
                    max = skusql.Max(s => s.SalePrice);
                    if (min == 0 && max == 0)
                    {
                        price = product.MinSalePrice.ToString("f2");
                    }
                }
                else if (max > min)
                {
                    price = string.Format("{0}-{1}", min.ToString("f2"), max.ToString("f2"));
                }
                else
                {
                    price = string.Format("{0}", min.ToString("f2"));
                }

            }
            model.Price = string.IsNullOrWhiteSpace(price) ? product.MinSalePrice.ToString("f2") : price;
            #endregion

            #region 限时购预热
            bool isPreheat = false, isNormalPurchase = false;
            FlashSaleModel flashSale = LimitTimeApplication.IsFlashSaleDoesNotStarted(id);
            FlashSaleConfigModel flashSaleConfig = LimitTimeApplication.GetConfig();
            var strFlashSaleTime = string.Empty;
            if (flashSale != null)
            {
                TimeSpan flashSaleTime = DateTime.Parse(flashSale.BeginDate) - DateTime.Now;  //开始时间还剩多久
                if (flashSaleConfig != null)
                {
                    isNormalPurchase = flashSaleConfig.IsNormalPurchase;
                    TimeSpan preheatTime = new TimeSpan(flashSaleConfig.Preheat, 0, 0);  //预热时间是多久
                    if (preheatTime >= flashSaleTime)  //预热大于开始
                    {
                        isPreheat = true;
                    }
                }
            }
            ViewBag.isPrehear = isPreheat && (!isNormalPurchase);
            #endregion
            model.Product = product;
            if (product.ProductType == 0)
            {
                model.FreightTemplate = FreightTemplateApplication.GetFreightTemplate(product.FreightTemplateId);
            }
            var shop = ShopApplication.GetShop(product.ShopId);
            //检查当前产品是否产自官方自营店
            model.IsSellerAdminProdcut = shop.IsSelf;
            model.CouponCount = ServiceApplication.Create<ICouponService>().GetTopCoupon(product.ShopId).Count();
            model.IsExpiredShop = ServiceApplication.Create<IShopService>().IsExpiredShop(product.ShopId);

            #region 获取店铺的评价统计
            var shopStatisticOrderComments = ServiceApplication.Create<IShopService>().GetShopStatisticOrderComments(product.ShopId);

            var productAndDescription = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.ProductAndDescription).FirstOrDefault();
            var sellerServiceAttitude = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerServiceAttitude).FirstOrDefault();
            var sellerDeliverySpeed = shopStatisticOrderComments.Where(c => c.CommentKey ==
                Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerDeliverySpeed).FirstOrDefault();

            var productAndDescriptionPeer = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.ProductAndDescriptionPeer).FirstOrDefault();
            var sellerServiceAttitudePeer = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerServiceAttitudePeer).FirstOrDefault();
            var sellerDeliverySpeedPeer = shopStatisticOrderComments.Where(c => c.CommentKey ==
                Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerDeliverySpeedPeer).FirstOrDefault();

            var productAndDescriptionMax = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.ProductAndDescriptionMax).FirstOrDefault();
            var productAndDescriptionMin = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.ProductAndDescriptionMin).FirstOrDefault();

            var sellerServiceAttitudeMax = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerServiceAttitudeMax).FirstOrDefault();
            var sellerServiceAttitudeMin = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerServiceAttitudeMin).FirstOrDefault();

            var sellerDeliverySpeedMax = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerDeliverySpeedMax).FirstOrDefault();
            var sellerDeliverySpeedMin = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerDeliverySpeedMin).FirstOrDefault();

            decimal defaultValue = 5;
            //宝贝与描述
            if (productAndDescription != null && productAndDescriptionPeer != null)
            {
                model.ProductAndDescription = productAndDescription.CommentValue;
                model.ProductAndDescriptionPeer = productAndDescriptionPeer.CommentValue;
                model.ProductAndDescriptionMin = productAndDescriptionMin.CommentValue;
                model.ProductAndDescriptionMax = productAndDescriptionMax.CommentValue;
            }
            else
            {
                model.ProductAndDescription = defaultValue;
                model.ProductAndDescriptionPeer = defaultValue;
                model.ProductAndDescriptionMin = defaultValue;
                model.ProductAndDescriptionMax = defaultValue;
            }
            //卖家服务态度
            if (sellerServiceAttitude != null && sellerServiceAttitudePeer != null)
            {
                model.SellerServiceAttitude = sellerServiceAttitude.CommentValue;
                model.SellerServiceAttitudePeer = sellerServiceAttitudePeer.CommentValue;
                model.SellerServiceAttitudeMax = sellerServiceAttitudeMax.CommentValue;
                model.SellerServiceAttitudeMin = sellerServiceAttitudeMin.CommentValue;
            }
            else
            {
                model.SellerServiceAttitude = defaultValue;
                model.SellerServiceAttitudePeer = defaultValue;
                model.SellerServiceAttitudeMax = defaultValue;
                model.SellerServiceAttitudeMin = defaultValue;
            }
            //卖家发货速度
            if (sellerDeliverySpeedPeer != null && sellerDeliverySpeed != null)
            {
                model.SellerDeliverySpeed = sellerDeliverySpeed.CommentValue;
                model.SellerDeliverySpeedPeer = sellerDeliverySpeedPeer.CommentValue;
                model.SellerDeliverySpeedMax = sellerDeliverySpeedMax != null ? sellerDeliverySpeedMax.CommentValue : 0;
                model.sellerDeliverySpeedMin = sellerDeliverySpeedMin != null ? sellerDeliverySpeedMin.CommentValue : 0;
            }
            else
            {
                model.SellerDeliverySpeed = defaultValue;
                model.SellerDeliverySpeedPeer = defaultValue;
                model.SellerDeliverySpeedMax = defaultValue;
                model.sellerDeliverySpeedMin = defaultValue;
            }
            #endregion

            #region 买过该商品的还买了
            if (CurrentUser != null)
            {
                var queryModel = new OrderQuery()
                {
                    UserName = CurrentUser.UserName,
                    UserId = CurrentUser.Id,
                    PageSize = 20,
                    PageNo = 1
                };
                var orders = ServiceApplication.Create<IOrderService>().GetOrders<OrderInfo>(queryModel).Models.OrderByDescending(c => c.OrderDate).Select(c => c.Id).ToList();
                var orderItems = ServiceApplication.Create<IOrderService>().GetOrderItemsByOrderId(orders).OrderByDescending(c => c.Id);
                foreach (var orderItem in orderItems)
                {
                    if (model.BoughtProducts.Where(c => c.Id == orderItem.ProductId).Count() > 0)
                        continue;
                    //TODO:[lly]买过该商品的还买了 过滤已删除的商品
                    /* zjt  
					 * productservice 命名应为productService
					 */

                    var productInfo = productservice.GetProduct(orderItem.ProductId);
                    if (productInfo != null)
                    {
                        if (productInfo.IsDeleted)
                        {
                            continue;
                        }
                    }
                    model.BoughtProducts.Add(new HotProductInfo
                    {
                        Id = orderItem.ProductId,
                        Name = orderItem.ProductName,
                        Price = orderItem.SalePrice,
                        ImgPath = orderItem.ThumbnailsUrl
                    });
                    if (model.BoughtProducts.Select(c => c.Id).Distinct().Count() == 20)
                    {
                        break;
                    }
                }
                if (model.BoughtProducts.Count < 5)
                {
                    model.BoughtProducts.Clear();
                }

            }
            #endregion 买过该商品的还买了

            var brandModel = ServiceApplication.Create<IBrandService>().GetBrand(model.Product.BrandId);
            model.Product.BrandName = brandModel == null ? "" : brandModel.Name;

            var map = Core.Helper.QRCodeHelper.Create(CurrentUrlHelper.CurrentUrlNoPort() + "/m-wap/product/detail/" + product.Id);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            //  将图片内存流转成base64,图片以DataURI形式显示  
            string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
            ms.Dispose();
            //  显示  
            model.Code = strUrl;
            ViewBag.IsEnableCashOnDelivery = ServiceApplication.Create<IPaymentConfigService>().IsEnable() && model.Shop.Id == 1;
            model.CashDepositsObligation = ServiceApplication.Create<ICashDepositsService>().GetCashDepositsObligation(product.Id);
            model.Product.VideoPath = model.Product.VideoPath ?? string.Empty;
            //补充当前店铺红包功能
            ViewBag.isShopPage = true;
            ViewBag.CurShopId = product.ShopId;
            TempData["isShopPage"] = true;
            TempData["CurShopId"] = product.ShopId;
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);
            ViewBag.Keyword = SiteSettings.Keyword;
            model.VirtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(product.Id);
            return View(model);
        }

        public JsonResult GetProductDetails(long id = 0, long shopId = 0)
        {
            var productservice = _iProductService;
            var typeservice = _iTypeService;
            var productId = id;
            if (productId == 0)
                return Json(new { data = false });
            var productInfo = productservice.GetNeedRefreshProductInfo(productId);
            ProductDetailModelForWeb model = new ProductDetailModelForWeb
            {
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU()
            };


            Entities.TypeInfo typeInfo = typeservice.GetType(productInfo.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            if (productInfo != null)
            {
                colorAlias = !string.IsNullOrWhiteSpace(productInfo.ColorAlias) ? productInfo.ColorAlias : colorAlias;
                sizeAlias = !string.IsNullOrWhiteSpace(productInfo.SizeAlias) ? productInfo.SizeAlias : sizeAlias;
                versionAlias = !string.IsNullOrWhiteSpace(productInfo.VersionAlias) ? productInfo.VersionAlias : versionAlias;
            }
            var shopInfo = ShopApplication.GetShop(productInfo.ShopId);
            //会员级别比例
            decimal discount = 1M;
            if (CurrentUser != null && shopInfo.IsSelf)
            {
                discount = CurrentUser.MemberDiscount;
            }

            var skus = ProductManagerApplication.GetSKUs(productId);
            #region 商品规格
            var price = string.Empty;
            if (skus != null && skus.Count() > 0)
            {
                model.StockAll = skus.Where(p => p.Id.Contains(productId + "_")).Sum(p => p.Stock);//总库存(它where一次是因为有效规格是“产品ID_”,过滤无效“{0}_”)
                long colorId = 0, sizeId = 0, versionId = 0;
                foreach (var sku in skus.OrderBy(s => s.AutoId).ToList())
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 0 && !string.IsNullOrEmpty(sku.Color))
                    {
                        if (long.TryParse(specs[1], out colorId))
                        {
                        }
                        if (colorId != 0)
                        {
                            if (!model.Color.Any(v => v.Value.Equals(sku.Color)))
                            {
                                var c = skus.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.Stock);
                                model.Color.Add(new ProductSKU
                                {
                                    //Name = "选择颜色",
                                    Name = "选择" + colorAlias,
                                    EnabledClass = c != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Color.Any(c1 => c1.SelectedClass.Equals("selected")) && c != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = colorId,
                                    Value = sku.Color,
                                    Img = Mall.Core.MallIO.GetImagePath(sku.ShowPic)
                                });
                            }
                        }
                    }
                    if (specs.Count() > 1 && !string.IsNullOrEmpty(sku.Size))
                    {
                        if (long.TryParse(specs[2], out sizeId))
                        {
                        }
                        if (sizeId != 0)
                        {
                            if (!model.Size.Any(v => v.Value.Equals(sku.Size)))
                            {
                                var ss = skus.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.Stock);
                                model.Size.Add(new ProductSKU
                                {
                                    //Name = "选择尺码",
                                    Name = "选择" + sizeAlias,
                                    EnabledClass = ss != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Size.Any(s1 => s1.SelectedClass.Equals("selected")) && ss != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = sizeId,
                                    Value = sku.Size
                                });
                            }
                        }
                    }

                    if (specs.Count() > 2 && !string.IsNullOrEmpty(sku.Version))
                    {
                        if (long.TryParse(specs[3], out versionId))
                        {
                        }
                        if (versionId != 0)
                        {
                            if (!model.Version.Any(v => v.Value.Equals(sku.Version)))
                            {
                                var v = skus.Where(s => s.Version.Equals(sku.Version)).Sum(s => s.Stock);
                                model.Version.Add(new ProductSKU
                                {
                                    //Name = "选择版本",
                                    Name = "选择" + versionAlias,
                                    EnabledClass = v != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Version.Any(v1 => v1.SelectedClass.Equals("selected")) && v != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = versionId,
                                    Value = sku.Version
                                });
                            }
                        }
                    }

                }
                
                price = ProductWebApplication.GetProductPriceStr2(productInfo, skus, discount);//最小价或区间价文本
            }
            if (shopInfo.IsSelf)
            {
                model.Price = string.IsNullOrWhiteSpace(price) ? (productInfo.MinSalePrice * discount).ToString("f2") : price;
            }
            else
            {
                model.Price = string.IsNullOrWhiteSpace(price) ? productInfo.MinSalePrice.ToString("f2") : price;
            }
            #endregion
            skus = null;

            var isFavorite = false;
            if (CurrentUser != null && CurrentUser.Id > 0)
                isFavorite = productservice.IsFavorite(productId, CurrentUser.Id);
            #region 物流信息
            //var template = ServiceApplication.Create<IFreightTemplateService>().GetFreightTemplate(productInfo.FreightTemplateId);
            //decimal freight = 0;
            //int cityId = 0;
            //string shippingAddress = "";
            //string shippingValue = string.Empty;
            //string freightStr = string.Empty;
            //string productAddress = string.Empty;
            //if (template != null)
            //{
            //    if (template.SourceAddress.HasValue)
            //    {
            //        var fullName = RegionApplication.GetFullName(template.SourceAddress.Value);
            //        if (fullName != null)
            //        {
            //            var ass = fullName.Split(' ');
            //            if (ass.Length >= 2)
            //            {
            //                productAddress = ass[0] + " " + ass[1];
            //            }
            //            else
            //            {
            //                productAddress = ass[0];
            //            }
            //        }
            //    }
            //    ShippingAddressInfo defaultAddress = null;
            //    if (CurrentUser != null)
            //    {
            //        defaultAddress = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(CurrentUser.Id);
            //    }
            //    int regionId = 0;
            //    if (defaultAddress != null)
            //    {
            //        regionId = defaultAddress.RegionId;
            //    }
            //    else
            //    {
            //        string curip = Mall.Core.Helper.WebHelper.GetIP();
            //        regionId = (int)RegionApplication.GetRegionByIPInTaobao(curip);
            //    }
            //    if (regionId > 0)
            //    {
            //        shippingValue = RegionApplication.GetRegionPath(regionId);

            //        string addrs = RegionApplication.GetFullName(regionId);

            //        shippingAddress = addrs;
            //        if (string.IsNullOrEmpty(shippingAddress))
            //        {
            //            shippingAddress = "请选择";
            //        }
            //        cityId = regionId;// ServiceApplication.Create<IRegionService>.Create.GetCityId(defaultAddress.RegionIdPath);
            //    }
            //}
            //if (template.IsFree == FreightTemplateType.Free)
            //{
            //    freightStr = "卖家承担运费";
            //}
            //else
            //{
            //    if (CurrentUser != null)
            //    {
            //        if (cityId > 0)
            //        {
            //            List<long> productIds = new List<long>();
            //            List<int> counts = new List<int>();
            //            productIds.Add(id);
            //            counts.Add(1);
            //            freight = ServiceApplication.Create<IProductService>().GetFreight(productIds, counts, cityId);
            //            freightStr = "运费 ￥" + freight;
            //        }
            //    }
            //}

            //if (cityId > 0)
            //{
            //    var shopBranchQuery = new ShopBranchQuery();
            //    shopBranchQuery.ShopId = shopId;
            //    shopBranchQuery.Status = ShopBranchStatus.Normal;
            //    shopBranchQuery.ProductIds = new[] { productId };
            //    var cityRegion = RegionApplication.GetRegion(cityId, CommonModel.Region.RegionLevel.City);
            //    shopBranchQuery.AddressPath = cityRegion.GetIdPath();

            //    model.CanSelfTake = ShopBranchApplication.Exists(shopBranchQuery);
            //}

            //model.ProductAddress = productAddress;
            //model.ShippingAddress = shippingAddress;
            //model.ShippingValue = shippingValue;
            //model.Freight = freightStr;
            //#endregion

            //#region 限时购预热
            //var iLimitService = ServiceApplication.Create<ILimitTimeBuyService>();
            //bool isPreheat = false;
            //model.FlashSale = iLimitService.IsFlashSaleDoesNotStarted(id);
            //model.FlashSaleConfig = iLimitService.GetConfig();
            //var strFlashSaleTime = string.Empty;
            //if (model.FlashSale != null)
            //{
            //    TimeSpan flashSaleTime = DateTime.Parse(model.FlashSale.BeginDate) - DateTime.Now;  //开始时间还剩多久
            //    TimeSpan preheatTime = new TimeSpan(model.FlashSaleConfig.Preheat, 0, 0);  //预热时间是多久
            //    if (preheatTime >= flashSaleTime)  //预热大于开始
            //    {
            //        isPreheat = true;
            //        strFlashSaleTime = Math.Floor(flashSaleTime.TotalHours) + "时" + flashSaleTime.Minutes + "分";
            //    }
            //}

            #endregion

            //#region  商品评分等级
            //var productMark = _iCommentService.GetProductMark(id);
            //#endregion
            bool hasFightGroup = false;
            decimal fightFroupPrice = 0;
            string fightGroupUrl = "";
            var fgactobj = FightGroupApplication.GetActiveAndItemsByProductId(id);
            if (fgactobj != null)
            {
                if (fgactobj.ActiveStatus == FightGroupActiveStatus.Ongoing)  //只跳转进行中的活动
                {
                    hasFightGroup = true;
                    if (fgactobj.ActiveItems != null && fgactobj.ActiveItems.Count > 0)
                    {
                        fightFroupPrice = fgactobj.ActiveItems.Min(p => p.ActivePrice);
                    }
                    string webRoot = Core.Helper.WebHelper.GetScheme() + "://" + Core.Helper.WebHelper.GetHost();
                    fightGroupUrl = webRoot + "/m-WeiXin/FightGroup/Detail/" + fgactobj.Id;
                }
            }
            VirtualProductInfo virtualProductInfo = null;
            if (productInfo.ProductType == 1)
            {
                virtualProductInfo=ProductManagerApplication.GetVirtualProductInfoByProductId(id);
            }
            #region 商品阶梯价--张宇枫20171018

            var ladderPrices = ProductManagerApplication.GetLadderPriceByProductIds(productId, shopInfo.IsSelf, discount);
            #endregion
            return Json(new
            {
                price = model.Price,
                saleCounts = productInfo.SaleCounts + Mall.Core.Helper.TypeHelper.ObjectToInt(productInfo.VirtualSaleCounts),
                isSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1,
                measureUnit = productInfo.MeasureUnit,
                skuColors = model.Color.OrderBy(p => p.SkuId),
                skuSizes = model.Size.OrderBy(a => a.SkuId),
                skuVersions = model.Version.OrderBy(a => a.SkuId),
                FreightTemplateId = productInfo.FreightTemplateId,
                colorAlias = !string.IsNullOrWhiteSpace(productInfo.ColorAlias) ? productInfo.ColorAlias : colorAlias,//如果该商品有自定义规格名称，则用
                sizeAlias = !string.IsNullOrWhiteSpace(productInfo.SizeAlias) ? productInfo.SizeAlias : sizeAlias,
                versionAlias = !string.IsNullOrWhiteSpace(productInfo.VersionAlias) ? productInfo.VersionAlias : versionAlias,
                LadderPrices = ladderPrices,
                IsOpenLadderPrice = productInfo.IsOpenLadder,
                IsFavorite = isFavorite,
                hasFightGroup = hasFightGroup,
                fightFroupPrice = fightFroupPrice,
                fightGroupUrl = fightGroupUrl,
                isoverdue = (virtualProductInfo != null && virtualProductInfo.ValidityType && DateTime.Now > virtualProductInfo.EndDate.Value) ? 1 : 0,
                StockAll=model.StockAll,
            //productMark = productMark,
            //isPreheat = isPreheat,
            //flashSaleId = model.FlashSale == null ? 0 : model.FlashSale.Id,
            //flashSaleTime =strFlashSaleTime,
            //isNormalPurchase = model.FlashSaleConfig.IsNormalPurchase,
            //shippingValue = model.ShippingValue,
            //shippingAddress = model.ShippingAddress,
            //freight = model.Freight
        });
        }
        public JsonResult GetProductShipAndLimit(long id = 0, long shopId = 0, long templateId = 0, string skuId = "")
        {
            var productId = id;
            if (productId == 0)
                return Json(new { data = false });
            ProductDetailModelForWeb model = new ProductDetailModelForWeb
            {
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU()
            };
            #region 物流信息
            if (templateId > 0)  //虚拟商品不需要发货，没有物流信息
            {
                var template = ServiceApplication.Create<IFreightTemplateService>().GetFreightTemplate(templateId);
                decimal freight = 0;
                int cityId = 0;
                string shippingAddress = "";
                string shippingValue = string.Empty;
                string freightStr = string.Empty;
                string productAddress = string.Empty;
                if (template != null)
                {

                    var fullName = RegionApplication.GetFullName(template.SourceAddress);
                    if (fullName != null)
                    {
                        var ass = fullName.Split(' ');
                        if (ass.Length >= 2)
                        {
                            productAddress = ass[0] + " " + ass[1];
                        }
                        else
                        {
                            productAddress = ass[0];
                        }
                    }

                    Entities.ShippingAddressInfo defaultAddress = null;
                    if (CurrentUser != null)
                    {
                        defaultAddress = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(CurrentUser.Id);
                    }
                    int regionId = 0;
                    if (defaultAddress != null)
                    {
                        regionId = defaultAddress.RegionId;
                    }
                    else
                    {
                        string curip = Mall.Core.Helper.WebHelper.GetIP();
                        regionId = (int)RegionApplication.GetRegionByIPInTaobao(curip);
                    }
                    if (regionId > 0)
                    {
                        shippingValue = RegionApplication.GetRegionPath(regionId);

                        string addrs = RegionApplication.GetFullName(regionId);

                        shippingAddress = addrs;
                        if (string.IsNullOrEmpty(shippingAddress))
                        {
                            shippingAddress = "请选择";
                        }
                        cityId = regionId;// ServiceApplication.Create<IRegionService>.Create.GetCityId(defaultAddress.RegionIdPath);
                    }
                }
                if (template.IsFree == FreightTemplateType.Free)
                {
                    freightStr = "卖家承担运费";
                }
                else
                {
                    if (CurrentUser != null)
                    {
                        if (cityId > 0)
                        {
                            List<long> productIds = new List<long>();
                            List<int> counts = new List<int>();
                            productIds.Add(id);
                            counts.Add(1);

                            bool isFree = ProductManagerApplication.IsFreeRegion(id, CurrentUser.MemberDiscount, cityId, 1, skuId);
                            if (isFree)
                            {
                                freightStr = "卖家承担运费";
                            }
                            else
                            {
                                freight = ServiceApplication.Create<IProductService>().GetFreight(productIds, counts, cityId);
                                freightStr = "运费 ￥" + freight.ToString("f2");
                            }
                        }
                    }
                }


                if (cityId > 0)
                {
                    var shopBranchQuery = new ShopBranchQuery();
                    shopBranchQuery.ShopId = shopId;
                    shopBranchQuery.Status = ShopBranchStatus.Normal;
                    shopBranchQuery.ProductIds = new[] { productId };
                    shopBranchQuery.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
                    var cityRegion = RegionApplication.GetRegion(cityId, CommonModel.Region.RegionLevel.City);
                    if (cityRegion != null)
                    {
                        shopBranchQuery.AddressPath = cityRegion.GetIdPath();
                    }
                    model.CanSelfTake = ShopBranchApplication.Exists(shopBranchQuery);
                }

                model.ProductAddress = productAddress;
                model.ShippingAddress = shippingAddress;
                model.ShippingValue = shippingValue;
                model.Freight = freightStr;
            }
            #endregion

            #region 限时购预热
            var iLimitService = ServiceApplication.Create<ILimitTimeBuyService>();
            bool isPreheat = false;
            model.FlashSale = iLimitService.IsFlashSaleDoesNotStarted(id);
            model.FlashSaleConfig = iLimitService.GetConfig();
            var strFlashSaleTime = string.Empty;
            double flashSecond = 0;
            if (model.FlashSale != null)
            {
                TimeSpan flashSaleTime = DateTime.Parse(model.FlashSale.BeginDate) - DateTime.Now;  //开始时间还剩多久
                if (model.FlashSaleConfig != null)
                {
                    TimeSpan preheatTime = new TimeSpan(model.FlashSaleConfig.Preheat, 0, 0);  //预热时间是多久
                    if (preheatTime >= flashSaleTime)  //预热大于开始
                    {
                        isPreheat = true;
                        strFlashSaleTime = Math.Floor(flashSaleTime.TotalHours) + "时" + flashSaleTime.Minutes + "分";

                        TimeSpan end = new TimeSpan(DateTime.Parse(model.FlashSale.BeginDate).Ticks);
                        TimeSpan start = new TimeSpan(DateTime.Now.Ticks);
                        TimeSpan ts = end.Subtract(start);
                        flashSecond = ts.TotalSeconds < 0 ? 0 : ts.TotalSeconds;
                    }
                }
            }

            #endregion

            #region  商品评分等级
            var productMark = _iCommentService.GetProductMark(id);
            #endregion

            #region 最小批量--张与枫
            var minMath = ProductManagerApplication.GetProductLadderMinMath(productId);
            #endregion

            return Json(new
            {
                productMark = productMark,
                isPreheat = isPreheat,
                flashSaleId = model.FlashSale == null ? 0 : model.FlashSale.Id,
                flashSaleTime = strFlashSaleTime,
                isNormalPurchase = model.FlashSaleConfig.IsNormalPurchase,
                shippingValue = model.ShippingValue,
                shippingAddress = model.ShippingAddress,
                freight = model.Freight,
                minMath = minMath,
                flashSecond = flashSecond,
                canSelfTake = model.CanSelfTake
            });
        }
        /// <summary>
        /// 获取登录用户的信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetLoginUserInfo()
        {
            #region 登录信息
            var isLogin = false;
            List<ProductInfo> concern = new List<ProductInfo>();
            List<ProductInfo> browsingProducts = new List<ProductInfo>();
            //var integral = 0;
            if (CurrentUser != null)
            {
                isLogin = true;
                //关注商品
                var favorites = isLogin ? FavoriteApplication.GetFavoriteByUser(CurrentUser.Id, 10) : new List<FavoriteInfo>();
                var products = ProductManagerApplication.GetProducts(favorites.Select(p => p.ProductId));
                concern = favorites.Select(item =>
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    return new ProductInfo
                    {
                        Id = product.Id,
                        ProductName = product.ProductName,
                        ImagePath = MallIO.GetProductSizeImage(product.RelativePath, 1, (int)ImageSize.Size_50),
                        IgnoreReference = true
                    };
                }).ToList();

                //浏览的商品
                var viewHistoryModel = isLogin ? BrowseHistrory.GetBrowsingProducts(10, CurrentUser.Id) : new List<ProductBrowsedHistoryModel>();
                browsingProducts = viewHistoryModel.Select(item => new ProductInfo
                {
                    Id = item.ProductId,
                    ProductName = item.ProductName,
                    ImagePath = MallIO.GetProductSizeImage(item.ImagePath, 1, 50),
                    IgnoreReference = true
                }).ToList();
            }
            var userName = CurrentUser == null ? "" : (string.IsNullOrEmpty(CurrentUser.Nick) ? CurrentUser.UserName : CurrentUser.Nick);

            return Json(new { isLogin = CurrentUser != null, currentUserName = userName, concern = concern, browsingProducts = browsingProducts });
            #endregion
        }

        public ActionResult UserCoupons()
        {
            var model = new ProductPartialHeaderModel();
            var baseCoupons = new List<DisplayCoupon>();
            var _iCouponService = ServiceApplication.Create<ICouponService>();
            var user = CurrentUser?.Id ?? 0;

            if (user > 0)
            {
                model.MemberIntegral = MemberIntegralApplication.GetAvailableIntegral(user);//用户积分
                var usercoupons = _iCouponService.GetAllUserCoupon(user);//优惠卷
                var coupons = CouponApplication.GetCouponInfo(usercoupons.Select(p => p.CouponId));
                var shops = ShopApplication.GetShops(coupons.Select(p => p.ShopId));
                var shopBonus = ShopBonusApplication.GetShopBounsByUser(user);//红包
                baseCoupons.AddRange(usercoupons.Select(item =>
                {
                    var coupon = coupons.FirstOrDefault(p => p.Id == item.CouponId);
                    var shop = shops.FirstOrDefault(p => p.Id == coupon.ShopId);
                    return new DisplayCoupon
                    {
                        Type = CommonModel.CouponType.Coupon,
                        Limit = coupon.OrderAmount,
                        Price = item.Price,
                        ShopId = shop.Id,
                        ShopName = shop.ShopName,
                        EndTime = coupon.EndTime,
                    };
                }));

                baseCoupons.AddRange(shopBonus.Select(p => new DisplayCoupon
                {
                    Type = CommonModel.CouponType.ShopBonus,
                    EndTime = p.Bonus.DateEnd,
                    Limit = p.Bonus.UsrStatePrice,
                    Price = p.Receive.Price,
                    ShopId = p.Shop.Id,
                    ShopName = p.Shop.ShopName,
                    UseState = p.Bonus.UseState
                }));
            }
            model.BaseCoupon = baseCoupons;
            return PartialView(model);
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult CalceFreight(int cityId, int streetId, long pId, int count, string skuId = "")
        {
            List<long> productIds = new List<long>();
            List<int> counts = new List<int>();
            productIds.Add(pId);
            counts.Add(count);

            string freightStr = string.Empty;
            decimal discount = 1M;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            bool isFree = ProductManagerApplication.IsFreeRegion(pId, discount, streetId, count, skuId);
            if (isFree)
            {
                freightStr = "卖家承担运费";
            }
            else
            {
                decimal freight = ServiceApplication.Create<IProductService>().GetFreight(productIds, counts, cityId);
                freightStr = "运费 ￥" + freight.ToString("f2");
            }
            return Json(new Result() { success = true, msg = freightStr });
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult IsCashOnDelivery(long countyId)
        {
            bool result = PaymentConfigApplication.IsCashOnDelivery(countyId);
            if (result)
            {
                return Json(new Result() { success = result, msg = "支持货到付款" });
            }
            return Json(new Result() { success = result, msg = "" });

        }

        public JsonResult GetPrice(string skuId)
        {
            var price = ServiceApplication.Create<IProductService>().GetSku(skuId).SalePrice.ToString("f2");
            return Json(new
            {
                price = price
            });
        }


        [UnAuthorize]
        [HttpPost]
        public ActionResult GetProductActives(long shopId, long productId)
        {
            ProductActives actives = new ProductActives();
            var freeFreight = ServiceApplication.Create<IShopService>().GetShopFreeFreight(shopId);
            actives.freeFreight = freeFreight;
            var bonus = ServiceApplication.Create<IShopBonusService>().GetByShopId(shopId);
            if (bonus != null)
            {
                ProductBonusLableModel model = new ProductBonusLableModel();
                model.Count = bonus.Count;
                model.GrantPrice = bonus.GrantPrice;
                model.RandomAmountStart = bonus.RandomAmountStart;
                model.RandomAmountEnd = bonus.RandomAmountEnd;
                actives.ProductBonus = model;
            }
            var fullDiscount = FullDiscountApplication.GetOngoingActiveByProductId(productId, shopId);
            if (fullDiscount != null)
            {
                actives.FullDiscount = fullDiscount;
            }
            return Json(actives);
        }



        [HttpGet]
        public JsonResult GetStock(string skuId)
        {
            var sku = ProductManagerApplication.GetSKU(skuId);
            var stock = sku.Stock;
            var product = ProductManagerApplication.GetProduct(sku.ProductId);
            var status = 0;
            if (product.AuditStatus == Entities.ProductInfo.ProductAuditStatus.Audited && product.SaleStatus == Entities.ProductInfo.ProductSaleStatus.OnSale)
            {
                status = 1;
            }
            return Json(new
            {
                Stock = stock,
                Status = status
            });
        }

        public JsonResult GetSKUInfo(long pId, long colloPid = 0)
        {
            var product = ServiceApplication.Create<IProductService>().GetProduct(pId);
            List<Mall.Entities.CollocationSkuInfo> collProduct = null;
            if (colloPid != 0)
            {
                collProduct = ServiceApplication.Create<ICollocationService>().GetProductColloSKU(pId, colloPid);
            }

            decimal discount = 1M;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            var shopInfo = ShopApplication.GetShop(product.ShopId);

            var skuArray = new List<ProductSKUModel>();
            var skus = ProductManagerApplication.GetSKUs(product.Id);
            foreach (var sku in skus.Where(s => s.Stock > 0))
            {
                decimal price = 1M;
                if (shopInfo.IsSelf)
                {
                    price = sku.SalePrice * discount;
                }
                else
                {
                    price = sku.SalePrice;
                }
                if (collProduct != null && collProduct.Count > 0)
                {
                    var collsku = collProduct.FirstOrDefault(a => a.SkuID == sku.Id);
                    if (collsku != null)
                        price = collsku.Price;
                }
                skuArray.Add(new ProductSKUModel
                {
                    Price = price,
                    SkuId = sku.Id,
                    Stock = (int)sku.Stock
                });
            }

            return Json(new
            {
                skuArray = skuArray
            });
        }

        [HttpPost]
        public JsonResult AddFavoriteProduct(long pid)
        {
            int status = 0;
            ServiceApplication.Create<IProductService>().AddFavorite(pid, CurrentUser.Id, out status);
            if (status == 1)
            {
                return Json(new
                {
                    success = true,
                    favorited = true,
                    mess = "您已经收藏过该商品了."
                });
            }
            else
            {
                return Json(new
                {
                    success = true,
                    favorited = false,
                    mess = "成功收藏该商品."
                });
            }
        }

        public ActionResult CustmerServices(long shopId)
        {
            //排除掉移动端的客服
            var model = CustomerServiceApplication.GetPreSaleByShopId(shopId).Where(c => c.TerminalType == Entities.CustomerServiceInfo.ServiceTerminalType.PC).ToList();
            ViewBag.Keyword = SiteSettings.Keyword;
            return View(model);
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult List(long? categoryId, string brandName, string productCode, int? auditStatus, string ids, int page, int rows, string keyWords, string shopName, int? saleStatus)
        {
            var queryModel = new ProductQuery()
            {
                PageSize = rows,
                PageNo = page,
                BrandName = brandName,
                KeyWords = keyWords,
                CategoryId = categoryId,
                Ids = string.IsNullOrWhiteSpace(ids) ? null : ids.Split(',').Select(item => long.Parse(item)),
                ShopName = shopName,
                ProductCode = productCode
            };
            if (auditStatus.HasValue)
            {
                queryModel.AuditStatus = new Entities.ProductInfo.ProductAuditStatus[] { (Entities.ProductInfo.ProductAuditStatus)auditStatus };
                if (auditStatus == (int)Entities.ProductInfo.ProductAuditStatus.WaitForAuditing)
                    queryModel.SaleStatus = Entities.ProductInfo.ProductSaleStatus.OnSale;
            }
            if (saleStatus.HasValue)
                queryModel.SaleStatus = (Entities.ProductInfo.ProductSaleStatus)saleStatus;

            var products = ProductManagerApplication.GetProducts(queryModel);
            var productDescriptions = ProductManagerApplication.GetProductDescription(products.Models.Select(p => p.Id).ToArray());
            var brands = BrandApplication.GetBrandsByIds(products.Models.Select(p => p.BrandId));
            var categories = CategoryApplication.GetCategories();
            var shops = ShopApplication.GetShops(products.Models.Select(p => p.ShopId).ToList());

            var dataGrid = new DataGridModel<Mall.Web.Areas.Admin.Models.Product.ProductModel>();
            dataGrid.rows = products.Models.Select(item => new Mall.Web.Areas.Admin.Models.Product.ProductModel()
            {
                name = item.ProductName,
                brandName = item.BrandId == 0 ? "" : brands.Any(b => b.Id == item.BrandId) ? brands.FirstOrDefault(b => b.Id == item.BrandId).Name : "",
                categoryName = categories.Any(c => c.Id == item.CategoryId) ? categories.FirstOrDefault(c => c.Id == item.CategoryId).Name : "",
                id = item.Id,
                imgUrl = item.GetImage(Mall.CommonModel.ImageSize.Size_50),
                price = item.MinSalePrice,
                state = item.AuditStatus == Entities.ProductInfo.ProductAuditStatus.WaitForAuditing ? (item.SaleStatus == Entities.ProductInfo.ProductSaleStatus.OnSale ? ProductInfo.ProductAuditStatus.WaitForAuditing.ToDescription() : ProductInfo.ProductSaleStatus.InStock.ToDescription()) : item.AuditStatus.ToDescription(),
                auditStatus = (int)item.AuditStatus,
                url = "",
                auditReason = productDescriptions.Any(pd => pd.ProductId == item.Id) ? productDescriptions.FirstOrDefault(pd => pd.ProductId == item.Id).AuditReason : "",
                shopName = shops.Any(s => s.Id == item.ShopId) ? shops.FirstOrDefault(s => s.Id == item.ShopId).ShopName : "",
                saleStatus = (int)item.SaleStatus,
                productCode = item.ProductCode
            });
            dataGrid.total = products.Total;

            return Json(dataGrid);
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult Browse(long? shopId, long? categoryId, int? auditStatus, string ids, int page, string keyWords, int? saleStatus, bool? isShopCategory, int rows = 10, bool isLimitTimeBuy = false)
        {
            var query = new ProductQuery()
            {
                PageSize = rows,
                PageNo = page,
                KeyWords = keyWords,
                CategoryId = categoryId,
                Ids = string.IsNullOrWhiteSpace(ids) ? null : ids.Split(',').Select(item => long.Parse(item)),
                ShopId = shopId,
                IsLimitTimeBuy = isLimitTimeBuy
            };
            if (auditStatus.HasValue)
                query.AuditStatus = new Entities.ProductInfo.ProductAuditStatus[] { (Entities.ProductInfo.ProductAuditStatus)auditStatus };

            if (saleStatus.HasValue)
                query.SaleStatus = (Entities.ProductInfo.ProductSaleStatus)saleStatus;


            var productEntities = ProductManagerApplication.GetProducts(query);
            ICategoryService productCategoryService = ServiceApplication.Create<ICategoryService>();
            IBrandService brandService = ServiceApplication.Create<IBrandService>();
            var products = productEntities.Models.ToArray().Select(item => new
            {
                //TODO:FG 循环内查询
                name = item.ProductName,
                brandName = item.BrandId == 0 ? "" : brandService.GetBrand(item.BrandId).Name,
                categoryName = productCategoryService.GetCategory(item.CategoryId).Name,
                id = item.Id,
                imgUrl = item.GetImage(ImageSize.Size_50),
                price = item.MinSalePrice
            });

            var dataGrid = new
            {
                rows = products,
                total = productEntities.Total
            };
            return Json(dataGrid);
        }

        public ActionResult GetCollocationProducts(string productIds, string colloPids)
        {
            var ids = productIds.Split(',').Select(a => long.Parse(a)).ToArray();
            var cids = colloPids.Split(',').Select(a => long.Parse(a)).ToArray();
            var dic = new Dictionary<long, long>();
            for (var i = 0; i < ids.Length; i++)
            {
                dic.Add(ids[i], cids[i]);
            }
            var product = ProductManagerApplication.GetProductByIds(ids); ;

            var products = ids.Select(a => product.Where(t => t.Id == a).FirstOrDefault());


            List<CollocationSkusModel> model = new List<CollocationSkusModel>();

            foreach (var p in products)
            {
                model.Add(GetCollocationSku(p, dic.Where(a => a.Key == p.Id).FirstOrDefault().Value));
            }


            ViewBag.Keyword = SiteSettings.Keyword;
            return View(model);
        }

        private CollocationSkusModel GetCollocationSku(Entities.ProductInfo product, long colloId)
        {
            var typeservice = _iTypeService;
            Entities.TypeInfo typeInfo = typeservice.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            var skus = ProductManagerApplication.GetSKUByProducts(new[] { product.Id });
            CollocationSkusModel model = new CollocationSkusModel();
            colorAlias = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias;
            sizeAlias = !string.IsNullOrWhiteSpace(product.SizeAlias) ? product.SizeAlias : sizeAlias;
            versionAlias = !string.IsNullOrWhiteSpace(product.VersionAlias) ? product.VersionAlias : versionAlias;
            model.ProductName = product.ProductName;
            model.ColloProductId = colloId;
            model.ImagePath = product.ImagePath;
            model.ProductId = product.Id;
            model.MeasureUnit = product.MeasureUnit;
            model.Stock = skus.Sum(a => a.Stock);
            model.ColorAlias = colorAlias;
            model.SizeAlias = sizeAlias;
            model.VersionAlias = versionAlias;
            var coll = CollocationApplication.GetProductColloSKU(product.Id, colloId);
            if (coll != null)
                model.MinPrice = coll.Min(a => a.Price);
            else
                model.MinPrice = skus.Min(a => a.SalePrice);

            if (skus != null && skus.Count() > 0)
            {
                long colorId = 0, sizeId = 0, versionId = 0;
                foreach (var sku in skus.OrderBy(s => s.AutoId).ToList())
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 0 && !string.IsNullOrEmpty(sku.Color))
                    {
                        if (long.TryParse(specs[1], out colorId))
                        {
                        }
                        if (colorId != 0)
                        {
                            if (!model.Color.Any(v => v.Value.Equals(sku.Color)))
                            {
                                var c = skus.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.Stock);
                                model.Color.Add(new ProductSKU
                                {
                                    //Name = "选择颜色",
                                    Name = "选择" + colorAlias,
                                    EnabledClass = c != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Color.Any(c1 => c1.SelectedClass.Equals("selected")) && c != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = colorId,
                                    Value = sku.Color,
                                    Img = sku.ShowPic
                                });
                            }
                        }
                    }
                    if (specs.Count() > 1 && !string.IsNullOrEmpty(sku.Size))
                    {
                        if (long.TryParse(specs[2], out sizeId))
                        {
                        }
                        if (sizeId != 0)
                        {
                            if (!model.Size.Any(v => v.Value.Equals(sku.Size)))
                            {
                                var ss = skus.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.Stock);
                                model.Size.Add(new ProductSKU
                                {
                                    //Name = "选择尺码",
                                    Name = "选择" + sizeAlias,
                                    EnabledClass = ss != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Size.Any(s1 => s1.SelectedClass.Equals("selected")) && ss != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = sizeId,
                                    Value = sku.Size

                                });
                            }
                        }
                    }

                    if (specs.Count() > 2 && !string.IsNullOrEmpty(sku.Version))
                    {
                        if (long.TryParse(specs[3], out versionId))
                        {
                        }
                        if (versionId != 0)
                        {
                            if (!model.Version.Any(v => v.Value.Equals(sku.Version)))
                            {
                                var v = skus.Where(s => s.Version.Equals(sku.Version)).Sum(s => s.Stock);
                                model.Version.Add(new ProductSKU
                                {
                                    //Name = "选择版本",
                                    Name = "选择" + versionAlias,
                                    EnabledClass = v != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Version.Any(v1 => v1.SelectedClass.Equals("selected")) && v != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = versionId,
                                    Value = sku.Version
                                });
                            }
                        }
                    }

                }
            }
            return model;
        }

        public JsonResult GetHotProduct(long productId, int categoryId = 0)
        {
            var relationProduct = ProductManagerApplication.GetRelationProductByProductId(productId);
            List<DTO.Product.Product> products;
            if (relationProduct == null || ProductManagerApplication.GetProductsByIds(relationProduct.RelationProductIds).Count == 0)
            {
                if (categoryId == 0)
                {
                    var mainProduct = ProductManagerApplication.GetProduct(productId);
                    if (mainProduct != null)
                    {
                        categoryId = (int)mainProduct.CategoryId;
                    }
                }
                products = ProductManagerApplication.GetHotSaleProductByCategoryId(categoryId, 10);
            }
            else
                products = ProductManagerApplication.GetProductsByIds(relationProduct.RelationProductIds);

            decimal discount = 1M;
            long SelfShopId = 0;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
                var shopInfo = ShopApplication.GetSelfShop();
                SelfShopId = shopInfo.Id;
            }

            foreach (var item in products)
            {
                var salePrice = item.MinSalePrice;
                if (item.ShopId == SelfShopId)
                    salePrice = item.MinSalePrice * discount;
                var limitBuy = LimitTimeApplication.GetLimitTimeMarketItemByProductId(item.Id);
                if (limitBuy != null)
                {
                    salePrice = limitBuy.MinPrice;
                }
                item.MinSalePrice = salePrice;
                item.ImagePath = item.GetImage(ImageSize.Size_220);
                item.SaleCounts = item.SaleCounts + Core.Helper.TypeHelper.ObjectToInt(item.VirtualSaleCounts);
            }

            return Json(products, true);
        }

        public JsonResult CanBuy(long productId, int count)
        {
            if (CurrentUser == null)
                return Json(new { Result = true }, true);

            int reason;
            var msg = new Dictionary<int, string>() { { 0, "" }, { 1, "商品已下架" }, { 2, "商品已删除" }, { 3, "超出商品最大限购数" }, { 9, "商品无货" } };
            var result = ProductManagerApplication.CanBuy(CurrentUser.Id, productId, count, out reason);

            return Json(new
            {
                Result = result,
                Message = msg[reason]
            }, true);
        }

        #region 拼团
        /// <summary>
        /// 拼团显示二维码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ShowActiveQRCode(long id)
        {
            string showurl = CurrentUrlHelper.CurrentUrlNoPort() + "/m-" + PlatformType.Wap.ToString() + "/FightGroup/Detail/" + id.ToString();
            Image map;
            map = Core.Helper.QRCodeHelper.Create(showurl);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);

            return File(ms.ToArray(), "image/png");
        }
        #endregion
    }
}