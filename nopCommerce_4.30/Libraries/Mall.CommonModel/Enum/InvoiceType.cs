using System.ComponentModel;

namespace Mall.CommonModel
{

    /// <summary>
    /// 发票类型
    /// </summary>
    public enum InvoiceType
    {
        /// <summary>
        /// 不需要发票
        /// </summary>
        [Description("不需要发票")]
        None = 0,
        /// <summary>
        /// 增值税发票
        /// </summary>
        [Description("增值税发票")]
        VATInvoice = 3,

        /// <summary>
        /// 普通发票
        /// </summary>
        [Description("普通发票")]
        OrdinaryInvoices = 1,

        /// <summary>
        /// 电子普通发票
        /// </summary>
        [Description("电子普通发票")]
        ElectronicInvoice =2
    }

    public class InvoiceTypes
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Rate { get; set; }
    }
}
