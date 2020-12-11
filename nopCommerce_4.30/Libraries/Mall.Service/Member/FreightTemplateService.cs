using Mall.Core;
using Mall.IServices;
using System.Linq;
using System.Collections.Generic;
using Mall.CommonModel;
using NetRube.Data;
using Mall.Entities;

namespace Mall.Service
{
    public class FreightTemplateService : ServiceBase, IFreightTemplateService
    {
        public List<FreightTemplateInfo> GetShopFreightTemplate(long shop)
        {
            return DbFactory.Default.Get<FreightTemplateInfo>().Where(p => p.ShopID == shop).ToList();
        }

        public FreightTemplateInfo GetFreightTemplate(long TemplateId)
        {
            if (TemplateId <= 0)
                return null;

            string cacheKey = CacheKeyCollection.CACHE_FREIGHTTEMPLATE(TemplateId);
            if (Cache.Exists(cacheKey))
                return Cache.Get<FreightTemplateInfo>(cacheKey);

            var result = DbFactory.Default.Get<FreightTemplateInfo>().Where(e => e.Id == TemplateId).FirstOrDefault();
            if (result == null)
                return null;//当前运费模板不存直接返回null

            result.FreightAreaContentInfo = DbFactory.Default.Get<FreightAreaContentInfo>().Where(e => e.FreightTemplateId == result.Id).ToList();
            foreach(var area in result.FreightAreaContentInfo)
            {
                area.FreightAreaDetailInfo = DbFactory.Default.Get<FreightAreaDetailInfo>().Where(a => a.FreightAreaId == area.Id).ToList();
            }
            Cache.Insert(cacheKey, result, 1800);
            return result;
        }

        public List<FreightAreaDetailInfo> GetFreightAreaDetail(long TemplateId)
        {
            string cacheKey = CacheKeyCollection.CACHE_FREIGHTAREADETAIL(TemplateId);
            if (Cache.Exists(cacheKey))
                return Cache.Get<List<FreightAreaDetailInfo>>(cacheKey);
            var result = DbFactory.Default.Get<FreightAreaDetailInfo>().Where(a => a.FreightTemplateId == TemplateId).ToList();
            Cache.Insert(cacheKey, result, 1800);
            return result;
        }

        public List<FreightAreaContentInfo> GetFreightAreaContent(long TemplateId)
        {
            return DbFactory.Default.Get<FreightAreaContentInfo>().Where(e => e.FreightTemplateId == TemplateId).ToList();
        }

        public void UpdateFreightTemplate(FreightTemplateInfo templateInfo)
        {
            FreightTemplateInfo model;
            if (templateInfo.Id == 0)
            {
                DbFactory.Default.InTransaction(() =>
                {
                    var ret1 = DbFactory.Default.Add(templateInfo);
                   
                    foreach (var t in templateInfo.FreightAreaContentInfo)
                    {
                        t.FreightTemplateId = templateInfo.Id;
                    }

                    if (templateInfo.FreightAreaContentInfo.Count() > 0)
                    {
                        var ret2 = DbFactory.Default.Add<FreightAreaContentInfo>(templateInfo.FreightAreaContentInfo);
                    }

                    var areaDetailList = new List<FreightAreaDetailInfo>();
                    foreach (var t in templateInfo.FreightAreaContentInfo)
                    {
                        foreach (var d in t.FreightAreaDetailInfo)
                        {
                            d.FreightAreaId = t.Id;
                            d.FreightTemplateId = t.FreightTemplateId;
                            areaDetailList.Add(d);
                        }
                    }

                    if (areaDetailList.Count > 0)
                    {
                        var ret3 = DbFactory.Default.Add<FreightAreaDetailInfo>(areaDetailList);
                    }

                    #region 指定地区包邮
                    if (templateInfo.ShippingFreeGroupInfo != null)
                    {
                        foreach (var t in templateInfo.ShippingFreeGroupInfo)
                        {
                            t.TemplateId = templateInfo.Id;//模板ID
                                                       
                            DbFactory.Default.Add(t);
                            if (t.Id > 0)
                            {
                                foreach (var item in t.ShippingFreeRegionInfo)
                                {
                                    item.GroupId = t.Id;//组ID
                                    item.TemplateId = templateInfo.Id;//模板ID
                                    DbFactory.Default.Add(item);
                                }
                            }
                        }
                    }
                });
                #endregion
            }
            else
            {
                model = DbFactory.Default.Get<FreightTemplateInfo>().Where(e => e.Id == templateInfo.Id).FirstOrDefault();
                model.Name = templateInfo.Name;
                model.IsFree = templateInfo.IsFree;
                model.ValuationMethod = templateInfo.ValuationMethod;
                model.ShopID = templateInfo.ShopID;
                model.SourceAddress = templateInfo.SourceAddress;
                model.SendTime = templateInfo.SendTime;
                
           
                var flag = DbFactory.Default.InTransaction(() =>
                {
                    DbFactory.Default.Update(model);
                    //先删除
                    DbFactory.Default.Del<FreightAreaContentInfo>(e => e.FreightTemplateId == model.Id);
                    //删除详情表
                    DbFactory.Default.Del<FreightAreaDetailInfo>(e => e.FreightTemplateId == model.Id);

                    if (model.IsFree == FreightTemplateType.SelfDefine)
                    {
                        //重新插入地区运费
                        templateInfo.FreightAreaContentInfo.ForEach(e =>
                        {
                            e.FreightTemplateId = model.Id;
                        });

                        if(templateInfo.FreightAreaContentInfo.Count>0)
                        {
                            DbFactory.Default.Add<FreightAreaContentInfo>(templateInfo.FreightAreaContentInfo);
                        }
                     
                        var detailList = new List<FreightAreaDetailInfo>();
                        foreach (var t in templateInfo.FreightAreaContentInfo)
                        {
                            foreach (var d in t.FreightAreaDetailInfo)
                            {
                                d.FreightAreaId = t.Id;
                                d.FreightTemplateId = model.Id;
                                detailList.Add(d);
                            }
                        }
                        if(detailList.Count>0)
                        {
                            DbFactory.Default.Add<FreightAreaDetailInfo>(detailList);
                        }
                    }

                    #region 指定地区包邮
                    DbFactory.Default.Del<ShippingFreeGroupInfo>(e => e.TemplateId == model.Id);
                    DbFactory.Default.Del<ShippingFreeRegionInfo>(e => e.TemplateId == model.Id);

                    if (templateInfo.ShippingFreeGroupInfo != null)
                    {
                        foreach (var t in templateInfo.ShippingFreeGroupInfo)
                        {
                            t.TemplateId = model.Id;//模板ID
                            DbFactory.Default.Add(t);
                            if (t.Id > 0)
                            {
                                foreach (var item in t.ShippingFreeRegionInfo)
                                {
                                    item.GroupId = t.Id;//组ID
                                    item.TemplateId = model.Id;//模板ID

                                    DbFactory.Default.Add(item);
                                }
                            }
                        }
                    }
                    #endregion
                    return true;
                });
                Cache.Remove(CacheKeyCollection.CACHE_FREIGHTTEMPLATE(templateInfo.Id));
                Cache.Remove(CacheKeyCollection.CACHE_FREIGHTAREADETAIL(templateInfo.Id));
            }
        }


        public void DeleteFreightTemplate(long TemplateId)
        {
            var flag = DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Delete<FreightTemplateInfo>(TemplateId);
                DbFactory.Default.Del<FreightAreaContentInfo>(e => e.FreightTemplateId == TemplateId);
                DbFactory.Default.Del<FreightAreaDetailInfo>(e => e.FreightTemplateId == TemplateId);
                DbFactory.Default.Del<ShippingFreeGroupInfo>(e => e.TemplateId == TemplateId);
                DbFactory.Default.Del<ShippingFreeRegionInfo>(e => e.TemplateId == TemplateId);
                return true;
            });
        }

        /// <summary>
        /// 是否有商品使用过该运费模板
        /// </summary>
        /// <param name="TemplateId"></param>
        /// <returns></returns>
        public bool IsProductUseFreightTemp(long TemplateId)
        {
            return DbFactory.Default.Get<ProductInfo>().Where(item => item.FreightTemplateId == TemplateId && item.IsDeleted == false).Exist();
        }

        public List<ShippingFreeRegionInfo> GetShippingFreeRegions(long TemplateId)
        {
            return DbFactory.Default.Get<ShippingFreeRegionInfo>().Where(a => a.TemplateId == TemplateId).ToList();
        }

        public List<ShippingFreeGroupInfo> GetShippingFreeGroups(long templateId) {
            return DbFactory.Default.Get<ShippingFreeGroupInfo>().Where(p => p.TemplateId == templateId).ToList();
        }
        public List<ShippingFreeGroupInfo> GetShippingFreeGroupInfos(long TemplateId, List<long> groupIds)
        {
            var result = DbFactory.Default.Get<ShippingFreeGroupInfo>().Where(a => a.TemplateId == TemplateId);
            if (groupIds != null && groupIds.Count > 0)
            {
                result.Where(a => a.Id.ExIn(groupIds));
            }
            return result.ToList();
        }

        /// <summary>
        /// 获取运费模板列表
        /// </summary>
        /// <param name="templateIds">id集合</param>
        /// <returns></returns>
        public List<FreightTemplateInfo> GetFreightTemplateList(List<long> templateIds)
        {
            if (templateIds == null || templateIds.Count <= 0)
                return null;

            var result = DbFactory.Default.Get<FreightTemplateInfo>().Where(e => e.Id.ExIn(templateIds)).ToList();
            return result;
        }
    }
}
