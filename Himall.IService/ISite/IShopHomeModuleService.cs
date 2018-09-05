﻿using Himall.Model;
using System.Collections.Generic;
using System.Linq;
using Himall.Model.DTO;

namespace Himall.IServices
{
    /// <summary>
    /// 店铺首页诊疗项目模块服务
    /// </summary>
    public interface IShopHomeModuleService : IService
    {
        #region 老版本
        /// <summary>
        /// 添加诊疗项目模块信息
        /// </summary>
        /// <param name="shopProductModuleInfo">诊疗项目模块</param>
        void AddShopProductModule(ShopHomeModuleInfo shopProductModuleInfo);

        /// <summary>
        /// 更新诊疗项目模块名称
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="id">诊疗项目模块id</param>
        /// <param name="name">待更新的诊疗项目模块名称</param>
        void UpdateShopProductModuleName(long shopId, long id, string name);

        /// <summary>
        /// 更新诊疗项目模块所包含的诊疗项目模块
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="id">待更新的诊疗项目模块id</param>
        /// <param name="productIds">诊疗项目id</param>
        void UpdateShopProductModuleProducts(long shopId,long id, IEnumerable<long> productIds);

        /// <summary>
        /// 删除诊疗项目模块
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="id">待删除的诊疗项目模块id</param>
        void Delete(long shopId, long id);


        /// <summary>
        /// 获取指定店铺所有诊疗项目模块
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <returns></returns>
        IQueryable<ShopHomeModuleInfo> GetAllShopHomeModuleInfos(long shopId);


        /// <summary>
        /// 获取指定诊疗项目模块
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="id">诊疗项目模块id</param>
        /// <returns></returns>
        ShopHomeModuleInfo GetShopHomeModuleInfo(long shopId, long id);
        #endregion

        string GetFooter( long shopid );

        void EditFooter( long shopid , string footer );

        void SaveFloor( AddShopHomeModuleModel model );

        void Enable( long id , bool enable );

        void UpdateFloorSequence( long shopId , int oriRowNumber , int newRowNumber , string direction );

        void DelFloor( long id );
    }
}
