using Mall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

using Mall.IServices;
using Mall.Web.Areas.SellerAdmin.Models;

using Mall.Core;
using Mall.Web.Models;
using Mall.CommonModel;
using AutoMapper;
using Mall.Application;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    [MarketingAuthorization]
    public class ShopBonusController : BaseSellerController
    {
        private IShopBonusService _bonusService;
        private IMarketService _iMarketService;
        public ShopBonusController(IShopBonusService iShopBonusService, IMarketService iMarketService)
        {
            this._bonusService = iShopBonusService;
            _iMarketService = iMarketService;
        }

        public ActionResult Management()
        {
            var settings = _iMarketService.GetServiceSetting(MarketType.RandomlyBonus);
            if (settings == null)
            {
                return View("UnSet");
            }

            var market = MarketApplication.GetMarketService(CurrentSellerManager.ShopId, MarketType.RandomlyBonus);
            //未购买服务且列表刚进来则让进入购买服务页
            if ((market == null || market.Id <= 0) && Request.Query["first"].ToString() == "1")
            {
                return RedirectToAction("BuyService");
            }

            ViewBag.Available = false;
            if (market != null && MarketApplication.GetServiceEndTime(market.Id) > DateTime.Now)
                ViewBag.Available = true;

            return View();
        }

        public ActionResult Add()
        {
            return View();
        }

        public ActionResult Edit(long id)
        {
            var obj = this._bonusService.GetShopBonus(id);
            ShopBonusModel model = new ShopBonusModel(obj);
            return View(model);
        }

        public ActionResult Detail(long id)
        {
            ViewBag.Id = id;
            return View();
        }

        public ActionResult UnConfig()
        {
            return View();
        }

        public ActionResult UnSet()
        {
            return View();
        }

        [HttpPost]
        public ActionResult List(string name, int state, int page = 1, int rows = 20)
        {
            var result = this._bonusService.Get(CurrentSellerManager.ShopId, name.Trim(), state, page, rows);
            List<ShopBonusModel> datas = new List<ShopBonusModel>();
            foreach (var m in result.Models)
            {
                datas.Add(new ShopBonusModel(m));
            }
            DataGridModel<ShopBonusModel> model = new DataGridModel<ShopBonusModel>
            {
                rows = datas,
                total = result.Total
            };
            return Json(model);
        }

        [HttpPost]
        public ActionResult DetailList(long id, int page = 1, int rows = 20)
        {
            var result = this._bonusService.GetDetail(id, page, rows);
            DateTime iniDate = new DateTime(1970, 1, 1);
            var datas = result.Models.ToList().Select(p => new ShopBonusReceiveModel()
            {
                OpenId = p.OpenId,
                Price = (decimal)p.Price,
                ReceiveTime = p.ReceiveTime != null && p.ReceiveTime> iniDate ? ((DateTime)p.ReceiveTime).ToString("yyyy-MM-dd"): "",
                StateStr = p.State.ToDescription(),
                UsedTime = p.UsedTime == null ? "" : ((DateTime)p.UsedTime).ToString("yyyy-MM-dd"),
                UsedOrderId = p.UsedOrderId.ToString()
            }).ToList();
            DataGridModel<ShopBonusReceiveModel> model = new DataGridModel<ShopBonusReceiveModel>
            {
                rows = datas,
                total = result.Total
            };
            return Json(model);
        }

        [HttpPost]
        public ActionResult IsOverDate(string bend, string end)
        {
            bool isExpired = false;
            var market = _bonusService.GetShopBonusService(CurrentSellerManager.ShopId);
            var time = MarketApplication.GetServiceEndTime(market.Id);
            var newEnd = DateTime.Parse(time.ToString("yyyy-MM-dd") + " 23:59:59");
            if (DateTime.Parse(bend) > newEnd || DateTime.Parse(end) > newEnd)
            {
                isExpired = true;
            }
            return Json(new { IsExpired = isExpired, ExpiredDate = newEnd.ToString("yyyy-MM-dd HH:mm:ss") });
        }

        [HttpPost]
       
        public ActionResult Add(ShopBonusModel model)
        {
            string image = model.ShareImg;
            string imageName = Guid.NewGuid().ToString() + ".png";
            string moveDir = "/Storage/Shop/Bonus/";
            if (image.Contains("/temp/"))
            {
                var source = image.Substring(image.LastIndexOf("/temp"));
                Core.MallIO.CopyFile(source, moveDir + imageName, true);
                model.ShareImg = "/Storage/Shop/Bonus/" + imageName;
            }
            else
            {
                model.ShareImg = "";
            }
            var market = this._bonusService.GetShopBonusService(CurrentSellerManager.ShopId);
            var time = MarketApplication.GetServiceEndTime(market.Id);
            if (model.BonusDateEnd > time || model.DateEnd > time)
            {
                throw new MallException("随机红包截止日期不允许超过您购买的服务到期时间。");
            }
            if (model.Count < 1 || model.Count > 1000)
            {
                throw new MallException("红包个数请控制在1-1000个！");
            }
            if(model.IsShowSyncWeiXin && string.IsNullOrWhiteSpace(model.CardTitle))
            {
                throw new MallException("同步卡券时，请填写卡券必填项");
            }
            if (!string.IsNullOrWhiteSpace(model.CardTitle))
            {
                //判断字符长度
                var enc = System.Text.Encoding.Default;
                if (enc.GetBytes(model.CardTitle).Count() > 18)
                {
                    throw new MallException("卡券标题不得超过9个汉字。");
                }
                if (!string.IsNullOrWhiteSpace(model.CardSubtitle))
                {
                    if (enc.GetBytes(model.CardSubtitle).Count() > 36)
                    {
                        throw new MallException("卡券副标题不得超过18个汉字。");
                    }
                }
            }
            this._bonusService.Add(model, CurrentSellerManager.ShopId);
            return RedirectToAction("Management");
        }

        [HttpPost]
       
        public ActionResult EditData(ShopBonusModel datamodel)
        {
            var market = this._bonusService.GetShopBonusService(CurrentSellerManager.ShopId);
            var olddata = _bonusService.GetShopBonus(datamodel.Id);
            //Mapper.CreateMap<Entities.ShopBonusInfo, ShopBonusModel>();
            ShopBonusModel model = olddata.Map<ShopBonusModel>();
            TryUpdateModelAsync(model);
            if (olddata==null)
            {
                throw new MallException("错误的参数！");
            }
            var time = MarketApplication.GetServiceEndTime(market.Id);
            if (model.BonusDateEnd > time || model.DateEnd > time)
            {
                throw new MallException("随机红包截止日期不允许超过您购买的服务到期时间。");
            }
            if (model.Count < 1 || model.Count > 1000)
            {
                throw new MallException("红包个数请控制在1-1000个！");
            }
            string image = model.ShareImg;
            string imageName = Guid.NewGuid().ToString() + ".png";
            string moveDir = "/Storage/Shop/Bonus/";
            if (image.Contains("/temp/"))
            {
                var source = image.Substring(image.LastIndexOf("/temp"));
                Core.MallIO.CopyFile(source, moveDir + imageName);
                model.ShareImg = "/Storage/Shop/Bonus/" + imageName;
            }
            else if (image.Contains("/Storage/"))
            {
                model.ShareImg = image.Substring(image.LastIndexOf("/Storage/"));
            }
            else
            {
                model.ShareImg = "";
            }
            if (string.IsNullOrEmpty(model.QRPath))
                model.QRPath = "";
            this._bonusService.Update(model);
            return RedirectToAction("Management");
        }

        [HttpPost]
        public ActionResult IsAdd()
        {
            return Json(this._bonusService.IsAdd(CurrentSellerManager.ShopId));
        }

        [HttpPost]
        public JsonResult Invalid(long id)
        {
            this._bonusService.Invalid(id);
            return Json(true);
        }

        #region 服务费用设置
        public ActionResult BuyService()
        {
            var model = new ShopBonusBuyServiceViewModel();
            model.Market = this._bonusService.GetShopBonusService(CurrentSellerManager.ShopId);
            var active = _iMarketService.GetServiceSetting(MarketType.RandomlyBonus);
            model.IsNo = true;
            string endDate = null;
            var now = DateTime.Now.Date;
            ViewBag.LastBuyPrice = -1;
            if (model.Market != null)
            {
                var endtime = MarketApplication.GetServiceEndTime(model.Market.Id);
                if (endtime < now)
                {
                    endDate = string.Format("<font class=\"red\">{0} 年 {1} 月 {2} 日</font> (您的随机红包服务已经过期)", endtime.Year, endtime.Month, endtime.Day);
                }
                else if(endtime >= now)
                {
                    model.Available = true;
                    endDate = string.Format("{0} 年 {1} 月 {2} 日", endtime.Year, endtime.Month, endtime.Day);
                }
                ViewBag.LastBuyPrice = MarketApplication.GetLastBuyPrice(model.Market.Id);
            }
           
            else if (active == null)
            {
                model.IsNo = false;
                return View(model);
            }
            model.EndDate = endDate;
            model.Price = active.Price;
            return View(model);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult BuyService(int month)
        {
            Result result = new Result();
            var service = _iMarketService;
            service.OrderMarketService(month, CurrentSellerManager.ShopId, MarketType.RandomlyBonus);
            result.success = true;
            result.msg = "购买服务成功";
            return Json(result);
        }
        #endregion
    }
}