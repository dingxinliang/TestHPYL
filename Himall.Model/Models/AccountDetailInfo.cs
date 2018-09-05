using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Himall.Model
{
    public partial class AccountDetailInfo
    {
        /// <summary>
        /// 预约单状态
        /// </summary>
        public enum EnumOrderType
        {
            /// <summary>
            /// 退预约单
            /// </summary>
            [Description("退单列表")]
            ReturnOrder = 0,

            /// <summary>
            /// 已完成
            /// </summary>
            [Description("预约单列表")]
            FinishedOrder
        }

       
    }
}
