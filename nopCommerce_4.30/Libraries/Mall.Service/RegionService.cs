
using IO.Swagger.Api;
using IO.Swagger.Model;
using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.IServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mall.Service
{
    public class RegionService : ServiceBase, IRegionService
    {
        const int REGION_CACHETIME = 600;//秒
        private IEnumerable<Region> regions;
        private string REGION_FILE_PATH = "/Scripts/region.json";
        private string REGION_BAK_PATH = "/Scripts/regionbak.json";


        private static IWebHostEnvironment _hostingEnvironment = EngineContext.Current.Resolve<IWebHostEnvironment>();


        private void GetFilePath(out string filePath, out string bakPath)
        {
            var isOpenJdRegion = IsOpenJdRegion();
            filePath = isOpenJdRegion ? "/Scripts/regionjd.json" : "/Scripts/region.json";
            bakPath = isOpenJdRegion ? "/Scripts/regionjdbak.json" : "/Scripts/regionbak.json";
        }

        /// <summary>
        /// 横向平铺的地区数据
        /// </summary>
        private IEnumerable<Region> RegionSource
        {
            get
            {
                if (regions == null)
                {
                    regions = Cache.GetLocal<IEnumerable<Region>>(CacheKeyCollection.Region);
                    if (regions == null)
                    {
                        #region 下载到本地
                        //从OSS上下载至本地(如果OSS上文件比本地文件修改时间更新时)
                        //MetaInfo metaRemoteInfo = null;
                        //if (Core.MallIO.IsNeedRefreshFile(REGION_FILE_PATH, out metaRemoteInfo))
                        //{
                        //    var metaLocalFile = GetFileMetaInfo(REGION_FILE_PATH);
                        //    if (CheckMetaInfo(metaRemoteInfo, metaLocalFile))
                        //    {
                        //        var dataFileBytes = Core.MallIO.DownloadTemplateFile(REGION_FILE_PATH);
                        //        if (null != dataFileBytes)
                        //        {
                        //            var strDataContent = Encoding.UTF8.GetString(dataFileBytes);
                        //            string abDataPath = System.Web.HttpContext.Current.Server.MapPath(REGION_FILE_PATH);
                        //            using (StreamWriter sw = new StreamWriter(abDataPath, false, Encoding.UTF8))
                        //            {
                        //                foreach (var s in strDataContent)
                        //                {
                        //                    sw.Write(s);
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                        #endregion
                        regions = LoadRegionData().Where(r => r.Status == Region.RegionStatus.Normal);
                        Cache.InsertLocal(CacheKeyCollection.Region, regions, REGION_CACHETIME);
                    }
                }
                return regions;
            }
        }

        /// <summary>
        /// 从JS加载地区数据
        /// </summary>
        /// <returns></returns>
        private List<Region> LoadRegionData()
        {
            IEnumerable<Region> region;
            #region TDO张宇枫修改
            GetFilePath(out REGION_FILE_PATH, out REGION_BAK_PATH);
            string regionString = string.Empty;
            using (FileStream fs = new FileStream(Core.Helper.IOHelper.GetMapPath(REGION_FILE_PATH), FileMode.Open))
            {
                using (StreamReader streamReader = new StreamReader(fs))
                {
                    regionString = streamReader.ReadToEnd();
                }
            }
            //使用这个方法获取JSON文件会自动带上BOM编码格式，导致JSON无法解析
            //var regionBytes = MallIO.GetFileContent(REGION_FILE_PATH);
            //var regionString = System.Text.Encoding.UTF8.GetString(regionBytes);
            #endregion
            List<Region> source = new List<Region>();
            region = JsonConvert.DeserializeObject<IEnumerable<Region>>(regionString);
            foreach (var item in region)
            {
                FillRegion(source, item, 1);
            }
            return source;
        }

        /// <summary>
        /// 把子地区平铺
        /// </summary>
        /// <param name="list"></param>
        /// <param name="parent"></param>
        /// <param name="level"></param>
        private void FillRegion(List<Region> list, Region parent, int level)
        {
            list.Add(parent);
            parent.Level = (Region.RegionLevel)level;
            level++;
            if (parent.Sub == null)
                return;
            if (level > 4) { parent.Sub = null; return; }//清空等级大于4的行政区域

            foreach (var sub in parent.Sub)
            {
                sub.Parent = parent;
                FillRegion(list, sub, level);
            }
        }

        /// <summary>
        /// 检查同名区域
        /// </summary>
        /// <param name="regionName"></param>
        /// <param name="region"></param>
        private void CheckRegionName(string regionName, IEnumerable<Region> region)
        {
            if (region.Any(a => a.Name == regionName || a.ShortName == regionName))
            {
                throw new MallException("已存在相同区域名称");
            }
        }

        public void DelRegion(int regionId)
        {
            var region = GetRegion(regionId);

            var provinces = RegionSource.Where(a => a.Level == Region.RegionLevel.Province);
            region.Status = Region.RegionStatus.Delete;
            var json = JsonConvert.SerializeObject(provinces);
            GetFilePath(out REGION_FILE_PATH, out REGION_BAK_PATH);
            Core.MallIO.CreateFile(REGION_FILE_PATH, json, FileCreateType.Create);
            Cache.Remove(CacheKeyCollection.Region);
            region = null;
        }

        public void EditRegion(string regionName, int regionId)
        {
            var region = GetRegion(regionId);

            IEnumerable<Region> regs;
            //检查重名
            if (region.Level == Region.RegionLevel.Province)
            {
                regs = RegionSource.Where(a => a.Level == Region.RegionLevel.Province & a.Id != regionId);
            }
            else
            {
                regs = region.Parent.Sub.Where(a => a.Id != regionId);
            }
            CheckRegionName(regionName, regs);
            var provinces = RegionSource.Where(a => a.Level == Region.RegionLevel.Province);
            region.Name = regionName;
            region.ShortName = GetShortAddressName(regionName);
            //var level = region.Level;
            //var regionPath = region.GetIdPath().Split(',').Select(a => int.Parse(a)).ToList();
            //if (region.Level== Region.RegionLevel.Province)
            //{
            //    CheckRegionName(regionName, provinces.Where(a=>a.Id!=regionId).ToList());
            //    var topRegion = provinces.Where(a => a.Id == regionId).FirstOrDefault();
            //    topRegion.Name = regionName;
            //    topRegion.ShortName = GetShortAddressName(regionName);
            //}
            //else if (level == Region.RegionLevel.City)
            //{
            //    var topRegion = region.Parent;
            //    CheckRegionName(regionName, topRegion.Sub.Where(a => a.Id != regionId).ToList());
            //    var secondRegion = topRegion.Sub.Where(a => a.Id == regionPath[1]).FirstOrDefault();
            //    secondRegion.Name = regionName;
            //    secondRegion.ShortName = GetShortAddressName(regionName);
            //}
            //else if (level == Region.RegionLevel.County)
            //{
            //    var topRegion = provinces.Where(a => a.Id == regionPath[0]).FirstOrDefault();
            //    var secondRegion = topRegion.Sub.Where(a => a.Id == regionPath[1]).FirstOrDefault();
            //    CheckRegionName(regionName, secondRegion.Sub.Where(a => a.Id != regionId).ToList());
            //    var thirdRegion = secondRegion.Sub.Where(a => a.Id == regionPath[2]).FirstOrDefault();
            //    thirdRegion.Name = regionName;
            //    thirdRegion.ShortName = GetShortAddressName(regionName);
            //}
            //else if (level == Region.RegionLevel.Town)
            //{
            //    var topRegion = provinces.Where(a => a.Id == regionPath[0]).FirstOrDefault();
            //    var secondRegion = topRegion.Sub.Where(a => a.Id == regionPath[1]).FirstOrDefault();
            //    var thirdRegion = secondRegion.Sub.Where(a => a.Id == regionPath[2]).FirstOrDefault();
            //    CheckRegionName(regionName, thirdRegion.Sub.Where(a => a.Id != regionId).ToList());
            //    var fourthRegion = thirdRegion.Sub.Where(a => a.Id == regionPath[3]).FirstOrDefault();
            //    fourthRegion.Name = regionName;
            //    fourthRegion.ShortName = GetShortAddressName(regionName);
            //}

            // var provinces = RegionSource.Where(p => p.Level == Region.RegionLevel.Province);
            var json = JsonConvert.SerializeObject(provinces);
            //using (StreamWriter sw = System.IO.File.CreateText(Core.Helper.IOHelper.GetMapPath("/Scripts/Region.json")))
            //{
            //    sw.WriteLine(json);
            //    sw.Flush();
            //    sw.Close();
            //}
            GetFilePath(out REGION_FILE_PATH, out REGION_BAK_PATH);
            Core.MallIO.CreateFile(REGION_FILE_PATH, json, FileCreateType.Create);
            Cache.Remove(CacheKeyCollection.Region);
            region = null;
        }

        public long AddRegion(string regionName, long parentid)
        {
            var region = new Region();
            var provinces = RegionSource;
            region.Id = RegionSource.Max(a => a.Id) + 1;
            if (parentid == 0)
            {//检查重名
                CheckRegionName(regionName, RegionSource.Where(a => a.Level == Region.RegionLevel.Province));
                region.Level = Region.RegionLevel.Province;
                region.Name = regionName;
                region.ShortName = GetShortAddressName(regionName);
                provinces.Concat(new Region[] { region });
            }
            else
            {
                var parent = RegionSource.FirstOrDefault(p => p.Id == parentid);
                if (parent.Sub != null)
                {
                    //检查重名
                    CheckRegionName(regionName, parent.Sub);
                }
                else
                {
                    parent.Sub = new List<Region>();
                }

                region.Level = parent.Level + 1;
                region.Name = regionName;
                region.ShortName = GetShortAddressName(regionName);
                region.Parent = parent;
                parent.Sub.Add(region);
                provinces = RegionSource;
            }
            provinces = provinces.Where(p => p.Level == Region.RegionLevel.Province);
            var json = JsonConvert.SerializeObject(provinces);
            GetFilePath(out REGION_FILE_PATH, out REGION_BAK_PATH);
            Core.MallIO.CreateFile(REGION_FILE_PATH, json, FileCreateType.Create);
            //using (StreamWriter sw = System.IO.File.CreateText(Core.Helper.IOHelper.GetMapPath("/Scripts/Region.json")))
            //{
            //    sw.WriteLine(json);
            //    sw.Flush();
            //    sw.Close();
            //}
            Cache.Remove(CacheKeyCollection.Region);
            regions = null;
            return region.Id;
        }


        [Obsolete("请使用AddRegion有返回值的方法")]
        public void AddRegion(string regionName, Region.RegionLevel level, string path)
        {
            var maxId = RegionSource.Max(a => a.Id) + 1;
            var provinces = RegionSource.Where(a => a.Level == Region.RegionLevel.Province);
            var regionPath = path.Split(',').Select(a => int.Parse(a));
            if (level == Region.RegionLevel.Province)
            {
                provinces.Concat(new Region[] { new Region { Name = regionName, Id = maxId, ShortName = GetShortAddressName(regionName) } });
            }
            else if (level == Region.RegionLevel.City)
            {
                var topRegion = provinces.Where(a => a.Id == regionPath.First()).FirstOrDefault();
                SetSub(topRegion, regionName, maxId, regionName);
            }
            else if (level == Region.RegionLevel.County)
            {
                var topRegion = provinces.Where(a => a.Id == regionPath.First()).FirstOrDefault();
                var secondRegion = topRegion.Sub.Where(a => a.Id == regionPath.Skip(1).First()).FirstOrDefault();
                SetSub(secondRegion, regionName, maxId, regionName);
            }
            else if (level == Region.RegionLevel.Town)
            {
                var topRegion = provinces.Where(a => a.Id == regionPath.First()).FirstOrDefault();
                var secondRegion = topRegion.Sub.Where(a => a.Id == regionPath.Skip(1).First()).FirstOrDefault();
                var thirdRegion = secondRegion.Sub.Where(a => a.Id == regionPath.Skip(2).First()).FirstOrDefault();
                SetSub(thirdRegion, regionName, maxId, regionName);
            }
            var json = JsonConvert.SerializeObject(provinces);
            GetFilePath(out REGION_FILE_PATH, out REGION_BAK_PATH);
            using (StreamWriter sw = System.IO.File.CreateText(Core.Helper.IOHelper.GetMapPath(REGION_FILE_PATH)))
            {
                sw.WriteLine(json);
                sw.Flush();
                sw.Close();
            }
            Cache.Remove(CacheKeyCollection.Region);
            regions = null;
        }
        /// <summary>
        /// 获取横向平铺的地区数据
        /// </summary>
        public IEnumerable<Region> GetAllRegions()
        {
            return RegionSource;
        }

        /// <summary>
        /// 重置地区数据 
        /// </summary>
        public void ResetRegions()
        {
            GetFilePath(out REGION_FILE_PATH, out REGION_BAK_PATH);
            Core.MallIO.CopyFile(REGION_BAK_PATH, REGION_FILE_PATH, true);
            Cache.Remove(CacheKeyCollection.Region);
            regions = null;
        }

        /// <summary>
        /// 根据ID获取某个地区的信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Region GetRegion(long id)
        {
            return RegionSource.FirstOrDefault(p => p.Id == id);
        }

        public Region GetRegion(long id, Region.RegionLevel level)
        {
            var region = RegionSource.FirstOrDefault(p => p.Id == id);
            while (region != null && region.Level > level)
            {
                region = region.Parent;
            }
            if (region != null && region.Level == level)
                return region;
            return new Region();
        }

        public Region GetRegionByName(string name)
        {
            return RegionSource.FirstOrDefault(p => p.Name.Contains(name) || name.Contains(p.Name));
        }

        public Region GetRegionByName(string name, Region.RegionLevel level)
        {
            return RegionSource.FirstOrDefault(p => (p.Name.Contains(name) || name.Contains(p.Name)) && p.Level == level);
        }

        public IEnumerable<Region> GetSubs(long parent, bool trace = false)
        {
            if (parent == 0)
                return RegionSource.Where(p => p.ParentId == 0);

            var region = RegionSource.FirstOrDefault(p => p.Id == parent);
            if (trace)
            {
                List<Region> sub = new List<Region>();
                FillSubRegion(sub, region);
                return sub;
            }

            IEnumerable<Region> subs = new List<Region>();
            if (region.Sub != null)
                subs = region.Sub.Where(r => r.Status == Region.RegionStatus.Normal);
            return subs;//下属区域
        }


        /// <summary>
        /// 获取三级子类
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public IEnumerable<Region> GetThridSubs(long parent)
        {
            if (parent == 0)
                return RegionSource.Where(p => p.ParentId == 0);

            var region = RegionSource.FirstOrDefault(p => p.Id == parent);
            List<Region> sub = new List<Region>();
            FillSubRegion(sub, region, Region.RegionLevel.County);
            return sub;
        }



        /// <summary>
        /// 填充追溯所有下属子集
        /// </summary>
        /// <param name="list">填充列表</param>
        /// <param name="model"></param>
        private void FillSubRegion(IEnumerable<Region> list, Region model, Region.RegionLevel level = Region.RegionLevel.Village)
        {
            if (model.Sub == null || model.Sub.Count == 0 || model.Level == level)
                return;
            foreach (var item in model.Sub)//市级
            {
                if (item.Sub != null && item.Sub.Count > 0 && item.Level != level)
                {
                    list.Concat(item.Sub.Where(i => i.Status == Region.RegionStatus.Normal));
                    FillSubRegion(list, item, level);
                }
                else
                {
                    item.Sub = new List<Region>();
                }
            }
        }

        public string GetFullName(long id, string seperator = " ")
        {
            var region = RegionSource.FirstOrDefault(p => p.Id == id);
            if (region == null)
                return string.Empty;
            var name = region.Name;
            //追溯上级区域,最多追溯5次 防循环溢出
            for (int i = 0; i < 5 && region.Parent != null; i++)
            {
                region = region.Parent;
                name = region.Name + seperator + name;
            }
            return name;
        }

        public string GetRegionPath(long id, string seperator = ",")
        {
            var region = RegionSource.FirstOrDefault(p => p.Id == id);
            if (region == null)
                return string.Empty;
            var path = id.ToString();

            //追溯上级区域,最多追溯5次 防循环溢出
            for (int i = 0; i < 5 && region.Parent != null; i++)
            {
                region = region.Parent;
                path = region.Id + "," + path;
            }
            return path;
        }
        /// <summary>
        /// 根据地址名称反查地址全路径
        /// </summary>
        /// <param name="city">城市名</param>
        /// <param name="district">区名</param>
        /// <param name="street">街道名</param>
        /// <returns></returns>
        public string GetAddress_Components(string city, string district, string street, out string newStreet)
        {
            Region myregion = null;
            var cityData = RegionSource.FirstOrDefault(p => p.Name == city.Trim() && p.Level == Region.RegionLevel.City);//城市
            if (cityData != null)
            {
                var parent = RegionSource.FirstOrDefault(p => p.Name == district.Trim() && p.Level == Region.RegionLevel.County && p.ParentId == cityData.Id);//区域
                if (parent == null)
                {
                    parent = RegionSource.FirstOrDefault(p => p.Name.Contains("其它区") && p.Level == Region.RegionLevel.County && p.ParentId == cityData.Id);//区域
                }
                if (parent != null)
                {
                    myregion = RegionSource.Where(p => p.Name == street.Trim() && p.ParentId == (parent.Id)).FirstOrDefault();//优先街道
                    if (myregion == null)
                    {
                        street = street.Replace("街道", "").Replace("镇", "").Replace("街", "");//地区库第四级可能不包含“街道、镇等文字”
                        myregion = RegionSource.Where(p => p.Name == street.Trim() && p.ParentId == (parent.Id)).FirstOrDefault();//特殊替换处理
                    }
                }
                if (myregion == null && parent != null)//街空取区
                {
                    myregion = parent;
                }
            }

            newStreet = street;

            if (myregion == null)
                return string.Empty;


            var path = myregion.Id.ToString();
            //追溯上级区域,最多追溯5次 防循环溢出
            for (int i = 0; i < 5 && myregion.Parent != null; i++)
            {
                myregion = myregion.Parent;
                path = myregion.Id + "," + path;
            }
            return path;
        }
        /// <summary>
        /// 通过IP取地区信息
        /// <para>(数据来源：淘宝)</para>
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public long GetRegionByIPInTaobao(string ip)
        {
            if (Core.Cache.Exists(ip))
                return Cache.Get<long>(ip);
            string RequestUrl = "http://ip.taobao.com/service/getIpInfo.php?ip={0}";
            long result = 0;
            RequestUrl = string.Format(RequestUrl, ip);
            try
            {
                string requestdata = Mall.Core.Helper.WebHelper.GetRequestData(RequestUrl, "");
                TaobaoIpDataModel tbipdata = JsonConvert.DeserializeObject<TaobaoIpDataModel>(requestdata);
                if (tbipdata != null && tbipdata.code == 0)
                {
                    if (!string.IsNullOrWhiteSpace(tbipdata.data.city))
                    {
                        var city = GetRegionByName(tbipdata.data.city, Region.RegionLevel.City);
                        if (city != null)
                            return city.Id;
                    }
                    if (!string.IsNullOrWhiteSpace(tbipdata.data.region))
                    {
                        var province = GetRegionByName(tbipdata.data.region, Region.RegionLevel.Province);
                        if (province != null)
                            return province.Id;
                    }
                }
            }
            catch
            {
            }
            Cache.Insert(ip, result);
            return result;
        }

        /// <summary>
        /// 同步京东地址库
        /// </summary>
        /// <param name="appkey"></param>
        public void SysJDRegions(string appkey)
        {
            string errorMsg = "同步失败";
            List<Region> jdregions = new List<Region>();
            try
            {
                var apiInstance = new DefaultApi("https://way.jd.com");
                var provinceJson = apiInstance.GetProvince(appkey);
                if (!string.IsNullOrEmpty(provinceJson))
                {

                    var provinces = JsonConvert.DeserializeObject<ProvinceResponce>(provinceJson);
                    if (provinces != null && provinces.code.Equals("10000") && provinces.result.jingdong_area_province_get_responce.province_areas.Any())
                    {
                        //省份循环
                        foreach (var province in provinces.result.jingdong_area_province_get_responce.province_areas)
                        {
                            Region proRegion = new Region();
                            proRegion.Id = int.Parse(province.id);
                            proRegion.Name = province.name;
                            proRegion.ShortName = GetShortAddressName(province.name);
                            proRegion.Status = Region.RegionStatus.Normal;
                            List<Region> CitySubs = new List<Region>();
                            var cityJson = apiInstance.GetCity(province.id, appkey);
                            if (!string.IsNullOrEmpty(cityJson))
                            {
                                //市循环
                                var citys = new CityResponce();
                                try
                                {
                                    citys = JsonConvert.DeserializeObject<CityResponce>(cityJson);
                                    if (citys != null && citys.code.Equals("10000") && citys.result.jingdong_areas_city_get_responce.baseAreaServiceResponse.data.Any())
                                    {
                                        foreach (var city in citys.result.jingdong_areas_city_get_responce.baseAreaServiceResponse.data)
                                        {
                                            Region cityRegion = new Region();
                                            cityRegion.Id = city.areaId;
                                            cityRegion.Name = city.areaName;
                                            cityRegion.ShortName = GetShortAddressName(city.areaName);
                                            cityRegion.Status = Region.RegionStatus.Normal;
                                            List<Region> countrySubs = new List<Region>();

                                            //区循环
                                            var countryJson = apiInstance.GetCountry(city.areaId.ToString(), appkey);
                                            if (!string.IsNullOrEmpty(countryJson))
                                            {
                                                var countrys = new CountryResponce();
                                                try
                                                {
                                                    countrys = JsonConvert.DeserializeObject<CountryResponce>(countryJson);
                                                    if (countrys != null && countrys.code.Equals("10000") && countrys.result.jingdong_areas_county_get_responce.baseAreaServiceResponse.data.Any())
                                                    {
                                                        foreach (var country in countrys.result.jingdong_areas_county_get_responce.baseAreaServiceResponse.data)
                                                        {
                                                            Region countryRegion = new Region();
                                                            countryRegion.Id = country.areaId;
                                                            countryRegion.Name = country.areaName;
                                                            countryRegion.ShortName = GetShortAddressName(country.areaName);
                                                            countryRegion.Status = Region.RegionStatus.Normal;
                                                            List<Region> townSubs = new List<Region>();

                                                            //街道循环
                                                            var townJson = apiInstance.GetTown(country.areaId.ToString(), appkey);
                                                            if (!string.IsNullOrEmpty(townJson))
                                                            {
                                                                var towns = new TownResponce();
                                                                try
                                                                {
                                                                    towns = JsonConvert.DeserializeObject<TownResponce>(townJson);
                                                                }
                                                                catch (Exception)
                                                                {
                                                                    towns = null;
                                                                }
                                                                if (towns != null && towns.code.Equals("10000") && towns.result.jingdong_areas_town_get_responce.baseAreaServiceResponse.data.Any())
                                                                {
                                                                    foreach (var town in towns.result.jingdong_areas_town_get_responce.baseAreaServiceResponse.data)
                                                                    {
                                                                        Region townRegion = new Region();
                                                                        townRegion.Id = town.areaId;
                                                                        townRegion.Name = town.areaName;
                                                                        townRegion.ShortName = GetShortAddressName(town.areaName);
                                                                        townRegion.Status = Region.RegionStatus.Normal;
                                                                        townRegion.Sub = new List<Region>();

                                                                        townSubs.Add(townRegion);
                                                                    }
                                                                }
                                                                else
                                                                    errorMsg = towns.msg;
                                                            }

                                                            countryRegion.Sub = townSubs;
                                                            countrySubs.Add(countryRegion);
                                                        }
                                                    }
                                                    else
                                                        errorMsg = countrys.msg;
                                                }
                                                catch (Exception)
                                                {
                                                    countrys = null;
                                                }
                                            }

                                            cityRegion.Sub = countrySubs;
                                            CitySubs.Add(cityRegion);
                                        }
                                    }
                                    else
                                        errorMsg = citys.msg;
                                }
                                catch (Exception)
                                {
                                    citys = null;
                                }
                            }
                            proRegion.Sub = CitySubs;
                            jdregions.Add(proRegion);
                        }
                    }
                    else
                        errorMsg = provinces.msg;

                    if (jdregions.Any())
                    {
                        var json = JsonConvert.SerializeObject(jdregions);
                        GetFilePath(out REGION_FILE_PATH, out REGION_BAK_PATH);
                        Core.MallIO.CreateFile(REGION_FILE_PATH, json, FileCreateType.Create);
                        Cache.Remove(CacheKeyCollection.Region);
                    }
                    else
                    {
                        throw new MallException("同步失败，" + errorMsg);
                    }
                }
                else
                {
                    throw new MallException("同步失败，请检查appkey");
                }
            }
            catch (Exception ex)
            {
                if (jdregions.Any())
                {
                    var json = JsonConvert.SerializeObject(jdregions);
                    GetFilePath(out REGION_FILE_PATH, out REGION_BAK_PATH);
                    Core.MallIO.CreateFile(REGION_FILE_PATH, json, FileCreateType.Create);
                    Cache.Remove(CacheKeyCollection.Region);
                }
                throw new MallException(ex.Message);
            }

        }

        /// <summary>
        /// 是否开启京东地址库
        /// </summary>
        /// <returns></returns>
        public bool IsOpenJdRegion()
        {
            var siteSetting = SiteSettingApplication.SiteSettings;
            if (string.IsNullOrWhiteSpace(siteSetting.MallJDVersion))
            {
                siteSetting.MallJDVersion = "0.0.0";
            }
            Version v1 = new Version(siteSetting.MallJDVersion), v2 = new Version(Mall.CommonModel.CommonConst.DefaultMALLJDVersion);

            return v1 >= v2;
        }

        #region 私有方法

        /// <summary>
        /// 获取短地址   
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string GetShortAddressName(string str)
        {
            string result = str;
            result = result.Replace("特别行政区", "");
            result = result.Replace("省", "");
            result = result.Replace("维吾尔", "");
            result = result.Replace("回族", "");
            result = result.Replace("壮族", "");
            result = result.Replace("自治区", "");
            result = result.Replace("市", "");
            result = result.Replace("盟", "");
            result = result.Replace("林区", "");
            result = result.Replace("地区", "");
            result = result.Replace("土家族", "");
            result = result.Replace("苗族", "");
            result = result.Replace("回族", "");
            result = result.Replace("黎族", "");
            result = result.Replace("藏族", "");
            result = result.Replace("傣族", "");
            result = result.Replace("彝族", "");
            result = result.Replace("哈尼族", "");
            result = result.Replace("壮族", "");
            result = result.Replace("白族", "");
            result = result.Replace("景颇族", "");
            result = result.Replace("傈僳族", "");
            result = result.Replace("朝鲜族", "");
            result = result.Replace("蒙古", "");
            result = result.Replace("哈萨克", "");
            result = result.Replace("柯尔克孜", "");
            result = result.Replace("自治州", "");
            result = result.Replace("自治县", "");
            result = result.Replace("县", "");
            return result;
        }
        private void SetSub(Region topRegion, string regionName, int Id, string shortName)
        {
            var sub = topRegion.Sub;
            if (sub == null)
            {
                sub = new List<Region>();
                sub.Add(new Region() { Name = regionName, Id = Id, ShortName = GetShortAddressName(regionName) });
                topRegion.Sub = sub;
            }
            else
            {
                CheckRegionName(regionName, sub);
                sub.Add(new Region() { Name = regionName, Id = Id, ShortName = GetShortAddressName(regionName) });
            }
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static MetaInfo GetFileMetaInfo(string fileName)
        {
            MetaInfo minfo = new MetaInfo();
            var file = _hostingEnvironment.ContentRootPath + fileName;
            FileInfo finfo = new FileInfo(file);
            if (finfo.Exists)
            {
                minfo.ContentLength = finfo.Length;
                string contentType;

                new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);


                minfo.ContentType = contentType;
                minfo.LastModifiedTime = finfo.LastWriteTime;
                return minfo;
            }
            return null;
        }

        /// <summary>
        /// 检查文件信息
        /// </summary>
        /// <param name="remote"></param>
        /// <param name="local"></param>
        /// <returns></returns>
        private static bool CheckMetaInfo(MetaInfo remote, MetaInfo local)
        {
            if (null == remote) return false;
            return null != local ? remote.LastModifiedTime > local.LastModifiedTime : true;
        }

        public IEnumerable<Region> GetSubsNew(long parent, bool trace = false)
        {
            if (parent == 0)
                return RegionSource.Where(p => p.ParentId == 0);

            var sub = new List<Region>();
            var region = RegionSource.FirstOrDefault(p => p.Id == parent);
            if (null == region) return sub;
            if (trace)
            {
                FillSubRegionNew(sub, region);
                return sub;
            }
            return region.Sub ?? new List<Region>();//下属区域
        }

        private void FillSubRegionNew(List<Region> list, Region model)
        {
            if (model.Sub == null || model.Sub.Count == 0)
                return;
            foreach (var item in model.Sub)
            {
                list.Add(item);
                FillSubRegionNew(list, item);
            }
        }

        #endregion
    }






    #region 淘宝Ip数据
    public class TaobaoIpDataModel
    {
        public int code { get; set; }
        public TaobaoIpData data { get; set; }
    }

    public class TaobaoIpData
    {
        public string country { get; set; }
        public string country_id { get; set; }
        public string area { get; set; }
        public string area_id { get; set; }
        public string region { get; set; }
        public string region_id { get; set; }
        public string city { get; set; }
        public string city_id { get; set; }
        public string county { get; set; }
        public string county_id { get; set; }
        public string isp { get; set; }
        public string isp_id { get; set; }
        public string ip { get; set; }
    }

    #endregion
}



