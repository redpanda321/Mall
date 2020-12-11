using Mall.Application;
using Mall.DTO;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


namespace Mall.Web.Areas.Admin.Controllers
{
    [MarketingAuthorization]
    public class TopicController : BaseAdminController
    {
        ITopicService _iTopicService;
        public TopicController(ITopicService iTopicService)
        {
            _iTopicService = iTopicService;
        }
        // GET: Admin/Topic
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Management()
        {
            return View();
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult List(int page, int rows)
        {
            var topics = _iTopicService.GetTopics(page, rows);
            var curUrl = CurrentUrlHelper.CurrentUrl();
            var list = new
            {
                rows = topics.Models.Select(item => new
                {
                    id = item.Id,
                    name = item.Name,
                    url = curUrl+ "/topic/detail/" + item.Id,
                    IsRecommend = item.IsRecommend
                }),
                total = topics.Total
            };
            return Json(list);
        }

        public ActionResult Add(long? id)
        {
            if (id.HasValue)
            {
                var topicInfo = TopicApplication.GetTopic(id.Value);
                var modules = TopicApplication.GetModules(id.Value);
                var products = TopicApplication.GetModuleProducts(modules.Select(p => p.Id));
                var topicModel = new Models.TopicModel()
                {
                    BackgroundImage = topicInfo.BackgroundImage,
                    Id = topicInfo.Id,
                    Name = topicInfo.Name,
                    TopImage = topicInfo.TopImage,
                    TopicModuleInfo = modules,
                    ModuleProducts = products,
                    IsRecommend = topicInfo.IsRecommend,
                    SelfDefineText = topicInfo.SelfDefineText
                };
                return View(topicModel);
            }
            else
                return View(new Models.TopicModel());
        }
        [UnAuthorize]
  
        [HttpPost]
        public JsonResult Add(string topicJson)
        {
            var s = new Newtonsoft.Json.JsonSerializerSettings();
            s.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
            s.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            var topic = Newtonsoft.Json.JsonConvert.DeserializeObject<Topic>(topicJson, s);
            if (topic.TopicModuleInfo.Count == 0)
                return Json(new { success = false, msg = "至少需要录入1个模块！" });

            if (topic.Id > 0)
                _iTopicService.UpdateTopic(topic);
            else
                _iTopicService.AddTopic(topic);

            return Json(new { success = true });
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult Delete(long id)
        {
            Result result = new Result();
            _iTopicService.DeleteTopic(id);
            result.success = true;
            return Json(result);
        }

    }
}