using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopHomeModuleTopImgInfo
    {
        public long Id { get; set; }
        public string ImgPath { get; set; }
        public string Url { get; set; }
        public long HomeModuleId { get; set; }
        public int DisplaySequence { get; set; }
    }
}
