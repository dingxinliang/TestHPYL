using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class CouponRecord
    {
        // Properties
        public string ClosingTime { get; set; }

        public long CouponId { get; set; }

        public string CouponName { get; set; }

        public int IsUse { get; set; }

        public decimal Price { get; set; }

        public string StartTime { get; set; }


    }
}
