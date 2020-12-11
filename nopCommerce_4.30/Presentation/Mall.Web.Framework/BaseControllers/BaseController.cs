using Mall.Application;
using Mall.Core;
using Mall.Core.Helper;
using Mall.Core.Plugins;
using Mall.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using Microsoft.Extensions.Configuration;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;


namespace Mall.Web.Framework
{

    public abstract class BaseController : Controller
    {
        #region MyRegion

        private Entities.MemberInfo _currentUser;

        private IHttpContextAccessor _httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();


        public IHttpContextAccessor httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();

        public IWebHostEnvironment hostingEnvironment = EngineContext.Current.Resolve<IWebHostEnvironment>();

        public IConfiguration configuration = EngineContext.Current.Resolve<IConfiguration>();


        #endregion

        #region 构造函数
        public BaseController()
        {
    
           
            if (!IsInstalled())
            {
                base.RedirectToAction("/Web/Installer/Agreement");


            }

            
        }


        // Methods
        public BaseController(IHttpContextAccessor httpContextAccessor)
        {

            _httpContextAccessor = httpContextAccessor;

            if (!this.IsInstalled())
            {
                base.RedirectToAction("/Web/Installer/Agreement");
            }



        }


        #endregion

        #region 方法

        #region 来源终端信息
        /// <summary>
        /// 是否自动跳到移动端
        /// </summary>
        public bool IsAutoJumpMobile = false;
        //Add:Dzy[150720]
        /// <summary>
        /// 访问终端信息
        /// </summary>
        public VisitorTerminal visitorTerminalInfo { get; set; }
        /// <summary>
        /// 是否当前为移动端访问
        /// </summary>
        public bool IsMobileTerminal { get; set; }
        /// <summary>
        /// 获取访问终端信息
        /// </summary>
        protected void InitVisitorTerminal()
        {
            VisitorTerminal result = new VisitorTerminal();
            string sUserAgent = Request.Headers["User-Agent"].ToString();
            if (string.IsNullOrWhiteSpace(sUserAgent))
            {
                sUserAgent = "";
            }
            sUserAgent = sUserAgent.ToLower();
            //终端类型
            bool IsIpad = sUserAgent.Contains("ipad");
            bool IsIphoneOs = sUserAgent.Contains("iphone os");
            bool IsMidp = sUserAgent.Contains("midp");
            bool IsUc = sUserAgent.Contains("rv:1.2.3.4") || sUserAgent.Contains("ucweb");
            bool IsAndroid = sUserAgent.Contains("android");
            bool IsCE = sUserAgent.Contains("windows ce");
            bool IsWM = sUserAgent.Contains("windows mobile");
            bool IsWeiXin = sUserAgent.Contains("micromessenger");
            bool IsWP = sUserAgent.Contains("windows phone ");
            bool IsIosApp = sUserAgent.Contains("appwebview(ios)");

            //初始为电脑端
            result.Terminal = EnumVisitorTerminal.PC;

            //所有移动平台
            if (IsIpad || IsIphoneOs || IsMidp || IsUc || IsAndroid || IsCE || IsWM || IsWP)
            {
                result.Terminal = EnumVisitorTerminal.Moblie;
            }
            if (IsIpad || IsIphoneOs)
            {
                //苹果系统
                result.OperaSystem = EnumVisitorOperaSystem.IOS;
                result.Terminal = EnumVisitorTerminal.Moblie;
                if (IsIpad)
                {
                    result.Terminal = EnumVisitorTerminal.PAD;
                }
                if (IsIosApp)
                {
                    result.Terminal = EnumVisitorTerminal.IOS;
                }
            }
            if (IsAndroid)
            {
                //安卓
                result.OperaSystem = EnumVisitorOperaSystem.Android;
                result.Terminal = EnumVisitorTerminal.Moblie;
            }
            if (IsWeiXin)
            {
                //微信特殊化
                result.Terminal = EnumVisitorTerminal.WeiXin;
            }

            //TODO:DZY[150731] 整合移动端请求
            /* zjt  
			 * TODO可移除，保留注释即可
			 */
            #region  移动端判定
            if (result.Terminal == EnumVisitorTerminal.Moblie
                || result.Terminal == EnumVisitorTerminal.PAD
                || result.Terminal == EnumVisitorTerminal.WeiXin
                || result.Terminal == EnumVisitorTerminal.IOS
                )
            {
                IsMobileTerminal = true;
            }
            #endregion

            visitorTerminalInfo = result;
        }
        /// <summary>
        /// 跳转到手机URL
        /// </summary>
        /// <param name="JumpUrl">为空表示自动处理</param>
        //TODO:DZY[150730] 统一跳转
        /* zjt  
		 * Url请改为小写 【参数命名规范】
		 */
        protected void JumpMobileUrl(ActionExecutingContext filterContext, string url = "")
        {
            string curUrl = Request.Path.ToString()+ Request.QueryString.ToString();
            string jumpUrl = curUrl;
            var route = filterContext.RouteData;
            //无路由信息不跳转
            if (route == null)
                return;

            //路由处理
            string controller = route.Values["controller"].ToString().ToLower();
            string action = route.Values["action"].ToString().ToLower();
            //string area = (route.DataTokens["area"] == null ? "" : route.DataTokens["area"].ToString().ToLower());


            object areaObj = null;
            string area = "";
            if (filterContext.RouteData.Values.TryGetValue("area", out areaObj))
                area = areaObj.ToString().ToLower();



            if (area == "mobile" || area == "admin")
            {
                return;  //在移动端跳出
            }
            //Web区域跳转移动端
            if (area == "web")
                IsAutoJumpMobile = true;

            if (this.IsAutoJumpMobile && IsMobileTerminal)
            {
                if (Regex.Match(curUrl, @"\/m(\-.*)?").Length < 1)
                {
                    JumpUrlRoute jurdata = GetRouteUrl(controller, action, area, curUrl);
                    //非手机端跳转
                    switch (visitorTerminalInfo.Terminal)
                    {
                        case EnumVisitorTerminal.WeiXin:
                            if (jurdata != null)
                            {
                                jumpUrl = jurdata.WX;
                            }
                            jumpUrl = @"/m-WeiXin" + jumpUrl;
                            break;
                        case EnumVisitorTerminal.IOS:
                            if (jurdata != null)
                            {
                                jumpUrl = jurdata.WAP;
                            }
                            jumpUrl = @"/m-ios" + jumpUrl;
                            break;
                        default:
                            if (jurdata != null)
                            {
                                jumpUrl = jurdata.WAP;
                            }
                            jumpUrl = @"/m-wap" + jumpUrl;
                            break;
                    }

                    #region 参数特殊处理
                    if (jurdata.IsSpecial)
                    {
                        #region 店铺参数转换
                        if (jurdata.PC.ToLower() == @"/shop")
                        {
                            //商家首页参数处理
                            string strid = route.Values["id"].ToString();
                            long shopId = 0;
                            long vshopId = 0;
                            if (!long.TryParse(strid, out shopId))
                            {
                                shopId = 0;
                            }
                            if (shopId > 0)
                            {
                                var vshop = VshopApplication.GetVShopByShopId(shopId);
                                if (vshop != null)
                                    vshopId = vshop.Id;
                            }
                            jumpUrl = jumpUrl + "/" + vshopId.ToString();
                        }
                        #endregion

                        //TODO:LRL 订单页面参数转换
                        /* zjt  
						 * TODO可移除，保留注释即可
						 */
                        #region 下单页面参数转换
                        if (jurdata.PC.ToLower() == @"/order/submit")
                        {
                            //商家首页参数处理
                            var strcartid = string.Empty;
                            var arg = route.Values["cartitemids"];
                            if (arg == null)
                            {
                                strcartid = Request.Query["cartitemids"].ToString();
                            }
                            else
                            {
                                strcartid = arg.ToString();
                            }
                            jumpUrl = jumpUrl + "/?cartItemIds=" + strcartid;
                        }
                        #endregion

                    }
                    #endregion

                    if (!string.IsNullOrWhiteSpace(url))
                        jumpUrl = url;
                    //string testurl = jumpUrl;
                    //testurl = Request.Url.Scheme + "://" + Request.Url.Authority + testurl;
                    ////页面不存在
                    //if (!IsExistPage(testurl))
                    //{
                    //    if (visitorTerminalInfo.Terminal == EnumVisitorTerminal.WeiXin)
                    //    {
                    //        jumpUrl = @"/m-WeiXin/";
                    //    }
                    //    else
                    //    {
                    //        jumpUrl = @"/m-wap/";
                    //    }

                    //}

                    filterContext.Result = Redirect(jumpUrl);
                }
            }
        }

        /// <summary>
        /// 移动页面跳转路由列表(保护)
        /// </summary>
        protected List<JumpUrlRoute> _JumpUrlRouteData { get; set; }
        /// <summary>
        /// 移动页面跳转路由列表
        /// </summary>
        public List<JumpUrlRoute> JumpUrlRouteData
        {
            get
            {
                return _JumpUrlRouteData;
            }
        }
        /// <summary>
        /// 初始移动页面跳转路由列表
        /// </summary>
        //TODO:DZY[150730] 路由初始
        /*
		 * _JumpUrlRouteData , 是否需要确认带下划线的成员变量规范 
		 */
        public void InitJumpUrlRoute()
        {
            _JumpUrlRouteData = new System.Collections.Generic.List<JumpUrlRoute>();
            JumpUrlRoute _tmp = new JumpUrlRoute() { Action = "Index", Area = "Web", Controller = "UserOrder", PC = @"/userorder", WAP = @"/member/orders", WX = @"/member/orders" };
            _JumpUrlRouteData.Add(_tmp);
            _tmp = new JumpUrlRoute() { Action = "Index", Area = "Web", Controller = "UserCenter", PC = @"/usercenter", WAP = @"/member/center", WX = @"/member/center" };
            _JumpUrlRouteData.Add(_tmp);
            _tmp = new JumpUrlRoute() { Action = "Index", Area = "Web", Controller = "Login", PC = @"/login", WAP = @"/login/entrance", WX = @"/login/entrance" };
            _JumpUrlRouteData.Add(_tmp);
            _tmp = new JumpUrlRoute() { Action = "Home", Area = "Web", Controller = "Shop", PC = @"/shop", WAP = @"/vshop/detail", WX = @"/vshop/detail", IsSpecial = true };
            _JumpUrlRouteData.Add(_tmp);
            _tmp = new JumpUrlRoute() { Action = "Submit", Area = "Web", Controller = "Order", PC = @"/order/submit", WAP = @"/order/SubmiteByCart", WX = @"/order/SubmiteByCart", IsSpecial = true };
            _JumpUrlRouteData.Add(_tmp);
        }
        /// <summary>
        /// 返回URL对应路由信息
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        //TODO:DZY[150730] 路由信息获取
        /*
		 * TODO可移除
		 */
        public JumpUrlRoute GetRouteUrl(string controller, string action, string area, string url)
        {
            InitJumpUrlRoute();
            JumpUrlRoute result = null;
            url = url.ToLower();
            controller = controller.ToLower();
            action = action.ToLower();
            area = area.ToLower();
            List<JumpUrlRoute> sql = JumpUrlRouteData;
            if (!string.IsNullOrWhiteSpace(area))
            {
                sql = sql.FindAll(d => d.Area.ToLower() == area);
            }
            if (!string.IsNullOrWhiteSpace(controller))
            {
                sql = sql.FindAll(d => d.Controller.ToLower() == controller);
            }
            if (!string.IsNullOrWhiteSpace(action))
            {
                sql = sql.FindAll(d => d.Action.ToLower() == action);
            }
            result = sql.Count > 0 ? sql[0] : null;

            if (result == null)
            {
                result = new JumpUrlRoute() { PC = url, WAP = url, WX = url };
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 当前站点配置
        /// </summary>
        public SiteSettings SiteSettings
        {
            get
            {
                return SiteSettingApplication.SiteSettings;
            }
        }

        /// <summary>
        /// 当前登录的会员
        /// </summary>
        public Entities.MemberInfo CurrentUser
        {
            get
            {
                if (_currentUser == null)
                    _currentUser = GetUser(Request);
                return _currentUser;
            }
        }

        protected Exception GerInnerException(Exception ex)
        {
            if (ex.InnerException != null)
            {
                return GerInnerException(ex.InnerException);
            }
            else
            {
                return ex;
            }
        }

       public  void OnException(ExceptionContext filterContext)
        {
            var pra = _httpContextAccessor.HttpContext.Request;
            Exception innerEx = GerInnerException(filterContext.Exception);
            string msg = innerEx.Message;

            if (!(innerEx is MallException) && !(innerEx is PluginException))
            {
                var controllerName = filterContext.RouteData.Values["controller"].ToString();
                var actionName = filterContext.RouteData.Values["action"].ToString();
               // var areaName = filterContext.RouteData.DataTokens["area"];
 object area = null;
                string areaName = "";
                if (filterContext.RouteData.Values.TryGetValue("area", out area))
                    areaName = area.ToString();
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
                //ServiceApplication.DisposeService((ControllerContext) filterContext);
            }
            else
            {
                //
                var erroView = "Error";
                if (innerEx is Mall404)
                {
                    erroView = "404";
                }
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
              //  ServiceApplication.DisposeService(filterContext);

                //#endif
            }
            if (innerEx is Exception)
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
                //ServiceApplication.DisposeService(filterContext);
            }

        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            /*
            
            if (IsInstalled())
            {

                var route = filterContext.RouteData;
                string allowUrl = Core.Helper.WebHelper.GetRawUrl().ToLower();
                string area = string.Empty;
                if (route != null)
                {
                    area = (route.DataTokens["area"] == null ? "" : route.DataTokens["area"].ToString().ToLower());
                }
                //PC端手动输入移动端地址+wap端+微信端
                if (area == "mobile" || visitorTerminalInfo.Terminal == EnumVisitorTerminal.WeiXin || visitorTerminalInfo.Terminal == EnumVisitorTerminal.Moblie || visitorTerminalInfo.Terminal == EnumVisitorTerminal.PAD)
                {
                    if (!allowUrl.Contains("/shopregisterjump/") && !allowUrl.Contains("/shopregister/") && !allowUrl.Contains("/topic/detail/")&& 
                        !allowUrl.Contains("/bigwheel/") && !allowUrl.Contains("/scratchcard/") && !allowUrl.Contains("/payment/"))//商家入驻，专题页，大转盘，刮刮卡,移动端支付控制器(小程序支付后异步处理需权限)
                    {
                        if (!SiteSettings.IsOpenH5)
                        {
                            var erroView = "~/Areas/Mobile/Views/Shared/Authorize.cshtml";
                            var result = new ViewResult() { ViewName = erroView };
                            filterContext.Result = result;
                            filterContext.HttpContext.Response.StatusCode = 200;
                           // ServiceApplication.DisposeService(filterContext);
                            return;
                        }
                    }
                }
                if (visitorTerminalInfo.Terminal == EnumVisitorTerminal.PC && area == "web" && (!allowUrl.Contains("login")) && (!allowUrl.Contains("seleradmin")) && (!allowUrl.Contains("findpassword")))//不应处理商家后台，登录，找回密码
                {
                    if (!allowUrl.Contains("/pay/"))//支付不需验证
                    {
                        if (!SiteSettings.IsOpenPC)
                        {
                            var erroView = "~/Areas/Web/Views/Shared/Authorize.cshtml";
                            var result = new ViewResult() { ViewName = erroView };
                            filterContext.Result = result;
                            filterContext.HttpContext.Response.StatusCode = 200;
                          //  ServiceApplication.DisposeService(filterContext);
                            return;
                        }
                    }
                }
            }
          
            */


     


            //跳转移动端
            JumpMobileUrl(filterContext);
            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// <para>请在重写时优先调用此方法</para>
        /// </summary>
        /// <param name="filterContext"></param>
        public virtual void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            //初始获取访问终端信息 Add:Dzy[150731]
            InitVisitorTerminal();
            if (IsInstalled() && SiteSettings.SiteIsClose)
            { //在已经安装完成的基础上检查站点是否已经关闭

                //站点已关闭时，仅可以访问平台中心
                string controllerName = filterContext.RouteData.Values["controller"].ToString();
                if (controllerName.ToLower() != "admin")
                    filterContext.Result = new RedirectResult("/common/site/close");
            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
          //  ServiceApplication.DisposeService(filterContext);
        }

        /// <summary>
        /// 创建一个将指定对象序列化为 JavaScript 对象表示法 (JSON) 的 System.Web.Mvc.JsonResult 对象。
        /// </summary>
        /// <param name="data">要序列化的 JavaScript 对象图</param>
        /// <param name="camelCase">data中的数据是否使用驼峰风格进行转换</param>
        /// <returns>将指定对象序列化为 JSON 格式的 JSON 结果对象。在执行此方法所准备的结果对象时，ASP.NET MVC 框架会将该对象写入响应。</returns>
        protected JsonResult Json(object data, bool camelCase)
        {
            if (!camelCase)
                return Json(data);

            return new JsonResult(data);
          
        }

        /// <summary>
        /// 设置普通用户登录cookie
        /// </summary>
        /// <param name="userId">登录用户的id</param>
        /// <param name="expiredTime">cookie过期时间</param>
        protected virtual void SetUserLoginCookie(long userId, DateTime? expiredTime = null)
        {
            var cookieValue = UserCookieEncryptHelper.Encrypt(userId, CookieKeysCollection.USERROLE_USER);
            if (expiredTime.HasValue)
                WebHelper.SetCookie(CookieKeysCollection.Mall_USER, cookieValue, expiredTime.Value);
            else
                WebHelper.SetCookie(CookieKeysCollection.Mall_USER, cookieValue);
        }

        /// <summary>
        /// 设置Admin登录cookie
        /// </summary>
        /// <param name="adminId">Admin的id</param>
        /// <param name="expiredTime">cookie过期时间</param>
        protected virtual void SetAdminLoginCookie(long adminId, DateTime? expiredTime = null)
        {
            var cookieValue = UserCookieEncryptHelper.Encrypt(adminId, CookieKeysCollection.USERROLE_ADMIN, 1);
            if (expiredTime.HasValue)
                WebHelper.SetCookie(CookieKeysCollection.PLATFORM_MANAGER, cookieValue, expiredTime.Value);
            else
                WebHelper.SetCookie(CookieKeysCollection.PLATFORM_MANAGER, cookieValue);
        }

        /// <summary>
        /// 设置SellerAdmin登录cookie
        /// </summary>
        /// <param name="sellerAdminId">SellerAdmin的id</param>
        /// <param name="expiredTime">cookie过期时间</param>
        protected virtual void SetSellerAdminLoginCookie(long sellerAdminId, DateTime? expiredTime = null)
        {
            var cookieValue = UserCookieEncryptHelper.Encrypt(sellerAdminId, CookieKeysCollection.USERROLE_SELLERADMIN);
            if (expiredTime.HasValue)
                WebHelper.SetCookie(CookieKeysCollection.SELLER_MANAGER, cookieValue, expiredTime.Value);
            else
                WebHelper.SetCookie(CookieKeysCollection.SELLER_MANAGER, cookieValue);
        }

        /// <summary>
        /// 发送消息到前端页面
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type">消息的类型，不同的类型对应不同的展示方式</param>
        /// <param name="showTime">对话框显示时间,单位秒</param>
        /// <param name="goBack">是否后退</param>
        public virtual void SendMessage(string message, MessageType type = MessageType.Alert, int? showTime = null, bool goBack = false)
        {
            TempData["__Message__"] = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                message,
                type,
                goBack,
                time = showTime
            });
        }

        public virtual ExcelResult ExcelView(string fileName, object model)
        {
            return ExcelView(null, fileName, model);
        }

        public virtual ExcelResult ExcelView(string viewName, string fileName, object model)
        {
            return new ExcelResult(viewName, fileName, model);
        }
        #endregion

        #region 静态方法
        public static Entities.MemberInfo GetUser(HttpRequest request)
        {

            /*
            long id = UserCookieEncryptHelper.Decrypt(WebHelper.GetCookie("Mall-User"), "2");
            if (id == 0L)
            {
                string userIdCookie = request.Query["token"].ToString();
                //string userIdCookie = Guid.NewGuid().ToString();
                id = UserCookieEncryptHelper.Decrypt(userIdCookie, "2");
                if (id != 0L)
                {
                    WebHelper.SetCookie("Mall-User", userIdCookie);
                }
            }
            if (id != 0L)
            {
                var userInfo = MemberApplication.GetMember(id);
                var siteInfo = SiteSettingApplication.SiteSettings;
                if (siteInfo != null)
                {
                    if (!(siteInfo.IsOpenPC || siteInfo.IsOpenH5 || siteInfo.IsOpenMallSmallProg || siteInfo.IsOpenApp))//授权模块影响会员折扣功能
                    {
                        userInfo.MemberDiscount = 1M;
                    }

                }
                return userInfo;
            }
            return null;

            */

            
            long userId = 0;
            var token = request.Query["token"].ToString();
            if (!string.IsNullOrEmpty(token))
            {
                userId = UserCookieEncryptHelper.Decrypt(token, CookieKeysCollection.USERROLE_USER);
                if (userId != 0)
                {
                    WebHelper.SetCookie(CookieKeysCollection.Mall_USER, token);
                }
            }
            if (userId == 0)
            {
                var cookieValue = WebHelper.GetCookie(CookieKeysCollection.Mall_USER);
                userId = UserCookieEncryptHelper.Decrypt(cookieValue, CookieKeysCollection.USERROLE_USER);
            }

            if (userId != 0)
            {
                var userInfo = MemberApplication.GetMember(userId);
                var siteInfo = SiteSettingApplication.SiteSettings;
                if (siteInfo != null)
                {
                    if (!(siteInfo.IsOpenPC || siteInfo.IsOpenH5 || siteInfo.IsOpenMallSmallProg||siteInfo.IsOpenApp))//授权模块影响会员折扣功能
                    {
                        userInfo.MemberDiscount = 1M;
                    }

                }
                return userInfo;
            }

            return null;

            
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 页面是否存在
        /// <para>包括200、302、301</para>
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        //TODO:DZY[150730] 页面访问判定
        /* zjt  
		 * TODO可移除
		 */
        private bool IsExistPage(string url)
        {
            bool result = false;
            HttpWebResponse urlresponse = Mall.Core.Helper.WebHelper.GetURLResponse(url);
            if (urlresponse != null)
            {
                if (urlresponse.StatusCode == HttpStatusCode.OK || (int)urlresponse.StatusCode == 302 || (int)urlresponse.StatusCode == 301)
                {
                    result = true;
                }
            }
            return result;
        }

        private bool IsInstalled()
        {
            //var t = ConfigurationManager.AppSettings["IsInstalled"];
            // return null == t || bool.Parse(t);

            //   bool installed = DataSettingsManager.DatabaseIsInstalled;

            bool installed = configuration.GetValue<bool>("Mall:IsInstalled");

            return installed;


        }

        private List<string> GetEntityValidationErrorMessage(Exception exception)
        {
            var list = new List<string>();
            var temp = (dynamic)exception;
            foreach (var entityValidationError in temp.EntityValidationErrors)
            {
                foreach (var validationError in entityValidationError.ValidationErrors)
                {
                    list.Add(validationError.ErrorMessage);
                }
            }
            return list;
        }
        #endregion

        #region 内部类
        public class Result
        {
            public bool success { get; set; }

            public string msg { get; set; }
            /// <summary>
            /// 状态
            /// <para>1表成功</para>
            /// </summary>
            public int status { get; set; }

            public object data { get; set; }

            public int code { get; set; }
        }
        /// <summary>
        /// 统一返回json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="success"></param>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <param name="code"></param>
        /// camelCase,
        /// <returns></returns>
        protected JsonResult Json<T>(bool success, string msg = "", T data = default(T), int code = 0, bool camelCase = false)
        {
            if (camelCase)
            {
                return Json(new Result()
                {
                    data = data,
                    msg = msg,
                    success = success,
                    code = code
                }, camelCase);
            }
            else
            {
                return Json(new Result()
                {
                    data = data,
                    msg = msg,
                    success = success,
                    code = code
                });
            }
        }
        /// <summary>
        /// 失败结果，可以返回数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        protected JsonResult ErrorResult<T>(string msg = "", T data = default(T), int code = 0, bool camelCase = false)
        {
            return Json<T>(false, msg, data, code, camelCase: camelCase);
        }
        /// <summary>
        /// 失败结果，无数据
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        protected JsonResult ErrorResult(string msg = "", int code = 0, bool camelCase = false)
        {
            return ErrorResult<dynamic>(msg: msg, code: code, camelCase: camelCase);
        }
        /// <summary>
        /// 成功结果，可以返回数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        protected JsonResult SuccessResult<T>(string msg = "", T data = default(T), int code = 0, bool camelCase = false)
        {
            return Json<T>(true, msg, data, code, camelCase: camelCase);
        }
        /// <summary>
        /// 成功结果，无数据
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        protected JsonResult SuccessResult()
        {
            return Json<object>(true);
        }

        protected JsonResult SuccessResult(string msg)
        {
            return Json<object>(true, msg);
        }

        public enum MessageType
        {
            ErrorTips = -1,
            Alert,
            AlertTips,
            SuccessTips
        }
        #endregion
    }

    #region 访问设备
    //Add:Dzy[150720]
    public class VisitorTerminal
    {
        /// <summary>
        /// 终端类型
        /// </summary>
        public EnumVisitorTerminal Terminal { get; set; }
        /// <summary>
        /// 浏览器内核[暂不启用]
        /// </summary>
        //public EnumVisitorBrowserCore BrowserCore { get; set; }
        /// <summary>
        /// 操作系统
        /// </summary>
        public EnumVisitorOperaSystem OperaSystem { get; set; }
    }
    /// <summary>
    /// 浏览器内核
    /// </summary>
    public enum EnumVisitorBrowserCore
    {
        /// <summary>
        /// IE系内核
        /// <para>包括大部国产浏览器</para>
        /// </summary>
        Trident = 0,
        /// <summary>
        /// WebKit系
        /// <para>Chrome、Safari及大部份国产浏览器</para>
        /// </summary>
        WebKit = 1,
        /// <summary>
        /// Firefox
        /// </summary>
        Gecko = 2,
        /// <summary>
        /// Google新版
        /// <para>Chrome、Opera及大部份国产浏览器新版本</para>
        /// </summary>
        Blink = 3,
        /// <summary>
        /// Opera老版本
        /// </summary>
        Presto = 4,
        /// <summary>
        /// 微信
        /// </summary>
        WeiXin = 5,
        /// <summary>
        /// 其他
        /// </summary>
        Other = 99
    }
    /// <summary>
    /// 访问终端
    /// <para>判定打开页面的设备与浏览器</para>
    /// </summary>
    public enum EnumVisitorTerminal
    {
        /// <summary>
        /// 电脑端
        /// </summary>
        PC = 0,
        /// <summary>
        /// 手机端
        /// </summary>
        Moblie = 1,
        /// <summary>
        /// 平板
        /// </summary>
        PAD = 2,
        /// <summary>
        /// 微信
        /// <para>独立出微信特征</para>
        /// </summary>
        WeiXin = 3,
        /// IOSApp
        /// </summary>
        IOS = 4,
        /// <summary>
        /// 安卓App
        /// </summary>
        Android = 5,
        /// <summary>
        /// <summary>
        /// 其他
        /// </summary>
        Other = 99
    }
    /// <summary>
    /// 操作系统
    /// </summary>
    public enum EnumVisitorOperaSystem
    {
        /// <summary>
        /// MS出品
        /// </summary>
        Windows = 0,
        /// <summary>
        /// 安卓
        /// </summary>
        Android = 1,
        /// <summary>
        /// 苹果移动
        /// </summary>
        IOS = 2,
        /// <summary>
        /// Linux
        /// <para>Red Hat等</para>
        /// </summary>
        Linux = 3,
        /// <summary>
        /// UNIX
        /// <para>如BSD一类</para>
        /// </summary>
        UNIX = 4,
        /// <summary>
        /// 苹果桌面
        /// </summary>
        MacOS = 5,
        /// <summary>
        /// MS移动
        /// </summary>
        WindowsPhone = 6,
        /// <summary>
        /// Windows CE 嵌入式
        /// </summary>
        WindowsCE = 7,
        /// <summary>
        /// Windows Mobile
        /// </summary>
        WindowsMobile = 8,
        /// <summary>
        /// 其他
        /// </summary>
        Other = 99
    }
    #endregion

    #region 移动页面跳转路由
    //Add:Dzy[150720]
    /// <summary>
    /// 移动页面跳转路由
    /// </summary>
    //TODO:DZY[150730] 重理路由信息
    /*
     * TODO可移除
     */
    public class JumpUrlRoute
    {
        /// <summary>
        /// 控制器
        /// <para>为空表示不参与判断</para>
        /// </summary>
        public string Controller { get; set; }
        /// <summary>
        /// 行为
        /// <para>为空表示不参与判断</para>
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// 区域
        /// <para>为空表示不参与判断</para>
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// 电脑端
        /// </summary>
        public string PC { get; set; }
        /// <summary>
        /// 移动端
        /// </summary>
        public string WAP { get; set; }
        /// <summary>
        /// 微信
        /// </summary>
        public string WX { get; set; }
        /// <summary>
        /// 是否需要特殊处理
        /// </summary>
        public bool IsSpecial { get; set; }
    }
    #endregion
}
