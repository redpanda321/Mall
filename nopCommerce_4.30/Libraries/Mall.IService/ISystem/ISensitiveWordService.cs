using Mall.CommonModel;
using Mall.DTO.QueryModel;
using Mall.Entities;
using System.Collections.Generic;

namespace Mall.IServices
{
    /// <summary>
    /// 敏感关键词服务
    /// </summary>
    public interface ISensitiveWordService : IService
    {
        /// <summary>
        /// 获取敏感关键词列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        QueryPageModel<SensitiveWordInfo> GetSensitiveWords(SensitiveWordQuery query);

        /// <summary>
        /// 获取敏感词类别
        /// </summary>
        /// <returns></returns>
        List<string> GetCategories();

        /// <summary>
        /// 获取敏感关键词
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        SensitiveWordInfo GetSensitiveWord(int id);

        /// <summary>
        /// 添加敏感关键词
        /// </summary>
        /// <param name="model"></param>
        void AddSensitiveWord(SensitiveWordInfo model);

        /// <summary>
        /// 修改敏感关键词
        /// </summary>
        /// <param name="model"></param>
        void UpdateSensitiveWord(SensitiveWordInfo model);

        /// <summary>
        /// 删除敏感关键词
        /// </summary>
        /// <param name="id"></param>
        void DeleteSensitiveWord(int id);

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="ids"></param>
        void BatchDeleteSensitiveWord(int[] ids);

        /// <summary>
        /// 判断敏感关键词是否存在
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        bool ExistSensitiveWord(string word);
    }
}
