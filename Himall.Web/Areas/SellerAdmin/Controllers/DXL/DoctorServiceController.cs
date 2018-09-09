using Himall.Application;
using Himall.CommonModel;
using Himall.IServices.QueryModel;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin.Controllers.DXL
{
    public class DoctorServiceController : BaseSellerController
    {
        // GET: SellerAdmin/DoctorService
        public ActionResult management()
        {
            return View();
        }

        [HttpPost, UnAuthorize]
        public JsonResult List(UserQuery queryModel, string auditStatuses, int page, int rows)
        {
            queryModel.PageSize = rows;
            queryModel.PageNo = page;
            queryModel.shopId = base.CurrentSellerManager.ShopId;
            QueryPageModel<UserModel> follows = MemberApplication.GetdoctorList(queryModel);

            DataGridModel<UserModel> data = new DataGridModel<UserModel>
            {
                total = follows.Total,
                rows = follows.Models.ToList()
            };
            return base.Json(data);
        }

        public JsonResult Lock(long id)
        {
            MemberApplication.LockMember(id);
            return Json(new Result() { success = true, msg = "冻结成功！" });
        }

        public JsonResult UnLock(long id)
        {
            MemberApplication.UnLockMember(id);
            return Json(new Result() { success = true, msg = "解冻成功！" });
        }
    }
}