using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Entity;
using Himall.Core;
using System.Data.Entity.Infrastructure;
using Himall.Service.Market.Business;
using System.Drawing;
using System.IO;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.User;
using System.Linq.Expressions;

namespace Himall.Service
{
    public class BranchCartService : ServiceBase, IBranchCartService
    {
        // Methods
        public void AddToCart(IEnumerable<ShoppingCartItem> cartItems, long memberId)
        {
            using (List<ShoppingCartItem>.Enumerator enumerator = cartItems.ToList<ShoppingCartItem>().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ShoppingCartItem cartItem = enumerator.Current;
                    if (cartItem.ShopBranchId.HasValue)
                    {
                        this.CheckCartItem(cartItem.SkuId, cartItem.Quantity, memberId, cartItem.ShopBranchId.Value);
                        ShoppingCartItemInfo info = base.Context.ShoppingCartItemInfo.FirstOrDefault<ShoppingCartItemInfo>(item => (((item.UserId == memberId) && (item.SkuId == cartItem.SkuId)) && item.ShopBranchId.HasValue) && (item.ShopBranchId.Value == cartItem.ShopBranchId.Value));
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
                                AddTime = DateTime.Now,
                                ShopBranchId = new long?(cartItem.ShopBranchId.Value)
                            };
                            base.Context.ShoppingCartItemInfo.Add(entity);
                        }
                    }
                }
            }
            base.Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memberId));
        }

        public void AddToCart(string skuId, int count, long memberId, long shopbranchId)
        {
            if (count != 0)
            {
                this.CheckCartItem(skuId, count, memberId, shopbranchId);
                ShoppingCartItemInfo info = base.Context.ShoppingCartItemInfo.FirstOrDefault<ShoppingCartItemInfo>(item => (((item.UserId == memberId) && (item.SkuId == skuId)) && item.ShopBranchId.HasValue) && (item.ShopBranchId.Value == shopbranchId));
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
                        AddTime = DateTime.Now,
                        ShopBranchId = new long?(shopbranchId)
                    };
                    base.Context.ShoppingCartItemInfo.Add(entity);
                }
                base.Context.SaveChanges();
                Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memberId));
            }
        }

        private void CheckCartItem(string skuId, int count, long memberId, long shopbranchId)
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

        public void ClearCart(long memeberId, long shopbranchId)
        {
            base.Context.ShoppingCartItemInfo.Remove<ShoppingCartItemInfo>(item => ((item.UserId == memeberId) && item.ShopBranchId.HasValue) && (item.ShopBranchId.Value == shopbranchId));
            base.Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memeberId));
        }

        public void DeleteCartItem(IEnumerable<string> skuIds, long memberId, long shopbranchId)
        {
            base.Context.ShoppingCartItemInfo.Remove<ShoppingCartItemInfo>(item => ((skuIds.Contains<string>(item.SkuId) && (item.UserId == memberId)) && item.ShopBranchId.HasValue) && (item.ShopBranchId.Value == shopbranchId));
            base.Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memberId));
        }

        public void DeleteCartItem(string skuId, long memberId, long shopbranchId)
        {
            base.Context.ShoppingCartItemInfo.Remove<ShoppingCartItemInfo>(item => (((item.SkuId == skuId) && (item.UserId == memberId)) && item.ShopBranchId.HasValue) && (item.ShopBranchId.Value == shopbranchId));
            base.Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memberId));
        }

        public ShoppingCartInfo GetCart(long memeberId, long shopbranchId)
        {
            Func<ShoppingCartItem, bool> predicate = null;
            ShoppingCartInfo data = new ShoppingCartInfo
            {
                MemberId = memeberId
            };
            if (Cache.Exists(CacheKeyCollection.CACHE_BRANCHCART(memeberId)))
            {
                data = Cache.Get<ShoppingCartInfo>(CacheKeyCollection.CACHE_BRANCHCART(memeberId));
            }
            else
            {
                IQueryable<ShoppingCartItemInfo> queryable = from item in base.Context.ShoppingCartItemInfo
                                                             where (item.UserId == memeberId) && item.ShopBranchId.HasValue
                                                             select item;
                data.Items = (from item in queryable select new ShoppingCartItem { Id = item.Id, SkuId = item.SkuId, Quantity = item.Quantity, AddTime = item.AddTime, ProductId = item.ProductId, ShopBranchId = item.ShopBranchId }).ToList<ShoppingCartItem>();
                Cache.Insert<ShoppingCartInfo>(CacheKeyCollection.CACHE_BRANCHCART(memeberId), data, 600);
            }
            if (shopbranchId > 0L)
            {
                ShoppingCartInfo info2 = new ShoppingCartInfo
                {
                    MemberId = memeberId
                };
                if (predicate == null)
                {
                    predicate = delegate(ShoppingCartItem x)
                    {
                        long? shopBranchId = x.ShopBranchId;
                        long num1 = shopbranchId;
                        return (shopBranchId.GetValueOrDefault() == num1) && shopBranchId.HasValue;
                    };
                }
                info2.Items = data.Items.Where<ShoppingCartItem>(predicate);
                return info2;
            }
            return data;
        }

        public IQueryable<ShoppingCartItem> GetCartItems(IEnumerable<long> cartItemIds, long shopbranchId)
        {
            return (from item in base.Context.ShoppingCartItemInfo.FindBy<ShoppingCartItemInfo>(item => cartItemIds.Contains<long>(item.Id)) select new ShoppingCartItem { Id = item.Id, SkuId = item.SkuId, Quantity = item.Quantity, ProductId = item.ProductId, AddTime = item.AddTime, ShopBranchId = item.ShopBranchId });
        }

        public IQueryable<ShoppingCartItem> GetCartItems(IEnumerable<string> skuIds, long memberId, long shopbranchId)
        {
            return (from item in base.Context.ShoppingCartItemInfo
                    where (((item.UserId == memberId) && skuIds.Contains<string>(item.SkuId)) && item.ShopBranchId.HasValue) && (item.ShopBranchId.Value == shopbranchId)
                    select new ShoppingCartItem { Id = item.Id, SkuId = item.SkuId, Quantity = item.Quantity, ProductId = item.ProductId, AddTime = item.AddTime, ShopBranchId = item.ShopBranchId });
        }

        public ShoppingCartInfo GetCartNoCache(long memeberId, long shopbranchId)
        {
            ShoppingCartInfo info = new ShoppingCartInfo
            {
                MemberId = memeberId
            };
            IQueryable<ShoppingCartItemInfo> queryable = from item in base.Context.ShoppingCartItemInfo
                                                         where ((item.UserId == memeberId) && item.ShopBranchId.HasValue) && ((item.ShopBranchId.Value == shopbranchId) || (shopbranchId == 0L))
                                                         select item;
            info.Items = from item in queryable select new ShoppingCartItem { Id = item.Id, SkuId = item.SkuId, Quantity = item.Quantity, AddTime = item.AddTime, ProductId = item.ProductId, ShopBranchId = item.ShopBranchId };
            return info;
        }

        public int GetCartProductQuantity(long memberId, long shopbranchId, long productId = 0L, string skuId = "")
        {
            IQueryable<ShoppingCartItemInfo> queryable;
            int num = 0;
            if (productId > 0L)
            {
                queryable = from p in base.Context.ShoppingCartItemInfo
                            where (((p.UserId == memberId) && (p.ProductId == productId)) && p.ShopBranchId.HasValue) && (p.ShopBranchId.Value == shopbranchId)
                            select p;
                if (queryable != null)
                {
                    num += queryable.Sum<ShoppingCartItemInfo>((Expression<Func<ShoppingCartItemInfo, int>>)(d => d.Quantity));
                }
                return num;
            }
            queryable = from p in base.Context.ShoppingCartItemInfo
                        where (((p.UserId == memberId) && (p.SkuId == skuId)) && p.ShopBranchId.HasValue) && (p.ShopBranchId.Value == shopbranchId)
                        select p;
            if (queryable != null)
            {
                num += queryable.Sum<ShoppingCartItemInfo>((Expression<Func<ShoppingCartItemInfo, int>>)(d => d.Quantity));
            }
            return num;
        }

        public IQueryable<ShoppingCartItem> GetCartQuantityByIds(long memberId, IEnumerable<long> productIds, long shopbranchId)
        {
            return (from item in base.Context.ShoppingCartItemInfo
                    where (((item.UserId == memberId) && productIds.Contains<long>(item.ProductId)) && item.ShopBranchId.HasValue) && (item.ShopBranchId.Value == shopbranchId)
                    select new ShoppingCartItem { Id = item.Id, SkuId = item.SkuId, Quantity = item.Quantity, ProductId = item.ProductId, AddTime = item.AddTime, ShopBranchId = item.ShopBranchId });
        }

        public void UpdateCart(string skuId, int count, long memberId, long shopbranchId)
        {
            this.CheckCartItem(skuId, count, memberId, shopbranchId);
            ShoppingCartItemInfo info = base.Context.ShoppingCartItemInfo.FirstOrDefault<ShoppingCartItemInfo>(item => (((item.UserId == memberId) && (item.SkuId == skuId)) && item.ShopBranchId.HasValue) && (item.ShopBranchId.Value == shopbranchId));
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
                    AddTime = DateTime.Now,
                    ShopBranchId = new long?(shopbranchId)
                };
                base.Context.ShoppingCartItemInfo.Add(entity);
            }
            base.Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memberId));
        }

    }
}
