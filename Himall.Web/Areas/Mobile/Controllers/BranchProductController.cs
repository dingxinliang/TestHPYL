using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Helper;
using Himall.DTO;
using Himall.DTO.Product;
using Himall.IServices;
using Himall.Model;
using Himall.Web;
using Himall.Web.App_Code;
using Himall.Web.Areas.Mobile.Models;
using Himall.Web.Areas.Web;
using Himall.Web.Areas.Web.Helper;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Common;
using Himall.Web.Framework;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class BranchProductController : BaseMobileTemplatesController
    {
        private const string SMSPLUGIN = "Himall.Plugin.Message.SMS";
        private IShopService _iShopService;
        private IVShopService _iVShopService;
        private IProductService _iProductService;
        private ICashDepositsService _iCashDepositsService;
        private IFreightTemplateService _iFreightTemplateService;
        private IRegionService _iRegionService;
        private IDistributionService _iDistributionService;
        private IMessageService _iMessageService;
        private ITypeService _iTypeService;
        private IShopBranchService _iShopBranchService;

        public BranchProductController(IShopService iShopService, IVShopService iVShopService, IProductService iProductService, ICashDepositsService iCashDepositsService, IFreightTemplateService iFreightTemplateService, IRegionService iRegionService, IDistributionService iDistributionService, ITypeService iTypeService, IMessageService iMessageService, IShopBranchService iShopBranchService)
        {
            this._iShopService = iShopService;
            this._iVShopService = iVShopService;
            this._iProductService = iProductService;
            this._iCashDepositsService = iCashDepositsService;
            this._iFreightTemplateService = iFreightTemplateService;
            this._iRegionService = iRegionService;
            this._iDistributionService = iDistributionService;
            this._iTypeService = iTypeService;
            this._iMessageService = iMessageService;
            this._iShopBranchService = iShopBranchService;
        }

        public JsonResult GetNeedRefreshProductInfo(long id = 0L, long shopId = 0L, long bid = 0L)
        {
            IProductService productService = this._iProductService;
            long productId = id;
            if (productId == 0L)
                return this.Json((object)new
                {
                    data = false
                });
            ProductInfo refreshProductInfo = productService.GetNeedRefreshProductInfo(productId);
            if (refreshProductInfo == null)
                throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
            ShopBranchInfo shopBranchById = this._iShopBranchService.GetShopBranchById(bid);
            if (refreshProductInfo == null)
                throw new HimallException("很抱歉，您查看的门店不存在。");
            List<ShopBranchSkusInfo> skus = this._iShopBranchService.GetSkus(shopBranchById.ShopId, (IEnumerable<long>)new List<long>()
      {
        bid
      }, new ShopBranchSkuStatus?(ShopBranchSkuStatus.Normal));
            if (skus == null || skus.Count <= 0)
                throw new HimallException("很抱歉，您查看的门店没有该商品，可能已下架。");
            VShopInfo vshopByShopId = this._iVShopService.GetVShopByShopId(refreshProductInfo.ShopId);
            long num1 = vshopByShopId != null ? vshopByShopId.Id : -1L;
            IQueryable<SKUInfo> skUs = productService.GetSKUs(productId);
            Decimal num2 = new Decimal(0);
            Decimal num3 = new Decimal(0);
            string str1 = string.Empty;
            IQueryable<SKUInfo> source = Queryable.Where<SKUInfo>(skUs, (Expression<Func<SKUInfo, bool>>)(s => s.Stock >= 0L));
            if (Queryable.Count<SKUInfo>(source) > 0)
            {
                if (Queryable.Min<SKUInfo, Decimal>(source, (Expression<Func<SKUInfo, Decimal>>)(s => s.SalePrice)) == new Decimal(0) && Queryable.Max<SKUInfo, Decimal>(source, (Expression<Func<SKUInfo, Decimal>>)(s => s.SalePrice)) == new Decimal(0))
                    str1 = refreshProductInfo.MinSalePrice.ToString("f2");
            }
            else
                str1 = !(num3 > num2) ? string.Format("{0}", (object)num2.ToString("f2")) : string.Format("{0}-{1}", (object)num2.ToString("f2"), (object)num3.ToString("f2"));
            Decimal num4 = new Decimal(1);
            if (this.CurrentUser != null)
                num4 = this.CurrentUser.MemberDiscount;
            string str2 = !ShopApplication.GetShop(refreshProductInfo.ShopId, false).IsSelf ? (string.IsNullOrWhiteSpace(str1) ? refreshProductInfo.MinSalePrice.ToString("f2") : str1) : (string.IsNullOrWhiteSpace(str1) ? (refreshProductInfo.MinSalePrice * num4).ToString("f2") : (Convert.ToDecimal(str1) * num4).ToString("f2"));
            bool flag1 = false;
            bool flag2;
            if (this.CurrentUser == null)
            {
                flag2 = false;
            }
            else
            {
                flag2 = this._iProductService.IsFavorite(productId, this.CurrentUser.Id);
                flag1 = Enumerable.Contains<long>((IEnumerable<long>)Enumerable.ToArray<long>((IEnumerable<long>)Queryable.Select<FavoriteShopInfo, long>(this._iShopService.GetFavoriteShopInfos(this.CurrentUser.Id), (Expression<Func<FavoriteShopInfo, long>>)(item => item.ShopId))), shopId);
            }
            int num5 = 0;
            long memberId = 0L;
            if (this.CurrentUser != null)
                memberId = this.CurrentUser.Id;
            IEnumerable<ShoppingCartItem> enumerable = Enumerable.Where<ShoppingCartItem>(new BranchCartHelper().GetCart(memberId, bid).Items, (Func<ShoppingCartItem, bool>)(x => x.ProductId == productId));
            if (enumerable != null)
            {
                foreach (ShoppingCartItem shoppingCartItem in enumerable)
                {
                    ShoppingCartItem cartitem = shoppingCartItem;
                    ShopBranchSkusInfo shopBranchSkusInfo = Enumerable.FirstOrDefault<ShopBranchSkusInfo>((IEnumerable<ShopBranchSkusInfo>)skus, (Func<ShopBranchSkusInfo, bool>)(x => x.SkuId == cartitem.SkuId));
                    if (shopBranchSkusInfo != null && shopBranchSkusInfo.Status == ShopBranchSkuStatus.Normal && shopBranchSkusInfo.Stock >= cartitem.Quantity)
                        num5 += cartitem.Quantity;
                }
            }
            IQueryable<ProductCommentInfo> commentsByProductId = ServiceHelper.Create<ICommentService>().GetCommentsByProductId(productId);
            DateTime now = DateTime.Now;
            return this.Json((object)new
            {
                salecount = ShopBranchApplication.GetProductSaleCount(bid, productId, now.AddDays(-30.0).Date, now),
                measureunit = refreshProductInfo.MeasureUnit,
                price = str2,
                auditStatus = (Enumerable.Sum<ShopBranchSkusInfo>((IEnumerable<ShopBranchSkusInfo>)skus, (Func<ShopBranchSkusInfo, long>)(x => (long)(uint)x.Status)) > 0L ? 1 : 0),
                isFavorite = flag2,
                isFavoriteShop = flag1,
                freightTemplateId = refreshProductInfo.FreightTemplateId,
                vShopId = num1,
                stock = Enumerable.Sum<ShopBranchSkusInfo>((IEnumerable<ShopBranchSkusInfo>)skus, (Func<ShopBranchSkusInfo, int>)(a => a.Stock)),
                cartCount = num5,
                allComment = Queryable.Count<ProductCommentInfo>(commentsByProductId)
            });
        }

        public ActionResult Detail(string id = "", long partnerid = 0L, int nojumpfg = 0, string shopBranchId = "")
        {
            IProductService productService = this._iProductService;
            long result1 = 0L;
            if (!long.TryParse(id, out result1))
                ;
            if (result1 == 0L)
                return (ActionResult)this.RedirectToAction("Error404", "Error", (object)new
                {
                    area = "Web"
                });
            if (!productService.CheckProductIsExist(result1))
                throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
            ProductInfo product = productService.GetProduct(result1);
            if (product == null)
                throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
            long result2 = 0L;
            long.TryParse(shopBranchId, out result2);
            if (result2 == 0L)
                return (ActionResult)this.RedirectToAction("Error404", "Error", (object)new
                {
                    area = "Web"
                });
            ShopBranchInfo shopBranchById = this._iShopBranchService.GetShopBranchById(result2);
            if (shopBranchById == null || shopBranchById.Status == ShopBranchStatus.Freeze)
                throw new HimallException("很抱歉，您查看的门店不存在，可能已关闭。");
            if (!this._iShopBranchService.CheckProductIsExist(result2, result1))
                throw new HimallException("很抱歉，该门店不存在您查看的商品，可能已转移。");
            List<ShopBranchSkusInfo> skus = this._iShopBranchService.GetSkus(shopBranchById.ShopId, (IEnumerable<long>)new List<long>()
      {
        result2
      }, new ShopBranchSkuStatus?(ShopBranchSkuStatus.Normal));
            if (skus == null || skus.Count <= 0)
                throw new HimallException("很抱歉，您查看的门店没有该商品，可能已下架。");
            bool flag = false;
            foreach (ShopBranchSkusInfo shopBranchSkusInfo in skus)
            {
                if (shopBranchSkusInfo.Status == ShopBranchSkuStatus.Normal)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
                throw new HimallException("很抱歉，您查看的门店商品已下架。");
            if (partnerid > 0L)
            {
                long uid = 0L;
                if (this.CurrentUser != null)
                    uid = this.CurrentUser.Id;
                this.SaveDistributionUserLinkId(partnerid, product.ShopId, uid);
            }
            ProductManagerApplication.GetWAPBranchHtml(result1, result2);
            string fileName = "/Storage/Products/Statics/" + (object)id + "-" + (string)(object)result2 + "-wap-branch.html";
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);
            return (ActionResult)this.File(fileName, "text/html");
        }

        public JsonResult GetWeiXinShareArgs()
        {
            return this.Json((object)WXApiApplication.GetWeiXinShareArgs(this.HttpContext.Request.Url.AbsoluteUri), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(long id = 0L, long partnerid = 0L, int nojumpfg = 0, string shopBranchId = "")
        {
            bool flag;
            long num16;
            ParameterExpression expression;
            IShopService service = this._iShopService;
            ICustomerService service2 = ServiceHelper.Create<ICustomerService>();
            string str = "";
            ProductDetailModel model = new ProductDetailModel();
            ProductDetailModelForWeb web = new ProductDetailModelForWeb
            {
                Product = new ProductInfo(),
                Shop = new ShopInfoModel(),
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU()
            };
            ProductInfo product = null;
            ShopInfo shop = null;
            long num = id;
            if (num == 0L)
            {
                throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
            }
            product = this._iProductService.GetProduct(num);
            if (product == null)
            {
                throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
            }
            if (product.IsDeleted)
            {
                throw new HimallException("很抱歉，您查看的商品不存在，可能被转移。");
            }
            long result = 0;
            long.TryParse(shopBranchId, out result);
            if (result == 0)
            {
                throw new HimallException("很抱歉，您查看的门店不存在。");
            }
            ShopBranchInfo shopBranchById = this._iShopBranchService.GetShopBranchById(result);
            if (shopBranchById == null)
            {
                throw new HimallException("很抱歉，您查看的门店不存在。");
            }
            if (shopBranchById.Status == ShopBranchStatus.Freeze)
            {
                throw new HimallException("很抱歉，您查看的门店已冻结。");
            }
            if (!this._iShopBranchService.CheckProductIsExist(result, num))
            {
                throw new HimallException("很抱歉，该门店不存在您查看的商品，可能已转移。");
            }
            ((dynamic)base.ViewBag).bid = result;
            if (partnerid > 0L)
            {
                long uid = 0L;
                if (base.CurrentUser != null)
                {
                    uid = base.CurrentUser.Id;
                }
                base.SaveDistributionUserLinkId(partnerid, product.ShopId, uid);
            }
            web.ProductDescription = product.ProductDescriptionInfo.ShowMobileDescription;
            shop = this._iShopService.GetShop(product.ShopId, false);
            ShopServiceMarkModel shopComprehensiveMark = ShopServiceMark.GetShopComprehensiveMark(shop.Id);
            web.Shop.PackMark = shopComprehensiveMark.PackMark;
            web.Shop.ServiceMark = shopComprehensiveMark.ServiceMark;
            web.Shop.ComprehensiveMark = shopComprehensiveMark.ComprehensiveMark;
            IQueryable<ProductCommentInfo> commentsByProductId = ServiceHelper.Create<ICommentService>().GetCommentsByProductId(num);
            web.Shop.Name = shop.ShopName;
            Decimal num2;
            if (commentsByProductId != null && Queryable.Count<ProductCommentInfo>(commentsByProductId) != 0)
                num2 = Queryable.Average<ProductCommentInfo>(commentsByProductId, (Expression<Func<ProductCommentInfo, Decimal>>)(p => (Decimal)p.ReviewMark));
            else
                num2 = new Decimal(0);
            web.Shop.ProductMark = num2;
            web.Shop.Id = product.ShopId;
            web.Shop.FreeFreight = shop.FreeFreight;
            model.ProductNum = this._iProductService.GetShopOnsaleProducts(product.ShopId);
            model.FavoriteShopCount = this._iShopService.GetShopFavoritesCount(product.ShopId);
            if (base.CurrentUser == null)
            {
                flag = false;
                model.IsFavoriteShop = false;
            }
            else
            {
                flag = this._iProductService.IsFavorite(product.Id, base.CurrentUser.Id);
                model.IsFavoriteShop = (from item in this._iShopService.GetFavoriteShopInfos(base.CurrentUser.Id) select item.ShopId).ToArray<long>().Contains<long>(product.ShopId);
            }
            model.IsFavorite = flag;
            ProductTypeInfo type = this._iTypeService.GetType(product.TypeId);
            string str2 = ((type == null) || string.IsNullOrEmpty(type.ColorAlias)) ? SpecificationType.Color.ToDescription() : type.ColorAlias;
            string str3 = ((type == null) || string.IsNullOrEmpty(type.SizeAlias)) ? SpecificationType.Size.ToDescription() : type.SizeAlias;
            string str4 = ((type == null) || string.IsNullOrEmpty(type.VersionAlias)) ? SpecificationType.Version.ToDescription() : type.VersionAlias;
            web.ColorAlias = str2;
            web.SizeAlias = str3;
            web.VersionAlias = str4;
            if ((product.SKUInfo != null) && (product.SKUInfo.Count<SKUInfo>() > 0))
            {
                long num4 = 0L;
                long num5 = 0L;
                long num6 = 0L;
                using (IEnumerator<SKUInfo> enumerator = product.SKUInfo.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Func<ProductSKU, bool> predicate = null;
                        Func<SKUInfo, bool> func2 = null;
                        Func<ProductSKU, bool> func3 = null;
                        Func<SKUInfo, bool> func4 = null;
                        Func<ProductSKU, bool> func5 = null;
                        Func<SKUInfo, bool> func6 = null;
                        SKUInfo sku = enumerator.Current;
                        string[] strArray = sku.Id.Split(new char[] { '_' });
                        if (strArray.Count<string>() > 0)
                        {
                            if (long.TryParse(strArray[1], out num4))
                            {
                            }
                            if (num4 != 0L)
                            {
                                if (predicate == null)
                                {
                                    predicate = v => v.Value.Equals(sku.Color);
                                }
                                if (!web.Color.Any<ProductSKU>(predicate))
                                {
                                    if (func2 == null)
                                    {
                                        func2 = s => s.Color.Equals(sku.Color);
                                    }
                                    long num7 = product.SKUInfo.Where<SKUInfo>(func2).Sum<SKUInfo>((Func<SKUInfo, long>)(s => s.Stock));
                                    ProductSKU tsku = new ProductSKU
                                    {
                                        Name = "选择" + str2,
                                        EnabledClass = (num7 != 0L) ? "enabled" : "disabled",
                                        SelectedClass = "",
                                        SkuId = num4,
                                        Value = sku.Color,
                                        Img = HimallIO.GetImagePath(sku.ShowPic, null)
                                    };
                                    web.Color.Add(tsku);
                                }
                            }
                        }
                        if (strArray.Count<string>() > 1)
                        {
                            if (long.TryParse(strArray[2], out num5))
                            {
                            }
                            if (num5 != 0L)
                            {
                                if (func3 == null)
                                {
                                    func3 = v => v.Value.Equals(sku.Size);
                                }
                                if (!web.Size.Any<ProductSKU>(func3))
                                {
                                    if (func4 == null)
                                    {
                                        func4 = s => s.Size.Equals(sku.Size);
                                    }
                                    long num8 = product.SKUInfo.Where<SKUInfo>(func4).Sum<SKUInfo>((Func<SKUInfo, long>)(s1 => s1.Stock));
                                    ProductSKU tsku2 = new ProductSKU
                                    {
                                        Name = "选择" + str3,
                                        EnabledClass = (num8 != 0L) ? "enabled" : "disabled",
                                        SelectedClass = "",
                                        SkuId = num5,
                                        Value = sku.Size
                                    };
                                    web.Size.Add(tsku2);
                                }
                            }
                        }
                        if (strArray.Count<string>() > 2)
                        {
                            if (long.TryParse(strArray[3], out num6))
                            {
                            }
                            if (num6 != 0L)
                            {
                                if (func5 == null)
                                {
                                    func5 = v => v.Value.Equals(sku.Version);
                                }
                                if (!web.Version.Any<ProductSKU>(func5))
                                {
                                    if (func6 == null)
                                    {
                                        func6 = s => s.Version.Equals(sku.Version);
                                    }
                                    long num9 = product.SKUInfo.Where<SKUInfo>(func6).Sum<SKUInfo>((Func<SKUInfo, long>)(s => s.Stock));
                                    ProductSKU tsku3 = new ProductSKU
                                    {
                                        Name = "选择" + str4,
                                        EnabledClass = (num9 != 0L) ? "enabled" : "disabled",
                                        SelectedClass = "",
                                        SkuId = num6,
                                        Value = sku.Version
                                    };
                                    web.Version.Add(tsku3);
                                }
                            }
                        }
                    }
                }
                decimal num10 = 0M;
                decimal num11 = 0M;
                num10 = (from s in product.SKUInfo
                         where s.Stock >= 0L
                         select s).Min<SKUInfo>((Func<SKUInfo, decimal>)(s => s.SalePrice));
                num11 = (from s in product.SKUInfo
                         where s.Stock >= 0L
                         select s).Max<SKUInfo>((Func<SKUInfo, decimal>)(s => s.SalePrice));
                if ((num10 == 0M) && (num11 == 0M))
                {
                    str = product.MinSalePrice.ToString("f2");
                }
                else if (num11 > num10)
                {
                    str = string.Format("{0}-{1}", num10.ToString("f2"), num11.ToString("f2"));
                }
                else
                {
                    str = string.Format("{0}", num10.ToString("f2"));
                }
            }
            ((dynamic)base.ViewBag).Price = string.IsNullOrWhiteSpace(str) ? product.MinSalePrice.ToString("f2") : str;
            IQueryable<StatisticOrderCommentsInfo> shopStatisticOrderComments = this._iShopService.GetShopStatisticOrderComments(product.ShopId);
            StatisticOrderCommentsInfo info5 = (from c in shopStatisticOrderComments
                                                where ((int)c.CommentKey) == 1
                                                select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info6 = (from c in shopStatisticOrderComments
                                                where ((int)c.CommentKey) == 9
                                                select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info7 = (from c in shopStatisticOrderComments
                                                where ((int)c.CommentKey) == 5
                                                select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info8 = (from c in shopStatisticOrderComments
                                                where ((int)c.CommentKey) == 2
                                                select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info9 = (from c in shopStatisticOrderComments
                                                where ((int)c.CommentKey) == 10
                                                select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info10 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 6
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info11 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 3
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info12 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 4
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info13 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 11
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info14 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 12
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info15 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 7
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            StatisticOrderCommentsInfo info16 = (from c in shopStatisticOrderComments
                                                 where ((int)c.CommentKey) == 8
                                                 select c).FirstOrDefault<StatisticOrderCommentsInfo>();
            decimal num12 = 5M;
            if (!(((info5 == null) || (info8 == null)) || shop.IsSelf))
            {
                model.ProductAndDescription = info5.CommentValue;
                model.ProductAndDescriptionPeer = info8.CommentValue;
                model.ProductAndDescriptionMin = info12.CommentValue;
                model.ProductAndDescriptionMax = info11.CommentValue;
            }
            else
            {
                model.ProductAndDescription = num12;
                model.ProductAndDescriptionPeer = num12;
                model.ProductAndDescriptionMin = num12;
                model.ProductAndDescriptionMax = num12;
            }
            if (!(((info6 == null) || (info9 == null)) || shop.IsSelf))
            {
                model.SellerServiceAttitude = info6.CommentValue;
                model.SellerServiceAttitudePeer = info9.CommentValue;
                model.SellerServiceAttitudeMax = info13.CommentValue;
                model.SellerServiceAttitudeMin = info14.CommentValue;
            }
            else
            {
                model.SellerServiceAttitude = num12;
                model.SellerServiceAttitudePeer = num12;
                model.SellerServiceAttitudeMax = num12;
                model.SellerServiceAttitudeMin = num12;
            }
            if (!(((info10 == null) || (info7 == null)) || shop.IsSelf))
            {
                model.SellerDeliverySpeed = info7.CommentValue;
                model.SellerDeliverySpeedPeer = info10.CommentValue;
                model.SellerDeliverySpeedMax = (info15 != null) ? info15.CommentValue : 0M;
                model.sellerDeliverySpeedMin = (info16 != null) ? info16.CommentValue : 0M;
            }
            else
            {
                model.SellerDeliverySpeed = num12;
                model.SellerDeliverySpeedPeer = num12;
                model.SellerDeliverySpeedMax = num12;
                model.sellerDeliverySpeedMin = num12;
            }
            ILimitTimeBuyService service4 = ServiceHelper.Create<ILimitTimeBuyService>();
            bool flag2 = false;
            web.FlashSale = service4.IsFlashSaleDoesNotStarted(num);
            web.FlashSaleConfig = service4.GetConfig();
            if (web.FlashSale != null)
            {
                TimeSpan span = (TimeSpan)(DateTime.Parse(web.FlashSale.BeginDate) - DateTime.Now);
                TimeSpan span2 = new TimeSpan(web.FlashSaleConfig.Preheat, 0, 0);
                if (span2 >= span)
                {
                    flag2 = true;
                    if (!web.FlashSaleConfig.IsNormalPurchase)
                    {
                        return base.RedirectToAction("Detail", "LimitTimeBuy", new { Id = web.FlashSale.Id });
                    }
                }
            }
            ((dynamic)base.ViewBag).IsPreheat = flag2;
            web.Product = product;
            web.Favorites = product.Himall_Favorites.Count<FavoriteInfo>();
            IEnumerable<ProductCommentInfo> source = from item in product.Himall_ProductComments
                                                     select item;
            int num13 = source.Count<ProductCommentInfo>();
            model.CommentCount = num13;
            IQueryable<ProductConsultationInfo> consultations = ServiceHelper.Create<IConsultationService>().GetConsultations(num);
            double num14 = num13;
            double num15 = source.Count<ProductCommentInfo>(item => item.ReviewMark >= 4);
            model.NicePercent = (int)((num15 / num14) * 100.0);
            model.Consultations = consultations.Count<ProductConsultationInfo>();
            VShopInfo vShopByShopId = this._iVShopService.GetVShopByShopId(shop.Id);
            if (vShopByShopId == null)
            {
                num16 = -1L;
            }
            else
            {
                num16 = vShopByShopId.Id;
            }
            model.VShopId = num16;
            web.Shop.VShopId = num16;
            web.VShopLog = this._iVShopService.GetVShopLog(web.Shop.VShopId);
            if (string.IsNullOrWhiteSpace(web.VShopLog))
            {
                web.VShopLog = base.CurrentSiteSetting.WXLogo;
            }
            List<CustomerService> mobileCustomerService = CustomerServiceApplication.GetMobileCustomerService(product.ShopId);
            CustomerService service5 = CustomerServiceApplication.GetPreSaleByShopId(product.ShopId).FirstOrDefault<CustomerService>(p => p.Tool == CustomerServiceInfo.ServiceTool.MeiQia);
            if (service5 != null)
            {
                mobileCustomerService.Insert(0, service5);
            }
            ((dynamic)base.ViewBag).CustomerServices = mobileCustomerService;
            ((dynamic)base.ViewBag).DetailModel = model;
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);
            DateTime now = DateTime.Now;
            web.Product.SaleCounts = ShopBranchApplication.GetProductSaleCount(result, product.Id, now.AddDays(-30.0).Date, now);
            return base.View(web);

        }

        public JsonResult GetProductColloCation(long productId)
        {
            List<CollocationPoruductInfo> collocationListByProductId = ServiceHelper.Create<ICollocationService>().GetCollocationListByProductId(productId);
            int num1 = 0;
            if (collocationListByProductId != null)
            {
                foreach (CollocationPoruductInfo collocationPoruductInfo in collocationListByProductId)
                {
                    int num2 = Enumerable.Count<CollocationPoruductInfo>(Enumerable.Where<CollocationPoruductInfo>((IEnumerable<CollocationPoruductInfo>)collocationPoruductInfo.Himall_Collocation.Himall_CollocationPoruducts, (Func<CollocationPoruductInfo, bool>)(a => a.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && a.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited)));
                    if (Enumerable.FirstOrDefault<CollocationPoruductInfo>((IEnumerable<CollocationPoruductInfo>)collocationPoruductInfo.Himall_Collocation.Himall_CollocationPoruducts, (Func<CollocationPoruductInfo, bool>)(p => p.IsMain && p.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && p.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited && !p.Himall_Products.IsDeleted)) != null && num2 > 1)
                        ++num1;
                }
            }
            return this.Json((object)new
            {
                count = num1
            });
        }

        public ActionResult ProductColloCation(long productId)
        {
            List<CollocationPoruductInfo> collocationListByProductId = ServiceHelper.Create<ICollocationService>().GetCollocationListByProductId(productId);
            List<ProductCollocationModel> list1 = new List<ProductCollocationModel>();
            if (collocationListByProductId != null && collocationListByProductId.Count > 0)
            {
                int number = 0;
                foreach (CollocationPoruductInfo collocationPoruductInfo1 in collocationListByProductId)
                {
                    ++number;
                    ProductCollocationModel collocationModel = new ProductCollocationModel();
                    List<CollocationProducts> list2 = Enumerable.ToList<CollocationProducts>((IEnumerable<CollocationProducts>)Enumerable.OrderBy<CollocationProducts, int>(Enumerable.Select<CollocationPoruductInfo, CollocationProducts>(Enumerable.Where<CollocationPoruductInfo>((IEnumerable<CollocationPoruductInfo>)collocationPoruductInfo1.Himall_Collocation.Himall_CollocationPoruducts, (Func<CollocationPoruductInfo, bool>)(a => a.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && a.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited)), (Func<CollocationPoruductInfo, CollocationProducts>)(a => new CollocationProducts()
                    {
                        DisplaySequence = a.DisplaySequence.Value,
                        IsMain = a.IsMain,
                        Stock = Enumerable.Sum<SKUInfo>((IEnumerable<SKUInfo>)a.Himall_Products.SKUInfo, (Func<SKUInfo, long>)(t => t.Stock)),
                        MaxCollPrice = Enumerable.Max<CollocationSkuInfo>((IEnumerable<CollocationSkuInfo>)a.Himall_CollocationSkus, (Func<CollocationSkuInfo, Decimal>)(x => x.Price)),
                        MaxSalePrice = Enumerable.Max<CollocationSkuInfo>((IEnumerable<CollocationSkuInfo>)a.Himall_CollocationSkus, (Func<CollocationSkuInfo, Decimal?>)(x => x.SkuPirce)).GetValueOrDefault(),
                        MinCollPrice = Enumerable.Min<CollocationSkuInfo>((IEnumerable<CollocationSkuInfo>)a.Himall_CollocationSkus, (Func<CollocationSkuInfo, Decimal>)(x => x.Price)),
                        MinSalePrice = Enumerable.Min<CollocationSkuInfo>((IEnumerable<CollocationSkuInfo>)a.Himall_CollocationSkus, (Func<CollocationSkuInfo, Decimal?>)(x => x.SkuPirce)).GetValueOrDefault(),
                        ProductName = a.Himall_Products.ProductName,
                        ProductId = a.ProductId,
                        ColloPid = a.Id,
                        Image = HimallIO.GetImagePath(a.Himall_Products.RelativePath, (string)null)
                    })), (Func<CollocationProducts, int>)(a => a.DisplaySequence)));
                    Decimal num = new Decimal(0);
                    if (list2 != null && list2.Count > 1)
                        num = Enumerable.Sum<CollocationProducts>((IEnumerable<CollocationProducts>)list2, (Func<CollocationProducts, Decimal>)(a => a.MaxSalePrice)) - Enumerable.Sum<CollocationProducts>((IEnumerable<CollocationProducts>)list2, (Func<CollocationProducts, Decimal>)(a => a.MinCollPrice));
                    CollocationPoruductInfo collocationPoruductInfo2 = Enumerable.FirstOrDefault<CollocationPoruductInfo>((IEnumerable<CollocationPoruductInfo>)collocationPoruductInfo1.Himall_Collocation.Himall_CollocationPoruducts, (Func<CollocationPoruductInfo, bool>)(p => p.IsMain));
                    collocationModel.Id = collocationPoruductInfo1.Id;
                    collocationModel.Name = "组合购" + CollocationApplication.GetChineseNumber(number);
                    collocationModel.ProductId = collocationPoruductInfo2.ProductId;
                    collocationModel.ShopId = collocationPoruductInfo1.Himall_Collocation.ShopId;
                    collocationModel.Products = list2;
                    collocationModel.Cheap = num;
                    list1.Add(collocationModel);
                }
            }
            return (ActionResult)this.View((object)list1);
        }

        [HttpPost]
        public JsonResult LogProduct(long pid)
        {
            if (this.CurrentUser != null)
                BrowseHistrory.AddBrowsingProduct(pid, this.CurrentUser.Id);
            else
                BrowseHistrory.AddBrowsingProduct(pid, 0L);
            return this.Json((object)null);
        }

        [HttpPost]
        public JsonResult AddFavoriteProduct(long pid)
        {
            int status = 0;
            this._iProductService.AddFavorite(pid, this.CurrentUser.Id, out status);
            return this.Json((object)new BaseController.Result()
            {
                success = true,
                msg = "成功关注"
            });
        }

        [HttpPost]
        public JsonResult DeleteFavoriteProduct(long pid)
        {
            this._iProductService.DeleteFavorite(pid, this.CurrentUser.Id);
            return this.Json((object)new BaseController.Result()
            {
                success = true,
                msg = "已取消关注"
            });
        }

        public JsonResult GetSKUInfo(long pId, long bid)
        {
            ProductInfo product = this._iProductService.GetProduct(pId);
            List<ShopBranchSkusInfo> skus = this._iShopBranchService.GetSkus(this._iShopBranchService.GetShopBranchById(bid).ShopId, (IEnumerable<long>)new List<long>()
      {
        bid
      }, new ShopBranchSkuStatus?(ShopBranchSkuStatus.Normal));
            long memberId = 0L;
            if (this.CurrentUser != null)
                memberId = this.CurrentUser.Id;
            ShoppingCartInfo cart = new BranchCartHelper().GetCart(memberId, bid);
            Decimal num1 = new Decimal(1);
            if (this.CurrentUser != null)
                num1 = this.CurrentUser.MemberDiscount;
            Shop shop = ShopApplication.GetShop(product.ShopId, false);
            List<ProductSKUModel> list = new List<ProductSKUModel>();
            foreach (SKUInfo skuInfo in Enumerable.Where<SKUInfo>((IEnumerable<SKUInfo>)product.SKUInfo, (Func<SKUInfo, bool>)(s => s.Stock > 0L)))
            {
                SKUInfo sku = skuInfo;
                Decimal num2 = new Decimal(1);
                Decimal num3 = !shop.IsSelf ? sku.SalePrice : sku.SalePrice * num1;
                if (Enumerable.Count<ShopBranchSkusInfo>((IEnumerable<ShopBranchSkusInfo>)skus, (Func<ShopBranchSkusInfo, bool>)(x => x.SkuId == sku.Id && x.Stock > 0)) > 0)
                {
                    int num4 = 0;
                    if (cart != null && cart.Items != null && Enumerable.Count<ShoppingCartItem>(cart.Items) > 0)
                    {
                        ShoppingCartItem shoppingCartItem = Enumerable.FirstOrDefault<ShoppingCartItem>(cart.Items, (Func<ShoppingCartItem, bool>)(x => x.SkuId == sku.Id));
                        if (shoppingCartItem != null)
                            num4 = shoppingCartItem.Quantity;
                    }
                    list.Add(new ProductSKUModel()
                    {
                        Price = num3,
                        SkuId = sku.Id,
                        Stock = Enumerable.FirstOrDefault<ShopBranchSkusInfo>((IEnumerable<ShopBranchSkusInfo>)skus, (Func<ShopBranchSkusInfo, bool>)(x => x.SkuId == sku.Id)).Stock,
                        cartCount = cart == null || Enumerable.Count<ShoppingCartItem>(cart.Items) == 0 ? 0 : (Enumerable.FirstOrDefault<ShoppingCartItem>(cart.Items, (Func<ShoppingCartItem, bool>)(x => x.SkuId == sku.Id)) == null ? 0 : Enumerable.FirstOrDefault<ShoppingCartItem>(cart.Items, (Func<ShoppingCartItem, bool>)(x => x.SkuId == sku.Id)).Quantity)
                    });
                }
            }
            return this.Json((object)new
            {
                skuArray = list
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSKUCartCount(string skuId, long bid)
        {
            int num = 0;
            long memberId = 0L;
            if (this.CurrentUser != null)
                memberId = this.CurrentUser.Id;
            IEnumerable<ShoppingCartItem> source = Enumerable.Where<ShoppingCartItem>(new BranchCartHelper().GetCart(memberId, bid).Items, (Func<ShoppingCartItem, bool>)(x => x.SkuId == skuId));
            if (source != null && Enumerable.Count<ShoppingCartItem>(source) > 0)
                num = Enumerable.Sum<ShoppingCartItem>(source, (Func<ShoppingCartItem, int>)(x => x.Quantity));
            return this.Json((object)new
            {
                CartCount = num
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProductCartCount(long pid, long bid)
        {
            int num = 0;
            long memberId = 0L;
            if (this.CurrentUser != null)
                memberId = this.CurrentUser.Id;
            ShoppingCartInfo cart = new BranchCartHelper().GetCart(memberId, bid);
            List<ShopBranchSkusInfo> skus = this._iShopBranchService.GetSkus(this._iShopBranchService.GetShopBranchById(bid).ShopId, (IEnumerable<long>)new List<long>()
      {
        bid
      }, new ShopBranchSkuStatus?(ShopBranchSkuStatus.Normal));
            IEnumerable<ShoppingCartItem> source = Enumerable.Where<ShoppingCartItem>(cart.Items, (Func<ShoppingCartItem, bool>)(x => x.ProductId == pid));
            if (source != null && Enumerable.Count<ShoppingCartItem>(source) > 0)
            {
                foreach (ShoppingCartItem shoppingCartItem in source)
                {
                    ShoppingCartItem cartitem = shoppingCartItem;
                    ShopBranchSkusInfo shopBranchSkusInfo = Enumerable.FirstOrDefault<ShopBranchSkusInfo>((IEnumerable<ShopBranchSkusInfo>)skus, (Func<ShopBranchSkusInfo, bool>)(x => x.SkuId == cartitem.SkuId));
                    if (shopBranchSkusInfo.Status == ShopBranchSkuStatus.Normal && shopBranchSkusInfo.Stock >= cartitem.Quantity)
                        num += cartitem.Quantity;
                }
            }
            return this.Json((object)new
            {
                CartCount = num
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCommentByProduct(long pId)
        {
            IQueryable<ProductCommentInfo> commentsByProductId = ServiceHelper.Create<ICommentService>().GetCommentsByProductId(pId);
            if (commentsByProductId == null || Queryable.Count<ProductCommentInfo>(commentsByProductId) <= 0)
                return this.Json((object)null, JsonRequestBehavior.AllowGet);
            IQueryable<ProductCommentInfo> source = commentsByProductId;
            Expression<Func<ProductCommentInfo, int>> keySelector = (Expression<Func<ProductCommentInfo, int>>)(c => c.ReviewMark);
            return this.Json((object)new
            {
                success = true,
                comments = Enumerable.Select((IEnumerable<ProductCommentInfo>)Enumerable.ToArray<ProductCommentInfo>(Enumerable.Take<ProductCommentInfo>((IEnumerable<ProductCommentInfo>)Enumerable.OrderByDescending<ProductCommentInfo, DateTime>((IEnumerable<ProductCommentInfo>)Enumerable.ToArray<ProductCommentInfo>((IEnumerable<ProductCommentInfo>)Queryable.OrderByDescending<ProductCommentInfo, int>(source, keySelector)), (Func<ProductCommentInfo, DateTime>)(a => a.ReviewDate)), 3)), c =>
                {
                    string userName = c.UserName;
                    string reviewContent = c.ReviewContent;
                    DateTime reviewDate = c.ReviewDate;
                    string str1 = reviewDate.ToString("yyyy-MM-dd HH:mm:ss");
                    string str2 = string.IsNullOrWhiteSpace(c.ReplyContent) ? "暂无回复" : c.ReplyContent;
                    string str3;
                    if (!c.ReplyDate.HasValue)
                    {
                        str3 = " ";
                    }
                    else
                    {
                        reviewDate = c.ReplyDate.Value;
                        str3 = reviewDate.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    int reviewMark = c.ReviewMark;
                    string str4 = "";
                    var fAnonymousType2c = new
                    {
                        UserName = userName,
                        ReviewContent = reviewContent,
                        ReviewDate = str1,
                        ReplyContent = str2,
                        ReplyDate = str3,
                        ReviewMark = reviewMark,
                        BuyDate = str4
                    };
                    return fAnonymousType2c;
                }),
                goodComment = Enumerable.Count<ProductCommentInfo>(Enumerable.Where<ProductCommentInfo>((IEnumerable<ProductCommentInfo>)Enumerable.ToArray<ProductCommentInfo>((IEnumerable<ProductCommentInfo>)commentsByProductId), (Func<ProductCommentInfo, bool>)(c => c.ReviewMark >= 4))),
                badComment = Enumerable.Count<ProductCommentInfo>(Enumerable.Where<ProductCommentInfo>((IEnumerable<ProductCommentInfo>)Enumerable.ToArray<ProductCommentInfo>((IEnumerable<ProductCommentInfo>)commentsByProductId), (Func<ProductCommentInfo, bool>)(c => c.ReviewMark == 1))),
                comment = Enumerable.Count<ProductCommentInfo>(Enumerable.Where<ProductCommentInfo>((IEnumerable<ProductCommentInfo>)Enumerable.ToArray<ProductCommentInfo>((IEnumerable<ProductCommentInfo>)commentsByProductId), (Func<ProductCommentInfo, bool>)(c => c.ReviewMark == 2 || c.ReviewMark == 3)))
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProductComment(long pId = 0L, int commentType = 0)
        {
            IQueryable<ProductCommentInfo> commentsByProductId = ServiceHelper.Create<ICommentService>().GetCommentsByProductId(pId);
            ((dynamic)base.ViewBag).goodComment = commentsByProductId.Count<ProductCommentInfo>(c => c.ReviewMark >= 4);
            ((dynamic)base.ViewBag).mediumComment = commentsByProductId.Count<ProductCommentInfo>(c => c.ReviewMark == 3);
            ((dynamic)base.ViewBag).bedComment = commentsByProductId.Count<ProductCommentInfo>(c => c.ReviewMark <= 2);
            ((dynamic)base.ViewBag).allComment = commentsByProductId.Count<ProductCommentInfo>();
            ((dynamic)base.ViewBag).hasAppend = (from c in commentsByProductId
                                                 where c.AppendDate.HasValue
                                                 select c).Count<ProductCommentInfo>();
            ((dynamic)base.ViewBag).hasImages = (from c in commentsByProductId
                                                 where c.Himall_ProductCommentsImages.Count >= 0
                                                 select c).Count<ProductCommentInfo>();
            return base.View();

        }

        public JsonResult GetProductComment(long pId, int pageNo, int commentType = 0, int pageSize = 10)
        {
            IEnumerable<ProductCommentInfo> enumerable;
            IQueryable<ProductCommentInfo> commentsByProductId = ServiceHelper.Create<ICommentService>().GetCommentsByProductId(pId);
            switch (commentType)
            {
                case 1:
                    enumerable = from c in
                                     (from c in commentsByProductId
                                      orderby c.ReviewMark descending
                                      select c).ToArray<ProductCommentInfo>()
                                 where c.ReviewMark >= 4
                                 select c;
                    break;

                case 2:
                    enumerable = from c in
                                     (from c in commentsByProductId
                                      orderby c.ReviewMark descending
                                      select c).ToArray<ProductCommentInfo>()
                                 where c.ReviewMark == 3
                                 select c;
                    break;

                case 3:
                    enumerable = from c in
                                     (from c in commentsByProductId
                                      orderby c.ReviewMark descending
                                      select c).ToArray<ProductCommentInfo>()
                                 where c.ReviewMark <= 2
                                 select c;
                    break;

                case 4:
                    enumerable = from c in commentsByProductId
                                 where c.Himall_ProductCommentsImages.Count >= 0
                                 select c;
                    break;

                case 5:
                    enumerable = from c in commentsByProductId
                                 where c.AppendDate.HasValue
                                 select c;
                    break;

                default:
                    enumerable = (from c in commentsByProductId
                                  orderby c.ReviewMark descending
                                  select c).ToArray<ProductCommentInfo>();
                    break;
            }
            IProductService productService = this._iProductService;
            var data = from c in
                           (from a in enumerable
                            orderby a.ReviewDate descending
                            select a).Skip<ProductCommentInfo>(((pageNo - 1) * pageSize)).Take<ProductCommentInfo>(pageSize).ToArray<ProductCommentInfo>().ToArray<ProductCommentInfo>()
                       select new
                       {
                           Sku = productService.GetSkuString(c.Himall_OrderItems.SkuId),
                           UserName = c.UserName,
                           ReviewContent = c.ReviewContent,
                           AppendContent = c.AppendContent,
                           AppendDate = c.AppendDate.HasValue ? c.AppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                           ReplyAppendContent = c.ReplyAppendContent,
                           ReplyAppendDate = c.ReplyAppendDate.HasValue ? c.ReplyAppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                           FinshDate = c.Himall_OrderItems.OrderInfo.FinishDate.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                           Images = (from a in c.Himall_ProductCommentsImages
                                     where a.CommentType == 0
                                     select new { CommentImage = HimallIO.GetImagePath(a.CommentImage, null) }).ToList(),
                           AppendImages = (from a in c.Himall_ProductCommentsImages
                                           where a.CommentType == 1
                                           select new { CommentImage = HimallIO.GetImagePath(a.CommentImage, null) }).ToList(),
                           ReviewDate = c.ReviewDate.ToString("yyyy-MM-dd HH:mm:ss"),
                           ReplyContent = string.IsNullOrWhiteSpace(c.ReplyContent) ? "暂无回复" : c.ReplyContent,
                           ReplyDate = c.ReplyDate.HasValue ? c.ReplyDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : " ",
                           ReviewMark = c.ReviewMark,
                           BuyDate = c.Himall_OrderItems.OrderInfo.OrderDate.ToString("yyyy-MM-dd HH:mm:ss")
                       };
            return base.Json(data);
        }

        private IEnumerable<CouponInfo> GetCouponList(long shopid)
        {
            IQueryable<CouponInfo> couponList = ServiceHelper.Create<ICouponService>().GetCouponList(shopid);
            IQueryable<long> couponSetList = Queryable.Select<CouponSettingInfo, long>(Queryable.Where<CouponSettingInfo>(ServiceHelper.Create<IVShopService>().GetVShopCouponSetting(shopid), (Expression<Func<CouponSettingInfo, bool>>)(a => (int)a.PlatForm == 4)), (Expression<Func<CouponSettingInfo, long>>)(item => item.CouponID));
            if (Queryable.Count<CouponInfo>(couponList) > 0 && Queryable.Count<long>(couponSetList) > 0)
                return Enumerable.Where<CouponInfo>((IEnumerable<CouponInfo>)Enumerable.ToArray<CouponInfo>((IEnumerable<CouponInfo>)couponList), (Func<CouponInfo, bool>)(item => Queryable.Contains<long>(couponSetList, item.Id)));
            return (IEnumerable<CouponInfo>)new List<CouponInfo>();
        }

        public ActionResult GetShopCoupons(long shopId)
        {
            this.GetCouponList(shopId);
            return (ActionResult)null;
        }

        [HttpPost]
        [UnAuthorize]
        public ActionResult GetProductActives(long shopId, long productId, long branchId)
        {
            ProductActives productActives = new ProductActives();
            ShopBranch shopBranchById = ShopBranchApplication.GetShopBranchById(branchId);
            productActives.freeFreight = (Decimal)shopBranchById.FreeMailFee;
            ShopBonusInfo byShopId = ServiceHelper.Create<IShopBonusService>().GetByShopId(shopId);
            if (byShopId != null)
                productActives.ProductBonus = new ProductBonusLableModel()
                {
                    Count = byShopId.Count,
                    GrantPrice = byShopId.GrantPrice,
                    RandomAmountStart = byShopId.RandomAmountStart,
                    RandomAmountEnd = byShopId.RandomAmountEnd
                };
            FullDiscountActive activeByProductId = FullDiscountApplication.GetOngoingActiveByProductId(productId, shopId);
            if (activeByProductId != null)
                productActives.FullDiscount = activeByProductId;
            return (ActionResult)this.Json((object)productActives);
        }

        public ActionResult HistoryVisite(long userId)
        {
            return (ActionResult)this.View((object)BrowseHistrory.GetBrowsingProducts(10, userId));
        }

        [HttpPost]
        public ActionResult GetStock(string skuId, long bid)
        {
            ProductInfo productInfo = this._iProductService.GetSku(skuId).ProductInfo;
            if (ShopBranchApplication.GetShopBranchById(bid) == null)
                throw new HimallException("很抱歉，您查看的门店不存在。");
            List<ShopBranchSkusInfo> skusByIds = ShopBranchApplication.GetSkusByIds(bid, (IEnumerable<string>)new string[1]
      {
        skuId
      });
            if (skusByIds == null)
                throw new HimallException("很抱歉，您查看的商品不存在，或已下架。");
            int stock = Enumerable.FirstOrDefault<ShopBranchSkusInfo>((IEnumerable<ShopBranchSkusInfo>)skusByIds).Stock;
            int num1 = 0;
            long memberId = 0L;
            if (this.CurrentUser != null)
                memberId = this.CurrentUser.Id;
            IEnumerable<ShoppingCartItem> source = Enumerable.Where<ShoppingCartItem>(new BranchCartHelper().GetCart(memberId, bid).Items, (Func<ShoppingCartItem, bool>)(x => x.SkuId == skuId));
            if (source != null && Enumerable.Count<ShoppingCartItem>(source) > 0)
                num1 = Enumerable.Sum<ShoppingCartItem>(source, (Func<ShoppingCartItem, int>)(x => x.Quantity));
            int num2 = 0;
            if (productInfo.AuditStatus == ProductInfo.ProductAuditStatus.Audited && productInfo.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && Enumerable.FirstOrDefault<ShopBranchSkusInfo>((IEnumerable<ShopBranchSkusInfo>)skusByIds).Status == ShopBranchSkuStatus.Normal)
                num2 = 1;
            return (ActionResult)this.Json((object)new
            {
                Stock = stock,
                cartCount = num1,
                Status = num2
            });
        }

        public ActionResult ShowSkuInfo(long id, long bid)
        {
            ProductInfo product = this._iProductService.GetProduct(id);
            if (product == null)
                throw new HimallException("产品编号错误");
            if (product.IsDeleted)
                throw new HimallException("产品编号错误");
            ShopBranchInfo shopBranchById = this._iShopBranchService.GetShopBranchById(bid);
            if (shopBranchById == null)
                throw new HimallException("门店不存在");
            List<ShopBranchSkusInfo> skus = this._iShopBranchService.GetSkus(shopBranchById.ShopId, (IEnumerable<long>)new List<long>()
      {
        bid
      }, new ShopBranchSkuStatus?(ShopBranchSkuStatus.Normal));
            if (skus == null || skus.Count <= 0)
                throw new HimallException("门店商品不存在");
            bool flag = false;
            foreach (ShopBranchSkusInfo shopBranchSkusInfo in skus)
            {
                if (shopBranchSkusInfo.Status == ShopBranchSkuStatus.Normal)
                    flag = true;
            }
            if (!flag)
                throw new HimallException("门店商品已下架");
            ProductShowSkuInfoModel showSkuInfoModel = new ProductShowSkuInfoModel();
            showSkuInfoModel.MinSalePrice = product.MinSalePrice;
            showSkuInfoModel.ProductImagePath = product.RelativePath;
            showSkuInfoModel.MeasureUnit = product.MeasureUnit;
            showSkuInfoModel.MaxBuyCount = product.MaxBuyCount;
            ProductTypeInfo type = this._iTypeService.GetType(product.TypeId);
            string str1 = type == null || string.IsNullOrEmpty(type.ColorAlias) ? EnumHelper.ToDescription((Enum)SpecificationType.Color) : type.ColorAlias;
            string str2 = type == null || string.IsNullOrEmpty(type.SizeAlias) ? EnumHelper.ToDescription((Enum)SpecificationType.Size) : type.SizeAlias;
            string str3 = type == null || string.IsNullOrEmpty(type.VersionAlias) ? EnumHelper.ToDescription((Enum)SpecificationType.Version) : type.VersionAlias;
            showSkuInfoModel.ColorAlias = str1;
            showSkuInfoModel.SizeAlias = str2;
            showSkuInfoModel.VersionAlias = str3;
            if (product.SKUInfo != null && Enumerable.Count<SKUInfo>((IEnumerable<SKUInfo>)product.SKUInfo) > 0 && skus.Count > 0)
            {
                long result1 = 0L;
                long result2 = 0L;
                long result3 = 0L;
                foreach (SKUInfo skuInfo in Enumerable.ToList<SKUInfo>((IEnumerable<SKUInfo>)Enumerable.OrderBy<SKUInfo, long>((IEnumerable<SKUInfo>)product.SKUInfo, (Func<SKUInfo, long>)(s => s.AutoId))))
                {
                    SKUInfo sku = skuInfo;
                    string[] strArray = sku.Id.Split('_');
                    if (Enumerable.Count<string>((IEnumerable<string>)strArray) > 0 && Enumerable.Count<ShopBranchSkusInfo>((IEnumerable<ShopBranchSkusInfo>)skus, (Func<ShopBranchSkusInfo, bool>)(x => x.SkuId == sku.Id)) > 0)
                    {
                        if (!long.TryParse(strArray[1], out result1))
                            ;
                        if (result1 != 0L && !Enumerable.Any<ProductSKU>((IEnumerable<ProductSKU>)showSkuInfoModel.Color, (Func<ProductSKU, bool>)(v => v.Value.Equals(sku.Color))))
                        {
                            long num = Enumerable.Sum<SKUInfo>(Enumerable.Where<SKUInfo>((IEnumerable<SKUInfo>)product.SKUInfo, (Func<SKUInfo, bool>)(s => s.Color.Equals(sku.Color))), (Func<SKUInfo, long>)(s => s.Stock));
                            showSkuInfoModel.Color.Add(new ProductSKU()
                            {
                                Name = "选择" + str1,
                                EnabledClass = num != 0L ? "enabled" : "disabled",
                                SelectedClass = "",
                                SkuId = result1,
                                Value = sku.Color,
                                Img = HimallIO.GetImagePath(sku.ShowPic, (string)null)
                            });
                        }
                    }
                    if (Enumerable.Count<string>((IEnumerable<string>)strArray) > 1 && Enumerable.Count<ShopBranchSkusInfo>((IEnumerable<ShopBranchSkusInfo>)skus, (Func<ShopBranchSkusInfo, bool>)(x => x.SkuId == sku.Id)) > 0)
                    {
                        if (!long.TryParse(strArray[2], out result2))
                            ;
                        if (result2 != 0L && !Enumerable.Any<ProductSKU>((IEnumerable<ProductSKU>)showSkuInfoModel.Size, (Func<ProductSKU, bool>)(v => v.Value.Equals(sku.Size))))
                        {
                            long num = Enumerable.Sum<SKUInfo>(Enumerable.Where<SKUInfo>((IEnumerable<SKUInfo>)product.SKUInfo, (Func<SKUInfo, bool>)(s => s.Size.Equals(sku.Size))), (Func<SKUInfo, long>)(s1 => s1.Stock));
                            showSkuInfoModel.Size.Add(new ProductSKU()
                            {
                                Name = "选择" + str2,
                                EnabledClass = num != 0L ? "enabled" : "disabled",
                                SelectedClass = "",
                                SkuId = result2,
                                Value = sku.Size
                            });
                        }
                    }
                    if (Enumerable.Count<string>((IEnumerable<string>)strArray) > 2 && Enumerable.Count<ShopBranchSkusInfo>((IEnumerable<ShopBranchSkusInfo>)skus, (Func<ShopBranchSkusInfo, bool>)(x => x.SkuId == sku.Id)) > 0)
                    {
                        if (!long.TryParse(strArray[3], out result3))
                            ;
                        if (result3 != 0L && !Enumerable.Any<ProductSKU>((IEnumerable<ProductSKU>)showSkuInfoModel.Version, (Func<ProductSKU, bool>)(v => v.Value.Equals(sku.Version))))
                        {
                            long num = Enumerable.Sum<SKUInfo>(Enumerable.Where<SKUInfo>((IEnumerable<SKUInfo>)product.SKUInfo, (Func<SKUInfo, bool>)(s => s.Version.Equals(sku.Version))), (Func<SKUInfo, long>)(s => s.Stock));
                            showSkuInfoModel.Version.Add(new ProductSKU()
                            {
                                Name = "选择" + str3,
                                EnabledClass = num != 0L ? "enabled" : "disabled",
                                SelectedClass = "",
                                SkuId = result3,
                                Value = sku.Version
                            });
                        }
                    }
                }
            }
            return (ActionResult)this.View((object)showSkuInfoModel);
        }

        [HttpPost]
        public JsonResult GetHasSku(long id)
        {
            return this.Json((object)new
            {
                hassku = this._iProductService.HasSKU(id)
            });
        }

        public JsonResult GetProductDescription(long pid)
        {
            ProductDescriptionInfo productDescription = ServiceHelper.Create<IProductService>().GetProductDescription(pid);
            if (productDescription == null)
                throw new HimallException("错误的商品编号");
            string str1 = "";
            string str2 = "";
            IProductDescriptionTemplateService descriptionTemplateService = ServiceHelper.Create<IProductDescriptionTemplateService>();
            if (productDescription.DescriptionPrefixId != 0L)
            {
                ProductDescriptionTemplateInfo template = descriptionTemplateService.GetTemplate(productDescription.DescriptionPrefixId, productDescription.ProductInfo.ShopId);
                str1 = template == null ? "" : template.MobileContent;
            }
            if (productDescription.DescriptiondSuffixId != 0L)
            {
                ProductDescriptionTemplateInfo template = descriptionTemplateService.GetTemplate(productDescription.DescriptiondSuffixId, productDescription.ProductInfo.ShopId);
                str2 = template == null ? "" : template.MobileContent;
            }
            return this.Json((object)new
            {
                prodes = productDescription.ShowMobileDescription,
                prodesPrefix = str1,
                prodesSuffix = str2
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetHotProduct(long productId, int categoryId)
        {
            ProductRelationProduct productByProductId = ProductManagerApplication.GetRelationProductByProductId(productId);
            List<Product> list = productByProductId != null && ProductManagerApplication.GetProductsByIds((IEnumerable<long>)productByProductId.RelationProductIds).Count != 0 ? ProductManagerApplication.GetProductsByIds((IEnumerable<long>)productByProductId.RelationProductIds) : ProductManagerApplication.GetHotSaleProductByCategoryId(categoryId, 10);
            foreach (Product product in list)
                product.ImagePath = product.GetImage(ImageSize.Size_220, 1);
            return this.Json((object)list, true);
        }

        public JsonResult CanBuy(long productId, int count)
        {
            if (this.CurrentUser == null)
                return this.Json((object)new
                {
                    Result = true
                }, true);
            if (this.CurrentUser.Disabled)
                return this.Json((object)new
                {
                    Result = false,
                    ResultType = -10,
                    Message = "用户被冻结"
                }, true);
            Dictionary<int, string> dictionary = new Dictionary<int, string>()
      {
        {
          0,
          ""
        },
        {
          1,
          "商品已下架"
        },
        {
          2,
          "很抱歉，您查看的商品不存在，可能被转移。"
        },
        {
          3,
          "超出商品最大限购数"
        },
        {
          9,
          "商品无货"
        }
      };
            int reason;
            return this.Json((object)new
            {
                Result = ProductManagerApplication.BranchCanBuy(this.CurrentUser.Id, productId, count, out reason),
                ResultType = reason,
                Message = dictionary[reason]
            }, true);
        }

        public JsonResult GetDistributionInfo(long id)
        {
            ProductGetDistributionInfoModel distributionInfoModel1 = new ProductGetDistributionInfoModel()
            {
                ProductId = id
            };
            UserMemberInfo currentUser = this.CurrentUser;
            distributionInfoModel1.ShareUrl = this.Request.Url.Scheme + "://" + this.Request.Url.Authority + "/m-wap/product/Detail/" + id.ToString();
            if (currentUser != null && currentUser.Id > 0L)
            {
                distributionInfoModel1.UserId = currentUser.Id;
                Promoter promoterByUserId = DistributionApplication.GetPromoterByUserId(distributionInfoModel1.UserId);
                if (promoterByUserId != null)
                {
                    distributionInfoModel1.IsPromoter = true;
                    distributionInfoModel1.PromoterStatus = promoterByUserId.Status;
                    if (promoterByUserId.Status == PromoterInfo.PromoterStatus.Audited || promoterByUserId.Status == PromoterInfo.PromoterStatus.NotAvailable)
                    {
                        ProductGetDistributionInfoModel distributionInfoModel2 = distributionInfoModel1;
                        string str = distributionInfoModel2.ShareUrl + "?partnerid=" + distributionInfoModel1.UserId.ToString();
                        distributionInfoModel2.ShareUrl = str;
                    }
                }
            }
            DistributionProducts distributionProductInfo = DistributionApplication.GetDistributionProductInfo(id);
            if (distributionProductInfo != null)
            {
                distributionInfoModel1.IsDistribution = true;
                distributionInfoModel1.Brokerage = distributionProductInfo.Commission;
            }
            distributionInfoModel1.WeiXinShareArgs = WXApiApplication.GetWeiXinShareArgs(this.HttpContext.Request.UrlReferrer.AbsoluteUri);
            return this.Json((object)distributionInfoModel1, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ApplyDistribution(long id)
        {
            if (base.CurrentUser == null)
            {
                BaseController.Result result = new BaseController.Result
                {
                    success = false,
                    msg = "请登录"
                };
                return base.Json(result);
            }
            BaseController.Result data = new BaseController.Result();
            long userId = base.CurrentUser.Id;
            RecruitSettingInfo recruitmentSetting = this._iDistributionService.GetRecruitmentSetting();
            if (recruitmentSetting == null)
            {
                BaseController.Result result3 = new BaseController.Result
                {
                    success = false,
                    msg = "平台未设置招募审核！"
                };
                return base.Json(result3);
            }
            if (recruitmentSetting.MustRealName || recruitmentSetting.MustAddress)
            {
                BaseController.Result result4 = new BaseController.Result
                {
                    success = false,
                    status = -1
                };
                return base.Json(result4);
            }
            string str = "";
            if (recruitmentSetting.MustMobile)
            {
                str = this._iMessageService.GetDestination(userId, "Himall.Plugin.Message.SMS", MemberContactsInfo.UserTypes.General);
                if (string.IsNullOrEmpty(str))
                {
                    BaseController.Result result5 = new BaseController.Result
                    {
                        success = false,
                        status = -1
                    };
                    return base.Json(result5);
                }
            }
            PromoterInfo promoterByUserId = this._iDistributionService.GetPromoterByUserId(userId);
            if ((promoterByUserId != null) && (promoterByUserId.Status == PromoterInfo.PromoterStatus.Audited))
            {
                BaseController.Result result6 = new BaseController.Result
                {
                    success = true,
                    msg = "你已经是销售员了！"
                };
                return base.Json(result6);
            }
            Himall.Model.PromoterModel model = new Himall.Model.PromoterModel
            {
                Mobile = str,
                UserId = userId
            };
            _iDistributionService.ApplyForDistributor(model);
            promoterByUserId = this._iDistributionService.GetPromoterByUserId(userId);
            data = new BaseController.Result
            {
                success = true,
                msg = "提交成功！",
                status = 1
            };
            if ((promoterByUserId != null) && (promoterByUserId.Status == PromoterInfo.PromoterStatus.UnAudit))
            {
                data = new BaseController.Result
                {
                    success = false,
                    msg = "提交成功！",
                    status = 2
                };
            }
            return base.Json(data);

        }

        public ActionResult ShowDepotAddress(long ftid)
        {
            FreightTemplateInfo freightTemplate = this._iFreightTemplateService.GetFreightTemplate(ftid);
            string str = string.Empty;
            if (freightTemplate != null && freightTemplate.SourceAddress.HasValue)
            {
                string fullName = this._iRegionService.GetFullName((long)freightTemplate.SourceAddress.Value, " ");
                if (fullName != null)
                {
                    string[] strArray = fullName.Split(' ');
                    str = strArray.Length < 2 ? strArray[0] : strArray[0] + " " + strArray[1];
                }
            }
            freightTemplate.DepotAddress = str;
            return (ActionResult)this.View((object)freightTemplate);
        }

        [ChildActionOnly]
        public ActionResult ShowServicePromise(long id, long shopId)
        {
            CashDepositsObligation cashDepositsObligation = this._iCashDepositsService.GetCashDepositsObligation(id);
            int regionId = 0;
            if (base.CurrentUser != null)
            {
                ShippingAddressInfo defaultUserShippingAddressByUserId = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(base.CurrentUser.Id);
                if (defaultUserShippingAddressByUserId != null)
                {
                    regionId = defaultUserShippingAddressByUserId.RegionId;
                }
            }
            if (regionId == 0)
            {
                regionId = (int)RegionApplication.GetRegionByIPInTaobao(WebHelper.GetIP());
            }
            Region region = RegionApplication.GetRegion((long)regionId, Region.RegionLevel.City);
            if (region != null)
            {
                ShopBranchQuery query = new ShopBranchQuery
                {
                    ShopId = shopId,
                    Status = 0,
                    ProductIds = new long[] { id },
                    AddressPath = region.GetIdPath(",")
                };
                cashDepositsObligation.CanSelfTake = ShopBranchApplication.Exists(query);
            }
            ((dynamic)base.ViewBag).ProductId = id;
            ((dynamic)base.ViewBag).ShopId = shopId;
            return base.View(cashDepositsObligation);

        }

        [ChildActionOnly]
        public ActionResult ShowProductDescription(long id)
        {
            return (ActionResult)this.View((object)id);
        }

        public ActionResult ProductCommentShow(long id, int top = 1, bool isshowtit = false)
        {
            ProductCommentShowModel commentShowModel = new ProductCommentShowModel();
            commentShowModel.ProductId = id;
            ProductInfo product = this._iProductService.GetProduct(id);
            commentShowModel.CommentList = new List<ProductDetailCommentModel>();
            commentShowModel.IsShowColumnTitle = isshowtit;
            commentShowModel.IsShowCommentList = true;
            if (top < 1)
                commentShowModel.IsShowCommentList = false;
            if (product == null)
                throw new HimallException("商品不存在");
            if (product.IsDeleted)
                throw new HimallException("商品不存在");
            IEnumerable<ProductCommentInfo> source = Enumerable.Where<ProductCommentInfo>((IEnumerable<ProductCommentInfo>)product.Himall_ProductComments, (Func<ProductCommentInfo, bool>)(item => !item.IsHidden.HasValue || !item.IsHidden.Value));
            int num = Enumerable.Count<ProductCommentInfo>(source);
            commentShowModel.CommentCount = num;
            if (num > 0 && top > 0)
            {
                commentShowModel.CommentList = Enumerable.ToList<ProductDetailCommentModel>(Enumerable.Select<ProductCommentInfo, ProductDetailCommentModel>(Enumerable.Take<ProductCommentInfo>((IEnumerable<ProductCommentInfo>)Enumerable.OrderByDescending<ProductCommentInfo, DateTime>(source, (Func<ProductCommentInfo, DateTime>)(a => a.ReviewDate)), top), (Func<ProductCommentInfo, ProductDetailCommentModel>)(c => new ProductDetailCommentModel()
                {
                    Sku = this._iProductService.GetSkuString(c.Himall_OrderItems.SkuId),
                    UserName = c.UserName.Substring(0, 1) + "***" + c.UserName.Substring(c.UserName.Length - 1, 1),
                    ReviewContent = c.ReviewContent,
                    AppendContent = c.AppendContent,
                    AppendDate = c.AppendDate,
                    ReplyAppendContent = c.ReplyAppendContent,
                    ReplyAppendDate = c.ReplyAppendDate,
                    FinshDate = c.Himall_OrderItems.OrderInfo.FinishDate,
                    Images = Enumerable.ToList<string>(Enumerable.Select<ProductCommentsImagesInfo, string>(Enumerable.Where<ProductCommentsImagesInfo>((IEnumerable<ProductCommentsImagesInfo>)c.Himall_ProductCommentsImages, (Func<ProductCommentsImagesInfo, bool>)(a => a.CommentType == 0)), (Func<ProductCommentsImagesInfo, string>)(a => a.CommentImage))),
                    AppendImages = Enumerable.ToList<string>(Enumerable.Select<ProductCommentsImagesInfo, string>(Enumerable.Where<ProductCommentsImagesInfo>((IEnumerable<ProductCommentsImagesInfo>)c.Himall_ProductCommentsImages, (Func<ProductCommentsImagesInfo, bool>)(a => a.CommentType == 1)), (Func<ProductCommentsImagesInfo, string>)(a => a.CommentImage))),
                    ReviewDate = new DateTime?(c.ReviewDate),
                    ReplyContent = string.IsNullOrWhiteSpace(c.ReplyContent) ? "暂无回复" : c.ReplyContent,
                    ReplyDate = c.ReplyDate,
                    ReviewMark = c.ReviewMark,
                    BuyDate = new DateTime?(c.Himall_OrderItems.OrderInfo.OrderDate)
                })));
                foreach (ProductDetailCommentModel detailCommentModel in commentShowModel.CommentList)
                {
                    if (detailCommentModel.Images.Count > 0)
                    {
                        for (int index = 0; index < detailCommentModel.Images.Count; ++index)
                            detailCommentModel.Images[index] = HimallIO.GetRomoteImagePath(detailCommentModel.Images[index], (string)null);
                    }
                    if (detailCommentModel.AppendImages.Count > 0)
                    {
                        for (int index = 0; index < detailCommentModel.AppendImages.Count; ++index)
                            detailCommentModel.AppendImages[index] = HimallIO.GetRomoteImagePath(detailCommentModel.AppendImages[index], (string)null);
                    }
                }
            }
            return (ActionResult)this.View((object)commentShowModel);
        }

        public JsonResult GetIsOpenStore()
        {
            var fAnonymousType35 = new
            {
                Success = true,
                IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore
            };
            return this.Json((object)fAnonymousType35, true);
        }

        public JsonResult GetStroreInfo(long shopId, long productId, string fromLatLng = "")
        {
            if (shopId <= 0L)
                return this.Json((object)new
                {
                    Success = false,
                    Message = "请传入合法商家ID"
                }, true);
            if (fromLatLng.Split(',').Length != 2)
                return this.Json((object)new
                {
                    Success = false,
                    Message = "您当前定位信息异常"
                }, true);
            List<ShopBranch> list = Enumerable.ToList<ShopBranch>(Enumerable.Where<ShopBranch>((IEnumerable<ShopBranch>)ShopBranchApplication.GetShopBranchsAll(new ShopBranchQuery()
            {
                ShopId = shopId,
                FromLatLng = fromLatLng,
                Status = new ShopBranchStatus?(ShopBranchStatus.Normal),
                ProductIds = new long[1]
        {
          productId
        }
            }).Models, (Func<ShopBranch, bool>)(p => (double)p.Latitude > 0.0 && (double)p.Longitude > 0.0)));
            int count = list.Count;
            var fAnonymousType37 = new
            {
                Success = true,
                StoreInfo = Enumerable.FirstOrDefault<ShopBranch>(Enumerable.Take<ShopBranch>((IEnumerable<ShopBranch>)Enumerable.OrderBy<ShopBranch, double>((IEnumerable<ShopBranch>)list, (Func<ShopBranch, double>)(p => p.Distance)), 1)),
                Total = count
            };
            return this.Json((object)fAnonymousType37, true);
        }

        public JsonResult GetIsSelfDelivery(long shopId, long productId, string fromLatLng = "")
        {
            if (shopId <= 0L)
                return this.Json((object)new
                {
                    Message = "请传入合法商家ID",
                    IsSelfDelivery = 0
                }, true);
            if (fromLatLng.Split(',').Length != 2)
                return this.Json((object)new
                {
                    Message = "请传入合法经纬度",
                    IsSelfDelivery = 0
                }, true);
            ShopBranchQuery query = new ShopBranchQuery()
            {
                ShopId = shopId,
                Status = new ShopBranchStatus?(ShopBranchStatus.Normal)
            };
            string address = "";
            string province = "";
            string city = "";
            string district = "";
            string street = "";
            ShopbranchHelper.GetAddressByLatLng(fromLatLng, ref address, ref province, ref city, ref district, ref street);
            if (string.IsNullOrWhiteSpace(city))
                return this.Json((object)new
                {
                    Message = "无法获取当前城市",
                    IsSelfDelivery = 0
                }, true);
            Region regionByName = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
            if (regionByName == null)
                return this.Json((object)new
                {
                    Message = "获取当前城市异常",
                    IsSelfDelivery = 0
                }, true);
            query.CityId = regionByName.Id;
            query.ProductIds = new long[1]
      {
        productId
      };
            List<ShopBranch> models = ShopBranchApplication.GetShopBranchsAll(query).Models;
            List<SKU> skuInfos = ProductManagerApplication.GetSKU(productId);
            if (!skuInfos.Exists((Predicate<SKU>)(p => p.Id == string.Format("{0}_0_0_0", (object)productId))))
                skuInfos.Add(new SKU()
                {
                    Id = string.Format("{0}_0_0_0", (object)productId)
                });
            List<ShopBranchSkusInfo> shopBranchSkus = ShopBranchApplication.GetSkus(query.ShopId, Enumerable.Select<ShopBranch, long>((IEnumerable<ShopBranch>)models, (Func<ShopBranch, long>)(p => p.Id)));
            models.ForEach((Action<ShopBranch>)(p => p.Enabled = Enumerable.Count<SKU>(Enumerable.Where<SKU>((IEnumerable<SKU>)skuInfos, (Func<SKU, bool>)(skuInfo => Enumerable.Count<ShopBranchSkusInfo>(Enumerable.Where<ShopBranchSkusInfo>((IEnumerable<ShopBranchSkusInfo>)shopBranchSkus, (Func<ShopBranchSkusInfo, bool>)(sbSku => sbSku.ShopBranchId == p.Id && sbSku.Stock > 0 && sbSku.SkuId == skuInfo.Id))) > 0))) > 0));
            return this.Json((object)new
            {
                Message = "",
                IsSelfDelivery = (Enumerable.Count<ShopBranch>(Enumerable.Where<ShopBranch>((IEnumerable<ShopBranch>)models, (Func<ShopBranch, bool>)(p => p.Enabled))) > 0 ? 1 : 0)
            }, true);
        }
    }
}