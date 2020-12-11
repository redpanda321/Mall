using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class TypeInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public byte IsSupportColor { get; set; }
        public byte IsSupportSize { get; set; }
        public byte IsSupportVersion { get; set; }
        public short IsDeleted { get; set; }
        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { get; set; }
    }
}
