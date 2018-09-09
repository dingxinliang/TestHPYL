using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public enum SaleDimension : int
    {
        /// <summary>
        /// 预约单量
        /// </summary>
        [Description("使用量")]
        SaleCount = 1,

        /// <summary>
        /// 使用额
        /// </summary>
        [Description("使用额")]
        Sales,
    }
}
