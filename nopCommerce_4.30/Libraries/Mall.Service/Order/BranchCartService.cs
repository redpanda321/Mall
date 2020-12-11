using Mall.CommonModel;
using Mall.Core;
using Mall.IServices;
using Mall.Entities;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Service
{
    public class BranchCartService : ServiceBase, IBranchCartService
    {
        public void AddToCart(string skuId, int count, long memberId, long shopbranchId)
        {
            if (count != 0)
            {
                CheckCartItem(skuId, count, memberId, shopbranchId);
                var cartItem = DbFactory.Default.Get<ShoppingCartInfo>().Where(item => item.UserId == memberId && item.SkuId == skuId && item.ShopBranchId != null && item.ShopBranchId.ExIfNull(0) == shopbranchId).FirstOrDefault();
                if (cartItem != null)//首先查询，如果已经存在则直接更新，否则新建
                {
                    cartItem.Quantity += count;//否则更新数量
                    DbFactory.Default.Update(cartItem);
                }
                else if (count > 0)
                {
                    long productId = long.Parse(skuId.Split('_')[0]);//SKU第一节为商品Id
                    DbFactory.Default.Add(new ShoppingCartInfo() { UserId = memberId, Quantity = count, SkuId = skuId, ProductId = productId, AddTime = DateTime.Now, ShopBranchId = shopbranchId });
                }
                Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memberId));
            }
        }

        public void AddToCart(IEnumerable<ShoppingCartItem> cartItems, long memberId)
        {
            foreach (var cartItem in cartItems.ToList())
            {
                if (cartItem.ShopBranchId>0)
                {
                    CheckCartItem(cartItem.SkuId, cartItem.Quantity, memberId, cartItem.ShopBranchId);
                    var oriCartItem = DbFactory.Default.Get<ShoppingCartInfo>().Where(item => item.UserId == memberId && item.SkuId == cartItem.SkuId && item.ShopBranchId == cartItem.ShopBranchId).FirstOrDefault();
                    if (oriCartItem != null)//首先查询，如果已经存在则直接更新，否则新建
                    {
                        oriCartItem.Quantity += cartItem.Quantity;
                        DbFactory.Default.Update(oriCartItem);
                    }
                    else
                    {
                        long productId = long.Parse(cartItem.SkuId.Split('_')[0]);//SKU第一节为商品Id
                        DbFactory.Default.Add(new ShoppingCartInfo() { UserId = memberId, Quantity = cartItem.Quantity, SkuId = cartItem.SkuId, ProductId = productId, AddTime = DateTime.Now, ShopBranchId = cartItem.ShopBranchId });
                    }
                }
            }

            Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memberId));
        }

        public void UpdateCart(string skuId, int count, long memberId, long shopbranchId)
        {
            CheckCartItem(skuId, count, memberId, shopbranchId);
            var cartItem = DbFactory.Default.Get<ShoppingCartInfo>().Where(item => item.UserId == memberId && item.SkuId == skuId && item.ShopBranchId.ExIsNotNull() && item.ShopBranchId.ExIfNull(0) == shopbranchId).FirstOrDefault();
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
                DbFactory.Default.Add(new ShoppingCartInfo() { UserId = memberId, Quantity = count, SkuId = skuId, ProductId = productId, AddTime = DateTime.Now, ShopBranchId = shopbranchId });
            }
            Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memberId));
        }

        public void ClearCart(long memeberId, long shopbranchId)
        {
            DbFactory.Default.Del<ShoppingCartInfo>().Where(item => item.UserId == memeberId && item.ShopBranchId.ExIsNotNull() && item.ShopBranchId.ExIfNull(0) == shopbranchId).Succeed();
            Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memeberId));
        }

        public void ClearInvalidCart(long memeberId, long shopbranchId)
        {
            //清理状态不对
            var skusql = DbFactory.Default.Get<ShopBranchSkuInfo>()
                .Where(d => d.Status == CommonModel.ShopBranchSkuStatus.Normal && d.ShopBranchId == shopbranchId)
                .Select(d => d.SkuId);

            DbFactory.Default.Del<ShoppingCartInfo>(item => item.UserId == memeberId &&  item.ShopBranchId.ExIfNull(0) == shopbranchId && item.SkuId.ExNotIn(skusql));
            //清理库存不足
            var delsql = DbFactory.Default.Get<ShoppingCartInfo>().InnerJoin<ShopBranchSkuInfo>((oi, oii) => oi.ShopBranchId == oii.ShopBranchId && oi.SkuId == oii.SkuId)
              .Where<ShopBranchSkuInfo, ShoppingCartInfo>((oi, oii) => oii.Quantity > oi.Stock && oii.UserId == memeberId && oi.ShopBranchId == shopbranchId).Select(a => a.Id).ToList<long>();

            DbFactory.Default.Del<ShoppingCartInfo>(n => n.Id.ExIn(delsql));

            Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memeberId));
        }

        public void DeleteCartItem(string skuId, long memberId, long shopbranchId)
        {
            DbFactory.Default.Del<ShoppingCartInfo>().Where(item => item.SkuId == skuId && item.UserId == memberId && item.ShopBranchId.ExIsNotNull() && item.ShopBranchId.ExIfNull(0) == shopbranchId).Succeed();
            Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memberId));
        }


        public void DeleteCartItem(IEnumerable<string> skuIds, long memberId, long shopbranchId)
        {
            DbFactory.Default.Del<ShoppingCartInfo>().Where(item => item.SkuId.ExIn(skuIds) && item.UserId == memberId && item.ShopBranchId.ExIsNotNull() && item.ShopBranchId.ExIfNull(0) == shopbranchId).Succeed();
            Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memberId));
        }

        public ShoppingCartInfo GetCart(long memeberId, long shopbranchId)
        {
            ShoppingCartInfo shoppingCartInfo = new ShoppingCartInfo() { MemberId = memeberId };
            if (Cache.Exists(CacheKeyCollection.CACHE_BRANCHCART(memeberId)))
            {
                shoppingCartInfo = Cache.Get<ShoppingCartInfo>(CacheKeyCollection.CACHE_BRANCHCART(memeberId));
            }
            else
            {
                var cartItems = DbFactory.Default.Get<ShoppingCartInfo>().Where(item => item.UserId == memeberId && item.ShopBranchId > 0);
                shoppingCartInfo.Items = cartItems.Select(item => new
                {
                    Id = item.Id,
                    SkuId = item.SkuId,
                    Quantity = (int)item.Quantity,
                    AddTime = item.AddTime,
                    ProductId = item.ProductId,
                    ShopBranchId = item.ShopBranchId
                }).ToList<ShoppingCartItem>();
                Cache.Insert<ShoppingCartInfo>(CacheKeyCollection.CACHE_BRANCHCART(memeberId), shoppingCartInfo, 600);
            }
            if (shopbranchId > 0)
            {
                return new ShoppingCartInfo() { MemberId = memeberId, Items = shoppingCartInfo.Items.Where(x => x.ShopBranchId == shopbranchId).ToList() };
            }
            return shoppingCartInfo;
        }

        public ShoppingCartInfo GetCartNoCache(long memeberId, long shopbranchId)
        {
            ShoppingCartInfo shoppingCartInfo = new ShoppingCartInfo() { MemberId = memeberId };
            var cartItems = DbFactory.Default.Get<ShoppingCartInfo>().Where(item => item.UserId == memeberId && item.ShopBranchId > 0);
            if (shopbranchId > 0)
                cartItems = cartItems.Where(item => item.ShopBranchId == shopbranchId);
            shoppingCartInfo.Items = cartItems.Select(item => new
            {
                Id = item.Id,
                SkuId = item.SkuId,
                Quantity = (int)item.Quantity,
                AddTime = item.AddTime,
                ProductId = item.ProductId,
                ShopBranchId = item.ShopBranchId
            }).ToList<ShoppingCartItem>();
            return shoppingCartInfo;
        }

        void CheckCartItem(string skuId, int count, long memberId, long shopbranchId)
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

        public List<ShoppingCartItem> GetCartItems(IEnumerable<long> cartItemIds, long shopbranchId)
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


        public List<ShoppingCartItem> GetCartItems(IEnumerable<string> skuIds, long memberId, long shopbranchId)
        {
            return DbFactory.Default.Get<ShoppingCartInfo>()
                .Where(item => item.UserId == memberId && item.SkuId.ExIn(skuIds.ToList()) && item.ShopBranchId.ExIsNotNull() && item.ShopBranchId.ExIfNull(0) == shopbranchId)
                .Select(item => new
                {
                    Id = item.Id,
                    SkuId = item.SkuId,
                    Quantity = item.Quantity,
                    ProductId = item.ProductId,
                    AddTime = item.AddTime,
                    ShopBranchId = item.ShopBranchId
                }).ToList<ShoppingCartItem>();
        }
        /// <summary>
        /// 获取购物车对应商品数量
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="skuId"></param>
        /// <returns></returns>
        public long GetCartProductQuantity(long memberId, long shopbranchId, long productId = 0, string skuId = "")
        {
            long cartQuantity = 0;
            if (productId > 0)
            {
                var shopInfo = DbFactory.Default.Get<ShoppingCartInfo>().Where(p => p.UserId == memberId && p.ProductId == productId && p.ShopBranchId.ExIsNotNull() && p.ShopBranchId == shopbranchId);
                cartQuantity = shopInfo.Sum(d => d.Quantity);
            }
            else
            {
                var shopInfo = DbFactory.Default.Get<ShoppingCartInfo>().Where(p => p.UserId == memberId && p.SkuId == skuId && p.ShopBranchId.ExIsNotNull() && p.ShopBranchId == shopbranchId);
                cartQuantity = shopInfo.Sum(d => d.Quantity);
            }
            return cartQuantity;
        }

        public List<ShoppingCartItem> GetCartQuantityByIds(long memberId, IEnumerable<long> productIds, long shopbranchId)
        {
            return DbFactory.Default.Get<ShoppingCartInfo>()
                .Where(item => item.UserId == memberId && item.ProductId.ExIn(productIds) && item.ShopBranchId.ExIsNotNull() && item.ShopBranchId.ExIfNull(0) == shopbranchId)
               .Select(item => new
               {
                   Id = item.Id,
                   SkuId = item.SkuId,
                   Quantity = item.Quantity,
                   ProductId = item.ProductId,
                   AddTime = item.AddTime,
                   ShopBranchId = item.ShopBranchId
               }).ToList<ShoppingCartItem>();
        }
    }
}
