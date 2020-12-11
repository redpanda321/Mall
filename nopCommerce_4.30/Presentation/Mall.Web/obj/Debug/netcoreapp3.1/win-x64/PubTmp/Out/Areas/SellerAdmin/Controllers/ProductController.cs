using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

using Mall.Application;
using Mall.Core;
using Mall.IServices;
using Mall.DTO.QueryModel;

using Mall.Entities;
using Mall.Web.Areas.SellerAdmin.Models;
using Mall.Web.Framework;
using Mall.Web.Models;
using Mall.CommonModel;
using Mall.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Nop.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Mall.Core.Helper;
using Microsoft.AspNetCore.Http;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class ProductController : BaseSellerController
    {
        #region 字段
        private IShopCategoryService _iShopCategoryService;
        private IProductService _iProductService;
        private ITypeService _iTypeService;
        private ICategoryService _iCategoryService;
        private IShopService _iShopService;
        private ISearchProductService _iSearchProductService;
        private long shopId = 2;
        #endregion

        #region 构造函数
        public ProductController(IShopCategoryService iShopCategoryService,
            IProductService iProductService,
            ITypeService iTypeService,
            ICategoryService iCategoryService,
            IShopService iShopService,
            ISearchProductService iSearchProductService
            )
            : this()
        {
            _iShopCategoryService = iShopCategoryService;
            _iProductService = iProductService;
            _iTypeService = iTypeService;
            _iCategoryService = iCategoryService;
            _iShopService = iShopService;
            _iSearchProductService = iSearchProductService;
        }


        public ProductController()
            : base()
        {
            if (CurrentSellerManager != null)
                shopId = CurrentSellerManager.ShopId;
        }
        #endregion

        #region Action
        public ActionResult Create()
        {
            var model = InitCreateModel();

            string message;
            if (!CanCreate(out message))
                this.SendMessage(message);

            bool canLadder = CanLadder();
            ViewBag.CanLadder = canLadder;
            model.MeasureUnit = "件";
            model.ProductCode = new Random().Next(10000000, 99999999).ToString();

            return View(model);
        }

        private bool CanLadder()
        {
            bool canLadder = true;//授权模块控制阶梯价功能是否开放
            var settings = SiteSettingApplication.SiteSettings;
            if (!(settings.IsOpenPC || settings.IsOpenH5 || settings.IsOpenApp || settings.IsOpenMallSmallProg))
            {
                canLadder = false;
            }

            return canLadder;
        }

        [HttpPost]
       
        public ActionResult Create(ProductCreateModel model)
        {
            if (model.ProductType == 1)
            {
                model.FreightTemplateId = 0;
            }
            if (model.VirtualProductItemInfo != null && model.VirtualProductItemInfo.Count() > 0)
            {
                model.VirtualProductItemInfo = model.VirtualProductItemInfo.Where(a => !string.IsNullOrWhiteSpace(a.Name)).ToArray();
            }
            if (!string.IsNullOrWhiteSpace(model.EndDate))
            {
                model.EndDate = DateTime.Parse(model.EndDate).ToString("yyyy-MM-dd") + " 23:59:59";
            }
            if (model.SKUExs != null && model.SKUExs.Any(d => d.SafeStock.HasValue && d.SafeStock < 0))
            {
                return Json(false, "警戒库存不可以为负数！");
            }
            if ((model.SKUExs == null || model.SKUExs.Count() < 1) && model.SafeStock.HasValue && model.SafeStock < 0)
            {
                return Json(false, "警戒库存不可以为负数！");
            }
            model.MinSalePrice = model.SKUExs.Min(p => p.SalePrice);//model.SKUInfoExs一定有值,当用户没有设置规格时将会有默认值

            model.IsOpenLadder = !string.IsNullOrEmpty(model.IsOpen) && model.IsOpen.Equals("on");//是否开启阶梯价
            if (!ModelState.IsValid)
            {
                var errlist = ModelState.Values;
                foreach (var item in errlist)
                {
                    if (item.Errors.Count > 0) Core.Log.Error(item.Errors[0].ErrorMessage);
                }
                return ValidError();
            }

            string message;
            if (!CanCreate(out message))
                return Json(false, message);

            var product = model.DynamicMap<DTO.Product.Product>();
            var success = false;
            try
            {
                var skus = model.SKUExs.Select(p =>
                {
                    p.Id = p.CreateId(null);
                    return p;
                }).ToArray();
                if (skus != null && skus.Count() == 1)
                {
                    skus.FirstOrDefault().Sku = model.ProductCode;//如果商品没有开启规格，则将ProductCode默认作为Sku
                }

                bool canLadder = CanLadder();//授权模块影响阶梯价功能开放
                ProductLadderPrice[] prices = new ProductLadderPrice[] { };
                if (canLadder)
                {
                    prices = model.Prices.OrderBy(i => i.MinBath).Select(price =>
                    {
                        price.ProductId = model.Id;
                        return price;
                    }).ToArray();

                    var priceLength = prices.Length;
                    for (var i = 0; i < priceLength; i++)
                    {
                        var item = prices[i];
                        var maxcount = 999999999;
                        if (priceLength > 1 && priceLength != (i + 1))
                        {
                            maxcount = prices[i + 1].MinBath - 1;
                        }
                        item.MaxBath = maxcount;
                    }
                }

                var descriptionInfo = model.Description;

                var attributes = model.GetProductAttribute(model.Id);
                var sellerSpecs = model.GetSellerSpecification(this.CurrentSellerManager.ShopId, product.TypeId);

                var productDto = Mall.Application.ProductManagerApplication.AddProduct(this.CurrentShop.Id, product,
                    model.Pics, skus, descriptionInfo, attributes, model.GoodsCategory, sellerSpecs, prices);

                success = true;
            }
            catch (Exception e)
            {
                Core.Log.Error("创建商品失败", e);
            }

            try
            {
                //添加商家操作日志
                ServiceApplication.Create<IOperationLogService>().AddSellerOperationLog(new Entities.LogInfo
                {
                    Date = DateTime.Now,
                    Description = string.Format("商家发布商品，Id={0}, 名称={1} [{2}]", product.Id, product.ProductName, success ? "成功" : "失败"),
                    IPAddress =  base.Request.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress.ToString(),
                    PageUrl = "/Product/Create",
                    UserName = CurrentSellerManager.UserName,
                    ShopId = CurrentSellerManager.ShopId
                });
            }
            catch
            {
            }

            if (success)
                return Json();
            return Json(false, "发布商品出错");
        }

        public ActionResult Edit(long id)
        {
            var product = ProductManagerApplication.GetProduct(id);
            if (product == null)
                return NotFound();

            var model = InitEditModel(product);
            if (Core.Helper.TypeHelper.ObjectToDecimal(model.MarketPrice) == 0)
            {
                model.MarketPrice = null;
            }
            bool canLadder = true;//授权模块控制阶梯价功能是否开放
            var settings = SiteSettingApplication.SiteSettings;
            if (!(settings.IsOpenPC || settings.IsOpenH5 || settings.IsOpenApp || settings.IsOpenMallSmallProg))
            {
                canLadder = false;
            }
            ViewBag.CanLadder = canLadder && product.ProductType == 0;

            return View(model);
        }

        [HttpPost]
       
        public ActionResult Edit(ProductCreateModel model)
        {
            if (model.ProductType == 1)
            {
                model.FreightTemplateId = 0;//这里因为不好将FreightTemplateId改为long?，会很多地方影响。所以如果是虚拟商品，则此处运费模板ID为0
            }
            if (model.VirtualProductItemInfo != null && model.VirtualProductItemInfo.Count() > 0)
            {
                model.VirtualProductItemInfo = model.VirtualProductItemInfo.Where(a => !string.IsNullOrWhiteSpace(a.Name)).ToArray();
            }
            if (!string.IsNullOrWhiteSpace(model.EndDate))
            {
                model.EndDate = DateTime.Parse(model.EndDate).ToString("yyyy-MM-dd") + " 23:59:59";
            }
            if (model.SKUExs != null && model.SKUExs.Any(d => d.SafeStock.HasValue && d.SafeStock < 0))
            {
                return Json(false, "警戒库存不可以为负数！");
            }
            if ((model.SKUExs == null || model.SKUExs.Count() < 1) && model.SafeStock.HasValue && model.SafeStock < 0)
            {
                return Json(false, "警戒库存不可以为负数！");
            }
            model.MinSalePrice = model.SKUExs.Min(p => p.SalePrice);//model.SKUInfoExs一定有值,当用户没有设置规格时将会有默认值
            if (!ModelState.IsValid)
                return ValidError();
            foreach (var item in model.SKUExs)
            {
                if (string.IsNullOrWhiteSpace(item.Color) && string.IsNullOrWhiteSpace(item.Size) && string.IsNullOrWhiteSpace(item.Version))
                {
                    item.ColorId = 0;
                    item.SizeId = 0;
                    item.VersionId = 0;
                    item.Id = model.Id + "_0_0_0";
                }
            }
            var skus = model.SKUExs.Select(sku =>
            {
                sku.ProductId = model.Id;
                sku.Id = sku.CreateId(model.Id);
                return sku;
            }).ToArray();

            model.IsOpenLadder = !string.IsNullOrEmpty(model.IsOpen) && model.IsOpen.Equals("on");
            bool canLadder = CanLadder();//授权模块影响阶梯价功能开放
            ProductLadderPrice[] prices = new ProductLadderPrice[] { };
            if (canLadder && model.Prices != null)
            {
                prices = model.Prices.OrderBy(i => i.MinBath).Select(price =>
                {
                    price.ProductId = model.Id;
                    return price;
                }).ToArray();

                var priceLength = prices.Length;
                for (var i = 0; i < priceLength; i++)
                {
                    var item = prices[i];
                    var maxcount = 999999999;
                    if (priceLength > 1 && priceLength != (i + 1))
                    {
                        maxcount = prices[i + 1].MinBath - 1;
                    }
                    item.MaxBath = maxcount;
                }
            }

            //处理限时购 逻辑完全copy之前的
            #region 更改限时购数据
            using (ILimitTimeBuyService ilimit = ServiceApplication.Create<ILimitTimeBuyService>())
            {
                FlashSaleModel fsi = ilimit.GetFlaseSaleByProductId(model.Id);
                if (fsi != null)
                {
                    //delete-pengjiangxiong//
                    //if (DateTime.Parse(fsi.EndDate) > DateTime.Now)
                    //    foreach (var sku in skus)
                    //    {
                    //        LimitOrderHelper.ModifyLimitStock(sku.Id, (int)sku.Stock, DateTime.Parse(fsi.EndDate));
                    //    }

                    List<FlashSaleDetailInfo> details = new List<FlashSaleDetailInfo>();

                    foreach (var sku in skus)
                    {
                        var fsd = LimitTimeApplication.GetDetail(sku.Id);

                        if (fsd == null)
                        {
                            details.Add(new FlashSaleDetailInfo
                            {
                                FlashSaleId = fsi.Id,
                                Price = sku.SalePrice,
                                ProductId = fsi.ProductId,
                                SkuId = sku.Id,
                                TotalCount = (int)sku.Stock
                            });
                        }
                    }

                    LimitTimeApplication.AddFlashSaleDetails(details);

                }
            }
            #endregion 更改限时购数据

            if (model.SaleStatus != Entities.ProductInfo.ProductSaleStatus.InDraft)
            {
                model.SaleStatus = Entities.ProductInfo.ProductSaleStatus.OnSale;
            }

            var descriptionInfo = model.Description;
            var product = model.Map<DTO.Product.Product>();
            product.MarketPrice = Core.Helper.TypeHelper.ObjectToDecimal(model.MarketPrice);
            var attributes = model.GetProductAttribute(model.Id);
            var sellerSpecs = model.GetSellerSpecification(this.CurrentSellerManager.ShopId, model.TypeId);
            bool success = false;
            try
            {
                Mall.Application.ProductManagerApplication.UpdateProduct(product, model.Pics, skus, descriptionInfo, attributes, model.GoodsCategory, sellerSpecs, prices);
                _iSearchProductService.UpdateSearchProduct(product.Id);
                var curpro = ProductManagerApplication.GetProduct(product.Id);
                //修正门店规格
                ShopBranchApplication.CorrectBranchProductSkus(curpro.Id, curpro.ShopId);
                if (curpro.SaleStatus != Entities.ProductInfo.ProductSaleStatus.OnSale || curpro.AuditStatus != Entities.ProductInfo.ProductAuditStatus.Audited)
                {
                    //处理拼团
                    var fg = FightGroupApplication.GetActiveByProductId(curpro.Id);
                    if (fg != null)
                    {
                        fg.EndTime = DateTime.Now.AddMinutes(-1);
                        FightGroupApplication.UpdateActive(fg);
                    }
                    //处理门店
                    ShopBranchApplication.UnSaleProduct(curpro.Id);
                }

                ProductManagerApplication.SaveCaculateMinPrice(product.Id, product.ShopId);
                success = true;
            }
            catch (Exception e)
            {
                Core.Log.Error("编辑商品异常", e);
            }

            try
            {
                //添加商家操作日志
                ServiceApplication.Create<IOperationLogService>().AddSellerOperationLog(new Entities.LogInfo
                {
                    Date = DateTime.Now,
                    Description = string.Format("商家修改商品，Id={0}, 名称={1} [{2}]", model.Id, model.ProductName, success ? "成功" : "失败"),
                    IPAddress =  base.Request.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress.ToString(),
                    PageUrl = "/Product/Edit",
                    UserName = CurrentSellerManager.UserName,
                    ShopId = CurrentSellerManager.ShopId
                });
            }
            catch
            {
            }

            if (success)
                return Json();
            return Json(false, "操作失败！");
        }

        public ActionResult CategoryBrands(long categoryId)
        {
            var brands = ServiceApplication.Create<IBrandService>().GetBrandsByCategoryIds(this.CurrentShop.Id, categoryId).Select(p => new
            {
                p.Id,
                p.Name
            }).ToList();

            return Json(brands);
        }

        // GET: SellerAdmin/Product
        public ActionResult PublicStepOne()
        {
            return View();
        }

        [UnAuthorize]
        private List<CategoryJsonModel> GetShopCategoryJson(long shopId)
        {
            var categories = _iShopCategoryService.GetShopCategory(shopId).ToArray();
            var json = new List<CategoryJsonModel>();
            foreach (var main in categories.Where(s => s.ParentCategoryId == 0))
            {
                var topC = new CategoryJsonModel()
                {
                    Name = main.Name,
                    Id = main.Id.ToString(),
                    SubCategory = new List<SecondLevelCategory>()
                };
                foreach (var secondItem in categories.Where(s => s.ParentCategoryId == main.Id))
                {
                    var secondC = new SecondLevelCategory()
                    {
                        Name = secondItem.Name,
                        Id = secondItem.Id.ToString(),
                    };

                    topC.SubCategory.Add(secondC);
                }
                json.Add(topC);
            }
            return json;
        }

        [UnAuthorize]
        public JsonResult GetShopProductCategory(long productId = 0)
        {
            var json = new ShopProductCategoryModel()
            {
                SelectedCategory = new List<SelectedCategory>()
            };
            json.Data = GetShopCategoryJson(shopId);
            if (0 != productId)
            {
                var cates = _iProductService.GetProductShopCategories(productId);
                foreach (var top in json.Data)
                {
                    var id = long.Parse(top.Id);
                    if (cates.Any(c => c.ShopCategoryId == id))
                        json.SelectedCategory.Add(new SelectedCategory
                        {
                            Id = top.Id,
                            Level = "1"
                        });
                    foreach (var second in top.SubCategory)
                    {
                        id = long.Parse(second.Id);
                        if (cates.Any(c => c.ShopCategoryId == id))
                            json.SelectedCategory.Add(new SelectedCategory
                            {
                                Id = second.Id,
                                Level = "2"
                            });
                    }
                }
            }
            return Json(new
            {
                json
            });
        }

        private List<TypeAttributesModel> GetPlateformAttr(long categoryId)
        {
            var cate = _iCategoryService.GetCategory(categoryId);
            var json = new List<TypeAttributesModel>();
            var attributes = _iTypeService.GetAttributesByType(cate.TypeId);
            var values = _iTypeService.GetAttributeValues(attributes.Select(p => p.Id).ToList());
            foreach (var attr in attributes)
            {
                var attrItem = new TypeAttributesModel()
                {
                    Name = attr.Name,
                    AttrId = attr.Id,
                    Selected = "",
                    IsMulti = attr.IsMulti,
                    AttrValues = new List<TypeAttrValue>()
                };
                var vals = values.Where(p => p.AttributeId == attr.Id);
                foreach (var value in vals)
                {
                    attrItem.AttrValues.Add(new TypeAttrValue
                    {
                        Id = value.Id.ToString(),
                        Name = value.Value
                    });
                }
                json.Add(attrItem);
            }
            return json;
        }

        [UnAuthorize]
        public JsonResult GetAttributes(long categoryId, long productId = 0, long isCategoryId = 0)
        {
            Dictionary<long, string> AttrMap = new Dictionary<long, string>();
            if (productId > 0)
            {
                var product = _iProductService.GetProduct(productId);
                if (product != null)
                {
                    //修改商品分类时获取分类属性
                    if (product.CategoryId != categoryId)
                    {
                        isCategoryId = 1;
                    }
                }
            }
            //如果是发布商品
            if (isCategoryId == 1)
            {
                var json = GetPlateformAttr(categoryId);
                return Json(new
                {
                    json
                });
            }
            //如果是编辑商品
            else
            {
                var json = new List<TypeAttributesModel>();

                //获取商品已经有的属性
                var attrs = ProductManagerApplication.GetProductAttributes(productId);
                if (attrs.Count == 0)
                {
                    json = GetPlateformAttr(categoryId);
                    return Json(new { json });
                }
                foreach (var item in attrs)
                {
                    //使用字典处理同一属性的多个属性值被选中
                    if (!AttrMap.ContainsKey(item.AttributeId))
                        AttrMap.Add(item.AttributeId, item.ValueId.ToString());
                    else
                    {
                        AttrMap[item.AttributeId] = AttrMap[item.AttributeId] + "," + item.ValueId.ToString();
                        continue;
                    }

                    var attr = _iTypeService.GetAttribute(item.AttributeId);
                    //获取平台与该属性对应的所有属性值
                    var attrValue = _iTypeService.GetAttributeValues(attr.Id);

                    var attrItem = new TypeAttributesModel()
                    {
                        Name = attr.Name,
                        AttrId = item.AttributeId,
                        Selected = "",
                        IsMulti = attr.IsMulti,
                        AttrValues = new List<TypeAttrValue>()
                    };
                    //添加属性值
                    foreach (var value in attrValue.ToArray())
                    {
                        attrItem.AttrValues.Add(new TypeAttrValue
                        {
                            Id = value.Id.ToString(),
                            Name = value.Value
                        });
                    }
                    categoryId = _iProductService.GetProduct(productId).CategoryId;
                    json.Add(attrItem);
                }
                var platAttr = GetPlateformAttr(categoryId);
                //更新选中属性值
                foreach (var item in json)
                {
                    item.Selected = AttrMap[item.AttrId];
                    platAttr.Remove(platAttr.FirstOrDefault(a => a.AttrId == item.AttrId));
                }
                json.AddRange(platAttr);
                return Json(new
                {
                    json
                });
            }
        }

        private SpecJosnModel GetPlatformSpec(long categoryId, long productId = 0)
        {
            var data = new SpecJosnModel
            {
                json = new List<TypeSpecificationModel>(),
                tableData = new tableDataModel()
                {
                    cost = new List<SKUSpecModel>(),
                    mallPrice = new List<SKUSpecModel>(),
                    productId = productId,
                    sku = new List<SKUSpecModel>(),
                    stock = new List<SKUSpecModel>()
                }
            };
            var cate = _iCategoryService.GetCategory(categoryId);
            var type = _iTypeService.GetType(cate.TypeId);

            foreach (SpecificationType spec in Enum.GetValues(typeof(SpecificationType)))
            {
                bool isreaddata = true;
                if (productId == 0)
                {
                    isreaddata = false;
                    switch (spec)
                    {
                        case SpecificationType.Color:
                            if (type.IsSupportColor)
                            {
                                isreaddata = true;
                            }
                            break;
                        case SpecificationType.Size:
                            if (type.IsSupportSize)
                            {
                                isreaddata = true;
                            }
                            break;
                        case SpecificationType.Version:
                            if (type.IsSupportVersion)
                            {
                                isreaddata = true;
                            }
                            break;
                    }
                }
                if (isreaddata)
                {
                    var specItem = new TypeSpecificationModel()
                    {
                        Name = Enum.GetNames(typeof(SpecificationType))[(int)spec - 1],
                        Values = new List<Specification>(),
                        SpecId = (int)spec
                    };
                    var values = _iTypeService.GetValuesByType(type.Id);
                    foreach (var value in values.Where(s => s.Specification == spec).OrderBy(s => s.Value))
                    {
                        specItem.Values.Add(new Specification
                        {
                            Id = value.Id.ToString(),
                            Name = value.Value,
                            isPlatform = true,
                            Selected = false
                        });
                    }
                    data.json.Add(specItem);
                }
            }
            var skus = _iProductService.GetSKUs(productId).OrderBy(s => s.Color).ThenBy(s => s.Size).ThenBy(s => s.Version);

            InitialTableData(skus, data);

            return data;
        }

        private void InitialTableData(IOrderedEnumerable<Entities.SKUInfo> skus, SpecJosnModel data)
        {
            if (skus.Count() == 0)
                return;
            var specType = 0;
            var value = "";
            var skuArray = skus.ToArray();
            if (!string.IsNullOrWhiteSpace(skuArray[0].Version))
            {
                specType = 2;
                value = skuArray[0].Version;
            }
            if (!string.IsNullOrWhiteSpace(skuArray[0].Size))
            {
                specType = 1;
                value = skuArray[0].Size;
            }
            if (!string.IsNullOrWhiteSpace(skuArray[0].Color))
            {
                specType = 0;
                value = skuArray[0].Color;
            }
            if (string.IsNullOrWhiteSpace(value))
                return;
            SKUSpecModel cost = new SKUSpecModel()
            {
                ValueSet = new List<string>()
            };
            SKUSpecModel stock = new SKUSpecModel()
            {
                ValueSet = new List<string>()
            };
            SKUSpecModel sku = new SKUSpecModel()
            {
                ValueSet = new List<string>()
            };
            SKUSpecModel mallPrice = new SKUSpecModel()
            {
                ValueSet = new List<string>()
            };
            foreach (var s in skus)
            {
                var specValue = "";
                switch (specType)
                {
                    case 0:
                        specValue = s.Color;
                        break;

                    case 1:
                        specValue = s.Size;
                        break;

                    case 2:
                        specValue = s.Version;
                        break;
                }
                if (specValue.Equals(value))
                {
                    cost.ValueSet.Add(s.CostPrice == 0 ? "" : s.CostPrice.ToString("f2"));
                    cost.index = specValue;

                    stock.ValueSet.Add(s.Stock == 0 ? "" : s.Stock.ToString("f2"));
                    stock.index = specValue;

                    sku.ValueSet.Add(s.Sku);
                    sku.index = specValue;

                    mallPrice.ValueSet.Add(s.SalePrice == 0 ? "" : s.SalePrice.ToString("f2"));
                    mallPrice.index = specValue;
                }
                else
                {
                    data.tableData.cost.Add(DeepClone(cost));
                    data.tableData.stock.Add(DeepClone(stock));
                    data.tableData.sku.Add(DeepClone(sku));
                    data.tableData.mallPrice.Add(DeepClone(mallPrice));

                    cost = new SKUSpecModel()
                    {
                        ValueSet = new List<string>()
                    };
                    stock = new SKUSpecModel()
                    {
                        ValueSet = new List<string>()
                    };
                    sku = new SKUSpecModel()
                    {
                        ValueSet = new List<string>()
                    };
                    mallPrice = new SKUSpecModel()
                    {
                        ValueSet = new List<string>()
                    };
                    cost.ValueSet.Add(s.CostPrice == 0 ? "" : s.CostPrice.ToString("f2"));
                    cost.index = specValue;

                    stock.ValueSet.Add(s.Stock == 0 ? "" : s.Stock.ToString("f2"));
                    stock.index = specValue;

                    sku.ValueSet.Add(s.Sku);
                    sku.index = specValue;

                    mallPrice.ValueSet.Add(s.SalePrice == 0 ? "" : s.SalePrice.ToString("f2"));
                    mallPrice.index = specValue;
                    value = specValue;
                }
            }
            data.tableData.cost.Add(DeepClone(cost));
            data.tableData.stock.Add(DeepClone(stock));
            data.tableData.sku.Add(DeepClone(sku));
            data.tableData.mallPrice.Add(DeepClone(mallPrice));
        }

        private SKUSpecModel DeepClone(SKUSpecModel obj)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(memory, obj);
                memory.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(memory) as SKUSpecModel;
            }
        }

        private List<SKUInfo> GetProducrSpec(List<Entities.SKUInfo> skuList)
        {
            List<SKUInfo> temp = new List<SKUInfo>();
            foreach (var sku in skuList)
            {
                var array = string.IsNullOrWhiteSpace(sku.Id) ? new string[1] { "" } : sku.Id.Split('_');
                temp.Add(new SKUInfo
                {
                    Color = array.Length >= 2 ? array[1] : "",
                    Size = array.Length >= 3 ? array[2] : "",
                    Version = array.Length >= 4 ? array[3] : "",
                    Id = sku.Id
                });
            }
            return temp;
        }

        [UnAuthorize]
        public JsonResult GetSpecifications(long categoryId, long productId = 0, long isCategoryId = 0)
        {
            Dictionary<long, string> SpecMap = new Dictionary<long, string>();
            var cate = _iCategoryService.GetCategory(categoryId);

            //平台规格
            var platfromSpec = GetPlatformSpec(categoryId, productId);

            //商家规格
            var sellerSpec = _iProductService.GetSellerSpecifications(shopId, cate.TypeId);

            var SKUs = _iProductService.GetSKUs(productId).ToList();

            //商品规格
            var productSpec = GetProducrSpec(SKUs);
            var skusList = SKUs.ToList();
            //遍历平台规格，使用商家规格或者商家规格覆盖
            foreach (var item in platfromSpec.json)
            {
                var specs = sellerSpec.Where(s => (int)s.Specification == item.SpecId);
                Specification updateSpec = null;

                foreach (var pspec in specs)
                {
                    updateSpec = item.Values.FirstOrDefault(s => s.Id == pspec.ValueId.ToString());

                    #region 使用商家的数据覆盖(使用商家规格值)

                    if (null != updateSpec && updateSpec.Id == pspec.ValueId.ToString())
                    {
                        updateSpec.Name = pspec.Value;
                        updateSpec.isPlatform = false;
                    }

                    //判断规格是否选中(使用商家规格值)
                    if (skusList.Any(s => s.Color.Equals(pspec.Value)) ||
                        skusList.Any(s => s.Size.Equals(pspec.Value)) ||
                        skusList.Any(s => s.Version.Equals(pspec.Value)))
                        updateSpec.Selected = true;

                    #endregion 使用商家的数据覆盖(使用商家规格值)
                }

                #region 使用商品SKU的规格值覆盖

                foreach (var val in item.Values)
                {
                    if (item.Name == "Color"
                    && productSpec.Any(s => val.Id == s.Color))
                    {
                        var sku = productSpec.FirstOrDefault(s => val.Id == s.Color);
                        val.Name = SKUs.FirstOrDefault(p => p.Id == sku.Id).Color;
                        val.isPlatform = false;
                        val.Selected = true;
                    }
                    if (item.Name == "Size"
                        && productSpec.Any(s => val.Id == s.Size))
                    {
                        var sku = productSpec.FirstOrDefault(s => val.Id == s.Size);
                        val.Name = SKUs.FirstOrDefault(p => p.Id == sku.Id).Size;
                        val.isPlatform = false;
                        val.Selected = true;
                    }
                    if (item.Name == "Version"
                        && productSpec.Any(s => val.Id == s.Version))
                    {
                        var sku = productSpec.FirstOrDefault(s => val.Id == s.Version);
                        val.Name = SKUs.FirstOrDefault(p => p.Id == sku.Id).Version;
                        val.isPlatform = false;
                        val.Selected = true;
                    }
                }

                #endregion 使用商品SKU的规格值覆盖
            }
            return Json(new
            {
                data = platfromSpec
            });
        }

        public JsonResult Specifications(long categoryId, long productId = 0)
        {
            Dictionary<Dictionary<SpecificationType, long>, Entities.SKUInfo> skus = null;
            Entities.ProductInfo productInfo = null;
            if (productId > 0)
            {
                skus = _iProductService.GetSKUs(productId).ToDictionary(p => SKUEx.SplitId(p.Id), p => p);
                productInfo = _iProductService.GetProduct(productId);
            }

          //  List<Mall.Entities.SpecificationValueInfo> specifications =  EngineContext.Current.Resolve<ISpecificationService>().GetSpecification(categoryId, this.CurrentShop.Id);


            List<Mall.Entities.SpecificationValueInfo> specifications =  EngineContext.Current.Resolve<ISpecificationService>().GetSpecification(categoryId, this.CurrentShop.Id);


            Mall.Entities.SpecificationValueInfo info = new Mall.Entities.SpecificationValueInfo();
            if (specifications != null && specifications.Count > 0) info = specifications[0];
            var newspecifications = specifications.GroupBy(item => item.Specification).Select(item => new
            {
                Specification = new
                {
                    Value = (int)item.Key,
                    Name = item.Key.ToString(),
                    Alias = GetAlias(productInfo, item.Key.ToDescription(), item.Key, info),
                    Text = item.Key.ToDescription(),
                    NeedPic = item.Key == SpecificationType.Color//是否需要设置图片
                },
                Values = item.Select(model =>
                {
                    var value = model.Value;
                    if (skus != null)
                    {
                        var key = skus.Keys.FirstOrDefault(dic => dic.ContainsKey(item.Key) && dic[item.Key] == model.Id);
                        if (key != null)
                        {
                            string _tmpvalue = "";
                            switch (item.Key)
                            {
                                case SpecificationType.Color:
                                    _tmpvalue = skus[key].Color;
                                    break;
                                case SpecificationType.Size:
                                    _tmpvalue = skus[key].Size;
                                    break;
                                default:
                                    _tmpvalue = skus[key].Version;
                                    break;
                            }
                            if (!string.IsNullOrWhiteSpace(_tmpvalue))
                            {
                                value = _tmpvalue;
                            }
                        }
                    }

                    return new
                    {
                        model.Id,
                        Value = value
                    };
                }).ToArray()
            });
            return Json(newspecifications);
        }
        private string GetAlias(Entities.ProductInfo productInfo, string text, SpecificationType key, Mall.Entities.SpecificationValueInfo info)
        {
            string alias = GetSpecificationTypeAlias(key, info);//先读取默认的
            if (productInfo != null)
            {
                if (text == "颜色" && (!string.IsNullOrWhiteSpace(productInfo.ColorAlias)))//如果该商品有自定义规格名则用
                {
                    alias = productInfo.ColorAlias;
                }
                else if (text == "尺码" && (!string.IsNullOrWhiteSpace(productInfo.SizeAlias)))
                {
                    alias = productInfo.SizeAlias;
                }
                else if (text == "规格" && (!string.IsNullOrWhiteSpace(productInfo.VersionAlias)))
                {
                    alias = productInfo.VersionAlias;
                }
            }
            return alias;
        }

        private string GetSpecificationTypeAlias(SpecificationType typeId, Mall.Entities.SpecificationValueInfo item)
        {
            string alias = "";
            var type = TypeApplication.GetType(item.TypeId);
            switch (typeId)
            {
                case SpecificationType.Color:
                    alias = type.ColorAlias;
                    break;
                case SpecificationType.Size:
                    alias = type.SizeAlias;
                    break;
                default:
                    alias = type.VersionAlias;
                    break;
            }
            return alias;
        }

        [UnAuthorize]
        public ActionResult PublicStepTwo(string categoryNames = "", long categoryId = 0, long productId = 0)
        {
            var iProduct = _iProductService;
            var iCategory = _iCategoryService;
            string sale = "0", stock = "0", cost = "0";
            bool isChangeCate = false;
            Entities.ProductInfo product = new Entities.ProductInfo();
            if (productId != 0)
            {
                product = iProduct.GetProduct(productId);
                var skus = iProduct.GetSKUs(productId);
                if (product == null || product.ShopId != CurrentSellerManager.ShopId)
                {
                    throw new MallException(productId + ",该商品已删除或者不属于该店铺");
                }
                if (skus.Count > 0)
                {
                    var tempS = skus.Where(s => s.SalePrice > 0);
                    var tempC = skus.Where(s => s.CostPrice > 0);
                    sale = tempS.Count() == 0 ? product.MinSalePrice.ToString("f3") : tempS.Min(s => s.SalePrice).ToString();
                    stock = skus.Sum(s => s.Stock).ToString();
                    cost = tempC.Count() == 0 ? cost : tempC.Min(s => s.CostPrice).ToString();
                }
                if (string.IsNullOrWhiteSpace(categoryNames))
                {
                    var cateArray = product.CategoryPath.Split('|');
                    for (int i = 0; i < cateArray.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(cateArray[i]))
                        {
                            var cobj = iCategory.GetCategory(long.Parse(cateArray[i]));
                            categoryNames += string.Format("{0} {1} ",
                                cobj == null ? "" : cobj.Name,
                                (i == cateArray.Length - 1) ? "" : " > ");
                        }
                    }
                }
                if (categoryId == 0)
                {
                    categoryId = product.CategoryId;
                }
                else
                {
                    if (categoryId != product.CategoryId)
                    {
                        isChangeCate = true;
                    }
                }
                product.CategoryNames = categoryNames;
                if (isChangeCate)
                {
                    product.CategoryId = categoryId;
                }
            }
            else
            {
                product = new Entities.ProductInfo();
                product.CategoryId = categoryId;
                product.CategoryNames = categoryNames;
            }

            var brands = ServiceApplication.Create<IBrandService>().GetBrandsByCategoryIds(shopId, categoryId);
            List<SelectListItem> brandList = new List<SelectListItem> { new SelectListItem
                {
                    Selected = false,
                    Text ="请选择品牌...",
                    Value = "0"
                }};
            foreach (var item in brands)
            {
                brandList.Add(new SelectListItem
                {
                    Selected = productId != 0 && product != null && product.BrandId == item.Id,
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }

            var freightTemplates = ServiceApplication.Create<IFreightTemplateService>().GetShopFreightTemplate(CurrentSellerManager.ShopId);
            var freightList = new List<SelectListItem> { new SelectListItem
                {
                    Selected = false,
                    Text ="请选择运费模板...",
                    Value = "0"
                }};
            foreach (var item in freightTemplates)
            {
                freightList.Add(new SelectListItem
                {
                    Selected = productId != 0 && product != null && product.FreightTemplateId == item.Id,
                    Text = item.Name + "【" + item.ValuationMethod.ToDescription() + "】",
                    Value = item.Id.ToString()
                });
            }
            var desc = ProductManagerApplication.GetProductDescription(product.Id);

            product.IsCategory = productId == 0 ? 1 : 0;
            product.ShopId = shopId;
            product.TopId = desc?.DescriptionPrefixId ?? 0;
            product.BottomId = desc?.DescriptiondSuffixId ?? 0;

            ViewBag.FreightTemplates = freightList;
            ViewBag.BrandDrop = brandList;
            ViewBag.SalePrice = sale;
            ViewBag.Stock = stock;
            ViewBag.CostPrice = cost;

            return View(product);
        }

        public JsonResult GetFreightTemplate()
        {
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
            return Json(new
            {
                success = true,
                model = freightList.Where(f => !f.Value.Equals("0")).ToList()
            });
        }

        public ActionResult Management()
        {
            var auditOnOff = SiteSettingApplication.SiteSettings.ProdutAuditOnOff;
            ViewBag.AuditOnOff = auditOnOff;
            ViewBag.ShopCategorys = ShopCategoryApplication.GetShopCategory(shopId);
            ViewBag.FreightTemplates = FreightTemplateApplication.GetShopFreightTemplate(shopId);
            return View();
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult List(ProductQuery query, string auditStatuses, int page, int rows, bool? isAsc)
        {
            query.PageSize = rows;
            query.PageNo = page;
            query.ShopId = CurrentSellerManager.ShopId;
            if (isAsc.HasValue)//如果按序号排序不为null
            {
                query.OrderKey = 4;
                query.OrderType = isAsc.Value;
            }
            if (query.SaleStatus == Entities.ProductInfo.ProductSaleStatus.InStock)
            {
                query.AuditStatus = new Entities.ProductInfo.ProductAuditStatus[] { Entities.ProductInfo.ProductAuditStatus.Audited
                    , Entities.ProductInfo.ProductAuditStatus.WaitForAuditing
                    , Entities.ProductInfo.ProductAuditStatus.UnAudit
                    , Entities.ProductInfo.ProductAuditStatus.AuditFailed };
            }
            else if (query.SaleStatus == ProductInfo.ProductSaleStatus.InDraft)
            {
                query.NotIncludedInDraft = false;
            }
            var products = ProductManagerApplication.GetProducts(query);
            var proIds = products.Models.Select(p => p.Id);//当前商品所有Id

            var productDescriptions = ProductManagerApplication.GetProductDescription(proIds.ToArray());
            var categories = CategoryApplication.GetCategories();
            var brands = BrandApplication.GetBrandsByIds(products.Models.Select(p => p.BrandId));
            var relationProducts = ProductManagerApplication.GetRelationProductByProductIds(proIds);
            //取超出安全库存的商品ID
            var overSafeStockPids = ProductManagerApplication.GetOverSafeStockProductIds(proIds);

            var skus = ProductManagerApplication.GetSKUByProducts(products.Models.Select(p => p.Id));
            var catePros = _iShopCategoryService.GetCategoryNameAndProductIdByProductId(proIds);//商家商品ID与商家分类

            var dataGrid = new DataGridModel<ProductModel>();
            dataGrid.total = products.Total;
            dataGrid.rows = products.Models.Select(item =>
            {
                var cate = catePros.Where(t => t.ProductId == item.Id).FirstOrDefault();
                var branditem = (item.BrandId != 0 ? brands.FirstOrDefault(b => b.Id == item.BrandId) : null);
                var relationitem = (item.BrandId != 0 ? relationProducts.FirstOrDefault(b => b.ProductId == item.Id) : null);
                return new ProductModel()
                {
                    Name = item.ProductName,
                    Id = item.Id,
                    Image = item.GetImage(ImageSize.Size_50),
                    Price = item.MinSalePrice,
                    Url = "",
                    PublishTime = item.AddedDate.ToString("yyyy-MM-dd HH:mm"),
                    SaleState = (int)item.SaleStatus,
                    CategoryId = item.CategoryId,
                    BrandId = item.BrandId,
                    AuditState = (int)item.AuditStatus,
                    AuditReason = productDescriptions.Any(pd => pd.ProductId == item.Id) ? productDescriptions.FirstOrDefault(pd => pd.ProductId == item.Id).AuditReason : "",
                    ProductCode = item.ProductCode,
                    QrCode = GetQrCodeImagePath(item.Id),
                    SaleCount = item.SaleCounts,
                    Unit = item.MeasureUnit,
                    Uid = CurrentSellerManager.Id,
                    CategoryName = cate == null ? "" : cate.CategoryName,
                    BrandName = branditem == null ? "" : branditem.Name,
                    RelationProducts = relationitem == null ? "" : relationitem.Relation,
                    IsOverSafeStock = overSafeStockPids.Any(e => e == item.Id),
                    Stock = skus.Where(sku => sku.ProductId == item.Id).Sum(sku => sku.Stock),
                    MaxBuyCount = item.MaxBuyCount,
                    ShopDisplaySequence = item.ShopDisplaySequence,
                    VirtualSaleCounts = item.VirtualSaleCounts,
                    IsOpenLadder = item.IsOpenLadder,
                    ProductType = item.ProductType
                };
            }).ToList();

            return Json(dataGrid);
        }

        public ActionResult ExportToExcel(long? categoryId = null, string productCode = "", string brandName = "", int? auditStatus = null, string auditStatuses = null, int? saleStatus = null, string ids = "", string keyWords = "", DateTime? startDate = null, DateTime? endDate = null, long? freightTemplateId = null, sbyte? productType = null)
        {
            #region 获取查询数据

            var query = new ProductQuery()
            {
                SaleStatus = (Entities.ProductInfo.ProductSaleStatus?)saleStatus,
                PageSize = int.MaxValue,
                PageNo = 1,
                BrandName = brandName,
                KeyWords = keyWords,
                ShopCategoryId = categoryId,
                Ids = string.IsNullOrWhiteSpace(ids) ? null : ids.Split(',').Select(item => long.Parse(item)),
                ShopId = CurrentSellerManager.ShopId,
                StartDate = startDate,
                EndDate = endDate,
                ProductCode = productCode,
                FreightTemplateId = freightTemplateId
            };
            if (productType.HasValue && productType.Value > -1)
            {
                query.ProductType = productType.Value;
            }
            if (!string.IsNullOrWhiteSpace(auditStatuses))
            {
                query.AuditStatus = auditStatuses.Split(',').Select(item => (Entities.ProductInfo.ProductAuditStatus)(long.Parse(item))).ToArray();
                if (auditStatuses == "1,3" || auditStatuses == "1")//查询待审核时，仅查出售中状态的
                    query.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
            }

            if (saleStatus.HasValue && saleStatus == (int)Entities.ProductInfo.ProductSaleStatus.InStock && auditStatus != (int)ProductInfo.ProductAuditStatus.InfractionSaleOff)
                query.AuditStatus = new ProductInfo.ProductAuditStatus[] { ProductInfo.ProductAuditStatus.Audited, ProductInfo.ProductAuditStatus.UnAudit, Entities.ProductInfo.ProductAuditStatus.WaitForAuditing };

            if (auditStatus.HasValue)
                query.AuditStatus = new ProductInfo.ProductAuditStatus[] { (ProductInfo.ProductAuditStatus)auditStatus };

            var productEntities = ProductManagerApplication.GetProducts(query);

            #region 先查询到列表，便于下面循环读取
            List<CategoryInfo> listproductcate = new List<CategoryInfo>();//商品分类
            List<BrandInfo> listbrand = new List<BrandInfo>();//品牌
            List<FreightTemplateInfo> listtemplate = new List<FreightTemplateInfo>();//运费模板
            List<Mall.DTO.ProductShopCategory> listshopcate = new List<DTO.ProductShopCategory>();//商铺分类
            Dictionary<long, string> shopnames = new Dictionary<long, string>();
            if (productEntities.Models != null)
            {
                List<long> cateids = productEntities.Models.Select(p => p.CategoryId).ToList();
                listproductcate = CategoryApplication.GetCateogryListByIds(cateids);//商铺分类
                if (listproductcate == null)
                    listproductcate = new List<CategoryInfo>();

                List<long> templateIds = productEntities.Models.Select(p => p.FreightTemplateId).ToList();
                listtemplate = FreightTemplateApplication.GetFreightTemplateList(templateIds);//运费模板
                if (listtemplate == null)
                    listtemplate = new List<FreightTemplateInfo>();//实例下便免下面循环里判断空

                IEnumerable<long> brandIds = productEntities.Models.Select(p => p.BrandId);//品牌
                listbrand = BrandApplication.GetBrands(brandIds);
                if (listbrand == null)
                    listbrand = new List<BrandInfo>();

                List<long> productids = productEntities.Models.Select(p => p.Id).ToList();
                listshopcate = ShopCategoryApplication.GetCategorysByProduct(productids);
                if (listshopcate == null)
                    listshopcate = new List<DTO.ProductShopCategory>();
            }
            #endregion

            var brandService = ServiceApplication.Create<IBrandService>();
            var products = productEntities.Models.Select(item =>
            {
                //TODO:FG 循环内查询(大数据)
                var desc = ProductManagerApplication.GetProductDescription(item.Id);
                var skus = _iProductService.GetSKUs(item.Id);

                var brand = (item.BrandId <= 0) ? null : listbrand.Where(p => p.Id == item.BrandId).FirstOrDefault();//品牌
                var freightTem = (item.FreightTemplateId <= 0) ? null : listtemplate.Where(p => p.Id == item.FreightTemplateId).FirstOrDefault();//运费模板
                var platFormCate = (item.CategoryId <= 0) ? null : listproductcate.Where(p => p.Id == item.CategoryId).FirstOrDefault();//平台分类
                var shopcate = listshopcate.Where(p => p.ProductId == item.Id).FirstOrDefault();//商铺分类

                return new ProductInfoForExportModel()
                {
                    Id = item.Id,
                    CategoryName = shopcate == null ? "" : shopcate.ShopCategoryName,
                    BrandName = item.BrandId == 0 || brand == null ? "" : brand.Name,
                    ProductName = item.ProductName,
                    MarketPrice = item.MarketPrice,
                    MinSalePrice = item.MinSalePrice,
                    ProductCode = item.ProductCode,
                    ShortDescription = item.ShortDescription,
                    SaleStatus = item.SaleStatus,
                    AddedDate = item.AddedDate,
                    HasSKU = item.HasSKU,
                    VistiCounts = item.VistiCounts,
                    SaleCounts = item.SaleCounts,
                    AuditStatus = item.AuditStatus,
                    AuditStatusText = (item.AuditStatus == ProductInfo.ProductAuditStatus.Audited ? "已审核" : "待审核"),
                    AuditReason = desc?.AuditReason ?? string.Empty,
                    Quantity = item.Quantity,
                    MeasureUnit = item.MeasureUnit,
                    SKUInfo = skus,
                    PlatFormCategoryName = platFormCate == null ? "" : platFormCate.Name,   //平台分类
                    FreightTemplateName = freightTem == null ? "" : freightTem.Name,   //运费模板名称
                    IsOpenLadder = item.IsOpenLadder,   //是否开启阶梯批发
                    ProductLadderPrice = ProductManagerApplication.GetLadderPriceInfosByProductId(item.Id, item.IsOpenLadder),
                    ProductTypeName = item.ProductType == 1 ? "虚拟商品" : "实物商品",   //商品类型(虚拟或实物)
                    VirtualProduct = (item.ProductType == 1 ? ProductManagerApplication.GetVirtualProductInfoByProductId(item.Id) : null),  //虚拟商品
                };
            }).ToList();
            #endregion 获取查询数据

            #region 构建Excel文档

            ViewData.Model = products;
            string viewHtml = RenderPartialViewToString(this, "ExportProductinfo");
            return File(System.Text.Encoding.UTF8.GetBytes(viewHtml), "application/ms-excel", string.Format("店铺商品信息_{0}.xls", DateTime.Now.ToString("yyyy-MM-dd")));

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

        public string GetQrCodeImagePath(long productId)
        {
            var map = Core.Helper.QRCodeHelper.Create(CurrentUrlHelper.CurrentUrlNoPort() + "/m-wap/product/detail/" + productId);
            MemoryStream ms = new MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            //  将图片内存流转成base64,图片以DataURI形式显示
            string strUrl = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray());
            return strUrl;
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult Browse(long? categoryId, int? auditStatus, string ids, int page, string keyWords, string shopName,
            int? saleStatus, bool? isShopCategory, int rows = 10, bool isLimitTimeBuy = false, bool showSku = false, long[] exceptProductIds = null, sbyte productType = -1)
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
                //IsFilterStock = true,
                //ProductType = productType
            };
            if (productType > -1)
            {
                query.ProductType = productType;
            }
            if (auditStatus.HasValue)
                query.AuditStatus = new ProductInfo.ProductAuditStatus[] { (ProductInfo.ProductAuditStatus)auditStatus };

            if (saleStatus.HasValue)
                query.SaleStatus = (ProductInfo.ProductSaleStatus)saleStatus;

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

        [HttpPost]
        [UnAuthorize]
        public JsonResult BatchSaleOff(string ids)
        {
            IEnumerable<long> ids_long = ids.Split(',').Select(item => long.Parse(item));

            _iProductService.SaleOff(ids_long, CurrentSellerManager.ShopId);
            ServiceApplication.Create<IOperationLogService>().AddSellerOperationLog(new Entities.LogInfo
            {
                Date = DateTime.Now,
                Description = "商家商品批量下架，Ids=" + ids,
                IPAddress =  base.Request.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress.ToString(),
                PageUrl = "/Product/BatchSaleOff",
                UserName = CurrentSellerManager.UserName,
                ShopId = CurrentSellerManager.ShopId
            });

            _iSearchProductService.UpdateSearchStatusByProducts(ids_long.ToList());
            foreach (var item in ids_long)
            {
                //处理门店
                ShopBranchApplication.UnSaleProduct(item);
            }

            return Json(new
            {
                success = true
            });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult BatchOnSale(string ids)
        {
            IEnumerable<long> ids_long = ids.Split(',').Select(item => long.Parse(item));

            _iProductService.OnSale(ids_long, CurrentSellerManager.ShopId);
            ServiceApplication.Create<IOperationLogService>().AddSellerOperationLog(new Entities.LogInfo
            {
                Date = DateTime.Now,
                Description = "商家商品批量上架，Ids=" + ids,
                IPAddress =  base.Request.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress.ToString(),
                PageUrl = "/Product/BatchOnSale",
                UserName = CurrentSellerManager.UserName,
                ShopId = CurrentSellerManager.ShopId
            });
            #region 更新搜索状态
            _iSearchProductService.UpdateSearchStatusByProducts(ids_long.ToList());
            #endregion
            return Json(new
            {
                success = true
            });
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult Delete(string ids)
        {
            try
            {
                IEnumerable<long> ids_long = ids.Split(',').Select(item => long.Parse(item));
                //状态改为 已删除
                ProductManagerApplication.DeleteProduct(ids_long, CurrentSellerManager.ShopId);

                _iSearchProductService.UpdateSearchStatusByProducts(ids_long.ToList());

                #region 它虽删除了商品，但已下单的订单页商品、分享或浏览记录等会需要用到商品图片，则这里不删除图片处理；
                //foreach (var id in ids_long)
                //{
                //    string path = (string.Format(@"/Storage/Shop/{0}/Products/{1}", CurrentSellerManager.ShopId, id));
                //    if (MallIO.ExistDir(path))
                //        Core.MallIO.DeleteDir(path, true);
                //}
                #endregion

                ServiceApplication.Create<IOperationLogService>().AddSellerOperationLog(new Entities.LogInfo
                {
                    Date = DateTime.Now,
                    Description = "商家删除商品，Ids=" + ids,
                    IPAddress =  base.Request.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress.ToString(),
                    PageUrl = "/Product/Delete",
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
        /// 删除指定路径文件夹
        /// </summary>
        /// <param name="path"></param>
        private void DeleteDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                DirectoryInfo[] childs = dir.GetDirectories();
                foreach (DirectoryInfo child in childs)
                {
                    child.Delete(true);
                }
                dir.Delete(true);
            }
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult BindTemplates(string ids, long? topTemplateId, long? bottomTemplateId)
        {
            IEnumerable<long> productIds = ids.Split(',').Select(item => long.Parse(item));
            _iProductService.BindTemplate(topTemplateId, bottomTemplateId, productIds);
            foreach (var pid in productIds)
            {
                string cacheKey = CacheKeyCollection.CACHE_PRODUCTDESC(pid);
                Cache.Remove(cacheKey);
            }
            return Json(new
            {
                success = true
            });
        }

        public JsonResult Recommend(long productId, string productIds)
        {
            bool success = false;
            try
            {
                Application.ProductManagerApplication.UpdateRelationProduct(productId, productIds);
                success = true;
            }
            catch
            { }

            return Json(success);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SetProductOverSafeStock(string ids, string stock)
        {
            long safeStock = 0;
            if (!long.TryParse(stock, out safeStock))
            {
                throw new MallException("您输入的信息有误，此处只能输入大于等于0的整数！");
            }
            if (safeStock < 0)
            {
                throw new MallException("库存必须大于0");
            }
            IEnumerable<long> productIds = ids.Split(',').Select(item => long.Parse(item));
            ProductManagerApplication.SetProductOverSafeStock(productIds, safeStock);
            return Json(new
            {
                success = true
            });
        }

        /// <summary>
        /// 获取可以批量设置的运费模板
        /// </summary>
        /// <param name="ids">商品的模板ID集合</param>
        /// <returns></returns>

        public JsonResult GetFreightTemplatesByIds(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return Json(new { success = false, msg = "参数传递错误" });

            var pIds = ids.Split(',').Select(a => long.Parse(a)).ToList();
            var products = ProductManagerApplication.GetProductByIds(pIds);
            var number = products.Count;
            var templateIds = products.Select(a => a.FreightTemplateId).ToList();
            var freightTemplates = ServiceApplication.Create<IFreightTemplateService>().GetShopFreightTemplate(CurrentSellerManager.ShopId);

            List<long> result1 = freightTemplates.Where(a => a.IsFree == FreightTemplateType.Free || a.ValuationMethod == ValuationMethodType.Piece).Select(a => a.Id).ToList();
            List<long> result2 = new List<long>();
            List<long> result3 = new List<long>();
            var productsFreightTemplates = freightTemplates.Where(a => templateIds.Contains(a.Id));//所选商品中的运费模板
            var existWeight = productsFreightTemplates.Where(a => a.ValuationMethod == ValuationMethodType.Weight).ToList();
            var existBulk = productsFreightTemplates.Where(a => a.ValuationMethod == ValuationMethodType.Bulk).ToList();

            if (existWeight != null && existWeight.Count > 0)
            {
                result2 = freightTemplates.Where(a => a.ValuationMethod == ValuationMethodType.Weight || a.IsFree == FreightTemplateType.Free || a.ValuationMethod == ValuationMethodType.Piece).Select(a => a.Id).ToList();
            }
            if (existBulk != null && existBulk.Count > 0)
            {
                result3 = freightTemplates.Where(a => a.ValuationMethod == ValuationMethodType.Bulk || a.IsFree == FreightTemplateType.Free || a.ValuationMethod == ValuationMethodType.Piece).Select(a => a.Id).ToList();
            }
            var intersectIds = new List<long>(); //取三者集合的交集ID
            if (result1.Count > 0 && result2.Count > 0 && result3.Count > 0)
            {
                if (number == 1)
                {
                    intersectIds = result1.Union(result2).Union(result3).ToList();
                }
                else
                {
                    intersectIds = result1.Intersect(result2).Intersect(result3).ToList();
                }
            }
            else if (result1.Count > 0 && result2.Count > 0)
            {
                if (number == 1)
                {
                    intersectIds = result1.Union(result2).ToList();
                }
                else
                {
                    intersectIds = result1.Intersect(result2).ToList();
                }
            }
            else if (result1.Count > 0 && result3.Count > 0)
            {
                if (number == 1)
                {
                    intersectIds = result1.Union(result3).ToList();
                }
                else
                {
                    intersectIds = result1.Intersect(result3).ToList();
                }
            }
            else if (result2.Count > 0 && result3.Count > 0)
            {
                if (number == 1)
                {
                    intersectIds = result2.Union(result3).ToList();
                }
                else
                {
                    intersectIds = result2.Intersect(result3).ToList();
                }

            }
            else if (result1.Count > 0)
            {
                intersectIds = result1;
            }
            else if (result2.Count > 0)
            {
                intersectIds = result2;
            }
            else if (result3.Count > 0)
            {
                intersectIds = result3;
            }

            freightTemplates = freightTemplates.Where(a => intersectIds.Contains(a.Id)).ToList();
            return Json(new
            {
                success = true,
                model = freightTemplates.Select(a => new
                {
                    a.Name,
                    a.Id,
                    a.ValuationMethod
                })
            });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult BatchSettingFreightTemplate(string ids, long freightTemplateId)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return Json(new { success = false, msg = "所选商品不能为空" });
            }
            if (freightTemplateId == 0)
            {
                return Json(new { success = false, msg = "运费模板不能为空" });
            }
            IEnumerable<long> productIds = ids.Split(',').Select(item => long.Parse(item));
            var products = ProductManagerApplication.GetProductByIds(productIds);
            int failureNum = 0;
            List<long> pids = new List<long>();
            products.ForEach(a =>
            {
                if (a.ProductType == 1)
                {
                    failureNum++;
                }
                else
                {
                    pids.Add(a.Id);
                }
            });
            if (pids.Count == 0)
            {
                return Json(new { success = false, msg = "虚拟商品不支持运费设置" });
            }

            var result = ProductManagerApplication.BatchSettingFreightTemplate(pids, freightTemplateId);
            return Json(new
            {
                success = result,
                successNum = pids.Count,
                failureNum = failureNum
            });
        }

        /// <summary>
        /// 获取商品信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public JsonResult GetProductsByIds(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return Json(new { success = false, msg = "参数传递错误" });

            var pIds = ids.Split(',').Select(a => long.Parse(a)).ToList();
            var products = ProductManagerApplication.GetAllProductByIds(pIds);
            List<SKuModel> skuLis = new List<SKuModel>();
            SKuModel model = null;
            foreach (var item in products)
            {
                var skus = _iProductService.GetSKUs(item.Id);

                foreach (var skuInfo in skus)
                {
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
                    model = new SKuModel()
                    {
                        ProductName = item.ProductName + (item.HasSKU ? "（" + str.TrimEnd('；') + "）" : ""),
                        Sku = item.HasSKU ? (skuInfo.Sku ?? string.Empty) : item.ProductCode,
                        Id = skuInfo.Id,
                        AutoId = skuInfo.AutoId,
                        Stock = skuInfo.Stock,
                        MarketPrice = item.MarketPrice,
                        SalePrice = item.HasSKU ? skuInfo.SalePrice : item.MinSalePrice,//有规格则取规格商城价，否则取商品本身商城价
                        ProductId = item.Id,
                        HasSKU = item.HasSKU ? 1 : 0
                    };
                    if (skus.IndexOf(skuInfo) == 0)
                    {
                        model.IsFirst = 1;
                    }
                    skuLis.Add(model);
                }
            }
            return Json(new
            {
                success = true,
                model = skuLis
            });

        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult BatchSettingStock(List<StockModel> stocks, string productIds)
        {
            if (stocks == null)
            {
                return Json(new { success = false, msg = "参数传递错误！" });
            }
            Dictionary<long, long> dics = stocks.ToDictionary(key => key.AutoId, value => value.Stock);
            var result = ProductManagerApplication.BatchSettingStock(dics);
            return Json(new
            {
                success = result,
                msg = ""
            });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult BatchSettingPrice(List<ProductModel> products, List<PriceModel> prices)
        {
            if (products == null)
            {
                return Json(new { success = false, msg = "参数传递错误！" });
            }
            if (prices == null)
            {
                return Json(new { success = false, msg = "参数传递错误！" });
            }
            Dictionary<long, decimal> productDics = products.ToDictionary(key => key.Id, value => value.MarketPrice);
            Dictionary<long, string> priceDics = prices.ToDictionary(key => key.AutoId, value => string.Format(value.SalePrice + "@" + value.HasSku + "@" + value.ProductId));
            var result = ProductManagerApplication.BatchSettingPrice(productDics, priceDics);
            if (result && products != null)
            {
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    products.ForEach(a =>
                    {
                        ProductManagerApplication.CreateHtml(a.Id);
                        ProductManagerApplication.SaveCaculateMinPrice(a.Id, CurrentShop.Id);
                    });
                });
            }
            return Json(new
            {
                success = result,
                msg = ""
            });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult UpdateShopDisplaySequence(long id, int order)
        {
            if (order < 0)
            {
                return Json(new { success = false, msg = "商品序号必须大于0！" });
            }
            var result = ProductManagerApplication.UpdateShopDisplaySequence(id, order);
            return Json(new { success = result, msg = "" });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult PostRecordedAudioVideo()
        {
            string DirUrl = IOHelper.GetMapPath("~/videos/");
            if (!System.IO.Directory.Exists(DirUrl))
            {
                System.IO.Directory.CreateDirectory(DirUrl);
            }
            string filename = "";
            foreach (IFormFile upload in Request.Form.Files)
            {
                var path = AppDomain.CurrentDomain.BaseDirectory + "videos/";
                //  var file = Request.Form.Files[upload];
                var file = upload;

                if (file == null) continue;

                filename = DateTime.Now.ToString("yyyyMMddHHmmssffffff") + Path.GetExtension(file.FileName);
              //  file.SaveAs(Path.Combine(path, filename));

                FileStream stream = new FileStream(Path.Combine(path, filename), FileMode.OpenOrCreate);

                file.CopyTo(stream);

            }
            return Json(new { success = true, videopath = "/videos/" + filename, msg = "" });
        }

        [HttpPost]
        public JsonResult RefreshUploadVedio()
        {
            var importCache = Cache.Get<object>(CacheKeyCollection.CACHE_UPLOADVIDEO("SellerAdmin_UploadSpeed_" + HttpContext.Session.Id));

            string value = "";
            if (importCache != null)
            {
                value = (string)importCache;
            }
            return Json(new
            {
                success = true,
                value = value
            });
        }


        #endregion

        #region 私有方法
        private JsonResult Json(bool success = true, string message = null)
        {
            return Json(new
            {
                Success = success,
                Message = message
            }, true);
        }

        private JsonResult ValidError()
        {
            var errors = ModelState.Where(p => p.Value.Errors.Count > 0).Select(p => new
            {
                p.Key,
                p.Value.Errors[0].ErrorMessage
            }).ToArray();

            return Json(new
            {
                success = false,
                errors,
                message = "验证失败"
            });
        }

        private bool CanCreate(out string message)
        {
            if (ServiceApplication.Create<IShopService>().GetShopSurplusSpace(CurrentSellerManager.ShopId) <= 0)
            {
                message = "存储图片空间不足,不能发布商品!";
                return false;
            }

            var grade = ServiceApplication.Create<IShopService>().GetShopGrade(CurrentShop.GradeId);
            if (grade != null)
            {
                var count = _iProductService.GetShopAllProducts(CurrentSellerManager.ShopId);
                if (count >= grade.ProductLimit)
                {
                    message = "此店铺等级最多只能发布" + grade.ProductLimit + "件商品";
                    return false;
                }
            }

            message = "";
            return true;
        }

        private ProductCreateModel InitCreateModel(ProductCreateModel model = null)
        {
            if (model == null)
                model = new ProductCreateModel();

            var shopId = this.CurrentShop.Id;
            var categories = _iShopCategoryService.GetBusinessCategory(shopId, this.CurrentShop.IsSelf);
            var categoryGroups = this.GroupCategory(categories.ToList());

            model.CategoryGroups = categoryGroups;
            model.FreightTemplates = FreightTemplateApplication.GetShopFreightTemplate(shopId);
            model.ShopCategorys = ShopCategoryApplication.GetShopCategory(shopId);
            model.DescriptionTemplates = ProductManagerApplication.GetDescriptionTemplatesByShopId(shopId);
            if (model.Id > 0)
            {
                model.CurrentJoinActive = ProductManagerApplication.CurrentJoinActive(model.Id);
                var virtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(model.Id);
                if (virtualProductInfo != null)
                {
                    model.ValidityType = virtualProductInfo.ValidityType;
                    model.StartDate = virtualProductInfo.StartDate.HasValue ? virtualProductInfo.StartDate.Value.ToString("yyyy-MM-dd") : "";
                    model.EndDate = virtualProductInfo.EndDate.HasValue ? virtualProductInfo.EndDate.Value.ToString("yyyy-MM-dd") : "";
                    model.EffectiveType = (ProductInfo.VirtualProductEffectiveType)virtualProductInfo.EffectiveType;
                    model.Hour = virtualProductInfo.Hour;
                    model.SupportRefundType = (ProductInfo.SupportVirtualRefundType)virtualProductInfo.SupportRefundType;
                    model.UseNotice = virtualProductInfo.UseNotice;
                }
                model.VirtualProductItemInfo = ProductManagerApplication.GetVirtualProductItemInfoByProductId(model.Id).ToArray();

            }


            return model;
        }

        private ProductCreateModel InitEditModel(ProductInfo product, ProductCreateModel model = null)
        {
            if (model == null)
            {
                model = product.DynamicMap<ProductCreateModel>();
                model.GoodsCategory = ProductManagerApplication.GetProductShopCategoriesByProductId(product.Id).Select(item => item.ShopCategoryId).ToArray();
                model.SKUExs = ProductManagerApplication.GetSKUByProducts(new[] { product.Id }).Select(p => p.DynamicMap<SKUEx>()).ToArray();
                var description = ProductManagerApplication.GetProductDescription(product.Id);
                model.Description = description.Map<ProductDescription>();

                model.SelectAttributes = ProductManagerApplication.GetProductAttributes(product.Id)
                    .GroupBy(p => p.AttributeId)
                    .Select(p => new AttrSelectData()
                    {
                        AttributeId = p.Key,
                        ValueId = string.Join(",", p.Select(item => item.ValueId))
                    }).ToArray();
                //编辑商品获取批价--张宇枫
                model.LadderPrices = ProductManagerApplication.GetLadderPriceByProductIds(model.Id);
            }

            model.Stock = model.SKUExs.Sum(p => p.Stock);
            model.SafeStock = model.SKUExs.Min(p => p.SafeStock);

            //活动启用后禁用SKU编辑模块
            model.AllowSKUEdit = true;
            //LimitTimeApplication
            model.ColorAlias = product.ColorAlias;
            model.SizeAlias = product.SizeAlias;
            model.VersionAlias = product.VersionAlias;
            model.ProductType = product.ProductType;
            InitCreateModel(model);

            return model;
        }

        private List<CategoryGroup> GroupCategory(List<Entities.CategoryInfo> categorys, long pid = 0)
        {
            var result = new List<CategoryGroup>();
            var parents = categorys.Where(p => p.ParentCategoryId == pid);

            foreach (var parent in parents)
            {
                var item = new CategoryGroup();
                item.Id = parent.Id;
                item.Name = parent.Name;
                item.Path = parent.Path;
                item.TypeId = parent.TypeId;
                item.SubCategorys = GroupCategory(categorys, item.Id);
                item.SupportVirtualProduct = parent.SupportVirtualProduct;

                result.Add(item);
            }

            return result;
        }
        #endregion
    }
}