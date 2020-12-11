using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class ProductDescriptionInfo
    {
        /// <summary>
        /// 显示手机端描述
        /// <para>后台未添加手机端描述，将显示电脑端描述</para>
        /// </summary>
        [ResultColumn]
        public string ShowMobileDescription
        {
            get
            {
                string result = "";
                if (this != null)
                {
                    result = this.MobileDescription;
                    if (string.IsNullOrWhiteSpace(result)) result = this.Description;
                }
                return result;
            }
        }
    }
}
