
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System.Collections.Generic;
using Mall.DTO;
namespace Mall.IServices
{
    /// <summary>
    /// 商品专题服务接口
    /// </summary>
    public interface ITopicService:IService
    {
        /// <summary>
        /// 获取所有专题
        /// </summary>
        /// <param name="pageNo">页号</param>
        /// <param name="pageSize">每页行数</param>
        /// <param name="platformType">平台类型</param>
        /// <returns></returns>
        QueryPageModel<Entities.TopicInfo> GetTopics(int pageNo, int pageSize, PlatformType platformType = PlatformType.PC);
        List<TopicInfo> GetTopics(List<long> topics);
        List<TopicModuleInfo> GetModules(long topic);
        List<ModuleProductInfo> GetModuleProducts(List<long> modules);
        TopicModuleInfo GetTopicModule(long moduleId);

            /// <summary>
            /// 根据条件获取专题
            /// </summary>
            /// <param name="topicQuery">条件</param>
            /// <returns></returns>
        QueryPageModel<TopicInfo> GetTopics(TopicQuery topicQuery);

        /// <summary>
        /// 删除专题
        /// </summary>
        /// <param name="id">专题编号</param>
        void DeleteTopic(long id);


        /// <summary>
        /// 新增商品专题
        /// </summary>
        /// <param name="topicInfo">商品专题实体</param>
        void AddTopic(Topic topic);
        void AddTopicInfo(TopicInfo topic);
        void UpdateTopicInfo(TopicInfo topic);

        /// <summary>
        /// 更新商品专题
        /// </summary>
        /// <param name="topicInfo">商品专题实体</param>
        void UpdateTopic(Topic topicInfo);

        /// <summary>
        /// 获取商品专题
        /// </summary>
        /// <param name="id">商品专题编号</param>
        /// <returns></returns>
        TopicInfo GetTopicInfo(long id);
    }
}
