using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Framework;

using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using Himall.IServices.QueryModel;
using Himall.Core;
using System.IO;
using Himall.Application;
using System.Web;
using Himall.CommonModel;
using Newtonsoft.Json;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class WeiXinController : BaseAdminController
    {
        private ISlideAdsService _iSlideAdsService;
        private ISiteSettingService _iSiteSettingService;
        private IWeixinMenuService _iWeixinMenuService;
        private IMobileHomeTopicService _iMobileHomeTopicService;
        private ITopicService _iTopicService;
        private ITemplateSettingsService _iTemplateSettingsService;
        private IWXMsgTemplateService _iWXMsgTemplateService;
        public WeiXinController(ISlideAdsService iSlideAdsService,
            ISiteSettingService iSiteSettingService,
            IWeixinMenuService iWeixinMenuService,
            IMobileHomeTopicService iMobileHomeTopicService,
            ITopicService iTopicService,
            ITemplateSettingsService iTemplateSettingsService,
            IWXMsgTemplateService iWXMsgTemplateService
            )
        {
            _iSiteSettingService = iSiteSettingService;
            _iWeixinMenuService = iWeixinMenuService;
            _iMobileHomeTopicService = iMobileHomeTopicService;
            _iTopicService = iTopicService;
            _iSlideAdsService = iSlideAdsService;
            _iTemplateSettingsService = iTemplateSettingsService;
            _iWXMsgTemplateService = iWXMsgTemplateService;
        }
        /// <summary>
        /// 首页跳转
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return (ActionResult)this.View();
        }
        /// <summary>
        /// 自动回复
        /// </summary>
        /// <returns></returns>
        public ActionResult AutoReplay()
        {
            return (ActionResult)this.View();
        }
        /// <summary>
        /// 回复列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult PostAutoReplyList(int page, int rows)
        {
            ObsoletePageModel<AutoReplyInfo> model = WeixinAutoReplyApplication.GetPage(page, rows, ReplyType.Keyword);
            return base.Json(new { rows = model.Models.ToList<AutoReplyInfo>(), total = model.Total });


        }
        /// <summary>
        /// 根据ID查询自动回复
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetAutoReplayById(int Id)
        {
            BaseController.Result result = new BaseController.Result();
            AutoReplyInfo autoReplyById = WeixinAutoReplyApplication.GetAutoReplyById(Id);
            result.success = true;
            result.Data = (object)autoReplyById;
            return this.Json((object)result);
        }
        /// <summary>
        /// 修改自动回复
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public JsonResult ModAutoReplay(AutoReplyInfo item)
        {
            BaseController.Result result = new BaseController.Result();
            WeixinAutoReplyApplication.ModAutoReply(item);
            result.success = true;
            result.msg = "规则保存成功！";
            return this.Json((object)result);
        }
        /// <summary>
        /// 删除自动回复
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public JsonResult DelAutoReplay(AutoReplyInfo item)
        {
            BaseController.Result result = new BaseController.Result();
            WeixinAutoReplyApplication.DeleteAutoReply(item);
            result.success = true;
            result.msg = "规则删除成功！";
            return this.Json((object)result);
        }
        /// <summary>
        /// 关注回复
        /// </summary>
        /// <returns></returns>
        public ActionResult FocusReplay()
        {
            return (ActionResult)this.View((object)(WeixinAutoReplyApplication.GetAutoReplyByKey(ReplyType.Follow, "", true, false, false) ?? new AutoReplyInfo()));
        }
        /// <summary>
        /// 新回复
        /// </summary>
        /// <returns></returns>
        public ActionResult NewsReplay()
        {
            return (ActionResult)this.View((object)(WeixinAutoReplyApplication.GetAutoReplyByKey(ReplyType.Msg, "", true, false, false) ?? new AutoReplyInfo()));
        }
        /// <summary>
        /// 基本设置
        /// </summary>
        /// <returns></returns>
        public ActionResult BasicSettings()
        {
            SiteSettingsInfo siteSettings = this._iSiteSettingService.GetSiteSettings();
            if (string.IsNullOrEmpty(siteSettings.WeixinToken))
            {
                siteSettings.WeixinToken = this.CreateKey(8);
                this._iSiteSettingService.SetSiteSettings(siteSettings);
            }
            SiteSettingModel model = new SiteSettingModel
            {
                WeixinAppId = string.IsNullOrEmpty(siteSettings.WeixinAppId) ? string.Empty : siteSettings.WeixinAppId.Trim(),
                WeixinAppSecret = string.IsNullOrEmpty(siteSettings.WeixinAppSecret) ? string.Empty : siteSettings.WeixinAppSecret.Trim(),
                WeixinToKen = siteSettings.WeixinToken.Trim()
            };
            ((dynamic)base.ViewBag).Url = string.Format("{0}/m-Weixin/WXApi", CurrentUrlHelper.CurrentUrlNoPort());
            if (DemoAuthorityHelper.IsDemo())
            {
                model.WeixinAppId = "*".PadRight(model.WeixinAppId.Length, '*');
                model.WeixinAppSecret = "*".PadRight(model.WeixinAppSecret.Length, '*');
                ((dynamic)base.ViewBag).isDemo = true;
            }
            return base.View(model);

        }
        /// <summary>
        /// 创建Key
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        private string CreateKey(int len)
        {
            byte[] data = new byte[len];
            new RNGCryptoServiceProvider().GetBytes(data);
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < data.Length; ++index)
                stringBuilder.Append(string.Format("{0:X2}", (object)data[index]));
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 保存微信设置
        /// </summary>
        /// <param name="weixinAppId"></param>
        /// <param name="WeixinAppSecret"></param>
        /// <returns></returns>
        [UnAuthorize]
        [HttpPost]
        public JsonResult SaveWeiXinSettings(string weixinAppId, string WeixinAppSecret)
        {
            if (DemoAuthorityHelper.IsDemo())
                return this.Json((object)new
                {
                    success = false,
                    msg = "演示站点自动隐藏此参数，且不能保存！"
                });
            BaseController.Result result = new BaseController.Result();
            SiteSettingsInfo siteSettings = this._iSiteSettingService.GetSiteSettings();
            siteSettings.WeixinAppId = weixinAppId.Trim();
            siteSettings.WeixinAppSecret = WeixinAppSecret.Trim();
            this._iSiteSettingService.SetSiteSettings(siteSettings);
            result.success = true;
            return this.Json((object)result);
        }
        /// <summary>
        /// 菜单管理
        /// </summary>
        /// <returns></returns>
        public ActionResult MenuManage()
        {
            List<MenuManageModel> list = new List<MenuManageModel>();
            foreach (MenuInfo menuInfo in this._iWeixinMenuService.GetMainMenu(this.CurrentManager.ShopId))
            {
                MenuManageModel menuManageModel = new MenuManageModel()
                {
                    ID = menuInfo.Id,
                    TopMenuName = menuInfo.Title,
                    SubMenu = this._iWeixinMenuService.GetMenuByParentId(menuInfo.Id),
                    URL = menuInfo.Url,
                    LinkType = menuInfo.UrlType
                };
                list.Add(menuManageModel);
            }
            return (ActionResult)this.View((object)list);
        }
        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteMenu(int menuId)
        {
            BaseController.Result result = new BaseController.Result();
            this._iWeixinMenuService.DeleteMenu((long)menuId);
            result.success = true;
            return this.Json((object)result);
        }
        /// <summary>
        /// 提交到微信
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RequestToWeixin()
        {
            BaseController.Result result = new BaseController.Result();
            this._iWeixinMenuService.ConsistentToWeixin(this.CurrentManager.ShopId);
            result.success = true;
            return this.Json((object)result);
        }
        /// <summary>
        /// 添加菜单
        /// </summary>
        /// <param name="title"></param>
        /// <param name="url"></param>
        /// <param name="parentId"></param>
        /// <param name="urlType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddMenu(string title, string url, string parentId, int urlType)
        {
            short num = !(parentId == "0") ? (short)2 : (short)1;
            string str = CurrentUrlHelper.CurrentUrlNoPort();
            switch (urlType)
            {
                case 1:
                    url = str + "/m-" + PlatformType.WeiXin.ToString() + "/";
                    break;
                case 2:
                    url = str + "/m-" + PlatformType.WeiXin.ToString() + "/vshop";
                    break;
                case 3:
                    url = str + "/m-" + PlatformType.WeiXin.ToString() + "/category/Index";
                    break;
                case 4:
                    url = str + "/m-" + PlatformType.WeiXin.ToString() + "/member/center";
                    break;
                case 5:
                    url = str + "/m-" + PlatformType.WeiXin.ToString() + "/cart/cart";
                    break;
            }
            if (!string.IsNullOrEmpty(url) && (!url.ToLower().Contains("http://") && !url.ToLower().Contains("https://")))
                throw new HimallException("链接必须以http://开头");
            BaseController.Result result = new BaseController.Result();
            this._iWeixinMenuService.AddMenu(new MenuInfo()
            {
                Title = title,
                Url = url,
                ParentId = Convert.ToInt64(parentId),
                Platform = PlatformType.WeiXin,
                Depth = num,
                ShopId = this.CurrentManager.ShopId,
                FullIdPath = "1",
                Sequence = (short)1,
                UrlType = new MenuInfo.UrlTypes?((MenuInfo.UrlTypes)urlType)
            });
            result.success = true;
            return this.Json((object)result);
        }
        /// <summary>
        /// 编辑菜单
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        public ActionResult EditMenu(long menuId)
        {
            MenuInfo menu = this._iWeixinMenuService.GetMenu(menuId);
            MenuManageModel model = new MenuManageModel
            {
                ID = menu.Id,
                TopMenuName = menu.Title,
                URL = menu.Url,
                LinkType = menu.UrlType
            };
            if (menu.ParentId != 0L)
            {
                ((dynamic)base.ViewBag).parentName = this._iWeixinMenuService.GetMenu(menu.ParentId).Title;
                ((dynamic)base.ViewBag).parentId = menu.ParentId;
            }
            else
            {
                ((dynamic)base.ViewBag).parentId = 0;
            }
            return base.View(model);
        }
        /// <summary>
        /// 微信回复
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult WeiXinReplay1()
        {
            return (ActionResult)this.View();
        }
        /// <summary>
        /// 更新菜单
        /// </summary>
        /// <param name="menuId"></param>
        /// <param name="menuName"></param>
        /// <param name="urlType"></param>
        /// <param name="url"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public JsonResult UpdateMenu(string menuId, string menuName, int urlType, string url, string parentId)
        {
            string str = CurrentUrlHelper.CurrentUrlNoPort();
            switch (urlType)
            {
                case 1:
                    url = str + "/m-" + PlatformType.WeiXin.ToString() + "/";
                    break;
                case 2:
                    url = str + "/m-" + PlatformType.WeiXin.ToString() + "/vshop";
                    break;
                case 3:
                    url = str + "/m-" + PlatformType.WeiXin.ToString() + "/category/Index";
                    break;
                case 4:
                    url = str + "/m-" + PlatformType.WeiXin.ToString() + "/member/center";
                    break;
                case 5:
                    url = str + "/m-" + PlatformType.WeiXin.ToString() + "/cart/cart";
                    break;
            }
            if (!string.IsNullOrEmpty(url) && (!url.ToLower().Contains("http://") && !url.ToLower().Contains("https://")))
                throw new HimallException("链接必须以http://开头");
            BaseController.Result result = new BaseController.Result();
            this._iWeixinMenuService.UpdateMenu(new MenuInfo()
            {
                Id = Convert.ToInt64(menuId),
                Title = menuName,
                UrlType = new MenuInfo.UrlTypes?((MenuInfo.UrlTypes)urlType),
                Url = url,
                ParentId = Convert.ToInt64(parentId)
            });
            result.success = true;
            return this.Json((object)result);
        }
        /// <summary>
        /// 专题设置
        /// </summary>
        /// <returns></returns>
        public ActionResult TopicSettings()
        {
            MobileHomeTopicsInfo[] mobileHomeTopicsInfoArray = Enumerable.ToArray<MobileHomeTopicsInfo>((IEnumerable<MobileHomeTopicsInfo>)this._iMobileHomeTopicService.GetMobileHomeTopicInfos(PlatformType.WeiXin, 0L));
            ITopicService topicService = this._iTopicService;
            return (ActionResult)this.View((object)Enumerable.Select<MobileHomeTopicsInfo, TopicModel>((IEnumerable<MobileHomeTopicsInfo>)mobileHomeTopicsInfoArray, (Func<MobileHomeTopicsInfo, TopicModel>)(item =>
            {
                TopicInfo topicInfo = topicService.GetTopicInfo(item.TopicId);
                return new TopicModel()
                {
                    FrontCoverImage = topicInfo.FrontCoverImage,
                    Id = item.Id,
                    Name = topicInfo.Name,
                    Tags = topicInfo.Tags,
                    Sequence = item.Sequence
                };
            })));
        }
        /// <summary>
        /// 选择专题
        /// </summary>
        /// <param name="frontCoverImage"></param>
        /// <param name="topicId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ChooseTopic(string frontCoverImage, long topicId)
        {
            ITopicService topicService = this._iTopicService;
            TopicInfo topicInfo = topicService.GetTopicInfo(topicId);
            topicInfo.FrontCoverImage = frontCoverImage;
            topicService.UpdateTopic(topicInfo);
            this._iMobileHomeTopicService.AddMobileHomeTopic(topicId, 0L, PlatformType.WeiXin, frontCoverImage);
            return this.Json((object)new
            {
                success = true
            });
        }
        /// <summary>
        /// 删除选择专题
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RemoveChoseTopic(long id)
        {
            this._iMobileHomeTopicService.Delete(id);
            return this.Json((object)new
            {
                success = true
            });
        }

        [HttpPost]
        public JsonResult UpdateSequence(long id, int sequence)
        {
            this._iMobileHomeTopicService.SetSequence(id, sequence, 0L);
            return this.Json((object)new
            {
                success = true
            });
        }
        /// <summary>
        /// 更专图片设置
        /// </summary>
        /// <returns></returns>
        public ActionResult SlideImageSettings()
        {
            SlideAdInfo[] slideAdInfoArray = Enumerable.ToArray<SlideAdInfo>((IEnumerable<SlideAdInfo>)this._iSlideAdsService.GetSlidAds(0L, SlideAdInfo.SlideAdType.WeixinHome));
            ISlideAdsService slideImageService = this._iSlideAdsService;
            return (ActionResult)this.View((object)Enumerable.Select<SlideAdInfo, SlideAdModel>((IEnumerable<SlideAdInfo>)slideAdInfoArray, (Func<SlideAdInfo, SlideAdModel>)(item =>
            {
                slideImageService.GetSlidAd(0L, item.Id);
                return new SlideAdModel()
                {
                    ID = item.Id,
                    imgUrl = item.ImageUrl,
                    DisplaySequence = item.DisplaySequence,
                    Url = item.Url,
                    Description = item.Description
                };
            })));
        }
        /// <summary>
        /// 添加滚动图片
        /// </summary>
        /// <param name="id"></param>
        /// <param name="description"></param>
        /// <param name="imageUrl"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public JsonResult AddSlideImage(string id, string description, string imageUrl, string url)
        {
            BaseController.Result result = new BaseController.Result();
            SlideAdInfo slideAdInfo = new SlideAdInfo();
            slideAdInfo.Id = Convert.ToInt64(id);
            slideAdInfo.ImageUrl = imageUrl;
            slideAdInfo.TypeId = SlideAdInfo.SlideAdType.WeixinHome;
            slideAdInfo.Url = url;
            slideAdInfo.Description = description;
            slideAdInfo.ShopId = 0L;
            if (slideAdInfo.Id > 0L)
                this._iSlideAdsService.UpdateSlidAd(slideAdInfo);
            else
                this._iSlideAdsService.AddSlidAd(slideAdInfo);
            result.success = true;
            return this.Json((object)result);
        }
        /// <summary>
        /// 删除滚动图片
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteSlideImage(string id)
        {
            BaseController.Result result = new BaseController.Result();
            this._iSlideAdsService.DeleteSlidAd(0L, Convert.ToInt64(id));
            result.success = true;
            return this.Json((object)result);
        }

        public ActionResult SaveSlideImage(long id = 0L)
        {
            SlideAdInfo slideAdInfo = id <= 0L ? new SlideAdInfo() : this._iSlideAdsService.GetSlidAd(0L, id);
            return (ActionResult)this.View((object)new SlideAdModel()
            {
                Description = slideAdInfo.Description,
                imgUrl = HimallIO.GetImagePath(slideAdInfo.ImageUrl, (string)null),
                Url = slideAdInfo.Url,
                ID = id
            });
        }
        /// <summary>
        /// 滚动图片修改
        /// </summary>
        /// <param name="oriRowNumber"></param>
        /// <param name="newRowNumber"></param>
        /// <returns></returns>
        [UnAuthorize]
        [HttpPost]
        public JsonResult SlideImageChangeSequence(int oriRowNumber, int newRowNumber)
        {
            this._iSlideAdsService.UpdateWeixinSlideSequence(0L, (long)oriRowNumber, (long)newRowNumber, SlideAdInfo.SlideAdType.WeixinHome);
            return this.Json((object)new
            {
                success = true
            });
        }
        /// <summary>
        /// 产品设置
        /// </summary>
        /// <returns></returns>
        public ActionResult ProductSettings()
        {
            return (ActionResult)this.View();
        }
        /// <summary>
        /// 滚动图片
        /// </summary>
        /// <returns></returns>
        public JsonResult GetSlideImages()
        {
            SlideAdInfo[] slideAdInfoArray = Enumerable.ToArray<SlideAdInfo>((IEnumerable<SlideAdInfo>)this._iSlideAdsService.GetSlidAds(0L, SlideAdInfo.SlideAdType.WeixinHome));
            ISlideAdsService slideImageService = this._iSlideAdsService;
            return this.Json((object)new
            {
                rows = Enumerable.Select((IEnumerable<SlideAdInfo>)slideAdInfoArray, item =>
                {
                    slideImageService.GetSlidAd(0L, item.Id);
                    var fAnonymousTypeb = new
                    {
                        id = item.Id,
                        imgUrl = HimallIO.GetImagePath(item.ImageUrl, (string)null),
                        displaySequence = item.DisplaySequence,
                        url = item.Url,
                        description = item.Description
                    };
                    return fAnonymousTypeb;
                }),
                total = 100
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult WXMsgTemplateManage(string mediaid)
        {
            IEnumerable<WXMaterialInfo> enumerable = (IEnumerable<WXMaterialInfo>)new List<WXMaterialInfo>()
            {
                 new WXMaterialInfo()
            };
            if (!string.IsNullOrWhiteSpace(mediaid))
                enumerable = this._iWXMsgTemplateService.GetMedia(mediaid, this.CurrentSiteSetting.WeixinAppId, this.CurrentSiteSetting.WeixinAppSecret);
            return (ActionResult)this.View((object)enumerable);
        }

        [ValidateInput(false)]
        public JsonResult AddWXMsgTemplate(string mediaid, string data)
        {
            IEnumerable<WXMaterialInfo> enumerable = JsonConvert.DeserializeObject<IEnumerable<WXMaterialInfo>>(data);
            if (string.IsNullOrWhiteSpace(mediaid))
            {
                WXUploadNewsResult uploadNewsResult = this._iWXMsgTemplateService.Add(enumerable, this.CurrentSiteSetting.WeixinAppId, this.CurrentSiteSetting.WeixinAppSecret);
                if (string.IsNullOrEmpty(uploadNewsResult.media_id))
                    return this.Json((object)new
                    {
                        success = false,
                        msg = uploadNewsResult.errmsg
                    });
            }
            else
            {
                WxJsonResult wxJsonResult = Enumerable.FirstOrDefault<WxJsonResult>(Enumerable.Where<WxJsonResult>(this._iWXMsgTemplateService.UpdateMedia(mediaid, enumerable, this.CurrentSiteSetting.WeixinAppId, this.CurrentSiteSetting.WeixinAppSecret), (Func<WxJsonResult, bool>)(e => !string.IsNullOrWhiteSpace(e.errmsg))));
                if (wxJsonResult != null)
                {
                    if (wxJsonResult.errmsg == "ok")
                        return this.Json((object)new
                        {
                            success = true,
                            msg = wxJsonResult.errmsg
                        });
                    return this.Json((object)new
                    {
                        success = false,
                        msg = wxJsonResult.errmsg
                    });
                }
            }
            return this.Json((object)new
            {
                success = true
            });
        }
        /// <summary>
        /// 添加图片素材
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public JsonResult AddWXImageMsg(string name)
        {
            string str = this._iWXMsgTemplateService.AddImage(this.Server.MapPath(name), this.CurrentSiteSetting.WeixinAppId, this.CurrentSiteSetting.WeixinAppSecret);
            if (string.IsNullOrWhiteSpace(str))
                return this.Json((object)new
                {
                    success = false,
                    msg = "上传图片失败！"
                });
            return this.Json((object)new
            {
                success = true,
                media = str
            });
        }
        /// <summary>
        /// 文件列表
        /// </summary>
        /// <param name="pageIdx"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public JsonResult GetWXMaterialList(int pageIdx, int pageSize)
        {
            SiteSettingsInfo siteSettings = this._iSiteSettingService.GetSiteSettings();
            if (string.IsNullOrWhiteSpace(siteSettings.WeixinAppId))
                throw new HimallException("未配置微信公众号");
            int offset = (pageIdx - 1) * pageSize;
            return this.Json((object)this._iWXMsgTemplateService.GetMediaMsgTemplateList(siteSettings.WeixinAppId, siteSettings.WeixinAppSecret, offset, pageSize));
        }
        /// <summary>
        /// 文件
        /// </summary>
        /// <param name="mediaid"></param>
        /// <returns></returns>
        public ActionResult GetMedia(string mediaid)
        {
            SiteSettingsInfo siteSettings = this._iSiteSettingService.GetSiteSettings();
            if (string.IsNullOrWhiteSpace(siteSettings.WeixinAppId))
                throw new HimallException("未配置微信公众号");
            MemoryStream memoryStream = new MemoryStream();
            this._iWXMsgTemplateService.GetMedia(mediaid, siteSettings.WeixinAppId, siteSettings.WeixinAppSecret, (Stream)memoryStream);
            return (ActionResult)this.File(memoryStream.ToArray(), "Image/png");
        }
        /// <summary>
        /// 消息模版
        /// </summary>
        /// <returns></returns>
        public ActionResult WXMsgTemplate()
        {
            return (ActionResult)this.View();
        }
        /// <summary>
        /// 读取素材
        /// </summary>
        /// <param name="mediaid"></param>
        /// <returns></returns>
        public JsonResult GetMediaInfo(string mediaid)
        {
            if (string.IsNullOrWhiteSpace(this._iSiteSettingService.GetSiteSettings().WeixinAppId))
                throw new HimallException("未配置微信公众号");
            if (string.IsNullOrEmpty(mediaid))
                throw new HimallException("素材ID不能为空");
            return this.Json((object)new
            {
                success = true,
                data = this._iWXMsgTemplateService.GetMedia(mediaid, this.CurrentSiteSetting.WeixinAppId, this.CurrentSiteSetting.WeixinAppSecret)
            });
        }
        /// <summary>
        /// 删除素材
        /// </summary>
        /// <param name="mediaid"></param>
        /// <returns></returns>
        public JsonResult DeleteMedia(string mediaid)
        {
            if (string.IsNullOrWhiteSpace(this._iSiteSettingService.GetSiteSettings().WeixinAppId))
                throw new HimallException("未配置微信公众号");
            if (string.IsNullOrEmpty(mediaid))
                throw new HimallException("素材ID不能为空");
            WxJsonResult wxJsonResult = this._iWXMsgTemplateService.DeleteMedia(mediaid, this.CurrentSiteSetting.WeixinAppId, this.CurrentSiteSetting.WeixinAppSecret);
            if (string.IsNullOrEmpty(wxJsonResult.errmsg))
                return this.Json((object)new
                {
                    success = false,
                    msg = wxJsonResult.errmsg
                });
            return this.Json((object)new
            {
                success = true
            });
        }
        /// <summary>
        /// 附近门店设置
        /// </summary>
        /// <returns></returns>
        public ActionResult NearShopBranchSetting()
        {
            MobileHomeTopicsInfo[] infoArray = this._iMobileHomeTopicService.GetMobileHomeTopicInfos(PlatformType.Mobile, 0L).ToArray<MobileHomeTopicsInfo>();
            List<ShopBranchTagModel> allShopBranchTagInfos = ShopBranchApplication.GetAllShopBranchTagInfos();
            List<SelectListItem> list3 = new List<SelectListItem>();
            SelectListItem item2 = new SelectListItem
            {
                Selected = true
            };
            item2.Value = 0.ToString();
            item2.Text = "请选择...";
            list3.Add(item2);
            List<SelectListItem> list2 = list3;
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
            ViewBag.ShopBranchTags = list2;
            ViewBag.imageAds = this._iSlideAdsService.GetSlidAdsOrInit(0, SlideAdInfo.SlideAdType.NearShopBranchSpecial).ToList<SlideAdInfo>();
            ViewBag.IsOpenStore = (SiteSettingApplication.GetSiteSettings() != null) && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            IEnumerable<TopicModel> enumerable = from item in infoArray select new TopicModel { FrontCoverImage = item.Himall_Topics.frontCoverImage, Id = item.Id, Name = item.Himall_Topics.Name, Tags = item.Himall_Topics.Tags, Sequence = item.Sequence };
            return View(enumerable);
        }
        /// <summary>
        /// 底部菜单设置
        /// </summary>
        /// <returns></returns>
        public ActionResult WXMobileFootMenu()
        {
            return (ActionResult)this.View();
        }
        /// <summary>
        /// 读取所有菜单
        /// </summary>
        /// <returns></returns>
        public JsonResult GetFootMenus()
        {
            MobileFootMenuInfo[] mobileFootMenuInfoArray = Enumerable.ToArray<MobileFootMenuInfo>((IEnumerable<MobileFootMenuInfo>)this._iWeixinMenuService.GetFootMenus());
            IWeixinMenuService weixinMenuService = this._iWeixinMenuService;
            if (mobileFootMenuInfoArray.Length > 0)
                return this.Json((object)new
                {
                    status = "0",
                    shopmenustyle = "",
                    enableshopmenu = "True",
                    data = Enumerable.Select((IEnumerable<MobileFootMenuInfo>)mobileFootMenuInfoArray, item =>
                    {
                        var fAnonymousType8d = new
                        {
                            menuid = item.Id,
                            childdata = new
                            {
                            },
                            type = "click",
                            name = item.Name,
                            shopmenupic = item.MenuIcon,
                            content = item.Url
                        };
                        return fAnonymousType8d;
                    })
                });
            return this.Json((object)new
            {
                status = " -1 "
            });
        }
        /// <summary>
        /// 读取底部菜单根据ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetFootMenuInfoById(string id)
        {
            if (!(id != "undefined"))
                return this.Json((object)new
                {
                    status = "1"
                });
            MobileFootMenuInfo footMenusById = this._iWeixinMenuService.GetFootMenusById(Convert.ToInt64(id));
            if (footMenusById == null)
                return this.Json((object)new
                {
                    status = "1"
                });
            var fAnonymousType8f = new
            {
                menuid = footMenusById.Id,
                type = "",
                name = footMenusById.Name,
                shopmenupic = footMenusById.MenuIcon,
                content = footMenusById.Url
            };
            return this.Json((object)new
            {
                status = "0",
                data = fAnonymousType8f
            });
        }
        /// <summary>
        /// 添加底部菜单
        /// </summary>
        /// <param name="id"></param>
        /// <param name="description"></param>
        /// <param name="imageUrl"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public JsonResult AddFootMenu(string id, string description, string imageUrl, string url)
        {
            BaseController.Result result = new BaseController.Result();
            MobileFootMenuInfo footmenu = new MobileFootMenuInfo();
            long num = 0L;
            if (!string.IsNullOrEmpty(id))
                num = Convert.ToInt64(id);
            footmenu.Id = Convert.ToInt64(num);
            footmenu.MenuIcon = imageUrl;
            footmenu.Url = url.ToLower();
            footmenu.Name = HttpUtility.UrlDecode(description);
            if (footmenu.Id > 0L)
                this._iWeixinMenuService.UpdateMobileFootMenu(footmenu);
            else
                this._iWeixinMenuService.AddMobileFootMenu(footmenu);
            result.success = true;
            if (result.success)
                return this.Json((object)new
                {
                    status = "0"
                });
            return this.Json((object)new
            {
                status = "1"
            });
        }
        /// <summary>
        /// 删除底部菜单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult DelFootMenu(string id)
        {
            BaseController.Result result = new BaseController.Result();
            if (string.IsNullOrEmpty(id))
                return this.Json((object)new
                {
                    status = "1"
                });
            this._iWeixinMenuService.DeleteFootMenu(Convert.ToInt64(id));
            result.success = true;
            if (result.success)
                return this.Json((object)new
                {
                    status = "0"
                });
            return this.Json((object)new
            {
                status = "1"
            });
        }

    }
}