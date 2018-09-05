﻿using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Web.Models;

namespace Himall.Web.Areas.Mobile.Models
{
    public class ShopCartItemModel
    {
        public ShopCartItemModel()
        {
            CartItemModels = new List<CartItemModel>();
        }
        public long shopId { set; get; }

        public long vshopId { set; get; }
        public string ShopName { set; get; }

        public decimal Freight { set; get; }
        public bool isFreeFreight { get; set; }
        public decimal shopFreeFreight { get; set; }
        /// <summary>
        /// 店铺预约单金额
        /// </summary>
        public decimal OrderAmount { get; set; }
        public IBaseCoupon Coupon { get; set; }
        /// <summary>
        /// 是否自营店
        /// </summary>
        public bool IsSelf { get; set; }

        public List<CartItemModel> CartItemModels { set; get; }

    }
}