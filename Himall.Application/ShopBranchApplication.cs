using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.DTO;
using Himall.IServices;
using Himall.Core;
using Himall.Application.Mappers;
using Himall.Model;
using Himall.Core.Helper;
using Himall.CommonModel;
using Himall.DTO.Product;
using Himall.IServices.QueryModel;
using System.Web;
using AutoMapper;

namespace Himall.Application
{
    /// <summary>
    /// 门店应用服务
    /// </summary>
    public class ShopBranchApplication
    {
        private static IShopBranchService _shopBranchService = ObjectContainer.Current.Resolve<IShopBranchService>();
        private static IAppMessageService _appMessageService = ObjectContainer.Current.Resolve<IAppMessageService>();
        private static ICouponService _iCouponService = ObjectContainer.Current.Resolve<ICouponService>();

        #region 密码加密处理
        /// <summary>
        /// 二次加盐后的密码
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string GetPasswrodEncode(string password, string salt)
        {
            string encryptedPassword = SecureHelper.MD5(password);//一次MD5加密
            string encryptedWithSaltPassword = SecureHelper.MD5(encryptedPassword + salt);//一次结果加盐后二次加密
            return encryptedWithSaltPassword;
        }
        /// <summary>
        /// 取密码盐
        /// </summary>
        /// <returns></returns>
        public static string GetSalt()
        {
            return Guid.NewGuid().ToString("N").Substring(12);
        }
        #endregion 密码加密处理

        #region 查询相关
        /// <summary>
        /// 获取门店
        /// </summary>
        /// <returns></returns>
        public static ShopBranch GetShopBranchById(long id)
        {
            var branchInfo = _shopBranchService.GetShopBranchById(id);
            if (branchInfo == null)
                return null;
            var branchManagers = _shopBranchService.GetShopBranchManagers(id);
            var shopBranch = AutoMapper.Mapper.Map<ShopBranchInfo, ShopBranch>(branchInfo);
            //补充地址中文名称
            //shopBranch.AddressFullName = RegionApplication.GetFullName(shopBranch.AddressId, CommonConst.ADDRESS_PATH_SPLIT);
            shopBranch.AddressFullName = RenderAddress(shopBranch.AddressPath,shopBranch.AddressDetail,0);
            if (branchManagers != null && branchManagers.Count() > 0)
            {//补充管理员名称
                shopBranch.UserName = branchManagers.FirstOrDefault().UserName;
            }
            return shopBranch;
        }


        /// <summary>
        /// 根据 IDs批量获取门店信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<ShopBranch> GetShopBranchByIds(IEnumerable<long> ids)
        {
            var branchInfos = _shopBranchService.GetShopBranchByIds(ids);
            var shopBranchs = AutoMapper.Mapper.Map<List<ShopBranch>>(branchInfos);
            return shopBranchs;
        }

        /// <summary>
        /// 根据门店联系方式获取门店信息
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public static ShopBranch GetShopBranchByContact(string contact)
        {
            var branchInfo = _shopBranchService.GetShopBranchByContact(contact);
            return branchInfo.Map<DTO.ShopBranch>();
        }

        /// <summary>
        /// 分页查询门店
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopBranch> GetShopBranchs(ShopBranchQuery query)
        {
            var shopBranchInfos = _shopBranchService.GetShopBranchs(query);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    AddressDetail = e.AddressDetail,
                    AddressFullName = RegionApplication.GetFullName(e.AddressId, CommonConst.ADDRESS_PATH_SPLIT) + CommonConst.ADDRESS_PATH_SPLIT + e.AddressDetail,
                    AddressId = e.AddressId,
                    ContactPhone = e.ContactPhone,
                    ContactUser = e.ContactUser,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    ShopId = e.ShopId,
                    Status = e.Status
                }).ToList(),
                Total = shopBranchInfos.Total
            };
            return shopBranchs;
        }
        public static List<ShopBranch> GetShopBranchByShopId(long shopId)
        {
            var shopBranch = _shopBranchService.GetShopBranchByShopId(shopId).ToList();
            return shopBranch.Map<List<ShopBranch>>();
        }
        /// <summary>
        /// 根据分店id获取分店信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<ShopBranch> GetShopBranchs(IEnumerable<long> ids)
        {
            var shopBranchs = _shopBranchService.GetShopBranchs(ids).Map<List<ShopBranch>>();
            //补充地址详细信息,地址库采用了缓存，循环取
            foreach (var b in shopBranchs)
            {
                b.AddressFullName = RegionApplication.GetFullName(b.AddressId);
                b.RegionIdPath = RegionApplication.GetRegionPath(b.AddressId);
            }
            return shopBranchs;
        }
        /// <summary>
        /// 获取分店经营的商品SKU
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="shopBranchIds"></param>
        /// <returns></returns>
        public static List<ShopBranchSkusInfo> GetSkus(long shopId, IEnumerable<long> shopBranchIds)
        {
            var list = _shopBranchService.GetSkus(shopId, shopBranchIds);
            return list.Map<List<ShopBranchSkusInfo>>();
        }
        /// <summary>
        /// 根据SKU AUTOID取门店SKU
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="skuids"></param>
        /// <returns></returns>
        public static List<ShopBranchSkusInfo> GetSkusByIds(long shopBranchId, IEnumerable<string> skuids)
        {
            var list = _shopBranchService.GetSkusByIds(shopBranchId, skuids);
            return list;
        }
        /// <summary>
        /// 根据商品ID取门店sku信息
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static List<DTO.SKU> GetSkusByProductId(long shopBranchId, long pid)
        {
            var sku = ProductManagerApplication.GetSKU(pid);
            var shopBranchSkus = _shopBranchService.GetSkusByIds(shopBranchId, sku.Select(e => e.Id));
            foreach (var item in sku)
            {
                var branchSku = shopBranchSkus.FirstOrDefault(e => e.SkuId == item.Id);
                if (branchSku != null)
                    item.Stock = branchSku.Stock;
            }
            return sku;
        }
        /// <summary>
        /// 根据ID取门店管理员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ShopBranchManager GetShopBranchManager(long id)
        {
            var managerInfo = _shopBranchService.GetShopBranchManagersById(id);
            AutoMapper.Mapper.CreateMap<ShopBranchManagersInfo, ShopBranchManager>();
            var manager = AutoMapper.Mapper.Map<ShopBranchManagersInfo, ShopBranchManager>(managerInfo);
            //管理员类型为门店管理员
            manager.UserType = ManagerType.ShopBranchManager;
            return manager;
        }

        /// <summary>
        /// 根据门店id获取门店管理员
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public static List<DTO.ShopBranchManager> GetShopBranchManagerByShopBranchId(long shopBranchId)
        {
            return _shopBranchService.GetShopBranchManagers(shopBranchId).Map<List<DTO.ShopBranchManager>>();
        }

        /// <summary>
        /// 门店商品查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ProductInfo> GetShopBranchProducts(ShopBranchProductQuery query)
        {
            var pageModel = _shopBranchService.SearchProduct(query);
            //TODO:补充门店销售数量
            var orders = OrderApplication.GetOrdersNoPage(new OrderQuery { ShopBranchId = query.shopBranchId });
            var orderItems = OrderApplication.GetOrderItemsByOrderId(orders.Select(e => e.Id));
            var pids = pageModel.Models.Select(e => e.Id);
            var productSaleCounts = orderItems.Where(e => pids.Contains(e.ProductId)).GroupBy(o => o.ProductId).Select(e => new { productId = e.Key, saleCount = e.Sum(p => p.Quantity) });
            foreach (var p in pageModel.Models)
            {
                var productCount = productSaleCounts.FirstOrDefault(e => e.productId == p.Id);
                if (productCount != null)
                    p.SaleCounts = productCount.saleCount;
                else
                    p.SaleCounts = 0;//门店商品无销量则为0，不应用默认的商家商品销量

            }
            return pageModel;
        }
        /// <summary>
        /// 获取门店的月产品数量
        /// </summary>
        /// <param name="query"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static QueryPageModel<ProductInfo> GetShopBranchProductsMonth(ShopBranchProductQuery query, DateTime startDate, DateTime endDate)
        {
            QueryPageModel<ProductInfo> model = _shopBranchService.SearchProduct(query);
            OrderQuery orderQuery = new OrderQuery
            {
                ShopBranchId = query.shopBranchId,
                StartDate = new DateTime?(startDate),
                EndDate = new DateTime?(endDate.AddDays(-1.0))
            };
            List<OrderItem> orderItemsByOrderId = OrderApplication.GetOrderItemsByOrderId((IEnumerable<long>)(from e in OrderApplication.GetOrdersNoPage(orderQuery) select e.Id));
            IEnumerable<long> pids = from e in model.Models select e.Id;
            var source = from o in orderItemsByOrderId
                         where pids.Contains<long>(o.ProductId)
                         group o by o.ProductId into e
                         select new { productId = e.Key, saleCount = e.Sum<OrderItem>((Func<OrderItem, long>)(p => p.Quantity)) };
            using (List<ProductInfo>.Enumerator enumerator = model.Models.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ProductInfo p = enumerator.Current;
                    var fAnonymousType0 = Enumerable.FirstOrDefault(source, e => e.productId == p.Id);
                    p.SaleCounts = fAnonymousType0 == null ? 0 : fAnonymousType0.saleCount;
                }
            }
            return model;
        }
        /// <summary>
        /// 产品销量
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="productId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static long GetProductSaleCount(long shopBranchId, long productId, DateTime startDate, DateTime endDate)
        {
            IEnumerable<OrderItem> source = Enumerable.Where<OrderItem>((IEnumerable<OrderItem>)OrderApplication.GetOrderItemsByOrderId(Enumerable.Select<Order, long>((IEnumerable<Order>)OrderApplication.GetOrdersNoPage(new OrderQuery()
            {
                ShopBranchId = new long?(shopBranchId),
                StartDate = new DateTime?(startDate),
                EndDate = new DateTime?(endDate.AddDays(-1.0))
            }), (Func<Order, long>)(e => e.Id))), (Func<OrderItem, bool>)(e => e.ProductId == productId));
            if (Enumerable.Any<OrderItem>(source))
                return Enumerable.Sum<OrderItem>(source, (Func<OrderItem, long>)(x => x.Quantity));
            return 0;
        }
        /// <summary>
        /// 门店销量
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static long GetShopBranchSaleCount(long shopBranchId, DateTime startDate, DateTime endDate)
        {
            return Enumerable.Sum<OrderItem>((IEnumerable<OrderItem>)OrderApplication.GetOrderItemsByOrderId(Enumerable.Select<Order, long>((IEnumerable<Order>)OrderApplication.GetOrdersNoPage(new OrderQuery()
            {
                ShopBranchId = new long?(shopBranchId),
                StartDate = new DateTime?(startDate),
                EndDate = new DateTime?(endDate.AddDays(-1.0))
            }), (Func<Order, long>)(e => e.Id))), (Func<OrderItem, long>)(p => p.Quantity));
        }
        /// <summary>
        /// 产品是否存在
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static bool CheckProductIsExist(long shopBranchId, long productId)
        {
            return ShopBranchApplication._shopBranchService.CheckProductIsExist(shopBranchId, productId);
        }

        /// <summary>
        /// 根据查询条件判断是否有门店
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static bool Exists(CommonModel.ShopBranchQuery query)
        {
            return _shopBranchService.Exists(query);
        }

        #endregion

        #region 门店管理
        /// <summary>
        /// 新增门店
        /// </summary>
        public static void AddShopBranch(ShopBranch shopBranch, out long shopBranchId)
        {
            if (isRepeatBranchName(shopBranch.ShopId, shopBranch.Id, shopBranch.ShopBranchName))
            {
                throw new HimallException("门店名称不能重复！");
            }
            var branchManangerInfo = _shopBranchService.GetShopBranchManagersByName(shopBranch.UserName);
            if (branchManangerInfo != null)
            {
                throw new HimallException("门店管理员名称不能重复！");
            }
            if (ManagerApplication.CheckUserNameExist(shopBranch.UserName))
            {
                throw new HimallException("门店管理员名称不能与商家重复！");
            }
            AutoMapper.Mapper.CreateMap<ShopBranch, ShopBranchInfo>();
            var shopBranchInfo = AutoMapper.Mapper.Map<ShopBranch, ShopBranchInfo>(shopBranch);
            shopBranchInfo.AddressPath = RegionApplication.GetRegionPath(shopBranchInfo.AddressId);
            //默认在结尾增加分隔符
            shopBranchInfo.AddressPath = shopBranchInfo.AddressPath + CommonConst.ADDRESS_PATH_SPLIT;
            _shopBranchService.AddShopBranch(shopBranchInfo);
            shopBranchId = shopBranchInfo.Id;
            var salt = GetSalt();
            var shopBranchManagerInfo = new ShopBranchManagersInfo
            {
                CreateDate = DateTime.Now,
                UserName = shopBranch.UserName,
                ShopBranchId = shopBranchInfo.Id,
                PasswordSalt = salt,
                Password = GetPasswrodEncode(shopBranch.PasswordOne, salt)
            };
            _shopBranchService.AddShopBranchManagers(shopBranchManagerInfo);
        }
        /// <summary>
        /// 更新门店信息、管理员密码
        /// </summary>
        /// <param name="shopBranch"></param>
        public static void UpdateShopBranch(ShopBranch shopBranch)
        {
            if (isRepeatBranchName(shopBranch.ShopId, shopBranch.Id, shopBranch.ShopBranchName))
            {
                throw new HimallException("门店名称不能重复！");
            }
            Mapper.CreateMap<ShopBranch, ShopBranchInfo>();
            ShopBranchInfo info = Mapper.Map<ShopBranch, ShopBranchInfo>(shopBranch);
            info.AddressPath = RegionApplication.GetRegionPath(info.AddressId);
            info.AddressPath = info.AddressPath + ",";
            _shopBranchService.UpdateShopBranch(info);
            if (!(string.IsNullOrWhiteSpace(shopBranch.PasswordOne) || string.IsNullOrWhiteSpace(shopBranch.PasswordTwo)))
            {
                string salt = GetSalt();
                string passwrodEncode = GetPasswrodEncode(shopBranch.PasswordOne, salt);
                _shopBranchService.UpdateShopBranchManagerPwd(shopBranch.Id, shopBranch.UserName, passwrodEncode, salt);
            }

        }

        /// <summary>
        /// 更新指定门店管理员的密码
        /// </summary>
        /// <param name="managerId"></param>
        /// <param name="password"></param>
        public static void UpdateShopBranchManagerPwd(long managerId, string password)
        {
            _shopBranchService.UpdateShopBranchManagerPwd(managerId, password);
        }

        /// <summary>
        /// 删除门店
        /// </summary>
        /// <param name="branchId"></param>
        public static void DeleteShopBranch(long branchId)
        {
            //TODO:门店删除逻辑

            _shopBranchService.DeleteShopBranch(branchId);
        }
        /// <summary>
        /// 冻结门店
        /// </summary>
        /// <param name="shopBranchId"></param>
        public static void Freeze(long shopBranchId)
        {
            _shopBranchService.FreezeShopBranch(shopBranchId);
        }
        /// <summary>
        /// 解冻门店
        /// </summary>
        /// <param name="shopBranchId"></param>
        public static void UnFreeze(long shopBranchId)
        {
            _shopBranchService.UnFreezeShopBranch(shopBranchId);
        }
        #endregion 门店管理

        #region 门店登录
        /// <summary>
        /// 门店登录验证
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static ShopBranchManager ShopBranchLogin(string userName, string password)
        {
            var managerInfo = _shopBranchService.GetShopBranchManagersByName(userName);
            if (managerInfo == null)
                return null;

            password = GetPasswrodEncode(password, managerInfo.PasswordSalt);
            if (!string.Equals(password, managerInfo.Password))
                return null;

            AutoMapper.Mapper.CreateMap<ShopBranchManagersInfo, ShopBranchManager>();
            var manager = AutoMapper.Mapper.Map<ShopBranchManagersInfo, ShopBranchManager>(managerInfo);
            return manager;
        }
        #endregion 门店登录

        #region 门店商品管理
        /// <summary>
        /// 添加SKU，并过滤已添加的
        /// </summary>
        /// <param name="pids"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="shopId"></param>
        public static void AddProductSkus(IEnumerable<long> pids, long shopBranchId, long shopId)
        {
            var productsInfo = ProductManagerApplication.GetProductsByIds(pids).Where(e => e.ShopId == shopId);
            if (productsInfo == null)
                throw new HimallException("未找到商品数据");
            //查询已添加的SKU，用于添加时过滤
            var oldskus = _shopBranchService.GetSkus(shopId, new List<long> { shopBranchId },null).Select(e => e.SkuId);
            var allSkus = SKUApplication.GetByProductIds(productsInfo.Select(p => p.Id));
            var shopBranchSkus = new List<ShopBranchSkusInfo> { };

            var skus = allSkus.Where(s => !oldskus.Any(sku => sku == s.Id)).Select(e => new ShopBranchSkusInfo
            {
                ProductId = e.ProductId,
                SkuId = e.Id,
                ShopId = shopId,
                ShopBranchId = shopBranchId,
                Stock = 0,
                CreateDate = DateTime.Now
            });
            shopBranchSkus.AddRange(skus);

            _shopBranchService.AddSkus(shopBranchSkus);
        }
        /// <summary>
        /// 修正商品sku
        /// <para>0库存添加新的sku</para>
        /// </summary>
        /// <param name="productId"></param>
        public static void CorrectBranchProductSkus(long productId, long shopId)
        {
            var productsInfo = ProductManagerApplication.GetProduct(productId);
            if (productsInfo == null || productsInfo.ShopId != shopId)
            {
                throw new HimallException("未找到商品数据");
            }
            var shopbrids = _shopBranchService.GetAgentShopBranchIds(productId);
            List<long> pids = new List<long>();
            pids.Add(productId);

            foreach (var shopBranchId in shopbrids)
            {
                //查询已添加的SKU，用于添加时过滤
                var oldskus = _shopBranchService.GetSkus(shopId, new List<long> { shopBranchId }, null).Select(e => e.SkuId);
                var allSkus = SKUApplication.GetByProductIds(pids);
                var shopBranchSkus = new List<ShopBranchSkusInfo> { };

                var skus = allSkus.Where(s => !oldskus.Any(sku => sku == s.Id)).Select(e => new ShopBranchSkusInfo
                {
                    ProductId = e.ProductId,
                    SkuId = e.Id,
                    ShopId = shopId,
                    ShopBranchId = shopBranchId,
                    Stock = 0,
                    CreateDate = DateTime.Now
                });
                shopBranchSkus.AddRange(skus);

                _shopBranchService.AddSkus(shopBranchSkus);
            }
        }
        /// <summary>
        /// 设置门店SKU库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="skuIds"></param>
        /// <param name="stock"></param>
        /// <param name="opType"></param>
        public static void SetSkuStock(long shopBranchId, IEnumerable<string> skuIds, IEnumerable<int> stock, CommonModel.StockOpType opType)
        {
            switch (opType)
            {
                case StockOpType.Normal:
                    _shopBranchService.SetStock(shopBranchId, skuIds, stock);
                    break;
                case StockOpType.Add:
                    _shopBranchService.AddStock(shopBranchId, skuIds, stock);
                    break;
                case StockOpType.Reduce:
                    _shopBranchService.ReduceStock(shopBranchId, skuIds, stock);
                    break;
            }
        }
        /// <summary>
        /// 修改门店商品库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="pids"></param>
        /// <param name="stock"></param>
        /// <param name="opType"></param>
        public static void SetProductStock(long shopBranchId, IEnumerable<long> pids, int stock, CommonModel.StockOpType opType)
        {
            switch (opType)
            {
                case StockOpType.Normal:
                    _shopBranchService.SetProductStock(shopBranchId, pids, stock);
                    break;
                case StockOpType.Add:
                    _shopBranchService.AddProductStock(shopBranchId, pids, stock);
                    break;
                case StockOpType.Reduce:
                    _shopBranchService.ReduceProductStock(shopBranchId, pids, stock);
                    break;
            }
        }

        /// <summary>
        /// 下架商品
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="productIds"></param>
        public static void UnSaleProduct(long shopBranchId, IEnumerable<long> productIds)
        {
            _shopBranchService.SetBranchProductStatus(shopBranchId, productIds, ShopBranchSkuStatus.InStock);
        }
        /// <summary>
        /// 下架所有门店的商品
        /// <para></para>
        /// </summary>
        /// <param name="productId"></param>
        public static void UnSaleProduct(long productId)
        {
            _shopBranchService.SetBranchProductStatus(productId, ShopBranchSkuStatus.InStock);
        }
        public static bool CanOnSaleProduct(IEnumerable<long> productIds)
        {
            bool flag = false;
            if (Enumerable.Count<Product>((IEnumerable<Product>)ProductManagerApplication.GetProductsByIds(productIds)) == Enumerable.Count<long>(productIds))
                flag = true;
            return flag;
        }
        /// <summary>
        /// 上架商品
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="productIds"></param>
        public static void OnSaleProduct(long shopBranchId, IEnumerable<long> productIds)
        {
            _shopBranchService.SetBranchProductStatus(shopBranchId, productIds, ShopBranchSkuStatus.Normal);
        }
        #endregion 门店商品管理

        #region 私有方法
        private static bool isRepeatBranchName(long shopId, long shopBranchId, string branchName)
        {
            var exists = _shopBranchService.Exists(shopId, shopBranchId, branchName);
            return exists;
        }
        #endregion
        /// <summary>
        /// 取门店商品数量
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static IEnumerable<ShopBranchSkusInfo> GetShopBranchProductCount(long shopBranchId, DateTime? startDate, DateTime? endDate)
        {
            var skus = _shopBranchService.SearchShopBranchSkus(shopBranchId, startDate, endDate);
            return skus;
        }
        #region 周边门店
        /// <summary>
        /// 获取周边门店-分页
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopBranch> GetNearShopBranchs(ShopBranchQuery query)
        {
            var shopBranchInfos = _shopBranchService.GetNearShopBranchs(query);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    AddressDetail = RenderAddress(e.AddressPath, e.AddressDetail, 1),
                    ContactPhone = e.ContactPhone,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    Status = e.Status,
                    DistanceUnit = e.Distance >= 1 ? e.Distance + "KM" : e.Distance * 1000 + "M",
                    Distance = e.Distance,
                    ServeRadius=e.ServeRadius.HasValue ? e.ServeRadius.Value : 0,
                    Latitude = e.Latitude.HasValue ? e.Latitude.Value : 0,
                    Longitude = e.Longitude.HasValue ? e.Longitude.Value : 0,
                    DeliveFee = e.DeliveFee,
                    DeliveTotalFee = e.DeliveTotalFee,
                    IsAboveSelf = e.IsAboveSelf,
                    IsStoreDelive = e.IsStoreDelive,
                    ShopImages=HimallIO.GetRomoteImagePath(e.ShopImages, (string) null),
                    ShopId = e.ShopId,
                    FreeMailFee = e.FreeMailFee,
                    IsRecommend = e.IsRecommend,
                    RecommendSequence = e.RecommendSequence == 0 ? long.MaxValue : e.RecommendSequence
                }).ToList(),
                Total = shopBranchInfos.Total
            };
            return shopBranchs;
        }
        /// <summary>
        /// 查询附近门店
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopBranch> SearchNearShopBranchs(ShopBranchQuery search)
        {
            QueryPageModel<ShopBranchInfo> queryPageModel = ShopBranchApplication._shopBranchService.SearchNearShopBranchs(search);
            return new QueryPageModel<ShopBranch>()
            {
                Models = Enumerable.ToList<ShopBranch>(Enumerable.Select<ShopBranchInfo, ShopBranch>((IEnumerable<ShopBranchInfo>)queryPageModel.Models, (Func<ShopBranchInfo, ShopBranch>)(e =>
                {
                    ShopBranch shopBranch1 = new ShopBranch();
                    shopBranch1.AddressDetail = ShopBranchApplication.RenderAddress(e.AddressPath, e.AddressDetail, 1);
                    shopBranch1.ContactPhone = e.ContactPhone;
                    shopBranch1.Id = e.Id;
                    shopBranch1.ShopBranchName = e.ShopBranchName;
                    shopBranch1.Status = e.Status;
                    shopBranch1.DistanceUnit = e.Distance >= 1.0 ? (string)(object)e.Distance + (object)"KM" : (string)(object)(e.Distance * 1000.0) + (object)"M";
                    shopBranch1.Distance = e.Distance;
                    ShopBranch shopBranch2 = shopBranch1;
                    int? serveRadius;
                    int num1;
                    if (!e.ServeRadius.HasValue)
                    {
                        num1 = 0;
                    }
                    else
                    {
                        serveRadius = e.ServeRadius;
                        num1 = serveRadius.Value;
                    }
                    shopBranch2.ServeRadius = num1;
                    ShopBranch shopBranch3 = shopBranch1;
                    float? nullable;
                    double num2;
                    if (!e.Latitude.HasValue)
                    {
                        num2 = 0.0;
                    }
                    else
                    {
                        nullable = e.Latitude;
                        num2 = (double)nullable.Value;
                    }
                    shopBranch3.Latitude = (float)num2;
                    ShopBranch shopBranch4 = shopBranch1;
                    nullable = e.Longitude;
                    double num3;
                    if (!nullable.HasValue)
                    {
                        num3 = 0.0;
                    }
                    else
                    {
                        nullable = e.Longitude;
                        num3 = (double)nullable.Value;
                    }
                    shopBranch4.Longitude = (float)num3;
                    shopBranch1.DeliveFee = e.DeliveFee;
                    shopBranch1.DeliveTotalFee = e.DeliveTotalFee;
                    shopBranch1.IsAboveSelf = e.IsAboveSelf;
                    shopBranch1.IsStoreDelive = e.IsStoreDelive;
                    shopBranch1.ShopImages = HimallIO.GetRomoteImagePath(e.ShopImages, (string)null);
                    shopBranch1.ShopId = e.ShopId;
                    shopBranch1.FreeMailFee = e.FreeMailFee;
                    ShopBranch shopBranch5 = shopBranch1;
                    int num4;
                    if (!e.IsAboveSelf)
                    {
                        if (e.IsStoreDelive)
                        {
                            double distance = e.Distance;
                            serveRadius = e.ServeRadius;
                            if ((distance > (double)serveRadius.GetValueOrDefault() ? 0 : (serveRadius.HasValue ? 1 : 0)) != 0)
                            {
                                num4 = e.IsRecommend ? 1 : 0;
                                goto label_15;
                            }
                        }
                        num4 = 0;
                    }
                    else
                        num4 = 1;
                label_15:
                    shopBranch5.IsRecommend = num4 != 0;
                    shopBranch1.RecommendSequence = e.RecommendSequence == 0L ? long.MaxValue : e.RecommendSequence;
                    return shopBranch1;
                }))),
                Total = queryPageModel.Total
            };
        }
        /// <summary>
        /// 根据标签查询附近门店
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopBranch> TagsSearchNearShopBranchs(ShopBranchQuery search)
        {
            QueryPageModel<ShopBranchInfo> queryPageModel = ShopBranchApplication._shopBranchService.TagsSearchNearShopBranchs(search);
            return new QueryPageModel<ShopBranch>()
            {
                Models = Enumerable.ToList<ShopBranch>(Enumerable.Select<ShopBranchInfo, ShopBranch>((IEnumerable<ShopBranchInfo>)queryPageModel.Models, (Func<ShopBranchInfo, ShopBranch>)(e =>
                {
                    ShopBranch shopBranch1 = new ShopBranch();
                    shopBranch1.AddressDetail = ShopBranchApplication.RenderAddress(e.AddressPath, e.AddressDetail, 1);
                    shopBranch1.ContactPhone = e.ContactPhone;
                    shopBranch1.Id = e.Id;
                    shopBranch1.ShopBranchName = e.ShopBranchName;
                    shopBranch1.Status = e.Status;
                    shopBranch1.DistanceUnit = e.Distance >= 1.0 ? (string)(object)e.Distance + (object)"KM" : (string)(object)(e.Distance * 1000.0) + (object)"M";
                    shopBranch1.Distance = e.Distance;
                    shopBranch1.ServeRadius = e.ServeRadius.HasValue ? e.ServeRadius.Value : 0;
                    ShopBranch shopBranch2 = shopBranch1;
                    float? nullable;
                    double num1;
                    if (!e.Latitude.HasValue)
                    {
                        num1 = 0.0;
                    }
                    else
                    {
                        nullable = e.Latitude;
                        num1 = (double)nullable.Value;
                    }
                    shopBranch2.Latitude = (float)num1;
                    ShopBranch shopBranch3 = shopBranch1;
                    nullable = e.Longitude;
                    double num2;
                    if (!nullable.HasValue)
                    {
                        num2 = 0.0;
                    }
                    else
                    {
                        nullable = e.Longitude;
                        num2 = (double)nullable.Value;
                    }
                    shopBranch3.Longitude = (float)num2;
                    shopBranch1.DeliveFee = e.DeliveFee;
                    shopBranch1.DeliveTotalFee = e.DeliveTotalFee;
                    shopBranch1.IsAboveSelf = e.IsAboveSelf;
                    shopBranch1.IsStoreDelive = e.IsStoreDelive;
                    shopBranch1.ShopImages = HimallIO.GetRomoteImagePath(e.ShopImages, (string)null);
                    shopBranch1.ShopId = e.ShopId;
                    shopBranch1.FreeMailFee = e.FreeMailFee;
                    shopBranch1.IsRecommend = e.IsRecommend;
                    shopBranch1.RecommendSequence = e.RecommendSequence == 0L ? long.MaxValue : e.RecommendSequence;
                    return shopBranch1;
                }))),
                Total = queryPageModel.Total
            };
        }
        /// <summary>
        /// 查询门店产品
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopBranch> StoreByProductNearShopBranchs(ShopBranchQuery search)
        {
            QueryPageModel<ShopBranchInfo> queryPageModel = ShopBranchApplication._shopBranchService.StoreByProductNearShopBranchs(search);
            return new QueryPageModel<ShopBranch>()
            {
                Models = Enumerable.ToList<ShopBranch>(Enumerable.Select<ShopBranchInfo, ShopBranch>((IEnumerable<ShopBranchInfo>)queryPageModel.Models, (Func<ShopBranchInfo, ShopBranch>)(e =>
                {
                    ShopBranch shopBranch1 = new ShopBranch();
                    shopBranch1.AddressDetail = ShopBranchApplication.RenderAddress(e.AddressPath, e.AddressDetail, 1);
                    shopBranch1.ContactPhone = e.ContactPhone;
                    shopBranch1.Id = e.Id;
                    shopBranch1.ShopBranchName = e.ShopBranchName;
                    shopBranch1.Status = e.Status;
                    shopBranch1.DistanceUnit = e.Distance >= 1.0 ? (string)(object)e.Distance + (object)"KM" : (string)(object)(e.Distance * 1000.0) + (object)"M";
                    shopBranch1.Distance = e.Distance;
                    shopBranch1.ServeRadius = e.ServeRadius.HasValue ? e.ServeRadius.Value : 0;
                    ShopBranch shopBranch2 = shopBranch1;
                    float? nullable;
                    double num1;
                    if (!e.Latitude.HasValue)
                    {
                        num1 = 0.0;
                    }
                    else
                    {
                        nullable = e.Latitude;
                        num1 = (double)nullable.Value;
                    }
                    shopBranch2.Latitude = (float)num1;
                    ShopBranch shopBranch3 = shopBranch1;
                    nullable = e.Longitude;
                    double num2;
                    if (!nullable.HasValue)
                    {
                        num2 = 0.0;
                    }
                    else
                    {
                        nullable = e.Longitude;
                        num2 = (double)nullable.Value;
                    }
                    shopBranch3.Longitude = (float)num2;
                    shopBranch1.DeliveFee = e.DeliveFee;
                    shopBranch1.DeliveTotalFee = e.DeliveTotalFee;
                    shopBranch1.IsAboveSelf = e.IsAboveSelf;
                    shopBranch1.IsStoreDelive = e.IsStoreDelive;
                    shopBranch1.ShopImages = HimallIO.GetRomoteImagePath(e.ShopImages, (string)null);
                    shopBranch1.ShopId = e.ShopId;
                    shopBranch1.FreeMailFee = e.FreeMailFee;
                    shopBranch1.IsRecommend = e.IsRecommend;
                    shopBranch1.RecommendSequence = e.RecommendSequence == 0L ? long.MaxValue : e.RecommendSequence;
                    return shopBranch1;
                }))),
                Total = queryPageModel.Total
            };
        }
        /// <summary>
        /// 组合新地址
        /// </summary>
        /// <param name="addressPath"></param>
        /// <param name="address"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static string RenderAddress(string addressPath, string address, int level)
        {
            if (!string.IsNullOrWhiteSpace(addressPath))
            {
                string fullName = RegionApplication.GetRegionName(addressPath);
                string[] arr = fullName.Split(',');//省，市，区，街道
                if (arr.Length > 0)
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        address = address.Replace(arr[i], "");//去掉原详细地址中的省市区街道。(为兼容旧门店数据)
                    }
                }

                if (level <= arr.Length)
                {
                    for (int i = 0; i < level; i++)
                    {
                        fullName = fullName.Replace(arr[i], "");
                    }
                    address = fullName + address;
                }
            }
            return address.Replace(",", "");
        }
        public static QueryPageModel<ShopBranch> GetShopBranchsAll(ShopBranchQuery query)
        {
            var shopBranchInfos = _shopBranchService.GetShopBranchsAll(query);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    AddressDetail = e.AddressDetail,
                    ContactPhone = e.ContactPhone,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    Status = e.Status,
                    DistanceUnit = e.Distance >= 1 ? e.Distance + "KM" : e.Distance * 1000 + "M",
                    Distance = e.Distance,
                    ServeRadius = TypeHelper.ObjectToInt(e.ServeRadius),
                    Latitude = e.Latitude.HasValue ? e.Latitude.Value : 0,
                    Longitude = e.Longitude.HasValue ? e.Longitude.Value : 0,
                    AddressPath = e.AddressPath
                }).ToList()
            };
            return shopBranchs;
        }
        /// <summary>
        /// 不分页查询门店
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static List<ShopBranch> GetShopBranchsNoPager(ShopBranchQuery query)
        {
            return (from n in _shopBranchService.GetShopBranchsNoPager(query)
                    select new ShopBranch { Id = n.Id, ShopBranchName = n.ShopBranchName, AddressDetail = GetShopBranchsFullAddress(n.AddressPath) + n.AddressDetail, RecommendSequence = n.RecommendSequence } into n
                    orderby n.RecommendSequence
                    select n).ToList<ShopBranch>();

        }
        /// <summary>
        /// 地址组合
        /// </summary>
        /// <param name="addressPath"></param>
        /// <returns></returns>
        public static string GetShopBranchsFullAddress(string addressPath)
        {
            string str = string.Empty;
            if (!string.IsNullOrEmpty(addressPath))
                str = RegionApplication.GetRegionName(addressPath, ",");
            return str.Replace(",", "");
        }
        /// <summary>
        /// 门店推荐
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static bool RecommendShopBranch(long[] ids)
        {
            return ShopBranchApplication._shopBranchService.RecommendShopBranch(ids);
        }
        /// <summary>
        /// 修改配送范围
        /// </summary>
        /// <param name="oriShopBranchId"></param>
        /// <param name="newShopBranchId"></param>
        /// <returns></returns>
        public static bool RecommendChangeSequence(long oriShopBranchId, long newShopBranchId)
        {
            return ShopBranchApplication._shopBranchService.RecommendChangeSequence(oriShopBranchId, newShopBranchId);
        }
        /// <summary>
        /// 重置推荐
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public static bool ResetShopBranchRecommend(long shopBranchId)
        {
            return ShopBranchApplication._shopBranchService.ResetShopBranchRecommend(shopBranchId);
        }
        /// <summary>
        /// 查询范围内的门店
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="shopId"></param>
        /// <param name="latAndLng"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopBranch> GetArealShopBranchsAll(int areaId, int shopId, string latAndLng)
        {
            float result1 = 0.0f;
            float result2 = 0.0f;
            string[] strArray = HttpUtility.UrlDecode(latAndLng).Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strArray.Length == 2)
            {
                float.TryParse(strArray[0], out result1);
                float.TryParse(strArray[1], out result2);
            }
            QueryPageModel<ShopBranchInfo> arealShopBranchsAll = ShopBranchApplication._shopBranchService.GetArealShopBranchsAll(areaId, shopId, result1, result2);
            return new QueryPageModel<ShopBranch>()
            {
                Models = Enumerable.ToList<ShopBranch>(Enumerable.Select<ShopBranchInfo, ShopBranch>((IEnumerable<ShopBranchInfo>)arealShopBranchsAll.Models, (Func<ShopBranchInfo, ShopBranch>)(e => new ShopBranch()
                {
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName
                })))
            };
        }
        /// <summary>
        /// 所有门店标签信息
        /// </summary>
        /// <returns></returns>
        public static List<ShopBranchTagModel> GetAllShopBranchTagInfos()
        {
            return Enumerable.ToList<ShopBranchTagModel>(Enumerable.Select<ShopBranchTagInfo, ShopBranchTagModel>((IEnumerable<ShopBranchTagInfo>)ShopBranchApplication._shopBranchService.GetAllShopBranchTagInfo(), (Func<ShopBranchTagInfo, ShopBranchTagModel>)(e => new ShopBranchTagModel()
            {
                Id = e.Id,
                Title = e.Title,
                ShopBranchCount = Enumerable.Count<ShopBranchInTagInfo>((IEnumerable<ShopBranchInTagInfo>)e.Himall_ShopBranchInTags)
            })));
        }
        /// <summary>
        /// 查询标签信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static ShopBranchTagModel GetShopBranchTagInfo(long Id)
        {
            ShopBranchTagInfo shopBranchTagInfo = Enumerable.FirstOrDefault<ShopBranchTagInfo>((IEnumerable<ShopBranchTagInfo>)ShopBranchApplication._shopBranchService.GetAllShopBranchTagInfo(), (Func<ShopBranchTagInfo, bool>)(n => n.Id == Id));
            if (null == shopBranchTagInfo)
                return (ShopBranchTagModel)null;
            return new ShopBranchTagModel()
            {
                Id = shopBranchTagInfo.Id,
                Title = shopBranchTagInfo.Title,
                ShopBranchCount = shopBranchTagInfo.Himall_ShopBranchInTags.Count
            };
        }
        /// <summary>
        /// 添加标签信息
        /// </summary>
        /// <param name="title"></param>
        public static void AddShopBranchTagInfo(string title)
        {
            if (string.IsNullOrEmpty(title))
                throw new Exception("标签名称不可为空");
            ShopBranchApplication._shopBranchService.AddShopBranchTagInfo(new ShopBranchTagInfo()
            {
                Title = title
            });
        }
        /// <summary>
        /// 更新标签信息
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static bool UpdateShopBranchTagInfo(long Id, string title)
        {
            if (Id <= 0L)
                throw new Exception("修改目标标签不可为空");
            if (string.IsNullOrEmpty(title))
                throw new Exception("标签名称不可为空");
            return ShopBranchApplication._shopBranchService.UpdateShopBranchTagInfo(new ShopBranchTagInfo()
            {
                Id = Id,
                Title = title
            });
        }
        /// <summary>
        /// 删除标签信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static bool DeleteShopBranchTagInfo(long Id)
        {
            return ShopBranchApplication._shopBranchService.DeleteShopBranchTagInfo(Id);
        }
        /// <summary>
        /// 门店品牌标签信息
        /// </summary>
        /// <param name="shopBranchIds"></param>
        /// <param name="shopBranchTagIds"></param>
        public static void SetShopBrandTagInfos(long[] shopBranchIds, long[] shopBranchTagIds)
        {
            ShopBranchApplication._shopBranchService.DeleteShopBranchInTagInfo(shopBranchIds);
            ShopBranchApplication._shopBranchService.AddShopBranchInTagInfo(shopBranchIds, shopBranchTagIds);
        }
        /// <summary>
        /// 读取坐标
        /// </summary>
        /// <param name="fromLatLng"></param>
        /// <param name="latlng"></param>
        /// <returns></returns>
        public static double GetLatLngDistances(string fromLatLng, string latlng)
        {
            return ShopBranchApplication._shopBranchService.GetLatLngDistancesFromAPI(fromLatLng, latlng);
        }
        /// <summary>
        /// 优惠券是否可用
        /// </summary>
        /// <param name="couponinfo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static int CouponIsUse(CouponInfo couponinfo, long userId)
        {
            int num = 0;
            ObsoletePageModel<CouponRecordInfo> couponRecordList = ShopBranchApplication._iCouponService.GetCouponRecordList(new CouponRecordQuery()
            {
                CouponId = new long?(couponinfo.Id),
                UserId = new long?(userId)
            });
            if (couponinfo.PerMax != 0 && couponRecordList.Total >= couponinfo.PerMax)
                num = 1;
            if (num == 0)
            {
                CouponRecordQuery query = new CouponRecordQuery()
                {
                    CouponId = new long?(couponinfo.Id)
                };
                if (ShopBranchApplication._iCouponService.GetCouponRecordList(query).Total >= couponinfo.Num)
                    num = 2;
            }
            return num;
        }
        #endregion
    }
}
