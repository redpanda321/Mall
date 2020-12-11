using Mall.CommonModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;

namespace Mall.Service
{
    public class ExpressDaDa : ServiceBase, IExpressDaDaService
    {

        /// <summary>
        /// 取消发单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="cancel_reason"></param>
        public void SetOrderCancel(long orderId, string cancel_reason)
        {
            //var order = DbFactory.Default.Get<OrderInfo>().Where(d => d.Id == orderId).FirstOrDefault();
            //if (order != null)
            //{
            //    order.DadaStatus = DadaStatus.Cancel.GetHashCode();
            //    order.OrderStatus = OrderInfo.OrderOperateStatus.WaitDelivery;
            //    DbFactory.Default.Update(order);
            //}
            DbFactory.Default.Set<OrderInfo>().Set(n => n.DadaStatus, DadaStatus.Cancel.GetHashCode()).Set(n => n.OrderStatus, OrderInfo.OrderOperateStatus.WaitDelivery)
                .Where(n => n.Id == orderId).Succeed();
        }

        /// <summary>
        /// 已发货
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="client_id"></param>
        public void SetOrderWaitTake(long orderId, string client_id)
        {
            var order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == orderId).FirstOrDefault();
            order.DeliveryType = CommonModel.DeliveryType.CityExpress;
            order.ExpressCompanyName = "同城合作物流";
            order.ShipOrderNumber = client_id;
            order.ShippingDate = DateTime.Now;
            order.LastModifyTime = DateTime.Now;
            order.DadaStatus = DadaStatus.WaitTake.GetHashCode();
            DbFactory.Default.Update(order);
        }
        /// <summary>
        /// 设置订单达达状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        /// <param name="client_id">运单号</param>
        public void SetOrderDadaStatus(long orderId, int status, string client_id)
        {
            var order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == orderId).FirstOrDefault();
            if (order != null)
            {
                order.DadaStatus = status;
                order.ShipOrderNumber = client_id;
                if (status == (int)DadaStatus.Cancel)
                {
                    order.DeliveryType = CommonModel.DeliveryType.Express;
                    order.ShipOrderNumber = "";
                    order.ExpressCompanyName = "";
                }
                DbFactory.Default.Update(order);
            }
        }
    }
}