using Himall.Core;
using Himall.Core.Plugins.Payment;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Himall.Web.App_Code.Common;
using Himall.Application;
using Himall.DTO;
using Himall.IServices.IDXL;
using static Himall.Model.OrderInfo;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class OrderController : BaseAdminController
    {
        private IOrderService _iOrderService;
        private IExpressService _iExpressService;
        private IPaymentConfigService _iPaymentConfigService;
        private IFightGroupService _iFightGroupService;
        private INOrderService _NOrderService;
        public OrderController(IOrderService iOrderService, IExpressService iExpressService, IPaymentConfigService iPaymentConfigService
            , IFightGroupService iFightGroupService, INOrderService nNOrderService)
        {
            _iOrderService = iOrderService;
            _iExpressService = iExpressService;
            _iPaymentConfigService = iPaymentConfigService;
            _iFightGroupService = iFightGroupService;
            _NOrderService = nNOrderService;
        }

        public ActionResult Management()
        {
            var model = _iPaymentConfigService.GetPaymentTypes();
            return View(model);
        }


        public ActionResult Detail(long id)
        {
            OrderInfo order = _iOrderService.GetOrder(id);
            var Newmodel = GetNewmodel(order.Id);
            if (order == null)
            {
                throw new HimallException("错误的预约单信息");
            }
            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                var fgord = _iFightGroupService.GetFightGroupOrderStatusByOrderId(order.Id);
                order.FightGroupOrderJoinStatus = fgord.GetJoinStatus;
                order.FightGroupCanRefund = fgord.CanRefund;
            }
            //if (order.ShopBranchId.HasValue && order.ShopBranchId.Value!=0)
            //{//补充数据
            //    var branch = ShopBranchApplication.GetShopBranchById(order.ShopBranchId.Value);
            //    ViewBag.ShopBranchContactUser = branch.UserName;
            //}
            order.ReceiveDate = Newmodel[0].ReceiveDate.ToString("yyyy-MM-dd");
            order.ReceiveStartTime = Newmodel[0].ReceiveStartTime;
            order.ReceiveEndTime = Newmodel[0].ReceiveEndTime;
            order.doctorName = Newmodel[0].doctorName;
            ViewBag.Coupon = 0;
            //var coupon = ServiceHelper.Create<ICouponService>().GetCouponRecordInfo( order.UserId , order.Id );
            //if( coupon != null )
            //{
            //    ViewBag.Coupon = coupon.Himall_Coupon.Price;
            //}
            #region 门店信息
            if (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0)
            {
                var shopBranchInfo = ShopBranchApplication.GetShopBranchById(order.ShopBranchId.Value);
                if (shopBranchInfo != null)
                {
                    ViewBag.ShopBranchInfo = shopBranchInfo;
                    if (order.OrderStatus == OrderInfo.OrderOperateStatus.Finish) ViewBag.ShopBranchContactUser = shopBranchInfo.UserName;
                }
            }
            #endregion
            return View(order);
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult List(OrderQuery query, int page, int rows)
        {
            query.PageNo = page;
            query.PageSize = rows;

            //var orders = OrderApplication.GetOrders(query);
            var fullOrders = OrderApplication.GetFullOrders(query);
            var models = fullOrders.Models.ToList();

            var shops = Application.ShopApplication.GetShops(fullOrders.Models.Select(p => p.ShopId).ToArray());
            var shopBranchs = Application.ShopBranchApplication.GetShopBranchs(models.Where(p => p.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake && p.ShopBranchId.HasValue && p.ShopBranchId.Value != 0).Select(p => p.ShopBranchId.Value));

            IEnumerable<OrderModel> orderModels = models.Select(item =>
            {
                var shop = shops.FirstOrDefault(sp => sp.Id == item.ShopId);
                var Newmodel = GetNewmodel(item.Id);
                return new OrderModel()
                {
                    OrderId = item.Id,
                    OrderStatus = item.OrderStatus.ToDescription(),
                    OrderState = (int)item.OrderStatus,
                    OrderDate = item.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    ShopId = item.ShopId,
                    ShopName = item.ShopName,
                    ShopBranchName = item.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake && item.ShopBranchId.HasValue && item.ShopBranchId.Value != 0 ? shopBranchs.FirstOrDefault(sb => sb.Id == item.ShopBranchId.Value).ShopBranchName : "",
                    UserId = item.UserId,
                    UserName = item.UserName,
                    ShipTo = item.ShipTo,
                    TotalPrice = item.OrderTotalAmount,
                    PaymentTypeName = item.PaymentTypeName,
                    PlatForm = (int)item.Platform,
                    IconSrc = GetIconSrc(item.Platform),
                    PlatformText = item.Platform.ToDescription(),
                    PaymentTypeGateway = item.PaymentTypeGateway,
                    PayDate = item.PayDate,
                    PaymentTypeStr = item.PaymentType.ToDescription(),
                    PaymentType = item.PaymentType,
                    OrderType = item.OrderType,
                    GatewayOrderId = item.GatewayOrderId,
                    Payee = shop.ContactsName,
                    CellPhone = item.CellPhone,
                    RegionFullName = item.RegionFullName,
                    Address = item.Address,
                    SellerRemark = item.SellerRemark,
                    UserRemark = item.UserRemark,
                    OrderItems = item.OrderItems,
                    SellerRemarkFlag = item.SellerRemarkFlag,
                    YYDate = Newmodel[0].ReceiveDate.ToString("yyyy-MM-dd"),
                    ReceiveDate = Newmodel[0].ReceiveDate,
                    ReceiveStartTime = Newmodel[0].ReceiveStartTime,
                    ReceiveEndTime = Newmodel[0].ReceiveEndTime,
                    doctorName = Newmodel[0].doctorName,
                    SellerAddress = item.SellerAddress,
                };
            });
            //开始结束时间
            if (query.StartDate.HasValue)
            {
                DateTime sdt = query.StartDate.Value;
                orderModels = orderModels.Where(d => d.ReceiveDate >= sdt);
            }
            if (query.EndDate.HasValue)
            {
                DateTime edt = query.EndDate.Value.AddDays(1);
                orderModels = orderModels.Where(d => d.ReceiveDate < edt);
            }
            DataGridModel<OrderModel> dataGrid = new DataGridModel<OrderModel>()
            {
                rows = orderModels,
                total = fullOrders.Total
            };
            return Json(dataGrid);
        }
        /// <summary>
        /// 获取新模型
        /// </summary>
        /// <param name="OrderId"></param>
        /// <returns></returns>
        public List<NewOrdermodel> GetNewmodel(long OrderId)
        {
            var data = _NOrderService.GetNewmodel(OrderId);
            return data;
        }
        public ActionResult ExportToExcel(OrderQuery query)
        {
            var orders = OrderApplication.GetFullOrdersNoPage(query);

            return ExcelView("ExportOrderinfo", "平台预约单信息", orders);
        }


        /// <summary>
        /// 获取预约单来源图标地址
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        string GetIconSrc(PlatformType platform)
        {
            if (platform == PlatformType.IOS || platform == PlatformType.Android)
                return "/images/app.png";
            return string.Format("/images/{0}.png", platform.ToString());
        }

        /// <summary>
        /// 付款
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="payRemark">收款备注</param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult ConfirmPay(long orderId, string payRemark)
        {
            Result result = new Result();
            OrderApplication.PlatformConfirmOrderPay(orderId, payRemark, CurrentManager.UserName);
            PaymentHelper.IncreaseSaleCount(new List<long> { orderId });
            result.success = true;
            return Json(result);

        }

        /// <summary>
        /// 取消预约单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="payRemark">收款备注</param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult CloseOrder(long orderId)
        {
            Result result = new Result();
            _iOrderService.PlatformCloseOrder(orderId, CurrentManager.UserName);
            result.success = true;

            return Json(result);

        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult GetExpressData(string expressCompanyName, string shipOrderNumber)
        {
            string content = "暂时没有此快递单号的信息";
            if (string.IsNullOrEmpty(expressCompanyName) || string.IsNullOrEmpty(shipOrderNumber))
                return Json(content);
            string kuaidi100Code = _iExpressService.GetExpress(expressCompanyName).Kuaidi100Code;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format("http://www.kuaidi100.com/query?type={0}&postid={1}", kuaidi100Code, shipOrderNumber));
            request.Timeout = 8000;


            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream stream = response.GetResponseStream();
                System.IO.StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.GetEncoding("UTF-8"));

                // 读取流字符串内容
                content = streamReader.ReadToEnd();
                content = content.Replace("&amp;", "");
                content = content.Replace("&nbsp;", "");
                content = content.Replace("&", "");
            }

            return Json(content);
        }

        public ActionResult InvoiceContext()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetInvoiceContexts(int page = 1, int rows = 20)
        {
            var model = _iOrderService.GetInvoiceContexts(page, rows);
            return Json(new
            {
                rows = model.Models,
                total = model.Total
            });
        }

        [HttpPost]
        public ActionResult SaveInvoiceContext(string name, long id = -1)
        {
            InvoiceContextInfo info = new InvoiceContextInfo()
            {
                Id = id,
                Name = name
            };
            _iOrderService.SaveInvoiceContext(info);
            return Json(true);
        }

        [HttpPost]
        public ActionResult DeleteInvoiceContexts(long id)
        {
            _iOrderService.DeleteInvoiceContext(id);
            return Json(true);
        }

    }
}