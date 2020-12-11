using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Mall.DTO
{
    public class ShopShipper
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public bool IsDefaultSendGoods { get; set; }
        public bool IsDefaultGetGoods { get; set; }
        public bool IsDefaultVerification { get; set; }
        [Required(ErrorMessage = "请填写发货点名称")]
        public string ShipperTag { get; set; }
        [Required(ErrorMessage = "请填写发货人姓名")]
        public string ShipperName { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "请选择发货点所在地区")]
        public int RegionId { get; set; }

        public string RegionStr { get; set; }
        public string Address { get; set; }
        [Required(ErrorMessage = "请填写发货人电话")]
        public string TelPhone { get; set; }
        public string Zipcode { get; set; }
        public string WxOpenId { get; set; }
        public Nullable<float> Longitude { get; set; }
        public Nullable<float> Latitude { get; set; }
    }
}
