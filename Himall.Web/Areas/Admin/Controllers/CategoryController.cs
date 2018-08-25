using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.Admin.Models.Product;
using Himall.Web.Framework;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Himall.Application;
using System.Threading.Tasks;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class CategoryController : BaseAdminController
    {
        private ICategoryService _iCategoryService;
        private ITypeService _iTypeService;
        private ISearchProductService _iSearchProductService;
        public CategoryController(ICategoryService iCategoryService, ITypeService iTypeService,ISearchProductService iSearchProductService)
        {
            _iCategoryService = iCategoryService;
            _iTypeService = iTypeService;
            _iSearchProductService = iSearchProductService;
        }
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 读取所有分类列表
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetCatgegotyList()
        {
            List<SelectListItem> cateList = new List<SelectListItem>{ new SelectListItem
                {
                    Selected = false,
                    Text ="请选择...",
                    Value = "0"
                }
            };
            var cate = _iCategoryService.GetFirstAndSecondLevelCategories();
            foreach (var item in cate)
            {
                StringBuilder space = new StringBuilder();
                for (int i = 1; i < item.Depth; i++)
                {
                    space.Append("&nbsp;&nbsp;&nbsp;");
                }
                cateList.Add(new SelectListItem
                {
                    Selected = false,
                    Text = space + item.Name,
                    Value = item.Id.ToString()
                });
            }
            return cateList;
        }
        /// <summary>
        /// 读取类型
        /// </summary>
        /// <param name="selectId"></param>
        /// <returns></returns>
        private List<SelectListItem> GetTypesList(long selectId = -1)
        {
            var types = _iTypeService.GetTypes();
            List<SelectListItem> typesList = new List<SelectListItem> { new SelectListItem
                {
                    Selected = false,
                    Text ="请选择...",
                    Value = "-1"
                }
            };
            foreach (var item in types)
            {
                if (item.Id != selectId)
                {
                    typesList.Add(new SelectListItem
                    {
                        Selected = false,
                        Text = item.Name,
                        Value = item.Id.ToString()
                    });
                }
                else
                {
                    typesList.Add(new SelectListItem
                    {
                        Selected = true,
                        Text = item.Name,
                        Value = item.Id.ToString()
                    });

                }
            }
            return typesList;
        }
        /// <summary>
        /// 添加分类
        /// </summary>
        /// <returns></returns>
        [UnAuthorize]
        public ActionResult Add()
        {
            if (null == TempData["Categories"])
                TempData["Categories"] = GetCatgegotyList();
            if (null == TempData["Types"])
                TempData["Types"] = GetTypesList();
            if (null == TempData["Depth"])
                TempData["Depth"] = 1;
            return View();
        }
        /// <summary>
        /// 分类深度(级数)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetCateDepth(long id)
        {
            return Json(new { successful = true, depth = _iCategoryService.GetCategory(id).Depth }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 根据父类来添加子类
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [UnAuthorize]
        public ActionResult AddByParent(long Id)
        {
            var cateList = GetCatgegotyList();
            cateList.FirstOrDefault(c => c.Value.Equals(Id.ToString())).Selected = true;
            TempData["Categories"] = cateList;
            var cate = _iCategoryService.GetCategory(Id);
            var typeId = cate.TypeId.ToString();
            var typeList = GetTypesList();
            var t = typeList.FirstOrDefault(c => c.Value.Equals(typeId));
            if (t == null) typeList.FirstOrDefault().Selected = true;
            else
            {
                t.Selected = true;
            }
            TempData["Types"] = typeList;
            TempData["Depth"] = cate.Depth;
            return RedirectToAction("Add");
        }
        /// <summary>
        /// 添加平台分类
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [UnAuthorize]
        [OperationLog(Message = "添加平台分类")]
        [HttpPost]
        public ActionResult Add(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                var ICategory = _iCategoryService;
                var cates = _iCategoryService.GetCategories();
                var isExists = cates.Any(c => c.ParentCategoryId == category.ParentCategoryId && c.Name.Equals(category.Name));
                bool dataisok = true;
                if (isExists)
                {
                    dataisok = false;
                    //throw new Himall.Core.HimallException("已存在相同分类名");
                    //ViewBag.Categories = _iCategoryService.GetFirstAndSecondLevelCategories();
                    //ViewBag.Types = _iTypeService.GetTypes();
                    ViewBag.Msg = "已存在相同分类名";
                    //return View();
                }
                //判断类型是否存在
                if (_iTypeService.GetType(category.TypeId) == null)
                {
                    dataisok = false;
                    ViewBag.Msg = "类型不存在，请刷新页面";
                }
                if(!dataisok)
                {
                    if (null == TempData["Categories"])
                        TempData["Categories"] = GetCatgegotyList();
                    if (null == TempData["Types"])
                        TempData["Types"] = GetTypesList();
                    if (null == TempData["Depth"])
                        TempData["Depth"] = 1;
                    return View();
                }
                ProcessingParentCategoryId(category);
                ProcessingDepth(category, ICategory);
                ProcessingPath(category, ICategory);
                ProcessingDisplaySequence(category, ICategory);
                ProcessingIcon(category, ICategory);
                ICategory.AddCategory(category);
				return Redirect(Url.Action("Management") + "#add_" + category.ParentCategoryId);
            }
            else
            {
                TempData["Categories"] = GetCatgegotyList();// _iCategoryService.GetFirstAndSecondLevelCategories();
                TempData["Types"] = GetTypesList();  //ViewBag.Types = _iTypeService.GetTypes();
                return View(category);
            }

        }
        /// <summary>
        /// 分类编辑
        /// </summary>
        /// <param name="Id">分类编号</param>
        /// <returns></returns>
        public ActionResult Edit(long Id = 0)
        {
            var cateInfo = _iCategoryService.GetCategory(Id);
            ViewBag.Depth = cateInfo.Depth;
            ViewBag.Types = GetTypesList(cateInfo.TypeId);
            cateInfo.CommisRate = Math.Round(cateInfo.CommisRate, 2);
            return View(new CategoryModel(cateInfo));
        }
        /// <summary>
        /// 修改平台分类
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpPost]
        [OperationLog("修改平台分类", "Id")]
        public ActionResult Edit(CategoryModel category)
        {
            if ((category.Depth == 3 && ModelState.IsValid) || category.Depth != 3)
            {
                var ICategory = _iCategoryService;
                var cates = _iCategoryService.GetCategories();
                var isExists = cates.Any(c => c.ParentCategoryId == category.ParentCategoryId && c.Name.Equals(category.Name) && c.Id != category.Id);
                if (!isExists)
                {
                    ProcessingIcon(category, ICategory);

                    //判断类型是否存在
                    if (_iTypeService.GetType(category.TypeId) != null)
                    {
						ICategory.UpdateCategory(category);
                        Task.Factory.StartNew(() =>
                        {
                            _iSearchProductService.UpdateCategory(category);
                        });
                       
						return Redirect(Url.Action("Management") + "#edit_" + category.Id);
                    }
                    else
                    {
                        ViewBag.Error = "类型不存在，请刷新页面";
                    }
                }
                else
                {
                    ViewBag.Error = "已存在相同分类名";
                    ViewBag.Types = GetTypesList(category.TypeId);
                    ViewBag.Depth = category.Depth;
                    return View(category);
                }
            }
            ViewBag.Types = GetTypesList(category.TypeId);
            ViewBag.Depth = category.Depth;
            return View(category);
        }
        /// <summary>
        /// 分类管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Management()
        {
            var ICategory = _iCategoryService;
            var categories = ICategory.GetCategories().ToList();
            var firstLevel = categories.Where(c => c.Depth == 1).OrderBy(c => c.DisplaySequence);
            List<CategoryModel> list = new List<CategoryModel>();
            foreach (var item in firstLevel)
            {
                list.Add(new CategoryModel(item));
                AddChildCategory(list, categories, item.Id);
            }
            return View(list);
        }
        /// <summary>
        /// 添加子类
        /// </summary>
        /// <param name="list"></param>
        /// <param name="categories"></param>
        /// <param name="pid"></param>
        void AddChildCategory(List<CategoryModel> list, List<CategoryInfo> categories, long pid)
        {
            var childCategories = categories.Where(c => c.ParentCategoryId == pid).OrderBy(c => c.DisplaySequence);
            if (childCategories.Count() == 0)
                return;
            foreach (var item in childCategories)
            {
                list.Add(new CategoryModel(item));
                AddChildCategory(list, categories, item.Id);
            }
        }
        /// <summary>
        /// 获取非叶子节点分类数据（包含Id、ParentId和名称）
        /// </summary>
        /// <returns></returns>
        [UnAuthorize]
        public ActionResult GetNonLeafCategoryList()
        {
            var category = _iCategoryService.GetFirstAndSecondLevelCategories();
            List<CategoryDropListModel> data = new List<CategoryDropListModel>();
            foreach (var item in category)
            {
                data.Add(new CategoryDropListModel
                {
                    Id = item.Id,
                    ParentCategoryId = item.ParentCategoryId,
                    Name = item.Name,
                    Depth = item.Depth
                });
            }
            return Json(new
            {
                Successful = true,
                list = data.OrderBy(d => d.ParentCategoryId).ThenBy(d => d.Depth).ThenBy(d => d.Id).ToList()
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 根据父类来查询子类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [UnAuthorize]
        public ActionResult GetCategoryByParentId(int id)
        {
            List<CategoryModel> list = new List<CategoryModel>();
            var categoryList = _iCategoryService.GetCategoryByParentId(id).OrderBy(r => r.DisplaySequence);
            foreach (var item in categoryList)
            {
                list.Add(new CategoryModel(item));
            }
            return Json(new { Successfly = true, Category = list }, JsonRequestBehavior.AllowGet);
        }
        #region 深度
        /// <summary>
        /// 深度
        /// </summary>
        /// <param name="model"></param>
        /// <param name="IProductCategory"></param>
        private void ProcessingPath(CategoryModel model, ICategoryService IProductCategory)
        {
            var maxId = IProductCategory.GetMaxCategoryId() + 1;
            model.Id = maxId;
            if (model.ParentCategoryId == 0)
            {
                model.Path = maxId.ToString();
            }
            else
            {
                var category = IProductCategory.GetCategory(model.ParentCategoryId);
                model.Path = string.Format("{0}|{1}", category.Path, maxId);
            }
        }
        private void ProcessingDepth(CategoryModel model, ICategoryService IProductCategory)
        {
            if (model.ParentCategoryId == 0) model.Depth = 1;
            else
            {
                var category = IProductCategory.GetCategory(model.ParentCategoryId);
                model.Depth = category.Depth + 1;
            }
        }
        #endregion
        private void ProcessingParentCategoryId(CategoryModel model)
        {
            // if (model.ParentCategoryId == null) model.ParentCategoryId = 0;
        }
        private void ProcessingDisplaySequence(CategoryModel model, ICategoryService IProductCategory)
        {
            var index = IProductCategory.GetCategoryByParentId(model.ParentCategoryId).Count() + 1;
            model.DisplaySequence = index;
        }
        /// <summary>
        /// 图标
        /// </summary>
        /// <param name="model"></param>
        /// <param name="IProductCategory"></param>
        private void ProcessingIcon(CategoryModel model, ICategoryService IProductCategory)
        {
            if (!string.IsNullOrWhiteSpace(model.Icon))
            {
                //转移图片
                if (model.Icon.Contains("/temp/"))
                {
                    string source = model.Icon.Substring(model.Icon.LastIndexOf("/temp"));
                    string dest = @"/Storage/Plat/ImageAd/";
                    model.Icon = Path.Combine(dest, Path.GetFileName(source));
                    Core.HimallIO.CopyFile(source, model.Icon, true);
                }
                else if (model.Icon.Contains("/Storage/"))
                {
                    model.Icon = model.Icon.Substring(model.Icon.LastIndexOf("/Storage/"));
                }
            }

        }
        /// <summary>
        /// 管理页面修改名称
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        [UnAuthorize]
        public JsonResult UpdateName(string name, long id,int depth)
        {
            try
            {
                this._iCategoryService.UpdateCategoryName(id, name);
                CategoryInfo temp = new CategoryInfo
                {
                    Id = id,
                    Name = name,
                    Depth = depth
                };
                Task.Factory.StartNew(delegate
                {
                    this._iSearchProductService.UpdateCategory(temp);
                });
            }
            catch (Exception exception)
            {
                BaseController.Result result = new BaseController.Result
                {
                    success = false,
                    msg = exception.Message
                };
                return base.Json(result, JsonRequestBehavior.AllowGet);
            }
            BaseController.Result data = new BaseController.Result
            {
                success = true
            };
            return base.Json(data, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 管理页面更改排序
        /// </summary>
        /// <param name="order"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [UnAuthorize]
        public JsonResult UpdateOrder(long order, long id)
        {
            this._iCategoryService.UpdateCategoryDisplaySequence(id, order);
            Result data = new BaseController.Result
            {
                success = true
            };
            return base.Json(data, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 管理页面修改分成
        /// </summary>
        /// <param name="commis"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult UpdateCommis(decimal commis, long id)
        {
            this._iCategoryService.UpdateCategoryCommis(id, commis);
            Result data = new BaseController.Result
            {
                success = true
            };
            return base.Json(data, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 删除平台分类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationLog("删除平台分类", "Id")]
        [HttpPost]
        public JsonResult DeleteCategoryById(long id)
        {
			CategoryApplication.DeleteCategory(id); //用AOP来做
            //ServiceHelper.Create<IOperationLogService>().AddPlatformOperationLog(
            //       new LogInfo
            //       {
            //           Date = DateTime.Now,
            //           Description = "删除平台分类，Id=" + id,
            //           IPAddress = Request.UserHostAddress,
            //           PageUrl = "/Category/DeleteCategoryById/" + id,
            //           UserName = CurrentManager.UserName,
            //           ShopId = 0
            //       });
            Result data = new BaseController.Result{success = true};
            return base.Json(data, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 删除平台分类（多选）
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        [OperationLog("删除平台分类", "Ids")]
        [HttpPost]
        public JsonResult BatchDeleteCategory(string Ids)
        {
            foreach (string str in Ids.Split(new char[] { '|' }))
            {
                int num;
                if (!string.IsNullOrWhiteSpace(str) && int.TryParse(str, out num))
                {
                    CategoryApplication.DeleteCategory((long)num);
                }
            }
            BaseController.Result data = new BaseController.Result
            {
                success = true
            };
            return base.Json(data, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 查询分类信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetCategoryById(long id)
        {
            var model = new CategoryModel(_iCategoryService.GetCategory(id));
            return Json(new { Successful = true, category = model }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 查询分类信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        [UnAuthorize]
        [HttpPost]
        public JsonResult GetCategory(long? key = null, int? level = -1)
        {
            if (level == -1)
                key = 0;    
            if (key.HasValue)
            {
                var categories = _iCategoryService.GetCategoryByParentId(key.Value);
                var cateoriesPair = categories.Select(item => new KeyValuePair<long, string>(item.Id, item.Name));
                return Json(cateoriesPair);
            }
            else
                return Json(new object[] { });
        }
        [HttpPost]
        public JsonResult GetValidCategories(long? key = null, int? level = -1)
        {
            var categories = _iCategoryService.GetValidBusinessCategoryByParentId(key.GetValueOrDefault());
            var models = categories.Select(item => new KeyValuePair<long, string>(item.Id, item.Name));
            return Json(models);
        }
        /// <summary>
        /// 根据父ID查询子类
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [UnAuthorize]
        public JsonResult GetSecondAndThirdCategoriesByTopId(long id)
        {
            ICategoryService service = this._iCategoryService;
            CategoryTreeModel[] modelArray = (from item in service.GetCategoryByParentId(id) select new CategoryTreeModel { Id = item.Id, Name = item.Name, ParentCategoryId = item.ParentCategoryId, Depth = item.Depth }).ToArray<CategoryTreeModel>();
            foreach (CategoryTreeModel model in modelArray)
            {
                model.Children = from item in service.GetCategoryByParentId(model.Id) select new CategoryTreeModel { Id = item.Id, Name = item.Name, ParentCategoryId = item.ParentCategoryId, Depth = item.Depth };
            }
            return base.Json(new { success = true, categoies = modelArray }, JsonRequestBehavior.AllowGet);

        }
    }
}