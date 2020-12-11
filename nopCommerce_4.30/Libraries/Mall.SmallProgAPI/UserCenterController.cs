using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.SmallProgAPI.Model;
using Mall.SmallProgAPI.Model.ParamsModel;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.SmallProgAPI
{
    public class UserCenterController : BaseApiController
    {
        private static string _encryptKey = Guid.NewGuid().ToString("N");
        /// <summary>
        /// 个人中心主页
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpGet("GetUser")]
        public new object GetUser()
        {
            CheckUserLogin();
            dynamic d = new System.Dynamic.ExpandoObject();
            long id = CurrentUser.Id;
            var member = MemberApplication.GetMember(id);
            DistributorInfo currentDistributor = DistributionApplication.GetDistributor(member.Id);

            d.UserName = member.UserName;//用户名
            d.RealName = member.RealName;//真实姓名
            d.Nick = member.Nick;//昵称 
            d.UserId = member.Id.ToString();
            d.CellPhone = member.CellPhone;//绑定的手机号码
            d.Photo = String.IsNullOrEmpty(member.Photo) ? "" : MallIO.GetRomoteImagePath(member.Photo);//头像

            var statistic = StatisticApplication.GetMemberOrderStatistic(id, true);
            d.AllOrders = statistic.OrderCount;
            d.WaitingForPay = statistic.WaitingForPay;
            d.WaitingForRecieve = statistic.WaitingForRecieve + OrderApplication.GetWaitConsumptionOrderNumByUserId(id);
            d.WaitingForDelivery = statistic.WaitingForDelivery;
            d.WaitingForComments = statistic.WaitingForComments;
            d.RefundOrders = statistic.RefundCount;

            d.FavoriteShop = ShopApplication.GetUserConcernShopsCount(member.Id); //收藏的店铺数
            d.FavoriteProduct = FavoriteApplication.GetFavoriteCountByUser(member.Id); //收藏的商品数

            d.Counpon = MemberApplication.GetAvailableCouponCount(id);

            d.Integral = MemberIntegralApplication.GetAvailableIntegral(member.Id);//我的积分
            d.Balance = MemberCapitalApplication.GetBalanceByUserId(member.Id);//我的资产
            d.IsOpenRechargePresent = SiteSettingApplication.SiteSettings.IsOpenRechargePresent;
            var phone = SiteSettingApplication.SiteSettings.SitePhone;
            d.ServicePhone = string.IsNullOrEmpty(phone) ? "" : phone;
            d.IsDistributor = (currentDistributor != null && currentDistributor.DistributionStatus == (int)DistributorStatus.Audited);
            return Json(d);
        }
        [HttpGet("GetIntegralRecordList")]
        public object GetIntegralRecordList(string openId, int pageNo = 1, int pageSize = 10)
        {
            CheckUserLogin();
            IntegralRecordQuery query = new IntegralRecordQuery
            {
                UserId = CurrentUserId,
                PageNo = pageNo,
                PageSize = pageSize
            };
            var list = MemberIntegralApplication.GetIntegralRecordList(query);
            if (list.Models != null)
            {
                var recordlist = list.Models.Select(a =>
                {
                    var actions = ServiceProvider.Instance<IMemberIntegralService>.Create.GetIntegralRecordAction(a.Id);
                    return new
                    {
                        Id = a.Id,
                        MemberId = a.MemberId,
                        UserName = a.UserName,
                        TypeName = (a.TypeId == MemberIntegralInfo.IntegralType.WeiActivity) ? a.ReMark : a.TypeId.ToDescription(),
                        Integral = a.Integral,
                        RecordDate = ((DateTime)a.RecordDate).ToString("yyyy-MM-dd HH:mm:ss"),
                        ReMark = GetRemarkFromIntegralType(a.TypeId, actions, a.ReMark)
                    };
                });
                return Json(recordlist);
            }

            return Json(new int[0]);
        }

        private string GetRemarkFromIntegralType(MemberIntegralInfo.IntegralType type, ICollection<MemberIntegralRecordActionInfo> recordAction, string remark = "")
        {
            if (recordAction == null || recordAction.Count == 0)
                return remark;
            switch (type)
            {

                case MemberIntegralInfo.IntegralType.Consumption:
                    var orderIds = "";
                    foreach (var item in recordAction)
                    {
                        orderIds += item.VirtualItemId + ",";
                    }
                    remark = "订单号：" + orderIds.TrimEnd(',');
                    break;
                default:
                    return remark;
            }
            return remark;
        }

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
        protected override object  ChangePayPwdByCertificate(string certificate, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return Json(ErrorResult<int>("密码不能为空"));

            certificate = SecureHelper.AESDecrypt(certificate, _encryptKey);
            long userId = long.TryParse(certificate.Split(':')[0], out userId) ? userId : 0;

            if (userId == 0)
                throw new MallException("数据异常");

            var _iMemberCapitalService = ServiceProvider.Instance<IMemberCapitalService>.Create;

            _iMemberCapitalService.SetPayPwd(userId, password);
            return Json(new { msg = "支付密码修改成功" });
        }

        /// <summary>
        /// 用户收藏的商品
        /// </summary>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        /// 

        [HttpGet("GetUserCollectionProduct")]
        public object GetUserCollectionProduct(int pageNo = 1, int pageSize = 16)
        {
            CheckUserLogin();
            if (CurrentUser != null)
            {
                var model = ServiceProvider.Instance<IProductService>.Create.GetUserConcernProducts(CurrentUser.Id, pageNo, pageSize);
                var result = model.Models.ToArray().Select(item =>
                {
                    var pro = ProductManagerApplication.GetProduct(item.ProductId);
                    return new
                    {
                        Id = item.ProductId,
                        Image = MallIO.GetRomoteProductSizeImage(pro.RelativePath, 1, (int)Mall.CommonModel.ImageSize.Size_220),
                        ProductName = pro.ProductName,
                        SalePrice = pro.MinSalePrice.ToString("F2"),
                        Evaluation = CommentApplication.GetCommentCountByProduct(pro.Id),
                        Status = ProductManagerApplication.GetProductShowStatus(pro)
                    };
                });
                return new { success = true, data = result, total = model.Total };
            }
            else
            {
                return new Result { success = false, msg = "未登录" };
            }
        }
        /// <summary>
        /// 删除关注商品
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetCancelConcernProduct")]
        public object GetCancelConcernProduct(long productId)
        {
            CheckUserLogin();
            if (CurrentUser != null)
            {
                if (productId < 1)
                {
                    throw new MallException("错误的参数");
                }
                ServiceProvider.Instance<IProductService>.Create.DeleteFavorite(productId, CurrentUser.Id);

                return new { success = true };
            }
            else
            {
                return new Result { success = false, msg = "未登录" };
            }
        }

        /// <summary>
        /// 用户收藏的店铺
        /// </summary>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetUserCollectionShop")]
        public object GetUserCollectionShop(int pageNo = 1, int pageSize = 8)
        {
            CheckUserLogin();
            if (CurrentUser != null)
            {
                var model = ServiceProvider.Instance<IShopService>.Create.GetUserConcernShops(CurrentUser.Id, pageNo, pageSize);

                var result = model.Models.Select(item =>
                {
                    var shop = ShopApplication.GetShop(item.ShopId);
                    var vShop = VshopApplication.GetVShopByShopId(item.ShopId);
                    return new
                    {
                        //Id = item.Id,
                        Id = vShop?.Id ?? 0,
                        ShopId = item.ShopId,
                        Logo = vShop == null ? MallIO.GetRomoteImagePath(shop.Logo) : MallIO.GetRomoteImagePath(vShop.Logo),
                        Name = shop.ShopName,
                        Status = shop.ShopStatus,
                        ConcernTime = item.Date,
                        ConcernTimeStr = item.Date.ToString("yyyy-MM-dd"),
                        ConcernCount = FavoriteApplication.GetFavoriteShopCountByShop(item.ShopId)
                    };
                });
                return new { success = true, data = result };
            }
            else
            {
                return new Result { success = false, msg = "未登录" };
            }
        }

        /// <summary>
        /// 取消店铺关注
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetCancelConcernShop")]
        public object GetCancelConcernShop(long shopId)
        {
            CheckUserLogin();
            if (CurrentUser != null)
            {
                ServiceProvider.Instance<IShopService>.Create.CancelConcernShops(shopId, CurrentUser.Id);
                return new Result() { success = true, msg = "取消成功！" };
            }
            else
            {
                return new Result { success = false, msg = "未登录" };
            }
        }

        /// <summary>
        /// 用户是否没设密码
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetIsNotSetPwd")]
        public object GetIsNotSetPwd()
        {
            CheckUserLogin();

            bool isLoginPwdNotModify = false;//信任登录后密码是否没修改
            if (!string.IsNullOrEmpty(CurrentUser.PasswordSalt))
                isLoginPwdNotModify = CurrentUser.PasswordSalt.StartsWith("o");//如信任登录加密盐第一个字符串是“o”

            var result = new
            {
                IsLoginPwdNotModify = isLoginPwdNotModify,
            };
            return new { success = true, data = result };
        }

        /// <summary>
        /// 根据旧密码修改密码
        /// </summary>
        /// <param name="oldPassword"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// 
        [HttpGet("ChangePasswordByOld")]
        public object ChangePasswordByOld(string oldPassword, string password)
        {
            CheckUserLogin();

            var _iMemberService = ServiceProvider.Instance<IMemberService>.Create;

            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(password))
            {
                return new { success = false, msg = "密码不能为空！" };
            }
            var model = CurrentUser;
            var pwd = SecureHelper.MD5(SecureHelper.MD5(oldPassword) + model.PasswordSalt);
            bool CanChange = false;
            if (pwd == model.Password)
            {
                CanChange = true;
            }
            if (model.PasswordSalt.StartsWith("o"))
            {
                CanChange = true;
            }
            if (CanChange)
            {
                Application.MemberApplication.ChangePassword(model.Id, password);
                return new { success = true, msg = "密码修改成功！" };
            }
            else
                return new { success = false, msg = "旧密码错误！" };
        }

    }
}
