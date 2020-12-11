using Mall.IServices;
using Mall.Web.Framework;
using System.Collections.Generic;
using System.Linq;

using Mall.Web.Areas.Admin.Models;
using Mall.Entities;
using Mall.Application;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class IntegralRuleController : BaseAdminController
    {
        private IMemberIntegralService _iMemberIntegralService;
        public IntegralRuleController(IMemberIntegralService iMemberIntegralService)
        {
            _iMemberIntegralService = iMemberIntegralService;
        }
        public ActionResult Management()
        {
            var model = _iMemberIntegralService.GetIntegralRule();
            IntegralRule rule = new IntegralRule();
            var bindWX = model.FirstOrDefault(a => a.TypeId == (int)MemberIntegralInfo.IntegralType.BindWX);
            var reg = model.FirstOrDefault(a => a.TypeId == (int)MemberIntegralInfo.IntegralType.Reg);
            var login = model.FirstOrDefault(a => a.TypeId == (int)MemberIntegralInfo.IntegralType.Login);
            var comment = model.FirstOrDefault(a => a.TypeId == (int)MemberIntegralInfo.IntegralType.Comment);
            var share = model.FirstOrDefault(a => a.TypeId == (int)MemberIntegralInfo.IntegralType.Share);

            rule.BindWX = bindWX == null ? 0 : bindWX.Integral;
            rule.Reg = reg == null ? 0 : reg.Integral;
            rule.Comment = comment == null ? 0 : comment.Integral; ;
            rule.Login = login == null ? 0 : login.Integral;
            rule.Share = share == null ? 0 : share.Integral; ;
            var info = _iMemberIntegralService.GetIntegralChangeRule();
            if (info != null)
            {
                rule.MoneyPerIntegral = info.MoneyPerIntegral;
            }
            return View(rule);
        }

        [HttpPost]
        public JsonResult Management(IntegralRule rule)
        {
            List<MemberIntegralRuleInfo> rules = new List<MemberIntegralRuleInfo>();
            rules.Add(new MemberIntegralRuleInfo() { Integral = rule.Reg, TypeId = (int)MemberIntegralInfo.IntegralType.Reg });
            rules.Add(new MemberIntegralRuleInfo() { Integral = rule.BindWX, TypeId = (int)MemberIntegralInfo.IntegralType.BindWX });
            rules.Add(new MemberIntegralRuleInfo() { Integral = rule.Login, TypeId = (int)MemberIntegralInfo.IntegralType.Login });
            rules.Add(new MemberIntegralRuleInfo() { Integral = rule.Comment, TypeId = (int)MemberIntegralInfo.IntegralType.Comment });
            rules.Add(new MemberIntegralRuleInfo() { Integral = rule.Share, TypeId = (int)MemberIntegralInfo.IntegralType.Share });
            _iMemberIntegralService.SetIntegralRule(rules);
            var info = _iMemberIntegralService.GetIntegralChangeRule();
            if (info != null)
            {
                info.MoneyPerIntegral = rule.MoneyPerIntegral;
            }
            else
            {
                info = new MemberIntegralExchangeRuleInfo();
                info.MoneyPerIntegral = rule.MoneyPerIntegral;
            }
            _iMemberIntegralService.SetIntegralChangeRule(info);
            return Json(new Result() { success = true, msg = "保存成功" });
        }

        public ActionResult Change()
        {
            var model = _iMemberIntegralService.GetIntegralChangeRule();
            var IntegralDeductibleRate = SiteSettings.IntegralDeductibleRate;
            ViewBag.IntegralDeductibleRate = IntegralDeductibleRate;
            return View(model);
        }

        [HttpPost]
        public JsonResult Change(int IntegralPerMoney,int IntegralDeductibleRate)
        {
            if(IntegralDeductibleRate<0 || IntegralDeductibleRate > 100)
            {
                return Json(new Result() { success = false, msg = "错误的积分最高抵扣比例" });
            }
            var info = _iMemberIntegralService.GetIntegralChangeRule();
            if (info != null)
            {
                info.IntegralPerMoney = IntegralPerMoney;
            }
            else
            {
                info = new MemberIntegralExchangeRuleInfo();
                info.IntegralPerMoney = IntegralPerMoney;
            }
            //TODO:FG MemberIntegralExchangeRuleInfo 唯一配置值 待移入Setting对象
            _iMemberIntegralService.SetIntegralChangeRule(info);
            var setting = SiteSettingApplication.SiteSettings;
            setting.IntegralDeductibleRate = IntegralDeductibleRate;
            SiteSettingApplication.SaveChanges();
            return Json(new Result() { success = true, msg = "保存成功" });
        }
    }
}