using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MemberContactInfo
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public int UserType { get; set; }
        public string ServiceProvider { get; set; }
        public string Contact { get; set; }
    }
}
