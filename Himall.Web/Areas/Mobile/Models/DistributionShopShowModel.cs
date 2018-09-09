using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;

namespace Himall.Web.Areas.Mobile.Models
{
    public class DistributionShopShowModel
    {
        public string ShopName { get; set; }
        public string SearchKey { get; set; }
        /// <summary>
        /// 用户编号
        /// <para>适应于使用员诊所</para>
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 诊所编号
        /// <para>适应于诊所聚合页</para>
        /// </summary>
        public long ShopId { get; set; }
        /// <summary>
        /// 合作编号
        /// <para>使用员编号</para>
        /// </summary>
        public long PartnerId { get; set; }


        /// <summary>
        /// 诊所分享logo图片
        /// </summary>
        public string ShopShareLogo { set; get; }


        /// <summary>
        /// 诊所分享logo图片
        /// </summary>
        public string ShopShareTitle { set; get; }


        /// <summary>
        /// 诊所分享详情
        /// </summary>
        public string ShopShareDesc { set; get; }

        /// <summary>
        /// 诊所图片
        /// </summary>
        public string ShopLogo { get; set; }
        /// <summary>
        /// 诊所二维码
        /// </summary>
        public string ShopQCode { get; set; }
        /// <summary>
        /// 诊所二维码网址
        /// </summary>
        public string ShopQCodeUrl { get; set; }
        /// <summary>
        /// 是否已收藏
        /// </summary>
        public bool isFavorite { get; set; }
    }
}