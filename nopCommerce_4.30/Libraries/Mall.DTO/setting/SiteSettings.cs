using Mall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class SiteSettings
    {
        /// <summary>
        /// 站点名称
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// ICP编号
        /// </summary>
        public string ICPNubmer { get; set; }

        /// <summary>
        /// 客服联系电话
        /// </summary>

        public string CustomerTel { get; set; }

        /// <summary>
        /// 站点
        /// </summary>

        public bool SiteIsClose { get; set; }

        /// <summary>
        /// 头部广告图
        /// </summary>
        public string HeadArea { get; set; }
        /// <summary>
        /// 注册方式
        /// </summary>

        public int RegisterType { get; set; }
        /// <summary>
        /// 手机是否需验证
        /// </summary>

        public bool MobileVerifOpen { set; get; }
        /// <summary>
        /// 邮箱是否必填
        /// </summary>

        public bool RegisterEmailRequired { get; set; }
        /// <summary>
        /// 邮箱是否需要验证
        /// </summary>

        public bool EmailVerifOpen { set; get; }

        /// <summary>
        /// lOGO图片
        /// </summary>

        public string Logo { set; get; }
        /// <summary>
        /// 微信Logo
        /// <para>用于微信卡券，100*100，小于1M</para>
        /// </summary>

        public string WXLogo
        {
            get;
            set;
        }
        /// <summary>
        /// PC登录页左侧图片
        /// </summary>

        public string PCLoginPic
        {
            get;
            set;
        }
        /// <summary>
        /// PC底部服务图片
        /// </summary>

        public string PCBottomPic
        {
            get;
            set;
        }
        /// <summary>
        /// 搜索关键字
        /// </summary>

        public string Keyword { set; get; }
        /// <summary>
        /// 热门关键字
        /// </summary>

        public string Hotkeywords { set; get; }

        /// <summary>
        /// 首页页脚
        /// </summary>

        public string PageFoot { get; set; }

        /// <summary>
        /// 微信AppId
        /// </summary>

        public string WeixinAppId { get; set; }

        /// <summary>
        /// 微信AppSecret
        /// </summary>

        public string WeixinAppSecret { get; set; }

        /// <summary>
        /// 微信AppId
        /// </summary>

        public string WeixinAppletId { get; set; }

        /// <summary>
        /// 微信AppSecret
        /// </summary>

        public string WeixinAppletSecret { get; set; }

        ///// <summary>
        ///// 门店小程序AppId
        ///// </summary>

        //public string WeixinO2OAppletId { get; set; }

        ///// <summary>
        ///// 门店小程序AppSecret
        ///// </summary>

        //public string WeixinO2OAppletSecret { get; set; }

        /// <summary>
        /// 微信WeixinToken
        /// </summary>

        public string WeixinToken { get; set; }


        /// <summary>
        /// 微信合作伙伴ID
        /// </summary>

        public string WeixinPartnerID { get; set; }

        /// <summary>
        /// 微信合作伙伴key
        /// </summary>

        public string WeixinPartnerKey { get; set; }

        /// <summary>
        /// 微信URL配置
        /// </summary>

        public string WeixinLoginUrl { get; set; }

        /// <summary>
        /// 微信是否为服务号
        /// </summary>

        public bool WeixinIsValidationService { get; set; }

        /// <summary>
        /// 用户Cookie密钥
        /// </summary>

        public string UserCookieKey { get; set; }

        /// <summary>
        /// 商家入住协议
        /// </summary>

        public string SellerAdminAgreement { get; set; }

        /// <summary>
        /// 预付款百分比
        /// </summary>

        public decimal AdvancePaymentPercent { get; set; }

        /// <summary>
        /// 预付款上限
        /// </summary>

        public decimal AdvancePaymentLimit { get; set; }

        /// <summary>
        /// 结算周期
        /// </summary>

        public int WeekSettlement { get; set; }


        public string MemberLogo
        {
            get;
            set;
        }


        public string QRCode
        {
            get;
            set;
        }


        public string FlowScript
        {
            get;
            set;
        }


        public string Site_SEOTitle
        {
            get;
            set;
        }


        public string Site_SEOKeywords
        {
            get;
            set;
        }


        public string Site_SEODescription
        {
            get;
            set;
        }

        /// <summary>
        /// 未付款超时(小时)
        /// </summary>        

        public int UnpaidTimeout { get; set; }

        /// <summary>
        /// 确认收货超时(天数)
        /// </summary>        

        public int NoReceivingTimeout { get; set; }
        /// <summary>
        /// 关闭评价通道时限(天数)
        /// </summary>

        public int OrderCommentTimeout { get; set; }

        /// <summary>
        /// 确认收货后可退货周期(天数)
        /// </summary>

        public int SalesReturnTimeout { get; set; }
        /// <summary>
        /// 售后-商家自动确认售后时限(天数)
        /// <para>到期未审核，自动认为同意售后</para>
        /// </summary>

        public int AS_ShopConfirmTimeout { get; set; }
        /// <summary>
        /// 售后-用户发货限时(天数)
        /// <para>到期未发货自动关闭售后</para>
        /// </summary>

        public int AS_SendGoodsCloseTimeout { get; set; }
        /// <summary>
        /// 售后-商家确认到货时限(天数)
        /// <para>到期未收货，自动收货</para>
        /// </summary>

        public int AS_ShopNoReceivingTimeout { get; set; }


        /// <summary>
        /// 是否开启商品免审核上架
        /// </summary>

        public int ProdutAuditOnOff { get; set; }
        /// <summary>
        /// 是否开启商品显示销量
        /// </summary>

        public int ProductSaleCountOnOff { get; set; }

        /// <summary>
        /// 微信获取红包模板消息编号
        /// </summary>

        public string WX_MSGGetCouponTemplateId
        {
            get;
            set;
        }
        /// <summary>
        /// APP更新说明
        /// </summary>

        public string AppUpdateDescription { set; get; }

        /// <summary>
        /// app版本号
        /// </summary>

        public string AppVersion { set; get; }

        /// <summary>
        /// 安卓下载地址
        /// </summary>

        public string AndriodDownLoad { set; get; }



        /// <summary>
        /// IOS下载地址
        /// </summary>

        public string IOSDownLoad { set; get; }


        /// <summary>
        /// 是否提供下载
        /// </summary>

        public bool CanDownload { set; get; }


        /// <summary>
        /// 门店、商家app版本号
        /// </summary>

        public string ShopAppVersion { set; get; }

        /// <summary>
        /// 门店、商家安卓下载地址
        /// </summary>

        public string ShopAndriodDownLoad { set; get; }



        /// <summary>
        /// 门店、商家IOS下载地址
        /// </summary>

        public string ShopIOSDownLoad { set; get; }


        /// <summary>
        /// 0、快递100；1、快递鸟
        /// </summary>

        public int KuaidiType { get; set; }


        /// <summary>
        /// 快递100KEY
        /// </summary>


        public string Kuaidi100Key { set; get; }

        /// <summary>
        /// 快递鸟物流appkey
        /// </summary>

        public string KuaidiApp_key { get; set; }

        /// <summary>
        /// 快递鸟物流AppSecret
        /// </summary>

        public string KuaidiAppSecret { get; set; }

        /// <summary>
        /// 顺丰物流客户编码
        /// </summary>
        public string SFKuaidiCustomerCode { get; set; }

        /// <summary>
        /// 顺丰物流校验码
        /// </summary>
        public string SFKuaidiCheckWord { get; set; }

        /// <summary>
        /// 提现最低金额
        /// </summary>

        public int WithDrawMinimum { get; set; }

        /// <summary>
        /// 提现最高金额
        /// </summary>

        public int WithDrawMaximum { get; set; }

        /// <summary>
        /// 商家提现最低金额
        /// </summary>

        public int ShopWithDrawMinimum { get; set; }

        /// <summary>
        /// 商家提现最高金额
        /// </summary>

        public int ShopWithDrawMaximum { get; set; }

        /// <summary>
        /// 首页是否显示限时购
        /// </summary>

        public bool Limittime { get; set; }

        /// <summary>
        /// 广告图片地址
        /// </summary>

        public string AdvertisementImagePath { get; set; }

        /// <summary>
        /// 广告链接地址
        /// </summary>

        public string AdvertisementUrl { get; set; }

        /// <summary>
        /// 广告状态
        /// </summary>

        public bool AdvertisementState { get; set; }

        /// <summary>
        /// 是否开启订单自动匹配到门店
        /// </summary>

        public bool AutoAllotOrder { set; get; }

        /// <summary>
        /// 是否授权门店
        /// </summary>

        public bool IsOpenStore { get; set; }

        /// <summary>
        /// 是否授权商家APP
        /// </summary>

        public bool IsOpenShopApp { get; set; }

        /// <summary>
        /// 客服电话
        /// </summary>

        public string SitePhone { get; set; }

        /// <summary>
        /// 小程序版本号（编辑首页模版时每次改变）
        /// </summary>

        public long XcxHomeVersionCode { get; set; }

        /// <summary>
        /// 是否开启支付宝提现
        /// </summary>

        public bool Withdraw_AlipayEnable { set; get; }

        ///// <summary>
        ///// 多门店小程序是否使用顶部轮播图
        ///// </summary>

        //public bool O2OApplet_IsUseTopSlide { set; get; }
        ///// <summary>
        ///// 多门店小程序是否使用图标区
        ///// </summary>

        //public bool O2OApplet_IsUseIconArea { set; get; }
        ///// <summary>
        ///// 多门店小程序是否使用广告区
        ///// </summary>

        //public bool O2OApplet_IsUseAdArea { set; get; }
        ///// <summary>
        ///// 多门店小程序是否使用中间轮播图
        ///// </summary>

        //public bool O2OApplet_IsUseMiddleSlide { set; get; }
        ///// <summary>
        ///// 积分最高可扣抵比例
        ///// </summary>

        public int IntegralDeductibleRate { get; set; }

        /// <summary>
        /// 京东地址库APPKEY
        /// </summary>

        public string JDRegionAppKey { get; set; }
        /// <summary>
        /// 是否授权PC端
        /// </summary>

        public bool IsOpenPC { get; set; }
        /// <summary>
        /// 是否授权H5
        /// </summary>

        public bool IsOpenH5 { get; set; }
        /// <summary>
        /// 是否授权APP[IOS和安卓的商城APP]
        /// </summary>

        public bool IsOpenApp { get; set; }
        /// <summary>
        /// 是否授权商城小程序
        /// </summary>

        public bool IsOpenMallSmallProg { get; set; }
        ///// <summary>
        ///// 是否授权多门店小程序
        ///// </summary>

        //public bool IsOpenMultiStoreSmallProg { get; set; }
        /// <summary>
        /// 限时购活动是否需要审核
        /// </summary>

        public bool LimitTimeBuyNeedAuditing { get; set; }


        public bool IsOpenHistoryOrder { get; set; }

        /// <summary>
        /// 门店、商家app版本号
        /// </summary>
        public string MallJDVersion { set; get; }
        /// <summary>
        /// 是否开启充值赠送
        /// </summary>
        public bool IsOpenRechargePresent { set; get; }
        /// <summary>
        /// 是否开启提现
        /// </summary>

        public bool IsOpenWithdraw { set; get; }

        /// <summary>
        /// 是否强制绑定手机号
        /// </summary>

        public bool IsConBindCellPhone { get; set; }
        /// <summary>
        /// 是否可以清理演示数据
        /// </summary>

        public bool IsCanClearDemoData { get; set; }

        #region 分销
        /// <summary>
        /// 是否启用分销功能
        /// </summary>
        public bool DistributionIsEnable { get; set; }
        /// <summary>
        /// 分销最大级数
        /// <para>最大3级</para>
        /// </summary>
        public int DistributionMaxLevel { get; set; } = 1;
        /// <summary>
        /// 最高分佣比
        /// </summary>
        public decimal DistributionMaxBrokerageRate { get; set; }
        /// <summary>
        /// 非销售员-商品详情页分佣显示提示
        /// </summary>
        public bool DistributionIsProductShowTips { get; set; }
        /// <summary>
        /// 是否可以销售员自购
        /// </summary>
        public bool DistributionCanSelfBuy { get; set; }
        /// <summary>
        /// 销售员是否需要审核
        /// </summary>
        public bool DistributorNeedAudit { get; set; }
        /// <summary>
        /// 销售员申请条件
        /// <para>0表示无条件要求</para>
        /// </summary>
        public int DistributorApplyNeedQuota { get; set; } = 0;
        /// <summary>
        /// 我要开店页面内容
        /// </summary>
        public string DistributorPageContent { get; set; }
        /// <summary>
        /// 分销重命名-我要开店
        /// </summary>
        public string DistributorRenameOpenMyShop { get; set; } = "我要开店";
        /// <summary>
        /// 分销重命名-我的小店
        /// </summary>
        public string DistributorRenameMyShop { get; set; } = "我的小店";
        /// <summary>
        /// 分销重命名-推广小店
        /// </summary>
        public string DistributorRenameSpreadShop { get; set; } = "推广小店";
        /// <summary>
        /// 分销重命名-佣金
        /// </summary>
        public string DistributorRenameBrokerage { get; set; } = "佣金";
        /// <summary>
        /// 分销重命名-分销市场
        /// </summary>
        public string DistributorRenameMarket { get; set; } = "分销市场";
        /// <summary>
        /// 分销重命名-小店订单
        /// </summary>
        public string DistributorRenameShopOrder { get; set; } = "小店订单";
        /// <summary>
        /// 分销重命名-我的佣金
        /// </summary>
        public string DistributorRenameMyBrokerage { get; set; } = "我的佣金";
        /// <summary>
        /// 分销重命名-我的下级
        /// </summary>
        public string DistributorRenameMySubordinate { get; set; } = "我的下级";
        /// <summary>
        /// 分销重命名-一级会员
        /// </summary>
        public string DistributorRenameMemberLevel1 { get; set; } = "一级会员";
        /// <summary>
        /// 分销重命名-二级会员
        /// </summary>
        public string DistributorRenameMemberLevel2 { get; set; } = "二级会员";
        /// <summary>
        /// 分销重命名-三级会员
        /// </summary>
        public string DistributorRenameMemberLevel3 { get; set; } = "三级会员";
        /// <summary>
        /// 分销重命名-小店设置
        /// </summary>
        public string DistributorRenameShopConfig { get; set; } = "小店设置";

        /// <summary>
        /// 分销提现最小金额
        /// </summary>
        public decimal DistributorWithdrawMinLimit { get; set; } = 1;
        /// <summary>
        /// 分销提现最大金额
        /// </summary>
        public decimal DistributorWithdrawMaxLimit { get; set; } = 2000;
        /// <summary>
        /// 支持提现方式保存格式 capital,alipay,wechat
        /// </summary>
        public string DistributorWithdrawTypes { get; set; } = "capital";
        #endregion

        #region 首页可视化
        /// <summary>
        /// 首页可视化模板配置的搜索关键词
        /// </summary>
        public string SearchKeyword { get; set; }
        /// <summary>
        /// 热搜词
        /// </summary>
        public string HotKeyWords { get; set; }
        #endregion
    }
}
