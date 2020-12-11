using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    /// <summary>
    /// 周边门店页面Model
    /// </summary>
    public class ShopBranchListHomePage
    {
        /// <summary>
        /// 轮播图、图标、广告图
        /// </summary>
        public List<Mall.DTO.SlideAdModel> SlideAdModels { get; set; }
        /// <summary>
        /// 广告位
        /// </summary>
        //public List<Mall.Model.ImageAdInfo> ImageAdInfos { get; set; }
    }
}
