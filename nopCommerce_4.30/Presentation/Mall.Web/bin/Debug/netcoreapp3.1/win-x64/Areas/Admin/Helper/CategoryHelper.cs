using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mall.Entities;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Microsoft.AspNetCore.Mvc
{
    public static class CategoryHelper
    {

        public static HtmlString GenerateSelectHtml(this IHtmlHelper _html, List<SelectListItem> model, string name)
        {
            StringBuilder html = new StringBuilder("<select name=\"" + name + "\" id=\"" + name + "\" class=\"form-control input-sm\">");
            foreach (var item in model)
            {
                string selected = item.Selected ? "selected" : "";
                html.Append("<option " + selected + " value=\"" + item.Value + "\">");
                html.Append(item.Text);
                html.Append("</option>");
            }
            html.Append("</select>");
            return new HtmlString(html.ToString());
        }

        public static HtmlString GenerateSelectHtml(this IHtmlHelper _html, List<SelectListItem> model, string name, string className)
        {
            StringBuilder html = new StringBuilder("<select name=\"" + name + "\" id=\"" + name + "\" class=\"form-control input-sm " + className + "\">");
            foreach (var item in model)
            {
                string selected = item.Selected ? "selected" : "";
                html.Append("<option " + selected + " value=\"" + item.Value + "\">");
                html.Append(item.Text);
                html.Append("</option>");
            }
            html.Append("</select>");
            return new HtmlString(html.ToString());
        }

        public static HtmlString GenerateTypeSelect(this IHtmlHelper _html, IEnumerable<TypeInfo> model, long selectedId = 0)
        {
            StringBuilder html = new StringBuilder("<option value=\"0\">请选择...</option>");
            foreach (var item in model)
            {
                string selected = item.Id == selectedId ? "selected" : "";
                html.Append("<option " + selected + " value=\"" + item.Id + "\">");
                html.Append(item.Name);
                html.Append("</option>");
            }
            html.Append("</select>");
            return new HtmlString(html.ToString());
        }


        public static HtmlString AlignTypeSelect(this IHtmlHelper _html, string selectedId)
        {
            var items = Mall.Core.EnumHelper.ToDescriptionDictionary<Mall.CommonModel.TopicAlign>().Select(e => new SelectListItem
            {
                Text = e.Value,
                Value = e.Key.ToString()
            }).ToList();

            StringBuilder html = new StringBuilder("<select  class=\"form-control input-sm\">");
            foreach (var item in items)
            {
                string selected = item.Value == selectedId ? "selected" : "";
                html.Append("<option " + selected + " value=\"" + item.Value + "\">");
                html.Append(item.Text);
                html.Append("</option>");
            }
            return new HtmlString(html.ToString());
        }

        public static HtmlString GenerateCategorySelect(this IHtmlHelper _html, IEnumerable<CategoryInfo> model)
        {
            StringBuilder html = new StringBuilder("<option value=\"0\">请选择...</option>");
            foreach (var item in model)
            {
                if (item.ParentCategoryId == 0)
                {
                    html.Append("<option value=\"" + item.Id + "\">");
                    html.Append(GetSpace(item.Depth) + item.Name);
                    html.Append("</option>");
                    html.Append(GetSubCategory(model, item.Id));
                }
            }
            return new HtmlString(html.ToString());
        }

        private static string GetSpace(int depth)
        {
            StringBuilder space = new StringBuilder();
            for (int i = 0; i < depth - 1; i++)
            {
                space.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
            }
            return space.ToString();
        }

        private static string GetSubCategory(IEnumerable<CategoryInfo> list, long Id)
        {
            StringBuilder html = new StringBuilder();
            for (int i = 0; i < list.Count(); i++)
            {
                if (list.ElementAt(i).ParentCategoryId == Id)
                {
                    html.Append("<option value=\"" + list.ElementAt(i).Id + "\">");
                    html.Append(GetSpace(list.ElementAt(i).Depth) + list.ElementAt(i).Name);
                    html.Append("</option>");
                }
            }
            return html.ToString();
        }
    }
}