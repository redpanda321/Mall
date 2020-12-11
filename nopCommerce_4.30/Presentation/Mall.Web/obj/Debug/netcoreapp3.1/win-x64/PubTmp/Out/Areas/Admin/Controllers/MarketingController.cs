using Mall.Application;
using Mall.Web.Areas.Admin.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;


namespace Mall.Web.Areas.Admin.Controllers
{
    public class MarketingController : BaseAdminController
    {

        // GET: Admin/sale
        public ActionResult Management()
        {
            ViewBag.Rights = string.Join(",", CurrentManager.AdminPrivileges.Select(a => (int)a).OrderBy(a => a));
            return View();
        }

        #region 充值赠送配置
        public ActionResult RechargePresentRule()
        {
            RechargePresentRuleModel model = new RechargePresentRuleModel();
            model.IsEnable = SiteSettings.IsOpenRechargePresent;
            model.Rules = RechargePresentRuleApplication.GetRules();
            model.RulesJson = JsonConvert.SerializeObject(model.Rules);
            return View(model);
        }
        [HttpPost]
        public JsonResult SaveRechargePresentRule(RechargePresentRuleModel model)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            if (ModelState.IsValid)
            {
                model.CheckValidation();
                var setting = SiteSettingApplication.SiteSettings;
                setting.IsOpenRechargePresent = model.IsEnable;
                SiteSettingApplication.SaveChanges();
                if (model.IsEnable)
                {
                    RechargePresentRuleApplication.SetRules(model.Rules);
                }
                result.success = true;
                result.msg = "配置充值赠送规则成功";
            }
            else
            {
                result.success = false;
                result.msg = "数据错误";
            }
            return Json(result);
        }
        #endregion
    }
}