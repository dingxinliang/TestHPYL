﻿using System;
using System.Collections.Generic;
using System.Linq;
using Himall.DTO;
using Himall.IServices;
using Himall.Model;
using Himall.CommonModel;
using Himall.Core;

namespace Himall.Application
{
    public class WeiActivityWinApplication
    {
        private static IWeiActivityWinService _iWeiActivityWinService = ObjectContainer.Current.Resolve<IWeiActivityWinService>();
        public static void WinnerSubmit(WeiActivityWinModel winModel)
        {

            //添加中奖信息记录
            WeiActivityWinInfo activityWinInfo = new WeiActivityWinInfo();
            activityWinInfo.ActivityId = winModel.activityId;
            activityWinInfo.AddDate = DateTime.Now;
            activityWinInfo.UserId = winModel.userId;
            activityWinInfo.IsWin = winModel.isWin;
            if (activityWinInfo.IsWin)
            {
                if (winModel.awardType == WeiActivityAwardType.Integral)//积分
                {
                    activityWinInfo.AwardName = winModel.awardName + " 积分";
                    //新增积分记录
                    var factoryService = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create;
                    MemberIntegralRecord info = new MemberIntegralRecord();
                    info.UserName = MemberApplication.GetMember(activityWinInfo.UserId).UserName;
                    info.MemberId = activityWinInfo.UserId;
                    info.RecordDate = DateTime.Now;
                    info.ReMark = "微信活动中奖";
                    info.TypeId = MemberIntegral.IntegralType.WeiActivity;
                    var memberIntegral = factoryService.Create(MemberIntegral.IntegralType.WeiActivity, Convert.ToInt32(winModel.awardName));//winModel.integrals为消耗积分 此处不是用户总积分
                    MemberIntegralApplication.AddMemberIntegral(info, memberIntegral);
                }
                else if (winModel.awardType == WeiActivityAwardType.Bonus)//红包
                {
                    activityWinInfo.AwardName = winModel.awardName;
                    BonusApplication.SetShareByUserId(winModel.bonusId, false, winModel.userId);//添加红包记录
                    BonusApplication.IncrReceiveCount(winModel.bonusId);
                }
                else
                {
                    activityWinInfo.AwardName = winModel.awardName;
                    //添加优惠券记录
                    CouponRecordInfo recordInfo = new CouponRecordInfo();
                    recordInfo.UserId = winModel.userId;
                    recordInfo.UserName = MemberApplication.GetMember(winModel.userId).UserName;
                    recordInfo.ShopId = CouponApplication.GetCouponInfo(winModel.couponId).ShopId;
                    recordInfo.CouponId = winModel.couponId;
                    CouponApplication.AddCouponRecord(recordInfo);
                }
            }
            else
            {
                activityWinInfo.AwardName = winModel.awardName;
            }
            activityWinInfo.AwardId = winModel.awardId;
            _iWeiActivityWinService.AddWinner(activityWinInfo);

            //消耗积分
            if (winModel.integrals > 0)
            {
                var factoryService = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create;
                MemberIntegralRecord info = new MemberIntegralRecord();
                info.UserName = MemberApplication.GetMember(activityWinInfo.UserId).UserName;
                info.MemberId = activityWinInfo.UserId;
                info.RecordDate = DateTime.Now;
                info.ReMark = "微信活动消耗";
                info.TypeId = MemberIntegral.IntegralType.WeiActivity;
                var memberIntegral = factoryService.Create(MemberIntegral.IntegralType.WeiActivity, -winModel.integrals);
                MemberIntegralApplication.AddMemberIntegral(info, memberIntegral);
            }




        }

        public static ObsoletePageModel<WeiActivityWinModel> GetActivityWin(string text, long id, int pageIndex, int pageSize)
        {

            ObsoletePageModel<WeiActivityWinInfo> weiInfo = _iWeiActivityWinService.Get(text, id, pageIndex, pageSize);

            var datas = weiInfo.Models.ToList().Select(m => new WeiActivityWinModel()
            {
                userName = MemberApplication.GetMember(m.UserId).UserName,
                awardName = m.AwardName,
                addDate = m.AddDate
            }).ToList();

            ObsoletePageModel<WeiActivityWinModel> t = new ObsoletePageModel<WeiActivityWinModel>();
            t.Models = datas.AsQueryable();
            t.Total = weiInfo.Total;
            return t;
        }

        public static IEnumerable<BonusInfo> GetBonusByType(BonusInfo.BonusType bonusType)
        {
            return BonusApplication.GetBonusByType(bonusType);
        }

        public static string GetBonusSurplus(long bonusId)
        {
            return BonusApplication.GetBonusSurplus(bonusId);
        }

        public static int GetWinModel(WeiActivityModel model)
        {
            List<WeiActivityWinInfo> listWinModel = _iWeiActivityWinService.GetWinInfo(model.userId);
            listWinModel = listWinModel.Where(p => p.ActivityId == model.Id).ToList();
            if (model.participationType == WeiParticipateType.DayCount)
            {
                return listWinModel.Where(p => p.AddDate.Date == DateTime.Now.Date).Count();
            }
            else if (model.participationType == WeiParticipateType.CommonCount)
            {
                return listWinModel.Count();
            }
            else
            {
                return -1;
            }

        }
        public static bool GetWinNumberByAward(long id, long awardId)
        {
            bool flag = false;
            var item = WeiActivityApplication.GetActivityInfo(id);
            var awardItem = item.Himall_WeiActivityAward.Where(p => p.Id == awardId).ToList();
            int num = item.Himall_WeiActivityWinInfo.Where(p => p.AwardId == awardId).Count();
            foreach (var i in awardItem)
            {
                if (num < i.AwardCount)
                {
                    flag = true;
                }
            }

            return flag;
        }

        public static int GetCouPonMax(long userId, long activityId, long awardId)
        {
            var item = _iWeiActivityWinService.GetWinInfo(userId);
            return item.Where(p => p.ActivityId == activityId && p.AwardId == awardId).Count();
        }


       public static  string GetWinNumber(long activityId, string text)
        {
          return   _iWeiActivityWinService.GetWinNumber(activityId, text);
        }
    }
}
