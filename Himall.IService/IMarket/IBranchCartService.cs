using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface IBranchCartService:IService
    {
        /// <summary>
        /// 加入购物车
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="count"></param>
        /// <param name="memberId"></param>
        /// <param name="shopbranchId"></param>
        void AddToCart(string skuId, int count, long memberId, long shopbranchId);
        /// <summary>
        /// 加入购物车
        /// </summary>
        /// <param name="cartItems"></param>
        /// <param name="memberId"></param>
        void AddToCart(IEnumerable<ShoppingCartItem> cartItems, long memberId);
        /// <summary>
        /// 更新购物车
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="count"></param>
        /// <param name="memberId"></param>
        /// <param name="shopbranchId"></param>
        void UpdateCart(string skuId, int count, long memberId, long shopbranchId);
        /// <summary>
        /// 清空购物车
        /// </summary>
        /// <param name="memeberId"></param>
        /// <param name="shopbranchId"></param>
        void ClearCart(long memeberId, long shopbranchId);
        /// <summary>
        /// 删除购物车项
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="memberId"></param>
        /// <param name="shopbranchId"></param>
        void DeleteCartItem(string skuId, long memberId, long shopbranchId);
        /// <summary>
        /// 删除购物车项
        /// </summary>
        /// <param name="skuIds"></param>
        /// <param name="memberId"></param>
        /// <param name="shopbranchId"></param>
        void DeleteCartItem(IEnumerable<string> skuIds, long memberId, long shopbranchId);
        /// <summary>
        /// 读取购物车
        /// </summary>
        /// <param name="memeberId"></param>
        /// <param name="shopbranchId"></param>
        /// <returns></returns>
        ShoppingCartInfo GetCart(long memeberId, long shopbranchId);
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="memeberId"></param>
        /// <param name="shopbranchId"></param>
        /// <returns></returns>
        ShoppingCartInfo GetCartNoCache(long memeberId, long shopbranchId);
        /// <summary>
        /// 读取购物车某个
        /// </summary>
        /// <param name="cartItemIds"></param>
        /// <param name="shopbranchId"></param>
        /// <returns></returns>
        IQueryable<ShoppingCartItem> GetCartItems(IEnumerable<long> cartItemIds, long shopbranchId);
        /// <summary>
        /// 读取购物车某个
        /// </summary>
        /// <param name="cartItemIds"></param>
        /// <param name="shopbranchId"></param>
        /// <returns></returns>
        IQueryable<ShoppingCartItem> GetCartItems(IEnumerable<string> skuIds, long memberId, long shopbranchId);
        /// <summary>
        /// 过去购物车产品数量
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="shopbranchId"></param>
        /// <param name="productId"></param>
        /// <param name="skuId"></param>
        /// <returns></returns>
        int GetCartProductQuantity(long memberId, long shopbranchId, long productId = 0, string skuId = "");
        /// <summary>
        /// 根据ID查看数量
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="productIds"></param>
        /// <param name="shopbranchId"></param>
        /// <returns></returns>
        IQueryable<ShoppingCartItem> GetCartQuantityByIds(long memberId, IEnumerable<long> productIds, long shopbranchId);
    }
}
