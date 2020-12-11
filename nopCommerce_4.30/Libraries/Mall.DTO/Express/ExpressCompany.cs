using System.Collections.Generic;

namespace Mall.DTO
{
    /// <summary>
    /// 快递公司
    /// </summary>
    public class ExpressCompany
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 快递公司名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 快递公司的淘宝编号
        /// </summary>
        public string TaobaoCode { get; set; }
        /// <summary>
        /// 快递公司的快递100编号
        /// </summary>
        public string Kuaidi100Code { get; set; }
        /// <summary>
        /// 快递公司的快递鸟编号
        /// </summary>
        public string KuaidiNiaoCode { get; set; }
        /// <summary>
        /// 快递面单图片宽度
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// 快递面单图片高度
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// 快递面单图片
        /// </summary>
        public string BackGroundImage { get; set; }
        /// <summary>
        /// 快递公司状态，开启、关闭
        /// </summary>
        public Mall.CommonModel.ExpressStatus Status { get; set; }
        /// <summary>
        /// 快递公司面单元素
        /// </summary>
        public List<ExpressElement> Elements { get; set; }
        /// <summary>
        /// 快递公司logo
        /// </summary>
        public string Logo { get; set; }
        /// <summary>
        /// 快递单号是否符合规则
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool CheckExpressCodeIsValid(string code) {
            long current;
            return long.TryParse(code, out current);
        }
    }

    
}
