using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class VerificationRecordModel: VerificationRecordInfo
    {
        public long Quantity { get; set; }
        public string ImagePath { get; set; }
        public string Specifications { get; set; }

        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string Time { get; set; }

    }
}
