using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.API.Model.ParamsModel
{
    public class UserCenterChangePasswordModel
    {
        public string oldPassword { get; set; }
        public string password { get; set; }
    }
}
