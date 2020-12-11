
using System.ComponentModel.DataAnnotations;

namespace Mall.Web.Areas.SellerAdmin.Models
{
    public class WdgjApiModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "接入码必须填写")]
        public string uCode { get; set; }

        [Required(ErrorMessage = "效验码必须填写")]
        public string uSign { get; set; }
    }
}