using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;


namespace Mall.Web.Areas.Web.Controllers
{
    public class PayStateController : BaseAsyncController
    {
        public void CheckAsync(string orderIds)
        {
           // AsyncManager.OutstandingOperations.Increment();
            int interval = 200;//定义刷新间隔为200ms
            int maxWaitingTime = 10 * 1000;//定义最大等待时间为15s
            Task.Factory.StartNew(() =>
               {
                   string payStateKey = CacheKeyCollection.PaymentState(string.Join(",", orderIds));//获取支付状态缓存键
                   int time = 0;
                   while (true)
                   {
                       //检查是否已经建立缓存
                       var payStateObj = Cache.Exists(payStateKey);
                       if (!payStateObj)
                       {//没有进入缓存
                           var orderIdArr = orderIds.Split(',').Select(item => long.Parse(item));
                           //检查对应订单是否已经支付

                           using (var service = ServiceApplication.Create<IOrderService>())
                           {
                               var payDone = !service.GetOrders(orderIdArr).Any(item => item.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitPay);
                               Cache.Insert(payStateKey, payDone, 15);//标记支付状态
                           }
                       }

                       //检查缓存的值，如果已支付则直接结束，若未替并小于15秒则阻塞200ms后重复检查，否则直接结束
                       var payState = Cache.Get<bool>(payStateKey);
                       if (payState)
                       {//如果已成功支付，则调用成功
                           //AsyncManager.Parameters["done"] = true;
                           break;
                       }
                       if (time > maxWaitingTime)
                       {//大于15秒
                         //  AsyncManager.Parameters["done"] = false;
                           break;
                       }
                       else
                       {
                           time += interval;
                           System.Threading.Thread.Sleep(interval);
                       }
                   }
                  // AsyncManager.OutstandingOperations.Decrement();
               });
        }


        public JsonResult CheckCompleted(bool done)
        {
            return Json(new { success = done });
        }

        public void CheckChargeAsync(string orderIds)
        {
            //AsyncManager.OutstandingOperations.Increment();
            int interval = 200;//定义刷新间隔为200ms
            int maxWaitingTime = 10 * 1000;//定义最大等待时间为15s
            Task.Factory.StartNew(() =>
            {
                string payStateKey = CacheKeyCollection.PaymentState(orderIds);//获取支付状态缓存键
                int time = 0;
                while (true)
                {
                    //检查是否已经建立缓存
                    var payStateObj = Cache.Exists(payStateKey);
                    if (!payStateObj)
                    {//没有进入缓存
                        //检查对应订单是否已经支付

                        using (var service = ServiceApplication.Create<IMemberCapitalService>())
                        {
                            var model = service.GetChargeDetail(long.Parse(orderIds));
                            var payDone = model != null && model.ChargeStatus == Mall.Entities.ChargeDetailInfo.ChargeDetailStatus.ChargeSuccess;
                            Cache.Insert(payStateKey, payDone, 15);//标记支付状态
                        }
                    }

                    //检查缓存的值，如果已支付则直接结束，若未替并小于15秒则阻塞200ms后重复检查，否则直接结束
                    var payState = Cache.Get<bool>(payStateKey);
                    if (payState)
                    {//如果已成功支付，则调用成功
                        //AsyncManager.Parameters["done"] = true;
                        break;
                    }
                    if (time > maxWaitingTime)
                    {//大于15秒
                        //AsyncManager.Parameters["done"] = false;
                        break;
                    }
                    else
                    {
                        time += interval;
                        System.Threading.Thread.Sleep(interval);
                    }
                }
                //AsyncManager.OutstandingOperations.Decrement();
            });
        }

        public JsonResult CheckChargeCompleted(bool done)
        {
            return Json(new { success = done });
        }

        /// <summary>
        /// 检查店铺续费或交保证金是否支付成功
        /// </summary>
        /// <param name="orderIds"></param>
        public void CheckShopAsync(string orderIds)
        {
            //AsyncManager.OutstandingOperations.Increment();
            //Log.Error("orderIds:" + orderIds);
            int interval = 200;//定义刷新间隔为200ms
            int maxWaitingTime = 10 * 1000;//定义最大等待时间为15s
            Task.Factory.StartNew(() =>
            {
                string payStateKey = CacheKeyCollection.PaymentState(orderIds);//获取支付状态缓存键
                //Log.Error("payStateKey:" + payStateKey);
                int time = 0;
                while (true)
                {
                    //检查是否已经建立缓存
                    var payStateObj = Cache.Exists(payStateKey);
                    if (!payStateObj)
                    {//没有进入缓存
                        //检查对应订单是否已经支付
                        //int shopId = 0;
                        //if(orderIds!=null && orderIds.Length > 12)
                        //{
                        //    int.TryParse(orderIds.Substring(orderIds.Length - 12), out shopId);

                        //    using (var service = ServiceApplication.Create<IShopService>())
                        //    {
                        //        var payDone = service.GetShopRenewRecordIsExists(shopId,15);//15秒内
                        //        Cache.Insert(payStateKey, payDone, 15);//标记支付状态
                        //    }
                        //}
                    }

                    //检查缓存的值，如果已支付则直接结束，若未替并小于15秒则阻塞200ms后重复检查，否则直接结束
                    var payState = Cache.Get<bool>(payStateKey);
                    if (payState)
                    {//如果已成功支付，则调用成功
                        //AsyncManager.Parameters["done"] = true;
                        break;
                    }
                    if (time > maxWaitingTime)
                    {//大于15秒
                       // AsyncManager.Parameters["done"] = false;
                        break;
                    }
                    else
                    {
                        time += interval;
                        System.Threading.Thread.Sleep(interval);
                    }
                }
                //AsyncManager.OutstandingOperations.Decrement();
            });
        }

        public JsonResult CheckShopCompleted(bool done)
        {
            return Json(new { success = done });
        }

    }
}