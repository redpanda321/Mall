namespace Mall.Web.Areas.Admin.Models
{
    public class GiftsAppManageModel
    {
        /// <summary>
        /// 选中的大转盘
        /// </summary>
        public long RouletteId { get; set; }
        /// <summary>
        /// 选中的刮刮卡
        /// </summary>
        public long ScratchCardId { get; set; }
    }
}