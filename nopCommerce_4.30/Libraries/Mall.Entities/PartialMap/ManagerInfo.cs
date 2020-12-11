using Mall.CommonModel;
using NPoco;
using System.Collections.Generic;
using System;

namespace Mall.Entities
{
    public partial class ManagerInfo : ISellerManager, IPaltManager
    {
        /// <summary>
        /// 管理员角色名
        /// </summary>
        [ResultColumn]
        public string RoleName { get; set; }

        /// <summary>
        /// 平台管理员权限列表
        /// </summary>
        [ResultColumn]
        public List<AdminPrivilege> AdminPrivileges { set; get; }

        /// <summary>
        /// 商家管理员权限列表
        /// </summary>
        [ResultColumn]
        public List<SellerPrivilege> SellerPrivileges { set; get; }


        /// <summary>
        /// 管理员角色说明
        /// </summary>
        [ResultColumn]
        public string Description { set; get; }
        /// <summary>
        /// 微店编号
        /// </summary>
        [ResultColumn]
        public long VShopId { get; set; }
        /// <summary>
        /// 是否主账号
        /// </summary>
        [ResultColumn]
        public bool IsMainAccount
        {
            get
            {
                if (this.UserName.Contains(":"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
