using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.DTO
{
    /// <summary>
    /// 预约单积分
    /// </summary>
    public class OrderIntegralModel
    {
        public OrderIntegralModel()
        {
            IntegralPerMoney = 0;
            UserIntegrals = 0;
        }

        /// <summary>
        /// 多少积分换一无
        /// </summary>
        public decimal IntegralPerMoney { get; set; }

        /// <summary>
        /// 预约单需要使用积分
        /// </summary>
        public decimal UserIntegrals { get; set; } 
    }
}