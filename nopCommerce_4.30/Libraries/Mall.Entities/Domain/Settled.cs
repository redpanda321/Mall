using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class SettledInfo
    {
        public long Id { get; set; }
        public int BusinessType { get; set; }
        public int SettlementAccountType { get; set; }
        public int TrialDays { get; set; }
        public int IsCity { get; set; }
        public int IsPeopleNumber { get; set; }
        public int IsAddress { get; set; }
        public int IsBusinessLicenseCode { get; set; }
        public int IsBusinessScope { get; set; }
        public int IsBusinessLicense { get; set; }
        public int IsAgencyCode { get; set; }
        public int IsAgencyCodeLicense { get; set; }
        public int IsTaxpayerToProve { get; set; }
        public int CompanyVerificationType { get; set; }
        public int IsSname { get; set; }
        public int IsScity { get; set; }
        public int IsSaddress { get; set; }
        public int IsSidcard { get; set; }
        public int IsSidCardUrl { get; set; }
        public int SelfVerificationType { get; set; }
    }
}
