using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class RolePrivilegeInfo
    {
        public long Id { get; set; }
        public int Privilege { get; set; }
        public long RoleId { get; set; }
    }
}
