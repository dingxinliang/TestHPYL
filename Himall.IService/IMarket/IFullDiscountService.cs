using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices.QueryModel;
using Himall.CommonModel;
using Himall.Model;

namespace Himall.IServices
{
    /// <summary>
    /// 满减服务
    /// </summary>
    public interface IFullDiscountService : IService
    {
        #region 满减活动操作
        /// <summary>
        /// 新增满减活动
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rules"></param>
        /// <param name="products"></param>
        void AddActive(ActiveInfo data, IEnumerable<FullDiscountRulesInfo> rules, IEnumerable<ActiveProductInfo> products);
        /// <summary>
        /// 更新满减活动
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rules"></param>
        /// <param name="products"></param>
        void UpdateActive(ActiveInfo data, IEnumerable<FullDiscountRulesInfo> rules, IEnumerable<ActiveProductInfo> products);
        /// <summary>
        /// 删除满减活动
        /// </summary>
        /// <param name="id"></param>
        void DeleteActive(long id);
        #endregion

        #region 满减活动查询
        /// <summary>
        /// 诊疗项目是否可以参加满减活动
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="activeId">添加活动使用0</param>
        /// <returns></returns>
        bool ProductCanJoinActive(long productId,long activeId);
        /// <summary>
        /// 过滤活动诊疗项目编号
        /// <para>返回可以能加商动的诊疗项目</para>
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="activeId">添加活动使用0</param>
        /// <param name="shopId">店铺编号</param>
        /// <returns></returns>
        List<long> FilterActiveProductId(IEnumerable<long> productIds, long activeId, long shopId);
        /// <summary>
        /// 根据诊疗项目ID和店铺ID取正在参与且进行中的活动信息
        /// </summary>
        /// <param name="proId"></param>
        /// <returns></returns>
        ActiveInfo GetOngoingActiveByProductId(long proId, long shopId);
        /// <summary>
        /// 获取满减活动
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ActiveInfo GetActive(long id);
        /// <summary>
        /// 获取满减优惠阶梯
        /// </summary>
        /// <param name="activeId"></param>
        /// <returns></returns>
        List<FullDiscountRulesInfo> GetActiveRules(long activeId);
        /// <summary>
        /// 获取满减诊疗项目
        /// </summary>
        /// <param name="activeId"></param>
        /// <returns></returns>
        List<ActiveProductInfo> GetActiveProducts(long activeId);
        /// <summary>
        /// 获取活动列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<ActiveInfo> GetActives(FullDiscountActiveQuery query);
        /// <summary>
        /// 获取活动诊疗项目数量聚合
        /// </summary>
        /// <param name="activeId"></param>
        /// <returns></returns>
        List<FullDiscountProductCountAggregate> GetActivesProductCountAggregate(IEnumerable<long> activeId);



        /// <summary>
        /// 获取某个店铺正在进行的满额减活动列表
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        List<ActiveInfo> GetOngoingActiveByShopId(long shopId);

        /// <summary>
        /// 获取某个店铺的一批诊疗项目正在进行的满额减活动
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        List<ActiveInfo> GetOngoingActiveByProductIds(IEnumerable<long> productIds, long shopId);
        #endregion

        #region 其他功能
        /// <summary>
        /// 获取可以参与活动的诊疗项目列表
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="productName"></param>
        /// <param name="productCode"></param>
        /// <param name="selectedProductIds"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        QueryPageModel<ProductInfo> GetCanJoinProducts(long shopId
            , string productName = null, string productCode = null
            , IEnumerable<long> selectedProductIds = null, int activeId = 0
            , int page = 1, int pagesize = 10);
        /// <summary>
        /// 是否可以操作(添加/修改)活动
        /// </summary>
        /// <param name="active"></param>
        /// <param name="products"></param>
        /// <returns></returns>
        bool CanOperationActive(ActiveInfo active, IEnumerable<ActiveProductInfo> products);
        /// <summary>
        /// 获取不在销售中的诊疗项目
        /// </summary>
        /// <param name="productIds"></param>
        /// <returns></returns>
        List<long> GetNoSaleProductId(IEnumerable<long> productIds);
        #endregion

        /// <summary>
        /// 提交保存
        /// </summary>
        void Commit();
    }
}
