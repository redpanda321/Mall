using System.Collections.Generic;

namespace Mall.DTO
{
    /// <summary>
    /// 首页分类集
    /// </summary>
    public class HomeCategorySet
    {

        public class HomeCategoryTopic
        {

            public string Url{get;set;}

            public string ImageUrl{get;set;}
        }


        /// <summary>
        /// 首页分类集所包含的分类
        /// </summary>
        public List<Entities.HomeCategoryInfo> HomeCategories { get; set; }

        /// <summary>
        /// 首页分类集所包含的专题
        /// </summary>
        public List<HomeCategoryTopic> HomeCategoryTopics { get; set; }
       

        /// <summary>
        /// 分类集所在行号
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// 产品对拼品牌
        /// </summary>
        public List<Entities.BrandInfo> HomeBrand { get; set; }
    }
}
