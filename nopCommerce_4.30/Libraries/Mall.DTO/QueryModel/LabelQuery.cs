using System.Collections.Generic;

namespace Mall.DTO.QueryModel
{
    public class LabelQuery : QueryBase
    {
        public string LabelName { get; set; }
        /// <summary>
        /// 标签ID列表
        /// </summary>
        public IEnumerable<long> LabelIds { get; set; }

    }
}
