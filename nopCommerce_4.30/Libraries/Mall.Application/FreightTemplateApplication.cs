using Mall.Core;
using Mall.DTO;
using Mall.Entities;
using Mall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mall.Application
{
    public class FreightTemplateApplication : BaseApplicaion<IFreightTemplateService>
    {
        public static List<FreightTemplate> GetShopFreightTemplate(long shopId)
        {
            var data = Service.GetShopFreightTemplate(shopId);
            var templates = data.Map<List<FreightTemplate>>();
            foreach (var template in templates)
            {
                var detail = Service.GetFreightAreaDetail(template.Id).Map<List<FreightAreaDetail>>();
                template.FreightArea = Service.GetFreightAreaContent(template.Id).Map<List<FreightArea>>();
                foreach (var area in template.FreightArea)
                    area.FreightAreaDetail = detail.Where(p => p.FreightAreaId == area.Id).Map<List<FreightAreaDetail>>();
            }
            return templates;
            //var list = AutoMapper.Mapper.Map<List<FreightTemplate>>(model);
            //return model.Map<List<FreightTemplate>>();
        }

        /// <summary>
        /// 根据模板ID获取运费模板信息
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        [Obsolete]
        public static FreightTemplate GetFreightTemplate(long templateId, long shopId)
        {
            var model = Service.GetFreightTemplate(templateId);
            if (model.ShopID != shopId)
            {
                throw new MallException("此运费模板不存在");
            }

            var freeregions = Service.GetShippingFreeRegions(templateId);

            //     var freightTemplate = AutoMapper.Mapper.Map<FreightTemplate>(model);
            FreightTemplate freightTemplate = model.Map<FreightTemplate>();

            //     freightTemplate.FreightArea = AutoMapper.Mapper.Map<List<FreightArea>>(model.FreightAreaContentInfo);

            freightTemplate.FreightArea = model.FreightAreaContentInfo.Map<List<FreightArea>>();


            foreach (var area in freightTemplate.FreightArea)
            {
                //   area.FreightAreaDetail = AutoMapper.Mapper.Map<List<FreightAreaDetail>>(model.FreightAreaContentInfo.Where(a => a.Id == area.Id).FirstOrDefault().FreightAreaDetailInfo);
                area.FreightAreaDetail = model.FreightAreaContentInfo.Where(a => a.Id == area.Id).FirstOrDefault().FreightAreaDetailInfo.Map<List<FreightAreaDetail>>();
            
            }
            freightTemplate.FreeTempContent = new List<FreeTempContent>();
            var freeGroups = Service.GetShippingFreeGroups(model.Id);
            var freeRegions = Service.GetShippingFreeRegions(model.Id);
            freeGroups.ForEach(item =>
            {
                var info = new FreeTempContent()
                {
                    ConditionNumber = item.ConditionNumber,
                    ConditionType = item.ConditionType
                };
                if (!string.IsNullOrWhiteSpace(info.ConditionNumber))
                {
                    var condition = info.ConditionNumber.Split('$');
                    if (condition.Length == 2)
                    {
                        info.ConditionNumber1 = condition[0];
                        info.ConditionNumber2 = condition[1];
                    }
                    else if (condition.Length == 1)
                    {
                        info.ConditionNumber1 = condition[0];
                    }
                }
                info.FreightAreaDetail = new List<FreightAreaDetail>();
                freeRegions.Where(p => p.GroupId == item.Id).ToList().ForEach(a =>
                {
                    info.FreightAreaDetail.Add(new FreightAreaDetail()
                    {
                        RegionPath = a.RegionPath
                    });
                });
                freightTemplate.FreeTempContent.Add(info);
            });
            return freightTemplate;
        }

        public static FreightTemplateInfo GetFreightTemplate(long id)
        {
            return Service.GetFreightTemplate(id);
        }

        public static FreightTemplate GetFreightAreaDetail(long templateId, long shopId)
        {
            var model = Service.GetFreightTemplate(templateId);
            if (model.ShopID != shopId)
            {
                throw new MallException("此运费模板不存在");
            }

            //   var freightTemplate = AutoMapper.Mapper.Map<FreightTemplate>(model);
            var freightTemplate = model.Map<FreightTemplate>();


            // freightTemplate.FreightArea = AutoMapper.Mapper.Map<List<FreightArea>>(model.FreightAreaContentInfo);
            freightTemplate.FreightArea = model.FreightAreaContentInfo.Map<List<FreightArea>>();


            foreach (var m in freightTemplate.FreightArea)
            {
                //       m.FreightAreaDetail = AutoMapper.Mapper.Map<List<FreightAreaDetail>>(model.FreightAreaContentInfo.Where(a => a.Id == m.Id).FirstOrDefault().FreightAreaDetailInfo);

                m.FreightAreaDetail = model.FreightAreaContentInfo.Where(a => a.Id == m.Id).FirstOrDefault().FreightAreaDetailInfo.Map<List<FreightAreaDetail>>();


            }
            freightTemplate.FreeTempContent = new List<FreeTempContent>();
            var freeGroups = Service.GetShippingFreeGroups(model.Id);
            var freeRegions = Service.GetShippingFreeRegions(model.Id);
            freeGroups.ForEach(item =>
            {
                var info = new FreeTempContent()
                {
                    ConditionNumber = item.ConditionNumber,
                    ConditionType = item.ConditionType
                };
                if (!string.IsNullOrWhiteSpace(info.ConditionNumber))
                {
                    var condition = info.ConditionNumber.Split('$');
                    if (condition.Length == 2)
                    {
                        info.ConditionNumber1 = condition[0];
                        info.ConditionNumber2 = condition[1];
                    }
                    else if (condition.Length == 1)
                    {
                        info.ConditionNumber1 = condition[0];
                    }
                }
                info.FreightAreaDetail = freeRegions
                                        .Where(p => p.GroupId == item.Id)
                                        .Select(a => new FreightAreaDetail() { RegionPath = a.RegionPath })
                                        .ToList();

                freightTemplate.FreeTempContent.Add(info);
            });
            var area = freightTemplate.FreightArea.Where(a => a.IsDefault == 0).ToList();
            foreach (var f in area)
            {
                foreach (var a in f.FreightAreaDetail)
                {
                    a.ProvinceName = RegionApplication.GetRegion(a.ProvinceId).Name;
                    if (a.CityId.HasValue && a.CityId.Value != 0)
                        a.CityName = RegionApplication.GetRegion(a.CityId.Value).Name;
                    if (a.CountyId.HasValue && a.CountyId.Value != 0)
                        a.CountyName = RegionApplication.GetRegion(a.CountyId.Value).Name;
                    if (!string.IsNullOrEmpty(a.TownIds))
                    {
                        var regionNames = RegionApplication.GetAllRegions().Where(x => a.TownIds.Split(',').ToList().Contains(x.Id.ToString())).Select(t => t.Name).ToList();
                        a.TownNames = string.Join(",", regionNames);
                    }
                }
            }

            foreach (var item in freightTemplate.FreeTempContent)
            {
                Parallel.ForEach(item.FreightAreaDetail, a =>
                {
                    var path = a.RegionPath;
                    var arr = path.Split(',');
                    if (arr.Length >= 3)
                    {
                        a.ProvinceId = int.Parse(arr[0]);
                        a.CityId = int.Parse(arr[1]);
                        a.CountyId = int.Parse(arr[2]);

                        a.ProvinceName = RegionApplication.GetRegion(a.ProvinceId).Name;
                        a.CityName = RegionApplication.GetRegion(a.CityId.Value).Name;
                        a.CountyName = RegionApplication.GetRegion(a.CountyId.Value).Name;
                    }
                    else if (arr.Length >= 2)
                    {
                        a.ProvinceId = int.Parse(arr[0]);
                        a.CityId = int.Parse(arr[1]);
                        a.ProvinceName = RegionApplication.GetRegion(a.ProvinceId).Name;
                        a.CityName = RegionApplication.GetRegion(a.CityId.Value).Name;
                    }
                    else if (arr.Length >= 1)
                    {
                        a.ProvinceId = int.Parse(arr[0]);
                        a.ProvinceName = RegionApplication.GetRegion(a.ProvinceId).Name;
                    }
                });
            }
            return freightTemplate;
        }
        /// <summary>
        /// 复制运费模板
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="shopId"></param>
        public static void CopyFreightTemplate(long templateId, long shopId)
        {
            var freight = GetFreightTemplate(templateId, shopId);
            freight.Name = freight.Name + DateTime.Now.ToString("yyyyMMddHHmmss");
            freight.Id = 0;
            AddOrUpdateFreightTemplate(freight);
        }

        /// <summary>
        /// 添加修改运费模板
        /// </summary>
        /// <param name="templateInfo"></param>
        public static void AddOrUpdateFreightTemplate(FreightTemplate templateInfo)
        {
            //   var freightTemplate = AutoMapper.Mapper.Map<FreightTemplateInfo>(templateInfo);
            var freightTemplate = templateInfo.Map<FreightTemplateInfo>();


            freightTemplate.ShippingFreeGroupInfo = new List<ShippingFreeGroupInfo>();
            if (templateInfo.FreeTempContent != null)
            {
                templateInfo.FreeTempContent.ForEach(p =>
                {
                    var info = new Mall.Entities.ShippingFreeGroupInfo()
                    {
                        ConditionNumber = p.ConditionNumber,
                        ConditionType = p.ConditionType
                    };
                    info.ShippingFreeRegionInfo = new List<Mall.Entities.ShippingFreeRegionInfo>();
                    p.FreightAreaDetail.ForEach(a =>
                    {
                        info.ShippingFreeRegionInfo.Add(new Mall.Entities.ShippingFreeRegionInfo()
                        {
                            RegionId = a.RegionId,//保留最后一级地址ID
                            RegionPath = a.RegionPath//保留地址全路径方便查询
                        });
                    });
                    freightTemplate.ShippingFreeGroupInfo.Add(info);
                });
            }
            Service.UpdateFreightTemplate(freightTemplate);
        }
        /// <summary>
        /// 删除运费模板
        /// </summary>
        /// <param name="templateId"></param>
        public static void DeleteFreightTemplate(long templateId, long shopId)
        {
            bool used = Service.IsProductUseFreightTemp(templateId);
            if (used)
            {
                throw new MallException("此运费模板已使用，不能删除");
            }
            var templateInfo = Service.GetFreightTemplate(templateId);
            if (templateInfo.ShopID != shopId)
            {
                throw new MallException("此运费模板不存在");
            }
            Service.DeleteFreightTemplate(templateId);
        }

        /// <summary>
        /// 是否有商品使用过改运费模板
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        public static bool IsProductUseFreightTemp(long templateId)
        {
            return Service.IsProductUseFreightTemp(templateId);
        }

        /// <summary>
        /// 用户当前商品运费显示文本
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <param name="freightTemplate">运费末班</param>
        /// <param name="member">访问的用户</param>
        /// <param name="product">商品实体</param>
        /// <param name="buynumber">购买件数</param>
        /// <returns></returns>
        public static string GetFreightStr(long productId, FreightTemplateInfo freightTemplate, MemberInfo member = null, ProductInfo product = null, int buynumber = 1)
        {
            string freightStr = "免运费";
            if (freightTemplate == null || freightTemplate.IsFree == CommonModel.FreightTemplateType.Free)
                return freightStr;

            IProductService productService = ServiceApplication.Create<IProductService>();
            #region 是否虚拟商品，如是虚拟商品免运费
            if (product == null)
            {
                product = productService.GetProduct(productId);
                if (product == null || product.ProductType == 1)//虚拟商品无运费
                    return freightStr;
            }
            #endregion

            int addressId = 0;
            if (member != null)
            {
                var addressInfo = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(member.Id);
                if (addressInfo != null)
                {
                    addressId = addressInfo.RegionId;
                }
            }

            #region 当前是否是指定地区包邮
            if (addressId > 0)
            {
                bool isFree = false;//当前地区是否为自定包邮
                decimal price = 0;//购买商品的金额

                var freeRegions = Service.GetShippingFreeRegions(freightTemplate.Id);
                if (freeRegions != null && freeRegions.Count() > 0)
                {
                    IRegionService regionService = ServiceProvider.Instance<IRegionService>.Create;
                    freeRegions.ForEach(p =>
                    {
                        p.RegionSubList = regionService.GetSubsNew(p.RegionId, true).Select(a => a.Id).ToList();
                    });
                    var regions = freeRegions.Where(p => p.RegionSubList.Contains(addressId));//根据模板设置的包邮地区过滤出当前配送地址所在地址
                    if (regions != null && regions.Count() > 0)
                    {
                        var groupIds = regions.Select(p => p.GroupId).ToList();
                        var freeGroups = Service.GetShippingFreeGroupInfos(freightTemplate.Id, groupIds);

                        //只要有一个符合包邮，则退出
                        decimal amount = price * buynumber;
                        freeGroups.ForEach(p =>
                        {
                            if (p.ConditionType == 1)//购买件数
                            {
                                if (buynumber >= int.Parse(p.ConditionNumber))
                                {
                                    isFree = true;
                                    return;
                                }
                            }
                            else if (p.ConditionType == 2)//金额
                            {
                                if (amount >= decimal.Parse(p.ConditionNumber))
                                {
                                    isFree = true;
                                    return;
                                }
                            }
                            else if (p.ConditionType == 3)//件数+金额
                            {
                                var condition1 = int.Parse(p.ConditionNumber.Split('$')[0]);
                                var condition2 = decimal.Parse(p.ConditionNumber.Split('$')[1]);
                                if (buynumber >= condition1 && amount >= condition2)
                                {
                                    isFree = true;
                                    return;
                                }
                            }
                        });
                    }
                }

                if (isFree)
                    return freightStr;//直接返回免运费

            }
            #endregion

            decimal freight = productService.GetFreight(new List<long>() { productId }, new List<int>() { 1 }, addressId, true);
            freightStr = freight <= 0 ? "免运费" : string.Format("运费 {0}元", freight.ToString("f2"));

            return freightStr;
        }
        
        /// <summary>
        /// 获取运费模板列表
        /// </summary>
        /// <param name="templateIds">id集合</param>
        /// <returns></returns>
        public static List<FreightTemplateInfo> GetFreightTemplateList(List<long> templateIds)
        {
            return Service.GetFreightTemplateList(templateIds);
        }
    }
}
