#pragma checksum "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "090fc57d2abfff5723823bc43fc319ece698a0de"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Areas_Mobile_Templates_Default_Views_FightGroup_ShowActionHead), @"mvc.1.0.view", @"/Areas/Mobile/Templates/Default/Views/FightGroup/ShowActionHead.cshtml")]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"090fc57d2abfff5723823bc43fc319ece698a0de", @"/Areas/Mobile/Templates/Default/Views/FightGroup/ShowActionHead.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"251375f151a8ed74fad18b07a59f5490bcea9ba2", @"/Areas/Mobile/Templates/Default/Views/_ViewImports.cshtml")]
    public class Areas_Mobile_Templates_Default_Views_FightGroup_ShowActionHead : Mall.Web.Framework.MobileWebViewPage<Mall.Web.Areas.Mobile.Models.FightGroupShowDetailModel>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#nullable restore
#line 2 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
  
    Layout = null;
    var actdata = Model.ActiveData;

#line default
#line hidden
#nullable disable
#nullable restore
#line 6 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
Write(Html.Hidden("gid", actdata.ProductId));

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n");
#nullable restore
#line 7 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
Write(Html.Hidden("aid", actdata.Id));

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n");
#nullable restore
#line 8 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
Write(Html.Hidden("sid", actdata.ShopId));

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n");
#nullable restore
#line 9 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
Write(Html.Hidden("has", actdata.ActiveItems.Count() == 0 ? 0 : 1));

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n");
#nullable restore
#line 10 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
Write(Html.Hidden("maxSaleCount", actdata.LimitQuantity));

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n<div class=\"detail-hd\">\r\n    <div id=\"slides\">\r\n");
#nullable restore
#line 13 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
         if (!string.IsNullOrWhiteSpace(actdata.VideoPath) && actdata.VideoPath.Length > 0)
        {

#line default
#line hidden
#nullable disable
            WriteLiteral("            <div class=\"video-box\">\r\n                <div class=\"j_preview\">\r\n                    <video id=\"video1\" width=\"100%\" height=\"100%\"");
            BeginWriteAttribute("poster", " poster=\"", 648, "\"", 779, 1);
#nullable restore
#line 17 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
WriteAttributeValue("", 657, Mall.Core.MallIO.GetProductSizeImage(actdata.ProductImages.FirstOrDefault(), 1, (int)Mall.CommonModel.ImageSize.Size_500), 657, 122, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(">\r\n                        <source");
            BeginWriteAttribute("src", " src=\"", 814, "\"", 838, 1);
#nullable restore
#line 18 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
WriteAttributeValue("", 820, actdata.VideoPath, 820, 18, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" type=\"video/mp4\">\r\n                    </video>\r\n                    <div class=\"fd_gif\"><i class=\"j_startPic\"></i></div>\r\n                </div>\r\n            </div>\r\n");
#nullable restore
#line 23 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
        }

#line default
#line hidden
#nullable disable
            WriteLiteral("        <img data-src=\"");
#nullable restore
#line 24 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
                   Write(actdata.ProductDefaultImage);

#line default
#line hidden
#nullable disable
            WriteLiteral("\">\r\n");
#nullable restore
#line 25 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
         foreach (var item in actdata.ProductImages)
            {

#line default
#line hidden
#nullable disable
            WriteLiteral("            <img data-src=\"");
#nullable restore
#line 27 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
                       Write(item);

#line default
#line hidden
#nullable disable
            WriteLiteral("\">\r\n");
#nullable restore
#line 28 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
        }

#line default
#line hidden
#nullable disable
            WriteLiteral("    </div>\r\n");
#nullable restore
#line 30 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
     if (actdata.StartTime.Date <= DateTime.Now.Date)
    {

#line default
#line hidden
#nullable disable
            WriteLiteral("        <div");
            BeginWriteAttribute("class", " class=\"", 1279, "\"", 1353, 2);
            WriteAttributeValue("", 1287, "merge-size", 1287, 10, true);
#nullable restore
#line 32 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
WriteAttributeValue("", 1297, actdata.EndTime.Date<DateTime.Now.Date?"-disabled":"", 1297, 56, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral("><span>");
#nullable restore
#line 32 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
                                                                                          Write(actdata.LimitedNumber);

#line default
#line hidden
#nullable disable
            WriteLiteral("</span>人团</div>\r\n");
#nullable restore
#line 33 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Mobile\Templates\Default\Views\FightGroup\ShowActionHead.cshtml"
    }

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n</div>");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<Mall.Web.Areas.Mobile.Models.FightGroupShowDetailModel> Html { get; private set; }
    }
}
#pragma warning restore 1591