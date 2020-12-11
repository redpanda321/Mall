using Mall.Core;
using Mall.IServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Mall.Application
{
    /// <summary>
    /// 达达物流数据获取
    /// <para>感谢：云商城 袁章秋提供</para>
    /// </summary>
    public class ExpressDaDaHelper
    {
        /// <summary>
        /// 获取城市信息
        /// </summary>
        /// <param name="source_id">商户编号</param>
        /// <returns>
        /// {"status":"success","result":[{"cityName":"上海","cityCode":"021"},{"cityName":"北京","cityCode":"010"},{"cityName":"合肥","cityCode":"0551"},{"cityName":"南京","cityCode":"025"},{"cityName":"苏州","cityCode":"0512"},{"cityName":"武汉","cityCode":"027"},{"cityName":"无锡","cityCode":"0510"},{"cityName":"常州","cityCode":"0519"},{"cityName":"杭州","cityCode":"0571"},{"cityName":"广州","cityCode":"020"},{"cityName":"深圳","cityCode":"0755"},{"cityName":"重庆","cityCode":"023"},{"cityName":"长沙","cityCode":"0731"},{"cityName":"成都","cityCode":"028"},{"cityName":"天津","cityCode":"022"},{"cityName":"厦门","cityCode":"0592"},{"cityName":"福州","cityCode":"0591"},{"cityName":"大连","cityCode":"0411"},{"cityName":"青岛","cityCode":"0532"},{"cityName":"哈尔滨","cityCode":"0451"},{"cityName":"济南","cityCode":"0531"},{"cityName":"郑州","cityCode":"0371"},{"cityName":"西安","cityCode":"029"},{"cityName":"宁波","cityCode":"0574"},{"cityName":"温州","cityCode":"0577"},{"cityName":"芜湖","cityCode":"0553"},{"cityName":"南通","cityCode":"0513"},{"cityName":"南昌","cityCode":"0791"},{"cityName":"石家庄","cityCode":"0311"},{"cityName":"潍坊","cityCode":"0536"},{"cityName":"嘉兴","cityCode":"0573"},{"cityName":"金华","cityCode":"0579"},{"cityName":"绍兴","cityCode":"0575"},{"cityName":"烟台","cityCode":"0535"},{"cityName":"扬州","cityCode":"0514"},{"cityName":"昆山","cityCode":"0512"},{"cityName":"佛山","cityCode":"0757"},{"cityName":"东莞","cityCode":"0769"},{"cityName":"马鞍山","cityCode":"0555"}],"code":0,"msg":"成功"}
        /// cityCode 城市编码
        /// cityName 城市名称
        /// </returns>
        public static string cityCodeList(long shopId)
        {
            string body = "";
            string url = "/api/cityCode/list";
            return DadaAPI(shopId, body, url);
        }

        /// <summary>
        /// 新增门店
        /// </summary>
        /// <param name="source_id">商户编号</param>
        /// <param name="station_name">门店名称</param>
        /// <param name="business">业务类型(食品小吃-1,饮料-2,鲜花-3,文印票务-8,便利店-9,水果生鲜-13,同城电商-19, 医药-20,蛋糕-21,酒品-24,小商品市场-25,服装-26,汽修零配-27,数码-28,小龙虾-29, 其他-5)</param>
        /// <param name="city_name">城市名称(如,上海)</param>
        /// <param name="area_name">区域名称(如,浦东新区)</param>
        /// <param name="station_address">门店地址</param>
        /// <param name="lng">门店经度</param>
        /// <param name="lat">门店纬度</param>
        /// <param name="contact_name">联系人姓名</param>
        /// <param name="phone">联系人电话</param>
        /// <param name="origin_shop_id">门店编码,可自定义,但必须唯一;若不填写,则系统自动生成</param>
        /// <param name="id_card">联系人身份证</param>
        /// <param name="username">达达商家app账号(必须手机号,若不需要登陆app,则不用设置)</param>
        /// <param name="password">达达商家app密码(若不需要登陆app,则不用设置)</param>
        /// <returns>http://newopen.imdada.cn/#/development/file/shopAdd?_k=8rg6ln</returns>
        public static string shopAdd(long shopId, string station_name, int business, string city_name, string area_name, string station_address, double lng, double lat, string contact_name, string phone, string origin_shop_id = "", string id_card = "", string username = "", string password = "")
        {
            SortedDictionary<string, object> tparams = new SortedDictionary<string, object>();
            tparams.Add("station_name", station_name);
            tparams.Add("business", business);
            tparams.Add("city_name", city_name);
            tparams.Add("area_name", area_name);
            tparams.Add("station_address", station_address);
            tparams.Add("lng", lng);
            tparams.Add("lat", lat);
            tparams.Add("contact_name", contact_name);
            tparams.Add("phone", phone);
            if (!string.IsNullOrEmpty(origin_shop_id))
            {
                tparams.Add("origin_shop_id", origin_shop_id);
            }
            if (!string.IsNullOrEmpty(id_card))
            {
                tparams.Add("id_card", id_card);
            }
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                tparams.Add("username", username);
                tparams.Add("password", password);
            }

            string body = "[" + JsonConvert.SerializeObject(tparams) + "]";
            string url = "/api/shop/add";
            return DadaAPI(shopId, body, url);
        }

        /// <summary>
        /// 编辑门店
        /// </summary>
        /// <param name="source_id">商户编号</param>
        /// <param name="origin_shop_id">门店编码</param>
        /// <param name="new_shop_id">新的门店编码</param>
        /// <param name="station_name">门店名称</param>
        /// <param name="business">业务类型(食品小吃-1,饮料-2,鲜花-3,文印票务-8,便利店-9,水果生鲜-13,同城电商-19, 医药-20,蛋糕-21,酒品-24,小商品市场-25,服装-26,汽修零配-27,数码-28,小龙虾-29, 其他-5)</param>
        /// <param name="city_name">城市名称(如,上海)</param>
        /// <param name="area_name">区域名称(如,浦东新区)</param>
        /// <param name="station_address">门店地址</param>
        /// <param name="lng">门店经度</param>
        /// <param name="lat">门店纬度</param>
        /// <param name="contact_name">联系人姓名</param>
        /// <param name="phone">联系人电话</param>
        /// <param name="status">门店状态（1-门店激活，0-门店下线）</param>
        /// <returns>{"status": "success","code": 0,"msg": "成功"}</returns>
        public static string shopUpdate(long shopId, string origin_shop_id, string station_name = "", int business = -1, string city_name = "", string area_name = "", string station_address = "", double lng = -1, double lat = -1, string contact_name = "", string phone = "", int status = -1, string new_shop_id = "")
        {
            SortedDictionary<string, object> tparams = new SortedDictionary<string, object>();
            tparams.Add("origin_shop_id", origin_shop_id);
            if (!string.IsNullOrEmpty(new_shop_id))
            {
                tparams.Add("new_shop_id", new_shop_id);
            }
            if (!string.IsNullOrEmpty(station_name))
            {
                tparams.Add("station_name", station_name);
            }
            if (business > 0)
            {
                tparams.Add("business", business);
            }
            if (!string.IsNullOrEmpty(city_name))
            {
                tparams.Add("city_name", city_name);
            }
            if (!string.IsNullOrEmpty(area_name))
            {
                tparams.Add("area_name", area_name);
            }
            if (!string.IsNullOrEmpty(station_address))
            {
                tparams.Add("station_address", station_address);
            }
            if (lng > 0)
            {
                tparams.Add("lng", lng);
            }
            if (lat > 0)
            {
                tparams.Add("lat", lat);
            }
            if (!string.IsNullOrEmpty(contact_name))
            {
                tparams.Add("contact_name", contact_name);
            }
            if (!string.IsNullOrEmpty(phone))
            {
                tparams.Add("phone", phone);
            }
            if (status >= 0)
            {
                tparams.Add("status", status);
            }


            string body = JsonConvert.SerializeObject(tparams);
            string url = "/api/shop/update";
            return DadaAPI(shopId, body, url);
        }

        /// <summary>
        /// 门店详情
        /// </summary>
        /// <param name="source_id">商户编号</param>
        /// <param name="origin_shop_id">门店编码</param>
        /// <returns>{"status": "success","result": {"station_name": "shop001","area_name": "闵行区","station_address": "明翰教育(上中西路)","city_name": "上海","contact_name": "shop001","origin_shop_id": "shop001","business": 9,"lng": 121.407631,"phone": "13810001001","id_card": "123456789987654321","lat": 31.129922,"status": 0},"code": 0,"msg": "成功"}</returns>
        public static string shopDetail(long shopId, string origin_shop_id)
        {
            SortedDictionary<string, object> tparams = new SortedDictionary<string, object>();
            tparams.Add("origin_shop_id", origin_shop_id);
            string body = JsonConvert.SerializeObject(tparams);
            string url = "/api/shop/detail";
            return DadaAPI(shopId, body, url);
        }

        /// <summary>
        /// 新增订单/重新发布订单/查询订单运费
        /// </summary>
        /// <param name="source_id">商户编号</param>
        /// <param name="shop_no">门店编号</param>
        /// <param name="origin_id">第三方订单编号</param>
        /// <param name="city_code">订单所在城市的cityCode</param>
        /// <param name="cargo_price">订单金额</param>
        /// <param name="is_prepay">是否需要垫付 1:是 0:否</param>
        /// <param name="expected_fetch_time">期望取货时间（1.时间戳,以秒计算时间，即unix-timestamp; 2.该字段的设定，不会影响达达正常取货; 3.订单被接单后,该时间往后推半小时后，配送员还未取货会自动被系统取消，;4.建议取值为当前时间往后推10~15分钟）</param>
        /// <param name="receiver_name">收货人姓名</param>
        /// <param name="receiver_address">收货人地址</param>
        /// <param name="receiver_lat">收货人地址维度（高德坐标系）</param>
        /// <param name="receiver_lng">收货人地址经度（高德坐标系）</param>
        /// <param name="callback">回调URL http://3.2kf.ysctest.kuaidiantong.cn/pay/dadaOrderNotify </param>
        /// <param name="receiver_phone">收货人手机号（手机号和座机号必填一项）</param>
        /// <param name="receiver_tel">收货人座机号（手机号和座机号必填一项）</param>
        /// <param name="tips">小费（单位：元，精确小数点后一位）</param>
        /// <param name="pay_for_supplier_fee">商家应付金额（单位：元）</param>
        /// <param name="fetch_from_receiver_fee">用户应收金额（单位：元）</param>
        /// <param name="deliver_fee">第三方平台补贴运费金额（单位：元）</param>
        /// <param name="create_time">订单创建时间（时间戳,以秒计算时间，即unix-timestamp）</param>
        /// <param name="info">订单备注</param>
        /// <param name="cargo_type">订单商品类型：1、餐饮 2、饮 料 3、鲜花 4、票 务 5、其他 8、印刷品 9、便利店 10、学校餐饮 11、校园便利 12、生鲜 13、水果</param>
        /// <param name="cargo_weight">订单重量（单位：Kg）</param>
        /// <param name="cargo_num">订单商品数量</param>
        /// <param name="expected_finish_time">期望完成时间（时间戳,以秒计算时间，即unix-timestamp）</param>
        /// <param name="invoice_title">发票抬头</param>
        /// <param name="deliver_locker_code">送货开箱码</param>
        /// <param name="pickup_locker_code">取货开箱码</param>
        /// <param name="isReAddOrder">是否重新发布订单 在调用新增订单后，订单被取消、过期或者投递异常的情况下，可以在达达平台重新发布订单。</param>
        /// <param name="isQueryDeliverFee">是否查询订单运费 </param>
        /// <returns>{"status":"success","result":{"distance":53459.0,"fee":51.0},"code":0,"msg":"成功"}
        /// distance 配送距离(单位：米)
        /// fee 运费(单位：元)
        /// deliveryNo 平台订单编号 查询订单运费返回 注意：该平台订单编号有效期为3分钟。
        /// </returns>
        public static string addOrder(long shopId, string shop_no, string origin_id, string city_code, double cargo_price, int is_prepay, long expected_fetch_time, string receiver_name, string receiver_address, double receiver_lat, double receiver_lng, string callback, string receiver_phone = "", string receiver_tel = "", double tips = -1, double pay_for_supplier_fee = -1, double fetch_from_receiver_fee = -1, double deliver_fee = -1, long create_time = -1, string info = "", int cargo_type = -1, double cargo_weight = -1, int cargo_num = -1, long expected_finish_time = -1, string invoice_title = "", string deliver_locker_code = "", string pickup_locker_code = "", bool isReAddOrder = false, bool isQueryDeliverFee = false)
        {
            SortedDictionary<string, object> tparams = new SortedDictionary<string, object>();
            tparams.Add("shop_no", shop_no);
            tparams.Add("origin_id", origin_id);
            tparams.Add("city_code", city_code);
            tparams.Add("cargo_price", cargo_price);
            tparams.Add("is_prepay", is_prepay);
            tparams.Add("expected_fetch_time", expected_fetch_time);
            tparams.Add("receiver_name", receiver_name);
            tparams.Add("receiver_address", receiver_address);
            tparams.Add("receiver_lat", receiver_lat);
            tparams.Add("receiver_lng", receiver_lng);
            tparams.Add("callback", callback);
            if (!string.IsNullOrEmpty(receiver_phone))
            {
                tparams.Add("receiver_phone", receiver_phone);
            }
            if (!string.IsNullOrEmpty(receiver_tel))
            {
                tparams.Add("receiver_tel", receiver_tel);
            }
            if (tips > 0)
            {
                tparams.Add("tips", tips);
            }
            if (pay_for_supplier_fee > 0)
            {
                tparams.Add("pay_for_supplier_fee", pay_for_supplier_fee);
            }
            if (fetch_from_receiver_fee > 0)
            {
                tparams.Add("fetch_from_receiver_fee", fetch_from_receiver_fee);
            }
            if (deliver_fee > 0)
            {
                tparams.Add("deliver_fee", deliver_fee);
            }
            if (create_time > 0)
            {
                tparams.Add("create_time", create_time);
            }
            if (!string.IsNullOrEmpty(info))
            {
                tparams.Add("info", info);
            }
            if (cargo_type > 0)
            {
                tparams.Add("cargo_type", cargo_type);
            }
            if (cargo_weight > 0)
            {
                tparams.Add("cargo_weight", cargo_weight);
            }
            if (cargo_num > 0)
            {
                tparams.Add("cargo_num", cargo_num);
            }
            if (expected_finish_time > 0)
            {
                tparams.Add("expected_finish_time", expected_finish_time);
            }
            if (!string.IsNullOrEmpty(invoice_title))
            {
                tparams.Add("invoice_title", invoice_title);
            }
            if (!string.IsNullOrEmpty(deliver_locker_code))
            {
                tparams.Add("deliver_locker_code", deliver_locker_code);
            }
            if (!string.IsNullOrEmpty(pickup_locker_code))
            {
                tparams.Add("pickup_locker_code", pickup_locker_code);
            }

            string body = JsonConvert.SerializeObject(tparams);
            string url = "/api/order/addOrder";
            if (isReAddOrder)
            {
                url = "/api/order/reAddOrder";
            }
            else if (isQueryDeliverFee)
            {
                url = "/api/order/queryDeliverFee";
            }
            return DadaAPI(shopId, body, url);
        }

        /// <summary>
        /// 订单预发布 查询运费后发单接口
        /// </summary>
        /// <param name="source_id">商户编号</param>
        /// <param name="deliveryNo">平台订单编号</param>
        /// <returns>{"status": "success","code": 0,"msg": "成功"}</returns>
        public static string addAfterQuery(long shopId, string source_id, string deliveryNo)
        {
            SortedDictionary<string, object> tparams = new SortedDictionary<string, object>();
            tparams.Add("deliveryNo", deliveryNo);
            string body = JsonConvert.SerializeObject(tparams);
            string url = "/api/order/addAfterQuery";
            return DadaAPI(shopId, body, url, source_id);
        }

        /// <summary>
        /// 订单增加小费
        /// </summary>
        /// <param name="source_id">商户编号</param>
        /// <param name="order_id">第三方订单编号</param>
        /// <param name="tips">小费金额(精确到小数点后一位，单位：元) 订单的小费，以最新一次加小费动作的金额为准，故下一次增加小费额必须大于上一次小费</param>
        /// <param name="city_code">订单城市区号</param>
        /// <param name="info">备注(字段最大长度：512)</param>
        /// <returns>{"status": "success","code": 0,"msg": "成功"}</returns>
        public static string addTip(long shopId, string order_id, float tips, string city_code, string info = "")
        {
            SortedDictionary<string, object> tparams = new SortedDictionary<string, object>();
            tparams.Add("order_id", order_id);
            tparams.Add("tips", tips);
            tparams.Add("city_code", city_code);
            if (!string.IsNullOrEmpty(info))
            {
                tparams.Add("info", info);
            }
            string body = JsonConvert.SerializeObject(tparams);
            string url = "/api/order/addTip";
            return DadaAPI(shopId, body, url);
        }

        /// <summary>
        /// 订单状态查询
        /// </summary>
        /// <param name="source_id">商户编号</param>
        /// <param name="order_id">第三方订单编号</param>
        /// <returns>result业务参数： 
        ///orderId	String	第三方订单编号
        ///statusCode	Integer	状态编码
        ///statusMsg	String	订单状态
        ///transporterName	String	骑手姓名 只有当订单被接单后才会有骑手信息，并且在待取货和配送中可以查询骑手实时的坐标信息。
        ///transporterPhone	String	骑手电话
        ///transporterLng	String	骑手经度
        ///transporterLat	String	骑手纬度
        ///deliveryFee	Double	配送费,单位为元
        ///tips	Double	小费,单位为元
        ///distance	Integer	配送距离,单位为米
        ///createTime	String	发单时间
        ///acceptTime	String	接单时间,若未接单,则为空
        ///fetchTime	String	取货时间,若未取货,则为空
        ///finishTime	String	送达时间,若未送达,则为空
        ///cancelTime	String	取消时间,若未取消,则为空
        /// </returns>
        public static string orderStatusQuery(long shopId, string order_id)
        {
            SortedDictionary<string, object> tparams = new SortedDictionary<string, object>();
            tparams.Add("order_id", order_id);
            string body = JsonConvert.SerializeObject(tparams);
            string url = "/api/order/status/query";
            return DadaAPI(shopId, body, url);
        }

        /// <summary>
        /// 获取订单取消原因
        /// </summary>
        /// <param name="source_id">商户编号</param>
        /// <returns>result业务参数：id 理由编号 reason 取消理由</returns>
        public static string orderCancelReasons(long shopId)
        {
            string body = "";
            string url = "/api/order/cancel/reasons";
            return DadaAPI(shopId, body, url);
        }

        /// <summary>
        /// 取消订单(线上环境) 
        /// </summary>
        /// 在订单待接单或待取货情况下，调用此接口可取消订单。注意：接单后1－15分钟内取消订单，运费退回。同时扣除2元作为给配送员的违约金
        /// <param name="source_id">商户编号</param>
        /// <param name="order_id">第三方订单编号</param>
        /// <param name="cancel_reason_id">取消原因ID</param>
        /// <param name="cancel_reason">取消原因(当取消原因ID为其他时，此字段必填)</param>
        /// <returns>result业务参数：deduct_fee	Double	扣除的违约金(单位：元)</returns>
        public static string orderFormalCancel(long shopId, string order_id, int cancel_reason_id, string cancel_reason = "")
        {
            SortedDictionary<string, object> tparams = new SortedDictionary<string, object>();
            tparams.Add("order_id", order_id);
            tparams.Add("cancel_reason_id", cancel_reason_id);
            if (!string.IsNullOrEmpty(cancel_reason))
            {
                tparams.Add("cancel_reason", cancel_reason);
            }
            string body = JsonConvert.SerializeObject(tparams);
            string url = "/api/order/formalCancel";
            return DadaAPI(shopId, body, url);
        }

        /// <summary>
        /// 获取商家投诉达达原因
        /// </summary>
        /// <param name="source_id">商户编号</param>
        /// <returns>result业务参数：id 原因编号 reason 投诉原因</returns>
        public static string complaintReasons(long shopId)
        {
            string body = "";
            string url = "/api/complaint/reasons";
            return DadaAPI(shopId, body, url);
        }

        /// <summary>
        /// 商家投诉达达
        /// </summary>
        /// <param name="source_id">商户编号</param>
        /// <param name="order_id">第三方订单编号</param>
        /// <param name="reason_id">投诉原因ID</param>
        /// <returns>{"status": "success","code": 0,"msg": "成功"}</returns>
        public static string complaintDada(long shopId, string order_id, int reason_id)
        {
            var dadaconfig = CityExpressConfigApplication.GetDaDaCityExpressConfig(shopId);
            string appkey = dadaconfig.app_key;
            string appsecret = dadaconfig.app_secret;
            string source_id = dadaconfig.source_id;
            SortedDictionary<string, object> tparams = new SortedDictionary<string, object>();
            tparams.Add("source_id", source_id);
            tparams.Add("order_id", order_id);
            tparams.Add("reason_id", reason_id);
            string body = JsonConvert.SerializeObject(tparams);
            string url = "/api/complaint/dada";
            return DadaAPI(shopId, body, url);
        }

        private static string DadaAPI(long shopId, string body, string url, string source_id = "")
        {
            var dadaconfig = CityExpressConfigApplication.GetDaDaCityExpressConfig(shopId);
            string appkey = dadaconfig.app_key;
            string appsecret = dadaconfig.app_secret;
            source_id = string.IsNullOrWhiteSpace(source_id) ? dadaconfig.source_id : source_id;
            if (!url.Contains("imdada.cn") && source_id == "73753")
            {
                url = "http://newopen.qa.imdada.cn" + url;
            }
            if (!url.Contains("imdada.cn"))
            {
                url = "http://newopen.imdada.cn" + url;
            }
            SortedDictionary<string, string> tparams = new SortedDictionary<string, string>();
            tparams.Add("app_key", appkey);
            tparams.Add("body", body);
            tparams.Add("format", "json");
            tparams.Add("source_id", source_id);
            tparams.Add("timestamp", DateTimeToUnixTimestamp(DateTime.Now).ToString());
            tparams.Add("v", "1.0");

            string sign = Sign(appsecret, tparams);
            tparams.Add("signature", sign);
            string strJsonParams = JsonConvert.SerializeObject(tparams);

            string result = GetResponseResult(url, strJsonParams);
            return result;
        }

        /// <summary>
        /// 日期转换成unix时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return Convert.ToInt64((dateTime - startTime).TotalSeconds);
        }
        /// <summary>
        /// 转换成字典
        /// </summary>
        /// <param name="j"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(JObject j)
        {
            var result = j.ToObject<Dictionary<string, object>>();

            var JObjectKeys = (from r in result
                               let key = r.Key
                               let value = r.Value
                               where value.GetType() == typeof(JObject)
                               select key).ToList();

            var JArrayKeys = (from r in result
                              let key = r.Key
                              let value = r.Value
                              where value.GetType() == typeof(JArray)
                              select key).ToList();

            JArrayKeys.ForEach(key => result[key] = ((JArray)result[key]).Values().Select(x => ((JValue)x).Value).ToArray());
            JObjectKeys.ForEach(key => result[key] = ToDictionary(result[key] as JObject));

            return result;
        }
        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="appsecret"></param>
        /// <param name="tParams"></param>
        /// <returns></returns>
        private static string Sign(string appsecret, SortedDictionary<string, string> tParams)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(appsecret);
            foreach (KeyValuePair<string, string> temp in tParams)
            {
                sb.Append(string.Format("{0}{1}", temp.Key, temp.Value));
            }
            sb.Append(appsecret);

            string inStr = sb.ToString();
            sb = new StringBuilder(32);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(Encoding.GetEncoding("utf-8").GetBytes(inStr));
            for (int i = 0; i < t.Length; i++)
            {
                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }

            return sb.ToString().ToUpper();
        }



        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; }
        private static string GetResponseResult(string url, string param)
        {
            string result = string.Empty;

            string strURL = url;
            try
            {
                System.Net.HttpWebRequest request;
                request = (HttpWebRequest)WebRequest.Create(strURL);
                request.Method = "POST";
                request.ContentType = "application/json;charset=UTF-8";
                string paraUrlCoded = param;
                byte[] payload;
                payload = System.Text.Encoding.UTF8.GetBytes(paraUrlCoded);
                request.ContentLength = payload.Length;
                Stream writer = request.GetRequestStream();
                writer.Write(payload, 0, payload.Length);
                writer.Close();
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream receiveStream = response.GetResponseStream())
                    {
                        using (StreamReader readerOfStream = new StreamReader(receiveStream, System.Text.Encoding.UTF8))
                        {
                            result = readerOfStream.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("GetResponseResult:" + url + "^" + param, ex);
            }
            return result;
        }
    }
}
