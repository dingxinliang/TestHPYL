using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// APP消息类型
    /// </summary>
    public enum AppMessagesType
    {
        /// <summary>
		/// 预约单
		/// </summary>
		[Description("预约单")]
        Order = 1,
        /// <summary>
		/// 售后
		/// </summary>
		[Description("售后")]
        AfterSale = 2
    }
}
