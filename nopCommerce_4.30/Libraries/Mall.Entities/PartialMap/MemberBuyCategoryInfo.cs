using NPoco;
using System.ComponentModel;
using System.Configuration;

namespace Mall.Entities
{
    public partial class MemberBuyCategoryInfo
    {

        /// <summary>
        /// 类别名称,需手动填充数据
        /// </summary>
        [ResultColumn]
        public string CategoryName { get; set; }
    }
}
