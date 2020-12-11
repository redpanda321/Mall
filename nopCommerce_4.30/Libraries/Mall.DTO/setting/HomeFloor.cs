using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class HomeFloor
    {
        public long Id { get { return Info.Id; } }
        public HomeFloorInfo Info { get; set; }
        public List<FloorCategoryInfo> Categorys { get; set; }
        public List<FloorTablInfo> Tabs { get; set; }
        public List<FloorBrandInfo> Brands { get; set; }

        public List<FloorTopicInfo> Topics { get; set; }

        public List<FloorProductInfo> Products { get; set; }
    }
}
