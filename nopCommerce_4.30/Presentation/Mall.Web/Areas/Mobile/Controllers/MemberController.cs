using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.App_Code.Common;
using Mall.Web.Areas.Mobile.Models;
using Mall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.IO;
using Mall.Web.Areas.Web.Models;
using Mall.CommonModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;

namespace Mall.Web.Areas.Mobile.Controllers
{

    //TODO:Service 好多Service ？
    public class MemberController : BaseMobileMemberController
    {
        private IOrderService _iOrderService;
        private IMemberService _iMemberService;
        private IMemberCapitalService _iMemberCapitalService;
        private ICouponService _iCouponService;
        private IShopBonusService _iShopBonusService;
        private IVShopService _iVShopService;
        private IShopService _iShopService;
        private IProductService _iProductService;
        private IShippingAddressService _iShippingAddressService;
        private IMessageService _iMessageService;
        private IMemberSignInService _iMemberSignInService;
        private IRefundService _iRefundService;
        private ICommentService _iCommentService;
        private IShopBranchService _iShopBranchService;
        public MemberController(
            IOrderService iOrderService,
            IMemberService iMemberService,
             IMemberCapitalService iMemberCapitalService,
             ICouponService iCouponService,
             IShopBonusService iShopBonusService,
             IVShopService iVShopService,
             IProductService iProductService,
             IShippingAddressService iShippingAddressService,
             IMessageService iMessageService,
            IMemberSignInService iMemberSignInService,
            IRefundService iRefundService,
            ICommentService iCommentService,
            IShopBranchService iShopBranchService,
            IShopService iShopService
            )
        {
            _iOrderService = iOrderService;
            _iMemberService = iMemberService;
            _iMemberCapitalService = iMemberCapitalService;
            _iCouponService = iCouponService;
            _iShopBonusService = iShopBonusService;
            _iVShopService = iVShopService;
            _iProductService = iProductService;
            _iShippingAddressService = iShippingAddressService;
            _iMessageService = iMessageService;
            _iMemberSignInService = iMemberSignInService;
            _iRefundService = iRefundService;
            _iCommentService = iCommentService;
            _iShopBranchService = iShopBranchService;
            _iShopService = iShopService;
        }
        public ActionResult Center()
        {
            var userId = CurrentUser.Id;
            MemberCenterModel model = new MemberCenterModel();

            var statistic = StatisticApplication.GetMemberOrderStatistic(userId, true);

            var member = _iMemberService.GetMember(userId);
            model.Member = member;
            model.AllOrders = statistic.OrderCount;
            model.WaitingForRecieve = statistic.WaitingForRecieve + OrderApplication.GetWaitConsumptionOrderNumByUserId(UserId);
            model.WaitingForPay = statistic.WaitingForPay;
            model.WaitingForDelivery = statistic.WaitingForDelivery;
            model.WaitingForComments = statistic.WaitingForComments;
            model.RefundOrders = statistic.RefundCount;
            model.FavoriteProductCount = FavoriteApplication.GetFavoriteCountByUser(userId);

            //拼团
            model.CanFightGroup = FightGroupApplication.IsOpenMarketService();
            model.BulidFightGroupNumber = FightGroupApplication.CountJoiningOrder(userId);

            model.Capital = MemberCapitalApplication.GetBalanceByUserId(userId);
            model.CouponsCount = MemberApplication.GetAvailableCouponCount(userId);
            var integral = MemberIntegralApplication.GetMemberIntegral(userId);
            model.GradeName = MemberGradeApplication.GetMemberGradeByUserIntegral(integral.HistoryIntegrals).GradeName;
            model.MemberAvailableIntegrals = MemberIntegralApplication.GetAvailableIntegral(userId);

            model.CollectionShop = ShopApplication.GetUserConcernShopsCount(userId);

            model.CanSignIn = _iMemberSignInService.CanSignInByToday(userId);
            model.SignInIsEnable = _iMemberSignInService.GetConfig().IsEnable;
            model.userMemberInfo = CurrentUser;
            model.IsOpenRechargePresent = SiteSettings.IsOpenRechargePresent;

            model.DistributionOpenMyShopShow = SiteSettings.DistributorRenameOpenMyShop;
            model.DistributionMyShopShow = SiteSettings.DistributorRenameMyShop;
            
            if (PlatformType == PlatformType.WeiXin)
            {
                //分销
                model.IsShowDistributionOpenMyShop = SiteSettings.DistributionIsEnable;
                var duser = DistributionApplication.GetDistributor(CurrentUser.Id);
                if (duser != null && duser.DistributionStatus != (int)DistributorStatus.UnApply)
                {
                    model.IsShowDistributionOpenMyShop = false;
                    //拒绝的分销员显示“我要开店”
                    if (duser.DistributionStatus == (int)DistributorStatus.Refused || duser.DistributionStatus == (int)DistributorStatus.UnAudit)
                        model.IsShowDistributionOpenMyShop = true && SiteSettings.DistributionIsEnable;

                    model.IsShowDistributionMyShop = true && SiteSettings.DistributionIsEnable;
                    if (duser.DistributionStatus == (int)DistributorStatus.NotAvailable || duser.DistributionStatus == (int)DistributorStatus.Refused || duser.DistributionStatus == (int)DistributorStatus.UnAudit)
                    {
                        model.IsShowDistributionMyShop = false;
                    }
                }
            }
            _iMemberService.AddIntegel(member); //给用户加积分//执行登录后初始化相关操作
            return View(model);
        }

        public ActionResult ShippingAddress()
        {
            return View();
        }

        #region 订单相关处理
        public ActionResult Orders(int? orderStatus)
        {
            //判断是否需要跳转到支付地址
            if (this.Request.GetDisplayUrl().EndsWith("/member/orders", StringComparison.OrdinalIgnoreCase) && (orderStatus == null || orderStatus == 0 || orderStatus == 1))
            {
                var returnUrl = Request.Query["returnUrl"];
                return Redirect(Url.RouteUrl("PayRoute") + "?area=mobile&platform=" + this.PlatformType.ToString() + "&controller=member&action=orders&orderStatus=" + orderStatus + (string.IsNullOrEmpty(returnUrl) ? "" : "&returnUrl=" + HttpUtility.UrlEncode(returnUrl)));
            }
            var statistic = StatisticApplication.GetMemberOrderStatistic(CurrentUser.Id);
            ViewBag.AllOrders = statistic.OrderCount;
            ViewBag.WaitingForComments = statistic.WaitingForComments;
            ViewBag.WaitingForRecieve = statistic.WaitingForRecieve + OrderApplication.GetWaitConsumptionOrderNumByUserId(CurrentUser.Id);
            ViewBag.WaitingForPay = statistic.WaitingForPay;
            ViewBag.WaitingForDelivery = statistic.WaitingForDelivery;
            return View();
        }

        public ActionResult PaymentToOrders(string ids)
        {
            //红包数据
            var bonusGrantIds = new Dictionary<long, Entities.ShopBonusInfo>();
            string url = CurrentUrlHelper.CurrentUrlNoPort() + "/m-weixin/shopbonus/index/";
            if (!string.IsNullOrEmpty(ids))
            {
                string[] strIds = ids.Split(',');
                List<long> longIds = new List<long>();
                foreach (string id in strIds)
                {
                    longIds.Add(long.Parse(id));
                }
                var result = PaymentHelper.GenerateBonus(longIds, Request.Host.ToString());
                foreach (var item in result)
                {
                    bonusGrantIds.Add(item.Key, item.Value);
                }
            }

            ViewBag.Path = url;
            ViewBag.BonusGrantIds = bonusGrantIds;
            ViewBag.Shops = ShopApplication.GetShops(bonusGrantIds.Select(p => p.Value.ShopId));
            ViewBag.BaseAddress = CurrentUrlHelper.CurrentUrlNoPort();

            var statistic = StatisticApplication.GetMemberOrderStatistic(CurrentUser.Id);
            ViewBag.WaitingForComments = statistic.WaitingForComments;
            ViewBag.AllOrders = statistic.OrderCount;
            ViewBag.WaitingForRecieve = statistic.WaitingForRecieve + OrderApplication.GetWaitConsumptionOrderNumByUserId(CurrentUser.Id);
            ViewBag.WaitingForPay = statistic.WaitingForPay;
            ViewBag.WaitingForDelivery = statistic.WaitingForDelivery;

            var order = OrderApplication.GetUserOrders(CurrentUser.Id, 1).FirstOrDefault();
            if (order != null && order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                var gpord = FightGroupApplication.GetOrder(order.Id);
                if (gpord != null)
                {
                    return Redirect(string.Format("/m-{0}/FightGroup/GroupOrderOk?orderid={1}", PlatformType.ToString(), order.Id));
                }
            }
            return View("~/Areas/Mobile/Templates/Default/Views/Member/Orders.cshtml");
        }

        public JsonResult GetUserOrders(int? orderStatus, int pageNo, int pageSize = 8)
        {
            if (orderStatus.HasValue && orderStatus == 0)
            {
                orderStatus = null;
            }
            var queryModel = new OrderQuery()
            {
                Status = (Entities.OrderInfo.OrderOperateStatus?)orderStatus,
                UserId = CurrentUser.Id,
                PageSize = pageSize,
                PageNo = pageNo,
                IsFront = true
            };
            if (queryModel.Status.HasValue && queryModel.Status.Value == Entities.OrderInfo.OrderOperateStatus.WaitReceiving)
            {
                if (queryModel.MoreStatus == null)
                {
                    queryModel.MoreStatus = new List<Entities.OrderInfo.OrderOperateStatus>() { };
                }
                queryModel.MoreStatus.Add(Entities.OrderInfo.OrderOperateStatus.WaitSelfPickUp);
            }
            if (orderStatus.GetValueOrDefault() == (int)OrderInfo.OrderOperateStatus.Finish)
                queryModel.Commented = false;//只查询未评价的订单

            var orders = OrderApplication.GetOrders(queryModel);
            var orderItems = OrderApplication.GetOrderItemsByOrderId(orders.Models.Select(p => p.Id));
            var orderComments = OrderApplication.GetOrderCommentCount(orders.Models.Select(p => p.Id));
            var orderRefunds = OrderApplication.GetOrderRefunds(orderItems.Select(p => p.Id));
            var products = ProductManagerApplication.GetProductsByIds(orderItems.Select(p => p.ProductId));
            var vshops = VshopApplication.GetVShopsByShopIds(products.Select(p => p.ShopId));
            //查询结果的门店ID
            var branchIds = orders.Models.Where(e => e.ShopBranchId>0).Select(p => p.ShopBranchId).ToList();
            //根据门店ID获取门店信息
            var shopBranchs = ShopBranchApplication.GetShopBranchByIds(branchIds);
            var orderVerificationCodes = OrderApplication.GetOrderVerificationCodeInfosByOrderIds(orders.Models.Select(p => p.Id).ToList());
            var result = orders.Models.Select(item =>
            {
                var codes = orderVerificationCodes.Where(a => a.OrderId == item.Id);
                var _ordrefobj = _iRefundService.GetOrderRefundByOrderId(item.Id) ?? new Entities.OrderRefundInfo { Id = 0 };
                if (item.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitDelivery && item.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                {
                    _ordrefobj = new Entities.OrderRefundInfo { Id = 0 };
                }
                int? ordrefstate = (_ordrefobj == null ? null : (int?)_ordrefobj.SellerAuditStatus);
                ordrefstate = (ordrefstate > 4 ? (int?)_ordrefobj.ManagerConfirmStatus : ordrefstate);
                var branchObj = shopBranchs.FirstOrDefault(e => item.ShopBranchId > 0 && e.Id == item.ShopBranchId);
                string branchName = branchObj == null ? string.Empty : branchObj.ShopBranchName;
                return new
                {
                    id = item.Id,
                    status = item.OrderStatus.ToDescription(),
                    orderStatus = item.OrderStatus,
                    shopname = item.ShopName,
                    orderTotalAmount = item.OrderTotalAmount,
                    capitalAmount = item.CapitalAmount,
                    productCount = orderItems.Where(oi => oi.OrderId == item.Id).Sum(a=>a.Quantity),
                    commentCount = orderComments.ContainsKey(item.Id) ? orderComments[item.Id] : 0,
                    PaymentType = item.PaymentType,
                    RefundStats = ordrefstate,
                    OrderRefundId = _ordrefobj.Id,
                    OrderType = item.OrderType,
                    PickUp = item.PickupCode,
                    ShopBranchId = item.ShopBranchId,
                    ShopBranchName = branchName,
                    DeliveryType = item.DeliveryType,
                    ShipOrderNumber = item.ShipOrderNumber,
                    EnabledRefundAmount = item.OrderEnabledRefundAmount,
                    itemInfo = orderItems.Where(oi => oi.OrderId == item.Id).Select(a =>
                            {
                                var prodata = products.FirstOrDefault(p => p.Id == a.ProductId);
                                VShop vshop = null;
                                if (prodata != null)
                                    vshop = vshops.FirstOrDefault(vs => vs.ShopId == prodata.ShopId);
                                if (vshop == null)
                                    vshop = new VShop { Id = 0 };

                                var itemrefund = orderRefunds.Where(or => or.OrderItemId == a.Id).FirstOrDefault(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                                int? itemrefstate = (itemrefund == null ? null : (int?)itemrefund.SellerAuditStatus);
                                itemrefstate = (itemrefstate > 4 ? (int?)itemrefund.ManagerConfirmStatus : itemrefstate);
                                return new
                                {
                                    itemid = a.Id,
                                    productId = a.ProductId,
                                    productName = a.ProductName,
                                    image = MallIO.GetProductSizeImage(a.ThumbnailsUrl, 1, (int)ImageSize.Size_100),
                                    count = a.Quantity,
                                    price = a.SalePrice,
                                    Unit = prodata == null ? "" : prodata.MeasureUnit,
                                    vshopid = vshop.Id,
                                    color = a.Color,
                                    size = a.Size,
                                    version = a.Version,
                                    RefundStats = itemrefstate,
                                    OrderRefundId = (itemrefund == null ? 0 : itemrefund.Id),
                                    EnabledRefundAmount = a.EnabledRefundAmount
                                };
                            }),
                    HasAppendComment = HasAppendComment(orderItems.Where(oi => oi.OrderId == item.Id).FirstOrDefault()),
                    CanRefund = OrderApplication.CanRefund(item, ordrefstate),
                    IsVirtual = item.OrderType == OrderInfo.OrderTypes.Virtual ? 1 : 0,
                    IsPay = item.PayDate.HasValue ? 1 : 0
                };
            });

            foreach (var item in result)
            {
                var refund = item.itemInfo.Any(p => p.OrderRefundId > 0);
                //if (!refund)
                //item.CanRefund = false;
            }
            return Json(new { success = true, data = result });
        }

        public ActionResult PickupGoods(long id)
        {
            var orderInfo = OrderApplication.GetOrder(id);
            if (orderInfo == null)
                throw new MallException("订单不存在！");
            if (orderInfo.UserId != CurrentUser.Id)
                throw new MallException("只能查看自己的提货码！");

            //AutoMapper.Mapper.CreateMap<Order, Mall.DTO.OrderListModel>();
            //AutoMapper.Mapper.CreateMap<DTO.OrderItem, OrderItemListModel>();
            var orderModel = orderInfo.Map<Mall.DTO.OrderListModel>();
            var orderItems = OrderApplication.GetOrderItemsByOrderId(orderInfo.Id);
            orderModel.OrderItemList = orderItems.Map< List<OrderItemListModel>>();
            if ( orderInfo.ShopBranchId >0)
            {//补充数据
                var branch = ShopBranchApplication.GetShopBranchById(orderInfo.ShopBranchId);
                orderModel.ShopBranchName = branch.ShopBranchName;
                orderModel.ShopBranchAddress = branch.AddressFullName;
                orderModel.ShopBranchContactPhone = branch.ContactPhone;
            }

            return View(orderModel);
        }
        #endregion 订单相关处理
        public ActionResult CollectionProduct()
        {
            return View();
        }
        private bool HasAppendComment(DTO.OrderItem orderItem)
        {
            var result = _iCommentService.HasAppendComment(orderItem.Id);
            return result;
        }
        public ActionResult CollectionShop()
        {
            ViewBag.SiteName = SiteSettings.SiteName;
            return View();
        }

        public ActionResult ChangeLoginPwd()
        {
            return View(CurrentUser);
        }

        /// <summary>
        /// 修改支付密码
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangePayPwd()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePayPwd(ChangePayPwd model)
        {
            if (string.IsNullOrEmpty(model.NewPayPwd))
                return Json(new { success = false, msg = "请输入新支付密码" });

            if (!string.IsNullOrEmpty(model.OldPayPwd))
            {
                var success = MemberApplication.VerificationPayPwd(CurrentUser.Id, model.OldPayPwd);
                if (!success)
                    return Json(new { success = false, msg = "原支付密码输入不正确" });
                MemberApplication.ChangePayPassword(CurrentUser.Id, model.NewPayPwd);
            }
            else if (!string.IsNullOrEmpty(model.PhoneCode))
            {
                var codeCacheKey = CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, model.SendCodePluginId);
                var codeCache = Core.Cache.Get<string>(codeCacheKey);
                if (string.IsNullOrEmpty(codeCache))
                    return Json(new { success = false, msg = "验证码已过期" });

                //if (!string.Equals(codeCache, model.PhoneCode, StringComparison.OrdinalIgnoreCase))
                //    return Json(new { success = false, msg = "验证码不正确" });

                MemberApplication.ChangePayPassword(CurrentUser.Id, model.NewPayPwd);
            }
            else
                return Json(new { success = false });
            return Json(new { success = true });
        }

        public JsonResult GetUserCollectionProduct(int pageNo, int pageSize = 16)
        {
            var data = _iProductService.GetUserConcernProducts(CurrentUser.Id, pageNo, pageSize).Models;
            var products = _iProductService.GetProducts(data.Select(p => p.ProductId).ToList());
            var result = data.Select(item =>
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                return new
                {
                    Id = product.Id,
                    Image = product.GetImage(ImageSize.Size_220),
                    ProductName = product.ProductName,
                    SalePrice = product.MinSalePrice.ToString("F2"),
                    Evaluation = CommentApplication.GetCommentCountByProduct(product.Id),
                    Status = GetProductShowStatus(product)
                };
            });
            return Json(new { success = true, data = result });
        }
        /// <summary>
        /// 删除关注商品
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult CancelConcernProduct(long productId)
        {
            if (productId < 1)
            {
                throw new MallException("错误的参数");
            }
            _iProductService.DeleteFavorite(productId, CurrentUser.Id);

            return Json(new { success = true });
        }

        private int GetProductShowStatus(ProductInfo pro)
        {
            int result = 0;  //0:正常；1：失效；2：库存不足 3：下架
            if (pro.AuditStatus != ProductInfo.ProductAuditStatus.Audited || pro.SaleStatus != ProductInfo.ProductSaleStatus.OnSale)
                result = 3;
            else
            {
                var skus = ProductManagerApplication.GetSKUs(pro.Id);
                if (skus.Sum(d => d.Stock) < 1)
                {
                    result = 2;
                }
            }
            if (pro.IsDeleted)
                result = 1;
            return result;
        }

        public JsonResult GetUserCollectionShop(int pageNo, int pageSize = 8)
        {

            var model = _iShopService.GetUserConcernShops(CurrentUser.Id, pageNo, pageSize);
            List<ShopConcernModel> list = new List<ShopConcernModel>();
            foreach (var m in model.Models)
            {
                var shop = ShopApplication.GetShop(m.ShopId);
                if (shop == null) continue;
                var vshopobj = _iVShopService.GetVShopByShopId(m.ShopId);
                ShopConcernModel concern = new ShopConcernModel();
                concern.FavoriteShopInfo.Id = m.Id;
                concern.FavoriteShopInfo.Logo = vshopobj == null ? shop.Logo : vshopobj.Logo;
                concern.FavoriteShopInfo.ConcernTime = m.Date;
                concern.FavoriteShopInfo.ConcernTimeStr = m.Date.ToString("yyyy-MM-dd");
                concern.FavoriteShopInfo.ShopId = m.ShopId;
                concern.FavoriteShopInfo.ShopName = shop.ShopName;
                concern.FavoriteShopInfo.ConcernCount = FavoriteApplication.GetFavoriteShopCountByShop(m.ShopId);
                concern.FavoriteShopInfo.ShopStatus = shop.ShopStatus;
                list.Add(concern);
            }

            return SuccessResult<dynamic>(data: list);
        }

        public JsonResult CheckVshopIfExist(long shopid)
        {
            var vshop = _iVShopService.GetVShopByShopId(shopid);
            if (vshop != null)
                return SuccessResult<dynamic>(data: new { vshopid = vshop.Id });
            else
                return ErrorResult();
        }

        [HttpGet]
        public JsonResult GetCancelConcernShop(long shopId)
        {
            _iShopService.CancelConcernShops(shopId, CurrentUser.Id);
            return Json(new Result() { success = true, msg = "取消成功！" });
        }

        [HttpPost]
        public JsonResult AddShippingAddress(Entities.ShippingAddressInfo info)
        {
            info.UserId = CurrentUser.Id;

            if (info.shopBranchId>0)
            {
                var shopBranchInfo = _iShopBranchService.GetShopBranchById(info.shopBranchId);
                if (shopBranchInfo == null)
                {
                    return Json(new { success = false, msg = "门店信息获取失败" }, true);
                }
                if (shopBranchInfo.ServeRadius > 0)
                {
                    string form = string.Format("{0},{1}", info.Latitude, info.Longitude);//收货地址的经纬度
                    if (form.Length <= 1)
                    {
                        return Json(new { success = false, msg = "收货地址经纬度获取失败" }, true);
                    }
                    double Distances = _iShopBranchService.GetLatLngDistancesFromAPI(form, string.Format("{0},{1}", shopBranchInfo.Latitude, shopBranchInfo.Longitude));
                    if (Distances > shopBranchInfo.ServeRadius)
                    {
                        _iShippingAddressService.AddShippingAddress(info);
                        return Json(new { success = true, msg = "收货地址超出门店配送范围" }, true);
                    }
                }
                else
                {
                    return Json(new { success = false, msg = "门店不提供配送服务" }, true);
                }

            }

            _iShippingAddressService.AddShippingAddress(info);
            return Json(new
            {
                success = true,
                msg = "",
                data = new
                {
                    regionFullName = RegionApplication.GetFullName(info.RegionId),
                    id = info.Id
                }
            }, true);
        }

        [HttpPost]
        public JsonResult DeleteShippingAddress(long id)
        {
            var userId = CurrentUser.Id;
            _iShippingAddressService.DeleteShippingAddress(id, userId);
            return Json(new Result() { success = true, msg = "删除成功" });
        }

        [HttpPost]
        public JsonResult EditShippingAddress(Entities.ShippingAddressInfo info)
        {
            info.UserId = CurrentUser.Id;
            if (info.shopBranchId>0)
            {
                var shopBranchInfo = _iShopBranchService.GetShopBranchById(info.shopBranchId);
                if (shopBranchInfo == null)
                {
                    return Json(new { success = false, msg = "门店信息获取失败" }, true);
                }
                if (shopBranchInfo.ServeRadius > 0)
                {
                    string form = string.Format("{0},{1}", info.Latitude, info.Longitude);//收货地址的经纬度
                    if (form.Length <= 1)
                    {
                        return Json(new { success = false, msg = "收货地址经纬度获取失败" }, true);
                    }
                    double Distances = _iShopBranchService.GetLatLngDistancesFromAPI(form, string.Format("{0},{1}", shopBranchInfo.Latitude, shopBranchInfo.Longitude));
                    if (Distances > shopBranchInfo.ServeRadius)
                    {
                        _iShippingAddressService.UpdateShippingAddress(info);
                        return Json(new { success = true, msg = "收货地址超出门店配送范围" }, true);
                    }
                }
                else
                {
                    return Json(new { success = false, msg = "门店不提供配送服务" }, true);
                }

            }

            _iShippingAddressService.UpdateShippingAddress(info);
            return Json(new
            {
                success = true,
                data = new { regionFullName = RegionApplication.GetFullName(info.RegionId) },
                msg = ""
            }, true);
        }

        [HttpPost]
        public JsonResult ChangePassword(string oldpassword, string password)
        {
            if (string.IsNullOrWhiteSpace(oldpassword) || string.IsNullOrWhiteSpace(password))
            {
                return Json(new Result() { success = false, msg = "密码不能为空！" });
            }
            var model = CurrentUser;
            var pwd = SecureHelper.MD5(SecureHelper.MD5(oldpassword) + model.PasswordSalt);
            bool CanChange = false;
            if (pwd == model.Password)
            {
                CanChange = true;
            }
            if (model.PasswordSalt.StartsWith("o"))
            {
                CanChange = true;
            }
            if (CanChange)
            {
                _iMemberService.ChangePassword(model.Id, password);
                return Json(new Result() { success = true, msg = "修改成功" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "旧密码错误" });
            }
        }

        public ActionResult AccountManagement()
        {
            return View();
        }

        public ActionResult AccountSecure()
        {
            return View(CurrentUser);
        }

        public ActionResult BindPhone()
        {
            return View(CurrentUser);
        }

        public ActionResult BindEmail()
        {
            return View(CurrentUser);
        }

        [HttpPost]
        public JsonResult SendCode(string pluginId, string destination = null, bool checkBind = false)
        {
            if (string.IsNullOrEmpty(destination))
                destination = CurrentUser.CellPhone;

            if (string.IsNullOrEmpty(destination))
                return Json(new { success = false, msg = "请输入手机号码" });

            if (checkBind && _iMessageService.GetMemberContactsInfo(pluginId, destination, Entities.MemberContactInfo.UserTypes.General) != null)
            {
                return Json(new Result() { success = false, msg = destination + "已经绑定过了！" });
            }
            _iMemberService.CheckContactInfoHasBeenUsed(pluginId, destination);
            var timeout = CacheKeyCollection.MemberPluginCheckTime(CurrentUser.UserName, pluginId);
            if (Core.Cache.Exists(timeout))
            {
                return Json(new Result() { success = false, msg = "120秒内只允许请求一次，请稍后重试!" });
            }
            var checkCode = new Random().Next(10000, 99999);
            //Log.Debug(destination + "--checkCode:" + checkCode);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            if (pluginId.ToLower().Contains("email"))
            {
                cacheTimeout = DateTime.Now.AddHours(24);
            }
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId), checkCode.ToString(), cacheTimeout);
            var user = new Mall.Core.Plugins.Message.MessageUserInfo() { UserName = CurrentUser.UserName, SiteName = SiteSettings.SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheckTime(CurrentUser.UserName, pluginId), "0", DateTime.Now.AddSeconds(120));
            return Json(new Result() { success = true, msg = "发送成功" });
        }

        [HttpPost]
        public JsonResult SendFindCode(string pluginId, string destination = null)
        {
            if (string.IsNullOrEmpty(destination))
                destination = CurrentUser.CellPhone;

            if (string.IsNullOrEmpty(destination))
                return Json(new { success = false, msg = "请先绑定手机" });

            var timeout = CacheKeyCollection.MemberPluginCheckTime(CurrentUser.UserName, pluginId);
            if (Core.Cache.Exists(timeout))
            {
                return Json(new Result() { success = false, msg = "120秒内只允许请求一次，请稍后重试!" });
            }
            var checkCode = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            if (pluginId.ToLower().Contains("email"))
            {
                cacheTimeout = DateTime.Now.AddHours(24);
            }
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId), checkCode.ToString(), cacheTimeout);
            var user = new Mall.Core.Plugins.Message.MessageUserInfo() { UserName = CurrentUser.UserName, SiteName = SiteSettings.SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheckTime(CurrentUser.UserName, pluginId), "0", DateTime.Now.AddSeconds(120));
            return Json(new Result() { success = true, msg = "发送成功" });
        }



        [HttpPost]
        public JsonResult CheckCode(string pluginId, string code, string destination)
        {
            var cache = CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId);
            var cacheCode = Core.Cache.Get<string>(cache);
            var member = CurrentUser;
            var mark = "";
            if (cacheCode != null && cacheCode == code)
            {
                var service = _iMessageService;
                if (service.GetMemberContactsInfo(pluginId, destination, Entities.MemberContactInfo.UserTypes.General) != null)
                {
                    return Json(new Result() { success = false, msg = destination + "已经绑定过了！" });
                }
                if (pluginId.ToLower().Contains("email"))
                {
                    member.Email = destination;
                    mark = "邮箱";
                }
                else if (pluginId.ToLower().Contains("sms"))
                {
                    member.CellPhone = destination;
                    mark = "手机";
                }
                _iMemberService.UpdateMember(member);
                service.UpdateMemberContacts(new Entities.MemberContactInfo()
                {
                    Contact = destination,
                    ServiceProvider = pluginId,
                    UserId = CurrentUser.Id,
                    UserType = Entities.MemberContactInfo.UserTypes.General
                });
                Core.Cache.Remove(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId));
                Core.Cache.Remove(CacheKeyCollection.Member(CurrentUser.Id));//移除用户缓存
                Core.Cache.Remove("Rebind" + CurrentUser.Id);

                Mall.Entities.MemberIntegralRecordInfo info = new Mall.Entities.MemberIntegralRecordInfo();
                info.UserName = member.UserName;
                info.MemberId = member.Id;
                info.RecordDate = DateTime.Now;
                info.TypeId = Mall.Entities.MemberIntegralInfo.IntegralType.Reg;
                info.ReMark = "绑定" + mark;
                var memberIntegral = ServiceApplication.Create<IMemberIntegralConversionFactoryService>().Create(Mall.Entities.MemberIntegralInfo.IntegralType.Reg);
                ServiceApplication.Create<IMemberIntegralService>().AddMemberIntegral(info, memberIntegral);
               
                //TODO:DZY[180507] 去掉会员推广
                /*
                if (member.InviteUserId > 0)
                {
                    var inviteMember = _iMemberService.GetMember(member.InviteUserId);
                    if (inviteMember != null)
                        ServiceApplication.Create<IMemberInviteService>().AddInviteIntegel(member, inviteMember, true);
                }
                */

                return Json(new Result() { success = true, msg = "验证正确" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "验证码不正确或者已经超时" });
            }
        }

        public ActionResult Integral()
        {
            return View();
        }

        public ActionResult AccountInfo()
        {
            ViewBag.WeiXin = PlatformType == PlatformType.WeiXin;
            return View(CurrentUser);
        }

        [HttpPost]
        public JsonResult SaveAccountInfo(MemberUpdate model)
        {
            if (string.IsNullOrWhiteSpace(model.RealName))
            {
                return ErrorResult("真实姓名必须填写");
            }
            if (!string.IsNullOrWhiteSpace(model.Photo))
            {
                model.Photo = UploadPhoto(model.Photo);
            }
            model.Id = CurrentUser.Id;
            MemberApplication.UpdateMemberInfo(model);
            return Json<dynamic>(success: true, msg: "修改成功");
        }

        private string UploadPhoto(string strPhoto)
        {
            string url = string.Empty;
            string fullPath = "/Storage/Member/" + CurrentUser.Id + "/headImage.jpg";
            try
            {
                byte[] bytes = Convert.FromBase64String(strPhoto.Replace("data:image/jpeg;base64,", ""));
                MemoryStream memStream = new MemoryStream(bytes);
                Core.MallIO.CreateFile(fullPath, memStream, FileCreateType.Create);
                url = fullPath;
            }
            catch (Exception ex)
            {
                Core.Log.Error("头像上传异常：" + ex);
            }
            return url;
        }

        /// <summary>
        /// 获取用户积分明细
        /// </summary>
        /// <param name="id">用户编号</param>
        /// <param name="type"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNo"></param>
        /// <returns></returns>
        public object GetIntegralRecord(int? type, int pageSize = 10, int pageNo = 1)
        {
            var id = CurrentUser.Id;
            //处理当前用户与id的判断
            var _iMemberIntegralService = ServiceApplication.Create<IMemberIntegralService>();
            Mall.Entities.MemberIntegralInfo.IntegralType? integralType = null;
            if (type.HasValue)
            {
                integralType = (Mall.Entities.MemberIntegralInfo.IntegralType)type.Value;
            }
            var query = new IntegralRecordQuery() { IntegralType = integralType, UserId = CurrentUser.Id, PageNo = pageNo, PageSize = pageSize };
            var result = _iMemberIntegralService.GetIntegralRecordListForWeb(query);
            var list = result.Models.Select(item =>
            {
                var actions = _iMemberIntegralService.GetIntegralRecordAction(item.Id);
                return new
                {
                    Id = item.Id,
                    RecordDate = ((DateTime)item.RecordDate).ToString("yyyy-MM-dd HH:mm:ss"),
                    Integral = item.Integral,
                    TypeId = item.TypeId,
                    ShowType = (item.TypeId == Mall.Entities.MemberIntegralInfo.IntegralType.WeiActivity) ? item.ReMark : item.TypeId.ToDescription(),
                    ReMark = GetRemarkFromIntegralType(item.TypeId, actions, item.ReMark)
                };
            }).ToList();
            var userInte = MemberIntegralApplication.GetMemberIntegral(UserId);
            return Json(new
            {
                success = true,
                total = result.Total,
                availableIntegrals = userInte.AvailableIntegrals,
                data = Json(list)
            });
        }

        private string GetRemarkFromIntegralType(Mall.Entities.MemberIntegralInfo.IntegralType type, ICollection<Mall.Entities.MemberIntegralRecordActionInfo> recordAction, string remark = "")
        {
            if (recordAction == null || recordAction.Count == 0)
                return remark;
            switch (type)
            {
                case Mall.Entities.MemberIntegralInfo.IntegralType.Consumption:
                    var orderIds = "";
                    foreach (var item in recordAction)
                    {
                        orderIds += item.VirtualItemId + ",";
                    }
                    remark = "订单号:" + orderIds.TrimEnd(',');
                    break;
                default:
                    return remark;
            }
            return remark;
        }

        /// <summary>
        /// 是否强制绑定手机号
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult IsConBindSms()
        {
            return Json<dynamic>(success: MessageApplication.IsOpenBindSms(CurrentUser.Id));
        }


        public ActionResult GotoGifts()
        {
            return RedirectToAction("Index", "Gifts");
        }
        public ActionResult GotoChooseAddress()
        {
            return RedirectToAction("StoreListAddress", "ShopBranch");
        }
    }
}