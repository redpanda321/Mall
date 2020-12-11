using AutoMapper;
using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.SmallProgAPI.Model;
using Mall.SmallProgAPI.Model.ParaModel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Mall.SmallProgAPI
{
    public class GiftsController: BaseApiController
    {
        /// <summary>
        /// 获取积分商城首页信息
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetIndexData")]
        public object GetIndexData(string openId = "")
        {
            //轮播图
            dynamic result = new System.Dynamic.ExpandoObject();
            var slidads = SlideApplication.GetSlidAds(0, Entities.SlideAdInfo.SlideAdType.AppGifts).ToList();
            foreach (var item in slidads)
            {
                item.ImageUrl = MallIO.GetRomoteImagePath(item.ImageUrl);
            }
            result.SlideAds = slidads;

            //大转盘刮刮卡
            var malllist = new List<IntegralMallAdInfo>();
            var robj = GiftApplication.GetAdInfo(IntegralMallAdInfo.AdActivityType.Roulette, IntegralMallAdInfo.AdShowPlatform.APP);
            if (robj != null)
            {
                //robj.LinkUrl = Request.RequestUri.Scheme + "://" + Request.RequestUri.Authority + "/m-wap/BigWheel/index/" + robj.ActivityId;
                robj.LinkUrl = "/m-wap/BigWheel/index/" + robj.ActivityId;
                malllist.Add(robj);
            }
            var cobj = GiftApplication.GetAdInfo(IntegralMallAdInfo.AdActivityType.ScratchCard, IntegralMallAdInfo.AdShowPlatform.APP);
            if (cobj != null)
            {
                //cobj.LinkUrl = Request.RequestUri.Scheme + "://" + Request.RequestUri.Authority + "/m-wap/ScratchCard/index/" + cobj.ActivityId;
                cobj.LinkUrl = "/m-wap/ScratchCard/index/" + cobj.ActivityId;
                malllist.Add(cobj);
            }
            result.WeiActives = malllist;

            //首页礼品
            GiftQuery query = new GiftQuery();
            query.skey = "";
            query.status = Mall.Entities.GiftInfo.GiftSalesStatus.Normal;
            query.PageSize = 4;
            query.PageNo = 1;
            QueryPageModel<GiftModel> gifts = GiftApplication.GetGifts(query);
            result.HomeGiftses = gifts.Models.ToList();
            result.HasMoreGifts = gifts.Total > 4;
            foreach (var item in result.HomeGiftses)
            {
                item.DefaultShowImage = MallIO.GetRomoteImagePath(item.GetImage(ImageSize.Size_350));
            }

            //积分优惠券
            var coupons = CouponApplication.GetIntegralCoupons(1, 3);
         //   Mapper.CreateMap<CouponInfo, CouponGetIntegralCouponModel>();
            if (coupons.Models.Count > 0)
            {
                var datalist = coupons.Models.ToList();
                var objlist = new List<CouponGetIntegralCouponModel>();
                foreach (var item in datalist)
                {
                    var tmp = item.Map<CouponGetIntegralCouponModel>();
                    tmp.ShowIntegralCover = MallIO.GetRomoteImagePath(item.IntegralCover);
                    var vshopobj = VshopApplication.GetVShopByShopId(tmp.ShopId);
                    if (vshopobj != null)
                    {
                        tmp.VShopId = vshopobj.Id;
                        //优惠价封面为空时，取微店Logo，微店Logo为空时，取商城微信Logo
                        if (string.IsNullOrWhiteSpace(tmp.ShowIntegralCover))
                        {
                            if (!string.IsNullOrWhiteSpace(vshopobj.WXLogo))
                            {
                                tmp.ShowIntegralCover = MallIO.GetRomoteImagePath(vshopobj.WXLogo);
                            }
                        }
                    }
                    if (string.IsNullOrWhiteSpace(tmp.ShowIntegralCover))
                    {
                        var siteset = SiteSettingApplication.SiteSettings;
                        tmp.ShowIntegralCover = MallIO.GetRomoteImagePath(siteset.WXLogo);
                    }
                    objlist.Add(tmp);
                }
                result.IntegralCoupons = objlist.ToList();
                result.HasMoreIntegralCoupons = coupons.Total > 3;
            }

            result.HasLogined = false;

            //用户积分与等级
            if (CurrentUser != null)
            {
                //登录后处理会员积分
                var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
                var userGrade = MemberGradeApplication.GetMemberGradeByUserIntegral(userInte.HistoryIntegrals);
                result.MemberAvailableIntegrals = userInte.AvailableIntegrals;
                result.MemberGradeName = userGrade.GradeName;
                result.HasLogined = true;
            }
            return Json(result);
        }
        /// <summary>
        /// 获取礼品列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetList")]
        public object GetList(int pageNo = 1, int pageSize = 10)
        {
            
            //礼品数据
            GiftQuery query = new GiftQuery();
            query.skey = "";
            query.status = Mall.Entities.GiftInfo.GiftSalesStatus.Normal;
            query.PageSize = pageSize;
            query.PageNo = pageNo;
            QueryPageModel<GiftModel> gifts = GiftApplication.GetGifts(query);
            var list = gifts.Models.ToList();
            foreach (var item in list)
            {
                item.DefaultShowImage = MallIO.GetRomoteImagePath(item.GetImage(ImageSize.Size_350));
            }
            return Json(new
            {
                Gifts = list,
                Total = gifts.Total,
                MaxPage = GetMaxPage(gifts.Total, pageSize)
            });
        }
        /// <summary>
        /// 礼品详情
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetGiftDetail")]
        public object GetGiftDetail(long id)
        {
            GiftsDetailModel result = new GiftsDetailModel();
            var data = GiftApplication.GetById(id);
           // Mapper.CreateMap<Entities.GiftInfo, GiftsDetailModel>();
            result = data.Map<GiftsDetailModel>();
            if (data == null)
            {
                throw new Exception("礼品信息无效！");
            }
            result.EndDateStr = data.EndDate.ToString("yyyy-MM-dd HH:mm:ss");
            string tmpdefaultimg = result.GetImage(ImageSize.Size_100);
            result.DefaultShowImage = MallIO.GetRomoteImagePath(tmpdefaultimg);
            result.Images = new List<string>();
            result.Description = result.Description.Replace("src=\"/Storage/", "src=\"" + Core.MallIO.GetRomoteImagePath("/Storage/") + "/");

            //补充图片信息
            for (var _n = 1; _n < 6; _n++)
            {
                string _imgpath = data.ImagePath + "/" + _n.ToString() + ".png";
                if (MallIO.ExistFile(_imgpath))
                {
                    var tmp = MallIO.GetRomoteImagePath(result.GetImage(ImageSize.Size_500, _n));
                    result.Images.Add(tmp);
                }
            }

            #region 礼品是否可兑
            result.CanBuy = true;
            //礼品信息
            if (result.CanBuy)
            {
                if (result.GetSalesStatus != Mall.Entities.GiftInfo.GiftSalesStatus.Normal)
                {
                    result.CanBuy = false;
                    result.CanNotBuyDes = "礼品" + result.ShowSalesStatus;
                }
            }

            if (result.CanBuy)
            {
                //库存判断
                if (result.StockQuantity < 1)
                {
                    result.CanBuy = false;
                    result.CanNotBuyDes = "已兑完";
                }
            }

            if (result.CanBuy)
            {
                //积分数
                if (result.NeedIntegral < 1)
                {
                    result.CanBuy = false;
                    result.CanNotBuyDes = "礼品信息错误";
                }
            }
            #endregion

            #region 用户信息判断

            if (result.CanBuy && CurrentUser != null)
            {
                //限购数量
                if (result.LimtQuantity > 0)
                {
                    int ownbuynumber = GiftsOrderApplication.GetOwnBuyQuantity(CurrentUser.Id, id);
                    if (ownbuynumber >= result.LimtQuantity)
                    {
                        result.CanBuy = false;
                        result.CanNotBuyDes = "限兑数量已满";
                    }
                }
                if (result.CanBuy)
                {
                    var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUserId);
                    if (userInte.AvailableIntegrals < result.NeedIntegral)
                    {
                        result.CanBuy = false;
                        result.CanNotBuyDes = "积分不足";
                    }
                }
            }
            #endregion
            return Json(result);
        }


        /// <summary>
        /// 下单前判断
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetCanBuy")]
        public object GetCanBuy(long id, int count)
        {
            CheckUserLogin();
            var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
            //礼品信息
            var giftdata = GiftApplication.GetById(id);
            if (giftdata == null)
            {
                return Json(ErrorResult<bool>("礼品不存在！", code: -2));
            }
            if (giftdata.GetSalesStatus != Mall.Entities.GiftInfo.GiftSalesStatus.Normal)
            {
                return Json(ErrorResult<bool>("礼品已失效！", code: -2));
            }
            //库存判断
            if (count > giftdata.StockQuantity)
            {
                return Json(ErrorResult<bool>("礼品库存不足,仅剩 " + giftdata.StockQuantity.ToString() + " 件！", code: -3));
            }
            //积分数
            if (giftdata.NeedIntegral < 1)
            {
                return Json(ErrorResult<bool>("礼品关联等级信息有误或礼品积分数据有误！", code: -5));
            }
            //限购数量
            if (giftdata.LimtQuantity > 0)
            {
                int ownbuynumber = GiftsOrderApplication.GetOwnBuyQuantity(CurrentUser.Id, id);
                if (ownbuynumber + count > giftdata.LimtQuantity)
                {
                    return Json(ErrorResult<bool>("超过礼品限兑数量！", code: -4));
                }
            }
            if (giftdata.NeedIntegral * count > userInte.AvailableIntegrals)
            {
                return Json(ErrorResult<bool>("积分不足！", code: -6));
            }
            //等级判定
            if (!MemberGradeApplication.IsAllowGrade(CurrentUser.Id, giftdata.NeedGrade))
            {
                return Json(ErrorResult<bool>("用户等级不足！", code: -6));
            }
            return Json(new { success=true, msg= "可以购买！", code= 1 });
        }

        /// <summary>
        /// 确认订单信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetConfirmOrder")]
        public object GetConfirmOrder(long id, int count, long? regionId = null)
        {
            CheckUserLogin();
            if (count < 1)
            {
                count = 1;
            }
            //Checkout
            dynamic data = new System.Dynamic.ExpandoObject();
            List<GiftOrderItemDtoModel> gorditemlist = new List<GiftOrderItemDtoModel>();
            GiftOrderItemDtoModel gorditem;     //订单项
            
            //礼品信息
            var giftdata = GiftApplication.GetById(id);
            if (giftdata == null)
            {
                throw new Exception("错误的礼品编号！");
            }
            gorditem = new GiftOrderItemDtoModel(); //补充订单项
            gorditem.GiftId = giftdata.Id;
            gorditem.GiftName = giftdata.GiftName;
            gorditem.GiftValue = giftdata.GiftValue;
            gorditem.ImagePath = giftdata.ImagePath;
            gorditem.OrderId = 0;
            gorditem.Quantity = count;
            gorditem.SaleIntegral = giftdata.NeedIntegral;
            if (!string.IsNullOrWhiteSpace(gorditem.ImagePath))
            {
                gorditem.DefaultImage = MallIO.GetRomoteProductSizeImage(gorditem.ImagePath, 1, ImageSize.Size_100.GetHashCode());
            }
            gorditemlist.Add(gorditem);

            data.GiftList = gorditemlist;

            data.GiftValueTotal = (decimal)gorditemlist.Sum(d => d.Quantity * d.GiftValue);
            data.TotalAmount = (int)gorditemlist.Sum(d => d.SaleIntegral * d.Quantity);

            //用户地址
            Entities.ShippingAddressInfo shipdata = GetShippingAddress(regionId);
            if (shipdata != null)
            {
               // Mapper.CreateMap<Entities.ShippingAddressInfo, ShippingAddressDtoModel>();
                ShippingAddressDtoModel shipobj = shipdata.Map<ShippingAddressDtoModel>();
                shipobj.Address = shipobj.Address + " " + shipdata.AddressDetail;
                data.ShipAddress = shipobj;
            }
            return Json(data);
        }


        /// <summary>
        /// 提交并处理订单
        /// </summary>
        /// <param name="id"></param>
        /// <param name="regionId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// 
        [HttpPost("PostSubmitOrder")]
        public object PostSubmitOrder(GiftConfirmOrderModel value)
        {
            CheckUserLogin();
            long id = value.ID;
            var addressId = value.AddressId;
            if (addressId < 1)
            {
                return Json(ErrorResult<bool>("错误的收货地址！", code: -8));
            }
            int count = value.Count;

            if (count < 1)
            {
                return Json(ErrorResult<bool>("错误的兑换数量！", code: -8));
            }
            List<GiftOrderItemModel> gorditemlist = new List<GiftOrderItemModel>();
            var curUser = MemberApplication.GetMember(CurrentUser.Id);
            if (curUser == null)
            {
                return Json(ErrorResult<bool>("用户登录错误！", code: -6));
            }
            var userInte = MemberIntegralApplication.GetMemberIntegral(curUser.Id);
            
            //礼品信息
            var giftdata = GiftApplication.GetById(id);
            if (giftdata == null)
            {
                return Json(ErrorResult<bool>("礼品不存在！", code: -2));
            }

            if (giftdata.GetSalesStatus != Entities.GiftInfo.GiftSalesStatus.Normal)
            {
                return Json(ErrorResult<bool>("礼品已失效！", code: -2));
            }

            //库存判断
            if (count > giftdata.StockQuantity)
            {
                return Json(ErrorResult<bool>("礼品库存不足,仅剩 " + giftdata.StockQuantity.ToString() + " 件！", code: -3));
            }

            //积分数
            if (giftdata.NeedIntegral < 1)
            {
                return Json(ErrorResult<bool>("礼品关联等级信息有误或礼品积分数据有误！", code: -5));
            }
            //限购数量
            if (giftdata.LimtQuantity > 0)
            {
                int ownbuynumber = GiftsOrderApplication.GetOwnBuyQuantity(CurrentUser.Id, id);
                if (ownbuynumber + count > giftdata.LimtQuantity)
                {
                    return Json(ErrorResult<bool>("超过礼品限兑数量！", code: -4));
                }
            }
            if (giftdata.NeedIntegral * count > userInte.AvailableIntegrals)
            {
                return Json(ErrorResult<bool>("积分不足！", code: -6));
            }

            //等级判定
            if (!MemberGradeApplication.IsAllowGrade(CurrentUser.Id, giftdata.NeedGrade))
            {
                return Json(ErrorResult<bool>("用户等级不足！", code: -6));
            }
            Entities.ShippingAddressInfo shipdata = GetShippingAddress(addressId);
            if (shipdata == null)
            {
                return Json(ErrorResult<bool>("错误的收货人地址信息！", code: -6));
            }
            gorditemlist.Add(new GiftOrderItemModel { GiftId = giftdata.Id, Counts = count });
            GiftOrderModel createorderinfo = new GiftOrderModel();
            createorderinfo.Gifts = gorditemlist;
            createorderinfo.CurrentUser = curUser;
            createorderinfo.ReceiveAddress = shipdata;
            var orderdata = GiftsOrderApplication.CreateOrder(createorderinfo);

            return Json( new { success = true, msg = orderdata.Id.ToString(), code = 1 });
        }

        /// <summary>
        /// 获取礼品订单列表
        /// </summary>
        /// <param name="skey"></param>
        /// <param name="status"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetMyOrderList")]
        public object GetMyOrderList(string skey = "", Mall.Entities.GiftOrderInfo.GiftOrderStatus? status = null, int pageNo = 1, int pageSize = 10)
        {
            CheckUserLogin();
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
            query.PageSize = pageSize;
            query.PageNo = pageNo;
            var orderdata = GiftsOrderApplication.GetOrders(query);
            List<Mall.Entities.GiftOrderInfo> orderlist = orderdata.Models.ToList();
            var result = orderlist.ToList();
          //  Mapper.CreateMap<GiftOrderInfo, GiftsOrderDtoModel>();
           // Mapper.CreateMap<GiftOrderItemInfo, GiftOrderItemDtoModel>();
            List<GiftsOrderDtoModel> pagedata = new List<GiftsOrderDtoModel>();
            foreach (var order in result)
            {
                order.Address = ClearHtmlString(order.Address);
                order.CloseReason = ClearHtmlString(order.CloseReason);
                order.UserRemark = ClearHtmlString(order.UserRemark);

                var tmpordobj = order.Map<GiftsOrderDtoModel>();
                tmpordobj.Items = new List<GiftOrderItemDtoModel>();
                var orderitems = GiftsOrderApplication.GetOrderItemByOrder(order.Id);
                foreach (var subitem in orderitems)
                {
                    var tmporditemobj = subitem.Map<GiftOrderItemDtoModel>();
                    tmporditemobj.DefaultImage = MallIO.GetRomoteProductSizeImage(tmporditemobj.ImagePath, 1, ImageSize.Size_150.GetHashCode());
                    tmpordobj.Items.Add(tmporditemobj);
                }
                pagedata.Add(tmpordobj);
            }

            return Json(new { Total = orderdata.Total, GiftOrders = pagedata });
        }

        /// <summary>
        /// 获取订单综合数据
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetOrderCount")]
        public object GetOrderCount()
        {
            CheckUserLogin();
            dynamic result = new System.Dynamic.ExpandoObject();
            long curid = CurrentUser.Id;
            result.AllCount = GiftsOrderApplication.GetOrderCount(null, curid);
            result.WaitDeliveryCount = GiftsOrderApplication.GetOrderCount(Mall.Entities.GiftOrderInfo.GiftOrderStatus.WaitDelivery, curid);
            result.WaitReceivingCount = GiftsOrderApplication.GetOrderCount(Mall.Entities.GiftOrderInfo.GiftOrderStatus.WaitReceiving, curid);
            result.FinishCount = GiftsOrderApplication.GetOrderCount(Mall.Entities.GiftOrderInfo.GiftOrderStatus.Finish, curid);
            return Json(result);
        }

        /// <summary>
        /// 获取订单信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="orderid"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetGiftOrderDetail")]
        public object GetGiftOrderDetail(long id)
        {
            CheckUserLogin();
            var orderdata = GiftsOrderApplication.GetOrder(id, CurrentUser.Id);
            if (orderdata == null)
            {
                return Json(ErrorResult<dynamic>("错误的订单编号！"));
            }
            //Mapper.CreateMap<GiftOrderInfo, GiftsOrderDtoModel>();
           // Mapper.CreateMap<GiftOrderItemInfo, GiftOrderItemDtoModel>();
            //_iGiftsOrderService.OrderAddUserInfo(orderlist);
            orderdata.Address = ClearHtmlString(orderdata.Address);
            orderdata.CloseReason = ClearHtmlString(orderdata.CloseReason);
            orderdata.UserRemark = ClearHtmlString(orderdata.UserRemark);

            var result = orderdata.Map<GiftsOrderDtoModel>();
            result.Items = new List<GiftOrderItemDtoModel>();
            result.OrderDateStr = result.OrderDate.ToString("yyyy-MM-dd HH:mm:ss");
            var items = GiftsOrderApplication.GetOrderItemByOrder(orderdata.Id);
            foreach (var subitem in items)
            {
                var tmporditemobj = subitem.Map<GiftOrderItemDtoModel>();
                tmporditemobj.DefaultImage = MallIO.GetRomoteProductSizeImage(tmporditemobj.ImagePath, 1, ImageSize.Size_150.GetHashCode());
                result.Items.Add(tmporditemobj);
            }
            return Json(result);
        }

        /// <summary>
        /// 确认收货
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [HttpPost("PostConfirmOrderOver")]
        public object PostConfirmOrderOver(GiftConfirmOrderOver value)
        {
            CheckUserLogin();
            long id = value.OrderId;
            if (id < 1)
            {
                return Json(ErrorResult<bool>("错误的订单编号！"));
            }
            Result result = new Result();
            GiftsOrderApplication.ConfirmOrder(id, CurrentUser.Id);

            return Json(new { msg = "订单完成", code = 1 });
        }

        /// <summary>
        /// 获取物流信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetGiftExpressInfo")]
        public object GetGiftExpressInfo(long orderId)
        {
            CheckUserLogin();
            var order = GiftsOrderApplication.GetOrder(orderId, CurrentUser.Id);


            List<object> TracesList = new List<object>();
            //取订单物流信息
            if (!string.IsNullOrWhiteSpace(order.ShipOrderNumber))
            {
                var expressData = ExpressApplication.GetExpressData(order.ExpressCompanyName, order.ShipOrderNumber);
                if (expressData.Success)
                {
                    expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列
                    foreach (var item in expressData.ExpressDataItems)
                    {
                        var traces = new
                        {
                            acceptStation = item.Content,
                            acceptTime = item.Time.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        TracesList.Add(traces);
                    }
                }
            }
            var data = new
            {
                LogisticsData = new
                {
                    success = TracesList.Count > 0,
                    traces = TracesList
                },
                ExpressCompanyName = order.ExpressCompanyName,
                ShipOrderNumber = order.ShipOrderNumber,
                ShipTo = order.ShipTo,
                CellPhone = order.CellPhone,
                Address = order.RegionFullName + order.Address
            };
            //var expressData = ExpressApplication.GetExpressData(order.ExpressCompanyName, order.ShipOrderNumber);
            //if (expressData == null)
            //{
            //    return Json(ErrorResult<dynamic>("没有物流记录！",data:new { ExpressNum = order.ShipOrderNumber, ExpressCompanyName = order.ExpressCompanyName, Comment = "" }));
            //}
            //if (expressData.Success)
            //    expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列
            //var json = new
            //{
            //    Success = expressData.Success,
            //    Msg = expressData.Message,
            //    Data = expressData.ExpressDataItems.Select(item => new
            //    {
            //        time = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
            //        content = item.Content
            //    })
            //};
            return Json(data);
        }

        /// <summary>
        /// 获取收货地址
        /// </summary>
        /// <param name="regionId"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetShippingAddress")]
        private Entities.ShippingAddressInfo GetShippingAddress(long? regionId)
        {
            Entities.ShippingAddressInfo result = null;
            if (regionId != null)
            {
                result = ShippingAddressApplication.GetUserShippingAddress((long)regionId);
            }
            else
            {
                if (CurrentUser != null)
                {
                    result = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(CurrentUser.Id);
                }
            }
            return result;
        }

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

    }
}
