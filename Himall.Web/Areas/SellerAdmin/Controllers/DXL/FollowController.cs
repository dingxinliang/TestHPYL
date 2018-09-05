using Himall.Application;
using Himall.CommonModel;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin.Controllers.DXL
{
    public class FollowController : BaseSellerController
    {
        private IShopCategoryService _iShopCategoryService;
        // GET: SellerAdmin/Flow
        public ActionResult FollowPlanDetail()
        {
            return View();
        }
  public ActionResult management()
        {
            return View();
        }
        /// <summary>
        /// 获取随访列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <param name="auditStatuses"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult List(FollowPlanQuery queryModel, string auditStatuses, int page, int rows)
        {
            queryModel.PageSize = rows;
            queryModel.PageNumber = page;
            queryModel.shopId = base.CurrentSellerManager.ShopId;
            QueryPageModel<FollowPlan> follows = FollowManagerApplication.GetFollowPan(queryModel);

            DataGridModel<FollowPlan> data = new DataGridModel<FollowPlan>
            {
                total = follows.Total,
                rows = follows.Models.ToList()
            };
            return base.Json(data);
        }


        /// <summary>
        /// 删除随访
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult Delete(string ids)
        {
            try
            {
                IEnumerable<long> enumerable = from item in ids.Split(new char[] { ',' }) select long.Parse(item);
                FollowManagerApplication.DeleteFollowPlan(enumerable, base.CurrentSellerManager.ShopId);
                LogInfo info = new LogInfo
                {
                    Date = DateTime.Now,
                    Description = "删除随访，Ids=" + ids,
                    IPAddress = base.Request.UserHostAddress,
                    PageUrl = "/Follow/Delete",
                    UserName = base.CurrentSellerManager.UserName,
                    ShopId = base.CurrentSellerManager.ShopId
                };
                ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(info);
                return base.Json(new { success = true });
            }
            catch (Exception exception)
            {
                return base.Json(new { success = false, msg = exception.Message });
            }
        }

        /// <summary>
        /// 取消随访
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult Cancel(string ids)
        {
            try
            {
                IEnumerable<long> enumerable = from item in ids.Split(new char[] { ',' }) select long.Parse(item);
                FollowManagerApplication.CancelFollowPlan(enumerable, base.CurrentSellerManager.ShopId);
                LogInfo info = new LogInfo
                {
                    Date = DateTime.Now,
                    Description = "取消随访，Ids=" + ids,
                    IPAddress = base.Request.UserHostAddress,
                    PageUrl = "/Follow/Cancel",
                    UserName = base.CurrentSellerManager.UserName,
                    ShopId = base.CurrentSellerManager.ShopId
                };
                ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(info);
                return base.Json(new { success = true });
            }
            catch (Exception exception)
            {
                return base.Json(new { success = false, msg = exception.Message });
            }
        }

        /// <summary>
        /// 确认随访
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult Down(string ids,string lastuser,string result)
        {
            try
            {
                IEnumerable<long> enumerable = from item in ids.Split(new char[] { ',' }) select long.Parse(item);
                FollowManagerApplication.DownFollowPlan(enumerable, base.CurrentSellerManager.ShopId, lastuser, result);
                LogInfo info = new LogInfo
                {
                    Date = DateTime.Now,
                    Description = "确认随访，Ids=" + ids,
                    IPAddress = base.Request.UserHostAddress,
                    PageUrl = "/Follow/Down",
                    UserName = base.CurrentSellerManager.UserName,
                    ShopId = base.CurrentSellerManager.ShopId
                };
                ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(info);
                return base.Json(new { success = true });
            }
            catch (Exception exception)
            {
                return base.Json(new { success = false, msg = exception.Message });
            }
        }


        /// <summary>
        /// 获取随访计划
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="lastuser"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult UserFollowPlan(string ids)
        {
            IEnumerable<long> enumerable = from item in ids.Split(new char[] { ',' }) select long.Parse(item);
            var data = FollowManagerApplication.UserFollowPlan(enumerable);
            return base.Json(data);
        }
    }
}