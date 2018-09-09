using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 诊所财务总览
    /// </summary>
    public class ShopBillingIndex
    {

        /// <summary>
        /// 诊所帐户
        /// </summary>
        public ShopAccount ShopAccout { set; get; }

        /// <summary>
        /// 昨日使用额
        /// </summary>
        public decimal YesterDaySaleAmount { set; get; }

        /// <summary>
        /// 昨日下单总数
        /// </summary>
        public int YesterDayOrders { set; get; }

        /// <summary>
        /// 昨日付款预约单数
        /// </summary>
        public int YesterDayPayOrders { set; get; }

    }
}
