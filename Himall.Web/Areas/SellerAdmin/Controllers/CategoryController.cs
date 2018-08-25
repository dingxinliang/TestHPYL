using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class CategoryController : BaseSellerController
    {
        // Fields
        private ICategoryService _iCategoryService;
        private IShopCategoryService _iShopCategoryService;

        // Methods
        public CategoryController(ICategoryService iCategoryService, IShopCategoryService iShopCategoryService)
        {
            this._iCategoryService = iCategoryService;
            this._iShopCategoryService = iShopCategoryService;
        }

        [UnAuthorize, HttpPost]
        public JsonResult BatchDeleteCategory(string Ids)
        {
            foreach (string str in Ids.Split(new char[] { '|' }))
            {
                int num;
                if (!string.IsNullOrWhiteSpace(str) && int.TryParse(str, out num))
                {
                    this._iShopCategoryService.DeleteCategory((long)num, base.CurrentSellerManager.ShopId);
                }
            }
            return base.Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [ShopOperationLog("创建店铺分类", "pid,name"), HttpPost]
        public JsonResult CreateCategory(string name, long pId)
        {
            if (string.IsNullOrWhiteSpace(name) || (name.Length > 12))
            {
                throw new Exception("分类名称长度不能多于12位");
            }
            ShopCategoryInfo model = new ShopCategoryInfo
            {
                Name = name,
                ParentCategoryId = pId,
                IsShow = true,
                DisplaySequence = this._iShopCategoryService.GetCategoryByParentId(pId).Count<ShopCategoryInfo>() + 1,
                ShopId = base.CurrentSellerManager.ShopId
            };
            this._iShopCategoryService.AddCategory(model);
            return base.Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ShopOperationLog("删除店铺分类", "id")]
        public JsonResult DeleteCategoryById(long id)
        {
            this._iShopCategoryService.DeleteCategory(id, base.CurrentSellerManager.ShopId);
            return base.Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [UnAuthorize, HttpPost]
        public JsonResult GetCategory(long? key = new long?(), int? level = -1)
        {
            if (level == -1)
            {
                key = 0L;
            }
            if (key.HasValue)
            {
                IEnumerable<KeyValuePair<long, string>> data = from item in this._iShopCategoryService.GetCategoryByParentId(key.Value, base.CurrentSellerManager.ShopId) select new KeyValuePair<long, string>(item.Id, item.Name);
                return base.Json(data);
            }
            return base.Json(new object[0]);
        }

        [UnAuthorize]
        public ActionResult GetCategoryByParentId(int id)
        {
            List<ShopCategoryModel> list = new List<ShopCategoryModel>();
            IEnumerable<ShopCategoryInfo> categoryByParentId = this._iShopCategoryService.GetCategoryByParentId((long)id);
            foreach (ShopCategoryInfo info in categoryByParentId)
            {
                list.Add(new ShopCategoryModel(info));
            }
            return base.Json(new { success = true, Category = list }, JsonRequestBehavior.AllowGet);
        }

        [UnAuthorize]
        public JsonResult GetCategoryDrop(long id = 0L)
        {
            List<SelectListItem> list2 = new List<SelectListItem>();
            SelectListItem item2 = new SelectListItem
            {
                Selected = id == 0L,
                Text = "请选择...",
                Value = "0"
            };
            list2.Add(item2);
            List<SelectListItem> list = list2;
            IEnumerable<ShopCategoryInfo> mainCategory = this._iShopCategoryService.GetMainCategory(base.CurrentSellerManager.ShopId);
            foreach (ShopCategoryInfo info in mainCategory)
            {
                SelectListItem item = new SelectListItem
                {
                    Selected = id == info.Id,
                    Text = info.Name,
                    Value = info.Id.ToString()
                };
                list.Add(item);
            }
            return base.Json(new { success = true, category = list }, JsonRequestBehavior.AllowGet);
        }

        [UnAuthorize, HttpPost]
        public JsonResult GetEffectCategory(long categoryId)
        {
            CategoryInfo category = this._iCategoryService.GetCategory(categoryId);
            string effectCategoryName = this._iCategoryService.GetEffectCategoryName(base.CurrentSellerManager.ShopId, category.TypeId);
            return base.Json(new { json = effectCategoryName }, JsonRequestBehavior.AllowGet);
        }

        [UnAuthorize, HttpPost]
        public JsonResult GetSystemCategory(long? key = new long?(), int? level = -1)
        {
            if (level == -1)
            {
                key = 0L;
            }
            if (key.HasValue)
            {
                IEnumerable<KeyValuePair<long, string>> data = from item in this._iCategoryService.GetCategoryByParentId(key.Value) select new KeyValuePair<long, string>(item.Id, item.Name);
                return base.Json(data);
            }
            return base.Json(new object[0]);
        }

        public ActionResult Management()
        {
            IEnumerable<ShopCategoryInfo> mainCategory = this._iShopCategoryService.GetMainCategory(base.CurrentSellerManager.ShopId);
            List<ShopCategoryModel> model = new List<ShopCategoryModel>();
            foreach (ShopCategoryInfo info in mainCategory)
            {
                model.Add(new ShopCategoryModel(info));
            }
            return base.View(model);
        }

        [ShopOperationLog("修改店铺分类名称", "id,name")]
        public JsonResult UpdateName(string name, long id)
        {
            this._iShopCategoryService.UpdateCategoryName(id, name);
            return base.Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [UnAuthorize]
        public JsonResult UpdateOrder(long order, long id)
        {
            this._iShopCategoryService.UpdateCategoryDisplaySequence(id, order);
            return base.Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

    }
}