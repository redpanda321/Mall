using NPoco;
using Mall.Core;

namespace Mall.Entities
{
    public partial class OrderInvoiceInfo
    {

        /// <summary>
        /// 发票类型名称
        /// </summary>
        [ResultColumn]
        public string InvoiceTypeName { get { return this.InvoiceType.ToDescription(); } }

        /// <summary>
        /// 地址全路径名称
        /// </summary>
        [ResultColumn]
        public string RegionFullName { get; set; }
    }
}
