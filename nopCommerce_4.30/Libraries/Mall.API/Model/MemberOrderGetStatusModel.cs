using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.API.Model
{
    public class MemberOrderGetStatusModel
    {
        public bool groupsuccess;

        public long orderId { get; set; }
        public int status { get; set; }
        public long activeId { get; set; }
        public long groupId { get; set; }
    }
}
