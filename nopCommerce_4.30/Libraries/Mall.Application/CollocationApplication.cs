using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.Core;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.CommonModel;
using Mall.Entities;
using Mall.DTO;

namespace Mall.Application
{
    public class CollocationApplication:BaseApplicaion<ICollocationService>
    {

        /// <summary>
        /// 根据组合商品获取组合SKU信息
        /// </summary>
        /// <param name="colloPid"></param>
        /// <param name="skuid"></param>
        /// <returns></returns>
        public static CollocationSkuInfo GetColloSku(long colloPid, string skuid)
        {
            return Service.GetColloSku(colloPid, skuid);
        }

        //获取一个商品的组合购SKU信息
        public static List<CollocationSkuInfo> GetProductColloSKU(long productid, long colloPid)
        {
            return Service.GetProductColloSKU(productid, colloPid);
        }
        public static string GetChineseNumber(int number)
        {
            string numberStr = NumberToChinese(number);
            string firstNumber = string.Empty;
            string lastNumber = string.Empty;
            string str = number.ToString();
            firstNumber = str.Substring(0, 1);
            lastNumber = str.Substring(str.Length - 1);
            if (str.Length > 1 && lastNumber == "0")
            {
                numberStr = numberStr.Substring(0, numberStr.Length - 1);
            }
            if (str.Length == 2 && firstNumber == "1")
            {
                numberStr = numberStr.Substring(1);
            }

            return numberStr;
        }
        /// <summary>
        /// 数字转中文
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string NumberToChinese(int number)
        {
            string res = string.Empty;
            string str = number.ToString();
            string schar = str.Substring(0, 1);
            switch (schar)
            {
                case "1":
                    res = "一";
                    break;
                case "2":
                    res = "二";
                    break;
                case "3":
                    res = "三";
                    break;
                case "4":
                    res = "四";
                    break;
                case "5":
                    res = "五";
                    break;
                case "6":
                    res = "六";
                    break;
                case "7":
                    res = "七";
                    break;
                case "8":
                    res = "八";
                    break;
                case "9":
                    res = "九";
                    break;
                default:
                    res = "零";
                    break;
            }
            if (str.Length > 1)
            {
                switch (str.Length)
                {
                    case 2:
                    case 6:
                        res += "十";
                        break;
                    case 3:
                    case 7:
                        res += "百";
                        break;
                    case 4:
                        res += "千";
                        break;
                    case 5:
                        res += "万";
                        break;
                    default:
                        res += "";
                        break;
                }
                res += NumberToChinese(int.Parse(str.Substring(1, str.Length - 1)));
            }
            return res;
        }

        public static List<CollocationPoruductInfo> GetCollocationListByProductId(long productId)
        {
            return Service.GetCollocationListByProductId(productId);
        }

        /// <summary>
        /// 获取当前商品可参与的组合购
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public static int GetCollocationCount(long product)
        {
            var colls = Service.GetAvailableCollocationByProduct(product);
            var collProducts = Service.GetProducts(colls.Select(p => p.Id).ToList());
            var products = ProductManagerApplication.GetOnSaleProducts(collProducts.Select(p => p.ProductId).ToList());
            int result = 0;
            foreach (var coll in colls)
            {
                var cProduct = collProducts.Where(p => p.ColloId == coll.Id);
                var mainProduct = cProduct.FirstOrDefault(p => p.IsMain);
                if (products.Any(p => p.Id == mainProduct.ProductId))
                    result++;
            }
            return result;
        }


        public static List<ProductCollocationModel> GetDisplayCollocation(long productID)
        {
            var result = new List<ProductCollocationModel>();

            var collocations = Service.GetAvailableCollocationByProduct(productID);
            if (collocations.Count == 0) return result;

            var collProducts = Service.GetProducts(collocations.Select(p => p.Id).ToList());
            var collSKUs = Service.GetSKUs(collProducts.Select(p => p.Id).ToList());
            var allProducts = ProductManagerApplication.GetOnSaleProducts(collProducts.Select(p => p.ProductId).ToList());
            var allSKUs = ProductManagerApplication.GetSKUByProducts(allProducts.Select(p => p.Id).ToList());

            //移除不在销售中的活动商品
            collProducts = collProducts.Where(p => allProducts.Select(o => o.Id).Contains(p.ProductId)).ToList();
            var index = 0;
            foreach (var collocation in collocations)
            {
                var cProducts = collProducts.Where(p => p.ColloId == collocation.Id);
                var mainProduct = cProducts.FirstOrDefault(p => p.IsMain);
                if (mainProduct==null) continue;//主商品不在销售中
                index++;
                var item = new ProductCollocationModel();
                item.Id = item.Id;
                item.Name = "组合购" + GetChineseNumber(index);
                item.ProductId = mainProduct.ProductId;
                item.ShopId = collocation.ShopId;
                item.Products = cProducts.Select(cp =>
                 {
                     var product = allProducts.FirstOrDefault(p => p.Id == cp.ProductId);
                     var skus = allSKUs.Where(p => p.ProductId == cp.ProductId);
                     var cSKUs = collSKUs.Where(p => p.ColloProductId == cp.Id);
                     return new CollocationProducts()
                     {
                         DisplaySequence = cp.DisplaySequence,
                         IsMain = cp.IsMain,
                         Stock = skus.Sum(t => t.Stock),
                         MaxCollPrice = cSKUs.Max(x => x.Price),
                         MaxSalePrice = cSKUs.Max(x => x.SkuPirce),
                         MinCollPrice = cSKUs.Min(x => x.Price),
                         MinSalePrice = cSKUs.Min(x => x.SkuPirce),
                         ProductName = product.ProductName,
                         ProductId = cp.ProductId,
                         ColloPid = cp.Id,
                         Image = Core.MallIO.GetImagePath(product.RelativePath)
                     };
                 }).ToList();
                item.Cheap = item.Products.Sum(a => a.MaxSalePrice) - item.Products.Sum(a => a.MinCollPrice);
                result.Add(item);
            }
            return result;
        }
    }
}
