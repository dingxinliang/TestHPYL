﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{

    /// <summary>
    /// 使用指标汇总
    /// </summary>
    public class SaleStatistics
    {
        /// <summary>
        /// 使用件数
        /// </summary>
        public int SaleCount { get; set; }
        /// <summary>
        /// 成交笔数
        /// </summary>
        public int DealCount { get; set; }

        private decimal _SalesVolume = 0;
        /// <summary>
        /// 使用额
        /// </summary>
        public decimal SalesVolume { get { return Math.Round(_SalesVolume, 2); } set { _SalesVolume = value; } }

        private decimal _OrderPrice { get; set; }
        /// <summary>
        /// 客单价(预约单平均金额)
        /// </summary>
        public decimal OrderPrice { get { return Math.Round(_OrderPrice, 2); } set { _OrderPrice = value; } }

        private decimal _OrderItemPrice = 0;
        /// <summary>
        /// 件单价 (诊疗项目平均售价)
        /// </summary>
        public decimal OrderItemPrice { get { return Math.Round(_OrderItemPrice, 2); } set { _OrderItemPrice = value; } }

        private decimal _OrderAverage = 0;
        /// <summary>
        /// 连带率(预约单平均诊疗项目数量)
        /// </summary>
        public decimal OrderAverage { get { return Math.Round(_OrderAverage, 2); } set { _OrderAverage = value; } }

        /// <summary>
        /// 退款率
        /// </summary>
        public decimal RefundRate { get; set; }

    }
}
