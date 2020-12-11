using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ExpressInfoInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string TaobaoCode { get; set; }
        public string Kuaidi100Code { get; set; }
        public string KuaidiNiaoCode { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Logo { get; set; }
        public string BackGroundImage { get; set; }
        public int Status { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
