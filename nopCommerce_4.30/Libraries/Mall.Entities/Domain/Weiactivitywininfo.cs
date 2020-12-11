using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class WeiActivityWinInfoInfo
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long ActivityId { get; set; }
        public long AwardId { get; set; }
        public byte IsWin { get; set; }
        public string AwardName { get; set; }
        public DateTime AddDate { get; set; }
    }
}
