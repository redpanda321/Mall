using System;
using System.Collections.Generic;
using System.Linq;

using Mall.Core;
using Mall.DTO;
using Mall.Web.Framework;
using Mall.CommonModel;
using Mall.Application;
using Mall.DTO.QueryModel;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    [MarketingAuthorization]
    /// <summary>
    /// 拼团
    /// </summary>
    public class FightGroupController : BaseAdminController
    {
        #region 活动列表
        /// <summary>
        /// 拼团活动列表
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult PostActiveList(FightGroupActiveQuery query)
        {
            var data = FightGroupApplication.GetActives(query);
            return Json(data, true);
        }
        /// <summary>
        /// 取消活动
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult CancelActive(long id,string mremark)
        {
            Result result = new Result { success = false, msg = "操作失败", status = -1 };
            mremark = ClearHtmlString(mremark);
            if (string.IsNullOrWhiteSpace(mremark))
            {
                result = new Result { success = false, msg = "请填写下架理由", status = -1 };
                return Json(result);
            }
            FightGroupApplication.CancelActive(id,mremark,CurrentManager.Id);

            var info = FightGroupApplication.GetFightGroupActiveInfo(id);
            ProductManagerApplication.SaveCaculateMinPrice(info.ProductId, info.ShopId);
            result = new Result { success = true, msg = "操作成功", status = 1 };
            return Json(result);
        }

        /// <summary>
        /// 清理引号类字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string ClearHtmlString(string str)
        {
            string result = str;
            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.Replace("'", "&#39;");
                result = result.Replace("\"", "&#34;");
                result = result.Replace(">", "&gt;");
                result = result.Replace("<", "&lt;");
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 查看活动
        /// </summary>
        /// <param name="id">活动编号</param>
        /// <returns></returns>
        public ActionResult ViewActive(long id)
        {
            FightGroupActiveModel model = FightGroupApplication.GetActive(id);
            if (model == null)
            {
                throw new MallException("错误的活动编号");
            }
            return View(model);
        }

        #region 拼团详情
        /// <summary>
        /// 查看拼团情况
        /// </summary>
        /// <param name="id">活动编号</param>
        /// <returns></returns>
        public ActionResult ViewGroupList(long id)
        {
            ViewBag.ActionId = id;
            return View();
        }

        [HttpPost]
        public JsonResult PostGroupList(long actionId,FightGroupBuildStatus? groupStatus, DateTime? startTime, DateTime? endTime, int page, int rows)
        {
            List<FightGroupBuildStatus> seastatus = new List<FightGroupBuildStatus>();
            if (groupStatus.HasValue)
            {
                seastatus.Add(groupStatus.Value);
            }
            var data = FightGroupApplication.GetGroups(actionId, seastatus, startTime, endTime, page, rows);
            return Json(new { rows = data.Models.ToList(), total = data.Total });
        }
        #endregion

        #region 服务购买列表

        public ActionResult BoughtList()
        {
            return View();
        }

        [UnAuthorize]
        public JsonResult GetBoughtList(MarketBoughtQuery query)
        {
            query.MarketType = MarketType.FightGroup;
            var data = MarketApplication.GetBoughtShopList(query);
            var list = data.Models.Select(d => {
                var market = MarketApplication.GetMarketService(d.MarketServiceId);
                return new MarketServiceBuyRecordModel
                {
                    Id = d.Id,
                    EndTime = d.EndTime,
                    MarketServiceId = d.MarketServiceId,
                    StartTime = d.StartTime,
                    SettlementFlag = d.SettlementFlag,
                    ShopName = market.ShopName
                };
            }).ToList();
            var result = new QueryPageModel<MarketServiceBuyRecordModel>
            {
                Models = list,
                Total = data.Total
            };
            return Json(result, true);
        }
        #endregion

        #region 服务费用设置

        public ActionResult ServiceSetting()
        {
            decimal model = FightGroupApplication.GetMarketServicePrice();
            return View(model);
        }

        [HttpPost]
        public JsonResult ServiceSetting(decimal Price)
        {
            Result result = new Result();
            if(Price<0)
            {
                result.success = false;
                result.msg = "错误的服务价格！";
                return Json(result);
            }
            FightGroupApplication.SetMarketServicePrice(Price);
            result.success = true;
            result.msg = "保存成功！";
            return Json(result);
        }
        #endregion
    }
}