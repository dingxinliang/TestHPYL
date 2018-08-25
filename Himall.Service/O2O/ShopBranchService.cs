using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices;
using Himall.Model;
using Himall.Entity;
using Himall.CommonModel;
using Himall.Core;
using Himall.IServices.QueryModel;
using Himall.Core.Helper;
using Dapper;
using MySql.Data.MySqlClient;
using System.Linq.Expressions;
using System.Data;

namespace Himall.Service
{
    public class ShopBranchService : ServiceBase, IShopBranchService
    {
        /// <summary>
        /// 添加分店
        /// </summary>
        /// <param name="shopBranchInfo"></param>
        public void AddShopBranch(Model.ShopBranchInfo shopBranchInfo)
        {
            Context.ShopBranchInfo.Add(shopBranchInfo);
            Context.SaveChanges();
        }

        /// <summary>
        /// 添加分店管理员
        /// </summary>
        /// <param name="shopBranchManagersInfo"></param>
        public void AddShopBranchManagers(Model.ShopBranchManagersInfo shopBranchManagersInfo)
        {
            Context.ShopBranchManagersInfo.Add(shopBranchManagersInfo);
            Context.SaveChanges();
        }

        /// <summary>
        /// 判断门店名称是否重复
        /// </summary>
        /// <param name="shopId">商家店铺ID</param>
        /// <param name="shopBranchName">门店名字</param>
        /// <returns></returns>
        public bool Exists(long shopId, long shopBranchId, string shopBranchName)
        {
            return Context.ShopBranchInfo.Any(e => e.ShopBranchName == shopBranchName && e.ShopId == shopId && e.Id != shopBranchId);
        }

        /// <summary>
        /// 根据查询条件判断是否有门店
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public bool Exists(CommonModel.ShopBranchQuery query)
        {
            var shopBranchs = ToWhere(query);

            return shopBranchs.Any();
        }

        public ShopBranchInfo GetShopBranchById(long id)
        {
            return Context.ShopBranchInfo.FirstOrDefault(e => e.Id == id);
        }


        /// <summary>
        /// 根据门店IDs获取门店
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        public List<ShopBranchInfo> GetShopBranchByIds(IEnumerable<long> Ids)
        {
            return Context.ShopBranchInfo.Where(e => Ids.Contains(e.Id)).ToList();
        }


        /// <summary>
        /// 根据门店联系方式获取门店信息
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public ShopBranchInfo GetShopBranchByContact(string contact)
        {
            return this.Context.ShopBranchInfo.FirstOrDefault(p => p.ContactPhone == contact);
        }

        public QueryPageModel<ShopBranchInfo> GetShopBranchs(CommonModel.ShopBranchQuery query)
        {
            int total = 0;
            var shopBranchs = ToWhere(query);
            shopBranchs = shopBranchs.GetPage(out total, query.PageNo, query.PageSize, s => s.OrderBy(e => e.Id));
            QueryPageModel<ShopBranchInfo> pageModel = new QueryPageModel<ShopBranchInfo>() { Models = shopBranchs.ToList(), Total = total };
            return pageModel;
        }

        /// <summary>
        /// 获取周边门店-分页
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<ShopBranchInfo> GetNearShopBranchs(CommonModel.ShopBranchQuery query)
        {
            decimal num = 0M;
            decimal num2 = 0M;
            if (query.FromLatLng.Split(new char[] { ',' }).Length != 2)
            {
                return new QueryPageModel<ShopBranchInfo>();
            }
            num = TypeHelper.ObjectToDecimal(query.FromLatLng.Split(new char[] { ',' })[0]);
            num2 = TypeHelper.ObjectToDecimal(query.FromLatLng.Split(new char[] { ',' })[1]);
            QueryPageModel<ShopBranchInfo> model = new QueryPageModel<ShopBranchInfo>();
            DynamicParameters parms = new DynamicParameters();
            string str = "select count(1) from himall_shopbranch ";
            string str2 = string.Format("select AddressDetail,AddressPath,ContactPhone,Id,ShopBranchName,Status,Latitude,Longitude,ServeRadius,truncate(6378.137*2*ASIN(SQRT(POW(SIN(({0}*PI()/180.0-Latitude*PI()/180.0)/2),2)+COS({0}*PI()/180.0)*COS(Latitude*PI()/180.0)*POW(SIN(({1}*PI()/180.0-Longitude*PI()/180.0)/2),2))),2) AS Distance,DeliveFee,DeliveTotalFee,IsStoreDelive,IsAboveSelf,ShopId,ShopImages,IsRecommend,FreeMailFee from himall_shopbranch", num, num2);
            string searchWhere = this.GetSearchWhere(query, parms);
            string searchOrder = this.GetSearchOrder(query);
            if (query.OrderKey == 2)
            {
                searchOrder = string.Format(" {0}, id desc ", searchOrder.TrimEnd(new char[] { ',' }));
            }
            string searchPage = this.GetSearchPage(query);
            using (MySqlConnection connection = new MySqlConnection(Connection.ConnectionString))
            {
                int? commandTimeout = null;
                CommandType? commandType = null;
                model.Models = connection.Query<ShopBranchInfo>((str2 + searchWhere + searchOrder + searchPage), parms, null, true, commandTimeout, commandType).ToList<ShopBranchInfo>();
                model.Total = int.Parse(connection.ExecuteScalar((str + searchWhere), parms, null, null, null).ToString());
            }
            return model;

        }
        public QueryPageModel<ShopBranchInfo> GetShopBranchsAll(CommonModel.ShopBranchQuery query)
        {
            var shopBranchs = ToWhere(query).ToList();
            shopBranchs.ForEach(p =>
            {
                if (p.Latitude.HasValue && p.Longitude.HasValue && p.Latitude.Value > 0 && p.Longitude.Value > 0)
                    p.Distance = GetLatLngDistancesFromAPI(query.FromLatLng, string.Format("{0},{1}", p.Latitude, p.Longitude));
                else
                    p.Distance = 0;
            });
            return new QueryPageModel<ShopBranchInfo>() { Models = shopBranchs.AsQueryable().OrderBy(p => p.Distance).ToList() };
        }
        /// <summary>
        /// 获取门店配送范围在同一区域的门店
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        //public QueryPageModel<ShopBranchInfo> GetArealShopBranchsAll(int areaId, int shopId)
        //{
        //    return new QueryPageModel<ShopBranchInfo>() { Models = ToArealShopWhere(areaId, shopId).ToList() };
        //}
        /// <summary>
        /// 获取一个起点坐标到多个终点坐标之间的距离
        /// </summary>
        /// <param name="fromLatLng"></param>
        /// <param name="toLatLngs"></param>
        /// <returns></returns>
        private double GetLatLngDistancesFromAPI(string fromLatLng, string latlng)
        {
            if (!string.IsNullOrWhiteSpace(fromLatLng) && (!string.IsNullOrWhiteSpace(latlng)))
            {
                try
                {
                    var aryLatlng = fromLatLng.Split(',');
                    var fromlat = double.Parse(aryLatlng[0]);
                    var fromlng = double.Parse(aryLatlng[1]);
                    double EARTH_RADIUS = 6378.137;//地球半径

                    var aryToLatlng = latlng.Split(',');
                    var tolat = double.Parse(aryToLatlng[0]);
                    var tolng = double.Parse(aryToLatlng[1]);
                    var fromRadLat = fromlat * Math.PI / 180.0;
                    var toRadLat = tolat * Math.PI / 180.0;
                    double a = fromRadLat - toRadLat;
                    double b = (fromlng * Math.PI / 180.0) - (tolng * Math.PI / 180.0);
                    double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
                     Math.Cos(fromRadLat) * Math.Cos(toRadLat) * Math.Pow(Math.Sin(b / 2), 2)));
                    s = s * EARTH_RADIUS;
                    return Math.Round((Math.Round(s * 10000) / 10000), 2);
                }
                catch (Exception ex)
                {
                    Core.Log.Error("计算经纬度距离异常", ex);
                    return 0;
                }
            }
            return 0;
        }

        /// <summary>
        /// 根据分店id获取分店信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<ShopBranchInfo> GetShopBranchs(IEnumerable<long> ids)
        {
            return this.Context.ShopBranchInfo.Where(p => ids.Contains(p.Id)).ToList();
        }

        public void FreezeShopBranch(long shopBranchId)
        {
            var shopBranch = Context.ShopBranchInfo.FirstOrDefault(e => e.Id == shopBranchId);
            shopBranch.Status = CommonModel.ShopBranchStatus.Freeze;
            Context.SaveChanges();
        }

        public void UnFreezeShopBranch(long shopBranchId)
        {
            var shopBranch = Context.ShopBranchInfo.FirstOrDefault(e => e.Id == shopBranchId);
            shopBranch.Status = CommonModel.ShopBranchStatus.Normal;
            Context.SaveChanges();
        }

        public IEnumerable<ShopBranchManagersInfo> GetShopBranchManagers(long branchId)
        {
            var managers = Context.ShopBranchManagersInfo.Where(e => e.ShopBranchId == branchId);
            return managers.ToList();
        }

        public void UpdateShopBranch(ShopBranchInfo shopBranch)
        {
            ShopBranchInfo info = base.Context.ShopBranchInfo.FirstOrDefault<ShopBranchInfo>(e => e.Id == shopBranch.Id);
            if (info == null)
            {
                throw new HimallException("数据异常，更新失败！");
            }
            info.ShopBranchName = shopBranch.ShopBranchName;
            info.AddressDetail = shopBranch.AddressDetail;
            info.AddressId = shopBranch.AddressId;
            info.ContactPhone = shopBranch.ContactPhone;
            info.ContactUser = shopBranch.ContactUser;
            info.AddressPath = shopBranch.AddressPath;
            info.ShopImages = shopBranch.ShopImages;
            info.Longitude = shopBranch.Longitude;
            info.Latitude = shopBranch.Latitude;
            info.ServeRadius = shopBranch.ServeRadius;
            info.IsAboveSelf = shopBranch.IsAboveSelf;
            info.IsStoreDelive = shopBranch.IsStoreDelive;
            info.DeliveFee = shopBranch.DeliveFee;
            info.DeliveTotalFee = shopBranch.DeliveTotalFee;
            info.StoreOpenStartTime = shopBranch.StoreOpenStartTime;
            info.StoreOpenEndTime = shopBranch.StoreOpenEndTime;
            info.FreeMailFee = shopBranch.FreeMailFee;
            base.Context.SaveChanges();
        }

        public void UpdateShopBranchManagerPwd(long branchId, string userName, string pwd, string pwdSalt)
        {
            var branchManager = Context.ShopBranchManagersInfo.FirstOrDefault(e => e.ShopBranchId == branchId && e.UserName == userName);
            if (branchManager == null)
                throw new Himall.Core.HimallException("数据异常，更新失败！");
            branchManager.Password = pwd;
            branchManager.PasswordSalt = pwdSalt;
            Context.SaveChanges();
        }

        /// <summary>
        /// 更新指定门店管理员的密码
        /// </summary>
        /// <param name="managerId"></param>
        /// <param name="password"></param>
        public void UpdateShopBranchManagerPwd(long managerId, string password)
        {
            var branchManager = Context.ShopBranchManagersInfo.FirstOrDefault(e => e.Id == managerId);
            if (branchManager == null)
                throw new Himall.Core.HimallException("数据异常，更新失败！");

            branchManager.Password = SecureHelper.MD5(SecureHelper.MD5(password) + branchManager.PasswordSalt);
            Context.SaveChanges();
        }

        public void DeleteShopBranch(long id)
        {
            var branchShop = Context.ShopBranchInfo.FirstOrDefault(e => e.Id == id);
            if (branchShop == null)
                throw new Himall.Core.HimallException("未找到门店，删除失败！");
            Context.ShopBranchInfo.Remove(branchShop);
            Context.SaveChanges();
        }

        /// <summary>
        /// 获取分店经营的商品SKU
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="shopBranchIds"></param>
        /// <param name="status">null表示所有</param>
        /// <returns></returns>
        public List<ShopBranchSkusInfo> GetSkus(long shopId, IEnumerable<long> shopBranchIds, ShopBranchSkuStatus? status = ShopBranchSkuStatus.Normal)
        {
            var sql = Context.ShopBranchSkusInfo.Where(p => p.ShopId == shopId && shopBranchIds.Contains(p.ShopBranchId));
            if (status.HasValue)
            {
                sql = sql.Where(p => p.Status == status.Value);
            }
            return sql.ToList();
        }
        /// <summary>
        /// 根据SKUID取门店SKU
        /// </summary>
        /// <param name="skuIds"></param>
        /// <returns></returns>
        public List<ShopBranchSkusInfo> GetSkusByIds(long shopBranchId, IEnumerable<string> skuIds)
        {
            return this.Context.ShopBranchSkusInfo.Where(p => skuIds.Contains(p.SkuId) && p.ShopBranchId == shopBranchId).ToList();
        }
        /// <summary>
        /// 根据ID取门店管理员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ShopBranchManagersInfo GetShopBranchManagersById(long id)
        {
            return Context.ShopBranchManagersInfo.FirstOrDefault(e => e.Id == id);
        }


        public ShopBranchManagersInfo GetShopBranchManagersByName(string userName)
        {
            return Context.ShopBranchManagersInfo.FirstOrDefault(e => e.UserName == userName);
        }

        /// <summary>
        /// 添加门店sku
        /// </summary>
        /// <param name="infos"></param>
        public void AddSkus(IEnumerable<ShopBranchSkusInfo> infos)
        {
            Context.ShopBranchSkusInfo.AddRange(infos);
            Context.Database.ExecuteSqlCommandAsync("DELETE from himall_shopbranchskus where SkuId not in (SELECT Id from himall_skus)");
            Context.SaveChanges();
        }

        #region 修改库存
        public void SetStock(long shopBranchId, IEnumerable<string> skuIds, IEnumerable<int> stock)
        {
            var skus = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == shopBranchId && skuIds.Any(s => s == e.SkuId));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            var i = 0;
            var stk = stock.ToArray();
            foreach (var sku in skus)
            {
                sku.Stock = stk[i];
                i++;
            }
            Context.SaveChanges();
        }
        public void SetProductStock(long shopBranchId, IEnumerable<long> pids, int stock)
        {
            var skus = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == shopBranchId && pids.Any(s => s == e.ProductId));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            foreach (var sku in skus)
            {
                sku.Stock = stock;
            }
            Context.SaveChanges();
        }
        public void AddStock(long shopBranchId, IEnumerable<string> skuIds, IEnumerable<int> stock)
        {
            var skus = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == shopBranchId && skuIds.Any(s => s == e.SkuId));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            var i = 0;
            var stk = stock.ToArray();
            foreach (var sku in skus)
            {
                sku.Stock = sku.Stock + stk[i];
            }
            Context.SaveChanges();
        }
        public void AddProductStock(long shopBranchId, IEnumerable<long> pids, int stock)
        {
            var skus = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == shopBranchId && pids.Any(s => s == e.ProductId));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            foreach (var sku in skus)
            {
                sku.Stock = sku.Stock + stock;
            }
            Context.SaveChanges();
        }
        public void ReduceStock(long shopBranchId, IEnumerable<string> skuIds, IEnumerable<int> stock)
        {
            var skus = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == shopBranchId && skuIds.Any(s => s == e.SkuId));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            var i = 0;
            var stk = stock.ToArray();
            foreach (var sku in skus)
            {
                sku.Stock = (sku.Stock - stk[i]) > 0 ? sku.Stock - stk[i] : 0;
            }
            Context.SaveChanges();
        }

        public void ReduceProductStock(long shopBranchId, IEnumerable<long> pids, int stock)
        {
            var skus = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == shopBranchId && pids.Any(s => s == e.ProductId));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            foreach (var sku in skus)
            {
                sku.Stock = (sku.Stock - stock) > 0 ? sku.Stock - stock : 0;
            }
            Context.SaveChanges();
        }
        #endregion 修改库存

        public void SetBranchProductStatus(long shopBranchId, IEnumerable<long> pIds, ShopBranchSkuStatus status)
        {
            var skus = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == shopBranchId && pIds.Any(s => s == e.ProductId));
            if (skus.Count() == 0)
                throw new HimallException("参数异常,未找到数据");
            foreach (var sku in skus)
            {
                sku.Status = status;
            }
            Context.SaveChanges();
        }

        public void SetBranchProductStatus(long productId, ShopBranchSkuStatus status)
        {
            var skus = Context.ShopBranchSkusInfo.Where(e => e.ProductId == productId);
            //if (skus.Count() == 0)
            //    throw new HimallException("参数异常,未找到数据");
            if (skus != null && skus.Count() > 0)
            {
                foreach (var sku in skus)
                {
                    sku.Status = status;
                }
                Context.SaveChanges();
            }
        }

        public QueryPageModel<ProductInfo> SearchProduct(ShopBranchProductQuery productQueryModel)
        {
            var products = Context.ProductInfo.Where(item => true);
            //过滤已删除的商品
            products = products.Where(item => item.IsDeleted == false);

            if (productQueryModel.Ids != null && productQueryModel.Ids.Count() > 0)//条件 编号
                products = products.Where(item => productQueryModel.Ids.Contains(item.Id));

            if (!string.IsNullOrWhiteSpace(productQueryModel.ProductCode))
                products = products.Where(item => item.ProductCode == productQueryModel.ProductCode);

            if (productQueryModel.ShopId.HasValue)//过滤店铺
            {
                products = products.Where(item => item.ShopId == productQueryModel.ShopId);
                if (productQueryModel.IsOverSafeStock.HasValue)
                {
                    List<long> pids = products.Select(e => e.Id).ToList();
                    if (productQueryModel.IsOverSafeStock.Value)
                    {
                        pids = Context.SKUInfo.Where(e => e.SafeStock.Value >= e.Stock && pids.Contains(e.ProductId)).Select(e => e.ProductId).ToList();
                    }
                    else
                    {
                        pids = Context.SKUInfo.Where(e => e.SafeStock.Value < e.Stock && pids.Contains(e.ProductId)).Select(e => e.ProductId).ToList();
                    }
                    products = products.Where(e => pids.Contains(e.Id));
                }
            }
            if (productQueryModel.AuditStatus != null)//条件 审核状态
                products = products.Where(item => productQueryModel.AuditStatus.Contains(item.AuditStatus));

            if (productQueryModel.SaleStatus.HasValue)
            {
                products = products.Where(item => item.SaleStatus == productQueryModel.SaleStatus);
            }

            if (productQueryModel.CategoryId.HasValue)//条件 分类编号
                products = products.Where(item => ("|" + item.CategoryPath + "|").Contains("|" + productQueryModel.CategoryId.Value + "|"));

            if (productQueryModel.NotIncludedInDraft)
            {
                products = products.Where(item => item.SaleStatus != ProductInfo.ProductSaleStatus.InDraft);
            }

            if (productQueryModel.StartDate.HasValue)//添加日期筛选
                products = products.Where(item => item.AddedDate >= productQueryModel.StartDate);
            if (productQueryModel.EndDate.HasValue)//添加日期筛选
            {
                var end = productQueryModel.EndDate.Value.Date.AddDays(1);
                products = products.Where(item => item.AddedDate < end);
            }
            if (!string.IsNullOrWhiteSpace(productQueryModel.KeyWords))// 条件 关键字
                products = products.Where(item => item.ProductName.Contains(productQueryModel.KeyWords));

            if (!string.IsNullOrWhiteSpace(productQueryModel.ShopName))//查询商家关键字
            {
                var shopIds = Context.ShopInfo.FindBy(item => item.ShopName.Contains(productQueryModel.ShopName)).Select(item => item.Id);
                products = products.Where(item => shopIds.Contains(item.ShopId));
            }
            if (productQueryModel.IsLimitTimeBuy)
            {
                var limits = Context.LimitTimeMarketInfo.Where(l => l.AuditStatus == LimitTimeMarketInfo.LimitTimeMarketAuditStatus.Ongoing).Select(l => l.ProductId);
                products = products.Where(p => !limits.Contains(p.Id));
            }
            if (productQueryModel.shopBranchId.HasValue && productQueryModel.shopBranchId.Value != 0)
            {//过滤门店已选商品
                var pid = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == productQueryModel.shopBranchId.Value).Select(item => item.ProductId).Distinct();
                products = products.Where(e => pid.Any(id => id == e.Id));
            }
            if (productQueryModel.ShopBranchProductStatus.HasValue)
            {//门店商品状态
                var pid = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == productQueryModel.shopBranchId.Value && e.Status == productQueryModel.ShopBranchProductStatus.Value).Select(item => item.ProductId).Distinct();
                products = products.Where(e => pid.Any(id => id == e.Id));
            }

            long shopCateogryId = productQueryModel.ShopCategoryId.GetValueOrDefault();

            var proorder = products.GetOrderBy(d => d.OrderByDescending(o => o.Id));
            switch (productQueryModel.OrderKey)
            {
                case 2:
                    if (!productQueryModel.OrderType)
                    {
                        proorder = products.GetOrderBy(d => d.OrderByDescending(o => o.AddedDate));
                    }
                    else
                    {
                        proorder = products.GetOrderBy(d => d.OrderBy(o => o.AddedDate));
                    }
                    break;
                case 3:
                    if (!productQueryModel.OrderType)
                    {
                        proorder = products.GetOrderBy(d => d.OrderByDescending(o => o.SaleCounts));
                    }
                    else
                    {
                        proorder = products.GetOrderBy(d => d.OrderBy(o => o.SaleCounts));
                    }
                    break;
                default:
                    if (!productQueryModel.OrderType)
                    {
                        proorder = products.GetOrderBy(d => d.OrderByDescending(o => o.Id));
                    }
                    else
                    {
                        proorder = products.GetOrderBy(d => d.OrderBy(o => o.Id));
                    }
                    break;
            }

            //店铺分类
            IEnumerable<long> productIds = new long[] { };
            if (productQueryModel.ShopCategoryId.HasValue)
            {
                productIds = Context.ProductShopCategoryInfo
                    .Where(
                          item => item.ShopCategoryInfo.Id == shopCateogryId ||
                                  item.ShopCategoryInfo.ParentCategoryId == shopCateogryId).Select(item => item.ProductId);
            }

            int total = products.Count();
            products = products.Where(item => (shopCateogryId == 0 || productIds.Contains(item.Id)));
            products = products.GetPage(out total, proorder, productQueryModel.PageNo, productQueryModel.PageSize);

            QueryPageModel<ProductInfo> pageModel = new QueryPageModel<ProductInfo>()
            {
                Total = total,
                Models = products.ToList()
            };
            return pageModel;
        }
        #region 私有方法
        /// <summary>
        /// 周边门店推荐
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private IQueryable<ShopBranchInfo> ToWhere(CommonModel.ShopBranchQuery query)
        {
            IQueryable<ShopBranchInfo> queryable = from p in base.Context.ShopBranchInfo
                                                   where true
                                                   select p;
            if (query.ShopBranchTagId.HasValue && (query.ShopBranchTagId.Value > 0))
            {
                queryable = from p in queryable
                            where p.Himall_ShopBranchInTags.Count<ShopBranchInTagInfo>(x => (x.ShopBranchTagId == query.ShopBranchTagId.Value)) >= 0
                            select p;
            }
            if (query.Id > 0)
            {
                queryable = from p in queryable
                            where p.Id == query.Id
                            select p;
            }
            if (query.ShopId > 0)
            {
                queryable = from p in queryable
                            where p.ShopId == query.ShopId
                            select p;
            }
            if (query.CityId > 0)
            {
                queryable = from p in queryable
                            where p.AddressPath.Contains("," + query.CityId + ",")
                            select p;
            }
            if (query.ProvinceId > 0)
            {
                queryable = from p in queryable
                            where p.AddressPath.Contains(query.ProvinceId + ",")
                            select p;
            }
            if (!string.IsNullOrWhiteSpace(query.ShopBranchName))
            {
                queryable = from e in queryable
                            where e.ShopBranchName.Contains(query.ShopBranchName)
                            select e;
            }
            if (!string.IsNullOrWhiteSpace(query.ContactUser))
            {
                queryable = from e in queryable
                            where e.ContactUser.Contains(query.ContactUser)
                            select e;
            }
            if (!string.IsNullOrWhiteSpace(query.ContactPhone))
            {
                queryable = from e in queryable
                            where e.ContactPhone.Contains(query.ContactPhone)
                            select e;
            }
            if (query.Status.HasValue)
            {
                ShopBranchStatus status = query.Status.Value;
                queryable = from e in queryable
                            where ((int)e.Status) == ((int)status)
                            select e;
            }
            if (!string.IsNullOrWhiteSpace(query.AddressPath))
            {
                string addressPath = query.AddressPath;
                if (!addressPath.EndsWith(","))
                {
                    addressPath = addressPath + ",";
                }
                queryable = from p in queryable
                            where p.AddressPath.StartsWith(addressPath)
                            select p;
            }
            if ((query.ProductIds != null) && (query.ProductIds.Length > 0))
            {
                long[] pids = query.ProductIds.Distinct<long>().ToArray<long>();
                int length = pids.Length;
                IQueryable<long> shopBranchIds = from p in
                                                     from p in base.Context.ShopBranchSkusInfo
                                                     where ((p.ShopId == query.ShopId) && (query.ShopBranchProductStatus.HasValue ? (((int)p.Status) == ((int)query.ShopBranchProductStatus.Value)) : true)) && pids.Contains<long>(p.ProductId)
                                                     group p by new { ShopBranchId = p.ShopBranchId, ProductId = p.ProductId }
                                                 group p by p.Key.ShopBranchId into p
                                                 where p.Count() == length
                                                 select p.Key;
                queryable = from p in queryable
                            where shopBranchIds.Contains<long>(p.Id)
                            select p;
            }
            if (query.IsRecommend.HasValue)
            {
                queryable = from p in queryable
                            where p.IsRecommend == query.IsRecommend.Value
                            select p;
            }
            return queryable;

        }
        #endregion


        public IEnumerable<ShopBranchSkusInfo> SearchShopBranchSkus(long shopBranchId, DateTime? startDate, DateTime? endDate)
        {
            var branchSkus = Context.ShopBranchSkusInfo.Where(e => e.ShopBranchId == shopBranchId);
            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                branchSkus = branchSkus.Where(e => e.CreateDate >= start);
            }
            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1);
                branchSkus = branchSkus.Where(e => e.CreateDate < end);
            }
            return branchSkus.ToList();

        }
        /// <summary>
        /// 周边门店匹配查询条件
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private IQueryable<ShopBranchInfo> ToNearShopWhere(CommonModel.ShopBranchQuery query)
        {
            var shopBranchs = Context.ShopBranchInfo.Where(p => p.Longitude > 0 && p.Latitude > 0);//周边门店只取定位了经纬度的数据
            if (query.ShopId > 0)//取商家下的门店数据
            {
                shopBranchs = shopBranchs.Where(p => p.ShopId == query.ShopId);
            }
            if (query.CityId > 0)//同城门店
            {
                shopBranchs = shopBranchs.Where(p => p.AddressPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + query.CityId + CommonConst.ADDRESS_PATH_SPLIT));
            }
            if (query.Status.HasValue)
            {
                shopBranchs = shopBranchs.Where(e => e.Status == query.Status.Value);
            }
            return shopBranchs;
        }

        /// <summary>
        /// 获取门店配送范围在同一区域的正常门店
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        //private IQueryable<ShopBranchInfo> ToArealShopWhere(int areaId, int shopId)
        //{
        //    var shopBranchs = Context.ShopBranchInfo.Where(p => p.Status == CommonModel.ShopBranchStatus.Normal);
        //    if (shopId > 0)
        //    {
        //        shopBranchs = shopBranchs.Where(p => p.ShopId == shopId);//过滤出商家下门店
        //    }
        //    if (areaId > 0)
        //    {
        //        var shopBranchIds = this.Context.DeliveryScopeInfo.Where(p => p.FullRegionPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + areaId + CommonConst.ADDRESS_PATH_SPLIT))
        //            .GroupBy(p => new { p.ShopBranchId })
        //            .GroupBy(p => p.Key.ShopBranchId)
        //            .Select(p => p.Key);
        //        shopBranchs = shopBranchs.Where(p => shopBranchIds.Contains(p.Id));
        //    }
        //    return shopBranchs;
        //}
        public IEnumerable<ShopBranchInfo> GetShopBranchByShopId(long shopId)
        {
            return Context.ShopBranchInfo.Where(e => e.ShopId == shopId);
        }
        /// <summary>
        /// 分页查询门店配送范围
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        //public QueryPageModel<DeliveryScopeInfo> GetShopDeliveryScope(ShopDeliveryScopeQuery query)
        //{
        //    int total = 0;
        //    var shopDeliveryScope = ToWhereDeliveryScope(query);
        //    shopDeliveryScope = shopDeliveryScope.GetPage(out total, query.PageNo, query.PageSize, s => s.OrderBy(e => e.Id));
        //    return new QueryPageModel<DeliveryScopeInfo>() { Models = shopDeliveryScope.ToList(), Total = total };
        //}
        //public QueryPageModel<DeliveryScopeInfo> GetShopDeliveryScopeAll(ShopDeliveryScopeQuery query)
        //{
        //    return new QueryPageModel<DeliveryScopeInfo>() { Models = ToWhereDeliveryScope(query).ToList() };
        //}
        //private IQueryable<DeliveryScopeInfo> ToWhereDeliveryScope(ShopDeliveryScopeQuery query)
        //{
        //    var shopBranchDeliveryScope = Context.DeliveryScopeInfo.Where(p => true);
        //    if (query.ShopBranchId > 0)
        //    {
        //        shopBranchDeliveryScope = Context.DeliveryScopeInfo.Where(p => p.ShopBranchId == query.ShopBranchId);
        //    }
        //    return shopBranchDeliveryScope;
        //}
        /// <summary>
        /// 批量新增门店配送范围
        /// </summary>
        /// <param name="deliveryScopeInfoList"></param>
        //public void AddShopDeliveryScope(IEnumerable<DeliveryScopeInfo> deliveryScopeInfoList)
        //{
        //    Context.DeliveryScopeInfo.AddRange(deliveryScopeInfoList);
        //    Context.SaveChanges();
        //}
        /// <summary>
        /// 更新门店配送范围
        /// </summary>
        /// <param name="deliveryScopeInfo"></param>
        //public void UpdateShopDeliveryScope(DeliveryScopeInfo deliveryScopeInfo)
        //{
        //    var deliveryScopeEntity = Context.DeliveryScopeInfo.FirstOrDefault(e => e.Id == deliveryScopeInfo.Id);
        //    if (deliveryScopeEntity == null)
        //        throw new Himall.Core.HimallException("数据异常，更新失败！");
        //    deliveryScopeEntity.RegionId = deliveryScopeInfo.RegionId;
        //    deliveryScopeEntity.RegionName = deliveryScopeInfo.RegionName;
        //    deliveryScopeEntity.FullRegionPath = deliveryScopeInfo.FullRegionPath;
        //    Context.SaveChanges();
        //}
        /// <summary>
        /// 根据条件删除门店配送范围
        /// </summary>
        /// <param name="query"></param>
        //public void DeleteShopDeliveryScope(ShopDeliveryScopeQuery query)
        //{
        //    var deliveryScope = Context.DeliveryScopeInfo.Where(e => e.ShopBranchId == query.ShopBranchId);
        //    if (deliveryScope == null)
        //        throw new Himall.Core.HimallException("未找到门店配送范围，删除失败！");
        //    if (query.RegionIdList != null && query.RegionIdList.Count > 0)
        //    {
        //        string deleteIds = string.Join(",", query.RegionIdList.ToArray());
        //        deliveryScope = deliveryScope.Where(e => !deleteIds.Contains(e.RegionId.ToString()));
        //    }
        //    Context.DeliveryScopeInfo.RemoveRange(deliveryScope);
        //    Context.SaveChanges();
        //}
        /// <summary>
        /// 同一门店是否存在相同的区域标识
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        //public bool ExistsShopDeliveryScope(ShopDeliveryScopeQuery query)
        //{
        //    return Context.DeliveryScopeInfo.Any(e => e.ShopBranchId == query.ShopBranchId && e.RegionId == query.RegionId);
        //}
        /// <summary>
        /// 订单自动分配到门店
        /// </summary>
        /// <param name="query"></param>
        /// <param name="skuIds">订单内商品SkuId</param>
        /// <param name="counts">订单内商品购买数量</param>
        /// <returns></returns>
        //public ShopBranchInfo GetAutoMatchShopBranch(ShopBranchQuery query, string[] skuIds, int[] counts)
        //{
        //    ShopBranchInfo resultObj = null;
        //    var skuInfos = Context.SKUInfo.Where(p => skuIds.Contains(p.Id)).ToList();
        //    query.ProductIds = skuInfos.Select(p => p.ProductId).ToArray();
        //    query.Status = CommonModel.ShopBranchStatus.Normal;
        //    var data = GetShopBranchsAll(query);//获取商家下的有该产品SKU的状态正常门店

        //    var shopBranchSkus = GetSkus(query.ShopId, data.Models.Select(p => p.Id));//获取当前商家下门店的SKU
        //    data.Models.ForEach(p =>
        //        p.Enabled = skuInfos.All(skuInfo => shopBranchSkus.Any(sbSku => sbSku.ShopBranchId == p.Id && sbSku.Stock >= counts[skuInfos.IndexOf(skuInfo)] && sbSku.SkuId == skuInfo.Id))
        //    );

        //    var result = data.Models.Where(p => p.Enabled).ToList();//只取商家下都有该商品SKU库存的门店数据

        //    bool fromLatLng = false;//用户收货地址是否定位了经纬度
        //    if (!string.IsNullOrWhiteSpace(query.FromLatLng))
        //    {
        //        fromLatLng = query.FromLatLng.Split(',').Length == 2;
        //    }

        //    if (result.Count > 0)
        //    {
        //        if (fromLatLng)//优先用服务半径匹配,取距离最近的、又有库存的门店。前提要收货地址定位了经纬度
        //            resultObj = result.Where(p => p.Latitude > 0 && p.Longitude > 0 && p.ServeRadius >= p.Distance).OrderBy(p => p.Distance).Take(1).FirstOrDefault<ShopBranchInfo>();

        //        if (resultObj == null)//如果服务半径无法满足，则根据配送范围去匹配
        //        {
        //            var deliveryScope = GetShopDeliveryScopeAll(new ShopDeliveryScopeQuery() { });
        //            if (query.StreetId > 0) //优先筛选出与买家收货地址同街道的所有门店
        //            {
        //                List<long> shopBrachIds = deliveryScope.Models.Where(p => p.FullRegionPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + query.StreetId + CommonConst.ADDRESS_PATH_SPLIT)).Select(p => p.ShopBranchId).Distinct().ToList();
        //                if (shopBrachIds.Count > 0)
        //                {
        //                    if (shopBrachIds.Count > 1)//如果匹配多个则取距离最近一个
        //                    {
        //                        if (fromLatLng)
        //                            resultObj = result.Where(p => p.Latitude > 0 && p.Longitude > 0 && shopBrachIds.Contains(p.Id)).OrderBy(p => p.Distance).Take(1).FirstOrDefault<ShopBranchInfo>();
        //                    }
        //                    else
        //                    {
        //                        resultObj = result.Where(p => shopBrachIds.Contains(p.Id)).FirstOrDefault<ShopBranchInfo>();
        //                    }
        //                }
        //            }
        //            if (resultObj == null)//如果街道无法满足，则根据区县匹配
        //            {
        //                if (query.DistrictId > 0)
        //                {
        //                    List<long> shopBrachIds = deliveryScope.Models.Where(p => p.FullRegionPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + query.DistrictId + CommonConst.ADDRESS_PATH_SPLIT)).Select(p => p.ShopBranchId).Distinct().ToList();
        //                    if (shopBrachIds.Count > 0)
        //                    {
        //                        if (shopBrachIds.Count > 1)//如果匹配多个则取距离最近一个
        //                        {
        //                            if (fromLatLng)
        //                                resultObj = result.Where(p => p.Latitude > 0 && p.Longitude > 0 && shopBrachIds.Contains(p.Id)).OrderBy(p => p.Distance).Take(1).FirstOrDefault<ShopBranchInfo>();
        //                        }
        //                        else
        //                        {
        //                            resultObj = result.Where(p => shopBrachIds.Contains(p.Id)).FirstOrDefault<ShopBranchInfo>();
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return resultObj;
        //}

        #region 组合Sql
        private string GetSearchWhere(ShopBranchQuery query, DynamicParameters parms)
        {
            StringBuilder where = new StringBuilder();
            where.Append(" where Longitude>0 and Latitude>0 ");//周边门店只取定位了经纬度的数据

            if (query.ShopId > 0)//取商家下的门店数据
            {
                where.Append(" and ShopId=@ShopId ");
                parms.Add("@ShopId", query.ShopId);
            }
            if (query.CityId > 0)//同城门店
            {
                where.Append(" and AddressPath like concat('%',@AddressPath,'%') ");
                parms.Add("@AddressPath", CommonConst.ADDRESS_PATH_SPLIT + query.CityId + CommonConst.ADDRESS_PATH_SPLIT);
            }
            if (query.Status.HasValue)
            {
                where.Append(" and Status=@Status ");
                parms.Add("@Status", query.Status.Value);
            }

            return where.ToString();
        }
        /// <summary>
        /// 获取搜索排序sql
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private string GetSearchOrder(ShopBranchQuery query)
        {
            string order = string.Empty;
            switch (query.OrderKey)
            {
                case 2:
                    order = " ORDER BY Distance ";
                    break;

                default:
                    order = " ORDER BY Id ";
                    break;
            }
            if (!query.OrderType)
                order += " DESC ";
            else
                order += " ASC ";

            return order;
        }
        private string GetSearchPage(ShopBranchQuery query)
        {
            return string.Format(" LIMIT {0},{1} ", (query.PageNo - 1) * query.PageSize, query.PageSize);
        }
        #endregion


        /// <summary>
        /// 获取代理商品的门店编号集
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public List<long> GetAgentShopBranchIds(long productId)
        {
            List<long> result = new List<long>();
            result = Context.ShopBranchSkusInfo.Where(d => d.ProductId == productId).Select(d => d.ShopBranchId).Distinct().ToList();
            return result;
        }

        /// <summary>
        /// 判断产品是否存在
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public bool CheckProductIsExist(long shopBranchId, long productId)
        {
            return Queryable.Count<ShopBranchSkusInfo>((IQueryable<ShopBranchSkusInfo>)this.Context.ShopBranchSkusInfo, (Expression<Func<ShopBranchSkusInfo, bool>>)(e => e.ShopBranchId == shopBranchId && e.ProductId == productId)) > 0;
        }
        /// <summary>
        /// 查询附近门店
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public QueryPageModel<ShopBranchInfo> SearchNearShopBranchs(ShopBranchQuery search)
        {
            Decimal num1 = new Decimal(0);
            Decimal num2 = new Decimal(0);
            if (search.FromLatLng.Split(',').Length != 2)
                return new QueryPageModel<ShopBranchInfo>();
            Decimal num3 = TypeHelper.ObjectToDecimal((object)search.FromLatLng.Split(',')[0]);
            Decimal num4 = TypeHelper.ObjectToDecimal((object)search.FromLatLng.Split(',')[1]);
            QueryPageModel<ShopBranchInfo> queryPageModel = new QueryPageModel<ShopBranchInfo>();
            DynamicParameters parms = new DynamicParameters();
            string format = "SELECT COUNT(0) FROM (select 1 from himall_shopbranch sb left JOIN Himall_ShopBranchInTags tag ON sb.Id = tag.ShopBranchId LEFT JOIN Himall_ShopBranchSkus sbs ON sb.Id = sbs.ShopBranchId LEFT JOIN Himall_Products p ON sbs.ProductId = p.Id {0}) a ";
            string str1 = string.Format("select AddressDetail,AddressPath,ContactPhone,sb.Id,ShopBranchName,sb.Status,Latitude,Longitude,ServeRadius,truncate(6378.137*2*ASIN(SQRT(POW(SIN(({0}*PI()/180.0-Latitude*PI()/180.0)/2),2)+COS({0}*PI()/180.0)*COS(Latitude*PI()/180.0)*POW(SIN(({1}*PI()/180.0-Longitude*PI()/180.0)/2),2))),2) AS Distance,DeliveFee,DeliveTotalFee,IsStoreDelive,IsAboveSelf,sb.ShopId,ShopImages,IsRecommend,RecommendSequence,FreeMailFee from himall_shopbranch sb LEFT JOIN Himall_ShopBranchSkus sbs ON sb.Id = sbs.ShopBranchId LEFT JOIN Himall_Products p ON sbs.ProductId = p.Id left JOIN Himall_ShopBranchInTags tag ON sb.Id = tag.ShopBranchId ", (object)num3, (object)num4);
            string whereForSearch = this.GetWhereForSearch(search, parms);
            string str2 = this.GetSearchOrder(search);
            if (search.OrderKey == 2)
                str2 = string.Format(" {0}, sb.id desc ", (object)str2.TrimEnd(','));
            string searchPage = this.GetSearchPage(search);
            using (MySqlConnection mySqlConnection = new MySqlConnection(Connection.ConnectionString))
            {
                queryPageModel.Models = Enumerable.ToList<ShopBranchInfo>(SqlMapper.Query<ShopBranchInfo>((IDbConnection)mySqlConnection, str1 + whereForSearch + str2 + searchPage, (object)parms, (IDbTransaction)null, true, new int?(), new CommandType?()));
                queryPageModel.Total = int.Parse(SqlMapper.ExecuteScalar((IDbConnection)mySqlConnection, string.Format(format, (object)whereForSearch), (object)parms, (IDbTransaction)null, new int?(), new CommandType?()).ToString());
            }
            return queryPageModel;
        }
        /// <summary>
        /// 附近门店标签查询
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public QueryPageModel<ShopBranchInfo> TagsSearchNearShopBranchs(ShopBranchQuery search)
        {
            Decimal num1 = new Decimal(0);
            Decimal num2 = new Decimal(0);
            if (search.FromLatLng.Split(',').Length != 2)
                return new QueryPageModel<ShopBranchInfo>();
            Decimal num3 = TypeHelper.ObjectToDecimal((object)search.FromLatLng.Split(',')[0]);
            Decimal num4 = TypeHelper.ObjectToDecimal((object)search.FromLatLng.Split(',')[1]);
            QueryPageModel<ShopBranchInfo> queryPageModel = new QueryPageModel<ShopBranchInfo>();
            DynamicParameters parms = new DynamicParameters();
            string format = "SELECT COUNT(0) FROM (select 1 from himall_shopbranch sb INNER JOIN Himall_ShopBranchInTags tag ON sb.Id = tag.ShopBranchId LEFT JOIN Himall_ShopBranchSkus sbs ON sb.Id = sbs.ShopBranchId {0}) a ";
            string str1 = string.Format("select AddressDetail,AddressPath,ContactPhone,sb.Id,ShopBranchName,sb.Status,Latitude,Longitude,ServeRadius,truncate(6378.137*2*ASIN(SQRT(POW(SIN(({0}*PI()/180.0-Latitude*PI()/180.0)/2),2)+COS({0}*PI()/180.0)*COS(Latitude*PI()/180.0)*POW(SIN(({1}*PI()/180.0-Longitude*PI()/180.0)/2),2))),2) AS Distance,DeliveFee,DeliveTotalFee,IsStoreDelive,IsAboveSelf,sb.ShopId,ShopImages,IsRecommend,RecommendSequence,FreeMailFee from himall_shopbranch sb INNER JOIN Himall_ShopBranchInTags tag ON sb.Id = tag.ShopBranchId LEFT JOIN Himall_ShopBranchSkus sbs ON sb.Id = sbs.ShopBranchId ", (object)num3, (object)num4);
            string whereForSearch = this.GetWhereForSearch(search, parms);
            string str2 = this.GetSearchOrder(search);
            if (search.OrderKey == 2)
                str2 = string.Format(" {0}, sb.id desc ", (object)str2.TrimEnd(','));
            string searchPage = this.GetSearchPage(search);
            using (MySqlConnection mySqlConnection = new MySqlConnection(Connection.ConnectionString))
            {
                queryPageModel.Models = Enumerable.ToList<ShopBranchInfo>(SqlMapper.Query<ShopBranchInfo>((IDbConnection)mySqlConnection, str1 + whereForSearch + str2 + searchPage, (object)parms, (IDbTransaction)null, true, new int?(), new CommandType?()));
                queryPageModel.Total = int.Parse(SqlMapper.ExecuteScalar((IDbConnection)mySqlConnection, string.Format(format, (object)whereForSearch), (object)parms, (IDbTransaction)null, new int?(), new CommandType?()).ToString());
            }
            return queryPageModel;
        }
        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private string GetStoreByProductOrder(ShopBranchQuery query)
        {
            StringBuilder stringBuilder = new StringBuilder(" ORDER BY  ");
            if (query.OrderKey == 2)
                stringBuilder.Append(" Distance ");
            else
                stringBuilder.Append(" sb.Id ");
            if (!query.OrderType)
                stringBuilder.Append(" DESC ");
            else
                stringBuilder.Append(" ASC ");
            return stringBuilder.ToString();
        }
        /// <summary>
        /// Where条件查询
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        private string GetWhereForSearch(ShopBranchQuery query, DynamicParameters parms)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(" where Longitude>0 and Latitude>0 ");
            if (query.ShopId > 0L)
            {
                stringBuilder.Append(" and sb.ShopId=@ShopId ");
                parms.Add("@ShopId", (object)query.ShopId, new DbType?(), new ParameterDirection?(), new int?(), new byte?(), new byte?());
            }
            if (query.CityId > 0)
            {
                stringBuilder.Append(" and AddressPath like concat('%',@AddressPath,'%') ");
                parms.Add("@AddressPath", (object)("," + (object)query.CityId + ","), new DbType?(), new ParameterDirection?(), new int?(), new byte?(), new byte?());
            }
            if (query.Status.HasValue)
            {
                stringBuilder.Append(" and sb.Status=@Status ");
                parms.Add("@Status", (object)query.Status.Value, new DbType?(), new ParameterDirection?(), new int?(), new byte?(), new byte?());
            }
            if (!string.IsNullOrEmpty(query.ShopBranchName))
            {
                stringBuilder.Append(" and (ShopBranchName LIKE concat('%',@KeyWords,'%') OR (ProductName LIKE concat('%',@KeyWords,'%') ");
                stringBuilder.Append("  AND sbs.`Status`= " + (object)ShopBranchSkuStatus.Normal.GetHashCode());
                stringBuilder.Append(" )) ");
                parms.Add("@KeyWords", (object)query.ShopBranchName, new DbType?(), new ParameterDirection?(), new int?(), new byte?(), new byte?());
            }
            long? shopBranchTagId = query.ShopBranchTagId;
            int num;
            if (shopBranchTagId.HasValue)
            {
                shopBranchTagId = query.ShopBranchTagId;
                num = shopBranchTagId.Value <= 0L ? 1 : 0;
            }
            else
                num = 1;
            if (num == 0)
            {
                stringBuilder.Append(" and tag.ShopBranchTagId = @ShopBranchTagId ");
                DynamicParameters dynamicParameters = parms;
                string name = "@ShopBranchTagId";
                shopBranchTagId = query.ShopBranchTagId;
                var local = (System.ValueType)shopBranchTagId.Value;
                DbType? dbType = new DbType?();
                ParameterDirection? direction = new ParameterDirection?();
                int? size = new int?();
                byte? precision = new byte?();
                byte? scale = new byte?();
                dynamicParameters.Add(name, (object)local, dbType, direction, size, precision, scale);
            }
            if (query.ProductIds != null && query.ProductIds.Length > 0)
            {
                stringBuilder.Append(" and ProductId in (@ProductIds) ");
                parms.Add("@ProductIds", (object)string.Join<long>(",", (IEnumerable<long>)query.ProductIds), new DbType?(), new ParameterDirection?(), new int?(), new byte?(), new byte?());
            }
            if (query.ShopBranchProductStatus.HasValue)
            {
                stringBuilder.Append(" and sbs.Status = @ShopBranchProductStatus ");
                parms.Add("@ShopBranchProductStatus", (object)query.ShopBranchProductStatus.Value, new DbType?(), new ParameterDirection?(), new int?(), new byte?(), new byte?());
            }
            stringBuilder.Append(" GROUP BY sb.Id ");
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 附近门店
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public QueryPageModel<ShopBranchInfo> StoreByProductNearShopBranchs(ShopBranchQuery search)
        {
            Decimal num1 = new Decimal(0);
            Decimal num2 = new Decimal(0);
            if (search.FromLatLng.Split(',').Length != 2)
                return new QueryPageModel<ShopBranchInfo>();
            Decimal num3 = TypeHelper.ObjectToDecimal((object)search.FromLatLng.Split(',')[0]);
            Decimal num4 = TypeHelper.ObjectToDecimal((object)search.FromLatLng.Split(',')[1]);
            QueryPageModel<ShopBranchInfo> queryPageModel = new QueryPageModel<ShopBranchInfo>();
            DynamicParameters parms = new DynamicParameters();
            string format = "SELECT COUNT(0) FROM (select 1 from himall_shopbranch sb left JOIN Himall_ShopBranchInTags tag ON sb.Id = tag.ShopBranchId LEFT JOIN Himall_ShopBranchSkus sbs ON sb.Id = sbs.ShopBranchId LEFT JOIN Himall_Products p ON sbs.ProductId = p.Id {0}) a ";
            string str1 = string.Format("select AddressDetail,AddressPath,ContactPhone,sb.Id,ShopBranchName,sb.Status,Latitude,Longitude,ServeRadius,truncate(6378.137*2*ASIN(SQRT(POW(SIN(({0}*PI()/180.0-Latitude*PI()/180.0)/2),2)+COS({0}*PI()/180.0)*COS(Latitude*PI()/180.0)*POW(SIN(({1}*PI()/180.0-Longitude*PI()/180.0)/2),2))),2) AS Distance,DeliveFee,DeliveTotalFee,IsStoreDelive,IsAboveSelf,sb.ShopId,ShopImages,IsRecommend,RecommendSequence,FreeMailFee from himall_shopbranch sb LEFT JOIN Himall_ShopBranchSkus sbs ON sb.Id = sbs.ShopBranchId LEFT JOIN Himall_Products p ON sbs.ProductId = p.Id left JOIN Himall_ShopBranchInTags tag ON sb.Id = tag.ShopBranchId ", (object)num3, (object)num4);
            string whereForSearch = this.GetWhereForSearch(search, parms);
            string str2 = this.GetStoreByProductOrder(search);
            if (search.OrderKey == 2)
                str2 = string.Format(" {0}, sb.id desc ", (object)str2.TrimEnd(','));
            string searchPage = this.GetSearchPage(search);
            using (MySqlConnection mySqlConnection = new MySqlConnection(Connection.ConnectionString))
            {
                queryPageModel.Models = Enumerable.ToList<ShopBranchInfo>(SqlMapper.Query<ShopBranchInfo>((IDbConnection)mySqlConnection, str1 + whereForSearch + str2 + searchPage, (object)parms, (IDbTransaction)null, true, new int?(), new CommandType?()));
                queryPageModel.Total = int.Parse(SqlMapper.ExecuteScalar((IDbConnection)mySqlConnection, string.Format(format, (object)whereForSearch), (object)parms, (IDbTransaction)null, new int?(), new CommandType?()).ToString());
            }
            return queryPageModel;
        }
        /// <summary>
        /// 计算距离API
        /// </summary>
        /// <param name="fromLatLng"></param>
        /// <param name="latlng"></param>
        /// <returns></returns>
        double IShopBranchService.GetLatLngDistancesFromAPI(string fromLatLng, string latlng)
        {
            if (string.IsNullOrWhiteSpace(fromLatLng) || string.IsNullOrWhiteSpace(latlng))
                return 0.0;
            try
            {
                string[] strArray1 = fromLatLng.Split(',');
                double num1 = double.Parse(strArray1[0]);
                double num2 = double.Parse(strArray1[1]);
                double num3 = 6378.137;
                string[] strArray2 = latlng.Split(',');
                double num4 = double.Parse(strArray2[0]);
                double num5 = double.Parse(strArray2[1]);
                double d1 = num1 * Math.PI / 180.0;
                double d2 = num4 * Math.PI / 180.0;
                double num6 = d1 - d2;
                double num7 = num2 * Math.PI / 180.0 - num5 * Math.PI / 180.0;
                return Math.Round(Math.Round(2.0 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(num6 / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num7 / 2.0), 2.0))) * num3 * 10000.0) / 10000.0, 2);
            }
            catch (Exception ex)
            {
                Log.Error((object)"计算经纬度距离异常", ex);
                return 0.0;
            }
        }
        /// <summary>
        /// 不分页查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<ShopBranchInfo> GetShopBranchsNoPager(ShopBranchQuery query)
        {
            return this.ToWhere(query).ToList<ShopBranchInfo>();
        }
        /// <summary>
        /// Where条件拼接
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="shopId"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        private IEnumerable<ShopBranchInfo> ToArealShopWhere(int areaId, int shopId, float latitude, float longitude)
        {
            string str1 = "SELECT Id,ShopId,ShopBranchName,AddressId,AddressPath,AddressDetail,ContactUser,ContactPhone,Status,CreateDate,ServeRadius,Longitude,Latitude,ShopImages,IsStoreDelive,IsAboveSelf,StoreOpenStartTime,StoreOpenEndTime,IsRecommend,RecommendSequence,DeliveFee,DeliveTotalFee,FreeMailFee,truncate(6378.137*2*ASIN(SQRT(POW(SIN(({0}*PI()/180.0-Latitude*PI()/180.0)/2),2)+COS({1}*PI()/180.0)*COS(Latitude*PI()/180.0)*POW(SIN(({2}*PI()/180.0-Longitude*PI()/180.0)/2),2))),2) AS Distance FROM Himall_ShopBranch WHERE Status = 0 AND Longitude> 0 AND Latitude > 0 ";
            StringBuilder stringBuilder = new StringBuilder();
            if (shopId > 0)
                stringBuilder.Append(" AND ShopId = {3} ");
            if (areaId > 0)
                stringBuilder.Append(" AND AddressPath LIKE '%,{4},%' ");
            if ((double)latitude > 0.0 && (double)longitude > 0.0)
                stringBuilder.Append(" AND ServeRadius > truncate(6378.137*2*ASIN(SQRT(POW(SIN(({0}*PI()/180.0-Latitude*PI()/180.0)/2),2)+COS({1}*PI()/180.0)*COS(Latitude*PI()/180.0)*POW(SIN(({2}*PI()/180.0-Longitude*PI()/180.0)/2),2))),2) ");
            string str2 = " ORDER BY Distance ";
            return (IEnumerable<ShopBranchInfo>)this.Context.Database.SqlQuery<ShopBranchInfo>(string.Format(str1 + stringBuilder.ToString() + str2, (object)latitude, (object)latitude, (object)longitude, (object)shopId, (object)areaId));
        }
        public QueryPageModel<ShopBranchInfo> GetArealShopBranchsAll(int areaId, int shopId, float latitude, float longitude)
        {
            return new QueryPageModel<ShopBranchInfo>()
            {
                Models = Enumerable.ToList<ShopBranchInfo>(this.ToArealShopWhere(areaId, shopId, latitude, longitude))
            };
        }
        /// <summary>
        /// 自动匹配
        /// </summary>
        /// <param name="query"></param>
        /// <param name="skuIds"></param>
        /// <param name="counts"></param>
        /// <returns></returns>
        public ShopBranchInfo GetAutoMatchShopBranch(ShopBranchQuery query, string[] skuIds, int[] counts)
        {
            ShopBranchInfo shopBranchInfo = (ShopBranchInfo)null;
            List<SKUInfo> skuInfos = Enumerable.ToList<SKUInfo>((IEnumerable<SKUInfo>)Queryable.Where<SKUInfo>((IQueryable<SKUInfo>)this.Context.SKUInfo, (Expression<Func<SKUInfo, bool>>)(p => Enumerable.Contains<string>(skuIds, p.Id))));
            query.ProductIds = Enumerable.ToArray<long>(Enumerable.Select<SKUInfo, long>((IEnumerable<SKUInfo>)skuInfos, (Func<SKUInfo, long>)(p => p.ProductId)));
            query.Status = new ShopBranchStatus?(ShopBranchStatus.Normal);
            QueryPageModel<ShopBranchInfo> shopBranchsAll = this.GetShopBranchsAll(query);
            List<ShopBranchSkusInfo> shopBranchSkus = this.GetSkus(query.ShopId, Enumerable.Select<ShopBranchInfo, long>((IEnumerable<ShopBranchInfo>)shopBranchsAll.Models, (Func<ShopBranchInfo, long>)(p => p.Id)), new ShopBranchSkuStatus?(ShopBranchSkuStatus.Normal));
            shopBranchsAll.Models.ForEach((Action<ShopBranchInfo>)(p => p.Enabled = Enumerable.All<SKUInfo>((IEnumerable<SKUInfo>)skuInfos, (Func<SKUInfo, bool>)(skuInfo => Enumerable.Any<ShopBranchSkusInfo>((IEnumerable<ShopBranchSkusInfo>)shopBranchSkus, (Func<ShopBranchSkusInfo, bool>)(sbSku => sbSku.ShopBranchId == p.Id && sbSku.Stock >= counts[skuInfos.IndexOf(skuInfo)] && sbSku.SkuId == skuInfo.Id))))));
            List<ShopBranchInfo> list = Enumerable.ToList<ShopBranchInfo>(Enumerable.Where<ShopBranchInfo>((IEnumerable<ShopBranchInfo>)shopBranchsAll.Models, (Func<ShopBranchInfo, bool>)(p => p.Enabled)));
            bool flag = false;
            if (!string.IsNullOrWhiteSpace(query.FromLatLng))
                flag = query.FromLatLng.Split(',').Length == 2;
            if (list.Count > 0 && flag)
                shopBranchInfo = Enumerable.FirstOrDefault<ShopBranchInfo>(Enumerable.Take<ShopBranchInfo>((IEnumerable<ShopBranchInfo>)Enumerable.OrderBy<ShopBranchInfo, double>(Enumerable.Where<ShopBranchInfo>((IEnumerable<ShopBranchInfo>)list, (Func<ShopBranchInfo, bool>)(p =>
                {
                    float? latitude = p.Latitude;
                    int num;
                    if (((double)latitude.GetValueOrDefault() <= 0.0 ? 0 : (latitude.HasValue ? 1 : 0)) != 0)
                    {
                        float? longitude = p.Longitude;
                        if (((double)longitude.GetValueOrDefault() <= 0.0 ? 0 : (longitude.HasValue ? 1 : 0)) != 0)
                        {
                            int? serveRadius = p.ServeRadius;
                            double distance = p.Distance;
                            num = (double)serveRadius.GetValueOrDefault() < distance ? 0 : (serveRadius.HasValue ? 1 : 0);
                            goto label_4;
                        }
                    }
                    num = 0;
                label_4:
                    return num != 0;
                })), (Func<ShopBranchInfo, double>)(p => p.Distance)), 1));
            return shopBranchInfo;
        }
        /// <summary>
        /// 推荐门店
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool RecommendShopBranch(long[] ids)
        {
            List<ShopBranchInfo> list = Enumerable.ToList<ShopBranchInfo>((IEnumerable<ShopBranchInfo>)Queryable.Where<ShopBranchInfo>((IQueryable<ShopBranchInfo>)this.Context.ShopBranchInfo, (Expression<Func<ShopBranchInfo, bool>>)(n => Enumerable.Contains<long>(ids, n.Id))));
            long index = Queryable.Max<ShopBranchInfo, long>((IQueryable<ShopBranchInfo>)this.Context.ShopBranchInfo, (Expression<Func<ShopBranchInfo, long>>)(n => n.RecommendSequence));
            list.ForEach((Action<ShopBranchInfo>)(n =>
            {
                n.IsRecommend = true;
                n.RecommendSequence = n.RecommendSequence > 0L ? n.RecommendSequence : ++index;
            }));
            return this.Context.SaveChanges() > 0;
        }
        /// <summary>
        /// 修改推荐
        /// </summary>
        /// <param name="oriShopBranchId"></param>
        /// <param name="newShopBranchId"></param>
        /// <returns></returns>
        public bool RecommendChangeSequence(long oriShopBranchId, long newShopBranchId)
        {
            ShopBranchInfo shopBranchInfo1 = Queryable.FirstOrDefault<ShopBranchInfo>((IQueryable<ShopBranchInfo>)this.Context.ShopBranchInfo, (Expression<Func<ShopBranchInfo, bool>>)(n => n.Id == oriShopBranchId));
            ShopBranchInfo shopBranchInfo2 = Queryable.FirstOrDefault<ShopBranchInfo>((IQueryable<ShopBranchInfo>)this.Context.ShopBranchInfo, (Expression<Func<ShopBranchInfo, bool>>)(n => n.Id == newShopBranchId));
            if (shopBranchInfo1 == null || null == shopBranchInfo2)
                return false;
            long recommendSequence1 = shopBranchInfo1.RecommendSequence;
            long recommendSequence2 = shopBranchInfo2.RecommendSequence;
            shopBranchInfo1.RecommendSequence = recommendSequence2;
            shopBranchInfo2.RecommendSequence = recommendSequence1;
            return this.Context.SaveChanges() > 0;
        }
        /// <summary>
        /// 重置门店标签
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public bool ResetShopBranchRecommend(long shopBranchId)
        {
            ShopBranchInfo info = base.Context.ShopBranchInfo.FirstOrDefault<ShopBranchInfo>(n => n.Id == shopBranchId);
            if (null == info)
            {
                return false;
            }
            info.IsRecommend = false;
            info.RecommendSequence = 0;
            return (base.Context.SaveChanges() > 0);

        }
        /// <summary>
        /// 读取所有门店标签
        /// </summary>
        /// <returns></returns>
        public List<ShopBranchTagInfo> GetAllShopBranchTagInfo()
        {
            return Enumerable.ToList<ShopBranchTagInfo>((IEnumerable<ShopBranchTagInfo>)DbSetExtend.FindAll<ShopBranchTagInfo>(this.Context.ShopBranchTagInfo));
        }
        /// <summary>
        /// 添加门店标签
        /// </summary>
        /// <param name="shopBranchTagInfo"></param>
        public void AddShopBranchTagInfo(ShopBranchTagInfo shopBranchTagInfo)
        {
            if (Queryable.FirstOrDefault<ShopBranchTagInfo>((IQueryable<ShopBranchTagInfo>)this.Context.ShopBranchTagInfo, (Expression<Func<ShopBranchTagInfo, bool>>)(x => x.Title == shopBranchTagInfo.Title)) != null)
                throw new Exception("标签名称不可重复");
            this.Context.ShopBranchTagInfo.Add(shopBranchTagInfo);
            this.Context.SaveChanges();
        }
        /// <summary>
        /// 更新门店标签
        /// </summary>
        /// <param name="shopBranchTagInfo"></param>
        /// <returns></returns>
        public bool UpdateShopBranchTagInfo(ShopBranchTagInfo shopBranchTagInfo)
        {
            if (shopBranchTagInfo != null)
            {
                if (Queryable.FirstOrDefault<ShopBranchTagInfo>((IQueryable<ShopBranchTagInfo>)this.Context.ShopBranchTagInfo, (Expression<Func<ShopBranchTagInfo, bool>>)(x => x.Title == shopBranchTagInfo.Title && x.Id != shopBranchTagInfo.Id)) != null)
                    throw new Exception("标签名称不可重复");
                ShopBranchTagInfo shopBranchTagInfo1 = Queryable.FirstOrDefault<ShopBranchTagInfo>((IQueryable<ShopBranchTagInfo>)this.Context.ShopBranchTagInfo, (Expression<Func<ShopBranchTagInfo, bool>>)(x => x.Id == shopBranchTagInfo.Id));
                if (shopBranchTagInfo1 != null)
                {
                    shopBranchTagInfo1.Title = shopBranchTagInfo.Title;
                    return this.Context.SaveChanges() > 0;
                }
            }
            return false;
        }
        /// <summary>
        /// 删除门店标签信息
        /// </summary>
        /// <param name="shopBranchTagInfoId"></param>
        /// <returns></returns>
        public bool DeleteShopBranchTagInfo(long shopBranchTagInfoId)
        {
            if (shopBranchTagInfoId > 0L)
            {
                ShopBranchTagInfo info = Queryable.FirstOrDefault<ShopBranchTagInfo>((IQueryable<ShopBranchTagInfo>)this.Context.ShopBranchTagInfo, (Expression<Func<ShopBranchTagInfo, bool>>)(x => x.Id == shopBranchTagInfoId));
                if (info != null)
                {
                    this.Context.ShopBranchInTagInfo.RemoveRange((IEnumerable<ShopBranchInTagInfo>)Queryable.Where<ShopBranchInTagInfo>((IQueryable<ShopBranchInTagInfo>)this.Context.ShopBranchInTagInfo, (Expression<Func<ShopBranchInTagInfo, bool>>)(d => d.ShopBranchTagId == info.Id)));
                    this.Context.ShopBranchTagInfo.Remove(info);
                    return this.Context.SaveChanges() > 0;
                }
            }
            return false;
        }
        /// <summary>
        /// 读取门店标签名称
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public List<ShopBranchTagInfo> GetShopBranchTagsTitle(long shopBranchId)
        {
            if (shopBranchId <= 0L)
                return (List<ShopBranchTagInfo>)null;
            List<ShopBranchInTagInfo> list = Enumerable.ToList<ShopBranchInTagInfo>((IEnumerable<ShopBranchInTagInfo>)Queryable.Where<ShopBranchInTagInfo>((IQueryable<ShopBranchInTagInfo>)this.Context.ShopBranchInTagInfo, (Expression<Func<ShopBranchInTagInfo, bool>>)(x => x.ShopBranchId == shopBranchId)));
            if (list == null)
                return (List<ShopBranchTagInfo>)null;
            IEnumerable<long> Ids = Enumerable.Select<ShopBranchInTagInfo, long>((IEnumerable<ShopBranchInTagInfo>)list, (Func<ShopBranchInTagInfo, long>)(x => x.ShopBranchTagId));
            return Enumerable.ToList<ShopBranchTagInfo>((IEnumerable<ShopBranchTagInfo>)Queryable.Where<ShopBranchTagInfo>((IQueryable<ShopBranchTagInfo>)this.Context.ShopBranchTagInfo, (Expression<Func<ShopBranchTagInfo, bool>>)(e => Enumerable.Contains<long>(Ids, e.Id))));
        }
        /// <summary>
        /// 添加门店标签
        /// </summary>
        /// <param name="shopBranchIds"></param>
        /// <param name="shopBranchTagIds"></param>
        public void AddShopBranchInTagInfo(long[] shopBranchIds, long[] shopBranchTagIds)
        {
            if (Enumerable.Count<long>((IEnumerable<long>)shopBranchIds) <= 0 || Enumerable.Count<long>((IEnumerable<long>)shopBranchTagIds) <= 0)
                return;
            foreach (long num1 in shopBranchIds)
            {
                foreach (long num2 in shopBranchTagIds)
                {
                    this.Context.ShopBranchInTagInfo.Add(new ShopBranchInTagInfo()
                    {
                        ShopBranchId = num1,
                        ShopBranchTagId = num2
                    });
                    this.Context.SaveChanges();
                }
            }
        }
        /// <summary>
        /// 删除门店标签
        /// </summary>
        /// <param name="shopBranchIds"></param>
        public void DeleteShopBranchInTagInfo(long[] shopBranchIds)
        {
            if (Enumerable.Count<long>((IEnumerable<long>)shopBranchIds) <= 0)
                return;
            this.Context.ShopBranchInTagInfo.RemoveRange((IEnumerable<ShopBranchInTagInfo>)Queryable.Where<ShopBranchInTagInfo>((IQueryable<ShopBranchInTagInfo>)this.Context.ShopBranchInTagInfo, (Expression<Func<ShopBranchInTagInfo, bool>>)(x => Enumerable.Contains<long>(shopBranchIds, x.ShopBranchId))));
            this.Context.SaveChanges();
        }
    }
}
