using Mall.CommonModel;
using Mall.Entities;

namespace Mall.Web.Areas.Web.Models
{
    public class RegisterIndexPageModel
    {
        public bool MobileVerifOpen { get; set; }
        public long Introducer { get; set; }
        public RegisterTypes RegisterType { get; set; }
        public bool RegisterEmailRequired { get; set; }
        public bool EmailVerifOpen { get; set; }
    }

}