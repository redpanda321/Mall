#pragma checksum "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "e554993ecc10221e05536aab625994853a84a926"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Areas_Admin_Views_PageSettings_CategorySelect), @"mvc.1.0.view", @"/Areas/Admin/Views/PageSettings/CategorySelect.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
#nullable restore
#line 12 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Mvc.ViewFeatures;

#line default
#line hidden
#nullable disable
#nullable restore
#line 13 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Html;

#line default
#line hidden
#nullable disable
#nullable restore
#line 14 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Mvc.Rendering;

#line default
#line hidden
#nullable disable
#nullable restore
#line 15 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Http.Extensions;

#line default
#line hidden
#nullable disable
#nullable restore
#line 16 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Mvc;

#line default
#line hidden
#nullable disable
#nullable restore
#line 17 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Mvc.Razor;

#line default
#line hidden
#nullable disable
#nullable restore
#line 21 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\_ViewImports.cshtml"
using System.Net;

#line default
#line hidden
#nullable disable
#nullable restore
#line 22 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\_ViewImports.cshtml"
using System.IO;

#line default
#line hidden
#nullable disable
#nullable restore
#line 23 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\_ViewImports.cshtml"
using System.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 25 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\_ViewImports.cshtml"
using Mall.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 26 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\_ViewImports.cshtml"
using Mall.Web.Framework;

#line default
#line hidden
#nullable disable
#nullable restore
#line 27 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\_ViewImports.cshtml"
using Mall.Web.Framework.Infrastructure;

#line default
#line hidden
#nullable disable
#nullable restore
#line 28 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\_ViewImports.cshtml"
using Mall.DTO;

#line default
#line hidden
#nullable disable
#nullable restore
#line 29 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\_ViewImports.cshtml"
using Mall.Service;

#line default
#line hidden
#nullable disable
#nullable restore
#line 30 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\_ViewImports.cshtml"
using Mall.Core;

#line default
#line hidden
#nullable disable
#nullable restore
#line 32 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\_ViewImports.cshtml"
using Mall.CommonModel;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"e554993ecc10221e05536aab625994853a84a926", @"/Areas/Admin/Views/PageSettings/CategorySelect.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"0fdcb39a21de2ae84fe919284139b99d5fdc20b3", @"/Areas/Admin/Views/_ViewImports.cshtml")]
    public class Areas_Admin_Views_PageSettings_CategorySelect : Mall.Web.Framework.WebViewPage<List<Mall.Web.Areas.Admin.Models.Product.CategoryModel>>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("src", new global::Microsoft.AspNetCore.Html.HtmlString("~/Areas/Admin/Scripts/categoryJS.js"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("href", new global::Microsoft.AspNetCore.Html.HtmlString("~/Content/pagesetting.css"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_2 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("rel", new global::Microsoft.AspNetCore.Html.HtmlString("stylesheet"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        #line hidden
        #pragma warning disable 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        #pragma warning restore 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
        {
            get
            {
                if (__backed__tagHelperScopeManager == null)
                {
                    __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
                }
                return __backed__tagHelperScopeManager;
            }
        }
        private global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\r\n");
#nullable restore
#line 3 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
  
    ViewBag.Title = "分类管理";

#line default
#line hidden
#nullable disable
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("script", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "e554993ecc10221e05536aab625994853a84a9267203", async() => {
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_0);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("link", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.SelfClosing, "e554993ecc10221e05536aab625994853a84a9268242", async() => {
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_1);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_2);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n\r\n<div class=\"container\">\r\n    <table class=\"table category_table\">\r\n        <tbody>\r\n");
#nullable restore
#line 12 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
             if (null == Model)
            {


#line default
#line hidden
#nullable disable
            WriteLiteral("                <tr style=\"text-align:center\">\r\n                    <td style=\"text-align:center;\" colspan=\"3\"><h2 class=\"none-data\">没有任何分类</h2></td>\r\n                </tr>");
#nullable restore
#line 17 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
                     }
            else
            {
                var cookie = Context.Request.Cookies["CategoryHideItems"];
                var cookieValue = "";
                if (cookie != null && !string.IsNullOrEmpty(cookie))
                {
                    cookieValue = Uri.UnescapeDataString(cookie);
                }

                long temp;
                var hideItems = cookieValue.Split(',').Where(p => long.TryParse(p, out temp)).Select(p => long.Parse(p)).ToArray();


                

#line default
#line hidden
#nullable disable
#nullable restore
#line 66 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
             foreach (var category in Model)
            {
                CreateTr(category, hideItems);
            }

#line default
#line hidden
#nullable disable
#nullable restore
#line 69 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
             
            }

#line default
#line hidden
#nullable disable
            WriteLiteral(@"        </tbody>
    </table>
</div>
<script type=""text/javascript"">
    var id = 0;
    $('.category_table').on('click', '.select-classify', function () {
        id = $(this).attr(""categoryId"");
        $("".select-classify"").html(""选取"");
        $(this).html(""已选取"");
        //选取的某个商品分类
        if (window.top) {
            window.top._categorId = id;
        }
    });

</script>");
        }
        #pragma warning restore 1998
#nullable restore
#line 31 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
                             
                public  void CreateTr(Mall.Web.Areas.Admin.Models.Product.CategoryModel category, long[] hideItems)
                {
                var hide = hideItems.Contains(category.Id);
                var allChildHide = Model.Where(p => p.ParentCategoryId == category.Id).All(p => hideItems.Contains(p.Id));


#line default
#line hidden
#nullable disable
        WriteLiteral("    <tr");
        BeginWriteAttribute("class", " class=\"", 1385, "\"", 1414, 2);
        WriteAttributeValue("", 1393, "level-", 1393, 6, true);
#nullable restore
#line 37 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
WriteAttributeValue("", 1399, category.Depth, 1399, 15, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        BeginWriteAttribute("cid", " cid=\"", 1415, "\"", 1433, 1);
#nullable restore
#line 37 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
WriteAttributeValue("", 1421, category.Id, 1421, 12, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        BeginWriteAttribute("parentid", " parentid=\"", 1434, "\"", 1471, 1);
#nullable restore
#line 37 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
WriteAttributeValue("", 1445, category.ParentCategoryId, 1445, 26, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        BeginWriteAttribute("style", " style=\"", 1472, "\"", 1535, 1);
#nullable restore
#line 37 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
WriteAttributeValue("", 1480, category.ParentCategoryId == 0 ? "" : "display:none", 1480, 55, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(">\r\n        <td");
        BeginWriteAttribute("class", " class=\"", 1550, "\"", 1605, 1);
#nullable restore
#line 38 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
WriteAttributeValue("", 1558, category.Depth == 3 ? "clear-m2" : "clear-e", 1558, 47, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(">\r\n");
#nullable restore
#line 39 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
             if (category.Depth == 2)
            {

#line default
#line hidden
#nullable disable
        WriteLiteral("                <s class=\"line clear-m4\">└───</s>\r\n");
#nullable restore
#line 42 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
            }

#line default
#line hidden
#nullable disable
#nullable restore
#line 43 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
             if (category.Depth == 3)
            {

#line default
#line hidden
#nullable disable
        WriteLiteral("                <s class=\"line clear-m3\">├─── </s>\r\n");
#nullable restore
#line 46 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
            }

#line default
#line hidden
#nullable disable
#nullable restore
#line 47 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
             if (category.Depth < 3)
            {

#line default
#line hidden
#nullable disable
        WriteLiteral("                <span class=\"glyphicon glyphicon-plus-sign\"></span>\r\n");
#nullable restore
#line 50 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
            }

#line default
#line hidden
#nullable disable
        WriteLiteral("            <input type=\"hidden\" class=\"hidden_id\"");
        BeginWriteAttribute("value", " value=\"", 2037, "\"", 2057, 1);
#nullable restore
#line 51 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
WriteAttributeValue("", 2045, category.Id, 2045, 12, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(" />\r\n            <input type=\"hidden\" class=\"hidden_depth\"");
        BeginWriteAttribute("value", " value=\"", 2116, "\"", 2139, 1);
#nullable restore
#line 52 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
WriteAttributeValue("", 2124, category.Depth, 2124, 15, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(" />\r\n            <label class=\"text-name\">");
#nullable restore
#line 53 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
                                Write(category.Name);

#line default
#line hidden
#nullable disable
        WriteLiteral("</label>\r\n        </td>\r\n        <td class=\"td-operate\">\r\n            <span class=\"btn-a\">\r\n                <a");
        BeginWriteAttribute("id", " id=\"", 2306, "\"", 2327, 2);
        WriteAttributeValue("", 2311, "del_", 2311, 4, true);
#nullable restore
#line 57 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
WriteAttributeValue("", 2315, category.Id, 2315, 12, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(" data-categoryid=\"");
#nullable restore
#line 57 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
                                                     Write(category.Id);

#line default
#line hidden
#nullable disable
        WriteLiteral("\"");
        BeginWriteAttribute("categoryId", " categoryId=\"", 2359, "\"", 2384, 1);
#nullable restore
#line 57 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
WriteAttributeValue("", 2372, category.Id, 2372, 12, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(" class=\"select-classify\">选取</a>\r\n            </span>\r\n        </td>\r\n    </tr>\r\n");
#nullable restore
#line 61 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\PageSettings\CategorySelect.cshtml"
                }

            

#line default
#line hidden
#nullable disable
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<List<Mall.Web.Areas.Admin.Models.Product.CategoryModel>> Html { get; private set; }
    }
}
#pragma warning restore 1591
