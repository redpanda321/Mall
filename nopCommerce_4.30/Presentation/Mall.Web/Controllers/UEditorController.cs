using Mall.Entities;
using Mall.Web.App_Code.UEditor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Web;


namespace Mall.Web.Controllers
{
    public class UEditorController : Controller
    {

        // GET: UEditor
        public ContentResult Handle()
        {
            UploadConfig config = null;
            IUEditorHandle handle = null;
            string action = Request.Query["action"].ToString();
            switch (action)
            {
                case "config":
                    handle = new ConfigHandler();
                    break;
                case "uploadimage":
                    config = new UploadConfig()
                    {
                        AllowExtensions = Config.GetStringList("imageAllowFiles"),
                        PathFormat = Config.GetString("imagePathFormat"),
                        SizeLimit = Config.GetInt("imageMaxSize"),
                        UploadFieldName = Config.GetString("imageFieldName")
                    };
                    handle = new UploadHandle(config);
                    break;
                default:
                    handle = new NotSupportedHandler();
                    break;
            }

            var result = handle.Process();
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            return Content(jsonString);
        }

        [HttpPost]
        // GET: UEditor
        public ContentResult Handle(string action)
        {
            UploadConfig config = null;
            IUEditorHandle handle = null;
            action = Request.Query["action"];
            switch (action)
            {
                case "config":
                    handle = new ConfigHandler();
                    break;
                case "uploadimage":
                    config = new UploadConfig()
                    {
                        AllowExtensions = Config.GetStringList("imageAllowFiles"),
                        PathFormat = Config.GetString("imagePathFormat"),
                        SizeLimit = Config.GetInt("imageMaxSize"),
                        UploadFieldName = Config.GetString("imageFieldName")
                    };
                    handle = new UploadHandle(config);
                    break;
                case "uploadtemplateimage":
                    var controllerName = Request.Query["areaName"].ToString();
                    var shopId = "0";
                    if (controllerName.ToLower().Equals("selleradmin"))
                    {
                        //long userId = UserCookieEncryptHelper.Decrypt(WebHelper.GetCookie(CookieKeysCollection.SELLER_MANAGER), "SellerAdmin");
                        string _tmpstr = Request.Query["ShopId"].ToString();
                        //if (userId != 0)
                        //{
                        //    sellerManager = ServiceApplication.Create<IManagerService>().GetSellerManager(userId);
                        //}
                        shopId = (string.IsNullOrWhiteSpace(_tmpstr) ? "NonShopID" : _tmpstr);
                    }
                    config = new UploadConfig()
                    {
                        AllowExtensions = Config.GetStringList("templateimageAllowFiles"),
                        PathFormat = Config.GetString("templateimagePathFormat").Replace("{ShopID}", shopId),
                        SizeLimit = Config.GetInt("templateimageMaxSize"),
                        UploadFieldName = Config.GetString("templateimageFieldName"),
                        ShopId = long.Parse(shopId)

                    };
                    handle = new UploadHandle(config);
                    break;
                default:
                    handle = new NotSupportedHandler();
                    break;
            }

            var result = handle.Process();
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            return Content(jsonString);
        }





        //  
        // GET: /Upload/  
        [HttpGet]
        public ActionResult Upload()
        {
            string url = Request.Query["url"].ToString();
            if (url == null)
            {
                url = "";
            }
            ViewData["url"] = url;
            return View();
        }

        [HttpPost]
        public ActionResult UploadImage(IFormFile filename)
        {
            //具体的保存代码  
            return View();
        }

    }




}

