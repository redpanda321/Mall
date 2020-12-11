using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class MemberInfo
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string Nick { get; set; }
        public int Sex { get; set; }
        public string Email { get; set; }
        public DateTime CreateDate { get; set; }
        public int TopRegionId { get; set; }
        public int RegionId { get; set; }
        public string RealName { get; set; }
        public string CellPhone { get; set; }
        public string Qq { get; set; }
        public string Address { get; set; }
        public byte Disabled { get; set; }
        public DateTime LastLoginDate { get; set; }
        public int OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Expenditure { get; set; }
        public int Points { get; set; }
        public string Photo { get; set; }
        public long ParentSellerId { get; set; }
        public string Remark { get; set; }
        public string PayPwd { get; set; }
        public string PayPwdSalt { get; set; }
        public long InviteUserId { get; set; }
        public DateTime? BirthDay { get; set; }
        public string Occupation { get; set; }
        public decimal NetAmount { get; set; }
        public DateTime? LastConsumptionTime { get; set; }
        public int Platform { get; set; }
    }
}
