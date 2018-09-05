﻿using System;
using System.Collections.Generic;

namespace Himall.Model
{
    public class ShoppingCartInfo
    {

        public ShoppingCartInfo()
        {
            Items = new ShoppingCartItem[] { };
        }

        /// <summary>
        /// 购物车 诊疗项目项
        /// </summary>
        public IEnumerable<ShoppingCartItem> Items { get; set; }

        /// <summary>
        /// 会员Id
        /// </summary>
        public long MemberId { get; set; }

    }

    /// <summary>
    /// 购物车诊疗项目项
    /// </summary>
    public class ShoppingCartItem
    {
        /// <summary>
        /// 购物车诊疗项目项Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 诊疗项目Id
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// 诊疗项目SKUID
        /// </summary>
        public string SkuId { get; set; }

        /// <summary>
        /// 诊疗项目数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 门店编号
        /// </summary>
        public long? ShopBranchId { get; set; }
        
        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime AddTime { get; set; }
    }


}
