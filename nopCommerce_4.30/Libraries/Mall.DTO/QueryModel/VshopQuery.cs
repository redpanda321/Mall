
using Mall.Entities;

namespace Mall.DTO.QueryModel
{
    public partial class VshopQuery : QueryBase
    {
        public string Name { get; set; }

        public Entities.VShopExtendInfo.VShopExtendType? VshopType { get; set; }

        public Entities.VShopExtendInfo.VShopExtendState VshopState { get; set; }

        /// <summary>
        /// 要排除的VshopId
        /// </summary>
        public long? ExcepetVshopId { set; get; }

        /// <summary>
        /// 微店状态
        /// </summary>
        public VShopInfo.VShopStates? Status { get; set; }

        /// <summary>
        /// 微店开启状态
        /// </summary>
        public bool? IsOpen { get; set; }

    }
}
