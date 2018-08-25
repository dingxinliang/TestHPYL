using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API.Model.ParamsModel
{
    public class OrderSubmitOrderModel
    {
        public string skuIds { get; set; }

        public string counts { get; set; }

        public long recieveAddressId { get; set; }

        public string couponIds { get; set; }

        public int integral { get; set; }

        public bool isCashOnDelivery { get; set; }

        public int invoiceType { get; set; }

        public string invoiceTitle { get; set; }

        public string invoiceContext { get; set; }

        public string invoiceCode { get; set; }

        public string orderRemarks { get; set; }

        public string jsonOrderShops { get; set; }

        public Decimal CapitalAmount { get; set; }

        public string PayPwd { get; set; }
    }
}
