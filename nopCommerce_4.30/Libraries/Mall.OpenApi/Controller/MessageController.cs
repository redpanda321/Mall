using Mall.Application;
using Mall.Core.Plugins.Message;
using Mall.OpenApi.Model.Parameter;
using Mall.OpenApi.Model.Parameter.Message;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Mall.OpenApi
{
    /// <summary>
    /// 消息接口
    /// </summary>
    [Route("OpenApi")]
    public class MessageController : OpenAPIController
    {

        [HttpPost]
        public object SendMessageOnDistributorCommissionSettled(DistributorCommissionSettledArgs args)
        {
            MessageApplication.SendMessageOnDistributorCommissionSettled(args.UserId, args.Amount, args.SettlementDate);
            return new { success = true };
        }

        [HttpPost]
        public object SendMessageOnRefundDeliver(RefundDeliverArgs args)
        {
            MessageApplication.SendMessageOnRefundDeliver(args.UserId, args.Info, args.RefundId);
            return new { success = true };
        }

        [HttpPost]
        public object SendMessageOnRefundApply(RefundApply args)
        {
            MessageApplication.SendMessageOnRefundApply(args.UserId, args.Info, args.RefundMode, args.RefundId);
            return new { success = true };
        }

        [HttpPost]
        public object ConfirmRefund(ConfirmRefund args)
        {
            string notifyurl = CurrentUrlHelper.CurrentUrlNoPort() + "/Pay/RefundNotify/{0}";
            var result = RefundApplication.ConfirmRefund(args.RefundId, args.Remark, "", notifyurl);
            return new { success = true };
        }
    }
}
