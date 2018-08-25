using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.DTO;
using Himall.Model;
using Himall.Web.Areas.Mobile.Models;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Common;
using Himall.Web.Framework;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class BranchOrderController : BaseMobileMemberController
    {// Methods
        [HttpPost]
        public JsonResult CalcFreight(int addressId, CalcFreightparameter[] parameters)
        {
            Dictionary<long, decimal> dictionary = OrderApplication.CalcFreight(addressId, (from p in parameters group p by p.ShopId).ToDictionary<IGrouping<long, CalcFreightparameter>, long, Dictionary<long, int>>(p => p.Key, p => (from pp in p group pp by pp.ProductId).ToDictionary<IGrouping<long, CalcFreightparameter>, long, int>(pp => pp.Key, pp => pp.Sum<CalcFreightparameter>((Func<CalcFreightparameter, int>)(ppp => ppp.Count)))));
            if (dictionary.Count == 0)
            {
                return base.Json(new { success = false, msg = "计算运费失败" });
            }
            return base.Json(new { success = true, freights = (from p in dictionary select new { shopId = p.Key, freight = p.Value }).ToArray() });
        }

        [HttpPost]
        public JsonResult CancelOrders(string orderIds)
        {
            try
            {
                OrderApplication.CancelOrder(orderIds, base.UserId);
                return base.Json(new { success = true });
            }
            catch (Exception exception)
            {
                return base.Json(new { success = false, msg = exception.Message });
            }
        }

        public ActionResult ChooseShippingAddress(string returnURL = "", long shopBranchId = 0L)
        {
            if (shopBranchId == 0L)
            {
                throw new HimallException("获取门店ID失败，不可提交非门店商品");
            }
            ((dynamic)base.ViewBag).shopBranchId = shopBranchId;
            return base.View(OrderApplication.GetUserAddresses(base.UserId, shopBranchId));
        }

        [HttpPost]
        public JsonResult CloseOrder(long orderId)
        {
            UserMemberInfo currentUser = base.CurrentUser;
            if (OrderApplication.CloseOrder(orderId, currentUser.Id, currentUser.UserName))
            {
                BaseController.Result result = new BaseController.Result
                {
                    success = true,
                    msg = "取消成功"
                };
                return base.Json(result);
            }
            BaseController.Result data = new BaseController.Result
            {
                success = false,
                msg = "取消失败，该订单已删除或者不属于当前用户！"
            };
            return base.Json(data);
        }

        [HttpPost]
        public JsonResult ConfirmOrder(long orderId)
        {
            int num = OrderApplication.ConfirmOrder(orderId, base.CurrentUser.Id, base.CurrentUser.UserName);
            BaseController.Result data = new BaseController.Result
            {
                status = num
            };
            switch (num)
            {
                case 0:
                    data.success = true;
                    data.msg = "操作成功";
                    break;

                case 1:
                    data.success = false;
                    data.msg = "该订单已经确认过!";
                    break;

                case 2:
                    data.success = false;
                    data.msg = "订单状态发生改变，请重新刷页面操作!";
                    break;
            }
            return base.Json(data);
        }

        [HttpPost]
        public JsonResult DeleteInvoiceTitle(long id)
        {
            OrderApplication.DeleteInvoiceTitle(id, 0L);
            return base.Json(true);
        }

        [HttpPost]
        public ActionResult DeleteShippingAddress(long addressId)
        {
            OrderApplication.DeleteShippingAddress(addressId, base.UserId);
            return base.Json(new { success = true });
        }

        public ActionResult Detail(long id)
        {
            long? nullable;
            OrderDetailView view = OrderApplication.Detail(id, base.UserId, base.PlatformType, base.Request.Url.Host);
            ((dynamic)base.ViewBag).Detail = view.Detail;
            ((dynamic)base.ViewBag).Bonus = view.Bonus;
            ((dynamic)base.ViewBag).ShareHref = view.ShareHref;
            ((dynamic)base.ViewBag).IsRefundTimeOut = view.IsRefundTimeOut;
            ((dynamic)base.ViewBag).Logo = base.CurrentSiteSetting.Logo;
            view.Order.FightGroupOrderJoinStatus = new FightGroupOrderJoinStatus?(view.FightGroupJoinStatus);
            view.Order.FightGroupCanRefund = new bool?(view.FightGroupCanRefund);
            List<CustomerService> mobileCustomerService = CustomerServiceApplication.GetMobileCustomerService(view.Order.ShopId);
            CustomerService item = CustomerServiceApplication.GetPreSaleByShopId(view.Order.ShopId).FirstOrDefault<CustomerService>(p => p.Tool == CustomerServiceInfo.ServiceTool.MeiQia);
            if (item != null)
            {
                mobileCustomerService.Insert(0, item);
            }
            ((dynamic)base.ViewBag).CustomerServices = mobileCustomerService;
            if (view.Order.ShopBranchId.HasValue && (((nullable = view.Order.ShopBranchId).GetValueOrDefault() > 0L) && nullable.HasValue))
            {
                ((dynamic)base.ViewBag).ShopBranchInfo = ShopBranchApplication.GetShopBranchById(view.Order.ShopBranchId.Value);
            }
            return base.View(view.Order);
        }

        public ActionResult EditShippingAddress(long addressId = 0L, string returnURL = "", long shopBranchId = 0L)
        {
            if (shopBranchId == 0L)
            {
                throw new HimallException("获取门店ID失败，不可提交非门店商品");
            }
            ((dynamic)base.ViewBag).shopBranchId = shopBranchId;
            ShippingAddressInfo userAddress = OrderApplication.GetUserAddress(addressId);
            ((dynamic)base.ViewBag).addId = addressId;
            if (userAddress != null)
            {
                ((dynamic)base.ViewBag).fullPath = RegionApplication.GetRegionPath(userAddress.RegionId);
                ((dynamic)base.ViewBag).fullName = RegionApplication.GetFullName(userAddress.RegionId, " ");
            }
            return base.View(userAddress);
        }

        public JsonResult ExistShopBranch(int shopId, int regionId, long[] productIds)
        {
            ShopBranchQuery query = new ShopBranchQuery
            {
                Status = 0,
                ShopId = shopId
            };
            query.AddressPath = RegionApplication.GetRegion((long)regionId, Region.RegionLevel.City).GetIdPath(",");
            query.ProductIds = productIds;
            bool data = ShopBranchApplication.Exists(query);
            return base.Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExpressInfo(long orderId)
        {
            string[] expressInfo = OrderApplication.GetExpressInfo(orderId, base.UserId);
            ((dynamic)base.ViewBag).ExpressCompanyName = expressInfo[0];
            ((dynamic)base.ViewBag).ShipOrderNumber = expressInfo[1];
            return base.View();
        }

        [HttpPost]
        public JsonResult GetOrderPayStatus(string orderids)
        {
            bool flag = OrderApplication.AllOrderIsPaied(orderids);
            return base.Json(new { success = flag });
        }

        [HttpPost]
        public JsonResult GetShopBranchs(long shopId, long regionId, bool getParent, string[] skuIds, int[] counts, int page, int rows, long shippingAddressId)
        {
            ShippingAddressInfo userShippingAddress = ShippingAddressApplication.GetUserShippingAddress(shippingAddressId);
            int streetId = 0;
            int districtId = 0;
            ShopBranchQuery query = new ShopBranchQuery
            {
                ShopId = shopId,
                PageNo = page,
                PageSize = rows,
                Status = 0
            };
            if (userShippingAddress != null)
            {
                query.FromLatLng = string.Format("{0},{1}", userShippingAddress.Latitude, userShippingAddress.Longitude);
                streetId = userShippingAddress.RegionId;
                Region region = RegionApplication.GetRegion((long)userShippingAddress.RegionId, Region.RegionLevel.Town);
                if ((region != null) && (region.ParentId > 0))
                {
                    districtId = region.ParentId;
                }
                else
                {
                    districtId = streetId;
                    streetId = 0;
                }
            }
            bool hasLatLng = false;
            if (!string.IsNullOrWhiteSpace(query.FromLatLng))
            {
                hasLatLng = query.FromLatLng.Split(new char[] { ',' }).Length == 2;
            }
            Region region2 = RegionApplication.GetRegion(regionId, getParent ? Region.RegionLevel.City : Region.RegionLevel.County);
            if (region2 != null)
            {
                query.AddressPath = region2.GetIdPath(",");
            }
            List<SKU> skuInfos = ProductManagerApplication.GetSKUs(skuIds);
            query.ProductIds = (from p in skuInfos select p.ProductId).ToArray<long>();
            QueryPageModel<ShopBranch> shopBranchsAll = ShopBranchApplication.GetShopBranchsAll(query);
            List<ShopBranchSkusInfo> shopBranchSkus = ShopBranchApplication.GetSkus(shopId, from p in shopBranchsAll.Models select p.Id);
            shopBranchsAll.Models.ForEach(delegate(ShopBranch p)
            {
                p.Enabled = skuInfos.All<SKU>(skuInfo => shopBranchSkus.Any<ShopBranchSkusInfo>(sbSku => ((sbSku.ShopBranchId == p.Id) && (sbSku.Stock >= counts[skuInfos.IndexOf(skuInfo)])) && (sbSku.SkuId == skuInfo.Id)));
            });
            List<ShopBranch> source = new List<ShopBranch>();
            List<long> fillterIds = new List<long>();
            List<ShopBranch> list2 = (from p in shopBranchsAll.Models
                                      where (hasLatLng && p.Enabled) && ((p.Latitude > 0f) && (p.Longitude > 0f))
                                      orderby p.Distance
                                      select p).ToList<ShopBranch>();
            if ((list2 != null) && (list2.Count<ShopBranch>() > 0))
            {
                fillterIds.AddRange(from p in list2 select p.Id);
                source.AddRange(list2);
            }
            List<ShopBranch> list3 = (from p in shopBranchsAll.Models
                                      where (!fillterIds.Contains(p.Id) && p.Enabled) && p.AddressPath.Contains("," + streetId + ",")
                                      select p).ToList<ShopBranch>();
            if ((list3 != null) && (list3.Count<ShopBranch>() > 0))
            {
                fillterIds.AddRange(from p in list3 select p.Id);
                source.AddRange(list3);
            }
            List<ShopBranch> list4 = (from p in shopBranchsAll.Models
                                      where (!fillterIds.Contains(p.Id) && p.Enabled) && p.AddressPath.Contains("," + districtId + ",")
                                      select p).ToList<ShopBranch>();
            if ((list4 != null) && (list4.Count<ShopBranch>() > 0))
            {
                fillterIds.AddRange(from p in list4 select p.Id);
                source.AddRange(list4);
            }
            List<ShopBranch> list5 = (from p in shopBranchsAll.Models
                                      where !fillterIds.Contains(p.Id) && p.Enabled
                                      select p).ToList<ShopBranch>();
            if ((list5 != null) && (list5.Count<ShopBranch>() > 0))
            {
                fillterIds.AddRange(from p in list5 select p.Id);
                source.AddRange(list5);
            }
            List<ShopBranch> list6 = (from p in shopBranchsAll.Models
                                      where !fillterIds.Contains(p.Id)
                                      select p).ToList<ShopBranch>();
            if ((list6 != null) && (list6.Count<ShopBranch>() > 0))
            {
                source.AddRange(list6);
            }
            if (source.Count<ShopBranch>() != shopBranchsAll.Models.Count<ShopBranch>())
            {
                return base.Json(new { Rows = "" }, true);
            }
            var data = new
            {
                Rows = (from sb in source select new { ContactUser = sb.ContactUser, ContactPhone = sb.ContactPhone, AddressDetail = sb.AddressDetail, ShopBranchName = sb.ShopBranchName, Id = sb.Id, Enabled = sb.Enabled }).ToArray(),
                Total = source.Count
            };
            return base.Json(data, true);
        }

        [HttpPost]
        public JsonResult GetUserShippingAddresses(long addressId)
        {
            ShippingAddressInfo userAddress = OrderApplication.GetUserAddress(addressId);
            var data = new
            {
                id = userAddress.Id,
                fullRegionName = userAddress.RegionFullName,
                address = userAddress.Address,
                phone = userAddress.Phone,
                shipTo = userAddress.ShipTo,
                fullRegionIdPath = userAddress.RegionIdPath
            };
            return base.Json(data);
        }

        public ActionResult Index()
        {
            return base.View();
        }

        private void InitOrderSubmitModel(MobileOrderDetailConfirmModel model)
        {
            if (model.Address != null)
            {
                ShopBranchQuery query = new ShopBranchQuery
                {
                    Status = 0
                };
                query.AddressPath = RegionApplication.GetRegion((long)model.Address.RegionId, Region.RegionLevel.City).GetIdPath(",");
                foreach (MobileShopCartItemModel model2 in model.products)
                {
                    query.ShopId = model2.shopId;
                    query.ProductIds = (from p in model2.CartItemModels select p.id).ToArray<long>();
                    model2.ExistShopBranch = ShopBranchApplication.Exists(query);
                }
            }
        }

        [HttpGet]
        public ActionResult InitRegion(string fromLatLng)
        {
            string address = string.Empty;
            string province = string.Empty;
            string city = string.Empty;
            string district = string.Empty;
            string street = string.Empty;
            string newStreet = string.Empty;
            ShopbranchHelper.GetAddressByLatLng(fromLatLng, ref address, ref province, ref city, ref district, ref street);
            if ((district == "") && (street != ""))
            {
                district = street;
                street = "";
            }
            string str7 = RegionApplication.GetAddress_Components(city, district, street, out newStreet);
            if (str7.Split(new char[] { ',' }).Length <= 3)
            {
                newStreet = string.Empty;
            }
            return base.Json(new { fullPath = str7, showCity = string.Format("{0} {1} {2}", province, city, district), street = newStreet }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult IsAllDeductible(int integral, decimal total)
        {
            return base.Json(OrderApplication.IsAllDeductible(integral, total, base.UserId));
        }

        private bool IsOutMaxBuyCount(IEnumerable<ProductInfo> products, Dictionary<long, int> buyCounts)
        {
            Dictionary<long, int> buyedCounts = OrderApplication.GetProductBuyCount(base.CurrentUser.Id, from pp in products select pp.Id);
            return products.Any<ProductInfo>(pp => ((pp.MaxBuyCount > 0) && (pp.MaxBuyCount < ((buyedCounts.ContainsKey(pp.Id) ? buyedCounts[pp.Id] : 0) + buyCounts[pp.Id]))));
        }

        public ActionResult IsSubmited(string orderTag)
        {
            return base.Json(!object.Equals(base.Session["OrderTag"], orderTag), JsonRequestBehavior.AllowGet);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if ((base.CurrentUser != null) && base.CurrentUser.Disabled)
            {
                filterContext.Result = base.RedirectToAction("Entrance", "Login", new { returnUrl = base.Request.Url.AbsoluteUri });
            }
        }

        public ActionResult OrderShare(string orderids, string source)
        {
            if (string.IsNullOrWhiteSpace(orderids))
            {
                throw new HimallException("订单号不能为空！");
            }
            long orderId = 0L;
            IEnumerable<long> enumerable = orderids.Split(new char[] { ',' }).Select<string, long>(delegate(string e)
            {
                if (long.TryParse(e, out orderId))
                {
                    return orderId;
                }
                return 0L;
            });
            if (MemberIntegralApplication.OrderIsShared(enumerable))
            {
                ((dynamic)base.ViewBag).IsShared = true;
            }
            ((dynamic)base.ViewBag).Source = source;
            ((dynamic)base.ViewBag).OrderIds = orderids;
            List<OrderDetailView> orderDetailViews = OrderApplication.GetOrderDetailViews(enumerable);
            return base.View(orderDetailViews);
        }

        [HttpPost]
        public JsonResult OrderShareAddIntegral(string orderids)
        {
            if (string.IsNullOrWhiteSpace(orderids))
            {
                throw new HimallException("订单号不能为空！");
            }
            long orderId = 0L;
            IEnumerable<long> enumerable = orderids.Split(new char[] { ',' }).Select<string, long>(delegate(string e)
            {
                if (!long.TryParse(e, out orderId))
                {
                    throw new HimallException("订单分享增加积分时，订单号异常！");
                }
                return orderId;
            });
            if (MemberIntegralApplication.OrderIsShared(enumerable))
            {
                throw new HimallException("订单已经分享过！");
            }
            MemberIntegralRecord model = new MemberIntegralRecord
            {
                MemberId = base.CurrentUser.Id,
                UserName = base.CurrentUser.UserName,
                RecordDate = new DateTime?(DateTime.Now),
                TypeId = MemberIntegral.IntegralType.Share,
                ReMark = string.Format("订单号:{0}", orderids)
            };
            List<MemberIntegralRecordAction> list = new List<MemberIntegralRecordAction>();
            foreach (long num in enumerable)
            {
                MemberIntegralRecordAction item = new MemberIntegralRecordAction
                {
                    VirtualItemId = num,
                    VirtualItemTypeId = MemberIntegral.VirtualItemType.ShareOrder
                };
                list.Add(item);
            }
            model.Himall_MemberIntegralRecordAction = list;
            MemberIntegralApplication.AddMemberIntegralByEnum(model, MemberIntegral.IntegralType.Share);
            return base.Json(new { success = true, msg = "晒单添加积分成功！" });
        }

        [HttpPost]
        public JsonResult PayOrderByIntegral(string orderIds)
        {
            try
            {
                OrderApplication.PayOrderByIntegral(orderIds, base.UserId);
                return base.Json(new { success = true });
            }
            catch (Exception exception)
            {
                return base.Json(new { success = false, msg = exception.Message });
            }
        }

        [HttpPost]
        public JsonResult SaveInvoiceTitle(string name, string code)
        {
            return base.Json(OrderApplication.SaveInvoiceTitle(base.UserId, name, code));
        }

        [HttpPost]
        public JsonResult SetDefaultUserShippingAddress(long addId)
        {
            OrderApplication.SetDefaultUserShippingAddress(addId, base.UserId);
            return base.Json(new { success = true, addId = addId });
        }

        public ActionResult ShopBranchs(int shopId, int regionId, string[] skuIds, int[] counts, long shippingAddressId)
        {
            ((dynamic)base.ViewBag).ShippingAddressId = shippingAddressId;
            ShopBranchModel model = new ShopBranchModel
            {
                ShopId = shopId,
                RegionId = regionId,
                SkuIds = skuIds,
                Counts = counts
            };
            return base.View(model);
        }

        public ActionResult Submit(string skuIds, string counts, int islimit = 0)
        {
            throw new HimallException("门店订单不支持立即购买");
        }

        public ActionResult SubmiteByCart(string cartItemIds)
        {
            MobileOrderDetailConfirmModel mobileSubmiteByCart = OrderApplication.GetMobileSubmiteByCart(base.UserId, cartItemIds);
            ((dynamic)base.ViewBag).InvoiceContext = mobileSubmiteByCart.InvoiceContext;
            ((dynamic)base.ViewBag).InvoiceTitle = mobileSubmiteByCart.InvoiceTitle;
            ((dynamic)base.ViewBag).IsCashOnDelivery = mobileSubmiteByCart.IsCashOnDelivery;
            ((dynamic)base.ViewBag).address = mobileSubmiteByCart.Address;
            ((dynamic)base.ViewBag).ConfirmModel = mobileSubmiteByCart;
            if (mobileSubmiteByCart.shopBranchInfo == null)
            {
                throw new HimallException("获取门店信息失败，不可提交非门店商品");
            }
            string str = Guid.NewGuid().ToString("N");
            ((dynamic)base.ViewBag).OrderTag = str;
            base.Session["OrderTag"] = str;
            this.InitOrderSubmitModel(mobileSubmiteByCart);
            ((dynamic)base.ViewBag).IsOpenStore = (SiteSettingApplication.GetSiteSettings() != null) && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            bool flag = false;
            if (mobileSubmiteByCart.products != null)
            {
                foreach (MobileShopCartItemModel model2 in mobileSubmiteByCart.products)
                {
                    if (model2.shopId > 0L)
                    {
                        ShopInfo shop = ShopApplication.GetShop(model2.shopId, false);
                        if (!(!shop.ProvideInvoice.HasValue ? true : !shop.ProvideInvoice.Value))
                        {
                            flag = true;
                        }
                    } 
                }
            }
            ((dynamic)base.ViewBag).ProvideInvoice = flag;
            return base.View("submit");
        }

        [HttpPost]
        public JsonResult SubmitOrder(OrderPostModel model, string payPwd)
        {
            model.CurrentUser = base.CurrentUser;
            model.DistributionUserLinkId = base.GetDistributionUserLinkId();
            model.PlatformType = base.PlatformType.GetHashCode();
            model.IsShopbranchOrder = true;
            OrderReturnModel model2 = OrderApplication.SubmitOrder(model, payPwd);
            base.ClearDistributionUserLinkId();
            OrderApplication.AddVshopBuyNumber(model2.OrderIds);
            base.Session.Remove("OrderTag");
            return base.Json(new { success = model2.Success, orderIds = model2.OrderIds, realTotalIsZero = model2.OrderTotal == 0M });
        }

    }
}