using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Web.Routing;
using Himall.Core.Plugins;
using System.Collections;
using Himall.Application;
using Newtonsoft.Json;

namespace Himall.Web.Framework
{

    public abstract class BaseController : Controller
    {
        // Fields
        private UserMemberInfo _currentUser;
        private SiteSettingsInfo _sitesetting;
        public bool IsAutoJumpMobile = false;

        // Methods
        public BaseController()
        {
            HttpContext current = System.Web.HttpContext.Current;
            if (!this.IsInstalled())
            {
                base.RedirectToAction("/Web/Installer/Agreement");
            }
        }

        public void ClearDistributionUserLinkId()
        {
            WebHelper.DeleteCookie("d2cccb104922d434");
        }

        protected void DisposeService(ControllerContext filterContext)
        {
            if (!filterContext.IsChildAction)
            {
                List<IService> list = filterContext.HttpContext.Session["_serviceInstace"] as List<IService>;
                if (list != null)
                {
                    foreach (IService service in list)
                    {
                        try
                        {
                            service.Dispose();
                        }
                        catch (Exception exception)
                        {
                            Log.Error(service.GetType().ToString() + "释放失败！", exception);
                        }
                    }
                    filterContext.HttpContext.Session["_serviceInstace"] = null;
                }
            }
        }

        public virtual ExcelResult ExcelView(string fileName, object model)
        {
            return this.ExcelView(null, fileName, model);
        }

        public virtual ExcelResult ExcelView(string viewName, string fileName, object model)
        {
            return new ExcelResult(viewName, fileName, model);
        }

        protected Exception GerInnerException(Exception ex)
        {
            if (ex.InnerException != null)
            {
                return this.GerInnerException(ex.InnerException);
            }
            return ex;
        }

        public List<long> GetDistributionUserLinkId()
        {
            List<long> list = new List<long>();
            string cookie = WebHelper.GetCookie("d2cccb104922d434");
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                string[] strArray = cookie.Split(new char[] { ',' });
                long result = 0L;
                foreach (string str2 in strArray)
                {
                    if (long.TryParse(str2, out result) && (result > 0L))
                    {
                        list.Add(result);
                    }
                }
            }
            return list;
        }

        private List<string> GetEntityValidationErrorMessage(Exception exception)
        {
            List<string> list = new List<string>();
            dynamic obj2 = exception;
            foreach (dynamic obj3 in (IEnumerable)obj2.EntityValidationErrors)
            {
                foreach (dynamic obj4 in (IEnumerable)obj3.ValidationErrors)
                {
                    list.Add(obj4.ErrorMessage);
                }
            }
            return list;
        }

        public JumpUrlRoute GetRouteUrl(string controller, string action, string area, string url)
        {
            Predicate<JumpUrlRoute> match = null;
            Predicate<JumpUrlRoute> predicate2 = null;
            Predicate<JumpUrlRoute> predicate3 = null;
            this.InitJumpUrlRoute();
            JumpUrlRoute route = null;
            url = url.ToLower();
            controller = controller.ToLower();
            action = action.ToLower();
            area = area.ToLower();
            List<JumpUrlRoute> jumpUrlRouteData = this.JumpUrlRouteData;
            if (!string.IsNullOrWhiteSpace(area))
            {
                if (match == null)
                {
                    match = d => d.Area.ToLower() == area;
                }
                jumpUrlRouteData = jumpUrlRouteData.FindAll(match);
            }
            if (!string.IsNullOrWhiteSpace(controller))
            {
                if (predicate2 == null)
                {
                    predicate2 = d => d.Controller.ToLower() == controller;
                }
                jumpUrlRouteData = jumpUrlRouteData.FindAll(predicate2);
            }
            if (!string.IsNullOrWhiteSpace(action))
            {
                if (predicate3 == null)
                {
                    predicate3 = d => d.Action.ToLower() == action;
                }
                jumpUrlRouteData = jumpUrlRouteData.FindAll(predicate3);
            }
            route = (jumpUrlRouteData.Count > 0) ? jumpUrlRouteData[0] : null;
            if (route == null)
            {
                route = new JumpUrlRoute
                {
                    PC = url,
                    WAP = url,
                    WX = url
                };
            }
            return route;
        }

        public static UserMemberInfo GetUser(HttpRequestBase request)
        {
            long id = UserCookieEncryptHelper.Decrypt(WebHelper.GetCookie("Himall-User"), "2");
            if (id == 0L)
            {
                string userIdCookie = request.QueryString["token"];
                id = UserCookieEncryptHelper.Decrypt(userIdCookie, "2");
                if (id != 0L)
                {
                    WebHelper.SetCookie("Himall-User", userIdCookie);
                }
            }
            if (id != 0L)
            {
                return MemberApplication.GetUserByCache(id);
            }
            return null;
        }

        public void InitJumpUrlRoute()
        {
            this._JumpUrlRouteData = new List<JumpUrlRoute>();
            JumpUrlRoute item = new JumpUrlRoute
            {
                Action = "Index",
                Area = "Web",
                Controller = "UserOrder",
                PC = "/userorder",
                WAP = "/member/orders",
                WX = "/member/orders"
            };
            this._JumpUrlRouteData.Add(item);
            item = new JumpUrlRoute
            {
                Action = "Index",
                Area = "Web",
                Controller = "UserCenter",
                PC = "/usercenter",
                WAP = "/member/center",
                WX = "/member/center"
            };
            this._JumpUrlRouteData.Add(item);
            item = new JumpUrlRoute
            {
                Action = "Index",
                Area = "Web",
                Controller = "Login",
                PC = "/login",
                WAP = "/login/entrance",
                WX = "/login/entrance"
            };
            this._JumpUrlRouteData.Add(item);
            item = new JumpUrlRoute
            {
                Action = "Home",
                Area = "Web",
                Controller = "Shop",
                PC = "/shop",
                WAP = "/vshop/detail",
                WX = "/vshop/detail",
                IsSpecial = true
            };
            this._JumpUrlRouteData.Add(item);
            item = new JumpUrlRoute
            {
                Action = "Submit",
                Area = "Web",
                Controller = "Order",
                PC = "/order/submit",
                WAP = "/order/SubmiteByCart",
                WX = "/order/SubmiteByCart",
                IsSpecial = true
            };
            this._JumpUrlRouteData.Add(item);
        }

        protected void InitVisitorTerminal()
        {
            VisitorTerminal terminal = new VisitorTerminal();
            string userAgent = base.Request.UserAgent;
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                userAgent = "";
            }
            userAgent = userAgent.ToLower();
            bool flag = userAgent.Contains("ipad");
            bool flag2 = userAgent.Contains("iphone os");
            bool flag3 = userAgent.Contains("midp");
            bool flag4 = userAgent.Contains("rv:1.2.3.4");
            flag4 = flag4 ? flag4 : userAgent.Contains("ucweb");
            bool flag5 = userAgent.Contains("android");
            bool flag6 = userAgent.Contains("windows ce");
            bool flag7 = userAgent.Contains("windows mobile");
            bool flag8 = userAgent.Contains("micromessenger");
            bool flag9 = userAgent.Contains("windows phone ");
            bool flag10 = userAgent.Contains("appwebview(ios)");
            terminal.Terminal = EnumVisitorTerminal.PC;
            if ((((flag || flag2) || (flag3 || flag4)) || ((flag5 || flag6) || flag7)) || flag9)
            {
                terminal.Terminal = EnumVisitorTerminal.Moblie;
            }
            if (flag || flag2)
            {
                terminal.OperaSystem = EnumVisitorOperaSystem.IOS;
                terminal.Terminal = EnumVisitorTerminal.Moblie;
                if (flag)
                {
                    terminal.Terminal = EnumVisitorTerminal.PAD;
                }
                if (flag10)
                {
                    terminal.Terminal = EnumVisitorTerminal.IOS;
                }
            }
            if (flag5)
            {
                terminal.OperaSystem = EnumVisitorOperaSystem.Android;
                terminal.Terminal = EnumVisitorTerminal.Moblie;
            }
            if (flag8)
            {
                terminal.Terminal = EnumVisitorTerminal.WeiXin;
            }
            if ((((terminal.Terminal == EnumVisitorTerminal.Moblie) || (terminal.Terminal == EnumVisitorTerminal.PAD)) || (terminal.Terminal == EnumVisitorTerminal.WeiXin)) || (terminal.Terminal == EnumVisitorTerminal.IOS))
            {
                this.IsMobileTerminal = true;
            }
            this.visitorTerminalInfo = terminal;
        }

        private bool IsExistPage(string url)
        {
            bool flag = false;
            HttpWebResponse response = WebHelper.GetURLResponse(url, "get", "", null, 0x4e20);
            if ((response != null) && (((response.StatusCode == HttpStatusCode.OK) || (response.StatusCode == HttpStatusCode.Found)) || (response.StatusCode == HttpStatusCode.MovedPermanently)))
            {
                flag = true;
            }
            return flag;
        }

        private bool IsInstalled()
        {
            string str = ConfigurationManager.AppSettings["IsInstalled"];
            return ((str == null) || bool.Parse(str));
        }

        protected JsonResult Json(object data, bool camelCase)
        {
            if (!camelCase)
            {
                return base.Json(data);
            }
            return new JsonNetResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        protected void JumpMobileUrl(ActionExecutingContext filterContext, string url = "")
        {
            JumpUrlRoute route;
            string pathAndQuery = base.Request.Url.PathAndQuery;
            string wX = pathAndQuery;
            RouteData routeData = filterContext.RouteData;
            if (routeData == null)
            {
                return;
            }
            string controller = routeData.Values["controller"].ToString().ToLower();
            string action = routeData.Values["action"].ToString().ToLower();
            string area = (routeData.DataTokens["area"] == null) ? "" : routeData.DataTokens["area"].ToString().ToLower();
            switch (area)
            {
                case "mobile":
                case "admin":
                    return;

                default:
                    if (area == "web")
                    {
                        this.IsAutoJumpMobile = true;
                    }
                    if ((!this.IsAutoJumpMobile || !this.IsMobileTerminal) || (Regex.Match(pathAndQuery, @"\/m(\-.*)?").Length >= 1))
                    {
                        return;
                    }
                    route = this.GetRouteUrl(controller, action, area, pathAndQuery);
                    switch (this.visitorTerminalInfo.Terminal)
                    {
                        case EnumVisitorTerminal.WeiXin:
                            if (route != null)
                            {
                                wX = route.WX;
                            }
                            wX = "/m-WeiXin" + wX;
                            goto Label_01B9;

                        case EnumVisitorTerminal.IOS:
                            if (route != null)
                            {
                                wX = route.WAP;
                            }
                            wX = "/m-ios" + wX;
                            goto Label_01B9;
                    }
                    if (route != null)
                    {
                        wX = route.WAP;
                    }
                    wX = "/m-wap" + wX;
                    break;
            }
        Label_01B9:
            if (route.IsSpecial)
            {
                if (route.PC.ToLower() == "/shop")
                {
                    string s = routeData.Values["id"].ToString();
                    long result = 0L;
                    long id = 0L;
                    if (!long.TryParse(s, out result))
                    {
                        result = 0L;
                    }
                    if (result > 0L)
                    {
                        VShopInfo vShopByShopId = ServiceHelper.Create<IVShopService>().GetVShopByShopId(result);
                        if (vShopByShopId != null)
                        {
                            id = vShopByShopId.Id;
                        }
                    }
                    wX = wX + "/" + id.ToString();
                }
                if (route.PC.ToLower() == "/order/submit")
                {
                    string str7 = string.Empty;
                    object obj2 = routeData.Values["cartitemids"];
                    if (obj2 == null)
                    {
                        str7 = base.Request.QueryString["cartitemids"];
                    }
                    else
                    {
                        str7 = obj2.ToString();
                    }
                    wX = wX + "/?cartItemIds=" + str7;
                }
            }
            if (!string.IsNullOrWhiteSpace(url))
            {
                wX = url;
            }
            string str8 = wX;
            str8 = base.Request.Url.Scheme + "://" + base.Request.Url.Authority + str8;
            if (!this.IsExistPage(str8))
            {
                if (this.visitorTerminalInfo.Terminal == EnumVisitorTerminal.WeiXin)
                {
                    wX = "/m-WeiXin/";
                }
                else
                {
                    wX = "/m-wap/";
                }
            }
            filterContext.Result = this.Redirect(wX);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this.JumpMobileUrl(filterContext, "");
            base.OnActionExecuting(filterContext);
        }

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            this.InitVisitorTerminal();
            if ((this.IsInstalled() && this.CurrentSiteSetting.SiteIsClose) && (filterContext.RouteData.Values["controller"].ToString().ToLower() != "admin"))
            {
                filterContext.Result = new RedirectResult("/common/site/close");
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            var pra = filterContext.Controller.ControllerContext.HttpContext.Request;
            Exception innerEx = GerInnerException(filterContext.Exception);
            string msg = innerEx.Message;

            if (!(innerEx is HimallException) && !(innerEx is PluginException))
            {
                var controllerName = filterContext.RouteData.Values["controller"].ToString();
                var actionName = filterContext.RouteData.Values["action"].ToString();
                var areaName = filterContext.RouteData.DataTokens["area"];
                var Id = filterContext.RouteData.DataTokens["id"];
                var erroMsg = string.Format("页面未捕获的异常：Area:{0},Controller:{1},Action:{2},id:{3}", areaName, controllerName, actionName, Id);
                erroMsg += "Get:" + pra.QueryString + "post:" + pra.Form.ToString();

                if (filterContext.Exception.GetType().FullName == "System.Data.Entity.Validation.DbEntityValidationException")
                {
                    try
                    {
                        var errorMessages = GetEntityValidationErrorMessage(filterContext.Exception);
                        if (errorMessages.Count > 0)
                            erroMsg += "\r\n" + string.Join("\r\n", errorMessages);
                    }
                    catch
                    { }
                }

                Log.Error(erroMsg, filterContext.Exception);
                msg = "系统发生错误，请重试，如果再次发生错误，请联系客服！";
            }

            if (WebHelper.IsAjax())
            {
                Result result = new Result();
                result.success = false;
                result.msg = msg;
                result.status = -9999;
                filterContext.Result = Json(result);
                //将状态码更新为200，否则在Web.config中配置了CustomerError后，Ajax会返回500错误导致页面不能正确显示错误信息
                filterContext.HttpContext.Response.StatusCode = 200;
                filterContext.ExceptionHandled = true;
                DisposeService(filterContext);
            }
            else
            {
                var erroView = "Error";
                //if (IsMobileTerminal)
                //    erroView = "~/Areas/Mobile/Templates/Default/Views/Shared/Error.cshtml";
                //#if !DEBUG
                var result = new ViewResult() { ViewName = erroView };
                result.ViewData["erroMsg"] = msg;
                filterContext.Result = result;
                // base.OnResultExecuting(filterContext.Result);
                //将状态码更新为200，否则在Web.config中配置了CustomerError后，Ajax会返回500错误导致页面不能正确显示错误信息
                filterContext.HttpContext.Response.StatusCode = 200;
                filterContext.ExceptionHandled = true;
                DisposeService(filterContext);

                //#endif
            }
            if (innerEx is HttpRequestValidationException)
            {
                if (WebHelper.IsAjax())
                {
                    Result result = new Result();
                    result.msg = "您提交了非法字符!";
                    filterContext.Result = Json(result);
                }
                else
                {
                    var result = new ContentResult();
                    result.Content = "<script src='/Scripts/jquery-1.11.1.min.js'></script>";
                    result.Content += "<script src='/Scripts/jquery.artDialog.js'></script>";
                    result.Content += "<script src='/Scripts/artDialog.iframeTools.js'></script>";
                    result.Content += "<link href='/Content/artdialog.css' rel='stylesheet' />";
                    result.Content += "<link href='/Content/bootstrap.min.css' rel='stylesheet' />";
                    result.Content += "<script>$(function(){$.dialog.errorTips('您提交了非法字符！',function(){window.history.back(-1)},2);});</script>";
                    filterContext.Result = result;
                }
                filterContext.HttpContext.Response.StatusCode = 200;
                filterContext.ExceptionHandled = true;
                DisposeService(filterContext);
            }

        }

        protected override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            this.DisposeService(filterContext);
        }

        public void SaveDistributionUserLinkId(long partnerid, long shopid, long uid)
        {
            if ((partnerid > 0L) && (shopid > 0L))
            {
                long item = 0L;
                item = ServiceHelper.Create<IMemberService>().UpdateShareUserId(uid, partnerid, shopid);
                List<long> distributionUserLinkId = this.GetDistributionUserLinkId();
                if (item > 0L)
                {
                    distributionUserLinkId.Add(item);
                }
                if (distributionUserLinkId.Count > 0)
                {
                    WebHelper.SetCookie("d2cccb104922d434", string.Join<long>(",", distributionUserLinkId.ToArray()));
                }
                else
                {
                    this.ClearDistributionUserLinkId();
                }
            }
        }

        public virtual void SendMessage(string message, MessageType type = 0, int? showTime = new int?(), bool goBack = false)
        {
            TempData["__Message__"] = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    message,
                    type,
                    goBack,
                    time = showTime
                });
        }

        protected virtual void SetAdminLoginCookie(long adminId, DateTime? expiredTime = new DateTime?())
        {
            string str = UserCookieEncryptHelper.Encrypt(adminId, "0");
            if (expiredTime.HasValue)
            {
                WebHelper.SetCookie("Himall-PlatformManager", str, expiredTime.Value);
            }
            else
            {
                WebHelper.SetCookie("Himall-PlatformManager", str);
            }
        }

        protected virtual void SetSellerAdminLoginCookie(long sellerAdminId, DateTime? expiredTime = new DateTime?())
        {
            string str = UserCookieEncryptHelper.Encrypt(sellerAdminId, "1");
            if (expiredTime.HasValue)
            {
                WebHelper.SetCookie("Himall-SellerManager", str, expiredTime.Value);
            }
            else
            {
                WebHelper.SetCookie("Himall-SellerManager", str);
            }
        }

        protected virtual void SetUserLoginCookie(long userId, DateTime? expiredTime = new DateTime?())
        {
            string str = UserCookieEncryptHelper.Encrypt(userId, "2");
            if (expiredTime.HasValue)
            {
                WebHelper.SetCookie("Himall-User", str, expiredTime.Value);
            }
            else
            {
                WebHelper.SetCookie("Himall-User", str);
            }
        }

        // Properties
        protected List<JumpUrlRoute> _JumpUrlRouteData { get; set; }

        public SiteSettingsInfo CurrentSiteSetting
        {
            get
            {
                if (this._sitesetting == null)
                {
                    this._sitesetting = SiteSettingApplication.GetSiteSettings();
                }
                return this._sitesetting;
            }
        }

        public UserMemberInfo CurrentUser
        {
            get
            {
                if (this._currentUser == null)
                {
                    this._currentUser = GetUser(base.Request);
                }
                return this._currentUser;
            }
        }

        public bool IsMobileTerminal { get; set; }

        public List<JumpUrlRoute> JumpUrlRouteData
        {
            get
            {
                return this._JumpUrlRouteData;
            }
        }

        public VisitorTerminal visitorTerminalInfo { get; set; }

        // Nested Types
        public enum MessageType
        {
            Alert = 0,
            AlertTips = 1,
            ErrorTips = -1,
            SuccessTips = 2
        }

        public class Result
        {
            // Properties
            public object Data { get; set; }

            public string msg { get; set; }

            public int status { get; set; }

            public bool success { get; set; }
        }
    }

}
