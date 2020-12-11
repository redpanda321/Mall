using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class Topic
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string FrontCoverImage { get; set; }
        public string TopImage { get; set; }
        public string BackgroundImage { get; set; }
        public Mall.Core.PlatformType Platform { get; set; }
        public string Tags { get; set; }
        public long ShopId { get; set; }
        public bool IsRecommend { get; set; }
        public string SelfDefineText { get; set; }
        public List<TopicModule> TopicModuleInfo { get; set; }
    }
    public class TopicModule
    {
        public long Id { get; set; }
        public long TopicId { get; set; }
        public string Name { get; set; }
        public int TitleAlign { get; set; }
        public List<TopicProduct> ModuleProductInfo { get; set; }
    }

    public class TopicProduct
    {
        public long Id { get; set; }
        public long ModuleId { get; set; }
        public long ProductId { get; set; }
        public long DisplaySequence { get; set; }
    }
}
