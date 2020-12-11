using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Service
{
    public class VShopService : ServiceBase, IVShopService
    {
        public QueryPageModel<VShopInfo> GetVShopByParamete(VshopQuery vshopQuery)
        {
            var vshops = DbFactory.Default.Get<VShopInfo>();
            if (vshopQuery.VshopType.HasValue)
            {
                if (vshopQuery.VshopType != 0)
                {
                    var vshopex = DbFactory.Default
                        .Get<VShopExtendInfo>()
                        .Where<VShopInfo>((vsei, vsi) => vsei.VShopId == vsi.Id && vsei.Type == vshopQuery.VshopType)
                        .Select(n => n.VShopId);
                      
                    vshops.Where(n => n.ExExists(vshopex));
                }
                else
                {
                    var vshopex = DbFactory.Default
                        .Get<VShopExtendInfo>()
                        .Where<VShopInfo>((vsei, vsi) => vsei.VShopId == vsi.Id);
                    vshops.Where(n => n.ExNotExists(vshopex));
                }
            }
            if (!string.IsNullOrEmpty(vshopQuery.Name))
            {
                vshops.Where(a => a.Name.Contains(vshopQuery.Name));
            }
            if (vshopQuery.ExcepetVshopId.HasValue && vshopQuery.ExcepetVshopId.Value != 0)
            {
                vshops.Where(a => a.Id != vshopQuery.ExcepetVshopId.Value);
            }

            //微店状态判断
            if (vshopQuery.Status.HasValue)
            {
                vshops.Where(e => e.State == vshopQuery.Status.Value);
            }
            else
            {
                vshops.Where(e => e.State == VShopInfo.VShopStates.Normal || e.State == VShopInfo.VShopStates.Close);
            }

            //微店开启状态判断
            if (vshopQuery.IsOpen.HasValue)
                vshops.Where(e => e.IsOpen == vshopQuery.IsOpen.Value);

            QueryPageModel<VShopInfo> result = new QueryPageModel<VShopInfo>();
            if (vshopQuery.IsAsc)
            {
                vshops.OrderBy(a => a.CreateTime);
            }
            else
            {
                vshops.OrderByDescending(a => a.CreateTime);
            }

            var rets = vshops.ToPagedList(vshopQuery.PageNo, vshopQuery.PageSize);
            result.Models = rets;
            result.Total = rets.TotalRecordCount;
            return result;
        }

    

        VShopInfo GetVshopById(long vshopId)
        {
            return DbFactory.Default.Get<VShopInfo>().Where(a => a.Id == vshopId).FirstOrDefault();
        }

        public List<VShopInfo> GetHotShop(VshopQuery vshopQuery, DateTime? startTime, DateTime? endTime, out int total)
        {
           
            var vshops = DbFactory.Default
                .Get<VShopInfo>()
                .Where(e => e.State != VShopInfo.VShopStates.Close)
                .InnerJoin<VShopExtendInfo>((vsi, vsei) => vsi.Id == vsei.VShopId && vsi.IsOpen == true)
                .Where<VShopExtendInfo>(n => n.Type == VShopExtendInfo.VShopExtendType.HotVShop)
                .OrderByDescending<VShopExtendInfo>(n => n.AddTime);
             
            if (!string.IsNullOrEmpty(vshopQuery.Name))
                vshops.Where(a => a.Name.Contains(vshopQuery.Name));
            if (startTime.HasValue)
            {
                vshops.Where<VShopExtendInfo>(item => item.AddTime >= startTime);
            }
            if (endTime.HasValue)
            {
                var end = endTime.Value.Date.AddDays(1);
                vshops.Where<VShopExtendInfo>(item => item.AddTime < end);
            }
            var rets = vshops.ToPagedList(vshopQuery.PageNo, vshopQuery.PageSize);
            total = rets.TotalRecordCount;
            return rets;
        }
        public List<VShopExtendInfo> GetExtends(List<long> vShops)
        {
            return DbFactory.Default.Get<VShopExtendInfo>(p => p.VShopId.ExIn(vShops)).ToList();
        }
        public VShopInfo GetTopShop()
        {
            var vseisql = DbFactory.Default
                .Get<VShopExtendInfo>()
                .Where<VShopInfo>((vsei, vsi) => vsei.VShopId == vsi.Id && vsei.Type == VShopExtendInfo.VShopExtendType.TopShow && vsi.IsOpen == true);
            return DbFactory.Default
                .Get<VShopInfo>()
                .Where(a => a.State != VShopInfo.VShopStates.Close && a.ExExists(vseisql))
                .FirstOrDefault();
        }

        public void SetTopShop(long vshopId)
        {
            var newTopVshop = GetVshopById(vshopId);
            DbFactory.Default.InTransaction(() =>
            {
                if (DbFactory.Default.Get<VShopExtendInfo>().Where(a => a.Type == VShopExtendInfo.VShopExtendType.TopShow).Count() == 1)
                {
                    DbFactory.Default.Del<VShopExtendInfo>().Where(a => a.Type == VShopExtendInfo.VShopExtendType.TopShow).Succeed();
                    //Context.VShopExtendInfo.Remove(oldTopVshop);
                }
                if (DbFactory.Default.Get<VShopExtendInfo>().Where(a => a.VShopId == vshopId).Count() >= 1)
                {
                    DbFactory.Default.Del<VShopExtendInfo>().Where(item => item.VShopId == vshopId).Succeed();
                }
                DbFactory.Default.Add(new VShopExtendInfo
                {
                    VShopId = vshopId,
                    Type = VShopExtendInfo.VShopExtendType.TopShow,
                    AddTime = DateTime.Now
                });
            });
        }

        public void SetHotShop(long vshopId)
        {
            var hotCount = DbFactory.Default.Get<VShopExtendInfo>()
                .LeftJoin<VShopInfo>((se, s) => se.VShopId == s.Id)
                .Where(p => p.Type == VShopExtendInfo.VShopExtendType.HotVShop)
                .Where<VShopInfo>(p => p.State != VShopInfo.VShopStates.Close).Count();
            if (hotCount >= 60)
                throw new MallException("热门微店最多为60个");

            var isHot = DbFactory.Default.Get<VShopExtendInfo>().Where(p => p.VShopId == vshopId && p.Type == VShopExtendInfo.VShopExtendType.HotVShop).Exist();
            if (isHot)
                throw new MallException("该微店已经是热门微店");

            DbFactory.Default.InTransaction(() =>
            {
                if (DbFactory.Default.Get<VShopExtendInfo>().Where(a => a.VShopId == vshopId).Count() >= 1)
                {
                    DbFactory.Default.Del<VShopExtendInfo>().Where(item => item.VShopId == vshopId).Succeed();
                }
                DbFactory.Default.Add(new VShopExtendInfo
                {
                    VShopId = vshopId,
                    AddTime = DateTime.Now,
                    Type = VShopExtendInfo.VShopExtendType.HotVShop,
                    Sequence = 1
                });
            });
        }

        public void CloseShop(long vshopId)
        {
            var vshopInfo = GetVshopById(vshopId);
            vshopInfo.State = VShopInfo.VShopStates.Close;
            vshopInfo.IsOpen = false;//下架微店，则一定要同步关闭
            DbFactory.Default.Update(vshopInfo);
        }

        public void SetShopNormal(long vshopId)
        {
            var vshopInfo = GetVshopById(vshopId);
            vshopInfo.State = VShopInfo.VShopStates.Normal;
            DbFactory.Default.Update(vshopInfo);
        }

        /// <summary>
        /// 开启或关闭微店
        /// </summary>
        public void SetVShopIsOpen(long vshopId,bool isOpen)
        {
            var vshopInfo = GetVshopById(vshopId);
            vshopInfo.IsOpen = isOpen;//开启或关闭微店
            if (isOpen) //后面调整商铺开启微店默认审核状态通过，既开启时不影响其他地方判断是否有没审核通过
                vshopInfo.State = Entities.VShopInfo.VShopStates.Normal;
            DbFactory.Default.Update(vshopInfo);
        }

        public void DeleteHotShop(long vshopId)
        {
            DbFactory.Default.Del<VShopExtendInfo>().Where(item => item.VShopId == vshopId && item.Type == VShopExtendInfo.VShopExtendType.HotVShop).Succeed();
        }

        public void ReplaceHotShop(long oldVShopId, long newHotVShopId)
        {
            //var vshopExtendInfo = DbFactory.Default.Get<VShopExtendInfo>().Where(item => item.VShopId == oldVShopId && item.Type == VShopExtendInfo.VShopExtendType.HotVShop).FirstOrDefault();
            //vshopExtendInfo.VShopId = newHotVShopId;
            //vshopExtendInfo.AddTime = DateTime.Now;
            //DbFactory.Default.Update(vshopExtendInfo);
            DbFactory.Default.Set<VShopExtendInfo>().Set(n => n.VShopId, newHotVShopId).Set(n => n.AddTime, DateTime.Now).Where(item => item.VShopId == oldVShopId && item.Type == VShopExtendInfo.VShopExtendType.HotVShop).Succeed();
        }

        public void UpdateSequence(long vshopId, int? sequence)
        {
            //var vshopExtendInfo = DbFactory.Default.Get<VShopExtendInfo>().Where(item => item.VShopId == vshopId && item.Type == VShopExtendInfo.VShopExtendType.HotVShop).FirstOrDefault();
            //vshopExtendInfo.Sequence = sequence;
            //DbFactory.Default.Update(vshopExtendInfo);
            DbFactory.Default.Set<VShopExtendInfo>().Set(n => n.Sequence, sequence).Where(item => item.VShopId == vshopId && item.Type == VShopExtendInfo.VShopExtendType.HotVShop).Succeed();
        }
        public void AuditThrough(long vshopId)
        {
            DbFactory.Default.Add(new VShopExtendInfo
            {
                Sequence = 1,
                VShopId = vshopId,
                AddTime = DateTime.Now,
                Type = VShopExtendInfo.VShopExtendType.HotVShop
            });
        }

        public void AuditRefused(long vshopId)
        {
            var vshopInfo = GetVshopById(vshopId);
            vshopInfo.State = VShopInfo.VShopStates.Refused;
            DbFactory.Default.Update(vshopInfo);
        }

        public void ReplaceTopShop(long oldVShopId, long newTopVShopId)
        {
            //var vshopExtendInfo = DbFactory.Default.Get<VShopExtendInfo>().Where(item => item.VShopId == oldVShopId && item.Type == VShopExtendInfo.VShopExtendType.TopShow).FirstOrDefault();
            //vshopExtendInfo.VShopId = newTopVShopId;
            //vshopExtendInfo.AddTime = DateTime.Now;
            //DbFactory.Default.Update(vshopExtendInfo);
            DbFactory.Default.Set<VShopExtendInfo>().Set(n => n.VShopId, newTopVShopId).Set(n => n.AddTime, DateTime.Now).Where(item => item.VShopId == oldVShopId && item.Type == VShopExtendInfo.VShopExtendType.TopShow).Succeed();
        }

        public void DeleteTopShop(long vshopId)
        {
            DbFactory.Default.Del<VShopExtendInfo>().Where(item => item.VShopId == vshopId && item.Type == VShopExtendInfo.VShopExtendType.TopShow).Succeed();
        }


        public VShopInfo GetVShopByShopId(long shopId)
        {
            return DbFactory.Default.Get<VShopInfo>().Where(item => item.ShopId == shopId).FirstOrDefault();
        }

        public void CreateVshop(VShopInfo vshopInfo)
        {
            if (vshopInfo.ShopId <= 0)
                throw new Mall.Core.InvalidPropertyException("请传入合法的店铺Id，店铺Id必须大于0");
            if (string.IsNullOrWhiteSpace(vshopInfo.StrLogo))
                throw new Mall.Core.InvalidPropertyException("微店Logo不能为空");
            if (string.IsNullOrWhiteSpace(vshopInfo.WXLogo))
                throw new Mall.Core.InvalidPropertyException("微信Logo不能为空");
            if (string.IsNullOrWhiteSpace(vshopInfo.StrBackgroundImage))
                throw new Mall.Core.InvalidPropertyException("微店背景图片不能为空");
            //if (string.IsNullOrWhiteSpace(vshopInfo.Description))
            //{
            //    throw new Mall.Core.InvalidPropertyException("微店描述不能为空");
            //}
            if (!string.IsNullOrWhiteSpace(vshopInfo.Tags) && vshopInfo.Tags.Split(',').Length > 4)
                throw new Mall.Core.InvalidPropertyException("最多只能有4个标签");

            bool exist = DbFactory.Default.Get<VShopInfo>().Where(item => item.ShopId == vshopInfo.ShopId).Exist();
            if (exist)
                throw new Mall.Core.InvalidPropertyException(string.Format("店铺{0}已经创建过微店", vshopInfo.ShopId));

            var shopInfo = ServiceProvider.Instance<IShopService>.Create.GetShop(vshopInfo.ShopId);
            vshopInfo.Name = shopInfo.ShopName;//使用店铺名称作为微店名称
            vshopInfo.CreateTime = DateTime.Now;
            vshopInfo.State = VShopInfo.VShopStates.Normal;

            string logo = vshopInfo.StrLogo, backgroundImage = vshopInfo.StrBackgroundImage;
            string wxlogo = vshopInfo.WXLogo;
            CopyImages(vshopInfo.ShopId, ref logo, ref backgroundImage, ref wxlogo);
            vshopInfo.StrLogo = logo;
            vshopInfo.StrBackgroundImage = backgroundImage;
            vshopInfo.WXLogo = wxlogo;
            vshopInfo.IsOpen = true;

            DbFactory.Default.Add(vshopInfo);
        }

        public void UpdateVShop(VShopInfo vshopInfo)
        {
            if (vshopInfo.Id <= 0)
                throw new Mall.Core.InvalidPropertyException("请传入合法的微店Id，微店Id必须大于0");
            if (string.IsNullOrWhiteSpace(vshopInfo.StrLogo))
                throw new Mall.Core.InvalidPropertyException("微店Logo不能为空");
            if (string.IsNullOrWhiteSpace(vshopInfo.WXLogo))
                throw new Mall.Core.InvalidPropertyException("微信Logo不能为空");
            if (string.IsNullOrWhiteSpace(vshopInfo.StrBackgroundImage))
                throw new Mall.Core.InvalidPropertyException("微店背景图片不能为空");
            //if (string.IsNullOrWhiteSpace(vshopInfo.Description))
            //{
            //    throw new Mall.Core.InvalidPropertyException("微店描述不能为空");
            //}
            //if (!string.IsNullOrWhiteSpace(vshopInfo.Tags) && vshopInfo.Tags.Split(';').Length > 4)
            //    throw new Mall.Core.InvalidPropertyException("最多只能有4个标签");

            if (!string.IsNullOrWhiteSpace(vshopInfo.Tags))
            {
                var tagArray = vshopInfo.Tags.Split(';');
                foreach (var tag in tagArray)
                {
                    if (tag.Length > 4)
                        throw new Mall.Core.InvalidPropertyException("每个标签限4个字");
                }
            }

            var oriVShop = GetVshopById(vshopInfo.Id);
            if (oriVShop.ShopId != vshopInfo.ShopId)
                throw new Mall.Core.InvalidPropertyException("修改微店信息时，不能变更所属店铺");

            oriVShop.HomePageTitle = vshopInfo.HomePageTitle;
            oriVShop.Tags = vshopInfo.Tags;
            oriVShop.Description = vshopInfo.Description;

            string logo = vshopInfo.StrLogo, backgroundImage = vshopInfo.StrBackgroundImage;
            string wxlogo = vshopInfo.WXLogo;
            CopyImages(vshopInfo.ShopId, ref logo, ref backgroundImage, ref wxlogo);
            oriVShop.StrLogo = logo;
            oriVShop.StrBackgroundImage = backgroundImage;
            oriVShop.WXLogo = wxlogo;
            oriVShop.IsOpen = vshopInfo.IsOpen;
            oriVShop.Name = !string.IsNullOrEmpty(vshopInfo.Name) ? vshopInfo.Name : oriVShop.Name;

            DbFactory.Default.Update(oriVShop);
        }

        void CopyImages(long shopId, ref string logo, ref string backgroundImage, ref string wxlogo)
        {
            string newDir = string.Format("/Storage/Shop/{0}/VShop/", shopId);

            if (!string.IsNullOrWhiteSpace(logo))
            {
                if (logo.Contains("/temp/"))
                {
                    string logoname = logo.Substring(logo.LastIndexOf('/') + 1);
                    string oldlogo = logo.Substring(logo.LastIndexOf("/temp"));
                    string newLogo = newDir + logoname;
                    Core.MallIO.CopyFile(oldlogo, newLogo, true);
                    logo = newLogo;
                }
                else if (logo.Contains("/Storage/"))
                {
                    logo = logo.Substring(logo.LastIndexOf("/Storage"));
                }
            }

            if (!string.IsNullOrWhiteSpace(backgroundImage))
            {
                if (backgroundImage.Contains("/temp/"))
                {
                    string logoname = backgroundImage.Substring(backgroundImage.LastIndexOf('/') + 1);
                    string oldpic = backgroundImage.Substring(backgroundImage.LastIndexOf("/temp"));
                    string newfile = newDir + logoname;
                    Core.MallIO.CopyFile(oldpic, newfile, true);
                    backgroundImage = newfile;
                }
                else if (backgroundImage.Contains("/Storage/"))
                {
                    backgroundImage = backgroundImage.Substring(logo.LastIndexOf("/Storage"));
                }
            }

            if (!string.IsNullOrWhiteSpace(wxlogo))
            {
                if (wxlogo.Contains("/temp/"))
                {
                    string logoname = wxlogo.Substring(wxlogo.LastIndexOf('/') + 1);
                    string logofilepername = logoname.Substring(0, logoname.LastIndexOf('.'));
                    string oldpic = wxlogo.Substring(wxlogo.LastIndexOf("/temp"));
                    string newfile = newDir + logofilepername + ".png";
                    //Core.MallIO.CreateThumbnail(oldpic, newfile, 100, 100);
                    Core.MallIO.CopyFile(oldpic, newfile, true);
                    wxlogo = newfile;
                }
                else if (wxlogo.Contains("/Storage/"))
                {
                    wxlogo = wxlogo.Substring(wxlogo.LastIndexOf("/Storage"));
                }
            }

        }
        /// <summary>
        /// 增加访问统计
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int LogVisit(long id)
        {
            int result = 0;
            var vshopInfo = GetVshopById(id);
            vshopInfo.VisitNum += 1;
            DbFactory.Default.Update(vshopInfo);
            result = vshopInfo.VisitNum;
            return result;
        }

        public List<VShopInfo> GetVShops()
        {
            return DbFactory.Default.Get<VShopInfo>().ToList();
        }

        /// <summary>
        /// 根据商家id获取微店信息
        /// </summary>
        /// <param name="shopIds"></param>
        /// <returns></returns>
        public List<VShopInfo> GetVShopsByShopIds(IEnumerable<long> shopIds)
        {
            return DbFactory.Default.Get<VShopInfo>().Where(p => p.ShopId.ExIn(shopIds)).ToList();
        }

        public List<VShopInfo> GetVShops(int page, int pageSize, out int total)
        {
            var rets = DbFactory.Default.Get<VShopInfo>().OrderByDescending(item => item.Id).ToPagedList(page, pageSize);
            total = rets.TotalRecordCount;
            return rets;
        }

        public List<VShopInfo> GetVShops(int page, int pageSize, out int total, VShopInfo.VShopStates state, bool? IsOpenVshop)
        {
            var vshop = DbFactory.Default.Get<VShopInfo>().Where(item => item.State == state);
            if (IsOpenVshop.HasValue)
            {
                vshop = vshop.Where(d => d.IsOpen == IsOpenVshop);
            }
            var rets = vshop.OrderByDescending(item => item.Id).ToPagedList(page, pageSize);
            total = rets.TotalRecordCount;
            return rets;
        }

        public VShopInfo GetVShop(long id)
        {
            return DbFactory.Default.Get<VShopInfo>().Where(item => item.Id == id).FirstOrDefault();
        }

        public List<VShopInfo> GetHotShops(int page, int pageSize, out int total)
        {
            var vshopex = DbFactory.Default
                        .Get<VShopExtendInfo>()
                        .Where<VShopInfo>((vsei, vsi) => vsei.VShopId == vsi.Id && vsei.Type == VShopExtendInfo.VShopExtendType.HotVShop && vsi.IsOpen == true)
                        .Select(n => n.VShopId);
                       
            var sequencesql = DbFactory.Default
                .Get<VShopExtendInfo>()
                .Where<VShopInfo>((vsei, vsi) => vsei.VShopId == vsi.Id)
                .Select(n => n.Sequence.ExIfNull(0))
                .Take(1);
             
            var vshops = DbFactory.Default
                .Get<VShopInfo>()
                .Where(n => n.State == VShopInfo.VShopStates.Normal && n.Id.ExIn(vshopex))
                .Select()
                .Select(n => new { Sequence = sequencesql.ExResolve<int>() });

            var rets = vshops.OrderBy(d => "Sequence").OrderByDescending(item => item.CreateTime).ToPagedList(page, pageSize);
            total = rets.TotalRecordCount;
            return rets;
        }


        public void AddVisitNumber(long vshopId)
        {
            DbFactory.Default
                .Set<VShopInfo>()
                .Set(n => n.VisitNum, n => n.VisitNum + 1)
                .Where(n => n.Id == vshopId)
                .Succeed();
        }

        public void AddBuyNumber(long vshopId)
        {
            DbFactory.Default
                .Set<VShopInfo>()
                .Set(n => n.buyNum, n => n.buyNum + 1)
                .Where(n => n.Id == vshopId)
                .Succeed();
        }

        public List<VShopInfo> GetUserConcernVShops(long userId, int pageNo, int pageSize)
        {
            var favorite = DbFactory.Default.Get<FavoriteShopInfo>().Where(item => item.UserId == userId).Select(item => item.ShopId);
            var vshops = DbFactory.Default.Get<VShopInfo>().Where(item => item.State != VShopInfo.VShopStates.Close);
            var vshop = vshops.Where(item => item.ShopId.ExIn(favorite)).OrderByDescending(item => item.Id).ToPagedList(pageNo, pageSize);
            return vshop;
        }
        public int GetFavoriteShopCountByUserId(long userId) {
            return DbFactory.Default.Get<VShopInfo>()
                .LeftJoin<FavoriteShopInfo>((fs, s) => fs.ShopId == s.ShopId)
                .Where(p => p.State != VShopInfo.VShopStates.Close)
                .Where<FavoriteShopInfo>(p => p.UserId == userId)
                .Count();
        }

        public WXshopInfo GetVShopSetting(long shopId)
        {
            return DbFactory.Default.Get<WXshopInfo>().Where(item => item.ShopId == shopId).FirstOrDefault();
        }

        public void SaveVShopSetting(WXshopInfo wxShop)
        {
            if (GetVShopSetting(wxShop.ShopId) == null)
                AddVShopSetting(wxShop);
            else
                UpdateVShopSetting(wxShop);

        }

        void AddVShopSetting(WXshopInfo vshopSetting)
        {
            if (string.IsNullOrEmpty(vshopSetting.AppId))
                throw new Mall.Core.MallException("微信AppId不能为空！");
            if (string.IsNullOrEmpty(vshopSetting.AppSecret))
                throw new MallException("微信AppSecret不能为空！");
            DbFactory.Default.Add(vshopSetting);
        }

        void UpdateVShopSetting(WXshopInfo vshopSetting)
        {
            var wxShop = GetVShopSetting(vshopSetting.ShopId);
            if (string.IsNullOrEmpty(vshopSetting.AppId))
                throw new Mall.Core.MallException("微信AppId不能为空！");
            if (string.IsNullOrEmpty(vshopSetting.AppSecret))
                throw new MallException("微信AppSecret不能为空！");
            wxShop.ShopId = vshopSetting.ShopId;
            wxShop.AppId = vshopSetting.AppId;
            wxShop.AppSecret = vshopSetting.AppSecret;
            wxShop.FollowUrl = vshopSetting.FollowUrl;
            DbFactory.Default.Update(wxShop);

        }
        /// <summary>
        /// 店铺要显示的优惠卷
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public List<CouponSettingInfo> GetVShopCouponSetting(long shopid)
        {
            //var couponIdList = DbFactory.Default.Get<CouponInfo>().Where(item => item.ShopId == shopid).Select(item => item.Id).Continue();
            var couponSetList = DbFactory.Default
                .Get<CouponSettingInfo>()
                .LeftJoin<CouponInfo>((csi, ci) => csi.CouponID == ci.Id && ci.ShopId == shopid)//过滤店铺
                .Where<CouponInfo>(item => item.EndTime > DateTime.Now)
                .Where(item => (item.Display.ExIfNull(1) == 1))//过滤不显示的
                .ToList();
            return couponSetList;
        }

        public void SaveVShopCouponSetting(IEnumerable<CouponSettingInfo> infolist)
        {
            if (infolist == null)
            {
                throw new MallException("没有可更新的数据！");
            }
            DbFactory.Default.InTransaction(() =>
            {
                foreach (var el in infolist)
                {
                    var setinfo = DbFactory.Default.Get<CouponSettingInfo>().Where(item => item.CouponID == el.CouponID).FirstOrDefault();
                    if (setinfo != null)
                    {
                        setinfo.Display = el.Display;
                        DbFactory.Default.Update(setinfo);
                    }
                    else
                    {
                        if (el.Display == 1)
                        {//Disply=1为显示，需增加配置信息
                            DbFactory.Default.Add(new CouponSettingInfo()
                            {
                                CouponID = el.CouponID,
                                Display = el.Display,
                                PlatForm = PlatformType.Mobile
                            });
                        }
                    }
                }
            });
        }

        public string GetVShopLog(long vshopid)
        {
            //TODO LRL 判断没有开微店的情况
            //var vshop = Context.VShopInfo.Where(p => p.Id == vshopid);
            //if (null == vshop || vshop.Count() == 0)
            //{
            //    return "";
            //}
            //else
            //{
            var ret = DbFactory.Default.Get<VShopInfo>().Where(p => p.Id == vshopid).Select(p => p.WXLogo).FirstOrDefault<string>();
            if (string.IsNullOrEmpty(ret)) return "";
            return ret;
        }
    }
}