using Mall.API.Model.ParamsModel;
using Mall.Application;
using Mall.DTO.QueryModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System;
using Mall.CommonModel;
using Microsoft.AspNetCore.Mvc;

namespace Mall.API
{
    public class ShopBranchProductController : BaseShopBranchLoginedApiController
    {
        /// <summary>
        /// 门店查询商家商品
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        /// 

        [HttpGet("GetProducts")]
        public object GetProducts(
            string keywords = "", /* 搜索关键字 */
            long cid = 0,  /* 分类ID */
            int pageNo = 1, /*页码*/
            int pageSize = 10/*每页显示数据量*/
            )
        {
            CheckUserLogin();
            ProductSearch model = new ProductSearch()
            {
                shopId = CurrentShopBranch.ShopId,
                Keyword = keywords,
                PageNumber = pageNo,
                PageSize = pageSize,
                CategoryId = cid,
                shopBranchId = CurrentShopBranch.Id,
                HasLadderProduct = true
            };
            model.AttrIds = new List<string>() { };
            var products = ProductManagerApplication.GetProducts(model);
            var skus = ProductManagerApplication.GetSKUByProducts(products.Models.Select(p => p.Id));

            var product = products.Models.Select(item => new
            {
                id = item.Id,
                name = item.ProductName,
                price = item.MinSalePrice,
                salesCount = item.SaleCounts + Mall.Core.Helper.TypeHelper.ObjectToInt(item.VirtualSaleCounts),
                img = Core.MallIO.GetRomoteProductSizeImage(item.RelativePath, 1, (int)ImageSize.Size_100),
                stock = skus.Where(sku => sku.ProductId == item.Id).Sum(sku => (long?)sku.Stock).GetValueOrDefault(0),
                productCode = item.ProductCode,
                isSaleCountOnOff = SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1
            }).ToList();
            var result = new
            {
                success = true,
                products = product,
                total = products.Total
            };
            return result;
        }
        /// <summary>
        /// 门店添加商品
        /// </summary>
        /// <param name="pids"></param>
        /// <returns></returns>
        /// 

        [HttpGet("GetAddProducts")]
        public object GetAddProducts(string pids)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(pids))
                return new { success = false, msg = "参数异常" };
            var ids = pids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => long.Parse(p)).ToList();
            ShopBranchApplication.AddProductSkus(ids, CurrentShopBranch.Id, CurrentShopBranch.ShopId);
            return new { success = true, msg = "添加成功" };
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
        /// 下架商品
        /// </summary>
        /// <param name="pids"></param>
        /// <returns></returns>
        /// 

        [HttpGet("GetUnSaleProduct")]
        public object GetUnSaleProduct(string pids)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(pids))
                return new { success = false, msg = "参数异常" };
            var ids = ConvertToIEnumerable(pids);
            ShopBranchApplication.UnSaleProduct(CurrentShopBranch.Id, ids);
            return new { success = true, msg = "已下架" };
        }
        /// <summary>
        /// 上架商品
        /// </summary>
        /// <param name="pids"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetOnSaleProduct")]
        public object GetOnSaleProduct(string pids)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(pids))
                return new { success = false, msg = "参数异常" };
            var ids = ConvertToIEnumerable(pids);
            if (!ShopBranchApplication.CanOnSaleProduct(ids))
            {
                return new { success = false, msg = "有不在销售状态的商品存在，不可执行上架操作" };
            }
            if (ShopBranchApplication.IsOpenLadderInProducts(ids))
            {
                return new { success = false, msg = "有商品为阶梯批发商品，不能上架到门店" };
            }
            ShopBranchApplication.OnSaleProduct(CurrentShopBranch.Id, ids);
            return new { success = true, msg = "已上架" };
        }
        /// <summary>
        /// 设置商品库存
        /// </summary>
        /// <param name="pids"></param>
        /// <param name="stock"></param>
        /// <param name="optype"></param>
        /// <returns></returns>
        [HttpPost("PostSetProductStock")]
        public object PostSetProductStock(SetProductStockModel model)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(model.pids))
                return new { success = false, msg = "参数异常" };
            var ids = ConvertToIEnumerable(model.pids);
            var type = (CommonModel.StockOptionType)model.optype;
            ShopBranchApplication.SetProductStock(CurrentShopBranch.Id, ids, model.stock, type);
            return new { success = true, msg = "设置成功" };
        }
        /// <summary>
        /// 设置SKU库存
        /// </summary>
        /// <param name="skus"></param>
        /// <param name="stock"></param>
        /// <param name="optype"></param>
        /// <returns></returns>
        [HttpPost("PostSetSkuStock")]
        public object PostSetSkuStock(SetSkuStockModel model)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(model.skus))
                return new { success = false, msg = "参数异常" };
            var changes = new Dictionary<string, int>();
            var ids = model.skus.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var stk = model.stock.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => int.Parse(p)).ToList();
            var type = (StockOptionType)model.optype;
            for (int i = 0; i < ids.Count; i++)
                changes.Add(ids[i], stk[i]);
            ShopBranchApplication.SetSkuStock(CurrentShopBranch.Id, type, changes);
            return new { success = true, msg = "设置成功" };
        }
        /// <summary>
        /// 查询门店商品
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="cid"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="branchProductStatus"></param>
        /// <returns></returns>
        /// 

        [HttpGet("GetShopBranchProducts")]
        public object GetShopBranchProducts(
            string keywords = "", /* 搜索关键字 */
            long cid = 0,  /* 分类ID */
            int pageNo = 1, /*页码*/
            int pageSize = 10,/*每页显示数据量*/
            int branchProductStatus = 0
            )
        {
            CheckUserLogin();
            ShopBranchProductQuery query = new ShopBranchProductQuery
            {
                PageNo = pageNo,
                PageSize = pageSize,
                KeyWords = keywords,
                ShopId = CurrentShopBranch.ShopId,
                ShopBranchId = CurrentShopBranch.Id,
                ShopBranchProductStatus = (CommonModel.ShopBranchSkuStatus)branchProductStatus,
                HasLadderProduct = true
            };
            if (cid > 0)
            {
                query.ShopCategoryId = cid;
            }
            //查询商品
            var pageModel = ShopBranchApplication.GetShopBranchProducts(query);

            //查询门店SKU库存
            

            var allSKU = ProductManagerApplication.GetSKUByProducts(pageModel.Models.Select(p => p.Id));
            List<string> skuids = allSKU.Select(p => p.Id).ToList();
            var shopBranchSkus = ShopBranchApplication.GetSkusByIds(CurrentShopBranch.Id, skuids);
            
            var product = pageModel.Models.Select(item =>
            {
                return new
                {
                    id = item.Id,
                    name = item.ProductName,
                    price = item.MinSalePrice,
                    salesCount = item.SaleCounts,
                    img = Core.MallIO.GetRomoteProductSizeImage(item.RelativePath, 1, (int)Mall.CommonModel.ImageSize.Size_350),
                    stock = shopBranchSkus.Where(e => e.ProductId == item.Id).Sum(s => s.Stock),
                    productCode = item.ProductCode
                };
            });
            var result = new
            {
                success = true,
                products = product,
                skus = shopBranchSkus,
                total = pageModel.Total
            };
            return result;
        }
        /// <summary>
        /// 取商品SKU
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetProductSku")]
        public object GetProductSku(long pid)
        {
            CheckUserLogin();
            var sku = ShopBranchApplication.GetSkusByProductId(CurrentUser.ShopBranchId, pid);
            return new { success = true, sku = sku };
        }
    }
}
