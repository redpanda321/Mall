using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class RoleInfo
    {
       
        /// <summary>
        /// Id == RoleId 
        /// </summary>
        [ResultColumn]
        [Obsolete("关联属性移除遗留")]
        public List<RolePrivilegeInfo> RolePrivilegeInfo { get; set; }
    }
}
