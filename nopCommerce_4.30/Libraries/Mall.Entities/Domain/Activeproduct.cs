using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ActiveProductInfo
    {
        public int Id { get; set; }
        public long ActiveId { get; set; }
        public long ProductId { get; set; }
    }
}
