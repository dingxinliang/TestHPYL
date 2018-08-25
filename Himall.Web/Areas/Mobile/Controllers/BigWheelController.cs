using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Himall.DTO;
using Himall.CommonModel;
using System;
using Senparc.Weixin.MP.CommonAPIs;
using Himall.Core;
using Himall.Web.App_Code.Common;
using Himall.Application;
using Senparc.Weixin.MP.AdvancedAPIs.QrCode;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class BigWheelController : BaseMobileTemplatesController
    {
        // Fields
        private IBonusService _iBonusService;
        private IMemberIntegralService _iMemberIntegralService;
        private ISiteSettingService _iSiteSettingService;
        private SiteSettingsInfo _siteSetting;
        private WeiActivityWinModel activityWinModel;
        private int consumePoint;
        private long curUserId;

        // Methods
        public BigWheelController(IBonusService iBonusService, IMemberIntegralService iMemberIntegralService, ISiteSettingService iSiteSettingService)
        {
            this._iBonusService = iBonusService;
            this._iMemberIntegralService = iMemberIntegralService;
            this._iSiteSettingService = iSiteSettingService;
            this._siteSetting = this._iSiteSettingService.GetSiteSettings();
            if (string.IsNullOrWhiteSpace(this._siteSetting.WeixinAppId) || string.IsNullOrWhiteSpace(this._siteSetting.WeixinAppSecret))
            {
                throw new HimallException("未配置公众号参数");
            }
        }

        public ActionResult Add(long id, long userId)
        {
            WeiActivityModel activityModel = WeiActivityApplication.GetActivityModel(id);
            this.activityWinModel = new WeiActivityWinModel();
            int num = new Random().Next(1, 100);
            this.activityWinModel.activityId = id;
            this.activityWinModel.addDate = DateTime.Now;
            this.activityWinModel.userId = userId;
            this.activityWinModel.integrals = activityModel.consumePoint;
            this.activityWinModel.isWin = false;
            this.activityWinModel.awardId = 0L;
            this.activityWinModel.awardName = "未中奖";
            foreach (WeiActivityAwardModel model2 in activityModel.awards)
            {
                if (num <= this.GetCount(activityModel.Id, model2.Id))
                {
                    this.activityWinModel.isWin = true;
                    if (model2.awardType == WeiActivityAwardType.Integral)
                    {
                        this.activityWinModel.awardName = model2.integral.ToString();
                        this.activityWinModel.awardId = model2.Id;
                        this.activityWinModel.awardType = model2.awardType;
                        break;
                    }
                    if (model2.awardType == WeiActivityAwardType.Bonus)
                    {
                        BonusInfo info = this._iBonusService.Get(model2.bonusId);
                        if (info == null)
                        {
                            this.activityWinModel.isWin = false;
                        }
                        else
                        {
                            string bonusSurplus = this._iBonusService.GetBonusSurplus(model2.bonusId);
                            if (info.IsInvalid)
                            {
                                this.activityWinModel.isWin = false;
                            }
                            else if (Convert.ToInt32(bonusSurplus) <= 0)
                            {
                                this.activityWinModel.isWin = false;
                            }
                            else
                            {
                                this.activityWinModel.awardName = info.Name;
                                this.activityWinModel.awardId = model2.Id;
                                this.activityWinModel.awardType = model2.awardType;
                                this.activityWinModel.bonusId = model2.bonusId;
                            }
                        }
                        break;
                    }
                    if (model2.awardType == WeiActivityAwardType.Coupon)
                    {
                        CouponModel model3 = CouponApplication.Get(model2.couponId);
                        if (model3 == null)
                        {
                            this.activityWinModel.isWin = false;
                        }
                        else
                        {
                            int num2 = WeiActivityWinApplication.GetCouPonMax(this.activityWinModel.userId, this.activityWinModel.activityId, model2.Id);
                            if (model3.Num <= 0)
                            {
                                this.activityWinModel.isWin = false;
                            }
                            else if ((model3.perMax != 0) && (num2 >= model3.perMax))
                            {
                                this.activityWinModel.isWin = false;
                            }
                            else if (model3.EndTime < DateTime.Now)
                            {
                                this.activityWinModel.isWin = false;
                            }
                            else
                            {
                                string couponName = model3.CouponName;
                                if (model3.ShopName != "")
                                {
                                    couponName = couponName + "(" + model3.ShopName + ")";
                                }
                                if (model3.OrderAmount != "")
                                {
                                    couponName = couponName + "(" + model3.OrderAmount + ")";
                                }
                                this.activityWinModel.awardName = couponName;
                                this.activityWinModel.awardId = model2.Id;
                                this.activityWinModel.awardType = model2.awardType;
                                this.activityWinModel.couponId = model2.couponId;
                            }
                        }
                        break;
                    }
                }
            }
            WeiActivityWinApplication.WinnerSubmit(this.activityWinModel);
            decimal receivePriceByUserId = 0M;
            if (this.activityWinModel.bonusId > 0L)
            {
                receivePriceByUserId = this._iBonusService.GetReceivePriceByUserId(this.activityWinModel.bonusId, userId);
            }
            BaseController.Result data = new BaseController.Result
            {
                success = true,
                msg = this.activityWinModel.awardId.ToString() + "," + receivePriceByUserId.ToString()
            };
            return base.Json(data);
        }

        public double GetCount(long id, long awardId)
        {
            WeiActivityModel activityModel = WeiActivityApplication.GetActivityModel(id);
            double num = 0.0;
            float num2 = 0f;
            List<WeiActivityAwardModel> list2 = (from p in activityModel.awards
                                                 where p.Id == awardId
                                                 select p).ToList<WeiActivityAwardModel>();
            foreach (WeiActivityAwardModel model2 in activityModel.awards)
            {
                if (!WeiActivityWinApplication.GetWinNumberByAward(id, model2.Id))
                {
                    num += model2.proportion;
                }
                else if (model2.awardLevel < list2[0].awardLevel)
                {
                    num2 += model2.proportion;
                }
            }
            if (!WeiActivityWinApplication.GetWinNumberByAward(id, awardId))
            {
                return 0.0;
            }
            return ((num + num2) + list2[0].proportion);
        }

        public string GetCouponName(long id)
        {
            return CouponApplication.Get(id).CouponName;
        }

        public int GetParticipationCount(WeiActivityModel activityModel)
        {
            int winModel = WeiActivityWinApplication.GetWinModel(activityModel);
            if (winModel != -1)
            {
                return (activityModel.participationCount - winModel);
            }
            return winModel;
        }

        public ActionResult Index(long id)
        {
            WeiActivityModel activityModelByBigWheel = WeiActivityApplication.GetActivityModelByBigWheel(id);
            activityModelByBigWheel.userId = this.curUserId;
            int availableIntegrals = this._iMemberIntegralService.GetMemberIntegral(this.curUserId).AvailableIntegrals;
            activityModelByBigWheel.participationCount = this.GetParticipationCount(activityModelByBigWheel);
            this.consumePoint = activityModelByBigWheel.consumePoint;
            ((dynamic)base.ViewBag).Integrals = availableIntegrals;
            try
            {
                string ticket = QrCodeApi.Create(AccessTokenContainer.TryGetToken(this._siteSetting.WeixinAppId, this._siteSetting.WeixinAppSecret, false), 0x15180, 0x7661b3, 0x2710).ticket;
                ((dynamic)base.ViewBag).QRTicket = ticket;
            }
            catch
            {
            }
            return base.View(activityModelByBigWheel);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (base.CurrentUser != null)
            {
                this.curUserId = base.CurrentUser.Id;
            }
        }

    }
}