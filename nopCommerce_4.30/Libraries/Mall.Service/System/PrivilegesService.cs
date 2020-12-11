using Mall.Core;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;

namespace Mall.Service
{
    public class PrivilegesService : ServiceBase, IPrivilegesService
    {
        public void AddPlatformRole(RoleInfo model)
        {
            model.ShopId = 0;
            if (string.IsNullOrEmpty(model.Description))
                model.Description = model.RoleName;

            var ex = DbFactory.Default.Get<RoleInfo>().Where(a => a.RoleName == model.RoleName && a.ShopId == model.ShopId).Exist();
            if (ex) throw new MallException("已存在相同名称的权限组");

            DbFactory.Default.Add(model);
            model.RolePrivilegeInfo.ForEach(p => p.RoleId = model.Id);
            DbFactory.Default.AddRange(model.RolePrivilegeInfo);
        }

        public void UpdatePlatformRole(RoleInfo model)
        {
            var exist = DbFactory.Default.Get<RoleInfo>().Where(a => a.RoleName == model.RoleName && a.ShopId == model.ShopId && a.Id != model.Id).Exist();
            if (exist) throw new MallException("已存在相同名称的权限组");
            if (string.IsNullOrEmpty(model.Description))
                model.Description = model.RoleName;
            model.RolePrivilegeInfo.ForEach(p => p.RoleId = model.Id);
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Set<RoleInfo>().Where(p => p.Id == model.Id)
                .Set(p => p.RoleName, model.RoleName)
                .Set(p => p.Description, model.Description)
                .Succeed();
                DbFactory.Default.Del<RolePrivilegeInfo>(p => p.RoleId == model.Id);
                DbFactory.Default.AddRange(model.RolePrivilegeInfo);
            });
        }

        public void UpdateSellerRole(RoleInfo model)
        {
            UpdatePlatformRole(model);
        }

        public void DeletePlatformRole(long id)
        {
            DbFactory.Default.Del<RoleInfo>().Where(a => a.Id == id && a.ShopId == 0).Succeed();
        }
        public RoleInfo GetPlatformRole(long id)
        {
            return DbFactory.Default.Get<RoleInfo>().Where(a => a.Id == id && a.ShopId == 0).FirstOrDefault();
        }
        public RoleInfo GetRoleInfo(long id)
        {
            return DbFactory.Default.Get<RoleInfo>().Where(p => p.Id == id).FirstOrDefault();
        }
        public RoleInfo GetSellerRole(long id, long shopid)
        {
            return DbFactory.Default.Get<RoleInfo>().Where(a => a.Id == id && a.ShopId == shopid).FirstOrDefault();
        }

        public List<RoleInfo> GetPlatformRoles()
        {
            return DbFactory.Default.Get<RoleInfo>().Where(item => item.ShopId == 0).ToList();
        }

        public List<RoleInfo> GetSellerRoles(long shopId)
        {
            return DbFactory.Default.Get<RoleInfo>().Where(item => item.ShopId == shopId && item.ShopId != 0).ToList();
        }

        public void AddSellerRole(RoleInfo model)
        {
            if (string.IsNullOrEmpty(model.Description))
                model.Description = model.RoleName;
            var ex = DbFactory.Default.Get<RoleInfo>().Where(a => a.RoleName == model.RoleName && a.ShopId == model.ShopId).Exist();
            if (ex) throw new MallException("已存在相同名称的权限组");
            DbFactory.Default.Add(model);
            model.RolePrivilegeInfo.ForEach(p => p.RoleId = model.Id);
            DbFactory.Default.AddRange(model.RolePrivilegeInfo);
        }


        public void DeleteSellerRole(long id, long shopId)
        {
            DbFactory.Default.Del<RoleInfo>().Where(a => a.Id == id && a.ShopId == shopId).Succeed();
        }

        public List<int> GetPrivileges(long role)
        {
            return DbFactory.Default.Get<RolePrivilegeInfo>().Where(p => p.RoleId == role).Select(p => p.Privilege).ToList<int>();
        }
    }
}
