
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class AttributeInfo
    {

        public string AttrValue { get; set; }
   
        
        [Obsolete("关联属性移除遗留")]
        public List<AttributeValueInfo> AttributeValueInfo { get; set; }

    }
}
