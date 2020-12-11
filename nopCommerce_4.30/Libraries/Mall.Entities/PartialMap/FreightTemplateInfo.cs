using Mall.CommonModel;
using NPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    public partial class FreightTemplateInfo
    {
        /// <summary>
        /// 仓库地址
        /// <para>手动补充</para>
        /// </summary>
        [ResultColumn]
        public string DepotAddress { get; set; }


        /// <summary>
        /// 获取发货时间
        /// </summary>
        public SendTimeEnum? GetSendTime
        {
            get
            {
                SendTimeEnum? result = null;
                if (!string.IsNullOrWhiteSpace(this.SendTime))
                {
                    int num = 0;
                    if (int.TryParse(this.SendTime, out num))
                    {
                        if (Enum.IsDefined(typeof(SendTimeEnum), num))
                        {
                            result = (SendTimeEnum)num;
                        }
                    }
                }
                return result;
            }
        }

       
        /// <summary>
        /// Id == FreightTemplateId 
        /// </summary>
        [ResultColumn]
        [Obsolete("关联属性移除遗留")]
        public List<FreightAreaContentInfo> FreightAreaContentInfo
        {
            get; set;
        }

      
        /// <summary>
        /// Id == TemplateId 
        /// </summary>
        [ResultColumn]
        [Obsolete("关联属性移除遗留")]
        public List<ShippingFreeGroupInfo> ShippingFreeGroupInfo { get; set; }
    }
}
