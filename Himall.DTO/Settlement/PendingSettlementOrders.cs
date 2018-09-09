using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace Himall.DTO
{
    /// <summary>
    /// 待结算预约单
    /// </summary>
    public class PendingSettlementOrders
    {
        /// <summary>
        /// 预约单ID
        /// </summary>
        public long OrderId { set; get; }

        /// <summary>
        /// 预约单号字符串
        /// </summary>
        public string strOrderId { get { return OrderId.ToString(); } }

        /// <summary>
        /// 预约单状态(目前只针对已完成预约单)
        /// </summary>
        public string Status
        {
            set { this.Status = value; }
            get { return "已完成"; }
        }

        /// <summary>
        /// 诊所名称
        /// </summary>
        public string ShopName { set; get; }

        /// <summary>
        /// 支付方式
        /// </summary>

        public string PaymentTypeName { set; get; }

        /// <summary>
        /// 预约单金额
        /// </summary>
        public decimal OrderAmount { get; set; }

        /// <summary>
        /// 平台佣金
        /// </summary>
        public decimal PlatCommission { set; get; }

        /// <summary>
        /// 分佣佣金
        /// </summary>
        public decimal DistributorCommission { set; get; }


        /// <summary>
        /// 平台佣金退款
        /// </summary>
        public decimal PlatCommissionReturn { set; get; }


        /// <summary>
        /// 分佣佣金退款
        /// </summary>
        public decimal DistributorCommissionReturn { set; get; }

        /// <summary>
        /// 运费
        /// </summary>
        public decimal FreightAmount { get; set; }

        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal RefundAmount { set; get; }

        /// <summary>
        /// 诊所结算金额
        /// </summary>
        public decimal SettlementAmount { set; get; }

        /// <summary>
        /// 预约单完成时间
        /// </summary>
        /// 
        [ScriptIgnore]
        public DateTime OrderFinshTime { set; get; }

        public string OrderFinshTimeStr { get { return OrderFinshTime.ToString("yyyy-MM-dd HH:mm:ss"); } }

        /// <summary>
        /// 结算周期范围(不在当前结算周期内的)
        /// </summary>
        public string SettlementCycle
        {
            set
            {
                if (value != null)  _SettlementCycle = value;
            }

            get { return _SettlementCycle; }
        }
        private string _SettlementCycle = "";
    }
}
