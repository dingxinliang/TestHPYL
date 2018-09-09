﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Himall.IServices;
using Himall.Model;
using Himall.Core;
using Himall.DTO;
using Himall.CommonModel;
using Himall.IServices.QueryModel;

namespace Himall.Application
{
    /// <summary>
    /// 拼团逻辑
    /// </summary>
    public class FightGroupApplication
    {
        private static IFightGroupService _iFightGroupService = ObjectContainer.Current.Resolve<IFightGroupService>();
        private static IProductService _iProductService = ObjectContainer.Current.Resolve<IProductService>();
        private static ILimitTimeBuyService _iLimitTimeBuyService = ObjectContainer.Current.Resolve<ILimitTimeBuyService>();

        /// <summary>
        /// 当前营销类型
        /// </summary>
        private static MarketType CurMarketType = MarketType.FightGroup;

        #region 系统
        /// <summary>
        /// 拼团营销活动费用设置
        /// </summary>
        /// <returns></returns>
        public static decimal GetMarketServicePrice()
        {
            var marketser = MarketApplication.GetServiceSetting(CurMarketType);
            if (marketser == null)
            {
                marketser = new MarketSettingInfo() { TypeId = CurMarketType, Price = 0 };
                MarketApplication.AddOrUpdateServiceSetting(marketser);
            }
            return marketser.Price;
        }
        /// <summary>
        /// 设置拼团营销活动费用设置
        /// </summary>
        /// <param name="price"></param>
        public static void SetMarketServicePrice(decimal price)
        {
            MarketSettingInfo marketser = new MarketSettingInfo() { TypeId = CurMarketType, Price = price };
            MarketApplication.AddOrUpdateServiceSetting(marketser);
        }
        /// <summary>
        /// 是否已开启拼团营销
        /// </summary>
        /// <returns></returns>
        public static bool IsOpenMarketService()
        {
            bool result = false;
            var marketser = MarketApplication.GetServiceSetting(CurMarketType);
            if (marketser != null)
            {
                if (marketser.Price >= 0)
                {
                    result = true;
                }
            }
            return result;
        }
        /// <summary>
        /// 获取拼团营销服务
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static MarketServiceModel GetMarketService(long shopId)
        {
            MarketServiceModel result = null;
            var market = MarketApplication.GetMarketService(shopId, CurMarketType);
            var marketser = MarketApplication.GetServiceSetting(CurMarketType);
            if (marketser != null)
            {
                if (marketser.Price >= 0)
                {
                    result = new MarketServiceModel();
                    result.ShopId = shopId;
                    result.Price = marketser.Price;
                    result.MarketType = CurMarketType;
                    if (market != null)
                    {
                        result.EndTime = market.ServiceEndTime;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 是否可以使用拼团服务
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static bool IsCanUseMarketService(long shopId)
        {
            bool result = false;
            if (shopId <= 0)
            {
                throw new HimallException("错误的诊所编号");
            }
            var market = GetMarketService(shopId);
            if (market != null)
            {
                if (market.IsBuy)
                {
                    result = !market.IsExpired;
                }
            }
            return result;
        }
        /// <summary>
        /// 购买拼团服务
        /// </summary>
        /// <param name="month">数量(月)</param>
        /// <param name="shopId">诊所编号</param>
        public static void BuyMarketService(int month, long shopId)
        {

            if (shopId <= 0)
            {
                throw new HimallException("错误的诊所编号");
            }
            if (month <= 0)
            {
                throw new HimallException("错误的购买数量(月)");
            }
            if (month > 120)
            {
                throw new HimallException("购买数量(月)过大");
            }
            MarketApplication.OrderMarketService(month, shopId, CurMarketType);
        }

        /// <summary>
        /// 获取服务购买列表
        /// </summary>
        /// <param name="shopName"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static QueryPageModel<MarketServiceBuyRecordModel> GetMarketServiceBuyList(string shopName, int page = 1, int pagesize = 10)
        {
            QueryPageModel<MarketServiceBuyRecordModel> result = new QueryPageModel<MarketServiceBuyRecordModel>();
            var queryModel = new MarketBoughtQuery()
            {
                PageSize = pagesize,
                PageNo = page,
                ShopName = shopName,
                MarketType = CurMarketType
            };

            ObsoletePageModel<MarketServiceRecordInfo> marketEntities = MarketApplication.GetBoughtShopList(queryModel);
            if (marketEntities.Total > 0)
            {
                result.Models = marketEntities.Models.Select(d => new MarketServiceBuyRecordModel
                {
                    Id = d.Id,
                    EndTime = d.EndTime,
                    MarketServiceId = d.MarketServiceId,
                    StartTime = d.StartTime,
                    SettlementFlag = d.SettlementFlag,
                    ShopName = d.ActiveMarketServiceInfo.ShopName
                }).ToList();
            }
            if (result.Models == null)
            {
                result.Models = new List<MarketServiceBuyRecordModel>();
            }
            result.Total = marketEntities.Total;

            return result;
        }
        #endregion

        #region 拼团活动
        /// <summary>
        /// 新增拼团活动
        /// </summary>
        /// <param name="data"></param>
        public static void AddActive(FightGroupActiveModel data)
        {
            Mapper.CreateMap<FightGroupActiveModel, FightGroupActiveInfo>();
            Mapper.CreateMap<FightGroupActiveItemModel, FightGroupActiveItemInfo>();
            Mapper.CreateMap<DTO.ComboDetail, Model.ComboDetail>();
            var model = Mapper.Map<FightGroupActiveModel, FightGroupActiveInfo>(data);
            _iFightGroupService.AddActive(model);
            Cache.Remove(CacheKeyCollection.CACHE_FIGHTGROUP);
        }
        /// <summary>
        /// 诊疗项目是否可以参加拼团活动
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool ProductCanJoinActive(long productId)
        {
            bool result = false;
            result = _iFightGroupService.ProductCanJoinActive(productId);
            return result;
        }
        /// <summary>
        /// 修改拼团活动
        /// </summary>
        /// <param name="data"></param>
        public static void UpdateActive(FightGroupActiveModel data)
        {
            #region 更新活动
            Mapper.CreateMap<FightGroupActiveModel, FightGroupActiveInfo>();
            Mapper.CreateMap<FightGroupActiveItemModel, FightGroupActiveItemInfo>();
            Mapper.CreateMap<DTO.ComboDetail, Model.ComboDetail>();
            var model = Mapper.Map<FightGroupActiveModel, FightGroupActiveInfo>(data);
            _iFightGroupService.UpdateActive(model);
            Cache.Remove(CacheKeyCollection.CACHE_FIGHTGROUP);
            #endregion
        }
        /// <summary>
        /// 下架拼团活动
        /// </summary>
        /// <param name="id"></param>
        /// <param name="manageRemark">下架原因</param>
        /// <param name="manageId">管理员编号</param>
        public static void CancelActive(long id, string manageRemark, long manageId)
        {
            _iFightGroupService.CancelActive(id, manageRemark, manageId);
        }
        /// <summary>
        /// 获取拼团活动
        /// </summary>
        /// <param name="id"></param>
        /// <param name="needGetProductCommentNumber">是否需要同步获取诊疗项目的评价数量</param>
        /// <param name="isLoadItems">是否加载节点信息</param>
        /// <returns></returns>
        public static FightGroupActiveModel GetActive(long id, bool needGetProductCommentNumber = false, bool isLoadItems = true)
        {
            FightGroupActiveInfo source = _iFightGroupService.GetActive(id, needGetProductCommentNumber, isLoadItems, true);
            Mapper.CreateMap<FightGroupActiveInfo, FightGroupActiveModel>();
            Mapper.CreateMap<FightGroupActiveItemInfo, FightGroupActiveItemModel>();
            FightGroupActiveModel model = Mapper.Map<FightGroupActiveInfo, FightGroupActiveModel>(source);
            if (model != null)
            {
                model.ProductDefaultImage = HimallIO.GetProductSizeImage(source.ProductImgPath, 1, 350);
                if (!string.IsNullOrWhiteSpace(model.IconUrl))
                {
                    model.IconUrl = HimallIO.GetImagePath(model.IconUrl, null);
                }
                if (model.ActiveItems == null)
                {
                    return model;
                }
                foreach (FightGroupActiveItemModel model2 in model.ActiveItems)
                {
                    if (!string.IsNullOrWhiteSpace(model2.ShowPic))
                    {
                        model2.ShowPic = HimallIO.GetImagePath(model2.ShowPic, null);
                    }
                }
            }
            return model;

        }
        /// <summary>
        /// 获取拼团活动
        /// </summary>
        /// <param name="id"></param>
        /// <param name="needGetProductCommentNumber">是否需要同步获取诊疗项目的评价数量</param>
        /// <param name="isLoadItems">是否加载节点信息</param>
        /// <returns></returns>
        public static FightGroupActiveModel XcxGetActive(long id, bool needGetProductCommentNumber = false, bool isLoadItems = true)
        {
            var data = _iFightGroupService.GetActive(id, needGetProductCommentNumber, isLoadItems);
            Mapper.CreateMap<FightGroupActiveInfo, FightGroupActiveModel>();
            //规格映射
            Mapper.CreateMap<FightGroupActiveItemInfo, FightGroupActiveItemModel>();
            Mapper.CreateMap<Model.ComboDetail, DTO.ComboDetail>();
            FightGroupActiveModel result = Mapper.Map<FightGroupActiveInfo, FightGroupActiveModel>(data);
            result.MiniSalePrice = data.MiniSalePrice;
            if (result != null)
            {
                //诊疗项目图片地址修正
                result.ProductDefaultImage = HimallIO.GetProductSizeImage(data.ProductImgPath, 1, (int)ImageSize.Size_350);
                if (!string.IsNullOrWhiteSpace(result.IconUrl))
                {
                    result.IconUrl = Himall.Core.HimallIO.GetImagePath(result.IconUrl);
                }
                if (result.ActiveItems != null)
                {
                    foreach (var item in result.ActiveItems)
                    {
                        if (!string.IsNullOrWhiteSpace(item.ShowPic))
                        {
                            item.ShowPic = HimallIO.GetImagePath(item.ShowPic);
                        }
                    }
                }
            }
            return result;
        }
        public static List<FightGroupActiveModel> GetActives(long[] ids)
        {
            var data = _iFightGroupService.GetActive(ids);
            Mapper.CreateMap<FightGroupActiveInfo, FightGroupActiveModel>();
            //规格映射
            Mapper.CreateMap<FightGroupActiveItemInfo, FightGroupActiveItemModel>();
            List<FightGroupActiveModel> result = Mapper.Map<List<FightGroupActiveInfo>, List<FightGroupActiveModel>>(data);
            return result;
        }
        /// <summary>
        /// 使用诊疗项目编号获取正在进行的拼团活动
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static FightGroupActiveModel GetActiveByProductId(long productId)
        {
            long actid = _iFightGroupService.GetActiveIdByProductId(productId);
            FightGroupActiveModel result = null;
            if (actid > 0)
            {
                result = GetActive(actid, false, false);
            }
            return result;
        }
        public static List<FightGroupActiveModel> GetActiveByProductIds(long[] productIds)
        {
            var actids = _iFightGroupService.GetActiveIdByProductIds(productIds);
            var result = new List<FightGroupActiveModel>();
            if (actids != null && actids.Count > 0)
            {
                var a =
                result = GetActives(actids.ToArray());
            }
            return result;
        }
        /// <summary>
        /// 获取拼团项
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<FightGroupActiveItemModel> GetActiveItems(long id)
        {
            List<FightGroupActiveItemModel> result = new List<FightGroupActiveItemModel>();
            var data = _iFightGroupService.GetActiveItems(id);
            Mapper.CreateMap<List<FightGroupActiveItemInfo>, List<FightGroupActiveItemModel>>();
            result = Mapper.Map<List<FightGroupActiveItemModel>>(data);
            return result;
        }
        /// <summary>
        /// 获取拼团项用于新增活动
        /// </summary>
        /// <param name="productId">诊疗项目编号</param>
        /// <returns></returns>
        public static FightGroupGetSkuListModel GetNewActiveItems(long productId)
        {
            FightGroupGetSkuListModel result = new FightGroupGetSkuListModel();
            var pro = _iProductService.GetProduct(productId);
            result.ProductImg = HimallIO.GetProductSizeImage(pro.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_150);
            result.skulist = new List<FightGroupActiveItemModel>();
            foreach (var item in pro.SKUInfo)
            {
                FightGroupActiveItemModel _data = new FightGroupActiveItemModel();
                _data.ProductId = productId;
                _data.SkuId = item.Id;
                _data.SkuName = item.Color + " " + item.Size + " " + item.Version;
                _data.ProductPrice = item.SalePrice;
                _data.ProductStock = item.Stock;
                _data.ActivePrice = _data.ProductPrice;
                _data.ActiveStock = 0;  //活动库存置空
                result.skulist.Add(_data);
            }
            return result;
        }
        /// <summary>
        /// 获取活动列表
        /// </summary>
        /// <param name="Statuses">状态集</param>
        /// <param name="ProductName">诊疗项目名</param>
        /// <param name="ShopName">诊所名</param>
        /// <param name="ShopId">诊所编号</param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static QueryPageModel<FightGroupActiveListModel> GetActives(
            List<FightGroupActiveStatus> Statuses = null,
            string ProductName = "",
            string ShopName = "",
            long? ShopId = null,
            int PageNo = 1,
            int PageSize = 10
            )
        {
            QueryPageModel<FightGroupActiveListModel> result = null;
            var data = _iFightGroupService.GetActives(Statuses, null, null, ProductName, ShopName, ShopId, PageNo, PageSize);
            Mapper.CreateMap<FightGroupActiveInfo, FightGroupActiveListModel>();
            Mapper.CreateMap<QueryPageModel<FightGroupActiveInfo>, QueryPageModel<FightGroupActiveListModel>>();
            result = Mapper.Map<QueryPageModel<FightGroupActiveListModel>>(data);
            if (result.Total > 0)
            {
                //数据映射
                foreach (var item in result.Models)
                {
                    if (!string.IsNullOrWhiteSpace(item.IconUrl))
                    {
                        item.IconUrl = Himall.Core.HimallIO.GetImagePath(item.IconUrl);
                    }
                }
            }
            return result;
        }
        #endregion

        #region 拼团组团详情
        /// <summary>
        /// 开团
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="userId">团长用户编号</param>
        /// <returns>组团编号</returns>
        public static long OpenGroup(long activeId, long userId)
        {
            return _iFightGroupService.AddGroup(activeId, userId).Id;
        }
        /// <summary>
        /// 是否可以参团
        /// </summary>
        /// <param name="activeId"></param>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool CanJoinGroup(long activeId, long groupId, long userId)
        {
            return _iFightGroupService.CanJoinGroup(activeId, groupId, userId);
        }
        /// <summary>
        /// 获取拼团
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="groupId">团编号</param>
        /// <returns></returns>
        public static FightGroupsModel GetGroup(long activeId, long groupId)
        {
            var data = _iFightGroupService.GetGroup(activeId, groupId);
            Mapper.CreateMap<FightGroupsInfo, FightGroupsModel>();
            //子数据映射
            Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            FightGroupsModel result = Mapper.Map<FightGroupsModel>(data);

            if (result != null)
            {
                result.HeadUserIcon = Himall.Core.HimallIO.GetImagePath(result.HeadUserIcon);
                if (result.GroupOrders != null)
                {
                    foreach (var subitem in result.GroupOrders)
                    {
                        subitem.Photo = Himall.Core.HimallIO.GetImagePath(subitem.Photo);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 获取拼团详情列表
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="Statuses">状态集</param>
        /// <param name="StartTime">开始时间</param>
        /// <param name="EndTime">结束时间</param>
        /// <param name="PageNo"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        public static QueryPageModel<FightGroupsListModel> GetGroups(
            long activeId,
            List<FightGroupBuildStatus> Statuses,
            DateTime? StartTime,
            DateTime? EndTime,
            int PageNo,
            int PageSize
            )
        {
            Mapper.CreateMap<FightGroupsInfo, FightGroupsListModel>();
            Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            var data = _iFightGroupService.GetGroups(activeId, Statuses, StartTime, EndTime, PageNo, PageSize);
            QueryPageModel<FightGroupsListModel> result = new QueryPageModel<FightGroupsListModel>();
            result.Total = data.Total;
            result.Models = new List<FightGroupsListModel>();
            if (data.Total > 0)
            {
                foreach (var item in data.Models)
                {
                    item.HeadUserIcon = Himall.Core.HimallIO.GetImagePath(item.HeadUserIcon);
                    var _tmp = Mapper.Map<FightGroupsListModel>(item);
                    if (item.GroupOrders != null)
                    {
                        _tmp.OrderIds = new List<long>();
                        foreach (var ord in item.GroupOrders)
                        {
                            _tmp.OrderIds.Add(ord.OrderId.Value);
                        }
                    }
                    result.Models.Add(_tmp);
                }
            }
            return result;
        }
        public static List<FightGroupsListModel> GetCanJoinGroupsFirst(List<FightGroupBuildStatus> Statuses, int PageNo = 1, int PageSize = 10)
        {
            Mapper.CreateMap<FightGroupsInfo, FightGroupsListModel>();
            Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            var data = _iFightGroupService.GetCanJoinGroupsFirst(Statuses, PageNo, PageSize);
            List<FightGroupsListModel> result = new List<FightGroupsListModel>();
            if (data.Count > 0)
            {
                foreach (var item in data)
                {
                    item.HeadUserIcon = Himall.Core.HimallIO.GetImagePath(item.HeadUserIcon);
                    var _tmp = Mapper.Map<FightGroupsListModel>(item);
                    if (item.GroupOrders != null)
                    {
                        _tmp.OrderIds = new List<long>();
                        foreach (var ord in item.GroupOrders)
                        {
                            _tmp.OrderIds.Add(ord.OrderId.Value);
                        }
                    }
                    result.Add(_tmp);
                }
            }
            return result;
        }
        public static List<FightGroupsListModel> GetCanJoinGroupsSecond(long[] unActiveId, List<FightGroupBuildStatus> Statuses)
        {
            Mapper.CreateMap<FightGroupsInfo, FightGroupsListModel>();
            Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            var data = _iFightGroupService.GetCanJoinGroupsSecond(unActiveId, Statuses);
            List<FightGroupsListModel> result = new List<FightGroupsListModel>();
            if (data.Count > 0)
            {
                foreach (var item in data)
                {
                    item.HeadUserIcon = Himall.Core.HimallIO.GetImagePath(item.HeadUserIcon);
                    var _tmp = Mapper.Map<FightGroupsListModel>(item);
                    if (item.GroupOrders != null)
                    {
                        _tmp.OrderIds = new List<long>();
                        foreach (var ord in item.GroupOrders)
                        {
                            _tmp.OrderIds.Add(ord.OrderId.Value);
                        }
                    }
                    result.Add(_tmp);
                }
            }
            return result;
        }
        /// <summary>
        /// 获取参与的拼团
        /// </summary>
        /// <param name="userId">用户编号</param>
        /// <param name="Statuses">参与状态</param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static QueryPageModel<FightGroupsModel> GetJoinGroups(
            long userId
            , List<FightGroupOrderJoinStatus> Statuses = null
            , int PageNo = 1
            , int PageSize = 10
            )
        {
            Mapper.CreateMap<FightGroupsInfo, FightGroupsModel>();
            Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            var map = Mapper.CreateMap<QueryPageModel<FightGroupsInfo>, QueryPageModel<FightGroupsModel>>();
            var data = _iFightGroupService.GetJoinGroups(userId, Statuses, PageNo, PageSize);
            QueryPageModel<FightGroupsModel> result = Mapper.Map<QueryPageModel<FightGroupsModel>>(data);

            foreach (var item in result.Models)
            {
                item.HeadUserIcon = Himall.Core.HimallIO.GetImagePath(item.HeadUserIcon);
                if (item.GroupOrders != null)
                {
                    foreach (var subitem in item.GroupOrders)
                    {
                        subitem.Photo = Himall.Core.HimallIO.GetImagePath(subitem.Photo);
                    }
                }
            }
            return result;
        }
        #endregion

        #region 拼团预约单
        /// <summary>
        /// 用户在营销活动中已购买数量
        /// </summary>
        /// <param name="activeId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static int GetMarketSaleCountForUserId(long activeId, long userId)
        {
            return _iFightGroupService.GetMarketSaleCountForUserId(activeId, userId);
        }
        /// <summary>
        /// 拼团预约单
        /// </summary>
        /// <param name="actionId">活动编号</param>
        /// <param name="orderId">预约单编号</param>
        /// <param name="userId">用户编号</param>
        /// <param name="groupId">拼团编号 0表示开新团</param>
        public static FightGroupOrderModel AddOrder(long actionId, long orderId, long userId, long groupId = 0)
        {
            FightGroupOrderInfo data = _iFightGroupService.AddOrder(actionId, orderId, userId, groupId);
            Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            FightGroupOrderModel result = Mapper.Map<FightGroupOrderModel>(data);
            if (result != null)
            {
                result.Photo = Himall.Core.HimallIO.GetImagePath(result.Photo);
            }
            return result;
        }
        /// <summary>
        /// 设定加入拼团状态
        /// </summary>
        /// <param name="orderId">预约单号</param>
        /// <param name="status">状态</param>
        public static FightGroupOrderJoinStatus SetOrderStatus(long orderId, FightGroupOrderJoinStatus status)
        {
            FightGroupOrderJoinStatus result = _iFightGroupService.SetOrderStatus(orderId, status);
            return result;
        }
        /// <summary>
        /// 获取拼团预约单
        /// </summary>
        /// <param name="orderId">预约单编号</param>
        /// <returns></returns>
        public static FightGroupOrderModel GetOrder(long orderId)
        {
            Mapper.CreateMap<FightGroupOrderInfo, FightGroupOrderModel>();
            var data = _iFightGroupService.GetOrder(orderId);
            FightGroupOrderModel result = Mapper.Map<FightGroupOrderModel>(data);
            if (result != null)
            {
                result.Photo = Himall.Core.HimallIO.GetImagePath(result.Photo);
            }
            return result;
        }

        /// <summary>
        /// 获取参团中的预约单数
        /// </summary>
        /// <param name="userId">用户编号</param>
        /// <returns></returns>
        public static int CountJoiningOrder(long userId)
        {
            return _iFightGroupService.CountJoiningOrder(userId);
        }

        #endregion

        /// <summary>
        /// 预约单是否可以支付
        /// <para>成团成功后，未完成支付的预约单不可付款</para>
        /// <para>成团失败后，未完成支付的预约单不可付款</para>
        /// </summary>
        /// <param name="orderId">预约单编号</param>
        /// <returns></returns>
        public static bool OrderCanPay(long orderId)
        {
            return _iFightGroupService.OrderCanPay(orderId);
        }

        /// <summary>
        /// 根据原预约单号获取拼团预约单信息
        /// </summary>
        /// <param name="orderId">原预约单号</param>
        /// <returns></returns>
        public static FightGroupOrderInfo GetFightGroupOrderStatusByOrderId(long orderId)
        {
            return _iFightGroupService.GetFightGroupOrderStatusByOrderId(orderId);
        }

        public static List<FightGroupPrice> GetFightGroupPrice()
        {
            if (Cache.Exists(CacheKeyCollection.CACHE_FIGHTGROUP))
                return Cache.Get<List<FightGroupPrice>>(CacheKeyCollection.CACHE_FIGHTGROUP);
            var result = _iFightGroupService.GetFightGroupPrice();
            Cache.Insert(CacheKeyCollection.CACHE_FIGHTGROUP, result, 120);
            return result;
        }
    }
}
