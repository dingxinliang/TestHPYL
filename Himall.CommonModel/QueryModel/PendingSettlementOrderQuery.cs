using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 待结算预约单查询实体
    /// </summary>
    public class PendingSettlementOrderQuery : BaseQuery
    {
       /// <summary>
       /// 预约单ID
       /// </summary>
        public long? OrderId { set; get; }

        /// <summary>
        /// 店铺ID
        /// </summary>
        public long? ShopId { set; get; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { set; get; }

        /// <summary>
        /// 支付方式
        /// </summary>
        public string PaymentName { set; get; }

        /// <summary>
        /// 预约单完成开始时间
        /// </summary>
        public DateTime? OrderStart { set; get; }
        /// <summary>
        /// 预约单完成结束时间
        /// </summary>
        public DateTime? OrderEnd { set; get; }
       
    }
}
