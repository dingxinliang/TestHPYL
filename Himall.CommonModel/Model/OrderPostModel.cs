using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    public class OrderPostModel
    {
        /// <summary>
        /// 收货地址
        /// </summary>
        public long RecieveAddressId { get; set; }
        public string LatAndLng { get; set; }
        /// <summary>
        /// 优惠券
        /// </summary>
        public string CouponIds { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public int InvoiceType { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string InvoiceTitle { get; set; }
        public string InvoiceCode { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string InvoiceContext { get; set; }
        public int Integral { get; set; }
        public Decimal Capital { get; set; }
        public string CollpIds { get; set; }
        public bool IsCashOnDelivery { get; set; }
        public long ActiveId { get; set; }
        public long GroupId { get; set; }

        public long groupActionId { set; get; }

        public int PlatformType { get; set; }

        public OrderShop[] OrderShops { get; set; }

        public object CurrentUser { get; set; }

        public List<long> DistributionUserLinkId { get; set; }

        public string CartItemIds { set; get; }
       // public long[] CartItemIds { get; set; }

        public bool IsShopbranchOrder { get; set; }
    }

    public class OrderShop
    {

        public long ShopId { get; set; }
        public OrderSku[] OrderSkus { get; set; }
        public CommonModel.Enum.DeliveryType DeliveryType { get; set; }
        public int? ShopBrandId { get; set; }
        public string Remark { get; set; }
    }

    public class OrderSku
    {
        public string SkuId { get; set; }

        public int Count { get; set; }
    }
}
