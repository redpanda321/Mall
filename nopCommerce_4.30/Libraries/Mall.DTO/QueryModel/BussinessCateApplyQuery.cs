using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO.QueryModel
{
    public class BussinessCateApplyQuery : QueryBase
    {
        /// <summary>
        /// 申请的店铺
        /// </summary>
        public long? shopId { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        public Entities.BusinessCategoryApplyInfo.BusinessCateApplyStatus? Status { set; get; }

        /// <summary>
        /// 申请的店铺
        /// </summary>
        public string ShopName{ get; set; }
    }
}
