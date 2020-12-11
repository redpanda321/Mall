using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Mall.DTO.QueryModel;
using Mall.Application;
using Mall.API.Model.ParamsModel;
using Mall.DTO;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.Core.Plugins;
using Mall.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Mall.API
{
    public class ShopBindContactController : BaseShopLoginedApiController
    {
		#region 静态字段
		private static string _encryptKey = Guid.NewGuid().ToString("N");
		#endregion

		/// <summary>
		/// 发送验证码之前验证联系方式
		/// </summary>
		/// <param name="contact"></param>
		/// <returns></returns>
		protected override bool CheckContact(string contact, out string errorMessage)
		{
			CheckShopManageLogin();
            errorMessage = string.Empty;
            var isMobile = Core.Helper.ValidateHelper.IsMobile(contact);
            var isEmail = Core.Helper.ValidateHelper.IsEmail(contact);
            if (!isMobile && !isEmail)
                errorMessage = "请输入正确的手机号或邮箱！";
            return (isMobile || isEmail);
		}

		/// <summary>
		/// 短信验证成功后创建验证成功凭证
		/// </summary>
		/// <param name="contact"></param>
		/// <returns></returns>
		protected override string CreateCertificate(string contact)
		{
            CheckShopManageLogin();
            var identity = CurrentUser.Id.ToString();
            identity = SecureHelper.MD5(identity + _encryptKey);
			return identity;
		}

        [HttpGet("GetCheckContact")]

        public object GetCheckContact(string contact, string certificate)
        {
            CheckShopManageLogin();
            if (!CheckCertificate(certificate))
                return ErrorResult("凭证无效");

            PluginInfo pluginInfo;
            var isMobile = Core.Helper.ValidateHelper.IsMobile(contact);
			if (isMobile)
				pluginInfo = PluginsManagement.GetInstalledPluginInfos(Core.Plugins.PluginType.SMS).First();
			else
				pluginInfo = PluginsManagement.GetInstalledPluginInfos(PluginType.Email).First();

            var user = MemberApplication.GetMemberByName(CurrentUser.UserName);
            MemberApplication.UpdateMemberContacts(new MemberContacts()
            {
                Contact = contact,
                ServiceProvider = pluginInfo.PluginId,
                UserId = user.Id,
                UserType = MemberContactInfo.UserTypes.General
            });

            return SuccessResult();
        }

		#region 私有方法
		private bool CheckCertificate(string certificate)
		{
            var identity = SecureHelper.MD5(CurrentUser.Id.ToString() + _encryptKey);
            return certificate == identity;
		}
		#endregion
    }
}
