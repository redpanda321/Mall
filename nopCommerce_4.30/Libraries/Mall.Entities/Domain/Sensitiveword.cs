using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class SensitiveWordInfo
    {
        public int Id { get; set; }
        public string SensitiveWord1 { get; set; }
        public string CategoryName { get; set; }
    }
}
