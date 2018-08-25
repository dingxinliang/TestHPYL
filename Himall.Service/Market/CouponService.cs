using Himall.Core;
using System.Linq.Expressions;
using Himall.Entity;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using MySql.Data.MySqlClient;
using Dapper;
using Himall.ServiceProvider;
using Himall.CommonModel;
using System.Data.Entity.Infrastructure;

namespace Himall.Service
{
    public class CouponService : ServiceBase, ICouponService
    {
        // Fields
        private IWXCardService ser_wxcard = Instance<IWXCardService>.Create;
        private WXCardLogInfo.CouponTypeEnum ThisCouponType = WXCardLogInfo.CouponTypeEnum.Coupon;

        // Methods
        public void AddCoupon(CouponInfo info)
        {
            this.CanAddOrEditCoupon(info);
            ActiveMarketServiceInfo info2 = base.Context.ActiveMarketServiceInfo.FirstOrDefault<ActiveMarketServiceInfo>(a => (((int)a.TypeId) == 2) && (a.ShopId == info.ShopId));
            if (info2 == null)
            {
                throw new HimallException("您没有订购此服务");
            }
            if (info2.MarketServiceRecordInfo.Max<MarketServiceRecordInfo, DateTime>(item => item.EndTime.Date) < info.EndTime)
            {
                throw new HimallException("结束日期已经超过购买优惠券服务的日期");
            }
            info.WXAuditStatus = 1;
            if (info.IsSyncWeiXin == 1)
            {
                info.WXAuditStatus = 0;
            }
            base.Context.CouponInfo.Add(info);
            base.Context.SaveChanges();
            if (info.IsSyncWeiXin == 1)
            {
                WXCardLogInfo info3 = new WXCardLogInfo
                {
                    CardColor = info.FormWXColor,
                    CardTitle = info.FormWXCTit,
                    CardSubTitle = info.FormWXCSubTit,
                    CouponType = new WXCardLogInfo.CouponTypeEnum?(this.ThisCouponType),
                    CouponId = new long?(info.Id),
                    ShopId = info.ShopId,
                    ShopName = info.ShopName,
                    ReduceCost = (int)(info.Price * 100M),
                    LeastCost = (int)(info.OrderAmount * 100M),
                    Quantity = info.Num,
                    GetLimit = info.PerMax,
                    DefaultDetail = info.Price.ToString("F2") + "元优惠券1张",
                    BeginTime = info.StartTime.Date,
                    EndTime = info.EndTime.AddDays(1.0).AddMinutes(-1.0)
                };
                if (!this.ser_wxcard.Add(info3))
                {
                    base.Context.CouponInfo.Remove(info);
                    base.Context.SaveChanges();
                    throw new HimallException("同步微信卡券失败，请检查参数是否有错！");
                }
                info.CardLogId = new long?(info3.Id);
                base.Context.SaveChanges();
            }
            this.SaveCover(info);
        }

        public CouponRecordInfo AddCouponRecord(CouponRecordInfo info)
        {
            string shopName = base.Context.ShopInfo.FindById<ShopInfo>(info.ShopId).ShopName;
            CouponInfo info2 = base.Context.CouponInfo.FirstOrDefault<CouponInfo>(d => d.Id == info.CouponId);
            if ((info2.IsSyncWeiXin == 1) && (info2.WXAuditStatus != 1))
            {
                throw new HimallException("优惠券状态错误，不可领取");
            }
            if (info2.ReceiveType == CouponInfo.CouponReceiveType.IntegralExchange)
            {
                MemberIntegralRecord model = new MemberIntegralRecord
                {
                    UserName = info.UserName,
                    MemberId = info.UserId,
                    RecordDate = new DateTime?(DateTime.Now),
                    TypeId = MemberIntegral.IntegralType.Exchange,
                    Integral = info2.NeedIntegral,
                    ReMark = "兑换优惠券:面值" + info2.Price.ToString("f2")
                };
                IConversionMemberIntegralBase conversionMemberIntegralEntity = Instance<IMemberIntegralConversionFactoryService>.Create.Create(MemberIntegral.IntegralType.Exchange, model.Integral);
                Instance<IMemberIntegralService>.Create.AddMemberIntegral(model, conversionMemberIntegralEntity);
            }
            info.CounponStatus = CouponRecordInfo.CounponStatuses.Unuse;
            info.CounponSN = Guid.NewGuid().ToString().Replace("-", "");
            info.UsedTime = null;
            info.CounponTime = DateTime.Now;
            info.ShopName = shopName;
            info.OrderId = null;
            CouponRecordInfo info3 = base.Context.CouponRecordInfo.Add(info);
            base.Context.SaveChanges();
            return info3;
        }

        public void AddSendmessagerecordCouponSN(List<SendmessagerecordCouponSNInfo> items)
        {
            if ((items != null) && (items.Count > 0))
            {
                base.Context.SendmessagerecordCouponSNInfo.AddRange(items);
                base.Context.SaveChanges();
            }
        }

        public bool CanAddIntegralCoupon(long shopid, long id = 0L)
        {
            DateTime CurDay = DateTime.Now.Date;
            DateTime CurTime = DateTime.Now;
            IQueryable<CouponInfo> source = from d in base.Context.CouponInfo
                                            where (((d.ShopId == shopid) && (((int)d.ReceiveType) == 1)) && (d.EndIntegralExchange > CurTime)) && (d.EndTime > CurDay)
                                            select d;
            if (id > 0L)
            {
                source = from d in source
                         where d.Id != id
                         select d;
            }
            return (source.Count<CouponInfo>() < 1);
        }

        private void CanAddOrEditCoupon(CouponInfo info)
        {
            List<long> ids = Enumerable.ToList<long>((IEnumerable<long>)Queryable.Select<CouponInfo, long>(Queryable.Where<CouponInfo>((IQueryable<CouponInfo>)this.Context.CouponInfo, (Expression<Func<CouponInfo, bool>>)(a => a.EndTime > DateTime.Now && a.ShopId == info.ShopId && (int)a.ReceiveType != 1 && (int)a.ReceiveType != 2)), (Expression<Func<CouponInfo, long>>)(a => a.Id)));
            List<CouponSettingInfo> list = Enumerable.ToList<CouponSettingInfo>((IEnumerable<CouponSettingInfo>)Queryable.Where<CouponSettingInfo>((IQueryable<CouponSettingInfo>)this.Context.CouponSettingInfo, (Expression<Func<CouponSettingInfo, bool>>)(a => ids.Contains(a.CouponID))));
            if (info.Himall_CouponSetting == null || info.Himall_CouponSetting.Count <= 0)
                return;
            int num = 5;
            if (Enumerable.Count<CouponSettingInfo>((IEnumerable<CouponSettingInfo>)list, (Func<CouponSettingInfo, bool>)(a => a.PlatForm == PlatformType.Wap)) >= num && !Enumerable.Any<long>((IEnumerable<long>)ids, (Func<long, bool>)(d => d == info.Id)) && Enumerable.Any<CouponSettingInfo>((IEnumerable<CouponSettingInfo>)info.Himall_CouponSetting, (Func<CouponSettingInfo, bool>)(d => d.PlatForm == PlatformType.Wap)))
                throw new HimallException("最多添加5个移动端优惠券");
            if (Enumerable.Count<CouponSettingInfo>((IEnumerable<CouponSettingInfo>)list, (Func<CouponSettingInfo, bool>)(a => a.PlatForm == PlatformType.PC)) >= num && !Enumerable.Any<long>((IEnumerable<long>)ids, (Func<long, bool>)(d => d == info.Id)) && Enumerable.Any<CouponSettingInfo>((IEnumerable<CouponSettingInfo>)info.Himall_CouponSetting, (Func<CouponSettingInfo, bool>)(d => d.PlatForm == PlatformType.PC)))
                throw new HimallException("最多添加5个PC端个优惠券");
        }

        public void CancelCoupon(long couponId, long shopId)
        {
            CouponInfo info = base.Context.CouponInfo.FirstOrDefault<CouponInfo>(a => (a.ShopId == shopId) && (a.Id == couponId));
            if (info == null)
            {
                throw new HimallException("找不到相对应的优惠券！");
            }
            info.EndTime = DateTime.Now.Date.AddDays(-1.0);
            base.Context.SaveChanges();
            if ((info.IsSyncWeiXin == 1) && info.CardLogId.HasValue)
            {
                this.ser_wxcard.Delete(info.CardLogId.Value);
            }
        }

        public void ClearErrorWeiXinCardSync()
        {
            DateTime overtime = DateTime.Now.AddDays(-2.0).Date;
            int wxaudstate = 0;
            List<CouponInfo> list = (from d in base.Context.CouponInfo
                                     where ((d.CreateTime < overtime) && (d.IsSyncWeiXin == 1)) && (d.WXAuditStatus == wxaudstate)
                                     select d).ToList<CouponInfo>();
            if (list.Count > 0)
            {
                List<long?> cardids = (from d in list select d.CardLogId).ToList<long?>();
                List<WXCardLogInfo> entities = (from d in base.Context.WXCardLogInfo
                                                where cardids.Contains(d.Id)
                                                select d).ToList<WXCardLogInfo>();
                if (entities.Count > 0)
                {
                    base.Context.WXCardLogInfo.RemoveRange(entities);
                }
                foreach (CouponInfo info in list)
                {
                    info.WXAuditStatus = -1;
                    info.EndTime = DateTime.Now.AddDays(-1.0);
                }
                base.Context.SaveChanges();
            }
        }

        public void EditCoupon(CouponInfo info)
        {
            this.CanAddOrEditCoupon(info);
            CouponInfo model = base.Context.CouponInfo.FirstOrDefault<CouponInfo>(a => (a.ShopId == info.ShopId) && (a.Id == info.Id));
            if (model == null)
            {
                throw new HimallException("错误的优惠券信息");
            }
            int num = base.Context.CouponRecordInfo.Count<CouponRecordInfo>(d => d.CouponId == model.Id);
            if (info.Num < num)
            {
                throw new HimallException("错误的发放总量，已领取数已超过发放总量");
            }
            base.Context.CouponSettingInfo.RemoveRange(model.Himall_CouponSetting);
            if ((info.Himall_CouponSetting != null) && (info.Himall_CouponSetting.Count > 0))
            {
                model.Himall_CouponSetting = info.Himall_CouponSetting;
            }
            model.CouponName = info.CouponName;
            model.PerMax = info.PerMax;
            model.Num = info.Num;
            model.ReceiveType = info.ReceiveType;
            model.EndIntegralExchange = info.EndIntegralExchange;
            model.NeedIntegral = info.NeedIntegral;
            model.IntegralCover = info.IntegralCover;
            if ((model.IsSyncWeiXin == 1) && model.CardLogId.HasValue)
            {
                WXCardLogInfo info2 = base.Context.WXCardLogInfo.FirstOrDefault<WXCardLogInfo>(d => d.Id == model.CardLogId.Value);
                if (info2 != null)
                {
                    int num2 = model.Num - num;
                    this.ser_wxcard.EditGetLimit(new int?(model.PerMax), info2.CardId);
                    this.ser_wxcard.EditStock(num2, info2.CardId);
                }
            }
            base.Context.SaveChanges();
            this.SaveCover(model);
        }

        public List<CouponRecordInfo> GetAllCoupon(long userid)
        {
            return (from p in base.Context.CouponRecordInfo
                    where p.UserId == userid
                    select p).ToList<CouponRecordInfo>();
        }

        public List<UserCouponInfo> GetAllUserCoupon(long userid)
        {
            DateTime date = DateTime.Now;
            return Enumerable.ToList<UserCouponInfo>((IEnumerable<UserCouponInfo>)Queryable.Select<CouponRecordInfo, UserCouponInfo>(Queryable.Where<CouponRecordInfo>((IQueryable<CouponRecordInfo>)this.Context.CouponRecordInfo, (Expression<Func<CouponRecordInfo, bool>>)(item => item.UserId == userid && (int)item.CounponStatus == 0 && item.Himall_Coupon.EndTime > date)), (Expression<Func<CouponRecordInfo, UserCouponInfo>>)(b => new UserCouponInfo()
            {
                UserId = b.UserId,
                ShopId = b.ShopId,
                CouponId = b.CouponId,
                Price = b.Himall_Coupon.Price,
                PerMax = b.Himall_Coupon.PerMax,
                OrderAmount = b.Himall_Coupon.OrderAmount,
                Num = b.Himall_Coupon.Num,
                StartTime = b.Himall_Coupon.StartTime,
                EndTime = b.Himall_Coupon.EndTime,
                ShopName = b.Himall_Coupon.ShopName,
                CreateTime = b.Himall_Coupon.CreateTime,
                CouponName = b.Himall_Coupon.CouponName,
                UseStatus = b.CounponStatus,
                UseTime = b.UsedTime
            })));
        }

        public ObsoletePageModel<CouponInfo> GetCouponByName(string text, DateTime endDate, int ReceiveType, int page, int pageSize)
        {
            int total = 0;
            IQueryable<CouponInfo> coupon = Context.CouponInfo.AsQueryable();

            if (!text.Equals(""))
                coupon = coupon.Where(d => d.CouponName.Contains(text));

            if (endDate > DateTime.Parse("2000-01-01"))
                coupon = coupon.Where(d => d.EndTime > endDate);
            else
                coupon = coupon.Where(d => d.EndTime > DateTime.Now);//查询未结束

            if (ReceiveType >= 0)
                coupon = coupon.Where(d => d.ReceiveType == (Model.CouponInfo.CouponReceiveType)ReceiveType);

            coupon = coupon.Where(d => d.Num > d.Himall_CouponRecord.Count());//查询库存剩余
            coupon = coupon.GetPage(out total, d => d.OrderByDescending(o => o.EndTime), page, pageSize);
            ObsoletePageModel<CouponInfo> pageModel = new ObsoletePageModel<CouponInfo>() { Models = coupon, Total = total };
            return pageModel;
        }

        public CouponInfo GetCouponInfo(long couponId)
        {
            CouponInfo result = base.Context.CouponInfo.FirstOrDefault<CouponInfo>(a => a.Id == couponId);
            if ((result != null) && (result.IsSyncWeiXin == 1))
            {
                result.WXCardInfo = base.Context.WXCardLogInfo.FirstOrDefault<WXCardLogInfo>(a => a.Id == result.CardLogId);
            }
            return result;
        }

        public List<CouponInfo> GetCouponInfo(long[] couponId)
        {
            return (from a in base.Context.CouponInfo
                    where couponId.Contains<long>(a.Id)
                    select a).ToList<CouponInfo>();
        }

        public CouponInfo GetCouponInfo(long shopId, long couponId)
        {
            CouponInfo result = base.Context.CouponInfo.FirstOrDefault<CouponInfo>(a => (a.ShopId == shopId) && (a.Id == couponId));
            if ((result != null) && (result.IsSyncWeiXin == 1))
            {
                result.WXCardInfo = base.Context.WXCardLogInfo.FirstOrDefault<WXCardLogInfo>(a => a.Id == result.CardLogId);
            }
            return result;
        }

        public ObsoletePageModel<CouponInfo> GetCouponList(CouponQuery query)
        {
            if (query.ShopId.HasValue)
            {
                if (query.ShopId <= 0)
                {
                    throw new HimallException("ShopId不能识别");
                }
            }
            int total = 0;
            int auditsuccess = (int)WXCardLogInfo.AuditStatusEnum.Audited;
            IQueryable<CouponInfo> coupon = Context.CouponInfo.AsQueryable();
            if (query.ShopId.HasValue)
            {
                coupon = coupon.Where(d => d.ShopId == query.ShopId);
            }
            if (query.IsShowAll != true)
            {
                coupon = coupon.Where(d => d.WXAuditStatus == auditsuccess);
            }
            if (query.ShowPlatform.HasValue)
            {
                coupon = coupon.Where(d => d.Himall_CouponSetting.Any(s => s.PlatForm == query.ShowPlatform.Value));
            }
            if (query.IsOnlyShowNormal == true)
            {
                DateTime curMindate = DateTime.Now.Date;
                DateTime curMaxdate = curMindate.AddDays(1).Date;
                coupon = coupon.Where(d => d.EndTime >= curMaxdate && d.StartTime <= curMindate);
            }
            if (!string.IsNullOrWhiteSpace(query.CouponName))
            {
                coupon = coupon.Where(d => d.CouponName.Contains(query.CouponName));
            }
            coupon = coupon.GetPage(out total, d => d.OrderByDescending(o => o.EndTime), query.PageNo, query.PageSize);
            ObsoletePageModel<CouponInfo> pageModel = new ObsoletePageModel<CouponInfo>() { Models = coupon, Total = total };
            return pageModel;
        }

        public IQueryable<CouponInfo> GetCouponList(long shopid)
        {
            int auditsuccess = 1;
            return (from item in base.Context.CouponInfo
                    where (item.ShopId == shopid) && (item.WXAuditStatus == auditsuccess)
                    select item);
        }

        public IQueryable<CouponInfo> GetCouponLists(long shopId)
        {
            DateTime Date = DateTime.Now;
            int auditsuccess = 1;
            return (from a in base.Context.CouponInfo
                    where ((a.ShopId == shopId) && (a.EndTime > Date)) && (a.WXAuditStatus == auditsuccess)
                    orderby a.Price descending
                    select a);
        }

        public CouponRecordInfo GetCouponRecordById(long id)
        {
            CouponRecordInfo result = base.Context.CouponRecordInfo.FirstOrDefault<CouponRecordInfo>(d => d.Id == id);
            if (result.WXCodeId.HasValue)
            {
                result.WXCardCodeInfo = base.Context.WXCardCodeLogInfo.FirstOrDefault<WXCardCodeLogInfo>(d => d.Id == result.WXCodeId.Value);
            }
            return result;
        }

        public CouponRecordInfo GetCouponRecordInfo(long userId, long orderId)
        {
            CouponRecordInfo result = base.Context.CouponRecordInfo.FirstOrDefault<CouponRecordInfo>(a => (a.UserId == userId) && (a.OrderId == orderId));
            if ((result != null) && result.WXCodeId.HasValue)
            {
                result.WXCardCodeInfo = base.Context.WXCardCodeLogInfo.FirstOrDefault<WXCardCodeLogInfo>(d => d.Id == result.WXCodeId.Value);
            }
            return result;
        }

        public ObsoletePageModel<CouponRecordInfo> GetCouponRecordList(CouponRecordQuery query)
        {
            int total = 0;
            var date = DateTime.Now;
            IQueryable<CouponRecordInfo> coupons = Context.CouponRecordInfo.AsQueryable();
            if (query.CouponId.HasValue)
            {
                coupons = coupons.Where(d => d.CouponId == query.CouponId);
            }
            if (query.UserId.HasValue)
            {
                coupons = coupons.Where(d => d.UserId == query.UserId.Value);
            }
            if (query.ShopId.HasValue)
            {
                coupons = coupons.Where(d => d.ShopId == query.ShopId.Value);
            }
            if (!string.IsNullOrWhiteSpace(query.UserName))
            {
                coupons = coupons.Where(d => d.UserName.Contains(query.UserName));
            }

            switch (query.Status)
            {
                case 0:
                    coupons = coupons.Where(item => item.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse && item.Himall_Coupon.EndTime > date);
                    break;
                case 1:
                    coupons = coupons.Where(item => item.CounponStatus == CouponRecordInfo.CounponStatuses.Used);
                    break;
                case 2:
                    coupons = coupons.Where(item => item.CounponStatus == CouponRecordInfo.CounponStatuses.Unuse && item.Himall_Coupon.EndTime <= date);
                    break;
            }
            coupons = coupons.GetPage(out total, query.PageNo, query.PageSize);
            ObsoletePageModel<CouponRecordInfo> pageModel = new ObsoletePageModel<CouponRecordInfo>() { Models = coupons, Total = total };
            return pageModel;
        }

        public List<CouponRecordInfo> GetCouponRecordTotal(long[] couponIds)
        {
            return (from d in base.Context.CouponRecordInfo
                    where couponIds.Contains<long>(d.CouponId)
                    select d).ToList<CouponRecordInfo>();
        }

        public ActiveMarketServiceInfo GetCouponService(long shopId)
        {
            if (shopId <= 0L)
            {
                throw new HimallException("ShopId不能识别");
            }
            return base.Context.ActiveMarketServiceInfo.FirstOrDefault<ActiveMarketServiceInfo>(m => ((m.ShopId == shopId) && (((int)m.TypeId) == 2)));
        }

        public ObsoletePageModel<CouponInfo> GetIntegralCoupons(int page, int pageSize)
        {
            ObsoletePageModel<CouponInfo> result = new ObsoletePageModel<CouponInfo>();
            DateTime CurDay = DateTime.Now;
            DateTime CurTime = DateTime.Now;
            int auditsuccess = (int)WXCardLogInfo.AuditStatusEnum.Audited;
            var sql = Context.CouponInfo.Where(d => d.ReceiveType == CouponInfo.CouponReceiveType.IntegralExchange
            && d.EndIntegralExchange >= CurTime && d.EndTime >= CurDay && d.StartTime <= CurDay
            && d.WXAuditStatus == auditsuccess && d.Num > 0);
            int total = 0;
            sql = sql.GetPage(out total, page, pageSize, d => d.OrderByDescending(o => o.CreateTime));
            result.Models = sql;
            result.Total = total;
            return result;
        }

        public IEnumerable<CouponRecordInfo> GetOrderCoupons(long userId, IEnumerable<long> Ids)
        {
            DateTime date = DateTime.Now;
            return LinqHelper.DistinctBy<CouponRecordInfo, long>((IEnumerable<CouponRecordInfo>)Queryable.Where<CouponRecordInfo>((IQueryable<CouponRecordInfo>)this.Context.CouponRecordInfo, (Expression<Func<CouponRecordInfo, bool>>)(a => a.UserId == userId && Enumerable.Contains<long>(Ids, a.Id) && (int)a.CounponStatus == 0 && a.Himall_Coupon.StartTime <= date && a.Himall_Coupon.EndTime > date)), (Func<CouponRecordInfo, long>)(a => a.ShopId));
        }

        public IEnumerable<CouponInfo> GetTopCoupon(long shopId, int top = 8, PlatformType type = 0)
        {
            var Date = DateTime.Now;
            int auditsuccess = (int)WXCardLogInfo.AuditStatusEnum.Audited;
            return Context.CouponInfo.Where(a => a.ShopId == shopId && a.EndTime >= Date && a.WXAuditStatus == auditsuccess && a.Himall_CouponSetting.Any(b => b.PlatForm == type)).OrderByDescending(a => a.Price).Take(top);
        }

        public List<CouponRecordInfo> GetUserCoupon(long shopId, long userId, decimal totalPrice)
        {
            DateTime date = DateTime.Now;
            return Enumerable.ToList<CouponRecordInfo>((IEnumerable<CouponRecordInfo>)Queryable.OrderByDescending<CouponRecordInfo, Decimal>(Queryable.Where<CouponRecordInfo>((IQueryable<CouponRecordInfo>)this.Context.CouponRecordInfo, (Expression<Func<CouponRecordInfo, bool>>)(item => item.ShopId == shopId && item.UserId == userId && (int)item.CounponStatus == 0 && item.Himall_Coupon.StartTime <= date && item.Himall_Coupon.EndTime > date && item.Himall_Coupon.OrderAmount <= totalPrice)), (Expression<Func<CouponRecordInfo, Decimal>>)(item => item.Himall_Coupon.Price)));
        }

        public int GetUserCouponCount(long shopId)
        {
            DynamicParameters parms = new DynamicParameters();
            parms.Add("@ShopId", shopId);
            parms.Add("@PlatForm", Core.PlatformType.Wap);
            StringBuilder countsql = new StringBuilder(2048);
            countsql.Append(" select count(1) from ( ");
            countsql.Append(" select A.Id,A.ShopId,A.CouponName,A.Num, ");
            countsql.Append(" (select count(1) from Himall_CouponRecord where CouponID=A.Id)AS hasNum from Himall_Coupon A ");
            countsql.Append(" left join Himall_CouponSetting S on A.Id=S.CouponID  ");
            countsql.Append(" where A.ShopId=@ShopId and A.EndTime>CURDATE() and S.PlatForm=@PlatForm and (S.Display=1 OR S.Display is NULL) ");
            countsql.Append(" order by A.Id desc )T where T.hasNum<T.Num ");/*此SQL还需优化*/
            using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
            {
                return Core.Helper.TypeHelper.ObjectToInt(conn.ExecuteScalar(string.Concat(countsql), parms));
            }
        }

        public IQueryable<UserCouponInfo> GetUserCouponList(long userid)
        {
            return (from a in base.Context.CouponInfo
                    join b in
                        from item in base.Context.CouponRecordInfo
                        where item.UserId == userid
                        select item on a.Id equals b.CouponId
                    select new UserCouponInfo { UserId = b.UserId, ShopId = a.ShopId, CouponId = a.Id, Price = a.Price, PerMax = a.PerMax, OrderAmount = a.OrderAmount, Num = a.Num, StartTime = a.StartTime, EndTime = a.EndTime, CreateTime = a.CreateTime, CouponName = a.CouponName, UseStatus = b.CounponStatus, UseTime = b.UsedTime, ShopName = a.ShopName });
        }

        public int GetUserReceiveCoupon(long couponId, long userId)
        {
            return base.Context.CouponRecordInfo.Count<CouponRecordInfo>(a => ((a.CouponId == couponId) && (a.UserId == userId)));
        }

        public void SaveCover(CouponInfo model)
        {
            string integralCover = model.IntegralCover;
            string str2 = string.Format("/Storage/Shop/{0}/Coupon/", model.ShopId);
            string str3 = ".png";
            string str4 = model.Id.ToString() + str3;
            string destFileName = Path.Combine(str2, str4);
            if ((integralCover != null) && integralCover.Contains("/temp/"))
            {
                HimallIO.CopyFile(integralCover.Substring(integralCover.LastIndexOf("/temp")), destFileName, true);
                model.IntegralCover = destFileName;
                base.Context.SaveChanges();
            }
        }

        public void SyncWeixinCardAudit(long id, string cardid, WXCardLogInfo.AuditStatusEnum auditstatus)
        {
            CouponInfo info = base.Context.CouponInfo.FirstOrDefault<CouponInfo>(d => d.Id == id);
            if (info != null)
            {
                info.WXAuditStatus = (int)auditstatus;
                base.Context.SaveChanges();
            }
        }

        public void UseCoupon(long userId, IEnumerable<long> Ids, IEnumerable<OrderInfo> orders)
        {
            DateTime date = DateTime.Now.Date;
            DbQuery<CouponRecordInfo> dbQuery = this.Context.CouponRecordInfo.Include("Himall_Coupon");
            Expression<Func<CouponRecordInfo, bool>> predicate = (Expression<Func<CouponRecordInfo, bool>>)(a => a.UserId == userId && Enumerable.Contains<long>(Ids, a.Id) && (int)a.CounponStatus == 0 && a.Himall_Coupon.EndTime > date);
            foreach (CouponRecordInfo couponRecordInfo in Enumerable.ToList<CouponRecordInfo>(LinqHelper.DistinctBy<CouponRecordInfo, long>((IEnumerable<CouponRecordInfo>)Queryable.Where<CouponRecordInfo>((IQueryable<CouponRecordInfo>)dbQuery, predicate), (Func<CouponRecordInfo, long>)(a => a.ShopId))))
            {
                CouponRecordInfo info = couponRecordInfo;
                info.CounponStatus = CouponRecordInfo.CounponStatuses.Used;
                info.UsedTime = new DateTime?(DateTime.Now);
                info.OrderId = new long?(Enumerable.FirstOrDefault<OrderInfo>(orders, (Func<OrderInfo, bool>)(a => a.ShopId == info.ShopId && a.ProductTotalAmount >= info.Himall_Coupon.OrderAmount)).Id);
                this.ser_wxcard.Consume(info.Id, this.ThisCouponType);
            }
            this.Context.SaveChanges();
        }

    }
}
