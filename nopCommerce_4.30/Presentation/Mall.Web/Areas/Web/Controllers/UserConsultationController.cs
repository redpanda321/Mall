using Mall.Application;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Mall.Web.Framework;
using Mall.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;


namespace Mall.Web.Areas.Web.Controllers
{
    public class UserConsultationController : BaseMemberController
    {
        private IConsultationService _iConsultationService;
        public UserConsultationController(IConsultationService iConsultationService)
        {
            _iConsultationService = iConsultationService;
        }

        // GET: Web/Register
        public ActionResult Index(int pageNo = 1, int pageSize = 20)
        {
            var query = new ConsultationQuery
            {
                UserID = CurrentUser.Id,
                PageNo = pageNo,
                PageSize = pageSize
            };
            var model = _iConsultationService.GetConsultations(query);
            var products = ProductManagerApplication.GetProductByIds(model.Models.Select(p => p.ProductId));
            var consultation = model.Models.Select(item =>
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                return new ProductConsultationModel()
                {
                    Id = item.Id,
                    ConsultationContent = item.ConsultationContent,
                    ConsultationDate = item.ConsultationDate,
                    ProductName = product.ProductName,
                    ProductPic = product.ImagePath,
                    ProductId = item.ProductId,
                    UserName = item.UserName,
                    ReplyContent = item.ReplyContent,
                    ReplyDate = item.ReplyDate,
                };
            });
            #region 分页控制
            PagingInfo info = new PagingInfo
            {
                CurrentPage = pageNo,
                ItemsPerPage = pageSize,
                TotalItems = model.Total
            };
            ViewBag.pageInfo = info;
            #endregion
            ViewBag.Keyword = string.IsNullOrWhiteSpace(SiteSettings.SearchKeyword) ? SiteSettings.Keyword : SiteSettings.SearchKeyword;
            ViewBag.Keywords = SiteSettings.HotKeyWords;
            return View(consultation);
        }
    }
}