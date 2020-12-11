using Mall.DTO;

namespace Mall.Web.Areas.Mobile.Models
{
    public class OrderSubmitGroupModel
    {
        public long GroupActionId { get; set; }
        public long? GroupId { get; set; }
        public MobileOrderDetailConfirmModel ConfirmModel { get; set; }
        /// <summary>
        /// 是否为微信端
        /// </summary>
        public bool IsWeiXin { get; set; }
    }
}