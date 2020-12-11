using Mall.API.Model.ParamsModel;
using Mall.Application;
using Mall.DTO.QueryModel;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Mall.API
{
    public class ShopProductController : BaseShopLoginedApiController
    {

        private List<long> ConvertToIEnumerable(string str, char sp = ',')
        {
            return str.Split(sp).Select(e =>
            {
                long id = 0;
                if (!long.TryParse(e, out id))
                {
                    id = 0;
                }
                return id;
            }).ToList();
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
            ProductManagerApplication.BatchSaleOff(ids, CurrentShop.Id);
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
            ProductManagerApplication.BatchOnSale(ids, CurrentShop.Id);
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
            ProductManagerApplication.SetProductStock(ids, model.stock, type);
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

            var ids = model.skus.Split(',').ToList();
            var stk = model.stock.Split(',').Select(e =>
            {
                int id = 0;
                if (!int.TryParse(e, out id))
                {
                    id = 0;
                }
                return id;
            }).ToList();
            var changes = new Dictionary<string, long>();
            for(var i =0;i<ids.Count;i++)
            {
                changes.Add(ids[i], stk[i]);
            }
            var type = (CommonModel.StockOptionType)model.optype;
            ProductManagerApplication.SetSkuStock(type, changes);
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

        [HttpGet("GetShopProducts")]
        public object GetShopProducts(
            string status,/* 销售中0, 仓库中1， 待审核2，违规下架3 */
            string keywords = "", /* 搜索关键字 */
            long cid = 0,  /* 分类ID */
            int pageNo = 1, /*页码*/
            int pageSize = 10/*每页显示数据量*/
            )
        {
            CheckUserLogin();
            ProductQuery query = new ProductQuery
            {
                PageNo = pageNo,
                PageSize = pageSize,
                KeyWords = keywords,
                ShopId = CurrentShop.Id
            };
            if (!string.IsNullOrWhiteSpace(status))
            {
                switch(status)
                {
                    case "0":
                        query.SaleStatus = Entities.ProductInfo.ProductSaleStatus.OnSale;
                        query.AuditStatus = new Entities.ProductInfo.ProductAuditStatus[] { Entities.ProductInfo.ProductAuditStatus.Audited };
                        break;
                    case "1":
                        query.SaleStatus = Entities.ProductInfo.ProductSaleStatus.InStock;
                        query.AuditStatus = new Entities.ProductInfo.ProductAuditStatus[] { Entities.ProductInfo.ProductAuditStatus.Audited, Entities.ProductInfo.ProductAuditStatus.UnAudit, Entities.ProductInfo.ProductAuditStatus.WaitForAuditing };
                        break;
                    case "2":
                        query.SaleStatus = Entities.ProductInfo.ProductSaleStatus.OnSale;
                        query.AuditStatus = new Entities.ProductInfo.ProductAuditStatus[] { Entities.ProductInfo.ProductAuditStatus.WaitForAuditing };
                        break;
                    case "3":
                        query.AuditStatus = new Entities.ProductInfo.ProductAuditStatus[] { Entities.ProductInfo.ProductAuditStatus.InfractionSaleOff };
                        break;
                }
            }

            if (cid > 0)
            {
                query.ShopCategoryId = cid;
            }
            //查询商品
            var products = ProductManagerApplication.GetProducts(query);
            var pids = products.Models.Select(e => e.Id);
            var skus = ProductManagerApplication.GetSKUByProducts(pids);
            //查询门店SKU库存
            var list = products.Models.Select(item =>
            {
                return new
                {
                    id = item.Id,
                    name = item.ProductName,
                    price = item.MinSalePrice,
                    salesCount = item.SaleCounts,
                    img = Core.MallIO.GetRomoteProductSizeImage(item.RelativePath, 1, (int)CommonModel.ImageSize.Size_350),
                    stock = skus.Where(e => e.ProductId == item.Id).Sum(e => e.Stock),
                    productCode=item.ProductCode
                };
            });
            var result = new
            {
                success = true,
                products = list,
                total = products.Total,
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
            var sku = ProductManagerApplication.GetSKUs(pid);
            return new { success = true, sku = sku };
        }
    }
}
