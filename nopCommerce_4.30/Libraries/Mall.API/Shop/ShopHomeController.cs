using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Web.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Infrastructure;
using Microsoft.AspNetCore.Hosting;

namespace Mall.API
{
    public class ShopHomeController : BaseShopLoginedApiController
    {

        [HttpGet("GetShopHome")]
        public object GetShopHome()
        {
            CheckUserLogin();
            var now = DateTime.Now;

            var orderQuery = new OrderCountStatisticsQuery()
            {
                ShopId = CurrentUser.ShopId,
                Fields = new List<OrderCountStatisticsFields> {
                    OrderCountStatisticsFields.ActualPayAmount
                }
            };
            //三月内
            orderQuery.OrderDateBegin = new DateTime(now.Year, now.Month, 1).AddMonths(-2);
            var threeMonthAmount = StatisticApplication.GetOrderCount(orderQuery).TotalActualPayAmount;
            //本周
            orderQuery.OrderDateBegin = now.Date.AddDays(-(int)now.DayOfWeek);
            var weekAmount = StatisticApplication.GetOrderCount(orderQuery).TotalActualPayAmount;
            //今天
            orderQuery.OrderDateBegin = now.Date;
            var todayAmount = StatisticApplication.GetOrderCount(orderQuery).TotalActualPayAmount;

            //近三天发布商品数
            var productCount = ProductManagerApplication.GetProductCount(new ProductQuery
            {
                ShopId = CurrentShop.Id,
                AuditStatus = new[] { Entities.ProductInfo.ProductAuditStatus.Audited },
                StartDate = now.Date.AddDays(-2)
            });

            //待审核退货/退款
            var refundCount = RefundApplication.GetOrderRefundsCount(new RefundQuery()
            {
                ShopId = CurrentUser.ShopId,
                AuditStatus = Entities.OrderRefundInfo.OrderRefundAuditStatus.WaitAudit,
            });
            return new
            {
                success = true,
                data = new
                {
                    shopName = CurrentShop.ShopName,
                    todayAmount = todayAmount,
                    weekAmount = weekAmount,
                    threeMonthAmounht = threeMonthAmount,
                    createProductCount = productCount,
                    refundCount = refundCount
                }
            };
        }
        [HttpGet("GetUpdateApp")]
        public object GetUpdateApp(string appVersion, int type)
        {
            var siteSetting = SiteSettingApplication.SiteSettings;

            if (string.IsNullOrWhiteSpace(appVersion) || (3 < type && type < 2))
            {
                return ErrorResult("版本号不能为空或者平台类型错误", 10006);
            }
            Version ver = null;
            try
            {
                ver = new Version(appVersion);
            }
            catch (Exception)
            {
                return ErrorResult("错误的版本号", 10005);
            }
            if (string.IsNullOrWhiteSpace(siteSetting.ShopAppVersion))
            {
                siteSetting.ShopAppVersion = "0.0.0";
            }
            var downLoadUrl = "";
            Version v1 = new Version(siteSetting.ShopAppVersion), v2 = new Version(appVersion);
            if (v1 > v2)
            {
                if (type == (int)PlatformType.IOS)
                {
                    if (string.IsNullOrWhiteSpace(siteSetting.ShopIOSDownLoad))
                    {
                        return ErrorResult("站点未设置IOS下载地址", 10004);
                    }
                    downLoadUrl = siteSetting.ShopIOSDownLoad;
                }
                if (type == (int)PlatformType.Android)
                {
                    if (string.IsNullOrWhiteSpace(siteSetting.ShopAndriodDownLoad))
                    {
                        return ErrorResult("站点未设置Andriod下载地址", 10003);
                    }
                    string str = siteSetting.ShopAndriodDownLoad.Substring(siteSetting.ShopAndriodDownLoad.LastIndexOf("/"), siteSetting.ShopAndriodDownLoad.Length - siteSetting.ShopAndriodDownLoad.LastIndexOf("/"));
                    //var curProjRootPath = System.Web.Hosting.HostingEnvironment.MapPath("~/app") + str;
                    var env = EngineContext.Current.Resolve<IWebHostEnvironment>();
                    var curProjRootPath = env.ContentRootPath + "/app" + str;


                    if (!System.IO.File.Exists(curProjRootPath))
                    {
                        return ErrorResult("站点未上传app安装包", 10002);
                    }
                    downLoadUrl = siteSetting.ShopAndriodDownLoad;
                }
            }
            else
            {
                return ErrorResult("当前为最新版本", 10001);
            }

            return new { success = true, code = 10000, DownLoadUrl = downLoadUrl, Description = siteSetting.AppUpdateDescription };
        }
        /// <summary>
        /// 获取未读消息数
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetNoReadMessageCount")]
        public object GetNoReadMessageCount()
        {
            CheckUserLogin();
            long shopid = CurrentUser.ShopId;
            int count = AppMessageApplication.GetShopNoReadMessageCount(shopid);
            return new { success = true, count = count };
        }
        /// <summary>
        /// 获取可管理门店列表
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetManagerShops")]
        public object GetManagerShops()
        {
            var branchs = ShopBranchApplication.GetSellerManager(CurrentUser.ShopId);
            var unReads = AppMessageApplication.GetBranchNoReadMessageCount(branchs.Select(p => p.Id).ToList());
            var data = branchs.Select(p =>
            new
            {
                id = p.Id,
                branchName = p.ShopBranchName,
                unRead = unReads.ContainsKey(p.Id) ? unReads[p.Id] : 0
            });

            return new { success = true, data };
        }
        /// <summary>
        /// 越权登录
        /// </summary>
        /// <param name="branchId"></param>
        /// <returns></returns>
        [HttpGet("JumpShopBranch")]
        public object JumpShopBranch(long branchId)
        {
            var branch = ShopBranchApplication.GetShopBranchById(branchId);
            if (branch.ShopId != CurrentUser.ShopId)
                return ErrorResult("非法操作,非所属门店。");//越权至非当前商家的门店
            if (!branch.EnableSellerManager)
                return ErrorResult("门店未开启商家管理");
            var manager = ShopBranchApplication.GetShopBranchManagerByShopBranchId(branch.Id);
            var userkey = UserCookieEncryptHelper.Encrypt(manager.Id, CookieKeysCollection.USERROLE_USER);
            return new { success = true, userKey = userkey };
        }
    }
}
