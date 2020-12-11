using Mall.Core.Plugins;
using Mall.Core.Plugins.Express;
using Mall.DTO;
using Mall.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Mall.IServices
{
    /// <summary>
    /// 快递服务接口
    /// </summary>
    public interface IExpressService : IService
    {
        /// <summary>
        /// 获取所有快递信息
        /// </summary>
        /// <returns></returns>
        IEnumerable<ExpressInfoInfo> GetAllExpress();

        /// <summary>
        /// 订阅快递100的物流信息
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="number"></param>

        void SubscribeExpress100(string expressCompanyName, string number, string kuaidi100Key, string city, string redirectUrl);

        /// <summary>
        /// 保存快递信息
        /// </summary>
        /// <param name="model"></param>

        void SaveExpressData(OrderExpressDataInfo model);

        /// <summary>
        /// 根据名称查找快递单模板信息
        /// </summary>
        /// <param name="name">快递名称</param>
        /// <returns></returns>
        ExpressInfoInfo GetExpress(string name);
        /// <summary>
        /// 获取快递面单元素
        /// </summary>
        /// <param name="expressid"></param>
        /// <returns></returns>
        IEnumerable<ExpressElementInfo> GetExpressElements(long expressid);
        /// <summary>
        /// 修改快递单模板元素信息
        /// </summary>
        /// <param name="elements">待修改快递单模板元素信息</param>
        /// <param name="name">快递名称</param>
        void UpdateExpressAndElement(ExpressInfoInfo express, ExpressElementInfo[] elements);
        /// <summary>
        /// 清除快递公司面单背景图片
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ExpressInfoInfo ClearExpressData(long id);
        /// <summary>
        /// 获取店铺最近使用的快递信息
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="takeNumber">取最近的个数</param>
        /// <returns></returns>
        IEnumerable<ExpressInfoInfo> GetRecentExpress(long shopId, int takeNumber);
        /// <summary>
        /// 获取快递物流信息
        /// </summary>
        /// <param name="expressCompanyName">快递公司名称</param>
        /// <param name="shipOrderNumber">快递单号</param>
        /// <returns></returns>
        ExpressData GetExpressData(string expressCompanyName, string shipOrderNumber);
        /// <summary>
        /// 增加快递公司
        /// </summary>
        /// <param name="model"></param>
        void AddExpress(ExpressInfoInfo model);
        /// <summary>
        /// 更新快递公司第三方编号
        /// </summary>
        /// <param name="model"></param>
        void UpdateExpressCode(ExpressInfoInfo model);

        void DeleteExpress(long id);

        void ChangeExpressStatus(long id, CommonModel.ExpressStatus status);
    }
}
