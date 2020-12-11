using Mall.CommonModel;
using System;
using System.Collections.Generic;
using System.Text;

using Mall.Entities;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Mall.Web.Areas.Admin.Helper
{
    public static class SpecificationHelper
    {
        private static Dictionary<SpecificationType, Func<TypeInfo, HtmlString>> SpecificationMap = 
            new Dictionary<SpecificationType, Func<TypeInfo, HtmlString>>();
        static SpecificationHelper()
        {
            SpecificationMap.Add(SpecificationType.Color, GenerateColor);
            SpecificationMap.Add(SpecificationType.Size, GenerateSize);
            SpecificationMap.Add(SpecificationType.Version, GenerateVersion);
        }
        private static HtmlString GenerateColor(TypeInfo model)
        {
            StringBuilder htmlTag = new StringBuilder();
            //string input = "<label class=\"label-custom\"><input class=\"specCheckbox\" type=\"checkbox\" value=\"{0}\" {1} name=\"IsSupportColor\" />颜色</label>";
            //string input = "<label class=\"label-custom\"><input class=\"specCheckbox\" type=\"checkbox\" value=\"{0}\" {1} name=\"IsSupportColor\" /><textarea class=\"input-sm\"  name=\"ColorAlias\" rows=\"1\">{2}</textarea></label>";
            string input = "<label class=\"label-custom\"><input class=\"specCheckbox\" type=\"checkbox\" value=\"{0}\" {1} name=\"IsSupportColor\" /><input type=\"text\" class=\"form-control input-sm\" name=\"ColorAlias\" value=\"{2}\"  maxlength=\"10\" {3} /></label>";
            
            string text = "<textarea class=\"form-control input-sm\" {0} name=\"ColorValue\" rows=\"2\">{1}</textarea>";
            if (model.IsSupportColor)
            {
                htmlTag.Append(string.Format(input, true, "checked", string.IsNullOrEmpty(model.ColorAlias) ? "颜色" : model.ColorAlias,""));
                htmlTag.Append(string.Format(text, "", model.ColorValue));
            }
            else
            {
                htmlTag.Append(string.Format(input, false, "", string.IsNullOrEmpty(model.ColorAlias) ? "颜色" : model.ColorAlias, "disabled"));
                htmlTag.Append(string.Format(text, "disabled", model.ColorValue));
            }
            return new HtmlString(htmlTag.ToString());
        }
        private static HtmlString GenerateSize(TypeInfo model)
        {
            StringBuilder htmlTag = new StringBuilder();
            //string input = "<label class=\"label-custom\"><input class=\"specCheckbox\" type=\"checkbox\" value=\"{0}\" {1} name=\"IsSupportSize\" />尺寸</label>";
            string input = "<label class=\"label-custom\"><input class=\"specCheckbox\" type=\"checkbox\" value=\"{0}\" {1} name=\"IsSupportSize\" /><input type=\"text\" class=\"form-control input-sm\" name=\"SizeAlias\" value=\"{2}\"  maxlength=\"10\" {3} /></label>";
            //string input = "<label class=\"label-custom\"><input class=\"specCheckbox\" type=\"checkbox\" value=\"{0}\" {1} name=\"IsSupportSize\" /><textarea class=\"input-sm\"  name=\"SizeAlias\" rows=\"1\">{2}</textarea></label>";
            string text = "<textarea class=\"form-control input-sm\" {0} name=\"SizeValue\" rows=\"2\">{1}</textarea>";
            if (model.IsSupportSize)
            {
                htmlTag.Append(string.Format(input, true, "checked", string.IsNullOrEmpty(model.SizeAlias) ? "尺寸" : model.SizeAlias, ""));
                htmlTag.Append(string.Format(text, "", model.SizeValue));
            }
            else
            {
                htmlTag.Append(string.Format(input, false, "", string.IsNullOrEmpty(model.SizeAlias) ? "尺寸" : model.SizeAlias, "disabled"));
                htmlTag.Append(string.Format(text, "disabled", model.SizeValue));
            }
            return new HtmlString(htmlTag.ToString());
        }

        private static HtmlString GenerateVersion(TypeInfo model)
        {
            StringBuilder htmlTag = new StringBuilder();
            //string input = "<label class=\"label-custom\"><input class=\"specCheckbox\" type=\"checkbox\" value=\"{0}\" {1} name=\"IsSupportVersion\" />规格</label>";
            string input = "<label class=\"label-custom\"><input class=\"specCheckbox\" type=\"checkbox\" value=\"{0}\" {1} name=\"IsSupportVersion\" /><input type=\"text\" class=\"form-control input-sm\" name=\"VersionAlias\" value=\"{2}\"  maxlength=\"10\" {3} /></label>";
            //string input = "<label class=\"label-custom\"><input class=\"specCheckbox\" type=\"checkbox\" value=\"{0}\" {1} name=\"IsSupportVersion\" /><textarea class=\"input-sm\"  name=\"VersionAlias\" rows=\"1\">{2}</textarea></label>";
            string text = "<textarea class=\"form-control input-sm\" {0} name=\"VersionValue\" rows=\"2\">{1}</textarea>";
            if (model.IsSupportVersion)
            {
                htmlTag.Append(string.Format(input, true, "checked", string.IsNullOrEmpty(model.VersionAlias) ? "规格" : model.VersionAlias, ""));
                htmlTag.Append(string.Format(text, "", model.VersionValue));
            }
            else
            {
                htmlTag.Append(string.Format(input, false, "", string.IsNullOrEmpty(model.VersionAlias) ? "规格" : model.VersionAlias, "disabled"));
                htmlTag.Append(string.Format(text, "disabled", model.VersionValue));
            }

            return new  HtmlString(htmlTag.ToString());
        }
        public static HtmlString GenerateSpecification(this IHtmlHelper _html, TypeInfo model, SpecificationType spec)
        {
            return SpecificationMap[spec](model);
        }
    }
}