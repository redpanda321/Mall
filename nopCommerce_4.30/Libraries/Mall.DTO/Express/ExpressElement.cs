using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class ExpressElement
    {
        /// <summary>
        /// 快递公司ID
        /// </summary>
        public long ExpressId { get; set; }
        /// <summary>
        /// 元素类型
        /// </summary>
        public Mall.CommonModel.ExpressElementType ElementType { get; set; }
        /// <summary>
        /// A点（左上角顶点）坐标
        /// </summary>
        public int[] a { get; set; }

        /// <summary>
        /// B点（右下解顶点）坐标
        /// </summary>
        public int[] b { get; set; }

        /// <summary>
        /// 元素名称(即元素类型，冗余字段)
        /// </summary>
        public int name { get; set; }
    }
}
