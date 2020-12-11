using Mall.Entities;

namespace Mall.IServices
{
    /// <summary>
    /// 商家入驻设置
    /// </summary>
    public interface ISettledService:IService
    {
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="mSettledInfo"></param>
        void AddSettled(SettledInfo mSettledInfo);

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="mSettledInfo"></param>
        void UpdateSettled(SettledInfo mSettledInfo);

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        SettledInfo GetSettled();

    }
}
