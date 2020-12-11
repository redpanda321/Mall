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
    public class BrandService : ServiceBase, IBrandService
    {
        public void AddBrand(BrandInfo model)
        {
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Add(model);
                //转移图片
                model.Logo = MoveImages(model.Id, model.Logo);
                DbFactory.Default.Update(model);
            });
        }

        public void UpdateBrand(BrandInfo model)
        {
            model.Logo = MoveImages(model.Id, model.Logo);
            BrandInfo brand = GetBrand(model.Id);
            brand.Name = model.Name.Trim();
            brand.Description = model.Description;
            brand.Logo = model.Logo;
            brand.Meta_Description = model.Meta_Description;
            brand.Meta_Keywords = model.Meta_Keywords;
            brand.Meta_Title = model.Meta_Title;
            brand.RewriteName = model.RewriteName;
            brand.IsRecommend = model.IsRecommend;

            ShopBrandApplyInfo info = DbFactory.Default.Get<ShopBrandApplyInfo>().Where(p => p.BrandId == model.Id).FirstOrDefault();
            if (info != null)
            {
                info.BrandName = model.Name;
                info.Description = model.Description;
                info.Logo = model.Logo;
            }
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Update(brand);
                if (info != null)
                    DbFactory.Default.Update(info);
            });
        }

        public void DeleteBrand(long id)
        {
            //this.Context.BrandInfo.Where(p => p.Id == id).Update(p => new BrandInfo { IsDeleted = true });
            DbFactory.Default.InTransaction(() =>
            {
                var brand = DbFactory.Default.Get<BrandInfo>().Where(p => p.Id == id).FirstOrDefault();
                if (brand != null)
                {
                    brand.IsDeleted = true;
                    DbFactory.Default.Update(brand);
                    var apply = DbFactory.Default.Get<ShopBrandApplyInfo>().Where(item => item.BrandName.ToLower().Trim() == brand.Name.ToLower().Trim()).ToList();
                    if (apply.Any())
                    {
                        DbFactory.Default.Del<ShopBrandApplyInfo>(apply);
                        foreach (var shopbrand in apply)
                        {
                            //删除对应图片
                            if (shopbrand.ApplyMode == (int)ShopBrandApplyInfo.BrandApplyMode.New)
                            {
                                try
                                {
                                    string logosrc = shopbrand.Logo;
                                    if (Core.MallIO.ExistFile(logosrc))
                                        Core.MallIO.DeleteFile(logosrc);
                                }
                                catch (Exception ex)
                                {
                                    Log.Info("删除图片失败" + ex.Message);
                                }
                            }
                            string imgsrc = shopbrand.AuthCertificate;
                            if (!string.IsNullOrWhiteSpace(imgsrc))
                            {
                                try
                                {
                                    if (Core.MallIO.ExistFile(imgsrc))
                                        Core.MallIO.DeleteFile(imgsrc);
                                }
                                catch (Exception ex)
                                {
                                    Log.Info("删除图片失败" + ex.Message);
                                }
                            }
                        }
                    }
                }
            });
        }

        public QueryPageModel<BrandInfo> GetBrands(string keyWords, int pageNo, int pageSize)
        {
            var brands = DbFactory.Default.Get<BrandInfo>().Where(p => p.IsDeleted == false);
            if (!string.IsNullOrWhiteSpace(keyWords))
                brands = brands.Where(p => p.Name.Contains(keyWords));
            var data = brands.OrderByDescending(pp => pp.Id).ToPagedList(pageNo, pageSize);

            var pageModel = new QueryPageModel<BrandInfo>()
            {
                Models = data,
                Total = data.TotalRecordCount
            };
            return pageModel;
        }

        public void AuditBrand(long id, ShopBrandApplyInfo.BrandAuditStatus status, string remark)
        {
            DbFactory.Default.InTransaction(() =>
            {
                var m = DbFactory.Default.Get<ShopBrandApplyInfo>().Where(p => p.Id == id).FirstOrDefault();
                m.AuditStatus = (int)status;
                m.PlatRemark = remark;
                if (status == ShopBrandApplyInfo.BrandAuditStatus.Audited) //审核通过
                {
                    if (m.ApplyMode == (int)ShopBrandApplyInfo.BrandApplyMode.New) //申请的是新品牌
                    {
                        var model = DbFactory.Default.Get<BrandInfo>().Where(r => r.Name.ToLower() == m.BrandName.ToLower() && r.IsDeleted == false).FirstOrDefault();
                        if (model == null) //是否已存在该品牌
                        {
                            //向品牌表里加入一条数据
                            BrandInfo brand = new BrandInfo()
                            {
                                Name = m.BrandName.Trim(),
                                Logo = m.Logo,
                                Description = m.Description
                            };

                            DbFactory.Default.Add(brand);

                            //关联申请表与品牌表的联系
                            m.BrandId = brand.Id;
                            DbFactory.Default.Update(m);

                            BrandInfo b = GetBrand(brand.Id);
                            b.Logo = MoveImages(b.Id, b.Logo, 1);
                            DbFactory.Default.Update(b);

                            //向商家品牌表加入一条数据 
                            ShopBrandInfo info = new ShopBrandInfo();
                            info.BrandId = b.Id;
                            info.ShopId = m.ShopId;

                            DbFactory.Default.Add(info);
                        }
                        else
                        {
                            //向商家品牌表加入一条数据 
                            ShopBrandInfo info = new ShopBrandInfo();
                            info.BrandId = model.Id;
                            info.ShopId = m.ShopId;
                            DbFactory.Default.Update(m);
                            DbFactory.Default.Add(info);
                        }
                    }
                    else
                    {
                        //向商家品牌表加入一条数据 
                        ShopBrandInfo info = new ShopBrandInfo();
                        info.BrandId = (long)m.BrandId;
                        info.ShopId = m.ShopId;
                        DbFactory.Default.Update(m);
                        DbFactory.Default.Add(info);
                    }
                }
                else
                {
                    DbFactory.Default.Update(m);
                }

            });
        }

        public List<BrandInfo> GetBrands(string keyWords, long shopId = 0, string action = "add")
        {
            var result = DbFactory.Default.Get<BrandInfo>().Where(p => p.IsDeleted == false);

            #region TDO:ZYF 商家新增品牌申请时，过滤有审核记录的品牌
            if (shopId > 0 && action.Equals("add"))
            {
                var brandids = DbFactory.Default.Get<ShopBrandApplyInfo>().Where(p => p.ShopId == shopId && p.BrandId.ExIsNotNull()).Select(b => b.BrandId).ToList<long>();
                if (brandids.Any())
                {
                    result.Where(p => p.Id.ExNotIn(brandids));
                }
            }
            #endregion

            if (!string.IsNullOrWhiteSpace(keyWords))
                result.Where(p => p.Name.Contains(keyWords) || p.RewriteName.Contains(keyWords));
            return result.ToList();
        }

        /// <summary>
        /// 根据品牌id获取品牌
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<BrandInfo> GetBrandsByIds(IEnumerable<long> ids)
        {
            return DbFactory.Default.Get<BrandInfo>().Where(p => p.Id.ExIn(ids) && p.IsDeleted == false).ToList();
        }

        public BrandInfo GetBrand(long id)
        {
            return DbFactory.Default.Get<BrandInfo>().Where(p => p.Id == id && p.IsDeleted == false).FirstOrDefault();
        }

        string MoveImages(long brandId, string image, int type = 0)
        {
            if (string.IsNullOrEmpty(image))
            {
                return "";
            }
            var ext = image.Substring(image.LastIndexOf("."));
            string relativeDir = "/Storage/Plat/Brand/";
            string fileName = "logo_" + brandId + ext;
            if (image.Contains("/temp/"))
            {
                string path = image.Substring(image.LastIndexOf("/temp"));
                Core.MallIO.CopyFile(path, relativeDir + fileName, true);
            }
            else if (type == 1)
            {
                string path = image;
                Core.MallIO.CopyFile(path, relativeDir + fileName, true);
            }
            return relativeDir + fileName;
        }

        string MoveImages(int id, long shopId, string image, string name, int index = 1)
        {
            if (string.IsNullOrEmpty(image))
            {
                return "";
            }
            var ext = ".png";
            string ImageDir = string.Empty;

            var path = "/Storage/Shop/" + shopId + "/Brand/";
            var fileName = name + "_" + id + "_" + index + ext;
            if (image.Contains("/temp/"))
            {
                string temp = image.Substring(image.LastIndexOf("/temp"));
                Core.MallIO.CopyFile(temp, path + fileName, true);
            }
            return path + fileName;
        }

        public List<BrandInfo> GetBrandsByCategoryIds(params long[] categoryIds)
        {
            var typeIds = DbFactory.Default
                .Get<CategoryInfo>()
                .InnerJoin<TypeInfo>((ci, ti) => ci.TypeId == ti.Id)
                .Where(p => p.Id.ExIn(categoryIds) && p.IsDeleted == false)
                .Where<TypeInfo>(p => p.IsDeleted == false)
                .Select(p => p.TypeId)
                .ToList<long>();
            var brandIds = DbFactory.Default.Get<TypeBrandInfo>().Where(p => p.TypeId.ExIn(typeIds)).Select(p => p.BrandId).ToList<long>();
            var select = DbFactory.Default.Get<BrandInfo>().Where(p => p.Id.ExIn(brandIds) && p.IsDeleted == false);
            var result = select.ToList();

            return result;
        }

        public List<BrandInfo> GetBrandsByCategoryIds(long shopId, params long[] categoryIds)
        {
            var isSelfShop = DbFactory.Default.Get<ShopInfo>().Where(p => p.Id == shopId && p.IsSelf == true).Exist();

            var typeIds = DbFactory.Default.Get<CategoryInfo>().Where(p => p.Id.ExIn(categoryIds) && p.IsDeleted == false).InnerJoin<TypeInfo>((a, b) => a.TypeId == b.Id && b.IsDeleted == false).Select(p => p.TypeId).ToList<long>();
            var typeBrands = DbFactory.Default.Get<TypeBrandInfo>().Where(p => p.TypeId.ExIn(typeIds));

            if (!isSelfShop)//平台店查询所有的
            {
                var sbisql = DbFactory.Default
                    .Get<ShopBrandInfo>()
                    .Where<TypeBrandInfo>((sbi, tbi) => sbi.BrandId == tbi.BrandId && sbi.ShopId == shopId)
                    .Select(n => n.Id);
                typeBrands.Where(p => p.ExExists(sbisql));
            }
            var brandIds = typeBrands.Select(p => p.BrandId).ToList<long>();
            var select = DbFactory.Default.Get<BrandInfo>().Where(p => p.Id.ExIn(brandIds) && p.IsDeleted == false);
            var sql = select.ToString();
            var result = select.ToList();

            return result;
        }

        public void ApplyBrand(ShopBrandApplyInfo model)
        {
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Add(model);

                //移动品牌Logo
                if (model.ApplyMode == 2)
                {
                    model.Logo = MoveImages(model.Id, model.ShopId, model.Logo, "logo", 1);
                }
                //移动品牌授权证书
                var pics = model.AuthCertificate;
                string newpics = string.Empty;
                if (!string.IsNullOrEmpty(pics))
                {
                    var arr = pics.Split(',');
                    var index = 0;
                    foreach (var image in arr)
                    {
                        index++;
                        newpics += MoveImages(model.Id, model.ShopId, image, "auth", index) + ",";
                    }
                }
                if (!string.IsNullOrEmpty(newpics))
                {
                    model.AuthCertificate = newpics.TrimEnd(',');
                }

                DbFactory.Default.Update(model);
            });
        }
        public void UpdateApplyBrand(ShopBrandApplyInfo model)
        {
            var data = DbFactory.Default.Get<ShopBrandApplyInfo>().Where(d => d.Id == model.Id).FirstOrDefault();
            if (data == null)
            {
                throw new MallException("错误的参数");
            }
            //移动品牌Logo
            if (model.ApplyMode == 2)
            {
                model.Logo = MoveImages(model.Id, model.ShopId, model.Logo, "logo", 1);
            }

            //移动品牌授权证书
            var pics = model.AuthCertificate;
            string newpics = string.Empty;
            if (!string.IsNullOrEmpty(pics))
            {
                var arr = pics.Split(',');
                var index = 0;
                foreach (var image in arr)
                {
                    index++;
                    newpics += MoveImages(model.Id, model.ShopId, image, "auth", index) + ",";
                }
            }
            if (!string.IsNullOrEmpty(newpics))
            {
                model.AuthCertificate = newpics.TrimEnd(',');
            }
            data.AuditStatus = model.AuditStatus;
            data.AuthCertificate = model.AuthCertificate;
            data.Description = model.Description;
            data.BrandName = model.BrandName;
            data.Logo = model.Logo;
            DbFactory.Default.Update(data);
        }

        public bool IsExistApply(long shopId, string brandName)
        {
            return DbFactory.Default.Get<ShopBrandApplyInfo>().Where(item => item.ShopId == shopId && item.BrandName.ToLower().Trim() == brandName.ToLower().Trim() && item.AuditStatus != (int)ShopBrandApplyInfo.BrandAuditStatus.Refused).Exist();
        }

        public ShopBrandApplyInfo GetExistApply(long shopId, string brandName)
        {
            var apply = DbFactory.Default.Get<ShopBrandApplyInfo>().Where(item => item.ShopId == shopId && item.BrandName.ToLower().Trim() == brandName.ToLower().Trim()).FirstOrDefault();
            if (apply != null)
                return apply;
            else
                return null;
        }

        public bool IsExistBrand(string brandName)
        {
            return DbFactory.Default.Get<BrandInfo>().Where(item => item.Name.Trim() == brandName.ToLower().Trim() && item.IsDeleted == false).Exist();
        }

        public void UpdateSellerBrand(ShopBrandApplyInfo model)
        {
            var m = DbFactory.Default.Get<ShopBrandApplyInfo>().Where(a => a.Id == model.Id && a.ShopId != 0 && a.AuditStatus == (int)ShopBrandApplyInfo.BrandAuditStatus.UnAudit).FirstOrDefault();
            if (m == null)
                throw new Mall.Core.MallException("该品牌已被审核或删除，不能修改！");
            m.Logo = MoveImages(model.Id, model.Logo);
            m.BrandName = model.BrandName;
            m.Description = model.Description;
            DbFactory.Default.Update(m);
        }

        public QueryPageModel<BrandInfo> GetShopBrands(long shopId, int pageNo, int pageSize)
        {
            var shopBrandIds = DbFactory.Default.Get<ShopBrandInfo>().InnerJoin<BrandInfo>((s, b) => s.BrandId == b.Id && s.ShopId == shopId && b.IsDeleted == false).Select(a => a.BrandId).ToList<long>();
            var data = DbFactory.Default.Get<BrandInfo>().Where(a => a.Id.ExIn(shopBrandIds)).OrderByDescending(a => a.Id).ToPagedList(pageNo, pageSize);
            var pageModel = new QueryPageModel<BrandInfo>()
            {
                Models = data,
                Total = data.TotalRecordCount
            };
            return pageModel;
        }

        public List<BrandInfo> GetShopBrands(long shopId)
        {
            var shopBrandIds = DbFactory.Default.Get<ShopBrandInfo>().InnerJoin<BrandInfo>((s, b) => s.BrandId == b.Id && s.ShopId == shopId && b.IsDeleted == false).Select(a => a.BrandId).ToList<long>();
            var data = DbFactory.Default.Get<BrandInfo>().Where(a => a.Id.ExIn(shopBrandIds)).ToList();
            return data;
        }

        public QueryPageModel<ShopBrandApplyInfo> GetShopBrandApplys(long? shopId, int? auditStatus, int pageNo, int pageSize, string keyWords)
        {
            ShopBrandApplyInfo.BrandAuditStatus? status = null;
            var db = WhereBuilder(new BrandApplyQuery
            {
                Keywords = keyWords,
                ShopId = shopId,
                AuditStatus = auditStatus.HasValue ? (ShopBrandApplyInfo.BrandAuditStatus)auditStatus.Value : status,
                PageNo = pageNo,
                PageSize = pageSize
            });
            var data = db.OrderByDescending(o => o.Id).ToPagedList(pageNo, pageSize);
            return new QueryPageModel<ShopBrandApplyInfo>
            {
                Models = data,
                Total = data.TotalRecordCount
            };
        }

        public int GetShopBrandApplyCount(BrandApplyQuery query)
        {
            var db = WhereBuilder(query);
            return db.Count();
        }

        private GetBuilder<ShopBrandApplyInfo> WhereBuilder(BrandApplyQuery query)
        {
            var db = DbFactory.Default.Get<ShopBrandApplyInfo>();
            if (query.AuditStatus.HasValue)
            {
                var status = (int)query.AuditStatus.Value;
                db.Where(item => item.AuditStatus == status);
            }

            if (query.ShopId.HasValue)
            {
                db.Where(a => a.ShopId == query.ShopId.Value);

                var childSql = DbFactory.Default
                        .Get<BrandInfo>().Where<ShopBrandApplyInfo>((b, s) => s.ShopId == query.ShopId.Value && ((b.IsDeleted == false && s.BrandId == b.Id) || (s.BrandId.ExIfNull(0) == 0)));
                db.Where(item => item.ExExists(childSql));
            }
            if (!string.IsNullOrEmpty(query.Keywords))
            {
                var sbisql = DbFactory.Default
                        .Get<ShopInfo>()
                        .Where<ShopBrandApplyInfo>((s, b) => b.ShopId == s.Id && s.ShopName.Contains(query.Keywords))
                        .Select(n => n.Id);

                db.Where(item => item.ExExists(sbisql));
            }
            return db;
        }
        public ShopBrandApplyInfo GetShopBrandApply(string keyWords)
        {
            var brands = DbFactory.Default.Get<ShopBrandApplyInfo>();
            return brands.Where(item => item.AuditStatus == 1 && item.ApplyMode == 2 && item.BrandName.Equals(keyWords)).FirstOrDefault();
        }

        public List<ShopBrandApplyInfo> GetShopBrandApplys(long shopId)
        {
            List<ShopBrandApplyInfo> brands = DbFactory.Default.Get<ShopBrandApplyInfo>(item => item.ShopId == shopId).ToList();
            return brands;
        }

        public ShopBrandApplyInfo GetBrandApply(long id)
        {
            var model = DbFactory.Default.Get<ShopBrandApplyInfo>().Where(p => p.Id == id).FirstOrDefault();

            if (model.ApplyMode == (int)ShopBrandApplyInfo.BrandApplyMode.Exist)
            {
                var brand = DbFactory.Default.Get<BrandInfo>(a => a.Id == model.BrandId).FirstOrDefault();
                model.Description = brand.Description;
            }
            return model;
        }

        public void DeleteApply(int id)
        {
            var model = DbFactory.Default.Get<ShopBrandApplyInfo>(p => p.Id == id).FirstOrDefault();
            DbFactory.Default.Del(model);

            //删除对应图片
            if (model.ApplyMode == (int)ShopBrandApplyInfo.BrandApplyMode.New)
            {
                string logosrc = model.Logo;
                if (Core.MallIO.ExistFile(logosrc))
                    Core.MallIO.DeleteFile(logosrc);
            }
            string imgsrc = model.AuthCertificate;
            if (!string.IsNullOrWhiteSpace(imgsrc))
            {
                try
                {
                    if (Core.MallIO.ExistFile(imgsrc))
                        Core.MallIO.DeleteFile(imgsrc);
                }
                catch (Exception ex)
                {
                    Log.Info("删除图片失败" + ex.Message);
                }
            }
        }

        public bool BrandInUse(long id)
        {
            return DbFactory.Default.Get<ProductInfo>().Where(item => item.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && item.IsDeleted == false && item.BrandId.Equals(id)).Exist();
            //bool flag = Context.ProductInfo.Any(item => item.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && item.IsDeleted == false && item.BrandId.Equals(id));
            //return flag;
        }
    }
}

