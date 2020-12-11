using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO.QueryModel
{
    public class BonusQuery:QueryBase
    {
        public BonusInfo.BonusType? Type { get; set; }
        public int? State { get; set; }

        public string Name { get; set; }
    }
}
