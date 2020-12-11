using AutoMapper;
using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Areas.Web.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


namespace Mall.Web.Areas.Web.Controllers
{
    public class GiftController : BaseWebController
    {
        private IGiftService _iGiftService;
        private IGiftsOrderService _iGiftsOrderService;
        private IMemberService _iMemberService;
        private IMemberGradeService _iMemberGradeService;
        public GiftController(IGiftService iGiftService, IGiftsOrderService iGiftsOrderService, IMemberService iMemberService, IMemberGradeService iMemberGradeService)
        {
            _iGiftService = iGiftService;
            _iGiftsOrderService = iGiftsOrderService;
            _iMemberService = iMemberService;
            _iMemberGradeService = iMemberGradeService;
        }

        /// <summary>
        /// 礼品详情
        /// </summary>
        /// <returns></returns>
        public ActionResult Detail(long id)
        {
            GiftDetailPageModel result = new Models.GiftDetailPageModel();
            var data = _iGiftService.GetById(id);
            if (data == null)
            {
                throw new MallException("礼品信息无效！");
            }
            //Mapper.CreateMap<GiftInfo, GiftModel>();
            result.GiftData = data.Map<GiftModel>();
            
            int hotnum = 10;
            GiftQuery query = new GiftQuery();
            query.skey = "";
            query.status = GiftInfo.GiftSalesStatus.Normal;
            query.PageSize = hotnum;
            query.PageNo = 1;
            query.Sort = "SalesNumber";
            query.IsAsc = false;
            QueryPageModel<GiftModel> hotgifts = _iGiftService.GetGifts(query);
            result.HotGifts = hotgifts.Models.ToList();

            #region 礼品是否可兑
            result.GiftCanBuy = true;
            //礼品信息
            if (result.GiftCanBuy)
            {
                if (result.GiftData.GetSalesStatus != GiftInfo.GiftSalesStatus.Normal)
                {
                    result.GiftCanBuy = false;
                    result.CanNotBuyDes = "礼品" + result.GiftData.ShowSalesStatus;
                    if (result.GiftData.GetSalesStatus == GiftInfo.GiftSalesStatus.HasExpired)
                        result.CanNotBuyDes = "活动已结束";//统一app名称而加
                }
            }

            if (result.GiftCanBuy)
            {
                //库存判断
                if (result.GiftData.StockQuantity < 1)
                {
                    result.GiftCanBuy = false;
                    result.CanNotBuyDes = "已兑完";
                }
            }

            if (result.GiftCanBuy)
            {
                //积分数
                if (result.GiftData.NeedIntegral < 1)
                {
                    result.GiftCanBuy = false;
                    result.CanNotBuyDes = "礼品信息错误";
                }
            }
            #endregion

            #region 用户信息判断

            if (result.GiftCanBuy && CurrentUser != null)
            {
                //限购数量
                if (result.GiftData.LimtQuantity > 0)
                {
                    int ownbuynumber = _iGiftsOrderService.GetOwnBuyQuantity(CurrentUser.Id, id);
                    if (ownbuynumber >= result.GiftData.LimtQuantity)
                    {
                        result.GiftCanBuy = false;
                        result.CanNotBuyDes = "限兑数量已满";
                    }
                }
                if (result.GiftCanBuy)
                {
                    var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
                    if (userInte.AvailableIntegrals < result.GiftData.NeedIntegral)
                    {
                        result.GiftCanBuy = false;
                        result.CanNotBuyDes = "积分不足";
                    }
                }
            }
            #endregion

            ViewBag.Keyword = SiteSettings.Keyword;
            return View(result);
        }
        /// <summary>
        /// 下单前判断
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CanBuy(long id, int count)
        {
            Result result = new Result();
            bool isdataok = true;

            if (CurrentUser == null)
            {
                isdataok = false;
                result.success = false;
                result.msg = "您还未登录！";
                result.status = -1;
                return Json(result);
            }


            #region 礼品信息判断
            //礼品信息
            GiftInfo giftdata = _iGiftService.GetById(id);
            if (isdataok)
            {
                if (giftdata == null)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "礼品不存在！";
                    result.status = -2;
                }
            }

            if (isdataok)
            {
                if (giftdata.GetSalesStatus != GiftInfo.GiftSalesStatus.Normal)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "礼品已失效！";
                    result.status = -2;
                }
            }

            if (isdataok)
            {
                //库存判断
                if (count > giftdata.StockQuantity)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "礼品库存不足,仅剩 " + giftdata.StockQuantity.ToString() + " 件！";
                    result.status = -3;
                }
            }

            if (isdataok)
            {
                //积分数
                if (giftdata.NeedIntegral < 1)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "礼品关联等级信息有误或礼品积分数据有误！";
                    result.status = -5;
                    return Json(result);
                }
            }

            #endregion

            #region 用户信息判断

            if (isdataok)
            {
                //限购数量
                if (giftdata.LimtQuantity > 0)
                {
                    int ownbuynumber = _iGiftsOrderService.GetOwnBuyQuantity(CurrentUser.Id, id);
                    if (ownbuynumber + count > giftdata.LimtQuantity)
                    {
                        isdataok = false;
                        result.success = false;
                        result.msg = "超过礼品限兑数量！";
                        result.status = -4;
                    }
                }
            }

            if (isdataok)
            {
                var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id);
                if (giftdata.NeedIntegral * count > userInte.AvailableIntegrals)
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "积分不足！";
                    result.status = -6;
                }
            }

            if (isdataok && giftdata.NeedGrade > 0)
            {
                //等级判定
                if (!MemberGradeApplication.IsAllowGrade(CurrentUser.Id, giftdata.NeedGrade))
                {
                    isdataok = false;
                    result.success = false;
                    result.msg = "用户等级不足！";
                    result.status = -6;
                }
            }
            #endregion

            if (isdataok)
            {
                result.success = true;
                result.msg = "可以购买！";
                result.status = 1;
            }

            return Json(result);
        }
    }
}