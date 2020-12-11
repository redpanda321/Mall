using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class ShopInvoiceConfigInfo
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public short IsInvoice { get; set; }
        public short IsPlainInvoice { get; set; }
        public short IsElectronicInvoice { get; set; }
        public decimal PlainInvoiceRate { get; set; }
        public short IsVatInvoice { get; set; }
        public int VatInvoiceDay { get; set; }
        public decimal VatInvoiceRate { get; set; }
    }
}
