using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    public enum ExpressElementType:int
    {
        /// <summary>
        /// 收货人-姓名
        /// </summary>
        [Description("收货人-姓名")]
        ReceiverUser =1,
        /// <summary>
        /// 收货人-地址
        /// </summary>
        [Description("收货人-地址")]
        ReceiverAddress = 2,
        /// <summary>
        /// 收货人-电话
        /// </summary>
        [Description("收货人-电话")]
        ReceiverPhone = 3,
        /// <summary>
        /// 收货人-邮编
        /// </summary>
        [Description("收货人-邮编")]
        ReceiverPostCode = 4,
        /// <summary>
        /// 收货人-地区1级
        /// </summary>
        [Description("收货人-地区1级")]
        ReceiverAddressLevel1,
        /// <summary>
        /// 收货人-地区2级
        /// </summary>
        [Description("收货人-地区2级")]
        ReceiverAddressLevel2,
        /// <summary>
        /// 收货人-地区3级
        /// </summary>
        [Description("收货人-地区3级")]
        ReceiverAddressLevel3,
        /// <summary>
        /// 发货人-姓名
        /// </summary>
        [Description("发货人-姓名")]
        ShipToUser,
        /// <summary>
        /// 发货人-地区1级
        /// </summary>
        [Description("发货人-地区1级")]
        ShipToAddressLevel1,
        /// <summary>
        /// 发货人-地区2级
        /// </summary>
        [Description("发货人-地区2级")]
        ShipToAddressLevel2,
        /// <summary>
        /// 发货人-地区3级
        /// </summary>
        [Description("发货人-地区3级")]
        ShipToAddressLevel3,
        /// <summary>
        /// 发货人-地址
        /// </summary>
        [Description("发货人-地址")]
        ShipToAddress,
        /// <summary>
        /// 发货人-电话
        /// </summary>
        [Description("发货人-电话")]
        ShipToPhone,
        /// <summary>
        /// 发货人-邮编
        /// </summary>
        [Description("发货人-邮编")]
        ShipToPostCode,
        /// <summary>
        /// 订单号
        /// </summary>
        [Description("订单号")]
        OrderNo,
        /// <summary>
        /// 总金额
        /// </summary>
        [Description("总金额")]
        OrderAmount,
        /// <summary>
        /// 物品总重量
        /// </summary>
        [Description("物品总重量")]
        OrderWeight,
        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        OrderRemark,
        /// <summary>
        /// 详情
        /// </summary>
        [Description("详情")]
        OrderDetail,
        /// <summary>
        /// 送货时间
        /// </summary>
        [Description("送货时间")]
        OrderDeliveTime,
        /// <summary>
        /// 网店名称
        /// </summary>
        [Description("网店名称")]
        ShopName,
        /// <summary>
        /// 22,对号-√
        /// </summary>
        [Description("对号-√")]
        RightChar,
        /*
        _allPrintElements.Add(1, "收货人-姓名");
            _allPrintElements.Add(2, "收货人-地址");
            _allPrintElements.Add(3, "收货人-电话");
           // _allPrintElements.Add(4, "收货人-邮编");
            _allPrintElements.Add(5, "收货人-地区1级");
            _allPrintElements.Add(6, "收货人-地区2级");
            _allPrintElements.Add(7, "收货人-地区3级");
            _allPrintElements.Add(8, "发货人-姓名");
            _allPrintElements.Add(9, "发货人-地区1级");
            _allPrintElements.Add(10, "发货人-地区2级");
            _allPrintElements.Add(11, "发货人-地区3级");
            _allPrintElements.Add(12, "发货人-地址");
           // _allPrintElements.Add(13, "发货人-邮编");
            _allPrintElements.Add(14, "发货人-电话");
            _allPrintElements.Add(15, "订单-订单号");
            _allPrintElements.Add(16, "订单-总金额");
            _allPrintElements.Add(17, "订单-物品总重量");
          //  _allPrintElements.Add(18, "订单-备注");
          //  _allPrintElements.Add(19, "订单-详情");
            //_allPrintElements.Add(20, "订单-送货时间");
            _allPrintElements.Add(21, "网店名称");
            _allPrintElements.Add(22, "对号-√");
            */
    }
}
