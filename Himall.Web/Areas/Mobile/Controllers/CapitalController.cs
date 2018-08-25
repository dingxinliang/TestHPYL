using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Framework;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Core;
using Himall.Model;
using Senparc.Weixin.MP.CommonAPIs;
using Himall.Core.Plugins.Payment;
using Himall.Application;
using Himall.CommonModel;
using Himall.Core.Helper;
using Senparc.Weixin.MP.Entities;
using Himall.Core.Plugins;
using Himall.DTO;


namespace Himall.Web.Areas.Mobile.Controllers
{
	public class CapitalController : BaseMobileMemberController
	{
        // Fields
        private IMemberCapitalService _iMemberCapitalService;
        private ISiteSettingService _iSiteSettingService;
        private const string PLUGIN_OAUTH_WEIXIN = "Himall.Plugin.OAuth.WeiXin";

        // Methods
        public CapitalController(IMemberCapitalService iMemberCapitalService, ISiteSettingService iSiteSettingService)
        {
            this._iMemberCapitalService = iMemberCapitalService;
            this._iSiteSettingService = iSiteSettingService;
        }

        public JsonResult ApplyWithDrawSubmit(string openid, string nickname, decimal amount, string pwd, int applyType = 1)
        {
            if (!MemberApplication.VerificationPayPwd(base.CurrentUser.Id, pwd))
            {
                throw new HimallException("支付密码不对，请重新输入！");
            }
            if (!((applyType != UserWithdrawType.ALiPay.GetHashCode()) || base.CurrentSiteSetting.Withdraw_AlipayEnable))
            {
                throw new HimallException("不支持支付宝提现方式！");
            }
            CapitalInfo capitalInfo = this._iMemberCapitalService.GetCapitalInfo(base.CurrentUser.Id);
            if (amount > capitalInfo.Balance)
            {
                throw new HimallException("提现金额不能超出可用金额！");
            }
            if (amount <= 0M)
            {
                throw new HimallException("提现金额不能小于等于0！");
            }
            if (string.IsNullOrWhiteSpace(openid) && (applyType == UserWithdrawType.WeiChat.GetHashCode()))
            {
                openid = WebHelper.GetCookie("Himall-User_OpenId");
            }
            if (string.IsNullOrWhiteSpace(nickname) && (applyType == UserWithdrawType.ALiPay.GetHashCode()))
            {
                throw new HimallException("数据异常,真实姓名不可为空！");
            }
            if (!string.IsNullOrWhiteSpace(openid) && (applyType == UserWithdrawType.WeiChat.GetHashCode()))
            {
                openid = SecureHelper.AESDecrypt(openid, "Mobile");
                SiteSettingsInfo siteSettings = this._iSiteSettingService.GetSiteSettings();
                if (!string.IsNullOrWhiteSpace(siteSettings.WeixinAppId) && !string.IsNullOrWhiteSpace(siteSettings.WeixinAppSecret))
                {
                    WeixinUserInfoResult userInfo = CommonApi.GetUserInfo(AccessTokenContainer.TryGetToken(siteSettings.WeixinAppId, siteSettings.WeixinAppSecret, false), openid);
                    if (userInfo != null)
                    {
                        nickname = userInfo.nickname;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(openid))
            {
                throw new HimallException("数据异常,OpenId或收款账号不可为空！");
            }
            ApplyWithDrawInfo model = new ApplyWithDrawInfo
            {
                ApplyAmount = amount,
                ApplyStatus = ApplyWithDrawInfo.ApplyWithDrawStatus.WaitConfirm,
                ApplyTime = DateTime.Now,
                MemId = base.CurrentUser.Id,
                OpenId = openid,
                NickName = nickname,
                ApplyType = new UserWithdrawType?((UserWithdrawType)applyType)
            };
            this._iMemberCapitalService.AddWithDrawApply(model);
            return base.Json(new { success = true });
        }

        [HttpPost]
        public JsonResult CanWithDraw()
        {
            bool flag = false;
            bool flag2 = false;
            if (base.PlatformType == PlatformType.WeiXin)
            {
                flag = true;
            }
            if (base.CurrentSiteSetting.Withdraw_AlipayEnable)
            {
                flag2 = true;
            }
            return base.Json(new { success = flag ? true : flag2, canWeiXin = flag, canAlipay = flag2 });
        }

        [HttpPost]
        public JsonResult Charge(string pluginId, decimal amount)
        {
            amount = Math.Round(amount, 2);
            if (amount <= 0M)
            {
                return base.Json(new { success = false, msg = "请输入正确的金额" });
            }
            Plugin<IPaymentPlugin> plugin = PluginsManagement.GetPlugin<IPaymentPlugin>(pluginId);
            ChargeDetail model = new ChargeDetail
            {
                ChargeAmount = amount,
                ChargeStatus = ChargeDetailInfo.ChargeDetailStatus.WaitPay,
                ChargeWay = plugin.PluginInfo.DisplayName,
                CreateTime = DateTime.Now,
                MemId = base.CurrentUser.Id
            };
            long num = MemberCapitalApplication.AddChargeApply(model);
            string cookie = WebHelper.GetCookie("Himall-User_OpenId");
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                cookie = SecureHelper.AESDecrypt(cookie, "Mobile");
            }
            else
            {
                MemberOpenId id = MemberApplication.GetMemberOpenIdInfoByuserId(base.CurrentUser.Id, MemberOpenIdInfo.AppIdTypeEnum.Payment, "");
                if (id != null)
                {
                    cookie = id.OpenId;
                }
            }
            string str2 = base.Request.Url.Scheme + "://" + base.Request.Url.Authority;
            string notifyUrl = string.Concat(new object[] { str2, "/m-", base.PlatformType, "/Payment/CapitalChargeNotify/", plugin.PluginInfo.PluginId.Replace(".", "-") });
            string returnUrl = string.Concat(new object[] { str2, "/m-", base.PlatformType, "/Capital/Index" });
            string str5 = plugin.Biz.GetRequestUrl(returnUrl, notifyUrl, num.ToString(), amount, "会员充值", cookie);
            return base.Json(new { href = str5, success = true });
        }

        public ActionResult ChargeSuccess(string id)
        {
            Log.Info("pluginId:" + id);
            Plugin<IPaymentPlugin> plugin = PluginsManagement.GetPlugin<IPaymentPlugin>(id.Replace("-", "."));
            PaymentInfo info = plugin.Biz.ProcessNotify(base.HttpContext.Request);
            if (info != null)
            {
                long chargeDetailId = info.OrderIds.FirstOrDefault<long>();
                Log.Info("chargeApplyId:" + chargeDetailId);
                MemberCapitalApplication.ChargeSuccess(chargeDetailId);
                string content = plugin.Biz.ConfirmPayResult();
                return base.Content(content);
            }
            Log.Info("payInfo:为空");
            return base.Content(string.Empty);
        }

        public ActionResult Index()
        {
            if (base.Request.Url.AbsolutePath.EndsWith("/Capital/Index", StringComparison.OrdinalIgnoreCase) || base.Request.Url.AbsolutePath.EndsWith("/Capital", StringComparison.OrdinalIgnoreCase))
            {
                return this.Redirect(base.Url.RouteUrl("PayRoute") + "?area=mobile&platform=" + base.PlatformType.ToString() + "&controller=Capital&action=Index");
            }
            CapitalInfo capitalInfo = this._iMemberCapitalService.GetCapitalInfo(base.CurrentUser.Id);
            decimal num = 0M;
            if (capitalInfo != null)
            {
                num = (from e in capitalInfo.Himall_CapitalDetail
                       where e.SourceType == CapitalDetailInfo.CapitalDetailType.RedPacket
                       select e).Sum<CapitalDetailInfo>((Func<CapitalDetailInfo, decimal>)(e => e.Amount));
                ((dynamic)base.ViewBag).CapitalDetails = (from e in capitalInfo.Himall_CapitalDetail
                                                          orderby e.CreateTime descending
                                                          select e).Take<CapitalDetailInfo>(15);
            }
            ((dynamic)base.ViewBag).RedPacketAmount = num;
            ((dynamic)base.ViewBag).IsSetPwd = !string.IsNullOrWhiteSpace(base.CurrentUser.PayPwd);
            SiteSettingsInfo siteSettings = this._iSiteSettingService.GetSiteSettings();
            ((dynamic)base.ViewBag).WithDrawMinimum = siteSettings.WithDrawMinimum;
            ((dynamic)base.ViewBag).WithDrawMaximum = siteSettings.WithDrawMaximum;
            return base.View(capitalInfo);
        }

        public JsonResult List(int page, int rows)
        {
            IMemberCapitalService service = this._iMemberCapitalService;
            CapitalDetailQuery query = new CapitalDetailQuery
            {
                memberId = base.CurrentUser.Id,
                PageSize = rows,
                PageNo = page
            };
            ObsoletePageModel<CapitalDetailInfo> capitalDetails = service.GetCapitalDetails(query);
            IEnumerable<CapitalDetailModel> enumerable = from e in capitalDetails.Models.ToList<CapitalDetailInfo>() select new CapitalDetailModel { Id = e.Id, Amount = e.Amount, CapitalID = e.CapitalID, CreateTime = e.CreateTime.Value.ToString("yyyy-MM-dd HH:mm:ss"), SourceData = e.SourceData, SourceType = e.SourceType, Remark = e.SourceType.ToDescription(), PayWay = e.Remark };
            return base.Json(new { model = enumerable, total = capitalDetails.Total });
        }

        public JsonResult SetPayPwd(string pwd)
        {
            this._iMemberCapitalService.SetPayPwd(base.CurrentUser.Id, pwd);
            return base.Json(new { success = true, msg = "设置成功" });
        }

	}
}