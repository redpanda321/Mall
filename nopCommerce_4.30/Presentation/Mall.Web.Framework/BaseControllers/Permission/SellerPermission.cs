using Mall.CommonModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mall.Web.Framework
{


    public static class SellerPermission
    {
        private readonly static Dictionary<SellerPrivilege, IEnumerable<ActionPermission>> privileges;
        private readonly static IEnumerable<ActionPermission> ActionPermissions;
        static SellerPermission()
        {
            ActionPermissions = GetAllActionByAssembly();
            privileges = new Dictionary<SellerPrivilege, IEnumerable<ActionPermission>>();
            var SellerAdminPrivileges = PrivilegeHelper.GetPrivileges<SellerPrivilege>().Privilege.Select(a => a.Items);
         
            foreach (var privilege in SellerAdminPrivileges)
            {
               
                foreach (var item in privilege)
                {
                    List<ActionPermission> actions = new List<ActionPermission>();
                    var ctrls = item.Controllers;
                    foreach (var ctrl in ctrls)
                    {
                        foreach (string act in ctrl.ActionNames)
                        {
                            var acts = GetActionByControllerName(ctrl.ControllerName, act);
                            actions.AddRange(acts);
                        }
                    }
                    privileges.Add((SellerPrivilege)item.PrivilegeId, actions);
                }
            }
        }

        private static IEnumerable<ActionPermission> GetActionByControllerName(string controllername, string actionname = "")
        {
            return ActionPermissions.Where(item => item.ControllerName.ToLower() == controllername.ToLower() && (actionname == "" || item.ActionName.ToLower() == actionname.ToLower()));
        }

        public static Dictionary<SellerPrivilege, IEnumerable<ActionPermission>> Privileges { get { return privileges; } }


        private static IList<ActionPermission> GetAllActionByAssembly()
        {
            var result = new List<ActionPermission>();
            var types = Assembly.Load("Mall.Web").GetTypes().Where(a => a.BaseType != null && a.BaseType.Name == "BaseSellerController");

            foreach (var type in types)
            {
                var members = type.GetMethods();
                foreach (var member in members)
                {
                    if (member.ReturnType.Name == "ActionResult" || member.ReturnType.Name == "JsonResult")//如果是Action
                    {
                        var ap = new ActionPermission();

                        ap.ActionName = member.Name;
                        ap.ControllerName = member.DeclaringType.Name.Substring(0, member.DeclaringType.Name.Length - 10); // 去掉“Controller”后缀

                        object[] attrs = member.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), true);
                        if (attrs.Length > 0)
                            ap.Description = (attrs[0] as System.ComponentModel.DescriptionAttribute).Description;

                        result.Add(ap);
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// 检查是否有权限访问该动作的授权
        /// </summary>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public static bool CheckPermissions(List<SellerPrivilege> userprivileages, string controllerName, string actionName)
        {
            //门店app账号特殊处理
            if (controllerName == "App" && actionName == "App" && userprivileages.Where(p => p == SellerPrivilege.SellerApp).Count() <= 0)
            {
                return false;
            }
            if (userprivileages.Contains(0))
                return true;
            //存在一些不在权限组中的Action,这些Action可能被一些小页面使用。如果有这种没有被权限控制的action,就默认为有权限。
            bool hasAction = false;
            foreach(var item in privileges)
            {
                var alist = item.Value.Where(p => p.ControllerName.ToLower() == controllerName.ToLower() && p.ActionName.ToLower() == actionName.ToLower());
                if (alist != null && alist.Count() > 0)
                {
                    hasAction = true;
                    break;
                }
            }
            if (!hasAction)
                return true;
            else
                return privileges.Where(a => userprivileages.Contains(a.Key)).Any(b => b.Value.Any(c => c.ControllerName.ToLower() == controllerName.ToLower() && c.ActionName.ToLower() == actionName.ToLower()));
        }

    }
}