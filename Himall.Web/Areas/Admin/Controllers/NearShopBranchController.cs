using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Himall.Core;
using System.Web;
using System.Web.Mvc;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Models;
using Himall.Core.Helper;
using System.IO;
using Himall.Application;
using Himall.IServices;
using Himall.DTO;
using Himall.CommonModel;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class NearShopBranchController : BaseAdminController
    {
        // Fields
        private IMobileHomeTopicService _iMobileHomeTopicService;
        private ISlideAdsService _iSlideAdsService;
        private ITopicService _iTopicService;

        // Methods
        public NearShopBranchController(IMobileHomeTopicService iMobileHomeTopicService, ISlideAdsService iSlideAdsService, ITopicService iTopicService)
        {
            this._iMobileHomeTopicService = iMobileHomeTopicService;
            this._iSlideAdsService = iSlideAdsService;
            this._iTopicService = iTopicService;
        }

        public JsonResult AddNearShopBranchIcon(string id, string description, string imageUrl, string url)
        {
            BaseController.Result data = new BaseController.Result();
            SlideAdInfo models = new SlideAdInfo
            {
                Id = Convert.ToInt64(id),
                ImageUrl = imageUrl,
                TypeId = SlideAdInfo.SlideAdType.NearShopBranchIcon,
                Url = url.ToLower(),
                Description = description,
                ShopId = 0L
            };
            if (models.Id > 0L)
            {
                this._iSlideAdsService.UpdateSlidAd(models);
            }
            else
            {
                this._iSlideAdsService.AddSlidAd(models);
            }
            data.success = true;
            return base.Json(data);
        }

        public JsonResult AddSlideImage(string id, string description, string imageUrl, string url, int slideTypeId)
        {
            BaseController.Result data = new BaseController.Result();
            SlideAdInfo models = new SlideAdInfo
            {
                Id = Convert.ToInt64(id),
                ImageUrl = imageUrl,
                TypeId = (SlideAdInfo.SlideAdType)slideTypeId,
                Url = url.ToLower(),
                Description = description,
                ShopId = 0L
            };
            if (models.Id > 0L)
            {
                this._iSlideAdsService.UpdateSlidAd(models);
            }
            else
            {
                this._iSlideAdsService.AddSlidAd(models);
            }
            data.success = true;
            return base.Json(data);
        }

        [HttpPost]
        public JsonResult DeleteNearShopBranchIcon(string id)
        {
            BaseController.Result data = new BaseController.Result();
            this._iSlideAdsService.DeleteSlidAd(0L, Convert.ToInt64(id));
            data.success = true;
            return base.Json(data);
        }

        [HttpPost]
        public JsonResult DeleteSlideImage(string id)
        {
            BaseController.Result data = new BaseController.Result();
            this._iSlideAdsService.DeleteSlidAd(0L, Convert.ToInt64(id));
            data.success = true;
            return base.Json(data);
        }

        public JsonResult GetImageAd(long id)
        {
            SlideAdInfo slidAd = this._iSlideAdsService.GetSlidAd(0L, id);
            if (null != slidAd)
            {
                return base.Json(new { success = true, imageUrl = HimallIO.GetImagePath(slidAd.ImageUrl, null), url = slidAd.Url }, JsonRequestBehavior.AllowGet);
            }
            return base.Json(new { success = false, msg = "未获取到项目" }, JsonRequestBehavior.AllowGet);
        }

        private string GetLinkName(string url)
        {
            string[] strArray = url.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strArray.Length == 2)
            {
                string s = strArray[1].Substring(strArray[1].LastIndexOf('/') + 1);
                long result = 0L;
                long.TryParse(s, out result);
                string str3 = strArray[0];
                if (str3 != null)
                {
                    if (!(str3 == "1"))
                    {
                        if (str3 == "2")
                        {
                            ShopBranch shopBranchById = ShopBranchApplication.GetShopBranchById(result);
                            if (null != shopBranchById)
                            {
                                return shopBranchById.ShopBranchName;
                            }
                        }
                        else if (str3 == "3")
                        {
                            TopicInfo topicInfo = this._iTopicService.GetTopicInfo(result);
                            if (null != topicInfo)
                            {
                                return topicInfo.Name;
                            }
                        }
                    }
                    else
                    {
                        ShopBranchTagModel shopBranchTagInfo = ShopBranchApplication.GetShopBranchTagInfo(result);
                        if (null != shopBranchTagInfo)
                        {
                            return shopBranchTagInfo.Title;
                        }
                    }
                }
            }
            return "";
        }

        public JsonResult GetNearShopBranchIcons()
        {
            SlideAdInfo[] infoArray = this._iSlideAdsService.GetSlidAds(0L, SlideAdInfo.SlideAdType.NearShopBranchIcon).ToArray<SlideAdInfo>();
            ISlideAdsService service = this._iSlideAdsService;
            var enumerable = from item in infoArray select new { id = item.Id, imgUrl = HimallIO.GetImagePath(item.ImageUrl, null), displaySequence = item.DisplaySequence, url = item.Url + "," + this.GetLinkName(item.Url), description = item.Description };
            return base.Json(new { rows = enumerable, total = 100 }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRecommendShopBranch()
        {
            ShopBranchQuery query = new ShopBranchQuery
            {
                IsRecommend = true
            };
            IOrderedEnumerable<ShopBranch> enumerable = from n in ShopBranchApplication.GetShopBranchsNoPager(query)
                                                        orderby n.RecommendSequence
                                                        select n;
            return base.Json(new { rows = enumerable }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSlideImages(int id)
        {
            SlideAdInfo[] infoArray = this._iSlideAdsService.GetSlidAds(0L, (SlideAdInfo.SlideAdType)id).ToArray<SlideAdInfo>();
            ISlideAdsService service = this._iSlideAdsService;
            var enumerable = from item in infoArray select new { id = item.Id, imgUrl = HimallIO.GetImagePath(item.ImageUrl, null), displaySequence = item.DisplaySequence, url = item.Url + "," + this.GetLinkName(item.Url), description = item.Description };
            return base.Json(new { rows = enumerable, total = 100 }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, UnAuthorize]
        public JsonResult NSBIconChangeSequence(int oriRowNumber, int newRowNumber)
        {
            this._iSlideAdsService.UpdateWeixinSlideSequence(0L, (long)oriRowNumber, (long)newRowNumber, SlideAdInfo.SlideAdType.NearShopBranchIcon);
            BaseController.Result data = new BaseController.Result
            {
                success = true
            };
            return base.Json(data);
        }

        public JsonResult RecommendChangeSequence(long oriShopBranchId, long newShopBranchId)
        {
            bool flag = ShopBranchApplication.RecommendChangeSequence(oriShopBranchId, newShopBranchId);
            BaseController.Result data = new BaseController.Result
            {
                success = flag,
                msg = flag ? "" : "未知错误,请重新排序"
            };
            return base.Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult RecommendShopBranch(long[] ids)
        {
            bool flag = ShopBranchApplication.RecommendShopBranch(ids);
            BaseController.Result data = new BaseController.Result
            {
                success = flag,
                msg = flag ? "" : "未知错误,请重试"
            };
            return base.Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ResetShopBranchRecommend(long id)
        {
            bool flag = ShopBranchApplication.ResetShopBranchRecommend(id);
            BaseController.Result data = new BaseController.Result
            {
                success = flag,
                msg = flag ? "" : "未知错误,请重试"
            };
            return base.Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ShopList(int page, int rows, string titleKeyword, string tagsId, string addressId, bool? isRecommend)
        {
            ShopBranchQuery query = new ShopBranchQuery
            {
                PageNo = page,
                PageSize = rows
            };
            if (!string.IsNullOrEmpty(titleKeyword))
            {
                query.ShopBranchName = titleKeyword;
            }
            if (!string.IsNullOrEmpty(addressId))
            {
                int num = Convert.ToInt32(addressId);
                switch (RegionApplication.GetRegion((long)num).Level)
                {
                    case Region.RegionLevel.Province:
                        query.ProvinceId = num;
                        break;

                    case Region.RegionLevel.City:
                        query.CityId = num;
                        break;
                }
            }
            if (!string.IsNullOrEmpty(tagsId))
            {
                query.ShopBranchTagId = new long?(Convert.ToInt64(tagsId));
            }
            if (isRecommend.HasValue)
            {
                query.IsRecommend = isRecommend;
            }
            QueryPageModel<ShopBranch> shopBranchs = ShopBranchApplication.GetShopBranchs(query);
            string str = CurrentUrlHelper.CurrentUrlNoPort();
            var data = new
            {
                rows = from item in shopBranchs.Models select new { id = item.Id, name = item.ShopBranchName, imgUrl = item.ShopImages, url = "/" + item.Id, tags = string.IsNullOrWhiteSpace(item.ShopBranchInTagNames) ? "" : item.ShopBranchInTagNames.Replace(",", " ") },
                total = shopBranchs.Total
            };
            return base.Json(data);
        }

        [HttpPost]
        public JsonResult ShopTagList(int page, int rows, string titleKeyword)
        {
            Func<ShopBranchTagModel, bool> predicate = null;
            List<ShopBranchTagModel> allShopBranchTagInfos = ShopBranchApplication.GetAllShopBranchTagInfos();
            if (!string.IsNullOrEmpty(titleKeyword))
            {
                if (predicate == null)
                {
                    predicate = n => n.Title.Contains(titleKeyword);
                }
                allShopBranchTagInfos = allShopBranchTagInfos.Where<ShopBranchTagModel>(predicate).ToList<ShopBranchTagModel>();
            }
            var data = new
            {
                rows = from item in allShopBranchTagInfos.Skip<ShopBranchTagModel>(((page - 1) * rows)).Take<ShopBranchTagModel>(rows) select new { id = item.Id, name = item.Title, imgUrl = "", url = "/" + item.Id, tags = "" },
                total = allShopBranchTagInfos.Count
            };
            return base.Json(data);
        }

        [HttpPost, UnAuthorize]
        public JsonResult SlideImageChangeSequence(int id, int oriRowNumber, int newRowNumber)
        {
            this._iSlideAdsService.UpdateWeixinSlideSequence(0L, (long)oriRowNumber, (long)newRowNumber, (SlideAdInfo.SlideAdType)id);
            return base.Json(new { success = true });
        }

        public JsonResult UpdateImageAd(long id, string pic, string url)
        {
            SlideAdInfo slidAd = this._iSlideAdsService.GetSlidAd(0L, id);
            if (!string.IsNullOrWhiteSpace(pic) && !slidAd.ImageUrl.Equals(pic))
            {
                if (pic.Contains("/temp/"))
                {
                    string path = pic.Substring(pic.LastIndexOf("/temp"));
                    string str2 = "/Storage/Plat/ImageAd/";
                    pic = Path.Combine(str2, Path.GetFileName(path));
                    HimallIO.CopyFile(path, pic, true);
                }
                else if (pic.Contains("/Storage/"))
                {
                    pic = pic.Substring(pic.LastIndexOf("/Storage"));
                }
            }
            SlideAdInfo models = new SlideAdInfo
            {
                ShopId = 0L,
                Url = url,
                ImageUrl = pic,
                Id = id
            };
            SlideAdInfo info3 = this._iSlideAdsService.UpdateSlidAd(models);
            if (null != info3)
            {
                info3.ImageUrl = HimallIO.GetImagePath(info3.ImageUrl, null);
                BaseController.Result result = new BaseController.Result
                {
                    success = true,
                    Data = info3
                };
                return base.Json(result, JsonRequestBehavior.AllowGet);
            }
            BaseController.Result data = new BaseController.Result
            {
                success = false,
                msg = "更新失败"
            };
            return base.Json(data, JsonRequestBehavior.AllowGet);
        }

    }
}