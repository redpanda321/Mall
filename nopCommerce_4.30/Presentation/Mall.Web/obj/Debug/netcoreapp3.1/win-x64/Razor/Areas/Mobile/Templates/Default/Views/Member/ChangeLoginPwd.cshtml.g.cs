#pragma checksum "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\Member\ChangeLoginPwd.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "b2f5e82b02769a48807fa44975a6043e5de35ef4"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Areas_Mobile_Templates_Default_Views_Member_ChangeLoginPwd), @"mvc.1.0.view", @"/Areas/Mobile/Templates/Default/Views/Member/ChangeLoginPwd.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
#nullable restore
#line 10 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Mvc.ViewFeatures;

#line default
#line hidden
#nullable disable
#nullable restore
#line 11 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Html;

#line default
#line hidden
#nullable disable
#nullable restore
#line 12 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Mvc.Rendering;

#line default
#line hidden
#nullable disable
#nullable restore
#line 13 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Http.Extensions;

#line default
#line hidden
#nullable disable
#nullable restore
#line 14 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Mvc;

#line default
#line hidden
#nullable disable
#nullable restore
#line 15 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Mvc.Razor;

#line default
#line hidden
#nullable disable
#nullable restore
#line 16 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Routing;

#line default
#line hidden
#nullable disable
#nullable restore
#line 18 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using System.Net;

#line default
#line hidden
#nullable disable
#nullable restore
#line 19 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using System.IO;

#line default
#line hidden
#nullable disable
#nullable restore
#line 20 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using System.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 22 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using Mall.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 23 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using Mall.Web.Framework;

#line default
#line hidden
#nullable disable
#nullable restore
#line 24 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using Mall.Web.Framework.Infrastructure;

#line default
#line hidden
#nullable disable
#nullable restore
#line 25 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using Mall.DTO;

#line default
#line hidden
#nullable disable
#nullable restore
#line 26 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using Mall.Service;

#line default
#line hidden
#nullable disable
#nullable restore
#line 27 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using Mall.Core;

#line default
#line hidden
#nullable disable
#nullable restore
#line 28 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using Mall.Entities;

#line default
#line hidden
#nullable disable
#nullable restore
#line 30 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\_ViewImports.cshtml"
using Mall.CommonModel;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"b2f5e82b02769a48807fa44975a6043e5de35ef4", @"/Areas/Mobile/Templates/Default/Views/Member/ChangeLoginPwd.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"251375f151a8ed74fad18b07a59f5490bcea9ba2", @"/Areas/Mobile/Templates/Default/Views/_ViewImports.cshtml")]
    public class Areas_Mobile_Templates_Default_Views_Member_ChangeLoginPwd : Mall.Web.Framework.MobileWebViewPage<Mall.Entities.MemberInfo>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#nullable restore
#line 2 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\Member\ChangeLoginPwd.cshtml"
  
    ViewBag.Title = "修改密码";
    bool isFirst = false;
    isFirst = Model.PasswordSalt.StartsWith("o");

#line default
#line hidden
#nullable disable
            WriteLiteral("<style> html,body{ background: #fff;}</style>\r\n<div class=\"container  \">\r\n    <div class=\"Acc-Manag\">\r\n        <label>用户账号：<span>");
#nullable restore
#line 14 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\Member\ChangeLoginPwd.cshtml"
                     Write(Model.UserName);

#line default
#line hidden
#nullable disable
            WriteLiteral("</span></label>\r\n        <input id=\"old\" type=\"password\" placeholder=\"请输入老密码\"");
            BeginWriteAttribute("value", " value=\"", 623, "\"", 677, 1);
#nullable restore
#line 15 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\Member\ChangeLoginPwd.cshtml"
WriteAttributeValue("", 631, isFirst ? Guid.NewGuid().ToString("N") : "", 631, 46, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" ");
#nullable restore
#line 15 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\Member\ChangeLoginPwd.cshtml"
                                                                                                                Write(isFirst ? "disabled" : "");

#line default
#line hidden
#nullable disable
            WriteLiteral(@"/>
        <input id=""new"" type=""password"" placeholder=""请输入新密码"" />
        <input id=""repeat"" type=""password"" placeholder=""请确认密码"" />
        <input id=""change"" class=""btn"" type=""button"" value=""提交"" />
    </div>
</div>
<script>
    $('#change').click(function () {
        var password = $.trim($('#old').val());
        var newPassword = $.trim($('#new').val());
        var repeatPassword = $.trim($('#repeat').val());
        if (!password) {
            $.dialog.errorTips('请输入密码');
            return;
        }
        if (!newPassword) {
            $.dialog.errorTips('请输入新密码');
            return;
        }
        if (!repeatPassword) {
            $.dialog.errorTips('请输入确认密码');
            return;
        }
        if (newPassword != repeatPassword) {
            $.dialog.errorTips('两次输入的密码不一致');
            return;
        }
        if (newPassword < 6 && newPassword > 20) {
            $.dialog.errorTips(""密码长度在6-20位之间"");
            return;
        }
        var loading = s");
            WriteLiteral(@"howLoading(""数据提交中..."");
        $.post('/' + areaName + '/Member/ChangePassword', { oldpassword: password, password: newPassword }, function (result) {
            loading.close();
            if(result.success)
            {
                $.dialog.succeedTips('修改成功!', function () {
                    var returnUrl = '/' + areaName + '/Member/Center';
                    location.replace(decodeURIComponent(returnUrl));
                })
            }
            else
            {
                $.dialog.errorTips('修改失败' + result.msg);
            }
        });
    });
</script>");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<Mall.Entities.MemberInfo> Html { get; private set; }
    }
}
#pragma warning restore 1591
