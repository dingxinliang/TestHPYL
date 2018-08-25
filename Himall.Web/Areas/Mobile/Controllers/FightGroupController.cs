using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Core;
using Himall.CommonModel;
using Himall.DTO;
using Himall.Web.Framework;

using Himall.IServices;
using Himall.Web.Areas.Mobile.Models;
using Himall.Web.App_Code.Common;
using Himall.Application;
using Himall.Model;
using AutoMapper;
using Himall.Web.Areas.Web.Models;

namespace Himall.Web.Areas.Mobile.Controllers
{
    /// <summary>
    /// 拼团
    /// </summary>
    public class FightGroupController : BaseMobileTemplatesController
    {
        // Fields
        private IProductService _iProductService;
        private ITypeService _iTypeService;

        // Methods
        public FightGroupController(IProductService iProductService, ITypeService iTypeService)
        {
            this._iProductService = iProductService;
            this._iTypeService = iTypeService;
        }

        public JsonResult CanJoin(long aid, long gpid)
        {
            BaseController.Result data = new BaseController.Result
            {
                success = false,
                msg = "不可重复参团"
            };
            if (FightGroupApplication.CanJoinGroup(aid, gpid, base.CurrentUser.Id))
            {
                data = new BaseController.Result
                {
                    success = true,
                    msg = "yes"
                };
            }
            return base.Json(data);
        }

        [HttpPost]
        public JsonResult CheckBuyNumber(long id, string skuId, int count)
        {
            int marketSaleCountForUserId = 0;
            if (base.CurrentUser != null)
            {
                marketSaleCountForUserId = FightGroupApplication.GetMarketSaleCountForUserId(id, base.CurrentUser.Id);
            }
            else
            {
                marketSaleCountForUserId = 0;
            }
            return base.Json(new { success = true, hasbuy = marketSaleCountForUserId });
        }

        public ActionResult Close()
        {
            return base.View();
        }

        public ActionResult Detail(long id)
        {
            FightGroupActiveModel source = FightGroupApplication.GetActive(id, false, true);
            if (source == null)
            {
                throw new HimallException("错误的活动信息");
            }
            source.InitProductImages();
            Mapper.CreateMap<FightGroupActiveModel, FightGroupActiveDetailModel>();
            FightGroupActiveDetailModel model = Mapper.Map<FightGroupActiveDetailModel>(source);
            decimal memberDiscount = 1;
            if (base.CurrentUser != null)
            {
                memberDiscount = base.CurrentUser.MemberDiscount;
            }
            ((dynamic)base.ViewBag).Discount = memberDiscount;
            Shop shop = ShopApplication.GetShop(model.ShopId, false);
            ((dynamic)base.ViewBag).IsSelf = shop.IsSelf;
            model.ShareUrl = string.Format("{0}/m-{1}/FightGroup/Detail/{2}", CurrentUrlHelper.CurrentUrlNoPort(), "WeiXin", source.Id);
            model.ShareTitle = (source.ActiveStatus == FightGroupActiveStatus.WillStart) ? "限时限量火拼 即将开始" : "限时限量火拼 正在进行";
            model.ShareImage = source.ProductDefaultImage;
            if (!string.IsNullOrWhiteSpace(model.ShareImage) && (model.ShareImage.Substring(0, 4) != "http"))
            {
                model.ShareImage = HimallIO.GetRomoteImagePath(model.ShareImage, null);
            }
            model.ShareDesc = source.ProductName;
            if (!string.IsNullOrWhiteSpace(source.ProductShortDescription))
            {
                model.ShareDesc = model.ShareDesc + "，(" + source.ProductShortDescription + ")";
            }
            if (model.ProductId.HasValue)
            {
                StatisticApplication.StatisticVisitCount(model.ProductId.Value, model.ShopId);
            }
            List<CustomerService> mobileCustomerService = CustomerServiceApplication.GetMobileCustomerService(model.ShopId);
            CustomerService item = CustomerServiceApplication.GetPreSaleByShopId(model.ShopId).FirstOrDefault<CustomerService>(p => p.Tool == CustomerServiceInfo.ServiceTool.MeiQia);
            if (item != null)
            {
                mobileCustomerService.Insert(0, item);
            }
            ((dynamic)base.ViewBag).CustomerServices = mobileCustomerService;
            return base.View(model);
        }

        [HttpPost]
        public JsonResult GetSkus(long id)
        {
            FightGroupActiveModel model = FightGroupApplication.GetActive(id, false, true);
            if (model == null)
            {
                throw new HimallException("错误的活动信息");
            }
            List<SKUDataModel> data = (from d in model.ActiveItems
                                       where d.ActiveStock > 0L
                                       select new SKUDataModel { SkuId = d.SkuId, Color = d.Color, Size = d.Size, Version = d.Version, Stock = (int)d.ActiveStock.Value, CostPrice = d.ProductCostPrice, SalePrice = d.ProductPrice, Price = d.ActivePrice }).ToList<SKUDataModel>();
            return base.Json(data);
        }

        public ActionResult GroupDetail(long id, long aid)
        {
            FightGroupActiveModel model = FightGroupApplication.GetActive(aid, false, true);
            if (model == null)
            {
                throw new HimallException("错误的活动信息");
            }
            model.InitProductImages();
            FightGroupsModel group = FightGroupApplication.GetGroup(aid, id);
            if (group == null)
            {
                throw new HimallException("错误的拼团信息");
            }
            if (group.BuildStatus == FightGroupBuildStatus.Opening)
            {
                return this.Redirect(string.Format("/m-{0}/Member/Center/", base.PlatformType.ToString()));
            }
            FightGroupGroupDetailModel model3 = new FightGroupGroupDetailModel
            {
                ActiveData = model,
                GroupsData = group,
                HasJoin = false
            };
            if (base.CurrentUser != null)
            {
                model3.HasJoin = !FightGroupApplication.CanJoinGroup(aid, id, base.CurrentUser.Id);
            }
            model3.ShareUrl = string.Format("{0}/m-{1}/FightGroup/GroupDetail/{2}?aid={3}", new object[] { CurrentUrlHelper.CurrentUrlNoPort(), "WeiXin", group.Id, group.ActiveId });
            model3.ShareTitle = "我参加了(" + model.ProductName + ")的拼团";
            model3.ShareImage = model.ProductDefaultImage;
            if (!string.IsNullOrWhiteSpace(model3.ShareImage) && (model3.ShareImage.Substring(0, 4) != "http"))
            {
                model3.ShareImage = HimallIO.GetRomoteImagePath(model3.ShareImage, null);
            }
            int num = group.LimitedNumber - group.JoinedNumber;
            num = (num < 0) ? 0 : num;
            if (num > 0)
            {
                model3.ShareDesc = "还差" + num + "人即可成团";
            }
            if (!string.IsNullOrWhiteSpace(model.ProductShortDescription))
            {
                if (!string.IsNullOrWhiteSpace(model3.ShareDesc))
                {
                    model3.ShareDesc = model3.ShareDesc + "，(" + model.ProductShortDescription + ")";
                }
                else
                {
                    model3.ShareDesc = model3.ShareDesc + model.ProductShortDescription;
                }
            }
            return base.View(model3);
        }

        public ActionResult GroupOrderOk(long orderid)
        {
            FightGroupOrderOkModel model = new FightGroupOrderOkModel();
            FightGroupOrderModel order = FightGroupApplication.GetOrder(orderid);
            if (order == null)
            {
                throw new HimallException("错误的拼团订单信息");
            }
            FightGroupsModel group = FightGroupApplication.GetGroup(order.ActiveId, order.GroupId);
            if (group == null)
            {
                throw new HimallException("错误的拼团信息");
            }
            if (group.BuildStatus == FightGroupBuildStatus.Opening)
            {
                return this.Redirect(string.Format("/m-{0}/Member/Center/", base.PlatformType.ToString()));
            }
            if (group.BuildStatus == FightGroupBuildStatus.Failed)
            {
            }
            FightGroupActiveModel model4 = FightGroupApplication.GetActive(group.ActiveId, false, true);
            model.isFirst = order.IsFirstOrder;
            model.LimitedNumber = group.LimitedNumber;
            model.JoinNumber = group.JoinedNumber;
            model.ShareUrl = string.Format("{0}/m-{1}/FightGroup/GroupDetail/{2}?aid={3}", new object[] { CurrentUrlHelper.CurrentUrlNoPort(), "WeiXin", group.Id, group.ActiveId });
            model.ShareTitle = "我参加了(" + group.ProductName + ")的拼团";
            model.ShareImage = model4.ProductDefaultImage;
            if (!string.IsNullOrWhiteSpace(model.ShareImage) && (model.ShareImage.Substring(0, 4) != "http"))
            {
                model.ShareImage = HimallIO.GetRomoteImagePath(model.ShareImage, null);
            }
            int num = group.LimitedNumber - group.JoinedNumber;
            num = (num < 0) ? 0 : num;
            if (num > 0)
            {
                model.ShareDesc = "还差" + num + "人即可成团";
            }
            if (!string.IsNullOrWhiteSpace(model4.ProductShortDescription))
            {
                if (!string.IsNullOrWhiteSpace(model.ShareDesc))
                {
                    model.ShareDesc = model.ShareDesc + "，(" + model4.ProductShortDescription + ")";
                }
                else
                {
                    model.ShareDesc = model.ShareDesc + model4.ProductShortDescription;
                }
            }
            return base.View(model);
        }

        public ActionResult Index()
        {
            return base.View();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if ((filterContext.RouteData.Values["action"].ToString().ToLower().ToLower() != "close") && !FightGroupApplication.IsOpenMarketService())
            {
                filterContext.Result = base.RedirectToAction("Close");
            }
            else
            {
                base.OnActionExecuting(filterContext);
            }
        }

        [HttpPost]
        public JsonResult PostActiveList(int page)
        {
            int pageSize = 5;
            QueryPageModel<FightGroupActiveListModel> model = FightGroupApplication.GetActives(new List<FightGroupActiveStatus> { 0, FightGroupActiveStatus.WillStart }, "", "", null, page, pageSize);
            List<FightGroupActiveListModel> list2 = model.Models.ToList<FightGroupActiveListModel>();
            return base.Json(new { success = true, rows = list2, total = model.Total });
        }

        [ChildActionOnly]
        public ActionResult ShowActionHead(FightGroupActiveModel data)
        {
            FightGroupShowDetailModel model = new FightGroupShowDetailModel();
            if (data == null)
            {
                throw new HimallException("错误的活动信息");
            }
            data.InitProductImages();
            model.ActiveData = data;
            return base.View(model);
        }

        [ChildActionOnly]
        public ActionResult ShowDetail(FightGroupActiveModel data, int hst = 1, DateTime? etime = new DateTime?())
        {
            FightGroupShowDetailModel model = new FightGroupShowDetailModel();
            if (data == null)
            {
                throw new HimallException("错误的活动信息");
            }
            model.ActiveData = data;
            model.LimitedHourShowType = hst;
            if (etime.HasValue)
            {
                model.EndBuildGroupTime = etime.Value;
            }
            model.ProductMiniPriceByUser = model.ActiveData.MiniSalePrice;
            if (ShopApplication.GetShop(data.ShopId, false).IsSelf)
            {
                decimal memberDiscount = 1M;
                if (base.CurrentUser != null)
                {
                    memberDiscount = base.CurrentUser.MemberDiscount;
                }
                model.ProductMiniPriceByUser *= memberDiscount;
            }
            return base.View(model);
        }

        [ChildActionOnly]
        public ActionResult ShowNewCanJoinGroup(long id)
        {
            List<FightGroupBuildStatus> statuses = new List<FightGroupBuildStatus> { 0 };
            DateTime? startTime = null;
            List<FightGroupsListModel> model = FightGroupApplication.GetGroups(id, statuses, startTime, null, 1, 10).Models.ToList<FightGroupsListModel>();
            return base.View(model);
        }

        [ChildActionOnly]
        public ActionResult ShowSkuInfo(FightGroupActiveModel data)
        {
            if (data == null)
            {
                throw new HimallException("错误的活动信息");
            }
            ProductShowSkuInfoModel model = new ProductShowSkuInfoModel
            {
                MinSalePrice = data.MiniGroupPrice,
                ProductImagePath = data.ProductImgPath
            };
            ProductTypeInfo typeByProductId = this._iTypeService.GetTypeByProductId(data.ProductId.Value);
            string str = ((typeByProductId == null) || string.IsNullOrEmpty(typeByProductId.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeByProductId.ColorAlias;
            string str2 = ((typeByProductId == null) || string.IsNullOrEmpty(typeByProductId.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeByProductId.SizeAlias;
            string str3 = ((typeByProductId == null) || string.IsNullOrEmpty(typeByProductId.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeByProductId.VersionAlias;
            model.ColorAlias = str;
            model.SizeAlias = str2;
            model.VersionAlias = str3;
            if ((data.ActiveItems != null) && (data.ActiveItems.Count<FightGroupActiveItemModel>() > 0))
            {
                long result = 0L;
                long num2 = 0L;
                long num3 = 0L;
                using (List<FightGroupActiveItemModel>.Enumerator enumerator = data.ActiveItems.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Func<ProductSKU, bool> predicate = null;
                        Func<FightGroupActiveItemModel, bool> func2 = null;
                        Func<ProductSKU, bool> func3 = null;
                        Func<FightGroupActiveItemModel, bool> func4 = null;
                        Func<ProductSKU, bool> func5 = null;
                        Func<FightGroupActiveItemModel, bool> func6 = null;
                        FightGroupActiveItemModel sku = enumerator.Current;
                        string[] source = sku.SkuId.Split(new char[] { '_' });
                        if (source.Count<string>() > 0)
                        {
                            if (long.TryParse(source[1], out result))
                            {
                            }
                            if (result != 0L)
                            {
                                if (predicate == null)
                                {
                                    predicate = v => v.Value.Equals(sku.Color);
                                }
                                if (!model.Color.Any<ProductSKU>(predicate))
                                {
                                    if (func2 == null)
                                    {
                                        func2 = s => s.Color.Equals(sku.Color);
                                    }
                                    long? nullable = data.ActiveItems.Where<FightGroupActiveItemModel>(func2).Sum<FightGroupActiveItemModel>((Func<FightGroupActiveItemModel, long?>)(s => s.ActiveStock));
                                    ProductSKU item = new ProductSKU
                                    {
                                        Name = "选择" + str,
                                        EnabledClass = (nullable != 0L) ? "enabled" : "disabled",
                                        SelectedClass = "",
                                        SkuId = result,
                                        Value = sku.Color,
                                        Img = sku.ShowPic
                                    };
                                    model.Color.Add(item);
                                }
                            }
                        }
                        if (source.Count<string>() > 1)
                        {
                            if (long.TryParse(source[2], out num2))
                            {
                            }
                            if (num2 != 0L)
                            {
                                if (func3 == null)
                                {
                                    func3 = v => v.Value.Equals(sku.Size);
                                }
                                if (!model.Size.Any<ProductSKU>(func3))
                                {
                                    if (func4 == null)
                                    {
                                        func4 = s => s.Size.Equals(sku.Size);
                                    }
                                    long? nullable2 = data.ActiveItems.Where<FightGroupActiveItemModel>(func4).Sum<FightGroupActiveItemModel>((Func<FightGroupActiveItemModel, long?>)(s1 => s1.ActiveStock));
                                    ProductSKU tsku2 = new ProductSKU
                                    {
                                        Name = "选择" + str2,
                                        EnabledClass = (nullable2 != 0L) ? "enabled" : "disabled",
                                        SelectedClass = "",
                                        SkuId = num2,
                                        Value = sku.Size
                                    };
                                    model.Size.Add(tsku2);
                                }
                            }
                        }
                        if (source.Count<string>() > 2)
                        {
                            if (long.TryParse(source[3], out num3))
                            {
                            }
                            if (num3 != 0L)
                            {
                                if (func5 == null)
                                {
                                    func5 = v => v.Value.Equals(sku.Version);
                                }
                                if (!model.Version.Any<ProductSKU>(func5))
                                {
                                    if (func6 == null)
                                    {
                                        func6 = s => s.Version.Equals(sku.Version);
                                    }
                                    long? nullable3 = data.ActiveItems.Where<FightGroupActiveItemModel>(func6).Sum<FightGroupActiveItemModel>((Func<FightGroupActiveItemModel, long?>)(s => s.ActiveStock));
                                    ProductSKU tsku3 = new ProductSKU
                                    {
                                        Name = "选择" + str3,
                                        EnabledClass = (nullable3 != 0L) ? "enabled" : "disabled",
                                        SelectedClass = "",
                                        SkuId = num3,
                                        Value = sku.Version
                                    };
                                    model.Version.Add(tsku3);
                                }
                            }
                        }
                    }
                }
            }
            return base.View(model);
        }
    }
}