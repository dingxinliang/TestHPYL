using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Framework;
using Himall.Model;
using Himall.IServices.QueryModel;
using Himall.IServices;
using Himall.Core;
using Himall.Web.Models;
using Himall.Web.Areas.Web.Models;
using Himall.Core.Plugins.Payment;
using Himall.Core.Plugins;
using Himall.CommonModel;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class CapitalController : BaseAdminController
    {
        IMemberCapitalService _iMemberCapitalService;
        IMemberService _iMemberService;
        ISiteSettingService _iSiteSettingService;
        IOperationLogService _iOperationLogService;
        public CapitalController(IMemberCapitalService iMemberCapitalService,
            IMemberService iMemberService,
            ISiteSettingService iSiteSettingService,
            IOperationLogService iOperationLogService)
        {
            _iMemberCapitalService = iMemberCapitalService;
            _iMemberService = iMemberService;
            _iSiteSettingService = iSiteSettingService;
            _iOperationLogService = iOperationLogService;
        }
        public ActionResult Index()
        {
            return (ActionResult)this.View();
        }

        public JsonResult GetMemberCapitals(string user, int page, int rows)
        {
            IMemberCapitalService memberCapitalService = this._iMemberCapitalService;
            IMemberService memberService = this._iMemberService;
            long? nullable1 = new long?();
            if (!string.IsNullOrWhiteSpace(user))
            {
                UserMemberInfo userMemberInfo = memberService.GetMemberByName(user);
                if (userMemberInfo == null)
                    userMemberInfo = new UserMemberInfo()
                    {
                        Id = 0L
                    };
                nullable1 = new long?(userMemberInfo.Id);
            }
            CapitalQuery capitalQuery = new CapitalQuery();
            capitalQuery.PageNo = page;
            capitalQuery.PageSize = rows;
            capitalQuery.Sort = "Balance";
            capitalQuery.memberId = nullable1;
            CapitalQuery query = capitalQuery;
            ObsoletePageModel<CapitalInfo> capitals = memberCapitalService.GetCapitals(query);
            IEnumerable<CapitalModel> enumerable = Enumerable.Select<CapitalInfo, CapitalModel>((IEnumerable<CapitalInfo>)Enumerable.ToList<CapitalInfo>((IEnumerable<CapitalInfo>)capitals.Models), (Func<CapitalInfo, CapitalModel>)(e =>
            {
                UserMemberInfo member = memberService.GetMember(e.MemId);
                CapitalModel capitalModel1 = new CapitalModel();
                capitalModel1.Balance = e.Balance.Value;
                CapitalModel capitalModel2 = capitalModel1;
                Decimal? nullable2;
                Decimal num1;
                if (!e.ChargeAmount.HasValue)
                {
                    num1 = new Decimal(0, 0, 0, false, (byte)2);
                }
                else
                {
                    nullable2 = e.ChargeAmount;
                    num1 = nullable2.Value;
                }
                capitalModel2.ChargeAmount = num1;
                CapitalModel capitalModel3 = capitalModel1;
                nullable2 = e.FreezeAmount;
                Decimal num2;
                if (!nullable2.HasValue)
                {
                    num2 = new Decimal(0, 0, 0, false, (byte)2);
                }
                else
                {
                    nullable2 = e.FreezeAmount;
                    num2 = nullable2.Value;
                }
                capitalModel3.FreezeAmount = num2;
                capitalModel1.Id = e.Id;
                capitalModel1.UserId = e.MemId;
                capitalModel1.UserCode = member.UserName;
                capitalModel1.UserName = string.IsNullOrWhiteSpace(member.RealName) ? member.UserName : member.RealName;
                return capitalModel1;
            }));
            return this.Json((object)new DataGridModel<CapitalModel>()
            {
                rows = enumerable,
                total = capitals.Total
            });
        }

        public ActionResult Detail(long id)
        {
            ViewBag.UserId = id;
            return base.View();
        }

        public ActionResult WithDraw()
        {
            return (ActionResult)this.View();
        }

        public ActionResult AlipayWithDraw()
        {
            return (ActionResult)this.View();
        }

        public JsonResult List(CapitalDetailInfo.CapitalDetailType capitalType, long userid, string startTime, string endTime, int page, int rows)
        {
            IMemberCapitalService service = this._iMemberCapitalService;
            CapitalDetailQuery query = new CapitalDetailQuery
            {
                memberId = userid,
                capitalType = new CapitalDetailInfo.CapitalDetailType?(capitalType),
                PageSize = rows,
                PageNo = page
            };
            if (!string.IsNullOrWhiteSpace(startTime))
            {
                query.startTime = new DateTime?(DateTime.Parse(startTime));
            }
            if (!string.IsNullOrWhiteSpace(endTime))
            {
                query.endTime = new DateTime?(DateTime.Parse(endTime).AddDays(1.0).AddSeconds(-1.0));
            }
            ObsoletePageModel<CapitalDetailInfo> capitalDetails = service.GetCapitalDetails(query);
            List<CapitalDetailModel> list = (from e in capitalDetails.Models.ToList<CapitalDetailInfo>() select new CapitalDetailModel { Id = e.Id, Amount = e.Amount, CapitalID = e.CapitalID, CreateTime = e.CreateTime.Value.ToString(), SourceData = e.SourceData, SourceType = e.SourceType, Remark = e.SourceType.ToDescription() + ",单号：" + e.Id, PayWay = e.Remark }).ToList<CapitalDetailModel>();
            DataGridModel<CapitalDetailModel> data = new DataGridModel<CapitalDetailModel>
            {
                rows = list,
                total = capitalDetails.Total
            };
            return base.Json(data);
        }

        public JsonResult ApplyWithDrawListByUser(long userid, UserWithdrawType? applyType, int page, int rows)
        {
            IMemberCapitalService service = this._iMemberCapitalService;
            ApplyWithDrawQuery query = new ApplyWithDrawQuery
            {
                memberId = new long?(userid),
                ApplyType = applyType,
                PageSize = rows,
                PageNo = page
            };
            ObsoletePageModel<ApplyWithDrawInfo> applyWithDraw = service.GetApplyWithDraw(query);
            IEnumerable<ApplyWithDrawModel> enumerable = applyWithDraw.Models.ToList<ApplyWithDrawInfo>().Select<ApplyWithDrawInfo, ApplyWithDrawModel>(delegate(ApplyWithDrawInfo e)
            {
                string str = string.Empty;
                if ((e.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.PayFail) || (e.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.WaitConfirm))
                {
                    str = "提现中";
                }
                else if (e.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.Refuse)
                {
                    str = "提现失败";
                }
                else if (e.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.WithDrawSuccess)
                {
                    str = "提现成功";
                }
                return new ApplyWithDrawModel { Id = e.Id, ApplyAmount = e.ApplyAmount, ApplyStatus = e.ApplyStatus, ApplyStatusDesc = str, ApplyTime = e.ApplyTime.ToString(), ApplyType = e.ApplyType };
            });
            DataGridModel<ApplyWithDrawModel> data = new DataGridModel<ApplyWithDrawModel>
            {
                rows = enumerable,
                total = applyWithDraw.Total
            };
            return base.Json(data);
        }

        public JsonResult ApplyWithDrawList(ApplyWithDrawInfo.ApplyWithDrawStatus capitalType, UserWithdrawType? applyType, string withdrawno, string user, int page, int rows)
        {
            IMemberCapitalService memberCapitalService = this._iMemberCapitalService;
            IMemberService memberService = this._iMemberService;
            long? nullable = new long?();
            if (!string.IsNullOrWhiteSpace(user))
            {
                UserMemberInfo userMemberInfo = memberService.GetMemberByName(user);
                if (userMemberInfo == null)
                    userMemberInfo = new UserMemberInfo()
                    {
                        Id = 0L
                    };
                nullable = new long?(userMemberInfo.Id);
            }
            ApplyWithDrawQuery applyWithDrawQuery = new ApplyWithDrawQuery();
            applyWithDrawQuery.memberId = nullable;
            applyWithDrawQuery.ApplyType = applyType;
            applyWithDrawQuery.PageSize = rows;
            applyWithDrawQuery.PageNo = page;
            applyWithDrawQuery.withDrawStatus = new ApplyWithDrawInfo.ApplyWithDrawStatus?(capitalType);
            ApplyWithDrawQuery query = applyWithDrawQuery;
            if (!string.IsNullOrWhiteSpace(withdrawno))
                query.withDrawNo = new long?(long.Parse(withdrawno));
            ObsoletePageModel<ApplyWithDrawInfo> applyWithDraw = memberCapitalService.GetApplyWithDraw(query);
            IEnumerable<ApplyWithDrawModel> enumerable = Enumerable.Select<ApplyWithDrawInfo, ApplyWithDrawModel>((IEnumerable<ApplyWithDrawInfo>)Enumerable.ToList<ApplyWithDrawInfo>((IEnumerable<ApplyWithDrawInfo>)applyWithDraw.Models), (Func<ApplyWithDrawInfo, ApplyWithDrawModel>)(e =>
            {
                string str = EnumHelper.ToDescription((Enum)e.ApplyStatus);
                UserMemberInfo member = memberService.GetMember(e.MemId);
                return new ApplyWithDrawModel()
                {
                    Id = e.Id,
                    ApplyAmount = e.ApplyAmount,
                    ApplyStatus = e.ApplyStatus,
                    ApplyStatusDesc = str,
                    ApplyTime = e.ApplyTime.ToString(),
                    NickName = e.NickName,
                    MemberName = member.UserName,
                    ConfirmTime = e.ConfirmTime.ToString(),
                    MemId = e.MemId,
                    OpUser = e.OpUser,
                    PayNo = e.PayNo,
                    PayTime = e.PayTime.ToString(),
                    Remark = e.Remark
                };
            }));
            return this.Json((object)new DataGridModel<ApplyWithDrawModel>()
            {
                rows = enumerable,
                total = applyWithDraw.Total
            });
        }

        [HttpPost]
        public JsonResult ConfirmApply(long id, ApplyWithDrawInfo.ApplyWithDrawStatus comfirmStatus, string remark)
        {
            IMemberCapitalService memberCapitalService = this._iMemberCapitalService;
            ApplyWithDrawInfo.ApplyWithDrawStatus status = comfirmStatus;
            ApplyWithDrawInfo applyWithDrawInfo1 = memberCapitalService.GetApplyWithDrawInfo(id);
            if (status == ApplyWithDrawInfo.ApplyWithDrawStatus.Refuse)
            {
                memberCapitalService.RefuseApplyWithDraw(id, status, this.CurrentManager.UserName, remark);
                this._iOperationLogService.AddPlatformOperationLog(new LogInfo()
                {
                    Date = DateTime.Now,
                    Description = string.Format("会员提现审核拒绝，会员Id={0},状态为：{1}, 说明是：{2}", (object)applyWithDrawInfo1.MemId, (object)status, (object)remark),
                    IPAddress = this.Request.UserHostAddress,
                    PageUrl = "/Admin/Capital/WithDraw",
                    UserName = this.CurrentManager.UserName,
                    ShopId = 0L
                });
                return this.Json((object)new BaseController.Result()
                {
                    success = true,
                    msg = "审核成功！"
                });
            }
            UserWithdrawType? applyType = applyWithDrawInfo1.ApplyType;
            if ((applyType.GetValueOrDefault() != UserWithdrawType.ALiPay ? 0 : (applyType.HasValue ? 1 : 0)) != 0)
            {
                if (applyWithDrawInfo1.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.PayPending)
                    return this.Json((object)new BaseController.Result()
                    {
                        success = false,
                        msg = "等待第三方处理中，如有误操作，请先取消后再进行付款操作！"
                    });
                Plugin<IPaymentPlugin> plugin = Enumerable.FirstOrDefault<Plugin<IPaymentPlugin>>(PluginsManagement.GetPlugins<IPaymentPlugin>(true), (Func<Plugin<IPaymentPlugin>, bool>)(e => e.PluginInfo.PluginId == "Himall.Plugin.Payment.Alipay"));
                if (plugin != null)
                {
                    try
                    {
                        string format = CurrentUrlHelper.CurrentUrlNoPort() + "/Pay/EnterpriseNotify/{0}?outid={1}";
                        EnterprisePayPara para = new EnterprisePayPara()
                        {
                            amount = applyWithDrawInfo1.ApplyAmount,
                            check_name = false,
                            openid = applyWithDrawInfo1.OpenId,
                            re_user_name = applyWithDrawInfo1.NickName,
                            out_trade_no = applyWithDrawInfo1.Id.ToString(),
                            desc = "提现",
                            notify_url = string.Format(format, (object)this.EncodePaymentId(plugin.PluginInfo.PluginId), (object)applyWithDrawInfo1.Id.ToString())
                        };
                        PaymentInfo paymentInfo = plugin.Biz.EnterprisePay(para);
                        ApplyWithDrawInfo applyWithDrawInfo2 = new ApplyWithDrawInfo();
                        applyWithDrawInfo2.PayNo = paymentInfo.TradNo;
                        applyWithDrawInfo2.ApplyStatus = ApplyWithDrawInfo.ApplyWithDrawStatus.PayPending;
                        ApplyWithDrawInfo applyWithDrawInfo3 = applyWithDrawInfo2;
                        DateTime? tradeTime = paymentInfo.TradeTime;
                        DateTime now;
                        if (!tradeTime.HasValue)
                        {
                            now = DateTime.Now;
                        }
                        else
                        {
                            tradeTime = paymentInfo.TradeTime;
                            now = tradeTime.Value;
                        }
                        DateTime? nullable = new DateTime?(now);
                        applyWithDrawInfo3.PayTime = nullable;
                        applyWithDrawInfo2.ConfirmTime = new DateTime?(DateTime.Now);
                        applyWithDrawInfo2.OpUser = this.CurrentManager.UserName;
                        applyWithDrawInfo2.ApplyAmount = applyWithDrawInfo1.ApplyAmount;
                        applyWithDrawInfo2.Id = applyWithDrawInfo1.Id;
                        ApplyWithDrawInfo info = applyWithDrawInfo2;
                        memberCapitalService.ConfirmApplyWithDraw(info);
                        this._iOperationLogService.AddPlatformOperationLog(new LogInfo()
                        {
                            Date = DateTime.Now,
                            Description = string.Format("会员提现审核成功，会员Id={0},状态为：{1}, 说明是：{2}", (object)applyWithDrawInfo1.MemId, (object)status, (object)remark),
                            IPAddress = this.Request.UserHostAddress,
                            PageUrl = "/Admin/Capital/WithDraw",
                            UserName = this.CurrentManager.UserName,
                            ShopId = 0L
                        });
                        return this.Json((object)new BaseController.Result()
                        {
                            success = true,
                            msg = "审核操作成功",
                            status = 2,
                            Data = (object)paymentInfo.ResponseContentWhenFinished
                        });
                    }
                    catch (PluginException ex)
                    {
                        Log.Error((object)("调用企业付款接口异常：" + ex.Message));
                        this._iOperationLogService.AddPlatformOperationLog(new LogInfo()
                        {
                            Date = DateTime.Now,
                            Description = string.Format("会员提现审核失败，会员Id={0},状态为：{1}, 说明是：{2}", (object)applyWithDrawInfo1.MemId, (object)status, (object)remark),
                            IPAddress = this.Request.UserHostAddress,
                            PageUrl = "/Admin/Capital/WithDraw",
                            UserName = this.CurrentManager.UserName,
                            ShopId = 0L
                        });
                        return this.Json((object)new BaseController.Result()
                        {
                            success = false,
                            msg = ex.Message
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.Error((object)("提现审核异常：" + ex.Message));
                        return this.Json((object)new BaseController.Result()
                        {
                            success = false,
                            msg = "付款接口异常"
                        });
                    }
                }
                else
                    return this.Json((object)new BaseController.Result()
                    {
                        success = false,
                        msg = "未找到支付插件"
                    });
            }
            else
            {
                Plugin<IPaymentPlugin> plugin = Enumerable.FirstOrDefault<Plugin<IPaymentPlugin>>(Enumerable.Where<Plugin<IPaymentPlugin>>(PluginsManagement.GetPlugins<IPaymentPlugin>(true), (Func<Plugin<IPaymentPlugin>, bool>)(e => e.PluginInfo.PluginId.ToLower().Contains("weixin"))));
                if (plugin != null)
                {
                    try
                    {
                        EnterprisePayPara para = new EnterprisePayPara()
                        {
                            amount = applyWithDrawInfo1.ApplyAmount,
                            check_name = false,
                            openid = applyWithDrawInfo1.OpenId,
                            out_trade_no = applyWithDrawInfo1.Id.ToString(),
                            desc = "提现"
                        };
                        PaymentInfo paymentInfo = plugin.Biz.EnterprisePay(para);
                        ApplyWithDrawInfo applyWithDrawInfo2 = new ApplyWithDrawInfo();
                        applyWithDrawInfo2.PayNo = paymentInfo.TradNo;
                        applyWithDrawInfo2.ApplyStatus = ApplyWithDrawInfo.ApplyWithDrawStatus.WithDrawSuccess;
                        applyWithDrawInfo2.Remark = plugin.PluginInfo.Description;
                        ApplyWithDrawInfo applyWithDrawInfo3 = applyWithDrawInfo2;
                        DateTime? tradeTime = paymentInfo.TradeTime;
                        DateTime now;
                        if (!tradeTime.HasValue)
                        {
                            now = DateTime.Now;
                        }
                        else
                        {
                            tradeTime = paymentInfo.TradeTime;
                            now = tradeTime.Value;
                        }
                        DateTime? nullable = new DateTime?(now);
                        applyWithDrawInfo3.PayTime = nullable;
                        applyWithDrawInfo2.ConfirmTime = new DateTime?(DateTime.Now);
                        applyWithDrawInfo2.OpUser = this.CurrentManager.UserName;
                        applyWithDrawInfo2.ApplyAmount = applyWithDrawInfo1.ApplyAmount;
                        applyWithDrawInfo2.Id = applyWithDrawInfo1.Id;
                        ApplyWithDrawInfo info = applyWithDrawInfo2;
                        memberCapitalService.ConfirmApplyWithDraw(info);
                        this._iOperationLogService.AddPlatformOperationLog(new LogInfo()
                        {
                            Date = DateTime.Now,
                            Description = string.Format("会员提现审核成功，会员Id={0},状态为：{1}, 说明是：{2}", (object)applyWithDrawInfo1.MemId, (object)status, (object)remark),
                            IPAddress = this.Request.UserHostAddress,
                            PageUrl = "/Admin/Capital/WithDraw",
                            UserName = this.CurrentManager.UserName,
                            ShopId = 0L
                        });
                    }
                    catch (PluginException ex)
                    {
                        Log.Error((object)("调用企业付款接口异常：" + ex.Message));
                        this._iOperationLogService.AddPlatformOperationLog(new LogInfo()
                        {
                            Date = DateTime.Now,
                            Description = string.Format("会员提现审核失败，会员Id={0},状态为：{1}, 说明是：{2}", (object)applyWithDrawInfo1.MemId, (object)status, (object)remark),
                            IPAddress = this.Request.UserHostAddress,
                            PageUrl = "/Admin/Capital/WithDraw",
                            UserName = this.CurrentManager.UserName,
                            ShopId = 0L
                        });
                        return this.Json((object)new BaseController.Result()
                        {
                            success = false,
                            msg = ex.Message
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.Error((object)("提现审核异常：" + ex.Message));
                        ApplyWithDrawInfo info = new ApplyWithDrawInfo()
                        {
                            ApplyStatus = ApplyWithDrawInfo.ApplyWithDrawStatus.PayFail,
                            Remark = plugin.PluginInfo.Description,
                            ConfirmTime = new DateTime?(DateTime.Now),
                            OpUser = this.CurrentManager.UserName,
                            ApplyAmount = applyWithDrawInfo1.ApplyAmount,
                            Id = applyWithDrawInfo1.Id
                        };
                        memberCapitalService.ConfirmApplyWithDraw(info);
                        this._iOperationLogService.AddPlatformOperationLog(new LogInfo()
                        {
                            Date = DateTime.Now,
                            Description = string.Format("会员提现审核失败，会员Id={0},状态为：{1}, 说明是：{2}", (object)applyWithDrawInfo1.MemId, (object)status, (object)remark),
                            IPAddress = this.Request.UserHostAddress,
                            PageUrl = "/Admin/Capital/WithDraw",
                            UserName = this.CurrentManager.UserName,
                            ShopId = 0L
                        });
                        return this.Json((object)new BaseController.Result()
                        {
                            success = false,
                            msg = "付款接口异常"
                        });
                    }
                    return this.Json((object)new BaseController.Result()
                    {
                        success = true,
                        msg = "审核操作成功"
                    });
                }
                return this.Json((object)new BaseController.Result()
                {
                    success = false,
                    msg = "未找到支付插件"
                });
            }
        }

        private string EncodePaymentId(string paymentId)
        {
            return paymentId.Replace(".", "-");
        }

        public JsonResult Pay(long id)
        {
            return this.Json((object)new BaseController.Result()
            {
                success = true,
                msg = "付款成功"
            });
        }

        public ActionResult Setting()
        {
            return (ActionResult)this.View((object)this.CurrentSiteSetting);
        }

        public JsonResult SaveWithDrawSetting(string minimum, string maximum, bool alipayEnable)
        {
            int result1 = 0;
            int result2 = 0;
            if (int.TryParse(minimum, out result1) && int.TryParse(maximum, out result2))
            {
                if (result1 > 0 && result1 < result2 && result2 <= 20000)
                {
                    this._iSiteSettingService.SaveSetting("WithDrawMaximum", maximum);
                    this._iSiteSettingService.SaveSetting("WithDrawMinimum", minimum);
                    this._iSiteSettingService.SaveSetting("Withdraw_AlipayEnable", alipayEnable == true ? 1 : 0);
                    return this.Json((object)new BaseController.Result()
                    {
                        success = true,
                        msg = "保存成功"
                    });
                }
                return this.Json((object)new BaseController.Result()
                {
                    success = false,
                    msg = "金额范围只能是(1-20000)"
                });
            }
            return this.Json((object)new BaseController.Result()
            {
                success = false,
                msg = "只能输入数字"
            });
        }

        public JsonResult CancelPay(long Id)
        {
            string str = "ok";
            bool flag;
            if (Id > 0L)
            {
                flag = this._iMemberCapitalService.CancelPay(Id);
                if (!flag)
                    str = "取消失败";
            }
            else
            {
                flag = false;
                str = "数据错误";
            }
            return this.Json((object)new BaseController.Result()
            {
                success = flag,
                msg = str
            });
        }
    }
}