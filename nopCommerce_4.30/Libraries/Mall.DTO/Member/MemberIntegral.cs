using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace Mall.DTO
{
    public class MemberIntegral
    {
        public long Id { get; set; }
        public long MemberId { get; set; }
        public string UserName { get; set; }
        public int HistoryIntegrals { get; set; }
        public int AvailableIntegrals { get; set; }
        public string MemberGrade { get; set; }
        [DisplayFormat(DataFormatString = "yyyy-MM-dd")]
        public DateTime CreateDate { get; set; }
    }
}
