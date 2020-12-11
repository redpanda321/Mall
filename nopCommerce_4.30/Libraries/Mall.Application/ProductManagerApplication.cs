using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.DTO;
using Mall.DTO.Product;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Mall.Application
{
    public class ProductManagerApplication : BaseApplicaion<IProductService>
    {
    //    private static IProductDescriptionTemplateService _productDescriptionTemplateService =  EngineContext.Current.Resolve<IProductDescriptionTemplateService>();
    //    private static ISearchProductService _searchProductService =  EngineContext.Current.Resolve<ISearchProductService>();
   //     private static IProductLadderPriceService _productLadderPriceService =  EngineContext.Current.Resolve<IProductLadderPriceService>();
    //    private static IFightGroupService _iFightGroupService =  EngineContext.Current.Resolve<IFightGroupService>();
 //       private static ILimitTimeBuyService _iLimitTimeBuyService =  EngineContext.Current.Resolve<ILimitTimeBuyService>();
  //      private static ICollocationService _iCollocationService =  EngineContext.Current.Resolve<ICollocationService>();



        private static IProductDescriptionTemplateService _productDescriptionTemplateService =  EngineContext.Current.Resolve<IProductDescriptionTemplateService>();
        private static ISearchProductService _searchProductService =  EngineContext.Current.Resolve<ISearchProductService>();
        private static IProductLadderPriceService _productLadderPriceService =  EngineContext.Current.Resolve<IProductLadderPriceService>();
        private static IFightGroupService _iFightGroupService =  EngineContext.Current.Resolve<IFightGroupService>();
        private static ILimitTimeBuyService _iLimitTimeBuyService =  EngineContext.Current.Resolve<ILimitTimeBuyService>();
        private static ICollocationService _iCollocationService =  EngineContext.Current.Resolve<ICollocationService>();


        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="product">商品信息</param>
        /// <param name="pics">需要转移的商品图片地址</param>
        /// <param name="skus">skus，至少要有一项</param>
        /// <param name="description">描述</param>
        /// <param name="attributes">商品属性</param>
        /// <param name="goodsCategory">商家分类</param>
        /// <param name="sellerSpecifications">商家自定义规格</param>
        public static Product AddProduct(long shopId, Product product, string[] pics, SKU[] skus, ProductDescription description, ProductAttribute[] attributes, long[] goodsCategory, SellerSpecificationValue[] sellerSpecifications, ProductLadderPrice[] prices)
        {
            var productInfo = product.Map<Entities.ProductInfo>();
            var skuInofs = skus.Map<Entities.SKUInfo[]>();
            var descriptionInfo = description.Map<Entities.ProductDescriptionInfo>();
            var attributeInfos = attributes.Map<Entities.ProductAttributeInfo[]>();
            var sellerSpecificationInfos = sellerSpecifications.Map<Entities.SellerSpecificationValueInfo[]>();
            var ladderpricesInfos = prices.Select(p =>
            {
                var ladder = new Entities.ProductLadderPriceInfo();
                ladder.Id = p.Id;
                ladder.MinBath = p.MinBath;
                ladder.MaxBath = p.MaxBath;
                ladder.ProductId = p.ProductId;
                ladder.Price = p.Price;
                return ladder;
            }).ToArray();
            Service.AddProduct(shopId, productInfo, pics, skuInofs, descriptionInfo, attributeInfos,
                goodsCategory, sellerSpecificationInfos, ladderpricesInfos);
            CreateHtml(productInfo.Id);
            //    return AutoMapper.Mapper.Map<Product>(productInfo);

            return productInfo.Map<Product>();
        }

        /// <summary>
        /// 更新商品
        /// </summary>
        /// <param name="product">修改后的商品</param>
        /// <param name="pics">需要转移的商品图片地址</param>
        /// <param name="skus">skus，至少要有一项</param>
        /// <param name="description">描述</param>
        /// <param name="attributes">商品属性</param>
        /// <param name="goodsCategory">商家分类</param>
        /// <param name="sellerSpecifications">商家自定义规格</param>
        public static void UpdateProduct(Product product, string[] pics, SKU[] skus, ProductDescription description, ProductAttribute[] attributes, long[] goodsCategory, SellerSpecificationValue[] sellerSpecifications, ProductLadderPrice[] prices)
        {
            var productInfo = Service.GetProduct(product.Id);
            if (productInfo == null)
                throw new MallException("指定id对应的数据不存在");

            var editStatus = productInfo.EditStatus;

            if (product.ProductName != productInfo.ProductName)
                editStatus = GetEditStatus(editStatus);
            if (product.ShortDescription != productInfo.ShortDescription)
                editStatus = GetEditStatus(editStatus);

            product.AddedDate = productInfo.AddedDate;
            if (productInfo.SaleStatus != Entities.ProductInfo.ProductSaleStatus.InDraft)
            {
                product.SaleStatus = productInfo.SaleStatus;
            }
            product.AuditStatus = productInfo.AuditStatus;
            product.DisplaySequence = productInfo.DisplaySequence;
            product.ShopId = productInfo.ShopId;
            product.HasSKU = productInfo.HasSKU;
            product.ImagePath = productInfo.ImagePath;
            product.SaleCounts = productInfo.SaleCounts;
            product.VirtualSaleCounts = productInfo.VirtualSaleCounts;

            if (product.IsOpenLadder)
            {
                editStatus = GetEditStatus(editStatus);
            }

            productInfo.ColorAlias = product.ColorAlias;
            productInfo.SizeAlias = product.SizeAlias;
            productInfo.VersionAlias = product.VersionAlias;
            productInfo.VideoPath = product.VideoPath;
            product.DynamicMap(productInfo);

            productInfo.EditStatus = editStatus;

            var skuInofs = skus.Map<Entities.SKUInfo[]>();
            var descriptionInfo = description.Map<Entities.ProductDescriptionInfo>();
            var attributeInfos = attributes.Map<Entities.ProductAttributeInfo[]>();
            var sellerSpecificationInfos = sellerSpecifications.Map<Entities.SellerSpecificationValueInfo[]>();
            var ladderpricesInfos = prices.Select(p =>
            {
                var ladder = new Entities.ProductLadderPriceInfo();
                ladder.Id = p.Id;
                ladder.MinBath = p.MinBath;
                ladder.MaxBath = p.MaxBath;
                ladder.ProductId = p.ProductId;
                ladder.Price = p.Price;
                return ladder;
            }).ToArray();
            Service.UpdateProduct(productInfo, pics, skuInofs, descriptionInfo, attributeInfos, goodsCategory,
                sellerSpecificationInfos, ladderpricesInfos);
            if (productInfo.IsOpenLadder)
            {
                //处理门店
                ShopBranchApplication.UnSaleProduct(productInfo.Id);
            }
            CreateHtml(product.Id);
        }

        public static bool HasSKU(long product)
        {
            return Service.HasSKU(product);
        }
        /// <summary>
        /// 生成指定商品详情html
        /// </summary>
        public static void CreateHtml(long productId)
        {
            WebClient wc = new WebClient();
            //var preUrl = System.Configuration.ConfigurationManager.AppSettings["CurDomainUrl"];
            var preUrl = WebHelper.GetCurrentUrl();

            string url = preUrl + "/Product/Details/" + productId;
            string wapurl = preUrl + "/m-wap/Product/Details/" + productId + "?nojumpfg = 1";
            string urlHtml = "/Storage/Products/Statics/" + productId + ".html";
            string wapHtml = "/Storage/Products/Statics/" + productId + "-wap.html";
            var data = wc.DownloadData(url);
            var wapdata = wc.DownloadData(wapurl);
            MemoryStream memoryStream = new MemoryStream(data);
            MemoryStream wapMemoryStream = new MemoryStream(wapdata);
            MallIO.CreateFile(urlHtml, memoryStream, FileCreateType.Create);
            MallIO.CreateFile(wapHtml, wapMemoryStream, FileCreateType.Create);
        }

        static void CreatPCHtml(long productId)
        {
            WebClient wc = new WebClient();
            //   var preUrl = System.Configuration.ConfigurationManager.AppSettings["CurDomainUrl"];
            var preUrl = WebHelper.GetCurrentUrl();
            string url = preUrl + "/Product/Details/" + productId;
            string urlHtml = "/Storage/Products/Statics/" + productId + ".html";
            var data = wc.DownloadData(url);
            MemoryStream memoryStream = new MemoryStream(data);
            MallIO.CreateFile(urlHtml, memoryStream, FileCreateType.Create);
        }

        static void CreatWAPHtml(long productId)
        {
            WebClient wc = new WebClient();
            // var preUrl = System.Configuration.ConfigurationManager.AppSettings["CurDomainUrl"];

            var preUrl = WebHelper.GetCurrentUrl();
            string wapurl = preUrl + "/m-wap/Product/Details/" + productId + "?nojumpfg=1";
            string wapHtml = "/Storage/Products/Statics/" + productId + "-wap.html";
            var wapdata = wc.DownloadData(wapurl);
            MemoryStream wapMemoryStream = new MemoryStream(wapdata);
            MallIO.CreateFile(wapHtml, wapMemoryStream, FileCreateType.Create);
        }

        static void CreatBrandchWAPHtml(long productId, long branchId)
        {
            WebClient wc = new WebClient();
            // var preUrl = System.Configuration.ConfigurationManager.AppSettings["CurDomainUrl"];

            var preUrl = WebHelper.GetCurrentUrl();
            string wapBranchurl = preUrl + "/m-wap/BranchProduct/Details/" + productId + "?nojumpfg=1&shopBranchId=" + branchId;
            string wapBranchHtml = "/Storage/Products/Statics/" + productId + "-" + branchId + "-wap-branch.html";
            var wapbranchdata = wc.DownloadData(wapBranchurl);
            MemoryStream wapMemoryStream = new MemoryStream(wapbranchdata);
            MallIO.CreateFile(wapBranchHtml, wapMemoryStream, FileCreateType.Create);
        }

        /// <summary>
        /// 获取指定商品详情html
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static void GetPCHtml(long productId)
        {
            string pcUrlHtml = "/Storage/Products/Statics/" + productId + ".html";
            string fuleUrl = Core.Helper.IOHelper.GetMapPath(pcUrlHtml);
            RefreshLocalProductHtml(productId, pcUrlHtml, fuleUrl);
        }

        /// <summary>
        /// 获取指定商品详情html
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static void GetWAPHtml(long productId)
        {
            string wapUrlHtml = "/Storage/Products/Statics/" + productId + "-wap.html";
            string fullUrl = Core.Helper.IOHelper.GetMapPath(wapUrlHtml);
            RefreshWAPLocalProductHtml(productId, wapUrlHtml, fullUrl);
        }
        /// <summary>
        /// 获取指定门店商品详情html
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static void GetWAPBranchHtml(long productId, long branchId)
        {
            string wapUrlHtml = "/Storage/Products/Statics/" + productId + "-" + branchId + "-wap-branch.html";
            string fullUrl = Core.Helper.IOHelper.GetMapPath(wapUrlHtml);
            RefreshWAPLocalBranchProductHtml(productId, wapUrlHtml, fullUrl, branchId);
        }

        /// <summary>
        /// 刷新本地缓存商品html文件 
        /// </summary>     
        /// <param name="targetFilename">本地待生成的html文件名</param>
        static void RefreshWAPLocalProductHtml(long productId, string htmlUrl, string targetFilename)
        {
            lock (htmlUrl)
            {
                if (!File.Exists(targetFilename) || CheckNeedRefreshFile(File.GetLastWriteTime(targetFilename), 20))
                {
                    if (!MallIO.ExistFile(htmlUrl))
                        CreatWAPHtml(productId);
                    else
                    {
                        var metaRemoteInfo = MallIO.GetFileMetaInfo(htmlUrl);
                        if (null == metaRemoteInfo || CheckNeedRefreshFile(metaRemoteInfo.LastModifiedTime, 60))
                        {
                            CreatWAPHtml(productId);
                        }
                    }

                    var dirFullname = Core.Helper.IOHelper.GetMapPath("/Storage/Products/Statics");
                    if (!Directory.Exists(dirFullname))
                        Directory.CreateDirectory(dirFullname);
                    byte[] test = MallIO.GetFileContent(htmlUrl);
                    File.WriteAllBytes(targetFilename, MallIO.GetFileContent(htmlUrl));
                }
            }
        }

        /// <summary>
        /// 检查文件信息
        /// </summary>
        /// <param name="remote"></param>
        /// <param name="local"></param>
        /// <returns></returns>
        private static bool CheckNeedRefreshFile(DateTime modified, int minutes)
        {
            return (DateTime.Now - modified).TotalMinutes > minutes;
        }

        /// <summary>
        /// 刷新本地缓存门店商品html文件 
        /// </summary>     
        /// <param name="targetFilename">本地待生成的html文件名</param>
        static void RefreshWAPLocalBranchProductHtml(long productId, string htmlUrl, string targetFilename, long branchId)
        {
            lock (htmlUrl)
            {
                if (!File.Exists(targetFilename) || CheckNeedRefreshFile(File.GetLastWriteTime(targetFilename), 20))
                {
                    if (!MallIO.ExistFile(htmlUrl))
                        CreatBrandchWAPHtml(productId, branchId);
                    else
                    {
                        var metaRemoteInfo = MallIO.GetFileMetaInfo(htmlUrl);
                        if (null == metaRemoteInfo || CheckNeedRefreshFile(metaRemoteInfo.LastModifiedTime, 60))
                        {
                            CreatBrandchWAPHtml(productId, branchId);
                        }
                    }
                    var dirFullname = Core.Helper.IOHelper.GetMapPath("/Storage/Products/Statics");
                    if (!Directory.Exists(dirFullname))
                        Directory.CreateDirectory(dirFullname);
                    byte[] test = MallIO.GetFileContent(htmlUrl);
                    File.WriteAllBytes(targetFilename, MallIO.GetFileContent(htmlUrl));
                }
            }
        }

        /// <summary>
        /// 刷新本地缓存商品html文件 
        /// </summary>
        /// <param name="htmlUrl">远程html文件地址</param>
        /// <param name="targetFilename">本地待生成的html文件名</param>
        static void RefreshLocalProductHtml(long productId, string htmlUrl, string targetFilename)
        {
            lock (htmlUrl)
            {
                if (!File.Exists(targetFilename) || CheckNeedRefreshFile(File.GetLastWriteTime(targetFilename), 20))
                {
                    if (!MallIO.ExistFile(htmlUrl))
                        CreatPCHtml(productId);
                    else
                    {
                        var metaRemoteInfo = MallIO.GetFileMetaInfo(htmlUrl);
                        if (null == metaRemoteInfo || CheckNeedRefreshFile(metaRemoteInfo.LastModifiedTime, 60))
                        {
                            CreatPCHtml(productId);
                        }
                    }
                    var dirFullname = Core.Helper.IOHelper.GetMapPath("/Storage/Products/Statics");
                    if (!Directory.Exists(dirFullname))
                        Directory.CreateDirectory(dirFullname);
                    byte[] test = MallIO.GetFileContent(htmlUrl);
                    File.WriteAllBytes(targetFilename, MallIO.GetFileContent(htmlUrl));
                }
            }
        }





        /// <summary>
        /// 获取一个商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ProductInfo GetProduct(long id)
        {
            return Service.GetProduct(id);
        }
        public static List<ProductInfo> GetOnSaleProducts(List<long> products)
        {
            var pros = Service.GetProductByIds(products);
            return pros.Where(p => p.AuditStatus == ProductInfo.ProductAuditStatus.Audited && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale).ToList();
        }

        public static Dictionary<string, int> GetCommentsNumber(long product)
        {
            return Service.GetCommentsNumber(product);
        }
        /// <summary>
        /// 根据多个ID取多个商品信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<Product> GetProductsByIds(IEnumerable<long> ids)
        {
            var productsInfo = Service.GetProductByIds(ids);
            return productsInfo.ToList().Map<List<Product>>();
        }
        /// <summary>
        /// 获取商品
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public static List<Product> GetProducts(List<long> ids, long shopid)
        {
            var products = GetProductsByIds(ids);
            //TODO:FG 查询方法待优化
            return products.Where(p => p.ShopId == shopid).ToList();
        }

        public static List<ProductInfo> GetProducts(IEnumerable<long> products)
        {
            return Service.GetProducts(products.ToList());
        }
        /// <summary>
        /// 根据多个ID，取商品信息（所有状态）
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<Product> GetAllStatusProductByIds(IEnumerable<long> ids)
        {
            var productsInfo = Service.GetAllStatusProductByIds(ids);
            return productsInfo.ToList().Map<List<Product>>();
        }
        public static QueryPageModel<Product> GetProducts(ProductSearch query)
        {
            var data = Service.SearchProduct(query);

            return new QueryPageModel<Product>()
            {
                Models = data.Models.ToList().Map<List<Product>>(),
                Total = data.Total
            };
        }

        public static QueryPageModel<ProductInfo> GetProducts(ProductQuery query)
        {
            if (query.CategoryId.HasValue)
            {
                var categories = GetService<ICategoryService>().GetAllCategoryByParent(query.CategoryId.Value);
                query.Categories = categories.Select(p => p.Id).ToList();
                query.Categories.Add(query.CategoryId.Value);
            }
            return Service.GetProducts(query);
        }

        public static int GetProductCount(ProductQuery query)
        {
            return Service.GetProductCount(query);
        }

        /// <summary>
        /// 根据商品id获取属性
        /// </summary>
        /// <param name="id">商品id</param>
        /// <returns></returns>
        public static List<ProductAttributeInfo> GetProductAttributes(long id)
        {
            return Service.GetProductAttribute(id);
        }

        /// <summary>
        /// 根据商品id获取描述
        /// </summary>
        /// <param name="id">商品id</param>
        /// <returns></returns>
        public static ProductDescriptionInfo GetProductDescription(long id)
        {
            return Service.GetProductDescription(id);

        }

        /// <summary>
        /// 根据商品id获取描述
        /// </summary>
        /// <param name="ids">商品ids</param>
        /// <returns></returns>
        public static List<DTO.ProductDescription> GetProductDescription(long[] ids)
        {
            var description = Service.GetProductDescriptions(ids);
            // return AutoMapper.Mapper.Map<List<DTO.ProductDescription>>(description);
            return description.Map<List<DTO.ProductDescription>>();

        }

        public static List<ProductShopCategory> GetProductShopCategoriesByProductId(long productId)
        {
            return Service.GetProductShopCategories(productId).ToList().Map<List<ProductShopCategory>>();
        }



        /// <summary>
        /// 获取商品的评论数
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static int GetProductCommentCount(long productId)
        {
            return Service.GetProductCommentCount(productId);
        }
        /// <summary>
        /// 取店铺超出安全库存的商品数
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
		public static long GetOverSafeStockProducts(long shopid)
        {
            return Service.GetProductCount(new ProductQuery
            {
                ShopId = shopid,
                OverSafeStock = true,
                SaleStatus = ProductInfo.ProductSaleStatus.OnSale,
                AuditStatus = new Entities.ProductInfo.ProductAuditStatus[] { ProductInfo.ProductAuditStatus.Audited }
            });
        }

        /// <summary>
        /// 取店铺商品数量
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static long GetProductCount(long shopId)
        {
            return Service.GetProductCount(new ProductQuery
            {
                ShopId = shopId,
                SaleStatus = ProductInfo.ProductSaleStatus.OnSale,
                AuditStatus = new Entities.ProductInfo.ProductAuditStatus[] { ProductInfo.ProductAuditStatus.Audited }
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pids"></param>
        /// <param name="stock"></param>
        public static void SetProductOverSafeStock(IEnumerable<long> pids, long stock)
        {
            Service.SetProductOverSafeStock(pids, stock);
        }
        /// <summary>
        /// 删除门店对应的商品
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="shopId"></param>
        public static void DeleteProduct(IEnumerable<long> ids, long shopId)
        {
            Service.DeleteProduct(ids, shopId);
        }

        /// <summary>
        /// 修改推荐商品
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="relationProductIds"></param>
        public static void UpdateRelationProduct(long productId, string relationProductIds)
        {
            Service.UpdateRelationProduct(productId, relationProductIds);
        }

        /// <summary>
        /// 获取商品的推荐商品
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static DTO.ProductRelationProduct GetRelationProductByProductId(long productId)
        {
            return Service.GetRelationProductByProductId(productId).Map<DTO.ProductRelationProduct>();
        }

        /// <summary>
        /// 获取商品的推荐商品
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static List<ProductRelationProduct> GetRelationProductByProductIds(IEnumerable<long> productIds)
        {
            return Service.GetRelationProductByProductIds(productIds).Map<List<DTO.ProductRelationProduct>>();
        }

        /// <summary>
        /// 获取指定类型下面热销的前N件商品
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<Product> GetHotSaleProductByCategoryId(int categoryId, int count)
        {
            return Service.GetHotSaleProductByCategoryId(categoryId, count).Map<List<Product>>();
        }

        /// <summary>
        /// 获取商家所有商品描述模板
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static List<ProductDescriptionTemplate> GetDescriptionTemplatesByShopId(long shopId)
        {
            return _productDescriptionTemplateService.GetTemplates(shopId).ToList().Map<List<ProductDescriptionTemplate>>();
        }
        /// <summary>
        /// 批量下架商品
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="shopid"></param>
        public static void BatchSaleOff(IEnumerable<long> ids, long shopid)
        {
            Service.SaleOff(ids, shopid);
        }
        /// <summary>
        /// 批量上架商品
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="shopid"></param>
        public static void BatchOnSale(IEnumerable<long> ids, long shopid)
        {
            Service.OnSale(ids, shopid);
        }



        /// <summary>
        /// 设置商品库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="products"></param>
        /// <param name="stock"></param>
        /// <param name="option"></param>
        public static void SetProductStock(List<long> products, int stock, StockOptionType option)
        {
            Service.SetProductStock(products, stock, option);
        }

        public static void SetProductStock(long product, int stock, StockOptionType option)
        {
            SetProductStock(new List<long> { product }, stock, option);
        }

        public static bool BranchCanBuy(long userId, long productId, int count, string skuId, long shopBranchId, out int reason)
        {
            var product = Service.GetProduct(productId);
            if (product.SaleStatus != Entities.ProductInfo.ProductSaleStatus.OnSale || product.AuditStatus != Entities.ProductInfo.ProductAuditStatus.Audited)
            {
                //商城商品下架，但是门店的商品状态销售中，允许用户购买。
                //商城商品下架后，销售状态-仓库中，审核状态-待审核
                if (product.SaleStatus != Entities.ProductInfo.ProductSaleStatus.InStock && product.AuditStatus != Entities.ProductInfo.ProductAuditStatus.WaitForAuditing)
                {
                    reason = 1;
                    return false;
                }
            }
            var sku = ProductManagerApplication.GetSKU(skuId);
            if (sku == null)
            {
                reason = 2;
                return false;
            }
            var shopBranch = ShopBranchApplication.GetShopBranchById(shopBranchId);
            if (shopBranch == null)
            {
                reason = 4;
                return false;
            }
            var shopBranchSkuList = ShopBranchApplication.GetSkusByIds(shopBranchId, new List<string> { skuId });
            if (shopBranchSkuList == null || shopBranchSkuList.Count == 0 || shopBranchSkuList[0].Status == ShopBranchSkuStatus.InStock)
            {
                reason = 2;
                return false;
            }
            var sbsku = shopBranchSkuList.FirstOrDefault();
            if (sbsku.Stock < count)
            {
                reason = 9;
                return false;
            }
            if (product.IsDeleted)
            {
                reason = 2;
                return false;
            }

            if (product.MaxBuyCount <= 0)
            {
                reason = 0;
                return true;
            }

            var buyedCounts = OrderApplication.GetProductBuyCount(userId, new long[] { productId });
            if (product.MaxBuyCount < count + (buyedCounts.ContainsKey(productId) ? buyedCounts[productId] : 0))
            {
                reason = 3;
                return false;
            }
            reason = 0;
            return true;
        }
        /// <summary>
        /// 普通商品是否可购买（过滤活动购买数量）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="productId"></param>
        /// <param name="count"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static bool CanBuy(long userId, long productId, int count, out int reason)
        {
            //TODO:FG 禁止使用 数字常量直接表示意义。 返回值不推荐使用out进行方法返回
            var product = Service.GetProduct(productId);
            if (product.SaleStatus != Entities.ProductInfo.ProductSaleStatus.OnSale || product.AuditStatus != Entities.ProductInfo.ProductAuditStatus.Audited)
            {
                reason = 1;
                return false;
            }
            var skus = Service.GetSKUs(productId);
            long stock = skus.Sum(p => p.Stock);
            if (stock == 0)
            {
                reason = 9;
                return false;
            }
            if (product.IsDeleted)
            {
                reason = 2;
                return false;
            }

            if (product.MaxBuyCount <= 0)
            {
                reason = 0;
                return true;
            }

            if (product.IsOpenLadder)
            {
                reason = 0;
                return true;
            }

            var buyedCounts = OrderApplication.GetProductBuyCount(userId, new long[] { productId });
            if (product.MaxBuyCount < count + (buyedCounts.ContainsKey(productId) ? buyedCounts[productId] : 0))
            {
                reason = 3;
                return false;
            }

            reason = 0;
            return true;
        }

        public static void AddBrowsingProduct(Entities.BrowsingHistoryInfo info)
        {
            Service.AddBrowsingProduct(info);
        }

        /// <summary>
		/// 批量获取商品信息
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
        public static List<Entities.ProductInfo> GetProductByIds(IEnumerable<long> ids)
        {
            return Service.GetProductByIds(ids);
        }
        /// <summary>
		/// 批量获取商品信息
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
        public static List<Entities.ProductInfo> GetAllProductByIds(IEnumerable<long> ids)
        {
            return Service.GetAllProductByIds(ids);
        }

        public static List<Entities.BrowsingHistoryInfo> GetBrowsingProducts(long userId)
        {
            return Service.GetBrowsingProducts(userId);
        }

        #region 阶梯价--张宇枫
        /// <summary>
        /// 根据商品ID获取阶梯价列表
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="isOpenLadder">实付开启了阶梯价</param>
        /// <returns></returns>
        public static List<ProductLadderPriceInfo> GetLadderPriceInfosByProductId(long productId, bool isOpenLadder = true)
        {
            if (isOpenLadder)
                return _productLadderPriceService.GetLadderPricesByProductIds(productId);
            return null;
        }

        /// <summary>
        /// 根据商品ID获取多个价格
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="IsSelf">是否官方直营店</param>
        /// <param name="discount">会员折扣(0.01-1)</param>
        /// <returns></returns>
        public static List<ProductLadderPrice> GetLadderPriceByProductIds(long productId, bool IsSelf = false, decimal discount = 1m)
        {
            var priceInfo = _productLadderPriceService.GetLadderPricesByProductIds(productId);
            if (priceInfo == null)
                priceInfo = new List<ProductLadderPriceInfo>();//特意不让为空，便于前台调用可取值

            return priceInfo.Select(p =>
            {
                var lprice = p.Price;
                if (IsSelf)
                    lprice = p.Price * discount;
                var price = new ProductLadderPrice
                {
                    Id = p.Id,
                    MinBath = p.MinBath,
                    MaxBath = p.MaxBath,
                    ProductId = p.ProductId,
                    Price = Convert.ToDecimal(lprice.ToString("F2"))
                };
                return price;
            }).ToList();
        }

        /// <summary>
        /// 获取商品销售价格
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public static decimal GetProductLadderPrice(long productId, int quantity)
        {
            var ladderPrices = _productLadderPriceService.GetLadderPricesByProductIds(productId);
            var price = 0m;
            if (ladderPrices.Count > 0)
                return GetProductLadderPrice(ladderPrices, quantity);
            return price;
        }

        public static decimal GetProductLadderPrice(List<ProductLadderPriceInfo> source, int quantity)
        {
            return source.Find(i => (quantity <= i.MinBath) || (quantity >= i.MinBath && quantity <= i.MaxBath)).Price;
        }


        public static void FillSkuAlias(SKUInfo sku, ProductInfo product, TypeInfo type)
        {
            //默认名
            sku.ColorAlias = SpecificationType.Color.ToDescription();
            sku.SizeAlias = SpecificationType.Size.ToDescription();
            sku.VersionAlias = SpecificationType.Version.ToDescription();

            //分类别名
            if (type != null)
            {
                if (!string.IsNullOrEmpty(type.ColorAlias)) sku.ColorAlias = type.ColorAlias;
                if (!string.IsNullOrEmpty(type.SizeAlias)) sku.SizeAlias = type.SizeAlias;
                if (!string.IsNullOrEmpty(type.VersionAlias)) sku.VersionAlias = type.VersionAlias;
            }

            //商品别名
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.ColorAlias)) sku.ColorAlias = product.ColorAlias;
                if (!string.IsNullOrEmpty(product.SizeAlias)) sku.SizeAlias = product.SizeAlias;
                if (!string.IsNullOrEmpty(product.VersionAlias)) sku.VersionAlias = product.VersionAlias;
            }
        }
        /// <summary>
        /// 获取阶梯商品最小批量
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static int GetProductLadderMinMath(long productId)
        {
            var minMath = 0;
            var ladder = GetLadderPriceByProductIds(productId);
            if (ladder.Any())
                minMath = ladder.Min(p => p.MinBath);
            return minMath;
        }

        /// <summary>
        /// 判断购物车提交时，阶梯商品是否达最小批量
        /// </summary>
        /// <param name="cartItemIds"></param>
        /// <returns></returns>
        public static bool IsExistLadderMinMath(string cartItemIds, ref string msg)
        {
            msg = "结算的商品必须满足最小批量才能购买！";
            var result = true;
            var cartItemIdsArr = cartItemIds.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(t => long.Parse(t));
            var cartItems = CartApplication.GetCartItems(cartItemIdsArr);
            if (cartItems.Any())
            {
                var groupCart = cartItems.Where(item => item.ShopBranchId == 0).ToList().Select(c =>
               {
                   var cItem = new Mall.Entities.ShoppingCartItem();
                   var skuInfo = Service.GetSku(c.SkuId);
                   if (skuInfo != null)
                       cItem = c;
                   return cItem;
               }).GroupBy(i => i.ProductId);
                foreach (var cart in cartItems.ToList())
                {
                    var product = GetProduct(cart.ProductId);
                    if (product.IsOpenLadder)
                    {
                        var quantity =
                            groupCart.Where(i => i.Key == cart.ProductId)
                                .ToList()
                                .Sum(cartitem => cartitem.Sum(i => i.Quantity));
                        var minMath = GetProductLadderMinMath(cart.ProductId);
                        if (minMath > 0 && quantity < minMath)
                            result = false;
                    }
                    else
                    {
                        var sku = Service.GetSku(cart.SkuId);
                        if (cart.Quantity > sku.Stock)
                        {
                            msg = string.Format("商品“{0}”库存不足,仅剩{1}件", CutString(product.ProductName, 10, "..."), sku.Stock);
                            return false;
                        }
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 切割字符
        /// </summary>
        /// <param name="str"></param>
        /// <param name="len"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        private static string CutString(string str, int len, string suffix)
        {
            if (!string.IsNullOrWhiteSpace(str) && str.Length > len)
            {
                str = str.Substring(0, len) + suffix;
            }
            return str;
        }
        #endregion

        /// <summary>
        /// 指定地区包邮
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="discount"></param>
        /// <param name="streetId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static bool IsFreeRegion(long productId, decimal discount, int streetId, int count, string skuId)
        {
            return Service.IsFreeRegion(productId, discount, streetId, count, skuId);
        }

        public static bool BatchSettingFreightTemplate(IEnumerable<long> pids, long freightTemplateId)
        {
            return Service.BatchSettingFreightTemplate(pids, freightTemplateId);
        }
        public static bool BatchSettingStock(Dictionary<long, long> dics)
        {
            return Service.BatchSettingStock(dics);
        }
        public static bool BatchSettingPrice(Dictionary<long, decimal> productDics, Dictionary<long, string> priceDics)
        {
            return Service.BatchSettingPrice(productDics, priceDics);
        }
        public static bool UpdateShopDisplaySequence(long id, int order)
        {
            return Service.UpdateShopDisplaySequence(id, order);
        }
        public static bool UpdateDisplaySequence(long id, int order)
        {
            return Service.UpdateDisplaySequence(id, order);
        }
        /// <summary>
        /// 批量更新虚拟销量
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="virtualSaleCounts"></param>
        /// <returns></returns>
        public static bool BtachUpdateSaleCount(List<long> productIds, long virtualSaleCounts, int minSaleCount = 0, int maxSaleCount = 0)
        {
            return Service.BtachUpdateSaleCount(productIds, virtualSaleCounts, minSaleCount, maxSaleCount);
        }

        /// <summary>
        /// 当前参加的活动
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static string CurrentJoinActive(long productId)
        {
            string result = Service.CurrentJoinActive(productId);
            return result;
        }
        public static void DeleteImportProduct(List<long> productIds)
        {
            Service.DeleteImportProduct(productIds);
        }
        /// <summary>
        /// 获取商品显示状态
        /// </summary>
        /// <param name="pro"></param>
        /// <param name="sku"></param>
        /// <param name="quantity"></param>
        /// <param name="sb"></param>
        /// <param name="sbsku"></param>
        /// <returns>状态值 0:正常；1：已失效；2：库存不足；3：已下架；</returns>
        public static int GetProductShowStatus(ProductInfo pro, DTO.SKU sku = null, int quantity = 1, DTO.ShopBranch sb = null, ShopBranchSkuInfo sbsku = null)
        {
            int result = 0;
            //己删除商品快速处理
            if (pro.IsDeleted)
            {
                return 1;
            }
            if (sb != null && pro.IsOpenLadder)
            {
                return 1;
            }
            if (sb != null && sbsku != null)
            {
                if (sbsku.Status == ShopBranchSkuStatus.Normal)
                {
                    if (sbsku.Stock < quantity)
                    {
                        result = 2;
                    }
                }
                else
                {
                    result = 3;
                }
                return result;
            }
            if (pro.AuditStatus == ProductInfo.ProductAuditStatus.Audited && pro.SaleStatus == ProductInfo.ProductSaleStatus.OnSale)
            {
                result = 0;
                if (sku == null)
                {
                    var skus = GetSKUs(pro.Id);
                    if (skus.Sum(d => d.Stock) < quantity)
                    {
                        result = 2;
                    }
                }
                else
                {
                    if (sku.Stock < quantity)
                    {
                        result = 2;
                    }
                }
            }
            else
            {
                result = 3;
            }
            return result;
        }
        #region 私有方法
        private static Entities.ProductInfo.ProductEditStatus GetEditStatus(Entities.ProductInfo.ProductEditStatus status)
        {
            if (status > Entities.ProductInfo.ProductEditStatus.EditedAndPending)
                return Entities.ProductInfo.ProductEditStatus.CompelPendingHasEdited;
            return Entities.ProductInfo.ProductEditStatus.EditedAndPending;
        }
        #endregion



        #region SKU相关 //TODO:FG 待移入SKUApplication

        /// <summary>
        /// 根据商品id获取SKU
        /// </summary>
        /// <param name="id">商品id</param>
        /// <returns></returns>
        public static List<SKU> GetSKUs(long product)
        {
            var skus = Service.GetSKUs(product);
            // return AutoMapper.Mapper.Map<List<SKU>>(skus);

            return skus.Map<List<SKU>>();

        }

        /// <summary>
        /// 根据商品id获取SKU
        /// </summary>
        /// <param name="productIds">商品id</param>
        /// <returns></returns>
        public static List<SKU> GetSKUByProducts(IEnumerable<long> productIds)
        {
            var skus = Service.GetSKUs(productIds);
            //  return AutoMapper.Mapper.Map<List<DTO.SKU>>(skus);

            return skus.Map<List<DTO.SKU>>();

        }

        public static List<SKUInfo> GetSKUsByProduct(IEnumerable<long> productIds)
        {
            return Service.GetSKUs(productIds);
        }
        /// <summary>
        /// 根据sku id 获取sku信息
        /// </summary>
        /// <param name="skuIds"></param>
        /// <returns></returns>
        public static List<SKU> GetSKUs(IEnumerable<string> skuIds)
        {
            var list = Service.GetSKUs(skuIds);
            return list.Map<List<DTO.SKU>>();
        }

        public static List<SKUInfo> GetSKUInfos(IEnumerable<string> skuIds)
        {
            return Service.GetSKUs(skuIds);
        }
        /// <summary>
        /// 根据商品id获取SKU
        /// </summary>
        /// <param name="id">商品id</param>
        /// <returns></returns>
        public static SKU GetSKU(string skuId)
        {
            var sku = Service.GetSku(skuId);
            // var ret = AutoMapper.Mapper.Map<DTO.SKU>(sku);

            var ret = sku.Map<DTO.SKU>();

            return ret;
        }
        /// <summary>
        /// 根据商品id获取SKU
        /// </summary>
        /// <param name="skuId"></param>
        /// <returns></returns>
        public static SKUInfo GetSKUInfo(string skuId)
        {
            var sku = Service.GetSku(skuId);
            return sku;
        }


        public static void SetSkuStock(StockOptionType option, Dictionary<string, long> changes)
        {
            Service.SetSkuStock(option, changes);
        }
        public static void SetSkuStock(StockOptionType option, string skuId, long change)
        {
            Service.SetSkuStock(skuId, change, option);
        }

        /// <summary>
        /// 取超出警戒库存的商品ID
        /// </summary>
        /// <param name="pids"></param>
        /// <returns></returns>
        public static IEnumerable<long> GetOverSafeStockProductIds(IEnumerable<long> pids)
        {
            var skus = Service.GetSKUs(pids).ToList();
            var overStockPids = skus.Where(e => e.SafeStock >= e.Stock).Select(e => e.ProductId).Distinct();
            return overStockPids;
        }



        #endregion

        #region 虚拟商品
        public static VirtualProductInfo GetVirtualProductInfoByProductId(long productId)
        {
            return Service.GetVirtualProductInfoByProductId(productId);
        }
        public static List<VirtualProductInfo> GetVirtualProductInfoByProductIds(List<long> productIds)
        {
            return Service.GetVirtualProductInfoByProductIds(productIds);
        }
        public static List<VirtualProductItemInfo> GetVirtualProductItemInfoByProductId(long productId)
        {
            return Service.GetVirtualProductItemInfoByProductId(productId);
        }

        /// <summary>
        /// 核销码生效类型文本
        /// </summary>
        /// <param name="effectiveType">核销码生效类型</param>
        /// <param name="hour">付完款几小时后生效</param>
        /// <returns></returns>
        public static string GetSupportRefundTypeText(sbyte supportRefundType)
        {
            string strText = string.Empty;
            switch (supportRefundType)
            {
                case 1:
                    strText = "支持有效期内退款";
                    break;
                case 2:
                    strText = "支持随时退款";
                    break;
                case 3:
                    strText = "不支持退款";
                    break;
            }
            return strText;
        }

        /// <summary>
        /// 核销码生效类型文本
        /// </summary>
        /// <param name="effectiveType">核销码生效类型</param>
        /// <param name="hour">付完款几小时后生效</param>
        /// <returns></returns>
        public static string GetEffectiveTypeText(sbyte effectiveType, int hour)
        {
            string strText = string.Empty;
            switch (effectiveType)
            {
                case 1:
                    strText = "立即生效";
                    break;
                case 2:
                    strText = string.Format("付款完成{0}小时后生效", hour);
                    break;
                case 3:
                    strText = "次日生效";
                    break;
            }
            return strText;
        }
        #endregion

        /// <summary>
        /// 同步商品的查询价格
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="newPrice"></param>
        public static void UpdateSearchProductPrice(long productId, long shopId, decimal newPrice)
        {
            _searchProductService.UpdateSearchProductPrice(productId, shopId, newPrice);
        }

        public static SearchProductInfo GetSingleSearchProductInfo(long productId, long shopId)
        {
            return _searchProductService.GetSingleSearchProductInfo(productId, shopId);
        }

        /// <summary>
        /// 将最低价格保存在商品查询表中
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="shopId"></param>
        public static void SaveCaculateMinPrice(long productId, long shopId)
        {
            var product = Service.GetProduct(productId);
            if (product == null) return;
            decimal minPrice = 0;
            if (product.IsOpenLadder)
            {
                //比较阶梯价，取阶梯价最小价格，不用管梯度
                var ladderSkus = _productLadderPriceService.GetLadderPricesByProductIds(productId);
                if (ladderSkus != null && ladderSkus.Count() > 0)
                {
                    decimal minLadderPrice = ladderSkus.Min(p => p.Price);
                    minPrice = minLadderPrice;
                }
            }
            else
            {
                //先取sku最小价格
                var skus = Service.GetSKUs(productId);
                minPrice = skus.Min(p => p.SalePrice);
            }
            //比较拼团价格
            var fightGroup = _iFightGroupService.GetActiveIdByProductIdAndShopId(productId, shopId);
            if (fightGroup != null && fightGroup.ActiveStatus == FightGroupActiveStatus.Ongoing)
            {
                var miniGroupPrice = fightGroup.ActiveItems.Min(p => p.ActivePrice);
                if (miniGroupPrice < minPrice)
                {
                    minPrice = miniGroupPrice;
                }
            }
            //比较限时购价格
            var ltmbuy = ServiceApplication.Create<ILimitTimeBuyService>().GetFlashSaleInfoByProductIdAndShopId(productId, shopId);
            if (ltmbuy != null)
            {
                if (ltmbuy.BeginDate <= DateTime.Now)
                {
                    if (ltmbuy.MinPrice < minPrice)
                    {
                        minPrice = ltmbuy.MinPrice;
                    }
                }
                else
                {
                    var flashSaleConfig = _iLimitTimeBuyService.GetConfig();
                    TimeSpan flashSaleTime = ltmbuy.BeginDate - DateTime.Now;  //开始时间还剩多久
                    TimeSpan preheatTime = new TimeSpan(flashSaleConfig.Preheat, 0, 0);  //预热时间是多久
                    if (preheatTime >= flashSaleTime && !flashSaleConfig.IsNormalPurchase && ltmbuy.MinPrice < minPrice)  //预热大于开始并且不能购买，写入最低价格，需求386
                    {
                        minPrice = ltmbuy.MinPrice;
                    }
                }
            }
            var searchInfo = _searchProductService.GetSingleSearchProductInfo(productId, shopId);
            if (searchInfo != null && minPrice != searchInfo.SalePrice)
            {
                _searchProductService.UpdateSearchProductPrice(productId, shopId, minPrice);
            }
        }

        /// <summary>
        /// 判断是否关注过此商品
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public static bool IsFavorite(long productId, long userId)
        {
            return Service.IsFavorite(productId, userId);
        }
    }
}
