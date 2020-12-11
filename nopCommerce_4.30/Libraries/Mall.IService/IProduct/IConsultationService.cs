using Mall.CommonModel;
using Mall.DTO.QueryModel;
using System.Collections.Generic;

namespace Mall.IServices
{
    public interface IConsultationService : IService
    {
        /// <summary>
        /// 添加一个产品咨询
        /// </summary>
        /// <param name="model"></param>
        void AddConsultation(Entities.ProductConsultationInfo model);
        /// <summary>
        ///回复咨询
        /// </summary>
        /// <param name="model"></param>
        void ReplyConsultation(long id, string ReplyContent, long shopId);
        /// <summary>
        /// 删除一个咨询
        /// </summary>
        /// <param name="id"></param>
        void DeleteConsultation(long id);
        /// <summary>
        /// 分页获取咨询列表
        /// </summary>
        /// <param name="query">咨询查询实体</param>
        /// <returns>咨询分页实体</returns>
        QueryPageModel<Entities.ProductConsultationInfo> GetConsultations(ConsultationQuery query);
        /// <summary>
        /// 获取咨询数
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int GetConsultationCount(ConsultationQuery query);

        /// <summary>
        /// 获取某一个商品的所有咨询
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        List<Entities.ProductConsultationInfo> GetConsultations(long pid);

        /// <summary>
        /// 获取一个咨询信息
        /// </summary>
        /// <param name="id">咨询ID</param>
        /// <returns></returns>
        Entities.ProductConsultationInfo GetConsultation(long id);
    }
}
