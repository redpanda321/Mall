#pragma checksum "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "90363aaa6d2724dab6e298375b395c6e3cca11b9"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Areas_Web_Views_PublicInfrastructure_AllCategory), @"mvc.1.0.view", @"/Areas/Web/Views/PublicInfrastructure/AllCategory.cshtml")]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"90363aaa6d2724dab6e298375b395c6e3cca11b9", @"/Areas/Web/Views/PublicInfrastructure/AllCategory.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"090c48694750ba7c62e752bf94ad67a159f3356f", @"/Areas/Web/Views/_ViewImports.cshtml")]
    public class Areas_Web_Views_PublicInfrastructure_AllCategory : Mall.Web.Framework.WebViewPage<List<Mall.Web.Areas.SellerAdmin.Models.CategoryJsonModel>>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#nullable restore
#line 2 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
  
    ViewBag.Title = "AllCategory";

#line default
#line hidden
#nullable disable
            WriteLiteral("<div class=\"w\">\r\n    <ul class=\"tab\" id=\"tab-link\">\r\n        <li class=\"curr\"><a href=\"./AllCategory\">全部商品分类</a></li>\r\n    </ul><!--tab end-->\r\n</div>\r\n<div class=\"w\" id=\"allsort\">\r\n    <div class=\"fl\">\r\n");
#nullable restore
#line 12 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
         for (int i = 0; i < Model.Count() / 2; i++)
        {

#line default
#line hidden
#nullable disable
            WriteLiteral("            <div class=\"m\">\r\n                <div class=\"mt\">\r\n                    <h2><a href=\"#\">");
#nullable restore
#line 16 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
                               Write(Model[i].Name);

#line default
#line hidden
#nullable disable
            WriteLiteral("</a></h2>\r\n                </div>\r\n                <div class=\"mc\">\r\n");
#nullable restore
#line 19 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
                     foreach (var second in Model[i].SubCategory)
                    {

#line default
#line hidden
#nullable disable
            WriteLiteral("                        <dl class=\"fore\">\r\n                            <dt>");
#nullable restore
#line 22 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
                           Write(second.Name);

#line default
#line hidden
#nullable disable
            WriteLiteral("</dt>\r\n                            <dd>\r\n");
#nullable restore
#line 24 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
                                 foreach (var third in second.SubCategory)
                                {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                    <em><a href=\"#\">");
#nullable restore
#line 26 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
                                               Write(third.Name);

#line default
#line hidden
#nullable disable
            WriteLiteral("</a></em>\r\n");
#nullable restore
#line 27 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
                                }

#line default
#line hidden
#nullable disable
            WriteLiteral("                            </dd>\r\n                        </dl>\r\n");
#nullable restore
#line 30 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
                    }

#line default
#line hidden
#nullable disable
            WriteLiteral("                </div>\r\n            </div>\r\n");
#nullable restore
#line 33 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
        }

#line default
#line hidden
#nullable disable
            WriteLiteral("        <!--左主体分类-->\r\n    </div>\r\n    <!--fl end-->\r\n    <div class=\"fr\">\r\n        <!--右主体分类-->\r\n");
#nullable restore
#line 39 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
         for (int i = Model.Count() / 2; i < Model.Count(); i++)
        {

#line default
#line hidden
#nullable disable
            WriteLiteral("            <div class=\"m\">\r\n                <div class=\"mt\">\r\n                    <h2><a href=\"#\">");
#nullable restore
#line 43 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
                               Write(Model[i].Name);

#line default
#line hidden
#nullable disable
            WriteLiteral("</a></h2>\r\n                </div>\r\n                <div class=\"mc\">\r\n");
#nullable restore
#line 46 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
                     foreach (var second in Model[i].SubCategory)
                    {

#line default
#line hidden
#nullable disable
            WriteLiteral("                        <dl class=\"fore\">\r\n                            <dt>");
#nullable restore
#line 49 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
                           Write(second.Name);

#line default
#line hidden
#nullable disable
            WriteLiteral("</dt>\r\n                            <dd>\r\n");
#nullable restore
#line 51 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
                                 foreach (var third in second.SubCategory)
                                {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                    <em><a href=\"#\">");
#nullable restore
#line 53 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
                                               Write(third.Name);

#line default
#line hidden
#nullable disable
            WriteLiteral("</a></em>\r\n");
#nullable restore
#line 54 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
                                }

#line default
#line hidden
#nullable disable
            WriteLiteral("                            </dd>\r\n                        </dl>\r\n");
#nullable restore
#line 57 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
                    }

#line default
#line hidden
#nullable disable
            WriteLiteral("                </div>\r\n            </div>\r\n");
#nullable restore
#line 60 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\PublicInfrastructure\AllCategory.cshtml"
        }

#line default
#line hidden
#nullable disable
            WriteLiteral("    </div><!--fr end-->\r\n    <span class=\"clr\"></span>\r\n\r\n</div>");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<List<Mall.Web.Areas.SellerAdmin.Models.CategoryJsonModel>> Html { get; private set; }
    }
}
#pragma warning restore 1591