using Himall.Application;
using Himall.CommonModel;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Areas.Admin.Models.Product;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin.Controllers.DXL
{
    public class TemPlateController : BaseSellerController
    {
        private IShopCategoryService _iShopCategoryService;
        // GET: SellerAdmin/TemPlate
        public ActionResult Follow()
        {
            return View();
        }

        public ActionResult doctorfollow()
        {
            return View();
        }
        public ActionResult followcontent(long id)
        {
            return base.View();
        }
        public ActionResult doctorcontent(long id)
        {
            return base.View();
        }

        public ActionResult Save()
        {
            string str;
            ProductCreateModel model = this.InitCreateModel(null);
            return base.View(model);
        }
     
        private ProductCreateModel InitCreateModel(ProductCreateModel model = null)
        {
            if (model == null)
            {
                model = new ProductCreateModel();
            }
            long id = base.CurrentShop.Id;
            model.ShopCategorys = ShopCategoryApplication.GetShopCategory(id);
            return model;
        }

        /// <summary>
        /// 获取医嘱模板
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult GetDoctor(string ids)
        {
          var data=FollowManagerApplication.GetDoctor(ids, base.CurrentSellerManager.ShopId);
            return base.Json(new { success = true,data=data });
        }
        /// <summary>
        /// 创建医嘱内容
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cId"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        [ValidateInput(false)]
        public JsonResult Creatdoctor(string cid, string title, string pic, string remark,string ids)
        {
            try
            {

                FollowManagerApplication.Creatdoctor(cid,title, pic, remark,ids, base.CurrentSellerManager.ShopId, base.CurrentSellerManager.Id);
                LogInfo info = new LogInfo
                {
                    Date = DateTime.Now,
                    Description = "创建医嘱内容，Ids=" + ids,
                    IPAddress = base.Request.UserHostAddress,
                    PageUrl = "/Follow/Creatdoctor",
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
        /// 新增编辑模板分类
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult CreateFollow(string name, string cId, string ids)
        {
            try
            {

                FollowManagerApplication.SaveFollow(name, cId, ids, base.CurrentSellerManager.ShopId, base.CurrentSellerManager.Id);
                LogInfo info = new LogInfo
                {
                    Date = DateTime.Now,
                    Description = "新增编辑模板分类，Ids=" + ids,
                    IPAddress = base.Request.UserHostAddress,
                    PageUrl = "/Follow/CreateFollow",
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
        /// 创建随访内容
        /// </summary>
        /// <param name="cId"></param>
        /// <param name="ids"></param>
        /// <param name="day"></param>
        /// <param name="cont"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult CreateFollowContent(string cId, string ids,int day,string cont)
        {
            try
            {

                FollowManagerApplication.SaveFollowContent(cId, ids, base.CurrentSellerManager.ShopId, day, cont);
                LogInfo info = new LogInfo
                {
                    Date = DateTime.Now,
                    Description = "创建随访内容，Ids=" + ids,
                    IPAddress = base.Request.UserHostAddress,
                    PageUrl = "/Follow/CreateFollowContent",
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
        /// 添加修改随访内容
        /// </summary>
        /// <param name="cId"></param>
        /// <param name="ids"></param>
        /// <param name="day"></param>
        /// <param name="cont"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult CreatedoctorContent(string cId, string ids, int day, string cont)
        {
            try
            {

                FollowManagerApplication.CreatedoctorContent(cId, ids, base.CurrentSellerManager.ShopId, day, cont);
                LogInfo info = new LogInfo
                {
                    Date = DateTime.Now,
                    Description = "创建/修改关联计划，Ids=" + ids,
                    IPAddress = base.Request.UserHostAddress,
                    PageUrl = "/Follow/CreatedoctorContent",
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
        /// 获取模板列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <param name="auditStatuses"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult List(FollowSearch queryModel, string auditStatuses, int page, int rows)
        {
            queryModel.PageSize = rows;
            queryModel.PageNumber = page;
            queryModel.AuditStatus = (queryModel.AuditStatus == null ? auditStatuses : queryModel.AuditStatus);
            queryModel.shopId = base.CurrentSellerManager.ShopId;
            QueryPageModel<FollowQuery> follows = FollowManagerApplication.GetFollow(queryModel);

            DataGridModel<FollowQuery> data = new DataGridModel<FollowQuery>
            {
                total = follows.Total,
                rows = follows.Models.ToList()
            };
            return base.Json(data);
        }
        /// <summary>
        /// 获取模板内容列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <param name="auditStatuses"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult ListContent(FollowSearch queryModel, long id, int page, int rows)
        {

            queryModel.PageSize = rows;
            queryModel.PageNumber = page;
           
            queryModel.shopId = base.CurrentSellerManager.ShopId;
            QueryPageModel<FollowContentQuery> followcontent = FollowManagerApplication.GetFollowContent(queryModel, id);

            DataGridModel<FollowContentQuery> data = new DataGridModel<FollowContentQuery>
            {
                total = followcontent.Total,
                rows = followcontent.Models.ToList()
            };
            return base.Json(data);
        }


        /// <summary>
        /// 获取医嘱关联计划
        /// </summary>
        /// <param name="queryModel"></param>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult ListDoctorContent(FollowSearch queryModel, long id, int page, int rows)
        {

            queryModel.PageSize = rows;
            queryModel.PageNumber = page;
            queryModel.shopId = base.CurrentSellerManager.ShopId;
            QueryPageModel<DoctorContentQuery> followcontent = FollowManagerApplication.ListDoctorContent(queryModel, id);

            DataGridModel<DoctorContentQuery> data = new DataGridModel<DoctorContentQuery>
            {
                total = followcontent.Total,
                rows = followcontent.Models.ToList()
            };
            return base.Json(data);
        }
        /// <summary>
        /// 获取医嘱文章列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult doctorlist(FollowSearch queryModel, string auditStatuses, int page, int rows)
        {

            queryModel.PageSize = rows;
            queryModel.PageNumber = page;
            queryModel.AuditStatus = (queryModel.AuditStatus == null ? auditStatuses : queryModel.AuditStatus);
            queryModel.shopId = base.CurrentSellerManager.ShopId;
            QueryPageModel<FollowDoctorQuery> followcontent = FollowManagerApplication.GetFollowDoctor(queryModel);

            DataGridModel<FollowDoctorQuery> data = new DataGridModel<FollowDoctorQuery>
            {
                total = followcontent.Total,
                rows = followcontent.Models.ToList()
            };
            return base.Json(data);
        }
        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult Delete(string ids)
        {
            try
            {
                IEnumerable<long> enumerable = from item in ids.Split(new char[] { ',' }) select long.Parse(item);
                FollowManagerApplication.DeleteProduct(enumerable, base.CurrentSellerManager.ShopId);
                LogInfo info = new LogInfo
                {
                    Date = DateTime.Now,
                    Description = "删除模板，Ids=" + ids,
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
        /// 删除模板内容
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult DeleteContent(string ids)
        {
            try
            {
                IEnumerable<long> enumerable = from item in ids.Split(new char[] { ',' }) select long.Parse(item);
                FollowManagerApplication.DeleteContent(enumerable, base.CurrentSellerManager.ShopId);
                LogInfo info = new LogInfo
                {
                    Date = DateTime.Now,
                    Description = "删除模板内容，Ids=" + ids,
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
        /// 删除关联计划
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult DeletedoctorContent(string ids)
        {
            try
            {
                IEnumerable<long> enumerable = from item in ids.Split(new char[] { ',' }) select long.Parse(item);
                FollowManagerApplication.DeletedoctorContent(enumerable, base.CurrentSellerManager.ShopId);
                LogInfo info = new LogInfo
                {
                    Date = DateTime.Now,
                    Description = "删除关联计划内容，Ids=" + ids,
                    IPAddress = base.Request.UserHostAddress,
                    PageUrl = "/Follow/DeletedoctorContent",
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
        /// 删除医嘱模板
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>

        [HttpPost, UnAuthorize]
        public JsonResult Deletedoctor(string ids)
        {
            try
            {
                IEnumerable<long> enumerable = from item in ids.Split(new char[] { ',' }) select long.Parse(item);
                FollowManagerApplication.Deletedoctor(enumerable, base.CurrentSellerManager.ShopId);
                LogInfo info = new LogInfo
                {
                    Date = DateTime.Now,
                    Description = "删除医嘱模板，Ids=" + ids,
                    IPAddress = base.Request.UserHostAddress,
                    PageUrl = "/Follow/Deletedoctor",
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
        /// 停止医嘱模板使用
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult doctorStop(string ids)
        {
            IEnumerable<long> enumerable = from item in ids.Split(new char[] { ',' }) select long.Parse(item);
            FollowManagerApplication.doctorStop(enumerable, base.CurrentSellerManager.ShopId);
            LogInfo info = new LogInfo
            {
                Date = DateTime.Now,
                Description = "停止医嘱模板使用，Ids=" + ids,
                IPAddress = base.Request.UserHostAddress,
                PageUrl = "/Follow/doctorStop",
                UserName = base.CurrentSellerManager.UserName,
                ShopId = base.CurrentSellerManager.ShopId
            };
            ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(info);

            return base.Json(new { success = true });
        }
        /// <summary>
        /// 启用医嘱模板
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult doctorOn(string ids)
        {
            IEnumerable<long> enumerable = from item in ids.Split(new char[] { ',' }) select long.Parse(item);
            FollowManagerApplication.doctorOn(enumerable, base.CurrentSellerManager.ShopId);
            LogInfo info = new LogInfo
            {
                Date = DateTime.Now,
                Description = "启用医嘱模板，Ids=" + ids,
                IPAddress = base.Request.UserHostAddress,
                PageUrl = "/Follow/doctorOn",
                UserName = base.CurrentSellerManager.UserName,
                ShopId = base.CurrentSellerManager.ShopId
            };
            ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(info);

            return base.Json(new { success = true });
        }
        /// <summary>
        /// 启用模板
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult BatchOn(string ids)
        {
            IEnumerable<long> enumerable = from item in ids.Split(new char[] { ',' }) select long.Parse(item);
            FollowManagerApplication.BatchOn(enumerable, base.CurrentSellerManager.ShopId);
            LogInfo info = new LogInfo
            {
                Date = DateTime.Now,
                Description = "启用模板，Ids=" + ids,
                IPAddress = base.Request.UserHostAddress,
                PageUrl = "/Follow/BatchOnSale",
                UserName = base.CurrentSellerManager.UserName,
                ShopId = base.CurrentSellerManager.ShopId
            };
            ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(info);

            return base.Json(new { success = true });
        }


        /// <summary>
        /// 停用模板
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult BatchStop(string ids)
        {
            IEnumerable<long> enumerable = from item in ids.Split(new char[] { ',' }) select long.Parse(item);
            FollowManagerApplication.BatchStop(enumerable, base.CurrentSellerManager.ShopId);
            LogInfo info = new LogInfo
            {
                Date = DateTime.Now,
                Description = "停用模板，Ids=" + ids,
                IPAddress = base.Request.UserHostAddress,
                PageUrl = "/Follow/BatchSaleOff",
                UserName = base.CurrentSellerManager.UserName,
                ShopId = base.CurrentSellerManager.ShopId
            };
            ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(info);
            return base.Json(new { success = true });
        }

    }
}