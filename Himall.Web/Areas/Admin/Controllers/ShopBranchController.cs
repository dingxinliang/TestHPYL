using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Plugins;
using Himall.DTO;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Areas.Admin.Models.Product;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class ShopBranchController : BaseAdminController
    {
        /// <summary>
        /// 标签管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Tags()
        {
            return (ActionResult)this.View();
        }
        /// <summary>
        /// 标签列表
        /// </summary>
        /// <returns></returns>
        public JsonResult TagList()
        {
            List<ShopBranchTagModel> shopBranchTagInfos = ShopBranchApplication.GetAllShopBranchTagInfos();
            return this.Json((object)new DataGridModel<ShopBranchTagModel>()
            {
                rows = (IEnumerable<ShopBranchTagModel>)shopBranchTagInfos,
                total = Enumerable.Count<ShopBranchTagModel>((IEnumerable<ShopBranchTagModel>)shopBranchTagInfos)
            });
        }
        /// <summary>
        /// 添加标签
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public JsonResult AddTag(string title)
        {
            try
            {
                ShopBranchApplication.AddShopBranchTagInfo(title);
                return this.Json((object)new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                return this.Json((object)new
                {
                    success = false,
                    msg = ex.Message
                });
            }
        }
        /// <summary>
        /// 编辑标签
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public JsonResult EditTag(long Id, string title)
        {
            try
            {
                ShopBranchApplication.UpdateShopBranchTagInfo(Id, title);
                return this.Json((object)new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                return this.Json((object)new
                {
                    success = false,
                    msg = ex.Message
                });
            }
        }
        /// <summary>
        /// 删除标签
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult DeleteTag(long Id)
        {
            try
            {
                ShopBranchApplication.DeleteShopBranchTagInfo(Id);
                return this.Json((object)new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                return this.Json((object)new
                {
                    success = false,
                    msg = ex.Message
                });
            }
        }
        /// <summary>
        /// 周边门店管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Management()
        {
            List<ShopBranchTagModel> allShopBranchTagInfos = ShopBranchApplication.GetAllShopBranchTagInfos();
            List<SelectListItem> list5 = new List<SelectListItem>();
            SelectListItem item3 = new SelectListItem
            {
                Selected = true
            };
            item3.Value = 0.ToString();
            item3.Text = "请选择...";
            list5.Add(item3);
            List<SelectListItem> list2 = list5;
            foreach (ShopBranchTagModel model in allShopBranchTagInfos)
            {
                SelectListItem item = new SelectListItem
                {
                    Selected = false,
                    Value = model.Id.ToString(),
                    Text = model.Title
                };
                list2.Add(item);
            }
            ((dynamic)base.ViewBag).ShopBranchTags = list2;
            List<ShopInfo> allShops = ObjectContainer.Current.Resolve<IShopService>().GetAllShops();
            List<SelectListItem> list6 = new List<SelectListItem>();
            SelectListItem item4 = new SelectListItem
            {
                Selected = true
            };
            item4.Value = 0.ToString();
            item4.Text = "请选择...";
            list6.Add(item4);
            List<SelectListItem> list4 = list6;
            foreach (ShopInfo info in allShops)
            {
                SelectListItem item2 = new SelectListItem
                {
                    Selected = false,
                    Value = info.Id.ToString(),
                    Text = info.ShopName
                };
                list4.Add(item2);
            }
            ((dynamic)base.ViewBag).Shops = list4;
            return base.View();

        }
        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="query"></param>
        /// <param name="rows"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public JsonResult List(ShopBranchQuery query, int rows, int page)
        {
            query.PageNo = page;
            query.PageSize = rows;
            if (query.AddressId.HasValue)
                query.AddressPath = RegionApplication.GetRegionPath(query.AddressId.Value);
            QueryPageModel<ShopBranch> shopBranchs = ShopBranchApplication.GetShopBranchs(query);
            return this.Json((object)new DataGridModel<ShopBranch>()
            {
                rows = (IEnumerable<ShopBranch>)shopBranchs.Models,
                total = shopBranchs.Total
            });
        }
        /// <summary>
        /// 门店冻结
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public JsonResult Freeze(long shopBranchId)
        {
            ShopBranchApplication.Freeze(shopBranchId);
            return this.Json((object)new
            {
                success = true,
                msg = "冻结成功！"
            });
        }
        /// <summary>
        /// 门店解冻
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public JsonResult UnFreeze(long shopBranchId)
        {
            ShopBranchApplication.UnFreeze(shopBranchId);
            return this.Json((object)new
            {
                success = true,
                msg = "解冻成功！"
            });
        }
        /// <summary>
        /// 设置门店标签
        /// </summary>
        /// <param name="shopIds"></param>
        /// <param name="tagIds"></param>
        /// <returns></returns>
        public JsonResult SetShopBranchTags(string shopIds, string tagIds)
        {
            try
            {
                string[] strs1 = shopIds.Split(new char[1]
        {
          ','
        }, StringSplitOptions.RemoveEmptyEntries);
                string[] strArray;
                if (!string.IsNullOrEmpty(tagIds))
                    strArray = tagIds.Split(new char[1]
          {
            ','
          }, StringSplitOptions.RemoveEmptyEntries);
                else
                    strArray = new string[0];
                string[] strs2 = strArray;
                ShopBranchApplication.SetShopBrandTagInfos(this.convertLongs(strs1), this.convertLongs(strs2));
                return this.Json((object)new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                return this.Json((object)new
                {
                    success = false,
                    msg = ex.Message
                });
            }
        }
        /// <summary>
        ///  类型转换
        /// </summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        private long[] convertLongs(string[] strs)
        {
            List<long> list = new List<long>();
            foreach (string s in strs)
            {
                long result = 0L;
                long.TryParse(s, out result);
                list.Add(result);
            }
            return list.ToArray();
        }
    }
}