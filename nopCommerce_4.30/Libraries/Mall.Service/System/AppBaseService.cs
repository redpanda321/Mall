using Mall.Core;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Linq;

namespace Mall.Service
{
    /// <summary>
    /// APP基础信息服务
    /// </summary>
    public class AppBaseService : ServiceBase, IAppBaseService
    {
        /// <summary>
        /// 使用appkey获取app基础信息
        /// </summary>
        /// <param name="appkey"></param>
        /// <returns></returns>
        public AppBaseSafeSettingInfo GetSetting(string appkey)
        {
            AppBaseSafeSettingInfo data = DbFactory.Default.Get<AppBaseSafeSettingInfo>().Where(d => d.AppKey == appkey).FirstOrDefault();
            if (data == null)
            {
                throw new MallException("错误的appkey");
            }
            return data;
        }
        /// <summary>
        /// 通过appkey获取AppSecret
        /// </summary>
        /// <param name="appkey"></param>
        /// <returns></returns>
        public string GetAppSecret(string appkey)
        {
            string result = "";
            var data = DbFactory.Default.Get<AppBaseSafeSettingInfo>().Where(d => d.AppKey == appkey).FirstOrDefault();
            if (data == null)
            {
                throw new MallException("错误的appkey");
            }
            if (string.IsNullOrWhiteSpace(data.AppSecret))
            {
                data.AppSecret = MakeAppSecreat();
                DbFactory.Default.Update(data);
            }
            result = data.AppSecret;
            return result;
        }
        /// <summary>
        /// 取第一个app基础信息
        /// </summary>
        /// <returns></returns>
        public AppBaseSafeSettingInfo GetFirstSetting()
        {
            var result = DbFactory.Default.Get<AppBaseSafeSettingInfo>().FirstOrDefault();
            if (result == null)
            {
                result = new AppBaseSafeSettingInfo();
                result.AppKey = MakeAppKey();
                result.AppSecret = MakeAppSecreat();
                DbFactory.Default.Add(result);
            }

            return result;
        }

        #region 私有方法
        /// <summary>
        /// 生成一个appkey
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        private string MakeAppKey()
        {
            string result = "Mall";
            while (true)
            {
                Random rnd = new Random();
                string[] seeds = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k",
                    "l", "m", "n", "o", "p","q", "r", "s", "t", "u", "v", "w", "x", "y",
                    "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
                int seedlen = seeds.Length;
                for (int _i = 0; _i < 4; _i++)
                {
                    result += seeds[rnd.Next(0, seedlen)];
                }
                if (!DbFactory.Default.Get<AppBaseSafeSettingInfo>().Where(d => d.AppKey == result).Exist())
                {
                    break;
                }
            }
            return result;
        }
        /// <summary>
        /// 生成一个AppSecreat
        /// </summary>
        /// <returns></returns>
        private string MakeAppSecreat()
        {
            string result = "has";
            while (true)
            {
                Random rnd = new Random();
                string[] seeds = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k",
                    "l", "m", "n", "o", "p","q", "r", "s", "t", "u", "v", "w", "x", "y",
                    "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
                int seedlen = seeds.Length;
                for (int _i = 0; _i < 7; _i++)
                {
                    result += seeds[rnd.Next(0, seedlen)];
                }
                if (!DbFactory.Default.Get<AppBaseSafeSettingInfo>().Where(d => d.AppKey == result).Exist())
                {
                    break;
                }
            }
            return result;
        }
        #endregion
    }
}

