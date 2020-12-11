using Mall.CommonModel;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Web.Framework;
using Mall.Web.Models;
using System.Collections.Generic;
using System.Linq;

using Mall.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;

namespace Mall.Web.Areas.SellerAdmin.Controllers
{
    public class ProductConsultationController : BaseSellerController
    {
        private IConsultationService _iConsultationService;
        public ProductConsultationController(IConsultationService iConsultationService)
        {
            _iConsultationService = iConsultationService;
        }

        public ActionResult Management()
        {
            bool isReply = Request.GetDisplayUrl().ToLower().Contains("selleradmin/home/console");
            ViewBag.isReply = isReply ? 1 : 0;
            return View();
        }

        [UnAuthorize]
        public JsonResult List(int page, int rows, string Keywords, bool? isReply = null)
        {
            var query = new ConsultationQuery { PageNo = page, PageSize = rows, KeyWords = Keywords, ShopID = base.CurrentSellerManager.ShopId, IsReply = isReply };
            var result = _iConsultationService.GetConsultations(query);
            var products = ProductManagerApplication.GetProductByIds(result.Models.Select(p => p.ProductId));
            var consultation = result.Models.ToList().Select(item => {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                return new ProductConsultationModel()
                {
                    Id = item.Id,
                    ConsultationContent = HTMLEncode(item.ConsultationContent),
                    ConsultationDate = item.ConsultationDate,
                    ConsultationDateStr = item.ConsultationDate.ToString("yyyy-MM-dd HH:mm"),
                    ProductName = (product == null) ? "" : product.ProductName,
                    ProductPic = product.ImagePath,
                    ProductId = item.ProductId,
                    UserName = item.UserName,
                    ReplyContent = HTMLEncode(item.ReplyContent),
                    ImagePath = (product == null) ? "" : product.GetImage(ImageSize.Size_50),
                    ReplyDate = item.ReplyDate,
                };
            });
            var model = new DataGridModel<ProductConsultationModel>() { rows = consultation, total = result.Total };
            return Json(model);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult Detail(long id)
        {
            var model = _iConsultationService.GetConsultation(id);
            return Json(new { ConsulationContent = model.ConsultationContent, ReplyContent = model.ReplyContent });
        }
        [HttpPost]
        [UnAuthorize]
        public JsonResult Delete(long id)
        {
            _iConsultationService.DeleteConsultation(id);
            return Json(new Result() { success = true, msg = "删除成功！" });
        }

        [UnAuthorize]
        public JsonResult ReplyConsultation(long id, string replycontent)
        {
            var shopid = base.CurrentSellerManager.ShopId;
            _iConsultationService.ReplyConsultation(id, HTMLEncode(replycontent), shopid);
            return Json(new Result() { success = true, msg = "回复成功！" });
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