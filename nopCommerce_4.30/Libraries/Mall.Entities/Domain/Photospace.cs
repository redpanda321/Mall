using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class PhotoSpaceInfo
    {
        public long Id { get; set; }
        public long PhotoCategoryId { get; set; }
        public long ShopId { get; set; }
        public string PhotoName { get; set; }
        public string PhotoPath { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}
