using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Model
{
    public class PlatConsoleModel
    {
        /// <summary>
        /// 今日使用总额
        /// </summary>
        public decimal? TodaySaleAmount { set; get; }

        /// <summary>
        /// 今日会员增长量
        /// </summary>
        public long TodayMemberIncrease { set; get; }

        /// <summary>
        /// 诊所总数
        /// </summary>
        public long ShopNum { set; get; }

        /// <summary>
        /// 今日诊所新增数
        /// </summary>
        public long TodayShopIncrease { set; get; }

        /// <summary>
        /// 昨日新增诊所数
        /// </summary>
        public long YesterdayShopIncrease { set; get; }
        /// <summary>
        /// 待审核诊所数
        /// </summary>
        public long WaitAuditShops { set; get; }
        /// <summary>
        /// 待审核诊所数
        /// </summary>
        public long WaitConfirmShops { set; get; }
        /// <summary>
        /// 到期诊所数
        /// </summary>
        public long ExpiredShops { set; get; }

        /// <summary>
        /// 诊所提现数
        /// </summary>
        public int ShopCashNumber { set; get; }

        /// <summary>
        /// 诊疗项目总数
        /// </summary>
        public long ProductNum { set; get; }
        /// <summary>
        /// 在用诊疗项目数
        /// </summary>
        public int OnSaleProducts { set; get; }

        /// <summary>
        /// 待审核诊疗项目数
        /// </summary>
        public int WaitForAuditingProducts { set; get; }

        /// <summary>
        /// 待审核品牌数
        /// </summary>
        public int WaitForAuditingBrands { set; get; }

          /// <summary>
        /// 诊疗项目评论数
        /// </summary>
        public long ProductComments { set; get; }

        /// <summary>
        /// 诊疗项目咨询数
        /// </summary>
        public long ProductConsultations { set; get; }

        /// <summary>
        /// 待付款交易
        /// </summary>
        public long WaitPayTrades { set; get; }

        /// <summary>
        /// 待发货交易数
        /// </summary>
        public long WaitDeliveryTrades { set; get; }

        /// <summary>
        /// 退款退货数
        /// </summary>
        public long RefundTrades { set; get; }
        /// <summary>
        /// 待处理退款
        /// </summary>
        public long OrderWithRefundAndRGoods { get; set; }

        /// <summary>
        /// 投诉数量
        /// </summary>
        public long Complaints { set; get; }


      //待处理提现数量
        public long Cash { set; get; }

        /// <summary>
        /// 待发货礼品数量
        /// </summary>
        public long GiftSend { set; get; }
        /// <summary>
        /// 预约单总数
        /// </summary>
        public int OrderCounts { set; get; }
    }
}