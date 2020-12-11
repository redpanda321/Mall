using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Mall.API
{
    public class ShopBranchLoginController : BaseShopBranchApiController
    {
        #region 静态字段
        private static string _encryptKey = Guid.NewGuid().ToString("N");
        #endregion

        #region 方法

        [HttpGet("GetUser")]
        public object GetUser(string userName, string password)
        {
            var siteSettings = SiteSettingApplication.SiteSettings;

            //普通登录
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                //商家登录也放在这里，因为商家app和门店app为同一个并且没做登录区分，只能通过登录后才能知道是登录的商家还是门店管理员
                var seller = ManagerApplication.Login(userName, password);
                dynamic result = SuccessResult();
                if (seller != null)
                {
                    if (!siteSettings.IsOpenShopApp)
                        return ErrorResult("未授权商家APP");

                    var shop = ShopApplication.GetShop(seller.ShopId);
                    if (shop != null && shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.HasExpired)
                        return ErrorResult("店铺已过期");
                    if (null != shop && shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.Freeze)
                        return ErrorResult("店铺已冻结");
                    if (shop != null && shop.ShopStatus != Entities.ShopInfo.ShopAuditStatus.Open)
                        return ErrorResult("无效的账号");
                    if (seller.RoleId != 0)
                    {
                        var model = RoleApplication.GetRoleInfo(seller.RoleId);
                        //TODO:FG 权限验证 实现逻辑待优化
                        var SellerPrivileges = RoleApplication.GetSellerPrivileges(seller.RoleId);
                        if (!SellerPermission.CheckPermissions(SellerPrivileges, "App", "App"))
                        {
                            return ErrorResult("您没有登录商家APP的权限");
                        }
                    }
                    result = SuccessResult();
                    result.UserKey = UserCookieEncryptHelper.Encrypt(seller.Id, CookieKeysCollection.USERROLE_SELLERADMIN);
                    result.type = ManagerType.ShopManager;
                    return result;
                }
                var member =ShopBranchApplication.ShopBranchLogin(userName, password);
                if (member == null)
                {
                    return ErrorResult("用户名或密码错误");
                }
                var shopbranch = ShopBranchApplication.GetShopBranchById(member.ShopBranchId);
                if (shopbranch != null)
                {
                    if (shopbranch.Status == ShopBranchStatus.Freeze)
                    {
                        return ErrorResult("门店已被冻结");
                    }
                    if (!siteSettings.IsOpenStore)
                    {
                        return ErrorResult("未授权门店模块");
                    }
                }
                var membershop = ShopApplication.GetShop(shopbranch.ShopId);
                if (membershop != null && membershop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.HasExpired)
                {
                    return ErrorResult("门店所属店铺已过期");
                }
                if (null != membershop && membershop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.Freeze)
                    return ErrorResult("门店所属店铺已冻结");
                if (member != null)
                {
                    result = SuccessResult();
                    result.UserKey = UserCookieEncryptHelper.Encrypt(member.Id, CookieKeysCollection.USERROLE_USER);
                    result.type = ManagerType.ShopBranchManager;
                    return result;
                }
            }
            return ErrorResult("用户名或密码不能为空");
        }

       
        protected override bool CheckContact(string contact, out string errorMessage)
        {
            errorMessage = string.Empty;
            var shopBranch = ShopBranchApplication.GetShopBranchByContact(contact);
            if (shopBranch == null)
                return false;

            var manager = ShopBranchApplication.GetShopBranchManagerByShopBranchId(shopBranch.Id);
            if (manager == null) return false; ;
                Cache.Insert(_encryptKey + contact, string.Format("{0}:{1:yyyyMMddHHmmss}", manager.Id, manager.CreateDate), DateTime.Now.AddHours(1));
            return true;
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

            ShopBranchApplication.SetManagerPassword(userId, password);
            return SuccessResult("密码修改成功");
        }

        protected override object ChangePasswordByOldPassword(string oldPassword, string password)
        {
            CheckUserLogin();

            if (string.IsNullOrWhiteSpace(password))
                return ErrorResult("密码不能为空");
            
            if (!ShopBranchApplication.CheckManagerPassword(CurrentUser.Id, oldPassword))
                return ErrorResult("旧密码输入不正确");

            ShopBranchApplication.SetManagerPassword(CurrentUser.Id, password);
            return SuccessResult("密码修改成功");
        }
        #endregion
    }
}
