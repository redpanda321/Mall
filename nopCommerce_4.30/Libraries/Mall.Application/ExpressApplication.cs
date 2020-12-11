using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Linq;
using Mall.Entities;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    /// <summary>
    /// 物流快递相关功能
    /// </summary>
    public class ExpressApplication
    {
       // private static IExpressService _iExpressService =  EngineContext.Current.Resolve<IExpressService>();


        private static IExpressService _iExpressService =  EngineContext.Current.Resolve<IExpressService>();

        /// <summary>
        /// 取所有快递公司，不包括面单元素
        /// </summary>
        /// <returns></returns>
        public static List<ExpressCompany> GetAllExpress()
        {
            var expressInfos = _iExpressService.GetAllExpress();
            //var result= AutoMapper.Mapper.Map<List<ExpressCompany>>(expressInfos);
            var result = expressInfos.Map<List<ExpressCompany>>();

            return result;
        }
        /// <summary>
        /// 快递公司
        /// </summary>
        /// <param name="expressname"></param>
        /// <returns></returns>
        public static bool IsExist(string expressname)
        {
            var express = _iExpressService.GetExpress(expressname);
            return express == null ? false : true;
        }
        /// <summary>
        /// 添加快递公司
        /// </summary>
        /// <param name="express"></param>
        public static void AddExpress(ExpressCompany express)
        {
            if (string.IsNullOrWhiteSpace(express.Name))
            {
                throw new MallException("快递公司名称不能为空");
            }
            express.Name = express.Name.TrimEnd(' ').TrimStart(' ');
            express.Kuaidi100Code = string.IsNullOrWhiteSpace(express.Kuaidi100Code) ? "" : express.Kuaidi100Code.TrimEnd(' ').TrimStart(' ');
            express.KuaidiNiaoCode = string.IsNullOrWhiteSpace(express.KuaidiNiaoCode) ? "" : express.KuaidiNiaoCode.TrimEnd(' ').TrimStart(' ');
            //  var expressInfo = AutoMapper.Mapper.Map<ExpressInfoInfo>(express);
            var expressInfo = express.Map<ExpressInfoInfo>();
            _iExpressService.AddExpress(expressInfo);
        }
        /// <summary>
        /// 删除快递公司（数据不能恢复，背景图片没有删除）
        /// </summary>
        /// <param name="id"></param>
        public static void DeleteExpress(long id)
        {
            _iExpressService.DeleteExpress(id);
        }
        /// <summary>
        /// 清除面单的背景图片
        /// </summary>
        /// <param name="id"></param>
        public static void ClearData(long id)
        {
             _iExpressService.ClearExpressData(id);
        }
        /// <summary>
        /// 更新快递公司名称、编号
        /// </summary>
        /// <param name="express"></param>
        public static void UpdateExpressCode(ExpressCompany express)
        {
            //验证公司名称
            if (string.IsNullOrWhiteSpace(express.Name))
            {
                throw new MallException("快递公司名称，不能为空");
            }
            var oldExpress = _iExpressService.GetExpress(express.Name);
            if (oldExpress != null && oldExpress.Id != express.Id)
            {
                throw new MallException("快递公司名称已存在！");
            }
            if ((string.IsNullOrWhiteSpace(express.Kuaidi100Code) && string.IsNullOrWhiteSpace(express.KuaidiNiaoCode) && string.IsNullOrWhiteSpace(express.TaobaoCode)))
            {//不能都为空
                throw new MallException("快递公司Code，不能为空");
            }
            //var expressInfo = AutoMapper.Mapper.Map<ExpressInfoInfo>(express);
            var expressInfo = express.Map<ExpressInfoInfo>();
            _iExpressService.UpdateExpressCode(expressInfo);
        }
        /// <summary>
        /// 更新快递面单信息（面单大小、面单图片地址，面单元素）
        /// </summary>
        /// <param name="express"></param>
        public static void UpdateExpressAndElement(ExpressCompany express) {
            if (string.IsNullOrWhiteSpace(express.Name))
            {
                throw new MallException("快递公司名称不能为空！");
            }
            //var expressinfo = AutoMapper.Mapper.Map<ExpressInfoInfo>(express);
            var expressinfo = express.Map<ExpressInfoInfo>();

            if (!string.IsNullOrWhiteSpace(expressinfo.BackGroundImage) && expressinfo.BackGroundImage.ToLower().Contains("/temp"))
            {
                string imageName = expressinfo.Name + ".png";
                string destFileName = CommonConst.ExpressImagePath + imageName;
                string filename = expressinfo.BackGroundImage.Substring(expressinfo.BackGroundImage.ToLower().LastIndexOf("/temp"));
                Core.MallIO.CopyFile(filename, destFileName, true);
                expressinfo.BackGroundImage = destFileName;
            }
            var elements = express.Elements.Select(e => new ExpressElementInfo
            {
                ElementType = e.ElementType,
                ExpressId = e.ExpressId,
                LeftTopPointX = e.a.Length > 0 ? e.a[0] : 0,
                LeftTopPointY = e.a.Length > 1 ? e.a[1] : 0,
                RightBottomPointX = e.b.Length > 0 ? e.b[0] : 0,
                RightBottomPointY = e.a.Length > 1 ? e.b[1] : 0
            });
            _iExpressService.UpdateExpressAndElement(expressinfo, elements.ToArray());
        }
        /// <summary>
        /// 取快递公司信息及面单元素
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ExpressCompany GetExpress(string name)
        {
            var expressInfo = _iExpressService.GetExpress(name);
            if (expressInfo != null)
            {
                var elements = _iExpressService.GetExpressElements(expressInfo.Id);
                // var express = AutoMapper.Mapper.Map<ExpressCompany>(expressInfo);
                var express = expressInfo.Map<ExpressCompany>();
                express.Elements = elements.Select(e => new ExpressElement
                {
                    a = new int[2] { e.LeftTopPointX, e.LeftTopPointY },
                    b = new int[2] { e.RightBottomPointX, e.RightBottomPointY },
                    ElementType = e.ElementType,
                    ExpressId = e.ExpressId,
                    name = (int)e.ElementType
                }).ToList();
                return express;
            }
            return null;
        }
        /// <summary>
        /// 取打印元素值
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="orderId"></param>
        /// <param name="printElementIndexes"></param>
        /// <returns></returns>
        public static IDictionary<int, string> GetPrintElementIndexAndOrderValue(long shopId, long orderId, IEnumerable<ExpressElementType> printElementIndexes)
        {
            var order = OrderApplication.GetOrder(orderId);
            if (order == null)
                throw new NullReferenceException("订单为空");

            var shopShipper = ShopShippersApplication.GetDefaultSendGoodsShipper(shopId);
            var dic = new Dictionary<int, string>();

            string[] regionNames = order.RegionFullName.Split(' ');
            string[] sellerRegionNames = new string[] { };

            if (shopShipper == null)
                throw new NullReferenceException("商家需先设置“发/退货地址”");

            string sellerRegionFullName = RegionApplication.GetFullName(shopShipper.RegionId);
            sellerRegionNames = sellerRegionFullName.Split(' ');

            string value;
            foreach (var index in printElementIndexes.ToList())
            {
                value = string.Empty;
                #region 获取对应值
                switch (index)
                {
                    case  ExpressElementType.ReceiverUser://"收货人-姓名"
                        value = order.ShipTo;
                        break;
                    case  ExpressElementType.ReceiverAddress://"收货人-地址"
                        value = order.Address;
                        break;
                    case  ExpressElementType.ReceiverPhone://"收货人-电话"
                        value = order.CellPhone;
                        break;
                    case  ExpressElementType.ReceiverPostCode://"收货人-邮编"
                        value = "";
                        break;
                    case  ExpressElementType.ReceiverAddressLevel1://"收货人-地区1级"
                        value = regionNames[0];
                        break;
                    case  ExpressElementType.ReceiverAddressLevel2://"收货人-地区2级"
                        value = regionNames.Length > 1 ? regionNames[1] : "";
                        break;
                    case  ExpressElementType.ReceiverAddressLevel3://"收货人-地区3级"
                        value = regionNames.Length > 2 ? regionNames[2] : "";
                        break;
                    case  ExpressElementType.ShipToUser://"发货人-姓名"
                        value = shopShipper.ShipperName;
                        break;
                    case  ExpressElementType.ShipToAddressLevel1://"发货人-地区1级"
                        value = sellerRegionNames.Length > 0 ? sellerRegionNames[0] : "";
                        break;
                    case  ExpressElementType.ShipToAddressLevel2:// "发货人-地区2级"
                        value = sellerRegionNames.Length > 1 ? sellerRegionNames[1] : "";
                        break;
                    case  ExpressElementType.ShipToAddressLevel3://"发货人-地区3级"
                        value = sellerRegionNames.Length > 2 ? sellerRegionNames[2] : "";
                        break;
                    case  ExpressElementType.ShipToAddress://"发货人-地址"
                        value = shopShipper.Address;
                        break;
                    case  ExpressElementType.ShipToPostCode://"发货人-邮编"
                        value = "";
                        break;
                    case  ExpressElementType.ShipToPhone://"发货人-电话"
                        value = shopShipper.TelPhone;
                        break;
                    case ExpressElementType.OrderNo://"订单-订单号"
                        value = order.Id.ToString();
                        break;
                    case ExpressElementType.OrderAmount://"订单-总金额"
                        value = order.OrderTotalAmount.ToString("F2");
                        break;
                    case  ExpressElementType.OrderWeight://"订单-物品总重量"
                        value = string.Empty;
                        break;
                    case  ExpressElementType.OrderRemark://"订单-备注"
                        value = string.IsNullOrWhiteSpace(order.UserRemark) ? "" : order.UserRemark.ToString();
                        break;
                    case ExpressElementType.OrderDetail://"订单-详情"
                        value = string.Empty;
                        break;
                    case ExpressElementType.ShopName://"网店名称"
                        value = order.ShopName;
                        break;
                    case ExpressElementType.RightChar://"对号-√"
                        value = "√";
                        break;
                    default:
                        value = string.Empty;
                        break;
                }
                #endregion
                dic.Add((int)index, value);
            }
            return dic;
        }
        /// <summary>
        /// 取最近使用的快递公司，没有则从平台快递公司列表补充
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="takeNumber"></param>
        /// <returns></returns>
        public static IEnumerable<ExpressCompany> GetRecentExpress(long shopId, int takeNumber)
        {
            var expressInfo = _iExpressService.GetRecentExpress(shopId, takeNumber);
            // var recentExpress = AutoMapper.Mapper.Map<List<ExpressCompany>>(expressInfo);
            var recentExpress = expressInfo.Map<List<ExpressCompany>>();
            return recentExpress;
        }
        public static DTO.ExpressData GetExpressData(string expressCompanyName, string shipOrderNumber)
        {
            return _iExpressService.GetExpressData(expressCompanyName, shipOrderNumber);
        }
        /// <summary>
        /// 修改快递公司状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        public static void ChangeExpressStatus(long id, ExpressStatus status)
        {
            _iExpressService.ChangeExpressStatus(id, status);
        }
        /// <summary>
        /// 快递公司单号
        /// </summary>
        /// <param name="expressName"></param>
        /// <param name="currentExpressCode"></param>
        /// <returns></returns>
        public static string GetNextExpressCode(string expressName, string currentExpressCode)
        {
            Entities.ExpressInfoInfo express = new Entities.ExpressInfoInfo();
            return express.NextExpressCode(expressName, currentExpressCode);
        }
    }
}
