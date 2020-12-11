using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.Web.Areas.SellerAdmin.Models
{
    /// <summary>
    /// 导入淘宝/天猫数据
    /// </summary>
    public class SpiderProductModel
    {
        /// <summary>
        /// 平台一级分类
        /// </summary>
        public long BigCategoryId { get; set; }
        /// <summary>
        /// 平台二级分类
        /// </summary>
        public long MidCategoryId { get; set; }
        /// <summary>
        /// 平台三级分类
        /// </summary>
        public long SmallCategoryId { get; set; }
        /// <summary>
        /// 商家一级分类
        /// </summary>
        public long SellerBigCategoryId { get; set; }
        /// <summary>
        /// 商家二级分类
        /// </summary>
        public long SellerMidcategoryId { get; set; }
        /// <summary>
        /// 品牌ID
        /// </summary>
        public long BrandId { get; set; }
        /// <summary>
        /// 运费模板ID
        /// </summary>
        public long FreightTemplateId { get; set; }
        /// <summary>
        /// 抓取链接地址集合
        /// </summary>
        public List<string> GrabUrl { get; set; }
        public string Guid { get; set; }
    }

    /// <summary>
    /// 导入商品处理结果实体
    /// </summary>
    public class SpiderProductResult
    {
        /// <summary>
        /// 成功条数
        /// </summary>
         public int SuccessNum { get; set; }
        /// <summary>
        /// 失败条数
        /// </summary>
        public int FailNum { get; set; }
        /// <summary>
        /// 错误解析数据
        /// </summary>
        public List<FailDataModel> FailDataModel { get; set; }

    }
    public class FailDataModel
    {
        /// <summary>
        /// 抓取地址
        /// </summary>
        public string GrabUrl { get; set; }
        /// <summary>
        /// 失败备注
        /// </summary>
        public string Remark { get; set; }
    }
}