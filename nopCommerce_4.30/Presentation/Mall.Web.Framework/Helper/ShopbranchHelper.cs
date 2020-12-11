using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using System;
using System.IO;

namespace Mall.Web.Framework
{
    /// <summary>
    ///  门店相关操作
    /// </summary>
    public class ShopbranchHelper
    {
        /// <summary>
        /// 通过坐标获取地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static QQGetAddressByLatLngResult GetAddressByLatLng(string latLng, ref string address, ref string province, ref string city, ref string district, ref string street)
        {
            string[] latlngarr = latLng.Split(',');
            string gaodeLngLat = latlngarr[1] + "," + latlngarr[0];
            string newLatLng = Math.Round(decimal.Parse(latlngarr[1]), 4) + "," + Math.Round(decimal.Parse(latlngarr[0]), 4);
            var objlatlng = Core.Cache.Get<QQGetAddressByLatLngResult>(CacheKeyCollection.LatlngCacheKey(newLatLng));
            if (objlatlng != null)
            {
                QQGetAddressByLatLngResult resultobj = objlatlng;
                if (resultobj.status == 0)
                {
                    province = resultobj.result.address_component.province;
                    city = resultobj.result.address_component.city;
                    district = resultobj.result.address_component.district;
                    if (null != resultobj.result.address_reference && null != resultobj.result.address_reference.town && !string.IsNullOrEmpty(resultobj.result.address_reference.town.title))
                    {
                        street = resultobj.result.address_reference.town.title;
                    }
                    else
                    {
                        street = resultobj.result.address_component.street;
                    }
                    address = resultobj.result.address;
                }
                return resultobj;
            }
            else
            {
                string gaoDeAPIKey = "SYJBZ-DSLR3-IWX3Q-3XNTM-ELURH-23FTP";
                string url = string.Format("http://apis.map.qq.com/ws/geocoder/v1/?location={0}&key={1}&get_poi=1", latLng, gaoDeAPIKey);
                string result = GetResponseResult(url);

                QQGetAddressByLatLngResult resultobj = result.FromJSON<QQGetAddressByLatLngResult>(new QQGetAddressByLatLngResult { });
                if (resultobj.status == 0)
                {
                    var cacheTimeout = DateTime.Now.AddDays(1);
                    Core.Cache.Insert(CacheKeyCollection.LatlngCacheKey(newLatLng), resultobj, cacheTimeout); //坐标地址信息缓存一天
                    province = resultobj.result.address_component.province;
                    city = resultobj.result.address_component.city;
                    district = resultobj.result.address_component.district;
                    if (null != resultobj.result.address_reference && null != resultobj.result.address_reference.town && !string.IsNullOrEmpty(resultobj.result.address_reference.town.title))
                    {
                        street = resultobj.result.address_reference.town.title;
                    }
                    else
                    {
                        street = resultobj.result.address_component.street;
                    }
                    address = resultobj.result.address;
                }
                return resultobj;
            }
        }

        /// <summary>
        /// 获取Web请求返回的字符串数据
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetResponseResult(string url)
        {
            string result;
            try
            {
                System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)req.GetResponse())
                {
                    using (Stream receiveStream = response.GetResponseStream())
                    {
                        using (StreamReader readerOfStream = new StreamReader(receiveStream, System.Text.Encoding.UTF8))
                        {
                            result = readerOfStream.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Log.Error("根据经纬度获取地理位置异常", ex);
                result = "";
            }
            return result;
        }
    }
}
