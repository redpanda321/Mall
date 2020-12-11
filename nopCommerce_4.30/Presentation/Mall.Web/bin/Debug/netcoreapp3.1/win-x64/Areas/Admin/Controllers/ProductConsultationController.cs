using Mall.CommonModel;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Web.Framework;
using Mall.Web.Models;
using System.Collections.Generic;
using System.Linq;

using Mall.Application;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class ProductConsultationController : BaseAdminController
    {
        private IConsultationService _iConsultationService;
        public ProductConsultationController(IConsultationService iConsultationService)
        {
           _iConsultationService = iConsultationService;
        }
        // GET: Admin/ProductConsultation
        public ActionResult Management()
        {
            return View();
        }

        [UnAuthorize]
        public JsonResult List(int page, int rows, string Keywords, int shopid = 0, bool? isReply = null)
        {
            var query = new ConsultationQuery { PageNo = page, PageSize = rows, KeyWords = Keywords, ShopID = shopid, IsReply = isReply };
            var data = _iConsultationService.GetConsultations(query);
            var products = ProductManagerApplication.GetProductByIds(data.Models.Select(p => p.ProductId));
            var list = data.Models.Select(item => {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                return new ProductConsultationModel()
                {
                    Id = item.Id,
                    ConsultationContent = HTMLEncode(item.ConsultationContent),
                    Date = item.ConsultationDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    ProductName = (product == null) ? "" : product.ProductName,
                    ProductId = item.ProductId,
                    UserName = item.UserName,
                    ReplyContent = HTMLEncode(item.ReplyContent),
                    ImagePath = (product == null) ? "" : product.GetImage(ImageSize.Size_50),
                    ReplyDate = item.ReplyDate,
                };
            });
            var model = new DataGridModel<ProductConsultationModel>() { rows = list, total = data.Total };
            return Json(model);
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult Detail(long id)
        {
            var model = _iConsultationService.GetConsultation(id);
            return Json(new { ConsulationContent = model.ConsultationContent, ReplyContent = model.ReplyContent });
        }
        [UnAuthorize]
        [HttpPost]
        public JsonResult Delete(long id)
        {
            _iConsultationService.DeleteConsultation(id);
            return Json(new Result() { success = true, msg = "删除成功！" });
        }

        public static string HTMLEncode(string txt)
        {
            if (string.IsNullOrEmpty(txt))
                return string.Empty;
            string Ntxt = txt;

            Ntxt = Ntxt.Replace(" ", "&nbsp;");

            Ntxt = Ntxt.Replace("<", "&lt;");

            Ntxt = Ntxt.Replace(">", "&gt;");

            Ntxt = Ntxt.Replace("\"", "&quot;");

            Ntxt = Ntxt.Replace("'", "&#39;");

            //Ntxt = Ntxt.Replace("\n", "<br>");

            return Ntxt;

        }
    }
}