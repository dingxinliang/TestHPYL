using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 提现状态
    /// </summary>
    public enum WithdrawStaus
    {
        /// <summary>
        /// 待处理
        /// </summary>
        [Description("待处理")]
        WatingAudit = 1,
        /// <summary>
        /// 拒绝提现
        /// </summary>
        [Description("拒绝提现")]
        Refused = 2,
        /// <summary>
        /// 提现完成
        /// </summary>
        [Description("提现完成")]
        Succeed = 3,
        /// <summary>
        /// 提现失败
        /// </summary>
        [Description("提现失败")]
        Fail = 4,
        /// <summary>
        /// 付款中
        /// </summary>
        [Description("付款中")]
        PayPending = 5,
    }
}
