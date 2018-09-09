using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Himall.Model
{
    public partial class StatisticOrderCommentsInfo
    {
        /// <summary>
        /// 结算状态
        /// </summary>
        public enum EnumCommentKey
        {
            /// <summary>
            /// 宝贝与描述相符 诊所得分
            /// </summary>
            [Description("宝贝与描述相符 诊所得分")]
            ProductAndDescription = 1,

            /// <summary>
            /// 宝贝与描述相符 同行业平均分
            /// </summary>
            [Description("宝贝与描述相符 同行业平均分")]
            ProductAndDescriptionPeer ,

            /// <summary>
            /// 宝贝与描述相符 同行业诊所最高得分
            /// </summary>
            [Description("宝贝与描述相符 同行业诊所最高得分")]
            ProductAndDescriptionMax ,

            /// <summary>
            /// 宝贝与描述相符 同行业诊所最低得分
            /// </summary>
            [Description("宝贝与描述相符 同行业诊所最低得分")]
            ProductAndDescriptionMin,
            /// <summary>
            /// 诊所发货速度 诊所得分
            /// </summary>
            [Description("诊所发货速度 诊所得分")]
            SellerDeliverySpeed,

            /// <summary>
            /// 诊所发货速度 同行业平均分
            /// </summary>
            [Description("诊所发货速度 同行业平均分")]
            SellerDeliverySpeedPeer,

            /// <summary>
            /// 诊所发货速度 同行业诊所最高得分
            /// </summary>
            [Description("诊所发货速度 同行业诊所最高得分")]
            SellerDeliverySpeedMax,

            /// <summary>
            /// 诊所发货速度 同行业诊所最低得分
            /// </summary>
            [Description("诊所发货速度 同行业诊所最低得分")]
            SellerDeliverySpeedMin,

            /// <summary>
            /// 诊所服务态度 诊所得分
            /// </summary>
            [Description("诊所服务态度 诊所得分")]
            SellerServiceAttitude ,

            /// <summary>
            /// 诊所服务态度 同行业平均分
            /// </summary>
            [Description("诊所服务态度 同行业平均分")]
            SellerServiceAttitudePeer ,

            /// <summary>
            /// 诊所服务态度 同行业诊所最高得分
            /// </summary>
            [Description("诊所服务态度 同行业诊所最高得分")]
            SellerServiceAttitudeMax ,

            /// <summary>
            /// 诊所服务态度 同行业诊所最低得分
            /// </summary>
            [Description("诊所服务态度 同行业诊所最低得分")]
            SellerServiceAttitudeMin,



        }

        
    }
}
