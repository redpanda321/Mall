﻿using System;
using System.Collections.Generic;

namespace Mall.Entities.Domain
{
    public partial class InvoiceTitleInfo
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public int InvoiceType { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string InvoiceContext { get; set; }
        public string RegisterAddress { get; set; }
        public string RegisterPhone { get; set; }
        public string BankName { get; set; }
        public string BankNo { get; set; }
        public string RealName { get; set; }
        public string CellPhone { get; set; }
        public string Email { get; set; }
        public int RegionId { get; set; }
        public string Address { get; set; }
        public byte IsDefault { get; set; }
    }
}
