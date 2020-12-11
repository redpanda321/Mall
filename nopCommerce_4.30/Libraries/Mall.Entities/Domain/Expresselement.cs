using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ExpressElementInfo
    {
        public long Id { get; set; }
        public long ExpressId { get; set; }
        public int ElementType { get; set; }
        public int LeftTopPointX { get; set; }
        public int LeftTopPointY { get; set; }
        public int RightBottomPointX { get; set; }
        public int RightBottomPointY { get; set; }
    }
}
