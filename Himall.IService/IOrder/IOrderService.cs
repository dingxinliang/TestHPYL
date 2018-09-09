﻿using Himall.Core;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Himall.CommonModel;
using Himall.CommonModel.Delegates;

namespace Himall.IServices
{
    /// <summary>
    /// 预约单服务接口
    /// </summary>
    public interface IOrderService : IService
    {
        #region 属性
        /// <summary>
        /// 预约单支付成功
        /// </summary>
        event OrderPaySuccessed OnOrderPaySuccessed;
        #endregion

        #region 方法
        SKUInfo GetSkuByID(string skuid);

        /// <summary>
        /// 创建预约单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        List<OrderInfo> CreateOrder(OrderCreateModel model);

        void UpdateProductVistiOrderCount(long orderId);
        /// <summary>
        /// 确认0元预约单（使用积分抵扣为0）
        /// </summary>
        /// <param name="orders"></param>
        void ConfirmZeroOrder(IEnumerable<long> Ids, long userId);

        /// <summary>
        /// 删除预约单（使用积分抵扣会生成预约单，生成后用户可能会点击取消使用积分抵扣）
        /// </summary>
        void CancelOrders(IEnumerable<long> Ids, long userId);

        /// <summary>
        /// 获取预约单列表
        /// </summary>
        /// <param name="orderQuery"></param>
        /// <returns></returns>
        ObsoletePageModel<OrderInfo> GetOrders<Tout>(OrderQuery orderQuery, Expression<Func<OrderInfo, Tout>> sort = null);
        /// <summary>
        /// 分页获取预约单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<OrderInfo> GetOrders(OrderQuery query);

        /// <summary>
        /// 获取预约单列表(忽略分页)
        /// </summary>
        /// <param name="orderQuery"></param>
        /// <returns></returns>
        List<OrderInfo> GetOrdersNoPage(OrderQuery orderQuery);

        /// <summary>
        /// 获取增量预约单
        /// </summary>
        /// <param name="orderQuery"></param>
        /// <returns></returns>
        ObsoletePageModel<OrderInfo> GetOrdersByLastModifyTime(OrderQuery orderQuery);

        /// <summary>
        /// 获取一批指定的预约单
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        List<OrderInfo> GetOrders(IEnumerable<long> ids);

        /// <summary>
        /// 根据预约单id获取预约单项
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        List<OrderItemInfo> GetOrderItemsByOrderId(long orderId);

        /// <summary>
        /// 根据预约单id获取预约单项
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        List<OrderItemInfo> GetOrderItemsByOrderId(IEnumerable<long> orderIds);

        /// <summary>
        /// 获取预约单的评论数
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        Dictionary<long, int> GetOrderCommentCount(IEnumerable<long> orderIds);

        /// <summary>
        /// 根据预约单项id获取预约单项
        /// </summary>
        /// <param name="orderItemIds"></param>
        /// <returns></returns>
        List<OrderItemInfo> GetOrderItemsByOrderItemId(IEnumerable<long> orderItemIds);

        /// <summary>
        /// 根据预约单项id获取售后记录
        /// </summary>
        /// <param name="orderItemIds"></param>
        /// <returns></returns>
        List<OrderRefundInfo> GetOrderRefunds(IEnumerable<long> orderItemIds);

        decimal GetIntegralDiscountAmount(int integral, long userId);

        /// <summary>
        /// 获取某个用户的预约单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        OrderInfo GetOrder(long orderId, long userId);

        /// <summary>
        /// 获取某个用户的预约单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        OrderInfo GetOrder(long orderId);
        /// <summary>
        /// 根据提货码取预约单
        /// </summary>
        /// <param name="pickCode"></param>
        /// <returns></returns>
        OrderInfo GetOrderByPickCode(string pickCode);
        /// <summary>
        /// 获取诊疗项目已购数
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        Dictionary<long, int> GetProductBuyCount(long userId, IEnumerable<long> productIds);

        /// <summary>
        /// 是否存在预约单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopId">诊所Id,0表示不限诊所</param>
        /// <returns></returns>
        bool IsExistOrder(long orderId, long shopId = 0);
        /// <summary>
        /// 平台确认预约单收款
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="payRemark"></param>
        /// <param name="managerName"></param>
        void PlatformConfirmOrderPay(long orderId, string payRemark, string managerName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderIds">预约单Id</param>
        /// <param name="paymentId">支付方式Id</param>
        /// <param name="payNo">支付流水号</param>
        /// <param name="payTime">支付时间</param>
        void PaySucceed(IEnumerable<long> orderIds, string paymentId, DateTime payTime, string payNo = null, long payId = 0);

        void PayCapital(IEnumerable<long> orderIds, string payNo = null, long payId = 0);

        bool PayByCapitalIsOk(long userid, IEnumerable<long> orderIds);
        /// <summary>
        /// 平台取消预约单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="managerName"></param>
        void PlatformCloseOrder(long orderId, string managerName, string CloseReason = "");

        /// <summary>
        /// 诊所取消预约单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="sellerName"></param>
        void SellerCloseOrder(long orderId, string sellerName);

        /// <summary>
        /// 诊所修改预约单收货地址
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="sellerName"></param>
        /// <param name="shipTo"></param>
        /// <param name="cellPhone"></param>
        /// <param name="topRegionId"></param>
        /// <param name="regionId"></param>
        /// <param name="regionFullName"></param>
        /// <param name="address"></param>
        void SellerUpdateAddress(long orderId, string sellerName, string shipTo, string cellPhone, int topRegionId, int regionId, string regionFullName, string address);

        /// <summary>
        /// 诊所修改预约单诊疗项目的优惠金额
        /// </summary>
        /// <param name="orderItemId"></param>
        /// <param name="discountAmount"></param>
        /// <param name="sellerName"></param>
        void SellerUpdateItemDiscountAmount(long orderItemId, decimal discountAmount, string sellerName);


        /// <summary>
        /// 诊所修改预约单的运费
        /// </summary>
        /// <param name="roderId"></param>
        /// <param name="Freight"></param>
        void SellerUpdateOrderFreight(long orderId, decimal freight);

        /// <summary>
        /// 诊所发货
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="companyName"></param>
        /// <param name="shipOrderNumber"></param>
        OrderInfo SellerSendGood(long orderId, string sellerName, string companyName, string shipOrderNumber);

        /// <summary>
        /// 门店发货
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="deliveryType"></param>
        /// <param name="shopkeeperName"></param>
        /// <param name="companyName"></param>
        /// <param name="shipOrderNumber"></param>
        /// <returns></returns>
        OrderInfo ShopSendGood(long orderId, int deliveryType, string shopkeeperName, string companyName, string shipOrderNumber);

        bool IsOrderAfterService(long orderId);
        /// <summary>
        /// 修改快递信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="companyName"></param>
        /// <param name="shipOrderNumber"></param>
        /// <returns></returns>
        OrderInfo UpdateExpress(long orderId, string companyName, string shipOrderNumber);

        /// <summary>
        /// 会员取消预约单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="memberName"></param>
        void MemberCloseOrder(long orderId, string memberName);

        /// <summary>
        /// 会员确认预约单收货
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="memberName"></param>
        void MembeConfirmOrder(long orderId, string memberName);
        /// <summary>
        /// 门店核销预约单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="managerName"></param>
        void ShopBranchConfirmOrder(long orderId, long shopBranchId, string managerName);
        /// <summary>
        /// 设置预约单物流信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="expressName"></param>
        /// <param name="startCode"></param>
        /// <param name="orderIds"></param>
        void SetOrderExpressInfo(long shopId, string expressName, string startCode, IEnumerable<long> orderIds);
        /// <summary>
        /// 设置预约单诊所备注
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="mark"></param>
        void SetOrderSellerRemark(long orderId, string mark);

        IQueryable<OrderInfo> GetTopOrders(int top, long userId);

        int GetFightGroupOrderByUser(long userId);

        /// <summary>
        /// 更新预约单数
        /// </summary>
        /// <param name="userId">会员Id</param>
        /// <param name="addOrderCount">变更预约单数(正数表示增加，负数表示减少）</param>
        /// <param name="addOrderAmount">变量预约单金额(正数表示增加，负数表示减少）</param>
        void UpdateMemberOrderInfo(long userId, decimal addOrderAmount = 0, int addOrderCount = 1);

        /// <summary>
        /// 获取指定诊疗项目最近一个月的平均成交价格
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        decimal GetRecentMonthAveragePrice(long shopId, long productId);

        /// <summary>
        /// 根据诊疗项目编号、状态 获取预约单成交笔数
        /// </summary>
        /// <param name="productId">诊疗项目编号</param>
        /// <param name="orserStatus">预约单状态</param>
        /// <returns></returns>
        int GetSuccessOrderCountByProductID(long productId = 0, OrderInfo.OrderOperateStatus orserStatus = OrderInfo.OrderOperateStatus.Finish);
        /// <summary>
        /// 计算预约单条目可退款金额
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="isCompel">是否强制计算</param>
        void CalculateOrderItemRefund(long orderId, bool isCompel = false);
        /// <summary>
        /// 诊所同意退款，关闭预约单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="managerName"></param>
        void AgreeToRefundBySeller(long orderId);
        /// <summary>
        /// 过期自动确认预约单
        /// </summary>
        void AutoConfirmOrder();

        /// <summary>
        /// 过期自动关闭预约单
        /// </summary>
        void AutoCloseOrder();
        /// <summary>
        /// 读取销量
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="shopId"></param>
        /// <param name="productId"></param>
        /// <param name="hasReturnCount"></param>
        /// <returns></returns>
        long GetSaleCount(DateTime? startDate = null, DateTime? endDate = null, long? shopBranchId = null, long? shopId = null, long? productId = null, bool hasReturnCount = false);
        /// <summary>
        /// 获取发票列表
        /// </summary>
        /// <param name="PageNo"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        QueryPageModel<InvoiceContextInfo> GetInvoiceContexts(int PageNo, int PageSize = 20);

        /// <summary>
        /// 获取发票列表
        /// </summary>
        /// <returns></returns>
        List<InvoiceContextInfo> GetInvoiceContexts();

        void SaveInvoiceContext(InvoiceContextInfo info);

        void DeleteInvoiceContext(long id);

        List<InvoiceTitleInfo> GetInvoiceTitles(long userid);

        long SaveInvoiceTitle(InvoiceTitleInfo info);

        void DeleteInvoiceTitle(long id, long userId = 0);

        /// <summary>
        /// 根据支付预约单号的取预约单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IQueryable<OrderPayInfo> GetOrderPay(long id);

        /// <summary>
        /// 保存支付预约单信息，生成支付预约单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        long SaveOrderPayInfo(IEnumerable<OrderPayInfo> model, PlatformType platform);

        /// <summary>
        /// 根据预约单id获取OrderPayInfo
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        List<OrderPayInfo> GetOrderPays(IEnumerable<long> orderIds);

        //TODO LRL 2015/08/06 添加获取子预约单对象的方法
        /// <summary>
        /// 获取子预约单对象
        /// </summary>
        /// <param name="orderItemId"></param>
        /// <returns></returns>
        OrderItemInfo GetOrderItem(long orderItemId);

        void MemberApplyCloseOrder(long orderId, string memberName, bool isBackStock = false);
        /// <summary>
        /// 是否超过售后期
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        bool IsRefundTimeOut(long orderId);

        /// <summary>
        /// 是否超过售后期
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        bool IsRefundTimeOut(OrderInfo order);

        /// <summary>
        /// 获取昨天预约单交易金额
        /// </summary>
        /// <param name="shopId">诊所ID平台不需要填写</param>
        /// <returns></returns>
        decimal GetYesterDaySaleAmount(long? shopId = null);

        /// <summary>
        /// 昨天下单预约单数
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        int GetYesterDayOrdersNum(long? shopId = null);

        /// <summary>
        /// 昨天付款预约单数
        /// </summary>
        /// <returns></returns>
        int GetYesterDayPayOrdersNum(long? shopId = null);
        /// <summary>
        /// 预约单完成预约单数据写入待结算表
        /// </summary>
        /// <param name="o"></param>
        void WritePendingSettlnment(OrderInfo o);

        /// <summary>
        /// 诊所给预约单备注
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="reMark"></param>
        void UpdateSellerRemark(long orderId, long shopId, string reMark, int flag);
        /// <summary>
        /// 分配诊所预约单到门店，更新诊所、门店库存
        /// </summary>
        /// <param name="skuIds"></param>
        /// <param name="quantity"></param>
        void DistributionStoreUpdateStock(List<string> skuIds, List<int> counts, long shopBranchId);
        void EndOrder(long orderId, string userName);

        /// <summary>
        /// 分配门店预约单到新门店
        /// </summary>
        /// <param name="skuIds"></param>
        /// <param name="newShopBranchId"></param>
        /// <param name="oldShopBranchId"></param>
        void DistributionStoreUpdateStockToNewShopBranch(List<string> skuIds, List<int> counts, long newShopBranchId, long oldShopBranchId);
        /// <summary>
        /// 分配门店预约单回到诊所
        /// </summary>
        /// <param name="skuIds"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="shopId"></param>
        void DistributionStoreUpdateStockToShop(List<string> skuIds, List<int> counts, long shopBranchId);
        /// <summary>
        /// 更新预约单所属门店
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopBranchId"></param>
        void UpdateOrderShopBranch(long orderId, long shopBranchId);
        #endregion
    }
}
