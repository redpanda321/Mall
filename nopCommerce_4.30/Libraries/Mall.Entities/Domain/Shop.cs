using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopInfo
    {
        public long Id { get; set; }
        public long GradeId { get; set; }
        public string ShopName { get; set; }
        public string Logo { get; set; }
        public string SubDomains { get; set; }
        public string Theme { get; set; }
        public byte IsSelf { get; set; }
        public int ShopStatus { get; set; }
        public string RefuseReason { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CompanyName { get; set; }
        public int CompanyRegionId { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public int CompanyEmployeeCount { get; set; }
        public decimal CompanyRegisteredCapital { get; set; }
        public string ContactsName { get; set; }
        public string ContactsPhone { get; set; }
        public string ContactsEmail { get; set; }
        public string BusinessLicenceNumber { get; set; }
        public string BusinessLicenceNumberPhoto { get; set; }
        public int BusinessLicenceRegionId { get; set; }
        public DateTime? BusinessLicenceStart { get; set; }
        public DateTime? BusinessLicenceEnd { get; set; }
        public string BusinessSphere { get; set; }
        public string OrganizationCode { get; set; }
        public string OrganizationCodePhoto { get; set; }
        public string GeneralTaxpayerPhot { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public int BankRegionId { get; set; }
        public string BankPhoto { get; set; }
        public string TaxRegistrationCertificate { get; set; }
        public string TaxpayerId { get; set; }
        public string TaxRegistrationCertificatePhoto { get; set; }
        public string PayPhoto { get; set; }
        public string PayRemark { get; set; }
        public string SenderName { get; set; }
        public string SenderAddress { get; set; }
        public string SenderPhone { get; set; }
        public decimal Freight { get; set; }
        public decimal FreeFreight { get; set; }
        public int Stage { get; set; }
        public int SenderRegionId { get; set; }
        public string BusinessLicenseCert { get; set; }
        public string ProductCert { get; set; }
        public string OtherCert { get; set; }
        public string LegalPerson { get; set; }
        public DateTime? CompanyFoundingDate { get; set; }
        public int BusinessType { get; set; }
        public string Idcard { get; set; }
        public string IdcardUrl { get; set; }
        public string IdcardUrl2 { get; set; }
        public string WeiXinNickName { get; set; }
        public int? WeiXinSex { get; set; }
        public string WeiXinAddress { get; set; }
        public string WeiXinTrueName { get; set; }
        public string WeiXinOpenId { get; set; }
        public string WeiXinImg { get; set; }
        public byte AutoAllotOrder { get; set; }
        public short IsAutoPrint { get; set; }
        public int PrintCount { get; set; }
        public byte IsOpenTopImageAd { get; set; }
    }
}
