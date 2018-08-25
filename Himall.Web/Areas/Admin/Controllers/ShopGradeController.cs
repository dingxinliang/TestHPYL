using Himall.IServices;
using Himall.Web.Framework;

using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class ShopGradeController : BaseAdminController
    {
        private IShopService _iShopService;
        public ShopGradeController(IShopService iShopService)
        {
            _iShopService = iShopService;
        }
        /// <summary>
        /// 店铺等级管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Management()
        {
            return View();
        }
        /// <summary>
        /// 列表查询
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult List()
        {
            var shopG = _iShopService.GetShopGrades();
            IEnumerable<ShopGradeModel> shopGs = shopG.ToArray().Select(item => new ShopGradeModel()
            {
                Id = item.Id,
                ChargeStandard = item.ChargeStandard,
                ImageLimit = item.ImageLimit,
                ProductLimit = item.ProductLimit,
                Name = item.Name,

            });

            DataGridModel<ShopGradeModel> dataGrid = new DataGridModel<ShopGradeModel>() { rows = shopGs, total = shopG.Count() };
            return Json(dataGrid);
        }
        /// <summary>
        /// 编辑店铺等级
        /// </summary>
        /// <param name="shopG"></param>
        /// <returns></returns>
        [UnAuthorize]
        [HttpPost]
        public ActionResult Edit(ShopGradeModel shopG)
        {
            if (ModelState.IsValid)
            {
                _iShopService.UpdateShopGrade(shopG);
                return RedirectToAction("Management");
            }
            return View(shopG);
        }
        /// <summary>
        /// 编辑跳转
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit(long id)
        {
            return View(new ShopGradeModel(_iShopService.GetShopGrade(id)));
        }
        /// <summary>
        /// 添加跳转
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }
        /// <summary>
        /// 添加店铺等级
        /// </summary>
        /// <param name="shopG"></param>
        /// <returns></returns>

        [HttpPost]
        [UnAuthorize]
        public ActionResult Add(ShopGradeModel shopG)
        {
           if (ModelState.IsValid)
            {
                _iShopService.AddShopGrade(shopG);
                return RedirectToAction("Management");
            }
            return View(shopG);
        }
        /// <summary>
        /// 删除店铺登录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult DeleteShopGrade(long id)
        {
            string msg = "";
            _iShopService.DeleteShopGrade(id,out msg);
            if (string.IsNullOrWhiteSpace(msg))
            {
                return Json(new { Successful = true });
            }
            else
            {
                return Json(new { Successful = false,msg=msg });
            }
        }
    }
}