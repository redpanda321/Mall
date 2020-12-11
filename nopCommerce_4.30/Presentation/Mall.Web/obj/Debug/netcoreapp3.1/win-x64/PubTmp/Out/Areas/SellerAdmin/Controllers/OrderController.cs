using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Mall.Entities;
using Mall.Web.Framework;
using Mall.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Mall.Web.Areas.SellerAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using Microsoft.AspNetCore.Http;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class OrderController : BaseSellerController
    {

        private IOrderService _iOrderService;
        private IExpressService _iExpressService;
        private IRegionService _iRegionService;
        private IShopService _iShopService;
        private IProductService _iProductService;
        private IRefundService _iRefundService;
        private IShopOpenApiService _iShopOpenApiService;
        private IFightGroupService _iFightGroupService;
        private IPaymentConfigService _iPaymentConfigService;
        public OrderController(IOrderService iOrderService,
            IExpressService iExpressService,
            IRegionService iRegionService,
            IShopService iShopService,
            IProductService iProductService,
            IRefundService iRefundService,
             IShopOpenApiService iShopOpenApiService,
             IFightGroupService iFightGroupService,
            IPaymentConfigService iPaymentConfigService
            )
        {
            _iOrderService = iOrderService;
            _iExpressService = iExpressService;
            _iRegionService = iRegionService;
            _iShopService = iShopService;
            _iProductService = iProductService;
            _iRefundService = iRefundService;
            _iShopOpenApiService = iShopOpenApiService;
            _iFightGroupService = iFightGroupService;
            _iPaymentConfigService = iPaymentConfigService;
        }

        public class SendGoodMode
        {
            public List<Mall.DTO.Order> Orders { get; set; }
            public IEnumerable<ExpressCompany> LogisticsCompanies { get; set; }
        }
        public ActionResult Management(long? shopBranchId)
        {
            var shopopen = _iShopOpenApiService.Get(CurrentSellerManager.ShopId);
            ViewBag.CanGoBills = false;
            if (shopopen != null)
            {
                if (!string.IsNullOrWhiteSpace(shopopen.AppKey))
                {
                    if (shopopen.IsEnable && shopopen.IsRegistered)
                    {
                        ViewBag.CanGoBills = true;
                    }
                }
            }
            #region 是否开启门店授权
            bool isOpenStore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            if (isOpenStore)
            {
                #region 商家下所有门店
                var data = ShopBranchApplication.GetShopBranchsAll(new ShopBranchQuery()
                {
                    ShopId = CurrentSellerManager.ShopId
                });
                ViewBag.StoreList = data.Models;
                #endregion
            }
            ViewBag.IsOpenStore = isOpenStore;
            #endregion
            ViewBag.hasHistory = SiteSettingApplication.SiteSettings.IsOpenHistoryOrder;
            var model = PaymentApplication.GetPaymentTypeDesc();
            ViewBag.ShopId = this.CurrentSellerManager.ShopId;
            ViewBag.ShopBranchId = shopBranchId.HasValue ? shopBranchId.Value : 0;
            ViewBag.ShopName = this.CurrentShop.ShopName;
            return View(model);
        }


        public ActionResult Detail(long id, bool updatePrice = false)
        {
            var order = _iOrderService.GetOrder(id);
            if (order == null || order.ShopId != CurrentSellerManager.ShopId)
                throw new MallException("订单已被删除，或者不属于该店铺！");

            if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
            {
                var fgord = _iFightGroupService.GetFightGroupOrderStatusByOrderId(order.Id);
                order.FightGroupOrderJoinStatus = fgord.GetJoinStatus;
                order.FightGroupCanRefund = fgord.CanRefund;
            }
            ViewBag.UpdatePrice = updatePrice;

            //TODO:FG 推荐建立ViewModel

            ViewBag.Logs = _iOrderService.GetOrderLogs(order.Id);
            //补充商品货号
            var orderiItems = _iOrderService.GetOrderItemsByOrderId(order.Id);
            ViewBag.OrderItems = orderiItems;

            var proids = orderiItems.Select(d => d.ProductId);
            var products = ProductManagerApplication.GetProductByIds(proids);
            var procodelist = products.Select(d => new { d.Id, d.ProductCode, d.FreightTemplateId }).ToList();
            foreach (var item in orderiItems)
            {
                var _tmp = procodelist.Find(d => d.Id == item.ProductId);
                if (_tmp != null)
                {
                    item.ProductCode = _tmp.ProductCode;
                    item.FreightId = _tmp.FreightTemplateId;
                }
            }
            var service = ServiceApplication.Create<IProductService>();

            var freightProductGroup = orderiItems.GroupBy(a => a.FreightId);
            if (order.OrderType != OrderInfo.OrderTypes.Virtual)
            {
                if (order.DeliveryType != CommonModel.DeliveryType.SelfTake)
                {
                    var regionService = ServiceApplication.Create<Mall.IServices.IRegionService>();
                    var region = regionService.GetRegion(order.RegionId);
                    int cityId = 0;
                    if (region != null)
                    {
                        cityId = region.Id;
                    }
                    foreach (var f in freightProductGroup)
                    {
                        var productIds = f.Select(a => a.ProductId);
                        var counts = f.Select(a => Convert.ToInt32(a.Quantity));
                        decimal freight = service.GetFreight(productIds, counts, cityId);

                        foreach (var item in f)
                        {
                            item.Freight = freight;
                        }
                    }
                }
            }
            ViewBag.freightProductGroup = freightProductGroup;
            string shipperAddress = string.Empty, shipperTelPhone = string.Empty;
            #region 门店信息
            if (order.ShopBranchId > 0)
            {
                var shopBranchInfo = ShopBranchApplication.GetShopBranchById(order.ShopBranchId);
                if (shopBranchInfo != null)
                {
                    ViewBag.ShopBranchInfo = shopBranchInfo;
                    if (order.OrderStatus == OrderInfo.OrderOperateStatus.Finish) ViewBag.ShopBranchContactUser = shopBranchInfo.UserName;
                    if (order.OrderType == OrderInfo.OrderTypes.Virtual)
                    {
                        shipperAddress = RegionApplication.GetFullName(shopBranchInfo.AddressId) + " " + shopBranchInfo.AddressDetail;
                        shipperTelPhone = shopBranchInfo.ContactPhone;
                    }
                }
            }
            #endregion
           
            if (order.OrderType == OrderInfo.OrderTypes.Virtual)
            {
                ViewBag.VirtualOrderItemInfos = OrderApplication.GetVirtualOrderItemInfosByOrderId(order.Id);
                ViewBag.OrderVerificationCodeInfos = OrderApplication.GetOrderVerificationCodeInfosByOrderIds(new List<long>() { order.Id });
                if (order.ShopBranchId == 0)
                {
                    var verificationShipper = ShopShippersApplication.GetDefaultVerificationShipper(order.ShopId);
                    if (verificationShipper != null)
                    {
                        shipperAddress = RegionApplication.GetFullName(verificationShipper.RegionId) + " " + verificationShipper.Address;
                        shipperTelPhone = verificationShipper.TelPhone;
                    }
                }
            }
            ViewBag.ShipperAddress = shipperAddress;
            ViewBag.ShipperTelPhone = shipperTelPhone;
            //发票信息
            ViewBag.OrderInvoiceInfo = OrderApplication.GetOrderInvoiceInfo(order.Id);
            //统一显示支付方式名称
            order.PaymentTypeName = PaymentApplication.GetPaymentTypeDescById(order.PaymentTypeGateway) ?? order.PaymentTypeName;

            return View(order);

        }

        private int GetCityRegion(int regionId)
        {
            var region = RegionApplication.GetRegion(regionId);
            if (region == null)
                return 0;
            if (region.Level > Region.RegionLevel.City)
            {
                while (region.Level != Region.RegionLevel.City)
                {
                    region = region.Parent;
                }
            }
            return region.Id;
        }
        [HttpPost]
        [UnAuthorize]
        public JsonResult List(OrderQuery query, int page, int rows)
        {
            query.ShopId = CurrentSellerManager.ShopId;
            query.PageNo = page;
            query.PageSize = rows;
            query.Operator = Operator.Seller;
            query.PaymentTypeGateways = PaymentApplication.GetPaymentIdByDesc(query.PaymentTypeGateway);
            //var orders = OrderApplication.GetOrders(query);
            var fullOrders = OrderApplication.GetFullOrders(query);
            var models = fullOrders.Models.ToList();

            var shops = Application.ShopApplication.GetShops(models.Select(p => p.ShopId).ToList());

            IEnumerable<OrderModel> orderModels = models.Select(item =>
            {
                var shop = shops.FirstOrDefault(sp => sp.Id == item.ShopId);
                return new OrderModel()
                {
                    OrderId = item.Id,
                    DeliveryType = item.DeliveryType,
                    OrderStatus = item.OrderStatus.ToDescription(),
                    OrderDate = item.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    ShopId = item.ShopId,
                    ShopName = item.ShopBranchId > 0 ? item.ShopBranchName : item.ShopName,
                    ShopBranchName = item.ShopBranchName,
                    UserId = item.UserId,
                    UserName = item.UserName,
                    TotalPrice = item.OrderTotalAmount,
                    PaymentTypeName = item.PaymentTypeName,
                    IconSrc = GetIconSrc(item.Platform),
                    PlatForm = (int)item.Platform,
                    PlatformText = item.Platform.ToDescription(),
                    PaymentType = item.PaymentType,
                    PaymentTypeStr = item.PaymentTypeDesc,
                    OrderType = item.OrderType,
                    SellerRemark = item.SellerRemark,
                    SellerRemarkFlag = item.SellerRemarkFlag,
                    GatewayOrderId = item.GatewayOrderId,
                    Payee = shop.ContactsName,
                    CellPhone = item.CellPhone,
                    RegionFullName = item.RegionFullName,
                    Address = !string.IsNullOrEmpty(item.Address) ? item.Address.Replace("\n", "").Replace("\t", "") : "",
                    ReceiveLatitude = item.ReceiveLatitude,
                    ReceiveLongitude = item.ReceiveLongitude,
                    UserRemark = item.UserRemark,
                    ShipOrderNumber = item.ShipOrderNumber,
                    OrderItems = item.OrderItems,
                    ShopBranchId = item.ShopBranchId,
                    RegionId = GetCityRegion(item.RegionId),
                    IsVirtual = item.OrderType == OrderInfo.OrderTypes.Virtual
                };
            });
            orderModels = orderModels.ToList();

            #region 数据补偿
            //EDIT DZY [150624]
            List<long> ordidl = orderModels.Select(d => d.OrderId).ToList();
            if (ordidl.Count > 0)
            {
                RefundQuery refquery = new RefundQuery();
                //refquery.OrderId = ordidl[0];
                refquery.MoreOrderId = ordidl;
                refquery.PageNo = 1;
                refquery.PageSize = orderModels.Count();
                var reflist = _iRefundService.GetOrderRefunds(refquery).Models.Where(d => d.RefundMode == Entities.OrderRefundInfo.OrderRefundMode.OrderRefund && d.SellerAuditStatus != Entities.OrderRefundInfo.OrderRefundAuditStatus.UnAudit).ToList();


                foreach (var item in orderModels)
                {
                    //退款状态补偿
                    if (reflist.Count() > 0)
                    {
                        var _tmpobj = reflist.FirstOrDefault(d => d.OrderId == item.OrderId);
                        if (_tmpobj != null && item.OrderStatus != OrderInfo.OrderOperateStatus.Close.ToDescription())
                        {
                            item.RefundStatusText = ((_tmpobj.SellerAuditStatus == Entities.OrderRefundInfo.OrderRefundAuditStatus.Audited)
                                ? _tmpobj.ManagerConfirmStatus.ToDescription()
                                : ((item.DeliveryType == CommonModel.DeliveryType.SelfTake || item.ShopBranchId > 0) ? ((CommonModel.Enum.OrderRefundShopAuditStatus)_tmpobj.SellerAuditStatus).ToDescription() : _tmpobj.SellerAuditStatus.ToDescription()));
                        }
                    }

                    item.FightGroupCanRefund = true;   //非拼团订单默认可退
                    item.CanSendGood = true;   //非拼团订单默认可以发货

                    if (item.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery.ToDescription())
                    {
                        item.CanSendGood = false;
                    }

                    //拼团状态补偿
                    if (item.OrderType == Entities.OrderInfo.OrderTypes.FightGroup)
                    {
                        var fgord = _iFightGroupService.GetFightGroupOrderStatusByOrderId(item.OrderId);
                        if (fgord != null)
                        {
                            item.FightGroupJoinStatus = fgord.GetJoinStatus;
                            item.FightGroupCanRefund = fgord.CanRefund;
                            item.CanSendGood = item.CanSendGood && fgord.CanSendGood;
                        }
                        else
                        {
                            item.FightGroupJoinStatus = CommonModel.FightGroupOrderJoinStatus.JoinFailed;
                            item.FightGroupCanRefund = false;
                            item.CanSendGood = false;
                        }
                    }

                }
            }
            #endregion

            DataGridModel<OrderModel> dataGrid = new DataGridModel<OrderModel>() { rows = orderModels, total = fullOrders.Total };
            return Json(dataGrid);

        }

        public JsonResult GoExpressBills()
        {
            Result result = new Result();
            result.success = false;
            result.status = -1;
            result.msg = "未开启开放平台";

            var erpuri = System.Configuration.ConfigurationManager.AppSettings["HishopErpDomain"];
            string gourl = "http://hierp.kuaidiantong.cn/ExpressBill/Allot?app_key={0}&timestamp={1}&sign={2}";
            if (!string.IsNullOrWhiteSpace(erpuri))
            {
                gourl = erpuri + "/ExpressBill/Allot?app_key={0}&timestamp={1}&sign={2}";
            }
            var shopopen = _iShopOpenApiService.Get(CurrentSellerManager.ShopId);
            ViewBag.CanGoBills = false;
            if (shopopen != null)
            {
                if (!string.IsNullOrWhiteSpace(shopopen.AppKey) && !string.IsNullOrWhiteSpace(shopopen.AppSecreat))
                {
                    if (shopopen.IsEnable && shopopen.IsRegistered)
                    {
                        SortedDictionary<string, string> data = new SortedDictionary<string, string>();
                        data.Add("app_key", shopopen.AppKey);
                        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        data.Add("timestamp", timestamp);
                        string sign = Hishop.Open.Api.OpenApiSign.GetSign(data, shopopen.AppSecreat);
                        gourl = string.Format(gourl, shopopen.AppKey, timestamp, sign);

                        result.success = true;
                        result.status = 1;
                        result.msg = gourl;
                    }
                    if (!shopopen.IsRegistered)
                    {
                        result.msg = "未完成ERP电子面单系统注册";
                    }
                }
            }

            return Json(result);
        }

        public ActionResult ExportToExcel(OrderQuery query)
        {
            query.ShopId = CurrentSellerManager.ShopId;
            var orders = OrderApplication.GetAllFullOrders(query);

            return ExcelView("ExportOrderinfo", "店铺订单信息", orders);
        }

        /// <summary>
        /// 获取订单来源图标地址
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        string GetIconSrc(PlatformType platform)
        {
            if (platform == PlatformType.IOS || platform == PlatformType.Android)
                return "/images/app.png";
            return string.Format("/images/{0}.png", platform.ToString());
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="payRemark">收款备注</param>
        /// <returns></returns>
        [ShopOperationLog(Message = "商家取消订单")]
        [HttpPost]
        public JsonResult CloseOrder(long orderId)
        {
            Result result = new Result();
            try
            {
                _iOrderService.SellerCloseOrder(orderId, CurrentSellerManager.UserName);
                result.success = true;
            }
            catch (Exception ex)
            {
                result.msg = ex.Message;
            }
            return Json(result);
        }

        public void DownloadProductList(string ids)
        {
            IEnumerable<long> idList = ids.Split(',').Select(item => long.Parse(item));

            var orderItems = _iOrderService.GetOrderItemsByOrderId(idList);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<html><head><meta http-equiv=Content-Type content=\"text/html; charset=gb2312\"></head><body>");
            sb.AppendLine("<table cellspacing=\"0\" cellpadding=\"5\" rules=\"all\" border=\"1\">");
            sb.AppendLine("<caption style='text-align:center;'>配货单(仓库拣货表)</caption>");
            sb.AppendLine("<tr style=\"font-weight: bold; white-space: nowrap;\">");
            sb.AppendLine("<td>商品名称</td>");
            sb.AppendLine("<td>货号</td>");
            sb.AppendLine("<td>规格</td>");
            sb.AppendLine("<td>拣货数量</td>");
            sb.AppendLine("<td>现库存数</td>");
            sb.AppendLine("</tr>");

            long stock = 0;
            Entities.SKUInfo sku;
            foreach (var orderItem in orderItems)
            {
                sku = _iProductService.GetSku(orderItem.SkuId);
                if (sku != null)
                    stock = sku.Stock;

                sb.AppendLine("<tr>");
                sb.AppendLine("<td>" + orderItem.ProductName + "</td>");
                sb.AppendLine("<td style=\"vnd.ms-excel.numberformat:@\">" + orderItem.SKU + "</td>");
                sb.AppendLine("<td>" + orderItem.Color + orderItem.Size + orderItem.Version + "</td>");
                sb.AppendLine("<td>" + orderItem.Quantity + "</td>");
                sb.AppendLine("<td>" + (stock + orderItem.Quantity).ToString() + "</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            sb.AppendLine("</body></html>");

            Response.Clear();
           // Response.Buffer = false;
            //Response.Charset = "GB2312";
            //Response.AppendHeader("Content-Disposition", "attachment;filename=" + "productgoods_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
            //Response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");
            Response.ContentType = "application/ms-excel";
            //EnableViewState = false;
            Response.WriteAsync(sb.ToString()).Wait();
           // Response.End();
        }
        /// <summary>
        /// 商品配货表
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public FileResult DownloadOrderProductList(string ids)
        {
           // HttpContext.Response.BufferOutput = true;
            string fileName = "goods_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

            string UserAgent = Request.Query["http_user_agent"].ToString().ToLower();
            // Firfox和IE下输出中文名显示正常 
            if (UserAgent.IndexOf("firefox") == -1)
            {
                fileName = System.Web.HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8);
            }
            //写入内容到Excel 
            NPOI.HSSF.UserModel.HSSFWorkbook hssfworkbook = writeProductToExcel(ids);
            //将Excel内容写入到流中 
            MemoryStream file = new MemoryStream();
            hssfworkbook.Write(file);
            file.Seek(0, SeekOrigin.Begin);
            return File(file, "application/vnd.ms-excel", fileName);

        }
        /// <summary>
        /// 订单配货表
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [UnAuthorize]
        public FileResult DownloadOrderList(string ids)
        {
            //IEnumerable<long> idList = ids.Split(',').Select(item => long.Parse(item));

            //IEnumerable<OrderInfo> orders = _iOrderService.GetOrders(idList);

            //StringBuilder sb = new StringBuilder();
            //sb.AppendLine("<html><head><meta http-equiv=Content-Type content=\"text/html; charset=gb2312\"></head><body>");
            //sb.AppendLine("<table cellspacing=\"0\" cellpadding=\"5\" rules=\"all\" border=\"1\">");
            //sb.AppendLine("<caption style='text-align:center;'>配货单(仓库拣货表)</caption>");
            //sb.AppendLine("<tr style=\"font-weight: bold; white-space: nowrap;\">");
            //sb.AppendLine("<td>订单单号</td>");
            //sb.AppendLine("<td>商品名称</td>");
            //sb.AppendLine("<td>货号</td>");
            //sb.AppendLine("<td>规格</td>");
            //sb.AppendLine("<td>拣货数量</td>");
            //sb.AppendLine("<td>现库存数</td>");
            //sb.AppendLine("<td>备注</td>");
            //sb.AppendLine("</tr>");

            //long stock = 0;
            //SKUInfo sku;

            //foreach (OrderInfo order in orders.ToList())
            //{
            //    foreach (OrderItemInfo orderItem in order.OrderItemInfo.ToList())
            //    {
            //        sku = _iProductService.GetSku(orderItem.SkuId);
            //        if (sku != null)
            //            stock = sku.Stock;

            //        sb.AppendLine("<tr>");
            //        sb.AppendLine("<td style=\"vnd.ms-excel.numberformat:@\">" + order.Id + "</td>");
            //        sb.AppendLine("<td>" + orderItem.ProductName + "</td>");
            //        sb.AppendLine("<td style=\"vnd.ms-excel.numberformat:@\">" + orderItem.SKU + "</td>");
            //        sb.AppendLine("<td>" + orderItem.Color + orderItem.Size + orderItem.Version + "</td>");
            //        sb.AppendLine("<td>" + orderItem.Quantity + "</td>");
            //        sb.AppendLine("<td>" + (stock + orderItem.Quantity).ToString() + "</td>");
            //        sb.AppendLine("<td>" + order.UserRemark + "</td>");
            //        sb.AppendLine("</tr>");
            //    }
            //}
            //sb.AppendLine("</table>");
            //sb.AppendLine("</body></html>");
            //// 输出
            ////
            //Response.Clear();
            //Response.Buffer = false;
            //Response.Charset = "GB2312";
            //Response.AppendHeader("Content-Disposition", "attachment;filename=" + "ordergoods_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
            //Response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");
            //Response.ContentType = "application/ms-excel";
            //Response.Write(sb.ToString());
            //Response.End();


         //   HttpContext.Response.BufferOutput = true;
            string fileName = "ordergoods_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

            string UserAgent = Request.Query["http_user_agent"].ToString().ToLower();
            // Firfox和IE下输出中文名显示正常 
            if (UserAgent.IndexOf("firefox") == -1)
            {
                fileName = System.Web.HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8);
            }
            //写入内容到Excel 
            NPOI.HSSF.UserModel.HSSFWorkbook hssfworkbook = writeToExcel(ids);
            //将Excel内容写入到流中 
            MemoryStream file = new MemoryStream();
            hssfworkbook.Write(file);
            file.Seek(0, SeekOrigin.Begin);
            return File(file, "application/vnd.ms-excel", fileName);

        }
        /// <summary>
        /// 导出商品配货单
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private NPOI.HSSF.UserModel.HSSFWorkbook writeProductToExcel(string ids)
        {
            long stock = 0;
            string productCode = string.Empty;
            var idList = ids.Split(',').Select(item => long.Parse(item));
            var orders = _iOrderService.GetOrders(idList);
            var allOrderItems = OrderApplication.GetOrderItemsByOrderId(orders.Select(p => p.Id));
            var skus = ProductManagerApplication.GetSKUInfos(allOrderItems.Select(p => p.SkuId));
            var products = ProductManagerApplication.GetProductsByIds(allOrderItems.Select(p => p.ProductId));

            var items = new List<PrepareGoodsModel> { };
            foreach (var order in orders)
            {
                var orderitems = allOrderItems.Where(p => p.OrderId == order.Id);
                foreach (var item in orderitems)
                {
                    var sku = skus.FirstOrDefault(p => p.Id == item.SkuId);
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (sku != null)
                    {
                        stock = sku.Stock;
                        productCode = product.ProductCode;
                    }
                    else
                    {
                        stock = 0;
                        productCode = string.Empty;
                    }

                    items.Add(new PrepareGoodsModel
                    {
                        SkuId = item.SkuId,
                        ProductName = item.ProductName,
                        ProductCode = productCode,
                        Standard = item.Color + item.Size + item.Version,
                        Quantity = item.Quantity,
                        Stock = stock
                    });
                }
            }
            var prepares = items.GroupBy(e => e.SkuId).Select(e => new PrepareGoodsModel
            {
                SkuId = e.Key,
                ProductName = e.FirstOrDefault().ProductName,
                ProductCode = e.FirstOrDefault().ProductCode,
                Standard = e.FirstOrDefault().Standard,
                Quantity = e.Sum(item => item.Quantity),
                Stock = e.Min(n => n.Stock)
            });

            NPOI.HSSF.UserModel.HSSFWorkbook hssfworkbook = new NPOI.HSSF.UserModel.HSSFWorkbook();

            NPOI.HSSF.UserModel.HSSFSheet excelSheet = (NPOI.HSSF.UserModel.HSSFSheet)hssfworkbook.CreateSheet("sheet1");

            IRow row0 = excelSheet.CreateRow(0);
            string[] columns ={
                                  "商品名称","货号","规格","拣货数量","现库存数"
                             };
            for (int i = 0; i < columns.Length; i++)
            {
               ICell cell = CreateCell(i, row0);
                cell.SetCellValue(columns[i]);
            }

            foreach (var orderItem in prepares)
            {
                int rows = excelSheet.PhysicalNumberOfRows;
                IRow row = CreateRow(rows, excelSheet);
              ICell cell0 = CreateCell(0, row);
               ICellStyle style = hssfworkbook.CreateCellStyle();
            IDataFormat format = hssfworkbook.CreateDataFormat();
                style.DataFormat = format.GetFormat("####################");
                cell0.CellStyle = style;
                cell0.SetCellValue(orderItem.ProductName);
                cell0 = CreateCell(1, row);
                cell0.SetCellValue(orderItem.ProductCode);
                cell0 = CreateCell(2, row);
                cell0.SetCellValue(orderItem.Standard);
                cell0 = CreateCell(3, row);
                cell0.SetCellValue(orderItem.Quantity);
                cell0 = CreateCell(4, row);
                cell0.SetCellValue(orderItem.Stock + orderItem.Quantity);
            }
            return hssfworkbook;
        }
        private NPOI.HSSF.UserModel.HSSFWorkbook writeToExcel(string ids)
        {
            long stock = 0;
            string productCode = string.Empty;
            IEnumerable<long> idList = ids.Split(',').Select(item => long.Parse(item));
            var orders = _iOrderService.GetOrders(idList);

            NPOI.HSSF.UserModel.HSSFWorkbook hssfworkbook = new NPOI.HSSF.UserModel.HSSFWorkbook();

            NPOI.HSSF.UserModel.HSSFSheet excelSheet = (NPOI.HSSF.UserModel.HSSFSheet)hssfworkbook.CreateSheet("sheet1");

         IRow row0 = excelSheet.CreateRow(0);
            string[] columns ={
                                  "订单单号","商品名称","货号","规格","拣货数量","现库存数","备注"
                             };
            for (int i = 0; i < columns.Length; i++)
            {
                ICell cell = CreateCell(i, row0);
                cell.SetCellValue(columns[i]);
            }
            var allOrderItems = OrderApplication.GetOrderItemsByOrderId(orders.Select(p => p.Id));
            var skus = ProductManagerApplication.GetSKUInfos(allOrderItems.Select(p => p.SkuId));
            var products = ProductManagerApplication.GetProductsByIds(allOrderItems.Select(p => p.ProductId));
            foreach (var order in orders)
            {
                var orderitems = allOrderItems.Where(p => p.OrderId == order.Id);
                foreach (var orderItem in orderitems)
                {
                    var sku = skus.FirstOrDefault(p => p.Id == orderItem.SkuId);
                    var product = products.FirstOrDefault(p => p.Id == orderItem.ProductId);
                    if (sku != null)
                    {
                        stock = sku.Stock;
                        productCode = product.ProductCode;
                    }
                    else
                    {
                        stock = 0;
                        productCode = string.Empty;
                    }

                    int rows = excelSheet.PhysicalNumberOfRows;
                   IRow row = CreateRow(rows, excelSheet);
                    ICell cell0 = CreateCell(0, row);
                    ICellStyle style = hssfworkbook.CreateCellStyle();
                   IDataFormat format = hssfworkbook.CreateDataFormat();
                    style.DataFormat = format.GetFormat("####################");
                    cell0.CellStyle = style;
                    cell0.SetCellValue(order.Id.ToString());
                    cell0 = CreateCell(1, row);
                    cell0.SetCellValue(orderItem.ProductName);
                    cell0 = CreateCell(2, row);
                    cell0.SetCellValue(productCode);
                    cell0 = CreateCell(3, row);
                    cell0.SetCellValue(orderItem.Color + orderItem.Size + orderItem.Version);
                    cell0 = CreateCell(4, row);
                    cell0.SetCellValue(orderItem.Quantity);
                    cell0 = CreateCell(5, row);
                    cell0.SetCellValue((stock + orderItem.Quantity).ToString());
                    cell0 = CreateCell(6, row);
                    cell0.SetCellValue(order.UserRemark);

                }
            }


            return hssfworkbook;
        }

        private IRow CreateRow(int rowID, NPOI.HSSF.UserModel.HSSFSheet excelSheet)
        {
            IRow row = excelSheet.GetRow(rowID);
            if (row == null)
            {
                row = excelSheet.CreateRow(rowID);
            }
            return row;
        }

        private ICell CreateCell(int cellID, IRow row)
        {
            ICell cell = row.GetCell(cellID);
            if (cell == null)
            {
                cell = row.CreateCell(cellID);
            }
            return cell;
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult GetOrderPrint(string ids)
        {
            Result result = new Result();
            try
            {
                IEnumerable<long> idList = ids.Split(',').Select(item => long.Parse(item));

                var orders = _iOrderService.GetOrders(idList);
                string siteName = SiteSettingApplication.SiteSettings.SiteName;
                StringBuilder sb = new StringBuilder();
                foreach (var order in orders.ToList())
                {
                    sb.AppendFormat("<h3 class=\"print-order\"><strong>{0}店发货清单</strong></h3>", order.ShopName);
                    sb.Append("<div class=\"print-detail\">");
                    sb.AppendFormat("<span>订单号：{0}</span><span>下单时间：{1}</span>", order.Id, order.OrderDate.ToString("yyyy-MM-dd hh:mm:ss"));
                    sb.AppendFormat("<span>姓名：{0}</span><span>联系方式：{1}</span>", order.ShipTo, order.CellPhone);
                    sb.AppendFormat("<span>地址：{0}</span>", order.RegionFullName + " " + order.Address);
                    sb.Append("</div>");


                    sb.Append("<table class=\"table table-bordered print-tab\"><thead><tr><th>商品名称</th><th>规格</th><th>数量</th><th>单价</th><th>总价</th></tr></thead><tbody>");
                    var orderitems = _iOrderService.GetOrderItemsByOrderId(order.Id);
                    foreach (var orderItem in orderitems)
                    {
                        sb.Append("<tr>");
                        sb.AppendFormat("<td style=\"text-align:left\">{0}</td>", orderItem.ProductName);
                        sb.AppendFormat("<td>{0} {1} {2}</td>", orderItem.Color, orderItem.Size, orderItem.Version);
                        sb.AppendFormat("<td>{0}</td>", orderItem.Quantity);
                        sb.AppendFormat("<td>￥{0}</td>", orderItem.SalePrice);
                        sb.AppendFormat("<td>￥{0}</td>", orderItem.RealTotalPrice);
                        sb.Append("</tr>");
                    }
                    sb.AppendFormat("<tr><td style=\"text-align:right\" colspan=\"6\"><span>商品总价：￥{0} &nbsp; 运费：￥{1}</span> &nbsp; <b>实付金额：￥{2}</b></td></tr>",
                        order.ProductTotalAmount, order.Freight, order.OrderTotalAmount);
                    sb.AppendLine("</tbody></table>");
                    sb.Append("<div class=\"print-tags\">");
                    sb.AppendFormat("<p><strong>备注：</strong></p>");
                    sb.AppendFormat("<p class=\"tag-content\">{0}</p>", order.OrderRemarks);
                    sb.Append("</div>");

                }
                result.success = true;
                result.msg = sb.ToString();
            }
            catch (Exception)
            {
                result.success = false;
            }

            return Json(result);
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult UpdateAddress(long orderId, string shipTo, string cellPhone, int topRegionId, int regionId, string address)
        {
            Result result = new Result();
            try
            {
                string regionFullName = RegionApplication.GetFullName(regionId);
                _iOrderService.SellerUpdateAddress(orderId, CurrentSellerManager.UserName, shipTo, cellPhone, topRegionId, regionId, regionFullName, address);
                result.success = true;
            }
            catch (Exception ex)
            {
                result.msg = ex.Message;
            }
            return Json(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult UpdateItemDiscountAmount(long orderItemId, decimal discountAmount)
        {
            Result result = new Result();
            try
            {
                _iOrderService.SellerUpdateItemDiscountAmount(orderItemId, discountAmount, CurrentSellerManager.UserName);
                result.success = true;
            }
            catch (Exception ex)
            {
                result.msg = ex.Message;
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult UpdateOrderFrieght(long orderId, decimal frieght)
        {
            _iOrderService.SellerUpdateOrderFreight(orderId, frieght);
            return Json(new { success = true });
        }

        public ActionResult SendGood(string ids)
        {
            IEnumerable<long> idList = ids.Split(',').Select(item => long.Parse(item));

            var sendGoodMode = new SendGoodMode();
            var orders = OrderApplication.GetOrders(idList).Where(a => a.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitDelivery && a.ShopId == CurrentSellerManager.ShopId && a.ShopBranchId== 0).OrderByDescending(a => a.OrderDate);//商家后台发货应排除门店订单
            if (orders == null)
            {
                throw new MallException("没有找到相关的订单" + ids);
            }
            sendGoodMode.Orders = orders.ToList();
            sendGoodMode.LogisticsCompanies = ExpressApplication.GetAllExpress();
            return View(sendGoodMode);
        }
        /// <summary>
        /// 获取发货单号
        /// </summary>
        /// <param name="cname"></param>
        /// <param name="snum">起始单号</param>
        /// <param name="count"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetBatOrderShipNumber(string cname, string snum, int count)
        {
            List<string> result = new List<string>();
            string curnum = snum;
            result.Add(curnum);
            for (int i = 0; i < count; i++)
            {
                curnum = ExpressApplication.GetNextExpressCode(cname, curnum);
                result.Add(curnum);
            }
            return Json(new { success = true, data = result });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="companyNames"></param>
        /// <param name="shipOrderNumbers"></param>
        /// <param name="needShip"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SendGood(string ids, string companyNames, string shipOrderNumbers, bool needShip = true)
        {
            var result = new Result();
            var returnurl = String.Format("{0}/Common/ExpressData/SaveExpressData", CurrentUrlHelper.CurrentUrlNoPort());

            try
            {
                IEnumerable<long> idList = ids.Split(',').Select(item => long.Parse(item));
                string[] companyNameList = companyNames.Split(',');
                string[] shipOrderNumberList = shipOrderNumbers.Split(',');

                int index = 0;
                foreach (long id in idList)
                {
                    if (needShip)
                    {
                        Application.OrderApplication.SellerSendGood(id, CurrentSellerManager.UserName, companyNameList[index], shipOrderNumberList[index], returnurl);
                    }
                    else
                    {
                        Application.OrderApplication.SellerSendGood(id, CurrentSellerManager.UserName, "商家合作物流", "商家合作物流", returnurl);
                    }
                    index++;
                }
                result.success = true;
            }
            catch (Exception ex)
            {
                Log.Error("商家发货操作失败", ex);
                result.msg = ex.Message;
            }

            return Json(result);
        }

        public ActionResult UpdateExpress(long id)
        {
            var sendGoodMode = new SendGoodMode();
            var order = OrderApplication.GetOrder(id);
            if (order == null || order.ShopId != CurrentSellerManager.ShopId)
            {
                throw new MallException("没有找到相关的订单" + id);
            }
            sendGoodMode.Orders = new List<Mall.DTO.Order>() { order };
            sendGoodMode.LogisticsCompanies = ExpressApplication.GetAllExpress();
            return View("SendGood", sendGoodMode);
        }

        [HttpPost]
        public JsonResult UpdateExpress(long ids, string companyNames, string shipOrderNumbers)
        {
            var result = new Result();
            var returnurl = String.Format("{0}/Common/ExpressData/SaveExpressData", CurrentUrlHelper.CurrentUrlNoPort());

            try
            {
                Application.OrderApplication.UpdateExpress(ids, companyNames, shipOrderNumbers, returnurl);
                result.success = true;
            }
            catch (Exception ex)
            {
                result.msg = ex.Message;
            }

            return Json(result);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult GetExpressData(string expressCompanyName, string shipOrderNumber)
        {
            string kuaidi100Code = _iExpressService.GetExpress(expressCompanyName).Kuaidi100Code;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format("https://www.kuaidi100.com/query?type={0}&postid={1}", kuaidi100Code, shipOrderNumber));
            request.Timeout = 8000;

            string content = "暂时没有此快递单号的信息";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = response.GetResponseStream();
                    System.IO.StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.GetEncoding("UTF-8"));

                    // 读取流字符串内容
                    content = streamReader.ReadToEnd();
                    content = content.Replace("&amp;", "");
                    content = content.Replace("&nbsp;", "");
                    content = content.Replace("&", "");
                }
            }
            catch
            {
            }
            return Json(content);
        }

        public JsonResult GetRegionIdPath(long regionId)
        {
            return Json(_iRegionService.GetRegionPath(regionId));
        }


        [HttpPost]
        public JsonResult GetRegion(long? key = null, int? level = -1)
        {
            if (level == -1)
                key = 0;

            if (key.HasValue)
            {
                var regions = _iRegionService.GetRegion(key.Value);
                return Json(regions);
            }
            else
                return Json(new object[] { });
        }


        public JsonResult SellerRemark(long id, string remark, int? flag)
        {
            var shopId = CurrentShop.Id;
            if (!flag.HasValue)
            {
                flag = 1;
            }
            if (remark.Length == 0)
            {
                return Json(new Result() { success = false, msg = "备注不能为空！" });
            }
            OrderApplication.UpdateSellerRemark(id, shopId, remark, flag.Value);
            return Json(new Result() { success = true, msg = "添加备注成功！" });
        }


        public ActionResult Print(string orderIds)
        {
            Models.OrderPrintViewModel model = new Models.OrderPrintViewModel();
            var orderIds_long = orderIds.Split(',').Select(item => long.Parse(item));
            model.OrdersCount = orderIds_long.Count();

            var expresses = ExpressApplication.GetRecentExpress(CurrentSellerManager.ShopId, int.MaxValue);
            model.Expresses = expresses;
            return View(model);
        }


        [HttpPost]
        public JsonResult Print(string orderIds, string expressName, string startNo)
        {
            var express = ExpressApplication.GetExpress(expressName);

            if (!express.CheckExpressCodeIsValid(startNo))
            {
                return Json(new { success = false, msg = "起始快递单号无效" });
            }
            else
            {
                var printElementIndexes = express.Elements.Select(item => item.ElementType);
                List<PrintModel> printModels = new List<PrintModel>();

                IEnumerable<long> orderIds_long = orderIds.Split(',').Select(item => long.Parse(item));

                foreach (long orderId in orderIds_long)
                {//为每个订单建立打印数据模型
                 //设置基本打印信息
                    PrintModel printModel = new PrintModel()
                    {
                        Width = express.Width,
                        Height = express.Height,
                        FontSize = 11
                    };
                    //获取打印元素对应订单中的实际内容
                    var dic = ExpressApplication.GetPrintElementIndexAndOrderValue(CurrentSellerManager.ShopId, orderId, printElementIndexes);
                    //获取打印元素
                    printModel.Elements = dic.Select(item =>
                    {
                        var expressElement = express.Elements.FirstOrDefault(t => (int)t.ElementType == item.Key);
                        return new PrintModel.PrintElement()
                        {
                            X = expressElement.a[0],
                            Y = expressElement.a[1],
                            Height = expressElement.b[1] - expressElement.a[1],
                            Width = expressElement.b[0] - expressElement.a[0],
                            Value = item.Value
                        };
                    });
                    printModel.Elements = printModel.Elements.OrderBy(t => t.Y);
                    printModels.Add(printModel);
                }

                //保存订单信息
                _iOrderService.SetOrderExpressInfo(CurrentSellerManager.ShopId, expressName, startNo, orderIds_long);

                return Json(new { success = true, data = printModels });

            }

        }

        /// <summary>
        /// 查询同一区域配送范围内的商家下门店
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetArealShopBranch(int areaId, string latAndLng, int shopId)
        {
            if (areaId <= 0)
                return Json(new { Success = false, Message = "请先选择区域！" });
            if (shopId <= 0)
                return Json(new { Success = false, Message = "无法确定商家！" });

            var shopBranchs = ShopBranchApplication.GetArealShopBranchsAll(areaId, shopId, latAndLng);
            return Json(new { Success = true, Models = shopBranchs.Models });
        }

        /// <summary>
        /// 商家手动给订单分配门店
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="shopBranchId">要分配的门店ID</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AllotStore(long orderId, long shopBranchId, long shopId)
        {
            try
            {
                var orderInfo = OrderApplication.GetOrder(orderId);//这里要查一次,靠传值不准确
                if (orderInfo == null)
                    return Json(new { Success = false, Message = "获取订单错误！" });

                if (orderInfo.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitDelivery || orderInfo.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitPay)
                {
                    var refunds = RefundApplication.GetOrderRefunds(new RefundQuery()
                    {
                        OrderId = orderId,
                        ShopId = shopId,
                        PageSize = 1,
                        PageNo = 1
                    });
                    if (refunds != null && refunds.Total > 0) return Json(new { Success = false, Message = "订单正在申请售后，请先处理售后申请！" });

                    var orderItems = OrderApplication.GetOrderItemsByOrderId(orderId);
                    List<string> skuIds = orderItems.Where(p => p.ShopId == shopId && p.OrderId == orderId).Select(p => p.SkuId).ToList();//原则上一个订单只属于一个商家,所以此处可不判断shopId
                    List<int> counts = orderItems.Where(p => p.ShopId == shopId && p.OrderId == orderId).Select(p => TypeHelper.ObjectToInt(p.Quantity)).ToList();

                    if (shopBranchId > 0)//分配给门店
                    {
                        var resultStock = InventoryThrough(shopBranchId, shopId, skuIds, counts);
                        if (!resultStock)
                            return Json(new { Success = false, Message = "分配失败，该门店库存不足！" });

                        if (orderInfo != null  && orderInfo.ShopBranchId > 0)
                        {
                            if (orderInfo.ShopBranchId != shopBranchId)
                            {
                                OrderApplication.AllotStoreUpdateStockToNewShopBranch(skuIds, counts, shopBranchId, orderInfo.ShopBranchId);
                                OrderApplication.UpdateOrderShopBranch(orderId, shopBranchId);// 更新订单所属门店为新分配的门店
                                orderInfo.ShopBranchId = shopBranchId;
                                if (orderInfo.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitDelivery) AddAppMessages(orderInfo);
                            }
                        }
                        else
                        {
                            OrderApplication.AllotStoreUpdateStock(skuIds, counts, shopBranchId);
                            OrderApplication.UpdateOrderShopBranch(orderId, shopBranchId);
                            orderInfo.ShopBranchId = shopBranchId;
                            if (orderInfo.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitDelivery) AddAppMessages(orderInfo);
                        }
                    }
                    else//分配给商家
                    {
                        if (orderInfo != null && orderInfo.ShopBranchId > 0)
                        {
                            OrderApplication.AllotStoreUpdateStockToShop(skuIds, counts, orderInfo.ShopBranchId);
                            OrderApplication.UpdateOrderShopBranch(orderId, 0);// 更新订单所属门店为商家总店，门店ID为0
                            orderInfo.ShopBranchId = 0;
                            if (orderInfo.OrderStatus == Entities.OrderInfo.OrderOperateStatus.WaitDelivery) AddAppMessages(orderInfo);
                        }
                    }
                    return Json(new { Success = true, Message = "分配成功！" });
                }
                else
                {
                    return Json(new { Success = false, Message = "订单只能在待付款、待发货状态下分配门店！" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// 判断门店是否具有订单中所有商品的库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="skuIds"></param>
        /// <param name="counts"></param>
        /// <returns></returns>
        private static bool InventoryThrough(long shopBranchId, long shopId, List<string> skuIds, List<int> counts)
        {
            var skuInfos = ProductManagerApplication.GetSKUs(skuIds);
            ShopBranchQuery query = new ShopBranchQuery();
            query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
            query.ProductIds = skuInfos.Select(p => p.ProductId).ToArray();
            query.Status = CommonModel.ShopBranchStatus.Normal;
            query.Id = shopBranchId;
            query.PageSize = 1;
            query.PageNo = 1;
            query.ShopId = shopId;

            var data = ShopBranchApplication.GetShopBranchsAll(query);//这里用来判断是否当前要分配的门店是否具有该订单内所有的商品
            var shopBranchSkus = ShopBranchApplication.GetSkus(query.ShopId, data.Models.Select(p => p.Id).ToList());//获取商家下门店的SKU
            return data.Models.Any(p => skuInfos.All(skuInfo => shopBranchSkus.Any(sbSku => sbSku.ShopBranchId == p.Id && sbSku.Stock >= counts[skuInfos.IndexOf(skuInfo)] && sbSku.SkuId == skuInfo.Id)));
        }

        [HttpGet]
        public JsonResult GetShopBranchStock(long orderId, long shopBranchId, long shopId)
        {
            if (orderId <= 0)
                return Json(new { Success = false, Message = "获取订单错误" });

            if (shopBranchId <= 0)//如果传过来的门店ID为0，则分配给商家
                return Json(new { Success = true });

            var orderItems = OrderApplication.GetOrderItemsByOrderId(orderId);
            List<string> skuIds = orderItems.Where(p => p.ShopId == shopId && p.OrderId == orderId).Select(p => p.SkuId).ToList();
            List<int> counts = orderItems.Where(p => p.ShopId == shopId && p.OrderId == orderId).Select(p => TypeHelper.ObjectToInt(p.Quantity)).ToList();

            var resultStock = InventoryThrough(shopBranchId, shopId, skuIds, counts);
            if (resultStock)
                return Json(new { Success = true });

            return Json(new { Success = false, Message = "当前门店库存不足" });
        }

        /// <summary>
        /// 新增门店/商家APP发货通知
        /// </summary>
        /// <param name="appMessages"></param>
        public static void AddAppMessages(DTO.Order orderInfo)
        {
            var app = new DTO.AppMessages()
            {
                Content = string.Format("{0} 等待您发货", orderInfo.Id),
                IsRead = false,
                SendTime = DateTime.Now,
                SourceId = orderInfo.Id,
                Title = "您有新的订单",
                TypeId = (int)AppMessagesType.Order,
                OrderPayDate = TypeHelper.ObjectToDateTime(orderInfo.PayDate),
                ShopId = 0,
                ShopBranchId = 0
            };
            if (orderInfo.ShopBranchId > 0)
            {
                app.ShopBranchId = orderInfo.ShopBranchId;
            }
            else
                app.ShopId = orderInfo.ShopId;
            AppMessageApplication.AddAppMessages(app);
        }

        /// <summary>
        /// 区分售后状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetRrefundType(long orderId, long shopId)
        {
            var queryModel = new RefundQuery()
            {
                OrderId = orderId,
                ShopId = shopId,
                PageSize = 1,
                PageNo = 1
            };
            var refunds = Application.RefundApplication.GetOrderRefunds(queryModel);
            if (refunds != null && refunds.Models.Count > 0)
            {
                var refundInfo = refunds.Models.FirstOrDefault();
                int showType = (refundInfo.RefundMode == OrderRefundInfo.OrderRefundMode.OrderItemRefund || refundInfo.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund) ? 2 : (refundInfo.RefundMode == OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund ? 3 : 0);
                if (showType == 0)
                    return Json(new { Success = false, Message = "判断售后状态错误" });

                return Json(new { Success = true, showtype = showType });
            }
            return Json(new { Success = false, Message = "无法判断售后状态" });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="verificationCode"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult VerifyVerification(string verificationCode, long shopId)
        {
            Result result = new Result();
            result.success = true;
            try
            {
                if (verificationCode.Length < 12)
                {
                    result.success = false; result.code = 1;//该核销码无效
                    return Json(result);
                }
                if (!Mall.Core.Helper.ValidateHelper.IsNumeric(verificationCode))
                {
                    result.success = false; result.code = 1;//该核销码无效
                    return Json(result);
                }
                var orderVerificationCodeInfo = OrderApplication.GetOrderVerificationCodeInfoByCode(verificationCode);
                if (orderVerificationCodeInfo != null)
                {
                    result.data = orderVerificationCodeInfo.OrderId;
                    var orderItem = OrderApplication.GetOrderItem(orderVerificationCodeInfo.OrderItemId);
                    var orderInfo = OrderApplication.GetOrder(orderVerificationCodeInfo.OrderId);
                    var virtualProductInfo = ProductManagerApplication.GetVirtualProductInfoByProductId(orderItem.ProductId);
                    if (orderVerificationCodeInfo.Status == OrderInfo.VerificationCodeStatus.AlreadyVerification)
                    {
                        result.success = false; result.code = 2; result.msg = orderVerificationCodeInfo.VerificationTime.HasValue ? orderVerificationCodeInfo.VerificationTime.Value.ToString("yyyy-MM-dd HH:mm") : ""; //已核销
                        return Json(result);
                    }

                    if (orderItem != null && orderItem.ShopId != shopId)
                    {
                        result.success = false; result.code = 3;//非本店核销码，请买家核对信息
                        return Json(result);
                    }
                    if (orderInfo != null && orderInfo.ShopBranchId > 0)
                    {
                        result.success = false; result.code = 3;//非本店核销码，请买家核对信息[商家后台不能核销门店虚拟订单]
                        return Json(result);
                    }
                    if (orderVerificationCodeInfo.Status == OrderInfo.VerificationCodeStatus.Expired)
                    {
                        result.success = false; result.code = 4;//此核销码已过期，无法核销
                        return Json(result);
                    }
                    if (orderVerificationCodeInfo.Status == OrderInfo.VerificationCodeStatus.Refund)
                    {
                        result.success = false; result.code = 5;//此核销码正处于退款中，无法核销
                        return Json(result);
                    }
                    if (orderVerificationCodeInfo.Status == OrderInfo.VerificationCodeStatus.RefundComplete)
                    {
                        result.success = false; result.code = 6;//此核销码已经退款成功，无法核销
                        return Json(result);
                    }

                    if (orderItem != null && orderItem.EffectiveDate.HasValue)
                    {
                        if (DateTime.Now < orderItem.EffectiveDate.Value)
                        {
                            result.success = false; result.code = 7;//该核销码暂时不能核销，请留意生效时间！
                            return Json(result);
                        }
                    }
                    if (virtualProductInfo != null && virtualProductInfo.ValidityType && DateTime.Now < virtualProductInfo.StartDate.Value)
                    {
                        result.success = false; result.code = 7; 
                        return Json(result);
                    }
                }
                else
                {
                    result.success = false; result.code = 1;//该核销码无效
                    return Json(result);
                }
            }
            catch (Exception ex)
            {
                result.msg = ex.Message;
            }
            return Json(result);
        }

        /// <summary>
        /// 根据核销码获取核销详情
        /// </summary>
        /// <param name="verificationCode"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetVerficationDetail(long orderId, long shopId)
        {
            Result result = new Result();
            result.success = false;
            var orderInfo = OrderApplication.GetOrder(orderId);
            if (orderInfo == null)
            {
                result.msg = "找不到对应的订单信息";
                return Json(result);
            }
            if (shopId != this.CurrentSellerManager.ShopId)
            {
                result.msg = "非本店核销码，请买家核对信息";
                return Json(result);
            }
            if (orderInfo.ShopBranchId > 0)//商家后台不能核销门店虚拟订单
            {
                result.msg = "非本店核销码，请买家核对信息";
                return Json(result);
            }
            VerficationDetail verficationDetail = new Models.VerficationDetail();
            var verifications = OrderApplication.GetOrderVerificationCodeInfosByOrderIds(new List<long>() { orderId }).Where(a => a.Status == OrderInfo.VerificationCodeStatus.WaitVerification);
            var virtualOrderItemInfos = OrderApplication.GetVirtualOrderItemInfosByOrderId(orderId);
            verficationDetail.VerificationCodes = verifications.ToList();//待消费的核销码
            if (verficationDetail.VerificationCodes != null)
            {
                verficationDetail.VerificationCodes.ForEach(a =>
                {
                    a.SourceCode = a.VerificationCode;
                    a.VerificationCode = System.Text.RegularExpressions.Regex.Replace(a.VerificationCode, "(\\d{4})\\d{4}(\\d{4})", "$1 **** $2");
                });
            }
            verficationDetail.VirtualOrderItemInfos = virtualOrderItemInfos;
            if (verifications != null && verifications.Count() > 0)
            {
                var orderItem = OrderApplication.GetOrderItem(verifications.FirstOrDefault().OrderItemId);
                if (orderItem != null)
                {
                    orderItem.Color = orderItem.Color ?? string.Empty;
                    orderItem.Size = orderItem.Size ?? string.Empty;
                    orderItem.Version = orderItem.Version ?? string.Empty;
                    var productInfo = ProductManagerApplication.GetProduct(orderItem.ProductId);
                    verficationDetail.OrderItemInfo = orderItem;
                    if (productInfo != null)
                    {
                        productInfo.ImagePath = productInfo.GetImage(Mall.CommonModel.ImageSize.Size_220);
                    }
                    verficationDetail.ProductInfo = productInfo;
                }
            }
            result.data = verficationDetail;
            result.success = true;
            return Json(result);
        }

        /// <summary>
        /// 确认核销
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CommitVerification(string verficationCodes, long shopId, long orderId)
        {
            Result result = new Result();
            result.success = false;
            var orderInfo = OrderApplication.GetOrder(orderId);
            if (orderInfo == null)
            {
                result.msg = "找不到对应的订单信息";
                return Json(result);
            }
            if (string.IsNullOrWhiteSpace(verficationCodes))
            {
                result.msg = "核销码无效";
                return Json(result);
            }
            if (shopId != this.CurrentSellerManager.ShopId)
            {
                result.msg = "非本店核销码，请买家核对信息";
                return Json(result);
            }
            if (orderInfo.ShopBranchId > 0)//商家后台不能核销门店虚拟订单
            {
                result.msg = "非本店核销码，请买家核对信息";
                return Json(result);
            }
            List<string> codes = verficationCodes.Split(',').ToList();//确认核销的核销码
            if (codes != null && codes.Count > 0)
            {
                OrderApplication.UpdateOrderVerificationCodeStatusByCodes(codes, orderId, OrderInfo.VerificationCodeStatus.AlreadyVerification, DateTime.Now, this.CurrentSellerManager.UserName);
                OrderApplication.AddVerificationRecord(new VerificationRecordInfo()
                {
                    OrderId = orderId,
                    VerificationCodeIds = "," + verficationCodes + ",",
                    VerificationTime = DateTime.Now,
                    VerificationUser = this.CurrentSellerManager.UserName
                });
                result.success = true;
            }
            return Json(result);
        }
    }
}