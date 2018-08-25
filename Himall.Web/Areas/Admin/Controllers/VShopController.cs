using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Core;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class VShopController : BaseAdminController
    {
        IVShopService _iVShopService;
        ICategoryService _iCategoryService;
        IShopCategoryService _iShopCategoryService;
        public VShopController( IVShopService iVShopService , ICategoryService iCategoryService , IShopCategoryService iShopCategoryService )
        {
            _iVShopService = iVShopService;
            _iCategoryService = iCategoryService;
            _iShopCategoryService = iShopCategoryService;
        }
        /// <summary>
        /// 首页跳转
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 微点管理
        /// </summary>
        /// <returns></returns>
        public ActionResult VShopManagement()
        {
            return View();
        }
        /// <summary>
        /// 详细信息查询
        /// </summary>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="vshopName"></param>
        /// <param name="vshopType"></param>
        /// <param name="oldVShopId"></param>
        /// <returns></returns>
        public JsonResult GetVshops(int page, int rows, string vshopName, int? vshopType = null, long? oldVShopId = null)
        {
            int total = 0;
            VshopQuery vshopQuery = new VshopQuery()
            {
                Name = vshopName ,
                PageNo = page ,
                PageSize = rows,
                ExcepetVshopId = oldVShopId,
            };
            if( vshopType == 1 )
                vshopQuery.VshopType = VShopExtendInfo.VShopExtendType.TopShow;
            if( vshopType == 2 )
                vshopQuery.VshopType = VShopExtendInfo.VShopExtendType.HotVShop;
            if( vshopType == 0 )
                vshopQuery.VshopType = 0;
            var pmdata = _iVShopService.GetVShopByParamete( vshopQuery );
            var vshops = pmdata.Models.ToList();
            total = pmdata.Total;
            var categoryService = _iCategoryService;
            var shopService = _iShopCategoryService;
            var model = vshops.ToArray().Select( item => new
            {
                id = item.Id ,
                name = item.Name ,
                creatTime = item.CreateTime.ToString() ,
                vshopTypes = item.VShopExtendInfo.Any( t => t.Type == VShopExtendInfo.VShopExtendType.TopShow ) ? "主推微店" : item.VShopExtendInfo.Any( t => t.Type == VShopExtendInfo.VShopExtendType.HotVShop ) ? "热门微店" : "普通微店" ,
                categoryName = shopService.GetBusinessCategory( item.ShopId ).FirstOrDefault() != null ? categoryService.GetCategory( long.Parse( shopService.GetBusinessCategory( item.ShopId ).FirstOrDefault().Path.Split( '|' ).First() ) ).Name : "" ,
                visiteNum = item.VisitNum ,
                buyNum = item.buyNum , 
                StateStr = item.State.ToDescription() ,
                State = (int)item.State
            } );
            return Json( new { rows = model , total = total } );

        }
        /// <summary>
        /// 设置推荐
        /// </summary>
        /// <param name="vshopId"></param>
        /// <returns></returns>
        public JsonResult SetTopVshop( long vshopId )
        {
            _iVShopService.SetTopShop( vshopId );
            return Json( new { success = true } );
        }
        /// <summary>
        /// 设置热门店铺
        /// </summary>
        /// <param name="vshopId"></param>
        /// <returns></returns>
        public JsonResult SetHotVshop( long vshopId )
        {
            _iVShopService.SetHotShop( vshopId );
            return Json( new { success = true } );
        }
        /// <summary>
        /// 删除微店
        /// </summary>
        /// <param name="vshopId"></param>
        /// <returns></returns>
        public JsonResult DeleteVshop( long vshopId )
        {
            _iVShopService.CloseShop( vshopId );
            return Json( new { success = true } );
        }

        #region 详细方法
        [HttpPost]
        public ActionResult SetShopNormal(long vshopId)
        {
            _iVShopService.SetShopNormal(vshopId);
            return Json(new { success = true });
        }
        public ActionResult HotVShop()
        {
            return View();
        }
        public JsonResult GetHotShop(int page, int rows, string vshopName, DateTime? startTime = null, DateTime? endTime = null)
        {
            int total;
            VshopQuery vshopQuery = new VshopQuery()
            {
                PageNo = page,
                PageSize = rows,
                Name = vshopName
            };
            var vshops = _iVShopService.GetHotShop(vshopQuery, startTime, endTime, out total).ToList();
            total = vshops.Count();
            var model = vshops.Select(item => new
            {
                id = item.Id,
                name = item.Name,
                squence = item.VShopExtendInfo.FirstOrDefault().Sequence,
                addTime = item.VShopExtendInfo.FirstOrDefault().AddTime.ToString(),
                creatTime = item.CreateTime.ToString(),
                visiteNum = item.VisitNum,
                buyNum = item.buyNum
            });
            return Json(new { rows = model, total = total });
        }

        public JsonResult DeleteHotVShop(int vshopId)
        {
            _iVShopService.DeleteHotShop(vshopId);
            return Json(new { success = true });
        }


        public JsonResult ReplaceHotShop(long oldVShopId, long newHotVShopId)
        {
            _iVShopService.ReplaceHotShop(oldVShopId, newHotVShopId);
            return Json(new { success = true });
        }
        #endregion

        /// <summary>
        /// 更新信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public JsonResult UpdateSequence( long id , int? sequence )
        {
            _iVShopService.UpdateSequence( id , sequence );
            return Json( new { success = true } );
        }
        /// <summary>
        /// 推荐
        /// </summary>
        /// <returns></returns>
        public ActionResult TopShop()
        {
            var vshop = _iVShopService.GetTopShop();

            return View( vshop );
        }
        /// <summary>
        /// 删除推荐
        /// </summary>
        /// <param name="vshopId"></param>
        /// <returns></returns>
        public JsonResult DeleteTopShow( long vshopId )
        {
            _iVShopService.DeleteTopShop( vshopId );
            return Json( new { success = true } );
        }
        /// <summary>
        /// 替换推荐店铺
        /// </summary>
        /// <param name="oldVShopId"></param>
        /// <param name="newVShopId"></param>
        /// <returns></returns>
        public JsonResult ReplaceTopShop( long oldVShopId , long newVShopId )
        {
            _iVShopService.ReplaceTopShop( oldVShopId , newVShopId );
            return Json( new { success = true } );
        }
    }
}