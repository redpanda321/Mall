using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Mall.Service
{
    public class ManagerService : ServiceBase, IManagerService
    {
        public QueryPageModel<ManagerInfo> GetPlatformManagers(ManagerQuery query)
        {
            var users = DbFactory.Default.Get<ManagerInfo>().Where(item => item.ShopId == 0).OrderBy(item => item.RoleId).ToPagedList(query.PageNo, query.PageSize);
            QueryPageModel<ManagerInfo> pageModel = new QueryPageModel<ManagerInfo>()
            {
                Models = users,
                Total = users.TotalRecordCount
            };
            return pageModel;
        }
        public QueryPageModel<ManagerInfo> GetSellerManagers(ManagerQuery query)
        {
            var users = DbFactory.Default.Get<ManagerInfo>().Where(item => item.ShopId == query.ShopID && item.RoleId != 0 && item.Id != query.userID).OrderBy(item => item.Id).ToPagedList(query.PageNo, query.PageSize);
            QueryPageModel<ManagerInfo> pageModel = new QueryPageModel<ManagerInfo>()
            {
                Models = users,
                Total = users.TotalRecordCount
            };
            return pageModel;
        }

        public List<ManagerInfo> GetPlatformManagerByRoleId(long roleId)
        {
            return DbFactory.Default.Get<ManagerInfo>().Where(item => item.ShopId == 0 && item.RoleId == roleId).ToList();
        }

        public ManagerInfo GetPlatformManager(long userId)
        {
            ManagerInfo manager = null;
            string CACHE_MANAGER_KEY = CacheKeyCollection.Manager(userId);

            if (Cache.Exists(CACHE_MANAGER_KEY))
            {
                manager = Core.Cache.Get<ManagerInfo>(CACHE_MANAGER_KEY);
            }
            else
            {
                manager = DbFactory.Default.Get<ManagerInfo>().Where(item => item.Id == userId && item.ShopId == 0).FirstOrDefault();
                if (manager == null)
                    return null;
                if (manager.RoleId == 0)
                {
                    List<AdminPrivilege> AdminPrivileges = new List<AdminPrivilege>();
                    AdminPrivileges.Add(0);
                    manager.RoleName = "系统管理员";
                    manager.AdminPrivileges = AdminPrivileges;
                    manager.Description = "系统管理员";
                }
                else
                {
                    var model = DbFactory.Default.Get<RoleInfo>().Where(p => p.Id == manager.RoleId).FirstOrDefault();
                    if (model != null)
                    {
                        var privilege = DbFactory.Default.Get<RolePrivilegeInfo>().Where(p => p.RoleId == model.Id).ToList();
                        #region 营销中心特别处理，当有营销中心子项权限，而没有包含营销中心权限，则默认有营销中心父权限
                        int intMarketing = (int)Mall.CommonModel.AdminPrivilege.Marketing;//营销中心值
                        var exMarketing = privilege.Where(t => t.Privilege != intMarketing && t.Privilege > 9000 && t.Privilege < 10000).FirstOrDefault();
                        if (exMarketing != null)
                        {
                            RolePrivilegeInfo rpInfo = new RolePrivilegeInfo();//之前不存在营销中心项目，且存在营销中心子项，如存在加上营销中心主值
                            rpInfo.Id = 9999;
                            rpInfo.Privilege = intMarketing;
                            rpInfo.RoleId = exMarketing.RoleId;
                            privilege.Add(rpInfo);
                        }
                        #endregion

                        manager.RoleName = model.RoleName;
                        manager.AdminPrivileges = privilege.Select(p => (AdminPrivilege)p.Privilege).ToList();
                        manager.Description = model.Description;
                    }
                }
                Cache.Insert(CACHE_MANAGER_KEY, manager);
            }
            return manager;
        }

        public List<ManagerInfo> GetSellerManagerByRoleId(long roleId, long shopId)
        {
            return DbFactory.Default.Get<ManagerInfo>().Where(item => item.ShopId == shopId && item.RoleId == roleId).ToList();
        }

        /// <summary>
        /// 根据ShopId获取对应系统管理信息
        /// <para>仅获取首页店铺系统管理员</para>
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public ManagerInfo GetSellerManagerByShopId(long shopId)
        {
            return DbFactory.Default.Get<ManagerInfo>().Where(item => item.ShopId == shopId && item.RoleId == 0).FirstOrDefault();
        }

        public ManagerInfo GetSellerManager(long userId)
        {
            ManagerInfo manager = null;

            string CACHE_MANAGER_KEY = CacheKeyCollection.Seller(userId);

            if (Cache.Exists(CACHE_MANAGER_KEY))
            {
                manager = Core.Cache.Get<ManagerInfo>(CACHE_MANAGER_KEY);
            }
            else
            {
                manager = DbFactory.Default.Get<ManagerInfo>().Where(item => item.Id == userId && item.ShopId != 0).FirstOrDefault();
                if (manager == null)
                    return null;
                if (manager.RoleId == 0)
                {
                    List<SellerPrivilege> SellerPrivileges = new List<SellerPrivilege>();
                    SellerPrivileges.Add(0);
                    manager.RoleName = "系统管理员";
                    manager.SellerPrivileges = SellerPrivileges;
                    manager.Description = "系统管理员";
                }
                else
                {
                    var model = DbFactory.Default.Get<RoleInfo>().Where(p => p.Id == manager.RoleId).FirstOrDefault();
                    if (model != null)
                    {
                        var privilege = DbFactory.Default.Get<RolePrivilegeInfo>().Where(p => p.RoleId == model.Id).ToList();
                        manager.RoleName = model.RoleName;
                        manager.SellerPrivileges = privilege.Select(p => (SellerPrivilege)p.Privilege).ToList(); ;
                        manager.Description = model.Description;
                    }
                }
                Cache.Insert(CACHE_MANAGER_KEY, manager);
            }
            if (manager != null)
            {
                var vshop = DbFactory.Default.Get<VShopInfo>().Where(item => item.ShopId == manager.ShopId).FirstOrDefault();
                manager.VShopId = -1;
                if (vshop != null)
                {
                    manager.VShopId = vshop.Id;
                }
            }
            return manager;
        }

        public void AddPlatformManager(ManagerInfo model)
        {
            if (model.RoleId == 0)
                throw new MallException("权限组选择不正确!");
            if (CheckUserNameExist(model.UserName, true))
            {
                throw new MallException("该用户名已存在！");
            }
            model.ShopId = 0;
            model.PasswordSalt = Guid.NewGuid().ToString();
            model.CreateDate = DateTime.Now;
            var pwd = SecureHelper.MD5(model.Password);
            model.Password = SecureHelper.MD5(pwd + model.PasswordSalt);
            DbFactory.Default.Add(model);
        }

        public void AddSellerManager(ManagerInfo model, string currentSellerName)
        {
            if (model.RoleId == 0)
                throw new MallException("权限组选择不正确!");
            if (CheckUserNameExist(model.UserName))
            {
                throw new MallException("该用户名已存在！");
            }
            if (model.ShopId == 0)
            {
                throw new MallException("没有权限进行该操作！");
            }
            model.PasswordSalt = Guid.NewGuid().ToString();
            model.CreateDate = DateTime.Now;
            var pwd = SecureHelper.MD5(model.Password);
            model.Password = SecureHelper.MD5(pwd + model.PasswordSalt);
            DbFactory.Default.Add(model);
        }


        public void ChangePlatformManagerPassword(long id, string password, long roleId)
        {
            var model = DbFactory.Default.Get<ManagerInfo>().Where(item => item.Id == id && item.ShopId == 0).FirstOrDefault();
            if (model == null)
                throw new MallException("该管理员不存在，或者已被删除!");
            if (roleId != 0 && model.RoleId != 0)
                model.RoleId = roleId;
            if (!string.IsNullOrWhiteSpace(password))
            {
                var pwd = SecureHelper.MD5(password);
                model.Password = SecureHelper.MD5(pwd + model.PasswordSalt);
            }

            DbFactory.Default.Update(model);
            string CACHE_MANAGER_KEY = CacheKeyCollection.Manager(id);
            Core.Cache.Remove(CACHE_MANAGER_KEY);
        }


        public void ChangeSellerManager(ManagerInfo info)
        {
            var model = DbFactory.Default.Get<ManagerInfo>().Where(item => item.Id == info.Id && item.ShopId == info.ShopId).FirstOrDefault();
            if (model == null)
                throw new MallException("该管理员不存在，或者已被删除!");
            if (info.RoleId != 0 && model.RoleId != 0)
                model.RoleId = info.RoleId;
            if (!string.IsNullOrWhiteSpace(info.Password))
            {
                var pwd = SecureHelper.MD5(info.Password);
                model.Password = SecureHelper.MD5(pwd + model.PasswordSalt);
            }
            model.RealName = info.RealName;
            model.Remark = info.Remark;
            DbFactory.Default.Update(model);
            string CACHE_MANAGER_KEY = CacheKeyCollection.Seller(info.Id);
            Core.Cache.Remove(CACHE_MANAGER_KEY);
        }

        public void ChangeSellerManagerPassword(long id, long shopId, string password, long roleId)
        {
            var model = DbFactory.Default.Get<ManagerInfo>().Where(item => item.Id == id && item.ShopId == shopId).FirstOrDefault();
            if (model == null)
                throw new MallException("该管理员不存在，或者已被删除!");
            if (roleId != 0 && model.RoleId != 0)
                model.RoleId = roleId;
            if (!string.IsNullOrWhiteSpace(password))
            {
                var pwd = SecureHelper.MD5(password);
                model.Password = SecureHelper.MD5(pwd + model.PasswordSalt);
            }
            DbFactory.Default.Update(model);
            string CACHE_MANAGER_KEY = CacheKeyCollection.Seller(id);
            Core.Cache.Remove(CACHE_MANAGER_KEY);
        }


        public void DeletePlatformManager(long id)
        {
            DbFactory.Default.Del<ManagerInfo>().Where(item => item.Id == id && item.ShopId == 0 && item.RoleId != 0).Succeed();

            string CACHE_MANAGER_KEY = CacheKeyCollection.Manager(id);
            Core.Cache.Remove(CACHE_MANAGER_KEY);
        }


        public void BatchDeletePlatformManager(long[] ids)
        {
            DbFactory.Default.Del<ManagerInfo>().Where(item => item.ShopId == 0 && item.RoleId != 0 && item.Id.ExIn(ids)).Succeed();
            foreach (var id in ids)
            {
                string CACHE_MANAGER_KEY = CacheKeyCollection.Manager(id);
                Core.Cache.Remove(CACHE_MANAGER_KEY);
            }
        }


        public void DeleteSellerManager(long id, long shopId)
        {
            DbFactory.Default.Del<ManagerInfo>().Where(item => item.Id == id && item.ShopId == shopId && item.RoleId != 0).Succeed();
            //日龙修改
            //var user = context.UserMemberInfo.FirstOrDefault( a => a.UserName == model.UserName );
            //context.ManagerInfo.Remove( user );
            string CACHE_MANAGER_KEY = CacheKeyCollection.Seller(id);
            Core.Cache.Remove(CACHE_MANAGER_KEY);
        }

        public void BatchDeleteSellerManager(long[] ids, long shopId)
        {
            DbFactory.Default.Del<ManagerInfo>().Where(item => item.ShopId == shopId && item.RoleId != 0 && item.Id.ExIn(ids)).Succeed();
            //    var username = model.Select( a => a.UserName ).ToList();
            //日龙修改
            //var user = context.UserMemberInfo.FindBy( item => username.Contains( item.UserName ) );
            //context.UserMemberInfo.Remove( user );

            foreach (var id in ids)
            {
                string CACHE_MANAGER_KEY = CacheKeyCollection.Seller(id);
                Core.Cache.Remove(CACHE_MANAGER_KEY);
            }
        }


        public List<ManagerInfo> GetManagers(string keyWords)
        {
            var sql = DbFactory.Default.Get<ManagerInfo>();
            if (string.IsNullOrEmpty(keyWords)) return sql.ToList();
            sql.Where(item => item.UserName.Contains(keyWords));
            return sql.ToList();
        }

        public ManagerInfo Login(string username, string password, bool isPlatFormManager = false)
        {
            #region 用手机号或者邮箱登录获取用户名登录商家TDO:ZYF
            var IsEmail = Core.Helper.ValidateHelper.IsEmail(username);
            var IsPhone = Core.Helper.ValidateHelper.IsPhone(username);
            if (IsEmail)
            {
                var contact = DbFactory.Default.Get<MemberContactInfo>().Where(a => a.ServiceProvider == "Mall.Plugin.Message.Email" && a.Contact == username && a.UserType == MemberContactInfo.UserTypes.General).FirstOrDefault();
                if(contact != null)
                {
                    var memberInfo = DbFactory.Default.Get<MemberInfo>().Where(a => a.Id == contact.UserId).FirstOrDefault();
                    username = memberInfo.UserName;
                }                    
            }
            else if (IsPhone)
            {
                var contact = DbFactory.Default.Get<MemberContactInfo>().Where(a => a.ServiceProvider == "Mall.Plugin.Message.SMS" && a.Contact == username && a.UserType == MemberContactInfo.UserTypes.General).FirstOrDefault();
                if (contact != null)
                {
                    var memberInfo = DbFactory.Default.Get<MemberInfo>().Where(a => a.Id == contact.UserId).FirstOrDefault();
                    username = memberInfo.UserName;
                }                    
            }
            #endregion

            ManagerInfo manager;
            if (isPlatFormManager)
                manager = DbFactory.Default.Get<ManagerInfo>().Where(item => item.UserName == username && item.ShopId == 0).FirstOrDefault();
            else
                manager = DbFactory.Default.Get<ManagerInfo>().Where(item => item.UserName == username && item.ShopId != 0).FirstOrDefault();
            if (manager != null)
            {

                string encryptedWithSaltPassword = GetPasswrodWithTwiceEncode(password, manager.PasswordSalt);
                if (encryptedWithSaltPassword.ToLower() != manager.Password && manager.Password != password)//比较密码是否一致
                    manager = null;//不一致，则置空，表示未找到指定的管理员
                else//一致，则表示登录成功，更新登录时间
                {
                    if (manager.ShopId > 0)//不处理平台
                    {
                        var shop = ServiceProvider.Instance<IShopService>.Create.GetShop(manager.ShopId);
                        if (shop == null)
                            throw new MallException("未找到帐户对应的店铺");

                        if (!shop.IsSelf)//只处理非官方店铺
                        {
                            if (shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.Freeze)//冻结店铺
                            {
                                //throw new MallException("帐户所在店铺已被冻结");
                            }
                        }
                    }
                }
            }
            return manager;
        }




        string GetPasswrodWithTwiceEncode(string password, string salt)
        {
            string encryptedPassword = Core.Helper.SecureHelper.MD5(password);//一次MD5加密
            string encryptedWithSaltPassword = Core.Helper.SecureHelper.MD5(encryptedPassword + salt);//一次结果加盐后二次加密
            return encryptedWithSaltPassword;
        }

        public ManagerInfo AddSellerManager(string username, string password, string salt = "")
        {
            var model = DbFactory.Default.Get<ManagerInfo>().Where(p => p.UserName == username && p.ShopId != 0).FirstOrDefault();
            if (model != null)
            {
                return new ManagerInfo()
                {
                    Id = model.Id
                };
            }
            if (string.IsNullOrEmpty(salt))
            {
                salt = Core.Helper.SecureHelper.MD5(Guid.NewGuid().ToString("N"));
            }
            ManagerInfo manager = null;
            DbFactory.Default.InTransaction(() =>
            {
                ShopInfo shopInfo = ServiceProvider.Instance<IShopService>.Create.CreateEmptyShop();
                manager = new ManagerInfo()
                {
                    CreateDate = DateTime.Now,
                    UserName = username,
                    Password = password,
                    PasswordSalt = salt,
                    ShopId = shopInfo.Id,
                    SellerPrivileges = new List<SellerPrivilege>() { (SellerPrivilege)0 },
                    AdminPrivileges = new List<AdminPrivilege>(),
                    RoleId = 0,
                };
                DbFactory.Default.Add(manager);
            }, failedAction: (e) => {
                throw e;
            });
            return manager;
        }

        public bool CheckUserNameExist(string username, bool isPlatFormManager = false)
        {
            if (isPlatFormManager)
            {
                return DbFactory.Default.Get<ManagerInfo>().Where(item => item.UserName.ToLower() == username.ToLower() && item.ShopId == 0).Exist();
            }
            var sellerManager = DbFactory.Default.Get<ManagerInfo>().Where(item => item.UserName.ToLower() == username.ToLower() && item.ShopId != 0).Exist();
            return DbFactory.Default.Get<MemberInfo>().Where(item => item.UserName.ToLower() == username.ToLower()).Exist() || sellerManager;
        }

        public ManagerInfo GetSellerManager(string userName)
        {
            var manager = DbFactory.Default.Get<ManagerInfo>().Where(item => item.UserName == userName && item.ShopId != 0).FirstOrDefault();
            return manager;
        }

        public void UpdateShopStatus()
        {
            List<ShopInfo> models = DbFactory.Default.Get<ShopInfo>().Where(s => s.EndDate < DateTime.Now).ToList();
            DbFactory.Default.InTransaction(() =>
            {
                foreach (var m in models)
                {
                    if (m.IsSelf)
                    {
                        //TODO:DZY[150729] 官方自营店到期自动延期
                        /* zjt  
                         * TODO可移除，保留注释即可
                         */
                        m.EndDate = DateTime.Now.AddYears(10);
                        m.ShopStatus = ShopInfo.ShopAuditStatus.Open;
                    }
                    else
                    {
                        m.ShopStatus = ShopInfo.ShopAuditStatus.Unusable;
                    }
                    DbFactory.Default.Update(m);
                }
            });
        }
    }
}

