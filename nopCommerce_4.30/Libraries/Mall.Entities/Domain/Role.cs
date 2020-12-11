using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class RoleInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
    }
}
