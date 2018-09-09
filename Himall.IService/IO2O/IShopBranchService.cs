﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;
using Himall.CommonModel;
using Himall.IServices.QueryModel;

namespace Himall.IServices
{

    public interface IShopBranchService : IService
    {
        /// <summary>
        /// 添加分店
        /// </summary>
        /// <param name="shopBranchInfo"></param>
        void AddShopBranch(ShopBranchInfo shopBranchInfo);

        /// <summary>
        /// 添加分店管理员
        /// </summary>
        /// <param name="shopBranchManagersInfo"></param>
        void AddShopBranchManagers(ShopBranchManagersInfo shopBranchManagersInfo);
        /// <summary>
        /// 更新门店信息
        /// </summary>
        /// <param name="shopBranch"></param>
        void UpdateShopBranch(ShopBranchInfo shopBranch);
        /// <summary>
        /// 更新门店管理员密码
        /// </summary>
        /// <param name="managerId"></param>
        /// <param name="pwd"></param>
        /// <param name="pwdSalt"></param>
        void UpdateShopBranchManagerPwd(long branchId, string userName, string pwd, string pwdSalt);

        /// <summary>
        /// 更新指定门店管理员的密码
        /// </summary>
        /// <param name="managerId"></param>
        /// <param name="password"></param>
        void UpdateShopBranchManagerPwd(long managerId, string password);

        /// <summary>
        /// 删除门店
        /// </summary>
        /// <param name="id"></param>
        void DeleteShopBranch(long id);
        /// <summary>
        /// 判断门店名称是否重复
        /// </summary>
        /// <param name="shopId">诊所诊所ID</param>
        /// <param name="shopBranchName">门店名字</param>
		/// <returns></returns>
		bool Exists(long shopId, long shopBranchId, string shopBranchName);

        /// <summary>
        /// 根据查询条件判断是否有门店
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        bool Exists(CommonModel.ShopBranchQuery query);
        /// <summary>
        /// 根据门店ID获取门店
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ShopBranchInfo GetShopBranchById(long id);

        /// <summary>
        /// 根据门店IDs获取门店
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        List<ShopBranchInfo> GetShopBranchByIds(IEnumerable<long> Ids);

        /// <summary>
        /// 根据门店联系方式获取门店信息
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        ShopBranchInfo GetShopBranchByContact(string contact);

        /// <summary>
        /// 根据门店ID取门店管理员
        /// </summary>
        /// <param name="branchId"></param>
        /// <returns></returns>
        IEnumerable<ShopBranchManagersInfo> GetShopBranchManagers(long branchId);
        /// <summary>
        /// 根据ID取门店管理员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ShopBranchManagersInfo GetShopBranchManagersById(long id);
        /// <summary>
        /// 根据用户名、密码、门店ID,取管理员信息
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        ShopBranchManagersInfo GetShopBranchManagersByName(string userName);
        /// <summary>
        /// 分页查询门店信息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<ShopBranchInfo> GetShopBranchs(ShopBranchQuery query);
        /// <summary>
        /// 取诊所所有门店
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        IEnumerable<ShopBranchInfo> GetShopBranchByShopId(long shopId);
        /// <summary>
        /// 根据分店id获取分店信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        List<ShopBranchInfo> GetShopBranchs(IEnumerable<long> ids);
        /// <summary>
        /// 冻结门店
        /// </summary>
        /// <param name="shopBranchId"></param>
        void FreezeShopBranch(long shopBranchId);
        /// <summary>
        /// 解冻门店
        /// </summary>
        /// <param name="shopBranchId"></param>
        void UnFreezeShopBranch(long shopBranchId);

        /// <summary>
        /// 获取分店经营的诊疗项目SKU
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="shopBranchIds"></param>
        /// <param name="status">null表示所有</param>
        /// <returns></returns>
        List<ShopBranchSkusInfo> GetSkus(long shopId, IEnumerable<long> shopBranchIds, ShopBranchSkuStatus? status = ShopBranchSkuStatus.Normal);
        /// <summary>
        /// 根据skuid取门店SKU
        /// </summary>
        /// <param name="skuIds"></param>
        /// <returns></returns>
        List<ShopBranchSkusInfo> GetSkusByIds(long shopBranchId, IEnumerable<string> skuIds);
        /// <summary>
        /// 添加门店sku
        /// </summary>
        /// <param name="infos"></param>
        void AddSkus(IEnumerable<ShopBranchSkusInfo> infos);
        /// <summary>
        /// 设置门店sku库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="skuIds"></param>
        void SetStock(long shopBranchId, IEnumerable<string> skuIds, IEnumerable<int> stock);
        /// <summary>
        /// 设置门店诊疗项目库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="pids"></param>
        /// <param name="stock"></param>
        void SetProductStock(long shopBranchId, IEnumerable<long> pids, int stock);
        /// <summary>
        /// 增加库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="skuIds"></param>
        /// <param name="stock"></param>
        void AddStock(long shopBranchId, IEnumerable<string> skuIds, IEnumerable<int> stock);
        /// <summary>
        /// 增加门店诊疗项目库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="pids"></param>
        /// <param name="stock"></param>
        void AddProductStock(long shopBranchId, IEnumerable<long> pids, int stock);
        /// <summary>
        /// 减少库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="skuIds"></param>
        /// <param name="stock"></param>
        void ReduceStock(long shopBranchId, IEnumerable<string> skuIds, IEnumerable<int> stock);
        /// <summary>
        /// 减少门店诊疗项目库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="pids"></param>
        /// <param name="stock"></param>
        void ReduceProductStock(long shopBranchId, IEnumerable<long> pids, int stock);
        /// <summary>
        /// 设置门店SKU状态
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="pIds"></param>
        /// <param name="status"></param>
        void SetBranchProductStatus(long shopBranchId, IEnumerable<long> pIds, ShopBranchSkuStatus status);
        /// <summary>
        /// 设置门店SKU状态
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="status"></param>
        void SetBranchProductStatus(long productId, ShopBranchSkuStatus status);
        /// <summary>
        /// 搜索门店诊疗项目
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        QueryPageModel<ProductInfo> SearchProduct(ShopBranchProductQuery search);
        /// <summary>
        /// 检查产品是否已经存在
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        bool CheckProductIsExist(long shopBranchId, long productId);
        /// <summary>
        /// 查询门店的sku
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        IEnumerable<ShopBranchSkusInfo> SearchShopBranchSkus(long shopBranchId, DateTime? startDate, DateTime? endDate);
        /// <summary>
        /// 获取周边门店-分页
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<ShopBranchInfo> GetNearShopBranchs(CommonModel.ShopBranchQuery query);
        QueryPageModel<ShopBranchInfo> SearchNearShopBranchs(ShopBranchQuery search);
        QueryPageModel<ShopBranchInfo> TagsSearchNearShopBranchs(ShopBranchQuery search);
        QueryPageModel<ShopBranchInfo> StoreByProductNearShopBranchs(ShopBranchQuery search);
        QueryPageModel<ShopBranchInfo> GetShopBranchsAll(CommonModel.ShopBranchQuery query);
        double GetLatLngDistancesFromAPI(string fromLatLng, string latlng);
        List<ShopBranchInfo> GetShopBranchsNoPager(ShopBranchQuery query);

        /// <summary>
        /// 获取门店配送范围在同一区域的门店
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        QueryPageModel<ShopBranchInfo> GetArealShopBranchsAll(int areaId, int shopId, float latitude, float longitude);
        /// <summary>
        /// 自动分配预约单到门店
        /// </summary>
        /// <param name="query"></param>
        /// <param name="skuIds"></param>
        /// <param name="counts"></param>
        /// <returns></returns>
        ShopBranchInfo GetAutoMatchShopBranch(ShopBranchQuery query, string[] skuIds, int[] counts);
        /// <summary>
        /// 获取代理诊疗项目的门店编号集
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        List<long> GetAgentShopBranchIds(long productId);

        bool RecommendShopBranch(long[] ids);

        bool RecommendChangeSequence(long oriShopBranchId, long newShopBranchId);

        bool ResetShopBranchRecommend(long shopBranchId);

        List<ShopBranchTagInfo> GetAllShopBranchTagInfo();

        void AddShopBranchTagInfo(ShopBranchTagInfo shopBranchTagInfo);

        bool UpdateShopBranchTagInfo(ShopBranchTagInfo shopBranchTagInfo);

        bool DeleteShopBranchTagInfo(long shopBranchTagInfoId);

        List<ShopBranchTagInfo> GetShopBranchTagsTitle(long shopBranchId);

        void AddShopBranchInTagInfo(long[] shopBranchIds, long[] shopBranchTagIds);

        void DeleteShopBranchInTagInfo(long[] shopBranchIds);
    }
}
