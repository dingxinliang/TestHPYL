using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 提现方式
    /// </summary>
    public enum WithdrawType
    {
        /// <summary>
        /// 微信
        /// </summary>
        [Description("微信")]
        WeiChat = 1,
        /// <summary>
        /// 银行卡
        /// </summary>
        [Description("银行卡")]
        BankCard = 2,
        /// <summary>
        /// 支付宝
        /// </summary>
        [Description("支付宝")]
        ALipay = 3,
    }
}
