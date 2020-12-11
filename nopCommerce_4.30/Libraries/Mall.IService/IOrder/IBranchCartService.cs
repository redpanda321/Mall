using Mall.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Mall.IServices
{
    public interface IBranchCartService : IService
    {
        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <param name="skuId">商品SKU Id</param>
        /// <param name="count">商品数量</param>
        /// <param name="memberId">会员id</param>
        void AddToCart(string skuId, int count, long memberId, long shopbranchId);

        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <param name="cartItems">购物车商品项</param>
        /// <param name="memberId">会员Id</param>
        void AddToCart(IEnumerable<ShoppingCartItem> cartItems, long memberId);

        /// <summary>
        /// 更新购物车
        /// </summary>
        /// <param name="skuId">商品SKU Id</param>
        /// <param name="count">商品数量</param>
        /// <param name="memberId">会员id</param>
        void UpdateCart(string skuId, int count, long memberId, long shopbranchId);

        /// <summary>
        /// 清空指定会员的购物车
        /// </summary>
        /// <param name="memeberId">会员id</param>
        void ClearCart(long memeberId, long shopbranchId);
        /// <summary>
        /// 清空指定会员的购物车的无效商品
        /// </summary>
        /// <param name="memeberId">会员id</param>
        void ClearInvalidCart(long memeberId, long shopbranchId);

        /// <summary>
        /// 删除指定会员购物车中的指定商品
        /// </summary>
        /// <param name="skuId">待删除的商品的skuid</param>
        /// <param name="memberId">会员id</param>
        void DeleteCartItem(string skuId, long memberId, long shopbranchId);


        /// <summary>
        /// 删除指定会员购物车中的指定商品
        /// </summary>
        /// <param name="skuIds">待删除的商品的skuid</param>
        /// <param name="memberId">会员id</param>
        void DeleteCartItem(IEnumerable<string> skuIds, long memberId, long shopbranchId);

        /// <summary>
        /// 获取指定会员购物车信息
        /// </summary>
        /// <param name="memeberId">会员id</param>
        /// <returns></returns>
        ShoppingCartInfo GetCart(long memeberId, long shopbranchId);

        /// <summary>
        /// 获取指定会员购物车信息（不从缓存取）
        /// </summary>
        /// <param name="memeberId">会员id</param>
        /// <returns></returns>
        ShoppingCartInfo GetCartNoCache(long memeberId, long shopbranchId);

        /// <summary>
        /// 获取购物车购物项
        /// </summary>
        /// <param name="cartItemIds">购物车项Id</param>
        /// <returns></returns>
        List<ShoppingCartItem> GetCartItems(IEnumerable<long> cartItemIds, long shopbranchId);

        /// <summary>
        /// 获取购物车购物项
        /// </summary>
        /// <param name="skuIds">SKUId</param>
        /// <returns></returns>
        List<ShoppingCartItem> GetCartItems(IEnumerable<string> skuIds, long memberId, long shopbranchId);

        long GetCartProductQuantity(long memberId, long shopbranchId, long productId = 0, string skuId = "");
        List<ShoppingCartItem> GetCartQuantityByIds(long memberId, IEnumerable<long> productIds, long shopbranchId);
    }
}
