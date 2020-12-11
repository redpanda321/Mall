using NPoco;
using System.ComponentModel;
using System.Configuration;

namespace Mall.Entities
{
    public partial class MemberGradeInfo
    {

        /// <summary>
        /// 是否可以删除
        /// <para>拥有关联礼品时不可删除</para>
        /// </summary>
        [ResultColumn]
        public bool IsNoDelete { get; set; }
    }
}
