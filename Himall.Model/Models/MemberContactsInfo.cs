using System.ComponentModel;

namespace Himall.Model
{
    public partial class MemberContactsInfo
    {

        public enum UserTypes
        {
            /// <summary>
            /// 普通用户
            /// </summary>
            [Description("普通用户")]
            General,

            /// <summary>
            /// 诊所用户
            /// </summary>
            [Description("诊所用户")]
            ShopManager,
        }

    }
}
