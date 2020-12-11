using Mall.Application;
using Mall.Core;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Web.Framework;
using Mall.Web.Models;
using System.Linq;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    public class LabelController : BaseAdminController
    {
        IMemberLabelService _iMemberLabelService;
        IMemberService _iMemberService;
        public LabelController(IMemberLabelService iMemberLabelService, IMemberService iMemberService)
        {
            _iMemberLabelService = iMemberLabelService;
            _iMemberService = iMemberService;
        }

        [Description("会员标签管理页面")]
        public ActionResult Management()
        {
            return View();
        }

        [Description("分页获取会员管理JSON数据")]
        public JsonResult List(int page, string keywords, int rows)
        {
            var result = _iMemberLabelService.GetMemberLabelList(new LabelQuery { LabelName = keywords, PageSize = rows, PageNo = page });
            var labels = result.Models.ToList().Select(item => new LabelModel()
            {
                MemberNum = _iMemberService.GetMembersByLabel(item.Id).Count(),
                LabelName = item.LabelName,
                Id = item.Id
            });
            return Json(new { rows = labels.ToList(), total = result.Total });
            //var model = new DataGridModel<LabelModel>() { rows = labels, total = result.Total };
            //return Json(model);
        }

        public ActionResult Label(long id=0)
        {
            var model = _iMemberLabelService.GetLabel(id) ?? new Mall.Entities.LabelInfo() { };
            LabelModel labelmodel = new LabelModel()
            {
                Id = model.Id,
                LabelName = model.LabelName
            };
            return View(labelmodel);
        }
        [HttpPost]
        public JsonResult Label(LabelModel model)
        {
            Mall.Entities.LabelInfo labelmodel = new Mall.Entities.LabelInfo()
            {
                Id = model.Id,
                LabelName = model.LabelName
            };

            if (MemberLabelApplication.CheckNameIsExist(model.LabelName))
            {
                throw new MallException("标签已经存在，不能重复！");
            }
            if (labelmodel.Id > 0)
            {
                _iMemberLabelService.UpdateLabel(labelmodel);
            }
            else
            {
                _iMemberLabelService.AddLabel(labelmodel);
            }
            return Json(new Result { success=true});
        }
        public JsonResult deleteLabel(long Id)
        {
            var count = _iMemberService.GetMembersByLabel(Id).Count();
            if (count>0)
            {
                throw new MallException("标签已经在使用，不能删除！");
            }
            _iMemberLabelService.DeleteLabel(new Mall.Entities.LabelInfo() { Id = Id });
            return Json(new Result { success = true });
        }

        public JsonResult CheckLabelIsExist(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new MallException("标签名不能为空！");
            }
            var labels = MemberLabelApplication.GetLabelList(new LabelQuery
            {
                LabelName = name
            });
            if (labels.Models.Count>0)
            {
                throw new MallException("标签已经存在，不能重复！");
            }
            return Json(new Result { success = true });
        }
    }
}