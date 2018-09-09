using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;

namespace Himall.Web.Areas.Web.Models
{
    public class CartCartModel
    {
        public CartCartModel()
        {
            Top3RecommendProducts = new List<ProductInfo>();
        }
        /// <summary>
        /// 最近一次交易的诊所的推荐诊疗项目
        /// </summary>
        public List<ProductInfo> Top3RecommendProducts { get; set; }
    }
}