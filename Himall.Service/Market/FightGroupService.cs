using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Himall.Core;
using Himall.CommonModel;
using Himall.Entity;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Core.Plugins.Message;
using MySql.Data.MySqlClient;
using Dapper;
using Himall.Model.Models;
using Himall.ServiceProvider;

namespace Himall.Service
{
    public class FightGroupService : ServiceBase, IFightGroupService
    {
        #region 拼团活动 

        /*public int AddProduct(ProductInfo p)
        {
            string sql = "INSERT INTO Himall_Products(ShopId,CategoryId,CategoryPath,TypeId,BrandId,ProductName,ProductCode,ShortDescription,SaleStatus,AuditStatus,AddedDate,DisplaySequence,MarketPrice,MinSalePrice,HasSKU,VistiCounts,SaleCounts,FreightTemplateId,EditStatus,IsDeleted,MaxBuyCount) VALUES(@ShopId,@CategoryId,@CategoryPath,@TypeId,@BrandId,@ProductName,@ProductCode,@ShortDescription,@SaleStatus,@AuditStatus,@AddedDate,@DisplaySequence,@MarketPrice,@MinSalePrice,@HasSKU,@VistiCounts,@SaleCounts,@FreightTemplateId,@EditStatus,@IsDeleted,@MaxBuyCount);";
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                var result=conn.Execute(sql, new { ShopId=p.ShopId, CategoryId=p.CategoryId, CategoryPath=p.CategoryId, TypeId=p.TypeId, BrandId=p.BrandId, ProductName=p.ProductName, ProductCode=p.ProductCode, ShortDescription=p.ShortDescription, SaleStatus=p.SaleStatus, AuditStatus=p.AuditStatus, AddedDate=p.AddedDate, DisplaySequence=p.DisplaySequence, MarketPrice=p.MarketPrice, MinSalePrice=p.MinSalePrice, HasSKU=p.HasSKU, VistiCounts=p.VistiCounts, SaleCounts=p.SaleStatus, FreightTemplateId=p.FreightTemplateId, EditStatus=p.EditStatus, IsDeleted=p.IsDeleted, MaxBuyCount=p.MaxBuyCount });
                return result;
            }
        }*/
        /// <summary>
        /// 新增拼团活动
        /// </summary>
        /// <param name="data"></param>
        public void AddActive(FightGroupActiveInfo data)
        {
            this.Context.FightGroupActiveInfo.Add(data);
            this.Commit();
            foreach (FightGroupActiveItemInfo groupActiveItemInfo in data.ActiveItems)
            {
                groupActiveItemInfo.ActiveId = new long?(data.Id);
                groupActiveItemInfo.ProductId = new long?(data.ProductId);
            }
            this.Context.FightGroupActiveItemInfo.AddRange((IEnumerable<FightGroupActiveItemInfo>)data.ActiveItems);
            this.Commit();
        }


        /// <summary>
        /// 商品是否可以参加拼团活动
        /// <para>其他活动限时请在bll层操作</para>
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public bool ProductCanJoinActive(long productId)
        {
            bool result = true;
            var edate = DateTime.Now;
            if (Context.FightGroupActiveInfo.Any(d => d.ProductId == productId && d.EndTime > edate))
            {
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 更新拼团活动
        /// </summary>
        /// <param name="data"></param>
        public void UpdateActive(FightGroupActiveInfo data)
        {
            if (data == null)
                throw new HimallException("错误的拼团活动");
            FightGroupActiveInfo fightGroupActiveInfo = Queryable.FirstOrDefault<FightGroupActiveInfo>((IQueryable<FightGroupActiveInfo>)this.Context.FightGroupActiveInfo, (Expression<Func<FightGroupActiveInfo, bool>>)(d => d.Id == data.Id));
            if (fightGroupActiveInfo == null)
                throw new HimallException("错误的拼团活动");
            fightGroupActiveInfo.IconUrl = data.IconUrl;
            fightGroupActiveInfo.EndTime = data.EndTime;
            fightGroupActiveInfo.LimitedNumber = data.LimitedNumber;
            fightGroupActiveInfo.LimitedHour = data.LimitedHour;
            fightGroupActiveInfo.LimitQuantity = data.LimitQuantity;
            DbSetExtend.Remove<FightGroupActiveItemInfo>(this.Context.FightGroupActiveItemInfo, (Expression<Func<FightGroupActiveItemInfo, bool>>)(d => d.ActiveId == (long?)data.Id));
            foreach (FightGroupActiveItemInfo entity in data.ActiveItems)
            {
                entity.ActiveId = new long?(data.Id);
                entity.ProductId = new long?(data.ProductId);
                this.Context.FightGroupActiveItemInfo.Add(entity);
            }
            this.Commit();
        }
        /// <summary>
        /// 修改活动库存
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="skuId"></param>
        /// <param name="stockChange">库存量 正数补充 负数消耗</param>
        private void UpdateActiveStock(long actionId, string skuId, long stockChange)
        {
            if (string.IsNullOrWhiteSpace(skuId))
            {
                throw new HimallException("错误的规格编号");
            }
            var actitemobj = Context.FightGroupActiveItemInfo.FirstOrDefault(d => d.ActiveId == actionId && d.SkuId == skuId);
            if (actitemobj == null)
            {
                throw new HimallException("错误的规格信息");
            }
            var skuinfo = Context.SKUInfo.FirstOrDefault(d => d.Id == skuId);
            if (skuinfo == null)
            {
                throw new HimallException("错误的规格信息");
            }
            //actitemobj.ActiveStock = actitemobj.ActiveStock < skuinfo.Stock ? actitemobj.ActiveStock : skuinfo.Stock;  //库存无需修正
            if (actitemobj.ActiveStock + stockChange < 0)
            {
                throw new HimallException("库存不足");
            }
            actitemobj.ActiveStock += stockChange;
            int buynum = (int)stockChange;
            actitemobj.BuyCount += buynum;
            if (actitemobj.BuyCount < 0)
            {
                actitemobj.BuyCount = 0;   //零值修正
            }
            Commit();
        }
        /// <summary>
        /// 下架拼团活动
        /// </summary>
        /// <param name="id"></param>
        /// <param name="manageRemark">下架原因</param>
        /// <param name="manageId">管理员编号</param>
        public void CancelActive(long id, string manageRemark, long manageId)
        {
            var data = Context.FightGroupActiveInfo.FirstOrDefault(d => d.Id == id);
            if (data == null)
            {
                throw new HimallException("错误的活动编号");
            }
            if (data.ActiveStatus == FightGroupActiveStatus.Ending)
            {
                throw new HimallException("活动已结束");
            }
            //直接改为过期
            data.EndTime = DateTime.Now.AddDays(-1);
            data.ManageAuditStatus = -1;
            data.ManageRemark = manageRemark;
            data.ManageDate = DateTime.Now;
            data.ManagerId = manageId;
            Commit();
        }

        /// <summary>
        /// 小程序下架拼团活动
        /// </summary>
        /// <param name="id"></param>
        public void XcxCancelActive(long id)
        {
            var data = Context.FightGroupActiveInfo.FirstOrDefault(d => d.Id == id);
            if (data == null)
            {
                throw new HimallException("错误的活动编号");
            }
            if (data.ActiveStatus == FightGroupActiveStatus.Ending)
            {
                throw new HimallException("活动已结束");
            }
            //直接改为过期
            data.EndTime = DateTime.Now.AddDays(-1);
            data.ManageAuditStatus = -1;
            data.ManageDate = DateTime.Now;
            Commit();
        }

        public void DeleteActive(long id)
        {
            var data = Context.FightGroupActiveInfo.FirstOrDefault(d => d.Id == id);
            if (data == null)
            {
                throw new HimallException("错误的活动编号");
            }
            Context.FightGroupActiveInfo.Remove(data);
            Commit();
        }
        /// <summary>
        /// 根据商品ID取活动信息
        /// </summary>
        /// <param name="proId"></param>
        /// <returns></returns>
        public FightGroupActiveInfo GetActiveByProId(long proId)
        {
            FightGroupActiveInfo fightGroupActiveInfo = new FightGroupActiveInfo();
            return Queryable.FirstOrDefault<FightGroupActiveInfo>((IQueryable<FightGroupActiveInfo>)this.Context.FightGroupActiveInfo.AsNoTracking(), (Expression<Func<FightGroupActiveInfo, bool>>)(d => d.ProductId == proId && d.EndTime > (DateTime?)DateTime.Now));
        }
        /// <summary>
        /// 获取拼团活动
        /// </summary>
        /// <param name="id"></param>
        /// <param name="needGetProductCommentNumber">是否需要同步获取商品的评价数量,会自动加载产品信息</param>
        /// <param name="isLoadItems">是否加载节点信息</param>
        /// <param name="isLoadPorductInfo">是否加载产品信息</param>
        /// <returns></returns>
        public FightGroupActiveInfo GetActive(long id, bool needGetProductCommentNumber = false, bool isLoadItems = true, bool isLoadPorductInfo = true)
        {
            FightGroupActiveInfo result = new FightGroupActiveInfo();
            result = base.Context.FightGroupActiveInfo.FirstOrDefault<FightGroupActiveInfo>(d => d.Id == id);
            if (result == null)
            {
                throw new HimallException("错误的活动编号");
            }
            result.HasStock = (from d in base.Context.FightGroupActiveItemInfo
                               where d.ActiveId == id
                               select d).Sum<FightGroupActiveItemInfo>(((Expression<Func<FightGroupActiveItemInfo, long?>>)(d => d.ActiveStock))) > 0L;
            if (needGetProductCommentNumber)
            {
                isLoadPorductInfo = true;
            }
            if (isLoadPorductInfo)
            {
                ProductInfo info = base.Context.ProductInfo.FirstOrDefault<ProductInfo>(d => d.Id == result.ProductId);
                if (info != null)
                {
                    result.ProductImgPath = info.RelativePath;
                    result.FreightTemplateId = info.FreightTemplateId;
                    result.ProductShortDescription = info.ShortDescription;
                    result.ProductCode = info.ProductCode;
                    result.MeasureUnit = info.MeasureUnit;
                    result.MiniSalePrice = info.MinSalePrice;
                    if ((info.AuditStatus == ProductInfo.ProductAuditStatus.Audited) && (info.SaleStatus == ProductInfo.ProductSaleStatus.OnSale))
                    {
                        result.CanBuy = true;
                    }
                    else
                    {
                        result.CanBuy = false;
                    }
                    if (needGetProductCommentNumber)
                    {
                        result.ProductCommentNumber = info.Himall_ProductComments.Count<ProductCommentInfo>(item => !item.IsHidden.HasValue ? true : !item.IsHidden.Value);
                    }
                }
            }
            if (isLoadItems)
            {
                result.ActiveItems = this.GetActiveItems(id);
            }
            return result;

        }
        public List<FightGroupActiveInfo> GetActive(long[] ids)
        {
            var result = new List<FightGroupActiveInfo>();
            var sql = Context.FightGroupActiveInfo.Where(d => ids.Contains(d.Id));
            result = sql.AsNoTracking().ToList();
            foreach (var info in result)
            {
                info.ActiveItems = GetActiveItems(info.Id);
            }
            return result;
        }
        /// <summary>
        /// 使用商品编号获取正在进行的拼团活动编号
        /// <para>0表示无数据</para>
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public long GetActiveIdByProductId(long productId)
        {
            long result = 0;
            DateTime curtime = DateTime.Now;
            var sql = Context.FightGroupActiveInfo.Where(d => d.ProductId == productId && d.StartTime <= curtime && d.EndTime >= curtime);
            var actobj = sql.FirstOrDefault();
            if (actobj != null)
            {
                result = actobj.Id;
            }
            return result;
        }
        public List<long> GetActiveIdByProductIds(long[] productIds)
        {
            var result = new List<long>();
            DateTime curtime = DateTime.Now;
            var sql = Context.FightGroupActiveInfo.Where(d => productIds.Contains(d.ProductId) && d.StartTime <= curtime && d.EndTime >= curtime);
            var actobj = sql.AsNoTracking();
            if (actobj != null)
            {
                result = actobj.Select(p => p.Id).ToList();
            }
            return result;
        }
        /// <summary>
        /// 获取拼团活动项
        /// </summary>
        /// <param name="activeId"></param>
        /// <returns></returns>
        public List<FightGroupActiveItemInfo> GetActiveItems(long activeId)
        {
            var datalist = Context.FightGroupActiveItemInfo.Where(d => d.ActiveId == activeId).AsNoTracking().ToList();
            List<FightGroupActiveItemInfo> result = new List<FightGroupActiveItemInfo>();
            if (datalist.Count > 0)
            {
                //补充信息
                var skuids = datalist.Select(d => d.SkuId).ToList();
                var skulist = Context.SKUInfo.Where(d => skuids.Contains(d.Id)).AsNoTracking().ToList();
                foreach (var item in datalist)
                {
                    var cursku = skulist.FirstOrDefault(d => d.Id == item.SkuId);
                    if (cursku != null)  //只使用有效的sku
                    {
                        item.Color = cursku.Color;
                        item.Size = cursku.Size;
                        item.Version = cursku.Version;
                        item.SkuName = cursku.Color + " " + cursku.Size + " " + cursku.Version;
                        item.ProductCostPrice = cursku.CostPrice;
                        item.ProductPrice = cursku.SalePrice;
                        item.ProductStock = cursku.Stock;
                        item.ShowPic = cursku.ShowPic;
                        if (item.ActiveStock > cursku.Stock)
                        {
                            item.ActiveStock = cursku.Stock;
                        }
                        result.Add(item);
                    }
                }
            }
            return result;
        }
        public List<FightGroupActiveItemInfo> GetXcxActiveItems(long activeId)
        {
            var datalist = Context.FightGroupActiveItemInfo.Where(d => d.ActiveId == activeId).AsNoTracking().ToList();
            List<FightGroupActiveItemInfo> result = new List<FightGroupActiveItemInfo>();
            /*if (datalist.Count > 0)
            {
                //补充信息
                var skuids = datalist.Select(d => d.SkuId).ToList();
                var skulist = Context.SKUInfo.Where(d => skuids.Contains(d.Id)).AsNoTracking().ToList();
                foreach (var item in datalist)
                {
                    var cursku = skulist.FirstOrDefault(d => d.Id == item.SkuId);
                    if (cursku != null)  //只使用有效的sku
                    {
                        item.Color = cursku.Color;
                        item.Size = cursku.Size;
                        item.Version = cursku.Version;
                        item.SkuName = cursku.Color + " " + cursku.Size + " " + cursku.Version;
                        item.ProductCostPrice = cursku.CostPrice;
                        item.ProductPrice = cursku.SalePrice;
                        item.ProductStock = cursku.Stock;
                        item.ShowPic = cursku.ShowPic;
                        if (item.ActiveStock > cursku.Stock)
                        {
                            item.ActiveStock = cursku.Stock;
                        }
                        result.Add(item);
                    }
                }
            }*/
            return datalist;
        }
        /// <summary>
        /// 获取活动信息集
        /// </summary>
        /// <param name="Statuses"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="ProductName">商品名</param>
        /// <param name="ShopName">店铺名</param>
        /// <param name="ShopId">店铺编号</param>
        /// <param name="PageNo"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>

        public QueryPageModel<FightGroupActiveInfo> GetActives(
            List<FightGroupActiveStatus> Statuses = null,
            DateTime? StartTime = default(DateTime?),
            DateTime? EndTime = default(DateTime?),
            string ProductName = "",
            string ShopName = "",
            long? ShopId = null,
            int PageNo = 1,
            int PageSize = 10,
            int? auditStatus = null)
        {
            AutoUpdateActiveTimeStatus();

            var _where = Context.FightGroupActiveInfo.GetDefaultPredicate(true);
            if (StartTime.HasValue)
            {
                _where = _where.And(d => d.StartTime >= StartTime.Value.Date);
            }
            if (EndTime.HasValue)
            {
                _where = _where.And(d => d.EndTime <= EndTime.Value);
            }
            if (ShopId.HasValue)
            {
                _where = _where.And(d => d.ShopId == ShopId.Value);
            }
            if (!string.IsNullOrWhiteSpace(ProductName))
            {
                _where = _where.And(d => d.ProductName.Contains(ProductName));
            }
            if (!string.IsNullOrWhiteSpace(ShopName))
            {
                var shopidsql = Context.ShopInfo.Where(d => d.ShopName.Contains(ShopName)).Select(d => d.Id);
                _where = _where.And(d => shopidsql.Contains(d.ShopId));
            }
            if (auditStatus.HasValue)
            {
                if (auditStatus.Value == -1)//库存为0
                {
                    var stockSql = Context.FightGroupActiveItemInfo.Where(s => s.ActiveStock == 0).Select(s => s.ActiveId);
                    _where = _where.And(d => stockSql.Contains(d.Id));
                }
                else if (auditStatus.Value == 0)
                {
                    //_where = _where.And(d => d.ManageAuditStatus == 0);
                    var stockSql = Context.FightGroupActiveItemInfo.Where(s => s.ActiveStock == 0).Select(s => s.ActiveId);
                    _where = _where.And(d => !stockSql.Contains(d.Id));
                    _where = _where.And(d => d.ManageAuditStatus != -1);
                }
                else
                {
                    var stockSql = Context.FightGroupActiveItemInfo.Where(s => s.ActiveStock == 0).Select(s => s.ActiveId);
                    _where = _where.And(d => !stockSql.Contains(d.Id));
                    _where = _where.And(d => d.ManageAuditStatus == -1);
                }
            }
            int ordermode = 0;  //排序方式
            if (Statuses != null)
            {
                if (Statuses.Count > 0)
                {
                    ordermode = 1;
                    var _subwhere = Context.FightGroupActiveInfo.GetDefaultPredicate(false);
                    var curtime = DateTime.Now;
                    foreach (var item in Statuses)
                    {
                        switch (item)
                        {
                            case FightGroupActiveStatus.Ending:
                                _subwhere = _subwhere.Or(d => d.EndTime.Value < curtime);
                                break;
                            case FightGroupActiveStatus.Ongoing:
                                _subwhere = _subwhere.Or(d => d.StartTime <= curtime && d.EndTime >= curtime);
                                break;
                            case FightGroupActiveStatus.WillStart:
                                _subwhere = _subwhere.Or(d => d.StartTime > curtime && d.EndTime > d.StartTime);
                                break;
                        }
                    }
                    _where = _where.And(_subwhere);
                }
            }
            QueryPageModel<FightGroupActiveInfo> result = new QueryPageModel<FightGroupActiveInfo>();
            var prosql = Context.ProductInfo.Where(d => d.IsDeleted == false && d.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                        && d.SaleStatus == ProductInfo.ProductSaleStatus.OnSale).Select(d => d.Id);//(过滤已经删除或下架的商品）
            _where = _where.And(d => prosql.Contains(d.ProductId));
            var sql = Context.FightGroupActiveInfo.Where(_where);
            int total = 0;
            var order = sql.GetOrderBy(d => d.OrderByDescending(o => o.Id));
            switch (ordermode)
            {
                case 1:
                    order = (d => d.OrderBy(o => o.ActiveTimeStatus).ThenByDescending(o => o.StartTime));
                    break;
            }
            List<FightGroupActiveInfo> datalist = sql.GetPage(out total, PageNo, PageSize, order).ToList();
            //外键值补充
            if (datalist.Count > 0)
            {
                //商家信息
                var shopids = datalist.Select(d => d.ShopId).ToList();
                var shopnames = Context.ShopInfo.Where(d => shopids.Contains(d.Id)).Select(d => new { Id = d.Id, ShopName = d.ShopName }).ToList();

                //最低销售价补充
                var proids = datalist.Select(d => d.ProductId).ToList();
                var prominprices = Context.SKUInfo.Where(d => proids.Contains(d.ProductId)).GroupBy(d => d.ProductId).Select(d => new { proid = d.Key, price = d.Min(g => g.SalePrice) });
                var pro = Context.ProductInfo.Where(d => proids.Contains(d.Id)).Select(d => new { Id = d.Id, ShortDescription = d.ShortDescription, Price = d.MinSalePrice }).ToList();
                //最低火拼价补充
                List<long?> actids = datalist.Select(d => (long?)d.Id).ToList();
                var actminprices = Context.FightGroupActiveItemInfo.Where(d => actids.Contains(d.ActiveId)).GroupBy(d => d.ActiveId).Select(d => new { actid = d.Key, price = d.Min(g => g.ActivePrice) });

                var actgroupsumstock = Context.FightGroupActiveItemInfo.Where(d => actids.Contains(d.ActiveId)).GroupBy(d => d.ActiveId).Select(d => new { ActiveId = d.Key, SumStock = d.Sum(s => s.ActiveStock) }).ToList();

                foreach (var item in datalist)
                {
                    var snameobj = shopnames.FirstOrDefault(d => d.Id == item.ShopId);
                    item.ShopName = (snameobj == null ? "" : snameobj.ShopName);

                    var propriceobj = prominprices.FirstOrDefault(d => d.proid == item.ProductId);
                    if (propriceobj != null)
                    {
                        item.MiniSalePrice = propriceobj.price;
                    }
                    else
                    {
                        item.MiniSalePrice = pro.FirstOrDefault().Price;
                    }


                    var actpriceobj = actminprices.FirstOrDefault(d => d.actid == item.Id);
                    if (actpriceobj != null)
                    {
                        item.MiniGroupPrice = actpriceobj.price;
                    }
                    var actsumstock = actgroupsumstock.FirstOrDefault(d => d.ActiveId == item.Id);
                    if (actsumstock != null)
                    {
                        item.HasStock = actsumstock.SumStock > 0;
                    }
                    var proobj = pro.FirstOrDefault(d => d.Id == item.ProductId);
                    item.ProductShortDescription = proobj.ShortDescription;
                    //没映射的
                    //GetActiveNoMap(item);
                }
            }

            result.Models = datalist;
            result.Total = total;
            return result;
        }
        #endregion


        #region 拼团详情
        /// <summary>
        /// 更新拼团详情
        /// </summary>
        /// <param name="data"></param>
        public void UpdateGroup(FightGroupsInfo data)
        {
            UpdateData<FightGroupsInfo>(data);
        }
        /// <summary>
        /// 获取拼团
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="groupId">团编号</param>
        /// <returns></returns>
        public FightGroupsInfo GetGroup(long activeId, long groupId)
        {
            var result = Context.FightGroupsInfo.FirstOrDefault(d => d.ActiveId == activeId && d.Id == groupId);
            if (result == null)
            {
                throw new HimallException("错误的拼团信息");
            }

            #region 信息修正
            //进行中的订单判定时间是否失效
            var gponstate = (int)FightGroupBuildStatus.Ongoing;
            if (result.GroupStatus == gponstate && (DateTime.Now - result.AddGroupTime.Value).TotalHours > (double)result.LimitedHour)
            {
                SetGroupFailed(result);
            }
            #endregion

            //补充订单与用户信息
            FightGroupOrderJoinStatus jstate = FightGroupOrderJoinStatus.JoinSuccess; //取参团成功、参团成功但拼团失败、拼团成功
            List<FightGroupsInfo> fglist = new List<FightGroupsInfo>();
            fglist.Add(result);
            GroupsInfoFill(fglist, true, jstate);
            result = fglist.FirstOrDefault();

            return result;
        }
        /// <summary>
        /// 获取拼团详情列表
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="Statuses">状态集</param>
        /// <param name="StartTime">开始时间</param>
        /// <param name="EndTime">结束时间</param>
        /// <param name="PageNo"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        public QueryPageModel<FightGroupsInfo> GetGroups(
             long activeId,
             List<FightGroupBuildStatus> Statuses = null,
             DateTime? StartTime = null,
             DateTime? EndTime = null,
             int PageNo = 1,
             int PageSize = 10
             )
        {
            QueryPageModel<FightGroupsInfo> result = new QueryPageModel<FightGroupsInfo>();
            var datalist = new List<FightGroupsInfo>();
            int opings = FightGroupBuildStatus.Opening.GetHashCode();
            var sql = Context.FightGroupsInfo.Where(d => d.ActiveId == activeId && d.GroupStatus != opings);
            if (Statuses != null)
            {
                if (Statuses.Count > 0)
                {
                    var _sorwhere = sql.GetDefaultPredicate(false);
                    foreach (var item in Statuses)
                    {
                        int _v = (int)item;
                        var _swhere = sql.GetDefaultPredicate(true);
                        _swhere = _swhere.And(d => d.GroupStatus == _v);

                        _sorwhere = _sorwhere.Or(_swhere);
                    }
                    sql = sql.Where(_sorwhere);
                }
            }
            if (StartTime.HasValue)
            {
                sql = sql.Where(d => d.AddGroupTime >= StartTime.Value);
            }
            if (EndTime.HasValue)
            {
                EndTime = EndTime.Value.AddDays(1).Date;
                sql = sql.Where(d => d.AddGroupTime < EndTime.Value);
            }
            int total = 0;
            var pagesql = sql.GetPage(out total, PageNo, PageSize, d => d.OrderByDescending(o => o.ActiveId));
            result.Total = total;
            datalist = pagesql.ToList();

            GroupsInfoFill(datalist, true);

            result.Models = datalist;
            return result;
        }
        public List<FightGroupsInfo> GetCanJoinGroupsFirst(List<FightGroupBuildStatus> Statuses, int PageNo = 1, int PageSize = 5)
        {
            AutoCloseGroup();
            var datalist = new List<FightGroupsInfo>();
            int opings = FightGroupBuildStatus.Opening.GetHashCode();
            var sql = Context.FightGroupsInfo.Where(d => d.GroupStatus != opings);
            if (Statuses != null)
            {
                if (Statuses.Count > 0)
                {
                    var _sorwhere = sql.GetDefaultPredicate(false);
                    foreach (var item in Statuses)
                    {
                        int _v = (int)item;
                        var _swhere = sql.GetDefaultPredicate(true);
                        _swhere = _swhere.And(d => d.GroupStatus == _v);

                        _sorwhere = _sorwhere.Or(_swhere);
                    }
                    sql = sql.Where(_sorwhere);
                }
            }
            int total = 0;
            var pagesql = sql.GetPage(out total, PageNo, PageSize, d => d.OrderByDescending(o => o.ActiveId));

            datalist = pagesql.ToList();
            GroupsInfoFill(datalist, true);

            return datalist;
        }
        public List<FightGroupsInfo> GetCanJoinGroupsSecond(long[] unActiveId, List<FightGroupBuildStatus> Statuses)
        {
            AutoCloseGroup();
            int opings = FightGroupBuildStatus.Opening.GetHashCode();
            var sql = Context.FightGroupsInfo.Where(d => d.GroupStatus != opings && !unActiveId.Contains(d.ActiveId.Value));
            if (Statuses != null)
            {
                if (Statuses.Count > 0)
                {
                    var _sorwhere = sql.GetDefaultPredicate(false);
                    foreach (var item in Statuses)
                    {
                        int _v = (int)item;
                        var _swhere = sql.GetDefaultPredicate(true);
                        _swhere = _swhere.And(d => d.GroupStatus == _v);

                        _sorwhere = _sorwhere.Or(_swhere);
                    }
                    sql = sql.Where(_sorwhere);
                }
            }
            var datalist = sql.AsNoTracking().ToList();
            GroupsInfoFill(datalist, true);
            return datalist;
        }
        /// <summary>
        /// 补充拼团附属信息
        /// </summary>
        /// <param name="datalist"></param>
        /// <param name="isLoadOrderData">是否装载订单信息</param>
        /// <param name="joinStatus">最低参团状态 (默认：参团成功、参团成功但拼团失败、拼团成功)</param>
        private void GroupsInfoFill(List<FightGroupsInfo> datalist, bool isLoadOrderData = false, FightGroupOrderJoinStatus joinStatus = FightGroupOrderJoinStatus.JoinSuccess)
        {
            if (datalist == null)
            {
                throw new HimallException("错误的数据");
            }
            //商品信息补充
            var proids = datalist.Select(d => d.ProductId);
            var products = Context.ProductInfo.Where(d => proids.Contains(d.Id)).ToList();



            //团长信息补充
            var huserids = datalist.Select(d => d.HeadUserId).ToList();
            var husers = Context.UserMemberInfo.Where(d => huserids.Contains(d.Id)).ToList();

            var ids = products.Select(d => d.ShopId).ToList();
            var shops = Context.ShopInfo.Where(d => ids.Contains(d.Id)).ToList();



            //拼装数据
            foreach (var item in datalist)
            {
                //商品信息
                var _pro = products.FirstOrDefault(d => d.Id == item.ProductId);
                if (_pro != null)
                {
                    item.ProductName = _pro.ProductName;
                    item.ProductImgPath = _pro.RelativePath;

                    var _shop = shops.FirstOrDefault(d => d.Id == _pro.ShopId);//FightGroupsInfo中ShopId保存的是ProductId,有问题
                    if (_shop != null)
                    {
                        item.ShopName = _shop.ShopName;
                        item.ShopLogo = _shop.Logo;
                        item.ShopId = _shop.Id;
                    }
                }

                //团长信息
                var _user = husers.FirstOrDefault(d => d.Id == item.HeadUserId.Value);
                if (_user != null)
                {
                    item.HeadUserName = _user.ShowNick;
                    item.HeadUserIcon = _user.Photo;
                }


            }

            #region 补充拼团订单信息
            if (isLoadOrderData)
            {
                var gpids = datalist.Select(d => (long?)d.Id);
                //TODO:DZY[160620] 补充拼团订单信息
                int jstate = (int)joinStatus;
                var gpordsql = Context.FightGroupOrderInfo.Where(d => gpids.Contains(d.GroupId));
                gpordsql = gpordsql.Where(d => d.JoinStatus >= jstate);
                var gpordlist = gpordsql.ToList();
                var userids = gpordlist.Select(d => d.OrderUserId).ToList();
                var userinfos = Context.UserMemberInfo.Where(d => userids.Contains(d.Id)).ToList();


                foreach (var item in datalist)
                {
                    var curgpordlist = gpordlist.Where(d => d.GroupId == item.Id).ToList();
                    if (curgpordlist.Count > 0)
                    {
                        var _tmplist = new List<FightGroupOrderInfo>();
                        foreach (var subitem in curgpordlist)
                        {
                            #region  成团成功状态修正
                            var gponstate = (int)FightGroupBuildStatus.Success;
                            if (item.GroupStatus == gponstate)
                            {
                                if (subitem.GetJoinStatus != FightGroupOrderJoinStatus.BuildSuccess)
                                {
                                    SetOrderStatus(subitem.OrderId.Value, FightGroupOrderJoinStatus.BuildSuccess);
                                }
                            }
                            #endregion

                            var curuser = userinfos.FirstOrDefault(d => d.Id == subitem.OrderUserId);
                            if (curuser != null)
                            {
                                subitem.RealName = curuser.RealName;
                                subitem.UserName = curuser.ShowNick;
                                subitem.Photo = curuser.Photo;
                            }
                            if (subitem.IsFirstOrder == true)
                            {
                                _tmplist.Insert(0, subitem);
                            }
                            else
                            {
                                _tmplist.Add(subitem);

                            }
                        }
                        item.GroupOrders = _tmplist.OrderByDescending(d => d.IsFirstOrder).ThenByDescending(d => d.JoinTime).ToList();
                    }
                }
            }
            #endregion

        }

        /// <summary>
        /// 获取参与的拼团
        /// </summary>
        /// <param name="userId">用户编号</param>
        /// <param name="Statuses">参与状态</param>
        /// <param name="PageNo"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        public QueryPageModel<FightGroupsInfo> GetJoinGroups(
            long userId
            , List<FightGroupOrderJoinStatus> Statuses = null
            , int PageNo = 1
            , int PageSize = 10
            )
        {
            QueryPageModel<FightGroupsInfo> result = new QueryPageModel<FightGroupsInfo>();
            var sql = Context.FightGroupOrderInfo.Where(d => d.OrderUserId == userId);
            if (Statuses != null)
            {
                if (Statuses.Count > 0)
                {
                    var _sorwhere = sql.GetDefaultPredicate(false);
                    foreach (var item in Statuses)
                    {
                        int _v = (int)item;
                        var _swhere = sql.GetDefaultPredicate(true);
                        _swhere = _swhere.And(d => d.JoinStatus == _v);

                        _sorwhere = _sorwhere.Or(_swhere);
                    }
                    sql = sql.Where(_sorwhere);
                }
            }

            List<FightGroupOrderInfo> list = sql.ToList();

            int total = 0;
            var pagesql = sql.GetPage(out total, PageNo, PageSize, d => d.OrderByDescending(o => o.JoinTime));
            result.Total = total;
            var gpids = pagesql.Select(d => d.GroupId);
            var gplist = Context.FightGroupsInfo.Where(d => gpids.Contains(d.Id)).ToList();
            List<FightGroupsInfo> datalist = new List<FightGroupsInfo>();
            if (gplist != null)
            {
                if (gplist.Count > 0)
                {
                    foreach (var item in gpids)
                    {
                        datalist.Add(gplist.FirstOrDefault(d => d.Id == item));
                    }
                    if (datalist.Count > 0)
                    {
                        GroupsInfoFill(datalist, true, FightGroupOrderJoinStatus.JoinSuccess);
                    }
                }
            }
            result.Models = datalist;
            return result;
        }
        /// <summary>
        /// 设定拼团失败
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="groupId">团编号</param>
        public void SetGroupFailed(long activeId, long groupId)
        {
            var data = Context.FightGroupsInfo.FirstOrDefault(d => d.ActiveId == activeId && d.Id == groupId);
            SetGroupFailed(data);
        }
        /// <summary>
        /// 设定拼团失败
        /// </summary>
        /// <param name="data"></param>
        private void SetGroupFailed(FightGroupsInfo data)
        {
            if (data != null)
            {
                var gporders = Context.FightGroupOrderInfo.Where(d => d.GroupId == data.Id && d.ActiveId == data.ActiveId).ToList();
                foreach (var item in gporders)
                {
                    SetOrderStatus(item.OrderId.Value, FightGroupOrderJoinStatus.BuildFailed);
                }
                var gponstate = (int)FightGroupBuildStatus.Failed;
                data.GroupStatus = gponstate;
                data.OverTime = DateTime.Now;
                UpdateGroup(data);
                Commit();
            }
        }

        /// <summary>
        /// 设定拼团成功
        /// </summary>
        /// <param name="activeId"></param>
        /// <param name="groupId"></param>
        private void SetGroupSuccess(long activeId, long groupId)
        {
            var data = Context.FightGroupsInfo.FirstOrDefault(d => d.ActiveId == activeId && d.Id == groupId);
            SetGroupSuccess(data);
        }
        /// <summary>
        /// 设定拼团成功
        /// </summary>
        /// <param name="data"></param>
        private void SetGroupSuccess(FightGroupsInfo data)
        {
            if (data != null)
            {
                var gporders = Context.FightGroupOrderInfo.Where(d => d.GroupId == data.Id && d.ActiveId == data.ActiveId).ToList();
                foreach (var item in gporders)
                {
                    SetOrderStatus(item.OrderId.Value, FightGroupOrderJoinStatus.BuildSuccess);
                }
                var gponstate = (int)FightGroupBuildStatus.Success;
                data.IsException = false;
                data.GroupStatus = gponstate;
                data.OverTime = DateTime.Now;
                if (data.JoinedNumber > data.LimitedNumber)
                {
                    data.IsException = true;
                }
                UpdateGroup(data);

                //成功成团数量维护
                var actobj = Context.FightGroupActiveInfo.FirstOrDefault(d => d.Id == data.ActiveId);
                if (actobj != null)
                {
                    if (!actobj.OkGroupCount.HasValue)
                    {
                        actobj.OkGroupCount = 0;
                    }
                    actobj.OkGroupCount++;
                }
                Commit();
            }

        }

        /// <summary>
        /// 自动关闭过期拼团
        /// </summary>
        public void AutoCloseGroup()
        {

            var gponstate = (int)FightGroupBuildStatus.Ongoing;

            //活动到期对已开团的拼团无影响
            var edate = DateTime.Now.AddDays(1).Date;
            //var endactids = context.FightGroupActiveInfo.Where(d => d.EndTime < edate).Select(d => (long?)d.Id);
            //var endactgroups = context.FightGroupsInfo.Where(d => endactids.Contains(d.ActiveId) && d.GroupStatus == gponstate).ToList();

            //处理超时的拼团
            var endgroups = Context.Database.SqlQuery<FightGroupsInfo>("select * from himall_FightGroups where TIMESTAMPDIFF(SECOND,AddGroupTime,NOW())>(LimitedHour*60*60) and GroupStatus=" + gponstate.ToString()).ToList();
            if (endgroups.Count > 0)
            {
                Log.Debug("[FG]" + DateTime.Now.ToString() + "AC_" + string.Join(",", endgroups.Select(d => d.Id).ToArray()));
            }
            foreach (var item in endgroups)
            {
                SetGroupFailed(item);
            }
        }

        public void AutoOpenGroup()
        {
            var gponstate = (int)FightGroupBuildStatus.Ongoing;
            //活动到期对已开团的拼团无影响
            var edate = DateTime.Now.AddDays(1).Date;
            //var endactids = context.FightGroupActiveInfo.Where(d => d.EndTime < edate).Select(d => (long?)d.Id);
            //var endactgroups = context.FightGroupsInfo.Where(d => endactids.Contains(d.ActiveId) && d.GroupStatus == gponstate).ToList();

            //处理超时的拼团
            var endgroups = Context.Database.SqlQuery<FightGroupsInfo>("select * from himall_FightGroups where TIMESTAMPDIFF(SECOND,AddGroupTime,NOW())>(LimitedHour*60*60) and GroupStatus=" + gponstate.ToString()).ToList();
            if (endgroups.Count > 0)
            {
                Log.Debug("[FG]" + DateTime.Now.ToString() + "AO_" + string.Join(",", endgroups.Select(d => d.Id).ToArray()));
            }
            foreach (var item in endgroups)
            {
                SetGroupSuccess(item);
            }
        }

        /// <summary>
        /// 开团
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="userId">用户编号</param>
        /// <returns></returns>
        public FightGroupsInfo AddGroup(long activeId, long userId)
        {
            var activeItem = Context.FightGroupActiveInfo.FirstOrDefault(a => a.Id == activeId);
            var data = new FightGroupsInfo
            {
                HeadUserId = userId,
                ActiveId = activeId,
                LimitedNumber = activeItem.LimitedNumber,
                LimitedHour = activeItem.LimitedHour,
                JoinedNumber = 1,
                IsException = false,
                GroupStatus = FightGroupBuildStatus.Opening.GetHashCode(),
                AddGroupTime = DateTime.Now,
                ProductId = activeItem.ProductId,
                ShopId = activeItem.ShopId,
            };
            var result = Context.FightGroupsInfo.Add(data);
            Context.SaveChanges();

            return result;
        }
        /// <summary>
        /// 参团
        /// </summary>
        /// <param name="activeId">活动编号</param>
        /// <param name="groupId">团组编号</param>
        /// <returns></returns>
        public FightGroupsInfo JoinGroup(long activeId, long groupId)
        {
            FightGroupsInfo result = Context.FightGroupsInfo.FirstOrDefault(d => d.ActiveId == activeId && d.Id == groupId);
            if (result == null)
            {
                throw new HimallException("错误的拼团信息");
            }

            if (!result.JoinedNumber.HasValue)
            {
                result.JoinedNumber = 0;
            }
            FightGroupBuildStatus fgpstate = (FightGroupBuildStatus)result.GroupStatus.Value;
            if (fgpstate == FightGroupBuildStatus.Failed)
            {
                //throw new HimallException("拼团已关闭，参团失败");
                //拼团失败后参与的订单不参与记数
            }
            else
            {
                result.JoinedNumber = result.JoinedNumber + 1;
            }
            if (fgpstate == FightGroupBuildStatus.Success)
            {
                result.IsException = true;
            }
            Commit();
            if (fgpstate == FightGroupBuildStatus.Ongoing)
            {
                //状态修正
                CheckAndUpdateGroupStatus(activeId, groupId);
            }
            return result;
        }
        /// <summary>
        /// 是否可以参团
        /// </summary>
        /// <param name="activeId"></param>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool CanJoinGroup(long activeId, long groupId, long userId)
        {
            bool result = true;
            if (groupId > 0)
            {
                var gpobj = Context.FightGroupsInfo.FirstOrDefault(d => d.ActiveId == activeId && d.Id == groupId);
                if (gpobj == null)
                {
                    result = false;
                    return result;
                }

                if (gpobj.GroupStatus != FightGroupBuildStatus.Ongoing.GetHashCode())
                {
                    if (gpobj.GroupStatus == FightGroupBuildStatus.Opening.GetHashCode())
                    {
                        if (gpobj.HeadUserId != userId)
                        {
                            result = false;
                            return result;
                        }
                    }
                    else
                    {
                        result = false;
                        return result;
                    }
                }
                if (result)
                {
                    int jstate = FightGroupOrderJoinStatus.JoinFailed.GetHashCode();  //参团失败可以再次参团
                    result = Context.FightGroupOrderInfo.Any(d => d.ActiveId == activeId && d.GroupId == groupId && d.OrderUserId == userId && d.JoinStatus > jstate);
                    result = !result;
                }
            }
            return result;
        }
        /// <summary>
        /// 检测并更新拼团状态
        /// </summary>
        /// <param name="activeId"></param>
        /// <param name="groupId"></param>
        public void CheckAndUpdateGroupStatus(long activeId, long groupId)
        {
            FightGroupsInfo result = Context.FightGroupsInfo.FirstOrDefault(d => d.ActiveId == activeId && d.Id == groupId);
            FightGroupBuildStatus fgpstate = (FightGroupBuildStatus)result.GroupStatus.Value;

            if (fgpstate == FightGroupBuildStatus.Ongoing)
            {
                if (result.LimitedNumber <= result.JoinedNumber)
                {
                    SetGroupSuccess(result);
                }
                else
                {
                    if (result.AddGroupTime.Value.AddHours((double)result.LimitedHour.Value) < DateTime.Now)
                    {
                        SetGroupFailed(result);
                    }
                }
            }
        }
        #endregion

        #region 拼团订单
        /// <summary>
        /// 根据拼团活动Id和团组Id获取用户
        /// </summary>
        /// <param name="activeId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public List<FightGroupOrderInfo> GetActiveUsers(long activeId, long groupId)
        {
            //int jstate = (int)FightGroupOrderJoinStatus.JoinSuccess;
            var statuses = new List<FightGroupOrderJoinStatus>();
            statuses.Add(FightGroupOrderJoinStatus.JoinSuccess);
            statuses.Add(FightGroupOrderJoinStatus.BuildFailed);
            statuses.Add(FightGroupOrderJoinStatus.BuildSuccess);

            var sql = Context.FightGroupOrderInfo.Where(d => d.ActiveId == activeId && d.GroupId == groupId);
            if (statuses != null)
            {
                if (statuses.Count > 0)
                {
                    var _sorwhere = sql.GetDefaultPredicate(false);
                    foreach (var item in statuses)
                    {
                        int _v = (int)item;
                        var _swhere = sql.GetDefaultPredicate(true);
                        _swhere = _swhere.And(d => d.JoinStatus == _v);

                        _sorwhere = _sorwhere.Or(_swhere);
                    }
                    sql = sql.Where(_sorwhere);
                }
            }
            var result = sql.AsNoTracking().ToList(); //context.FightGroupOrderInfo.Where(d => d.ActiveId == activeId && d.GroupId == groupId && d.JoinStatus == jstate).AsNoTracking().ToList();
            if (result.Count > 0)
            {
                foreach (var item in result)
                {
                    var pro = Context.UserMemberInfo.FirstOrDefault(d => d.Id == item.OrderUserId);
                    var groupData = Context.FightGroupsInfo.AsNoTracking().FirstOrDefault(d => d.Id == item.GroupId);
                    if (pro != null)
                    {
                        item.RealName = pro.RealName;
                        item.UserName = string.IsNullOrWhiteSpace(pro.Nick) ? pro.UserName : pro.Nick;
                        item.Photo = pro.Photo;
                        item.GroupStatus = (FightGroupBuildStatus)groupData.GroupStatus;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 根据用户id获取拼团订单
        /// </summary>
        /// <param name="userID">用户id</param>
        /// <returns></returns>
        public QueryPageModel<FightGroupOrderInfo> GetFightGroupOrderByUser(int PageNo, int PageSize, long userID, List<FightGroupOrderJoinStatus> status = null)
        {
            QueryPageModel<FightGroupOrderInfo> data = new QueryPageModel<FightGroupOrderInfo>();
            int total = 0;
            var sql = Context.FightGroupOrderInfo.Where(d => d.OrderUserId == userID);
            if (status != null)
            {
                if (status.Count > 0)
                {
                    var _sorwhere = sql.GetDefaultPredicate(false);
                    foreach (var item in status)
                    {
                        int _v = (int)item;
                        var _swhere = sql.GetDefaultPredicate(true);
                        _swhere = _swhere.And(d => d.JoinStatus == _v);

                        _sorwhere = _sorwhere.Or(_swhere);
                    }
                    sql = sql.Where(_sorwhere);
                }
            }

            var result = sql.GetPage(out total, PageNo, PageSize, d => d.OrderByDescending(o => o.JoinTime)).ToList();
            foreach (var item in result)
            {
                var group = Context.FightGroupsInfo.AsNoTracking().FirstOrDefault(d => d.Id == item.GroupId);
                item.GroupStatus = (FightGroupBuildStatus)group.GroupStatus;
                item.LimitedNumber = (int)group.LimitedNumber;
                item.LimitedHour = (int)group.LimitedHour;
                item.JoinedNumber = (int)group.JoinedNumber;
                item.AddGroupTime = (DateTime)group.AddGroupTime;
            }
            data.Models = result;
            data.Total = total;
            return data;
        }
        /// <summary>
        /// 虚拟订单
        /// </summary>
        /// <param name="PageNo"></param>
        /// <param name="PageSize"></param>
        /// <param name="ShopidId"></param>
        /// <returns></returns>
        //public QueryPageModel<VirtualOrderInfo> GetVirtualOrderInfoByShopid(int PageNo, int PageSize, long ShopidId)
        //{
        //   QueryPageModel<VirtualOrderInfo> data = new QueryPageModel<VirtualOrderInfo>();
        //    int total = 0;
        //    var sql =  Context.VirtualOrderInfo.Where(d => d.ShopId==ShopidId);

        //      var  resuit = sql.GetPage(out total, PageNo, PageSize, d => d.OrderByDescending(o => o.PayTime));
        //      data.Models = resuit.ToList();
        //      data.Total = total;
        //    return data;
        //}
        /// <summary>
        /// 用户在营销活动中已购买数量
        /// </summary>
        /// <param name="activeId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetMarketSaleCountForUserId(long groupId, long userId)
        {
            int jfstate = FightGroupOrderJoinStatus.JoinFailed.GetHashCode();  //参团失败可以再次购买
            int bfstate = FightGroupOrderJoinStatus.BuildFailed.GetHashCode();  //拼团失败可以再次购买

            var actordsql = Context.FightGroupOrderInfo.Where(d => d.GroupId == groupId && d.OrderUserId == userId && (d.JoinStatus != jfstate && d.JoinStatus != bfstate)).Select(d => d.Quantity);
            //var ordsql = context.OrderItemInfo.Where(d => actordsql.Contains(d.OrderId));
            //var count = ordsql.Sum(d => (long?)d.Quantity).GetValueOrDefault();
            long? count = 0;
            if (actordsql.Count() > 0)
            {
                count = actordsql.Sum();
            }
            if (count == null)
            {
                count = 0;
            }
            return (int)count;
        }
        /// <summary>
        /// 设定加入拼团状态
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <param name="status">状态</param>
        public FightGroupOrderJoinStatus SetOrderStatus(long orderId, FightGroupOrderJoinStatus status)
        {
            var gpord = Context.FightGroupOrderInfo.FirstOrDefault(d => d.OrderId == orderId);
            if (gpord == null)
            {
                throw new HimallException("错误的拼团订单信息");
            }
            var gpobj = Context.FightGroupsInfo.FirstOrDefault(d => d.Id == gpord.GroupId);
            switch (status)
            {
                case FightGroupOrderJoinStatus.BuildFailed:
                    #region 拼团失败
                    SetOrderBuildFailed(gpord, gpobj);
                    //发送提示消息
                    Task.Factory.StartNew(() => ServiceProvider.Instance<IFightGroupService>.Create.SendMessage(orderId, FightGroupOrderJoinStatus.BuildFailed));
                    #endregion
                    break;

                case FightGroupOrderJoinStatus.BuildSuccess:
                    #region 拼团成功
                    int jstate = FightGroupOrderJoinStatus.JoinSuccess.GetHashCode();  //参团成功的才可以拼团成功
                    if (gpord.JoinStatus.GetHashCode() >= jstate)
                    {
                        gpord.JoinStatus = status.GetHashCode();
                        gpord.OverTime = DateTime.Now;
                        //处理订单
                        var orderobj = Context.OrderInfo.FirstOrDefault(d => d.Id == gpord.OrderId);
                        if (orderobj != null)
                        {
                            orderobj.LastModifyTime = DateTime.Now;
                            if (orderobj.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake)
                            {
                                orderobj.OrderStatus = OrderInfo.OrderOperateStatus.WaitSelfPickUp;
                                orderobj.PickupCode = OrderService.GeneratePickupCode(orderobj.Id);
                            }
                        }

                        //发送提示消息
                        Task.Factory.StartNew(() => ServiceProvider.Instance<IFightGroupService>.Create.SendMessage(orderId, FightGroupOrderJoinStatus.BuildSuccess));
                    }
                    #endregion
                    break;
                case FightGroupOrderJoinStatus.JoinSuccess:
                    #region 参团成功
                    if (gpord.IsFirstOrder == true)
                    {
                        #region 开团
                        gpord.JoinStatus = status.GetHashCode();
                        if (gpobj.GroupStatus == FightGroupBuildStatus.Opening.GetHashCode())  //改为开团中改为成团中
                        {
                            gpobj.GroupStatus = FightGroupBuildStatus.Ongoing.GetHashCode();
                            gpobj.AddGroupTime = DateTime.Now;   //开团时间以付款时间为准

                            #region 维护开团数量
                            var activeItem = GetActive(gpobj.ActiveId.Value);
                            if (!activeItem.GroupCount.HasValue)
                            {
                                activeItem.GroupCount = 0;
                            }
                            activeItem.GroupCount++;
                            #endregion
                        }
                        gpord.OverTime = DateTime.Now;
                        //发送提示消息
                        Task.Factory.StartNew(() => ServiceProvider.Instance<IFightGroupService>.Create.SendMessage(orderId, FightGroupOrderJoinStatus.JoinSuccess));
                        #endregion
                    }
                    else
                    {
                        #region 参团
                        if (gpord.GetJoinStatus == FightGroupOrderJoinStatus.Ongoing)
                        {
                            gpord.JoinStatus = status.GetHashCode();
                            gpobj = JoinGroup(gpord.ActiveId.Value, gpord.GroupId.Value);
                            if (gpobj.GroupStatus == FightGroupBuildStatus.Failed.GetHashCode())
                            {
                                SetOrderStatus(orderId, FightGroupOrderJoinStatus.BuildFailed);   //拼团失效，自动退款
                            }
                            else
                            {
                                //发送参团成功消息
                                Task.Factory.StartNew(() => ServiceProvider.Instance<IFightGroupService>.Create.SendMessage(orderId, FightGroupOrderJoinStatus.JoinSuccess));
                            }
                        }
                        #endregion
                    }
                    gpord.JoinTime = DateTime.Now;
                    #endregion
                    break;
                case FightGroupOrderJoinStatus.JoinFailed:
                    #region 参团失败
                    gpord.JoinStatus = status.GetHashCode();
                    if (gpord.IsFirstOrder == true)
                    {
                        gpobj.GroupStatus = FightGroupBuildStatus.Failed.GetHashCode();   //改为开团中改为成团失败
                    }
                    gpord.OverTime = DateTime.Now;
                    //退库存
                    UpdateActiveStock(gpord.ActiveId.Value, gpord.SkuId, gpord.Quantity);
                    #endregion
                    break;
                default:
                    gpord.JoinStatus = status.GetHashCode();
                    break;
            }

            Commit();  //提交修改
            FightGroupOrderJoinStatus result = (FightGroupOrderJoinStatus)gpord.JoinStatus;
            return result;
        }

        #region 拼团订单状态处理私有方法
        /// <summary>
        /// 订单拼团失败
        /// </summary>
        /// <param name="groupOrder"></param>
        /// <param name="group"></param>
        private void SetOrderBuildFailed(FightGroupOrderInfo groupOrder, FightGroupsInfo group)
        {
            long orderId = groupOrder.OrderId.Value;

            #region 拼团失败
            if (group.BuildStatus == FightGroupBuildStatus.Failed)
            {
                groupOrder.JoinStatus = FightGroupOrderJoinStatus.JoinFailed.GetHashCode();
            }
            else
            {
                groupOrder.JoinStatus = FightGroupOrderJoinStatus.BuildFailed.GetHashCode();
            }
            groupOrder.OverTime = DateTime.Now;
            //退库存
            UpdateActiveStock(groupOrder.ActiveId.Value, groupOrder.SkuId, groupOrder.Quantity);

            #region 订单退款
            var _iOrderService = ServiceProvider.Instance<IOrderService>.Create;
            OrderInfo order = _iOrderService.GetOrder(orderId);
            //订单有可能被删
            if (order != null)
            {
                if (order.OrderStatus >= OrderInfo.OrderOperateStatus.WaitDelivery)  //已付款订单
                {
                    var userinfos = Context.UserMemberInfo.FirstOrDefault(d => d.Id == groupOrder.OrderUserId.Value);
                    //处理退款
                    var _iRefundService = ServiceProvider.Instance<IRefundService>.Create;
                    //计算可退金额 预留
                    _iOrderService.CalculateOrderItemRefund(orderId);

                    OrderRefundInfo refundinfo = new OrderRefundInfo();
                    refundinfo.OrderId = orderId;
                    refundinfo.UserId = groupOrder.OrderUserId.Value;
                    refundinfo.RefundMode = OrderRefundInfo.OrderRefundMode.OrderRefund;
                    refundinfo.Applicant = userinfos.UserName;
                    refundinfo.ApplyDate = DateTime.Now;
                    refundinfo.Reason = "拼团失败，系统处理";
                    refundinfo.Amount = order.OrderEnabledRefundAmount;
                    refundinfo.ContactPerson = order.ShipTo;
                    refundinfo.ContactCellPhone = order.CellPhone;
                    refundinfo.RefundPayType = OrderRefundInfo.OrderRefundPayType.BackCapital;
                    refundinfo.OrderItemId = order.OrderItemInfo.FirstOrDefault().Id;
                    if (order.CanBackOut())
                    {
                        refundinfo.RefundPayType = OrderRefundInfo.OrderRefundPayType.BackOut;
                    }
                    try
                    {
                        _iRefundService.AddOrderRefund(refundinfo);

                        //自动同意退款
                        _iRefundService.SellerDealRefund(refundinfo.Id, OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery, "拼团失败，系统自动处理", "系统Job");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                    }
                }
                else
                {
                    //未付款订单 先不处理，等job自动关闭
                    //_iOrderService.PlatformCloseOrder(orderId, "系统Job", "拼团失败，系统自动处理");
                }
            }
            #endregion

            #endregion
        }
        #endregion

        /// <summary>
        /// 发送提示消息
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        public void SendMessage(long orderId, FightGroupOrderJoinStatus status)
        {
            FightGroupOrderInfo gpord = this.GetOrder(orderId);
            if (gpord == null)
                return;
            FightGroupsInfo fightGroupsInfo1 = Queryable.FirstOrDefault<FightGroupsInfo>((IQueryable<FightGroupsInfo>)this.Context.FightGroupsInfo, (Expression<Func<FightGroupsInfo, bool>>)(d => d.Id == gpord.GroupId.Value));
            List<FightGroupsInfo> datalist = new List<FightGroupsInfo>();
            datalist.Add(fightGroupsInfo1);
            this.GroupsInfoFill(datalist, false, FightGroupOrderJoinStatus.JoinSuccess);
            FightGroupsInfo fightGroupsInfo2 = Enumerable.FirstOrDefault<FightGroupsInfo>((IEnumerable<FightGroupsInfo>)datalist);
            IWXMsgTemplateService create = Instance<IWXMsgTemplateService>.Create;
            WX_MsgTemplateSendDataModel data1 = new WX_MsgTemplateSendDataModel();
            switch (status)
            {
                case FightGroupOrderJoinStatus.JoinSuccess:
                    bool? isFirstOrder = gpord.IsFirstOrder;
                    if ((!isFirstOrder.GetValueOrDefault() ? 0 : (isFirstOrder.HasValue ? 1 : 0)) != 0)
                    {
                        data1.first.value = "您好，您的拼团已开团成功";
                        data1.first.color = "#000000";
                        data1.keyword1.value = fightGroupsInfo2.ProductName ?? "";
                        data1.keyword1.color = "#000000";
                        data1.keyword2.value = "￥" + gpord.SalePrice.ToString("F2");
                        data1.keyword2.color = "#000000";
                        WX_MSGItemBaseModel keyword3 = data1.keyword3;
                        DateTime dateTime = fightGroupsInfo2.AddGroupTime.Value;
                        dateTime = dateTime.AddHours((double)fightGroupsInfo2.LimitedHour.Value);
                        string str1 = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                        keyword3.value = str1;
                        data1.keyword3.color = "#FF0000";
                        data1.remark.value = "加油邀请小伙伴参团吧！";
                        data1.remark.color = "#000000";
                        string str2 = create.GetMessageTemplateShowUrl(MessageTypeEnum.FightGroupOpenSuccess).Replace("{gid}", gpord.GroupId.ToString());
                        string oldValue = "{aid}";
                        long? nullable = gpord.ActiveId;
                        string newValue = nullable.ToString();
                        string str3 = str2.Replace(oldValue, newValue);
                        IWXMsgTemplateService msgTemplateService = create;
                        int num = 13;
                        nullable = gpord.OrderUserId;
                        long userId = nullable.Value;
                        WX_MsgTemplateSendDataModel data2 = data1;
                        string url = str3;
                        string wxopenid = "";
                        msgTemplateService.SendMessageByTemplate((MessageTypeEnum)num, userId, data2, url, wxopenid);
                        break;
                    }
                    data1.first.value = "恭喜您，参团成功";
                    data1.first.color = "#000000";
                    data1.keyword1.value = fightGroupsInfo2.ProductName ?? "";
                    data1.keyword1.color = "#000000";
                    data1.keyword2.value = "￥" + gpord.SalePrice.ToString("F2");
                    data1.keyword2.color = "#000000";
                    WX_MSGItemBaseModel keyword3_1 = data1.keyword3;
                    DateTime dateTime1 = fightGroupsInfo2.AddGroupTime.Value;
                    dateTime1 = dateTime1.AddHours((double)fightGroupsInfo2.LimitedHour.Value);
                    string str4 = dateTime1.ToString("yyyy-MM-dd HH:mm:ss");
                    keyword3_1.value = str4;
                    data1.keyword3.color = "#FF0000";
                    data1.remark.value = "加油邀请小伙伴参团吧！";
                    data1.remark.color = "#000000";
                    string str5 = create.GetMessageTemplateShowUrl(MessageTypeEnum.FightGroupJoinSuccess).Replace("{gid}", gpord.GroupId.ToString());
                    string oldValue1 = "{aid}";
                    long? nullable1 = gpord.ActiveId;
                    string newValue1 = nullable1.ToString();
                    string str6 = str5.Replace(oldValue1, newValue1);
                    IWXMsgTemplateService msgTemplateService1 = create;
                    int num1 = 14;
                    nullable1 = gpord.OrderUserId;
                    long userId1 = nullable1.Value;
                    WX_MsgTemplateSendDataModel data3 = data1;
                    string url1 = str6;
                    string wxopenid1 = "";
                    msgTemplateService1.SendMessageByTemplate((MessageTypeEnum)num1, userId1, data3, url1, wxopenid1);
                    TimeSpan timeSpan = fightGroupsInfo2.AddGroupTime.Value.AddHours((double)fightGroupsInfo2.LimitedHour.Value) - DateTime.Now;
                    WX_MsgTemplateSendDataModel templateSendDataModel = new WX_MsgTemplateSendDataModel();
                    templateSendDataModel.first.value = "你发起的组团新增一位团员";
                    templateSendDataModel.first.color = "#000000";
                    templateSendDataModel.keyword1.value = (string)(object)(int)timeSpan.TotalMinutes + (object)" 分钟";
                    templateSendDataModel.keyword1.color = "#000000";
                    WX_MSGItemBaseModel keyword2 = templateSendDataModel.keyword2;
                    int? limitedNumber = fightGroupsInfo2.LimitedNumber;
                    int? joinedNumber = fightGroupsInfo2.JoinedNumber;
                    string str7 = (limitedNumber.HasValue & joinedNumber.HasValue ? new int?(limitedNumber.GetValueOrDefault() - joinedNumber.GetValueOrDefault()) : new int?()).ToString() + " 人";
                    keyword2.value = str7;
                    templateSendDataModel.keyword2.color = "#000000";
                    templateSendDataModel.remark.value = "加油邀请小伙伴参团吧！";
                    templateSendDataModel.remark.color = "#000000";
                    string messageTemplateShowUrl1 = create.GetMessageTemplateShowUrl(MessageTypeEnum.FightGroupJoinSuccess);
                    string oldValue2 = "{gid}";
                    nullable1 = gpord.GroupId;
                    string newValue2 = nullable1.ToString();
                    string str8 = messageTemplateShowUrl1.Replace(oldValue2, newValue2);
                    string oldValue3 = "{aid}";
                    nullable1 = gpord.ActiveId;
                    string newValue3 = nullable1.ToString();
                    string str9 = str8.Replace(oldValue3, newValue3);
                    IWXMsgTemplateService msgTemplateService2 = create;
                    int num2 = 15;
                    nullable1 = fightGroupsInfo2.HeadUserId;
                    long userId2 = nullable1.Value;
                    WX_MsgTemplateSendDataModel data4 = templateSendDataModel;
                    string url2 = str9;
                    string wxopenid2 = "";
                    msgTemplateService2.SendMessageByTemplate((MessageTypeEnum)num2, userId2, data4, url2, wxopenid2);
                    break;
                case FightGroupOrderJoinStatus.BuildFailed:
                    data1.first.value = "抱歉，您有一个团购拼团失败。";
                    data1.first.color = "#000000";
                    data1.keyword1.value = fightGroupsInfo2.ProductName ?? "";
                    data1.keyword1.color = "#000000";
                    data1.keyword2.value = fightGroupsInfo2.HeadUserName;
                    data1.keyword2.color = "#000000";
                    data1.keyword3.value = fightGroupsInfo2.JoinedNumber.ToString() + " 人";
                    data1.keyword3.color = "#000000";
                    data1.remark.value = "感谢您的参与，您可以另外开一个团购！";
                    data1.remark.color = "#000000";
                    string url3 = create.GetMessageTemplateShowUrl(MessageTypeEnum.FightGroupFailed).Replace("{aid}", gpord.ActiveId.ToString());
                    create.SendMessageByTemplate(MessageTypeEnum.FightGroupFailed, gpord.OrderUserId.Value, data1, url3, "");
                    break;
                case FightGroupOrderJoinStatus.BuildSuccess:
                    data1.first.value = "您参团的商品［" + fightGroupsInfo2.ProductName + "］已组团成功！";
                    data1.first.color = "#000000";
                    data1.keyword1.value = gpord.SalePrice.ToString("F2") + "元";
                    data1.keyword1.color = "#000000";
                    data1.keyword2.value = gpord.OrderId.ToString();
                    data1.keyword2.color = "#000000";
                    data1.remark.value = "点击查看订单详情！";
                    data1.remark.color = "#000000";
                    string messageTemplateShowUrl2 = create.GetMessageTemplateShowUrl(MessageTypeEnum.FightGroupSuccess);
                    string oldValue4 = "{gid}";
                    long? nullable2 = gpord.GroupId;
                    string newValue4 = nullable2.ToString();
                    string str10 = messageTemplateShowUrl2.Replace(oldValue4, newValue4);
                    string oldValue5 = "{aid}";
                    nullable2 = gpord.ActiveId;
                    string newValue5 = nullable2.ToString();
                    string str11 = str10.Replace(oldValue5, newValue5);
                    IWXMsgTemplateService msgTemplateService3 = create;
                    int num3 = 17;
                    nullable2 = gpord.OrderUserId;
                    long userId3 = nullable2.Value;
                    WX_MsgTemplateSendDataModel data5 = data1;
                    string url4 = str11;
                    string wxopenid3 = "";
                    msgTemplateService3.SendMessageByTemplate((MessageTypeEnum)num3, userId3, data5, url4, wxopenid3);
                    break;
            }
            Log.Debug((object)("[FG]SendMessage:" + orderId.ToString() + " _ " + EnumHelper.ToDescription((Enum)status)));
        }
        /// <summary>
        /// 获取拼团订单详情
        /// </summary>
        /// <param name="Id">拼团订单流水Id</param>
        /// <returns></returns>
        public FightGroupOrderInfo GetFightGroupOrderById(long id)
        {
            var result = Context.FightGroupOrderInfo.AsNoTracking().FirstOrDefault(d => d.Id == id);

            var group = Context.FightGroupsInfo.AsNoTracking().FirstOrDefault(d => d.Id == result.GroupId);
            if (group != null)
            {
                result.GroupStatus = (FightGroupBuildStatus)group.GroupStatus;
                result.LimitedNumber = (int)group.LimitedNumber;
                result.LimitedHour = (int)group.LimitedHour;
                result.JoinedNumber = (int)group.JoinedNumber;
                result.AddGroupTime = (DateTime)group.AddGroupTime;
            }
            return result;
        }
        /// <summary>
        /// 订单是否可以支付
        /// <para>成团成功后，未完成支付的订单不可付款</para>
        /// <para>成团失败后，未完成支付的订单不可付款</para>
        /// </summary>
        /// <param name="orderId">订单编号</param>
        /// <returns></returns>
        public bool OrderCanPay(long orderId)
        {
            bool result = true;
            var ordobj = GetOrder(orderId);
            if (ordobj != null)
            {
                switch (ordobj.GroupStatus)
                {
                    case FightGroupBuildStatus.Ongoing:
                        result = true;
                        break;
                    case FightGroupBuildStatus.Opening:
                        result = true;
                        break;
                }
            }
            return result;
        }
        /// <summary>
        /// 获取拼团订单详情
        /// </summary>
        /// <param name="orderId">订单编号</param>
        /// <returns></returns>
        public FightGroupOrderInfo GetOrder(long orderId)
        {
            var result = Context.FightGroupOrderInfo.AsNoTracking().FirstOrDefault(d => d.OrderId == orderId);
            if (result == null)
            {
                throw new HimallException("错误的拼团订单信息");
            }
            var group = Context.FightGroupsInfo.AsNoTracking().FirstOrDefault(d => d.Id == result.GroupId);
            if (group != null)
            {
                result.GroupStatus = (FightGroupBuildStatus)group.GroupStatus;
                result.LimitedNumber = (int)group.LimitedNumber;
                result.LimitedHour = (int)group.LimitedHour;
                result.JoinedNumber = (int)group.JoinedNumber;
                result.AddGroupTime = (DateTime)group.AddGroupTime;
            }
            return result;
        }
        /// <summary>
        /// 获取参团中的订单数
        /// </summary>
        /// <param name="userId">用户编号</param>
        /// <returns></returns>
        public int CountJoiningOrder(long userId)
        {
            int jstate = FightGroupOrderJoinStatus.JoinSuccess.GetHashCode();
            return Context.FightGroupOrderInfo.Count(d => d.JoinStatus == jstate && d.OrderUserId == userId);
        }
        /// <summary>
        /// 拼团订单
        /// <para>未付款</para>
        /// </summary>
        /// <param name="actionId">活动编号</param>
        /// <param name="orderId">订单编号</param>
        /// <param name="userId">用户编号</param>
        /// <param name="groupId">拼团编号 0表示开新团</param>
        /// <param name="invitationUserId">推荐人Id</param>
        public FightGroupOrderInfo AddOrder(long actionId, long orderId, long userId, long groupId = 0, long shopId = 0, long invitationUserId = 0)
        {
            bool flag = false;
            if (groupId == 0L)
                flag = true;
            OrderInfo orderInfo = Queryable.FirstOrDefault<OrderInfo>((IQueryable<OrderInfo>)this.Context.OrderInfo, (Expression<Func<OrderInfo, bool>>)(a => a.Id == orderId && a.UserId == userId));
            if (orderInfo == null)
                throw new HimallException("错误的订单信息，或订单不属于当前用户");
            OrderItemInfo orderItemInfo = Enumerable.FirstOrDefault<OrderItemInfo>((IEnumerable<OrderItemInfo>)orderInfo.OrderItemInfo);
            long quantity = orderItemInfo.Quantity;
            string skuId = orderItemInfo.SkuId;
            Decimal salePrice = orderItemInfo.SalePrice;
            long productId = orderItemInfo.ProductId;
            if (flag)
                groupId = this.AddGroup(actionId, userId).Id;
            this.UpdateActiveStock(actionId, skuId, -1L * quantity);
            FightGroupOrderInfo entity = new FightGroupOrderInfo();
            entity.Id = 0L;
            entity.ActiveId = new long?(actionId);
            entity.ProductId = new long?(productId);
            entity.SkuId = skuId;
            entity.GroupId = new long?(groupId);
            entity.OrderId = new long?(orderId);
            entity.OrderUserId = new long?(userId);
            entity.IsFirstOrder = new bool?(flag);
            entity.JoinTime = new DateTime?(DateTime.Now);
            entity.JoinStatus = !flag ? new int?(FightGroupOrderJoinStatus.Ongoing.GetHashCode()) : new int?(FightGroupOrderJoinStatus.BuildOpening.GetHashCode());
            entity.Quantity = quantity;
            entity.SalePrice = salePrice;
            this.Context.FightGroupOrderInfo.Add(entity);
            this.Commit();
            return entity;
        }
        /// <summary>
        /// 根据原订单号获取拼团订单信息
        /// </summary>
        /// <param name="orderId">原订单号</param>
        /// <returns></returns>
        public FightGroupOrderInfo GetFightGroupOrderStatusByOrderId(long orderId)
        {
            var result = Context.FightGroupOrderInfo.AsNoTracking().FirstOrDefault(d => d.OrderId == orderId);
            return result;
        }
        /// <summary>
        /// 新增拼团订单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public long AddGroupOrder(FightGroupOrderInfo data)
        {
            var result = Context.FightGroupOrderInfo.Add(data);
            return result.Id;
        }
        /// <summary>
        /// 付款成功后更新拼团订单状态
        /// </summary>
        /// <param name="data"></param>
        public void UpdateGroupOrderStatus(FightGroupOrderInfo data)
        {
            var m = Context.FightGroupOrderInfo.Where(a => a.OrderId == data.OrderId).FirstOrDefault();
            if (m != null)
            {
                m.JoinStatus = data.JoinStatus;
            }
            Context.SaveChanges();
        }
        /// <summary>
        /// 根据团组Id获取订单数据
        /// </summary>
        /// <param name="statuses"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public List<FightGroupOrderInfo> GetFightGroupOrderByGroupId(List<FightGroupOrderJoinStatus> statuses, long groupId)
        {
            var datalist = new List<FightGroupOrderInfo>();
            if (groupId > 0)
            {
                var sql = Context.FightGroupOrderInfo.Where(d => d.GroupId == groupId);
                if (statuses != null)
                {
                    if (statuses.Count > 0)
                    {
                        var _sorwhere = sql.GetDefaultPredicate(false);
                        foreach (var item in statuses)
                        {
                            int _v = (int)item;
                            var _swhere = sql.GetDefaultPredicate(true);
                            _swhere = _swhere.And(d => d.JoinStatus == _v);

                            _sorwhere = _sorwhere.Or(_swhere);
                        }
                        sql = sql.Where(_sorwhere);
                    }
                }
                var pagesql = sql.AsNoTracking();
                datalist = pagesql.ToList();
            }
            return datalist;
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 更新活动状态
        /// </summary>
        private void AutoUpdateActiveTimeStatus()
        {
            Context.Database.ExecuteSqlCommand("UPDATE himall_FightGroupActive set ActiveTimeStatus=-1 where EndTime<NOW()");
            Context.Database.ExecuteSqlCommand("UPDATE himall_FightGroupActive set ActiveTimeStatus=0 where StartTime<=NOW() and EndTime>NOW()");
            Context.Database.ExecuteSqlCommand("UPDATE himall_FightGroupActive set ActiveTimeStatus=1 where StartTime>NOW()");
        }
        #endregion

        /// <summary>
        /// 提交保存
        /// </summary>
        public void Commit()
        {
            Context.SaveChanges();
        }

        public List<FightGroupPrice> GetFightGroupPrice()
        {
            List<FightGroupPrice> list = new List<FightGroupPrice>();
            string sql = "select a.ProductId,a.ActivePrice from himall_fightgroupactiveitem a left join himall_fightgroupactive b on a.ActiveId = b.Id where b.StartTime < @StartTime and  b.EndTime > @EndTime";
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                list = conn.Query<FightGroupPrice>(sql, new { StartTime = DateTime.Now, EndTime = DateTime.Now }).ToList();
            }
            return list;
        }
    }
}
