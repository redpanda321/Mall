using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ThemeInfo
    {
        public long ThemeId { get; set; }
        public int TypeId { get; set; }
        public string MainColor { get; set; }
        public string SecondaryColor { get; set; }
        public string WritingColor { get; set; }
        public string FrameColor { get; set; }
        public string ClassifiedsColor { get; set; }
        public short IsUse { get; set; }
    }
}
