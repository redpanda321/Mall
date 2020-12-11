using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MenuInfo
    {
        public long Id { get; set; }
        public long ParentId { get; set; }
        public long ShopId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public short Depth { get; set; }
        public short Sequence { get; set; }
        public string FullIdPath { get; set; }
        public int Platform { get; set; }
        public int UrlType { get; set; }
    }
}
