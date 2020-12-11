using Mall.Application;
using Mall.CommonModel;
using Mall.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Web.Framework
{
    public class BrowseHistrory
    {
        public static void AddBrowsingProduct(long productId, long userId = 0)
        {
            List<ProductBrowsedHistoryModel> productIdList = new List<ProductBrowsedHistoryModel>();
            string productIds = Core.Helper.WebHelper.GetCookie(CookieKeysCollection.Mall_PRODUCT_BROWSING_HISTORY);
            if (!string.IsNullOrEmpty(productIds))
            {
                var arr = productIds.Split(',');
                foreach (var a in arr)
                {
                    var item = a.Split('#');
                    if (item.Length > 1)
                    {
                        productIdList.Add(new ProductBrowsedHistoryModel() { ProductId = long.Parse(item[0]), BrowseTime = DateTime.Parse(item[1]) });
                    }
                    else
                    {
                        productIdList.Add(new ProductBrowsedHistoryModel() { ProductId = long.Parse(item[0]), BrowseTime = DateTime.Now });
                    }
                }
            }
            if (productIdList.Count < 20 && !productIdList.Any(a => a.ProductId == productId))
            {
                productIdList.Add(new ProductBrowsedHistoryModel() { ProductId = productId, BrowseTime = DateTime.Now });
            }
            else if (productIdList.Count >= 20 && !productIdList.Any(a => a.ProductId == productId))
            {
                productIdList.RemoveAt(productIdList.Count - 1);
                productIdList.Add(new ProductBrowsedHistoryModel() { ProductId = productId, BrowseTime = DateTime.Now });
            }
            else
            {
                var model = productIdList.Where(a => a.ProductId == productId).FirstOrDefault();
                productIdList.Remove(model);
                productIdList.Add(new ProductBrowsedHistoryModel() { ProductId = productId, BrowseTime = DateTime.Now });
            }
            if (userId == 0)
            {
                var productsStr = "";
                foreach (var item in productIdList)
                {
                    productsStr += item.ProductId + "#" + item.BrowseTime.ToString() + ",";
                }
                Core.Helper.WebHelper.SetCookie(CookieKeysCollection.Mall_PRODUCT_BROWSING_HISTORY, productsStr.TrimEnd(','), DateTime.Now.AddDays(7));
            }
            else
            {
                foreach (var item in productIdList)
                {
                    try
                    {
                        ProductManagerApplication.AddBrowsingProduct(new Entities.BrowsingHistoryInfo
                        {
                            MemberId = userId,
                            BrowseTime = item.BrowseTime,
                            ProductId = item.ProductId
                        });
                    }
                    catch
                    {
                        continue;
                    }
                }
                Core.Helper.WebHelper.DeleteCookie(CookieKeysCollection.Mall_PRODUCT_BROWSING_HISTORY);
            }
        }

        public static List<ProductBrowsedHistoryModel> GetBrowsingProducts(int num, long userId = 0)
        {
            List<ProductBrowsedHistoryModel> productIdList = new List<ProductBrowsedHistoryModel>();
            string productIds = Core.Helper.WebHelper.GetCookie(CookieKeysCollection.Mall_PRODUCT_BROWSING_HISTORY);
            if (!string.IsNullOrEmpty(productIds))
            {
                var arr = productIds.Split(',');
                foreach (var a in arr)
                {
                    var item = a.Split('#');
                    if (item.Length > 1)
                    {
                        productIdList.Add(new ProductBrowsedHistoryModel() { ProductId = long.Parse(item[0]), BrowseTime = DateTime.Parse(item[1]) });
                    }
                    else
                    {
                        productIdList.Add(new ProductBrowsedHistoryModel() { ProductId = long.Parse(item[0]), BrowseTime = DateTime.Now });
                    }
                }
            }

            var ids = productIdList.Select(p => p.ProductId).ToList();
            List<FlashSalePrice> flashSaleList = LimitTimeApplication.GetPriceByProducrIds(ids);

            List<ProductBrowsedHistoryModel> model = new List<ProductBrowsedHistoryModel>();
            if (userId == 0)
            {
                var products = ProductManagerApplication.GetProductByIds(productIdList.Select(a => a.ProductId))
                    .Where(d => d.SaleStatus == Entities.ProductInfo.ProductSaleStatus.OnSale
                        && d.AuditStatus == Entities.ProductInfo.ProductAuditStatus.Audited).ToArray()
                    .Select(a => new ProductBrowsedHistoryModel()
                    {
                        ImagePath = Core.MallIO.GetProductSizeImage(a.RelativePath, 1, (int)ImageSize.Size_100),
                        ProductId = a.Id,
                        ProductName = a.ProductName,
                        ProductPrice = GetRealPrice(flashSaleList, a.Id, a.MinSalePrice),
                        ShopId = a.ShopId
                    }).ToList();

           
                return products.OrderByDescending(a => a.BrowseTime).ToList();
            }
            else
            {
                foreach (var m in productIdList)
                {
                    AddBrowsingProduct(m.ProductId, userId);
                }
                var browsing = ProductManagerApplication.GetBrowsingProducts(userId);
                var products = ProductManagerApplication.GetOnSaleProducts(browsing.Select(p => p.ProductId).ToList());
                browsing = browsing.Where(p => products.Select(o => o.Id).Contains(p.ProductId)).ToList() ;
                model =
                    browsing.OrderByDescending(a => a.BrowseTime).Take(num)
                   .Select(a => {
                       var product = products.FirstOrDefault(p => p.Id == a.ProductId);
                       return new ProductBrowsedHistoryModel()
                       {
                           ImagePath = Core.MallIO.GetProductSizeImage(product.RelativePath, 1, (int)ImageSize.Size_100),
                           ProductId = a.ProductId,
                           ProductName = product.ProductName,
                           ProductPrice = GetRealPrice(flashSaleList, a.ProductId, product.MinSalePrice),
                           BrowseTime = a.BrowseTime,
                           ShopId = product.ShopId
                       };
                   }).ToList();
            }
            return model;
        }

        private static decimal GetRealPrice(List<FlashSalePrice> list, long productid, decimal oldPrice)
        {
            var model = list.Where(p => p.ProductId == productid).FirstOrDefault();
            if (model != null)
            {
                return model.MinPrice;
            }
            return oldPrice;
        }
    }
}
