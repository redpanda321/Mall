using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class HomeCategoryRowInfo
    {
        public long Id { get; set; }
        public int RowId { get; set; }
        public string Image1 { get; set; }
        public string Url1 { get; set; }
        public string Image2 { get; set; }
        public string Url2 { get; set; }
    }
}
