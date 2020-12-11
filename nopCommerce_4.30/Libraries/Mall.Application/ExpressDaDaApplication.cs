using Mall.Core;
using Mall.IServices;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    /// <summary>
    /// 达达物流
    /// </summary>
    public class ExpressDaDaApplication
    {
        #region 字段
      //  private static IMemberService _iMemberService =  EngineContext.Current.Resolve<IMemberService>();
      //  private static IExpressDaDaService _iExpressDaDaService =  EngineContext.Current.Resolve<IExpressDaDaService>();


        private static IMemberService _iMemberService =  EngineContext.Current.Resolve<IMemberService>();
        private static IExpressDaDaService _iExpressDaDaService =  EngineContext.Current.Resolve<IExpressDaDaService>();
        #endregion
        /// <summary>
        /// 取消发单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="cancel_reason"></param>
        public static void SetOrderCancel(long orderId, string cancel_reason)
        {
            _iExpressDaDaService.SetOrderCancel(orderId, cancel_reason);
        }
        /// <summary>
        /// 已发货
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="client_id"></param>
        public static void SetOrderWaitTake(long orderId, string client_id)
        {
            _iExpressDaDaService.SetOrderWaitTake(orderId, client_id);
        }
        /// <summary>
        /// 已完成
        /// </summary>
        /// <param name="orderId"></param>
        public static void SetOrderFinished(long orderId)
        {
            var order = OrderApplication.GetOrder(orderId);
            var member = MemberApplication.GetMember(order.UserId);
            OrderApplication.MembeConfirmOrder(orderId, member.UserName);
        }
        /// <summary>
        /// 设置订单达达状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        /// <param name="client_id">运单号</param>
        public static void SetOrderDadaStatus(long orderId,int status,string client_id)
        {
            _iExpressDaDaService.SetOrderDadaStatus(orderId, status, client_id);
        }
    }
}
