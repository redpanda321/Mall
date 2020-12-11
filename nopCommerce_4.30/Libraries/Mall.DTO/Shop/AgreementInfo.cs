namespace Mall.DTO
{
    /// <summary>
    /// 协议
    /// </summary>
    public class AgreementInfo
    {
        long _id = 0;
        /// <summary>
        /// 协议ID
        /// </summary>
        public long Id { get { return _id; } set { _id = value; } }
        /// <summary>
        /// 协议类别
        /// </summary>
        public Entities.AgreementInfo.AgreementTypes AgreementType { get; set; }
        /// <summary>
        /// 协议内容
        /// </summary>
        public string AgreementContent { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public System.DateTime LastUpdateTime { get; set; }
    }
}
