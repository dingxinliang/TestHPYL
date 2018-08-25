using Himall.CommonModel;
using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;


namespace Himall.Service
{
    public class CartService : ServiceBase, ICartService
    {
        /// <summary>
        /// 加入购物车
        /// </summary>
        /// <param name="cartItems"></param>
        /// <param name="memberId"></param>
        public void AddToCart(IEnumerable<ShoppingCartItem> cartItems, long memberId)
        {
            using (List<ShoppingCartItem>.Enumerator enumerator = cartItems.ToList<ShoppingCartItem>().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ShoppingCartItem cartItem = enumerator.Current;
                    this.CheckCartItem(cartItem.SkuId, cartItem.Quantity, memberId);
                    ShoppingCartItemInfo info = base.Context.ShoppingCartItemInfo.FirstOrDefault<ShoppingCartItemInfo>(item => ((item.UserId == memberId) && (item.SkuId == cartItem.SkuId)) && !item.ShopBranchId.HasValue);
                    if (info != null)
                    {
                        info.Quantity += cartItem.Quantity;
                    }
                    else
                    {
                        long num = long.Parse(cartItem.SkuId.Split(new char[] { '_' })[0]);
                        ShoppingCartItemInfo entity = new ShoppingCartItemInfo
                        {
                            UserId = memberId,
                            Quantity = cartItem.Quantity,
                            SkuId = cartItem.SkuId,
                            ProductId = num,
                            AddTime = DateTime.Now
                        };
                        base.Context.ShoppingCartItemInfo.Add(entity);
                    }
                }
            }
            base.Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_CART(memberId));
        }
        /// <summary>
        /// 加入购物车
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="count"></param>
        /// <param name="memberId"></param>
        public void AddToCart(string skuId, int count, long memberId)
        {
            if (count != 0)
            {
                this.CheckCartItem(skuId, count, memberId);
                ShoppingCartItemInfo info = base.Context.ShoppingCartItemInfo.FirstOrDefault<ShoppingCartItemInfo>(item => ((item.UserId == memberId) && (item.SkuId == skuId)) && !item.ShopBranchId.HasValue);
                if (info != null)
                {
                    info.Quantity += count;
                }
                else if (count > 0)
                {
                    long num = long.Parse(skuId.Split(new char[] { '_' })[0]);
                    ShoppingCartItemInfo entity = new ShoppingCartItemInfo
                    {
                        UserId = memberId,
                        Quantity = count,
                        SkuId = skuId,
                        ProductId = num,
                        AddTime = DateTime.Now
                    };
                    base.Context.ShoppingCartItemInfo.Add(entity);
                }
                base.Context.SaveChanges();
                Cache.Remove(CacheKeyCollection.CACHE_CART(memberId));
            }
        }
        /// <summary>
        /// 检查购物车
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="count"></param>
        /// <param name="memberId"></param>
        private void CheckCartItem(string skuId, int count, long memberId)
        {
            if (string.IsNullOrWhiteSpace(skuId))
            {
                throw new InvalidPropertyException("SKUId不能为空");
            }
            if (count < 0)
            {
                throw new InvalidPropertyException("商品数量不能小于0");
            }
            if (base.Context.UserMemberInfo.FirstOrDefault<UserMemberInfo>(item => (item.Id == memberId)) == null)
            {
                throw new InvalidPropertyException("会员Id" + memberId + "不存在");
            }
        }
        /// <summary>
        /// 检查
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="count"></param>
        /// <param name="memberId"></param>
        /// <param name="shopbranchId"></param>
        private void CheckCartItemByBranch(string skuId, int count, long memberId, long shopbranchId)
        {
            if (string.IsNullOrWhiteSpace(skuId))
            {
                throw new InvalidPropertyException("SKUId不能为空");
            }
            if (count < 0)
            {
                throw new InvalidPropertyException("商品数量不能小于0");
            }
            if (shopbranchId <= 0L)
            {
                throw new InvalidPropertyException("门店ID不能为空");
            }
            if (base.Context.UserMemberInfo.FirstOrDefault<UserMemberInfo>(item => (item.Id == memberId)) == null)
            {
                throw new InvalidPropertyException("会员Id" + memberId + "不存在");
            }
        }

        public void ClearCart(long memeberId)
        {
            base.Context.ShoppingCartItemInfo.Remove<ShoppingCartItemInfo>(item => (item.UserId == memeberId) && !item.ShopBranchId.HasValue);
            base.Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_CART(memeberId));
        }

        public void DeleteCartItem(IEnumerable<string> skuIds, long memberId)
        {
            base.Context.ShoppingCartItemInfo.Remove<ShoppingCartItemInfo>(item => (skuIds.Contains<string>(item.SkuId) && (item.UserId == memberId)) && !item.ShopBranchId.HasValue);
            base.Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_CART(memberId));
        }

        public void DeleteCartItem(string skuId, long memberId)
        {
            base.Context.ShoppingCartItemInfo.Remove<ShoppingCartItemInfo>(item => ((item.SkuId == skuId) && (item.UserId == memberId)) && !item.ShopBranchId.HasValue);
            base.Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_CART(memberId));
        }

        public ShoppingCartInfo GetCart(long memeberId)
        {
            if (Cache.Exists(CacheKeyCollection.CACHE_CART(memeberId)))
            {
                return Cache.Get<ShoppingCartInfo>(CacheKeyCollection.CACHE_CART(memeberId));
            }
            ShoppingCartInfo data = new ShoppingCartInfo
            {
                MemberId = memeberId
            };
            IQueryable<ShoppingCartItemInfo> queryable = from item in base.Context.ShoppingCartItemInfo
                                                         where (item.UserId == memeberId) && !item.ShopBranchId.HasValue
                                                         select item;
            data.Items = from item in queryable select new ShoppingCartItem { Id = item.Id, SkuId = item.SkuId, Quantity = item.Quantity, AddTime = item.AddTime, ProductId = item.ProductId, ShopBranchId = item.ShopBranchId };
            using (IEnumerator<ShoppingCartItem> enumerator = data.Items.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    long? nullable;
                    ShoppingCartItem item = enumerator.Current;
                    if (item.ShopBranchId.HasValue && (((nullable = item.ShopBranchId).GetValueOrDefault() > 0L) && nullable.HasValue))
                    {
                        ShopBranchSkusInfo info2 = base.Context.ShopBranchSkusInfo.FirstOrDefault<ShopBranchSkusInfo>(d => (d.SkuId == item.SkuId) && (d.ShopBranchId == item.ShopBranchId));
                        item.Status = 1;
                        if ((info2 != null) && (info2.Status == ShopBranchSkuStatus.Normal))
                        {
                            item.Status = 0;
                            if (item.Quantity > info2.Stock)
                            {
                                item.Status = 2;
                            }
                        }
                        else
                        {
                            item.Status = 1;
                        }
                    }
                }
            }
            Cache.Insert<ShoppingCartInfo>(CacheKeyCollection.CACHE_CART(memeberId), data, 600);
            return data;
        }

        public IQueryable<ShoppingCartItem> GetCartItems(IEnumerable<long> cartItemIds)
        {
            return (from item in base.Context.ShoppingCartItemInfo.FindBy<ShoppingCartItemInfo>(item => cartItemIds.Contains<long>(item.Id)) select new ShoppingCartItem { Id = item.Id, SkuId = item.SkuId, Quantity = item.Quantity, ProductId = item.ProductId, AddTime = item.AddTime, ShopBranchId = item.ShopBranchId });
        }
        /// <summary>
        /// 读取购物项
        /// </summary>
        /// <param name="skuIds"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public IQueryable<ShoppingCartItem> GetCartItems(IEnumerable<string> skuIds, long memberId)
        {
            return (from item in base.Context.ShoppingCartItemInfo
                    where ((item.UserId == memberId) && skuIds.Contains<string>(item.SkuId)) && !item.ShopBranchId.HasValue
                    select new ShoppingCartItem { Id = item.Id, SkuId = item.SkuId, Quantity = item.Quantity, ProductId = item.ProductId, AddTime = item.AddTime });
        }
        /// <summary>
        /// 读取购物车数量
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="productId"></param>
        /// <param name="skuId"></param>
        /// <returns></returns>
        public int GetCartProductQuantity(long memberId, long productId = 0L, string skuId = "")
        {
            IQueryable<ShoppingCartItemInfo> queryable;
            int num = 0;
            if (productId > 0L)
            {
                queryable = from p in base.Context.ShoppingCartItemInfo
                            where ((p.UserId == memberId) && (p.ProductId == productId)) && !p.ShopBranchId.HasValue
                            select p;
                if (queryable != null)
                {
                    num += queryable.Sum<ShoppingCartItemInfo>((Expression<Func<ShoppingCartItemInfo, int>>)(d => d.Quantity));
                }
                return num;
            }
            queryable = from p in base.Context.ShoppingCartItemInfo
                        where ((p.UserId == memberId) && (p.SkuId == skuId)) && !p.ShopBranchId.HasValue
                        select p;
            if (queryable != null)
            {
                num += queryable.Sum<ShoppingCartItemInfo>((Expression<Func<ShoppingCartItemInfo, int>>)(d => d.Quantity));
            }
            return num;
        }
        /// <summary>
        /// 读取购物车信息
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public IQueryable<ShoppingCartItem> GetCartQuantityByIds(long memberId, IEnumerable<long> productIds)
        {
            return (from item in base.Context.ShoppingCartItemInfo
                    where ((item.UserId == memberId) && productIds.Contains<long>(item.ProductId)) && !item.ShopBranchId.HasValue
                    select new ShoppingCartItem { Id = item.Id, SkuId = item.SkuId, Quantity = item.Quantity, ProductId = item.ProductId, AddTime = item.AddTime });
        }
        /// <summary>
        /// 更新购物车
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="count"></param>
        /// <param name="memberId"></param>
        public void UpdateCart(string skuId, int count, long memberId)
        {
            this.CheckCartItem(skuId, count, memberId);
            ShoppingCartItemInfo info = base.Context.ShoppingCartItemInfo.FirstOrDefault<ShoppingCartItemInfo>(item => ((item.UserId == memberId) && (item.SkuId == skuId)) && !item.ShopBranchId.HasValue);
            if (info != null)
            {
                if (count == 0)
                {
                    base.Context.ShoppingCartItemInfo.Remove<ShoppingCartItemInfo>(new object[] { info.Id });
                }
                else
                {
                    info.Quantity = count;
                }
            }
            else if (count > 0)
            {
                long num = long.Parse(skuId.Split(new char[] { '_' })[0]);
                ShoppingCartItemInfo entity = new ShoppingCartItemInfo
                {
                    UserId = memberId,
                    Quantity = count,
                    SkuId = skuId,
                    ProductId = num,
                    AddTime = DateTime.Now
                };
                base.Context.ShoppingCartItemInfo.Add(entity);
            }
            base.Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_CART(memberId));
        }

    }
}
