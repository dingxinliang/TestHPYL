using Himall.Model;
using System;
using System.Collections.Generic;

namespace Himall.IServices.QueryModel
{
    public partial class RefundQuery : QueryBase
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public OrderRefundInfo.OrderRefundAuditStatus? AuditStatus { get; set; }

        public OrderRefundInfo.OrderRefundConfirmStatus? ConfirmStatus { get; set; }

        public long? OrderId { get; set; }

        public long? ShopId { get; set; }

        public string ShopName { get; set; }

        public string ProductName { set; get; }

        public long? UserId { get; set; }

        public string UserName { get; set; }
        /// <summary>
        /// 多个预约单编号
        /// <para>补充OrderId使用</para>
        /// </summary>
        public List<long> MoreOrderId { get; set; }
        /// <summary>
        /// 预约单退款类型
        /// <para>0 所有 1 预约单退款 2 仅退款(包含预约单退款) 3 退货 4 仅退款</para>
        /// </summary>
        public int? ShowRefundType { get; set; }


		public long? ShopBranchId { get; set; }
	}
}
