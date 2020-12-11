using Mall.CommonModel;
using System;

namespace Mall.DTO
{
    public class OrderRefundlog
	{
		public long Id { get; set; }
		public long RefundId { get; set; }
		public string Operator { get; set; }
		public System.DateTime OperateDate { get; set; }
        public string ShowOperateDate { get; set; }

        public string OperateContent { get; set; }
		public Nullable<int> ApplyNumber { get; set; }
		public OrderRefundStep Step { get; set; }

		public string Remark { get; set; }
	}
}
