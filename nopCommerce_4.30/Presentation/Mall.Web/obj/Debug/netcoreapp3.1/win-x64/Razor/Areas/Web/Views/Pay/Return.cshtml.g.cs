#pragma checksum "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Pay\Return.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "df146891fa4ac41afdbc527cef97939eb36b62ef"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Areas_Web_Views_Pay_Return), @"mvc.1.0.view", @"/Areas/Web/Views/Pay/Return.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
#nullable restore
#line 13 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Mvc.ViewFeatures;

#line default
#line hidden
#nullable disable
#nullable restore
#line 14 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Html;

#line default
#line hidden
#nullable disable
#nullable restore
#line 15 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Mvc.Rendering;

#line default
#line hidden
#nullable disable
#nullable restore
#line 16 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Http.Extensions;

#line default
#line hidden
#nullable disable
#nullable restore
#line 17 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Mvc;

#line default
#line hidden
#nullable disable
#nullable restore
#line 18 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Mvc.Razor;

#line default
#line hidden
#nullable disable
#nullable restore
#line 20 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using System.Net;

#line default
#line hidden
#nullable disable
#nullable restore
#line 21 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using System.IO;

#line default
#line hidden
#nullable disable
#nullable restore
#line 22 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using System.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 25 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using Nop.Core;

#line default
#line hidden
#nullable disable
#nullable restore
#line 28 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using Mall.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 29 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using Mall.Web.Framework;

#line default
#line hidden
#nullable disable
#nullable restore
#line 30 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using Mall.DTO;

#line default
#line hidden
#nullable disable
#nullable restore
#line 31 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using Mall.Entities;

#line default
#line hidden
#nullable disable
#nullable restore
#line 33 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using Mall.Web.Areas.SellerAdmin.Models;

#line default
#line hidden
#nullable disable
#nullable restore
#line 34 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using Mall.Web.Areas.Web.Models;

#line default
#line hidden
#nullable disable
#nullable restore
#line 36 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using Mall.Service;

#line default
#line hidden
#nullable disable
#nullable restore
#line 37 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using Mall.Core;

#line default
#line hidden
#nullable disable
#nullable restore
#line 41 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\_ViewImports.cshtml"
using Mall.CommonModel;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"df146891fa4ac41afdbc527cef97939eb36b62ef", @"/Areas/Web/Views/Pay/Return.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"090c48694750ba7c62e752bf94ad67a159f3356f", @"/Areas/Web/Views/_ViewImports.cshtml")]
    public class Areas_Web_Views_Pay_Return : Mall.Web.Framework.WebViewPage<dynamic>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\r\n");
#nullable restore
#line 2 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Pay\Return.cshtml"
  
    ViewBag.Title = "订单支付结果";
   // Layout = "~/Areas/Web/Views/Shared/_OrderTopBar.cshtml";

#line default
#line hidden
#nullable disable
            WriteLiteral("<div class=\"w cl\">\r\n");
#nullable restore
#line 7 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Pay\Return.cshtml"
      
        var errorMsg = ViewBag.Error;
        var flag = !string.IsNullOrWhiteSpace(errorMsg) ? "fail" : "success";
        var redirectUrl = string.IsNullOrWhiteSpace(errorMsg) ? "/userOrder" : ("/order/pay?orderIds=" + ViewBag.OrderIds);
        var htmlPart = string.IsNullOrWhiteSpace(errorMsg) ? ("秒后自动跳至<a  href=\"/userOrder\" class=\"link_1\">个人中心</a>") : ("秒后自动跳至支付页面<a  href=\"/order/pay?orderIds=" + ViewBag.OrderIds+"\" class=\"link_1\">支付页面</a>");
        

#line default
#line hidden
#nullable disable
            WriteLiteral("    <div");
            BeginWriteAttribute("class", " class=\"", 615, "\"", 647, 4);
            WriteAttributeValue("", 623, "tips-page", 623, 9, true);
            WriteAttributeValue(" ", 632, "pay-", 633, 5, true);
#nullable restore
#line 13 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Pay\Return.cshtml"
WriteAttributeValue("", 637, flag, 637, 5, false);

#line default
#line hidden
#nullable disable
            WriteAttributeValue("", 642, "-page", 642, 5, true);
            EndWriteAttribute();
            WriteLiteral(">\r\n");
#nullable restore
#line 14 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Pay\Return.cshtml"
         if (string.IsNullOrWhiteSpace(errorMsg))
        {

#line default
#line hidden
#nullable disable
            WriteLiteral("            <h2>\r\n                恭喜您，支付成功！\r\n            </h2>\r\n            <div class=\"error_child\">您可以： <a href=\"/\">继续购买</a>。</div>\r\n");
#nullable restore
#line 20 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Pay\Return.cshtml"
        }
        else
        {

#line default
#line hidden
#nullable disable
            WriteLiteral("            <h2>\r\n                抱歉，支付失败！请重新支付！\r\n            </h2>\r\n            <div class=\"error_child\">您可以：<a");
            BeginWriteAttribute("href", " href=\"", 996, "\"", 1042, 2);
            WriteAttributeValue("", 1003, "/order/pay?orderIds=", 1003, 20, true);
#nullable restore
#line 26 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Pay\Return.cshtml"
WriteAttributeValue("", 1023, ViewBag.OrderIds, 1023, 19, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(">重新支付</a></div>\r\n");
#nullable restore
#line 27 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Pay\Return.cshtml"
        }

#line default
#line hidden
#nullable disable
            WriteLiteral(@"      
        <div id=""ShowDiv""></div>
    </div>

</div>


<script>
    $(function () {
        $('.progress-').hide();
    })

    var secs = 5; //倒计时的秒数
    var URL ;
    function Load(url){
        URL =url;
        for(var i=secs;i>=0;i--)
        {
            window.setTimeout('doUpdate(' + i + ')', (secs-i) * 1000);
        }
    }
        
    function doUpdate(num)
    {
        document.getElementById(""ShowDiv"").innerHTML = '将在<strong><font color=red> '+num+' </font></strong>");
#nullable restore
#line 52 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Pay\Return.cshtml"
                                                                                                      Write(Html.Raw(htmlPart));

#line default
#line hidden
#nullable disable
            WriteLiteral("，请稍候...\' ;\r\n        if(num == 0) { window.location=URL;  }\r\n    }\r\n    $(function(){\r\n        Load(\"");
#nullable restore
#line 56 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Pay\Return.cshtml"
         Write(redirectUrl);

#line default
#line hidden
#nullable disable
            WriteLiteral("\");\r\n    })\r\n\r\r\n\r\n</script>");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591