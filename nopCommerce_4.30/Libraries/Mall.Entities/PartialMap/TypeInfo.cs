using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class TypeInfo
    {
        [ResultColumn]
        public string ColorValue { get; set; }

        [ResultColumn]
        public string SizeValue { get; set; }

        [ResultColumn]
        public string VersionValue { get; set; }

        public TypeInfo()
        {
            this.ColorValue = "紫色,红色,绿色,花色,蓝色,褐色,透明,酒红色,黄色,黑色,深灰色,深紫色,深蓝色";
            this.SizeValue = "160/80(XS),190/110(XXXL),165/84(S),170/88(M),175/92(L),180/96(XL),185/100(XXL),160/84(XS),165/88(S),170/92(M)";
            this.VersionValue = "版本1,版本2,版本3,版本4,版本5";
        }


        /// <summary>
        /// 关联品牌
        /// </summary>
        [ResultColumn]
        [Obsolete("关联属性移除遗留")]
        public List<TypeBrandInfo> TypeBrandInfo { get; set; }

        /// <summary>
        /// 关联规格值
        /// </summary>
        [ResultColumn]
        [Obsolete("关联属性移除遗留")]
        public List<SpecificationValueInfo> SpecificationValueInfo { get; set; }

        List<AttributeInfo> _AttributeInfo = null;
        /// <summary>
        /// 关联属性
        /// </summary>
        [ResultColumn]
        [Obsolete("关联属性移除遗留")]
        public List<AttributeInfo> AttributeInfo { get; set; }

    }
}
