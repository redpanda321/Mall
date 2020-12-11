using Mall.Core;
using Mall.IServices;
using Mall.Entities;
using NetRube.Data;

namespace Mall.Service
{
    public class CityExpressConfigServiceService : ServiceBase, ICityExpressConfigServiceService
    {
        /// <summary>
        /// 获得达达配置信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public CityExpressConfigInfo GetDaDaCityExpressConfig(long shopId)
        {
            if (shopId == 0)
            {
                throw new MallException("错误的参数：shopid");
            }
            //var result = Context.CityExpressConfigInfo.SingleOrDefault(d => d.ShopId == shopId);
            var result = DbFactory.Default.Get<CityExpressConfigInfo>().Where(d => d.ShopId == shopId).FirstOrDefault();
            if (result == null)
            {
                result = new CityExpressConfigInfo { ShopId = shopId, IsEnable = false };
                //Context.CityExpressConfigInfo.Add(result);
                //Context.SaveChanges();
                DbFactory.Default.Add(result);
            }
            return result;
        }
        /// <summary>
        /// 修改达达配置信息
        /// </summary>
        /// <param name="data"></param>
        public void Update(long shopId, CityExpressConfigInfo data)
        {
            var _data = GetDaDaCityExpressConfig(shopId);
            _data.ShopId = data.ShopId;
            _data.IsEnable = data.IsEnable;
            _data.app_key = data.app_key;
            _data.app_secret = data.app_secret;
            _data.source_id = data.source_id;
            //Context.SaveChanges();
            DbFactory.Default.Update(_data);
        }
        /// <summary>
        /// 设置达达物流开启关闭状态
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public void SetStatus(long shopId, bool status)
        {
            var data = GetDaDaCityExpressConfig(shopId);
            //data.IsEnable = status;
            //Context.SaveChanges();
            //DbFactory.Default.Update(data);
            DbFactory.Default.Set<CityExpressConfigInfo>().Set(n => n.IsEnable, status).Where(p => p.Id == data.Id).Succeed();
        }
        /// <summary>
        /// 获取达达物流开启状态
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public bool GetStatus(long shopId)
        {
            return GetDaDaCityExpressConfig(shopId).IsEnable;
        }
    }
}
