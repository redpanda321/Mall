using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.Web.Areas.Mobile.Models
{
    public class MySubordinateViewModel
    {
        public int MaxLevel { get; set; }
        public Dictionary<int, MySubordinateLevelViewModel> Levels { get; set; }
    }
    public class MySubordinateLevelViewModel {
        public string Name { get; set; }
        public int Count { get; set; }
    }
}