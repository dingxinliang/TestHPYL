using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel.Delegates
{
	/// <summary>
	/// 预约单支付成功
	/// </summary>
	/// <param name="orderId">预约单id</param>
	public delegate void OrderPaySuccessed(long orderId);

	/// <summary>
	/// 退款成功
	/// </summary>
	/// <param name="refundId">退款id</param>
	public delegate void RefundSuccessed(long refundId);
}
