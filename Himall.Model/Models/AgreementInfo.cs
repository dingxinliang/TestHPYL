using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Himall.Model
{
    public partial class AgreementInfo
    {
        /// <summary>
        /// 协议枚举
        /// </summary>
        public enum AgreementTypes
        {
            /// <summary>
            /// 患者
            /// </summary>
            [Description("患者")]
            Buyers = 0,

            /// <summary>
            /// 诊所
            /// </summary>
            [Description("诊所")]
            Seller = 1,
            [Description("APP关于我们")]
            APP = 2
        }
    }
}
