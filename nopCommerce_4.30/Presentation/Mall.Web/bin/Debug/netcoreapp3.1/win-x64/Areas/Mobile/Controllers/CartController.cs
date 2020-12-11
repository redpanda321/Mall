using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Mobile.Controllers
{
    public class CartController : BaseMobileMemberController
    {
        IProductService _iProductService;
        IShopService _iShopService;
        IVShopService _iVShopService;
        IShopBranchService _iShopBranchService;
        public CartController(IProductService iProductService, IShopService iShopService, IVShopService iVShopService, IShopBranchService iShopBranchService)
        {
            _iProductService = iProductService;
            _iShopService = iShopService;
            _iVShopService = iVShopService;
            _iShopBranchService = iShopBranchService;
        }
        // GET: Mobile/Cart
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Cart()
        {
            return View();
        }

        [HttpPost]
        public JsonResult AddProductToCart(string skuId, int count)
        {
            CartHelper cartHelper = new CartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            cartHelper.AddToCart(skuId, count, userId);
            return Json(new { success = true });
        }

        [HttpPost]


        public JsonResult GetCartProducts()
        {
            var memberId = CurrentUser?.Id ?? 0;
            var cartItems = CartApplication.GetCartItems(memberId);
            var vshops = VshopApplication.GetVShopsByShopIds(cartItems.Select(p => p.Shop.Id));
            var products = cartItems.Select(item =>
                {
                    var sku = item.Sku;
                    var product = item.Product;
                    var shop = item.Shop;
                    var vshop = vshops.FirstOrDefault(p => p.ShopId == shop.Id);

                    var skuDetails = string.Empty;
                    if (!string.IsNullOrWhiteSpace(sku.Size))
                        skuDetails += sku.SizeAlias + "：" + sku.Size + "&nbsp;&nbsp;";
                    if (!string.IsNullOrWhiteSpace(sku.Color))
                        skuDetails += sku.ColorAlias + "：" + sku.Color + "&nbsp;&nbsp;";
                    if (!string.IsNullOrWhiteSpace(sku.Version))
                        skuDetails += sku.VersionAlias + "：" + sku.Version + "&nbsp;&nbsp;";


                    return new
                    {
                        cartItemId = item.ItemId,
                        skuId = sku.Id,
                        id = product.Id,
                        imgUrl = Mall.Core.MallIO.GetProductSizeImage(product.RelativePath, 1, (int)ImageSize.Size_150),
                        name = product.ProductName,
                        price = sku.SalePrice,
                        count = item.Quantity,
                        shopId = shop.Id,
                        vshopId = vshop == null ? 0 : vshop.Id,
                        shopName = shop.ShopName,
                        shopLogo = vshop == null ? "" : vshop.Logo,
                        productstatus = item.IsLimit ? 0 : (sku.Stock <= 0 ? ProductInfo.ProductSaleStatus.InStock.GetHashCode() : product.SaleStatus.GetHashCode()),
                        status = item.IsLimit ? 1 : ProductManagerApplication.GetProductShowStatus(product, sku.Map<DTO.SKU>(), 1),// 0:正常；1：已失效；2：库存不足；3：已下架；
                        Size = sku.Size,
                        Color = sku.Color,
                        Version = sku.Version,
                        ColorAlias = sku.ColorAlias,
                        SizeAlias = sku.SizeAlias,
                        VersionAlias = sku.VersionAlias,
                        skuDetails = skuDetails,
                        AddTime = item.AddTime,
                        minMath = ProductManagerApplication.GetProductLadderMinMath(product.Id)
                    };
                }).OrderBy(p => p.status).ThenByDescending(o => o.AddTime).ToList();

            #region 门店购物车
            var discount = CurrentUser?.MemberDiscount ?? 1;
            var branchCartHelper = new BranchCartHelper();
            long userId = 0;
            if (CurrentUser != null)
            {
                userId = CurrentUser.Id;
            }
            var Branchcart = branchCartHelper.GetCartNoCache(userId, 0);
            var shopBranchList = Branchcart.Items.Where(x => x.ShopBranchId > 0).Select(x => x.ShopBranchId).ToList();
            shopBranchList = shopBranchList.GroupBy(x => x).Select(x => x.First()).ToList();
            Dictionary<long, long> buyedCounts = null;
            if (userId > 0)
            {
                buyedCounts = new Dictionary<long, long>();
                buyedCounts = OrderApplication.GetProductBuyCount(userId, Branchcart.Items.Select(x => x.ProductId));
            }

            var branchProducts = PackageCartProducts(Branchcart, discount, true);
            var sbProducts = branchProducts.OrderBy(p => p.status).ThenByDescending(item => item.AddTime);
            var shopBranchCart = sbProducts.GroupBy(item => item.shopBranchId);
            #endregion

            var cartModel = new { products = products, amount = products.Sum(item => item.price * item.count), totalCount = products.Sum(item => item.count), shopBranchCart = shopBranchCart };
            return SuccessResult<dynamic>(data: cartModel);
        }

        [HttpPost]
        public JsonResult UpdateCartItem(string skuId, int count)
        {
            //TODO:FG 购物车改造 购物车数量变更
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            var cartHelper = new CartHelper();
            var skucart = cartHelper.GetCart(userId).Items.FirstOrDefault(d => d.SkuId == skuId);
            bool isadd = true;
            if (skucart != null)
            {
                if (skucart.Quantity > count) isadd = false;
            }

            Entities.SKUInfo skuinfo = OrderApplication.GetSkuByID(skuId);
            if (skuinfo.Stock < count && isadd)
                return Json(new { success = false, msg = "库存不足", stock = skuinfo.Stock });

            var product = ProductManagerApplication.GetProduct(skuinfo.ProductId);
            if (product != null)
            {
                if (product.MaxBuyCount > 0 && count > product.MaxBuyCount && !product.IsOpenLadder)
                {
                    return Json(new { success = false, msg = string.Format("每个ID限购{0}件", product.MaxBuyCount), stock = product.MaxBuyCount });
                }
            }

            cartHelper.UpdateCartItem(skuId, count, userId);

            #region 购物车修改数量阶梯价变动--张宇枫
            //获取产品详情
            var price = 0m;
            if (product.IsOpenLadder)
            {
                var shop = ShopApplication.GetShop(product.ShopId);

                var groupCartByProduct = cartHelper.GetCart(userId).Items.Where(item => item.ShopBranchId == 0).Select(c =>
               {
                   var cItem = new Mall.Entities.ShoppingCartItem();
                   var skuInfo = _iProductService.GetSku(c.SkuId);
                   if (skuInfo != null)
                       cItem = c;
                   return cItem;
               }).GroupBy(i => i.ProductId).ToList();
                var quantity = groupCartByProduct.Where(i => i.Key == product.Id).ToList().Sum(cartitem => cartitem.Sum(i => i.Quantity));

                decimal discount = 1M;
                if (CurrentUser != null)
                {
                    discount = CurrentUser.MemberDiscount;
                }
                price = ProductManagerApplication.GetProductLadderPrice(product.Id, quantity);
                if (shop.IsSelf)
                    price = price * discount;
            }

            #endregion

            return SuccessResult<dynamic>(data: new { saleprice = price.ToString("F2"), productid = product.Id, isOpenLadder = product.IsOpenLadder });
        }

        public JsonResult UpdateCartItem(Dictionary<string, int> skus, long userId)
        {
            var cartHelper = new CartHelper();
            foreach (var sku in skus)
            {
                cartHelper.UpdateCartItem(sku.Key, sku.Value, userId);
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult BatchRemoveFromCart(string skuIds)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;

            var skuIdsArr = skuIds.Split(',');
            var cartHelper = new CartHelper();
            cartHelper.RemoveFromCart(skuIdsArr, userId);
            return Json(new { success = true });
        }
        [HttpPost]
        public JsonResult RemoveFromCart(string skuId)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;

            var cartHelper = new CartHelper();
            cartHelper.RemoveFromCart(skuId, userId);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult IsLadderCount(string cartItemIds)
        {
            var msg = "";
            var result = ProductManagerApplication.IsExistLadderMinMath(cartItemIds, ref msg);
            return Json(new { success = result, msg = msg });
        }

        private List<CartProductModel> PackageCartProducts(Mall.Entities.ShoppingCartInfo cart, decimal discount, bool isBranch = false)
        {
            List<CartProductModel> products = new List<CartProductModel>();
            var limitProducts = LimitTimeApplication.GetPriceByProducrIds(cart.Items.Select(e => e.ProductId).ToList());//限时购价格
            var groupCart = cart.Items.Where(item => item.ShopBranchId == 0).Select(c =>
           {
               var cItem = new Mall.Entities.ShoppingCartItem();
               var skuInfo = _iProductService.GetSku(c.SkuId);
               if (skuInfo != null)
                   cItem = c;
               return cItem;
           }).GroupBy(i => i.ProductId).ToList();
            foreach (var item in cart.Items)
            {
                var product = ProductManagerApplication.GetProduct(item.ProductId);
                var shop = _iShopService.GetShop(product.ShopId);
                DTO.ShopBranch shopbranch = null;
                Entities.ShopBranchSkuInfo shopbranchsku = null;
                if (item.ShopBranchId > 0)
                {
                    shopbranch = ShopBranchApplication.GetShopBranchById(item.ShopBranchId);
                    shopbranchsku = ShopBranchApplication.GetSkusByIds(item.ShopBranchId, new List<string> { item.SkuId }).FirstOrDefault();
                }

                if (null != shop)
                {
                    var vshop = VshopApplication.GetVShopByShopId(shop.Id);
                    var sku = ProductManagerApplication.GetSKU(item.SkuId);
                    if (sku == null)
                    {
                        continue;
                    }
                    //处理限时购、会员折扣价格
                    var prod = limitProducts.FirstOrDefault(e => e.ProductId == item.ProductId);
                    var prodPrice = sku.SalePrice;
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
                    var typeInfo = TypeApplication.GetProductType(product.TypeId);
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
                            cartItemId = item.Id,
                            skuId = item.SkuId,
                            id = product.Id,
                            imgUrl = Core.MallIO.GetRomoteProductSizeImage(product.RelativePath, 1, (int)Mall.CommonModel.ImageSize.Size_150),
                            name = product.ProductName,
                            price = prodPrice.ToString("F2"),
                            count = item.Quantity,
                            ShopId = shop.Id.ToString(),
                            Size = sku.Size,
                            Color = sku.Color,
                            Version = sku.Version,
                            VShopId = vshop == null ? "0" : vshop.Id.ToString(),
                            ShopName = shop.ShopName,
                            ShopLogo = vshop == null ? "" : Core.MallIO.GetRomoteImagePath(vshop.StrLogo),
                            Url = Core.MallIO.GetRomoteImagePath("/m-IOS/product/detail/") + item.ProductId,
                            ProductStatus = isLimit ? 0 : (sku.Stock <= 0 ? ProductInfo.ProductSaleStatus.InStock.GetHashCode() : product.SaleStatus.GetHashCode()),
                            status = isLimit ? 1 : ProductManagerApplication.GetProductShowStatus(product, sku, item.Quantity, shopbranch, shopbranchsku),// 0:正常；1：已失效；2：库存不足；3：已下架；
                            ColorAlias = colorAlias,
                            SizeAlias = sizeAlias,
                            VersionAlias = versionAlias,
                            AddTime = item.AddTime,
                            shopBranchId = item.ShopBranchId,
                            shopBranchName = null == shopbranch ? "" : shopbranch.ShopBranchName,
                            ShopBranchLogo = null == shopbranch ? "" : Core.MallIO.GetRomoteImagePath(shopbranch.ShopImages)
                        };
                        products.Add(_tmp);
                    }
                }
            }
            return products;
        }

        #region 门店相关购物车
        /// <summary>
        /// 修改购物车商品
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="count"></param>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditBranchProductToCart(string skuId, int count, long shopBranchId)
        {
            BranchCartHelper branchCartHelper = new BranchCartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            branchCartHelper.UpdateCartItem(skuId, count, userId, shopBranchId);
            return Json(new { success = true });
        }
        /// <summary>
        /// 获取底部购物车详情
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetBranchCartProducts(long shopBranchId)
        {
            var branchCartHelper = new BranchCartHelper();
            long userId = 0;
            //会员折扣
            decimal discount = 1.0M;//默认折扣为1（没有折扣）
            if (CurrentUser != null)
            {
                userId = CurrentUser.Id;
                discount = CurrentUser.MemberDiscount;
            }
            var cart = branchCartHelper.GetCart(userId, shopBranchId);
            //var shopBranch = _iShopBranchService.GetShopBranchById(shopBranchId);
            var shopBranch = ShopBranchApplication.GetShopBranchById(shopBranchId);
            Dictionary<long, long> buyedCounts = null;
            if (userId > 0)
            {
                buyedCounts = new Dictionary<long, long>();
                buyedCounts = OrderApplication.GetProductBuyCount(userId, cart.Items.Select(x => x.ProductId));
            }
            decimal prodPrice = 0.0M;//优惠价格
            var shopBranchSkuList = _iShopBranchService.GetSkusByIds(shopBranchId, cart.Items.Select(x => x.SkuId).ToList());

            var products = cart.Items.Select(item =>
            {
                var product = _iProductService.GetProduct(item.ProductId);
                var shopbranchsku = shopBranchSkuList.FirstOrDefault(x => x.SkuId == item.SkuId);
                long stock = shopbranchsku == null ? 0 : shopbranchsku.Stock;

                if (stock > product.MaxBuyCount && product.MaxBuyCount != 0)
                    stock = product.MaxBuyCount;
                if (product.MaxBuyCount > 0 && buyedCounts != null && buyedCounts.ContainsKey(item.ProductId))
                {
                    long buynum = buyedCounts[item.ProductId];
                    stock = stock - buynum;
                }

                var shop = _iShopService.GetShop(product.ShopId);
                //Entities.SKUInfo sku = null;
                string skuDetails = "";
                if (null != shop)
                {
                    var vshop = _iVShopService.GetVShopByShopId(shop.Id);
                    //sku = _iProductService.GetSku(item.SkuId);
                    DTO.SKU sku = ProductManagerApplication.GetSKU(item.SkuId);
                    if (sku == null)
                    {
                        return null;
                    }
                    prodPrice = sku.SalePrice;
                    if (shop.IsSelf)
                    {//官方自营店才计算会员折扣
                        prodPrice = sku.SalePrice * discount;
                    }

                    var typeInfo = TypeApplication.GetProductType(product.TypeId);
                    skuDetails = "";
                    if (!string.IsNullOrWhiteSpace(sku.Size))
                    {
                        skuDetails += sku.Size + "&nbsp;&nbsp;";
                    }
                    if (!string.IsNullOrWhiteSpace(sku.Color))
                    {
                        skuDetails += sku.Color + "&nbsp;&nbsp;";
                    }
                    if (!string.IsNullOrWhiteSpace(sku.Version))
                    {
                        skuDetails += sku.Version + "&nbsp;&nbsp;";
                    }
                    return new
                    {
                        bId = shopBranchId,
                        cartItemId = item.Id,
                        skuId = item.SkuId,
                        id = product.Id,
                        name = product.ProductName,
                        price = prodPrice,
                        count = item.Quantity,
                        stock = shopbranchsku == null ? 0 : stock,
                        //阶梯价商品在门店购物车自动下架
                        //status = product.IsOpenLadder ? 1 : (shopbranchsku == null ? 1 : (shopbranchsku.Status == ShopBranchSkuStatus.Normal) ? (item.Quantity > stock ? 2 : 0) : 1),//0:正常；1：冻结；2：库存不足
                        status = ProductManagerApplication.GetProductShowStatus(product, sku, 1, shopBranch, shopbranchsku),//0:正常；1：冻结；2：库存不足；3：已下架；
                        skuDetails = skuDetails,
                        AddTime = item.AddTime
                    };
                }
                else
                {
                    return null;
                }
            }).Where(d => d != null).OrderBy(s => s.status).ThenByDescending(o => o.AddTime);

            var cartModel = new { products = products, amount = products.Where(x => x.status == 0).Sum(item => item.price * item.count), totalCount = products.Where(x => x.status == 0).Sum(item => item.count), DeliveFee = shopBranch.DeliveFee, DeliveTotalFee = shopBranch.DeliveTotalFee, FreeMailFee = shopBranch.FreeMailFee, IsFreeMail = shopBranch.IsFreeMail, shopBranchStatus = (int)shopBranch.Status };
            return SuccessResult<dynamic>(data: cartModel);
        }


        /// <summary>
        /// 清理门店购物车所有商品
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public JsonResult ClearBranchCartProducts(long shopBranchId)
        {
            BranchCartHelper branchCartHelper = new BranchCartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            var cart = branchCartHelper.GetCart(userId, shopBranchId);
            foreach (var item in cart.Items)
            {
                branchCartHelper.RemoveFromCart(item.SkuId, userId, shopBranchId);
            }
            return Json(new { success = true });
        }
        /// <summary>
        /// 清理门店购物车所有无效商品
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public JsonResult ClearBranchCartInvalidProducts(long shopBranchId)
        {
            BranchCartHelper branchCartHelper = new BranchCartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            var cart = branchCartHelper.GetCart(userId, shopBranchId);
            Dictionary<long, long> buyedCounts = null;
            if (userId > 0)
            {
                buyedCounts = new Dictionary<long, long>();
                buyedCounts = OrderApplication.GetProductBuyCount(userId, cart.Items.Select(x => x.ProductId));
            }
            foreach (var item in cart.Items)
            {
                var product = _iProductService.GetProduct(item.ProductId);
                var shopbranchsku = _iShopBranchService.GetSkusByIds(shopBranchId, new List<string> { item.SkuId }).FirstOrDefault();
                long stock = shopbranchsku == null ? 0 : shopbranchsku.Stock;

                if (stock > product.MaxBuyCount && product.MaxBuyCount != 0)
                    stock = product.MaxBuyCount;
                if (product.MaxBuyCount > 0 && buyedCounts != null && buyedCounts.ContainsKey(item.ProductId))
                {
                    long buynum = buyedCounts[item.ProductId];
                    stock = stock - buynum;
                }

                if (shopbranchsku.Status != ShopBranchSkuStatus.Normal || item.Quantity > stock)
                {
                    branchCartHelper.RemoveFromCart(item.SkuId, userId, shopBranchId);
                }
            }
            return Json(new { success = true });
        }

        #endregion
    }
}