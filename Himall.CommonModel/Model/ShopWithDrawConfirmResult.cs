using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonMode
{
    public class ShopWithDrawConfirmResult
    {
        /// <summary>
        /// 成功状态
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 跳转地址
        /// </summary>
        public string JumpUrl { get; set; }
    }
}
