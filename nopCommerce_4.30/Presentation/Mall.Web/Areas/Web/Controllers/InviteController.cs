using Mall.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Web.Controllers
{
    public class InviteController : Controller
    {
        private IMemberInviteService _iMemberInviteService;
        public InviteController(IMemberInviteService iMemberInviteService)
        {
            _iMemberInviteService = iMemberInviteService;
        }
        // GET: Web/Invite
        public ActionResult Index()
        {
            var rule = _iMemberInviteService.GetInviteRule();
            return View(rule);
        }
    }
}