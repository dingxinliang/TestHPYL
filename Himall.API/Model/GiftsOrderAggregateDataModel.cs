using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model
{
    public class GiftsOrderAggregateDataModel
    {
        /// <summary>
        /// 所有预约单数
        /// </summary>
        public int AllCount { get; set; }
        /// <summary>
        /// 待发货预约单数
        /// </summary>
        public int WaitDeliveryCount { get; set; }
        /// <summary>
        /// 待结算预约单数
        /// </summary>
        public int WaitReceivingCount { get; set; }
        /// <summary>
        /// 己完成预约单数
        /// </summary>
        public int FinishCount { get; set; }
    }
}
