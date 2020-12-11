using Mall.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Mall.Web.Areas.Admin.Models.Distribution
{
    public class DistributorGradeModel
    {
        public long Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Required(ErrorMessage = "请输入销售员等级名称")]
        [StringLength(8,ErrorMessage = "销售员等级名称最多能输入8个字")]
        public string GradeName { get; set; }
        /// <summary>
        /// 条件
        /// </summary>
        [Required(ErrorMessage = "请输入销售员佣金门槛")]
        [Range(0, 99999, ErrorMessage = "您输入的信息有误，此处只能输入大于0 小于 99999的整数")]
        [RegularExpression("^\\d+$", ErrorMessage = "您输入的信息有误，此处只能输入大于等于0 小于 99999的整数")]
        public int Quota { get; set; }
    }
}