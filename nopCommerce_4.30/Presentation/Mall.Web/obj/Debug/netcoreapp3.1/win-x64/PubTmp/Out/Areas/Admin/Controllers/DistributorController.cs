using System;
using System.Collections.Generic;
using System.Linq;

using Mall.Core;
using Mall.DTO;
using Mall.Web.Framework;
using Mall.CommonModel;
using Mall.Application;
using Mall.DTO.QueryModel;
using Mall.Web.Areas.Admin.Models.Distribution;
using Mall.DTO.Distribution;
using Mall.Web.Areas.Admin.Models;
using Mall.CommonModel;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Areas.Admin.Controllers
{
    //[H5Authorization]
    /// <summary>
    /// 销售员管理
    /// </summary>
    public class DistributorController : BaseAdminController
    {
        #region 列表
        public ActionResult Management(long? gradeId)
        {
            DistributorManagementModel result = new DistributorManagementModel();
            result.Grades = DistributionApplication.GetDistributorGrades();
            result.DistributionMaxLevel = SiteSettings.DistributionMaxLevel;
            result.SiteSetting = SiteSettingApplication.SiteSettings;
            result.GradeId = gradeId;
            return View(result);
        }

        /// <summary>
        /// 获取销售员列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetDistributorList(string MemberName, string shopname, string SuperiorMemberName
            , int rows, int page
            , DateTime? StartTime = null, DateTime? EndTime = null
            , long? GradeId = null, DistributorStatus? status = null
            , string Sort = "", bool IsAsc = false)
        {
            //查询条件
            DistributorQuery query = new DistributorQuery();

            query.MemberName = MemberName;
            query.ShopName = shopname;
            query.SuperiorMemberName = SuperiorMemberName;
            query.StartTime = StartTime;
            query.EndTime = EndTime;
            query.GradeId = GradeId;
            query.Status = status;

            query.PageSize = rows;
            query.PageNo = page;
            query.Sort = Sort;
            query.IsAsc = IsAsc;
            query.IsIncludeMember = false;
            if (query.StartTime.HasValue)
            {
                query.StartTime = query.StartTime.Value.Date;
            }
            if (query.EndTime.HasValue)
            {
                query.EndTime = query.EndTime.Value.AddDays(1).Date;
            }
            if (query.Sort.ToLower() == "ShowApplyTime".ToLower())
            {
                query.Sort = "ApplyTime";
            }

            var data = DistributionApplication.GetDistributors(query);
            var result = new { rows = data.Models, total = data.Total };
            return Json(result);
        }
        /// <summary>
        /// 获取顶级销售员列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCanSuperDistributorList(string MemberName, long memberId
            , int rows, int page)
        {
            //查询条件
            DistributorQuery query = new DistributorQuery();

            query.MemberName = MemberName;
            query.ExcludeMemberIds = new List<long>();
            query.ExcludeMemberIds.Add(memberId);
            query.SuperiorMemberId = 0;
            query.Level = 1;

            query.PageSize = rows;
            query.PageNo = page;
            query.IsIncludeMember = false;

            var data = DistributionApplication.GetDistributors(query);
            var result = new { rows = data.Models, total = data.Total };
            return Json(result);
        }
        /// <summary>
        /// 更改上级
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ChangeSuper(long id, long superId)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            if (superId == 0 || id == 0)
            {
                throw new MallException("错误的参数");
            }
            if (id == superId)
            {
                throw new MallException("不可以调整自己为自己的上级");
            }
            var super = DistributionApplication.GetDistributor(superId);
            if (super.SuperiorId > 0)
            {
                throw new MallException("调整上级只可以为一级销售员");
            }
            DistributionApplication.UpdateDistributorSuperId(id, superId);
            result.success = true;
            result.msg = "操作成功";
            return Json(result);
        }

        #region 清退销售员
        /// <summary>
        /// 清退销售员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RemoveDistributor(long id)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            List<long> ids = new List<long>();
            if (id > 0)
                ids.Add(id);

            result = BatRemoveDistributor(ids);
            return Json(result);
        }
        /// <summary>
        /// 批量清退销售员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RemoveDistributorList(string ids)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            if (string.IsNullOrEmpty(ids))
                return Json(result);

            List<long> Ids = ids.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(d => long.Parse(d)).ToList();
            result = BatRemoveDistributor(Ids);
            return Json(result);
        }

        /// <summary>
        /// 清退销售员
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private Result BatRemoveDistributor(List<long> ids)
        {
            Result result = new Result { success = false, msg = "错误的编号参数" };
            if (ids.Count() > 0)
            {
                DistributionApplication.RemoveDistributor(ids);
                result = new Result { success = true, msg = "清退销售员完成" };
            }

            return result;
        }
        #endregion

        #region 恢复销售员
        /// <summary>
        /// 恢复销售员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RecoverDistributor(long id)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            List<long> ids = new List<long>();
            if (id > 0)
                ids.Add(id);

            result = BatRecoverDistributor(ids);
            return Json(result);
        }
        /// <summary>
        /// 批量恢复销售员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RecoverDistributorList(string ids)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            if (string.IsNullOrEmpty(ids))
                return Json(result);

            List<long> Ids = ids.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(d => long.Parse(d)).ToList();
            result = BatRecoverDistributor(Ids);
            return Json(result);
        }

        /// <summary>
        /// 恢复销售员
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private Result BatRecoverDistributor(List<long> ids)
        {
            Result result = new Result { success = false, msg = "错误的编号参数" };
            if (ids.Count() > 0)
            {
                DistributionApplication.RecoverDistributor(ids);
                result = new Result { success = true, msg = "恢复销售员完成" };
            }

            return result;
        }
        #endregion

        #region 审核销售员
        /// <summary>
        /// 审核销售员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AuditDistributor(string ids, string remark, DistributorStatus status)
        {
            Result result = new Result { success = false, msg = "未知错误" };
            if (string.IsNullOrEmpty(ids))
                return Json(result);
            remark = remark.Replace("'", "’").Replace("\"", "“").Replace("<", "&lt;").Replace(">", "&gt;");
            List<long> Ids = ids.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(d => long.Parse(d)).ToList();
            result = BatAuditDistributor(Ids, remark, status);
            return Json(result);
        }

        /// <summary>
        /// 审核销售员
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private Result BatAuditDistributor(List<long> ids, string remark, DistributorStatus status)
        {
            Result result = new Result { success = false, msg = "错误的编号参数" };
            if (status != DistributorStatus.Audited && status != DistributorStatus.Refused)
            {
                result = new Result { success = false, msg = "错误的状态参数" };
                return result;
            }
            if (ids.Count() > 0)
            {
                switch (status)
                {
                    case DistributorStatus.Refused:
                        DistributionApplication.RefuseDistributor(ids, remark);
                        break;
                    case DistributorStatus.Audited:
                        DistributionApplication.AgreeDistributor(ids, remark);
                        break;
                }
                result = new Result { success = true, msg = "审核销售员完成" };
            }

            return result;
        }
        #endregion

        /// <summary>
        /// 导出销售员
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ActionResult ExportToExcelDistributor(DistributorQuery query)
        {
            query.IsIncludeMember = false;
            if (query.StartTime.HasValue)
            {
                query.StartTime = query.StartTime.Value.Date;
            }
            if (query.EndTime.HasValue)
            {
                query.EndTime = query.EndTime.Value.AddDays(1).Date;
            }
            if (query.Sort.ToLower() == "ShowApplyTime".ToLower())
            {
                query.Sort = "ApplyTime";
            }

            var results = DistributionApplication.GetDistributorsAll(query);
            if(results!=null && results.Count() > 0)
            {
                results[0].DistributionMaxLevel = SiteSettings.DistributionMaxLevel;//最大分销级数，这里实体第一个保存值是导出方便取值
            }

            return ExcelView("ExportDistributor", "平台销售员列表", results);
        }
        #endregion

        #region 详情
        public ActionResult Detail(long id)
        {
            var model = new DistributorDetailModel
            {
                MaxLevel = SiteSettingApplication.SiteSettings.DistributionMaxLevel
            };
            model.Distributor = DistributionApplication.GetDistributorDTO(id);
            if (model.Distributor == null || model.Distributor.DistributionStatus == (int)DistributorStatus.UnApply)
                throw new MallException("错误的分销员ID");

            if (model.Distributor.SuperiorId > 0)
                model.SuperiorDistributor = DistributionApplication.GetDistributorDTO(model.Distributor.SuperiorId);

            model.Achievement = DistributionApplication.GetAchievement(id);
            model.SiteSetting = SiteSettingApplication.SiteSettings;
            return View(model);
        }

        /// <summary>
        /// 获取分销商品
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetSubDistributorList(DistributorSubQuery query)
        {
            query.IsAll = true;
            var data = DistributionApplication.GetDistributors(query);
            var list = data.Models.Select(item =>
            new
            {
                memberName = item.Member==null? "":item.Member.UserName,
                totalCount = item.Achievement?.TotalCount ?? 0,
                totalAmount = item.Achievement?.TotalAmount ?? 0,
                regTime = item.Member == null ? "" : item.Member.CreateDate.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            return Json(new { rows = list, total = data.Total });
        }
        #endregion


        #region 业绩排行
        public ActionResult Ranking()
        {
            var batch = DistributionApplication.GetLastRankingBatch();
            return View(batch);
        }
        public ActionResult GenerateRanking(DateTime begin, DateTime end)
        {
            DistributionApplication.GenerateRankingAsync(begin, end);
            return Json(new { success = true }, true);
        }
        public ActionResult CheckGenerate()
        {
            //是否完成生成任务
            var completed = !DistributionApplication.CheckGenerating();
            return Json(new { success = completed }, true);
        }
        public ActionResult Ranking(DistributorRankingQuery query)
        {
            var data = DistributionApplication.GetRankings(query);
            var list = data.Models.Select(item =>
            new
            {
                Ranking = item.Rank,
                item.Distributor.MemberId,
                DistributorName = item.Member.UserName,
                item.Distributor.ShopName,
                item.Grade.GradeName,
                item.Quantity,
                item.Amount,
                item.Settlement,
                item.NoSettlement
            });
            return Json(new { rows = list, total = data.Total }, true);
        }
        #endregion
    }
}