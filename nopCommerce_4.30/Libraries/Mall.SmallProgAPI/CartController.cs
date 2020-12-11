using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Entities;
using Mall.IServices;
using Mall.SmallProgAPI.Model;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.SmallProgAPI
{
    public class CartController : BaseApiController
    {
        /// <summary>
        /// 添加到购物车
        /// </summary>
        /// <param name="context"></param>
        /// 
        [HttpGet("GetAddToCart")]
        public object GetAddToCart(string openId, string SkuID, int Quantity, int GiftID = 0)
        {
            //验证用户
            CheckUserLogin();
            CartHelper cartHelper = new CartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            var msg = "";
            try
            {
                cartHelper.AddToCart(SkuID, Quantity, userId);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            if (!string.IsNullOrEmpty(msg))
            {
                return Json(new { Status = "NO", Message = msg });
            }
            else
            {
                return Json(new { Status = "OK" });
            }
        }
        /// <summary>
        /// 更新购物车数量
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="SkuID"></param>
        /// <param name="Quantity"></param>
        /// <param name="GiftID"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetUpdateToCart")]
        public object GetUpdateToCart(string openId, string SkuID, int Quantity, int GiftID = 0)
        {
            //验证用户
            CheckUserLogin();
            var cartHelper = new CartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            var skus = cartHelper.GetCart(userId);
            var oldQuantity = GetCartProductQuantity(skus, skuId: SkuID);
            oldQuantity = oldQuantity + Quantity;

            long productId = 0;
            var skuItem = skus.Items.FirstOrDefault(i => i.SkuId == SkuID);
            if (null == skuItem)
            {
                var sku = ProductManagerApplication.GetSKU(SkuID);
                if (null == sku)
                {
                    return Json(ErrorResult<dynamic>("错误的参数:SkuId"));
                }
                productId = sku.ProductId;
            }
            else
            {
                productId = skuItem.ProductId;
            }
            var ladderPrice = 0m;
            var product = ProductManagerApplication.GetProduct(productId);
            if (product != null)
            {
                if (product.MaxBuyCount > 0 && oldQuantity > product.MaxBuyCount && !product.IsOpenLadder)
                {
                    cartHelper.UpdateCartItem(SkuID, product.MaxBuyCount, userId);
                    return Json(ErrorResult<dynamic>(string.Format("每个ID限购{0}件", product.MaxBuyCount), new { stock = product.MaxBuyCount }));
                }
            }


            SKUInfo skuinfo = OrderApplication.GetSkuByID(SkuID);

            if (skuinfo.Stock < oldQuantity)
            {
                cartHelper.UpdateCartItem(SkuID, Convert.ToInt32(skuinfo.Stock), userId);
                return Json(ErrorResult<dynamic>("库存不足", new { stock = skuinfo.Stock }));
            }

            cartHelper.UpdateCartItem(SkuID, oldQuantity, userId);
            //调用查询购物车数据

            #region 阶梯价--张宇枫
            var isOpenLadder = product.IsOpenLadder;
            if (isOpenLadder)
            {
                var shop = ShopApplication.GetShop(product.ShopId);
                var groupCartByProduct = cartHelper.GetCart(userId).Items.Where(item => item.ShopBranchId == 0).GroupBy(i => i.ProductId).ToList();
                //var groupCartByProduct = skus.Items.Where(item => item.ShopBranchId == 0).Select(c =>
                //{
                //    var cItem = new Mall.Entities.ShoppingCartItem();
                //    var skuInfo = ServiceProvider.Instance<IProductService>.Create.GetSku(c.SkuId);
                //    if (skuInfo != null)
                //        cItem = c;
                //    return cItem;
                //}).GroupBy(i => i.ProductId).ToList();

                var quantity =
                    groupCartByProduct.Where(i => i.Key == productId)
                        .ToList()
                        .Sum(cartitem => cartitem.Sum(i => i.Quantity));
                ladderPrice = ProductManagerApplication.GetProductLadderPrice(productId, quantity);
                if (shop.IsSelf)
                {
                    ladderPrice = CurrentUser.MemberDiscount * ladderPrice;
                }
            }
            #endregion
            return Json (new { Price = ladderPrice.ToString("F2"), ProductId = productId, IsOpenLadder = isOpenLadder ? 1 : 0 });
        }

        /// <summary>
        /// 从购物车移除
        /// </summary>
        /// <param name="context"></param>
        /// 
        [HttpGet("GetdelCartItem")]
        public object GetdelCartItem(string openId, string SkuIds, int GiftID = 0)
        {
            //验证用户
            CheckUserLogin();
            CartHelper cartHelper = new CartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            var skuIdsArr = SkuIds.ToString().Split(',');
            cartHelper.RemoveFromCart(skuIdsArr, userId);
            //调用查询购物车数据

            return Json(new { Status = "OK" });
        }

        [HttpGet("GetCartProduct")]
        public object GetCartProduct(string openId = "")
        {
            CheckUserLogin();
            List<CartProductModel> products = new List<CartProductModel>();
            var cartItems = CartApplication.GetCartItems(CurrentUser.Id);
            var vshops = VshopApplication.GetVShopsByShopIds(cartItems.Select(p => p.Shop.Id));
            foreach (var item in cartItems)
            {
                var sku = item.Sku;
                var product = item.Product;
                var shop = item.Shop;
                var vshop = vshops.FirstOrDefault(p => p.ShopId == shop.Id);

                var _tmp = new CartProductModel
                {
                    CartItemId = item.ItemId.ToString(),
                    SkuId = sku.Id,
                    Id = product.Id.ToString(),
                    ImgUrl = Core.MallIO.GetRomoteProductSizeImage(product.RelativePath, 1, (int)ImageSize.Size_150),
                    Name = product.ProductName,
                    Price = sku.SalePrice.ToString("F2"),
                    MarketPrice = product.MarketPrice.ToString("F2"),
                    Count = item.Quantity.ToString(),
                    ShopId = shop.Id.ToString(),
                    Size = sku.Size,
                    Color = sku.Color,
                    Version = sku.Version,
                    VShopId = vshop == null ? "0" : vshop.Id.ToString(),
                    ShopName = shop.ShopName,
                    ShopLogo = vshop == null ? "" : Core.MallIO.GetRomoteImagePath(vshop.Logo),
                    Url = Core.MallIO.GetRomoteImagePath("/m-IOS/product/detail/") + product.Id,
                    ProductStatus = item.ShowStatus,
                    ColorAlias = sku.ColorAlias,
                    SizeAlias = sku.SizeAlias,
                    VersionAlias = sku.VersionAlias,
                    AddTime = item.AddTime,
                    IsOpenLadder = product.IsOpenLadder,
                    MaxBuyCount = product.MaxBuyCount,
                    MinBath = ProductManagerApplication.GetProductLadderMinMath(product.Id),
                    ActiveId = item.LimitId
                };
                _tmp.IsValid = (_tmp.ProductStatus == 0) ? 0 : 1;
                products.Add(_tmp);
            }


            products = products.OrderBy(p => p.IsValid).ThenByDescending(item => item.AddTime).ToList();
            var cartShop = products.GroupBy(item => item.ShopId);
            decimal prodPrice = 0.0M;//优惠价格
            decimal discount = 1.0M;//默认折扣为1（没有折扣）
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var shopService = ServiceProvider.Instance<IShopService>.Create;
            var vshopService = ServiceProvider.Instance<IVShopService>.Create;
            var siteSetting = SiteSettingApplication.SiteSettings;
            var typeservice = ServiceProvider.Instance<ITypeService>.Create;
            var shopBranchService = ServiceProvider.Instance<IShopBranchService>.Create;
            var shopCartHelper = ServiceProvider.Instance<IBranchCartService>.Create;
            var branchcart = shopCartHelper.GetCart(CurrentUser.Id, 0);
            var branchProducts = PackageCartProducts(branchcart, prodPrice, discount, productService, shopService, shopBranchService, vshopService, typeservice, true);
            var sbProducts = branchProducts.OrderBy(p => p.Status).ThenByDescending(item => item.AddTime);
            var sbCartShop = sbProducts.GroupBy(item => item.ShopBranchId);

            return  Json(new { Shop = cartShop, ShopBranch = sbCartShop });
        }


        private List<CartProductModel> PackageCartProducts(Mall.Entities.ShoppingCartInfo cart, decimal prodPrice, decimal discount, IProductService productService, IShopService shopService, IShopBranchService shopBranchService, IVShopService vshopService, ITypeService typeservice, bool isBranch = false)
        {
            List<CartProductModel> products = new List<CartProductModel>();
            var limitProducts = LimitTimeApplication.GetPriceByProducrIds(cart.Items.Select(e => e.ProductId).ToList());//限时购价格
            var groupCart = cart.Items.Where(item => item.ShopBranchId == 0).Select(c =>
            {
                var cItem = new Mall.Entities.ShoppingCartItem();
                var skuInfo = productService.GetSku(c.SkuId);
                if (skuInfo != null)
                    cItem = c;
                return cItem;
            }).GroupBy(i => i.ProductId).ToList();
            foreach (var item in cart.Items)
            {
                var product = ProductManagerApplication.GetProduct(item.ProductId);
                if (product == null) continue;
                var shop = shopService.GetShop(product.ShopId);
                DTO.ShopBranch shopbranch = null;
                Entities.ShopBranchSkuInfo shopbranchsku = null;
                if (item.ShopBranchId > 0)
                {
                    shopbranch = ShopBranchApplication.GetShopBranchById(item.ShopBranchId);
                    shopbranchsku = shopBranchService.GetSkusByIds(item.ShopBranchId, new List<string> { item.SkuId }).FirstOrDefault();
                }

                if (null != shop)
                {
                    var vshop = vshopService.GetVShopByShopId(shop.Id);
                    var sku = ProductManagerApplication.GetSKU(item.SkuId);
                    if (sku == null)
                    {
                        continue;
                    }
                    //处理限时购、会员折扣价格
                    var prod = limitProducts.FirstOrDefault(e => e.ProductId == item.ProductId);
                    prodPrice = sku.SalePrice;
                    if (prod != null && !isBranch)
                    {
                        prodPrice = prod.MinPrice;
                    }
                    else
                    {
                        if (shop.IsSelf)
                        {//官方自营店才计算会员折扣
                            prodPrice = sku.SalePrice * discount;
                        }
                    }
                    #region 阶梯价--张宇枫
                    //阶梯价
                    if (product.IsOpenLadder)
                    {
                        var quantity = groupCart.Where(i => i.Key == item.ProductId).ToList().Sum(cartitem => cartitem.Sum(i => i.Quantity));
                        prodPrice = ProductManagerApplication.GetProductLadderPrice(item.ProductId, quantity);
                        if (shop.IsSelf)
                            prodPrice = prodPrice * discount;
                    }
                    #endregion
                    Entities.TypeInfo typeInfo = typeservice.GetType(product.TypeId);
                    string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                    if (product != null)
                    {
                        colorAlias = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias;
                        sizeAlias = !string.IsNullOrWhiteSpace(product.SizeAlias) ? product.SizeAlias : sizeAlias;
                        versionAlias = !string.IsNullOrWhiteSpace(product.VersionAlias) ? product.VersionAlias : versionAlias;
                    }
                    if (sku != null)
                    {
                        #region 正在参加限时抢购商品在购物车失效 TDO:ZYF
                        var isLimit = false;
                        //门店商品，在参加限时购，也可以正常购买
                        var limit = LimitTimeApplication.GetLimitTimeMarketItemByProductId(item.ProductId);
                        if (limit != null && !isBranch)
                        {
                            isLimit = limit.Status == Entities.FlashSaleInfo.FlashSaleStatus.Ongoing;
                        }
                        #endregion
                        var _tmp = new CartProductModel
                        {
                            CartItemId = item.Id.ToString(),
                            SkuId = item.SkuId,
                            Id = product.Id.ToString(),
                            ImgUrl = Core.MallIO.GetRomoteProductSizeImage(product.RelativePath, 1, (int)Mall.CommonModel.ImageSize.Size_150),
                            Name = product.ProductName,
                            Price = prodPrice.ToString("F2"),
                            Count = item.Quantity.ToString(),
                            ShopId = shop.Id.ToString(),
                            Size = sku.Size,
                            Color = sku.Color,
                            Version = sku.Version,
                            VShopId = vshop == null ? "0" : vshop.Id.ToString(),
                            ShopName = shop.ShopName,
                            ShopLogo = vshop == null ? "" : Core.MallIO.GetRomoteImagePath(vshop.StrLogo),
                            Url = Core.MallIO.GetRomoteImagePath("/m-IOS/product/detail/") + item.ProductId,
                            ProductStatus = isLimit ? 0 : (sku.Stock <= 0 ? ProductInfo.ProductSaleStatus.InStock.GetHashCode() : product.SaleStatus.GetHashCode()),
                            Status = isLimit ? 1 : ProductManagerApplication.GetProductShowStatus(product, sku, shopbranch == null ? 1 : item.Quantity, shopbranch, shopbranchsku),// 0:正常；1：已失效；2：库存不足；3：已下架；
                            ColorAlias = colorAlias,
                            SizeAlias = sizeAlias,
                            VersionAlias = versionAlias,
                            AddTime = item.AddTime,
                            ShopBranchId = item.ShopBranchId,
                            ShopBranchName = null == shopbranch ? "" : shopbranch.ShopBranchName,
                            ShopBranchLogo = null == shopbranch ? "" : Core.MallIO.GetRomoteImagePath(shopbranch.ShopImages),
                        };
                        products.Add(_tmp);
                    }
                }
            }
            return products;
        }

        /// <summary>
        /// 检查失效商品
        /// </summary>
        /// <param name="skus"></param>
        /// <param name="memeberId"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetCanSubmitOrder")]
        public object GetCanSubmitOrder(string openId, string skus)
        {
            CheckUserLogin();
            if (!string.IsNullOrEmpty(skus))
            {
                bool status = true;
                var SkuIds = skus.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
                CartHelper cartHelper = new CartHelper();
                foreach (var item in SkuIds)
                {
                    if (!cartHelper.CheckSkuId(item))
                    {
                        status = false;
                    }
                }
                if (status)
                {
                    return GetOKJson("OK");
                }
                else
                {
                    return GetErrorJson("有失效商品");
                }
            }
            else
            {
                return GetErrorJson("请选择商品");
            }
        }


        object GetOKJson(string okMsg)
        {
            var message = new
            {
                Status = "OK",
                Message = okMsg
            };
            return message;
        }
        object GetErrorJson(string errorMsg)
        {
            var message = new
            {
                Status = "NO",
                Message = errorMsg
            };
            return message;
        }

        /// <summary>
        /// 判断购物车结算的阶梯商品是否达到最小批量
        /// </summary>
        /// <param name="cartItemIds"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetLadderMintMath")]
        public object GetLadderMintMath(string cartItemIds)
        {
            var msg = "";
            var result = ProductManagerApplication.IsExistLadderMinMath(cartItemIds, ref msg);
            return Json(new { result, msg= msg });
        }

        private int GetCartProductQuantity(Mall.Entities.ShoppingCartInfo cartInfo, long productId = 0, string skuId = "")
        {
            int cartQuantity = 0;
            if (cartInfo == null)
            {
                return 0;
            }
            else
            {
                if (productId > 0)
                {
                    cartQuantity += cartInfo.Items.Where(p => p.ProductId == productId).Sum(d => d.Quantity);
                }
                else
                {
                    cartQuantity += cartInfo.Items.Where(p => p.SkuId == skuId).Sum(d => d.Quantity);
                }
            }
            return cartQuantity;
        }
    }
}
