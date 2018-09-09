using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 诊所收支明细查询实体
    /// </summary>
    public class ShopAccountItemQuery:BaseQuery
    {
        /// <summary>
        /// 诊所ID
        /// </summary>
        public long? ShopId { set; get; }
		/// <summary>
		/// 诊所名称
		/// </summary>
		public string ShopName { get; set; }
        /// <summary>
        /// 诊所收支类型
        /// </summary>
        public ShopAccountType? ShopAccountType { set; get; }

        /// <summary>
        /// 是否收入
        /// </summary>
        public bool? IsIncome { set; get; }


        /// <summary>
        /// 收支时间区间起始
        /// </summary>

        public DateTime? TimeStart { set; get; }

        /// <summary>
        /// 收支时间区间结束
        /// </summary>
        public DateTime? TimeEnd { set; get; }

    }

    /// <summary>
    /// 平台收支明细查询实体
    /// </summary>
    public class PlatAccountItemQuery : BaseQuery
    {
        /// <summary>
        /// 诊所
        /// </summary>
        public long? ShopId { get; set; }
        /// <summary>
        /// 诊所名
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public PlatAccountType? PlatAccountType { set; get; }

        /// <summary>
        /// 收支时间区间起始
        /// </summary>

        public DateTime? TimeStart { set; get; }

        /// <summary>
        /// 收支时间区间结束
        /// </summary>
        public DateTime? TimeEnd { set; get; }
    }
}
