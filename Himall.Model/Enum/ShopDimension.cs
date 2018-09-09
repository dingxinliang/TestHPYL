using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public enum ShopDimension : int
    {
        /// <summary>
        /// 预约单量
        /// </summary>
        [Description("预约单量")]
        OrderCount = 1,

        /// <summary>
        /// 使用额
        /// </summary>
        [Description("使用额")]
        Sales,
    }
}
