using Mall.CommonModel;
using Mall.Core;
using Mall.IServices;
using Mall.Entities;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Mall.Core.Helper;

namespace Mall.Service
{
    public class CartService : ServiceBase, ICartService
    {
        public void AddToCart(string skuId, int count, long memberId)
        {
            if (count != 0)
            {
                CheckCartItem(skuId, count, memberId);
                var cartItem = DbFactory.Default.Get<ShoppingCartInfo>().Where(item => item.UserId == memberId && item.SkuId == skuId && item.ShopBranchId.ExIfNull(0) == 0).FirstOrDefault();
                if (cartItem != null)//首先查询，如果已经存在则直接更新，否则新建
                {
                    cartItem.Quantity += count;//否则更新数量
                    DbFactory.Default.Update(cartItem);
                }
                else if (count > 0)
                {
                    long productId = long.Parse(skuId.Split('_')[0]);//SKU第一节为商品Id
                    DbFactory.Default.Add(new ShoppingCartInfo() { UserId = memberId, Quantity = count, SkuId = skuId, ProductId = productId, AddTime = DateTime.Now });
                }
                Cache.Remove(CacheKeyCollection.CACHE_CART(memberId));
            }
        }
        public void AddToCart(IEnumerable<ShoppingCartItem> cartItems, long memberId)
        {
            foreach (var cartItem in cartItems.ToList())
            {
                CheckCartItem(cartItem.SkuId, cartItem.Quantity, memberId);
                var oriCartItem = DbFactory.Default.Get<ShoppingCartInfo>().Where(item => item.UserId == memberId && item.SkuId == cartItem.SkuId && item.ShopBranchId.ExIfNull(0) == 0).FirstOrDefault();
                if (oriCartItem != null)//首先查询，如果已经存在则直接更新，否则新建
                {
                    oriCartItem.Quantity += cartItem.Quantity;
                    DbFactory.Default.Update(oriCartItem);
                }
                else
                {
                    long productId = long.Parse(cartItem.SkuId.Split('_')[0]);//SKU第一节为商品Id
                    DbFactory.Default.Add(new ShoppingCartInfo() { UserId = memberId, Quantity = cartItem.Quantity, SkuId = cartItem.SkuId, ProductId = productId, AddTime = DateTime.Now });
                }
            }
            Cache.Remove(CacheKeyCollection.CACHE_CART(memberId));
        }

        public void UpdateCart(string skuId, int count, long memberId)
        {
            CheckCartItem(skuId, count, memberId);
            var cartItem = DbFactory.Default.Get<ShoppingCartInfo>().Where(item => item.UserId == memberId && item.SkuId == skuId && item.ShopBranchId.ExIfNull(0) == 0).FirstOrDefault();
            if (cartItem != null)//首先查询，如果已经存在则直接更新，否则新建
            {
                if (count == 0)//数量为0时，删除对应项
                {
                    DbFactory.Default.Del<ShoppingCartInfo>().Where(n => n.Id == cartItem.Id).Succeed();
                }
                else
                {
                    cartItem.Quantity = count;//否则更新数量
                    DbFactory.Default.Update(cartItem);
                }
            }
            else if (count > 0)
            {
                long productId = long.Parse(skuId.Split('_')[0]);//SKU第一节为商品Id
                DbFactory.Default.Add(new ShoppingCartInfo() { UserId = memberId, Quantity = count, SkuId = skuId, ProductId = productId, AddTime = DateTime.Now });
            }
            Cache.Remove(CacheKeyCollection.CACHE_CART(memberId));
        }

        public void ClearCart(long memeberId)
        {
            DbFactory.Default.Del<ShoppingCartInfo>().Where(item => item.UserId == memeberId && item.ShopBranchId.ExIfNull(0) == 0).Succeed();
            Cache.Remove(CacheKeyCollection.CACHE_CART(memeberId));
        }

        public void DeleteCartItem(string skuId, long memberId)
        {
            DbFactory.Default.Del<ShoppingCartInfo>().Where(item => item.SkuId == skuId && item.UserId == memberId && item.ShopBranchId.ExIfNull(0) == 0).Succeed();
            Cache.Remove(CacheKeyCollection.CACHE_CART(memberId));
        }


        public void DeleteCartItem(IEnumerable<string> skuIds, long memberId)
        {
            DbFactory.Default.Del<ShoppingCartInfo>().Where(item => item.SkuId.ExIn(skuIds) && item.UserId == memberId && item.ShopBranchId.ExIfNull(0) == 0).Succeed();
            Cache.Remove(CacheKeyCollection.CACHE_CART(memberId));
        }

        public ShoppingCartInfo GetCart(long memeberId)
        {
            if (Cache.Exists(CacheKeyCollection.CACHE_CART(memeberId)))
                return Cache.Get<ShoppingCartInfo>(CacheKeyCollection.CACHE_CART(memeberId));

            ShoppingCartInfo shoppingCartInfo = new ShoppingCartInfo() { MemberId = memeberId };
            var items = DbFactory.Default.Get<ShoppingCartInfo>()
                .Where(item => item.UserId == memeberId && item.ShopBranchId == 0).ToList();

            shoppingCartInfo.Items = items.Select(item => new ShoppingCartItem
            {
                Id = item.Id,
                SkuId = item.SkuId,
                Quantity =(int)item.Quantity,
                AddTime = item.AddTime,
                ProductId = item.ProductId,
                ShopBranchId = item.ShopBranchId
            }).ToList();

            //foreach (var item in shoppingCartInfo.Items)
            //{
            //    if (item.ShopBranchId.HasValue && item.ShopBranchId > 0)
            //    {
            //        var branchSku = DbFactory.Default.Get<ShopBranchSkuInfo>().Where(d => d.SkuId == item.SkuId && d.ShopBranchId == item.ShopBranchId).FirstOrDefault();
            //        item.Status = 1;
            //        if (branchSku != null && branchSku.Status == CommonModel.ShopBranchSkuStatus.Normal)
            //        {
            //            item.Status = 0;
            //            if (item.Quantity > branchSku.Stock)
            //            {
            //                item.Status = 2;
            //            }
            //        }
            //        else
            //        {
            //            item.Status = 1;
            //        }
            //    }
            //}
            Cache.Insert(CacheKeyCollection.CACHE_CART(memeberId), shoppingCartInfo, 600);
            return shoppingCartInfo;
        }

        public List<ShoppingCartInfo> GetCarts(long memberId,int top)
        {
            return DbFactory.Default.Get<ShoppingCartInfo>()
                    .LeftJoin<ProductInfo>((c, p) => c.ProductId == p.Id)
                    .Where(p => p.UserId == memberId)
                    .Where<ProductInfo>(p => p.IsDeleted == false)
                    .Take(top)
                    .ToList();
        }
        public int GetCartCount(long memberId)
        {
            return DbFactory.Default.Get<ShoppingCartInfo>()
                    .LeftJoin<ProductInfo>((c, p) => c.ProductId == p.Id)
                    .Where(p => p.UserId == memberId)
                    .Where<ProductInfo>(p => p.IsDeleted == false)
                    .Count();
        }



        void CheckCartItem(string skuId, int count, long memberId)
        {
            if (string.IsNullOrWhiteSpace(skuId))
                throw new InvalidPropertyException("SKUId不能为空");
            else if (count < 0)
                throw new InvalidPropertyException("商品数量不能小于0");
            else
            {
                var member = DbFactory.Default.Get<MemberInfo>().Where(item => item.Id == memberId).FirstOrDefault();
                if (member == null)
                    throw new InvalidPropertyException("会员Id" + memberId + "不存在");
            }
        }
        void CheckCartItemByBranch(string skuId, int count, long memberId, long shopbranchId)
        {
            if (string.IsNullOrWhiteSpace(skuId))
                throw new InvalidPropertyException("SKUId不能为空");
            else if (count < 0)
                throw new InvalidPropertyException("商品数量不能小于0");
            else if (shopbranchId <= 0)
                throw new InvalidPropertyException("门店ID不能为空");
            else
            {
                var member = DbFactory.Default.Get<MemberInfo>().Where(item => item.Id == memberId).FirstOrDefault();
                if (member == null)
                    throw new InvalidPropertyException("会员Id" + memberId + "不存在");
            }
        }

        public List<ShoppingCartItem> GetCartItems(IEnumerable<long> cartItemIds)
        {
            var shoppingCartItems = DbFactory.Default.Get<ShoppingCartInfo>()
              .Where(item => item.Id.ExIn(cartItemIds))
               .Select(item => new
               {
                   Id = item.Id,
                   SkuId = item.SkuId,
                   Quantity = item.Quantity,
                   ProductId = item.ProductId,
                   AddTime = item.AddTime,
                   ShopBranchId = item.ShopBranchId
               }).ToList<ShoppingCartItem>();
            return shoppingCartItems;
        }


        public List<ShoppingCartItem> GetCartItems(IEnumerable<string> skuIds, long memberId)
        {
            return DbFactory.Default.Get<ShoppingCartInfo>()
                .Where(item => item.UserId == memberId && item.SkuId.ExIn(skuIds) && item.ShopBranchId.ExIfNull(0) == 0)
                .Select(item => new
                {
                    Id = item.Id,
                    SkuId = item.SkuId,
                    Quantity = item.Quantity,
                    ProductId = item.ProductId,
                    AddTime = item.AddTime
                }).ToList<ShoppingCartItem>();
        }
        /// <summary>
        /// 获取购物车对应商品数量
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="skuId"></param>
        /// <returns></returns>
        public long GetCartProductQuantity(long memberId, long productId = 0, string skuId = "")
        {
            long cartQuantity = 0;
            if (productId > 0)
            {
                var shopInfo = DbFactory.Default.Get<ShoppingCartInfo>().Where(p => p.UserId == memberId && p.ProductId == productId && p.ShopBranchId.ExIsNull());

                cartQuantity = shopInfo.Sum<long>(d => d.Quantity);
            }
            else
            {
                var shopInfo = DbFactory.Default.Get<ShoppingCartInfo>().Where(p => p.UserId == memberId && p.SkuId == skuId && p.ShopBranchId.ExIsNull());

                cartQuantity = shopInfo.Sum<long>(d => d.Quantity);
            }
            return cartQuantity;
        }

        public List<ShoppingCartItem> GetCartQuantityByIds(long memberId, IEnumerable<long> productIds)
        {
            return DbFactory.Default.Get<ShoppingCartInfo>()
                .Where(item => item.UserId == memberId && item.ProductId.ExIn(productIds) && item.ShopBranchId.ExIfNull(0) == 0)
              .Select(item => new
              {
                  Id = item.Id,
                  SkuId = item.SkuId,
                  Quantity = item.Quantity,
                  ProductId = item.ProductId,
                  AddTime = item.AddTime
              }).ToList<ShoppingCartItem>();
        }
    }
}
