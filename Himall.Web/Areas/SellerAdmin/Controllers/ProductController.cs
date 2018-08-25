using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Mvc;
using Himall.Application;
using Himall.Core;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.App_Code;
using Himall.Web.App_Code.Common;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;
using Himall.Web.Models;
using Himall.CommonModel;
using AutoMapper;
using Himall.DTO;
using Himall.Entity;
using System.Collections;
using System.Drawing;
using Himall.Core.Helper;
using System.Drawing.Imaging;
using System.Text;
using System.Linq.Expressions;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class ProductController : BaseSellerController
    {
        // Fields
        private ICategoryService _iCategoryService;
        private IProductService _iProductService;
        private ISearchProductService _iSearchProductService;
        private IShopCategoryService _iShopCategoryService;
        private IShopService _iShopService;
        private ITypeService _iTypeService;
        private long shopId;

        // Methods
        public ProductController()
        {
            this.shopId = 2L;
            if (base.CurrentSellerManager != null)
            {
                this.shopId = base.CurrentSellerManager.ShopId;
            }
        }

        public ProductController(IShopCategoryService iShopCategoryService, IProductService iProductService, ITypeService iTypeService, ICategoryService iCategoryService, IShopService iShopService, ISearchProductService iSearchProductService)
            : this()
        {
            this._iShopCategoryService = iShopCategoryService;
            this._iProductService = iProductService;
            this._iTypeService = iTypeService;
            this._iCategoryService = iCategoryService;
            this._iShopService = iShopService;
            this._iSearchProductService = iSearchProductService;
        }

        [HttpPost, UnAuthorize]
        public JsonResult BatchOnSale(string ids)
        {
            IEnumerable<long> enumerable = from item in ids.Split(new char[] { ',' }) select long.Parse(item);
            this._iProductService.OnSale(enumerable, base.CurrentSellerManager.ShopId);
            LogInfo info = new LogInfo
            {
                Date = DateTime.Now,
                Description = "商家商品批量上架，Ids=" + ids,
                IPAddress = base.Request.UserHostAddress,
                PageUrl = "/Product/BatchOnSale",
                UserName = base.CurrentSellerManager.UserName,
                ShopId = base.CurrentSellerManager.ShopId
            };
            ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(info);
            this._iSearchProductService.UpdateSearchStatusByProducts(enumerable.ToList<long>());
            return base.Json(new { success = true });
        }

        [HttpPost, UnAuthorize]
        public JsonResult BatchSaleOff(string ids)
        {
            IEnumerable<long> enumerable = from item in ids.Split(new char[] { ',' }) select long.Parse(item);
            this._iProductService.SaleOff(enumerable, base.CurrentSellerManager.ShopId);
            LogInfo info = new LogInfo
            {
                Date = DateTime.Now,
                Description = "商家商品批量下架，Ids=" + ids,
                IPAddress = base.Request.UserHostAddress,
                PageUrl = "/Product/BatchSaleOff",
                UserName = base.CurrentSellerManager.UserName,
                ShopId = base.CurrentSellerManager.ShopId
            };
            ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(info);
            this._iSearchProductService.UpdateSearchStatusByProducts(enumerable.ToList<long>());
            foreach (long num in enumerable)
            {
                ShopBranchApplication.UnSaleProduct(num);
            }
            return base.Json(new { success = true });
        }

        [UnAuthorize, HttpPost]
        public JsonResult BindTemplates(string ids, long? topTemplateId, long? bottomTemplateId)
        {
            IEnumerable<long> productIds = from item in ids.Split(new char[] { ',' }) select long.Parse(item);
            this._iProductService.BindTemplate(topTemplateId, bottomTemplateId, productIds);
            foreach (long num in productIds)
            {
                Cache.Remove(CacheKeyCollection.CACHE_PRODUCTDESC(num));
            }
            return base.Json(new { success = true });
        }

        [HttpPost, UnAuthorize]
        public JsonResult Browse(long? categoryId, int? auditStatus, string ids, int page, string keyWords, int? saleStatus, bool? isShopCategory, int rows = 10, bool isLimitTimeBuy = false, bool showSku = false, long[] exceptProductIds = null)
        {
            ProductQuery productQueryModel = new ProductQuery
            {
                PageSize = rows,
                PageNo = page,
                KeyWords = keyWords,
                CategoryId = isShopCategory.GetValueOrDefault() ? null : categoryId,
                ShopCategoryId = isShopCategory.GetValueOrDefault() ? categoryId : null,
                Ids = string.IsNullOrWhiteSpace(ids) ? null : (from item in ids.Split(new char[] { ',' }) select long.Parse(item)),
                ShopId = new long?(base.CurrentSellerManager.ShopId),
                IsLimitTimeBuy = isLimitTimeBuy,
                ExceptIds = exceptProductIds
            };
            if (auditStatus.HasValue)
            {
                productQueryModel.AuditStatus = new ProductInfo.ProductAuditStatus[] { (ProductInfo.ProductAuditStatus)auditStatus.Value };
            }
            if (saleStatus.HasValue)
            {
                productQueryModel.SaleStatus = new ProductInfo.ProductSaleStatus?((ProductInfo.ProductSaleStatus)saleStatus.Value);
            }
            ObsoletePageModel<ProductInfo> products = this._iProductService.GetProducts(productQueryModel);
            ICategoryService productCategoryService = this._iCategoryService;
            IBrandService brandService = ServiceHelper.Create<IBrandService>();
            var data = new
            {
                rows = products.Models.ToArray().Select(delegate(ProductInfo item)
                {
                    BrandInfo brand = brandService.GetBrand(item.BrandId);
                    CategoryInfo category = productCategoryService.GetCategory(item.CategoryId);
                    return new { name = item.ProductName, brandName = ((item.BrandId == 0L) || (brand == null)) ? "" : brand.Name, categoryName = (brand == null) ? "" : category.Name, id = item.Id, imgUrl = item.GetImage(ImageSize.Size_50, 1), price = item.MinSalePrice, skus = !showSku ? null : (from a in item.SKUInfo select new SKUModel { Id = a.Id, SalePrice = a.SalePrice, Size = a.Size, Stock = a.Stock, Version = a.Version, Color = a.Color, Sku = a.Sku, AutoId = a.AutoId, ProductId = a.ProductId }) };
                }),
                total = products.Total
            };
            return base.Json(data);
        }

        private bool CanCreate(out string message)
        {
            if (ServiceHelper.Create<IShopService>().GetShopSpaceUsage(base.CurrentSellerManager.ShopId) == -1L)
            {
                message = "存储图片空间不足,不能发布商品!";
                return false;
            }
            ShopGradeInfo shopGrade = ServiceHelper.Create<IShopService>().GetShopGrade(base.CurrentShop.GradeId);
            if ((shopGrade != null) && (this._iProductService.GetShopAllProducts(base.CurrentSellerManager.ShopId) >= shopGrade.ProductLimit))
            {
                message = "此店铺等级最多只能发布" + shopGrade.ProductLimit + "件商品";
                return false;
            }
            message = "";
            return true;
        }

        public ActionResult CategoryBrands(long categoryId)
        {
            var data = (from p in ServiceHelper.Create<IBrandService>().GetBrandsByCategoryIds(base.CurrentShop.Id, new long[] { categoryId }) select new { Id = p.Id, Name = p.Name }).ToList();
            return base.Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CircleEdit(ProductCircleInfo model)
        {
            try
            {
                ProductManagerApplication.SubmitProductCircle(model);
                return this.Json(true, null);
            }
            catch (Exception exception)
            {
                Log.Error("编辑商品异常", exception);
                return this.Json(false, "操作失败！");
            }
        }

        public ActionResult CircleEdit(long id, long? circleId)
        {
            ProductCircleInfo model = new ProductCircleInfo
            {
                ProductId = new long?(id)
            };
            if (circleId.HasValue)
            {
                model = ProductManagerApplication.GetProductCircleInfo(circleId.Value);
            }
            return base.View(model);
        }

        public ActionResult CircleList(long id)
        {
            ((dynamic)base.ViewBag).ProductId = id;
            ((dynamic)base.ViewBag).List = ProductManagerApplication.GetProductCircleList(id);
            return base.View();
        }

        public ActionResult Create()
        {
            string str;
            ProductCreateModel model = this.InitCreateModel(null);
            if (!this.CanCreate(out str))
            {
                this.SendMessage(str, BaseController.MessageType.Alert, null, false);
            }
            return base.View(model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Create(ProductCreateModel model)
        {
            string str;
            model.MinSalePrice = model.SKUExs.Min<SKUEx>((Func<SKUEx, decimal>)(p => p.SalePrice));
            if (!base.ModelState.IsValid)
            {
                ICollection<ModelState> values = base.ModelState.Values;
                foreach (ModelState state in values)
                {
                    if (state.Errors.Count > 0)
                    {
                        Log.Error(state.Errors[0].ErrorMessage);
                    }
                }
                return this.ValidError();
            }
            if (!this.CanCreate(out str))
            {
                return this.Json(false, str);
            }
            Himall.DTO.Product.Product product = Mapper.DynamicMap<Himall.DTO.Product.Product>(model);
            bool flag = false;
            try
            {
                SKUEx[] skus = model.SKUExs.Select<SKUEx, SKUEx>(delegate(SKUEx p)
                {
                    p.Id = p.CreateId(null);
                    return p;
                }).ToArray<SKUEx>();
                ProductDescription description = model.Description;
                ProductAttribute[] productAttribute = model.GetProductAttribute(model.Id);
                SellerSpecificationValue[] sellerSpecification = model.GetSellerSpecification(base.CurrentSellerManager.ShopId, product.TypeId);
                Himall.DTO.Product.Product product2 = ProductManagerApplication.AddProduct(base.CurrentShop.Id, product, model.Pics, skus, description, productAttribute, model.GoodsCategory, sellerSpecification);
                this._iSearchProductService.AddSearchProduct(product2.Id);
                flag = true;
            }
            catch (Exception exception)
            {
                Log.Error("创建商品失败", exception);
            }
            try
            {
                LogInfo info = new LogInfo
                {
                    Date = DateTime.Now,
                    Description = string.Format("商家发布商品，Id={0}, 名称={1} [{2}]", product.Id, product.ProductName, flag ? "成功" : "失败"),
                    IPAddress = base.Request.UserHostAddress,
                    PageUrl = "/Product/Create",
                    UserName = base.CurrentSellerManager.UserName,
                    ShopId = base.CurrentSellerManager.ShopId
                };
                ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(info);
            }
            catch
            {
            }
            if (flag)
            {
                return this.Json(true, null);
            }
            return this.Json(false, "发布商品出错");
        }

        private SKUSpecModel DeepClone(SKUSpecModel obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Seek(0L, SeekOrigin.Begin);
                return (formatter.Deserialize(stream) as SKUSpecModel);
            }
        }

        [HttpPost, UnAuthorize]
        public JsonResult Delete(string ids)
        {
            try
            {
                IEnumerable<long> enumerable = from item in ids.Split(new char[] { ',' }) select long.Parse(item);
                ProductManagerApplication.DeleteProduct(enumerable, base.CurrentSellerManager.ShopId);
                this._iSearchProductService.UpdateSearchStatusByProducts(enumerable.ToList<long>());
                foreach (long num in enumerable)
                {
                    string dirName = string.Format("/Storage/Shop/{0}/Products/{1}", base.CurrentSellerManager.ShopId, num);
                    if (HimallIO.ExistDir(dirName))
                    {
                        HimallIO.DeleteDir(dirName, true);
                    }
                }
                LogInfo info = new LogInfo
                {
                    Date = DateTime.Now,
                    Description = "商家删除商品，Ids=" + ids,
                    IPAddress = base.Request.UserHostAddress,
                    PageUrl = "/Product/Delete",
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

        [UnAuthorize, HttpPost]
        public JsonResult DeleteCircle(long id)
        {
            try
            {
                ProductManagerApplication.DeleteProduct(id);
                return base.Json(new { success = true });
            }
            catch (Exception exception)
            {
                return base.Json(new { success = false, msg = exception.Message });
            }
        }

        private void DeleteDirectory(string path)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            if (info.Exists)
            {
                DirectoryInfo[] directories = info.GetDirectories();
                foreach (DirectoryInfo info2 in directories)
                {
                    info2.Delete(true);
                }
                info.Delete(true);
            }
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Edit(ProductCreateModel model)
        {
            int num;
            model.MinSalePrice = model.SKUExs.Min<SKUEx>((Func<SKUEx, decimal>)(p => p.SalePrice));
            if (!base.ModelState.IsValid)
            {
                return this.ValidError();
            }
            SKUEx[] sKUExs = model.SKUExs;
            for (num = 0; num < sKUExs.Length; num++)
            {
                SKUEx ex = sKUExs[num];
                if ((string.IsNullOrWhiteSpace(ex.Color) && string.IsNullOrWhiteSpace(ex.Size)) && string.IsNullOrWhiteSpace(ex.Version))
                {
                    ex.ColorId = 0;
                    ex.SizeId = 0;
                    ex.VersionId = 0;
                    ex.Id = model.Id + "_0_0_0";
                }
            }
            SKUEx[] skus = model.SKUExs.Select<SKUEx, SKUEx>(delegate(SKUEx sku)
            {
                sku.ProductId = model.Id;
                sku.Id = sku.CreateId(new long?(model.Id));
                return sku;
            }).ToArray<SKUEx>();
            using (ILimitTimeBuyService service = ServiceHelper.Create<ILimitTimeBuyService>())
            {
                FlashSaleModel flaseSaleByProductId = service.GetFlaseSaleByProductId(model.Id);
                if (flaseSaleByProductId != null)
                {
                    if (DateTime.Parse(flaseSaleByProductId.EndDate) > DateTime.Now)
                    {
                        sKUExs = skus;
                        for (num = 0; num < sKUExs.Length; num++)
                        {
                            SKUEx ex2 = sKUExs[num];
                            LimitOrderHelper.ModifyLimitStock(ex2.Id, (int)ex2.Stock, DateTime.Parse(flaseSaleByProductId.EndDate));
                        }
                    }
                    using (Entities entities = new Entities())
                    {
                        sKUExs = skus;
                        for (num = 0; num < sKUExs.Length; num++)
                        {
                            SKUEx sku = sKUExs[num];
                            if (entities.FlashSaleDetailInfo.FirstOrDefault<FlashSaleDetailInfo>(t => (t.SkuId == sku.Id)) == null)
                            {
                                FlashSaleDetailInfo entity = new FlashSaleDetailInfo
                                {
                                    FlashSaleId = flaseSaleByProductId.Id,
                                    Price = new decimal?(sku.SalePrice),
                                    ProductId = flaseSaleByProductId.ProductId,
                                    SkuId = sku.Id
                                };
                                entities.FlashSaleDetailInfo.Add(entity);
                            }
                        }
                        entities.SaveChanges();
                    }
                }
            }
            if (model.SaleStatus != ProductInfo.ProductSaleStatus.InDraft)
            {
                model.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
            }
            ProductDescription description = model.Description;
            Himall.DTO.Product.Product product = model.Map<Himall.DTO.Product.Product>();
            ProductAttribute[] productAttribute = model.GetProductAttribute(model.Id);
            SellerSpecificationValue[] sellerSpecification = model.GetSellerSpecification(base.CurrentSellerManager.ShopId, model.TypeId);
            bool flag = false;
            try
            {
                ProductManagerApplication.UpdateProduct(product, model.Pics, skus, description, productAttribute, model.GoodsCategory, sellerSpecification);
                this._iSearchProductService.UpdateSearchProduct(product.Id);
                Himall.DTO.Product.Product product2 = ProductManagerApplication.GetProduct(product.Id);
                ShopBranchApplication.CorrectBranchProductSkus(product2.Id, product2.ShopId);
                if ((product2.SaleStatus != ProductInfo.ProductSaleStatus.OnSale) || (product2.AuditStatus != ProductInfo.ProductAuditStatus.Audited))
                {
                    FightGroupActiveModel activeByProductId = FightGroupApplication.GetActiveByProductId(product2.Id);
                    if (activeByProductId != null)
                    {
                        activeByProductId.EndTime = DateTime.Now.AddMinutes(-1.0);
                        FightGroupApplication.UpdateActive(activeByProductId);
                    }
                    ShopBranchApplication.UnSaleProduct(product2.Id);
                }
                flag = true;
            }
            catch (Exception exception)
            {
                Log.Error("编辑商品异常", exception);
            }
            try
            {
                LogInfo info = new LogInfo
                {
                    Date = DateTime.Now,
                    Description = string.Format("商家修改商品，Id={0}, 名称={1} [{2}]", model.Id, model.ProductName, flag ? "成功" : "失败"),
                    IPAddress = base.Request.UserHostAddress,
                    PageUrl = "/Product/Edit",
                    UserName = base.CurrentSellerManager.UserName,
                    ShopId = base.CurrentSellerManager.ShopId
                };
                ServiceHelper.Create<IOperationLogService>().AddSellerOperationLog(info);
            }
            catch
            {
            }
            if (flag)
            {
                return this.Json(true, null);
            }
            return this.Json(false, "操作失败！");
        }

        public ActionResult Edit(long id)
        {
            Himall.DTO.Product.Product product = ProductManagerApplication.GetProduct(id);
            if (product == null)
            {
                return base.HttpNotFound();
            }
            ProductCreateModel model = this.InitEditModel(product, null);
            return base.View(model);
        }

        public ActionResult ExportToExcel(long? categoryId = new long?(), string productCode = "", string brandName = "", int? auditStatus = new int?(), string auditStatuses = null, int? saleStatus = new int?(), string ids = "", string keyWords = "", DateTime? startDate = new DateTime?(), DateTime? endDate = new DateTime?())
        {
            ProductQuery query2 = new ProductQuery();
            int? nullable = saleStatus;
            query2.SaleStatus = nullable.HasValue ? new ProductInfo.ProductSaleStatus?((ProductInfo.ProductSaleStatus)nullable) : null;
            query2.PageSize = 0x7fffffff;
            query2.PageNo = 1;
            query2.BrandNameKeyword = brandName;
            query2.KeyWords = keyWords;
            query2.ShopCategoryId = categoryId;
            query2.Ids = string.IsNullOrWhiteSpace(ids) ? null : (from item in ids.Split(new char[] { ',' }) select long.Parse(item));
            query2.ShopId = new long?(base.CurrentSellerManager.ShopId);
            query2.StartDate = startDate;
            query2.EndDate = endDate;
            query2.ProductCode = productCode;
            ProductQuery productQueryModel = query2;
            if (!string.IsNullOrWhiteSpace(auditStatuses))
            {
                productQueryModel.AuditStatus = (from item in auditStatuses.Split(new char[] { ',' }) select (ProductInfo.ProductAuditStatus)((int)long.Parse(item))).ToArray<ProductInfo.ProductAuditStatus>();
                if ((auditStatuses == "1,3") || (auditStatuses == "1"))
                {
                    productQueryModel.SaleStatus = ProductInfo.ProductSaleStatus.OnSale;
                }
            }
            if (saleStatus.HasValue)
            {
                nullable = saleStatus;
            }
            if (((nullable.GetValueOrDefault() == 2) && nullable.HasValue) && (((nullable = auditStatus).GetValueOrDefault() != 4) || !nullable.HasValue))
            {
                productQueryModel.AuditStatus = new ProductInfo.ProductAuditStatus[] { ProductInfo.ProductAuditStatus.Audited, ProductInfo.ProductAuditStatus.UnAudit, ProductInfo.ProductAuditStatus.WaitForAuditing };
            }
            if (auditStatus.HasValue)
            {
                productQueryModel.AuditStatus = new ProductInfo.ProductAuditStatus[] { (ProductInfo.ProductAuditStatus)auditStatus.Value };
            }
            ObsoletePageModel<ProductInfo> products = this._iProductService.GetProducts(productQueryModel);
            IShopCategoryService shopCategoryService = this._iShopCategoryService;
            IBrandService brandService = ServiceHelper.Create<IBrandService>();
            IEnumerable<ProductInfoForExportModel> enumerable = products.Models.ToArray().Select<ProductInfo, ProductInfoForExportModel>(delegate(ProductInfo item)
            {
                BrandInfo brand = brandService.GetBrand(item.BrandId);
                ShopCategoryInfo categoryByProductId = shopCategoryService.GetCategoryByProductId(item.Id);
                return new ProductInfoForExportModel
                {
                    Id = item.Id,
                    CategoryName = (categoryByProductId == null) ? "" : categoryByProductId.Name,
                    BrandName = ((item.BrandId == 0L) || (brand == null)) ? "" : brand.Name,
                    ProductName = item.ProductName,
                    MarketPrice = item.MarketPrice,
                    MinSalePrice = item.MinSalePrice,
                    ProductCode = item.ProductCode,
                    ShortDescription = item.ShortDescription,
                    SaleStatus = item.SaleStatus,
                    AddedDate = item.AddedDate,
                    HasSKU = (((item.SKUInfo != null) && (item.SKUInfo.Count<SKUInfo>() == 1)) && item.SKUInfo.FirstOrDefault<SKUInfo>().Id.Contains("0_0_0")) ? false : true,
                    VistiCounts = item.VistiCounts,
                    SaleCounts = item.SaleCounts,
                    AuditStatus = item.AuditStatus,
                    AuditReason = (item.ProductDescriptionInfo != null) ? item.ProductDescriptionInfo.AuditReason : "",
                    Quantity = item.Quantity,
                    MeasureUnit = item.MeasureUnit,
                    SKUInfo = item.SKUInfo
                };
            }).ToList<ProductInfoForExportModel>();
            base.ViewData.Model = enumerable;
            string s = this.RenderPartialViewToString(this, "ExportProductinfo");
            return this.File(Encoding.UTF8.GetBytes(s), "application/ms-excel", string.Format("店铺商品信息_{0}.xls", DateTime.Now.ToString("yyyy-MM-dd")));
        }

        [UnAuthorize]
        public JsonResult GetAttributes(long categoryId, long productId = 0L, long isCategoryId = 0L)
        {
            List<TypeAttributesModel> list;
            Dictionary<long, string> dictionary = new Dictionary<long, string>();
            if (productId > 0L)
            {
                ProductInfo product = this._iProductService.GetProduct(productId);
                if ((product != null) && (product.CategoryId != categoryId))
                {
                    isCategoryId = 1L;
                }
            }
            if (isCategoryId == 1L)
            {
                list = this.GetPlateformAttr(categoryId);
                return base.Json(new { json = list }, JsonRequestBehavior.AllowGet);
            }
            list = new List<TypeAttributesModel>();
            IQueryable<ProductAttributeInfo> productAttribute = this._iProductService.GetProductAttribute(productId);
            if ((productAttribute == null) || (productAttribute.Count<ProductAttributeInfo>() == 0))
            {
                list = this.GetPlateformAttr(categoryId);
                return base.Json(new { json = list }, JsonRequestBehavior.AllowGet);
            }
            foreach (ProductAttributeInfo info2 in productAttribute.ToArray<ProductAttributeInfo>())
            {
                if (!dictionary.ContainsKey(info2.AttributeId))
                {
                    dictionary.Add(info2.AttributeId, info2.ValueId.ToString());
                }
                else
                {
                    dictionary[info2.AttributeId] = dictionary[info2.AttributeId] + "," + info2.ValueId.ToString();
                    continue;
                }
                AttributeInfo attr = info2.AttributesInfo;
                ICollection<AttributeValueInfo> attributeValueInfo = this._iTypeService.GetType(attr.TypeId).AttributeInfo.FirstOrDefault<AttributeInfo>(a => (a.Id == attr.Id)).AttributeValueInfo;
                TypeAttributesModel model = new TypeAttributesModel
                {
                    Name = attr.Name,
                    AttrId = info2.AttributeId,
                    Selected = "",
                    IsMulti = attr.IsMulti,
                    AttrValues = new List<TypeAttrValue>()
                };
                foreach (AttributeValueInfo info3 in attributeValueInfo.ToArray<AttributeValueInfo>())
                {
                    TypeAttrValue value2 = new TypeAttrValue
                    {
                        Id = info3.Id.ToString(),
                        Name = info3.Value
                    };
                    model.AttrValues.Add(value2);
                }
                categoryId = this._iProductService.GetProduct(productId).CategoryId;
                list.Add(model);
            }
            List<TypeAttributesModel> plateformAttr = this.GetPlateformAttr(categoryId);
            using (List<TypeAttributesModel>.Enumerator enumerator = list.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Func<TypeAttributesModel, bool> predicate = null;
                    TypeAttributesModel item = enumerator.Current;
                    item.Selected = dictionary[item.AttrId];
                    if (predicate == null)
                    {
                        predicate = a => a.AttrId == item.AttrId;
                    }
                    plateformAttr.Remove(plateformAttr.FirstOrDefault<TypeAttributesModel>(predicate));
                }
            }
            list.AddRange(plateformAttr);
            return base.Json(new { json = list }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFreightTemplate()
        {
            List<FreightTemplateInfo> shopFreightTemplate = ServiceHelper.Create<IFreightTemplateService>().GetShopFreightTemplate(base.CurrentSellerManager.ShopId);
            List<SelectListItem> list3 = new List<SelectListItem>();
            SelectListItem item2 = new SelectListItem
            {
                Selected = false,
                Text = "请选择运费模板...",
                Value = "0"
            };
            list3.Add(item2);
            List<SelectListItem> list2 = list3;
            foreach (FreightTemplateInfo info in shopFreightTemplate)
            {
                SelectListItem item = new SelectListItem
                {
                    Text = info.Name + "【" + info.ValuationMethod.ToDescription() + "】",
                    Value = info.Id.ToString()
                };
                list2.Add(item);
            }
            return base.Json(new
            {
                success = true,
                model = (from f in list2
                         where !f.Value.Equals("0")
                         select f).ToList<SelectListItem>()
            }, JsonRequestBehavior.AllowGet);
        }

        private List<TypeAttributesModel> GetPlateformAttr(long categoryId)
        {
            CategoryInfo category = this._iCategoryService.GetCategory(categoryId);
            ProductTypeInfo type = this._iTypeService.GetType(category.TypeId);
            List<TypeAttributesModel> list = new List<TypeAttributesModel>();
            foreach (AttributeInfo info3 in type.AttributeInfo)
            {
                TypeAttributesModel item = new TypeAttributesModel
                {
                    Name = info3.Name,
                    AttrId = info3.Id,
                    Selected = "",
                    IsMulti = info3.IsMulti,
                    AttrValues = new List<TypeAttrValue>()
                };
                foreach (AttributeValueInfo info4 in info3.AttributeValueInfo)
                {
                    TypeAttrValue value2 = new TypeAttrValue
                    {
                        Id = info4.Id.ToString(),
                        Name = info4.Value
                    };
                    item.AttrValues.Add(value2);
                }
                list.Add(item);
            }
            return list;
        }

        private SpecJosnModel GetPlatformSpec(long categoryId, long productId = 0)
        {
            SpecJosnModel data = new SpecJosnModel()
            {
                json = new List<TypeSpecificationModel>(),
                tableData = new tableDataModel()
                {
                    cost = new List<SKUSpecModel>(),
                    mallPrice = new List<SKUSpecModel>(),
                    productId = productId,
                    sku = new List<SKUSpecModel>(),
                    stock = new List<SKUSpecModel>()
                }
            };
            ProductTypeInfo type = this._iTypeService.GetType(this._iCategoryService.GetCategory(categoryId).TypeId);
            foreach (SpecificationType specificationType in Enum.GetValues(typeof(SpecificationType)))
            {
                SpecificationType spec = specificationType;
                bool flag = true;
                if (productId == 0L)
                {
                    flag = false;
                    switch (spec)
                    {
                        case SpecificationType.Color:
                            if (type.IsSupportColor)
                            {
                                flag = true;
                                break;
                            }
                            break;
                        case SpecificationType.Size:
                            if (type.IsSupportSize)
                            {
                                flag = true;
                                break;
                            }
                            break;
                        case SpecificationType.Version:
                            if (type.IsSupportVersion)
                            {
                                flag = true;
                                break;
                            }
                            break;
                    }
                }
                if (flag)
                {
                    TypeSpecificationModel specificationModel = new TypeSpecificationModel()
                    {
                        Name = Enum.GetNames(typeof(SpecificationType))[(int)(spec - 1)],
                        Values = new List<Specification>(),
                        SpecId = (long)spec
                    };
                    foreach (SpecificationValueInfo specificationValueInfo in (IEnumerable<SpecificationValueInfo>)Enumerable.OrderBy<SpecificationValueInfo, string>(Enumerable.Where<SpecificationValueInfo>((IEnumerable<SpecificationValueInfo>)type.SpecificationValueInfo, (Func<SpecificationValueInfo, bool>)(s => s.Specification == spec)), (Func<SpecificationValueInfo, string>)(s => s.Value)))
                        specificationModel.Values.Add(new Specification()
                        {
                            Id = specificationValueInfo.Id.ToString(),
                            Name = specificationValueInfo.Value,
                            isPlatform = true,
                            Selected = false
                        });
                    data.json.Add(specificationModel);
                }
            }
            this.InitialTableData((IQueryable<SKUInfo>)Queryable.ThenBy<SKUInfo, string>(Queryable.ThenBy<SKUInfo, string>(Queryable.OrderBy<SKUInfo, string>(this._iProductService.GetSKUs(productId), (Expression<Func<SKUInfo, string>>)(s => s.Color)), (Expression<Func<SKUInfo, string>>)(s => s.Size)), (Expression<Func<SKUInfo, string>>)(s => s.Version)), data);
            return data;
        }

        private List<SKUInfo> GetProducrSpec(List<SKUInfo> skuList)
        {
            List<SKUInfo> list = new List<SKUInfo>();
            foreach (SKUInfo info in skuList)
            {
                string[] strArray = string.IsNullOrWhiteSpace(info.Id) ? new string[] { "" } : info.Id.Split(new char[] { '_' });
                SKUInfo item = new SKUInfo
                {
                    Color = (strArray.Length >= 2) ? strArray[1] : "",
                    Size = (strArray.Length >= 3) ? strArray[2] : "",
                    Version = (strArray.Length >= 4) ? strArray[3] : "",
                    Id = info.Id
                };
                list.Add(item);
            }
            return list;
        }

        public string GetQrCodeImagePath(long productId)
        {
            Bitmap bitmap = QRCodeHelper.Create(CurrentUrlHelper.CurrentUrlNoPort() + "/m-wap/product/detail/" + productId);
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Gif);
            return ("data:image/gif;base64," + Convert.ToBase64String(stream.ToArray()));
        }

        [UnAuthorize]
        private List<CategoryJsonModel> GetShopCategoryJson(long shopId)
        {
            ShopCategoryInfo[] source = this._iShopCategoryService.GetShopCategory(shopId).ToArray<ShopCategoryInfo>();
            List<CategoryJsonModel> list = new List<CategoryJsonModel>();
            using (IEnumerator<ShopCategoryInfo> enumerator = (from s in source
                                                               where s.ParentCategoryId == 0L
                                                               select s).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Func<ShopCategoryInfo, bool> predicate = null;
                    ShopCategoryInfo main = enumerator.Current;
                    CategoryJsonModel item = new CategoryJsonModel
                    {
                        Name = main.Name,
                        Id = main.Id.ToString(),
                        SubCategory = new List<SecondLevelCategory>()
                    };
                    if (predicate == null)
                    {
                        predicate = s => s.ParentCategoryId == main.Id;
                    }
                    foreach (ShopCategoryInfo info in source.Where<ShopCategoryInfo>(predicate))
                    {
                        SecondLevelCategory category = new SecondLevelCategory
                        {
                            Name = info.Name,
                            Id = info.Id.ToString()
                        };
                        item.SubCategory.Add(category);
                    }
                    list.Add(item);
                }
            }
            return list;
        }

        [UnAuthorize]
        public JsonResult GetShopProductCategory(long productId = 0L)
        {
            ShopProductCategoryModel model = new ShopProductCategoryModel
            {
                SelectedCategory = new List<SelectedCategory>()
            };
            model.Data = this.GetShopCategoryJson(this.shopId);
            if (0L != productId)
            {
                IQueryable<ProductShopCategoryInfo> productShopCategories = this._iProductService.GetProductShopCategories(productId);
                foreach (CategoryJsonModel model2 in model.Data)
                {
                    long id = long.Parse(model2.Id);
                    if (productShopCategories.Any<ProductShopCategoryInfo>(c => c.ShopCategoryId == id))
                    {
                        SelectedCategory item = new SelectedCategory
                        {
                            Id = model2.Id,
                            Level = "1"
                        };
                        model.SelectedCategory.Add(item);
                    }
                    foreach (SecondLevelCategory category in model2.SubCategory)
                    {
                        id = long.Parse(category.Id);
                        if (productShopCategories.Any<ProductShopCategoryInfo>(c => c.ShopCategoryId == id))
                        {
                            SelectedCategory category2 = new SelectedCategory
                            {
                                Id = category.Id,
                                Level = "2"
                            };
                            model.SelectedCategory.Add(category2);
                        }
                    }
                }
            }
            return base.Json(new { json = model }, JsonRequestBehavior.AllowGet);
        }

        [UnAuthorize]
        public JsonResult GetSpecifications(long categoryId, long productId = 0L, long isCategoryId = 0L)
        {
            Dictionary<long, string> dictionary = new Dictionary<long, string>();
            CategoryInfo category = this._iCategoryService.GetCategory(categoryId);
            SpecJosnModel platformSpec = this.GetPlatformSpec(categoryId, productId);
            IQueryable<SellerSpecificationValueInfo> sellerSpecifications = this._iProductService.GetSellerSpecifications(this.shopId, category.TypeId);
            List<SKUInfo> skuList = this._iProductService.GetSKUs(productId).ToList<SKUInfo>();
            List<SKUInfo> producrSpec = this.GetProducrSpec(skuList);
            List<SKUInfo> source = skuList.ToList<SKUInfo>();
            using (List<TypeSpecificationModel>.Enumerator enumerator = platformSpec.json.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    TypeSpecificationModel item = enumerator.Current;
                    IQueryable<SellerSpecificationValueInfo> queryable2 = from s in sellerSpecifications
                                                                          where ((int)s.Specification) == item.SpecId
                                                                          select s;
                    Specification specification = null;
                    using (IEnumerator<SellerSpecificationValueInfo> enumerator2 = queryable2.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            Func<Specification, bool> predicate = null;
                            Func<SKUInfo, bool> func2 = null;
                            Func<SKUInfo, bool> func3 = null;
                            Func<SKUInfo, bool> func4 = null;
                            SellerSpecificationValueInfo pspec = enumerator2.Current;
                            if (predicate == null)
                            {
                                predicate = s => s.Id == pspec.ValueId.ToString();
                            }
                            specification = item.Values.FirstOrDefault<Specification>(predicate);
                            if ((specification != null) && (specification.Id == pspec.ValueId.ToString()))
                            {
                                specification.Name = pspec.Value;
                                specification.isPlatform = false;
                            }
                            if (func2 == null)
                            {
                                func2 = s => s.Color.Equals(pspec.Value);
                            }
                            if (!source.Any<SKUInfo>(func2))
                            {
                                if (func3 == null)
                                {
                                    func3 = s => s.Size.Equals(pspec.Value);
                                }
                                if (!source.Any<SKUInfo>(func3))
                                {
                                }
                            }
                            if ((func4 != null) || source.Any<SKUInfo>((func4 = s => s.Version.Equals(pspec.Value))))
                            {
                                specification.Selected = true;
                            }
                        }
                    }
                    using (List<Specification>.Enumerator enumerator3 = item.Values.GetEnumerator())
                    {
                        while (enumerator3.MoveNext())
                        {
                            Func<SKUInfo, bool> func5 = null;
                            Func<SKUInfo, bool> func6 = null;
                            Func<SKUInfo, bool> func7 = null;
                            Func<SKUInfo, bool> func8 = null;
                            Func<SKUInfo, bool> func9 = null;
                            Func<SKUInfo, bool> func10 = null;
                            Specification val = enumerator3.Current;
                            if (item.Name == "Color")
                            {
                            }
                            if ((func5 == null) && producrSpec.Any<SKUInfo>((func5 = s => val.Id == s.Color)))
                            {
                                if (func6 == null)
                                {
                                    func6 = s => val.Id == s.Color;
                                }
                                SKUInfo sku = producrSpec.FirstOrDefault<SKUInfo>(func6);
                                val.Name = skuList.FirstOrDefault<SKUInfo>(p => (p.Id == sku.Id)).Color;
                                val.isPlatform = false;
                                val.Selected = true;
                            }
                            if (item.Name == "Size")
                            {
                            }
                            if ((func7 == null) && producrSpec.Any<SKUInfo>((func7 = s => val.Id == s.Size)))
                            {
                                if (func8 == null)
                                {
                                    func8 = s => val.Id == s.Size;
                                }
                                SKUInfo sku = producrSpec.FirstOrDefault<SKUInfo>(func8);
                                val.Name = skuList.FirstOrDefault<SKUInfo>(p => (p.Id == sku.Id)).Size;
                                val.isPlatform = false;
                                val.Selected = true;
                            }
                            if (item.Name == "Version")
                            {
                            }
                            if ((func9 == null) && producrSpec.Any<SKUInfo>((func9 = s => val.Id == s.Version)))
                            {
                                if (func10 == null)
                                {
                                    func10 = s => val.Id == s.Version;
                                }
                                SKUInfo sku = producrSpec.FirstOrDefault<SKUInfo>(func10);
                                val.Name = skuList.FirstOrDefault<SKUInfo>(p => (p.Id == sku.Id)).Version;
                                val.isPlatform = false;
                                val.Selected = true;
                            }
                        }
                    }
                }
            }
            return base.Json(new { data = platformSpec }, JsonRequestBehavior.AllowGet);
        }

        private string GetSpecificationTypeAlias(SpecificationType typeId, SpecificationValueInfo item)
        {
            switch (typeId)
            {
                case SpecificationType.Color:
                    return item.TypeInfo.ColorAlias;

                case SpecificationType.Size:
                    return item.TypeInfo.SizeAlias;
            }
            return item.TypeInfo.VersionAlias;
        }

        private List<CategoryGroup> GroupCategory(List<CategoryInfo> categorys, long pid = 0L)
        {
            List<CategoryGroup> list = new List<CategoryGroup>();
            foreach (CategoryInfo categoryInfo in Enumerable.Where<CategoryInfo>((IEnumerable<CategoryInfo>)categorys, (Func<CategoryInfo, bool>)(p => p.ParentCategoryId == pid)))
            {
                CategoryGroup categoryGroup = new CategoryGroup();
                categoryGroup.Id = categoryInfo.Id;
                categoryGroup.Name = categoryInfo.Name;
                categoryGroup.Path = categoryInfo.Path;
                categoryGroup.TypeId = categoryInfo.TypeId;
                categoryGroup.SubCategorys = this.GroupCategory(categorys, categoryGroup.Id);
                list.Add(categoryGroup);
            }
            return list;
        }

        private ProductCreateModel InitCreateModel(ProductCreateModel model = null)
        {
            if (model == null)
            {
                model = new ProductCreateModel();
            }
            long id = base.CurrentShop.Id;
            IQueryable<CategoryInfo> businessCategory = this._iShopCategoryService.GetBusinessCategory(id, base.CurrentShop.IsSelf);
            List<CategoryGroup> list = this.GroupCategory(businessCategory.ToList<CategoryInfo>(), 0L);
            model.CategoryGroups = list;
            model.FreightTemplates = FreightTemplateApplication.GetShopFreightTemplate(id);
            model.ShopCategorys = ShopCategoryApplication.GetShopCategory(id);
            model.DescriptionTemplates = ProductManagerApplication.GetDescriptionTemplatesByShopId(id);
            return model;
        }

        private ProductCreateModel InitEditModel(Himall.DTO.Product.Product product, ProductCreateModel model = null)
        {
            if (model == null)
            {
                model = Mapper.DynamicMap<ProductCreateModel>(product);
                model.GoodsCategory = (from item in ProductManagerApplication.GetProductShopCategoriesByProductId(product.Id) select item.ShopCategoryId).ToArray<long>();
                model.SKUExs = (from p in SKUApplication.GetByProductIds(new long[] { product.Id }) select p.DynamicMap<SKUEx>()).ToArray<SKUEx>();
                model.Description = ProductManagerApplication.GetProductDescription(product.Id);
                model.SelectAttributes = (from p in ProductManagerApplication.GetProductAttribute(product.Id)
                                          group p by p.AttributeId into p
                                          select new AttrSelectData { AttributeId = p.Key, ValueId = string.Join<long>(",", from item in p select item.ValueId) }).ToArray<AttrSelectData>();
            }
            model.Stock = model.SKUExs.Sum<SKUEx>((Func<SKUEx, long>)(p => p.Stock));
            model.SafeStock = model.SKUExs.Min<SKUEx>((Func<SKUEx, long?>)(p => p.SafeStock));
            this.InitCreateModel(model);
            return model;
        }

        private void InitialTableData(IQueryable<SKUInfo> skus, SpecJosnModel data)
        {
            if (skus.Count<SKUInfo>() != 0)
            {
                int num = 0;
                string version = "";
                SKUInfo[] infoArray = skus.ToArray<SKUInfo>();
                if (!string.IsNullOrWhiteSpace(infoArray[0].Version))
                {
                    num = 2;
                    version = infoArray[0].Version;
                }
                if (!string.IsNullOrWhiteSpace(infoArray[0].Size))
                {
                    num = 1;
                    version = infoArray[0].Size;
                }
                if (!string.IsNullOrWhiteSpace(infoArray[0].Color))
                {
                    num = 0;
                    version = infoArray[0].Color;
                }
                if (!string.IsNullOrWhiteSpace(version))
                {
                    SKUSpecModel model = new SKUSpecModel
                    {
                        ValueSet = new List<string>()
                    };
                    SKUSpecModel model2 = new SKUSpecModel
                    {
                        ValueSet = new List<string>()
                    };
                    SKUSpecModel model3 = new SKUSpecModel
                    {
                        ValueSet = new List<string>()
                    };
                    SKUSpecModel model4 = new SKUSpecModel
                    {
                        ValueSet = new List<string>()
                    };
                    foreach (SKUInfo info in skus)
                    {
                        string color = "";
                        switch (num)
                        {
                            case 0:
                                color = info.Color;
                                break;

                            case 1:
                                color = info.Size;
                                break;

                            case 2:
                                color = info.Version;
                                break;
                        }
                        if (color.Equals(version))
                        {
                            model.ValueSet.Add((info.CostPrice == 0M) ? "" : info.CostPrice.ToString("f2"));
                            model.index = color;
                            model2.ValueSet.Add((info.Stock == 0L) ? "" : info.Stock.ToString("f2"));
                            model2.index = color;
                            model3.ValueSet.Add(info.Sku);
                            model3.index = color;
                            model4.ValueSet.Add((info.SalePrice == 0M) ? "" : info.SalePrice.ToString("f2"));
                            model4.index = color;
                        }
                        else
                        {
                            data.tableData.cost.Add(this.DeepClone(model));
                            data.tableData.stock.Add(this.DeepClone(model2));
                            data.tableData.sku.Add(this.DeepClone(model3));
                            data.tableData.mallPrice.Add(this.DeepClone(model4));
                            model = new SKUSpecModel
                            {
                                ValueSet = new List<string>()
                            };
                            model2 = new SKUSpecModel
                            {
                                ValueSet = new List<string>()
                            };
                            model3 = new SKUSpecModel
                            {
                                ValueSet = new List<string>()
                            };
                            model4 = new SKUSpecModel
                            {
                                ValueSet = new List<string>()
                            };
                            model.ValueSet.Add((info.CostPrice == 0M) ? "" : info.CostPrice.ToString("f2"));
                            model.index = color;
                            model2.ValueSet.Add((info.Stock == 0L) ? "" : info.Stock.ToString("f2"));
                            model2.index = color;
                            model3.ValueSet.Add(info.Sku);
                            model3.index = color;
                            model4.ValueSet.Add((info.SalePrice == 0M) ? "" : info.SalePrice.ToString("f2"));
                            model4.index = color;
                            version = color;
                        }
                    }
                    data.tableData.cost.Add(this.DeepClone(model));
                    data.tableData.stock.Add(this.DeepClone(model2));
                    data.tableData.sku.Add(this.DeepClone(model3));
                    data.tableData.mallPrice.Add(this.DeepClone(model4));
                }
            }
        }

        private JsonResult Json(bool success = true, string message = null)
        {
            return base.Json(new { Success = success, Message = message }, true);
        }

        [HttpPost, UnAuthorize]
        public JsonResult List(ProductQuery queryModel, string auditStatuses, int page, int rows)
        {
            queryModel.PageSize = rows;
            queryModel.PageNo = page;
            queryModel.ShopId = new long?(base.CurrentSellerManager.ShopId);
            QueryPageModel<Himall.DTO.Product.Product> products = ProductManagerApplication.GetProducts(queryModel);
            List<ProductDescription> productDescriptions = ProductManagerApplication.GetProductDescription((from p in products.Models select p.Id).ToArray<long>());
            List<Category> categories = CategoryApplication.GetCategories();
            List<Brand> brands = BrandApplication.GetBrandsByIds(from p in products.Models select p.BrandId);
            List<ProductRelationProduct> relationProducts = ProductManagerApplication.GetRelationProductByProductIds(from p in products.Models select p.Id);
            IEnumerable<long> overSafeStockPids = ProductManagerApplication.GetOverSafeStockProductIds(from p in products.Models select p.Id);
            List<SKU> skus = ProductManagerApplication.GetSKU((IEnumerable<long>)(from p in products.Models select p.Id));
            DataGridModel<ProductModel> data = new DataGridModel<ProductModel>
            {
                total = products.Total,
                rows = products.Models.ToArray().Select<Himall.DTO.Product.Product, ProductModel>(delegate(Himall.DTO.Product.Product item)
                {
                    ShopCategoryInfo categoryByProductId = this._iShopCategoryService.GetCategoryByProductId(item.Id);
                    return new ProductModel
                    {
                        Name = item.ProductName,
                        Id = item.Id,
                        Image = item.GetImage(ImageSize.Size_50, 1),
                        Price = item.MinSalePrice,
                        Url = "",
                        PublishTime = item.AddedDate.ToString("yyyy-MM-dd HH:mm"),
                        SaleState = (int)item.SaleStatus,
                        CategoryId = item.CategoryId,
                        BrandId = item.BrandId,
                        AuditState = (int)item.AuditStatus,
                        AuditReason = productDescriptions.Any<ProductDescription>(pd => (pd.ProductId == item.Id)) ? productDescriptions.FirstOrDefault<ProductDescription>(pd => (pd.ProductId == item.Id)).AuditReason : "",
                        ProductCode = item.ProductCode,
                        QrCode = this.GetQrCodeImagePath(item.Id),
                        SaleCount = item.SaleCounts,
                        Unit = item.MeasureUnit,
                        Uid = this.CurrentSellerManager.Id,
                        CategoryName = (categoryByProductId == null) ? "" : categoryByProductId.Name,
                        BrandName = ((item.BrandId == 0L) || !brands.Any<Brand>(b => (b.Id == item.BrandId))) ? "" : brands.FirstOrDefault<Brand>(b => (b.Id == item.BrandId)).Name,
                        RelationProducts = relationProducts.Any<ProductRelationProduct>(p => (p.ProductId == item.Id)) ? relationProducts.FirstOrDefault<ProductRelationProduct>(p => (p.ProductId == item.Id)).Relation : "",
                        IsOverSafeStock = overSafeStockPids.Any<long>(e => e == item.Id),
                        Stock = (from sku in skus
                                 where sku.ProductId == item.Id
                                 select sku).Sum<SKU>((Func<SKU, long>)(sku => sku.Stock)),
                        MaxBuyCount = item.MaxBuyCount
                    };
                }).ToList<ProductModel>()
            };
            return base.Json(data);
        }

        public ActionResult Management()
        {
            int produtAuditOnOff = ServiceHelper.Create<ISiteSettingService>().GetSiteSettings().ProdutAuditOnOff;
            ((dynamic)base.ViewBag).AuditOnOff = produtAuditOnOff;
            return base.View();
        }

        public ActionResult PublicStepOne()
        {
            return base.View();
        }

        [UnAuthorize]
        public ActionResult PublicStepTwo(string categoryNames = "", long categoryId = 0L, long productId = 0L)
        {
            IProductService service = this._iProductService;
            ICategoryService service2 = this._iCategoryService;
            string str = "0";
            string str2 = "0";
            string str3 = "0";
            bool flag = false;
            ProductInfo model = new ProductInfo();
            if (productId != 0L)
            {
                model = service.GetProduct(productId);
                if ((model == null) || (model.ShopId != base.CurrentSellerManager.ShopId))
                {
                    throw new HimallException(productId + ",该商品已删除或者不属于该店铺");
                }
                if (model.SKUInfo.Count<SKUInfo>() > 0)
                {
                    IEnumerable<SKUInfo> source = from s in model.SKUInfo
                                                  where s.SalePrice > 0M
                                                  select s;
                    IEnumerable<SKUInfo> enumerable2 = from s in model.SKUInfo
                                                       where s.CostPrice > 0M
                                                       select s;
                    str = (source.Count<SKUInfo>() == 0) ? model.MinSalePrice.ToString("f3") : source.Min<SKUInfo>(((Func<SKUInfo, decimal>)(s => s.SalePrice))).ToString();
                    str2 = model.SKUInfo.Sum<SKUInfo>(((Func<SKUInfo, long>)(s => s.Stock))).ToString();
                    str3 = (enumerable2.Count<SKUInfo>() == 0) ? str3 : enumerable2.Min<SKUInfo>(((Func<SKUInfo, decimal>)(s => s.CostPrice))).ToString();
                }
                if (string.IsNullOrWhiteSpace(categoryNames))
                {
                    string[] strArray = model.CategoryPath.Split(new char[] { '|' });
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(strArray[i]))
                        {
                            CategoryInfo category = service2.GetCategory(long.Parse(strArray[i]));
                            categoryNames = categoryNames + string.Format("{0} {1} ", (category == null) ? "" : category.Name, (i == (strArray.Length - 1)) ? "" : " > ");
                        }
                    }
                }
                if (categoryId == 0L)
                {
                    categoryId = model.CategoryId;
                }
                else if (categoryId != model.CategoryId)
                {
                    flag = true;
                }
                model.CategoryNames = categoryNames;
                if (flag)
                {
                    model.SKUInfo = null;
                    model.CategoryId = categoryId;
                }
            }
            else
            {
                model = new ProductInfo
                {
                    CategoryId = categoryId,
                    CategoryNames = categoryNames
                };
            }
            IEnumerable<BrandInfo> brandsByCategoryIds = ServiceHelper.Create<IBrandService>().GetBrandsByCategoryIds(this.shopId, new long[] { categoryId });
            List<SelectListItem> list4 = new List<SelectListItem>();
            SelectListItem item3 = new SelectListItem
            {
                Selected = false,
                Text = "请选择品牌...",
                Value = "0"
            };
            list4.Add(item3);
            List<SelectListItem> list = list4;
            foreach (BrandInfo info3 in brandsByCategoryIds)
            {
                SelectListItem item = new SelectListItem
                {
                    Selected = ((productId != 0L) && (model != null)) && (model.BrandId == info3.Id),
                    Text = info3.Name,
                    Value = info3.Id.ToString()
                };
                list.Add(item);
            }
            List<FreightTemplateInfo> shopFreightTemplate = ServiceHelper.Create<IFreightTemplateService>().GetShopFreightTemplate(base.CurrentSellerManager.ShopId);
            List<SelectListItem> list5 = new List<SelectListItem>();
            SelectListItem item4 = new SelectListItem
            {
                Selected = false,
                Text = "请选择运费模板...",
                Value = "0"
            };
            list5.Add(item4);
            List<SelectListItem> list3 = list5;
            foreach (FreightTemplateInfo info4 in shopFreightTemplate)
            {
                SelectListItem item2 = new SelectListItem
                {
                    Selected = ((productId != 0L) && (model != null)) && (model.FreightTemplateId == info4.Id),
                    Text = info4.Name + "【" + info4.ValuationMethod.ToDescription() + "】",
                    Value = info4.Id.ToString()
                };
                list3.Add(item2);
            }
            model.IsCategory = (productId == 0L) ? 1 : 0;
            model.ShopId = this.shopId;
            model.TopId = (0L == model.Id) ? 0L : model.ProductDescriptionInfo.DescriptionPrefixId;
            model.BottomId = (0L == model.Id) ? 0L : model.ProductDescriptionInfo.DescriptiondSuffixId;
            ((dynamic)base.ViewBag).FreightTemplates = list3;
            ((dynamic)base.ViewBag).BrandDrop = list;
            ((dynamic)base.ViewBag).SalePrice = str;
            ((dynamic)base.ViewBag).Stock = str2;
            ((dynamic)base.ViewBag).CostPrice = str3;
            return base.View(model);
        }

        public JsonResult Recommend(long productId, string productIds)
        {
            bool success = false;
            try
            {
                ProductManagerApplication.UpdateRelationProduct(productId, productIds);
                success = true;
            }
            catch
            {
            }
            return this.Json(success, null);
        }

        [NonAction]
        protected string RenderPartialViewToString(Controller controller, string partialViewName)
        {
            IView view = ViewEngines.Engines.FindPartialView(controller.ControllerContext, partialViewName).View;
            using (StringWriter writer = new StringWriter())
            {
                ViewContext viewContext = new ViewContext(controller.ControllerContext, view, controller.ViewData, controller.TempData, writer);
                viewContext.View.Render(viewContext, writer);
                return writer.ToString();
            }
        }

        [UnAuthorize, HttpPost]
        public JsonResult SetProductOverSafeStock(string ids, long stock)
        {
            if (stock == 0L)
            {
                throw new HimallException("库存不能为0");
            }
            ProductManagerApplication.SetProductOverSafeStock(from item in ids.Split(new char[] { ',' }) select long.Parse(item), stock);
            return base.Json(new { success = true });
        }

        public JsonResult Specifications(long categoryId, long productId = 0L)
        {
            Dictionary<Dictionary<SpecificationType, long>, SKUInfo> skus = (Dictionary<Dictionary<SpecificationType, long>, SKUInfo>)null;
            if (productId > 0L)
                skus = Enumerable.ToDictionary<SKUInfo, Dictionary<SpecificationType, long>, SKUInfo>((IEnumerable<SKUInfo>)this._iProductService.GetSKUs(productId), (Func<SKUInfo, Dictionary<SpecificationType, long>>)(p => SKUEx.SplitId(p.Id)), (Func<SKUInfo, SKUInfo>)(p => p));
            List<SpecificationValueInfo> specification = ObjectContainer.Current.Resolve<ISpecificationService>().GetSpecification(categoryId, this.CurrentShop.Id);
            SpecificationValueInfo info = new SpecificationValueInfo();
            if (specification != null && specification.Count > 0)
                info = specification[0];
            return this.Json((object)Enumerable.Select(Enumerable.GroupBy<SpecificationValueInfo, SpecificationType>((IEnumerable<SpecificationValueInfo>)specification, (Func<SpecificationValueInfo, SpecificationType>)(item => item.Specification)), item =>
            {
                var fAnonymousTyped0 = new
                {
                    Specification = new
                    {
                        Value = (int)item.Key,
                        Name = item.Key.ToString(),
                        Alias = this.GetSpecificationTypeAlias(item.Key, info),
                        Text = EnumHelper.ToDescription((Enum)item.Key),
                        NeedPic = item.Key == SpecificationType.Color
                    },
                    Values = Enumerable.ToArray(Enumerable.Select((IEnumerable<SpecificationValueInfo>)item, model =>
                    {
                        string str1 = model.Value;
                        if (skus != null)
                        {
                            Dictionary<SpecificationType, long> index = Enumerable.FirstOrDefault<Dictionary<SpecificationType, long>>((IEnumerable<Dictionary<SpecificationType, long>>)skus.Keys, (Func<Dictionary<SpecificationType, long>, bool>)(dic => dic.ContainsKey(item.Key) && dic[item.Key] == model.Id));
                            if (index != null)
                            {
                                string str2;
                                switch (item.Key)
                                {
                                    case SpecificationType.Color:
                                        str2 = skus[index].Color;
                                        break;
                                    case SpecificationType.Size:
                                        str2 = skus[index].Size;
                                        break;
                                    default:
                                        str2 = skus[index].Version;
                                        break;
                                }
                                if (!string.IsNullOrWhiteSpace(str2))
                                    str1 = str2;
                            }
                        }
                        var fAnonymousTypecf = new
                        {
                            Id = model.Id,
                            Value = str1
                        };
                        return fAnonymousTypecf;
                    }))
                };
                return fAnonymousTyped0;
            }), JsonRequestBehavior.AllowGet);
        }

        private JsonResult ValidError()
        {
            var typedArray = (from p in base.ModelState
                              where p.Value.Errors.Count > 0
                              select new { Key = p.Key, ErrorMessage = p.Value.Errors[0].ErrorMessage }).ToArray();
            return base.Json(new { success = false, errors = typedArray, message = "验证失败" });
        }

    }
}