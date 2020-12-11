using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Core.Spider.TaoBao
{
    /// <summary>
    /// 淘宝/天猫商品数据实体
    /// </summary>
    public class TaoBaoProductDetailsInfo
    {
        public TMProductInfo ProductInfo { get; set; }
        /// <summary>
        /// 抓取对象[0=其他，1=淘宝，2=天猫]
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 抓取地址
        /// </summary>
        public string GrabUrl { get; set; }
    }

    /// <summary>
    /// 淘宝/天猫商品详情数据实体
    /// </summary>
    public class SkuInfo
    {
        public string skuId { get; set; }

        public string Version { get; set; }

        public string Color { get; set; }
    }

    /// <summary>
    /// 描述信息
    /// </summary>
    public class Description
    {
        public string date { get; set; }
        public string content { get; set; }
    }

    /// <summary>
    /// 商品基本信息
    /// </summary>
    public class TMProductInfo
    {
        /// <summary>
        /// 广告词
        /// </summary>
        public string ShortDescription { get; set; }
        public Api api { get; set; }
        public Detail detail { get; set; }
        public ItemDO itemDO { get; set; }
        public ValItemInfo valItemInfo { get; set; }
        public PropertyPics propertyPics { get; set; }
        public List<SkuProperty> skuProperty { get; set; }
        /// <summary>
        /// 获取规格动态链接
        /// </summary>
        public string initApi { get; set; }
        /// <summary>
        /// 没有SKU时特殊的处理
        /// </summary>
        public NoSkuData NoSkuInfo { get; set; }
    }

    /// <summary>
    /// 接口信息
    /// </summary>
    public class Api
    {
        public string descUrl { get; set; }
        public string fetchDcUrl { get; set; }
        public string httpsDescUrl { get; set; }
        public string productdescriptions { get; set; }
    }

    /// <summary>
    /// 部分详情信息
    /// </summary>
    public class Detail
    {
        public string defaultItemPrice { get; set; }
    }

    public class ItemDO
    {
        public string title { get; set; }
    }

    /// <summary>
    /// 主图5张，规格图片
    /// </summary>
    public class PropertyPics
    {
        public Dictionary<string, string> SkuPicsDic { get; set; }//规格属性图片
        public List<string> ImagesPics { get; set; }//主图
    }

    public class ValItemInfo
    {
        public List<SkuItem> skuList { get; set; }
        public List<SkuMap> skuMap { get; set; }
    }

    public class SkuItem
    {
        public string names { get; set; }
        public string pvs { get; set; }
        public string skuId { get; set; }
        public string sku { get { return this.skuId; } }
    }

    public class SkuMap
    {
        public string price { get; set; }
        public string priceCent { get; set; }
        public string skuId { get; set; }
        public string stock { get; set; }
        public string sku { get; set; }
        public string oversold { get; set; }
    }

    public class SkuProperty
    {
        public string PropertyName { get; set; }
        public string PropertyCode { get; set; }
        public List<SkuPropertyValue> SkuPropertyValues { get; set; }
        /// <summary>
        /// 1=颜色，2=尺码，3=规格
        /// </summary>
        public int Type { get; set; }

    }

    public class SkuPropertyValue
    {
        public string PropertyValueName { get; set; }
        public string PropertyValueCode { get; set; }
        public string PropertyValueIcon { get; set; }
        public string PropertyCode { get; set; }
        public long ValueId { get; set; }
    }

    public class TaoBaoSKUMap
    {
        public string holdQuantity { get; set; }
        public string oversold { get; set; }
        public string sellableQuantity { get; set; }
        public string stock { get; set; }
    }

    /// <summary>
    /// 动态解析天猫SKU库存和价格实体
    /// </summary>
    public class SkuQuantity
    {
        public string quantity { get; set; }
        public string totalQuantity { get; set; }
        public string type { get; set; }
    }
    public class PriceInfo
    {
        public string areaSold { get; set; }
        public string onlyShowOnePrice { get; set; }
        public string price { get; set; }
        public string sortOrder { get; set; }
    }
    /// <summary>
    /// 没有规格的商品特殊处理
    /// </summary>
    public class NoSkuData
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public string price { get; set; }
        public DynStock dynStock { get; set; }
    }

    public class DynStock
    {
        public string holdQuantity { get; set; }
        public string sellableQuantity { get; set; }
        public string stock { get; set; }
        public string stockType { get; set; }
    }
}
