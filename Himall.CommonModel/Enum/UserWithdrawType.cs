using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    public enum UserWithdrawType
    {
        /// <summary>
        /// 微信支付
        /// </summary>
        [Description("微信")] 
        WeiChat = 1,
        /// <summary>
        /// 支付宝支付
        /// </summary>
        [Description("支付宝")] 
        ALiPay = 3,
    }
}
