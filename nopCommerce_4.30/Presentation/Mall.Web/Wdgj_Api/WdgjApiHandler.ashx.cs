using Mall.Application;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Mall.Web.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Mall.Web.Wdgj_Api
{
    /// <summary>
    /// WdgjApiHandler 的摘要说明
    /// </summary>
    public class WdgjApiHandler : IHttpHandler
    {
        //private IShopService _iShopService;
        //private IProductService _iProductService;
        //private IOrderService _iOrderService;

        string uCode;//暂时无用，用来在SAAS模式下区分店铺用
        string mType;//操作
        string timestamp;//时间戳
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            timestamp = context.Request["TimeStamp"];
            uCode = context.Request["uCode"];
            mType = context.Request["mType"];
            //string signStr = context.Request["Sign"];
            //if (string.IsNullOrWhiteSpace(timestamp) || !checkTimeStamp(timestamp))
            //{
            //    return ExMsg("时间戳错误");
            //}
            ////根据uCode获取效验码
            //var usign = Mall.ServiceProvider.Instance<IShopService>.Create.GetshopInfoByCode(wdgj.uCode).uSign;
            //if (!signStr.Equals(sign(uCode, mType, usign, timestamp)))
            //{
            //    return ExMsg("签名错误");
            //}

            switch (mType)
            {
                case "mGetGoods"://查询商品
                    context.Response.Write(findProduct(context));
                    return;
                case "mOrderSearch"://查询订单列表
                    context.Response.Write(GetOrderList(context));
                    return;
                case "mGetOrder"://订单详情
                    context.Response.Write(GetOrderDetail(context));
                    return;
                case "mSndGoods"://发货订单
                    context.Response.Write(SendOrder(context));
                    return;
                case "mSysGoods"://同步库存
                    context.Response.Write(AdjustQuantity(context));
                    return;
            }

        }
        /// <summary>
        /// 查询商品
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="uCode"></param>
        /// <param name="mType"></param>
        /// <param name="Sign"></param>
        /// <param name="PageSize"></param>
        /// <param name="Page"></param>
        /// <param name="GoodsType"></param>
        /// <param name="OuterID"></param>
        /// <param name="GoodsName"></param>
        /// <returns></returns>
        public string findProduct(HttpContext context)
        {
            string uCode = context.Request["uCode"];
            string PageSize = context.Request["PageSize"];
            string Page = context.Request["Page"];
            string GoodsType = context.Request["GoodsType"];
            string OuterID = context.Request["OuterID"];
            string GoodsName = context.Request["GoodsName"];
         

            ProductQuery query = new ProductQuery();
            int page_size = 0;
            int page_index = 0;
            if (!string.IsNullOrEmpty(PageSize))
                page_size = Convert.ToInt32(PageSize);
            if (!string.IsNullOrEmpty(Page))
                page_index = Convert.ToInt32(Page);
            string type = null;
            string code = null;
            string name = null;
            Entities.ProductInfo.ProductSaleStatus status = new Entities.ProductInfo.ProductSaleStatus();
            if (!string.IsNullOrWhiteSpace(GoodsType))//按类型
            {
                type = GoodsType; //1在售 2下架
                if (type.Equals("OnSale"))
                    status = Entities.ProductInfo.ProductSaleStatus.OnSale;
                else if (type.Equals("InStock"))
                    status = Entities.ProductInfo.ProductSaleStatus.InStock;
            }
            else if (!string.IsNullOrWhiteSpace(OuterID))//按商家编码
            {
                code = OuterID;
            }
            else if (!string.IsNullOrWhiteSpace(GoodsName))//按商品名称
            {
                name = GoodsName;
            }
            query.PageSize = page_size;
            query.PageNo = page_index;
            query.SaleStatus = status;
            query.KeyWords = name;
            query.ProductCode = code;
            query.ShopId = 1;
            if (ServiceApplication.Create<IShopService>().GetshopInfoByCode(uCode) != null)
                query.ShopId = ServiceApplication.Create<IShopService>().GetshopInfoByCode(uCode).ShopId; //根据uCode获取店铺信息

            var products = ProductManagerApplication.GetProducts(query);
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            try
            {
                sb.Append("<Goods>");
                foreach (var item in products.Models)
                {
                    sb.Append("<Ware>");

                    bool hasSku = ProductManagerApplication.HasSKU(item.Id);

                    int isSku = hasSku ? 1 : 0;
                    sb.Append(string.Format("<ItemID><![CDATA[{0}]]></ItemID>", item.Id));
                    sb.Append(string.Format("<ItemName><![CDATA[{0}]]></ItemName>", item.ProductName));
                    sb.Append(string.Format("<OuterID><![CDATA[{0}]]></OuterID>", item.ProductCode));
                    sb.Append(string.Format("<Price><![CDATA[{0}]]></Price>", item.MinSalePrice));
                    sb.Append(string.Format("<IsSku><![CDATA[{0}]]></IsSku>", isSku));
                    var skus = ProductManagerApplication.GetSKUs(item.Id);
                    sb.Append(string.Format("<Num><![CDATA[{0}]]></Num>", skus.Sum(d => d.Stock)));//总数       
                    sb.Append("<Items>");
                    if (hasSku)
                    {
                        foreach (var sku in skus)
                        {
                            sb.Append("<Item>");
                            sb.Append(string.Format("<Unit><![CDATA[{0}]]></Unit>", sku.Color + " " + sku.Size + " " + sku.Version));
                            sb.Append(string.Format("<SkuOuterID><![CDATA[{0}]]></SkuOuterID>", sku.Sku));
                            sb.Append(string.Format("<SkuID><![CDATA[{0}]]></SkuID>", sku.Id));
                            sb.Append(string.Format("<Num><![CDATA[{0}]]></Num>", sku.Stock));
                            sb.Append(string.Format("<SkuPrice><![CDATA[{0}]]></SkuPrice>", sku.SalePrice));
                            sb.Append("</Item>");
                        }
                    }
                    sb.Append("</Items>");
                    sb.Append("</Ware>");
                }
                sb.Append("<Result>1</Result>");
                sb.Append(string.Format("<TotalCount>{0}</TotalCount>", products.Total));
                sb.Append("<Cause></Cause>");
                sb.Append("</Goods>");
            }
            catch (Exception ex)
            {
                sb.Clear();
                sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sb.Append("<Rsp><Result>0</Result><Cause>" + ex.Message + "</Cause></Rsp>");
            }
            return sb.ToString();
        }
        /// <summary>
        /// 更新库存
        /// </summary>
        /// <param name="ItemID"></param>
        /// <param name="SkuID"></param>
        /// <param name="Quantity"></param>
        /// <returns></returns>
        public string AdjustQuantity(HttpContext context)
        {
            string ItemID = context.Request["ItemID"];
            string SkuID = context.Request["SkuID"];
            string Quantity = context.Request["Quantity"];

            int productId = Convert.ToInt32(ItemID);
            string SkuId = SkuID.Trim();
            int quantity = Convert.ToInt32(Quantity);
            string ids = "";
            if (!string.IsNullOrWhiteSpace(SkuId))
                ids = SkuId;
            else
                ids = string.Format("{0}_0_0_0", productId);

            StringBuilder sb = new StringBuilder();
            try
            {
                sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sb.Append("<Rsp>");
                if (string.IsNullOrEmpty(SkuId) || string.IsNullOrWhiteSpace(SkuId))
                {
                    try
                    {
                        ProductManagerApplication.SetProductStock(productId, quantity, CommonModel.StockOptionType.Normal);
                        var product = ProductManagerApplication.GetProduct(productId);
                        sb.Append("<Result>1</Result>");
                        if (product != null && product.SaleStatus == Entities.ProductInfo.ProductSaleStatus.OnSale)
                            sb.Append("<GoodsType>Onsale</GoodsType>");
                        else
                            sb.Append("<GoodsType>InStock</GoodsType>");
                        sb.Append("<Cause></Cause>");
                    }
                    catch
                    {
                        sb.Append("<Result>0</Result>");
                        sb.Append("<GoodsType></GoodsType>");
                        sb.Append("<Cause><![CDATA[{修改库存失败}]]></Cause>");
                    }
                }
                else
                {
                    var skuIdArr = SkuId.Split(',').ToList();
                    var quantitys = Quantity.Split(',').Select(e => long.Parse(e)).ToList();
                    var changes = new System.Collections.Generic.Dictionary<string, long>();
                    for (var i = 0; i < skuIdArr.Count; i++)
                        changes.Add(skuIdArr[i], quantitys[i]);
                    ProductManagerApplication.SetSkuStock(CommonModel.StockOptionType.Normal, changes);
                    var product = ProductManagerApplication.GetProduct(productId);
                    if (product != null)
                    {
                        sb.Append("<Result>1</Result>");
                        if (product != null && product.SaleStatus == Entities.ProductInfo.ProductSaleStatus.OnSale)
                            sb.Append("<GoodsType>Onsale</GoodsType>");
                        else
                            sb.Append("<GoodsType>InStock</GoodsType>");
                        sb.Append("<Cause></Cause>");
                    }
                    else
                    {
                        sb.Append("<Result>0</Result>");
                        sb.Append("<GoodsType></GoodsType>");
                        sb.Append("<Cause><![CDATA[{修改库存失败}]]></Cause>");
                    }
                }
                sb.Append("</Rsp>");
            }
            catch (Exception ex)
            {
                sb.Clear();
                sb.Append("<Result>0</Result>");
                sb.Append("<GoodsType></GoodsType>");
                sb.Append("<Cause>" + ex.Message + "</Cause>");
                sb.Append("</Rsp>");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 订单列表查询
        /// </summary>
        /// <param name="OrderStatus"></param>
        /// <param name="Start_Modified"></param>
        /// <param name="End_Modified"></param>
        /// <param name="PageSize"></param>
        /// <param name="Page"></param>
        /// <returns></returns>
        public string GetOrderList(HttpContext context)
        {
            string uCode = context.Request["uCode"];
            string PageSize = context.Request["PageSize"];
            string Page = context.Request["Page"];
            string OrderStatus = context.Request["OrderStatus"];
            string Start_Modified = context.Request["Start_Modified"];
            string End_Modified = context.Request["End_Modified"];

            int page_size = 0;
            int page_index = 0;
            page_size = Convert.ToInt32(PageSize);
            page_index = Convert.ToInt32(Page);
            string start_time = null;
            string end_time = null;
            if (!string.IsNullOrWhiteSpace(Start_Modified))//按修改时间
            {
                start_time = Start_Modified; //开始时间
            }
            if (!string.IsNullOrWhiteSpace(End_Modified))//结束时间
            {
                end_time = End_Modified;
            }
            OrderQuery query = new OrderQuery();
            switch (OrderStatus)
            {
                case "0":
                    query.Status = Entities.OrderInfo.OrderOperateStatus.WaitPay;
                    break;
                case "1":
                    query.Status = Entities.OrderInfo.OrderOperateStatus.WaitDelivery;
                    break;
                case "-1":
                    break;
                    //return "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Order><Result>1</Result><OrderCount>0</OrderCount><OrderList></OrderList></Order>";
            }
            if (!string.IsNullOrEmpty(start_time) && !string.IsNullOrEmpty(end_time))
            {
                query.StartDate = DateTime.Parse(start_time);
                query.EndDate = DateTime.Parse(end_time);
            }
            query.PageNo = page_index;
            query.PageSize = page_size;

            //根据uCode获取店铺信息
            if (ServiceApplication.Create<IShopService>().GetshopInfoByCode(uCode) != null)
                query.ShopId = ServiceApplication.Create<IShopService>().GetshopInfoByCode(uCode).ShopId;

            var orders = ServiceApplication.Create<IOrderService>().GetOrders(query);

            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.Append("<Order><OrderList>");
            foreach (var item in orders.Models)
            {
                sb.Append(string.Format("<OrderNO>{0}</OrderNO>", item.Id));
            }
            sb.Append("</OrderList>");
            sb.AppendFormat("<Page>{0}</Page>", page_index);
            sb.Append("<Cause></Cause>");
            sb.Append("<Result>1</Result>");
            sb.AppendFormat("<OrderCount>{0}</OrderCount>", orders.Total);
            sb.Append("</Order>");
            return sb.ToString();

            //XmlDocument xmlDoc = new XmlDocument();
            //XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
            //xmlDoc.AppendChild(node);
            //XmlNode root = xmlDoc.CreateElement("Order");
            //XmlNode OrderList = xmlDoc.CreateElement("OrderList");
            //root.AppendChild(OrderList);
            //foreach (var item in orders.Models)
            //{
            //    XmlNode OrderNO = xmlDoc.CreateElement("OrderNO");
            //    OrderNO.InnerText = item.Id.ToString();
            //    OrderList.AppendChild(OrderNO);
            //}
            //CreateNode(xmlDoc, root, "Page", page_index.ToString());
            //CreateNode(xmlDoc, root, "Cause", "");
            //CreateNode(xmlDoc, root, "Result", "1");
            //CreateNode(xmlDoc, root, "OrderCount", orders.Total.ToString());
            //xmlDoc.AppendChild(root);
            //return xmlDoc;
        }
        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="OrderNO"></param>
        /// <returns></returns>
        public string GetOrderDetail(HttpContext context)
        {
            string OrderNO = context.Request["OrderNO"];
            long OrderId = Convert.ToInt64(OrderNO.Trim());
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.Append("<Order>");
            sb.Append(string.Format("<OrderNO>{0}</OrderNO>", OrderId));
            var order = ServiceApplication.Create<IOrderService>().GetOrder(OrderId);
            if (order == null)
            {
                sb.Append("<Result>0</Result>");
                sb.Append("<Cause><![CDATA[{订单不存在}]]></Cause>");
            }
            else
            {
                sb.Append("<Result>1</Result>");
                sb.Append("<Cause></Cause>");
                switch (order.OrderStatus)
                {
                    case Entities.OrderInfo.OrderOperateStatus.WaitPay:
                        sb.Append("<OrderStatus>WAIT_BUYER_PAY</OrderStatus>");
                        break;
                    case Entities.OrderInfo.OrderOperateStatus.WaitDelivery:
                        sb.Append("<OrderStatus>WAIT_SELLER_SEND_GOODS</OrderStatus>");
                        break;
                    case Entities.OrderInfo.OrderOperateStatus.WaitReceiving:
                        sb.Append("<OrderStatus>WAIT_BUYER_CONFIRM_GOODS</OrderStatus>");
                        break;
                    case Entities.OrderInfo.OrderOperateStatus.Finish:
                        sb.Append("<OrderStatus>TRADE_FINISHED</OrderStatus>");
                        break;
                    case Entities.OrderInfo.OrderOperateStatus.Close:
                        sb.Append("<OrderStatus>TRADE_CLOSED</OrderStatus>");
                        break;
                }
                sb.Append(string.Format("<DateTime>{0}</DateTime>", order.OrderDate));
                sb.Append(string.Format("<BuyerID><![CDATA[{0}]]></BuyerID>", string.IsNullOrEmpty(order.UserName) ? "匿名" : order.UserName));
                sb.Append(string.Format("<BuyerName><![CDATA[{0}]]></BuyerName>", string.IsNullOrEmpty(order.ShipTo) ? "匿名" : order.ShipTo));
                sb.Append("<CardType></CardType>");
                sb.Append("<IDCard></IDCard>");
                sb.Append("<Country><![CDATA[{中国}]]></Country>");
                //辽宁省 大连市 沙河口区 星海湾街道
                string[] regions = order.RegionFullName.Replace("，", ",").Split(' ');
                if (regions.Length >= 3)
                {
                    sb.Append(string.Format("<Province><![CDATA[{0}]]></Province>", regions[0]));
                    sb.Append(string.Format("<City><![CDATA[{0}]]></City>", regions[1]));
                    sb.Append(string.Format("<Town><![CDATA[{0}]]></Town>", regions[2]));
                }
                sb.Append(string.Format("<Adr><![CDATA[{0}]]></Adr>", order.RegionFullName.Replace(" ", "") + order.Address));
                sb.Append(string.Format("<Zip><![CDATA[{0}]]></Zip>", ""));
                sb.Append(string.Format("<Email><![CDATA[{0}]]></Email>", ""));
                sb.Append(string.Format("<Phone><![CDATA[{0}]]></Phone>", order.CellPhone));
                sb.Append(string.Format("<Total>{0}</Total>", order.ProductTotalAmount));
                sb.Append("<Currency>CNY</Currency>");
                sb.Append(string.Format("<Postage>{0}</Postage>", order.Freight));
                sb.Append(string.Format("<PayAccount>{0}</PayAccount>", order.PaymentType));
                sb.Append(string.Format("<PayID>{0}</PayID>", order.GatewayOrderId));
                sb.Append(string.Format("<LogisticsName><![CDATA[{0}]]></LogisticsName>", order.ExpressCompanyName));
                sb.Append(string.Format("<Chargetype>{0}</Chargetype>", order.PaymentTypeName));

                sb.Append(string.Format("<CustomerRemark><![CDATA[{0}]]></CustomerRemark>", order.OrderRemarks));
                sb.Append(string.Format("<InvoiceTitle><![CDATA[{0}]]></InvoiceTitle>", ""));
                sb.Append(string.Format("<Remark><![CDATA[{0}]]></Remark>", order.SellerRemark));
                var orderitem = ServiceApplication.Create<IOrderService>().GetOrderItemsByOrderId(order.Id);
                foreach (var item in orderitem)
                {
                    sb.Append("<Item>");
                    sb.Append(string.Format("<GoodsID>{0}</GoodsID>", item.SKU));
                    sb.Append(string.Format("<GoodsName><![CDATA[{0}]]></GoodsName>", item.ProductName));
                    sb.Append(string.Format("<GoodsSpec><![CDATA[{0}]]></GoodsSpec>", item.Color + item.Size + item.Version));
                   
                    sb.Append(string.Format("<Count>{0}</Count>", item.Quantity));
                    sb.Append(string.Format("<Price>{0}</Price>", (decimal)item.RealTotalPrice / item.Quantity));//实付订单价格（有折扣价传折扣价）
                    sb.Append("<Tax>0</Tax>");
                    sb.Append("</Item>");
                }
            }
            sb.Append("</Order>");
            return sb.ToString();
        }
        /// <summary>
        /// 发货
        /// </summary>
        /// <param name="OrderNO">订单号</param>
        /// <param name="SndStyle">发货方式：圆通，顺丰等</param>
        /// <param name="BillID">发货单号</param>
        /// <returns></returns>
        public string SendOrder(HttpContext context)
        {
            string OrderNO = context.Request["OrderNO"];
            string SndStyle = context.Request["SndStyle"];
            string BillID = context.Request["BillID"];

            StringBuilder sb = new StringBuilder();
            string OrderId = OrderNO.Trim();

            if (OrderNO.IndexOf(',') > 0)
            {
                return ExMsg("不支持合并发货，请选择单个订单");
            }
            long orderId = Convert.ToInt64(OrderId);
            var order = ServiceApplication.Create<IOrderService>().GetOrder(orderId);

            if (order == null)
                return ExMsg("未找到此订单");
            if (order.OrderStatus != Entities.OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                return ExMsg("只有待发货状态的订单才能发货！");
            }
            //if (!CanSendGood(orderId))
            //{
            //    return "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Rsp><Result>0</Result><Cause><![CDATA[{拼团完成后订单才可以发货！}]]></Cause></Rsp>";
            //}
            if (string.IsNullOrEmpty(BillID.Trim()) || BillID.Trim().Length > 20)
            {
                return ExMsg("运单号码不能为空，在1至20个字符之间！");
            }
            try
            {
                var result = new Result();
                //var returnurl = String.Format("http://{0}/Common/ExpressData/SaveExpressData", Request.RequestUri.Authority);
                //商家发货
                //Application.OrderApplication.SellerSendGood(orderId, order.ShopName, SndStyle, BillID, returnurl);
            }
            catch (Exception ex)
            {
                return ExMsg(ex.Message);
            }
            return "";
        }

        /// <summary>
        /// 检查时间戳是否超过15分钟
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        private bool checkTimeStamp(string timeStamp)
        {
            long t = Convert.ToInt64(timeStamp);
            DateTime now = DateTime.UtcNow;
            DateTime unnow = UnixTimestampToDateTime(now, t);
            TimeSpan span = now - unnow;
            return span.TotalMinutes < 15;
        }

        /// <summary>
        /// unix时间戳转换成日期
        /// </summary>
        /// <param name="unixTimeStamp">时间戳（秒）</param>
        /// <returns></returns>
        private DateTime UnixTimestampToDateTime(DateTime target, long timestamp)
        {
            //本地时间戳有问题
            //var start = new DateTime(1970, 1, 1, 0, 0, 0, target.Kind);
            //return start.AddSeconds(timestamp);
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timestamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);

            return dateTimeStart.Add(toNow);
        }

        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="uCode"></param>
        /// <param name="mType"></param>
        /// <param name="Secret"></param>
        /// <param name="TimeStamp"></param>
        /// <returns></returns>
        private string sign(string uCode, string mType, string Secret, string TimeStamp)
        {
            string inStr = string.Format("{0}mType{1}TimeStamp{2}uCode{3}{0}", Secret, mType, TimeStamp, uCode);
            StringBuilder sb = new StringBuilder(32);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(Encoding.GetEncoding("utf-8").GetBytes(inStr));
            for (int i = 0; i < t.Length; i++)
            {
                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }
            return sb.ToString().ToUpper();
        }

        public string ExMsg(string msg)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.Append("<Rsp><Result>0</Result>");
            sb.AppendFormat("<Cause>{0}</Cause>", string.Format("<![CDATA[{0}]]>", msg));
            sb.Append("</Rsp>");
            return sb.ToString();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}