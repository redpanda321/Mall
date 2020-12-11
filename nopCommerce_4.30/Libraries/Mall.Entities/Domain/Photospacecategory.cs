using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class PhotoSpaceCategoryInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string PhotoSpaceCatrgoryName { get; set; }
        public long DisplaySequence { get; set; }
    }
}
