using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class APPShopController : BaseAdminController
    {
        // Fields
        private IMobileHomeTopicService _iMobileHomeTopicService;
        private ISlideAdsService _iSlideAdsService;

        // Methods
        public APPShopController(IMobileHomeTopicService iMobileHomeTopicService, ISlideAdsService iSlideAdsService)
        {
            this._iMobileHomeTopicService = iMobileHomeTopicService;
            this._iSlideAdsService = iSlideAdsService;
        }

        public JsonResult AddAppIcon(string id, string description, string imageUrl, string url)
        {
            BaseController.Result data = new BaseController.Result();
            SlideAdInfo models = new SlideAdInfo
            {
                Id = Convert.ToInt64(id),
                ImageUrl = imageUrl,
                TypeId = SlideAdInfo.SlideAdType.APPIcon,
                Url = url.ToLower().Replace("/m-wap", "/m-ios").Replace("/m-weixin", "/m-ios"),
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

        public JsonResult AddSlideImage(string id, string description, string imageUrl, string url)
        {
            BaseController.Result data = new BaseController.Result();
            SlideAdInfo models = new SlideAdInfo
            {
                Id = Convert.ToInt64(id),
                ImageUrl = imageUrl,
                TypeId = SlideAdInfo.SlideAdType.IOSShopHome,
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

        public ActionResult APPGuidePages()
        {
            List<Himall.DTO.SlideAdModel> model = (from a in
                                            (from a in this._iSlideAdsService.GetSlidAds(0L, SlideAdInfo.SlideAdType.AppGuide)
                                             orderby a.DisplaySequence
                                             select a).ToList<SlideAdInfo>()
                                                   select new Himall.DTO.SlideAdModel { Id = a.Id, DisplaySequence = a.DisplaySequence, ImageUrl = a.ImageUrl }).ToList<Himall.DTO.SlideAdModel>();
            return base.View(model);
        }

        [HttpPost]
        public JsonResult APPGuidePages(List<string> pics)
        {
            List<Himall.DTO.SlideAdModel> list = new List<Himall.DTO.SlideAdModel>();
            foreach (string str in pics)
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    Himall.DTO.SlideAdModel item = new Himall.DTO.SlideAdModel
                    {
                        ImageUrl = str
                    };
                    list.Add(item);
                }
            }
            if (list.Count == 0)
            {
                throw new HimallException("至少上传一张引导页图");
            }
            SlideApplication.AddGuidePages(list);
            BaseController.Result data = new BaseController.Result
            {
                success = true
            };
            return base.Json(data);
        }

        public ActionResult AppHomePageSetting()
        {
            VTemplateEditModel model = new VTemplateEditModel
            {
                ClientType = VTemplateClientTypes.AppIndex,
                Name = "APP"
            };
            ((dynamic)base.ViewBag).IsOpenStore = (SiteSettingApplication.GetSiteSettings() != null) && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            return base.View(model);
        }

        [HttpPost, UnAuthorize]
        public JsonResult AppIconChangeSequence(int oriRowNumber, int newRowNumber)
        {
            this._iSlideAdsService.UpdateWeixinSlideSequence(0L, (long)oriRowNumber, (long)newRowNumber, SlideAdInfo.SlideAdType.APPIcon);
            BaseController.Result data = new BaseController.Result
            {
                success = true
            };
            return base.Json(data);
        }

        [HttpPost]
        public JsonResult DeleteAppIcon(string id)
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

        public JsonResult GetAPPIcons()
        {
            SlideAdInfo[] source = this._iSlideAdsService.GetSlidAds(0L, SlideAdInfo.SlideAdType.APPIcon).ToArray<SlideAdInfo>();
            ISlideAdsService slideImageService = this._iSlideAdsService;
            var enumerable = source.Select(delegate(SlideAdInfo item)
            {
                SlideAdInfo slidAd = slideImageService.GetSlidAd(0L, item.Id);
                return new { id = item.Id, imgUrl = HimallIO.GetImagePath(item.ImageUrl, null), displaySequence = item.DisplaySequence, url = item.Url, description = item.Description };
            });
            return base.Json(new { rows = enumerable, total = 100 }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetImageAd(long id)
        {
            ImageAdInfo imageAd = this._iSlideAdsService.GetImageAd(0L, id);
            return base.Json(new { success = true, imageUrl = HimallIO.GetImagePath(imageAd.ImageUrl, null), url = imageAd.Url }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetManagement(int agreementType)
        {
            return base.Json(this.GetManagementModel((AgreementInfo.AgreementTypes)agreementType));
        }

        public AgreementModel GetManagementModel(Himall.Model.AgreementInfo.AgreementTypes type)
        {
            AgreementModel model = new AgreementModel
            {
                AgreementType = (int)type
            };
            AgreementInfo agreement = SystemAgreementApplication.GetAgreement(type);
            if (agreement != null)
            {
                model.AgreementContent = agreement.AgreementContent;
            }
            return model;
        }

        public JsonResult GetSlideImages()
        {
            SlideAdInfo[] source = this._iSlideAdsService.GetSlidAds(0L, SlideAdInfo.SlideAdType.IOSShopHome).ToArray<SlideAdInfo>();
            ISlideAdsService slideImageService = this._iSlideAdsService;
            var enumerable = source.Select(delegate(SlideAdInfo item)
            {
                SlideAdInfo slidAd = slideImageService.GetSlidAd(0L, item.Id);
                return new { id = item.Id, imgUrl = HimallIO.GetImagePath(item.ImageUrl, null), displaySequence = item.DisplaySequence, url = item.Url, description = item.Description };
            });
            return base.Json(new { rows = enumerable, total = 100 }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult HomePageSetting()
        {
            MobileHomeTopicsInfo[] infoArray = this._iMobileHomeTopicService.GetMobileHomeTopicInfos(PlatformType.IOS, 0L).ToArray<MobileHomeTopicsInfo>();
            ((dynamic)base.ViewBag).imageAds = (from p in this._iSlideAdsService.GetImageAds(0L, 0)
                                                where p.TypeId == ImageAdsType.APPSpecial
                                                select p).ToList<ImageAdInfo>();
            ((dynamic)base.ViewBag).IsOpenStore = (SiteSettingApplication.GetSiteSettings() != null) && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            IEnumerable<TopicModel> model = from item in infoArray select new TopicModel { FrontCoverImage = item.Himall_Topics.frontCoverImage, Id = item.Id, Name = item.Himall_Topics.Name, Tags = item.Himall_Topics.Tags, Sequence = item.Sequence };
            return base.View(model);
        }

        public ActionResult Management()
        {
            AgreementInfo.AgreementTypes aPP = AgreementInfo.AgreementTypes.APP;
            return base.View(this.GetManagementModel(aPP));
        }

        private string ProcessHtml(string html)
        {
            if (!string.IsNullOrEmpty(html))
            {
                string desDir = "/Storage/Plat/APP/About";
                html = HtmlContentHelper.TransferToLocalImage(html, "/", desDir, HimallIO.GetImagePath(desDir, null) + "/");
                html = HtmlContentHelper.RemoveScriptsAndStyles(html);
            }
            return html;
        }

        public ActionResult ProductSetting()
        {
            return base.View();
        }

        [UnAuthorize, HttpPost]
        public JsonResult SlideImageChangeSequence(int oriRowNumber, int newRowNumber)
        {
            this._iSlideAdsService.UpdateWeixinSlideSequence(0L, (long)oriRowNumber, (long)newRowNumber, SlideAdInfo.SlideAdType.IOSShopHome);
            BaseController.Result data = new BaseController.Result
            {
                success = true
            };
            return base.Json(data);
        }

        [HttpPost, ValidateInput(false)]
        public JsonResult UpdateAgreement(int agreementType, string agreementContent)
        {
            AgreementInfo agreement = SystemAgreementApplication.GetAgreement((AgreementInfo.AgreementTypes)agreementType);
            bool flag = false;
            if (agreement == null)
            {
                agreement = new AgreementInfo
                {
                    AgreementType = agreementType,
                    AgreementContent = this.ProcessHtml(agreementContent)
                };
                SystemAgreementApplication.AddAgreement(agreement);
                flag = true;
            }
            else
            {
                agreement.AgreementType = agreementType;
                agreement.AgreementContent = this.ProcessHtml(agreementContent);
                flag = SystemAgreementApplication.UpdateAgreement(agreement);
            }
            if (flag)
            {
                BaseController.Result result = new BaseController.Result
                {
                    success = true,
                    msg = "更新协议成功！"
                };
                return base.Json(result);
            }
            BaseController.Result data = new BaseController.Result
            {
                success = false,
                msg = "更新协议失败！"
            };
            return base.Json(data);
        }

        public JsonResult UpdateImageAd(long id, string pic, string url)
        {
            ImageAdInfo imageAd = this._iSlideAdsService.GetImageAd(0L, id);
            if (!string.IsNullOrWhiteSpace(pic) && !imageAd.ImageUrl.Equals(pic))
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
            ImageAdInfo model = new ImageAdInfo
            {
                ShopId = 0L,
                Url = url,
                ImageUrl = pic,
                Id = id
            };
            this._iSlideAdsService.UpdateImageAd(model);
            BaseController.Result data = new BaseController.Result
            {
                success = true
            };
            return base.Json(data);
        }

    }
}