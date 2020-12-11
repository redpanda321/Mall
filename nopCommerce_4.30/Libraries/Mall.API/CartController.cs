using Mall.API.Model.ParamsModel;
using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Mall.API
{
    public class CartController : BaseApiController
    {
        public object GetCartProduct()
        {
            var includeShopBranch = WebHelper.GetQueryString("includeSB");
            var flag = includeShopBranch == "1" ? true : false;//是否包含周边门店的商品
            CheckUserLogin();
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var shopService = ServiceProvider.Instance<IShopService>.Create;
            var vshopService = ServiceProvider.Instance<IVShopService>.Create;
            var siteSetting = SiteSettingApplication.SiteSettings;
            var typeservice = ServiceProvider.Instance<ITypeService>.Create;
            var shopBranchService = ServiceProvider.Instance<IShopBranchService>.Create;
            //会员折扣
            decimal discount = 1.0M;//默认折扣为1（没有折扣）
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            decimal prodPrice = 0.0M;//优惠价格

            #region Shop

            var cartHelper = ServiceProvider.Instance<ICartService>.Create;
            var cart = cartHelper.GetCart(CurrentUser.Id);

            var products = PackageCartProducts(cart, prodPrice, discount, productService, shopService, shopBranchService, vshopService, typeservice);
            //var selfProducts = products.OrderBy(item => item.ShopId).ThenByDescending(o => o.AddTime);
            var selfProducts = products.OrderBy(p => p.Status).ThenByDescending(item => item.AddTime);
            var selfCartShop = selfProducts.GroupBy(item => item.ShopId);
            //foreach(var i in selfCartShop)
            //{
            //    i.OrderBy(p => p.Status).ThenByDescending(item => item.AddTime);
            //}

            #endregion

            #region ShopBranch

            IEnumerable<IGrouping<long, CartProductModel>> sbCartShop = new List<CartProductModel>().GroupBy(item => item.ShopBranchId);
            if (flag)
            {
                var shopCartHelper = ServiceProvider.Instance<IBranchCartService>.Create;
                var branchcart = shopCartHelper.GetCart(CurrentUser.Id, 0);
                var branchProducts = PackageCartProducts(branchcart, prodPrice, discount, productService, shopService, shopBranchService, vshopService, typeservice, true);
                //var sbProducts = branchProducts.OrderBy(item => item.ShopBranchId).ThenByDescending(o => o.AddTime);
                var sbProducts = branchProducts.OrderBy(p => p.Status).ThenByDescending(item => item.AddTime);
                sbCartShop = sbProducts.GroupBy(item => item.ShopBranchId);
                //foreach (var i in sbCartShop)
                //{
                //    i.OrderBy(item => item.Status).ThenByDescending(item => item.AddTime);
                //}
            }

            #endregion

            //var invalidProducts = products.Where(item => item.Status == 0).ToArray();
            //products = products.Where(item => item.Status == 1).ToArray();

            var cartModel = new { success = true, Shop = selfCartShop, ShopBranch = sbCartShop };
            return cartModel;
        }

        /// <summary>
        /// 删除购物车商品
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost("PostDeleteCartProduct")]
        public object PostDeleteCartProduct(CartDeleteCartProductModel value)
        {
            CheckUserLogin();
            var skuIdsArr = value.skuIds.ToString().Split(',');
            ServiceProvider.Instance<ICartService>.Create.DeleteCartItem(skuIdsArr, CurrentUser.Id);
            return SuccessResult();
        }
        //更新购物车商品数量(这里没有及时更新,而是本地更新等会员点击购买的时候再更新到数据库并且提交订单)
        [HttpPost("PostUpdateCartItem")]
        public object PostUpdateCartItem(CartUpdateCartItemModel value)
        {
            var productService = ServiceProvider.Instance<IProductService>.Create;
            CheckUserLogin();
            string Jsonstr = value.jsonstr;
            var datavalue = Newtonsoft.Json.JsonConvert.DeserializeObject<UpdateCartSKusModel>(Jsonstr);
            var cartService = ServiceProvider.Instance<ICartService>.Create;
            var ladderPrice = 0m;
            long productId = 0;
            var isOpenLadder = false;
            foreach (var sku in datavalue.skus)
            {
                Entities.SKUInfo skuinfo = OrderApplication.GetSkuByID(sku.skuId);
                if (skuinfo != null)
                {
                    var productInfo = ProductManagerApplication.GetProduct(skuinfo.ProductId);
                    if (productInfo != null)
                    {
                        if (productInfo.MaxBuyCount > 0 && sku.count > productInfo.MaxBuyCount && !productInfo.IsOpenLadder)
                        {
                            return ErrorResult(string.Format("商品[{0}]限购{1}件", productInfo.ProductName, productInfo.MaxBuyCount), data: new { maxBuyCount = productInfo.MaxBuyCount });
                            //throw new MallException(string.Format("每个ID限购{0}件", productInfo.MaxBuyCount));
                        }
                    }
                }
                cartService.UpdateCart(sku.skuId, sku.count, CurrentUser.Id);
                #region 阶梯价--张宇枫
                var skus = CartApplication.GetCart(CurrentUser.Id);
                var skuItem = skus.Items.ToList().Find(i => i.SkuId == sku.skuId);
                productId = skuItem.ProductId;
                var product = ProductManagerApplication.GetProduct(productId);
                isOpenLadder = product.IsOpenLadder;
                if (isOpenLadder)
                {
                    var shop = ShopApplication.GetShop(product.ShopId);
                    var groupCartByProduct = skus.Items.Where(item =>  item.ShopBranchId == 0).Select(c =>
                    {
                        var cItem = new Mall.Entities.ShoppingCartItem();
                        var skuInfo = productService.GetSku(c.SkuId);
                        if (skuInfo != null)
                            cItem = c;
                        return cItem;
                    }).GroupBy(i => i.ProductId).ToList();
                    var quantity =
                        groupCartByProduct.Where(i => i.Key == productId)
                            .ToList()
                            .Sum(cartitem => cartitem.Sum(i => i.Quantity));
                    ladderPrice = ProductManagerApplication.GetProductLadderPrice(skuItem.ProductId, quantity);
                    if (shop.IsSelf)
                    {
                        ladderPrice = CurrentUser.MemberDiscount * ladderPrice;
                    }
                }
                #endregion
            }
            var result = new
            {
                success = true,
                //d.Url = "http://" + Url.Request.RequestUri.Host + "/m-IOS/Order/SubmiteByCart";
                Url = Core.MallIO.GetRomoteImagePath("/m-IOS/Order/SubmiteByCart"),
                Price = ladderPrice.ToString("F2"),
                ProductId = productId,
                IsOpenLadder = isOpenLadder ? 1 : 0,
            };
            return result;
        }
        //添加商品到购物车

        [HttpPost("PostAddProductToCart")]
        public object PostAddProductToCart(CartAddProductToCartModel value)
        {
            CheckUserLogin();
            string skuId = value.skuId;
            int count = value.count;
            CartHelper cartHelper = new CartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            try
            {
                cartHelper.AddToCart(skuId, count, userId);
            }
            catch (MallException ex)
            {
                return (SuccessResult(ex.Message));
            }
            return SuccessResult();
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
            return new { success = result, msg = msg };
        }

        #region Private

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
                var shop = shopService.GetShop(product.ShopId);
                DTO.ShopBranch shopbranch = null;
                Entities.ShopBranchSkuInfo shopbranchsku = null;
                if (item.ShopBranchId>0)
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
                            ShopBranchId = item.ShopBranchId ,
                            ShopBranchName = null == shopbranch ? "" : shopbranch.ShopBranchName,
                            ShopBranchLogo = null == shopbranch ? "" : Core.MallIO.GetRomoteImagePath(shopbranch.ShopImages)
                        };
                        products.Add(_tmp);
                    }
                }
            }
            return products;
        }

        #endregion
    }
}
