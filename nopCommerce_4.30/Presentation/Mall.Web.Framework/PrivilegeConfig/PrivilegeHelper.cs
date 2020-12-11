using Mall.Application;
using Mall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mall.Web.Framework
{
    public class PrivilegeHelper
    {
        private static Privileges adminPrivileges;
        private static Privileges adminPrivilegesDefault;
        private static Privileges adminPrivilegesInternal;

        private static Privileges sellerAdminPrivileges;

        private static Privileges userPrivileges;
        public static Privileges UserPrivileges
        {
            set
            {
                userPrivileges = value;
            }
            get
            {
                //if (userPrivileges == null)
                {
                    userPrivileges = GetPrivileges<UserPrivilege>();
                }
                return userPrivileges;
            }
        }

        /// <summary>
        /// 平台后台权限
        /// </summary>
        public static Privileges AdminPrivileges
        {
            set
            {
                adminPrivileges = value;
            }
            get
            {
                //if (adminPrivileges == null)
                {
                    adminPrivileges = GetPrivileges<AdminPrivilege>();
                }
                return adminPrivileges;
            }
        }


        /// <summary>
        /// 平台后台导航
        /// </summary>
        public static Privileges AdminPrivilegesDefault
        {
            set
            {
                adminPrivilegesDefault = value;
            }
            get
            {
                //if (adminPrivilegesDefault == null)
                {
                    adminPrivilegesDefault = GetPrivileges<AdminPrivilege>(AdminCatalogType.Default);
                }
                return adminPrivilegesDefault;
            }
        }

        /// <summary>
        /// 平台后台内部导航
        /// </summary>
        public static Privileges AdminPrivilegesInternal
        {
            set
            {
                adminPrivilegesInternal = value;
            }
            get
            {
                //if (adminPrivilegesInternal == null)
                {
                    adminPrivilegesInternal = GetPrivileges<AdminPrivilege>(AdminCatalogType.Internal);
                }
                return adminPrivilegesInternal;
            }
        }

        public static Privileges SellerAdminPrivileges
        {
            set
            {
                sellerAdminPrivileges = value;
            }
            get
            {
                //if (sellerAdminPrivileges == null)
                {
                    sellerAdminPrivileges = GetPrivileges<SellerPrivilege>();
                }
                return sellerAdminPrivileges;
            }
        }

        /// <summary>
        /// 相当于根目录的路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Privileges GetPrivileges<TEnum>()
        {
            var sitesetting = SiteSettingApplication.SiteSettings;
            Type type = typeof(TEnum);
            FieldInfo[] fields = type.GetFields();
            if (fields.Length == 1)
            {
                return null;
            }
            Privileges p = new Privileges();
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(typeof(PrivilegeAttribute), true);
                if (attributes.Length == 0)
                {
                    continue;

                }
                GroupActionItem group = new GroupActionItem();
                ActionItem item = new ActionItem();
                List<string> actions = new List<string>();
                List<PrivilegeAttribute> attrs = new List<PrivilegeAttribute>();
                List<Controllers> ctrls = new List<Controllers>();

                foreach (var attr in attributes)
                {
                    Controllers ctrl = new Controllers();
                    var attribute = attr as PrivilegeAttribute;
                    ctrl.ControllerName = attribute.Controller;
                    ctrl.ActionNames.AddRange(attribute.Action.Split(','));
                    ctrls.Add(ctrl);
                    attrs.Add(attribute);
                }
                var groupInfo = attrs.FirstOrDefault(a => !string.IsNullOrEmpty(a.GroupName));
                if (sitesetting != null)
                {
                    if (!sitesetting.IsOpenPC)
                    {
                        if (groupInfo.GroupName == "店铺")
                        {
                            if (groupInfo.Name == "页面设置" && groupInfo.Pid == 4001)
                                continue;
                        }
                    }
                    if (!sitesetting.IsOpenH5)
                    {
                        if (groupInfo.GroupName == "分销" && !sitesetting.IsOpenMallSmallProg)
                        {
                            if (groupInfo.Name == "分销商品管理" && groupInfo.Pid == 10001)
                                continue;
                            if (groupInfo.Name == "分销业绩" && groupInfo.Pid == 10002)
                                continue;
                        }
                        if (groupInfo.GroupName == "微信")
                        {
                            if (groupInfo.Name == "微信配置" && groupInfo.Pid == 8002)
                                continue;
                            if (groupInfo.Name == "微信菜单" && groupInfo.Pid == 8003)
                                continue;
                            //if (groupInfo.Name == "摇一摇周边页面" && groupInfo.Pid == 8006)
                            //    continue;
                        }
                        if (groupInfo.GroupName == "微店")
                        {
                            //if (groupInfo.Name == "我的微店" && groupInfo.Pid == 9001)
                            //continue;
                            if (groupInfo.Name == "微信端配置" && groupInfo.Pid == 9002)
                                continue;
                        }
                    }
                    if (!sitesetting.IsOpenApp)
                    {
                        if (groupInfo.Name == "App端配置" && groupInfo.Pid == 9003)
                            continue;
                    }
                    if (!(sitesetting.IsOpenMallSmallProg || sitesetting.IsOpenStore))
                    {
                        if (groupInfo.GroupName == "店铺")
                        {
                            if (groupInfo.Name == "门店管理" && groupInfo.Pid == 4008)
                                continue;
                        }
                    }
                    if (!(sitesetting.IsOpenPC || sitesetting.IsOpenH5 || sitesetting.IsOpenApp || sitesetting.IsOpenMallSmallProg))
                    {
                        if (groupInfo.GroupName == "营销")
                        {
                            if (groupInfo.Name == "限时购" && groupInfo.Pid == 7001)
                                continue;
                        }
                    }
                    if (!(sitesetting.IsOpenPC || sitesetting.IsOpenH5 || sitesetting.IsOpenApp || sitesetting.IsOpenMallSmallProg))
                    {
                        if (groupInfo.GroupName == "营销")
                        {
                            if (groupInfo.Name == "优惠券" && groupInfo.Pid == 7002)
                                continue;
                        }

                        if (groupInfo.GroupName == "营销")
                        {
                            if (groupInfo.Name == "满额减" && groupInfo.Pid == 7008)
                                continue;
                        }
                    }
                    if (!(sitesetting.IsOpenPC || sitesetting.IsOpenH5))
                    {
                        if (groupInfo.GroupName == "营销")
                        {
                            if (groupInfo.Name == "组合购" && groupInfo.Pid == 7003)
                                continue;
                        }
                    }

                    if (!(sitesetting.IsOpenH5 || sitesetting.IsOpenApp))
                    {
                        if (groupInfo.GroupName == "营销")
                        {
                            if (groupInfo.Name == "代金红包" && groupInfo.Pid == 7006)
                                continue;
                        }

                        if (groupInfo.GroupName == "营销")
                        {
                            if (groupInfo.Name == "拼团" && groupInfo.Pid == 7007)
                                continue;
                        }
                    }

                    if (!(sitesetting.IsOpenH5 || sitesetting.IsOpenApp))
                    {
                        if (groupInfo.GroupName == "微店")
                        {
                            if (groupInfo.Name == "我的微店" && groupInfo.Pid == 9001)
                                continue;
                        }
                    }
                }

                group.GroupName = groupInfo.GroupName;
                item.PrivilegeId = groupInfo.Pid;
                item.Name = groupInfo.Name;
                item.Url = groupInfo.Url;
                item.Type = groupInfo.AdminCatalogType;

                item.Controllers.AddRange(ctrls);
                var currentGroup = p.Privilege.FirstOrDefault(a => a.GroupName == group.GroupName);
                if (currentGroup == null)
                {
                    group.Items.Add(item);
                    p.Privilege.Add(group);
                }
                else
                {
                    currentGroup.Items.Add(item);
                }
            }
            #region 门店授权
            bool isOpenStore = Application.SiteSettingApplication.SiteSettings != null && Application.SiteSettingApplication.SiteSettings.IsOpenStore;
            if (!isOpenStore)//未授权则关闭门店管理菜单
            {
                var shopManager = p.Privilege.Where(x => x.GroupName.Equals("店铺")).FirstOrDefault();
                if (shopManager != null)
                {
                    shopManager.Items.Remove(shopManager.Items.Where(x => x.PrivilegeId == 4008).FirstOrDefault());// 4008 = "门店管理"
                }
            }
            #endregion
            return p;
        }

        /// <summary>
        /// 相当于根目录的路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="Type">导航类别</param>
        /// <returns></returns>
        public static Privileges GetPrivileges<TEnum>(AdminCatalogType Type)
        {
            var sitesetting = SiteSettingApplication.SiteSettings;

            Type type = typeof(TEnum);
            FieldInfo[] fields = type.GetFields();
            if (fields.Length == 1)
            {
                return null;
            }
            Privileges p = new Privileges();
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(typeof(PrivilegeAttribute), true);
                if (attributes.Length == 0)
                {
                    continue;
                }
                GroupActionItem group = new GroupActionItem();
                ActionItem item = new ActionItem();
                List<string> actions = new List<string>();
                List<PrivilegeAttribute> attrs = new List<PrivilegeAttribute>();
                List<Controllers> ctrls = new List<Controllers>();
                string linkTarget = string.Empty;
                foreach (var attr in attributes)
                {
                    Controllers ctrl = new Controllers();
                    var attribute = attr as PrivilegeAttribute;
                    if (!attribute.AdminCatalogType.Equals(Type))
                    {
                        continue;
                    }
                    ctrl.ControllerName = attribute.Controller;
                    ctrl.ActionNames.AddRange(attribute.Action.Split(','));
                    ctrls.Add(ctrl);
                    attrs.Add(attribute);
                    linkTarget = attribute.LinkTarget;
                }
                if (attrs.Count.Equals(0))
                {
                    continue;
                }
                var groupInfo = attrs.FirstOrDefault(a => !string.IsNullOrEmpty(a.GroupName));
                if (sitesetting != null)
                {
                    if (!sitesetting.IsOpenPC)//PC端未开启授权
                    {
                        if (groupInfo.GroupName == "网站")
                        {
                            if (groupInfo.Name == "首页模板" && groupInfo.Pid == 7001)
                                continue;

                            if (groupInfo.Name == "主题设置" && groupInfo.Pid == 7004)
                                continue;
                        }
                        if (groupInfo.GroupName == "营销")
                        {
                            if (groupInfo.Name == "PC端专题" && groupInfo.Pid == 9010)
                                continue;
                        }
                    }

                    if (!sitesetting.IsOpenH5)//H5端未开启授权
                    {
                        if (groupInfo.GroupName == "微商城")
                        {
                            if (groupInfo.Name == "商城首页设置" && groupInfo.Pid == 10001)
                                continue;
                            if (groupInfo.Name == "底部导航栏" && groupInfo.Pid == 10007)
                                continue;
                            if (groupInfo.Name == "微店管理" && groupInfo.Pid == 10002)
                                continue;
                            if (groupInfo.Name == "菜单设置" && groupInfo.Pid == 10003)
                                continue;
                            if (groupInfo.Name == "公众号设置" && groupInfo.Pid == 10004)
                                continue;
                            if (groupInfo.Name == "素材管理" && groupInfo.Pid == 10005)
                                continue;
                        }

                        if (groupInfo.GroupName == "分销" && !sitesetting.IsOpenMallSmallProg)
                        {
                            if (groupInfo.Name == "分销设置" && groupInfo.Pid == 7101)
                                continue;
                            if (groupInfo.Name == "分销商品" && groupInfo.Pid == 7102)
                                continue;
                            if (groupInfo.Name == "销售员管理" && groupInfo.Pid == 7103)
                                continue;
                            if (groupInfo.Name == "销售员等级" && groupInfo.Pid == 7104)
                                continue;
                            if (groupInfo.Name == "分销业绩" && groupInfo.Pid == 7105)
                                continue;
                            if (groupInfo.Name == "佣金提现管理" && groupInfo.Pid == 7106)
                                continue;
                        }

                        if (groupInfo.GroupName == "营销")
                        {
                            if (groupInfo.Name == "签到" && groupInfo.Pid == 9008)
                                continue;
                            if (groupInfo.Name == "吸粉红包" && groupInfo.Pid == 9004)
                                continue;
                        }
                    }

                    if (!sitesetting.IsOpenApp)
                    {
                        if (groupInfo.GroupName == "APP")
                        {
                            if (groupInfo.Name == "APP商品配置" && groupInfo.Pid == 12003)
                                continue;

                            if (groupInfo.Name == "APP首页配置" && groupInfo.Pid == 12001)
                                continue;

                            if (groupInfo.Name == "关于我们" && groupInfo.Pid == 12002)
                                continue;

                            if (groupInfo.Name == "APP引导页" && groupInfo.Pid == 12004)
                                continue;
                        }

                        if (groupInfo.GroupName == "营销")
                        {
                            if (groupInfo.Name == "APP积分商城" && groupInfo.Pid == 9016)
                                continue;
                        }
                    }

                    if (!sitesetting.IsOpenMallSmallProg)
                    {
                        if (groupInfo.GroupName == "小程序")
                        {
                            if (groupInfo.Name == "首页配置" && groupInfo.Pid == 13001)
                                continue;
                            if (groupInfo.Name == "商品配置" && groupInfo.Pid == 13002)
                                continue;
                            if (groupInfo.Name == "消息配置" && groupInfo.Pid == 13003)
                                continue;
                        }
                    }

                    //if (!sitesetting.IsOpenMultiStoreSmallProg)
                    //{
                    //    //if (groupInfo.GroupName == "小程序")
                    //    //{
                    //    //    if (groupInfo.Name == "消息配置" && groupInfo.Pid == 13003)
                    //    //        continue;
                    //    //}

                    //    if (groupInfo.GroupName == "店铺")
                    //    {
                    //        if (groupInfo.Name == "O2O小程序消息" && groupInfo.Pid == 5008)
                    //            continue;
                    //    }
                    //}

                    if (!sitesetting.IsOpenStore)
                    {
                        if (groupInfo.GroupName == "店铺")
                        {
                            if (groupInfo.Name == "门店管理" && groupInfo.Pid == 5006)
                                continue;

                            if (groupInfo.Name == "周边门店设置" && groupInfo.Pid == 5007)
                                continue;
                        }
                    }

                    if (!(sitesetting.IsOpenPC || sitesetting.IsOpenH5 || sitesetting.IsOpenApp))
                    {
                        if (groupInfo.GroupName == "营销")
                        {
                            if (groupInfo.Name == "新人礼包" && groupInfo.Pid == 9013)//新人礼包
                                continue;
                        }
                    }
                    if (!(sitesetting.IsOpenH5 || sitesetting.IsOpenApp))
                    {
                        if (groupInfo.GroupName == "营销")
                        {
                            if (groupInfo.Name == "刮刮卡管理" && groupInfo.Pid == 9012)
                                continue;

                            if (groupInfo.Name == "大转盘管理" && groupInfo.Pid == 9014)
                                continue;

                            if (groupInfo.Name == "移动端专题" && groupInfo.Pid == 9009)
                                continue;
                        }
                    }

                    if (!(sitesetting.IsOpenPC || sitesetting.IsOpenApp))
                    {
                        if (groupInfo.Name == "礼品管理" && groupInfo.Pid == 9006)
                            continue;
                    }

                }


                group.GroupName = groupInfo.GroupName;
                item.PrivilegeId = groupInfo.Pid;
                item.Name = groupInfo.Name;
                item.Url = groupInfo.Url;
                item.Type = groupInfo.AdminCatalogType;
                item.LinkTarget = linkTarget;
                item.Controllers.AddRange(ctrls);
                var currentGroup = p.Privilege.FirstOrDefault(a => a.GroupName == group.GroupName);
                if (currentGroup == null)
                {
                    group.Items.Add(item);
                    p.Privilege.Add(group);
                }
                else
                {
                    currentGroup.Items.Add(item);
                }

            }
            return p;
        }
    }
}
