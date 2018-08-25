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
    public class WdgjApiController : BaseSellerController
    {
        // Fields
        private IShopService _iShopService;
        private long CurShopId;

        /// <summary>
        /// 网店管家
        /// </summary>
        /// <param name="iShopService"></param>
        public WdgjApiController(IShopService iShopService)
        {
            this._iShopService = iShopService;
            if (base.CurrentSellerManager != null)
            {
                this.CurShopId = base.CurrentSellerManager.ShopId;
            }
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="wdgj"></param>
        /// <returns></returns>
        [HttpPost, UnAuthorize]
        public JsonResult Add(WdgjApiModel wdgj)
        {
            IShopService service = this._iShopService;
            ShopWdgjSetting setting = new ShopWdgjSetting
            {
                Id = wdgj.Id,
                ShopId = this.CurShopId,
                uCode = wdgj.uCode,
                uSign = wdgj.uSign
            };
            if (setting.Id > 0L)
            {
                service.UpdateShopWdgj(setting);
            }
            else
            {
                service.AddShopWdgj(setting);
            }
            return base.Json(new { success = true });
        }
        /// <summary>
        /// 首页跳转
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ShopWdgjSetting shopWdgjInfoById = this._iShopService.GetshopWdgjInfoById(this.CurShopId);
            WdgjApiModel model = new WdgjApiModel
            {
                Id = (shopWdgjInfoById != null) ? shopWdgjInfoById.Id : 0L,
                uCode = (shopWdgjInfoById != null) ? shopWdgjInfoById.uCode : "",
                uSign = (shopWdgjInfoById != null) ? shopWdgjInfoById.uSign : ""
            };
            return base.View(model);
        }

    }
}