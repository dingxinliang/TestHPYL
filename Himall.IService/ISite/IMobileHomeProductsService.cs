using Himall.Core;
using Himall.IServices.QueryModel;
using Himall.Model;
using System.Collections.Generic;
using System.Linq;

namespace Himall.IServices
{
    public interface IMobileHomeProductsService : IService
    {

        /// <summary>
        /// 获取指定店铺移动端首页诊疗项目
        /// </summary>
        /// <param name="shopId">待获取移动端首页诊疗项目的店铺Id（平台为0）</param>
        /// <param name="platformType">平台类型</param>
        /// <returns></returns>
        ObsoletePageModel<MobileHomeProductsInfo> GetMobileHomePageProducts(long shopId, PlatformType platformType, ProductQuery productQuery);


        /// <summary>
        /// 获取指定店铺移动端首页诊疗项目
        /// </summary>
        /// <param name="shopId">待获取移动端首页诊疗项目的店铺Id（平台为0）</param>
        /// <param name="platformType">平台类型</param>
        /// <returns></returns>
        IQueryable<MobileHomeProductsInfo> GetMobileHomePageProducts(long shopId, PlatformType platformType);

        ObsoletePageModel<MobileHomeProductsInfo> GetSellerMobileHomePageProducts(long shopId, PlatformType platformType, ProductQuery productQuery);

        /// <summary>
        /// 添加诊疗项目至首页
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <param name="productIds">待添加的首页诊疗项目Id</param>
        /// <param name="platformType">平台类型</param>
        void AddProductsToHomePage(long shopId, PlatformType platformType, IEnumerable<long> productIds);

        /// <summary>
        /// 更新顺序
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <param name="id"></param>
        /// <param name="sequenc">顺序号</param>
        void UpdateSequence(long shopId, long id, short sequenc);

        /// <summary>
        /// 从首页删除诊疗项目
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shopId">店铺Id</param>
        void Delete(long shopId,long id);
        /// <summary>
        /// 多个删除
        /// </summary>
        /// <param name="id"></param>
        void DeleteList(long[] id);
    }
}
