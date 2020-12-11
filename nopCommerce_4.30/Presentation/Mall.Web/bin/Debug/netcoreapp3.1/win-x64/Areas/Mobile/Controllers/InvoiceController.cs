using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.Web.App_Code.Common;
using Mall.Web.Areas.Mobile.Models;
using Mall.Web.Areas.Web.Models;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace Mall.Web.Areas.Mobile.Controllers
{
    public class InvoiceController : BaseMobileMemberController
    {
        public ActionResult Index()
        {
            return View();
        }
    }

}