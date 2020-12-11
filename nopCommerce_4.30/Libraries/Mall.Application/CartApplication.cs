using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.Core;
using Mall.IServices;
using Mall.Entities;
using Mall.CommonModel;
using Mall.DTO;
using Mall.Core.Helper;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    public class CartApplication : BaseApplicaion<ICartService>
    {
        //private static IBranchCartService _iBranchCartService =  EngineContext.Current.Resolve<IBranchCartService>();

        private static IBranchCartService _iBranchCartService =  EngineContext.Current.Resolve<IBranchCartService>();
        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <param name="skuId">商品SKU Id</param>
        /// <param name="count">商品数量</param>
        /// <param name="memberId">会员id</param>
        public static void AddToCart(string skuId, int count, long memberId)
        {
            Service.AddToCart(skuId, count, memberId);
        }

        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <param name="cartItems">购物车商品项</param>
        /// <param name="memberId">会员Id</param>
        public static void AddToCart(IEnumerable<ShoppingCartItem> cartItems, long memberId)
        {
            Service.AddToCart(cartItems, memberId);
        }

        /// <summary>
        /// 更新购物车
        /// </summary>
        /// <param name="skuId">商品SKU Id</param>
        /// <param name="count">商品数量</param>
        /// <param name="memberId">会员id</param>
        public static void UpdateCart(string skuId, int count, long memberId)
        {
            Service.UpdateCart(skuId, count, memberId);
        }

        /// <summary>
        /// 清空指定会员的购物车
        /// </summary>
        /// <param name="memeberId">会员id</param>
        public static void ClearCart(long memeberId)
        {
            Service.ClearCart(memeberId);
        }

        /// <summary>
        /// 删除指定会员购物车中的指定商品
        /// </summary>
        /// <param name="skuId">待删除的商品的skuid</param>
        /// <param name="memberId">会员id</param>
        public static void DeleteCartItem(string skuId, long memberId)
        {
            Service.DeleteCartItem(skuId, memberId);
        }


        /// <summary>
        /// 删除指定会员购物车中的指定商品
        /// </summary>
        /// <param name="skuIds">待删除的商品的skuid</param>
        /// <param name="memberId">会员id</param>
        public static void DeleteCartItem(IEnumerable<string> skuIds, long memberId)
        {
            Service.DeleteCartItem(skuIds, memberId);
        }

        /// <summary>
        /// 获取指定会员购物车信息
        /// </summary>
        /// <param name="memeberId">会员id</param>
        /// <returns></returns>
        public static ShoppingCartInfo GetCart(long memeberId)
        {
            return Service.GetCart(memeberId);
        }
        /// <summary>
        /// 获取购物车购物项
        /// </summary>
        /// <param name="cartItemIds">购物车项Id</param>
        /// <returns></returns>
        public static List<ShoppingCartItem> GetCartItems(IEnumerable<long> cartItemIds)
        {
            return Service.GetCartItems(cartItemIds);
        }

        /// <summary>
        /// 获取购物车购物项
        /// </summary>
        /// <param name="skuIds">SKUId</param>
        /// <returns></returns>
        public static List<ShoppingCartItem> GetCartItems(IEnumerable<string> skuIds, long memberId)
        {
            return Service.GetCartItems(skuIds, memberId);
        }

        public static List<ShoppingCartItem> GetCartQuantityByIds(long memberId, IEnumerable<long> productIds)
        {
            var shopcart = Service.GetCartQuantityByIds(memberId, productIds).ToList();
            //return AutoMapper.Mapper.Map<List<ShoppingCartItem>>(shopcart);

            return shopcart;
        
        }


        #region 门店购物车
        /// <summary>
        /// 更新购物车
        /// </summary>
        /// <param name="skuId">商品SKU Id</param>
        /// <param name="count">商品数量</param>
        /// <param name="memberId">会员id</param>
        /// <param name="shopbranchId">门店编号</param>
        public static void UpdateShopBranchCart(string skuId, int count, long memberId, long shopbranchId)
        {
            _iBranchCartService.UpdateCart(skuId, count, memberId, shopbranchId);
        }
        /// <summary>
        /// 取会员门店购物车项
        /// </summary>
        /// <param name="memeberId"></param>
        /// <param name="shopbranchId">门店编号,0取所有</param>
        /// <returns></returns>
        public static ShoppingCartInfo GetShopBranchCart(long memberId, long shopbranchId = 0)
        {
            return _iBranchCartService.GetCart(memberId, shopbranchId);
        }
        /// <summary>
        /// 取会员门店购物车项(无缓存)
        /// </summary>
        /// <param name="memeberId"></param>
        /// <param name="shopbranchId">门店编号,0取所有</param>
        /// <returns></returns>
        public static ShoppingCartInfo GetShopBranchCartNoCache(long memberId, long shopbranchId = 0)
        {
            return _iBranchCartService.GetCartNoCache(memberId, shopbranchId);
        }
        /// <summary>
        /// 删除门店购物车某项
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="memberId"></param>
        /// <param name="shopbranchId"></param>
        public static void DeleteShopBranchCartItem(string skuId, long memberId, long shopbranchId)
        {
            _iBranchCartService.DeleteCartItem(skuId, memberId, shopbranchId);
        }
        /// <summary>
        /// 删除门店购物车多项
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="memberId"></param>
        /// <param name="shopbranchId"></param>
        public static void DeleteShopBranchCartItem(IEnumerable<string> skuIds, long memberId, long shopbranchId)
        {
            _iBranchCartService.DeleteCartItem(skuIds, memberId, shopbranchId);
        }
        /// <summary>
        /// 清理门店购物车
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="shopbranchId"></param>
        public static void ClearShopBranchCart(long memberId, long shopbranchId)
        {
            _iBranchCartService.ClearCart(memberId, shopbranchId);
        }
        /// <summary>
        /// 清理门店购物车
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="shopbranchId"></param>
        public static void ClearInvalidShopBranchCart(long memberId, long shopbranchId)
        {
            _iBranchCartService.ClearInvalidCart(memberId, shopbranchId);
        }
        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <param name="cartItems">购物车商品项</param>
        /// <param name="memberId">会员Id</param>
        public static void AddToShopBranchCart(IEnumerable<ShoppingCartItem> cartItems, long memberId)
        {
            _iBranchCartService.AddToCart(cartItems, memberId);
        }
        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <param name="cartItems">购物车商品项</param>
        /// <param name="memberId">会员Id</param>
        public static void AddToShopBranchCart(string skuId, int count, long memberId, long shopbranchId)
        {
            _iBranchCartService.AddToCart(skuId, count, memberId, shopbranchId);
        }
        #endregion
        public static List<ShoppingCartInfo> GetCarts(long member, int top)
        {
            return Service.GetCarts(member, top);
        }

        public static int GetCartCount(long member)
        {
            return Service.GetCartCount(member);
        }


        private static ShoppingCartInfo GetShoppingCart(long member)
        {
            if (member > 0)//已经登录，系统从服务器读取购物车信息，否则从Cookie获取购物车信息
                return Service.GetCart(member);

            var shoppingCartInfo = new ShoppingCartInfo();

            string carts = WebHelper.GetCookie("Mall-CART");
            if (!string.IsNullOrWhiteSpace(carts))
            {
                shoppingCartInfo.Items = carts.Split(',').Select(item => {
                    var cartItemParts = item.Split(':');
                    return new ShoppingCartItem
                    {
                        ProductId = long.Parse(cartItemParts[0].Split('_')[0]),
                        SkuId = cartItemParts[0],
                        Quantity = int.Parse(cartItemParts[1])
                    };
                }).ToList();
           
            }
            return shoppingCartInfo;

        }

        public static List<CartItem> GetCartItems(long memberId)
        {
            var cart = GetShoppingCart(memberId);
            //会员折扣
            var discount = MemberApplication.GetMemberDiscount(memberId);

            var skuIds = cart.Items.Select(p => p.SkuId).ToList();
            var skus = GetService<IProductService>().GetSKUs(skuIds);
            var products = GetService<IProductService>().GetProducts(skus.Select(p => p.ProductId).ToList());
            var shops = GetService<IShopService>().GetShops(products.Select(p => p.ShopId).ToList());
            var types = TypeApplication.GetTypes();
            //限时购活动
            var limitProducts = LimitTimeApplication.GetPriceByProducrIds(products.Select(p => p.Id).ToList());
            //阶梯价商品
            var allLadders = GetService<IProductLadderPriceService>().GetLadderPricesByProductIds(products.Where(p => p.IsOpenLadder).Select(p => p.Id).ToList());

            var result = new List<CartItem>();
            foreach (var item in cart.Items) {
                var sku = skus.FirstOrDefault(p => p.Id == item.SkuId);
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null) continue;//忽略异常数据
                var shop = shops.FirstOrDefault(p => p.Id == product.ShopId);
                var type = types.FirstOrDefault(p => p.Id == product.TypeId);
                if (sku == null || shop == null) continue;//忽略异常数据

                #region 价格覆盖
                var minMach = 0;
                //阶梯价格
                if (product.IsOpenLadder)
                {
                    var ladders = allLadders.Where(p => p.ProductId == item.ProductId).ToList();
                    var quantity = cart.Items.Where(c => c.ProductId == item.ProductId && c.ShopBranchId==0).ToList().Sum(c => c.Quantity);
                    sku.SalePrice = ProductManagerApplication.GetProductLadderPrice(ladders, quantity);
                    minMach = ladders.Min(p => p.MinBath);
                }

                var price = -1M;//其他价格
                //限时购价格
                var limit = limitProducts.FirstOrDefault(p => p.ProductId == item.ProductId);
                if (limit != null)
                    price = limit.MinPrice;
                //会员折扣
                if (price < 0 && shop.IsSelf)
                    price = sku.SalePrice * discount;

                if (price >= 0)
                    sku.SalePrice = price;
                #endregion

                //填充SKU别名
                ProductManagerApplication.FillSkuAlias(sku, product, type);
                //是否限时购
                var isLimit = limit != null;



                result.Add(new CartItem
                {
                    ItemId = item.Id,
                    Quantity = item.Quantity,
                    AddTime = item.AddTime,
                    Product = product,
                    Shop = shop,
                    Sku = sku,
                    ShowStatus = isLimit ? 1 : ProductManagerApplication.GetProductShowStatus(product, sku.Map<DTO.SKU>(), 1),
                     LimitId = isLimit?limit.Id:0,//参与限时购活动ID
                    IsLimit = isLimit, //是否参加限时购
                    MinMach = minMach,
                });
            }
            return result;
        }
    }
}
