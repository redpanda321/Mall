using PetaPoco;
using System;

namespace Mall.DTO
{
    public class FightGroupUserInfo
    {
        /// <summary>
        /// 用户头像
        /// </summary>
        [ResultColumn]
        public string Photo { get; set; }
        /// <summary>
        /// 订单用户名
        /// </summary>
        [ResultColumn]
        public string UserName { get; set; }
        /// <summary>
        /// 参团时间
        /// </summary>
        [ResultColumn]
        public DateTime? JoinTime { get; set; }
    }
}
