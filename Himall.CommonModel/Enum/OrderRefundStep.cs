using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel.Enum
{
	/// <summary>
	/// 退款步聚
	/// </summary>
	public enum OrderRefundStep:short
	{
		/// <summary>
		/// 待诊所/门店审核
		/// </summary>
		[Description("待诊所审核")]
		WaitAudit = 1,

		/// <summary>
		/// 待患者寄货
		/// </summary>
		[Description("待患者寄货")]
		WaitDelivery = 2,

		/// <summary>
		/// 待诊所/门店收货
		/// </summary>
		[Description("待诊所收货")]
		WaitReceiving = 3,

		/// <summary>
		/// 诊所/门店拒绝
		/// </summary>
		[Description("诊所拒绝")]
		UnAudit = 4,

		/// <summary>
		/// 待平台确认
		/// </summary>
		[Description("待平台确认")]
		UnConfirm = 5,

		/// <summary>
		/// 退款成功
		/// </summary>
		[Description("退款成功")]
		Confirmed = 6
	}
}
