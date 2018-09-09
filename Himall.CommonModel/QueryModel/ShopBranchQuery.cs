﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 门店查询参数
    /// </summary>
    public class ShopBranchQuery:BaseQuery
    {
        /// <summary>
        /// 门店标签编号
        /// </summary>
        public long? ShopBranchTagId { get; set; }
        /// <summary>
        /// 门店名字
        /// </summary>
        public string ShopBranchName { get; set; }
        /// <summary>
        /// 联系方式
        /// </summary>
        public string ContactPhone { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string ContactUser { get; set; }

		public int? AddressId { get; set; }
        /// <summary>
        /// 诊所诊所ID
        /// </summary>
		public long ShopId { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
		public string AddressPath { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
		public ShopBranchStatus? Status { get; set; }

        public ShopBranchSkuStatus? ShopBranchProductStatus { get; set; }
		/// <summary>
		/// 是否代理这些诊疗项目
		/// </summary>
		public long[] ProductIds { get; set; }
        /// <summary>
        /// 患者当前位置经纬度/患者收货地址经纬度。用半角逗号分隔:28.1657,112.434
        /// </summary>
        public string FromLatLng { get; set; }
        /// <summary>
        /// 省份
        /// </summary>
        public int ProvinceId { get; set; }
        /// <summary>
        /// 患者当前位置所在城市ID/患者收货地址市ID
        /// </summary>
        public int CityId { get; set; }
        /// <summary>
        /// 患者收货地址区ID
        /// </summary>
        public int DistrictId { get; set; }
        /// <summary>
        /// 患者收货地址街道ID
        /// </summary>
        public int StreetId { get; set; }
        public long Id { get; set; }
        /// <summary>
        /// ture升,false降
        /// </summary>
        public bool OrderType { get; set; }
        /// <summary>
        /// 排序关键字/* 排序项（1：默认，2：距离） */
        /// </summary>
        public int OrderKey { get; set; }
        /// <summary>
        /// 是否推荐
        /// </summary>
        public bool? IsRecommend { get; set; }
    }
}
