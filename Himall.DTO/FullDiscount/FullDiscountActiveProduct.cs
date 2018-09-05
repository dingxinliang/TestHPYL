using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    /// <summary>
    /// 满减-诊疗项目
    /// </summary>
    public class FullDiscountActiveProduct
    {
        public long Id { get; set; }
        public long ActiveId { get; set; }
        /// <summary>
        /// 诊疗项目编号
        /// <para>-1表示所有诊疗项目</para>
        /// </summary>
        public long ProductId { get; set; }
    }
}
