using Himall.Core;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web;
using Himall.IServices;
using Himall.CommonModel;
using Himall.Model;
using Himall.IServices.QueryModel;
using Himall.SmallProgAPI.Helper;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class MobileHomeProductsController : BaseSellerController
    {
        IMobileHomeProductsService _iMobileHomeProductsService;
        IBrandService _iBrandService;
        ICategoryService _iCategoryService;
        IShopCategoryService _iShopCategoryService;
        private MobileHomeProducts mobileHomeproduct;
        public MobileHomeProductsController(
             IMobileHomeProductsService iMobileHomeProductsService,
            IBrandService iBrandService,
            ICategoryService iCategoryService,
            IShopCategoryService iShopCategoryService

            )
        {
             _iBrandService = iBrandService;
            _iCategoryService = iCategoryService;
            _iMobileHomeProductsService = iMobileHomeProductsService;
            _iShopCategoryService = iShopCategoryService;
            mobileHomeproduct = new MobileHomeProducts(_iMobileHomeProductsService, _iBrandService, _iCategoryService, _iShopCategoryService);
        }
        public void AddHomeProducts(long shopId, string productIds, PlatformType platformType)
        {
            IEnumerable<long> enumerable = from item in productIds.Split(new char[] { ',' }) select long.Parse(item);
            this._iMobileHomeProductsService.AddProductsToHomePage(shopId, platformType, enumerable);
        }

        public void Delete(long shopId, long id)
        {
            this._iMobileHomeProductsService.Delete(shopId, id);
        }

        public void DeleteList(long[] ids)
        {
            this._iMobileHomeProductsService.DeleteList(ids);
        }

        public object GetAllHomeProductIds(long shopId, PlatformType platformType)
        {
            return (from item in this._iMobileHomeProductsService.GetMobileHomePageProducts(shopId, platformType) select item.ProductId);
        }

        public object GetMobileHomeProducts(long shopId, PlatformType platformType, int page, int rows, string keyWords, long? categoryId = new long?())
        {
            ProductQuery productQuery = new ProductQuery
            {
                CategoryId = categoryId,
                KeyWords = keyWords,
                PageSize = rows,
                PageNo = page
            };
            ObsoletePageModel<MobileHomeProductsInfo> model = this._iMobileHomeProductsService.GetMobileHomePageProducts(shopId, platformType, productQuery);
            IBrandService brandService = this._iBrandService;
            ICategoryService categoryService = this._iCategoryService;
            return new
            {
                rows = model.Models.ToArray().Select(delegate(MobileHomeProductsInfo item)
                {
                    BrandInfo brand = brandService.GetBrand(item.Himall_Products.BrandId);
                    return new { name = item.Himall_Products.ProductName, image = item.Himall_Products.GetImage(ImageSize.Size_50, 1), price = item.Himall_Products.MinSalePrice.ToString("F2"), brand = (brand == null) ? "" : brand.Name, sequence = item.Sequence, categoryName = categoryService.GetCategory(long.Parse(categoryService.GetCategory(item.Himall_Products.CategoryId).Path.Split(new char[] { '|' }).Last<string>())).Name, id = item.Id, productId = item.ProductId };
                }),
                total = model.Total
            };
        }

        public object GetSellerMobileHomePageProducts(long shopId, PlatformType platformType, int page, int rows, string brandName, long? categoryId = new long?())
        {
            ProductQuery productQuery = new ProductQuery
            {
                ShopCategoryId = categoryId,
                KeyWords = brandName,
                PageSize = rows,
                PageNo = page
            };
            ObsoletePageModel<MobileHomeProductsInfo> model = this._iMobileHomeProductsService.GetSellerMobileHomePageProducts(shopId, platformType, productQuery);
            IBrandService brandService = this._iBrandService;
            IShopCategoryService service = this._iShopCategoryService;
            return new
            {
                rows = model.Models.ToArray().Select(delegate(MobileHomeProductsInfo item)
                {
                    BrandInfo brand = brandService.GetBrand(item.Himall_Products.BrandId);
                    ProductShopCategoryInfo info2 = item.Himall_Products.Himall_ProductShopCategories.FirstOrDefault<ProductShopCategoryInfo>();
                    return new { name = item.Himall_Products.ProductName, image = item.Himall_Products.GetImage(ImageSize.Size_50, 1), price = item.Himall_Products.MinSalePrice.ToString("F2"), brand = (brand == null) ? "" : brand.Name, sequence = item.Sequence, id = item.Id, categoryName = (info2 == null) ? "" : info2.ShopCategoryInfo.Name, productId = item.ProductId };
                }),
                total = model.Total
            };
        }

        public void UpdateSequence(long shopId, long id, short sequence)
        {
            this._iMobileHomeProductsService.UpdateSequence(shopId, id, sequence);
        }

    }
}