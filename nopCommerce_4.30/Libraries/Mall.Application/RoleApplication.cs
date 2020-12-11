using Mall.CommonModel;
using Mall.Core;
using Mall.Entities;
using Mall.IServices;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Application
{
    /// <summary>
    /// 角色权限管理
    /// </summary>
    public class RoleApplication:BaseApplicaion
    {
       // private static ICartService _iCartService =  EngineContext.Current.Resolve<ICartService>();
       // private static IPrivilegesService _iPrivilegesService =  EngineContext.Current.Resolve<IPrivilegesService>();

        private static ICartService _iCartService =  EngineContext.Current.Resolve<ICartService>();
        private static IPrivilegesService _iPrivilegesService =  EngineContext.Current.Resolve<IPrivilegesService>();


        private const string RolePrivilegesCacheKey = "RolePrivileges:{0}";

        private static List<int> GetRolePrivileges(long role)
        {
            var result = Cache.Get<List<int>>(string.Format(RolePrivilegesCacheKey, role));
            if (result == null)
            {
                result = GetService<IPrivilegesService>().GetPrivileges(role);
                if (result.Count > 0) Cache.Insert(string.Format(RolePrivilegesCacheKey, role), result);
            }
            return result;
        }
        /// <summary>
        /// 清除角色权限缓存
        /// </summary>
        /// <param name="role"></param>
        public static void ClearRoleCache(long role)
        {
            Cache.Remove(string.Format(RolePrivilegesCacheKey, role));
        }
        /// <summary>
        /// 判断是否有该权限
        /// </summary>
        /// <param name="role">角色ID</param>
        /// <param name="privilege">权限枚举</param>
        /// <returns></returns>
        public static bool IsPrivileges(long role, SellerPrivilege privilege)
        {
            var privileges = GetRolePrivileges(role);
            return privileges.Contains((int)privilege);
        }

        public static RoleInfo GetRoleInfo(long id)
        {
            return _iPrivilegesService.GetRoleInfo(id);
        }

        public static List<int> GetPrivileges(long role) {
            return _iPrivilegesService.GetPrivileges(role);
        }
        public static List<SellerPrivilege> GetSellerPrivileges(long role)
        {
            return GetPrivileges(role).Select(p => (SellerPrivilege)p).ToList();
        }

    }
}
