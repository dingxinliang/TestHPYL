using Himall.CommonModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface IStatisticsService : IService
    {
        #region 平台

        #region 会员图表

        /// <summary>
        /// 获取按星期为时间跨度的图表数据
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <param name="weekIndex">星期序号</param>
        /// <returns></returns>
        LineChartDataModel<int> GetMemberChart(int year, int month, int weekIndex);

        /// <summary>
        /// 获取按月为时间跨度的图表数据
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns></returns>
        LineChartDataModel<int> GetMemberChart(int year, int month);

        /// <summary>
        /// 获取按日为时间跨度的图表数据
        /// </summary>
        /// <param name="day">指定的某一天</param>
        /// <returns></returns>
        LineChartDataModel<int> GetMemberChart(DateTime day);

        #endregion

        #region 区域分析地图

        MapChartDataModel GetAreaOrderChart(OrderDimension dimension, int year, int month);

        #endregion

        #region  诊所统计

        
        /// <summary>
        /// 获取新增诊所图表
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns></returns>
        LineChartDataModel<int> GetNewsShopChart(int year, int month);

        /// <summary>
        /// 获取诊所排行图表,按月统计
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <param name="dimension">统计指标</param>
        /// <param name="rankSize">统计名次个数</param>
        /// <returns></returns>
        LineChartDataModel<int> GetShopRankingChart(int year, int month, ShopDimension dimension= ShopDimension.OrderCount,int rankSize=15);

        /// <summary>
        /// 获取诊所排行图表,按星期统计
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <param name="weekIndex">星期序号</param>
        /// <param name="dimension">统计指标</param>
        /// <param name="rankSize">统计名次个数</param>
        /// <returns></returns>
        LineChartDataModel<int> GetShopRankingChart(int year, int month, int weekIndex, ShopDimension dimension = ShopDimension.OrderCount, int rankSize = 15);

        /// <summary>
        /// 获取诊所排行图表,按天统计
        /// </summary>
        /// <param name="day"></param>
        /// <param name="dimension">统计指标</param>
        /// <param name="rankSize">统计名次个数</param>
        /// <returns></returns>
        LineChartDataModel<int> GetShopRankingChart(DateTime day, ShopDimension dimension = ShopDimension.OrderCount, int rankSize = 15);


        /// <summary>
        /// 近一个月的诊所销量排行 (控制台)
        /// </summary>
        /// <returns></returns>
        LineChartDataModel<int> GetRecentMonthShopSaleRankChart();
        #endregion

        #region 销量排行

        /// <summary>
        /// 获取使用排行图表,按月统计
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <param name="dimension">统计指标</param>
        /// <param name="rankSize">统计名次个数</param>
        /// <returns></returns>
        LineChartDataModel<int> GetSaleRankingChart(int year, int month, SaleDimension dimension = SaleDimension.SaleCount, int rankSize = 15);

        /// <summary>
        /// 获取使用排行图表,按星期统计
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <param name="weekIndex">星期序号</param>
        /// <param name="dimension">统计指标</param>
        /// <param name="rankSize">统计名次个数</param>
        /// <returns></returns>
        LineChartDataModel<int> GetSaleRankingChart(int year, int month, int weekIndex, SaleDimension dimension = SaleDimension.SaleCount, int rankSize = 15);

        /// <summary>
        /// 获取使用排行图表,按天统计
        /// </summary>
        /// <param name="day"></param>
        /// <param name="dimension">统计指标</param>
        /// <param name="rankSize">统计名次个数</param>
        /// <returns></returns>
        LineChartDataModel<int> GetSaleRankingChart(DateTime day, SaleDimension dimension = SaleDimension.SaleCount, int rankSize = 15);

        /// <summary>
        /// 近一个月的诊疗项目销量排行 （控制台）
        /// </summary>
        /// <returns></returns>
        LineChartDataModel<int> GetRecentMonthSaleRankChart();

        #endregion

        #endregion

        #region 诊所
        /// <summary>
        /// 获取一个月的诊所流量走势
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        LineChartDataModel<int> GetShopFlowChart(long shopId, int year, int month);

        /// <summary>
        /// 获取诊所中诊疗项目浏览量排行
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="day"></param>
        /// <param name="rankSize"></param>
        /// <returns></returns>
        LineChartDataModel<int> GetProductVisitRankingChart(long shopId, DateTime day, int rankSize = 15);

        /// <summary>
        /// 获取诊所中诊疗项目浏览量排行
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="rankSize"></param>
        /// <returns></returns>
        LineChartDataModel<int> GetProductVisitRankingChart(long shopId, int year, int month, int rankSize = 15);

        /// <summary>
        /// 获取诊所中诊疗项目浏览量排行
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="weekIndex"></param>
        /// <param name="rankSize"></param>
        /// <returns></returns>
        LineChartDataModel<int> GetProductVisitRankingChart(long shopId, int year, int month, int weekIndex, int rankSize = 15);


        /// <summary>
        /// 获取诊所中诊疗项目使用量排行
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="day"></param>
        /// <param name="dimension"></param>
        /// <param name="rankSize"></param>
        /// <returns></returns>
        LineChartDataModel<int> GetProductSaleRankingChart(long shopId, DateTime day, SaleDimension dimension = SaleDimension.SaleCount, int rankSize = 15);

        /// <summary>
        /// 获取诊所中诊疗项目使用量排行
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="dimension"></param>
        /// <param name="rankSize"></param>
        /// <returns></returns>
        LineChartDataModel<int> GetProductSaleRankingChart(long shopId, int year, int month, SaleDimension dimension = SaleDimension.SaleCount, int rankSize = 15);

        /// <summary>
        /// 获取诊所中诊疗项目使用量排行
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="weekIndex"></param>
        /// <param name="dimension"></param>
        /// <param name="rankSize"></param>
        /// <returns></returns>
        LineChartDataModel<int> GetProductSaleRankingChart(long shopId, int year, int month, int weekIndex, SaleDimension dimension = SaleDimension.SaleCount, int rankSize = 15);

        /// <summary>
        /// 获取诊所的总使用额
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        LineChartDataModel<int> GetShopSaleChart(long shopId, int year, int month);

        /// <summary>
        /// 最近15天的使用额
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        LineChartDataModel<int> GetRecentMonthSaleRankChart(long shopId);

        /// <summary>
        /// 获取成交转化率
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        LineChartDataModel<float> GetDealConversionRateChart(long shopId, int year, int month);

        #endregion
        /// <summary>
        /// 添加平台浏览量(人数 UV)
        /// </summary>
        /// <param name="dt"></param>
        void AddPlatVisitUser(DateTime dt,long num);
        /// <summary>
        /// /添加诊所浏览量(人数 UV)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="shopId"></param>
        void AddShopVisitUser(DateTime dt, long shopId, long num);
        /// <summary>
        /// 添加诊疗项目浏览量(人数 UV)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="pid"></param>
        void AddProductVisitUser(DateTime dt, long pid, long shopId, long num);
        /// <summary>
        /// 添加诊疗项目浏览量(PV)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="pid"></param>
        void AddProductVisit(DateTime dt, long pid, long shopId, long num);
        /// <summary>
        /// 取时间段内诊疗项目浏览记录
        /// </summary>
        /// <param name="startDt"></param>
        /// <param name="endDt"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        QueryPageModel<ProductStatisticModel> GetProductVisits(ProductStatisticQuery query);
        /// <summary>
        /// 取时间端内诊疗项目使用记录（不分页）
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        IEnumerable<ProductVistiInfo> GetProductAllVisits(ProductStatisticQuery query);
        /// <summary>
        /// 取时间段内诊所浏览记录
        /// </summary>
        /// <param name="startDt"></param>
        /// <param name="endDt"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        IEnumerable<ShopVistiInfo> GetShopVisits(DateTime startDt, DateTime endDt, long shopId = 0);
        /// <summary>
        /// 取时间段内平台访问记录
        /// </summary>
        /// <param name="startDt"></param>
        /// <param name="endDt"></param>
        /// <returns></returns>
        IEnumerable<PlatVisitsInfo> GetPlatVisits(DateTime startDt, DateTime endDt);
    }
}
