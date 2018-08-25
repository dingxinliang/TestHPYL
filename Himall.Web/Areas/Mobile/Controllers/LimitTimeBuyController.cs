using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Areas.Web.Helper;
using Himall.Web.Areas.Mobile.Models;
using Himall.Web.Models;
using Quartz;
using Quartz.Impl;
using Himall.Web.App_Code.Common;
using Himall.Application;
using Himall.Core;
using System.Linq.Expressions;
using System.Reflection;
using Himall.Service;
using Himall.CommonModel;

namespace Himall.Web.Areas.Mobile.Controllers
{
    //ToDO...VIEWbag太多
    public class LimitTimeBuyController : BaseMobileTemplatesController
    {
        // Fields
        private ICommentService _iCommentService;
        private IConsultationService _iConsultationService;
        private ICustomerService _iCustomerService;
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private IProductDescriptionTemplateService _iProductDescriptionTemplateService;
        private IProductService _iProductService;
        private IShopCategoryService _iShopCategoryService;
        private IShopService _iShopService;
        private ITypeService _iTypeService;
        private IVShopService _iVShopService;

        // Methods
        public LimitTimeBuyController(IShopCategoryService iShopCategoryService, ILimitTimeBuyService iLimitTimeBuyService, IShopService iShopService, IProductService iProductService, ICommentService iCommentService, IVShopService iVShopService, IConsultationService iConsultationService, IProductDescriptionTemplateService iProductDescriptionTemplateService, ICustomerService iCustomerService, ITypeService iTypeService)
        {
            this._iShopCategoryService = iShopCategoryService;
            this._iLimitTimeBuyService = iLimitTimeBuyService;
            this._iShopService = iShopService;
            this._iProductService = iProductService;
            this._iCommentService = iCommentService;
            this._iVShopService = iVShopService;
            this._iProductDescriptionTemplateService = iProductDescriptionTemplateService;
            this._iConsultationService = iConsultationService;
            this._iCustomerService = iCustomerService;
            this._iTypeService = iTypeService;
        }

        [HttpPost]
        public ActionResult AddFavorite(long pid)
        {
            int status = 0;
            this._iProductService.AddFavorite(pid, base.CurrentUser.Id, out status);
            if (status == 0)
            {
                return base.Json(true);
            }
            return base.Json(false);
        }

        [HttpPost]
        public JsonResult CheckLimitTimeBuy(string skuIds, string counts)
        {
            string[] source = skuIds.Split(new char[] { ',' });
            IEnumerable<int> pCountsArr = from t in counts.TrimEnd(new char[] { ',' }).Split(new char[] { ',' }) select int.Parse(t);
            IProductService productService = this._iProductService;
            int index = 0;
            CartItemModel model = source.Select<string, CartItemModel>(delegate(string item)
            {
                SKUInfo sku = productService.GetSku(item);
                int num = pCountsArr.ElementAt<int>(index++);
                return new CartItemModel { id = sku.ProductInfo.Id, count = num };
            }).ToList<CartItemModel>().FirstOrDefault<CartItemModel>();
            int marketSaleCountForUserId = this._iLimitTimeBuyService.GetMarketSaleCountForUserId(model.id, base.CurrentUser.Id);
            int limitCountOfThePeople = this._iLimitTimeBuyService.GetLimitTimeMarketItemByProductId(model.id).LimitCountOfThePeople;
            return base.Json(new { success = limitCountOfThePeople >= (marketSaleCountForUserId + model.count), maxSaleCount = limitCountOfThePeople, remain = limitCountOfThePeople - marketSaleCountForUserId });
        }

        public ActionResult Detail(string id)
        {
            ProductDescriptionTemplateInfo template;
            long num15;
            TimeSpan span;
            TimeSpan span2;
            TimeSpan span3;
            ParameterExpression expression;
            LimitTimeBuyDetailModel model = new LimitTimeBuyDetailModel();
            string str = "";
            LimitTimeProductDetailModel model2 = new LimitTimeProductDetailModel
            {
                MainId = long.Parse(id),
                HotAttentionProducts = new List<HotProductInfo>(),
                HotSaleProducts = new List<HotProductInfo>(),
                Product = new ProductInfo(),
                Shop = new ShopInfoModel(),
                ShopCategory = new List<CategoryJsonModel>(),
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU()
            };
            FlashSaleModel model3 = null;
            ShopInfo shop = null;
            long productId = 0L;
            long result = 0L;
            if (long.TryParse(id, out result))
            {
            }
            if (result == 0L)
            {
                return base.RedirectToAction("Error404", "Error", new { area = "Mobile" });
            }
            model3 = this._iLimitTimeBuyService.Get(result);
            switch (model3.Status)
            {
                case FlashSaleInfo.FlashSaleStatus.Ended:
                    return base.RedirectToAction("Detail", "Product", new { id = model3.ProductId });

                case FlashSaleInfo.FlashSaleStatus.Cancelled:
                    return base.RedirectToAction("Detail", "Product", new { id = model3.ProductId });
            }
            model2.FlashSale = model3;
            if ((model3 == null) || (model3.Status != FlashSaleInfo.FlashSaleStatus.Ongoing))
            {
                model3 = (model3 == null) ? this._iLimitTimeBuyService.GetFlaseSaleByProductId(result) : model3;
                if (model3 == null)
                {
                    return base.RedirectToAction("Error404", "Error", new { area = "Mobile" });
                }
                if (model3.Status != FlashSaleInfo.FlashSaleStatus.Ongoing)
                {
                    return base.RedirectToAction("Detail", "Product", new { id = model3.ProductId });
                }
            }
            if ((model3 != null) && ((model3.Status != FlashSaleInfo.FlashSaleStatus.Ongoing) || (DateTime.Parse(model3.EndDate) < DateTime.Now)))
            {
                return base.RedirectToAction("Detail", "Product", new { id = model3.ProductId });
            }
            model2.MaxSaleCount = model3.LimitCountOfThePeople;
            model2.Title = model3.Title;
            shop = this._iShopService.GetShop(model3.ShopId, false);
            if ((model3 == null) || (model3.Id == 0L))
            {
                return base.RedirectToAction("Error404", "Error", new { area = "Web" });
            }
            ProductInfo product = this._iProductService.GetProduct(model3.ProductId);
            productId = model3.ProductId;
            model2.Product = product;
            model2.ProductDescription = product.ProductDescriptionInfo.ShowMobileDescription;
            if (product.ProductDescriptionInfo.DescriptionPrefixId != 0L)
            {
                template = this._iProductDescriptionTemplateService.GetTemplate(product.ProductDescriptionInfo.DescriptionPrefixId, product.ShopId);
                model2.DescriptionPrefix = (template == null) ? "" : template.Content;
            }
            if (product.ProductDescriptionInfo.DescriptiondSuffixId != 0L)
            {
                template = this._iProductDescriptionTemplateService.GetTemplate(product.ProductDescriptionInfo.DescriptiondSuffixId, product.ShopId);
                model2.DescriptiondSuffix = (template == null) ? "" : template.Content;
            }
            ShopServiceMarkModel shopComprehensiveMark = ShopServiceMark.GetShopComprehensiveMark(shop.Id);
            model2.Shop.PackMark = shopComprehensiveMark.PackMark;
            model2.Shop.ServiceMark = shopComprehensiveMark.ServiceMark;
            model2.Shop.ComprehensiveMark = shopComprehensiveMark.ComprehensiveMark;
            IQueryable<ProductCommentInfo> commentsByProductId = this._iCommentService.GetCommentsByProductId(productId);
            model2.Shop.Name = shop.ShopName;
            Decimal num1;
            if (commentsByProductId != null && Queryable.Count<ProductCommentInfo>(commentsByProductId) != 0)
                num1 = Queryable.Average<ProductCommentInfo>(commentsByProductId, (Expression<Func<ProductCommentInfo, Decimal>>)(p => (Decimal)p.ReviewMark));
            else
                num1 = new Decimal(0);
            model2.Shop.ProductMark = num1;
            model2.Shop.Id = product.ShopId;
            model2.Shop.FreeFreight = shop.FreeFreight;
            model.ProductNum = this._iProductService.GetShopOnsaleProducts(product.ShopId);
            model.FavoriteShopCount = this._iShopService.GetShopFavoritesCount(product.ShopId);
            if (base.CurrentUser == null)
            {
                model.IsFavorite = false;
                model.IsFavoriteShop = false;
            }
            else
            {
                model.IsFavorite = this._iProductService.IsFavorite(product.Id, base.CurrentUser.Id);
                model.IsFavoriteShop = (from item in this._iShopService.GetFavoriteShopInfos(base.CurrentUser.Id) select item.ShopId).ToArray<long>().Contains<long>(product.ShopId);
            }
            List<ShopCategoryInfo> source = this._iShopCategoryService.GetShopCategory(product.ShopId).ToList<ShopCategoryInfo>();
            using (IEnumerator<ShopCategoryInfo> enumerator = (from s in source
                                                               where s.ParentCategoryId == 0L
                                                               select s).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Func<ShopCategoryInfo, bool> predicate = null;
                    ShopCategoryInfo main = enumerator.Current;
                    CategoryJsonModel model5 = new CategoryJsonModel
                    {
                        Name = main.Name,
                        Id = main.Id.ToString(),
                        SubCategory = new List<SecondLevelCategory>()
                    };
                    if (predicate == null)
                    {
                        predicate = s => s.ParentCategoryId == main.Id;
                    }
                    foreach (ShopCategoryInfo info4 in source.Where<ShopCategoryInfo>(predicate))
                    {
                        SecondLevelCategory category = new SecondLevelCategory
                        {
                            Name = info4.Name,
                            Id = info4.Id.ToString()
                        };
                        model5.SubCategory.Add(category);
                    }
                    model2.ShopCategory.Add(model5);
                }
            }
            IQueryable<ProductInfo> hotSaleProduct = this._iProductService.GetHotSaleProduct(shop.Id, 5);
            if (hotSaleProduct != null)
            {
                foreach (ProductInfo info5 in hotSaleProduct.ToArray<ProductInfo>())
                {
                    HotProductInfo info6 = new HotProductInfo
                    {
                        ImgPath = info5.ImagePath,
                        Name = info5.ProductName,
                        Price = info5.MinSalePrice,
                        Id = info5.Id,
                        SaleCount = (int)info5.SaleCounts
                    };
                    model2.HotSaleProducts.Add(info6);
                }
            }
            IQueryable<ProductInfo> hotConcernedProduct = this._iProductService.GetHotConcernedProduct(shop.Id, 5);
            if (hotConcernedProduct != null)
            {
                foreach (ProductInfo info5 in hotConcernedProduct.ToArray<ProductInfo>())
                {
                    HotProductInfo info7 = new HotProductInfo
                    {
                        ImgPath = info5.ImagePath,
                        Name = info5.ProductName,
                        Price = info5.MinSalePrice,
                        Id = info5.Id,
                        SaleCount = info5.ConcernedCount
                    };
                    model2.HotAttentionProducts.Add(info7);
                }
            }
            ProductTypeInfo type = this._iTypeService.GetType(product.TypeId);
            string str2 = ((type == null) || string.IsNullOrEmpty(type.ColorAlias)) ? SpecificationType.Color.ToDescription() : type.ColorAlias;
            string str3 = ((type == null) || string.IsNullOrEmpty(type.SizeAlias)) ? SpecificationType.Size.ToDescription() : type.SizeAlias;
            string str4 = ((type == null) || string.IsNullOrEmpty(type.VersionAlias)) ? SpecificationType.Version.ToDescription() : type.VersionAlias;
            model2.ColorAlias = str2;
            model2.SizeAlias = str3;
            model2.VersionAlias = str4;
            if ((product.SKUInfo != null) && (product.SKUInfo.Count<SKUInfo>() > 0))
            {
                long num3 = 0L;
                long num4 = 0L;
                long num5 = 0L;
                using (IEnumerator<SKUInfo> enumerator3 = product.SKUInfo.GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        Func<ProductSKU, bool> func2 = null;
                        Func<SKUInfo, bool> func3 = null;
                        Func<ProductSKU, bool> func4 = null;
                        Func<SKUInfo, bool> func5 = null;
                        Func<ProductSKU, bool> func6 = null;
                        Func<SKUInfo, bool> func7 = null;
                        SKUInfo sku = enumerator3.Current;
                        string[] strArray = sku.Id.Split(new char[] { '_' });
                        if (strArray.Count<string>() > 0)
                        {
                            if (long.TryParse(strArray[1], out num3))
                            {
                            }
                            if (num3 != 0L)
                            {
                                if (func2 == null)
                                {
                                    func2 = v => v.Value.Equals(sku.Color);
                                }
                                if (!model2.Color.Any<ProductSKU>(func2))
                                {
                                    if (func3 == null)
                                    {
                                        func3 = s => s.Color.Equals(sku.Color);
                                    }
                                    long num6 = product.SKUInfo.Where<SKUInfo>(func3).Sum<SKUInfo>((Func<SKUInfo, long>)(s => s.Stock));
                                    ProductSKU tsku = new ProductSKU
                                    {
                                        Name = "选择" + str2,
                                        EnabledClass = (num6 != 0L) ? "enabled" : "disabled",
                                        SelectedClass = "",
                                        SkuId = num3,
                                        Value = sku.Color,
                                        Img = sku.ShowPic
                                    };
                                    model2.Color.Add(tsku);
                                }
                            }
                        }
                        if (strArray.Count<string>() > 1)
                        {
                            if (long.TryParse(strArray[2], out num4))
                            {
                            }
                            if (num4 != 0L)
                            {
                                if (func4 == null)
                                {
                                    func4 = v => v.Value.Equals(sku.Size);
                                }
                                if (!model2.Size.Any<ProductSKU>(func4))
                                {
                                    if (func5 == null)
                                    {
                                        func5 = s => s.Size.Equals(sku.Size);
                                    }
                                    long num7 = product.SKUInfo.Where<SKUInfo>(func5).Sum<SKUInfo>((Func<SKUInfo, long>)(s1 => s1.Stock));
                                    ProductSKU tsku2 = new ProductSKU
                                    {
                                        Name = "选择" + str3,
                                        EnabledClass = (num7 != 0L) ? "enabled" : "disabled",
                                        SelectedClass = "",
                                        SkuId = num4,
                                        Value = sku.Size
                                    };
                                    model2.Size.Add(tsku2);
                                }
                            }
                        }
                        if (strArray.Count<string>() > 2)
                        {
                            if (long.TryParse(strArray[3], out num5))
                            {
                            }
                            if (num5 != 0L)
                            {
                                if (func6 == null)
                                {
                                    func6 = v => v.Value.Equals(sku.Version);
                                }
                                if (!model2.Version.Any<ProductSKU>(func6))
                                {
                                    if (func7 == null)
                                    {
                                        func7 = s => s.Version.Equals(sku.Version);
                                    }
                                    long num8 = product.SKUInfo.Where<SKUInfo>(func7).Sum<SKUInfo>((Func<SKUInfo, long>)(s => s.Stock));
                                    ProductSKU tsku3 = new ProductSKU
                                    {
                                        Name = "选择" + str4,
                                        EnabledClass = (num8 != 0L) ? "enabled" : "disabled",
                                        SelectedClass = "",
                                        SkuId = num5,
                                        Value = sku.Version
                                    };
                                    model2.Version.Add(tsku3);
                                }
                            }
                        }
                    }
                }
                decimal num9 = 0M;
                decimal num10 = 0M;
                num9 = (from s in product.SKUInfo
                        where s.Stock >= 0L
                        select s).Min<SKUInfo>((Func<SKUInfo, decimal>)(s => s.SalePrice));
                num10 = (from s in product.SKUInfo
                         where s.Stock >= 0L
                         select s).Max<SKUInfo>((Func<SKUInfo, decimal>)(s => s.SalePrice));
                if ((num9 == 0M) && (num10 == 0M))
                {
                    str = product.MinSalePrice.ToString("f2");
                }
                else if (num10 > num9)
                {
                    str = string.Format("{0}-{1}", num9.ToString("f2"), num10.ToString("f2"));
                }
                else
                {
                    str = string.Format("{0}", num9.ToString("f2"));
                }
            }
            model.Price = string.IsNullOrWhiteSpace(str) ? product.MinSalePrice.ToString("f2") : str;
            List<TypeAttributesModel> list2 = new List<TypeAttributesModel>();
            List<ProductAttributeInfo> list3 = this._iProductService.GetProductAttribute(product.Id).ToList<ProductAttributeInfo>();
            using (List<ProductAttributeInfo>.Enumerator enumerator4 = list3.GetEnumerator())
            {
                while (enumerator4.MoveNext())
                {
                    Func<TypeAttributesModel, bool> func9 = null;
                    Func<TypeAttributesModel, bool> func10 = null;
                    Func<TypeAttrValue, bool> func11 = null;
                    Func<AttributeValueInfo, bool> func12 = null;
                    ProductAttributeInfo attr = enumerator4.Current;
                    if (func9 == null)
                    {
                        func9 = p => p.AttrId == attr.AttributeId;
                    }
                    if (!list2.Any<TypeAttributesModel>(func9))
                    {
                        TypeAttributesModel model7 = new TypeAttributesModel
                        {
                            AttrId = attr.AttributeId,
                            AttrValues = new List<TypeAttrValue>(),
                            Name = attr.AttributesInfo.Name
                        };
                        using (IEnumerator<AttributeValueInfo> enumerator5 = attr.AttributesInfo.AttributeValueInfo.GetEnumerator())
                        {
                            while (enumerator5.MoveNext())
                            {
                                Func<ProductAttributeInfo, bool> func8 = null;
                                AttributeValueInfo attrV = enumerator5.Current;
                                if (func8 == null)
                                {
                                    func8 = p => p.ValueId == attrV.Id;
                                }
                                if (list3.Any<ProductAttributeInfo>(func8))
                                {
                                    TypeAttrValue value2 = new TypeAttrValue
                                    {
                                        Id = attrV.Id.ToString(),
                                        Name = attrV.Value
                                    };
                                    model7.AttrValues.Add(value2);
                                }
                            }
                        }
                        list2.Add(model7);
                    }
                    else
                    {
                        if (func10 == null)
                        {
                            func10 = p => p.AttrId == attr.AttributeId;
                        }
                        TypeAttributesModel model9 = list2.FirstOrDefault<TypeAttributesModel>(func10);
                        if (func11 == null)
                        {
                            func11 = p => p.Id == attr.ValueId.ToString();
                        }
                        if (!model9.AttrValues.Any<TypeAttrValue>(func11))
                        {
                            TypeAttrValue value3 = new TypeAttrValue
                            {
                                Id = attr.ValueId.ToString()
                            };
                            if (func12 == null)
                            {
                                func12 = a => a.Id == attr.ValueId;
                            }
                            value3.Name = attr.AttributesInfo.AttributeValueInfo.FirstOrDefault<AttributeValueInfo>(func12).Value;
                            model9.AttrValues.Add(value3);
                        }
                    }
                }
            }
            model.ProductAttrs = list2;
            IEnumerable<ProductCommentInfo> enumerable = Enumerable.Where<ProductCommentInfo>((IEnumerable<ProductCommentInfo>) product.Himall_ProductComments, (Func<ProductCommentInfo, bool>) (item => !item.IsHidden.HasValue || !item.IsHidden.Value));
            int num11 = enumerable.Count<ProductCommentInfo>();
            model.CommentCount = num11;
            IQueryable<ProductConsultationInfo> consultations = this._iConsultationService.GetConsultations(productId);
            model.Consultations = consultations.Count<ProductConsultationInfo>();
            double num12 = num11;
            double num13 = enumerable.Count<ProductCommentInfo>(item => item.ReviewMark >= 4);
            model.NicePercent = (int)((num13 / num12) * 100.0);
            model.Consultations = consultations.Count<ProductConsultationInfo>();
            if (this._iVShopService.GetVShopByShopId(shop.Id) == null)
            {
                model.VShopId = -1L;
            }
            else
            {
                model.VShopId = this._iVShopService.GetVShopByShopId(shop.Id).Id;
            }
            IQueryable<StatisticOrderCommentsInfo> shopStatisticOrderComments = this._iShopService.GetShopStatisticOrderComments(product.ShopId);
            StatisticOrderCommentsInfo info9 = (from c in shopStatisticOrderComments
                                                where ((int)c.CommentKey) == 1
                                                select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info10 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 9
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info11 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 5
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info12 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 2
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info13 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 10
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info14 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 6
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info15 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 3
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info16 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 4
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info17 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 11
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info18 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 12
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info19 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 7
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info20 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 8
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            decimal num14 = 5M;
            if (!(((info9 == null) || (info12 == null)) || shop.IsSelf))
            {
                model.ProductAndDescription = info9.CommentValue;
                model.ProductAndDescriptionPeer = info12.CommentValue;
                model.ProductAndDescriptionMin = info16.CommentValue;
                model.ProductAndDescriptionMax = info15.CommentValue;
            }
            else
            {
                model.ProductAndDescription = num14;
                model.ProductAndDescriptionPeer = num14;
                model.ProductAndDescriptionMin = num14;
                model.ProductAndDescriptionMax = num14;
            }
            if (!(((info10 == null) || (info13 == null)) || shop.IsSelf))
            {
                model.SellerServiceAttitude = info10.CommentValue;
                model.SellerServiceAttitudePeer = info13.CommentValue;
                model.SellerServiceAttitudeMax = info17.CommentValue;
                model.SellerServiceAttitudeMin = info18.CommentValue;
            }
            else
            {
                model.SellerServiceAttitude = num14;
                model.SellerServiceAttitudePeer = num14;
                model.SellerServiceAttitudeMax = num14;
                model.SellerServiceAttitudeMin = num14;
            }
            if (!(((info14 == null) || (info11 == null)) || shop.IsSelf))
            {
                model.SellerDeliverySpeed = info11.CommentValue;
                model.SellerDeliverySpeedPeer = info14.CommentValue;
                model.SellerDeliverySpeedMax = (info19 != null) ? info19.CommentValue : 0M;
                model.sellerDeliverySpeedMin = (info20 != null) ? info20.CommentValue : 0M;
            }
            else
            {
                model.SellerDeliverySpeed = num14;
                model.SellerDeliverySpeedPeer = num14;
                model.SellerDeliverySpeedMax = num14;
                model.sellerDeliverySpeedMin = num14;
            }
            if ((base.CurrentUser != null) && (base.CurrentUser.Id > 0L))
            {
                model2.IsFavorite = this._iProductService.IsFavorite(product.Id, base.CurrentUser.Id);
            }
            else
            {
                model2.IsFavorite = false;
            }
            VShopInfo vShopByShopId = this._iVShopService.GetVShopByShopId(shop.Id);
            if (vShopByShopId == null)
            {
                num15 = -1L;
            }
            else
            {
                num15 = vShopByShopId.Id;
            }
            model.VShopId = num15;
            model2.Shop.VShopId = num15;
            model2.VShopLog = this._iVShopService.GetVShopLog(model2.Shop.VShopId);
            if (string.IsNullOrWhiteSpace(model2.VShopLog))
            {
                model2.VShopLog = base.CurrentSiteSetting.WXLogo;
            }
            model.Logined = (base.CurrentUser != null) ? 1 : 0;
            model2.EnabledBuy = (((product.AuditStatus == ProductInfo.ProductAuditStatus.Audited) && (DateTime.Parse(model3.BeginDate) <= DateTime.Now)) && (DateTime.Parse(model3.EndDate) > DateTime.Now)) && (product.SaleStatus == ProductInfo.ProductSaleStatus.OnSale);
            if (((model3.Status == FlashSaleInfo.FlashSaleStatus.Ongoing) && (DateTime.Parse(model3.BeginDate) < DateTime.Now)) && (DateTime.Parse(model3.EndDate) > DateTime.Now))
            {
                span = new TimeSpan(DateTime.Parse(model3.EndDate).Ticks);
                span2 = new TimeSpan(DateTime.Now.Ticks);
                span3 = span.Subtract(span2);
                model.Second = (span3.TotalSeconds < 0.0) ? 0.0 : span3.TotalSeconds;
            }
            else if ((model3.Status == FlashSaleInfo.FlashSaleStatus.Ongoing) && (DateTime.Parse(model3.BeginDate) > DateTime.Now))
            {
                span = new TimeSpan(DateTime.Parse(model3.BeginDate).Ticks);
                span2 = new TimeSpan(DateTime.Now.Ticks);
                span3 = span.Subtract(span2);
                model.Second = (span3.TotalSeconds < 0.0) ? 0.0 : span3.TotalSeconds;
            }
            ((dynamic)base.ViewBag).DetailModel = model;
            List<Himall.DTO.CustomerService> mobileCustomerService = CustomerServiceApplication.GetMobileCustomerService(model3.ShopId);
            Himall.DTO.CustomerService service = Enumerable.FirstOrDefault<Himall.DTO.CustomerService>((IEnumerable<Himall.DTO.CustomerService>)CustomerServiceApplication.GetPreSaleByShopId(model3.ShopId), (Func<Himall.DTO.CustomerService, bool>)(p => p.Tool == CustomerServiceInfo.ServiceTool.MeiQia));
            if (service != null)
            {
                mobileCustomerService.Insert(0, service);
            }
            ((dynamic)base.ViewBag).CustomerServices = mobileCustomerService;
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);
            return base.View(model2);
        }

        [HttpPost]
        public ActionResult GetData(int index, int size, string cname)
        {
            FlashSaleQuery query = new FlashSaleQuery
            {
                ItemName = cname,
                IsPreheat = true,
                PageNo = index,
                PageSize = size,
                AuditStatus = FlashSaleInfo.FlashSaleStatus.Ongoing,
                CheckProductStatus = true
            };
            ObsoletePageModel<FlashSaleInfo> all = this._iLimitTimeBuyService.GetAll(query);
            List<FlashSaleModel> list = new List<FlashSaleModel>();
            foreach (FlashSaleInfo info in all.Models.ToList<FlashSaleInfo>())
            {
                FlashSaleModel item = new FlashSaleModel
                {
                    Id = info.Id,
                    Title = info.Title,
                    ShopId = info.ShopId,
                    ProductId = info.ProductId,
                    Status = info.Status,
                    ProductName = info.Himall_Products.ProductName,
                    ProductImg = HimallIO.GetProductSizeImage(info.Himall_Products.RelativePath, 1, 0),
                    MarketPrice = info.Himall_Products.MarketPrice,
                    BeginDate = info.BeginDate.ToString("yyyy-MM-dd HH:mm"),
                    EndDate = info.EndDate.ToString("yyyy-MM-dd HH:mm"),
                    LimitCountOfThePeople = info.LimitCountOfThePeople,
                    SaleCount = info.SaleCount,
                    CategoryName = info.CategoryName,
                    MinPrice = info.MinPrice
                };
                list.Add(item);
            }
            DataGridModel<FlashSaleModel> data = new DataGridModel<FlashSaleModel>
            {
                total = all.Total,
                rows = list
            };
            return base.Json(data);
        }

        [HttpPost]
        public ActionResult GetSkus(long id)
        {
            FlashSaleModel data = this._iLimitTimeBuyService.Get(id);
            if (data != null)
            {
                return base.Json(data);
            }
            return base.Json(null);
        }

        public ActionResult Home(string catename = "")
        {
            Func<SelectListItem, bool> predicate = null;
            List<SelectListItem> source = new List<SelectListItem>();
            string[] serviceCategories = this._iLimitTimeBuyService.GetServiceCategories();
            foreach (string str in serviceCategories)
            {
                SelectListItem item = new SelectListItem
                {
                    Selected = false,
                    Text = str,
                    Value = str
                };
                source.Add(item);
            }
            if (!string.IsNullOrWhiteSpace(catename))
            {
                if (predicate == null)
                {
                    predicate = c => c.Text.Equals(catename);
                }
                SelectListItem item2 = source.FirstOrDefault<SelectListItem>(predicate);
                if (item2 != null)
                {
                    item2.Selected = true;
                }
            }
            FlashSaleConfigModel config = this._iLimitTimeBuyService.GetConfig();
            ((dynamic)base.ViewBag).Preheat = config.Preheat;
            ((dynamic)base.ViewBag).Cate = source;
            FlashSaleQuery query = new FlashSaleQuery
            {
                CategoryName = catename,
                OrderKey = 5,
                IsPreheat = true,
                PageNo = 1,
                PageSize = 10,
                AuditStatus = FlashSaleInfo.FlashSaleStatus.Ongoing,
                CheckProductStatus = true
            };
            ObsoletePageModel<FlashSaleInfo> all = this._iLimitTimeBuyService.GetAll(query);
            return base.View(all);
        }

    }
}