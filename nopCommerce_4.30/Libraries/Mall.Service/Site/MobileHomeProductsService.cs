using Mall.CommonModel;
using Mall.Core;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System.Collections.Generic;
using System.Linq;
using NetRube.Data;

namespace Mall.Service
{
    public class MobileHomeProductsService : ServiceBase, IMobileHomeProductsService
    {
        public void AddProductsToHomePage(long shopId, PlatformType platformType, IEnumerable<long> productIds)
        {
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var existHomepageProductIds = DbFactory.Default.Get<MobileHomeProductInfo>().Where(item => item.ShopId == shopId && item.PlatFormType == platformType).Select(item => item.ProductId).ToList<long>();//获取当前店铺已添加的首页商品Id
            var notExistProductIds = productIds.Where(item => !existHomepageProductIds.Contains(item));//从待添加的商品中去除已添加的商品

            DbFactory.Default.InTransaction(() =>
            {
                foreach (var productId in notExistProductIds)
                {
                    var product = productService.GetProduct(productId);
                    if (!product.IsDeleted)
                    {
                        if (shopId != 0 && product.ShopId != shopId)//店铺添加首页商品时，判断该商品是否为该店铺的商品
                            throw new Mall.Core.MallException("待添加至首页的商品不得包含非本店铺商品");

                        var mobileHomepageProduct = new MobileHomeProductInfo()
                        {
                            PlatFormType = platformType,
                            Sequence = 1,
                            ProductId = productId,
                            ShopId = shopId
                        };
                        DbFactory.Default.Add(mobileHomepageProduct);
                    }
                }
                var delProductIds = existHomepageProductIds.Where(e => !productIds.Contains(e));
                if (delProductIds.Count() > 0)
                {
                    DbFactory.Default.Del<MobileHomeProductInfo>(item => item.ShopId == shopId && item.PlatFormType == platformType && item.ProductId.ExIn(delProductIds));
                }
            });
        }

        public void UpdateSequence(long shopId, long id, short sequence)
        {
            var mobileHomeProduct = DbFactory.Default.Get<MobileHomeProductInfo>().Where(item => item.Id == id && item.ShopId == shopId).FirstOrDefault();
            if (mobileHomeProduct == null)
                throw new Mall.Core.MallException(string.Format("不存在Id为{0}的首页商品设置", id));
            DbFactory.Default.Set<MobileHomeProductInfo>().Set(n => n.Sequence, sequence).Where(p => p.Id == mobileHomeProduct.Id).Succeed();
        }

        public void Delete(long shopId, long id)
        {
            DbFactory.Default.Del<MobileHomeProductInfo>(n => n.Id == id);
        }

        public void DeleteList(long[] ids)
        {
            DbFactory.Default.Del<MobileHomeProductInfo>(item => item.Id.ExIn(ids));
        }

        public void DeleteAll(long shopId)
        {
            DbFactory.Default.Del<MobileHomeProductInfo>(n => n.ShopId == shopId);
        }

        public QueryPageModel<MobileHomeProductInfo> GetMobileHomePageProducts(long shopId, PlatformType platformType, ProductQuery productQuery)
        {

            var homeProducts = DbFactory.Default.Get<MobileHomeProductInfo>()
                .LeftJoin<ProductInfo>((mi, pi) => mi.ProductId == pi.Id)
                .Where(item => item.ShopId == shopId && item.PlatFormType == platformType);
                
            if (!string.IsNullOrWhiteSpace(productQuery.KeyWords))
            {
                productQuery.KeyWords = productQuery.KeyWords.Trim();
                var brandIds = DbFactory.Default.Get<BrandInfo>().Where(item => item.Name.Contains(productQuery.KeyWords) && item.IsDeleted == false).Select(item => item.Id).ToList<long>();
                homeProducts.Where<ProductInfo>(item => item.ProductName.Contains(productQuery.KeyWords) || item.BrandId.ExIn(brandIds));
            }

            if (!string.IsNullOrWhiteSpace(productQuery.ShopName))
            {
                productQuery.ShopName = productQuery.ShopName.Trim();
                var productids = DbFactory.Default.Get<ProductInfo>().LeftJoin<ShopInfo>((pi,si)=>pi.ShopId == si.Id).Where<ShopInfo>(item => item.ShopName.Contains(productQuery.ShopName)).Select(item => item.Id).ToList<long>();
                homeProducts.Where(p => p.ProductId.ExIn(productids));
            }

            if (productQuery.CategoryId.HasValue)
            {
                homeProducts.Where<ProductInfo>(item => ("|" + item.CategoryPath + "|").Contains("|" + productQuery.CategoryId.Value + "|"));
            }


            var models = homeProducts.OrderBy(item => item.Sequence).OrderBy(item => item.Id).ToPagedList(productQuery.PageNo, productQuery.PageSize);
            return new QueryPageModel<MobileHomeProductInfo>() { Models = models, Total = models.TotalRecordCount };
        }

        public QueryPageModel<MobileHomeProductInfo> GetSellerMobileHomePageProducts(long shopId, PlatformType platformType, ProductQuery productQuery)
        {
            var homeProducts = DbFactory.Default.Get<MobileHomeProductInfo>().Where(item => item.ShopId == shopId && item.PlatFormType == platformType);

            if (!string.IsNullOrWhiteSpace(productQuery.KeyWords))
            {
                productQuery.KeyWords = productQuery.KeyWords.Trim();
                var brandIds = DbFactory.Default.Get<BrandInfo>().Where(item => item.Name.Contains(productQuery.KeyWords) && item.IsDeleted == false).Select(item => item.Id).ToList<long>();
                homeProducts.LeftJoin<ProductInfo>((mi, pi) => mi.ProductId == pi.Id).Where<ProductInfo>(item => item.ProductName.Contains(productQuery.KeyWords) || item.BrandId.ExIn(brandIds));
            }



            if (productQuery.ShopCategoryId.HasValue)
            {
                var shopCategoryIds = DbFactory.Default.Get<ShopCategoryInfo>().Where(c => c.Id == productQuery.ShopCategoryId || c.ParentCategoryId == productQuery.ShopCategoryId).Select(c => c.Id).ToList<long>();
                var productIds = DbFactory.Default.Get<ProductShopCategoryInfo>().Where(item => item.ShopCategoryId.ExIn(shopCategoryIds)).Select(item => item.ProductId).ToList<long>();
                homeProducts.Where(item => item.ProductId.ExIn(productIds));
            }

            var models = homeProducts.OrderBy(item => item.Sequence).ToPagedList(productQuery.PageNo, productQuery.PageSize);
            QueryPageModel<MobileHomeProductInfo> pageModel = new QueryPageModel<MobileHomeProductInfo>() { Models = models, Total = models.TotalRecordCount };
            return pageModel;
        }

        public QueryPageModel<MobileHomeProductInfo> GetMobileHomeProducts(long shopId, PlatformType type, int pageNo, int pageSize)
        {
            var data = DbFactory.Default.Get<MobileHomeProductInfo>()
               .LeftJoin<ProductInfo>((mpi, pi) => mpi.ProductId == pi.Id)
               .Where(p => p.ShopId == shopId && p.PlatFormType == type)
               .Where<ProductInfo>(p => p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && p.AuditStatus == ProductInfo.ProductAuditStatus.Audited && p.IsDeleted == false)
               .OrderBy(p => p.Sequence)
               .OrderByDescending(p => p.Id)
               .ToPagedList(pageNo, pageSize);
            return new QueryPageModel<MobileHomeProductInfo>
            {
                Models = data,
                Total = data.TotalRecordCount
            };
        }
    }
}
