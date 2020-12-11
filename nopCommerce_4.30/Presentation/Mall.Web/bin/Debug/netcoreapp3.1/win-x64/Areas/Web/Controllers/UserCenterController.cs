using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Plugins.Message;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Areas.Web.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;


namespace Mall.Web.Areas.Web.Controllers
{
    //TODO:YZY好多Service
    public class UserCenterController : BaseMemberController
    {
        private IMemberService _iMemberService;
        private IProductService _iProductService;
        private IMessageService _iMessageService;
        private ICommentService _iCommentService;
        private IMemberCapitalService _iMemberCapitalService;
        private IOrderService _iOrderService;
        //private IMemberInviteService _iMemberInviteService;
        //private IMemberIntegralService _iMemberIntegralService;
        private ICartService _iCartService;
        public UserCenterController(
            IMemberService iMemberService,
            IProductService iProductService,
            IMessageService iMessageService,
            ICommentService iCommentService,
            IMemberCapitalService iMemberCapitalService,
            IOrderService iOrderService,
            ICartService iCartService
            )
        {
            _iMemberService = iMemberService;
            _iProductService = iProductService;
            _iMessageService = iMessageService;
            _iCommentService = iCommentService;
            _iMemberCapitalService = iMemberCapitalService;
            _iOrderService = iOrderService;
            _iCartService = iCartService;

        }

        public ActionResult Index()
        {
            //TODO:个人中心改成单页后，将index框架页改成与home页一致
            return RedirectToAction("home");
        }

        public ActionResult Home()
        {
            UserCenterHomeModel viewModel = new UserCenterHomeModel();

            viewModel.userCenterModel = MemberApplication.GetUserCenterModel(CurrentUser.Id);
            viewModel.UserName = CurrentUser.Nick == "" ? CurrentUser.UserName : CurrentUser.Nick;
            viewModel.Logo = CurrentUser.Photo;
            var items = _iCartService.GetCart(CurrentUser.Id).Items.OrderByDescending(a => a.AddTime).Select(p => p.ProductId).Take(3).ToArray();
            viewModel.ShoppingCartItems = ProductManagerApplication.GetProductByIds(items).ToArray();
            var UnEvaluatProducts = _iCommentService.GetUnEvaluatProducts(CurrentUser.Id).ToArray();
            viewModel.UnEvaluatProductsNum = UnEvaluatProducts.Count();
            viewModel.Top3UnEvaluatProducts = UnEvaluatProducts.Take(3).ToArray();
            viewModel.Top3RecommendProducts = _iProductService.GetPlatHotSaleProductByNearShop(8, CurrentUser.Id).ToArray();
            viewModel.BrowsingProducts = BrowseHistrory.GetBrowsingProducts(4, CurrentUser == null ? 0 : CurrentUser.Id);

            var messagePlugins = PluginsManagement.GetPlugins<IMessagePlugin>();
            var data = messagePlugins.Select(item => new PluginsInfo
            {
                ShortName = item.Biz.ShortName,
                PluginId = item.PluginInfo.PluginId,
                Enable = item.PluginInfo.Enable,
                IsSettingsValid = item.Biz.IsSettingsValid,
                IsBind = !string.IsNullOrEmpty(_iMessageService.GetDestination(CurrentUser.Id, item.PluginInfo.PluginId, Entities.MemberContactInfo.UserTypes.General))
            });
            viewModel.BindContactInfo = data;

            var statistic = StatisticApplication.GetMemberOrderStatistic(CurrentUser.Id);
            viewModel.OrderCount = statistic.OrderCount;
            viewModel.OrderWaitReceiving = statistic.WaitingForRecieve;
            viewModel.OrderWaitPay = statistic.WaitingForPay;
            viewModel.OrderEvaluationStatus = statistic.WaitingForComments;
            viewModel.Balance = MemberCapitalApplication.GetBalanceByUserId(CurrentUser.Id);
            //TODO:[YZG]增加账户安全等级
            MemberAccountSafety memberAccountSafety = new MemberAccountSafety
            {
                AccountSafetyLevel = 1
            };
            if (CurrentUser.PayPwd != null)
            {
                memberAccountSafety.PayPassword = true;
                memberAccountSafety.AccountSafetyLevel += 1;
            }
            var ImessageService = _iMessageService;
            foreach (var messagePlugin in data)
            {
                if (messagePlugin.PluginId.IndexOf("SMS") > 0)
                {
                    if (messagePlugin.IsBind)
                    {
                        memberAccountSafety.BindPhone = true;
                        memberAccountSafety.AccountSafetyLevel += 1;
                    }
                }
                else
                {
                    if (messagePlugin.IsBind)
                    {
                        memberAccountSafety.BindEmail = true;
                        memberAccountSafety.AccountSafetyLevel += 1;
                    }
                }
            }
            viewModel.memberAccountSafety = memberAccountSafety;
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View(viewModel);
        }

        public ActionResult Bind(string pluginId)
        {
            var messagePlugin = PluginsManagement.GetPlugin<IMessagePlugin>(pluginId);
            ViewBag.ShortName = messagePlugin.Biz.ShortName;
            ViewBag.id = pluginId;
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View();
        }

        [HttpPost]
        public ActionResult SendCode(string pluginId, string destination, bool checkBind = false)
        {
            if (checkBind && _iMessageService.GetMemberContactsInfo(pluginId, destination, Entities.MemberContactInfo.UserTypes.General) != null)
            {
                return Json(new Result() { success = false, msg = destination + "已经绑定过了！" });
            }
            _iMemberService.CheckContactInfoHasBeenUsed(pluginId, destination);
            var timeout = CacheKeyCollection.MemberPluginCheckTime(CurrentUser.UserName, pluginId);
            if (Core.Cache.Exists(timeout))
            {
                return Json(new Result() { success = false, msg = "120秒内只允许请求一次，请稍后重试!" });
            }
            var checkCode = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            if (pluginId.ToLower().Contains("email"))
            {
                cacheTimeout = DateTime.Now.AddHours(24);
            }
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId + destination), checkCode.ToString(), cacheTimeout);
            var user = new MessageUserInfo() { UserName = CurrentUser.UserName, SiteName = SiteSettings.SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheckTime(CurrentUser.UserName, pluginId), "0", DateTime.Now.AddSeconds(120));
            return Json(new Result() { success = true, msg = "发送成功" });
        }

        [HttpPost]
        public ActionResult CheckCode(string pluginId, string code, string destination)
        {
            var cache = CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId + destination);
            var cacheCode = Core.Cache.Get<string>(cache);
            var member = CurrentUser;
            var mark = "";
            if (cacheCode != null && cacheCode == code)
            {
                var service = _iMessageService;
                if (service.GetMemberContactsInfo(pluginId, destination, Entities.MemberContactInfo.UserTypes.General) != null)
                {
                    return Json(new Result() { success = false, msg = destination + "已经绑定过了！" });
                }
                if (pluginId.ToLower().Contains("email"))
                {
                    member.Email = destination;
                    mark = "邮箱";
                }
                else if (pluginId.ToLower().Contains("sms"))
                {
                    member.CellPhone = destination;
                    mark = "手机";
                }

                _iMemberService.UpdateMember(member);
                service.UpdateMemberContacts(new Entities.MemberContactInfo()
                {
                    Contact = destination,
                    ServiceProvider = pluginId,
                    UserId = CurrentUser.Id,
                    UserType = Entities.MemberContactInfo.UserTypes.General
                });
                Core.Cache.Remove(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId));
                Core.Cache.Remove(CacheKeyCollection.Member(CurrentUser.Id));//移除用户缓存
                Core.Cache.Remove("Rebind" + CurrentUser.Id);

                Mall.Entities.MemberIntegralRecordInfo info = new Mall.Entities.MemberIntegralRecordInfo();
                //_iMemberIntegralConversionFactoryService = ServiceApplication.Create<im;
                //_iMemberIntegralService = ServiceApplication.Create<IMemberIntegralService>();
                //_iMemberInviteService = ServiceApplication.Create<IMemberInviteService>();
                info.UserName = member.UserName;
                info.MemberId = member.Id;
                info.RecordDate = DateTime.Now;
                info.TypeId = Mall.Entities.MemberIntegralInfo.IntegralType.Reg;
                info.ReMark = "绑定" + mark;
                var memberIntegral = ServiceApplication.Create<IMemberIntegralConversionFactoryService>().Create(Mall.Entities.MemberIntegralInfo.IntegralType.Reg);
                ServiceApplication.Create<IMemberIntegralService>().AddMemberIntegral(info, memberIntegral);


                var inviteMember = _iMemberService.GetMember(member.InviteUserId);
                if (inviteMember != null)
                    ServiceApplication.Create<IMemberInviteService>().AddInviteIntegel(member, inviteMember);

                return Json(new Result() { success = true, msg = "验证正确" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "验证码不正确或者已经超时" });
            }
        }

        public ActionResult Finished()
        {
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View();
        }

        public ActionResult AccountSafety()
        {
            MemberAccountSafety model = new MemberAccountSafety();
            model.AccountSafetyLevel = 1;
            if (CurrentUser.PayPwd != null)
            {
                model.PayPassword = true;
                model.AccountSafetyLevel += 1;
            }
            var messagePlugins = PluginsManagement.GetPlugins<IMessagePlugin>();
            var ImessageService = _iMessageService;
            var data = messagePlugins.Select(item => new PluginsInfo
            {
                ShortName = item.Biz.ShortName,
                PluginId = item.PluginInfo.PluginId,
                Enable = item.PluginInfo.Enable,
                IsSettingsValid = item.Biz.IsSettingsValid,
                IsBind = !string.IsNullOrEmpty(ImessageService.GetDestination(CurrentUser.Id, item.PluginInfo.PluginId, Entities.MemberContactInfo.UserTypes.General))
            });
            foreach (var messagePlugin in data)
            {
                if (messagePlugin.PluginId.IndexOf("SMS") > 0)
                {
                    if (messagePlugin.IsBind)
                    {
                        model.BindPhone = true;
                        model.AccountSafetyLevel += 1;
                    }
                }
                else
                {
                    if (messagePlugin.IsBind)
                    {
                        model.BindEmail = true;
                        model.AccountSafetyLevel += 1;
                    }
                }
            }
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View(model);
        }
    }
}
