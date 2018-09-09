﻿using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Himall.Service
{
    public class MobileHomeProductsService : ServiceBase, IMobileHomeProductsService
    {
        public void AddProductsToHomePage(long shopId, PlatformType platformType, IEnumerable<long> productIds)
        {
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var existHomepageProductIds = Context.MobileHomeProductsInfo.Where(item => item.ShopId == shopId && item.PlatFormType == platformType).Select(item => item.ProductId);//获取当前诊所已添加的首页诊疗项目Id
            var notExistProductIds = productIds.Where(item => !existHomepageProductIds.Contains(item));//从待添加的诊疗项目中去除已添加的诊疗项目

            foreach (var productId in notExistProductIds)
            {
                var product = productService.GetProduct(productId);
                if (!product.IsDeleted)
                {
                    if (shopId != 0 && product.ShopId != shopId)//诊所添加首页诊疗项目时，判断该诊疗项目是否为该诊所的诊疗项目
                        throw new Himall.Core.HimallException("待添加至首页的诊疗项目不得包含非本诊所诊疗项目");

                    var mobileHomepageProduct = new MobileHomeProductsInfo()
                    {
                        PlatFormType = platformType,
                        Sequence = 1,
                        ProductId = productId,
                        ShopId = shopId
                    };
                    Context.MobileHomeProductsInfo.Add(mobileHomepageProduct);
                }
            }
            var delProductIds = existHomepageProductIds.Where(e => !productIds.Contains(e));
            if (delProductIds.Count() > 0)
            {
                var delProduct = Context.MobileHomeProductsInfo.Where(item => item.ShopId == shopId && item.PlatFormType == platformType && delProductIds.Contains(item.ProductId));
                Context.MobileHomeProductsInfo.RemoveRange(delProduct);
            }
            Context.SaveChanges();
        }

        public void UpdateSequence(long shopId, long id, short sequence)
        {
            var mobileHomeProduct = Context.MobileHomeProductsInfo.FirstOrDefault(item => item.Id == id && item.ShopId == shopId);
            if (mobileHomeProduct == null)
                throw new Himall.Core.HimallException(string.Format("不存在Id为{0}的首页诊疗项目设置", id));
            mobileHomeProduct.Sequence = sequence;
            Context.SaveChanges();
        }

        public void Delete(long shopId, long id)
        {
            Context.MobileHomeProductsInfo.Remove(id);
            Context.SaveChanges();
        }
        /// <summary>
        /// 多个删除
        /// </summary>
        /// <param name="id"></param>
        public void DeleteList(long[] id)
        {
            IQueryable<MobileHomeProductsInfo> entities = base.Context.MobileHomeProductsInfo.FindBy<MobileHomeProductsInfo>(item => id.Contains<long>(item.Id));
            base.Context.MobileHomeProductsInfo.RemoveRange(entities);
            base.Context.SaveChanges();

        }
        public ObsoletePageModel<Model.MobileHomeProductsInfo> GetMobileHomePageProducts(long shopId, PlatformType platformType, ProductQuery productQuery)
        {
            var homeProducts = Context.MobileHomeProductsInfo
                .FindBy(item => item.ShopId == shopId && item.PlatFormType == platformType);

            if (!string.IsNullOrWhiteSpace(productQuery.KeyWords))
            {
                productQuery.KeyWords = productQuery.KeyWords.Trim();
                var brandIds = Context.BrandInfo.FindBy(item => item.Name.Contains(productQuery.KeyWords) && item.IsDeleted == false).Select(item => item.Id).ToArray();
                homeProducts = homeProducts.FindBy(item => item.Himall_Products.ProductName.Contains(productQuery.KeyWords)||brandIds.Contains(item.Himall_Products.BrandId));
            }

            if (productQuery.CategoryId.HasValue)
            {
                homeProducts = homeProducts.FindBy(item => ("|" + item.Himall_Products.CategoryPath + "|").Contains("|" + productQuery.CategoryId.Value + "|"));
            }

            int total = homeProducts.Count();

            homeProducts= homeProducts.OrderBy(item => item.Sequence).ThenBy(item=>item.Id).Skip((productQuery.PageNo - 1) * productQuery.PageSize).Take(productQuery.PageSize);
            ObsoletePageModel<MobileHomeProductsInfo> pageModel = new ObsoletePageModel<MobileHomeProductsInfo>() { Models = homeProducts, Total = total };
            return pageModel;
        }

        public ObsoletePageModel<Model.MobileHomeProductsInfo> GetSellerMobileHomePageProducts(long shopId, PlatformType platformType, ProductQuery productQuery)
        {
            var homeProducts = Context.MobileHomeProductsInfo
                .FindBy(item => item.ShopId == shopId && item.PlatFormType == platformType);

            if (!string.IsNullOrWhiteSpace(productQuery.KeyWords))
            {
                productQuery.KeyWords = productQuery.KeyWords.Trim();
                var brandIds = Context.BrandInfo.FindBy(item => item.Name.Contains(productQuery.BrandNameKeyword) && item.IsDeleted == false).Select(item => item.Id).ToArray();
                homeProducts = homeProducts.FindBy(item => item.Himall_Products.ProductName.Contains(productQuery.KeyWords) || brandIds.Contains(item.Himall_Products.BrandId));
            }
            IEnumerable<long> productIds = new long[] { };
                
            

            if (productQuery.ShopCategoryId.HasValue)
            {
                productIds = Context.ProductShopCategoryInfo
                    .Where(item => item.ShopCategoryInfo.Id == productQuery.ShopCategoryId ||item.ShopCategoryInfo.ParentCategoryId == productQuery.ShopCategoryId).Select(item => item.ProductId);
                homeProducts = homeProducts.FindBy(item => productIds.Contains(item.ProductId));
            }

            int total=homeProducts.Count();
            homeProducts= homeProducts.OrderBy(item => item.Sequence).Skip((productQuery.PageNo - 1) * productQuery.PageSize).Take(productQuery.PageSize);
            ObsoletePageModel<MobileHomeProductsInfo> pageModel = new ObsoletePageModel<MobileHomeProductsInfo>() { Models = homeProducts, Total = total };
            return pageModel;
        }

        public IQueryable<MobileHomeProductsInfo> GetMobileHomePageProducts(long shopId, PlatformType platformType)
        {
            var products = (from mp in Context.MobileHomeProductsInfo
                           join p in Context.ProductInfo on mp.ProductId equals p.Id
                           where mp.ShopId == shopId && mp.PlatFormType == platformType && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && p.AuditStatus == ProductInfo.ProductAuditStatus.Audited && p.IsDeleted==false
                           select mp);

            return products;
        }
    }
}
