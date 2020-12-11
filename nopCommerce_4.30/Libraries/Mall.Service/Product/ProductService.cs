using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube;
using NetRube.Data;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Mall.Entities.ProductInfo;

namespace Mall.Service
{
    public class ProductService : ServiceBase, IProductService
    {
        #region 平台服务
        public QueryPageModel<ProductInfo> GetProducts(ProductQuery query)
        {
            var db = WhereBuilder(query);

            #region Sort排序
            if (!string.IsNullOrEmpty(query.Sort))
            {
                switch (query.Sort.ToLower())
                {
                    case "salecounts":
                        if (query.IsAsc) db.OrderBy(p => p.SaleCounts);
                        else db.OrderByDescending(p => p.SaleCounts);
                        break;
                    case "addeddate":
                        if (query.IsAsc) db.OrderBy(p => p.AddedDate);
                        else db.OrderByDescending(p => p.AddedDate);
                        break;
                    case "displaysequence":
                        if (query.IsAsc) db.OrderBy(p => p.DisplaySequence);
                        else db.OrderByDescending(p => p.DisplaySequence);
                        break;
                    case "shopdisplaysequence":
                        if (query.IsAsc)
                            db.OrderBy(p => p.ShopDisplaySequence);
                        else
                            db.OrderByDescending(p => p.ShopDisplaySequence);
                        break;
                    case "displaysale":
                        if (query.IsAsc)
                            db.OrderBy(p => p.SaleCounts + p.VirtualSaleCounts);
                        else
                            db.OrderByDescending(p => p.SaleCounts + p.VirtualSaleCounts);
                        break;
                    default:
                        db.OrderByDescending(p => p.AddedDate);
                        break;
                }
            }
            else
            {
                //TODO:FG 当前排序规则需要重构
                switch (query.OrderKey)
                {
                    case 2:
                        if (!query.OrderType)
                            db.OrderByDescending(p => p.AddedDate);
                        else
                            db.OrderBy(p => p.AddedDate);
                        break;
                    case 3:
                        if (!query.OrderType)
                            db.OrderByDescending(p => p.SaleCounts);
                        else
                            db.OrderBy(p => p.SaleCounts);
                        break;
                    case 4:
                        if (!query.OrderType)
                            db.OrderByDescending(p => p.ShopDisplaySequence);
                        else
                            db.OrderBy(p => p.ShopDisplaySequence);
                        break;
                    case 5:
                        if (!query.OrderType)
                            db.OrderByDescending(p => p.DisplaySequence);
                        else
                            db.OrderBy(p => p.DisplaySequence);
                        break;
                    default:
                        if (!query.OrderType)
                            db.OrderByDescending(p => p.Id);
                        else
                            db.OrderBy(p => p.Id);
                        break;
                }
            }
            #endregion

            var data = db.ToPagedList(query.PageNo, query.PageSize);

            return new QueryPageModel<ProductInfo>()
            {
                Models = data,
                Total = data.TotalRecordCount
            };
        }
        public int GetProductCount(ProductQuery query)
        {
            var db = WhereBuilder(query);
            return db.Count();
        }
        public long GetProductVirtualSale(long shop)
        {
            return DbFactory.Default.Get<ProductInfo>().Where(p => p.ShopId == shop
            && p.AuditStatus == ProductInfo.ProductAuditStatus.Audited
            && p.IsDeleted == false).Sum<long>(p => p.VirtualSaleCounts);
        }
        /// <summary>
        /// 根据条件获取商品
        /// </summary>
        /// <param name="query">商品查询模型</param>
        /// <param name="total">总记录数</param>
        /// <returns></returns>
        private GetBuilder<ProductInfo> WhereBuilder(ProductQuery query)
        {
            var db = DbFactory.Default.Get<ProductInfo>().Where(item => item.IsDeleted == false);
            if (query.IsFilterStock)
            {
                var skuinfosql = DbFactory.Default
                    .Get<SKUInfo>()
                    .Where<SKUInfo, ProductInfo>((si, pi) => si.ProductId == pi.Id && si.Stock > 0);
                db.Where(p => p.ExExists(skuinfosql));

            }
            if (query.ProductType.HasValue && query.ProductType.Value > -1)
            {
                db.Where(item => item.ProductType == query.ProductType.Value);
            }
            if (query.ShopId.HasValue)//过滤店铺
                db.Where(item => item.ShopId == query.ShopId);

            if (query.Ids != null && query.Ids.Count() > 0)//条件 编号
                db.Where(item => item.Id.ExIn(query.Ids));

            //排除某些商品的ID
            if (query.ExceptIds != null && query.ExceptIds.Count() > 0)
            {
                db.Where(item => item.Id.ExNotIn(query.ExceptIds));
            }

            if (!string.IsNullOrWhiteSpace(query.ProductCode))
                db.Where(item => item.ProductCode == query.ProductCode);


            if (query.OverSafeStock)
            {
                var skuinfosql = DbFactory.Default
                    .Get<SKUInfo>()
                    .Where<SKUInfo, ProductInfo>((si, pi) => si.ProductId == pi.Id && si.SafeStock >= si.Stock);
                db.Where(p => p.ExExists(skuinfosql));//有一个sku库存不足则显示
            }

            if (query.AuditStatus != null && query.AuditStatus.Count() > 0)//条件 审核状态
                db.Where(item => item.AuditStatus.ExIn(query.AuditStatus));

            if (query.SaleStatus.HasValue)
                db.Where(item => item.SaleStatus == query.SaleStatus.Value);

            //if (query.CategoryId.HasValue)//条件 分类编号
            //    db.Where(item => ("|" + item.CategoryPath + "|").Contains("|" + query.CategoryId.Value + "|"));

            if (query.Categories != null)
                db.Where(item => item.CategoryId.ExIn(query.Categories));

            if (query.FreightTemplateId.HasValue && query.FreightTemplateId.Value > 0)
            {
                db.Where(item => item.FreightTemplateId == query.FreightTemplateId.Value);
            }

            if (query.NotIncludedInDraft)
                db.Where(item => item.SaleStatus != ProductInfo.ProductSaleStatus.InDraft);

            if (query.StartDate.HasValue)//添加日期筛选
                db.Where(item => item.AddedDate >= query.StartDate);
            if (query.EndDate.HasValue)//添加日期筛选
            {
                var end = query.EndDate.Value.Date.AddDays(1);
                db.Where(item => item.AddedDate < end);
            }
            if (!string.IsNullOrWhiteSpace(query.KeyWords))// 条件 关键字
                db.Where(item => item.ProductName.Contains(query.KeyWords));

            if (!string.IsNullOrWhiteSpace(query.ShopName))//查询商家关键字
            {
                var shopIds = DbFactory.Default
                    .Get<ShopInfo>()
                    .Where<ProductInfo>((si, pi) => si.Id == pi.ShopId)
                    .Where(item => item.ShopName.Contains(query.ShopName))
                    .Select(item => item.Id);
                db.Where(item => item.ExExists(shopIds));
            }
            if (query.IsLimitTimeBuy)
            {
                var limits = DbFactory.Default
                    .Get<FlashSaleInfo>()
                    .Where<ProductInfo>((ltmi, pi) => ltmi.ProductId == pi.Id)
                    .Where(l => l.Status == FlashSaleInfo.FlashSaleStatus.Ongoing)
                    .Select(l => l.ProductId);
                db.Where(p => p.ExNotExists(limits));
            }

            if (!string.IsNullOrEmpty(query.BrandName))
            {
                var brandIds = DbFactory.Default
                    .Get<BrandInfo>()
                    .Where<ProductInfo>((bi, pi) => bi.Id == pi.BrandId)
                    .Where(p => p.Name.Contains(query.BrandName) && p.IsDeleted == false)
                    .Select(p => p.Id);
                db.Where(p => p.ExExists(brandIds));
            }

            if (query.ShopCategoryId.HasValue && query.ShopId.HasValue)
            {
                var shopCategoryId = query.ShopCategoryId.Value;
                var shopId = query.ShopId.Value;
                var shopCategoryIds = DbFactory.Default
                    .Get<ShopCategoryInfo>()
                    .Where(p => p.ShopId == shopId && (p.Id == shopCategoryId || p.ParentCategoryId == shopCategoryId))
                    .Select(p => p.Id);

                var productIds = DbFactory.Default
                    .Get<ProductShopCategoryInfo>()
                    .Where(p => p.ShopCategoryId.ExIn(shopCategoryIds))
                    .Select(p => p.ProductId);

                db.Where(p => p.Id.ExIn(productIds));
            }
            if (query.OnSale.HasValue && query.OnSale.Value)
                db.Where(p => p.SaleStatus == ProductSaleStatus.OnSale && p.AuditStatus == ProductAuditStatus.Audited);

            if (query.HasLadderProduct.HasValue && query.HasLadderProduct == false)
            {
                db.Where(d => d.IsOpenLadder == false);
            }
            return db;
        }

        public void AuditProduct(long productId, ProductInfo.ProductAuditStatus auditStatus, string message)
        {
            ProductInfo product = DbFactory.Default.Get<ProductInfo>().Where(p => p.Id == productId).FirstOrDefault();
            if (product == null)
                throw new MallException("此商品不存在");
            if (product.IsDeleted)
                throw new MallException("此商品已被删除");
            AuditProducts(new long[] { productId }, auditStatus, message);
        }

        public void AuditProducts(IEnumerable<long> productIds, ProductInfo.ProductAuditStatus auditStatus, string message)
        {
            var products = DbFactory.Default.Get<ProductInfo>().Where(item => item.IsDeleted == false && item.Id.ExIn(productIds)).ToList();
            DbFactory.Default
                .InTransaction(() =>
                {
                    foreach (var product in products)
                    {
                        product.AuditStatus = auditStatus;
                        switch (auditStatus)
                        {
                            case ProductInfo.ProductAuditStatus.Audited:
                                product.EditStatus = ProductInfo.ProductEditStatus.Normal;   //申核通过  置位修改生效
                                break;

                            case ProductInfo.ProductAuditStatus.InfractionSaleOff:
                                product.EditStatus = ProductInfo.ProductEditStatus.CompelPendingAudit;   //违规下架  置位强制需审核
                                break;
                        }

                        DbFactory.Default.Update(product);
                        if (!string.IsNullOrEmpty(message))
                        {
                            var desc = DbFactory.Default.Get<ProductDescriptionInfo>().Where(p => p.ProductId == product.Id).FirstOrDefault();
                            if (desc == null)
                                desc = new ProductDescriptionInfo { ProductId = product.Id };
                            desc.AuditReason = message;
                            DbFactory.Default.Save(desc);
                        }
                    }

                    if (auditStatus == ProductAuditStatus.InfractionSaleOff)
                    {
                        RemovedDistributionProduct(productIds);
                    }
                });
        }

        #endregion

        #region 商家服务
        /// <summary>
        /// 是否有规格
        /// </summary>
        /// <param name="id">产品编号</param>
        /// <returns></returns>
        public bool HasSKU(long id)
        {
            return DbFactory.Default
                .Get<SKUInfo>()
                .Where(d => d.ProductId == id && ((d.Color.ExIfNull("") != "") || (d.Version.ExIfNull("") != "") || (d.Size.ExIfNull("") != ""))).Exist();
        }

        public List<ProductAttributeInfo> GetProductAttribute(long productId)
        {
            return DbFactory.Default.Get<ProductAttributeInfo>()
                     .Where(p => p.ProductId == productId && p.ValueId != 0)
                     .ToList();
        }

        public List<SellerSpecificationValueInfo> GetSellerSpecifications(long shopId, long typeId)
        {
            return DbFactory.Default.Get<SellerSpecificationValueInfo>().Where(p => p.ShopId == shopId && p.TypeId == typeId).ToList();
        }

        /// <summary>
        /// 转移外站图片，去除script脚本
        /// </summary>
        /// <param name="content">html内容</param>
        /// <param name="id"></param>
        /// <returns></returns>
        string HTMLProcess(string content, string path)
        {
            var details = Path.Combine(path, "Details").Replace("\\", "/");
            try
            {

                string imageRealtivePath = details;
                content = Core.Helper.HtmlContentHelper.TransferToLocalImage(content, "/", imageRealtivePath, Core.MallIO.GetImagePath(imageRealtivePath) + "/");
                content = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(content);
            }
            catch
            {
            }
            return content;
        }

        public void AddProduct(ProductInfo model, ProductDescriptionInfo description, List<SKUInfo> skus)
        {
            DbFactory.Default
                .InTransaction(() =>
                {
                    model.EditStatus = ProductEditStatus.EditedAndPending;  //初始修改状态
                    DbFactory.Default.Add(model);
                    if (model.ProductAttributeInfo != null && model.ProductAttributeInfo.Count > 0)
                    {
                        model.ProductAttributeInfo.ForEach(p => p.ProductId = model.Id);
                        DbFactory.Default.AddRange(model.ProductAttributeInfo);
                    }
                    if (model.ProductShopCategoryInfo != null && model.ProductShopCategoryInfo.Count > 0)
                    {
                        model.ProductShopCategoryInfo.ForEach(p => p.ProductId = model.Id);
                        DbFactory.Default.AddRange(model.ProductShopCategoryInfo);
                    }
                    if (description == null)
                    {
                        description.ProductId = model.Id;
                        DbFactory.Default.Add(description);
                    }

                    //商品上架
                    if (model.SaleStatus == ProductSaleStatus.OnSale)
                    {
                        ApplyForSale(model.Id);
                    }
                });
        }

        public void AddProduct(long shopId, ProductInfo product, string[] pics, SKUInfo[] skus, ProductDescriptionInfo description, ProductAttributeInfo[] attributes, long[] goodsCategory, SellerSpecificationValueInfo[] sellerSpecifications, ProductLadderPriceInfo[] prices)
        {
            DbFactory.Default
                .InTransaction(() =>
                {
                    //设置商品基本属性
                    product.ProductType = product.ProductType;
                    product.AddedDate = DateTime.Now;
                    if (product.SaleStatus == ProductInfo.ProductSaleStatus.RawState)
                        product.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
                    product.AuditStatus = ProductInfo.ProductAuditStatus.WaitForAuditing;
                    product.DisplaySequence = 0;//默认的序号都为0
                    product.ShopId = shopId;
                    product.HasSKU = skus != null && skus.Length > 0 && (skus.Length > 1 || !string.IsNullOrEmpty(skus[0].Color) || !string.IsNullOrEmpty(skus[0].Size) || !string.IsNullOrEmpty(skus[0].Version));
                    if (product.IsOpenLadder)
                        product.MinSalePrice = prices.Min(p => p.Price);
                    this.AddProduct(product, description, skus.ToList());

                    //转移商品图片
                    if (pics != null)
                    {
                        product.ImagePath = this.ProductImageToStorageAndCreateThumbnail(product.ShopId, product.Id, pics);
                    }
                    product.VideoPath = this.ProductVideoToStorage(product.ShopId, product.Id, product.VideoPath);

                    this.ProcessSKU(shopId, product.Id, skus);

                    //product.SKUInfo = skus.ToList();
                    DbFactory.Default.Add<SKUInfo>(skus);
                    if (description != null)
                    {
                        description.ProductId = product.Id;
                        //处理html

                        description.Description = ProcessHtml(product.ImagePath, description.Description);
                        description.MobileDescription = ProcessHtml(product.ImagePath, description.MobileDescription);

                        //product.ProductDescriptionInfo = description;
                        var ret = DbFactory.Default.Add(description);
                    }

                    if (attributes != null && attributes.Length > 0)
                    {
                        foreach (var item in attributes)
                        {
                            item.ProductId = product.Id;
                            //product.ProductAttributeInfo.Add(item);                            
                        }

                        var ret = DbFactory.Default.Add<Entities.ProductAttributeInfo>(attributes);
                    }

                    if (goodsCategory != null && goodsCategory.Where(p => p > 0).Any())
                    {
                        //product.ProductShopCategoryInfo = goodsCategory.Where(p => p > 0).Select(id => new ProductShopCategoryInfo()
                        //{
                        //    ProductId = product.Id,
                        //    ShopCategoryId = id
                        //}).ToList();
                        var pcategorys = new List<ProductShopCategoryInfo>();
                        goodsCategory
                        .Where(n => n > 0)
                        .ForEach_(n => pcategorys.Add(new ProductShopCategoryInfo
                        {
                            ProductId = product.Id,
                            ShopCategoryId = n
                        }));

                        var ret = DbFactory.Default.Add<ProductShopCategoryInfo>(pcategorys);
                    }

                    //阶梯价
                    if (product.IsOpenLadder)
                    {
                        prices.ForEach_(n => n.ProductId = product.Id);
                        var ret = DbFactory.Default.Add<ProductLadderPriceInfo>(prices);
                    }

                    if (sellerSpecifications != null && sellerSpecifications.Length > 0)
                        this.SaveSellerSpecifications(sellerSpecifications.ToList());

                    DbFactory.Default.Update(product);
                 
                    var _iSearchProductService =  EngineContext.Current.Resolve<ISearchProductService>();


                    _iSearchProductService.AddSearchProduct(product.Id);

                    #region 处理虚拟商品
                    if (product.ProductType == 1)
                    {
                        VirtualProductInfo virtualProductInfo = new VirtualProductInfo()
                        {
                            ProductId = product.Id,
                            ValidityType = product.ValidityType,
                            EffectiveType = (sbyte)product.EffectiveType,
                            Hour = product.Hour,
                            SupportRefundType = (sbyte)product.SupportRefundType,
                            UseNotice = product.UseNotice
                        };
                        if (!string.IsNullOrWhiteSpace(product.StartDate))
                        {
                            virtualProductInfo.StartDate = DateTime.Parse(product.StartDate);
                        }
                        if (!string.IsNullOrWhiteSpace(product.EndDate))
                        {
                            virtualProductInfo.EndDate = DateTime.Parse(product.EndDate);
                        }
                        DbFactory.Default.Add(virtualProductInfo);

                        //批量添加商品信息项
                        if (product.VirtualProductItemInfo != null && product.VirtualProductItemInfo.Count() > 0)
                        {
                            product.VirtualProductItemInfo.ToList().ForEach(a => a.ProductId = product.Id);
                            DbFactory.Default.Add<VirtualProductItemInfo>(product.VirtualProductItemInfo);
                        }
                    }
                    #endregion
                });
        }

        private void UpdateCommon(ProductInfo model, ProductDescription description)
        {
            DbFactory.Default.InTransaction(() =>
            {
                var product = DbFactory.Default.Get<ProductInfo>().Where(p => p.Id == model.Id).FirstOrDefault();
                var productAuditONoff = SiteSettingApplication.SiteSettings.ProdutAuditOnOff;
                if (productAuditONoff == 0 && product.AuditStatus == ProductInfo.ProductAuditStatus.InfractionSaleOff)
                {
                    throw new MallException("违规下架的商品不能执行此操作！");
                }

                product.BrandId = model.BrandId;
                product.CategoryId = model.CategoryId;
                product.CategoryPath = model.CategoryPath;


                product.MarketPrice = model.MarketPrice;
                product.MinSalePrice = model.MinSalePrice;
                product.ProductCode = model.ProductCode;
                product.ProductName = model.ProductName;
                product.ShortDescription = model.ShortDescription;
                product.TypeId = model.TypeId;
                product.FreightTemplateId = model.FreightTemplateId;
                product.Volume = model.Volume;
                product.Weight = model.Weight;
                product.MeasureUnit = model.MeasureUnit;
                product.ImagePath = model.RelativePath;
                product.ColorAlias = model.ColorAlias;
                product.SizeAlias = model.SizeAlias;
                product.VersionAlias = model.VersionAlias;
                //商品信息修改状态
                product.EditStatus = model.EditStatus;
                DbFactory.Default.Update(product);

                var desc = DbFactory.Default.Get<ProductDescriptionInfo>(p => p.ProductId == model.Id).FirstOrDefault();
                desc.Description = HTMLProcess(description.Description, model.RelativePath);//转移外站图片，去除script脚本,防止注入 
                desc.DescriptionPrefixId = description.DescriptionPrefixId;
                desc.DescriptiondSuffixId = description.DescriptiondSuffixId;
                desc.Meta_Description = description.Meta_Description;
                desc.Meta_Keywords = description.Meta_Keywords;
                desc.Meta_Title = description.Meta_Title;
                desc.MobileDescription = HTMLProcess(description.MobileDescription, model.RelativePath);

                DbFactory.Default.Update(desc);

                //商品上架
                if (model.SaleStatus == ProductInfo.ProductSaleStatus.OnSale)
                {
                    ApplyForSale(product.Id);
                }
            });
        }

        /// <summary>
        /// 申请商品上架
        /// </summary>
        /// <param name="id">商品编号</param>
        /// <returns></returns>
        /// Add:DZY[150715]
        public bool ApplyForSale(long id)
        {
            var product = DbFactory.Default.Get<ProductInfo>().Where(p => p.Id == id).FirstOrDefault();

            this.ApplyForSale(product);

            DbFactory.Default.Update(product);

            return true;
        }

        public void ApplyForSale(ProductInfo product)
        {
            bool isCanAuditPass = true;
            //处理修改状态
            if (product.AuditStatus == ProductInfo.ProductAuditStatus.InfractionSaleOff)
            {
                //违规下架 必审
                product.EditStatus = ProductInfo.ProductEditStatus.CompelPendingAudit;
                isCanAuditPass = false;
            }

            if (product.IsDeleted)
            {
                //删除重新上架 必审
                product.EditStatus = ProductInfo.ProductEditStatus.CompelPendingAudit;
                isCanAuditPass = false;
            }

            #region 免审上架处理
            var productAuditONoff = SiteSettingApplication.SiteSettings.ProdutAuditOnOff == 0;  //是否免审核上架

            if (productAuditONoff)
            {
                if (product.AuditStatus == ProductInfo.ProductAuditStatus.InfractionSaleOff)
                {
                    throw new MallException("违规下架的商品不能申请免审核上架！");
                    //return false;   //强制退出
                }
                if (isCanAuditPass)
                {
                    if (product.EditStatus < ProductInfo.ProductEditStatus.CompelPendingAudit)
                    {
                        product.EditStatus = ProductInfo.ProductEditStatus.Normal;   //免审上架 置位修改状态为已生效
                    }
                }
            }
            #endregion

            if (product.SaleStatus != ProductInfo.ProductSaleStatus.InDraft)
            {
                //如果原状态为草稿箱，则不做修改，仍然为草稿箱
                product.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
            }
            if (product.EditStatus == ProductInfo.ProductEditStatus.EditedAndPending
                || product.EditStatus == ProductInfo.ProductEditStatus.PendingAudit
                || product.EditStatus == ProductInfo.ProductEditStatus.CompelPendingAudit
                || product.EditStatus == ProductInfo.ProductEditStatus.CompelPendingHasEdited)
            {
                product.AuditStatus = ProductInfo.ProductAuditStatus.WaitForAuditing;
            }
            else
            {
                product.AuditStatus = ProductInfo.ProductAuditStatus.Audited;
            }
        }

        private void UpdateAttr(ProductInfo model)
        {
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Del<ProductAttributeInfo>(s => s.ProductId == model.Id);
                DbFactory.Default.Add<ProductAttributeInfo>(model.ProductAttributeInfo);
            });
        }

        /// <summary>
        /// 更新商品分类服务
        /// </summary>
        /// <param name="model"></param>
        private void UpdateCategory(ProductInfo model)
        {
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Del<ProductShopCategoryInfo>(s => s.ProductId == model.Id);
                DbFactory.Default.Add<ProductShopCategoryInfo>(model.ProductShopCategoryInfo);
            });
        }
        /// <summary>
        /// 更新商品服务
        /// </summary>
        /// <param name="model">商品实体</param>
        public void UpdateProduct(ProductInfo model)
        {
            DbFactory.Default.InTransaction(() =>
            {
                //更新试商品基本信息
                UpdateCommon(model, null);

                //更新商品SKU
                //UpdateSKUs(model);

                //更新商品属性
                UpdateAttr(model);

                //更新商品分类
                UpdateCategory(model);
            });
        }

        public void UpdateProduct(ProductInfo product, string[] pics, SKUInfo[] skus, ProductDescriptionInfo description, ProductAttributeInfo[] attributes, long[] goodsCategory, SellerSpecificationValueInfo[] sellerSpecifications, ProductLadderPriceInfo[] prices)
        {
            DbFactory.Default.InTransaction(() =>
            {
                product.HasSKU = skus != null && skus.Length > 0 && (skus.Length > 1 || !string.IsNullOrEmpty(skus[0].Color) || !string.IsNullOrEmpty(skus[0].Size) || !string.IsNullOrEmpty(skus[0].Version));
                //转移商品图片
                if (pics != null)
                {
                    if (pics.Any(path => string.IsNullOrWhiteSpace(path) || !path.StartsWith(product.ImagePath)))
                    {
                        this.ProductImageToStorageAndCreateThumbnail(product.ShopId, product.Id, pics);
                    }
                }
                //处理商品视频
                if (!string.IsNullOrWhiteSpace(product.VideoPath))
                {
                    product.VideoPath = this.ProductVideoToStorage(product.ShopId, product.Id, product.VideoPath);
                }

                //处理html
                if (description != null)
                {
                    description.Description = ProcessHtml(product.ImagePath, description.Description);
                    description.MobileDescription = ProcessHtml(product.ImagePath, description.MobileDescription);

                    var descriptionInfo = DbFactory.Default.Get<ProductDescriptionInfo>().Where(p => p.ProductId == product.Id).FirstOrDefault();
                    if (description.Description != descriptionInfo.Description || description.MobileDescription != descriptionInfo.MobileDescription)
                        product.EditStatus = (product.EditStatus > ProductInfo.ProductEditStatus.EditedAndPending ? ProductInfo.ProductEditStatus.CompelPendingHasEdited : ProductInfo.ProductEditStatus.EditedAndPending);

                    descriptionInfo.Description = description.Description;
                    descriptionInfo.MobileDescription = description.MobileDescription;
                    descriptionInfo.DescriptionPrefixId = description.DescriptionPrefixId;
                    descriptionInfo.DescriptiondSuffixId = description.DescriptiondSuffixId;
                    DbFactory.Default.Update(descriptionInfo);
                }

                this.ApplyForSale(product);

                this.ProcessSKU(product.ShopId, product.Id, skus);
                //context.SKUInfo.RemoveRange(context.SKUInfo.Where(p => p.ProductId == product.Id));
                DbFactory.Default
                    .Del<SKUInfo>()
                    .Where(n => n.ProductId == product.Id)
                    .Succeed();
                DbFactory.Default.Add<SKUInfo>(skus);

                //删除阶梯价
                //context.ProductLadderPricesInfo.RemoveRange(
                //        context.ProductLadderPricesInfo.Where(p => p.ProductId == product.Id));
                DbFactory.Default
                    .Del<ProductLadderPriceInfo>()
                    .Where(n => n.ProductId == product.Id)
                    .Succeed();
                //阶梯价
                if (product.IsOpenLadder)
                {
                    product.MinSalePrice = prices.Min(p => p.Price);
                    DbFactory.Default.Add<ProductLadderPriceInfo>(prices);
                }

                //商品属性
                if (attributes != null && attributes.Length > 0)
                {
                    DbFactory.Default.Del<ProductAttributeInfo>().Where(p => p.ProductId == product.Id).Succeed();
                    DbFactory.Default.Add<ProductAttributeInfo>(attributes);
                }

                //商家分类
                if (goodsCategory != null && goodsCategory.Where(p => p > 0).Any())
                {
                    //var temp = goodsCategory.Where(p => p > 0).Select(shopCategoryId => new ProductShopCategoryInfo()
                    //{
                    //    ProductId = product.Id,
                    //    ShopCategoryId = shopCategoryId
                    //}).ToArray();

                    DbFactory.Default.Del<ProductShopCategoryInfo>().Where(p => p.ProductId == product.Id).Succeed();
                    DbFactory.Default
                        .Add<ProductShopCategoryInfo>(goodsCategory.Where(n => n > 0).Select(n => new ProductShopCategoryInfo
                        {
                            ProductId = product.Id,
                            ShopCategoryId = n
                        }));
                }

                //保存商家规格
                if (sellerSpecifications != null && sellerSpecifications.Length > 0)
                    this.SaveSellerSpecifications(sellerSpecifications.ToList());

                DbFactory.Default.Update(product);//保存商品基本信息

                #region 处理虚拟商品
                if (product.ProductType == 1)
                {
                    VirtualProductInfo virtualProductInfo = DbFactory.Default.Get<VirtualProductInfo>().Where(a => a.ProductId == product.Id).FirstOrDefault();
                    if (virtualProductInfo != null)
                    {
                        virtualProductInfo.UseNotice = product.UseNotice;
                        virtualProductInfo.EffectiveType = (sbyte)product.EffectiveType;
                        virtualProductInfo.Hour = product.Hour;
                        DbFactory.Default.Update(virtualProductInfo);
                    }

                    DbFactory.Default.Del<VirtualProductItemInfo>().Where(a => a.ProductId == product.Id).Succeed();
                    //批量添加商品信息项
                    if (product.VirtualProductItemInfo != null && product.VirtualProductItemInfo.Count() > 0)
                    {
                        product.VirtualProductItemInfo.ToList().ForEach(a => a.ProductId = product.Id);
                        DbFactory.Default.Add<VirtualProductItemInfo>(product.VirtualProductItemInfo);
                    }
                }
                #endregion

                #region 处理高价
                var faids = DbFactory.Default.Get<FlashSaleInfo>()
                .Where(d => d.ProductId == product.Id && d.EndDate > DateTime.Now.Date)
                .Select(d => d.Id)
                .ToList<long>();
                var lskdata = DbFactory.Default.Get<FlashSaleDetailInfo>()
                    .Where(d => d.FlashSaleId.ExIn(faids)).ToList();


                var gaids = DbFactory.Default.Get<FightGroupActiveInfo>()
                .Where(d => d.ProductId == product.Id && d.EndTime > DateTime.Now.Date)
                .Select(d => d.Id)
                .ToList<long>();
                var gskdata = DbFactory.Default.Get<FightGroupActiveItemInfo>()
                .Where(d => d.ActiveId.ExIn(gaids))
                .ToList();

                foreach (var item in skus)
                {
                    var clsku = lskdata.FirstOrDefault(d => d.SkuId == item.Id);
                    if (clsku != null && clsku.Price > item.SalePrice)
                    {
                        clsku.Price = item.SalePrice;
                        DbFactory.Default.Update(clsku);
                    }

                    var cgsku = gskdata.FirstOrDefault(d => d.SkuId == item.Id);
                    if (cgsku != null && cgsku.ProductPrice != item.SalePrice)
                    {
                        if (cgsku.ActivePrice > item.SalePrice)
                        {
                            cgsku.ActivePrice = item.SalePrice;
                        }
                        cgsku.ProductPrice = item.SalePrice;
                        DbFactory.Default.Update(cgsku);
                    }
                }

                if (lskdata != null && lskdata.Count > 0)
                {
                    decimal minp = lskdata.Min(s => s.Price);
                    DbFactory.Default.Set<FlashSaleInfo>()
                    .Set(d => d.MinPrice, minp)
                    .Where(d => d.ProductId == product.Id && d.EndDate > DateTime.Now.Date && d.MinPrice > minp)
                    .Succeed();
                }

                //名称
                DbFactory.Default.Set<FightGroupActiveInfo>()
                .Set(d => d.ProductName, product.ProductName)
                .Where(d => d.ProductId == product.Id && d.EndTime > DateTime.Now.Date)
                .Succeed();
                #endregion

                //处理商品名称

                //移除缓存，被动刷新
                string cacheKey = CacheKeyCollection.CACHE_PRODUCT(product.Id);
                if (Cache.Exists(cacheKey))
                    Cache.Remove(cacheKey);
            });
        }

        private string ProductVideoToStorage(long shopId, long productId, string videoPath)
        {
            if (!string.IsNullOrWhiteSpace(videoPath))
            {
                var destFileName = string.Format("/Storage/Shop/{0}/Products/{1}/Video/", shopId, productId);
                var path = videoPath;
                var dest = string.Format("{0}/1{1}", destFileName, videoPath.Substring(videoPath.IndexOf('.')));
                if (string.IsNullOrWhiteSpace(path) || path.Contains(destFileName))
                {
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        try
                        {
                            Core.MallIO.DeleteFile(dest);
                        }
                        catch
                        { }
                    }
                }

                try
                {
                    Mall.Core.MallIO.CopyFile(path, dest, true);
                }
                catch (FileNotFoundException fex)
                {
                    Core.Log.Error("没有找到文件", fex);
                }
                catch (System.Runtime.InteropServices.ExternalException eex)
                {
                    Core.Log.Error("ExternalException异常", eex);
                }
                catch (Exception ex)
                {
                    Core.Log.Error("Exception异常", ex);
                }
                return dest;
            }
            return videoPath;
        }

        private string ProcessHtml(string productImagePath, string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            var descriptionImagePath = productImagePath + "/remark";
            html = Core.Helper.HtmlContentHelper.TransferToLocalImage(html, "/", descriptionImagePath, Core.MallIO.GetImagePath(descriptionImagePath) + "/");
            html = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(html);
            return html;
        }

        public ProductInfo GetProduct(long id)
        {
            return DbFactory.Default.Get<ProductInfo>().Where(n => n.Id == id).FirstOrDefault();
        }
        public List<ProductInfo> GetProducts(List<long> products)
        {

            return DbFactory.Default.Get<ProductInfo>().Where(n => n.Id.ExIn(products)).ToList();
        }



        public Dictionary<string, int> GetCommentsNumber(long product)
        {
            var comments = DbFactory.Default.Get<ProductCommentInfo>().Where(p => p.ProductId == product).Count();
            var consultations = DbFactory.Default.Get<ProductConsultationInfo>().Where(p => p.ProductId == product).Count();
            return new Dictionary<string, int> { { "comments", comments }, { "consultations", consultations } };
        }



        /// <summary>
        /// 获取商品详情页需要及时刷新的信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ProductInfo GetNeedRefreshProductInfo(long id)
        {
            return DbFactory.Default
                .Get<ProductInfo>()
                .Where(n => n.Id == id)
                .Select(n => new { n.MinSalePrice, n.SaleCounts, n.MeasureUnit, n.SaleStatus, n.AuditStatus, n.FreightTemplateId, n.ShopId, n.TypeId, n.ColorAlias, n.SizeAlias, n.VersionAlias, n.VirtualSaleCounts, n.IsOpenLadder, n.ProductType })
                .FirstOrDefault();
        }

        /// <summary>
        /// 获取商品描述
        /// </summary>
        /// <param name="id">商品编号</param>
        /// <returns></returns>
        public ProductDescriptionInfo GetProductDescription(long id)
        {
            var re = DbFactory.Default.Get<ProductDescriptionInfo>().Where(d => d.ProductId == id).FirstOrDefault();
            return re;
        }

        /// <summary>
        /// 获取商品描述
        /// </summary>
        /// <param name="ids">商品编号</param>
        /// <returns></returns>
        public List<ProductDescriptionInfo> GetProductDescriptions(IEnumerable<long> ids)
        {
            return DbFactory.Default.Get<ProductDescriptionInfo>().Where(p => p.ProductId.ExIn(ids)).ToList();
        }

        /// <summary>
        /// 获取商品的评论数
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public int GetProductCommentCount(long productId)
        {
            return DbFactory.Default.Get<ProductCommentInfo>().Where(p => p.ProductId == productId).Count();
        }




        public void SaleOff(long id, long shopid)
        {
            var flag = DbFactory.Default.Set<ProductInfo>().Set(n => n.SaleStatus, ProductInfo.ProductSaleStatus.InStock).Where(n => n.Id == id && n.ShopId == shopid).Succeed();
            if (!flag) throw new MallException("只能下架指定店铺的商品");
            RemovedDistributionProduct(new long[] { id }, shopid);
        }


        public void SaleOff(IEnumerable<long> ids, long shopid)
        {

            var flag = DbFactory.Default
                .Set<ProductInfo>()
                .Set(n => n.SaleStatus, ProductInfo.ProductSaleStatus.InStock)
                .Set(n => n.AuditStatus, ProductInfo.ProductAuditStatus.WaitForAuditing)
                .Where(item => item.IsDeleted == false && item.ShopId == shopid && item.Id.ExIn(ids))
                .Succeed();
            if (!flag) throw new MallException("只能下架指定店铺的商品");
            RemovedDistributionProduct(ids, shopid);
        }


        public void OnSale(long id, long shopId)
        {
            OnSale(new long[] { id }, shopId);
        }

        public void OnSale(IEnumerable<long> ids, long shopId)
        {
            var products = DbFactory.Default.Get<ProductInfo>().Where(item => item.Id.ExIn(ids) && item.IsDeleted == false).ToList();
            if (products.Any(item => item.ShopId != shopId))
                throw new MallException("只能上架指定店铺的商品");

            //检测未为开启免审核上架，提前检测下(因为它在ApplyForSale在事件里检测错误只做了日志没外面没获取提前检测下)
            var productAuditONoff = SiteSettingApplication.SiteSettings.ProdutAuditOnOff == 0;  //是否免审核上架
            if (productAuditONoff)
            {
                var prolist = products.Where(p => p.AuditStatus == ProductInfo.ProductAuditStatus.InfractionSaleOff);
                if (prolist != null && prolist.Count() > 0)
                    throw new MallException("违规下架的商品不能申请免审核上架！");
            }

            DbFactory.Default.InTransaction(() =>
            {
                //标记为上架架状态
                foreach (var item in products)
                {
                    item.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
                    ApplyForSale(item);
                    DbFactory.Default.Update(item);
                }
            });
        }


        public void DeleteProduct(IEnumerable<long> ids, long shopId)
        {

            DbFactory.Default.InTransaction(() =>
            {
                var flag = DbFactory.Default
                    .Set<ProductInfo>()
                    .Set(n => n.IsDeleted, true)
                    .Set(n => n.EditStatus, ProductInfo.ProductEditStatus.CompelPendingAudit)
                    .Where(item => item.ShopId == shopId && item.Id.ExIn(ids) && item.IsDeleted == false)
                    .Succeed();
                if (!flag) throw new MallException("只能下架指定店铺的商品");
                #region
                //清理微店推荐商品
                DbFactory.Default.Del<MobileHomeProductInfo>().Where(d => d.ShopId == shopId && d.ProductId.ExIn(ids)).Succeed();
                //清理专题
                DbFactory.Default.Del<ModuleProductInfo>().Where(d => d.ProductId.ExIn(ids)).Succeed();

                #endregion

                //删除门店商品
                DbFactory.Default.Del<ShopBranchSkuInfo>().Where(n => n.ProductId.ExIn(ids)).Succeed();
                //商品被删除时，使活动提前结束
                var enddate = DateTime.Now.AddDays(-1);
                var fgInfos = DbFactory.Default.Get<FightGroupActiveInfo>().Where(p => p.EndTime > DateTime.Now && p.ProductId.ExIn(ids)).ToList();
                foreach (var fgInfo in fgInfos)
                {
                    fgInfo.EndTime = DateTime.Now.AddDays(-1);
                    fgInfo.ManageAuditStatus = FightGroupManageAuditStatus.SoldOut.GetHashCode();
                    fgInfo.ManageRemark = "商品被删除活动下架";
                    fgInfo.ManageDate = DateTime.Now;
                    fgInfo.ManagerId = 0;
                    DbFactory.Default.Update(fgInfo);
                }
                var cllIds = DbFactory.Default.Get<CollocationPoruductInfo>().Where(p => p.ProductId.ExIn(ids) && p.IsMain == true).Select(p => p.ColloId).Distinct().ToList<long>();
                if (cllIds != null && cllIds.Count() > 0)
                {
                    var edate = DateTime.Now.AddMinutes(-1);
                    DbFactory.Default.Set<CollocationInfo>().Set(p => p.EndTime, edate).Where(p => p.Id.ExIn(cllIds)).Succeed();
                }
                var ltbInfos = DbFactory.Default.Get<FlashSaleInfo>()
                .Where(n => n.Status == FlashSaleInfo.FlashSaleStatus.Ongoing && n.EndDate > DateTime.Now && n.ProductId.ExIn(ids)).ToList();
                foreach (var ltbInfo in ltbInfos)
                {
                    ltbInfo.EndDate = DateTime.Now.AddMinutes(-1);
                    DbFactory.Default.Update(ltbInfo);
                }
                //处理分销商品下架
                int dprsv = DistributionProductStatus.Removed.GetHashCode();
                DbFactory.Default.Set<DistributionProductInfo>()
                    .Set(d => d.ProductStatus, dprsv)
                    .Where(d => d.ProductId.ExIn(ids) && d.ShopId == shopId)
                    .Succeed();
            });
        }

        public List<SKUInfo> GetSKUs(long productId)
        {
            var sku = DbFactory.Default.Get<SKUInfo>().Where(s => s.ProductId == productId).ToList();
            ProductInfo productInfo = DbFactory.Default.Get<ProductInfo>().Where(pi => pi.Id == productId).FirstOrDefault();
            foreach (var item in sku)
            {
                TypeInfo typeInfo = DbFactory.Default.Get<TypeInfo>().InnerJoin<ProductInfo>((ti, pi) => pi.TypeId == pi.Id && pi.Id == item.ProductId).FirstOrDefault();
                item.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                item.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                item.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                if (productInfo != null)
                {
                    item.ColorAlias = !string.IsNullOrWhiteSpace(productInfo.ColorAlias) ? productInfo.ColorAlias : item.ColorAlias;
                    item.SizeAlias = !string.IsNullOrWhiteSpace(productInfo.SizeAlias) ? productInfo.SizeAlias : item.SizeAlias;
                    item.VersionAlias = !string.IsNullOrWhiteSpace(productInfo.VersionAlias) ? productInfo.VersionAlias : item.VersionAlias;
                }
            }
            return sku;
        }

        public List<SKUInfo> GetSkuList(long productId)
        {
            return DbFactory.Default.Get<SKUInfo>().Where(n => n.ProductId == productId).ToList();
        }

        public Entities.SKUInfo GetSku(string skuId)
        {
            return DbFactory.Default.Get<SKUInfo>().Where(s => s.Id == skuId).FirstOrDefault();
        }

        public string GetSkuString(string skuId)
        {
            var sku = DbFactory.Default.Get<SKUInfo>().Where(p => p.Id == skuId).FirstOrDefault();
            if (sku != null)
            {
                TypeInfo typeInfo = DbFactory.Default.Get<TypeInfo>().InnerJoin<ProductInfo>((ti, pi) => pi.TypeId == pi.Id && pi.Id == sku.ProductId).FirstOrDefault();
                ProductInfo productInfo = DbFactory.Default.Get<ProductInfo>().Where(pi => pi.Id == sku.ProductId).FirstOrDefault();
                string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                if (productInfo != null)
                {
                    colorAlias = !string.IsNullOrWhiteSpace(productInfo.ColorAlias) ? productInfo.ColorAlias : colorAlias;
                    sizeAlias = !string.IsNullOrWhiteSpace(productInfo.SizeAlias) ? productInfo.SizeAlias : sizeAlias;
                    versionAlias = !string.IsNullOrWhiteSpace(productInfo.VersionAlias) ? productInfo.VersionAlias : versionAlias;
                }
                string str = "";
                List<string> arrstr = new List<string>();
                if (!string.IsNullOrEmpty(sku.Color))
                    arrstr.Add(" " + colorAlias + "：" + sku.Color);
                if (!string.IsNullOrEmpty(sku.Size))
                    arrstr.Add(" " + sizeAlias + "：" + sku.Size);
                if (!string.IsNullOrEmpty(sku.Version))
                    arrstr.Add(" " + versionAlias + "：" + sku.Version);
                if (arrstr.Count > 0)
                {
                    str = string.Join(";", arrstr.ToArray());
                }
                return str;
            }

            return string.Empty;
        }

        public void SaveSellerSpecifications(List<SellerSpecificationValueInfo> info)
        {
            if (null != info && info.Count() > 0)
            {
                long shopId = info[0].ShopId, typeId = info[0].TypeId;

                DbFactory.Default.InTransaction(() =>
                {
                    var source = DbFactory.Default.Get<SellerSpecificationValueInfo>().Where(s => s.ShopId == shopId && s.TypeId == typeId).ToList();
                    foreach (var item in info)
                    {
                        //修改已有的
                        if (source.Any(s => s.ValueId == item.ValueId))
                        {
                            var sv = source.FirstOrDefault(s => s.ValueId == item.ValueId);
                            sv.Value = item.Value;
                            DbFactory.Default.Update(sv);
                        }
                        //添加
                        else
                        {
                            item.Specification = DbFactory.Default.Get<SpecificationValueInfo>().Where(p => p.Id == item.ValueId).FirstOrDefault().Specification;
                            DbFactory.Default.Add(item);
                        }
                    }
                });
            }
        }


        public List<ProductShopCategoryInfo> GetProductShopCategories(long productId)
        {
            return DbFactory.Default.Get<ProductShopCategoryInfo>().Where(p => p.ProductId == productId).ToList();
        }

        public void BindTemplate(long? topTemplateId, long? bottomTemplateId, IEnumerable<long> productIds)
        {
            var products = DbFactory.Default.Get<ProductInfo>().Where(item => item.Id.ExIn(productIds) && item.IsDeleted == false).ToList();
            var descList = DbFactory.Default.Get<ProductDescriptionInfo>().Where(p => p.ProductId.ExIn(productIds)).ToList();
            foreach (var product in products)
            {
                var desc = descList.FirstOrDefault(p => p.ProductId == product.Id);
                if (desc == null)
                    desc = new ProductDescriptionInfo();
                if (topTemplateId.HasValue)
                    desc.DescriptionPrefixId = topTemplateId.Value;
                if (bottomTemplateId.HasValue)
                    desc.DescriptiondSuffixId = bottomTemplateId.Value;
                DbFactory.Default.Save(desc);
            }
        }

        public List<ProductInfo> GetProductByIds(IEnumerable<long> ids)
        {
            return DbFactory.Default
                .Get<ProductInfo>()
                .Where(item => item.Id.ExIn(ids) &&
                    item.AuditStatus != ProductInfo.ProductAuditStatus.WaitForAuditing && item.AuditStatus != ProductInfo.ProductAuditStatus.InfractionSaleOff && item.IsDeleted == false)
                .ToList();
        }
        public List<ProductInfo> GetAllProductByIds(IEnumerable<long> ids)
        {
            return DbFactory.Default.Get<ProductInfo>().Where(n => n.Id.ExIn(ids)).ToList();
        }

        public List<ProductInfo> GetAllStatusProductByIds(IEnumerable<long> ids)
        {
            return DbFactory.Default.Get<ProductInfo>().Where(item => item.Id.ExIn(ids)).ToList();
        }

        public long GetNextProductId()
        {
            return DbFactory.Default.Get<ProductInfo>().Max<long>(p => p.Id) + 1;
        }

        public int GetShopAllProducts(long shopId)
        {
            return DbFactory.Default.Get<ProductInfo>().Where(item => item.ShopId == shopId && item.IsDeleted == false).Count();
        }

        public int GetShopOnsaleProducts(long shopId)
        {
            return DbFactory.Default
                .Get<ProductInfo>()
                .Where(item => item.ShopId == shopId && item.SaleStatus == ProductInfo.ProductSaleStatus.OnSale &&
                    item.AuditStatus == ProductInfo.ProductAuditStatus.Audited && item.IsDeleted == false)
                .Count();

        }
        #endregion

        #region   前台页面

        public QueryPageModel<ProductInfo> SearchProduct(ProductSearch search)
        {
            var pv = DbFactory.Default
                .Get<ProductVistiInfo>()
                .Where<ProductInfo>((pvi, pi) => pvi.ProductId == pi.Id)
                .GroupBy(n => n.ProductId)
                .Select(n => n.OrderCounts.ExSum());

            var pc = DbFactory.Default
                .Get<ProductCommentInfo>()
                .Where<ProductInfo>((pci, pi) => pci.ProductId == pi.Id)
                .GroupBy(n => n.ProductId)
                .Select(n => n.Id.ExCount(false));

            var result = DbFactory.Default.Get<ProductInfo>()
                         .Where(n => n.IsDeleted == false)
                         .Select(item => new
                         {
                             Id = item.Id,
                             ProductName = item.ProductName,
                             AuditStatus = item.AuditStatus,
                             SaleStatus = item.SaleStatus,
                             ShopId = item.ShopId,
                             CategoryPath = item.CategoryPath,
                             CategoryId = item.CategoryId,
                             BrandId = item.BrandId,
                             MinSalePrice = item.MinSalePrice,
                             SaleCounts = pv.ExResolve<long>(),
                             AddedDate = item.AddedDate,
                             IsOpenLadder = item.IsOpenLadder,
                             CommentCount = pc.ExResolve<int>(),
                             ImagePath = item.ImagePath
                         });

            //shopId
            if (search.shopId != 0)
            {
                result.Where(p => p.ShopId == search.shopId
                    && p.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                    && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale);
            }
            else
            {
                result.Where(p => p.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                    && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale);
            }
            if (search.HasLadderProduct.HasValue && search.HasLadderProduct == false)
            {
                result.Where(d => d.IsOpenLadder == false);
            }


            //Category
            if (search.CategoryId > 0)
            {
                var categoryId = search.CategoryId;
                //下面有一个按店铺分类查询的条件

                result.Where(p => ("|" + p.CategoryPath + "|").
                    Contains("|" + search.CategoryId.ToString() + "|")).OrderBy(s => s.CategoryId);
            }
            //ShopCategory
            if (search.ShopCategoryId.HasValue && search.ShopCategoryId > 0)
            {
                var productIds = DbFactory.Default.Get<ProductShopCategoryInfo>()
                    .LeftJoin<ShopCategoryInfo>((psci, sci) => psci.ShopCategoryId == sci.Id)
                    .Where<ShopCategoryInfo>(item => item.Id == search.ShopCategoryId ||
                        item.ParentCategoryId == search.ShopCategoryId)
                    .Where<ProductInfo>((psci, pi) => psci.ProductId == pi.Id)
                    .Select(item => item.ProductId);

                result.Where(p => p.ExExists(productIds));
            }

            if (search.BrandId > 0)
                result.Where(p => p.BrandId == search.BrandId);

            foreach (var attr in search.AttrIds)
            {
                long attrId = 0;
                long.TryParse(attr.Split('_')[0], out attrId);

                if (attr.Split('_').Length <= 1)
                {
                    continue;
                }
                long attrValueId = 0;
                long.TryParse(attr.Split('_')[1], out attrValueId);

                var pIds = DbFactory.Default
                    .Get<ProductAttributeInfo>()
                    .Where(p => p.AttributeId == attrId && p.ValueId == attrValueId)
                    .Select(p => p.ProductId);
                result.Where(p => p.Id.ExIn(pIds));
            }

            //Keyword
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keywords = search.Keyword.Replace("\t", " ").Replace("　", " ").Split(' ');//分隔多个关键字
                var where = PredicateExtensions.False<ProductInfo>();
                //bool first = true;//第一次查询标志
                foreach (var keyword in keywords)
                {
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        where = where.Or(n => n.ProductName.Contains(keyword));//MYSQL对大小写不敏感
                    }
                }
                result.Where(where);
            }
            if (search.startPrice >= 0 && search.EndPrice > search.startPrice && search.EndPrice != decimal.MaxValue)
            {
                result.Where(r => r.MinSalePrice >= search.startPrice && r.MinSalePrice <= search.EndPrice);
            }

            if (search.StartDate.HasValue)
            {
                //添加日期筛选
                result.Where(item => item.AddedDate >= search.StartDate);
            }

            if (search.EndDate.HasValue)
            {
                //添加日期筛选
                result.Where(item => item.AddedDate <= search.EndDate);
            }

            //Ex_Keyword
            if (!string.IsNullOrWhiteSpace(search.Ex_Keyword))
            {
                result.Where(p => p.ProductName.Contains(search.Ex_Keyword));
            }
            if (search.shopBranchId.HasValue && search.shopBranchId.Value != 0)
            {//过滤门店已选商品
                var pid = DbFactory.Default
                    .Get<ShopBranchSkuInfo>()
                    .Where(e => e.ShopBranchId == search.shopBranchId.Value)
                    .Select(item => item.ProductId)
                    .Distinct()
                    .ToList<long>();
                result = result.Where(e => e.Id.ExNotIn(pid));
            }

            //判断商品所在店铺是否已经过期
            result.InnerJoin<ShopInfo>((pi, si) => pi.ShopId == si.Id && si.EndDate > DateTime.Now && si.ShopStatus != ShopInfo.ShopAuditStatus.Freeze);


            switch (search.OrderKey)
            {
                case 2:
                    if (!search.OrderType)
                        result.OrderByDescending(p => (p.SaleCounts + p.VirtualSaleCounts));
                    else
                        result.OrderBy(p => (p.SaleCounts + p.VirtualSaleCounts));
                    break;
                case 3:
                    if (!search.OrderType)
                        result.OrderByDescending(p => p.MinSalePrice);
                    else
                        result.OrderBy(p => p.MinSalePrice);
                    break;
                case 4:
                    if (!search.OrderType)
                        result.OrderByDescending(p => "CommentCount");
                    else
                        result.OrderBy(p => "CommentCount");
                    break;
                case 5:
                    if (!search.OrderType)
                        result.OrderByDescending(p => p.AddedDate);
                    else
                        result.OrderBy(p => p.AddedDate);
                    break;
                default:
                    if (search.shopId > 0)
                    {
                        if (SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1)
                        {
                            result.OrderByDescending(item => item.ShopDisplaySequence).OrderByDescending(item => item.SaleCounts + item.VirtualSaleCounts).OrderByDescending(item => item.Id);
                        }
                        else
                        {
                            result.OrderByDescending(item => item.ShopDisplaySequence).OrderByDescending(item => item.Id);
                        }
                    }
                    else
                        result.OrderByDescending(item => item.Id);
                    break;
            }
            var rets = result.ToPagedList(search.PageNumber, search.PageSize);
            var shops = ShopApplication.GetShops(rets.Select(p => p.ShopId));
            foreach (var p in rets)
            {
                long freightTemplateId = p.FreightTemplateId;
                var shop = shops.FirstOrDefault(o => o.Id == p.ShopId);
                p.ShopName = shop.ShopName;
                p.Address = "";
                int? regionId = 0;
                if (freightTemplateId != 0)
                {
                    try
                    {
                        regionId = ServiceProvider.Instance<IFreightTemplateService>.Create.GetFreightTemplate(freightTemplateId).SourceAddress;
                        if (regionId.HasValue)
                        {
                            var region = ServiceProvider.Instance<IRegionService>.Create.GetRegion(regionId.Value);
                            if (region != null)
                                p.Address = region.Name;
                        }
                    }
                    catch
                    {
                        p.Address = "";
                        Core.Log.Error(string.Format("获取地区名字出错，参数 freightTemplateId={0},regionId={1}", freightTemplateId, regionId));
                    }
                }
            }

            QueryPageModel<ProductInfo> pageModel = new QueryPageModel<ProductInfo>()
            {
                Total = rets.TotalRecordCount,
                Models = rets
            };
            return pageModel;
        }


        #endregion

        #region 获取店铺热销的前N件商品
        public List<ProductInfo> GetHotSaleProduct(long shopId, int count = 5)
        {
            string CACHE_MANAGER_KEY = CacheKeyCollection.HotSaleProduct(shopId);
            if (Cache.Exists(CACHE_MANAGER_KEY))
            {
                return Cache.Get<List<ProductInfo>>(CACHE_MANAGER_KEY);
            }
            else
            {
                var result = DbFactory.Default.Get<ProductInfo>()
                    .Where(p => p.ShopId.Equals(shopId) && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale &&
                        p.AuditStatus == ProductInfo.ProductAuditStatus.Audited && p.IsDeleted == false);
                var rets = result.OrderByDescending(s => s.SaleCounts + s.VirtualSaleCounts).Take(count).ToList();

                Cache.Insert(CACHE_MANAGER_KEY, rets, DateTime.Now.AddMinutes(5));
                return rets;
            }
        }
        #endregion


        #region 获取店铺最新上架的前N件商品
        public List<ProductInfo> GetNewSaleProduct(long shopId, int count = 5)
        {
            //string CACHE_MANAGER_KEY = CacheKeyCollection.NewSaleProduct(shopId);
            //if (Cache.Exists(CACHE_MANAGER_KEY))
            //{
            //    return (Core.Cache.Get<List<ProductInfo>>(CACHE_MANAGER_KEY));
            //}
            //else
            //{
            var data = DbFactory.Default
                .Get<ProductInfo>()
                .Where(p => p.ShopId.Equals(shopId) && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale &&
                    p.AuditStatus == ProductInfo.ProductAuditStatus.Audited && p.IsDeleted == false)
                .OrderByDescending(p => p.AddedDate)
                .Take(count)
                .ToList();
            //  var result = context.ProductInfo.Where(p => data.Any(d => d.Id.Equals(p.Id)));
            //Cache.Insert(CACHE_MANAGER_KEY, data.ToList(), DateTime.Now.AddMinutes(5));
            return data;
            //}
        }
        #endregion

        #region 获取店铺最受关注的前N件商品
        public List<ProductInfo> GetHotConcernedProduct(long shopId, int count = 5)
        {
            var data = DbFactory.Default
                .Get<FavoriteInfo>()
                .LeftJoin<ProductInfo>((fi, pi) => fi.ProductId == pi.Id)
                .Where<ProductInfo>(p => p.ShopId == shopId && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                    && p.AuditStatus == ProductInfo.ProductAuditStatus.Audited && p.IsDeleted == false)
                .GroupBy(p => p.ProductId)
                .Select<ProductInfo>()
                .Select(p => new { ConcernedCount = p.Id.ExSum() })
                .OrderByDescending(c => "ConcernedCount")
                .Take(count)
                .ToList<ProductInfo>();

            return data;
        }
        #endregion

        #region 获取用户关注的商品
        public QueryPageModel<FavoriteInfo> GetUserConcernProducts(long userId, int pageNo, int pageSize)
        {
            var favorite = DbFactory.Default.Get<FavoriteInfo>().Where(a => a.UserId == userId).OrderByDescending(a => a.Date).ToPagedList(pageNo, pageSize);
            return new QueryPageModel<FavoriteInfo>()
            {
                Models = favorite,
                Total = favorite.TotalRecordCount
            };
        }

        public List<FavoriteInfo> GetUserAllConcern(long userId, int top)
        {
            return DbFactory.Default.Get<FavoriteInfo>().Where(p => p.UserId == userId).Take(top).ToList();
        }
        #endregion

        #region 取消用户关注的商品
        public void CancelConcernProducts(IEnumerable<long> ids, long userId)
        {
            DbFactory.Default.Del<FavoriteInfo>().Where(a => a.UserId == userId && a.Id.ExIn(ids)).Succeed();
        }

        public void DeleteFavorite(long productId, long userId)
        {
            DbFactory.Default.Del<FavoriteInfo>().Where(item => item.UserId == userId && item.ProductId == productId).Succeed();
        }
        #endregion



        #region 我的收藏
        public void AddFavorite(long productId, long userId, out int status)
        {
            var fav = DbFactory.Default.Get<FavoriteInfo>().Where(f => f.ProductId == productId && f.UserId == userId).Exist();
            if (fav)
            {
                status = 1;
            }
            else
            {
                DbFactory.Default.Add(new FavoriteInfo
                {
                    Date = DateTime.Now,
                    ProductId = productId,
                    UserId = userId,
                    Tags = ""
                });
                status = 0;
            }
        }

        public int GetFavoriteCountByUser(long userid)
        {
            return DbFactory.Default.Get<FavoriteInfo>().Where(p => p.UserId == userid).Count();
        }

        public int GetFavoriteCountByProduct(long product)
        {
            return DbFactory.Default.Get<FavoriteInfo>().Where(p => p.ProductId == product).Count();
        }

        public List<FavoriteShopInfo> GetFavoriteShop(long userid, int top)
        {
            return DbFactory.Default.Get<FavoriteShopInfo>()
                .Where(p => p.UserId == userid)
                .OrderByDescending(p => p.Id)
                .Take(top).ToList();
        }

        public int GetFavoriteShopCountByShop(long shop)
        {
            return DbFactory.Default.Get<FavoriteShopInfo>().Where(p => p.ShopId == shop).Count();
        }

        public int GetFavoriteShopCount(long userid)
        {
            return DbFactory.Default.Get<FavoriteShopInfo>().Where(p => p.UserId == userid).Count();
        }
        public bool IsFavorite(long productId, long userId)
        {
            return DbFactory.Default.Get<FavoriteInfo>().Where(f => f.ProductId == productId && f.UserId == userId).Exist();
        }
        #endregion

        #region 更新库存

        public void SetSkuStock(string skuId, long stock, StockOptionType option)
        {
            var sku = DbFactory.Default.Set<SKUInfo>().Where(p => p.Id == skuId);
            switch (option)
            {
                case StockOptionType.Add: sku.Set(p => p.Stock, n => n.Stock + stock); break;
                case StockOptionType.Reduce: sku.Set(p => p.Stock, n => n.Stock - stock); break;
                case StockOptionType.Normal: sku.Set(p => p.Stock, stock); break;
            }
            sku.Succeed();
        }
        public void SetSkuStock(StockOptionType option, Dictionary<string, long> changes)
        {
            foreach (var item in changes)
                SetSkuStock(item.Key, item.Value, option);
        }
        public void SetProductStock(List<long> products, long stock, StockOptionType option)
        {
            var sku = DbFactory.Default.Set<SKUInfo>().Where(p => p.ProductId.ExIn(products));
            switch (option)
            {
                case StockOptionType.Add: sku.Set(p => p.Stock, n => n.Stock + stock); break;
                case StockOptionType.Reduce: sku.Set(p => p.Stock, n => n.Stock - stock); break;
                case StockOptionType.Normal: sku.Set(p => p.Stock, stock); break;
            }
            sku.Succeed();
        }

        #endregion

        #region 添加浏览记录
        public void AddBrowsingProduct(BrowsingHistoryInfo info)
        {
            var model = DbFactory.Default.Get<BrowsingHistoryInfo>().Where(a => a.ProductId == info.ProductId && a.MemberId == info.MemberId).FirstOrDefault();
            if (model == null)
            {
                if (DbFactory.Default.Get<BrowsingHistoryInfo>().Where(a => a.MemberId == info.MemberId).Count() < 20)
                {
                    DbFactory.Default.Add(info);
                }
                else
                {
                    DbFactory.Default.InTransaction(() =>
                    {
                        var remove = DbFactory.Default.Get<BrowsingHistoryInfo>().Where(a => a.MemberId == info.MemberId).OrderBy(a => a.BrowseTime).FirstOrDefault();
                        DbFactory.Default.Del(remove);
                        DbFactory.Default.Add(info);
                    });
                }
            }
            else
            {
                model.BrowseTime = info.BrowseTime;
                DbFactory.Default.Update(model);
            }
        }
        #endregion

        #region 获取用户浏览记录
        public List<BrowsingHistoryInfo> GetBrowsingProducts(long userId)
        {
            return DbFactory.Default.Get<BrowsingHistoryInfo>().Where(p => p.MemberId == userId).ToList();
        }
        #endregion


        public ProductVistiInfo GetProductVistInfo(long pId, List<ProductVistiInfo> pInfo = null)
        {
            var result = new ProductVistiInfo();
            if (pInfo == null)
            {
                var xinfo = DbFactory.Default.Get<ProductVistiInfo>().Where(v => v.ProductId == pId).FirstOrDefault();
                if (xinfo != null)
                {
                    result.ProductId = pId;
                    result.SaleAmounts = xinfo.SaleAmounts;
                    result.SaleCounts = xinfo.SaleCounts;
                    result.Date = DateTime.Now;
                }
            }
            else
            {
                var v = (from pv in pInfo
                         group pv by pv.ProductId into G
                         select new
                         {
                             SaleCount = G.Sum(v1 => v1.SaleCounts),
                             SaleAmounts = G.Sum(v1 => v1.SaleAmounts),
                             ProductId = G.Key
                         }).FirstOrDefault();
                if (null != v && v.ProductId == pId)
                {
                    result.ProductId = pId;
                    result.SaleAmounts = v.SaleAmounts;
                    result.SaleCounts = v.SaleCount;
                    result.Date = DateTime.Now;
                }
            }
            return result;
        }

        #region 获取运费

        public decimal GetFreight(IEnumerable<long> productIds, IEnumerable<int> counts, int cityId, bool isShow = false)
        {
            CheckWhenGetFreight(productIds, counts, cityId);
            decimal freight = 0;

            IFreightTemplateService ftservice = ServiceProvider.Instance<IFreightTemplateService>.Create;
            IRegionService regionService = ServiceProvider.Instance<IRegionService>.Create;
            int i = 0;
            var pros = GetProductByIds(productIds);
            var products = productIds.Select(productId =>
            {
                var product = pros.FirstOrDefault(p => p.Id == productId);
                return new
                {
                    Product = product,
                    Quantity = counts.ElementAt(i++),
                };
            }).ToList();
            products = products.Where(p => p.Product != null).ToList();
            if (products == null || products.Count() <= 0)
                return freight;
            var freightProductGroup = products.GroupBy(item => item.Product.FreightTemplateId);//根据运费模版将产品分组

            foreach (var freightProduct in freightProductGroup)
            {
                var template = ftservice.GetFreightTemplate(freightProduct.Key);
                var freightAreas = GetAreaContent(template.Id);
                var freightAreaDetail = ftservice.GetFreightAreaDetail(freightProduct.Key);
                if (template != null)
                {
                    if (template.IsFree == FreightTemplateType.SelfDefine)//是否包邮
                    {
                        Mall.Entities.FreightAreaContentInfo freightAreaContent = null;
                        Mall.Entities.FreightAreaDetailInfo detail = null;
                        Region region = (cityId <= 0) ? null : (regionService.GetRegion(cityId));
                        if (region != null && region.Id > 0)
                        {
                            //从最后一级乡镇查找
                            if (region.Level == Region.RegionLevel.Town)//如果传过来ID是区级的话
                            {
                                detail = freightAreaDetail.Where(a => a.TownIds != "" && a.TownIds != null && a.TownIds.Split(',').Contains(cityId.ToString())).FirstOrDefault();
                            }
                            var pid = regionService.GetRegion(region.Id, Region.RegionLevel.Province).Id;//省级
                            var cid = regionService.GetRegion(region.Id, Region.RegionLevel.City).Id;//市级          

                            if (detail == null)
                            {
                                if (region.Level == Region.RegionLevel.County || region.Level == Region.RegionLevel.Town)
                                {
                                    var countyId = regionService.GetRegion(region.Id, Region.RegionLevel.County).Id;//区级
                                    detail = freightAreaDetail.Where(a => a.CountyId == countyId && (a.TownIds == "" || a.TownIds == null)).FirstOrDefault();
                                }
                            }
                            if (detail == null)
                            {
                                detail = freightAreaDetail.Where(a => a.CityId == cid && a.CountyId == 0).FirstOrDefault();
                            }
                            if (detail == null)
                            {
                                detail = freightAreaDetail.Where(a => a.ProvinceId == pid && (a.CityId == cid || a.CityId == 0)).FirstOrDefault();
                            }
                            if (detail != null)
                            {
                                freightAreaContent = freightAreas.Where(a => a.Id == detail.FreightAreaId).FirstOrDefault();
                            }
                        }
                        if (freightAreaContent == null)
                        {
                            //配送地址不包含当前城市则使用默认运费规则
                            freightAreaContent = freightAreas.Where(item => item.IsDefault == 1).FirstOrDefault();
                        }

                        if (template.ValuationMethod == ValuationMethodType.Weight)//按重量
                        {
                            decimal weight = freightProduct.Sum(item => item.Product.Weight != null ? ((decimal)item.Product.Weight * item.Quantity) : 0); //总重量
                            freight += GetFreight2(weight, (int)freightAreaContent.FirstUnit, (decimal)freightAreaContent.FirstUnitMonry, (int)freightAreaContent.AccumulationUnit, (decimal)freightAreaContent.AccumulationUnitMoney);
                        }
                        else if (template.ValuationMethod == ValuationMethodType.Bulk)//按体积
                        {
                            decimal volume = freightProduct.Sum(item => item.Product.Volume != null ? ((decimal)item.Product.Volume * item.Quantity) : 0);//总体积
                            freight += GetFreight2(volume, (int)freightAreaContent.FirstUnit, (decimal)freightAreaContent.FirstUnitMonry, (int)freightAreaContent.AccumulationUnit, (decimal)freightAreaContent.AccumulationUnitMoney);
                        }
                        else //按数量
                        {
                            int count = freightProduct.Sum(item => item.Quantity);//总数量
                            freight += GetFreight2(count, (int)freightAreaContent.FirstUnit, (decimal)freightAreaContent.FirstUnitMonry, (int)freightAreaContent.AccumulationUnit, (decimal)freightAreaContent.AccumulationUnitMoney);
                        }
                    }
                    else
                    {
                        if (isShow)
                        {
                            freight = -1;   //用来商品详情页显示，-1表示卖家承担运费，用来区分0元运费的情况
                        }
                    }
                }
            }

            return freight;
        }


        public List<FreightAreaContentInfo> GetAreaContent(long template)
        {
            return DbFactory.Default.Get<FreightAreaContentInfo>(p => p.FreightTemplateId == template).ToList();
        }
        /// <summary>
        /// 是否指定地区包邮
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="discount">会员折扣率</param>
        /// <returns></returns>
        public bool IsFreeRegion(long productId, decimal discount, int streetId, int count, string skuId)
        {
            bool isFree = false;
            if (streetId <= 0)
                return isFree;
            decimal price = 0;
            ProductInfo productInfo = GetNeedRefreshProductInfo(productId);
            if (productInfo != null)
            {
                var shopInfo = ServiceProvider.Instance<IShopService>.Create.GetShop(productInfo.ShopId);
                bool isSelf = shopInfo != null && shopInfo.IsSelf;
                if (!productInfo.IsOpenLadder)//没有开启阶梯价
                {
                    var skus = GetSkuList(productId).Where(s => s.Stock >= 0);//获取有效SKU
                    if (skus != null && skus.Count() > 0)
                    {
                        var skuInfo = skus.FirstOrDefault(p => p.Id == skuId);
                        if (skuInfo == null)
                        {
                            skuInfo = skus.FirstOrDefault();//取第一个规格
                        }
                        if (skuInfo != null)
                        {
                            if (isSelf)
                            {
                                price = skuInfo.SalePrice * discount;
                            }
                            else
                            {
                                price = skuInfo.SalePrice;
                            }
                        }
                    }
                    else//没有有效规格则取商品商城价
                    {
                        if (isSelf)
                        {
                            price = productInfo.MinSalePrice * discount;
                        }
                        else
                        {
                            price = productInfo.MinSalePrice;
                        }
                    }
                }
                else
                {
                    var priceInfo = ServiceProvider.Instance<IProductLadderPriceService>.Create.GetLadderPricesByProductIds(productId);
                    if (priceInfo != null && priceInfo.Count > 0)
                    {
                        var minInfo = priceInfo.FirstOrDefault(p => count >= p.MinBath);
                        if (minInfo != null)
                        {
                            if (isSelf)
                            {
                                price = minInfo.Price * discount;
                            }
                            else
                            {
                                price = minInfo.Price;
                            }
                        }
                    }
                }
                #region 指定地区包邮
                IFreightTemplateService ftservice = ServiceProvider.Instance<IFreightTemplateService>.Create;
                IRegionService regionService = ServiceProvider.Instance<IRegionService>.Create;
                var freeRegions = ftservice.GetShippingFreeRegions(productInfo.FreightTemplateId);
                freeRegions.ForEach(p =>
                {
                    p.RegionSubList = regionService.GetSubsNew(p.RegionId, true).Select(a => a.Id).ToList();
                });
                var regions = freeRegions.Where(p => p.RegionSubList.Contains(streetId));//根据模板设置的包邮地区过滤出当前配送地址所在地址
                if (regions != null && regions.Count() > 0)
                {
                    var groupIds = regions.Select(p => p.GroupId).ToList();
                    var freeGroups = ftservice.GetShippingFreeGroupInfos(productInfo.FreightTemplateId, groupIds);

                    //只要有一个符合包邮，则退出
                    decimal amount = price * count;
                    freeGroups.ForEach(p =>
                    {
                        if (p.ConditionType == 1)//购买件数
                        {
                            if (count >= int.Parse(p.ConditionNumber))
                            {
                                isFree = true;
                                return;
                            }
                        }
                        else if (p.ConditionType == 2)//金额
                        {
                            if (amount >= decimal.Parse(p.ConditionNumber))
                            {
                                isFree = true;
                                return;
                            }
                        }
                        else if (p.ConditionType == 3)//件数+金额
                        {
                            var condition1 = int.Parse(p.ConditionNumber.Split('$')[0]);
                            var condition2 = decimal.Parse(p.ConditionNumber.Split('$')[1]);
                            if (count >= condition1 && amount >= condition2)
                            {
                                isFree = true;
                                return;
                            }
                        }
                    });
                }
                #endregion
            }
            return isFree;
        }

        /// <summary>
        /// 获取运费（超出部分 不足1个按1个算）
        /// </summary>
        /// <param name="count">总数量/重量/体积</param>
        /// <param name="firstUnit">首件/重/体积</param>
        /// <param name="firstUnitMonry">首费</param>
        /// <param name="accumulationUnit">续件/重/体积</param>
        /// <param name="accumulationUnitMoney">续费</param>
        /// <returns></returns>
        decimal GetFreight2(decimal count, int firstUnit, decimal firstUnitMonry, int accumulationUnit, decimal accumulationUnitMoney)
        {
            decimal freight = 0;
            if (count <= firstUnit)
            {
                freight = firstUnitMonry;
            }
            else
            {
                decimal size = (count - firstUnit) / accumulationUnit;  //续件个数
                decimal prefix = Math.Truncate(size); //续件个数整数部分
                decimal suffix = size - prefix; //续件个数小数部分
                decimal p1 = prefix * accumulationUnitMoney; //续件个数整数部分金额
                decimal p2 = 0;
                if (suffix > 0)
                    p2 = 1 * accumulationUnitMoney; //续件个数小数部分金额 按超过1个算

                freight = firstUnitMonry + p1 + p2;
            }
            return freight;
        }


        void CheckWhenGetFreight(IEnumerable<long> productIds, IEnumerable<int> counts, int cityId)
        {
            if (productIds == null || productIds.Count() == 0)
                throw new InvalidPropertyException("待计算运费的商品不能为空");
            if (counts == null || counts.Count() == 0)
                throw new InvalidPropertyException("待计算运费的商品数量不能为空");
            if (counts.Count(item => item <= 0) > 0)
                throw new InvalidPropertyException("待计算运费的商品数量必须都大于0");
            if (productIds.Count() != counts.Count())
                throw new InvalidPropertyException("商品数量不一致");
        }
        #endregion


        public List<ProductInfo> GetPlatHotSaleProductByNearShop(int count, long userId, bool isRecommend = false)
        {
            List<ProductInfo> result;
            var order = DbFactory.Default.Get<OrderInfo>().Where(c => c.UserId == userId).OrderByDescending(c => c.Id).FirstOrDefault();
            if (order == null)
            {
                if (isRecommend)
                {
                    result = DbFactory.Default.Get<ProductInfo>().Where(c => c.IsDeleted == false && c.SaleStatus == ProductSaleStatus.OnSale).OrderByDescending(c => c.SaleCounts + c.VirtualSaleCounts).Take(count).ToList();
                }
                else
                {
                    result = DbFactory.Default.Get<ProductInfo>().Where(c => c.Id == -1 && c.SaleStatus == ProductSaleStatus.OnSale).ToList();
                }
                return result;
            }
            result = DbFactory.Default
                .Get<ProductInfo>()
                .Where(c => c.ShopId == order.ShopId && c.IsDeleted == false && c.SaleStatus == ProductSaleStatus.OnSale)
                .OrderByDescending(c => c.SaleCounts + c.VirtualSaleCounts)
                .Take(count)
                .ToList();
            return result;
        }

        public List<SKUInfo> GetSKUs(IEnumerable<long> productIds)
        {
            return DbFactory.Default.Get<SKUInfo>().Where(s => s.ProductId.ExIn(productIds)).ToList();
        }

        /// <summary>
        /// 根据sku id 获取sku信息
        /// </summary>
        /// <param name="skuIds"></param>
        /// <returns></returns>
        public List<SKUInfo> GetSKUs(IEnumerable<string> skuIds)
        {
            List<Entities.SKUInfo> list = new List<Entities.SKUInfo>();

            if (skuIds.Any())
            {
                list = DbFactory.Default.Get<SKUInfo>().Where(n => n.Id.ExIn(skuIds)).ToList();
            }
            return list;
        }

        /// <summary>
        /// 是否为限时购商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsLimitBuy(long id)
        {
            bool result = false;
            var now = DateTime.Now;
            result = DbFactory.Default
                .Get<FlashSaleInfo>()
                .Where(m => m.ProductId == id && m.Status == FlashSaleInfo.FlashSaleStatus.Ongoing &&
                    m.BeginDate <= now && m.EndDate > now)
                .Exist();
            return result;
        }

        /// <summary>
        /// 修改推荐商品
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="relationProductIds"></param>
        public void UpdateRelationProduct(long productId, string relationProductIds)
        {
            if (string.IsNullOrEmpty(relationProductIds))
                relationProductIds = "";

            var data = DbFactory.Default.Get<ProductRelationProductInfo>().Where(p => p.ProductId == productId).FirstOrDefault();

            if (data == null)
            {
                DbFactory.Default.Add(new ProductRelationProductInfo
                {
                    ProductId = productId,
                    Relation = relationProductIds
                });
            }
            else
            {
                data.Relation = relationProductIds;
                DbFactory.Default.Update(data);
            }
        }

        /// <summary>
        /// 获取商品的推荐商品
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public ProductRelationProductInfo GetRelationProductByProductId(long productId)
        {
            return DbFactory.Default.Get<ProductRelationProductInfo>().Where(p => p.ProductId == productId).FirstOrDefault();
        }

        /// <summary>
        /// 获取商品所有状态的推荐商品
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public List<ProductRelationProductInfo> GetRelationProductByProductIds(IEnumerable<long> productIds)
        {
            return DbFactory.Default.Get<ProductRelationProductInfo>().Where(p => p.ProductId.ExIn(productIds)).ToList();
        }

        /// <summary>
        /// 获取指定类型下面热销的前N件商品
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<ProductInfo> GetHotSaleProductByCategoryId(int categoryId, int count)
        {
            return DbFactory.Default
                .Get<ProductInfo>()
                .Where(p => p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && p.AuditStatus == ProductInfo.ProductAuditStatus.Audited &&
                    p.CategoryId == categoryId && p.IsDeleted == false)
                .OrderByDescending(p => p.SaleCounts + p.VirtualSaleCounts)
                .Take(count)
                .ToList();
        }

        #region 私有方法
        /// <summary>
        /// 设置sku id，转移sku图片等
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="productId"></param>
        /// <param name="skus"></param>
        private void ProcessSKU(long shopId, long productId, SKUInfo[] skus)
        {
            var skuInfoShowPics = new Dictionary<string, string>();
            foreach (var item in skus)
            {
                item.Id = string.Format(item.Id, productId);
                item.AutoId = item.Id.GetHashCode();
                item.ProductId = productId;
                if (!string.IsNullOrWhiteSpace(item.ShowPic))
                {
                    if (!skuInfoShowPics.ContainsKey(item.ShowPic))
                    {
                        var temp = this.SKUImageToStorage(shopId, productId, item.Id, item.ShowPic);
                        skuInfoShowPics.Add(item.ShowPic, temp);//处理重复图片路径
                        item.ShowPic = temp;
                    }
                    else
                        item.ShowPic = skuInfoShowPics[item.ShowPic];
                }
            }
        }

        private string ProductImageToStorageAndCreateThumbnail(long shopId, long productId, string[] paths)
        {
            if (paths == null || paths.Length == 0)
                throw new ArgumentNullException("paths");
            //paths = paths.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
            var destFileName = string.Format("/Storage/Shop/{0}/Products/{1}", shopId, productId);


            var index = 0;
            for (int i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                if (!string.IsNullOrWhiteSpace(path)) index++;
                else continue;

                var dest = string.Format("{0}/{1}.png", destFileName, index);
                if (path.Contains(dest)) continue;

                if (path.IndexOf('?') > 0) path = path.Substring(0, path.IndexOf('?'));
                try
                {
                    MallIO.CopyFile(path, dest, true);
                    var imageSizes = EnumHelper.ToDictionary<ImageSize>().Select(t => t.Key);

                    foreach (var imageSize in imageSizes)
                    {
                        string size = string.Format("{0}/{1}_{2}.png", destFileName, index, imageSize);
                        MallIO.CreateThumbnail(dest, size, imageSize, imageSize);
                    }
                }
                catch (FileNotFoundException fex)
                {
                    Core.Log.Error("没有找到文件", fex);
                }
                catch (System.Runtime.InteropServices.ExternalException eex)
                {
                    Core.Log.Error("ExternalException异常", eex);
                }
                catch (Exception ex)
                {
                    Core.Log.Error("Exception异常", ex);
                }
            }
            //BUG:39426 删除多余图片
            for (var i = paths.Where(p => !string.IsNullOrWhiteSpace(p)).Count(); i < 5; i++)
            {
                var file = string.Format("{0}/{1}.png", destFileName, i + 1);
                MallIO.DeleteFile(file);
            }
            return destFileName;
        }

        private string SKUImageToStorage(long shopId, long productId, string skuId, string path)
        {
            if (string.IsNullOrWhiteSpace(skuId))
                throw new ArgumentNullException("skuId");

            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            var destFileName = string.Format("/Storage/Shop/{0}/Products/{1}/skus/{2}.png", shopId, productId, skuId);

            if (path.EndsWith(destFileName))
                return destFileName;

            try
            {
                Core.MallIO.CopyFile(path, destFileName, true);
            }
            catch (Exception e)
            {
                Core.Log.Error("Exception异常", e);
            }

            return destFileName;
        }
        #endregion

        #region IProductService 成员
        /// <summary>
        /// 获取商品销量
        /// </summary>
        /// <param name="products">商品集合</param>
        /// <returns>Key:ProductID,Value:SaleCounts</returns>
        public Dictionary<long, long> GetProductSaleCounts(List<long> products)
        {
            //TODO:FG 无引用方法
            return DbFactory.Default.Get<ProductVistiInfo>()
                .Where(p => p.ProductId.ExIn(products))
                .GroupBy(p => p.ProductId)
                .Select(p => new { p.ProductId, SaleCounts = p.SaleCounts.ExSum() })
                .ToList<dynamic>()
                .ToDictionary(key => (long)key.ProductId, value => (long)value.SaleCounts);
        }

        public void SetProductOverSafeStock(IEnumerable<long> pids, long stock)
        {
            //var skus = DbFactory.Default.Get<SKUInfo>().Where(e => e.ProductId.ExIn(pids)).ToList();
            //DbFactory.Default.InTransaction(() =>
            //{
            //    foreach (var sku in skus)
            //    {
            //        sku.SafeStock = stock;
            //        DbFactory.Default.Update(sku);
            //    }
            //});
            var flag = DbFactory.Default.Set<SKUInfo>()
                .Set(n => n.SafeStock, stock).Where(e => e.ProductId.ExIn(pids)).Succeed();
        }

        #endregion



        #region 批量设置商品运费模板、库存、价格、虚拟销量
        public bool BatchSettingFreightTemplate(IEnumerable<long> pids, long freightTemplateId)
        {
            var flag = DbFactory.Default.Set<ProductInfo>()
                .Set(n => n.FreightTemplateId, freightTemplateId).Where(e => e.Id.ExIn(pids)).Succeed();

            return flag;
        }
        public bool BatchSettingStock(Dictionary<long, long> dics)
        {
            bool result = true;
            DbFactory.Default.InTransaction(() =>
            {
                foreach (var item in dics)
                {
                    var flag = DbFactory.Default.Set<SKUInfo>()
                   .Set(n => n.Stock, item.Value).Where(e => e.AutoId == item.Key).Succeed();
                    result = result && flag;
                }
            });
            return result;
        }
        public bool BatchSettingPrice(Dictionary<long, decimal> productDics, Dictionary<long, string> priceDics)
        {
            bool result = true;
            DbFactory.Default.InTransaction(() =>
            {
                //更新商品市场价
                foreach (var item in productDics)
                {
                    var flag = DbFactory.Default.Set<ProductInfo>()
                   .Set(n => n.MarketPrice, item.Value).Where(e => e.Id == item.Key).Succeed();
                    result = result && flag;
                }
                //更新商品市场价

                Dictionary<long, List<decimal>> dics = new Dictionary<long, List<decimal>>();
                foreach (var item in priceDics)
                {
                    List<decimal> list = new List<decimal>();
                    var salePrice = decimal.Parse(item.Value.Split('@')[0]);
                    var hasSku = int.Parse(item.Value.Split('@')[1]);
                    var productId = long.Parse(item.Value.Split('@')[2]);
                    if (hasSku == 0)//如果没有开启规格，则商城价更新商品本身，更新SKU表中的商城价
                    {
                        var flag = DbFactory.Default.Set<ProductInfo>()
                       .Set(n => n.MinSalePrice, salePrice).Where(e => e.Id == productId).Succeed();
                        result = result && flag;

                        //更新商品搜索表
                        flag = DbFactory.Default.Set<SearchProductInfo>()
                        .Set(a => a.SalePrice, salePrice).Where(e => e.ProductId == productId).Succeed();
                        result = result && flag;

                        flag = DbFactory.Default.Set<SKUInfo>()
                      .Set(n => n.SalePrice, salePrice).Where(e => e.ProductId == productId).Succeed();
                        result = result && flag;
                    }
                    else
                    {
                        var flag = DbFactory.Default.Set<SKUInfo>()
                       .Set(n => n.SalePrice, salePrice).Where(e => e.AutoId == item.Key).Succeed();
                        result = result && flag;

                        if (dics.ContainsKey(productId))
                        {
                            list = dics[productId];
                            list.Add(salePrice);
                            dics[productId] = list;
                        }
                        else
                        {
                            list.Add(salePrice);
                            dics.Add(productId, list);
                        }
                    }
                }
                foreach (var item in dics)
                {
                    DbFactory.Default.Set<ProductInfo>()
                    .Set(n => n.MinSalePrice, item.Value.Min()).Where(e => e.Id == item.Key).Succeed();
                    //更新商品搜索表
                    DbFactory.Default.Set<SearchProductInfo>()
                    .Set(a => a.SalePrice, item.Value.Min()).Where(e => e.ProductId == item.Key).Succeed();
                }
            });
            return result;
        }

        public bool UpdateShopDisplaySequence(long id, int order)
        {
            var flag = DbFactory.Default.Set<ProductInfo>()
              .Set(n => n.ShopDisplaySequence, order).Where(e => e.Id == id).Succeed();

            return flag;
        }
        public bool UpdateDisplaySequence(long id, int order)
        {
            var flag = DbFactory.Default.Set<ProductInfo>()
              .Set(n => n.DisplaySequence, order).Where(e => e.Id == id).Succeed();

            return flag;
        }

        public bool BtachUpdateSaleCount(List<long> productIds, long virtualSaleCounts, int minSaleCount = 0, int maxSaleCount = 0)
        {
            bool flag = false;
            if (minSaleCount > 0 && maxSaleCount > 0)
            {
                flag = true;
                var random = new Random();
                foreach (var item in productIds)
                {
                    var vsc = random.Next(minSaleCount, maxSaleCount);
                    flag = flag && DbFactory.Default.Set<ProductInfo>()
                    .Set(n => n.VirtualSaleCounts, vsc).Where(e => e.Id == item).Succeed();
                }
            }
            else
            {
                flag = DbFactory.Default.Set<ProductInfo>()
                    .Set(n => n.VirtualSaleCounts, virtualSaleCounts).Where(e => e.Id.ExIn(productIds)).Succeed();
            }
            return flag;
        }
        #endregion

        /// <summary>
        /// 当前参加的活动
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public string CurrentJoinActive(long productId)
        {
            string result = "";
            DateTime curtime = DateTime.Now;
            if (DbFactory.Default.Get<FightGroupActiveInfo>().Where(d => d.ProductId == productId && d.EndTime >= curtime).Exist())
            {
                result = "拼团";
            }
            if (DbFactory.Default.Get<FlashSaleInfo>().Where(p => p.ProductId == productId
                && p.EndDate > curtime).Exist())
            {
                result = "限时购";
            }
            if (DbFactory.Default
                    .Get<CollocationPoruductInfo>()
                    .InnerJoin<CollocationInfo>((cpi, ci) => cpi.ColloId == ci.Id)
                    .Where(a => a.ProductId == productId)
                    .Where<CollocationInfo>(a => a.EndTime > curtime)
                .Exist())
            {
                result = "组合购";
            }
            return result;
        }

        public void DeleteImportProduct(List<long> productIds)
        {
            DbFactory.Default
                .InTransaction(() =>
                {
                    DbFactory.Default.Del<ProductInfo>(item => item.Id.ExIn(productIds));
                    DbFactory.Default.Del<SKUInfo>(item => item.ProductId.ExIn(productIds));
                    DbFactory.Default.Del<ProductDescriptionInfo>(item => item.ProductId.ExIn(productIds));
                });

        }

        /// <summary>
        /// 下架分销商品
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="shopId"></param>
        private void RemovedDistributionProduct(IEnumerable<long> productIds, long? shopId = null)
        {
            //处理分销商品下架
            int dprsv = DistributionProductStatus.Removed.GetHashCode();
            var sql = DbFactory.Default.Set<DistributionProductInfo>()
                .Set(d => d.ProductStatus, dprsv)
                .Where(d => d.ProductId.ExIn(productIds));
            if (shopId.HasValue)
            {
                sql = sql.Where(d => d.ShopId == shopId);
            }
            sql.Succeed();
        }

        #region 虚拟商品
        public VirtualProductInfo GetVirtualProductInfoByProductId(long productId)
        {
            return DbFactory.Default.Get<VirtualProductInfo>().Where(n => n.ProductId == productId).FirstOrDefault();
        }
        public List<VirtualProductInfo> GetVirtualProductInfoByProductIds(List<long> productIds)
        {
            return DbFactory.Default.Get<VirtualProductInfo>().Where(n => n.ProductId.ExIn(productIds)).ToList();
        }

        public List<VirtualProductItemInfo> GetVirtualProductItemInfoByProductId(long productId)
        {
            return DbFactory.Default.Get<VirtualProductItemInfo>().Where(n => n.ProductId == productId).ToList();
        }

        #endregion

    }
}
