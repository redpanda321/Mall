using Mall.CommonModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Mall.Web.Areas.Admin.Models.Product
{
    public class TypeModel
    {
        public long Id { get; set; }
        [Required(ErrorMessage = "类型名称必填")]
        public string Name { get; set; }

        public bool IsSupportColor { get; set; }
        public bool IsSupportSize { get; set; }
        public bool IsSupportVersion { get; set; }

        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { get; set; }
        public string ColorValue { get; set; }
        public string SizeValue { get; set; }
        public string VersionValue { get; set; }

        
        public List<TypeAttribute> Attributes { get; set; }
        public List<TypeBrandModel> Brands { get; set; }

        public static implicit operator Entities.TypeInfo(TypeModel m)
        {
            Entities.TypeInfo info = new Entities.TypeInfo()
            {
                Id = m.Id,
                Name = m.Name,
                ColorValue = m.ColorValue,
                SizeValue = m.SizeValue,
                VersionValue = m.VersionValue,
                ColorAlias = m.ColorAlias,
                SizeAlias = m.SizeAlias,
                VersionAlias = m.VersionAlias,
                SpecificationValueInfo = new List<Entities.SpecificationValueInfo>(),
                AttributeInfo = new List<Entities.AttributeInfo>(),
                TypeBrandInfo = new List<Entities.TypeBrandInfo>()
            };

            #region 规格
            if (m.IsSupportColor && (!string.IsNullOrWhiteSpace(m.ColorValue)))
            {
                info.IsSupportColor = m.IsSupportColor;
                m.ColorValue = m.ColorValue.Replace("，", ",");
                var colorArray = m.ColorValue.Split(',');
                foreach (var item in colorArray)
                {
                    if (string.IsNullOrWhiteSpace(item)) continue;
                    info.SpecificationValueInfo.Add(new Entities.SpecificationValueInfo
                    {
                        Specification = SpecificationType.Color,
                        Value = item,
                        TypeId = m.Id
                    });
                }

            }
            if (m.IsSupportSize && (!string.IsNullOrWhiteSpace(m.SizeValue)))
            {
                info.IsSupportSize = m.IsSupportSize;
                m.SizeValue = m.SizeValue.Replace("，",",");
                var sizeArray = m.SizeValue.Split(',');
                foreach (var item in sizeArray)
                {
                    if (string.IsNullOrWhiteSpace(item)) continue;
                    info.SpecificationValueInfo.Add(new Entities.SpecificationValueInfo
                    {
                        Specification = SpecificationType.Size,
                        Value = item,
                        TypeId = m.Id
                    });
                }
            }
            if (m.IsSupportVersion && (!string.IsNullOrWhiteSpace(m.VersionValue)))
            {
                info.IsSupportVersion = m.IsSupportVersion;
                m.VersionValue = m.VersionValue.Replace("，", ",");
                var versionArray = m.VersionValue.Split(',');
                foreach (var item in versionArray)
                {
                    if (string.IsNullOrWhiteSpace(item)) continue;
                    info.SpecificationValueInfo.Add(new Entities.SpecificationValueInfo
                    {
                        Specification = SpecificationType.Version,
                        Value = item,
                        TypeId = m.Id
                    });
                }
            }
            #endregion

            #region 品牌

            if (null != m.Brands && m.Brands.Count() > 0)
            {
                foreach (var item in m.Brands)
                {
                    info.TypeBrandInfo.Add(new Entities.TypeBrandInfo()
                    {
                        BrandId = item.Id,
                        TypeId = m.Id
                    });
                }
            }

            #endregion

            #region 属性
            if (null != m.Attributes && m.Attributes.Count() > 0)
            {
                foreach (var item in m.Attributes)
                {
                    item.Value = string.IsNullOrWhiteSpace(item.Value) ? "" : item.Value.Replace("，", ",");
                    var attr = new Entities.AttributeInfo
                    {
                        Id = item.Id,
                        Name = item.Name,
                        TypeId = m.Id,
                        IsMulti = item.IsMulti,
                        AttributeValueInfo = item.Value.Split(',').Select(p => new Entities.AttributeValueInfo { Value = p, DisplaySequence = 1 }).ToList(),
                    };
                    info.AttributeInfo.Add(attr);
                }
            }
            #endregion

            return info;
        }

    }

    public class TypeBrandModel
    {
        public long Id { get; set; }
    }
    public class TypeAttribute
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsMulti { get; set; }
        public long TypeId { get; set; }
    }
}