using Himall.CommonModel;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Himall.Web.Wdgj_Api
{
    /// <summary>
    /// WdgjApiHandler 的摘要说明
    /// </summary>
    public class WdgjApiHandler : IHttpHandler
    {
        /// <summary>
        /// 网店管家接口
        /// </summary>
        private string uCode;
        private string mType;
        private string timestamp;

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 应用程序入口
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            this.timestamp = context.Request["TimeStamp"];
            this.uCode = context.Request["uCode"];
            this.mType = context.Request["mType"];
            switch (this.mType)
            {
                case "mGetGoods":
                    context.Response.Write(this.findProduct(context));
                    break;
                case "mOrderSearch":
                    context.Response.Write(this.GetOrderList(context));
                    break;
                case "mGetOrder":
                    context.Response.Write(this.GetOrderDetail(context));
                    break;
                case "mSndGoods":
                    context.Response.Write(this.SendOrder(context));
                    break;
                case "mSysGoods":
                    context.Response.Write(this.AdjustQuantity(context));
                    break;
            }
        }
        /// <summary>
        /// 查找产品
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string findProduct(HttpContext context)
        {
            string uCode = context.Request["uCode"];
            string str1 = context.Request["PageSize"];
            string str2 = context.Request["Page"];
            string str3 = context.Request["GoodsType"];
            string str4 = context.Request["OuterID"];
            string str5 = context.Request["GoodsName"];
            ProductQuery productQueryModel = new ProductQuery();
            int num1 = 0;
            int num2 = 0;
            if (!string.IsNullOrEmpty(str1))
                num1 = Convert.ToInt32(str1);
            if (!string.IsNullOrEmpty(str2))
                num2 = Convert.ToInt32(str2);
            string str6 = (string)null;
            string str7 = (string)null;
            ProductInfo.ProductSaleStatus productSaleStatus = ProductInfo.ProductSaleStatus.RawState;
            if (!string.IsNullOrWhiteSpace(str3))
            {
                string str8 = str3;
                if (str8.Equals("Onsale"))
                    productSaleStatus = ProductInfo.ProductSaleStatus.OnSale;
                else if (str8.Equals("InStock"))
                    productSaleStatus = ProductInfo.ProductSaleStatus.InStock;
            }
            else if (!string.IsNullOrWhiteSpace(str4))
                str6 = str4;
            else if (!string.IsNullOrWhiteSpace(str5))
                str7 = str5;
            productQueryModel.PageSize = num1;
            productQueryModel.PageNo = num2;
            productQueryModel.SaleStatus = new ProductInfo.ProductSaleStatus?(productSaleStatus);
            productQueryModel.KeyWords = str7;
            productQueryModel.ProductCode = str6;
            productQueryModel.ShopId = new long?(1L);
            if (ServiceHelper.Create<IShopService>().GetshopInfoByCode(uCode) != null)
                productQueryModel.ShopId = new long?(ServiceHelper.Create<IShopService>().GetshopInfoByCode(uCode).ShopId);
            ObsoletePageModel<ProductInfo> products = ServiceHelper.Create<IProductService>().GetProducts(productQueryModel);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            try
            {
                stringBuilder.Append("<Goods>");
                foreach (ProductInfo productInfo in products.Models)
                {
                    stringBuilder.Append("<Ware>");
                    int num3 = Convert.ToInt32(productInfo.Id);
                    bool flag = ServiceHelper.Create<IProductService>().HasSKU(productInfo.Id);
                    int num4 = flag ? 1 : 0;
                    stringBuilder.Append(string.Format("<ItemID><![CDATA[{0}]]></ItemID>", (object)num3));
                    stringBuilder.Append(string.Format("<ItemName><![CDATA[{0}]]></ItemName>", (object)productInfo.ProductName));
                    stringBuilder.Append(string.Format("<OuterID><![CDATA[{0}]]></OuterID>", (object)productInfo.ProductCode));
                    stringBuilder.Append(string.Format("<Price><![CDATA[{0}]]></Price>", (object)productInfo.MinSalePrice));
                    stringBuilder.Append(string.Format("<IsSku><![CDATA[{0}]]></IsSku>", (object)num4));
                    ProductInfo product = ServiceHelper.Create<IProductService>().GetProduct(productInfo.Id);
                    stringBuilder.Append(string.Format("<Num><![CDATA[{0}]]></Num>", (object)Enumerable.Sum<SKUInfo>((IEnumerable<SKUInfo>)product.SKUInfo, (Func<SKUInfo, long>)(d => d.Stock))));
                    stringBuilder.Append("<Items>");
                    if (flag)
                    {
                        foreach (SKUInfo skuInfo in (IEnumerable<SKUInfo>)product.SKUInfo)
                        {
                            stringBuilder.Append("<Item>");
                            stringBuilder.Append(string.Format("<Unit><![CDATA[{0}]]></Unit>", (object)(skuInfo.Color + " " + skuInfo.Size + " " + skuInfo.Version)));
                            stringBuilder.Append(string.Format("<SkuOuterID><![CDATA[{0}]]></SkuOuterID>", (object)skuInfo.Sku));
                            stringBuilder.Append(string.Format("<SkuID><![CDATA[{0}]]></SkuID>", (object)skuInfo.Id));
                            stringBuilder.Append(string.Format("<Num><![CDATA[{0}]]></Num>", (object)skuInfo.Stock));
                            stringBuilder.Append(string.Format("<SkuPrice><![CDATA[{0}]]></SkuPrice>", (object)skuInfo.SalePrice));
                            stringBuilder.Append("</Item>");
                        }
                    }
                    stringBuilder.Append("</Items>");
                    stringBuilder.Append("</Ware>");
                }
                stringBuilder.Append("<Result>1</Result>");
                stringBuilder.Append(string.Format("<TotalCount>{0}</TotalCount>", (object)products.Total));
                stringBuilder.Append("<Cause></Cause>");
                stringBuilder.Append("</Goods>");
            }
            catch (Exception ex)
            {
                stringBuilder.Clear();
                stringBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                stringBuilder.Append("<Rsp><Result>0</Result><Cause>" + ex.Message + "</Cause></Rsp>");
            }
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 产品数量
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string AdjustQuantity(HttpContext context)
        {
            string str1 = context.Request["ItemID"];
            string str2 = context.Request["SkuID"];
            string str3 = context.Request["Quantity"];
            int num = Convert.ToInt32(str1);
            string str4 = str2.Trim();
            int stockChange = Convert.ToInt32(str3);
            string str5 = "";
            str5 = string.IsNullOrWhiteSpace(str4) ? string.Format("{0}_0_0_0", (object)num) : str4;
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                stringBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                stringBuilder.Append("<Rsp>");
                if (string.IsNullOrEmpty(str4) || string.IsNullOrWhiteSpace(str4))
                {
                    if (ServiceHelper.Create<IProductService>().UpdateStockByProId((long)num, stockChange))
                    {
                        ProductInfo product = ServiceHelper.Create<IProductService>().GetProduct((long)num);
                        stringBuilder.Append("<Result>1</Result>");
                        if (product != null && product.SaleStatus == ProductInfo.ProductSaleStatus.OnSale)
                            stringBuilder.Append("<GoodsType>Onsale</GoodsType>");
                        else
                            stringBuilder.Append("<GoodsType>InStock</GoodsType>");
                        stringBuilder.Append("<Cause></Cause>");
                    }
                    else
                    {
                        stringBuilder.Append("<Result>0</Result>");
                        stringBuilder.Append("<GoodsType></GoodsType>");
                        stringBuilder.Append("<Cause><![CDATA[{修改库存失败}]]></Cause>");
                    }
                }
                else
                {
                    ServiceHelper.Create<IProductService>().SetSkusStock((IEnumerable<string>)Enumerable.ToList<string>((IEnumerable<string>)str4.Split(',')), Enumerable.Select<string, int>((IEnumerable<string>)str3.Split(','), (Func<string, int>)(e =>
                    {
                        int result = 0;
                        if (!int.TryParse(e, out result))
                            result = 0;
                        return result;
                    })));
                    ProductInfo product = ServiceHelper.Create<IProductService>().GetProduct((long)num);
                    if (product != null)
                    {
                        stringBuilder.Append("<Result>1</Result>");
                        if (product != null && product.SaleStatus == ProductInfo.ProductSaleStatus.OnSale)
                            stringBuilder.Append("<GoodsType>Onsale</GoodsType>");
                        else
                            stringBuilder.Append("<GoodsType>InStock</GoodsType>");
                        stringBuilder.Append("<Cause></Cause>");
                    }
                    else
                    {
                        stringBuilder.Append("<Result>0</Result>");
                        stringBuilder.Append("<GoodsType></GoodsType>");
                        stringBuilder.Append("<Cause><![CDATA[{修改库存失败}]]></Cause>");
                    }
                }
                stringBuilder.Append("</Rsp>");
            }
            catch (Exception ex)
            {
                stringBuilder.Clear();
                stringBuilder.Append("<Result>0</Result>");
                stringBuilder.Append("<GoodsType></GoodsType>");
                stringBuilder.Append("<Cause>" + ex.Message + "</Cause>");
                stringBuilder.Append("</Rsp>");
            }
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 读取产品信息
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetOrderList(HttpContext context)
        {
            string uCode = context.Request["uCode"];
            string str1 = context.Request["PageSize"];
            string str2 = context.Request["Page"];
            string str3 = context.Request["OrderStatus"];
            string str4 = context.Request["Start_Modified"];
            string str5 = context.Request["End_Modified"];
            int num1 = Convert.ToInt32(str1);
            int num2 = Convert.ToInt32(str2);
            string s1 = (string)null;
            string s2 = (string)null;
            if (!string.IsNullOrWhiteSpace(str4))
                s1 = str4;
            if (!string.IsNullOrWhiteSpace(str5))
                s2 = str5;
            OrderQuery query = new OrderQuery();
            switch (str3)
            {
                case "0":
                    query.Status = new OrderInfo.OrderOperateStatus?(OrderInfo.OrderOperateStatus.WaitPay);
                    break;
                case "1":
                    query.Status = new OrderInfo.OrderOperateStatus?(OrderInfo.OrderOperateStatus.WaitDelivery);
                    break;
            }
            if (!string.IsNullOrEmpty(s1) && !string.IsNullOrEmpty(s2))
            {
                query.StartDate = new DateTime?(DateTime.Parse(s1));
                query.EndDate = new DateTime?(DateTime.Parse(s2));
            }
            query.PageNo = num2;
            query.PageSize = num1;
            if (ServiceHelper.Create<IShopService>().GetshopInfoByCode(uCode) != null)
                query.ShopId = new long?(ServiceHelper.Create<IShopService>().GetshopInfoByCode(uCode).ShopId);
            QueryPageModel<OrderInfo> orders = ServiceHelper.Create<IOrderService>().GetOrders(query);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            stringBuilder.Append("<Order><OrderList>");
            foreach (OrderInfo orderInfo in orders.Models)
                stringBuilder.Append(string.Format("<OrderNO>{0}</OrderNO>", (object)orderInfo.Id));
            stringBuilder.Append("</OrderList>");
            stringBuilder.AppendFormat("<Page>{0}</Page>", (object)num2);
            stringBuilder.Append("<Cause></Cause>");
            stringBuilder.Append("<Result>1</Result>");
            stringBuilder.AppendFormat("<OrderCount>{0}</OrderCount>", (object)orders.Total);
            stringBuilder.Append("</Order>");
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetOrderDetail(HttpContext context)
        {
            long orderId = Convert.ToInt64(context.Request["OrderNO"].Trim());
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            stringBuilder.Append("<Order>");
            stringBuilder.Append(string.Format("<OrderNO>{0}</OrderNO>", (object)orderId));
            OrderInfo order = ServiceHelper.Create<IOrderService>().GetOrder(orderId);
            if (order == null)
            {
                stringBuilder.Append("<Result>0</Result>");
                stringBuilder.Append("<Cause><![CDATA[{订单不存在}]]></Cause>");
            }
            else
            {
                stringBuilder.Append("<Result>1</Result>");
                stringBuilder.Append("<Cause></Cause>");
                switch (order.OrderStatus)
                {
                    case OrderInfo.OrderOperateStatus.WaitPay:
                        stringBuilder.Append("<OrderStatus>WAIT_BUYER_PAY</OrderStatus>");
                        break;
                    case OrderInfo.OrderOperateStatus.WaitDelivery:
                        stringBuilder.Append("<OrderStatus>WAIT_SELLER_SEND_GOODS</OrderStatus>");
                        break;
                    case OrderInfo.OrderOperateStatus.WaitReceiving:
                        stringBuilder.Append("<OrderStatus>WAIT_BUYER_CONFIRM_GOODS</OrderStatus>");
                        break;
                    case OrderInfo.OrderOperateStatus.Close:
                        stringBuilder.Append("<OrderStatus>TRADE_CLOSED</OrderStatus>");
                        break;
                    case OrderInfo.OrderOperateStatus.Finish:
                        stringBuilder.Append("<OrderStatus>TRADE_FINISHED</OrderStatus>");
                        break;
                }
                stringBuilder.Append(string.Format("<DateTime>{0}</DateTime>", (object)order.OrderDate));
                stringBuilder.Append(string.Format("<BuyerID><![CDATA[{0}]]></BuyerID>", string.IsNullOrEmpty(order.UserName) ? (object)"匿名" : (object)order.UserName));
                stringBuilder.Append(string.Format("<BuyerName><![CDATA[{0}]]></BuyerName>", string.IsNullOrEmpty(order.ShipTo) ? (object)"匿名" : (object)order.ShipTo));
                stringBuilder.Append("<CardType></CardType>");
                stringBuilder.Append("<IDCard></IDCard>");
                stringBuilder.Append("<Country><![CDATA[{中国}]]></Country>");
                string[] strArray = order.RegionFullName.Replace("，", ",").Split(' ');
                if (strArray.Length >= 3)
                {
                    stringBuilder.Append(string.Format("<Province><![CDATA[{0}]]></Province>", (object)strArray[0]));
                    stringBuilder.Append(string.Format("<City><![CDATA[{0}]]></City>", (object)strArray[1]));
                    stringBuilder.Append(string.Format("<Town><![CDATA[{0}]]></Town>", (object)strArray[2]));
                }
                stringBuilder.Append(string.Format("<Adr><![CDATA[{0}]]></Adr>", (object)(order.RegionFullName.Replace(" ", "") + order.Address)));
                stringBuilder.Append(string.Format("<Zip><![CDATA[{0}]]></Zip>", (object)""));
                stringBuilder.Append(string.Format("<Email><![CDATA[{0}]]></Email>", (object)""));
                stringBuilder.Append(string.Format("<Phone><![CDATA[{0}]]></Phone>", (object)order.CellPhone));
                stringBuilder.Append(string.Format("<Total>{0}</Total>", (object)order.ProductTotalAmount));
                stringBuilder.Append("<Currency>CNY</Currency>");
                stringBuilder.Append(string.Format("<Postage>{0}</Postage>", (object)order.Freight));
                stringBuilder.Append(string.Format("<PayAccount>{0}</PayAccount>", (object)order.PaymentType));
                stringBuilder.Append(string.Format("<PayID>{0}</PayID>", (object)order.GatewayOrderId));
                stringBuilder.Append(string.Format("<LogisticsName><![CDATA[{0}]]></LogisticsName>", (object)order.ExpressCompanyName));
                stringBuilder.Append(string.Format("<Chargetype>{0}</Chargetype>", (object)order.PaymentTypeName));
                stringBuilder.Append(string.Format("<CustomerRemark><![CDATA[{0}]]></CustomerRemark>", (object)order.OrderRemarks));
                stringBuilder.Append(string.Format("<InvoiceTitle><![CDATA[{0}]]></InvoiceTitle>", (object)order.InvoiceTitle));
                stringBuilder.Append(string.Format("<Remark><![CDATA[{0}]]></Remark>", (object)order.SellerRemark));
                foreach (OrderItemInfo orderItemInfo in (IEnumerable<OrderItemInfo>)order.OrderItemInfo)
                {
                    stringBuilder.Append("<Item>");
                    stringBuilder.Append(string.Format("<GoodsID>{0}</GoodsID>", (object)orderItemInfo.SKU));
                    stringBuilder.Append(string.Format("<GoodsName><![CDATA[{0}]]></GoodsName>", (object)orderItemInfo.ProductName));
                    stringBuilder.Append(string.Format("<GoodsSpec><![CDATA[{0}]]></GoodsSpec>", (object)(orderItemInfo.Color + orderItemInfo.Size + orderItemInfo.Version)));
                    stringBuilder.Append(string.Format("<Count>{0}</Count>", (object)orderItemInfo.Quantity));
                    stringBuilder.Append(string.Format("<Price>{0}</Price>", (object)orderItemInfo.SalePrice));
                    stringBuilder.Append("<Tax>0</Tax>");
                    stringBuilder.Append("</Item>");
                }
            }
            stringBuilder.Append("</Order>");
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 订单发货
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string SendOrder(HttpContext context)
        {
            string str1 = context.Request["OrderNO"];
            string str2 = context.Request["SndStyle"];
            string str3 = context.Request["BillID"];
            StringBuilder stringBuilder = new StringBuilder();
            string str4 = str1.Trim();
            if (str1.IndexOf(',') > 0)
                return this.ExMsg("不支持合并发货，请选择单个订单");
            OrderInfo order = ServiceHelper.Create<IOrderService>().GetOrder(Convert.ToInt64(str4));
            if (order == null)
                return this.ExMsg("未找到此订单");
            if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
                return this.ExMsg("只有待发货状态的订单才能发货！");
            if (string.IsNullOrEmpty(str3.Trim()) || str3.Trim().Length > 20)
                return this.ExMsg("运单号码不能为空，在1至20个字符之间！");
            try
            {
                Himall.Web.Models.Result result = new Himall.Web.Models.Result();
            }
            catch (Exception ex)
            {
                return this.ExMsg(ex.Message);
            }
            return "";
        }
        /// <summary>
        /// 时间差
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        private bool checkTimeStamp(string timeStamp)
        {
            long timestamp = Convert.ToInt64(timeStamp);
            DateTime utcNow = DateTime.UtcNow;
            DateTime dateTime = this.UnixTimestampToDateTime(utcNow, timestamp);
            return (utcNow - dateTime).TotalMinutes < 15.0;
        }
        /// <summary>
        /// 时间转换
        /// </summary>
        /// <param name="target"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        private DateTime UnixTimestampToDateTime(DateTime target, long timestamp)
        {
            return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).Add(new TimeSpan(long.Parse((string)(object)timestamp + (object)"0000000")));
        }
        /// <summary>
        /// 密钥
        /// </summary>
        /// <param name="uCode"></param>
        /// <param name="mType"></param>
        /// <param name="Secret"></param>
        /// <param name="TimeStamp"></param>
        /// <returns></returns>
        private string sign(string uCode, string mType, string Secret, string TimeStamp)
        {
            string s = string.Format("{0}mType{1}TimeStamp{2}uCode{3}{0}", (object)Secret, (object)mType, (object)TimeStamp, (object)uCode);
            StringBuilder stringBuilder = new StringBuilder(32);
            foreach (byte num in new MD5CryptoServiceProvider().ComputeHash(Encoding.GetEncoding("utf-8").GetBytes(s)))
                stringBuilder.Append(num.ToString("x").PadLeft(2, '0'));
            return stringBuilder.ToString().ToUpper();
        }
        /// <summary>
        /// 执行消息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public string ExMsg(string msg)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            stringBuilder.Append("<Rsp><Result>0</Result>");
            stringBuilder.AppendFormat("<Cause>{0}</Cause>", (object)string.Format("<![CDATA[{0}]]>", (object)msg));
            stringBuilder.Append("</Rsp>");
            return stringBuilder.ToString();
        }
    }
}