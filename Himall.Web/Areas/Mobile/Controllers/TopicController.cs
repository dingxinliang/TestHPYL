using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System.Linq;
using System.Web.Mvc;
using Himall.Core;
using Himall.CommonModel;
using System.Collections.Generic;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class TopicController : BaseMobileTemplatesController
    {
        // Fields
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private IProductService _iProductService;
        private ITopicService _iTopicService;

        // Methods
        public TopicController(ITopicService iTopicService, IProductService iProductService, ILimitTimeBuyService iLimitTimeBuyService)
        {
            this._iProductService = iProductService;
            this._iTopicService = iTopicService;
            this._iLimitTimeBuyService = iLimitTimeBuyService;
        }

        public ActionResult Detail(long id)
        {
            TopicInfo topicInfo = this._iTopicService.GetTopicInfo(id);
            string str = VTemplateHelper.GetTemplatePath(id.ToString(), VTemplateClientTypes.WapSpecial, 0L);
            string viewName = "~" + str + "Skin-HomePage.cshtml";
            if (topicInfo != null)
            {
                ((dynamic)base.ViewBag).Title = "专题-" + topicInfo.Name;
            }
            VTemplateHelper.DownloadTemplate(id.ToString(), VTemplateClientTypes.WapSpecial, 0L);
            return base.View(viewName, topicInfo);
        }

        public ActionResult List(int pageNo = 1, int pageSize = 10)
        {
            TopicQuery topicQuery = new TopicQuery
            {
                ShopId = 0,
                PlatformType = PlatformType.Mobile,
                PageNo = pageNo,
                PageSize = pageSize
            };
            List<TopicInfo> models = this._iTopicService.GetTopics(topicQuery).Models;
            return base.View(models);
        }

        [HttpPost]
        public JsonResult LoadProducts(long topicId, long moduleId, int page, int pageSize)
        {
            IEnumerable<long> ids = from item in
                                        (from item in this._iTopicService.GetTopicInfo(topicId).TopicModuleInfo.FirstOrDefault<TopicModuleInfo>(item => (item.Id == moduleId)).ModuleProductInfo
                                         where (item.ProductInfo.SaleStatus == ProductInfo.ProductSaleStatus.OnSale) && (item.ProductInfo.AuditStatus == ProductInfo.ProductAuditStatus.Audited)
                                         orderby item.Id
                                         select item).Skip<ModuleProductInfo>((pageSize * (page - 1))).Take<ModuleProductInfo>(pageSize)
                                    select item.ProductId;
            var data = this._iProductService.GetProductByIds(ids).ToArray<ProductInfo>().Select(delegate(ProductInfo item)
            {
                FlashSaleModel flaseSaleByProductId = this._iLimitTimeBuyService.GetFlaseSaleByProductId(item.Id);
                return new { name = item.ProductName, id = item.Id, image = item.GetImage(ImageSize.Size_350, 1), price = (flaseSaleByProductId != null) ? flaseSaleByProductId.MinPrice : item.MinSalePrice, marketPrice = item.MarketPrice };
            });
            return base.Json(data);
        }

        [HttpPost]
        public JsonResult TopicList(int pageNo = 1, int pageSize = 10)
        {
            TopicQuery topicQuery = new TopicQuery
            {
                ShopId = 0L,
                PlatformType = PlatformType.Mobile,
                PageNo = pageNo,
                PageSize = pageSize
            };
            var data = from item in this._iTopicService.GetTopics(topicQuery).Models.ToList<TopicInfo>() select new { Id = item.Id, TopImage = item.TopImage, Name = item.Name };
            return base.Json(data);
        }

    }
}