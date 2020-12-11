using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Web.Framework
{
    public class CartHelper
    {
        /*
         *购物车存储说明：
         *游客访问时，点击加入购物车，购物车信息保存至Cookie中，游客点击结算时，Cookie中的购物车信息转移至数据库中并清空Cookie中购物车信息。
         *登录会员点击加入购物车时，购物车信息保存至数据库中。
         *Cookie存储格式： skuId1:count1,skuId2:count2,.....
         */

        /// <summary>
        /// 同步客户端购物车信息至服务器
        /// </summary>
        public void UpdateCartInfoFromCookieToServer(long memberId)
        {
            string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART);
            if (!string.IsNullOrWhiteSpace(cartInfo))
            {
                string[] cartItems = cartInfo.Split(',');
                var shoppingCartItems = new List<Mall.Entities.ShoppingCartItem>();
                foreach (string cartItem in cartItems)
                {
                    var cartItemParts = cartItem.Split(':');
                    shoppingCartItems.Add(new Mall.Entities.ShoppingCartItem() { SkuId = cartItemParts[0], Quantity = int.Parse(cartItemParts[1]) });
                }
                CartApplication.AddToCart(shoppingCartItems, memberId);
            }
            WebHelper.DeleteCookie(CookieKeysCollection.Mall_CART);
        }

        /// <summary>
        /// 获取购物车中的商品
        /// </summary>
        /// <returns></returns>
        public IEnumerable<long> GetCartProductIds(long memberId)
        {
            long[] productIds = new long[] { };
            if (memberId > 0)//已经登录，系统从服务器读取购物车信息，否则从Cookie获取购物车信息
            {
                var cartInfo = CartApplication.GetCart(memberId);
                productIds = cartInfo.Items.Select(item => item.ProductId).ToArray();
            }
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    productIds = new long[cartItems.Length];
                    int i = 0;
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        productIds[i++] = long.Parse(cartItemParts[0]);//获取商品Id
                    }
                }
            }
            return productIds;
        }


        /// <summary>
        /// 获取购物车中的商品
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetCartProductSKUIds(long memberId)
        {
            string[] productIds = new string[] { };
            if (memberId > 0)//已经登录，系统从服务器读取购物车信息，否则从Cookie获取购物车信息
            {
                var cartInfo = CartApplication.GetCart(memberId);
                productIds = cartInfo.Items.Select(item => item.SkuId).ToArray();
            }
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    productIds = new string[cartItems.Length];
                    int i = 0;
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        productIds[i++] = cartItemParts[0];//获取商品SKUId
                    }
                }
            }
            return productIds;
        }

        /// <summary>
        /// 获取购物车中的商品
        /// </summary>
        /// <returns></returns>
        public Mall.Entities.ShoppingCartInfo GetCart(long memberId)
        {
            Mall.Entities.ShoppingCartInfo shoppingCartInfo;
            if (memberId > 0)//已经登录，系统从服务器读取购物车信息，否则从Cookie获取购物车信息
                shoppingCartInfo = CartApplication.GetCart(memberId);
            else
            {
                shoppingCartInfo = new Mall.Entities.ShoppingCartInfo();

                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    var cartInfoItems = new List<Mall.Entities.ShoppingCartItem>();
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        cartInfoItems.Add( new Mall.Entities.ShoppingCartItem() { ProductId = long.Parse(cartItemParts[0].Split('_')[0]), SkuId = cartItemParts[0], Quantity = int.Parse(cartItemParts[1]) });
                    }
                    shoppingCartInfo.Items = cartInfoItems;
                }
            }
            return shoppingCartInfo;
        }


        public void RemoveFromCart(string skuId, long memberId)
        {
            if (memberId > 0)
                CartApplication.DeleteCartItem(skuId, memberId);
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    string newCartInfo = string.Empty;
                    foreach (string cartItem in cartItems)
                    {
                        string[] cartItemParts = cartItem.Split(':');
                        string cartItemSkuId = cartItemParts[0];
                        if (cartItemSkuId != skuId)
                            newCartInfo += "," + cartItem;
                    }
                    if (!string.IsNullOrWhiteSpace(newCartInfo))
                        newCartInfo = newCartInfo.Substring(1);
                    WebHelper.SetCookie(CookieKeysCollection.Mall_CART, newCartInfo);
                }
            }
        }

        public void RemoveFromCart(IEnumerable<string> skuIds, long memberId)
        {
            if (memberId > 0)
                CartApplication.DeleteCartItem(skuIds, memberId);
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    string newCartInfo = string.Empty;
                    foreach (string cartItem in cartItems)
                    {
                        string[] cartItemParts = cartItem.Split(':');
                        string cartItemSkuId = cartItemParts[0];
                        if (!skuIds.Contains(cartItemSkuId))
                            newCartInfo += "," + cartItem;
                    }
                    if (!string.IsNullOrWhiteSpace(newCartInfo))
                        newCartInfo = newCartInfo.Substring(1);
                    WebHelper.SetCookie(CookieKeysCollection.Mall_CART, newCartInfo);
                }
            }
        }



        public void UpdateCartItem(string skuId, int count, long memberId)
        {
            if (memberId > 0)
                CartApplication.UpdateCart(skuId, count, memberId);
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    string newCartInfo = string.Empty;
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        if (cartItemParts[0] == skuId)
                            newCartInfo += "," + skuId + ":" + count;
                        else
                            newCartInfo += "," + cartItem;
                    }

                    if (!string.IsNullOrWhiteSpace(newCartInfo))
                        newCartInfo = newCartInfo.Substring(1);
                    WebHelper.SetCookie(CookieKeysCollection.Mall_CART, newCartInfo);
                }
                else
                {
                    WebHelper.SetCookie(CookieKeysCollection.Mall_CART, string.Format("{0}:{1}", skuId, count));
                }
            }
        }


        public void AddToCart(string skuId, int count, long memberId)
        {
            CheckSkuIdIsValid(skuId);
            //判断库存
            var sku = ProductManagerApplication.GetSKU(skuId);
            if (sku == null)
            {
                throw new MallException("错误的SKU");
            }
            if (count > sku.Stock)
            {
                throw new MallException("库存不足");
            }
            #region 商品限购判断
            var prouctInfo = ProductManagerApplication.GetProduct(sku.ProductId);
            if (prouctInfo != null && prouctInfo.MaxBuyCount > 0 && !prouctInfo.IsOpenLadder)//商品有限购数量
            {
                var carInfo = CartApplication.GetCart(memberId);
                if (carInfo != null)
                {
                    var quantity = carInfo.Items.Where(p => p.ProductId == sku.ProductId).Sum(d => d.Quantity);//当前用户该商品已加入购物车数量
                    if (count + quantity > prouctInfo.MaxBuyCount)//已有数量+新数量
                        throw new MallException(string.Format("每个ID限购{0}件", prouctInfo.MaxBuyCount));
                }
            }
            #endregion

            if (memberId > 0)
                CartApplication.AddToCart(skuId, count, memberId);
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    string newCartInfo = string.Empty;
                    bool exist = false;
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        if (cartItemParts[0] == skuId)
                        {
                            newCartInfo += "," + skuId + ":" + (int.Parse(cartItemParts[1]) + count);
                            exist = true;
                        }
                        else
                            newCartInfo += "," + cartItem;
                    }
                    if (!exist)
                        newCartInfo += "," + skuId + ":" + count;

                    if (!string.IsNullOrWhiteSpace(newCartInfo))
                        newCartInfo = newCartInfo.Substring(1);
                    WebHelper.SetCookie(CookieKeysCollection.Mall_CART, newCartInfo);
                }
                else
                {
                    WebHelper.SetCookie(CookieKeysCollection.Mall_CART, string.Format("{0}:{1}", skuId, count));
                }
            }
        }

        public bool CheckSkuId(string skuId)
        {
            bool status = true;
            long productId = 0;
            long.TryParse(skuId.Split('_')[0], out productId);
            if (productId == 0)
                status = false;
            var skuItem = ProductManagerApplication.GetSKU(skuId);
            if (skuItem == null)
                status = false;
            var sku = ProductManagerApplication.GetSKU(skuId);
            if (sku == null)
            {
                status = false;
            }
            return status;
        }

        void CheckSkuIdIsValid(string skuId)
        {
            long productId = 0;
            long.TryParse(skuId.Split('_')[0], out productId);
            if (productId == 0)
                throw new Mall.Core.InvalidPropertyException("SKUId无效");

            var skuItem = ProductManagerApplication.GetSKU(skuId);
            if (skuItem == null)
                throw new Mall.Core.InvalidPropertyException("SKUId无效");

        }

    }


    public class BranchCartHelper
    {
        /*
         *购物车存储说明：
         *游客访问时，点击加入购物车，购物车信息保存至Cookie中，游客点击结算时，Cookie中的购物车信息转移至数据库中并清空Cookie中购物车信息。
         *登录会员点击加入购物车时，购物车信息保存至数据库中。
         *Cookie存储格式： skuId1:count1,skuId2:count2,.....
         */

        /// <summary>
        /// 同步客户端购物车信息至服务器
        /// </summary>
        public void UpdateCartInfoFromCookieToServer(long memberId)
        {
            string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART_BRANCH);
            if (!string.IsNullOrWhiteSpace(cartInfo))
            {
                string[] cartItems = cartInfo.Split(',');
                var shoppingCartItems = new List<Mall.Entities.ShoppingCartItem>();
                foreach (string cartItem in cartItems)
                {
                    var cartItemParts = cartItem.Split(':');
                    var skuId = cartItemParts[0];
                    int quantity = int.Parse(cartItemParts[1]);
                    long shopBranchId = long.Parse(cartItemParts[2]);
                    shoppingCartItems.Add(new Mall.Entities.ShoppingCartItem() { ShopBranchId = shopBranchId, SkuId = cartItemParts[0], Quantity = int.Parse(cartItemParts[1]) });
                }
                CartApplication.AddToShopBranchCart(shoppingCartItems, memberId);
            }
            WebHelper.DeleteCookie(CookieKeysCollection.Mall_CART_BRANCH);
        }

        /// <summary>
        /// 获取购物车中的商品
        /// </summary>
        /// <returns></returns>
        public IEnumerable<long> GetCartProductIds(long memberId, long shopBranchId)
        {
            long[] productIds = new long[] { };
            if (memberId > 0)//已经登录，系统从服务器读取购物车信息，否则从Cookie获取购物车信息
            {
                var cartInfo = CartApplication.GetShopBranchCart(memberId, shopBranchId);
                productIds = cartInfo.Items.Select(item => item.ProductId).ToArray();
            }
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART_BRANCH);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    productIds = new long[cartItems.Length];
                    int i = 0;
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        productIds[i++] = long.Parse(cartItemParts[0]);//获取商品Id
                    }
                }
            }
            return productIds;
        }


        /// <summary>
        /// 获取购物车中的商品
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetCartProductSKUIds(long memberId, long shopBranchId)
        {
            string[] productIds = new string[] { };
            if (memberId > 0)//已经登录，系统从服务器读取购物车信息，否则从Cookie获取购物车信息
            {
                var cartInfo = CartApplication.GetShopBranchCart(memberId, shopBranchId);
                productIds = cartInfo.Items.Select(item => item.SkuId).ToArray();
            }
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART_BRANCH);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    productIds = new string[cartItems.Length];
                    int i = 0;
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        productIds[i++] = cartItemParts[0];//获取商品SKUId
                    }
                }
            }
            return productIds;
        }

        /// <summary>
        /// 获取购物车中的商品
        /// </summary>
        /// <returns></returns>
        public Mall.Entities.ShoppingCartInfo GetCart(long memberId, long shopBranchId)
        {
            Mall.Entities.ShoppingCartInfo shoppingCartInfo;
            if (memberId > 0)//已经登录，系统从服务器读取购物车信息，否则从Cookie获取购物车信息
                shoppingCartInfo = CartApplication.GetShopBranchCart(memberId, shopBranchId);
            else
            {
                shoppingCartInfo = new Mall.Entities.ShoppingCartInfo();

                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART_BRANCH);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    var cartInfoItems = new List<Mall.Entities.ShoppingCartItem>();
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        if (shopBranchId == 0 || cartItemParts[2] == shopBranchId.ToString())
                        {
                            cartInfoItems.Add(new Mall.Entities.ShoppingCartItem() { ProductId = long.Parse(cartItemParts[0].Split('_')[0]), SkuId = cartItemParts[0], Quantity = int.Parse(cartItemParts[1]), ShopBranchId = long.Parse(cartItemParts[2]) });
                        }
                    }
                    shoppingCartInfo.Items = cartInfoItems;
                }
            }
            return shoppingCartInfo;
        }

        public Mall.Entities.ShoppingCartInfo GetCartNoCache(long memberId, long shopBranchId)
        {
            Mall.Entities.ShoppingCartInfo shoppingCartInfo;
            if (memberId > 0)//已经登录，系统从服务器读取购物车信息，否则从Cookie获取购物车信息
                shoppingCartInfo = CartApplication.GetShopBranchCartNoCache(memberId, shopBranchId);
            else
            {
                shoppingCartInfo = new Mall.Entities.ShoppingCartInfo();

                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART_BRANCH);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    var cartInfoItems = new List<Mall.Entities.ShoppingCartItem>();
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        if (shopBranchId == 0 || cartItemParts[2] == shopBranchId.ToString())
                            cartInfoItems.Add(new Mall.Entities.ShoppingCartItem() { ProductId = long.Parse(cartItemParts[0].Split('_')[0]), SkuId = cartItemParts[0], Quantity = int.Parse(cartItemParts[1]), ShopBranchId = long.Parse(cartItemParts[2]) });
                    }
                    shoppingCartInfo.Items = cartInfoItems;
                }
            }
            return shoppingCartInfo;
        }

        public void RemoveFromCart(string skuId, long memberId, long shopBranchId)
        {
            if (memberId > 0)
                CartApplication.DeleteShopBranchCartItem(skuId, memberId, shopBranchId);
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART_BRANCH);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    string newCartInfo = string.Empty;
                    foreach (string cartItem in cartItems)
                    {
                        string[] cartItemParts = cartItem.Split(':');
                        string cartItemSkuId = cartItemParts[0];
                        string cartItemshopBranchId = cartItemParts[2];
                        if (cartItemSkuId != skuId && shopBranchId.ToString() != cartItemshopBranchId)
                            newCartInfo += "," + cartItem;
                    }
                    if (!string.IsNullOrWhiteSpace(newCartInfo))
                        newCartInfo = newCartInfo.Substring(1);
                    WebHelper.SetCookie(CookieKeysCollection.Mall_CART_BRANCH, newCartInfo);
                }
            }
        }

        public void RemoveFromCart(IEnumerable<string> skuIds, long memberId, long shopBranchId)
        {
            if (memberId > 0)
                CartApplication.DeleteShopBranchCartItem(skuIds, memberId, shopBranchId);
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART_BRANCH);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    string newCartInfo = string.Empty;
                    foreach (string cartItem in cartItems)
                    {
                        string[] cartItemParts = cartItem.Split(':');
                        string cartItemSkuId = cartItemParts[0];
                        string cartItemshopBranchId = cartItemParts[2];
                        if (!skuIds.Contains(cartItemSkuId) && shopBranchId.ToString() != cartItemshopBranchId)
                            newCartInfo += "," + cartItem;
                    }
                    if (!string.IsNullOrWhiteSpace(newCartInfo))
                        newCartInfo = newCartInfo.Substring(1);
                    WebHelper.SetCookie(CookieKeysCollection.Mall_CART_BRANCH, newCartInfo);
                }
            }
        }



        public void UpdateCartItem(string skuId, int count, long memberId, long shopBranchId)
        {
            CheckSkuIdIsValid(skuId, shopBranchId);
            //判断库存
            var sku = ProductManagerApplication.GetSKU(skuId);
            if (sku == null)
            {
                throw new MallException("错误的SKU");
            }
            //if (count > sku.Stock)
            //{
            //    throw new MallException("库存不足");
            //}
            var shopBranch = ShopBranchApplication.GetShopBranchById(shopBranchId);
            if (shopBranch == null)
            {
                throw new MallException("错误的门店id");
            }
            var shopBranchSkuList = ShopBranchApplication.GetSkusByIds(shopBranchId, new List<string> { skuId });
            if (shopBranchSkuList == null || shopBranchSkuList.Count == 0)
            {
                throw new MallException("门店没有该商品");
            }
            if (shopBranchSkuList[0].Status == ShopBranchSkuStatus.InStock)
            {
                throw new MallException("此商品已下架");
            }
            var sbsku = shopBranchSkuList.FirstOrDefault();
            if (sbsku.Stock < count)
            {
                throw new MallException("门店库存不足");
            }

            if (memberId > 0)
                CartApplication.UpdateShopBranchCart(skuId, count, memberId, shopBranchId);
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART_BRANCH);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    string newCartInfo = string.Empty;
                    bool exist = false;
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        if (cartItemParts[0] == skuId && cartItemParts[2] == shopBranchId.ToString())
                        {
                            newCartInfo += "," + skuId + ":" + count + ":" + shopBranchId;
                            exist = true;
                        }
                        else
                            newCartInfo += "," + cartItem;
                    }
                    if (!exist)
                        newCartInfo += "," + skuId + ":" + count + ":" + shopBranchId;

                    if (!string.IsNullOrWhiteSpace(newCartInfo))
                        newCartInfo = newCartInfo.Substring(1);
                    WebHelper.SetCookie(CookieKeysCollection.Mall_CART_BRANCH, newCartInfo);
                }
                else
                {
                    WebHelper.SetCookie(CookieKeysCollection.Mall_CART_BRANCH, string.Format("{0}:{1}:{2}", skuId, count, shopBranchId));
                }
            }
        }


        public void AddToCart(string skuId, int count, long memberId, long shopBranchId)
        {
            CheckSkuIdIsValid(skuId, shopBranchId);
            //判断库存
            var sku = ProductManagerApplication.GetSKU(skuId);
            if (sku == null)
            {
                throw new MallException("错误的SKU");
            }
            if (count > sku.Stock)
            {
                throw new MallException("库存不足");
            }
            var shopBranch = ShopBranchApplication.GetShopBranchById(shopBranchId);
            if (shopBranch == null)
            {
                throw new MallException("错误的门店id");
            }
            var shopBranchSkuList = ShopBranchApplication.GetSkusByIds(shopBranchId, new List<string> { skuId });
            if (shopBranchSkuList == null || shopBranchSkuList.Count == 0)
            {
                throw new MallException("门店没有该商品");
            }
            if (shopBranchSkuList[0].Status == ShopBranchSkuStatus.InStock)
            {
                throw new MallException("此商品已下架");
            }

            if (memberId > 0)
                CartApplication.AddToShopBranchCart(skuId, count, memberId, shopBranchId);
            else
            {
                string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART_BRANCH);
                if (!string.IsNullOrWhiteSpace(cartInfo))
                {
                    string[] cartItems = cartInfo.Split(',');
                    string newCartInfo = string.Empty;
                    bool exist = false;
                    foreach (string cartItem in cartItems)
                    {
                        var cartItemParts = cartItem.Split(':');
                        if (cartItemParts[0] == skuId && cartItemParts[2] == shopBranchId.ToString())
                        {
                            newCartInfo += "," + skuId + ":" + (int.Parse(cartItemParts[1]) + count) + ":" + shopBranchId;
                            exist = true;
                        }
                        else
                            newCartInfo += "," + cartItem;
                    }
                    if (!exist)
                        newCartInfo += "," + skuId + ":" + count + ":" + shopBranchId;

                    if (!string.IsNullOrWhiteSpace(newCartInfo))
                        newCartInfo = newCartInfo.Substring(1);
                    WebHelper.SetCookie(CookieKeysCollection.Mall_CART_BRANCH, newCartInfo);
                }
                else
                {
                    WebHelper.SetCookie(CookieKeysCollection.Mall_CART_BRANCH, string.Format("{0}:{1}:{2}", skuId, count, shopBranchId));
                }
            }
        }

        void CheckSkuIdIsValid(string skuId, long shopBranchId)
        {
            long productId = 0;
            long.TryParse(skuId.Split('_')[0], out productId);
            if (productId == 0)
                throw new Mall.Core.InvalidPropertyException("SKUId无效");

            var skuItem = ProductManagerApplication.GetSKU(skuId);
            if (skuItem == null)
                throw new Mall.Core.InvalidPropertyException("SKUId无效");

        }

    }

    public class MobileHomeProducts
    {
        public object GetMobileHomeProducts(long shopId, PlatformType platformType, int page, int rows, string keyWords, string shopName, long? categoryId = null)
        {
            var homeProducts = MobileHomeProductApplication.GetMobileHomePageProducts(shopId, platformType, page, rows, keyWords, shopName, categoryId);
            var products = ProductManagerApplication.GetProducts(homeProducts.Models.Select(p => p.ProductId));
            var shops = ShopApplication.GetShops(products.Select(p => p.ShopId));
            var brands = BrandApplication.GetBrandsByIds(products.Select(p => p.BrandId));

            var model = homeProducts.Models.Select(item =>
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                var shop = shops.FirstOrDefault(p => p.Id == product.ShopId);
                var brand = brands.FirstOrDefault(p => p.Id == product.BrandId);
                return new
                {
                    name = product.ProductName,
                    image = product.GetImage(ImageSize.Size_50),
                    price = product.MinSalePrice.ToString("F2"),
                    brand = brand?.Name ?? string.Empty,
                    sequence = item.Sequence,
                    categoryName = CategoryApplication.GetCategory(product.CategoryId).Name,
                    id = item.Id,
                    productId = item.ProductId,
                    shopName = shop?.ShopName ?? string.Empty
                };
            });
            return new { rows = model, total = homeProducts.Total };
        }

        public object GetSellerMobileHomePageProducts(long shopId, PlatformType platformType, int page, int rows, string brandName, long? categoryId = null)
        {
            var data = MobileHomeProductApplication.GetSellerMobileHomePageProducts(shopId, platformType, page, rows, brandName, categoryId);
            var products = ProductManagerApplication.GetProducts(data.Models.Select(p => p.ProductId));
            var brands = BrandApplication.GetBrands(products.Select(p => p.BrandId).ToList());
            var categorys = ShopCategoryApplication.GetCategorysByProduct(products.Select(p => p.Id).ToList());

            var list = data.Models.Select(item =>
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                var brand = brands.FirstOrDefault(p => p.Id == product.BrandId);
                var category = categorys.FirstOrDefault(p => p.ProductId == product.Id);
                return new
                {
                    id = item.Id,
                    productId = item.ProductId,
                    name = product.ProductName,
                    image = product.GetImage(ImageSize.Size_50),
                    price = product.MinSalePrice.ToString("F2"),
                    brand = brand?.Name ?? string.Empty,
                    sequence = item.Sequence,
                    categoryName = category?.ShopCategoryName??string.Empty,
                };
            });


            return new { rows = list, total = data.Total };
        }

        public void AddHomeProducts(long shopId, string productIds, PlatformType platformType)
        {
            var productIdsArr = productIds.Split(',').Select(item => long.Parse(item));
            MobileHomeProductApplication.AddProductsToHomePage(shopId, platformType, productIdsArr);
        }
        public void UpdateSequence(long shopId, long id, short sequence)
        {
            MobileHomeProductApplication.UpdateSequence(shopId, id, sequence);
        }

        public void Delete(long shopId, long id)
        {
            MobileHomeProductApplication.Delete(shopId, id);
        }

        public void DeleteList(long[] ids)
        {
            MobileHomeProductApplication.DeleteList(ids);
        }

        public void DeleteAll(long shopId)
        {
            MobileHomeProductApplication.DeleteAll(shopId);
        }

        public object GetAllHomeProductIds(long shopId, PlatformType platformType)
        {
            var homeProductIds = MobileHomeProductApplication.GetMobileHomePageProducts(shopId, platformType).Select(item => item.ProductId);
            return homeProductIds;
        }
    }
}