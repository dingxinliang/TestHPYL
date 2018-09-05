using Himall.IServices.QueryModel;
using Himall.Model;
using System.Collections.Generic;
using System.Linq;
using System;
using Himall.CommonModel;

namespace Himall.IServices
{
	/// <summary>
	/// 诊疗项目服务接口
	/// </summary>
	public interface IProductService : IService
	{
		#region 平台服务

		/// <summary>
		/// 查询诊疗项目
		/// </summary>
		/// <returns></returns>
		ObsoletePageModel<ProductInfo> GetProducts(ProductQuery productQueryModel);

		/// <summary>
		/// 审核诊疗项目
		/// </summary>
		/// <param name="auditStatus">审核状态</param>
		/// <param name="productId">诊疗项目编号</param>
		/// <param name="failedMessage">说明</param>
		void AuditProduct(long productId, ProductInfo.ProductAuditStatus auditStatus, string message);


		/// <summary>
		/// 批量审核诊疗项目
		/// </summary>
		/// <param name="auditStatus">审核状态</param>
		/// <param name="productIds">诊疗项目编号</param>
		/// <param name="failedMessage">说明</param>
		void AuditProducts(IEnumerable<long> productIds, ProductInfo.ProductAuditStatus auditStatus, string message);
		#endregion

		#region 诊所服务

		long GetNextProductId();

		/// <summary>
		/// 加入产品SKU
		/// </summary>
		/// <param name="pInfo"></param>
		void AddSKU(ProductInfo pInfo);

		void UpdateProductImagePath(long pId, string path);

		IQueryable<ProductShopCategoryInfo> GetProductShopCategories(long productId);

		/// <summary>
		/// 根据诊疗项目Id获取属性
		/// </summary>
		/// <param name="productId">诊疗项目Id</param>
		/// <returns></returns>
		IQueryable<ProductAttributeInfo> GetProductAttribute(long productId);

		/// <summary>
		/// 根据诊疗项目Id获取规格
		/// </summary>
		/// <param name="productId">诊疗项目Id</param>
		/// <returns></returns>
		IQueryable<SellerSpecificationValueInfo> GetSellerSpecifications(long shopId, long typeId);

		/// <summary>
		/// 添加诊疗项目
		/// </summary>
		/// <param name="model"></param>
		[System.Obsolete]
		void AddProduct(ProductInfo model);

		/// <summary>
		/// 添加诊疗项目
		/// </summary>
		/// <param name="shopId">店铺id</param>
		/// <param name="product">诊疗项目信息</param>
		/// <param name="pics">需要转移的诊疗项目图片地址</param>
		/// <param name="skus">skus，至少要有一项</param>
		/// <param name="description">描述</param>
		/// <param name="attributes">诊疗项目属性</param>
		/// <param name="goodsCategory">诊所分类</param>
		/// <param name="sellerSpecifications">诊所自定义规格</param>
		void AddProduct(long shopId, ProductInfo product, string[] pics, SKUInfo[] skus, ProductDescriptionInfo description, ProductAttributeInfo[] attributes, long[] goodsCategory, SellerSpecificationValueInfo[] sellerSpecifications);

		/// <summary>
		/// 更新诊疗项目
		/// </summary>
		/// <param name="model"></param>
        void UpdateProduct(ProductInfo model);
        object GetProductCirclePage(long productId, long? circleId, bool? nextPage);
        List<ProductCircleInfo> GetProductCircleList(long productId);
        ProductCircleInfo GetProductCircleInfo(long circleId);
        void SubmitProductCircle(ProductCircleInfo info);

		/// <summary>
		/// 更新诊疗项目
		/// </summary>
		/// <param name="product">修改后的诊疗项目</param>
		/// <param name="pics">诊疗项目图片地址</param>
		/// <param name="skus">skus，至少要有一项</param>
		/// <param name="description">描述</param>
		/// <param name="attributes">诊疗项目属性</param>
		/// <param name="goodsCategory">诊所分类</param>
		/// <param name="sellerSpecifications">诊所自定义规格</param>
		void UpdateProduct(ProductInfo product, string[] pics, SKUInfo[] skus, ProductDescriptionInfo description, ProductAttributeInfo[] attributes, long[] goodsCategory, SellerSpecificationValueInfo[] sellerSpecifications);

		/// <summary>
		/// 删除诊疗项目
		/// </summary>
		/// <param name="ids">诊疗项目id</param>
		/// <param name="shopId">商铺id</param>
		void DeleteProduct(IEnumerable<long> ids, long shopId);

        void DeleteProductCircle(long id);

		/// <summary>
		/// 获取一个诊疗项目
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		ProductInfo GetProduct(long id);

        ProductInfo GetProductCache(long id);

        /// <summary>
        /// 获取诊疗项目详情页需要及时刷新的信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ProductInfo GetNeedRefreshProductInfo(long id);
        /// <summary>
        /// 获取诊疗项目描述
        /// </summary>
        /// <param name="productId">诊疗项目编号</param>
        /// <returns></returns>
        ProductDescriptionInfo GetProductDescription(long productId);

		/// <summary>
		/// 获取诊疗项目描述
		/// </summary>
		/// <param name="productIds">诊疗项目编号</param>
		/// <returns></returns>
		List<ProductDescriptionInfo> GetProductDescriptions(IEnumerable<long> productIds);

		/// <summary>
		/// 获取诊疗项目的评论数
		/// </summary>
		/// <param name="productId"></param>
		/// <returns></returns>
		int GetProductCommentCount(long productId);

		/// <summary>
		/// 获取指定的属性
		/// </summary>
		/// <param name="attrId"></param>
		/// <returns></returns>
		AttributeInfo GetAttributeInfo(long attrId);

		/// <summary>
		/// 获取某件诊疗项目的所有sku
		/// </summary>
		/// <param name="productId"></param>
		/// <returns></returns>
		IQueryable<SKUInfo> GetSKUs(long productId);
        List<SKUInfo> GetSkuList(long productId);
        /// <summary>
        /// 是否有规格
        /// </summary>
        /// <param name="id">产品编号</param>
        /// <returns></returns>
        bool HasSKU(long id);


		/// <summary>
		/// 获取某些诊疗项目的所有sku 
		/// </summary>
		/// <param name="productId"></param>
		/// <returns></returns>
		///  /// Add:zesion[150906]
		IQueryable<SKUInfo> GetSKUs(IEnumerable<long> productIds);

		/// <summary>
		/// 根据sku id 获取sku信息
		/// </summary>
		/// <param name="skuIds"></param>
		/// <returns></returns>
		List<SKUInfo> GetSKUs(IEnumerable<string> skuIds);

		/// <summary>
		/// 获取一个sku
		/// </summary>
		/// <param name="skuId"></param>
		/// <returns></returns>
		SKUInfo GetSku(string skuId);

		string GetSkuString(string skuId);

		/// <summary>
		/// 获取某店铺下所有诊疗项目数量
		/// </summary>
		/// <param name="shopId">店铺id</param>
		/// <returns></returns>
		int GetShopAllProducts(long shopId);

		/// <summary>
		/// 获取店铺销售中的所有诊疗项目
		/// </summary>
		/// <param name="shopId"></param>
		/// <returns></returns>
		int GetShopOnsaleProducts(long shopId);

		void SaveSellerSpecifications(List<SellerSpecificationValueInfo> info);

		/// <summary>
		/// 下架诊疗项目
		/// </summary>
		/// <param name="id">待下架的诊疗项目id</param>
		/// <param name="shopId">店铺Id</param>
		void SaleOff(long id, long shopId);

		/// <summary>
		/// 下架诊疗项目
		/// </summary>
		/// <param name="ids">待下架的诊疗项目id</param>
		/// <param name="shopId">店铺Id</param>
		void SaleOff(IEnumerable<long> ids, long shopId);
        /// <summary>
        /// 设置诊疗项目的安全库存
        /// </summary>
        /// <param name="pids"></param>
        /// <param name="stock"></param>
        void SetProductOverSafeStock(IEnumerable<long> pids, long stock);
		/// <summary>
		/// 上架诊疗项目
		/// </summary>
		/// <param name="id">诊疗项目id</param>
		/// <param name="shopId">店铺id</param>
		void OnSale(long id, long shopId);

		/// <summary>
		/// 上架诊疗项目
		/// </summary>
		/// <param name="ids">诊疗项目id</param>
		/// <param name="shopId">店铺id</param>
		void OnSale(IEnumerable<long> ids, long shopId);

        /// <summary>
        /// 检查诊疗项目是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool CheckProductIsExist(long id);

		/// <summary>
		/// 批量获取诊疗项目信息
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		IQueryable<ProductInfo> GetProductByIds(IEnumerable<long> ids);
        List<ProductInfo> GetAllProductByIds(IEnumerable<long> ids);
        /// <summary>
        /// 根据ID，取诊疗项目信息（所有状态）
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        IEnumerable<ProductInfo> GetAllStatusProductByIds(IEnumerable<long> ids);
		/// <summary>
		/// 绑定诊疗项目描述版式
		/// </summary>
		/// <param name="topTemplateId">顶部版式id</param>
		/// <param name="bottomTemplateId">底部版式id</param>
		/// <param name="productIds">诊疗项目id</param>
		void BindTemplate(long? topTemplateId, long? bottomTemplateId, IEnumerable<long> productIds);

		#endregion

		#region  前台页面(搜索)

		ObsoletePageModel<ProductInfo> SearchProduct(ProductSearch search);

		ObsoletePageModel<ProductInfo, SearchProductModel> SearchProductAndOtherModel(ProductSearch search);
		#endregion

		#region 获取店铺热销的前N件诊疗项目
		IQueryable<ProductInfo> GetHotSaleProduct(long shopId, int count = 5);
		#endregion

		#region 获取店铺最新上架的前N件诊疗项目
		IQueryable<ProductInfo> GetNewSaleProduct(long shopId, int count = 5);
		#endregion

		#region 获取店铺最受关注的前N件诊疗项目
		IQueryable<ProductInfo> GetHotConcernedProduct(long shopId, int count = 5);
		#endregion

		#region 获取用户关注的诊疗项目
		ObsoletePageModel<FavoriteInfo> GetUserConcernProducts(long userId, int pageNo, int pageSize);

		/// <summary>
		/// 添加诊疗项目关注
		/// </summary>
		/// <param name="productId"></param>
		void AddFavorite(long productId, long userId, out int status);

		/// <summary>
		/// 判断是否关注过此诊疗项目
		/// </summary>
		/// <param name="productId">诊疗项目ID</param>
		/// <param name="userId">用户ID</param>
		/// <returns></returns>
		bool IsFavorite(long productId, long userId);
		#endregion

		#region 取消用户关注的诊疗项目
		void CancelConcernProducts(IEnumerable<long> ids, long userId);

		void DeleteFavorite(long productId, long userId);
		#endregion

		#region 累加诊疗项目浏览次数

		/// <summary>
		/// 累计诊疗项目的浏览次数
		/// </summary>
		/// <param name="productId"></param>
		void LogProductVisti(long productId);

		#endregion

		#region 更新诊疗项目库存

		/// <summary>
		/// 更新诊疗项目库存
		/// </summary>
		/// <param name="skuId">诊疗项目SKUId</param>
		/// <param name="stockChange">库存变化量（正数表示增加库存，负数表示减少库存）</param>
		void UpdateStock(string skuId, long stockChange);
        bool UpdateStockBySkuId(string skuId, long stockChange);
        bool UpdateStockByProId(long proId, int stockChange);
        void SetSkusStock(IEnumerable<string> skuIds, IEnumerable<int> stock);
        void SetMoreProductToOneStock(IEnumerable<long> pids, int stock);
        void AddSkuStock(IEnumerable<string> skuIds, IEnumerable<int> stock);
        void AddProductStock(IEnumerable<long> pids, int stock);

        void ReduceSkuStock(IEnumerable<string> skuIds, IEnumerable<int> stock);

        void ReduceProductStock(IEnumerable<long> pids, int stock);

		#endregion

		#region 添加诊疗项目浏览记录
		void AddBrowsingProduct(BrowsingHistoryInfo info);
		#endregion

		#region 获取用户浏览记录
		IQueryable<Himall.Model.BrowsingHistoryInfo> GetBrowsingProducts(long userId);
		#endregion

		#region 获取平台最热销的前N件诊疗项目
		IQueryable<ProductInfo> GetPlatHotSaleProduct(int count = 3);
		#endregion

		#region 获取诊疗项目的销售情况

		ProductVistiInfo GetProductVistInfo(long pId, ICollection<ProductVistiInfo> pInfo = null);

		#endregion

		#region 更新诊疗项目销售数量
		/// <summary>
		/// 更新诊疗项目销售数量
		/// </summary>
		/// <param name="skuId">skuid</param>
		/// <param name="addSalesCount">添加的销售数量</param>
		void UpdateSalesCount(string skuId, int addSalesCount);
		#endregion

		#region 获取运费

		/// <summary>
		/// 根据产品获取运费
		/// </summary>
		/// <param name="productIds">产品Id集合</param>
		/// <param name="counts">购买总数（数量/重量/体积）集合</param>
		/// <param name="cityId">收货地址城市id</param>
		/// <returns></returns>
		decimal GetFreight(IEnumerable<long> productIds, IEnumerable<int> counts, int cityId);

		List<decimal> GetFreights(IEnumerable<string> skuIds, IEnumerable<int> counts, int cityId);

		#endregion

		#region 获取最近一次交易的卖家的推荐诊疗项目
		IQueryable<ProductInfo> GetPlatHotSaleProductByNearShop(int count, long userId, bool isRecommend = false);
		#endregion

		List<FavoriteInfo> GetUserAllConcern(long userId);

		/// <summary>
		/// 取数据修改状态
		/// </summary>
		/// <param name="id">用于比对的诊疗项目编号</param>
		/// <param name="model">当前诊疗项目数据</param>
		/// <returns></returns>
		/// Add:DZY[150714]
		ProductInfo.ProductEditStatus GetEditStatus(long id, ProductInfo model);
		/// <summary>
		/// 申请诊疗项目上架
		/// </summary>
		/// <param name="id">诊疗项目编号</param>
		/// <returns></returns>
		/// Add:DZY[150715]
		bool ApplyForSale(long id);
		void ApplyForSale(ProductInfo product);
		/// <summary>
		/// 是否为限时购诊疗项目
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		bool IsLimitBuy(long id);

		/// <summary>
		/// 修改推荐诊疗项目
		/// </summary>
		/// <param name="productId"></param>
		/// <param name="relationProductIds"></param>
		void UpdateRelationProduct(long productId, string relationProductIds);
		
		/// <summary>
		/// 获取诊疗项目的推荐诊疗项目
		/// </summary>
		/// <param name="productId"></param>
		/// <returns></returns>
		ProductRelationProductInfo GetRelationProductByProductId(long productId);

		/// <summary>
		/// 获取诊疗项目的推荐诊疗项目
		/// </summary>
		/// <param name="productId"></param>
		/// <returns></returns>
		List<ProductRelationProductInfo> GetRelationProductByProductIds(IEnumerable<long> productIds);

		/// <summary>
		/// 获取指定类型下面热销的前N件诊疗项目
		/// </summary>
		/// <param name="categoryId"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		List<ProductInfo> GetHotSaleProductByCategoryId(int categoryId, int count);
        /// <summary>
        /// 门店首页诊疗项目列表
        /// </summary>
        /// <param name="productQueryModel"></param>
        /// <returns></returns>
        QueryPageModel<ProductInfo> GetStoreHomeProducts(ProductQuery productQueryModel);
    }
}
