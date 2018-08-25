using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    public class ShopActiveList
    {
        public List<ActiveInfo> ShopActives { get; set; }

        public Decimal FreeFreightAmount { get; set; }

        public List<CouponModel> ShopCoupons { get; set; }
    }
}
