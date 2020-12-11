using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    public class ProductCommentCountAggregateModel
    {
        public int AllComment { get; set; }
        public int LowComment { get; set; }
        public int MediumComment { get; set; }
        public int HighComment { get; set; }
        public int HasImageComment { get; set; }
        public int AppendComment { get; set; }
    }
}
