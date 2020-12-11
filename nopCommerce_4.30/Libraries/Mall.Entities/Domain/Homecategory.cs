using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class HomeCategoryInfo
    {
        public long Id { get; set; }
        public int RowId { get; set; }
        public long CategoryId { get; set; }
        public int Depth { get; set; }
    }
}
