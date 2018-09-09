﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 预约单结算详情
    //结算金额=诊疗项目实付+运费-平台佣金-分佣佣金-退款金额+平台佣金退还+分佣佣金退还
    /// </summary>
    public class OrderSettlementDetail
    {
        /// <summary>
        /// 预约单支付时间
        /// </summary>
        public string OrderPayTime { set; get; }

        /// <summary>
        /// 诊疗项目实付
        /// </summary>
        public decimal ProductsTotal { set; get; }

        /// <summary>
        /// 预约单运费
        /// </summary>
        public decimal Freight { set; get; }

        /// <summary>
        /// 平台佣金
        /// </summary>
        public decimal PlatCommission { set; get; }

        /// <summary>
        /// 分佣佣金
        /// </summary>
        public decimal DistributorCommission { set; get; }

        /// <summary>
        /// 开团奖励
        /// </summary>
        public decimal OpenCommission { set; get; }
        /// <summary>
        /// 成团返现
        /// </summary>
        public decimal JoinCommission { set; get; }

        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal RefundAmount { set; get; }
         /// <summary>
        /// 平台佣金退还
        /// </summary>
        public decimal PlatCommissionReturn { set; get; }
         
        /// <summary>
        /// 分佣佣金退还
        /// </summary>
        public decimal DistributorCommissionReturn { set; get; }
        /// <summary>
        /// 退款确认时间
        /// </summary>
        public string OrderRefundTime { set; get; }

    }
}
