#pragma checksum "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "b5c089fa18f56559754a6cd8f405d40b25ec4ed7"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Areas_Web_Views_Product_ProductCollocation), @"mvc.1.0.view", @"/Areas/Web/Views/Product/ProductCollocation.cshtml")]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"b5c089fa18f56559754a6cd8f405d40b25ec4ed7", @"/Areas/Web/Views/Product/ProductCollocation.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"090c48694750ba7c62e752bf94ad67a159f3356f", @"/Areas/Web/Views/_ViewImports.cshtml")]
    public class Areas_Web_Views_Product_ProductCollocation : Mall.Web.Framework.WebViewPage<List<Mall.DTO.ProductCollocationModel>>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#nullable restore
#line 1 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
  
    Layout = null;

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n");
#nullable restore
#line 6 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
 if (Model != null && Model.Count > 0)
{
    var titles = Model.Select(p => p.Name).ToList();

#line default
#line hidden
#nullable disable
            WriteLiteral("    <ul class=\"tab-colle\">\r\n");
#nullable restore
#line 10 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
          
    var k = 0;
    foreach (var name in titles)
    {
        if (k == 0)
        {

#line default
#line hidden
#nullable disable
            WriteLiteral("            <li class=\"active\">");
#nullable restore
#line 16 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                          Write(name);

#line default
#line hidden
#nullable disable
            WriteLiteral("</li>\r\n");
#nullable restore
#line 17 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
        }
        else
        {

#line default
#line hidden
#nullable disable
            WriteLiteral("            <li>");
#nullable restore
#line 20 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
           Write(name);

#line default
#line hidden
#nullable disable
            WriteLiteral("</li>\r\n");
#nullable restore
#line 21 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
        }
        k++;
    }
        

#line default
#line hidden
#nullable disable
            WriteLiteral("    </ul>\r\n    <div class=\"collocation-wrap\">\r\n");
#nullable restore
#line 27 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
          
    var index = 0;
    foreach (var m in Model)
    {
        var pros = m.Products;
        int i = 0;
        var main = "";
        var mainpro = pros.FirstOrDefault(p => p.IsMain);
        var current = string.Empty;
        if (index == 0)
        {
            current = "current";
        }

#line default
#line hidden
#nullable disable
            WriteLiteral("        <div");
            BeginWriteAttribute("class", " class=\"", 834, "\"", 889, 5);
            WriteAttributeValue("", 842, "products-group", 842, 14, true);
            WriteAttributeValue(" ", 856, "m", 857, 2, true);
            WriteAttributeValue(" ", 858, "m2", 859, 3, true);
            WriteAttributeValue(" ", 861, "productcollocation", 862, 19, true);
#nullable restore
#line 40 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
WriteAttributeValue(" ", 880, current, 881, 8, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            BeginWriteAttribute("mainpid", " mainpid=\"", 890, "\"", 918, 1);
#nullable restore
#line 40 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
WriteAttributeValue("", 900, mainpro.ProductId, 900, 18, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(">\r\n            <div class=\"p-group-list cl\">\r\n                <div class=\"p-group-main curr\">\r\n                    <a");
            BeginWriteAttribute("title", " title=\"", 1036, "\"", 1064, 1);
#nullable restore
#line 43 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
WriteAttributeValue("", 1044, mainpro.ProductName, 1044, 20, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" target=\"_blank\"");
            BeginWriteAttribute("href", " href=\"", 1081, "\"", 1142, 1);
#nullable restore
#line 43 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
WriteAttributeValue("", 1088, Url.Action("detail", new { id = @mainpro.ProductId }), 1088, 54, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral("><img");
            BeginWriteAttribute("src", " src=\"", 1148, "\"", 1212, 1);
#nullable restore
#line 43 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
WriteAttributeValue("", 1154, Mall.Core.MallIO.GetProductSizeImage(mainpro.Image,1,220), 1154, 58, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" /></a>\r\n                    <p class=\"name\"><a target=\"_blank\"");
            BeginWriteAttribute("href", " href=\"", 1276, "\"", 1337, 1);
#nullable restore
#line 44 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
WriteAttributeValue("", 1283, Url.Action("detail", new { id = @mainpro.ProductId }), 1283, 54, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(">");
#nullable restore
#line 44 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                                                                                                                Write(mainpro.ProductName);

#line default
#line hidden
#nullable disable
            WriteLiteral("</a></p>\r\n                    <p class=\"price\">\r\n                        <label>\r\n                            <input type=\"checkbox\" checked=\"checked\" disabled name=\"collochk\" class=\"collochk\" data-collpid=\"");
#nullable restore
#line 47 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                                                                                                                        Write(mainpro.ColloPid);

#line default
#line hidden
#nullable disable
            WriteLiteral("\"\r\n                                   data-mincollprice=\"");
#nullable restore
#line 48 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                                                 Write(mainpro.MinCollPrice);

#line default
#line hidden
#nullable disable
            WriteLiteral("\" data-maxcollprice=\"");
#nullable restore
#line 48 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                                                                                           Write(mainpro.MaxCollPrice);

#line default
#line hidden
#nullable disable
            WriteLiteral("\" data-maxsaleprice=\"");
#nullable restore
#line 48 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                                                                                                                                     Write(mainpro.MaxSalePrice);

#line default
#line hidden
#nullable disable
            WriteLiteral("\" data-minsaleprice=\"");
#nullable restore
#line 48 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                                                                                                                                                                               Write(mainpro.MinSalePrice);

#line default
#line hidden
#nullable disable
            WriteLiteral("\"");
            BeginWriteAttribute("value", " value=\"", 1788, "\"", 1814, 1);
#nullable restore
#line 48 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
WriteAttributeValue("", 1796, mainpro.ProductId, 1796, 18, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" />\r\n                            价格 ¥ ");
#nullable restore
#line 49 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                            Write(mainpro.MinCollPrice);

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n                        </label>\r\n                    </p>\r\n                </div>\r\n                <div class=\"p-group-child-box\">\r\n                    <ul class=\"p-group-child cl\">\r\n");
#nullable restore
#line 55 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                         foreach (var t in pros.Skip(1))
                        {
                            if (t.Stock == 0)
                            {
                                main = "disabled";
                            }
                            else if (i < 3 && t.Stock > 0)
                            {
                                main = "checked";
                                i++;
                            }
                            else
                            {
                                main = "";
                            }

#line default
#line hidden
#nullable disable
            WriteLiteral("                            <li>\r\n                                <a");
            BeginWriteAttribute("title", " title=\"", 2725, "\"", 2747, 1);
#nullable restore
#line 71 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
WriteAttributeValue("", 2733, t.ProductName, 2733, 14, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            BeginWriteAttribute("href", " href=\"", 2748, "\"", 2801, 1);
#nullable restore
#line 71 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
WriteAttributeValue("", 2755, Url.Action("detail", new { id = t.ProductId}), 2755, 46, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral("><img");
            BeginWriteAttribute("src", " src=\"", 2807, "\"", 2865, 1);
#nullable restore
#line 71 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
WriteAttributeValue("", 2813, Mall.Core.MallIO.GetProductSizeImage(t.Image,1,220), 2813, 52, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(" /></a>\r\n                                <p class=\"name\"><a target=\"_blank\"");
            BeginWriteAttribute("href", " href=\"", 2941, "\"", 2996, 1);
#nullable restore
#line 72 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
WriteAttributeValue("", 2948, Url.Action("detail", new { id = @t.ProductId }), 2948, 48, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(">");
#nullable restore
#line 72 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                                                                                                                      Write(t.ProductName);

#line default
#line hidden
#nullable disable
            WriteLiteral("</a></p>\r\n                                <p class=\"price\">\r\n                                    <label>\r\n                                        <input type=\"checkbox\" ");
#nullable restore
#line 75 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                                                          Write(main);

#line default
#line hidden
#nullable disable
            WriteLiteral(" name=\"collochk\" class=\"collochk\" data-collpid=\"");
#nullable restore
#line 75 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                                                                                                               Write(t.ColloPid);

#line default
#line hidden
#nullable disable
            WriteLiteral("\" data-mincollprice=\"");
#nullable restore
#line 75 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                                                                                                                                               Write(t.MinCollPrice);

#line default
#line hidden
#nullable disable
            WriteLiteral("\" data-maxcollprice=\"");
#nullable restore
#line 75 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                                                                                                                                                                                   Write(t.MaxCollPrice);

#line default
#line hidden
#nullable disable
            WriteLiteral("\"\r\n                                               data-maxsaleprice=\"");
#nullable restore
#line 76 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                                                             Write(t.MaxSalePrice);

#line default
#line hidden
#nullable disable
            WriteLiteral("\" data-minsaleprice=\"");
#nullable restore
#line 76 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                                                                                                 Write(t.MinSalePrice);

#line default
#line hidden
#nullable disable
            WriteLiteral("\" value=\"");
#nullable restore
#line 76 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                                                                                                                         Write(t.ProductId);

#line default
#line hidden
#nullable disable
            WriteLiteral("\" />价格 ¥ ");
#nullable restore
#line 76 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                                                                                                                                              Write(t.MinCollPrice);

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n                                        </label>\r\n                                    </p>\r\n\r\n                                </li>\r\n");
#nullable restore
#line 81 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                        }

#line default
#line hidden
#nullable disable
            WriteLiteral(@"                    </ul>
                </div>

                <div class=""group-arrow"">
                    <a class=""group-arrow-pre""></a>
                    <a class=""group-arrow-next""></a>
                </div>

            </div>
            <div class=""p-group-btn"">
                <h3>组合价<span id=""collTotalPrice""></span></h3>
                <p class=""oldp"">原价：<s id=""saleTotalPrice""></s></p>
                <p><i>省</i><span class=""dis"" id=""groupPriceMinus""></span> </p>
                <a");
            BeginWriteAttribute("onclick", " onclick=\"", 4161, "\"", 4193, 3);
            WriteAttributeValue("", 4171, "CollocationBuy(", 4171, 15, true);
#nullable restore
#line 95 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
WriteAttributeValue("", 4186, index, 4186, 6, false);

#line default
#line hidden
#nullable disable
            WriteAttributeValue("", 4192, ")", 4192, 1, true);
            EndWriteAttribute();
            WriteLiteral(">立即购买</a>\r\n            </div>\r\n        </div>\r\n");
#nullable restore
#line 98 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
                index++;
    }
        

#line default
#line hidden
#nullable disable
            WriteLiteral("    </div>\r\n");
#nullable restore
#line 102 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Web\Views\Product\ProductCollocation.cshtml"
}

#line default
#line hidden
#nullable disable
            WriteLiteral(@"

<script type=""text/javascript"">
    $(""ul.tab-colle li"").on(""click"", function () {
        var index = $(this).index();
        $(this).addClass(""active"").siblings().removeClass(""active"");
        $("".collocation-wrap .products-group"").eq(index).addClass(""current"").siblings().removeClass(""current"");
        loadGroup();
    });
</script>
");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<List<Mall.DTO.ProductCollocationModel>> Html { get; private set; }
    }
}
#pragma warning restore 1591