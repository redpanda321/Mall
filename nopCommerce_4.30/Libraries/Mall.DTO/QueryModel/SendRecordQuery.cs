using System;

namespace Mall.DTO.QueryModel
{
    public partial class SendRecordQuery : QueryBase
    {
        public int? msgType { get; set; } 
        public int? sendState { get; set; } 
        public DateTime? startDate { get; set; } 
        public DateTime? endDate { get; set; }
    }
}
