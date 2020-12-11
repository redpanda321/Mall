using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.OpenApi.Model.DTO
{
    public class user_list_model
    {
        public long uid { get; set; }
        public string user_name { get; set; }
        public DateTime created { get; set; }
        public string real_name { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public string avatar { get; set; }
        public string sex { get; set; }
        public DateTime? birthday { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string district { get; set; }
        public string town { get; set; }
        public string address { get; set; }
        public int points { get; set; }
        public int trade_count { get; set; }
        public decimal trade_amount { get; set; }

    }
}
