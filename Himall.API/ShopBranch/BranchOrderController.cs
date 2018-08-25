using Himall.API.Helper;
using Himall.API.Model.ParamsModel;
using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Helper;
using Himall.DTO;
using Himall.IServices;
using Himall.Model;
using Himall.Service;
using Himall.ServiceProvider;
using Himall.Web.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http;

namespace Himall.API
{
    public class BranchOrderController : BaseApiController
    {
        private IOrderService _iOrderService;
        private IMemberIntegralService _iMemberIntegralService;
        private ICartService _iCartService;
        private IMemberService _iMemberService;
        private IProductService _iProductService;
        private IPaymentConfigService _iPaymentConfigService;
        private IShippingAddressService _iShippingAddressService;
        private IRegionService _iRegionService;
        private ICashDepositsService _iCashDepositsService;
        private ISiteSettingService _iSiteSettingService;
        private IShopService _iShopService;
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private ICouponService _iCouponService;
        private IShopBonusService _iShopBonusService;
        private ICollocationService _iCollocationService;
        private IMemberCapitalService _iMemberCapitalService;
        private IVShopService _iVShopService;
        private IRefundService _iRefundService;
        private IFightGroupService _iFightGroupService;

        public BranchOrderController()
        {
            this._iOrderService = (IOrderService)new OrderService();
            this._iCartService = (ICartService)new CartService();
            this._iMemberService = (IMemberService)new MemberService();
            this._iProductService = (IProductService)new ProductService();
            this._iPaymentConfigService = (IPaymentConfigService)new PaymentConfigService();
            this._iCashDepositsService = (ICashDepositsService)new CashDepositsService();
            this._iSiteSettingService = (ISiteSettingService)new SiteSettingService();
            this._iShopService = (IShopService)new ShopService();
            this._iLimitTimeBuyService = (ILimitTimeBuyService)new LimitTimeBuyService();
            this._iCouponService = (ICouponService)new CouponService();
            this._iShopBonusService = (IShopBonusService)new ShopBonusService();
            this._iCollocationService = (ICollocationService)new CollocationService();
            this._iMemberCapitalService = (IMemberCapitalService)new MemberCapitalService();
            this._iShippingAddressService = (IShippingAddressService)new ShippingAddressService();
            this._iMemberIntegralService = (IMemberIntegralService)new MemberIntegralService();
            this._iRegionService = (IRegionService)new RegionService();
            this._iVShopService = (IVShopService)new VShopService();
            this._iRefundService = (IRefundService)new RefundService();
            this._iFightGroupService = (IFightGroupService)new FightGroupService();
        }

        public object GetSubmitModel(string skuId, int count)
        {
            throw new HimallException("门店订单暂时不允许立即购买！");
        }

        public object GetSubmitByCartModel(string cartItemIds = "")
        {
            this.CheckUserLogin();
            MobileOrderDetailConfirmModel mobileSubmiteByCart = OrderApplication.GetMobileSubmiteByCart(this.CurrentUserId, cartItemIds);
            if (mobileSubmiteByCart.shopBranchInfo == null)
                throw new HimallException("获取门店信息失败，不可提交非门店商品");
            ShopBranch shopBranchById = ShopBranchApplication.GetShopBranchById(mobileSubmiteByCart.shopBranchInfo.Id);
            object obj1 = (object)new ExpandoObject();
            object obj2;
            if (mobileSubmiteByCart.Address != null)
            {
                string str = mobileSubmiteByCart.Address.AddressDetail ?? "";
                var fAnonymousType30 = new
                {
                    Id = mobileSubmiteByCart.Address.Id,
                    ShipTo = mobileSubmiteByCart.Address.ShipTo,
                    Phone = mobileSubmiteByCart.Address.Phone,
                    Address = mobileSubmiteByCart.Address.RegionFullName + " " + mobileSubmiteByCart.Address.Address + " " + str,
                    RegionId = mobileSubmiteByCart.Address.RegionId
                };
                obj2 = (object)fAnonymousType30;
            }
            else
                obj2 = (object)null;
            return (object)this.Json(new
            {
                Success = "true",
                Address = obj2,
                IsCashOnDelivery = mobileSubmiteByCart.IsCashOnDelivery,
                InvoiceContext = mobileSubmiteByCart.InvoiceContext,
                InvoiceTitle = OrderApplication.GetInvoiceTitles(this.CurrentUserId),
                products = mobileSubmiteByCart.products,
                integralPerMoney = mobileSubmiteByCart.integralPerMoney,
                userIntegrals = mobileSubmiteByCart.userIntegrals,
                TotalAmount = mobileSubmiteByCart.totalAmount,
                Freight = mobileSubmiteByCart.Freight,
                orderAmount = mobileSubmiteByCart.orderAmount,
                shopBranchInfo = shopBranchById,
                IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore,
                capitalAmount = mobileSubmiteByCart.capitalAmount
            });
        }

        public object PostDeleteInvoiceTitle(PostDeleteInvoiceTitlePModel para)
        {
            this.CheckUserLogin();
            OrderApplication.DeleteInvoiceTitle(para.id, this.CurrentUserId);
            return (object)new
            {
                Success = true
            };
        }

        [HttpPost]
        public object PostSaveInvoiceTitle(PostSaveInvoiceTitlePModel para)
        {
            this.CheckUserLogin();
            return (object)OrderApplication.SaveInvoiceTitle(this.CurrentUserId, para.name, para.code);
        }

        public object PostSubmitOrder(OrderSubmitOrderModel value)
        {
            this.CheckUserLogin();
            if (value.CapitalAmount > new Decimal(0) && !string.IsNullOrEmpty(value.PayPwd) && !MemberApplication.VerificationPayPwd(this.CurrentUser.Id, value.PayPwd))
                return (object)this.Json(new
                {
                    Success = "false",
                    Msg = "预付款支付密码错误"
                });
            string skuIds = value.skuIds;
            string counts = value.counts;
            long recieveAddressId = value.recieveAddressId;
            string couponIds = value.couponIds;
            int integral = value.integral;
            bool isCashOnDelivery = value.isCashOnDelivery;
            int invoiceType = value.invoiceType;
            string invoiceTitle = value.invoiceTitle;
            string invoiceContext = value.invoiceContext;
            string str = string.Empty;
            OrderCreateModel model = new OrderCreateModel();
            IOrderService create1 = Instance<IOrderService>.Create;
            IProductService create2 = Instance<IProductService>.Create;
            IEnumerable<string> source = Enumerable.Select<string, string>((IEnumerable<string>)skuIds.Split(','), (Func<string, string>)(item => item.ToString()));
            IEnumerable<int> enumerable = Enumerable.Select<string, int>((IEnumerable<string>)counts.Split(','), (Func<string, int>)(item => int.Parse(item)));
            model.CouponIdsStr = OrderHelper.ConvertUsedCoupon(couponIds);
            model.PlatformType = PlatformType.Android;
            model.CurrentUser = this.CurrentUser;
            model.ReceiveAddressId = recieveAddressId;
            model.SkuIds = source;
            model.Counts = enumerable;
            model.Integral = integral;
            model.Capital = value.CapitalAmount;
            model.IsCashOnDelivery = isCashOnDelivery;
            model.Invoice = (InvoiceType)invoiceType;
            model.InvoiceContext = invoiceContext;
            model.InvoiceTitle = invoiceTitle;
            model.InvoiceCode = value.invoiceCode;
            OrderShop[] orderShopArray = JsonConvert.DeserializeObject<OrderShop[]>(value.jsonOrderShops);
            model.OrderShops = orderShopArray;
            model.OrderRemarks = (IEnumerable<string>)Enumerable.ToArray<string>(Enumerable.Select<OrderShop, string>((IEnumerable<OrderShop>)orderShopArray, (Func<OrderShop, string>)(p => p.Remark)));
            model.IsShopbranchOrder = true;
            try
            {
                if (Enumerable.Count<string>(source) == 1)
                {
                    string skuId = Enumerable.ElementAt<string>(source, 0);
                    if (!string.IsNullOrWhiteSpace(skuId))
                    {
                        bool flag = Instance<ILimitTimeBuyService>.Create.IsLimitTimeMarketItem(create2.GetSku(skuId).ProductId);
                        model.IslimitBuy = flag;
                    }
                }
                List<OrderInfo> order = create1.CreateOrder(model);
                IEnumerable<long> orderIds = (IEnumerable<long>)Enumerable.ToArray<long>(Enumerable.Select<OrderInfo, long>((IEnumerable<OrderInfo>)order, (Func<OrderInfo, long>)(item => item.Id)));
                Decimal num = Enumerable.Sum<OrderInfo>((IEnumerable<OrderInfo>)order, (Func<OrderInfo, Decimal>)(item => item.OrderTotalAmount));
                this.AddVshopBuyNumber(orderIds);
                return (object)this.Json(new
                {
                    Success = "true",
                    OrderIds = orderIds,
                    RealTotalIsZero = num == new Decimal(0)
                });
            }
            catch (HimallException ex)
            {
                return (object)this.Json(new
                {
                    Success = "false",
                    Msg = ex.Message
                });
            }
            catch (Exception ex)
            {
                return (object)this.Json(new
                {
                    Success = "false",
                    Msg = "提交订单异常"
                });
            }
        }

        public object PostSubmitOrderByCart(OrderSubmitOrderByCartModel value)
        {
            this.CheckUserLogin();
            if (value.CapitalAmount > new Decimal(0) && !string.IsNullOrEmpty(value.PayPwd) && !MemberApplication.VerificationPayPwd(this.CurrentUser.Id, value.PayPwd))
                return (object)this.Json(new
                {
                    Success = "false",
                    Msg = "预付款支付密码错误"
                });
            string cartItemIds = value.cartItemIds;
            long recieveAddressId = value.recieveAddressId;
            string couponIds = value.couponIds;
            int integral = value.integral;
            bool isCashOnDelivery = value.isCashOnDelivery;
            int invoiceType = value.invoiceType;
            string invoiceTitle = value.invoiceTitle;
            string invoiceContext = value.invoiceContext;
            OrderCreateModel model = new OrderCreateModel();
            IOrderService create = Instance<IOrderService>.Create;
            model.PlatformType = PlatformType.Android;
            model.CurrentUser = this.CurrentUser;
            model.ReceiveAddressId = recieveAddressId;
            model.Integral = integral;
            model.Capital = value.CapitalAmount;
            model.IsCashOnDelivery = isCashOnDelivery;
            model.Invoice = (InvoiceType)invoiceType;
            model.InvoiceContext = invoiceContext;
            model.InvoiceTitle = invoiceTitle;
            model.InvoiceCode = value.invoiceCode;
            OrderShop[] orderShopArray = JsonConvert.DeserializeObject<OrderShop[]>(value.jsonOrderShops);
            model.OrderShops = orderShopArray;
            model.OrderRemarks = (IEnumerable<string>)Enumerable.ToArray<string>(Enumerable.Select<OrderShop, string>((IEnumerable<OrderShop>)orderShopArray, (Func<OrderShop, string>)(p => p.Remark)));
            try
            {
                long[] numArray = Enumerable.ToArray<long>(Enumerable.Select<string, long>(Enumerable.Where<string>((IEnumerable<string>)cartItemIds.Split(','), (Func<string, bool>)(d => !string.IsNullOrWhiteSpace(d))), (Func<string, long>)(item => long.Parse(item))));
                IQueryable<ShoppingCartItem> cartItems = CartApplication.GetCartItems((IEnumerable<long>)numArray);
                model.SkuIds = (IEnumerable<string>)Enumerable.ToList<string>((IEnumerable<string>)Queryable.Select<ShoppingCartItem, string>(cartItems, (Expression<Func<ShoppingCartItem, string>>)(e => e.SkuId)));
                model.Counts = (IEnumerable<int>)Enumerable.ToList<int>((IEnumerable<int>)Queryable.Select<ShoppingCartItem, int>(cartItems, (Expression<Func<ShoppingCartItem, int>>)(e => e.Quantity)));
                model.CartItemIds = numArray;
                model.CouponIdsStr = OrderHelper.ConvertUsedCoupon(couponIds);
                List<OrderInfo> order = create.CreateOrder(model);
                IEnumerable<long> enumerable = (IEnumerable<long>)Enumerable.ToArray<long>(Enumerable.Select<OrderInfo, long>((IEnumerable<OrderInfo>)order, (Func<OrderInfo, long>)(item => item.Id)));
                Decimal num = Enumerable.Sum<OrderInfo>((IEnumerable<OrderInfo>)order, (Func<OrderInfo, Decimal>)(item => item.OrderTotalAmount));
                string str = "false";
                if (num == new Decimal(0))
                    str = "true";
                return (object)this.Json(new
                {
                    Success = "true",
                    OrderIds = enumerable,
                    RealTotalIsZero = str
                });
            }
            catch (HimallException ex)
            {
                return (object)this.Json(new
                {
                    Success = "false",
                    Msg = ex.Message
                });
            }
            catch (Exception ex)
            {
                return (object)this.Json(new
                {
                    Success = "false",
                    Msg = "提交订单异常"
                });
            }
        }

        public object PostSubmitFightGroupOrder(OrderSubmitFightGroupOrderModel value)
        {
            this.CheckUserLogin();
            if (value.CapitalAmount > new Decimal(0) && !string.IsNullOrEmpty(value.PayPwd) && !MemberApplication.VerificationPayPwd(this.CurrentUser.Id, value.PayPwd))
                return (object)this.Json(new
                {
                    Success = "false",
                    Msg = "预付款支付密码错误"
                });
            string skuId = value.skuId;
            long count = value.count;
            long recieveAddressId = value.recieveAddressId;
            long groupActionId = value.GroupActionId;
            long groupId = value.GroupId;
            bool isCashOnDelivery = value.isCashOnDelivery;
            int invoiceType = value.invoiceType;
            string invoiceTitle = value.invoiceTitle;
            string invoiceContext = value.invoiceContext;
            string invoiceCode = value.invoiceCode;
            string orderRemarks = "";
            List<FightGroupOrderJoinStatus> list = new List<FightGroupOrderJoinStatus>()
      {
        FightGroupOrderJoinStatus.Ongoing,
        FightGroupOrderJoinStatus.JoinSuccess
      };
            FightGroupActiveInfo active = Instance<IFightGroupService>.Create.GetActive(groupActionId, false, false, true);
            long num = count;
            int? limitQuantity = active.LimitQuantity;
            if ((num <= (long)limitQuantity.GetValueOrDefault() ? 0 : (limitQuantity.HasValue ? 1 : 0)) != 0)
                return (object)this.Json(new
                {
                    Success = "false",
                    Msg = string.Format("每人限购数量：{0}！", (object)active.LimitQuantity)
                });
            try
            {
                OrderCreateModel groupOrder = OrderApplication.GetGroupOrder(this.CurrentUser.Id, skuId, count.ToString(), recieveAddressId, invoiceType, invoiceTitle, invoiceCode, invoiceContext, groupActionId, PlatformType.Android, groupId, isCashOnDelivery, orderRemarks, new Decimal(0));
                OrderShop[] orderShopArray = JsonConvert.DeserializeObject<OrderShop[]>(value.jsonOrderShops);
                groupOrder.OrderShops = orderShopArray;
                groupOrder.OrderRemarks = (IEnumerable<string>)Enumerable.ToArray<string>(Enumerable.Select<OrderShop, string>((IEnumerable<OrderShop>)orderShopArray, (Func<OrderShop, string>)(p => p.Remark)));
                OrderReturnModel orderReturnModel = OrderApplication.OrderSubmit(groupOrder, "");
                this.AddVshopBuyNumber((IEnumerable<long>)orderReturnModel.OrderIds);
                return (object)this.Json(new
                {
                    Success = "true",
                    OrderIds = orderReturnModel.OrderIds
                });
            }
            catch (HimallException ex)
            {
                return (object)this.Json(new
                {
                    Success = "false",
                    Msg = ex.Message
                });
            }
            catch (Exception ex)
            {
                return (object)this.Json(new
                {
                    Success = "false",
                    Msg = "提交订单异常"
                });
            }
        }

        private void AddVshopBuyNumber(IEnumerable<long> orderIds)
        {
            IEnumerable<long> source = Enumerable.Select<OrderInfo, long>((IEnumerable<OrderInfo>)Instance<IOrderService>.Create.GetOrders(orderIds), (Func<OrderInfo, long>)(item => item.ShopId));
            IVShopService vshopService = Instance<IVShopService>.Create;
            foreach (long vshopId in Enumerable.Where<long>(Enumerable.Select<long, long>(source, (Func<long, long>)(item =>
            {
                VShopInfo vshopByShopId = vshopService.GetVShopByShopId(item);
                if (vshopByShopId != null)
                    return vshopByShopId.Id;
                return 0L;
            })), (Func<long, bool>)(item => item > 0L)))
                vshopService.AddBuyNumber(vshopId);
        }

        public object GetPayOrderByIntegral(string orderIds)
        {
            this.CheckUserLogin();
            ServiceHelper.Create<IOrderService>().ConfirmZeroOrder(Enumerable.Select<string, long>((IEnumerable<string>)orderIds.Split(','), (Func<string, long>)(item => long.Parse(item))), this.CurrentUser.Id);
            return (object)this.Json(new
            {
                Success = "true"
            });
        }

        public object GetGroupOrderModel(string skuId, int count, long GroupActionId, long? GroupId = null)
        {
            this.CheckUserLogin();
            MobileOrderDetailConfirmModel detailConfirmModel = OrderApplication.SubmitByGroupId(this.CurrentUser.Id, skuId, count, GroupActionId, GroupId);
            if (detailConfirmModel.Address != null)
                detailConfirmModel.Address.MemberInfo = new UserMemberInfo();
            detailConfirmModel.IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            return (object)detailConfirmModel;
        }

        public object GetOrderShareProduct(string orderids)
        {
            this.CheckUserLogin();
            if (string.IsNullOrWhiteSpace(orderids))
                throw new HimallException("订单号不能为空！");
            long orderId = 0L;
            return (object)this.Json(new
            {
                Success = true,
                OrderDetail = OrderApplication.GetOrderDetailViews(Enumerable.Select<string, long>((IEnumerable<string>)orderids.Split(','), (Func<string, long>)(e =>
                {
                    if (long.TryParse(e, out orderId))
                        return orderId;
                    return 0L;
                })))
            });
        }

        public object PostOrderShareAddIntegral(OrderShareAddIntegralModel OrderIds)
        {
            this.CheckUserLogin();
            string orderids1 = OrderIds.orderids;
            if (string.IsNullOrWhiteSpace(orderids1))
                throw new HimallException("订单号不能为空！");
            long orderId = 0L;
            IEnumerable<long> orderids2 = Enumerable.Select<string, long>((IEnumerable<string>)orderids1.Split(','), (Func<string, long>)(e =>
            {
                if (long.TryParse(e, out orderId))
                    return orderId;
                throw new HimallException("订单分享增加积分时，订单号异常！");
            }));
            if (MemberIntegralApplication.OrderIsShared(orderids2))
                throw new HimallException("订单已经分享过！");
            MemberIntegralRecord model = new MemberIntegralRecord();
            model.MemberId = this.CurrentUser.Id;
            model.UserName = this.CurrentUser.UserName;
            model.RecordDate = new DateTime?(DateTime.Now);
            model.TypeId = MemberIntegral.IntegralType.Share;
            model.ReMark = string.Format("订单号:{0}", (object)orderids1);
            List<MemberIntegralRecordAction> list = new List<MemberIntegralRecordAction>();
            foreach (long num in orderids2)
                list.Add(new MemberIntegralRecordAction()
                {
                    VirtualItemId = num,
                    VirtualItemTypeId = new MemberIntegral.VirtualItemType?(MemberIntegral.VirtualItemType.ShareOrder)
                });
            model.Himall_MemberIntegralRecordAction = (ICollection<MemberIntegralRecordAction>)list;
            MemberIntegralApplication.AddMemberIntegralByEnum(model, MemberIntegral.IntegralType.Share);
            return (object)this.Json(new
            {
                success = true,
                msg = "晒单添加积分成功！"
            });
        }

        public object GetShopBranchs(long shopId, bool getParent, string skuIds, string counts, int page, int rows, long shippingAddressId, long regionId)
        {
            string[] strArray = skuIds.Split(',');
            int[] _counts = Enumerable.ToArray<int>(Enumerable.Select<string, int>((IEnumerable<string>)counts.Split(','), (Func<string, int>)(p => TypeHelper.ObjectToInt((object)p))));
            ShippingAddressInfo userShippingAddress = ShippingAddressApplication.GetUserShippingAddress(shippingAddressId);
            int streetId = 0;
            int districtId = 0;
            ShopBranchQuery shopBranchQuery = new ShopBranchQuery();
            shopBranchQuery.ShopId = shopId;
            shopBranchQuery.PageNo = page;
            shopBranchQuery.PageSize = rows;
            shopBranchQuery.Status = new ShopBranchStatus?(ShopBranchStatus.Normal);
            ShopBranchQuery query = shopBranchQuery;
            if (userShippingAddress != null)
            {
                query.FromLatLng = string.Format("{0},{1}", (object)userShippingAddress.Latitude, (object)userShippingAddress.Longitude);
                streetId = userShippingAddress.RegionId;
                Region region = RegionApplication.GetRegion((long)userShippingAddress.RegionId, Region.RegionLevel.Town);
                if (region != null && region.ParentId > 0)
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
                hasLatLng = (query.FromLatLng.Split(',').Length == 2 ? 1 : 0) != 0;
            Region region1 = RegionApplication.GetRegion(regionId, getParent ? Region.RegionLevel.City : Region.RegionLevel.County);
            if (region1 != null)
                query.AddressPath = region1.GetIdPath(",");
            List<SKU> skuInfos = ProductManagerApplication.GetSKUs((IEnumerable<string>)strArray);
            query.ProductIds = Enumerable.ToArray<long>(Enumerable.Select<SKU, long>((IEnumerable<SKU>)skuInfos, (Func<SKU, long>)(p => p.ProductId)));
            QueryPageModel<ShopBranch> shopBranchsAll = ShopBranchApplication.GetShopBranchsAll(query);
            List<ShopBranchSkusInfo> shopBranchSkus = ShopBranchApplication.GetSkus(shopId, Enumerable.Select<ShopBranch, long>((IEnumerable<ShopBranch>)shopBranchsAll.Models, (Func<ShopBranch, long>)(p => p.Id)));
            shopBranchsAll.Models.ForEach((Action<ShopBranch>)(p => p.Enabled = Enumerable.All<SKU>((IEnumerable<SKU>)skuInfos, (Func<SKU, bool>)(skuInfo => Enumerable.Any<ShopBranchSkusInfo>((IEnumerable<ShopBranchSkusInfo>)shopBranchSkus, (Func<ShopBranchSkusInfo, bool>)(sbSku => sbSku.ShopBranchId == p.Id && sbSku.Stock >= _counts[skuInfos.IndexOf(skuInfo)] && sbSku.SkuId == skuInfo.Id))))));
            List<ShopBranch> list1 = new List<ShopBranch>();
            List<long> fillterIds = new List<long>();
            List<ShopBranch> list2 = Enumerable.ToList<ShopBranch>((IEnumerable<ShopBranch>)Enumerable.OrderBy<ShopBranch, double>(Enumerable.Where<ShopBranch>((IEnumerable<ShopBranch>)shopBranchsAll.Models, (Func<ShopBranch, bool>)(p => hasLatLng && p.Enabled && ((double)p.Latitude > 0.0 && (double)p.Longitude > 0.0))), (Func<ShopBranch, double>)(p => p.Distance)));
            if (list2 != null && Enumerable.Count<ShopBranch>((IEnumerable<ShopBranch>)list2) > 0)
            {
                fillterIds.AddRange(Enumerable.Select<ShopBranch, long>((IEnumerable<ShopBranch>)list2, (Func<ShopBranch, long>)(p => p.Id)));
                list1.AddRange((IEnumerable<ShopBranch>)list2);
            }
            List<ShopBranch> list3 = Enumerable.ToList<ShopBranch>(Enumerable.Where<ShopBranch>((IEnumerable<ShopBranch>)shopBranchsAll.Models, (Func<ShopBranch, bool>)(p => !fillterIds.Contains(p.Id) && p.Enabled && p.AddressPath.Contains("," + (object)streetId + ","))));
            if (list3 != null && Enumerable.Count<ShopBranch>((IEnumerable<ShopBranch>)list3) > 0)
            {
                fillterIds.AddRange(Enumerable.Select<ShopBranch, long>((IEnumerable<ShopBranch>)list3, (Func<ShopBranch, long>)(p => p.Id)));
                list1.AddRange((IEnumerable<ShopBranch>)list3);
            }
            List<ShopBranch> list4 = Enumerable.ToList<ShopBranch>(Enumerable.Where<ShopBranch>((IEnumerable<ShopBranch>)shopBranchsAll.Models, (Func<ShopBranch, bool>)(p => !fillterIds.Contains(p.Id) && p.Enabled && p.AddressPath.Contains("," + (object)districtId + ","))));
            if (list4 != null && Enumerable.Count<ShopBranch>((IEnumerable<ShopBranch>)list4) > 0)
            {
                fillterIds.AddRange(Enumerable.Select<ShopBranch, long>((IEnumerable<ShopBranch>)list4, (Func<ShopBranch, long>)(p => p.Id)));
                list1.AddRange((IEnumerable<ShopBranch>)list4);
            }
            List<ShopBranch> list5 = Enumerable.ToList<ShopBranch>(Enumerable.Where<ShopBranch>((IEnumerable<ShopBranch>)shopBranchsAll.Models, (Func<ShopBranch, bool>)(p => !fillterIds.Contains(p.Id) && p.Enabled)));
            if (list5 != null && Enumerable.Count<ShopBranch>((IEnumerable<ShopBranch>)list5) > 0)
            {
                fillterIds.AddRange(Enumerable.Select<ShopBranch, long>((IEnumerable<ShopBranch>)list5, (Func<ShopBranch, long>)(p => p.Id)));
                list1.AddRange((IEnumerable<ShopBranch>)list5);
            }
            List<ShopBranch> list6 = Enumerable.ToList<ShopBranch>(Enumerable.Where<ShopBranch>((IEnumerable<ShopBranch>)shopBranchsAll.Models, (Func<ShopBranch, bool>)(p => !fillterIds.Contains(p.Id))));
            if (list6 != null && Enumerable.Count<ShopBranch>((IEnumerable<ShopBranch>)list6) > 0)
                list1.AddRange((IEnumerable<ShopBranch>)list6);
            if (Enumerable.Count<ShopBranch>((IEnumerable<ShopBranch>)list1) != Enumerable.Count<ShopBranch>((IEnumerable<ShopBranch>)shopBranchsAll.Models))
                return (object)this.Json(new
                {
                    Success = false
                });
            var content = new
            {
                Success = true,
                StoreList = Enumerable.Select((IEnumerable<ShopBranch>)list1, sb =>
                {
                    var fAnonymousType37 = new
                    {
                        ContactUser = sb.ContactUser,
                        ContactPhone = sb.ContactPhone,
                        AddressDetail = sb.AddressDetail,
                        ShopBranchName = sb.ShopBranchName,
                        Id = sb.Id,
                        Enabled = sb.Enabled
                    };
                    return fAnonymousType37;
                })
            };
            return (object)this.Json(content);
        }

        public object GetExistShopBranch(long shopId, long regionId, string productIds)
        {
            ShopBranchQuery query = new ShopBranchQuery();
            query.Status = new ShopBranchStatus?(ShopBranchStatus.Normal);
            query.ShopId = shopId;
            Region region = RegionApplication.GetRegion(regionId, Region.RegionLevel.City);
            query.AddressPath = region.GetIdPath(",");
            query.ProductIds = Enumerable.ToArray<long>(Enumerable.Select<string, long>((IEnumerable<string>)productIds.Split(','), (Func<string, long>)(p => long.Parse(p))));
            return (object)this.Json(new
            {
                Success = true,
                ExistShopBranch = ShopBranchApplication.Exists(query) ? 1 : 0
            });
        }
    }
}
