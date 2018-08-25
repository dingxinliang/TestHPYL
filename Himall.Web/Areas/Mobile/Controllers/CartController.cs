using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Helper;
using Himall.DTO;
using Himall.IServices;
using Himall.Model;
using Himall.Web.App_Code;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class CartController : BaseMobileMemberController
    {
        private IProductService _iProductService;
        private IShopBranchService _iShopBranchService;
        private IShopService _iShopService;
        private IVShopService _iVShopService;
        public CartController(IProductService iProductService, IShopService iShopService, IVShopService iVShopService, IShopBranchService iShopBranchService)
        {
            this._iProductService = iProductService;
            this._iShopService = iShopService;
            this._iVShopService = iVShopService;
            this._iShopBranchService = iShopBranchService;
        }

        // GET: Mobile/Cart
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Cart()
        {
            return View();
        }

        [HttpPost]
        public JsonResult AddProductToCart(string skuId, int count)
        {
            CartHelper cartHelper = new CartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            cartHelper.AddToCart(skuId, count, userId);
            return Json(new { success = true });
        }

        [HttpPost]


        public JsonResult GetCartProducts()
        {
            ShoppingCartInfo cart = new CartHelper().GetCart(base.CurrentUser.Id);
            IProductService productService = this._iProductService;
            IShopService shopService = this._iShopService;
            IVShopService vshopService = this._iVShopService;
            decimal discount = 1.0M;
            if (base.CurrentUser != null)
            {
                discount = base.CurrentUser.MemberDiscount;
            }
            List<long> list = new List<long>();
            decimal prodPrice = 0.0M;
            List<FlashSalePrice> limitProducts = LimitTimeApplication.GetPriceByProducrIds((from e in cart.Items select e.ProductId).ToList<long>());
            var source = from s in cart.Items.Where<ShoppingCartItem>(delegate(ShoppingCartItem d)
                         {
                             long? nullable;
                             return (!d.ShopBranchId.HasValue || (((nullable = d.ShopBranchId).GetValueOrDefault() == 0L) && nullable.HasValue));
                         }).Select(delegate(ShoppingCartItem item)
                         {
                             Func<FlashSalePrice, bool> predicate = null;
                             ProductInfo product = productService.GetProduct(item.ProductId);
                             ShopInfo shop = shopService.GetShop(product.ShopId, false);
                             SKUInfo sku = null;
                             string str = "";
                             string str2 = "";
                             string str3 = "";
                             string str4 = "";
                             if (null != shop)
                             {
                                 string str5;
                                 VShopInfo vShopByShopId = vshopService.GetVShopByShopId(shop.Id);
                                 sku = productService.GetSku(item.SkuId);
                                 if (sku == null)
                                 {
                                     return null;
                                 }
                                 if (predicate == null)
                                 {
                                     predicate = e => e.ProductId == item.ProductId;
                                 }
                                 FlashSalePrice price = limitProducts.FirstOrDefault<FlashSalePrice>(predicate);
                                 prodPrice = sku.SalePrice;
                                 if (price != null)
                                 {
                                     prodPrice = price.MinPrice;
                                 }
                                 else if (shop.IsSelf)
                                 {
                                     prodPrice = sku.SalePrice * discount;
                                 }
                                 ProductType type = TypeApplication.GetType(product.TypeId);
                                 str = ((type == null) || string.IsNullOrEmpty(type.ColorAlias)) ? SpecificationType.Color.ToDescription() : type.ColorAlias;
                                 str2 = ((type == null) || string.IsNullOrEmpty(type.SizeAlias)) ? SpecificationType.Size.ToDescription() : type.SizeAlias;
                                 str3 = ((type == null) || string.IsNullOrEmpty(type.VersionAlias)) ? SpecificationType.Version.ToDescription() : type.VersionAlias;
                                 str4 = "";
                                 if (!string.IsNullOrWhiteSpace(sku.Size))
                                 {
                                     str5 = str4;
                                     str4 = str5 + str2 + "：" + sku.Size + "&nbsp;&nbsp;";
                                 }
                                 if (!string.IsNullOrWhiteSpace(sku.Color))
                                 {
                                     str5 = str4;
                                     str4 = str5 + str + "：" + sku.Color + "&nbsp;&nbsp;";
                                 }
                                 if (!string.IsNullOrWhiteSpace(sku.Version))
                                 {
                                     str5 = str4;
                                     str4 = str5 + str3 + "：" + sku.Version + "&nbsp;&nbsp;";
                                 }
                                 return new
                                 {
                                     cartItemId = item.Id,
                                     skuId = item.SkuId,
                                     id = product.Id,
                                     imgUrl = HimallIO.GetProductSizeImage(product.RelativePath, 1, 150),
                                     name = product.ProductName,
                                     price = prodPrice,
                                     count = item.Quantity,
                                     shopId = shop.Id,
                                     vshopId = (vShopByShopId == null) ? 0L : vShopByShopId.Id,
                                     shopName = shop.ShopName,
                                     shopLogo = (vShopByShopId == null) ? "" : vShopByShopId.Logo,
                                     status = ((product.AuditStatus == ProductInfo.ProductAuditStatus.Audited) && (product.SaleStatus == ProductInfo.ProductSaleStatus.OnSale)) ? 1 : 0,
                                     Size = sku.Size,
                                     Color = sku.Color,
                                     Version = sku.Version,
                                     ColorAlias = str,
                                     SizeAlias = str2,
                                     VersionAlias = str3,
                                     skuDetails = str4,
                                     AddTime = item.AddTime
                                 };
                             }
                             return null;
                         })
                         where s != null
                         orderby s.vshopId, s.AddTime descending
                         select s;
            BranchCartHelper helper2 = new BranchCartHelper();
            long memberId = 0L;
            if (base.CurrentUser != null)
            {
                memberId = base.CurrentUser.Id;
            }
            ShoppingCartInfo cartNoCache = helper2.GetCartNoCache(memberId, 0L);
            List<long> list2 = (from x in
                                    (from x in cartNoCache.Items
                                     where x.ShopBranchId.HasValue
                                     select x.ShopBranchId.Value).ToList<long>()
                                group x by x into x
                                select x.First<long>()).ToList<long>();
            Dictionary<long, int> buyedCounts = null;
            if (memberId > 0L)
            {
                cart = helper2.GetCart(memberId, 0L);
                buyedCounts = new Dictionary<long, int>();
                buyedCounts = OrderApplication.GetProductBuyCount(memberId, from x in cart.Items select x.ProductId);
            }
            List<object> list3 = new List<object>();
            using (List<long>.Enumerator enumerator = list2.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Func<ShoppingCartItem, bool> func = null;
                    long shopBranchId = enumerator.Current;
                    prodPrice = 0.0M;
                    List<ShopBranchSkusInfo> shopBranchSkuList = this._iShopBranchService.GetSkusByIds(shopBranchId, from x in cartNoCache.Items select x.SkuId);
                    if (func == null)
                    {
                        func = delegate(ShoppingCartItem x)
                        {
                            long? nullable1 = x.ShopBranchId;
                            long num = shopBranchId;
                            return (nullable1.GetValueOrDefault() == num) && nullable1.HasValue;
                        };
                    }
                    var enumerable2 = from s in cartNoCache.Items.Where<ShoppingCartItem>(func).Select(delegate(ShoppingCartItem item)
                                      {
                                          Func<ShopBranchSkusInfo, bool> predicate = null;
                                          if (shopBranchId == 0x63L)
                                          {
                                          }
                                          ShopBranchInfo shopBranchById = this._iShopBranchService.GetShopBranchById(shopBranchId);
                                          ProductInfo product = this._iProductService.GetProduct(item.ProductId);
                                          ShopInfo shop = this._iShopService.GetShop(product.ShopId, false);
                                          SKUInfo sku = null;
                                          if ((shop != null) && (shopBranchById != null))
                                          {
                                              VShopInfo vShopByShopId = this._iVShopService.GetVShopByShopId(shop.Id);
                                              sku = this._iProductService.GetSku(item.SkuId);
                                              if (sku == null)
                                              {
                                                  return null;
                                              }
                                              prodPrice = sku.SalePrice;
                                              if (shop.IsSelf)
                                              {
                                                  prodPrice = sku.SalePrice * discount;
                                              }
                                              if (predicate == null)
                                              {
                                                  predicate = x => x.SkuId == item.SkuId;
                                              }
                                              ShopBranchSkusInfo info6 = shopBranchSkuList.FirstOrDefault<ShopBranchSkusInfo>(predicate);
                                              long maxBuyCount = (info6 == null) ? ((long)0) : ((long)info6.Stock);
                                              if ((maxBuyCount > product.MaxBuyCount) && (product.MaxBuyCount > 0))
                                              {
                                                  maxBuyCount = product.MaxBuyCount;
                                              }
                                              if (((product.MaxBuyCount > 0) && (buyedCounts != null)) && buyedCounts.ContainsKey(item.ProductId))
                                              {
                                                  int num3 = buyedCounts[item.ProductId];
                                                  maxBuyCount -= num3;
                                              }
                                              return new { cartItemId = item.Id, skuId = item.SkuId, id = product.Id, imgUrl = HimallIO.GetProductSizeImage(product.RelativePath, 1, 150), name = product.ProductName, price = prodPrice, count = item.Quantity, status = (info6 == null) ? 1 : ((info6.Status == ShopBranchSkuStatus.Normal) ? ((item.Quantity > maxBuyCount) ? 2 : 0) : 1), AddTime = item.AddTime, shopBranchId = shopBranchById.Id, shopBranchName = shopBranchById.ShopBranchName };
                                          }
                                          return null;
                                      })
                                      where s != null
                                      orderby s.AddTime descending
                                      select s;
                    list3.Add(enumerable2);
                }
            }
            var data = new
            {
                products = source,
                amount = Enumerable.Sum(source, item => item.price * (Decimal)item.count),
                totalCount = Enumerable.Sum(source, item => item.count),
                shopBranchCart = list3
            };
            return base.Json(data);

        }

        [HttpPost]
        public JsonResult UpdateCartItem(string skuId, int count)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            var cartHelper = new CartHelper();
            cartHelper.UpdateCartItem(skuId, count, userId);
            return Json(new { success = true });
        }
        [HttpPost]
        public JsonResult GetBranchCartProducts(long shopBranchId)
        {
            BranchCartHelper helper = new BranchCartHelper();
            long memberId = 0L;
            decimal discount = 1.0M;
            if (base.CurrentUser != null)
            {
                memberId = base.CurrentUser.Id;
                discount = base.CurrentUser.MemberDiscount;
            }
            ShoppingCartInfo cart = helper.GetCart(memberId, shopBranchId);
            ShopBranchInfo shopBranchById = this._iShopBranchService.GetShopBranchById(shopBranchId);
            Dictionary<long, int> buyedCounts = null;
            if (memberId > 0L)
            {
                buyedCounts = new Dictionary<long, int>();
                buyedCounts = OrderApplication.GetProductBuyCount(memberId, from x in cart.Items select x.ProductId);
            }
            decimal prodPrice = 0.0M;
            List<ShopBranchSkusInfo> shopBranchSkuList = this._iShopBranchService.GetSkusByIds(shopBranchId, from x in cart.Items select x.SkuId);
            var enumerable = from s in cart.Items.Select(delegate(ShoppingCartItem item)
                             {
                                 ProductInfo product = this._iProductService.GetProduct(item.ProductId);
                                 ShopBranchSkusInfo info2 = shopBranchSkuList.FirstOrDefault<ShopBranchSkusInfo>(x => x.SkuId == item.SkuId);
                                 long maxBuyCount = (info2 == null) ? ((long)0) : ((long)info2.Stock);
                                 if ((maxBuyCount > product.MaxBuyCount) && (product.MaxBuyCount != 0))
                                 {
                                     maxBuyCount = product.MaxBuyCount;
                                 }
                                 if (((product.MaxBuyCount > 0) && (buyedCounts != null)) && buyedCounts.ContainsKey(item.ProductId))
                                 {
                                     int num2 = buyedCounts[item.ProductId];
                                     maxBuyCount -= num2;
                                 }
                                 ShopInfo shop = this._iShopService.GetShop(product.ShopId, false);
                                 SKUInfo sku = null;
                                 string str = "";
                                 if (null != shop)
                                 {
                                     VShopInfo vShopByShopId = this._iVShopService.GetVShopByShopId(shop.Id);
                                     sku = this._iProductService.GetSku(item.SkuId);
                                     if (sku == null)
                                     {
                                         return null;
                                     }
                                     prodPrice = sku.SalePrice;
                                     if (shop.IsSelf)
                                     {
                                         prodPrice = sku.SalePrice * discount;
                                     }
                                     ProductType type = TypeApplication.GetType(product.TypeId);
                                     str = "";
                                     if (!string.IsNullOrWhiteSpace(sku.Size))
                                     {
                                         str = str + sku.Size + "&nbsp;&nbsp;";
                                     }
                                     if (!string.IsNullOrWhiteSpace(sku.Color))
                                     {
                                         str = str + sku.Color + "&nbsp;&nbsp;";
                                     }
                                     if (!string.IsNullOrWhiteSpace(sku.Version))
                                     {
                                         str = str + sku.Version + "&nbsp;&nbsp;";
                                     }
                                     return new { bId = shopBranchId, cartItemId = item.Id, skuId = item.SkuId, id = product.Id, name = product.ProductName, price = prodPrice, count = item.Quantity, stock = (info2 == null) ? 0L : maxBuyCount, status = (info2 == null) ? 1 : ((info2.Status == ShopBranchSkuStatus.Normal) ? ((item.Quantity > maxBuyCount) ? 2 : 0) : 1), skuDetails = str, AddTime = item.AddTime };
                                 }
                                 return null;
                             })
                             where s != null
                             orderby s.status, s.AddTime descending
                             select s;
            var data = new
            {
                products = enumerable,
                amount = Enumerable.Sum(Enumerable.Where(enumerable, x => x.status == 0), item => item.price * (Decimal)item.count),
                totalCount = Enumerable.Sum(Enumerable.Where(enumerable, x => x.status == 0), item => item.count),
                DeliveFee = shopBranchById.DeliveFee,
                DeliveTotalFee = shopBranchById.DeliveTotalFee,
                FreeMailFee = shopBranchById.FreeMailFee,
                shopBranchStatus = shopBranchById.Status
            };
            return base.Json(data);

        }



        public JsonResult UpdateCartItem(Dictionary<string, int> skus, long userId)
        {
            var cartHelper = new CartHelper();
            foreach (var sku in skus)
            {
                cartHelper.UpdateCartItem(sku.Key, sku.Value, userId);
            }
            return Json(new { success = true });
        }
        [HttpPost]
        public JsonResult EditBranchProductToCart(string skuId, int count, long shopBranchId)
        {
            BranchCartHelper helper = new BranchCartHelper();
            long memberId = (base.CurrentUser != null) ? base.CurrentUser.Id : 0L;
            helper.UpdateCartItem(skuId, count, memberId, shopBranchId);
            return base.Json(new { success = true });
        }
        public JsonResult ClearBranchCartProducts(long shopBranchId)
        {
            BranchCartHelper helper = new BranchCartHelper();
            long memberId = (base.CurrentUser != null) ? base.CurrentUser.Id : 0L;
            ShoppingCartInfo cart = helper.GetCart(memberId, shopBranchId);
            foreach (ShoppingCartItem item in cart.Items)
            {
                helper.RemoveFromCart(item.SkuId, memberId, shopBranchId);
            }
            return base.Json(new { success = true });
        }
        [HttpPost]
        public JsonResult BatchRemoveFromCart(string skuIds)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;

            var skuIdsArr = skuIds.Split(',');
            var cartHelper = new CartHelper();
            cartHelper.RemoveFromCart(skuIdsArr, userId);
            return Json(new { success = true });
        }
        [HttpPost]
        public JsonResult RemoveFromCart(string skuId)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;

            var cartHelper = new CartHelper();
            cartHelper.RemoveFromCart(skuId, userId);
            return Json(new { success = true });
        }
    }
}