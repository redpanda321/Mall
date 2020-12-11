using System;
using System.Collections.Generic;
using System.Linq;

using Mall.Core;
using Mall.DTO;
using Mall.Web.Framework;
using Mall.CommonModel;
using Mall.Application;
using Mall.DTO.QueryModel;
using Mall.Web.Areas.Admin.Models.Distribution;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    //[H5Authorization]
    /// <summary>
    /// 分销设置
    /// </summary>
    public class DistributionConfigController : BaseAdminController
    {
        #region 基础设置
        /// <summary>
        /// 基础设置
        /// </summary>
        /// <returns></returns>
        public ActionResult BaseConfig()
        {
            var sc = SiteSettings;
            var config = new DistributionConfigBaseConfigModel
            {
                DistributionCanSelfBuy = sc.DistributionCanSelfBuy,
                DistributionIsEnable = sc.DistributionIsEnable,
                DistributionIsProductShowTips = sc.DistributionIsProductShowTips,
                DistributionMaxBrokerageRate = sc.DistributionMaxBrokerageRate,
                DistributionMaxLevel = sc.DistributionMaxLevel,
                DistributorApplyNeedQuota = sc.DistributorApplyNeedQuota,
                DistributorNeedAudit = sc.DistributorNeedAudit,
            };
            return View(config);
        }

        public JsonResult SaveBaseConfig(DistributionConfigBaseConfigModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new MallException("有错误的参数");
            }
            if (model.DistributionIsEnable)
            {
                if ((model.DistributionMaxLevel < 1 || model.DistributionMaxLevel > 3))
                {
                    throw new MallException("错误的分销等级设置");
                }
                if ((model.DistributionMaxBrokerageRate < 0.1m || model.DistributionMaxBrokerageRate > 100))
                {
                    throw new MallException("最高分佣比例需在0.1%~100%之间，且只能保留一位小数!");
                }
            }
            SiteSettingApplication.SiteSettings.DistributionCanSelfBuy = model.DistributionCanSelfBuy;
            SiteSettingApplication.SiteSettings.DistributionIsEnable = model.DistributionIsEnable;
            SiteSettingApplication.SiteSettings.DistributionIsProductShowTips = model.DistributionIsProductShowTips;
            SiteSettingApplication.SiteSettings.DistributionMaxBrokerageRate = model.DistributionMaxBrokerageRate;
            SiteSettingApplication.SiteSettings.DistributionMaxLevel = model.DistributionMaxLevel;
            SiteSettingApplication.SiteSettings.DistributorApplyNeedQuota = model.DistributorApplyNeedQuota;
            SiteSettingApplication.SiteSettings.DistributorNeedAudit = model.DistributorNeedAudit;
            SiteSettingApplication.SaveChanges();
            DistributionApplication.ResetDefaultBrokerageRate(SiteSettingApplication.SiteSettings.DistributionMaxLevel);
            return Json(new { success = true });
        }
        #endregion


        #region 页面设置
        /// <summary>
        /// 基础设置
        /// </summary>
        /// <returns></returns>
        public ActionResult PageConfig()
        {
            ViewBag.content = SiteSettingApplication.SiteSettings.DistributorPageContent;
            return View();
        }
       
        public JsonResult SavePageConfig(string content)
        {
            SiteSettingApplication.SiteSettings.DistributorPageContent = content;
            SiteSettingApplication.SaveChanges();
            return Json(new { success = true });
        }
        #endregion


        #region 分销关键字重命名
        /// <summary>
        /// 基础设置
        /// </summary>
        /// <returns></returns>
        public ActionResult RenameConfig()
        {
            var sc = SiteSettings;
            var config = new DistributionConfigRenameConfigModel
            {
                DistributorRenameBrokerage = sc.DistributorRenameBrokerage,
                DistributorRenameMarket = sc.DistributorRenameMarket,
                DistributorRenameMemberLevel1 = sc.DistributorRenameMemberLevel1,
                DistributorRenameMemberLevel2 = sc.DistributorRenameMemberLevel2,
                DistributorRenameMemberLevel3 = sc.DistributorRenameMemberLevel3,
                DistributorRenameMyBrokerage = sc.DistributorRenameMyBrokerage,
                DistributorRenameMyShop = sc.DistributorRenameMyShop,
                DistributorRenameMySubordinate = sc.DistributorRenameMySubordinate,
                DistributorRenameOpenMyShop = sc.DistributorRenameOpenMyShop,
                DistributorRenameShopConfig = sc.DistributorRenameShopConfig,
                DistributorRenameShopOrder = sc.DistributorRenameShopOrder,
                DistributorRenameSpreadShop = sc.DistributorRenameSpreadShop,
            };
            return View(config);
        }

        public JsonResult SaveRenameConfig(DistributionConfigRenameConfigModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new MallException("所有选项必填");
            }
            SiteSettingApplication.SiteSettings.DistributorRenameBrokerage = model.DistributorRenameBrokerage;
            SiteSettingApplication.SiteSettings.DistributorRenameMarket = model.DistributorRenameMarket;
            SiteSettingApplication.SiteSettings.DistributorRenameMemberLevel1 = model.DistributorRenameMemberLevel1;
            SiteSettingApplication.SiteSettings.DistributorRenameMemberLevel2 = model.DistributorRenameMemberLevel2;
            SiteSettingApplication.SiteSettings.DistributorRenameMemberLevel3 = model.DistributorRenameMemberLevel3;
            SiteSettingApplication.SiteSettings.DistributorRenameMyBrokerage = model.DistributorRenameMyBrokerage;
            SiteSettingApplication.SiteSettings.DistributorRenameMyShop = model.DistributorRenameMyShop;
            SiteSettingApplication.SiteSettings.DistributorRenameMySubordinate = model.DistributorRenameMySubordinate;
            SiteSettingApplication.SiteSettings.DistributorRenameOpenMyShop = model.DistributorRenameOpenMyShop;
            SiteSettingApplication.SiteSettings.DistributorRenameShopConfig = model.DistributorRenameShopConfig;
            SiteSettingApplication.SiteSettings.DistributorRenameShopOrder = model.DistributorRenameShopOrder;
            SiteSettingApplication.SiteSettings.DistributorRenameSpreadShop = model.DistributorRenameSpreadShop;
            SiteSettingApplication.SaveChanges();
            return Json(new { success = true });
        }
        #endregion

    }
}