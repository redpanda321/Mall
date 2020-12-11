using Mall.Application;
using Mall.CommonModel;
using Mall.Core.Helper;
using Mall.DTO;
using Mall.IServices;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Mall.Web.Framework
{
    /// <summary>
    /// 页面基类型
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class WebViewPage<TModel> : RazorPage<TModel>
	{


        ISellerManager sellerManager = null;

        IPaltManager platManager = null;

        public IPaltManager CurrentManager
        {
            get
            {
                if (platManager != null)
                {
                    return platManager;
                }

                var userId = UserCookieEncryptHelper.Decrypt(WebHelper.GetCookie(CookieKeysCollection.PLATFORM_MANAGER), CookieKeysCollection.USERROLE_ADMIN);
                if (userId > 0)
                    platManager = ServiceHelper.Create<IManagerService>().GetPlatformManager(userId);
                return platManager;
            }
        }

	
	
		public SiteSettings SiteSetting
		{
			get
			{
				return SiteSettingApplication.SiteSettings;
			}
		}

        /// <summary>
        /// 当前用户信息
        /// </summary>
        public Entities.MemberInfo CurrentUser
        {
            get
			{
                /*
				if (this.ViewContext.Controller is BaseController)
					return ((BaseController)this.ViewContext.Controller).CurrentUser;
				return BaseController.GetUser(Request);
                */

                return BaseController.GetUser(Context.Request);
            }
        }
        
        
        
        
        
        
        
        
        public ISellerManager CurrentSellerManager

        {
            

            get
            {
                if (sellerManager != null)
                {
                    return sellerManager;
                }
                else
                {
                    long userId = UserCookieEncryptHelper.Decrypt(WebHelper.GetCookie(CookieKeysCollection.SELLER_MANAGER), CookieKeysCollection.USERROLE_SELLERADMIN);
                    if (userId != 0)
                    {
                        sellerManager = ServiceHelper.Create<IManagerService>().GetSellerManager(userId);
                    }
                }
                return sellerManager;
            }


        }
        public string Generator
        {
            get
            {
                return "3.3";
            }
        }
    }
    /// <summary>
    /// 页面基类型
    /// </summary>
    public abstract class WebViewPage : WebViewPage<dynamic>
    {

    }
}
