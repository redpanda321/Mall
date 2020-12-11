using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    /// <summary>
    /// 商家入驻设置
    /// </summary>
    public class Settled
    {
        private long _ID = 0;
        /// <summary>
        /// 设置主键ID
        /// </summary>
        public long ID { get { return _ID; } set { _ID = value; } }

        private Mall.CommonModel.BusinessType _BusinessType = Mall.CommonModel.BusinessType.Enterprise;
        /// <summary>
        /// 商家类型
        /// </summary>
        public Mall.CommonModel.BusinessType BusinessType { get { return _BusinessType; } set { _BusinessType = value; } }

        private Mall.CommonModel.SettleAccountsType _SettlementAccountType = Mall.CommonModel.SettleAccountsType.SettleBank;
        /// <summary>
        /// 商家结算类型
        /// </summary>
        public Mall.CommonModel.SettleAccountsType SettlementAccountType { get { return _SettlementAccountType; } set { _SettlementAccountType = value; } }

        private int _TrialDays = 0;
        /// <summary>
        /// 试用天数
        /// </summary>
        public int TrialDays { get { return _TrialDays; } set { _TrialDays = value; } }

        private Mall.CommonModel.VerificationStatus _IsCity = Mall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 地址必填
        /// </summary>
        public Mall.CommonModel.VerificationStatus IsCity { get { return _IsCity; } set { _IsCity = value; } }

        private Mall.CommonModel.VerificationStatus _IsPeopleNumber = Mall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 人数必填
        /// </summary>
        public Mall.CommonModel.VerificationStatus IsPeopleNumber { get { return _IsPeopleNumber; } set { _IsPeopleNumber = value; } }

        private Mall.CommonModel.VerificationStatus _IsAddress = Mall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 详细地址必填
        /// </summary>
        public Mall.CommonModel.VerificationStatus IsAddress { get { return _IsAddress; } set { _IsAddress = value; } }

        private Mall.CommonModel.VerificationStatus _IsBusinessLicenseCode = Mall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 营业执照号必填
        /// </summary>
        public Mall.CommonModel.VerificationStatus IsBusinessLicenseCode { get { return _IsBusinessLicenseCode; } set { _IsBusinessLicenseCode = value; } }

        private Mall.CommonModel.VerificationStatus _IsBusinessScope = Mall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 经营范围必填
        /// </summary>
        public Mall.CommonModel.VerificationStatus IsBusinessScope { get { return _IsBusinessScope; } set { _IsBusinessScope = value; } }

        private Mall.CommonModel.VerificationStatus _IsBusinessLicense = Mall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 营业执照必填
        /// </summary>
        public Mall.CommonModel.VerificationStatus IsBusinessLicense { get { return _IsBusinessLicense; } set { _IsBusinessLicense = value; } }

        private Mall.CommonModel.VerificationStatus _IsAgencyCode = Mall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 机构代码必填
        /// </summary>
        public Mall.CommonModel.VerificationStatus IsAgencyCode { get { return _IsAgencyCode; } set { _IsAgencyCode = value; } }

        private Mall.CommonModel.VerificationStatus _IsAgencyCodeLicense = Mall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 机构代码证必填
        /// </summary>
        public Mall.CommonModel.VerificationStatus IsAgencyCodeLicense { get { return _IsAgencyCodeLicense; } set { _IsAgencyCodeLicense = value; } }


        private Mall.CommonModel.VerificationStatus _IsTaxpayerToProve = Mall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 纳税人证明必填
        /// </summary>
        public Mall.CommonModel.VerificationStatus IsTaxpayerToProve { get { return _IsTaxpayerToProve; } set { _IsTaxpayerToProve = value; } }

        private Mall.CommonModel.VerificationType _CompanyVerificationType = Mall.CommonModel.VerificationType.VerifyPhone;
        /// <summary>
        /// 验证类型
        /// </summary>
        public Mall.CommonModel.VerificationType CompanyVerificationType { get { return _CompanyVerificationType; } set { _CompanyVerificationType = value; } }


        private Mall.CommonModel.VerificationStatus _IsSName = Mall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 个人姓名必填
        /// </summary>
        public Mall.CommonModel.VerificationStatus IsSName { get { return _IsSName; } set { _IsSName = value; } }

        private Mall.CommonModel.VerificationStatus _IsSCity = Mall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 个人地址必填
        /// </summary>
        public Mall.CommonModel.VerificationStatus IsSCity { get { return _IsSCity; } set { _IsSCity = value; } }

        private Mall.CommonModel.VerificationStatus _IsSAddress = Mall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 个人详细地址必填
        /// </summary>
        public Mall.CommonModel.VerificationStatus IsSAddress { get { return _IsSAddress; } set { _IsSAddress = value; } }

        private Mall.CommonModel.VerificationStatus _IsSIDCard = Mall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 个人身份证必填
        /// </summary>
        public Mall.CommonModel.VerificationStatus IsSIDCard { get { return _IsSIDCard; } set { _IsSIDCard = value; } }

        private Mall.CommonModel.VerificationStatus _IsSIdCardUrl = Mall.CommonModel.VerificationStatus.Must;
        /// <summary>
        /// 个人身份证上传
        /// </summary>
        public Mall.CommonModel.VerificationStatus IsSIdCardUrl { get { return _IsSIdCardUrl; } set { _IsSIdCardUrl = value; } }

        private Mall.CommonModel.VerificationType _SelfVerificationType = Mall.CommonModel.VerificationType.VerifyPhone;
        /// <summary>
        /// 个人验证类型
        /// </summary>
        public Mall.CommonModel.VerificationType SelfVerificationType { get { return _SelfVerificationType; } set { _SelfVerificationType = value; } }
    }
}
