using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Mobile.Models
{
    public class ProductGetDistributionInfoModel
    {
        /// <summary>
        /// 是否分佣诊疗项目
        /// </summary>
        public bool IsDistribution { get; set; }
        /// <summary>
        /// 佣金
        /// </summary>
        public decimal Brokerage { get; set; }
        /// <summary>
        /// 诊疗项目编号
        /// </summary>
        public long ProductId { get; set; }
        /// <summary>
        /// 分佣员会员编号
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 是否分佣员
        /// </summary>
        public bool IsPromoter { get; set; }
        /// <summary>
        /// 分佣员状态
        /// </summary>
        public PromoterInfo.PromoterStatus PromoterStatus { get; set; }
        /// <summary>
        /// 分享地址
        /// </summary>
        public string ShareUrl { get; set; }
        /// <summary>
        /// 微信分享参数
        /// </summary>
        public CommonModel.Model.WeiXinShareArgs WeiXinShareArgs { get; set; }
    }
}