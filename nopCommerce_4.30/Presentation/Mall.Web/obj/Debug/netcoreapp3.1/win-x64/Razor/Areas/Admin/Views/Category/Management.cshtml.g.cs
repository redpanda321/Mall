#pragma checksum "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "ea255f0512a79299651997c7a32ed0d5982a92a3"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Areas_Admin_Views_Category_Management), @"mvc.1.0.view", @"/Areas/Admin/Views/Category/Management.cshtml")]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"ea255f0512a79299651997c7a32ed0d5982a92a3", @"/Areas/Admin/Views/Category/Management.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"0fdcb39a21de2ae84fe919284139b99d5fdc20b3", @"/Areas/Admin/Views/_ViewImports.cshtml")]
    public class Areas_Admin_Views_Category_Management : Mall.Web.Framework.WebViewPage<List<Mall.Web.Areas.Admin.Models.Product.CategoryModel>>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("src", new global::Microsoft.AspNetCore.Html.HtmlString("~/Scripts/CommonJS.js"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("src", new global::Microsoft.AspNetCore.Html.HtmlString("~/Areas/Admin/Scripts/categoryJS.js"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_2 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("href", new global::Microsoft.AspNetCore.Html.HtmlString("~/Content/jquery.onoff.css.css"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_3 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("rel", new global::Microsoft.AspNetCore.Html.HtmlString("stylesheet"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_4 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("src", new global::Microsoft.AspNetCore.Html.HtmlString("~/Scripts/jquery.onoff.min.js"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
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
#line 3 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
  
    ViewBag.Title = "分类管理";

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n\r\n\r\n");
            WriteLiteral("\r\n\r\n");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("script", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "ea255f0512a79299651997c7a32ed0d5982a92a37968", async() => {
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
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("script", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "ea255f0512a79299651997c7a32ed0d5982a92a39007", async() => {
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_1);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral(@"

<div class=""container clear-m"">
    <ul class=""nav nav-tabs-custom clearfix"">
        <li class=""active""><a>管理</a></li>
        <li><a href=""./Add"">新增</a></li>
    </ul>
    <div class=""queryDiv"">
        <img class=""queryImg"" src=""/Images/ic_query.png"">
        <div class=""tipBox"">
            <h5>温馨提示:</h5>
            <ul>
                <li><span>可以在分类后设置排序值，数值越小排序越靠前。</span></li>
                <li><span>可以在三级分类下设置佣金比例，当商家成功卖出该分类下的商品时，平台可以抽取对应比例佣金。</span></li>
                <li><span>“√”则表示在前台会显示出来，“×”表示前台不显示出来，直接点击图标就可以切换两者的状态。</span></li>
                <li class=""mark-info""><span>注意最多有三级分类，并且分类下有商品时无法删除该分类。</span></li>
            </ul>
        </div>
    </div>
    <div class=""topbtn"">
        <a class=""add-business"" id=""btnlevel1""><span class=""glyphicon glyphicon-plus-sign""></span>折叠</a>
        <a class=""add-business"" id=""btnlevelAll""><span class=""glyphicon glyphicon-minus-sign""></span>展开</a>
    </div>

    <table class=""table category_table"">
        <thead>
     ");
            WriteLiteral(@"       <tr>

                <th style=""text-align:center"" width=""450"">分类名称</th>
                <th style=""text-align:center"" width=""150"">排序</th>
                <th style=""text-align:center"" width=""200"">佣金比率</th>
                <th style=""text-align:center"" width=""200"">是否显示</th>
                <th style=""text-align:center"" width=""150"">支持虚拟商品</th>
                <th class=""td-operate clear-m1"">操作</th>
            </tr>
        </thead>
        <tbody>
");
#nullable restore
#line 116 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
             if (null == Model)
            {


#line default
#line hidden
#nullable disable
            WriteLiteral("                <tr style=\"text-align:center\">\r\n                    <td style=\"text-align:center;\" colspan=\"3\"><h2 class=\"none-data\">没有任何分类</h2></td>\r\n                </tr>\r\n");
#nullable restore
#line 122 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
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



                foreach (var category in Model)
                {
                    CreateTr(category, hideItems);
                }
            }

#line default
#line hidden
#nullable disable
            WriteLiteral("        </tbody>\r\n    </table>\r\n</div>\r\n");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("link", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.SelfClosing, "ea255f0512a79299651997c7a32ed0d5982a92a312971", async() => {
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_2);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_3);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("script", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "ea255f0512a79299651997c7a32ed0d5982a92a314086", async() => {
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_4);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
        }
        #pragma warning restore 1998
#nullable restore
#line 9 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
            
   public  void CreateTr(Mall.Web.Areas.Admin.Models.Product.CategoryModel category, long[] hideItems)
    {
        var hide = hideItems.Contains(category.Id);
        var allChildHide = Model.Where(p => p.ParentCategoryId == category.Id).All(p => hideItems.Contains(p.Id));


#line default
#line hidden
#nullable disable
        WriteLiteral("        <tr");
        BeginWriteAttribute("class", " class=\"", 415, "\"", 444, 2);
        WriteAttributeValue("", 423, "level-", 423, 6, true);
#nullable restore
#line 15 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 429, category.Depth, 429, 15, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        BeginWriteAttribute("cid", " cid=\"", 445, "\"", 463, 1);
#nullable restore
#line 15 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 451, category.Id, 451, 12, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        BeginWriteAttribute("parentid", " parentid=\"", 464, "\"", 501, 1);
#nullable restore
#line 15 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 475, category.ParentCategoryId, 475, 26, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        BeginWriteAttribute("style", " style=\"", 502, "\"", 539, 1);
#nullable restore
#line 15 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 510, hide ? "display:none" : "", 510, 29, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(">\r\n            <td");
        BeginWriteAttribute("class", " class=\"", 558, "\"", 613, 1);
#nullable restore
#line 16 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 566, category.Depth == 3 ? "clear-m2" : "clear-e", 566, 47, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(">\r\n");
#nullable restore
#line 17 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
                 if (category.Depth == 2)
                {

#line default
#line hidden
#nullable disable
        WriteLiteral("                    <s class=\"line clear-m4\">└───</s>\r\n");
#nullable restore
#line 20 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
                }

#line default
#line hidden
#nullable disable
#nullable restore
#line 21 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
                 if (category.Depth == 3)
                {

#line default
#line hidden
#nullable disable
        WriteLiteral("                    <s class=\"line clear-m3\">├─── </s>\r\n");
#nullable restore
#line 24 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
                }

#line default
#line hidden
#nullable disable
#nullable restore
#line 25 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
                 if (category.Depth < 3)
                {

#line default
#line hidden
#nullable disable
        WriteLiteral("                    <span");
        BeginWriteAttribute("class", " class=\"", 976, "\"", 1043, 4);
        WriteAttributeValue("", 984, "glyphicon", 984, 9, true);
        WriteAttributeValue(" ", 993, "glyphicon-", 994, 11, true);
#nullable restore
#line 27 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 1004, allChildHide ? "plus" : "minus", 1004, 34, false);

#line default
#line hidden
#nullable disable
        WriteAttributeValue("", 1038, "-sign", 1038, 5, true);
        EndWriteAttribute();
        WriteLiteral("></span>\r\n");
#nullable restore
#line 28 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
                }

#line default
#line hidden
#nullable disable
        WriteLiteral("                <input type=\"hidden\" class=\"hidden_id\"");
        BeginWriteAttribute("value", " value=\"", 1127, "\"", 1147, 1);
#nullable restore
#line 29 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 1135, category.Id, 1135, 12, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(" />\r\n                <input type=\"hidden\" class=\"hidden_depth\"");
        BeginWriteAttribute("value", " value=\"", 1210, "\"", 1233, 1);
#nullable restore
#line 30 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 1218, category.Depth, 1218, 15, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(" />\r\n                <input class=\"text-name input-no-sp\" type=\"text\"");
        BeginWriteAttribute("value", " value=\"", 1303, "\"", 1325, 1);
#nullable restore
#line 31 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 1311, category.Name, 1311, 14, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        BeginWriteAttribute("categoryId", " categoryId=\"", 1326, "\"", 1351, 1);
#nullable restore
#line 31 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 1339, category.Id, 1339, 12, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(" />\r\n            </td>\r\n            <td class=\"clear-m2\">\r\n                <input class=\"text-order\" type=\"text\"");
        BeginWriteAttribute("value", " value=\"", 1464, "\"", 1497, 1);
#nullable restore
#line 34 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 1472, category.DisplaySequence, 1472, 25, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        BeginWriteAttribute("categoryId", " categoryId=\"", 1498, "\"", 1523, 1);
#nullable restore
#line 34 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 1511, category.Id, 1511, 12, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(" />\r\n            </td>\r\n");
#nullable restore
#line 36 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
             if (category.Depth == 3)
            {

#line default
#line hidden
#nullable disable
        WriteLiteral("                <td class=\"tac\"><input class=\"text-commis tac text-order\" type=\"text\"");
        BeginWriteAttribute("value", " value=\"", 1687, "\"", 1715, 1);
#nullable restore
#line 38 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 1695, category.CommisRate, 1695, 20, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral("><span>%</span></td>\r\n");
#nullable restore
#line 39 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
            }
            else
            {

#line default
#line hidden
#nullable disable
        WriteLiteral("                <td style=\"text-align:center\"></td>\r\n");
#nullable restore
#line 43 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
            }

#line default
#line hidden
#nullable disable
        WriteLiteral("            <td style=\"text-align:center\">\r\n                <span class=\"btn-a\">\r\n");
#nullable restore
#line 46 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
                     if (category.IsShow)
                    {

#line default
#line hidden
#nullable disable
        WriteLiteral("<a");
        BeginWriteAttribute("categoryId", " categoryId=\"", 2002, "\"", 2027, 1);
#nullable restore
#line 47 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 2015, category.Id, 2015, 12, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(" IsShow=\"1\" class=\"j_isshow\">√</a>");
#nullable restore
#line 47 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
                                                                                   }
                    else
                    {

#line default
#line hidden
#nullable disable
        WriteLiteral("<a");
        BeginWriteAttribute("categoryId", " categoryId=\"", 2114, "\"", 2139, 1);
#nullable restore
#line 49 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 2127, category.Id, 2127, 12, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(" IsShow=\"0\" class=\"j_isshow\">×</a>");
#nullable restore
#line 49 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
                                                                                   }

#line default
#line hidden
#nullable disable
        WriteLiteral("                </span>\r\n            </td>\r\n");
#nullable restore
#line 52 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
             if (category.Depth == 3)
            {

#line default
#line hidden
#nullable disable
        WriteLiteral("                <td>\r\n                    <input type=\"checkbox\"");
        BeginWriteAttribute("categoryId", " categoryId=\"", 2339, "\"", 2364, 1);
#nullable restore
#line 55 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 2352, category.Id, 2352, 12, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(" class=\"VirtualProductCheck\" ");
#nullable restore
#line 55 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
                                                                                             Write(category.SupportVirtualProduct ? "checked" : "");

#line default
#line hidden
#nullable disable
        WriteLiteral(">\r\n                </td>\r\n");
#nullable restore
#line 57 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
            }
            else
            {

#line default
#line hidden
#nullable disable
        WriteLiteral("                <td style=\"text-align:center\"></td>\r\n");
#nullable restore
#line 61 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
            }

#line default
#line hidden
#nullable disable
        WriteLiteral("            <td class=\"td-operate\">\r\n                <span class=\"btn-a\">\r\n");
#nullable restore
#line 64 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
                     if (category.Depth < 3)
                    {

#line default
#line hidden
#nullable disable
        WriteLiteral("                        <a");
        BeginWriteAttribute("id", " id=\"", 2756, "\"", 2777, 2);
        WriteAttributeValue("", 2761, "add_", 2761, 4, true);
#nullable restore
#line 66 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 2765, category.Id, 2765, 12, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        BeginWriteAttribute("href", " href=\"", 2778, "\"", 2814, 2);
        WriteAttributeValue("", 2785, "./AddByParent?Id=", 2785, 17, true);
#nullable restore
#line 66 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 2802, category.Id, 2802, 12, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(" class=\"add\"");
        BeginWriteAttribute("target", " target=\"", 2827, "\"", 2836, 0);
        EndWriteAttribute();
        WriteLiteral(">新增下级</a>\r\n");
#nullable restore
#line 67 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
                    }

#line default
#line hidden
#nullable disable
        WriteLiteral("                    <a");
        BeginWriteAttribute("id", " id=\"", 2893, "\"", 2915, 2);
        WriteAttributeValue("", 2898, "edit_", 2898, 5, true);
#nullable restore
#line 68 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 2903, category.Id, 2903, 12, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        BeginWriteAttribute("href", " href=\"", 2916, "\"", 2945, 2);
        WriteAttributeValue("", 2923, "./Edit?Id=", 2923, 10, true);
#nullable restore
#line 68 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 2933, category.Id, 2933, 12, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(" class=\"edit\">编辑</a>\r\n                    <a");
        BeginWriteAttribute("id", " id=\"", 2990, "\"", 3011, 2);
        WriteAttributeValue("", 2995, "del_", 2995, 4, true);
#nullable restore
#line 69 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 2999, category.Id, 2999, 12, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(" class=\"delete-classify\"");
        BeginWriteAttribute("categoryId", " categoryId=\"", 3036, "\"", 3061, 1);
#nullable restore
#line 69 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
WriteAttributeValue("", 3049, category.Id, 3049, 12, false);

#line default
#line hidden
#nullable disable
        EndWriteAttribute();
        WriteLiteral(">删除</a>\r\n                </span>\r\n            </td>\r\n        </tr>\r\n");
#nullable restore
#line 73 "C:\S\nopCommerce_4.30_dev\Presentation\Mall.Web\Areas\Admin\Views\Category\Management.cshtml"
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