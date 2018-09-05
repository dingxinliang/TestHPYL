using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Himall.Model;
using Himall.CommonModel;

namespace Himall.Web.Models
{

  
    public class OrderModel
    {
        public long OrderId { set; get; }

        public string OrderStatus { get; set; }
        public string ShipTo { get; set; }
        public string OrderDate { get; set; }
        
        public long ShopId { set; get; }

        public string ShopName { set; get; }

        public long ShareUserId { get; set; }
        public long UserId { set; get; }

        public string UserName { set; get; }

        public decimal TotalPrice { get; set; }

        public string PaymentTypeName { get; set; }

        public int PlatForm { get; set; }

        public string PlatformText { get; set; }

        public string IconSrc { get; set; }

        public CommonModel.Enum.DeliveryType DeliveryType { get; set; }

        public int? RefundStats { get; set; }

        public string RefundStatusText { get; set; }

        public string PaymentTypeGateway { get; set; }

        public int OrderState { get; set; }

        public DateTime? PayDate { get; set; }

        public string PaymentTypeStr { get; set; }

        public string SellerAddress { get; set; }
        /// <summary>
        /// 预约时间
        /// </summary>
        public string doctorName { get; set; }
        public int RemindType { get; set; }
        public string  YYDate { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string ReceiveStartTime { get; set; }
        public string ReceiveEndTime { get; set; }

        /// <summary>
        ///诊所 备注
        /// </summary>

        public string SellerRemark { set; get; }

        public int? SellerRemarkFlag { set; get; }

        public OrderInfo.PaymentTypes PaymentType { get; set; }
        public OrderInfo.OrderTypes? OrderType { get; set; }
        /// <summary>
        /// 是否可以发货
        /// </summary>
        public bool CanSendGood { get; set; }
        /// <summary>
        /// 拼团状态
        /// <para>拼团预约单独有</para>
        /// </summary>
        public FightGroupOrderJoinStatus FightGroupJoinStatus { get; set; }
        /// <summary>
        /// 拼团是否可以退款
        /// </summary>
        public bool FightGroupCanRefund { get; set; }

		/// <summary>
		/// 支付流水号
		/// </summary>
		public string GatewayOrderId { get; set; }

		/// <summary>
		/// 收款人
		/// </summary>
		public string Payee { get; set; }

		public string RegionFullName { get; set; }

		public string Address { get; set; }

		public string UserRemark { get; set; }

		public string CellPhone { get; set; }

		public string ShopBranchName { get; set; }

		public string ShipOrderNumber { get; set; }

        public List<Himall.DTO.OrderItem> OrderItems { get; set; }
        /// <summary>
        /// 门店ID
        /// </summary>
        public long? ShopBranchId { get; set; }
        public int RegionId { get; set; }
    }
}