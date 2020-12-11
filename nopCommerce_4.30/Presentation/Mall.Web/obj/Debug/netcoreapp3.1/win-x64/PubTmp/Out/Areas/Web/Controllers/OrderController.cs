using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Web.App_Code.Common;
using Mall.Web.Areas.Web.Models;
using Mall.Web.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Mall.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Web.Controllers
{
    public class OrderController : BaseMemberController
    {

        /// <summary>
        /// 提交订单页面显示
        /// </summary>
        /// <param name="cartItemIds">提交的购物车物品集合</param>
        /// <param name="regionId">客户送货区域标识</param>
        public ActionResult Submit(string cartItemIds, long? regionId, string CouponIds = "")
        {
            //Logo
            ViewBag.Logo = SiteSettings.Logo;//获取Logo
            ViewBag.Step = 2;
            //设置会员信息
            ViewBag.Member = CurrentUser;
            string cartInfo = WebHelper.GetCookie(CookieKeysCollection.Mall_CART);
            var coupons = CouponApplication.ConvertUsedCoupon(CouponIds);
            var submitModel = OrderApplication.Submit(cartItemIds, regionId, UserId, cartInfo, coupons);

            ViewBag.IsCashOnDelivery = submitModel.IsCashOnDelivery;
            ViewBag.IsLimitBuy = submitModel.IsLimitBuy;

            InitOrderSubmitModel(submitModel);
            #region 是否开启门店授权
            ViewBag.IsOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            #endregion
            #region 是否提供发票
            //bool ProvideInvoice = false;
            //if (submitModel.products != null)
            //    ProvideInvoice = ShopApplication.HasProvideInvoice(submitModel.products.Select(p => p.shopId).ToList());
            //ViewBag.ProvideInvoice = ProvideInvoice;
            #endregion
            bool canIntegralPerMoney = true, canCapital = true;
            CanDeductible(out canIntegralPerMoney, out canCapital);
            ViewBag.CanIntegralPerMoney = canIntegralPerMoney;
            ViewBag.CanCapital = canCapital;
            ViewBag.Keyword = SiteSettings.Keyword;
            return View(submitModel);
        }

        /// <summary>
        /// 点击立即购买调用的GET方法，但是重定向到了Submit页面
        /// </summary>
        /// <param name="skuIds">多个库存Id</param>
        /// <param name="counts">每个库存对应的数据量</param>
        /// <param name="regionId">客户收货地区的id</param>
        /// <param name="collpids">组合购Id集合</param>
        /// <returns>订单提交页面的数据</returns>
        public ActionResult SubmitByProductId(string skuIds, string counts, long? regionId, string collpids = null, string CouponIds = "", sbyte productType = 0, long productId = 0)
        {
            //Logo
            ViewBag.Logo = SiteSettings.Logo;//获取Logo
                                             //设置会员信息
            ViewBag.Member = CurrentUser;
            var coupons = CouponApplication.ConvertUsedCoupon(CouponIds);
            var submitModel = OrderApplication.SubmitByProductId(UserId, skuIds, counts, regionId, collpids, coupons, productType);

            ViewBag.IsCashOnDelivery = submitModel.IsCashOnDelivery;
            ViewBag.IsLimitBuy = submitModel.IsLimitBuy;

            InitOrderSubmitModel(submitModel);
            #region 是否开启门店授权
            ViewBag.IsOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            #endregion
            #region 是否提供发票
            //bool ProvideInvoice = false;
            //if (submitModel.products != null)
            //    ProvideInvoice = ShopApplication.HasProvideInvoice(submitModel.products.Select(p => p.shopId).ToList());
            //ViewBag.ProvideInvoice = ProvideInvoice;
            #endregion

            bool canIntegralPerMoney = true, canCapital = true;
            CanDeductible(out canIntegralPerMoney, out canCapital);
            ViewBag.CanIntegralPerMoney = canIntegralPerMoney;
            ViewBag.CanCapital = canCapital;

            ViewBag.Keyword = SiteSettings.Keyword;
            if (productType == 1 && productId > 0)
            {
                submitModel.VirtualProductItemInfos = ProductManagerApplication.GetVirtualProductItemInfoByProductId(productId);
                var virtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(productId);
                if (virtualProductInfo != null && virtualProductInfo.ValidityType && DateTime.Now > virtualProductInfo.EndDate.Value)
                {
                    throw new MallException("该虚拟商品已过期，不支持下单");
                }
            }
            submitModel.ProductType = productType;
            if (productType == 1 && submitModel.products != null && submitModel.products.Count > 0)
            {
                var verificationShipper = ShopShippersApplication.GetDefaultVerificationShipper(submitModel.products.FirstOrDefault().shopId);
                if (verificationShipper != null)
                {
                    ViewBag.ShipperAddress = RegionApplication.GetFullName(verificationShipper.RegionId) + " " + verificationShipper.Address;
                    ViewBag.ShipperTelPhone = verificationShipper.TelPhone;
                }
            }
            return View("Submit", submitModel);
        }

        /// <summary>
        /// 获取当前用户收货地址列表
        /// </summary>
        /// <returns>收货地址列表</returns>
        [HttpPost]
        public JsonResult GetUserShippingAddresses()
        {
            var json = OrderApplication.GetUserShippingAddresses(UserId);
            return Json(json);
        }

        /// <summary>
        /// 从购物车中提交订单时调用的POST方法
        /// </summary>
        /// <param name="model"></param>
        /// <returns>返回订单列表和调转地址</returns>
        [HttpPost]
        public JsonResult SubmitOrder(CommonModel.OrderPostModel model, string payPwd)
        {
            model.CurrentUser = CurrentUser;
            model.PlatformType = (int)PlatformType.PC;
            if (string.IsNullOrEmpty(model.LatAndLng)) model.LatAndLng = "0,0";
            var result = OrderApplication.SubmitOrder(model, payPwd);
            #region 处理虚拟订单项
            if (model.ProductType == 1 && model.VirtualProductItems != null && model.VirtualProductItems.Count() > 0)
            {
                var orderId = result.OrderIds.FirstOrDefault();
                if (orderId > 0)
                {
                    var orderItemInfo = OrderApplication.GetOrderItemsByOrderId(orderId).FirstOrDefault();
                    if (orderItemInfo != null)
                    {
                        var list = model.VirtualProductItems.ToList().Where(a => !string.IsNullOrWhiteSpace(a.Content)).ToList();//过滤空项
                        list.ForEach(a =>
                        {
                            a.OrderId = orderId;
                            a.OrderItemId = orderItemInfo.Id;
                            if (a.VirtualProductItemType == (sbyte)Mall.Entities.ProductInfo.VirtualProductItemType.Picture)
                            {
                                a.Content = MoveImages(a.Content, CurrentUser.Id);
                            }
                        });
                        if (list.Count > 0)
                        {
                            OrderApplication.AddVirtualOrderItemInfo(list);
                        }
                    }
                }
            }
            #endregion

            return Json(new { success = result.Success, orderIds = result.OrderIds, redirect = result.OrderTotal > 0 });
        }
        private string MoveImages(string image, long userId)
        {
            List<string> content = new List<string>();
            if (string.IsNullOrWhiteSpace(image))
            {
                return "";
            }
            var list = image.Split(',').ToList();
            if (list != null && list.Count > 0)
            {
                list.ForEach(a =>
                {
                    var oldname = Path.GetFileName(a);
                    string ImageDir = string.Empty;
                    //转移图片
                    string relativeDir = "/Storage/Plat/VirtualProduct/";
                    string fileName = userId + oldname;
                    if (a.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
                    {
                        var de = a.Substring(a.LastIndexOf("/temp/"));
                        Core.MallIO.CopyFile(de, relativeDir + fileName, true);
                        content.Add(relativeDir + fileName);
                    }  //目标地址
                    else if (a.Contains("/Storage"))
                    {
                        content.Add(a.Substring(a.LastIndexOf("/Storage")));
                    }
                    else
                        content.Add(a);
                });
            }
            return string.Join(",", content);
        }

        /// <summary>
        /// 限时购订单提交
        /// </summary>
        /// <param name="skuIds">库存ID</param>
        /// <param name="counts">购买数量</param>
        /// <param name="recieveAddressId">客户收货区域ID</param
        /// 
        /// 
        /// <param name="couponIds">优惠卷</param>
        /// <param name="invoiceType">发票类型0不要发票1增值税发票2普通发票</param>
        /// <param name="invoiceTitle">发票抬头</param>
        /// <param name="invoiceContext">发票内容</param>
        /// <param name="integral">使用积分</param>
        /// <param name="collpIds">组合构ID</param>
        /// <param name="isCashOnDelivery">是否货到付款</param>
        /// <returns>redis方式返回虚拟订单ID，数据库方式返回实际订单ID</returns>
        //[HttpPost]
        //public JsonResult SubmitLimitOrder(CommonModel.OrderPostModel model, string payPwd)
        //{
        //    model.CurrentUser = CurrentUser;
        //    model.PlatformType = (int)PlatformType.PC;
        //    if (string.IsNullOrEmpty(model.LatAndLng)) model.LatAndLng = "0,0";
        //    var result = OrderApplication.GetLimitOrder(model);
        //    if (LimitOrderHelper.IsRedisCache())
        //    {
        //        string id = "";
        //        SubmitOrderResult r = LimitOrderHelper.SubmitOrder(result, out id, payPwd);
        //        if (r == SubmitOrderResult.SoldOut)
        //            throw new MallException("已售空");
        //        else if (r == SubmitOrderResult.NoSkuId)
        //            throw new InvalidPropertyException("创建订单的时候，SKU为空，或者数量为0");
        //        else if (r == SubmitOrderResult.NoData)
        //            throw new InvalidPropertyException("参数错误");
        //        else if (r == SubmitOrderResult.NoLimit)
        //            throw new InvalidPropertyException("没有限时购活动");
        //        else if (string.IsNullOrEmpty(id))
        //            throw new InvalidPropertyException("参数错误");
        //        else
        //        {
        //            return Json(new { success = true, Id = id });
        //        }
        //    }
        //    else
        //    {
        //        var orderIds = OrderApplication.OrderSubmit(result, payPwd);
        //        return Json(new { success = true, orderIds = orderIds });
        //    }
        //}

        /// <summary>
        /// 确认零元订单
        /// </summary
        /// <param name="orderIds">订单ID集合</param>
        /// <returns>返回付款成功</returns>
        [HttpPost]
        public ActionResult PayConfirm(string orderIds)
        {
            if (string.IsNullOrEmpty(orderIds))
            {
                return RedirectToAction("index", "userCenter", new { url = "/userOrder", tar = "userOrder" });
            }
            OrderApplication.ConfirmOrder(UserId, orderIds);
            return RedirectToAction("ReturnSuccess", "pay", new { id = orderIds });
        }

        /// <summary>
        /// 进入支付页面
        /// </summary>
        /// <param name="orderIds">订单Id集合</param>
        /// <returns></returns>
        public ActionResult Pay(string orderIds)
        {
            //网站根目录
            string webRoot = CurrentUrlHelper.CurrentUrlNoPort();
            var result = OrderApplication.GetPay(UserId, orderIds, webRoot);
            if (!result.IsSuccess)
            {
                if (result.Msg == "没有钱要付")
                {
                    return RedirectToAction("Index", "UserOrder");
                }
                else
                {
                    throw new MallException(result.Msg);
                }
            }
            else
            {
                ViewBag.Orders = result.Orders;
                ViewBag.OrderIds = orderIds;
                ViewBag.TotalAmount = result.TotalAmount;
                ViewBag.Logo = SiteSettings.Logo;//获取Logo
                ViewBag.Keyword = SiteSettings.Keyword;
                if (result.TotalAmount == 0)
                {
                    return View("PayConfirm");
                }
                else
                {
                    ViewBag.HaveNoSalePro = result.HaveNoSalePro;
                    ViewBag.Step = 1;//支付第一步
                    ViewBag.UnpaidTimeout = SiteSettings.UnpaidTimeout;
                    return View(result.Models);
                }
            }
        }

        //TODO:【2015-09-07】是否设置支付密码
        /// <summary>
        /// 判断是否设置支付密码
        /// </summary>
        public JsonResult GetPayPwd()
        {
            bool result = false;
            result = OrderApplication.GetPayPwd(UserId);
            return Json(new { success = result });
        }

        /// <summary>
        /// 判断预存款支付密码
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public JsonResult ValidPayPwd(string pwd)
        {
            var ret = MemberApplication.VerificationPayPwd(CurrentUser.Id, pwd);
            return Json(new { success = ret, msg = "密码错误" });
        }

        //TODO:【2015-09-01】预付款支付
        /// <summary>
        /// 预存款支付
        /// </summary>
        /// <param name="orderIds">订单Id集合</param>
        /// <param name="pwd">密码</param>
        /// <returns>支付结果</returns>
        public JsonResult PayByCapital(string orderIds, string pwd)
        {
            OrderApplication.PayByCapital(UserId, orderIds, pwd, Request.Host.ToString());
            return Json(new { success = true, msg = "支付成功" });
        }

        //TODO:增加资产充值
        /// <summary>
        /// 增加资产充值
        /// </summary>
        /// <param name="orderIds">订单id集合</param>
        /// <returns></returns>
        public ActionResult ChargePay(string orderIds)
        {
            string webRoot = CurrentUrlHelper.CurrentUrlNoPort();
            if (string.IsNullOrEmpty(orderIds))
            {
                return RedirectToAction("index", "userCenter", new { url = "/UserCapital", tar = "UserCapital" });
            }
            var viewmodel = OrderApplication.ChargePay(UserId, orderIds, webRoot);
            ViewBag.Keyword = SiteSettings.Keyword;
            return View(viewmodel);
        }

        /// <summary>
        /// 获取运费
        /// </summary>
        /// <param name="addressId">地址ID</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CalcFreight(int addressId, CalcFreightparameter[] parameters)
        {
            var result = OrderApplication.CalcFreight(addressId, parameters.GroupBy(p => p.ShopId).ToDictionary(p => p.Key, p => p.GroupBy(pp => pp.ProductId).ToDictionary(pp => pp.Key, pp => string.Format("{0}${1}", pp.Sum(ppp => ppp.Count), pp.Sum(ppp => ppp.Amount)))));
            if (result.Count == 0)
                return Json(new { success = false, msg = "计算运费失败" });
            else
                return Json(new { success = true, freights = result.Select(p => new { shopId = p.Key, freight = p.Value }).ToArray() });
        }

        /// <summary>
        /// 设置发票抬头
        /// </summary>
        /// <param name="name">抬头名称</param>
        /// <returns>返回抬头ID</returns>
        [HttpPost]
        public JsonResult SaveInvoiceTitle(string name, string code, long id = 0)
        {
            return Json(OrderApplication.SaveInvoiceTitle(UserId, name, code, id));
        }

        /// <summary>
        /// 保存发票信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveInvoiceTitleNew(InvoiceTitleInfo para)
        {
            para.UserId = UserId;
            OrderApplication.SaveInvoiceTitleNew(para);
            return Json(new { success = true, msg = "保存成功" });
        }

        /// <summary>
        /// 删除发票抬头
        /// </summary>
        /// <param name="id">抬头ID</param>
        /// <returns>是否完成</returns>
        [HttpPost]
        public JsonResult DeleteInvoiceTitle(long id)
        {
            OrderApplication.DeleteInvoiceTitle(id);
            return Json(true);
        }

        /// <summary>
        /// 是否支持货到付款
        /// </summary>
        /// <param name="addreddId"></param>
        /// <returns></returns>
        public JsonResult IsCashOnDelivery(long addreddId)
        {
            var result = PaymentConfigApplication.IsCashOnDelivery(addreddId);

            return Json(result);
        }

        #region 私有方法
        private void InitOrderSubmitModel(DTO.OrderSubmitModel model)
        {
            if (model.address != null)
            {
                var query = new ShopBranchQuery();
                query.Status = ShopBranchStatus.Normal;

                var region = RegionApplication.GetRegion(model.address.RegionId, CommonModel.Region.RegionLevel.City);
                if (region != null)
                {
                    query.AddressPath = region.GetIdPath();
                }
                foreach (var item in model.products)
                {
                    query.ShopId = item.shopId;
                    query.ProductIds = item.freightProductGroup.Select(p => p.ProductId).ToArray();
                    query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
                    item.ExistShopBranch = ShopBranchApplication.Exists(query);
                }
            }
        }
        #endregion

        /// <summary>
        /// 获取商家分店[从移动端迁移出]
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="regionId"></param>
        /// <param name="getParent"></param>
        /// <param name="skuIds"></param>
        /// <param name="counts"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="shippingAddressId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetShopBranchs(long shopId, long regionId, bool getParent, List<string> skuIds, List<int> counts, int page, int rows, long shippingAddressId)
        {
            var shippingAddressInfo = ShippingAddressApplication.GetUserShippingAddress(shippingAddressId);
            int streetId = 0, districtId = 0;//收货地址的街道、区域

            var query = new ShopBranchQuery()
            {
                ShopId = shopId,
                PageNo = page,
                PageSize = rows,
                Status = ShopBranchStatus.Normal,
                ShopBranchProductStatus = ShopBranchSkuStatus.Normal,
                IsAboveSelf = true    //自提点，只取自提门店
            };
            if (shippingAddressInfo != null)
            {
                query.FromLatLng = string.Format("{0},{1}", shippingAddressInfo.Latitude, shippingAddressInfo.Longitude);//需要收货地址的经纬度
                streetId = shippingAddressInfo.RegionId;
                var parentAreaInfo = RegionApplication.GetRegion(shippingAddressInfo.RegionId, Region.RegionLevel.Town);//判断当前区域是否为第四级
                if (parentAreaInfo != null && parentAreaInfo.ParentId > 0) districtId = parentAreaInfo.ParentId;
                else { districtId = streetId; streetId = 0; }
            }
            bool hasLatLng = false;
            if (!string.IsNullOrWhiteSpace(query.FromLatLng)) hasLatLng = query.FromLatLng.Split(',').Length == 2;

            var region = RegionApplication.GetRegion(regionId, getParent ? Region.RegionLevel.City : Region.RegionLevel.County);
            if (region != null) query.AddressPath = region.GetIdPath();

            #region 3.0版本排序规则
            var skuInfos = ProductManagerApplication.GetSKUs(skuIds);
            query.ProductIds = skuInfos.Select(p => p.ProductId).ToArray();
            var data = ShopBranchApplication.GetShopBranchsAll(query);
            var shopBranchSkus = ShopBranchApplication.GetSkus(shopId, data.Models.Select(p => p.Id).ToList(), skuIds);//获取该商家下具有订单内所有商品的门店状态正常数据,不考虑库存
            data.Models.ForEach(p =>
            {
                p.Enabled = skuInfos.All(skuInfo => shopBranchSkus.Any(sbSku => sbSku.ShopBranchId == p.Id && sbSku.Stock >= counts[skuInfos.IndexOf(skuInfo)] && sbSku.SkuId == skuInfo.Id));
            });

            List<ShopBranch> newList = new List<ShopBranch>();
            List<long> fillterIds = new List<long>();
            var currentList = data.Models.Where(p => hasLatLng && p.Enabled && (p.Latitude > 0 && p.Longitude > 0)).OrderBy(p => p.Distance).ToList();
            if (currentList != null && currentList.Count() > 0)
            {
                fillterIds.AddRange(currentList.Select(p => p.Id));
                newList.AddRange(currentList);
            }
            var currentList2 = data.Models.Where(p => !fillterIds.Contains(p.Id) && p.Enabled && p.AddressPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + streetId + CommonConst.ADDRESS_PATH_SPLIT)).ToList();
            if (currentList2 != null && currentList2.Count() > 0)
            {
                fillterIds.AddRange(currentList2.Select(p => p.Id));
                newList.AddRange(currentList2);
            }
            var currentList3 = data.Models.Where(p => !fillterIds.Contains(p.Id) && p.Enabled && p.AddressPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + districtId + CommonConst.ADDRESS_PATH_SPLIT)).ToList();
            if (currentList3 != null && currentList3.Count() > 0)
            {
                fillterIds.AddRange(currentList3.Select(p => p.Id));
                newList.AddRange(currentList3);
            }
            var currentList4 = data.Models.Where(p => !fillterIds.Contains(p.Id) && p.Enabled).ToList();//非同街、非同区，但一定会同市
            if (currentList4 != null && currentList4.Count() > 0)
            {
                fillterIds.AddRange(currentList4.Select(p => p.Id));
                newList.AddRange(currentList4);
            }
            var currentList5 = data.Models.Where(p => !fillterIds.Contains(p.Id)).ToList();//库存不足的排最后
            if (currentList5 != null && currentList5.Count() > 0)
            {
                newList.AddRange(currentList5);
            }
            if (newList.Count() != data.Models.Count())//如果新组合的数据与原数据数量不一致，则异常
            {
                return Json(new
                {
                    rows = ""
                }, true);
            }
            var needDistance = false;
            if (shippingAddressInfo != null && shippingAddressInfo.Latitude != 0 && shippingAddressInfo.Longitude != 0)
            {
                needDistance = true;
            }
            var models = new
            {
                rows = newList.Select(sb => new
                {
                    sb.ContactUser,
                    sb.ContactPhone,
                    sb.AddressDetail,
                    sb.ShopBranchName,
                    sb.Id,
                    Enabled = sb.Enabled,
                    Distance = needDistance ? RegionApplication.GetDistance(sb.Latitude, sb.Longitude, shippingAddressInfo.Latitude, shippingAddressInfo.Longitude) : 0
                }).ToArray(),
                Total = newList.Count
            };
            #endregion

            return Json(models, true);
        }

        private void CanDeductible(out bool canIntegralPerMoney, out bool canCapital)
        {
            //授权模块控制积分抵扣、余额抵扣功能是否开放
            canIntegralPerMoney = true;
            canCapital = true;

            if (!(SiteSettings.IsOpenPC || SiteSettings.IsOpenH5 || SiteSettings.IsOpenApp || SiteSettings.IsOpenMallSmallProg))
            {
                canIntegralPerMoney = false;
            }

            if (!(SiteSettings.IsOpenPC || SiteSettings.IsOpenH5 || SiteSettings.IsOpenApp || SiteSettings.IsOpenMallSmallProg))
            {
                canCapital = false;
            }

        }
    }

    //delete-pengjiangxiong
    /// <summary>
    /// 服务器异步处理限时购订单
    /// </summary>
    //public class OrderStateController : BaseAsyncController
    //{
    //    public void CheckAsync(string id)
    //    {
    //        AsyncManager.OutstandingOperations.Increment();
    //        int interval = 3000;//定义刷新间隔为200ms
    //        int maxWaitingTime = 15 * 1000;//定义最大等待时间为15s
    //        long[] orderIds;
    //        string message = "";
    //        Task.Factory.StartNew(() =>
    //        {
    //            int time = 0;
    //            while (true)
    //            {
    //                time += interval;
    //                System.Threading.Thread.Sleep(interval);
    //                OrderState state = LimitOrderHelper.GetOrderState(id, out message, out orderIds);
    //                if (state == OrderState.Processed)//已处理
    //                {
    //                    AsyncManager.Parameters["state"] = state.ToString();
    //                    AsyncManager.Parameters["message"] = message;
    //                    AsyncManager.Parameters["ids"] = orderIds;
    //                    break;
    //                }
    //                else if (state == OrderState.Untreated)//未处理
    //                {
    //                    if (time > maxWaitingTime)
    //                    {//大于最大等待时间
    //                        AsyncManager.Parameters["state"] = state.ToString();
    //                        AsyncManager.Parameters["message"] = message;
    //                        AsyncManager.Parameters["ids"] = null;
    //                        break;
    //                    }
    //                    else
    //                        continue;
    //                }
    //                else//出错
    //                {
    //                    AsyncManager.Parameters["state"] = state.ToString();
    //                    AsyncManager.Parameters["message"] = message;
    //                    AsyncManager.Parameters["ids"] = null;
    //                    break;
    //                }
    //            }
    //            AsyncManager.OutstandingOperations.Decrement();
    //        });
    //    }


    //    public JsonResult CheckCompleted(string state, string message, long[] ids)
    //    {
    //        return Json(new { state = state, message = message, orderIds = ids });
    //    }
    //}

}


