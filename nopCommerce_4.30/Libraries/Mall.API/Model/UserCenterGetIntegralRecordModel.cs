using System;

namespace Mall.API.Model
{
    public class UserCenterGetIntegralRecordModel
    {
        public long Id { get; set; }
        public Mall.Entities.MemberIntegralInfo.IntegralType TypeId { get; set; }
        public string ShowType { get; set; }
        public int Integral { get; set; }
        public Nullable<System.DateTime> RecordDate { get; set; }
        public string ReMark { get; set; }
    }
}
