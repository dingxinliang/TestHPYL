﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;
using Himall.CommonModel;

namespace Himall.DTO
{
    public class FightGroupActiveResult
    {
        public FightGroupActiveResult()
        {
            ProductImages = new List<string>();
        }
        /// <summary>
        /// 是否已初始诊疗项目图片信息
        /// </summary>
        private bool isInitImagesed { get; set; }

        #region 属性
        /// <summary>
        /// 编号
        ///</summary>
        public long Id { get; set; }
        /// <summary>
        /// 店铺编号
        ///</summary>
        public long ShopId { get; set; }
        /// <summary>
        /// 诊疗项目编号
        ///</summary>
        public long? ProductId { get; set; }
        /// <summary>
        /// 诊疗项目名称
        ///</summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 图片
        ///</summary>
        public string IconUrl { get; set; }
        /// <summary>
        /// 开始时间
        ///</summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 结束时间
        ///</summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 参团人数限制
        ///</summary>
        public int? LimitedNumber { get; set; }
        /// <summary>
        /// 成团时限
        ///</summary>
        public decimal? LimitedHour { get; set; }
        /// <summary>
        /// 数量限制
        ///</summary>
        public int? LimitQuantity { get; set; }
        /// <summary>
        /// 组团数量
        /// </summary>
        public int? GroupCount { get; set; }
        /// <summary>
        /// 成团数量
        /// </summary>
        public int? OkGroupCount { get; set; }
        /// <summary>
        /// 活动添加时间
        ///</summary>
        public DateTime? AddTime { get; set; }
        /// <summary>
        /// 活动项
        /// </summary>
        public List<FightGroupActiveItemModel> ActiveItems { get; set; }

        /// <summary>
        ///活动是否结束
        /// </summary>
        public bool IsEnd { get; set; }
        /// <summary>
        /// 管理审核状态
        /// </summary>
        public FightGroupManageAuditStatus FightGroupManageAuditStatus { get; set; }
        /// <summary>
        /// 下架原因
        /// </summary>
        public string ManageRemark { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public Nullable<System.DateTime> ManageDate { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public Nullable<long> ManagerId { get; set; }

        /// <summary>
        /// 诊疗项目是否还有库存
        /// </summary>
        public bool HasStock { get; set; }
        /// <summary>
        /// 拼团活动状态
        /// </summary>
        public FightGroupActiveStatus ActiveStatus { get; set; }
        /// <summary>
        /// 活动状态显示名称
        /// </summary>
        public string ShowActiveStatus { get; set; }

        #region 诊疗项目信息补充
        /// <summary>
        /// 诊疗项目图片目录
        /// </summary>
        public string ProductImgPath { get; set; }
        /// <summary>
        /// 诊疗项目默认图片
        /// </summary>
        public string ProductDefaultImage { get; set; }
        /// <summary>
        /// 诊疗项目其他图片
        /// </summary>
        public List<string> ProductImages { get; set; }
        /// <summary>
        /// 运费模板
        /// </summary>
        public long FreightTemplateId { get; set; }
        /// <summary>
        /// 诊疗项目评价数
        /// </summary>
        public int ProductCommentNumber { get; set; }
        /// <summary>
        /// 诊疗项目广告语
        /// </summary>
        public string ProductShortDescription { get; set; }
        /// <summary>
        /// 诊疗项目编码
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// 诊疗项目单位
        /// </summary>
        public string MeasureUnit { get; set; }
        /// <summary>
        /// 诊疗项目是否可购买
        /// </summary>
        public bool CanBuy { get; set; }
        #endregion

        /// <summary>
        /// 火拼价
        /// </summary>
        public decimal MiniGroupPrice { get; set; }
        /// <summary>
        /// 最低售价
        /// </summary>
        public decimal MiniSalePrice { get; set; }
        #endregion
    }
}
