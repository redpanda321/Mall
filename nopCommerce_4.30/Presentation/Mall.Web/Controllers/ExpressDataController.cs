using Mall.Application;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;


namespace Mall.Web.Controllers
{
    public class ExpressDataController : BaseController
    {
        private const double EARTH_RADIUS = 6378137.0;//地球赤道半径(单位：m。6378137m是1980年的标准，比1975年的标准6378140少3m）
        private IExpressService _iExpressService;
        public ExpressDataController(IExpressService iExpressService)
        {
            _iExpressService = iExpressService;
        }
        // GET: ExpressData
        public JsonResult Search(string expressCompanyName, string shipOrderNumber)
        {
            #region 物流提供方显示TDO:ZYF
            var expressName = "";
            var expressUrl = "";
            var settting = SiteSettingApplication.SiteSettings;
            if (settting.KuaidiType.Equals(0))
            {
                expressName = "快递100";
                expressUrl = "https://www.kuaidi100.com";
            }
            else
            {
                expressName = "快递鸟";
                expressUrl = "http://www.kdniao.com/";
            }
            #endregion
            var expressData = _iExpressService.GetExpressData(expressCompanyName, shipOrderNumber);

            if (expressData != null && expressData.ExpressDataItems.Count() > 0)
            {
                if (expressData.Success)
                    expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列
                var json = new
                {
                    success = expressData.Success,
                    msg = expressData.Message,
                    data = expressData.ExpressDataItems.Select(item => new
                    {
                        time = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                        content = item.Content
                    }),
                    expressName = expressName,
                    expressUrl = expressUrl
                };
                return Json(json);
            }
            else
            {
                var json = new
                {
                    success = false,
                    msg = "无物流信息",
                    expressName = expressName,
                    expressUrl = expressUrl
                };
                return Json(json);
            }
        }
        /// <summary>
        /// 获取达达订单信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public JsonResult searchDada(long orderId)
        {
            var order = OrderApplication.GetOrder(orderId);
            string json = ExpressDaDaHelper.orderStatusQuery(order.ShopId, orderId.ToString());
            var resultObj = JsonConvert.DeserializeObject(json) as JObject;
            var rdic = ExpressDaDaHelper.ToDictionary(resultObj);
            return Json(rdic);
        }

        public JsonResult GetDistance(string fromLatLng, string endLatLng)
        {
            if (string.IsNullOrWhiteSpace(fromLatLng) || string.IsNullOrWhiteSpace(endLatLng))
            {
                return Json(new { result = 0 });
            }
            var aryLatlng = fromLatLng.Split(',');
            var aryToLatlng = endLatLng.Split(',');

            if (aryLatlng.Length < 2 || aryToLatlng.Length < 2)
            {
                return Json(new { result = 0 });
            }

            var fromlat = double.Parse(aryLatlng[0]);
            var fromlng = double.Parse(aryLatlng[1]);
            var tolat = double.Parse(aryToLatlng[0]);
            var tolng = double.Parse(aryToLatlng[1]);
            var fromRadLat = fromlat * Math.PI / 180.0;
            var toRadLat = tolat * Math.PI / 180.0;
            double a = fromRadLat - toRadLat;
            double b = (fromlng * Math.PI / 180.0) - (tolng * Math.PI / 180.0);
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
                Math.Cos(fromRadLat) * Math.Cos(toRadLat) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            s = (Math.Round(s * 10000) / 10000);
            return Json(new { result = s });

        }
        // GET: ExpressData
        public JsonResult SearchTop(string expressCompanyName, string shipOrderNumber)
        {
            var expressData = _iExpressService.GetExpressData(expressCompanyName, shipOrderNumber);

            if (expressData.Success)
                expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列

            var json = new
            {
                success = expressData.Success,
                msg = expressData.Message,
                data = expressData.ExpressDataItems.Take(1).Select(item => new
                {
                    time = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    content = item.Content
                })
            };

            return Json(json);
        }

        [HttpPost]
        public JsonResult SaveExpressData(string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                return Json(new { result = false, returnCode = 500, message = "服务器错误" });
            }
            try
            {
                var ReturnModel = new
                {
                    status = string.Empty,
                    message = string.Empty,
                    lastResult = new
                    {
                        message = string.Empty,
                        state = string.Empty,
                        status = string.Empty,
                        ischeck = string.Empty,
                        com = string.Empty,
                        nu = string.Empty
                    }
                };
                var obj = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(param, ReturnModel);
                Entities.OrderExpressDataInfo model = new Entities.OrderExpressDataInfo();
                model.DataContent = param;
                model.CompanyCode = obj.lastResult.com;
                model.ExpressNumber = obj.lastResult.nu;
                _iExpressService.SaveExpressData(model);
                return Json(new { result = true, returnCode = 200, message = "成功" });
            }
            catch (Exception ex)
            {
                Core.Log.Error("保存快递信息错误：" + ex.Message + param);
                return Json(new { result = false, returnCode = 500, message = "服务器错误" + ex.Message });
            }
        }
    }
}