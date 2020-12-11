using Mall.Application;
using Mall.Core;
using Mall.Core.Helper;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Mall.API
{
    public class LoginController : BaseApiController
    {
        #region 静态字段
        private static string _encryptKey = Guid.NewGuid().ToString("N");
        #endregion

        #region 方法

        [HttpGet("GetUser")]
        public object GetUser(string userName = "", string password = "", string oauthType = "", string oauthOpenId = "", string unionid = "", string headimgurl = "", string oauthNickName = "", int? sex = null, string city = "", string province = "")
        {
            dynamic data = new System.Dynamic.ExpandoObject();
            //信任登录
            if (!string.IsNullOrEmpty(oauthType) && (!string.IsNullOrEmpty(unionid) || !string.IsNullOrEmpty(oauthOpenId)) && string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(password))
            {
                Log.Debug(string.Format("oauthType={0} openId={1} unionid={2} userName={3}", oauthType, oauthOpenId, unionid, userName));

                var member = Application.MemberApplication.GetMemberByUnionIdAndProvider(oauthType, unionid);
                if (member == null)
                    member = Application.MemberApplication.GetMemberByOpenId(oauthType, oauthOpenId);

                if (member != null)
                {
                    if (member.Disabled)
                    {
                        data = ErrorResult("用户已被冻结", 105);
                    }
                    else
                    {
                        //信任登录并且已绑定
                        data.success = true;
                        data.UserId = member.Id.ToString();
                        string memberId = UserCookieEncryptHelper.Encrypt(member.Id, CookieKeysCollection.USERROLE_USER);
                        data.UserKey = memberId;
                    }
                }
                else
                {
                    data = ErrorResult("未绑定商城帐号", 104);
                }
            }
            //普通登录
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password) && string.IsNullOrEmpty(oauthType) && string.IsNullOrEmpty(unionid) && string.IsNullOrEmpty(oauthOpenId))
            {
                Entities.MemberInfo member = null;
                try
                {
                    member = ServiceProvider.Instance<IMemberService>.Create.Login(userName, password);
                }
                catch (Exception ex)
                {
                    data = ErrorResult(ex.Message, 104);
                    return data;
                }
                if (member == null)
                {
                    data = ErrorResult("用户名或密码错误", 103);
                }
                else if (member != null && member.Disabled)
                {
                    data = ErrorResult("用户已被冻结", 105);
                }
                else
                {
                    data.success = true;
                    data.UserId = member.Id.ToString();
                    string memberId = UserCookieEncryptHelper.Encrypt(member.Id, CookieKeysCollection.USERROLE_USER);
                    data.UserKey = memberId;
                }
            }
            //绑定
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(oauthType) && (!string.IsNullOrEmpty(unionid) || !string.IsNullOrEmpty(oauthOpenId)))
            {
                var service = ServiceProvider.Instance<IMemberService>.Create;
                var member = service.Login(userName, password);
                if (member == null)
                {
                    data = ErrorResult("用户名或密码错误", 103);
                }
                else if (member != null && member.Disabled)
                {
                    data = ErrorResult("用户已被冻结", 105);
                }
                else
                {
                    string wxsex = null;
                    if (null != sex)
                        wxsex = sex.Value.ToString();

                    province = System.Web.HttpUtility.UrlDecode(province);
                    city = System.Web.HttpUtility.UrlDecode(city);

                    service.BindMember(member.Id, oauthType, oauthOpenId, wxsex, headimgurl, unionid, null, city, province);
                    string memberId = UserCookieEncryptHelper.Encrypt(member.Id, CookieKeysCollection.USERROLE_USER);
                    data.success = true;
                    data.UserId = member.Id;
                    data.UserKey = memberId;
                }
            }
            return data;
        }

        #region 重写方法
        protected override bool CheckContact(string contact, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (!string.IsNullOrWhiteSpace(contact))
            {
                var userMenberInfo = Application.MemberApplication.GetMemberByContactInfo(contact);
                if (userMenberInfo != null)
                    Cache.Insert(_encryptKey + contact, string.Format("{0}:{1:yyyyMMddHHmmss}", userMenberInfo.Id, userMenberInfo.CreateDate), DateTime.Now.AddHours(1));
                return userMenberInfo != null;
            }

            return false;
        }

        protected override string CreateCertificate(string contact)
        {
            var identity = Cache.Get<string>(_encryptKey + contact);
            identity = SecureHelper.AESEncrypt(identity, _encryptKey);
            return identity;
        }

        protected override object ChangePassowrdByCertificate(string certificate, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return ErrorResult("密码不能为空");

            certificate = SecureHelper.AESDecrypt(certificate, _encryptKey);
            long userId = long.TryParse(certificate.Split(':')[0], out userId) ? userId : 0;

            if (userId == 0)
                throw new MallException("数据异常");

            Application.MemberApplication.ChangePassword(userId, password);

            return SuccessResult("密码修改成功");
        }

        protected override object ChangePayPwdByCertificate(string certificate, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return ErrorResult("密码不能为空");

            certificate = SecureHelper.AESDecrypt(certificate, _encryptKey);
            long userId = long.TryParse(certificate.Split(':')[0], out userId) ? userId : 0;

            if (userId == 0)
                throw new MallException("数据异常");

            var _iMemberCapitalService = ServiceProvider.Instance<IMemberCapitalService>.Create;

            _iMemberCapitalService.SetPayPwd(userId, password);

            return SuccessResult("支付密码修改成功");
        }
        #endregion
        #endregion
    }
}
