using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Framework;
using Himall.Application;
using Himall.DTO;
using Himall.Core;
using Himall.CommonModel;
using Himall.Web.Models;
using Himall.IServices;
using Himall.Model;
using System.Drawing;
using System.IO;
using Himall.Core.Helper;
using System.Drawing.Imaging;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    [StoreAuthorization]
    public class ShopBranchController : BaseSellerController
    {
        // Methods
        public ActionResult Add()
        {
            List<ShopBranchTagModel> allShopBranchTagInfos = ShopBranchApplication.GetAllShopBranchTagInfos();
            List<SelectListItem> list2 = new List<SelectListItem>();
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
            ShopBranch branch = new ShopBranch
            {
                IsStoreDelive = true,
                ServeRadius = 0,
                DeliveFee = 0,
                DeliveTotalFee = 0,
                FreeMailFee = 0
            };
            return base.View(branch);
        }

        [HttpPost]
        public ActionResult Add(ShopBranch shopBranch)
        {
            try
            {
                long num;
                if (!string.Equals(shopBranch.PasswordOne, shopBranch.PasswordTwo))
                {
                    throw new HimallException("两次密码输入不一致！");
                }
                if (string.IsNullOrWhiteSpace(shopBranch.PasswordOne) || string.IsNullOrWhiteSpace(shopBranch.PasswordTwo))
                {
                    throw new HimallException("密码不能为空！");
                }
                if (shopBranch.ShopBranchName.Length > 15)
                {
                    throw new HimallException("门店名称不能超过15个字！");
                }
                if (shopBranch.AddressDetail.Length > 50)
                {
                    throw new HimallException("详细地址不能超过50个字！");
                }
                if ((shopBranch.Latitude <= 0f) || (shopBranch.Longitude <= 0f))
                {
                    throw new HimallException("请搜索地址地图定位！");
                }
                shopBranch.ShopId = base.CurrentSellerManager.ShopId;
                shopBranch.CreateDate = DateTime.Now;
                ShopBranchApplication.AddShopBranch(shopBranch, out num);
                try
                {
                    string[] strs = new string[] { num.ToString() };
                    string[] strArray2 = string.IsNullOrEmpty(shopBranch.ShopBranchTagId) ? new string[0] : shopBranch.ShopBranchTagId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    ShopBranchApplication.SetShopBrandTagInfos(this.convertLongs(strs), this.convertLongs(strArray2));
                }
                catch
                {
                }
                List<ShopBranchTagModel> allShopBranchTagInfos = ShopBranchApplication.GetAllShopBranchTagInfos();
                List<SelectListItem> list2 = new List<SelectListItem>();
                foreach (ShopBranchTagModel model in allShopBranchTagInfos)
                {
                    SelectListItem item = new SelectListItem
                    {
                        Selected = (shopBranch.ShopBranchTagId == null) ? false : (shopBranch.ShopBranchTagId.Split(new char[] { ',' }).Contains<string>(model.Id.ToString()) ? true : false),
                        Value = model.Id.ToString(),
                        Text = model.Title
                    };
                    list2.Add(item);
                }
                ((dynamic)base.ViewBag).ShopBranchTags = list2;
            }
            catch (Exception exception)
            {
                BaseController.Result result = new BaseController.Result
                {
                    success = false,
                    msg = exception.Message
                };
                return base.Json(result);
            }
            BaseController.Result data = new BaseController.Result
            {
                success = true
            };
            return base.Json(data);
        }

        private long[] convertLongs(string[] strs)
        {
            List<long> list = new List<long>();
            foreach (string str in strs)
            {
                long result = 0L;
                long.TryParse(str, out result);
                list.Add(result);
            }
            return list.ToArray();
        }

        [HttpPost]
        public ActionResult Edit(ShopBranch shopBranch)
        {
            try
            {
                if (!string.Equals(shopBranch.PasswordOne, shopBranch.PasswordTwo))
                {
                    throw new HimallException("两次密码输入不一致！");
                }
                if (shopBranch.ShopBranchName.Length > 15)
                {
                    throw new HimallException("门店名称不能超过15个字！");
                }
                if (shopBranch.AddressDetail.Length > 50)
                {
                    throw new HimallException("详细地址不能超过50个字！");
                }
                if ((shopBranch.Latitude <= 0f) || (shopBranch.Longitude <= 0f))
                {
                    throw new HimallException("请搜索地址地图定位！");
                }
                shopBranch.ShopId = base.CurrentSellerManager.ShopId;
                ShopBranch shopBranchById = ShopBranchApplication.GetShopBranchById(shopBranch.Id);
                if ((shopBranchById != null) && (shopBranchById.ShopId != shopBranch.ShopId))
                {
                    throw new HimallException("不能修改其他商家的门店！");
                }
                try
                {
                    string[] strs = new string[] { shopBranch.Id.ToString() };
                    string[] strArray2 = (shopBranch.ShopBranchTagId == null) ? new string[0] : shopBranch.ShopBranchTagId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    ShopBranchApplication.SetShopBrandTagInfos(this.convertLongs(strs), this.convertLongs(strArray2));
                }
                catch
                {
                }
                ShopBranchApplication.UpdateShopBranch(shopBranch);
            }
            catch (Exception exception)
            {
                BaseController.Result result = new BaseController.Result
                {
                    success = false,
                    msg = exception.Message
                };
                return base.Json(result);
            }
            BaseController.Result data = new BaseController.Result
            {
                success = true
            };
            return base.Json(data);
        }

        public ActionResult Edit(long id)
        {
            ShopBranch shopBranchById = ShopBranchApplication.GetShopBranchById(id);
            List<ShopBranchTagModel> allShopBranchTagInfos = ShopBranchApplication.GetAllShopBranchTagInfos();
            List<SelectListItem> list2 = new List<SelectListItem>();
            foreach (ShopBranchTagModel model in allShopBranchTagInfos)
            {
                SelectListItem item = new SelectListItem
                {
                    Selected = (shopBranchById.ShopBranchTagId == null) ? false : (shopBranchById.ShopBranchTagId.Split(new char[] { ',' }).Contains<string>(model.Id.ToString()) ? true : false),
                    Value = model.Id.ToString(),
                    Text = model.Title
                };
                list2.Add(item);
            }
            ((dynamic)base.ViewBag).ShopBranchTags = list2;
            return base.View(shopBranchById);
        }

        public JsonResult Freeze(long shopBranchId)
        {
            ShopBranchApplication.Freeze(shopBranchId);
            return base.Json(new { success = true, msg = "冻结成功！" });
        }

        public JsonResult List(ShopBranchQuery query, int rows, int page)
        {
            query.PageNo = page;
            query.PageSize = rows;
            query.ShopId = (int)base.CurrentSellerManager.ShopId;
            if (query.AddressId.HasValue)
            {
                query.AddressPath = RegionApplication.GetRegionPath(query.AddressId.Value);
            }
            QueryPageModel<ShopBranch> shopBranchs = ShopBranchApplication.GetShopBranchs(query);
            DataGridModel<ShopBranch> data = new DataGridModel<ShopBranch>
            {
                rows = shopBranchs.Models,
                total = shopBranchs.Total
            };
            return base.Json(data);
        }

        public ActionResult Management()
        {
            return base.View();
        }

        public ActionResult Setting()
        {
            Shop shop = ShopApplication.GetShop(base.CurrentSellerManager.ShopId, false);
            if (shop != null)
            {
                ((dynamic)base.ViewBag).AutoAllotOrder = shop.AutoAllotOrder;
            }
            return base.View();
        }

        [HttpPost]
        public JsonResult Setting(bool autoAllotOrder)
        {
            try
            {
                Shop model = ShopApplication.GetShop(base.CurrentSellerManager.ShopId, false);
                model.AutoAllotOrder = new bool?(autoAllotOrder);
                ShopApplication.UpdateShop(model);
                Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(base.CurrentSellerManager.ShopId, false));
                Cache.Remove(CacheKeyCollection.CACHE_SHOP(base.CurrentSellerManager.ShopId, false));
                LogInfo info = new LogInfo
                {
                    Date = DateTime.Now,
                    Description = string.Format("{0}:订单自动分配到门店", autoAllotOrder ? "开启" : "关闭"),
                    IPAddress = base.Request.UserHostAddress,
                    PageUrl = "/ShopBranch/Setting",
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

        public JsonResult StoresLink(string vshopUrl)
        {
            string str = string.Empty;
            if (!string.IsNullOrWhiteSpace(vshopUrl))
            {
                Bitmap bitmap = QRCodeHelper.Create(vshopUrl);
                MemoryStream stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Bmp);
                str = "data:image/gif;base64," + Convert.ToBase64String(stream.ToArray());
                stream.Dispose();
            }
            return base.Json(new { success = true, qrCodeImagePath = str }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UnFreeze(long shopBranchId)
        {
            ShopBranchApplication.UnFreeze(shopBranchId);
            return base.Json(new { success = true, msg = "解冻成功！" });
        }
    }
}