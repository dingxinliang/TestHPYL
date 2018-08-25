using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Framework;
using Himall.CommonModel;
using Himall.Application;
using Himall.Web.Models;
using Himall.DTO;
using Himall.Web.Common;
using Himall.Core.Helper;
using Himall.IServices.QueryModel;
using Himall.IServices;
using Himall.Model;
using Himall.Core;
using Himall.Web.App_Code;
using Himall.Web.Areas.Web.Models;
using System.Linq.Expressions;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class ShopBranchController : BaseMobileTemplatesController
    {
        // Fields
        private ICartService _iCartService;
        private ICouponService _iCouponService;
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private IProductService _iProductService;
        private IShopBranchService _iShopBranchService;
        private ITypeService _iTypeService;

        // Methods
        public ShopBranchController(ILimitTimeBuyService iLimitTimeBuyService, ICartService iCartService, IProductService iProductService, ITypeService iTypeService, ICouponService iCouponService, IShopBranchService iShopBranchService)
        {
            this._iLimitTimeBuyService = iLimitTimeBuyService;
            this._iCartService = iCartService;
            this._iProductService = iProductService;
            this._iTypeService = iTypeService;
            this._iCouponService = iCouponService;
            this._iShopBranchService = iShopBranchService;
        }

        [HttpPost]
        public JsonResult GetBranchList(long shopBranchId, string fromLatLng)
        {
            ShopBranch shopBranchById = ShopBranchApplication.GetShopBranchById(shopBranchId);
            if (!((shopBranchById == null) || string.IsNullOrWhiteSpace(fromLatLng)))
            {
                shopBranchById.Distance = ShopBranchApplication.GetLatLngDistances(fromLatLng, string.Format("{0},{1}", shopBranchById.Latitude, shopBranchById.Longitude));
            }
            List<ShopBranch> list = new List<ShopBranch> {
            shopBranchById
        };
            List<HomePageShopBranch> list2 = this.ProcessBranchHomePageData(list);
            bool userCoupons = this.GetUserCoupons(shopBranchById.ShopId);
            if (list2.Count > 0)
            {
                return base.Json(new { Success = true, data = list2, isCouponsReceived = userCoupons });
            }
            return base.Json(new { Success = false });
        }

        private IEnumerable<CouponInfo> GetCouponList(long shopid)
        {
            Func<CouponInfo, bool> predicate = null;
            IQueryable<CouponInfo> couponList = ServiceHelper.Create<ICouponService>().GetCouponList(shopid);
            IQueryable<long> couponSetList = from item in ServiceHelper.Create<IVShopService>().GetVShopCouponSetting(shopid)
                                             where ((int)item.PlatForm) == 4
                                             select item.CouponID;
            if ((couponList.Count<CouponInfo>() > 0) && (couponSetList.Count<long>() > 0))
            {
                if (predicate == null)
                {
                    predicate = item => couponSetList.Contains<long>(item.Id);
                }
                return couponList.ToArray<CouponInfo>().Where<CouponInfo>(predicate);
            }
            return new List<CouponInfo>();
        }

        [HttpPost]
        public JsonResult GetLoadCoupon(long shopid)
        {
            Func<CouponInfo, bool> predicate = null;
            IQueryable<CouponInfo> couponList = this._iCouponService.GetCouponList(shopid);
            IQueryable<long> couponSetList = from item in ServiceHelper.Create<IVShopService>().GetVShopCouponSetting(shopid)
                                             where ((int)item.PlatForm) == 4
                                             select item.CouponID;
            if ((couponList.Count<CouponInfo>() > 0) && (couponSetList.Count<long>() > 0))
            {
                if (predicate == null)
                {
                    predicate = item => couponSetList.Contains<long>(item.Id);
                }
                List<CouponInfo> list = couponList.ToArray<CouponInfo>().Where<CouponInfo>(predicate).ToList<CouponInfo>();
                List<CouponRecord> list2 = new List<CouponRecord>();
                foreach (CouponInfo info in list)
                {
                    int num = 0;
                    if (base.CurrentUser != null)
                    {
                        num = ShopBranchApplication.CouponIsUse(info, base.CurrentUser.Id);
                    }
                    CouponRecord record = new CouponRecord
                    {
                        CouponId = info.Id,
                        CouponName = info.CouponName,
                        Price = Math.Round(info.Price, 2),
                        StartTime = info.StartTime.ToString("yyyy.MM.dd"),
                        ClosingTime = info.EndTime.ToString("yyyy.MM.dd"),
                        IsUse = num
                    };
                    list2.Add(record);
                }
                return base.Json(new { Success = true, Data = list2 });
            }
            return base.Json(new { Success = false, Message = "没有可领取的优惠券" });
        }

        public JsonResult GetShopBranchProductAndSaleCount(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                throw new HimallException("非法参数");
            }
            ShoppingCartInfo memberCartInfo = new ShoppingCartInfo();
            if (base.CurrentUser != null)
            {
                memberCartInfo = CartApplication.GetShopBranchCart(base.CurrentUser.Id, 0L);
            }
            var enumerable = ids.Split(new char[] { ',' }).Select(delegate(string e)
            {
                ShopBranchProductQuery query = new ShopBranchProductQuery
                {
                    PageSize = int.MaxValue,
                    PageNo = 1,
                    shopBranchId = new long?(long.Parse(e)),
                    ShopBranchProductStatus = 0
                };
                QueryPageModel<ProductInfo> shopBranchProducts = ShopBranchApplication.GetShopBranchProducts(query);
                DateTime endDate = DateTime.Now;
                long num = ShopBranchApplication.GetShopBranchSaleCount(query.shopBranchId.Value, endDate.AddDays(-30.0).Date, endDate);
                int num2 = memberCartInfo.Items.Where<ShoppingCartItem>(delegate(ShoppingCartItem c)
                {
                    long? shopBranchId = 0;
                    if (c.ShopBranchId.HasValue)
                    {
                        num = c.ShopBranchId.Value;
                        shopBranchId = query.shopBranchId;
                    }
                    return (((num == shopBranchId.GetValueOrDefault()) && shopBranchId.HasValue) && (c.Status == 0));
                }).Sum<ShoppingCartItem>((Func<ShoppingCartItem, int>)(c => c.Quantity));
                return new
                {
                    saleCount = shopBranchProducts.Models.Sum<ProductInfo>((Func<ProductInfo, long>)(p => p.SaleCounts)),
                    saleCountByMonth = num,
                    cartQuantity = num2,
                    products = from p in
                                   (from p in shopBranchProducts.Models
                                    orderby p.SaleCounts descending, p.Id descending
                                    select p).Take<ProductInfo>(4)
                               select new ProductInfo { Id = p.Id, ImagePath = p.ImagePath, MinSalePrice = p.MinSalePrice, ProductName = p.ProductName },
                    branchId = e
                };
            });
            return base.Json(new { success = true, Data = enumerable }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetStoreByProductAndSaleCount(string ids, long? productId)
        {
            if (string.IsNullOrEmpty(ids))
            {
                throw new HimallException("非法参数");
            }
            ShoppingCartInfo memberCartInfo = new ShoppingCartInfo();
            if (base.CurrentUser != null)
            {
                memberCartInfo = CartApplication.GetShopBranchCart(base.CurrentUser.Id, 0L);
            }
            var enumerable = ids.Split(new char[] { ',' }).Select(delegate(string e)
            {
                Func<ProductInfo, bool> predicate = null;
                ShopBranchProductQuery query = new ShopBranchProductQuery
                {
                    PageSize = 0x7fffffff,
                    PageNo = 1,
                    OrderKey = 3,
                    shopBranchId = new long?(long.Parse(e)),
                    ShopBranchProductStatus = 0
                };
                QueryPageModel<ProductInfo> shopBranchProducts = ShopBranchApplication.GetShopBranchProducts(query);
                if (productId.HasValue)
                {
                    if (predicate == null)
                    {
                        predicate = n => n.Id == productId.Value;
                    }
                    ProductInfo item = shopBranchProducts.Models.FirstOrDefault<ProductInfo>(predicate);
                    shopBranchProducts.Models.Remove(item);
                    List<ProductInfo> list = (from p in shopBranchProducts.Models
                                              orderby p.SaleCounts descending, p.Id descending
                                              select p).Take<ProductInfo>(3).ToList<ProductInfo>();
                    if (null != item)
                    {
                        list.Insert(0, item);
                    }
                    shopBranchProducts.Models = list;
                }
                DateTime now = DateTime.Now;
                long num = ShopBranchApplication.GetShopBranchSaleCount(query.shopBranchId.Value, now.AddDays(-30.0).Date, now.Date);
                int num2 = memberCartInfo.Items.Where<ShoppingCartItem>(delegate(ShoppingCartItem c)
                {
                    long? nullable;
                    return (c.ShopBranchId.HasValue && ((c.ShopBranchId.Value == (nullable = query.shopBranchId).GetValueOrDefault()) && nullable.HasValue));
                }).Sum<ShoppingCartItem>((Func<ShoppingCartItem, int>)(c => c.Quantity));
                return new { saleCount = shopBranchProducts.Models.Sum<ProductInfo>((Func<ProductInfo, long>)(p => p.SaleCounts)), saleCountByMonth = num, cartQuantity = num2, products = from p in shopBranchProducts.Models select new ProductInfo { Id = p.Id, ImagePath = p.ImagePath, MinSalePrice = p.MinSalePrice, ProductName = p.ProductName }, branchId = e };
            });
            return base.Json(new { success = true, Data = enumerable }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetUserCoupon(long couponId)
        {
            bool flag = true;
            string str = "";
            CouponInfo couponInfo = this._iCouponService.GetCouponInfo(couponId);
            if (couponInfo.EndTime < DateTime.Now)
            {
                return base.Json(new { Success = false, Message = "优惠券已经过期" });
            }
            if (base.CurrentUser == null)
            {
                return base.Json(new { Success = false, Message = "用户未登录" });
            }
            CouponRecordQuery query = new CouponRecordQuery
            {
                CouponId = new long?(couponId),
                UserId = new long?(base.CurrentUser.Id)
            };
            ObsoletePageModel<CouponRecordInfo> couponRecordList = this._iCouponService.GetCouponRecordList(query);
            if ((couponInfo.PerMax != 0) && (couponRecordList.Total >= couponInfo.PerMax))
            {
                return base.Json(new { Success = false, Message = "达到领取最大张数" });
            }
            query = new CouponRecordQuery
            {
                CouponId = new long?(couponId)
            };
            if (this._iCouponService.GetCouponRecordList(query).Total >= couponInfo.Num)
            {
                return base.Json(new { Success = false, Message = "此优惠券已经领完了" });
            }
            if ((couponInfo.ReceiveType == CouponInfo.CouponReceiveType.IntegralExchange) && (MemberIntegralApplication.GetMemberIntegral(base.CurrentUser.Id).AvailableIntegrals < couponInfo.NeedIntegral))
            {
                return base.Json(new { Success = false, Message = "积分不足" });
            }
            if (flag)
            {
                CouponRecordInfo info = new CouponRecordInfo
                {
                    CouponId = couponId,
                    UserId = base.CurrentUser.Id,
                    UserName = base.CurrentUser.UserName,
                    ShopId = couponInfo.ShopId
                };
                this._iCouponService.AddCouponRecord(info);
                return base.Json(new { Success = true, Message = "领取成功" });
            }
            return base.Json(new { Success = false, Message = str });
        }

        public bool GetUserCoupons(long shopid)
        {
            Func<CouponInfo, bool> predicate = null;
            bool flag = false;
            try
            {
                long id = base.CurrentUser.Id;
            }
            catch
            {
                return true;
            }
            IQueryable<CouponInfo> couponList = this._iCouponService.GetCouponList(shopid);
            IQueryable<long> couponSetList = from item in ServiceHelper.Create<IVShopService>().GetVShopCouponSetting(shopid)
                                             where ((int)item.PlatForm) == 4
                                             select item.CouponID;
            if ((couponList.Count<CouponInfo>() > 0) && (couponSetList.Count<long>() > 0))
            {
                if (predicate == null)
                {
                    predicate = item => couponSetList.Contains<long>(item.Id);
                }
                List<CouponInfo> list = couponList.ToArray<CouponInfo>().Where<CouponInfo>(predicate).ToList<CouponInfo>();
                foreach (CouponInfo info in list)
                {
                    bool flag2 = true;
                    CouponRecordQuery query = new CouponRecordQuery
                    {
                        CouponId = new long?(info.Id),
                        UserId = new long?(base.CurrentUser.Id)
                    };
                    ObsoletePageModel<CouponRecordInfo> couponRecordList = this._iCouponService.GetCouponRecordList(query);
                    if ((info.PerMax != 0) && (couponRecordList.Total >= info.PerMax))
                    {
                        flag2 = false;
                    }
                    query = new CouponRecordQuery
                    {
                        CouponId = new long?(info.Id)
                    };
                    if (this._iCouponService.GetCouponRecordList(query).Total >= info.Num)
                    {
                        flag2 = false;
                    }
                    if (flag2)
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        public JsonResult GetUserShippingAddressesList()
        {
            if (base.CurrentUser == null)
            {
                BaseController.Result result = new BaseController.Result
                {
                    success = false,
                    msg = "nologin"
                };
                return base.Json(result, JsonRequestBehavior.AllowGet);
            }
            var enumerable = from addresses in OrderApplication.GetUserAddresses(base.CurrentUser.Id, 0L) select new { id = addresses.Id, fullRegionName = addresses.RegionFullName, address = addresses.Address, addressDetail = addresses.AddressDetail, phone = addresses.Phone, shipTo = addresses.ShipTo, fullRegionIdPath = addresses.RegionIdPath, latitude = addresses.Latitude, longitude = addresses.Longitude };
            BaseController.Result data = new BaseController.Result
            {
                success = true,
                Data = enumerable
            };
            return base.Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index(long id = 0L)
        {
            if (!((SiteSettingApplication.GetSiteSettings() != null) && SiteSettingApplication.GetSiteSettings().IsOpenStore))
            {
                throw new HimallException("门店未授权！");
            }
            ShopBranch shopBranchById = ShopBranchApplication.GetShopBranchById(id);
            if (shopBranchById == null)
            {
                return base.RedirectToAction("Error404", "Error", new { area = "Web" });
            }
            shopBranchById.AddressDetail = ShopBranchApplication.RenderAddress(shopBranchById.AddressPath, shopBranchById.AddressDetail, 2);
            ((dynamic)base.ViewBag).ShopBranch = shopBranchById;
            ((dynamic)base.ViewBag).ShopCategory = ShopCategoryApplication.GetCategoryByParentId(0L, shopBranchById.ShopId);
            ((dynamic)base.ViewBag).QQMapKey = "SYJBZ-DSLR3-IWX3Q-3XNTM-ELURH-23FTP";
            return base.View();
        }

        [HttpGet]
        public JsonResult List(int pageSize, int pageNo, string fromLatLng, string shopId)
        {
            if (!((SiteSettingApplication.GetSiteSettings() != null) && SiteSettingApplication.GetSiteSettings().IsOpenStore))
            {
                throw new HimallException("门店未授权！");
            }
            ShopBranchQuery query = new ShopBranchQuery
            {
                PageNo = pageNo,
                PageSize = pageSize,
                Status = 0,
                CityId = -1,
                FromLatLng = fromLatLng,
                OrderKey = 2,
                OrderType = true
            };
            if (query.FromLatLng.Split(new char[] { ',' }).Length != 2)
            {
                BaseController.Result data = new BaseController.Result
                {
                    success = false,
                    msg = "无法获取您的当前位置，请确认是否开启定位服务！"
                };
                return base.Json(data, JsonRequestBehavior.AllowGet);
            }
            string address = "";
            string province = "";
            string city = "";
            string district = "";
            string street = "";
            string name = string.Empty;
            Region regionByName = new Region();
            if (!string.IsNullOrWhiteSpace(shopId))
            {
                query.ShopId = TypeHelper.ObjectToInt(shopId, 0);
                if (query.ShopId <= 0L)
                {
                    BaseController.Result result = new BaseController.Result
                    {
                        success = false,
                        msg = "无法定位到商家！"
                    };
                    return base.Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                GaodeGetAddressByLatLngResult result2 = ShopbranchHelper.GetAddressByLatLng(query.FromLatLng, ref address, ref province, ref city, ref district, ref street);
                if (string.IsNullOrWhiteSpace(city))
                {
                    BaseController.Result result3 = new BaseController.Result
                    {
                        success = false,
                        msg = "无法定位到城市！"
                    };
                    return base.Json(result3, JsonRequestBehavior.AllowGet);
                }
                regionByName = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
                if (regionByName != null)
                {
                    query.CityId = regionByName.Id;
                }
                if (!string.IsNullOrWhiteSpace(result2.regeocode.addressComponent.building.name))
                {
                    name = result2.regeocode.addressComponent.building.name;
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = result2.regeocode.addressComponent.neighborhood.name;
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    string str7 = province + result2.regeocode.addressComponent.city + district + street;
                    name = result2.regeocode.formatted_address.Remove(0, str7.Length);
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = street;
                }
            }
            QueryPageModel<ShopBranch> nearShopBranchs = ShopBranchApplication.GetNearShopBranchs(query);
            List<HomePageShopBranch> list = this.ProcessBranchHomePageData(nearShopBranchs.Models);
            return base.Json(new { success = true, Data = new { Models = list, Total = nearShopBranchs.Total, CityInfo = new { Id = regionByName.Id, Name = regionByName.Name }, CurrentAddress = name } }, JsonRequestBehavior.AllowGet);
        }

        private List<HomePageShopBranch> ProcessBranchHomePageData(List<ShopBranch> list)
        {
            IEnumerable<long> enumerable = (from e in list select e.ShopId).Distinct<long>();
            List<HomePageShopBranch> source = (from e in list select new HomePageShopBranch { ShopBranch = e }).ToList<HomePageShopBranch>();
            using (IEnumerator<long> enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Func<HomePageShopBranch, bool> predicate = null;
                    long sid = enumerator.Current;
                    ShopActiveList list3 = new ShopActiveList();
                    List<CouponInfo> list4 = (from d in CouponApplication.GetCouponLists(sid)
                                              where d.Himall_CouponSetting.Any<CouponSettingInfo>(c => ((int)c.PlatForm) == 4)
                                              select d).ToList<CouponInfo>();
                    list3.ShopCoupons = (from e in list4 select new CouponModel { Id = e.Id, CouponName = e.CouponName, ShopId = e.ShopId, OrderAmount = e.OrderAmount.ToString(), Price = e.Price, StartTime = e.StartTime, EndTime = e.EndTime }).ToList<CouponModel>();
                    List<ActiveInfo> ongoingActiveByShopId = FullDiscountApplication.GetOngoingActiveByShopId(sid);
                    if (ongoingActiveByShopId != null)
                    {
                        list3.ShopActives = (from e in ongoingActiveByShopId select new ActiveInfo { ActiveName = e.ActiveName, ShopId = e.ShopId }).ToList<ActiveInfo>();
                    }
                    if (predicate == null)
                    {
                        predicate = e => e.ShopBranch.ShopId == sid;
                    }
                    IEnumerable<HomePageShopBranch> enumerable2 = source.Where<HomePageShopBranch>(predicate);
                    foreach (HomePageShopBranch branch in enumerable2)
                    {
                        list3.FreeFreightAmount = branch.ShopBranch.FreeMailFee;
                        branch.ShopAllActives = list3;
                    }
                }
            }
            return source;
        }

        [HttpGet]
        public JsonResult ProductList(int pageSize, int pageNo, string productId, string shopCategoryId, string shopId, string shopBranchId, string keyWords, int type)
        {
            Shop shop;
            decimal memberDiscount;
            List<ProductInfo>.Enumerator enumerator;
            if (!((SiteSettingApplication.GetSiteSettings() != null) && SiteSettingApplication.GetSiteSettings().IsOpenStore))
            {
                throw new HimallException("门店未授权！");
            }
            long shopBrandId = TypeHelper.ObjectToInt(shopBranchId, 0);
            long num = TypeHelper.ObjectToInt(shopId, 0);
            long num2 = TypeHelper.ObjectToInt(productId, 0);
            ShopBranchProductQuery query = new ShopBranchProductQuery
            {
                PageSize = pageSize,
                PageNo = pageNo,
                KeyWords = keyWords,
                RproductId = new long?(num2),
                ShopId = new long?(num),
                shopBranchId = new long?(shopBrandId),
                ShopBranchProductStatus = 0,
                OrderKey = 5
            };
            if (query.ShopId <= 0L)
            {
                return base.Json(new { Success = false, Message = "无法定位到商家！" }, JsonRequestBehavior.AllowGet);
            }
            if (TypeHelper.ObjectToInt(shopCategoryId, 0) > 0)
            {
                query.ShopCategoryId = new long?((long)TypeHelper.ObjectToInt(shopCategoryId));
            }
            long? nullable = query.shopBranchId;
            if ((nullable.GetValueOrDefault() <= 0L) && nullable.HasValue)
            {
                return base.Json(new { Success = false, Message = "无法定位到门店！" }, JsonRequestBehavior.AllowGet);
            }
            DateTime now = DateTime.Now;
            QueryPageModel<ProductInfo> model = ShopBranchApplication.GetShopBranchProductsMonth(query, now.AddDays(-30.0).Date, now);
            ShoppingCartInfo cart = new ShoppingCartInfo();
            if (base.CurrentUser != null)
            {
                cart = new BranchCartHelper().GetCart(base.CurrentUser.Id, shopBrandId);
            }
            if ((model.Models != null) && (model.Models.Count > 0))
            {
                if (base.CurrentUser != null)
                {
                    nullable = query.ShopId;
                    shop = ShopApplication.GetShop(nullable.Value, false);
                    if ((shop != null) && shop.IsSelf)
                    {
                        memberDiscount = 1M;
                        memberDiscount = base.CurrentUser.MemberDiscount;
                        foreach (ProductInfo info2 in model.Models)
                        {
                            info2.MinSalePrice = Math.Round((decimal)(info2.MinSalePrice * memberDiscount), 2);
                        }
                    }
                }
                using (enumerator = model.Models.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Func<ShoppingCartItem, bool> predicate = null;
                        ProductInfo item = enumerator.Current;
                        if (predicate == null)
                        {
                            predicate = delegate(ShoppingCartItem d)
                            {
                                return (d.ProductId == item.Id) && (((nullable = d.ShopBranchId).GetValueOrDefault() == shopBrandId) && nullable.HasValue);
                            };
                        }
                        item.Quantity = new int?((cart != null) ? cart.Items.Where<ShoppingCartItem>(predicate).Sum<ShoppingCartItem>(((Func<ShoppingCartItem, int>)(d => d.Quantity))) : 0);
                    }
                }
            }
            var enumerable = from item in model.Models.ToList<ProductInfo>() select new { Id = item.Id, ProductName = item.ProductName, ProductTypeId = item.Himall_Categories.Id, ProductType = item.Himall_Categories.Name, MeasureUnit = item.MeasureUnit, MarketPrice = item.MarketPrice.ToString("F2"), MinSalePrice = item.MinSalePrice.ToString("f2"), SaleCounts = item.SaleCounts, HasSku = item.HasSKU, Quantity = item.Quantity, RelativePath = HimallIO.GetRomoteProductSizeImage(item.RelativePath, 1, 350) };
            nullable = null;
            query.RproductId = nullable;
            object obj2 = new object();
            if (num2 > 0L)
            {
                query.productId = new long?(num2);
                if (type == 1)
                {
                    query.ShopCategoryId = null;
                }
                QueryPageModel<ProductInfo> model2 = ShopBranchApplication.GetShopBranchProductsMonth(query, now.AddDays(-30.0).Date, now);
                if ((model2.Models != null) && (model2.Models.Count > 0))
                {
                    if (base.CurrentUser != null)
                    {
                        shop = ShopApplication.GetShop(query.ShopId.Value, false);
                        if ((shop != null) && shop.IsSelf)
                        {
                            memberDiscount = 1M;
                            memberDiscount = base.CurrentUser.MemberDiscount;
                            foreach (ProductInfo info2 in model2.Models)
                            {
                                info2.MinSalePrice = Math.Round((decimal)(info2.MinSalePrice * memberDiscount), 2);
                            }
                        }
                    }
                    using (enumerator = model2.Models.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Func<ShoppingCartItem, bool> func2 = null;
                            ProductInfo item = enumerator.Current;
                            if (func2 == null)
                            {
                                func2 = delegate(ShoppingCartItem d)
                                {
                                    return (d.ProductId == item.Id) && (((nullable = d.ShopBranchId).GetValueOrDefault() == shopBrandId) && nullable.HasValue);
                                };
                            }
                            item.Quantity = new int?((cart != null) ? cart.Items.Where<ShoppingCartItem>(func2).Sum<ShoppingCartItem>(((Func<ShoppingCartItem, int>)(d => d.Quantity))) : 0);
                        }
                    }
                }
                obj2 = from item in model2.Models.ToList<ProductInfo>() select new { Id = item.Id, ProductName = item.ProductName, ProductTypeId = item.Himall_Categories.Id, ProductType = item.Himall_Categories.Name, MeasureUnit = item.MeasureUnit, MarketPrice = item.MarketPrice.ToString("F2"), MinSalePrice = item.MinSalePrice.ToString("f2"), SaleCounts = item.SaleCounts, HasSku = item.HasSKU, Quantity = item.Quantity, RelativePath = HimallIO.GetRomoteProductSizeImage(item.RelativePath, 1, 350) };
            }
            return base.Json(new { Success = true, Models = enumerable, TopModels = obj2, Total = model.Total }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ProductSkuInfoById(string id, string shopBranchId)
        {
            if (string.IsNullOrEmpty(id))
                return this.Json((object)new
                {
                    Success = false
                });
            long shopBrandId = (long)TypeHelper.ObjectToInt((object)shopBranchId, 0);
            long id1 = (long)Convert.ToInt32(id);
            ProductInfo product = this._iProductService.GetProduct(id1);
            Decimal num1 = new Decimal(1);
            if (this.CurrentUser != null)
                num1 = this.CurrentUser.MemberDiscount;
            List<ProductSKUModel> list1 = new List<ProductSKUModel>();
            object obj = new object();
            Decimal num2 = new Decimal(0);
            ShoppingCartInfo shoppingCartInfo = new ShoppingCartInfo();
            if (this.CurrentUser != null)
                shoppingCartInfo = new BranchCartHelper().GetCart(this.CurrentUser.Id, shopBrandId);
            foreach (SKUInfo skuInfo in Enumerable.Where<SKUInfo>((IEnumerable<SKUInfo>)product.SKUInfo, (Func<SKUInfo, bool>)(s => s.Stock > 0L)))
            {
                Decimal num3 = skuInfo.SalePrice * num1;
                ProductSKUModel productSkuModel = new ProductSKUModel()
                {
                    Price = skuInfo.SalePrice,
                    SkuId = skuInfo.Id,
                    Stock = (int)skuInfo.Stock
                };
                list1.Add(productSkuModel);
            }
            ProductTypeInfo type = this._iTypeService.GetType(product.TypeId);
            string str1 = type == null || string.IsNullOrEmpty(type.ColorAlias) ? EnumHelper.ToDescription((Enum)SpecificationType.Color) : type.ColorAlias;
            string str2 = type == null || string.IsNullOrEmpty(type.SizeAlias) ? EnumHelper.ToDescription((Enum)SpecificationType.Size) : type.SizeAlias;
            string str3 = type == null || string.IsNullOrEmpty(type.VersionAlias) ? EnumHelper.ToDescription((Enum)SpecificationType.Version) : type.VersionAlias;
            List<object> list2 = new List<object>();
            List<object> list3 = new List<object>();
            if (product.SKUInfo != null && Enumerable.Count<SKUInfo>((IEnumerable<SKUInfo>)product.SKUInfo) > 0)
            {
                long result1 = 0L;
                long result2 = 0L;
                long result3 = 0L;
                List<object> list4 = new List<object>();
                List<string> list5 = new List<string>();
                foreach (SKUInfo skuInfo in (IEnumerable<SKUInfo>)product.SKUInfo)
                {
                    SKUInfo sku = skuInfo;
                    string[] strArray = sku.Id.Split('_');
                    if (Enumerable.Count<string>((IEnumerable<string>)strArray) > 0)
                    {
                        if (!long.TryParse(strArray[1], out result1))
                            ;
                        if (result1 != 0L && !list5.Contains(sku.Color))
                        {
                            Enumerable.Sum<SKUInfo>(Enumerable.Where<SKUInfo>((IEnumerable<SKUInfo>)product.SKUInfo, (Func<SKUInfo, bool>)(s => s.Color.Equals(sku.Color))), (Func<SKUInfo, long>)(s => s.Stock));
                            var fAnonymousType3f = new
                            {
                                ValueId = result1,
                                Value = sku.Color,
                                UseAttributeImage = "False",
                                ImageUrl = HimallIO.GetRomoteImagePath(sku.ShowPic, (string)null)
                            };
                            list5.Add(sku.Color);
                            list4.Add((object)fAnonymousType3f);
                        }
                    }
                }
                var fAnonymousType40_1 = new
                {
                    AttributeId = product.TypeId,
                    AttributeName = str1,
                    AttributeValue = list4
                };
                if (result1 > 0L)
                    list2.Add((object)fAnonymousType40_1);
                List<object> list6 = new List<object>();
                List<string> list7 = new List<string>();
                foreach (SKUInfo skuInfo in (IEnumerable<SKUInfo>)product.SKUInfo)
                {
                    SKUInfo sku = skuInfo;
                    string[] strArray = sku.Id.Split('_');
                    if (Enumerable.Count<string>((IEnumerable<string>)strArray) > 1)
                    {
                        if (!long.TryParse(strArray[2], out result2))
                            ;
                        if (result2 != 0L && !list7.Contains(sku.Size))
                        {
                            Enumerable.Sum<SKUInfo>(Enumerable.Where<SKUInfo>((IEnumerable<SKUInfo>)product.SKUInfo, (Func<SKUInfo, bool>)(s => s.Size.Equals(sku.Size))), (Func<SKUInfo, long>)(s1 => s1.Stock));
                            var fAnonymousType3f = new
                            {
                                ValueId = result2,
                                Value = sku.Size,
                                UseAttributeImage = false,
                                ImageUrl = HimallIO.GetRomoteImagePath(sku.ShowPic, (string)null)
                            };
                            list7.Add(sku.Size);
                            list6.Add((object)fAnonymousType3f);
                        }
                    }
                }
                var fAnonymousType40_2 = new
                {
                    AttributeId = product.TypeId,
                    AttributeName = str2,
                    AttributeValue = list6
                };
                if (result2 > 0L)
                    list2.Add((object)fAnonymousType40_2);
                List<object> list8 = new List<object>();
                List<string> list9 = new List<string>();
                foreach (SKUInfo skuInfo in (IEnumerable<SKUInfo>)product.SKUInfo)
                {
                    SKUInfo sku = skuInfo;
                    string[] strArray = sku.Id.Split('_');
                    if (Enumerable.Count<string>((IEnumerable<string>)strArray) > 2)
                    {
                        if (!long.TryParse(strArray[3], out result3))
                            ;
                        if (result3 != 0L && !list9.Contains(sku.Version))
                        {
                            Enumerable.Where<SKUInfo>((IEnumerable<SKUInfo>)product.SKUInfo, (Func<SKUInfo, bool>)(s => s.Version.Equals(sku.Version)));
                            var fAnonymousType3f = new
                            {
                                ValueId = result3,
                                Value = sku.Version,
                                UseAttributeImage = false,
                                ImageUrl = HimallIO.GetRomoteImagePath(sku.ShowPic, (string)null)
                            };
                            list9.Add(sku.Version);
                            list8.Add((object)fAnonymousType3f);
                        }
                    }
                }
                var fAnonymousType40_3 = new
                {
                    AttributeId = product.TypeId,
                    AttributeName = str3,
                    AttributeValue = list8
                };
                if (result3 > 0L)
                    list2.Add((object)fAnonymousType40_3);
                foreach (SKUInfo skuInfo in (IEnumerable<SKUInfo>)product.SKUInfo)
                {
                    SKUInfo sku = skuInfo;
                    var fAnonymousType41 = new
                    {
                        SkuId = sku.Id,
                        SKU = sku.Sku,
                        Weight = product.Weight,
                        Stock = sku.Stock,
                        WarningStock = sku.SafeStock,
                        SalePrice = sku.SalePrice.ToString("0.##"),
                        CartQuantity = shoppingCartInfo != null ? Enumerable.Sum<ShoppingCartItem>(Enumerable.Where<ShoppingCartItem>(shoppingCartInfo.Items, (Func<ShoppingCartItem, bool>)(d =>
                        {
                            int num3;
                            if (d.SkuId == sku.Id)
                            {
                                long? shopBranchId1 = d.ShopBranchId;
                                long num4 = shopBrandId;
                                num3 = shopBranchId1.GetValueOrDefault() != num4 ? 0 : (shopBranchId1.HasValue ? 1 : 0);
                            }
                            else
                                num3 = 0;
                            return num3 != 0;
                        })), (Func<ShoppingCartItem, int>)(d => d.Quantity)) : 0,
                        ImageUrl = HimallIO.GetRomoteProductSizeImage(sku.ShowPic, 1, 350)
                    };
                    list3.Add((object)fAnonymousType41);
                }
                obj = list3[0];
            }
            return this.Json((object)new
            {
                Success = true,
                Data = new
                {
                    ProductId = id1,
                    ProductName = product.ProductName,
                    ImageUrl = HimallIO.GetRomoteProductSizeImage(product.ImagePath, 1, 350),
                    Stock = Enumerable.Sum<ProductSKUModel>((IEnumerable<ProductSKUModel>)list1, (Func<ProductSKUModel, int>)(s => s.Stock)),
                    SkuItems = list2,
                    Skus = list3,
                    DefaultSku = obj
                }
            });
        }

        public ActionResult ProductStoreList(int? id, long? shopId)
        {
            if (!((SiteSettingApplication.GetSiteSettings() != null) && SiteSettingApplication.GetSiteSettings().IsOpenStore))
            {
                throw new HimallException("门店未授权！");
            }
            ((dynamic)base.ViewBag).QQMapKey = "SYJBZ-DSLR3-IWX3Q-3XNTM-ELURH-23FTP";
            if (!(id.HasValue && shopId.HasValue))
            {
                throw new HimallException("非法参数！");
            }
            ((dynamic)base.ViewBag).ShopID = shopId.Value;
            return base.View(id.Value);
        }

        [HttpGet]
        public JsonResult Search(int pageSize, int pageNo, string fromLatLng, string shopId, string keyWords)
        {
            if (!((SiteSettingApplication.GetSiteSettings() != null) && SiteSettingApplication.GetSiteSettings().IsOpenStore))
            {
                throw new HimallException("门店未授权！");
            }
            ShopBranchQuery search = new ShopBranchQuery
            {
                PageNo = pageNo,
                PageSize = pageSize,
                Status = 0,
                CityId = -1,
                FromLatLng = fromLatLng,
                ShopBranchName = keyWords.Trim(),
                OrderKey = 2,
                OrderType = true
            };
            if (search.FromLatLng.Split(new char[] { ',' }).Length != 2)
            {
                BaseController.Result data = new BaseController.Result
                {
                    success = false,
                    msg = "无法获取您的当前位置，请确认是否开启定位服务！"
                };
                return base.Json(data, JsonRequestBehavior.AllowGet);
            }
            string address = "";
            string province = "";
            string city = "";
            string district = "";
            string street = "";
            string name = string.Empty;
            Region regionByName = new Region();
            if (!string.IsNullOrWhiteSpace(shopId))
            {
                search.ShopId = TypeHelper.ObjectToInt(shopId, 0);
                if (search.ShopId <= 0L)
                {
                    BaseController.Result result = new BaseController.Result
                    {
                        success = false,
                        msg = "无法定位到商家！"
                    };
                    return base.Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                GaodeGetAddressByLatLngResult result2 = ShopbranchHelper.GetAddressByLatLng(search.FromLatLng, ref address, ref province, ref city, ref district, ref street);
                if (string.IsNullOrWhiteSpace(city))
                {
                    BaseController.Result result3 = new BaseController.Result
                    {
                        success = false,
                        msg = "无法定位到城市！"
                    };
                    return base.Json(result3, JsonRequestBehavior.AllowGet);
                }
                regionByName = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
                if (regionByName != null)
                {
                    search.CityId = regionByName.Id;
                }
                if (!string.IsNullOrWhiteSpace(result2.regeocode.addressComponent.building.name))
                {
                    name = result2.regeocode.addressComponent.building.name;
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = result2.regeocode.addressComponent.neighborhood.name;
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    string str7 = province + result2.regeocode.addressComponent.city + district + street;
                    name = result2.regeocode.formatted_address.Remove(0, str7.Length);
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = street;
                }
            }
            QueryPageModel<ShopBranch> model = ShopBranchApplication.SearchNearShopBranchs(search);
            List<HomePageShopBranch> list = this.ProcessBranchHomePageData(model.Models);
            return base.Json(new { success = true, Data = new { Models = list, Total = model.Total, CityInfo = new { Id = regionByName.Id, Name = regionByName.Name }, CurrentAddress = name } }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchList(int? id, string keywords)
        {
            if (!((SiteSettingApplication.GetSiteSettings() != null) && SiteSettingApplication.GetSiteSettings().IsOpenStore))
            {
                throw new HimallException("门店未授权！");
            }
            ((dynamic)base.ViewBag).QQMapKey = "SYJBZ-DSLR3-IWX3Q-3XNTM-ELURH-23FTP";
            return base.View(keywords);
        }

        public JsonResult SearchShopBranchProductAndSaleCount(string ids, string keyWords)
        {
            if (string.IsNullOrEmpty(ids))
            {
                throw new HimallException("非法参数");
            }
            ShoppingCartInfo memberCartInfo = new ShoppingCartInfo();
            if (base.CurrentUser != null)
            {
                memberCartInfo = CartApplication.GetShopBranchCart(base.CurrentUser.Id, 0L);
            }
            var enumerable = ids.Split(new char[] { ',' }).Select(delegate(string e)
            {
                ShopBranchProductQuery query = new ShopBranchProductQuery
                {
                    PageSize = 0x7fffffff,
                    PageNo = 1,
                    shopBranchId = new long?(long.Parse(e)),
                    KeyWords = keyWords,
                    ShopBranchProductStatus = 0
                };
                QueryPageModel<ProductInfo> shopBranchProducts = ShopBranchApplication.GetShopBranchProducts(query);
                DateTime endDate = DateTime.Now;
                long num = ShopBranchApplication.GetShopBranchSaleCount(query.shopBranchId.Value, endDate.AddDays(-30.0).Date, endDate);
                int num2 = memberCartInfo.Items.Where<ShoppingCartItem>(delegate(ShoppingCartItem c)
                {
                    long? nullable;
                    return (c.ShopBranchId.HasValue && ((c.ShopBranchId.Value == (nullable = query.shopBranchId).GetValueOrDefault()) && nullable.HasValue));
                }).Sum<ShoppingCartItem>((Func<ShoppingCartItem, int>)(c => c.Quantity));
                return new
                {
                    saleCount = shopBranchProducts.Models.Sum<ProductInfo>((Func<ProductInfo, long>)(p => p.SaleCounts)),
                    saleCountByMonth = num,
                    cartQuantity = num2,
                    products = from p in shopBranchProducts.Models
                               orderby p.SaleCounts descending, p.Id descending
                               select new ProductInfo { Id = p.Id, ImagePath = p.ImagePath, MinSalePrice = p.MinSalePrice, ProductName = p.ProductName },
                    branchId = e
                };
            });
            return base.Json(new { success = true, Data = enumerable }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult StoreList()
        {
            if (!((SiteSettingApplication.GetSiteSettings() != null) && SiteSettingApplication.GetSiteSettings().IsOpenStore))
            {
                throw new HimallException("门店未授权！");
            }
            ((dynamic)base.ViewBag).QQMapKey = "SYJBZ-DSLR3-IWX3Q-3XNTM-ELURH-23FTP";
            ShopBranchListHomePage model = new ShopBranchListHomePage
            {
                SlideAdModels = SlideApplication.GetShopBranchListSlide()
            };
            return base.View(model);
        }

        public ActionResult StoreListAddress()
        {
            ((dynamic)base.ViewBag).QQMapKey = "SYJBZ-DSLR3-IWX3Q-3XNTM-ELURH-23FTP";
            return base.View();
        }

        [HttpGet]
        public JsonResult StoresByProduct(int pageSize, int pageNo, string fromLatLng, string shopId, long productId)
        {
            if (!((SiteSettingApplication.GetSiteSettings() != null) && SiteSettingApplication.GetSiteSettings().IsOpenStore))
            {
                throw new HimallException("门店未授权！");
            }
            ShopBranchQuery search = new ShopBranchQuery
            {
                PageNo = pageNo,
                PageSize = pageSize,
                Status = 0,
                ProductIds = new long[] { productId },
                CityId = -1,
                FromLatLng = fromLatLng,
                OrderKey = 2,
                OrderType = true
            };
            if (search.FromLatLng.Split(new char[] { ',' }).Length != 2)
            {
                BaseController.Result data = new BaseController.Result
                {
                    success = false,
                    msg = "无法获取您的当前位置，请确认是否开启定位服务！"
                };
                return base.Json(data, JsonRequestBehavior.AllowGet);
            }
            string address = "";
            string province = "";
            string city = "";
            string district = "";
            string street = "";
            string name = string.Empty;
            Region regionByName = new Region();
            if (!string.IsNullOrWhiteSpace(shopId))
            {
                search.ShopId = TypeHelper.ObjectToInt(shopId, 0);
                if (search.ShopId <= 0L)
                {
                    BaseController.Result result = new BaseController.Result
                    {
                        success = false,
                        msg = "无法定位到商家！"
                    };
                    return base.Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                GaodeGetAddressByLatLngResult result2 = ShopbranchHelper.GetAddressByLatLng(search.FromLatLng, ref address, ref province, ref city, ref district, ref street);
                if (string.IsNullOrWhiteSpace(city))
                {
                    BaseController.Result result3 = new BaseController.Result
                    {
                        success = false,
                        msg = "无法定位到城市！"
                    };
                    return base.Json(result3, JsonRequestBehavior.AllowGet);
                }
                regionByName = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
                if (regionByName != null)
                {
                    search.CityId = regionByName.Id;
                }
                if (!string.IsNullOrWhiteSpace(result2.regeocode.addressComponent.building.name))
                {
                    name = result2.regeocode.addressComponent.building.name;
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = result2.regeocode.addressComponent.neighborhood.name;
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    string str7 = province + result2.regeocode.addressComponent.city + district + street;
                    name = result2.regeocode.formatted_address.Remove(0, str7.Length);
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = street;
                }
            }
            QueryPageModel<ShopBranch> model = ShopBranchApplication.StoreByProductNearShopBranchs(search);
            List<HomePageShopBranch> list = this.ProcessBranchHomePageData(model.Models);
            return base.Json(new { success = true, Data = new { Models = list, Total = model.Total, CityInfo = new { Id = regionByName.Id, Name = regionByName.Name }, CurrentAddress = name } }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Tags(int? id)
        {
            if (!((SiteSettingApplication.GetSiteSettings() != null) && SiteSettingApplication.GetSiteSettings().IsOpenStore))
            {
                throw new HimallException("门店未授权！");
            }
            ((dynamic)base.ViewBag).QQMapKey = "SYJBZ-DSLR3-IWX3Q-3XNTM-ELURH-23FTP";
            if (!id.HasValue)
            {
                throw new HimallException("非法参数！");
            }
            ShopBranchTagModel shopBranchTagInfo = ShopBranchApplication.GetShopBranchTagInfo((long)id.Value);
            if (null == shopBranchTagInfo)
            {
                throw new HimallException("门店标签已经被管理员删除 ！");
            }
            return base.View(shopBranchTagInfo);
        }

        [HttpGet]
        public JsonResult TagsSearch(int pageSize, int pageNo, string fromLatLng, string shopId, long tagsId)
        {
            if (!((SiteSettingApplication.GetSiteSettings() != null) && SiteSettingApplication.GetSiteSettings().IsOpenStore))
            {
                throw new HimallException("门店未授权！");
            }
            ShopBranchQuery search = new ShopBranchQuery
            {
                PageNo = pageNo,
                PageSize = pageSize,
                Status = 0,
                ShopBranchTagId = new long?(tagsId),
                CityId = -1,
                FromLatLng = fromLatLng,
                OrderKey = 2,
                OrderType = true
            };
            if (search.FromLatLng.Split(new char[] { ',' }).Length != 2)
            {
                BaseController.Result data = new BaseController.Result
                {
                    success = false,
                    msg = "无法获取您的当前位置，请确认是否开启定位服务！"
                };
                return base.Json(data, JsonRequestBehavior.AllowGet);
            }
            string address = "";
            string province = "";
            string city = "";
            string district = "";
            string street = "";
            string name = string.Empty;
            Region regionByName = new Region();
            if (!string.IsNullOrWhiteSpace(shopId))
            {
                search.ShopId = TypeHelper.ObjectToInt(shopId, 0);
                if (search.ShopId <= 0L)
                {
                    BaseController.Result result = new BaseController.Result
                    {
                        success = false,
                        msg = "无法定位到商家！"
                    };
                    return base.Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                GaodeGetAddressByLatLngResult result2 = ShopbranchHelper.GetAddressByLatLng(search.FromLatLng, ref address, ref province, ref city, ref district, ref street);
                if (string.IsNullOrWhiteSpace(city))
                {
                    BaseController.Result result3 = new BaseController.Result
                    {
                        success = false,
                        msg = "无法定位到城市！"
                    };
                    return base.Json(result3, JsonRequestBehavior.AllowGet);
                }
                regionByName = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
                if (regionByName != null)
                {
                    search.CityId = regionByName.Id;
                }
                if (!string.IsNullOrWhiteSpace(result2.regeocode.addressComponent.building.name))
                {
                    name = result2.regeocode.addressComponent.building.name;
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = result2.regeocode.addressComponent.neighborhood.name;
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    string str7 = province + result2.regeocode.addressComponent.city + district + street;
                    name = result2.regeocode.formatted_address.Remove(0, str7.Length);
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = street;
                }
            }
            QueryPageModel<ShopBranch> model = ShopBranchApplication.TagsSearchNearShopBranchs(search);
            List<HomePageShopBranch> list = this.ProcessBranchHomePageData(model.Models);
            return base.Json(new { success = true, Data = new { Models = list, Total = model.Total, CityInfo = new { Id = regionByName.Id, Name = regionByName.Name }, CurrentAddress = name } }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateCartItem(string skuId, string shopBranchId, string count)
        {
            BranchCartHelper helper = new BranchCartHelper();
            long memberId = (base.CurrentUser != null) ? base.CurrentUser.Id : 0L;
            int num2 = Convert.ToInt32(count);
            long num3 = Convert.ToInt64(shopBranchId);
            List<string> skuIds = new List<string> {
            skuId
        };
            List<ShopBranchSkusInfo> skusByIds = this._iShopBranchService.GetSkusByIds(num3, skuIds);
            long num4 = skusByIds.Sum<ShopBranchSkusInfo>((Func<ShopBranchSkusInfo, int>)(b => b.Stock));
            if ((skusByIds != null) && (num4 < num2))
            {
                return base.Json(new { Success = false, msg = "超出库存或限购量", stock = num4 });
            }
            helper.UpdateCartItem(skuId, num2, memberId, num3);
            return base.Json(new { Success = true });
        }
    }
}