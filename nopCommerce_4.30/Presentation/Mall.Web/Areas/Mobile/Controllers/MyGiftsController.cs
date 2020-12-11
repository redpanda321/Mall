using AutoMapper;
using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Areas.Mobile.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Mobile.Controllers
{
    /// <summary>
    /// 拼团
    /// </summary>
    public class MyGiftsController : BaseMobileMemberController
    {
        private IProductService _iProductService;
        private ITypeService _iTypeService;
        private IGiftsOrderService _iGiftsOrderService;
        private IExpressService _iExpressService;
        public MyGiftsController(IProductService iProductService, ITypeService iTypeService
            , IGiftsOrderService iGiftsOrderService, IExpressService iExpressService
            )
        {
            _iProductService = iProductService;
            _iTypeService = iTypeService;
            _iGiftsOrderService = iGiftsOrderService;
            _iExpressService = iExpressService;
        }

        #region 礼品订单列表
        /// <summary>
        /// 礼品订单列表
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderList(int? status = null)
        {
            ViewBag.status = status;
            return View();
        }

        /// <summary>
        /// 获取礼品订单列表
        /// </summary>
        /// <param name="skey"></param>
        /// <param name="status"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetOrderList(string skey = "", Mall.Entities.GiftOrderInfo.GiftOrderStatus? status = null, int page = 1, int pagesize = 10)
        {
            if (CurrentUser == null)
            {
                throw new MallException("错误的用户信息");
            }
            int rows = pagesize;
            GiftsOrderQuery query = new GiftsOrderQuery();
            query.Skey = skey;
            if (status != null)
            {
                if ((int)status != 0)
                {
                    query.Status = status;
                }
            }
            query.UserId = CurrentUser.Id;
            query.PageSize = rows;
            query.PageNo = page;
            var orderdata = _iGiftsOrderService.GetOrders(query);
            List<Mall.Entities.GiftOrderInfo> orderlist = orderdata.Models.ToList();
            //_iGiftsOrderService.OrderAddUserInfo(orderlist);
            var result = orderlist.ToList();
           // Mapper.CreateMap<GiftOrderInfo, GiftsOrderDtoModel>();
           // Mapper.CreateMap<GiftOrderItemInfo, GiftsOrderItemDtoModel>();
            List<GiftsOrderDtoModel> pagedata = new List<GiftsOrderDtoModel>();
            foreach (var order in result)
            {
                order.Address = ClearHtmlString(order.Address);
                order.CloseReason = ClearHtmlString(order.CloseReason);
                order.UserRemark = ClearHtmlString(order.UserRemark);

                var tmpordobj = order.Map<GiftsOrderDtoModel>();
                tmpordobj.Items = new List<GiftsOrderItemDtoModel>();
                var orderitems = _iGiftsOrderService.GetOrderItemByOrder(order.Id);
                foreach (var subitem in orderitems)
                {
                    var tmporditemobj = subitem.Map<GiftsOrderItemDtoModel>();
                    tmporditemobj.DefaultImage = MallIO.GetRomoteProductSizeImage(tmporditemobj.ImagePath, 1, ImageSize.Size_150.GetHashCode());
                    tmpordobj.Items.Add(tmporditemobj);
                }
                pagedata.Add(tmpordobj);
            }

            var pageresult = SuccessResult(data: new
            {
                total = orderdata.Total,
                rows = pagedata
            });
            return pageresult;
        }
        /// <summary>
        /// 获取订单综合数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetOrderCount()
        {
            if (CurrentUser == null)
            {
                throw new MallException("错误的用户信息");
            }
            GiftsOrderAggregateDataModel result = new GiftsOrderAggregateDataModel();
            long curid = CurrentUser.Id;
            result.AllCount = _iGiftsOrderService.GetOrderCount(null, curid);
            result.WaitDeliveryCount = _iGiftsOrderService.GetOrderCount(Mall.Entities.GiftOrderInfo.GiftOrderStatus.WaitDelivery, curid);
            result.WaitReceivingCount = _iGiftsOrderService.GetOrderCount(Mall.Entities.GiftOrderInfo.GiftOrderStatus.WaitReceiving, curid);
            result.FinishCount = _iGiftsOrderService.GetOrderCount(Mall.Entities.GiftOrderInfo.GiftOrderStatus.Finish, curid);
            result.success = true;
            return Json(result);
        }
        /// <summary>
        /// 获取订单信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="orderid"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetOrder(long id)
        {
            if (CurrentUser == null)
            {
                throw new MallException("错误的用户信息");
            }
            var orderdata = _iGiftsOrderService.GetOrder(id, CurrentUser.Id);
            if (orderdata == null)
            {
                throw new MallException("错误的订单编号");
            }
            //Mapper.CreateMap<GiftOrderInfo, GiftsOrderDtoModel>();
           // Mapper.CreateMap<GiftOrderItemInfo, GiftsOrderItemDtoModel>();

            //_iGiftsOrderService.OrderAddUserInfo(orderlist);
            orderdata.Address = ClearHtmlString(orderdata.Address);
            orderdata.CloseReason = ClearHtmlString(orderdata.CloseReason);
            orderdata.UserRemark = ClearHtmlString(orderdata.UserRemark);

            var result = orderdata.Map<GiftsOrderDtoModel>();
            result.Items = new List<GiftsOrderItemDtoModel>();
            var orderitems = _iGiftsOrderService.GetOrderItemByOrder(orderdata.Id);
            foreach (var subitem in orderitems)
            {
                var tmporditemobj = subitem.Map<GiftsOrderItemDtoModel>();
                tmporditemobj.DefaultImage = MallIO.GetRomoteProductSizeImage(tmporditemobj.ImagePath, 1, ImageSize.Size_150.GetHashCode());
                result.Items.Add(tmporditemobj);
            }
            result.success = true;
            return Json(result);
        }
        /// <summary>
        /// 获取物流信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetExpressInfo(long orderId)
        {
            if (CurrentUser == null)
            {
                throw new MallException("错误的用户信息");
            }
            var order = _iGiftsOrderService.GetOrder(orderId, CurrentUser.Id);
            var expressData = _iExpressService.GetExpressData(order.ExpressCompanyName, order.ShipOrderNumber);

            if (expressData == null)
            {
                return Json(new { success = false, ExpressNum = order.ShipOrderNumber, ExpressCompanyName = order.ExpressCompanyName, Comment = "" });
            }

            if (expressData.Success)
                expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列
            var json = new
            {
                Success = expressData.Success,
                Msg = expressData.Message,
                Data = expressData.ExpressDataItems.Select(item => new
                {
                    time = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    content = item.Content
                })
            };
            return Json(new { success = true, ExpressNum = order.ShipOrderNumber, ExpressCompanyName = order.ExpressCompanyName, Comment = json });
        }
        /// <summary>
        /// 确认收货
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult ConfirmOrderOver(long OrderId)
        {
            if (CurrentUser == null)
            {
                throw new MallException("错误的用户信息");
            }
            if (OrderId < 1)
            {
                throw new MallException("错误的订单编号");
            }
            Result result = new Result();
            _iGiftsOrderService.ConfirmOrder(OrderId, CurrentUser.Id);
            result.success = true;
            result.code = 1;
            result.msg = "订单完成";
            return Json(result);
        }
        #endregion

        /// <summary>
        /// 礼品订单详情
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderDetail(long id)
        {
            var order = _iGiftsOrderService.GetOrder(id,CurrentUser.Id);
            if (order == null)
            {
                throw new MallException("错误的参数");
            }
            var orderlist = new GiftOrderInfo[] { order };
            _iGiftsOrderService.OrderAddUserInfo(orderlist);
            order = orderlist.FirstOrDefault();
            var expressData = _iExpressService.GetExpressData(order.ExpressCompanyName, order.ShipOrderNumber);
            if (expressData.Success)
                expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列
            MyGiftsOrderDetailModel result = new MyGiftsOrderDetailModel();
            result.OrderData = order;
            result.OrderItems = _iGiftsOrderService.GetOrderItemByOrder(id);
            result.ExpressData = expressData;
            return View(result);
        }


        #region 私有
        /// <summary>
        /// 计算最大页数
        /// </summary>
        /// <param name="total"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        private int GetMaxPage(int total, int pagesize)
        {
            int result = 1;
            if (total > 0 && pagesize > 0)
            {
                result = (int)Math.Ceiling((double)total / (double)pagesize);
            }
            return result;
        }
        /// <summary>
        /// 获取收货地址
        /// </summary>
        /// <param name="regionId"></param>
        /// <returns></returns>
        private Entities.ShippingAddressInfo GetShippingAddress(long? regionId)
        {
            Entities.ShippingAddressInfo result = null;
            var _iShippingAddressService = ServiceApplication.Create<IShippingAddressService>();
            if (regionId != null)
            {
                result = _iShippingAddressService.GetUserShippingAddress((long)regionId);
            }
            else
            {
                if (CurrentUser != null)
                {
                    result = _iShippingAddressService.GetDefaultUserShippingAddressByUserId(CurrentUser.Id);
                }
            }
            return result;
        }
        /// <summary>
        /// 清理引号类字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string ClearHtmlString(string str)
        {
            string result = str;
            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.Replace("'", "&#39;");
                result = result.Replace("\"", "&#34;");
                result = result.Replace(">", "&gt;");
                result = result.Replace("<", "&lt;");
            }
            return result;
        }
        #endregion
    }
}