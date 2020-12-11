using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.IO;

namespace Mall.Service
{
    /// <summary>
    /// 积分礼品
    /// </summary>
    public partial class GiftService : ServiceBase, IGiftService
    {
        /// <summary>
        /// 添加礼品
        /// </summary>
        public void AddGift(GiftInfo model)
        {
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Add(model);

                //保存图片地址
                if (string.IsNullOrWhiteSpace(model.ImagePath))
                    model.ImagePath = string.Format(@"/Storage/Gift/{0}", model.Id);

                model.Description = HTMLProcess(model.Description, model.ImagePath);
                DbFactory.Default.Update(model);
            });
        }
        /// <summary>
        /// 修改礼品
        /// </summary>
        /// <param name="model"></param>
        public void UpdateGift(GiftInfo model)
        {
            var entity = DbFactory.Default.Get<GiftInfo>().Where(n => n.Id == model.Id).Exist();
            if (!entity) throw new MallException("未找到id为" + model.Id + "的礼品");
            //保存图片地址
            if (string.IsNullOrWhiteSpace(model.ImagePath))
                model.ImagePath = string.Format(@"/Storage/Gift/{0}", model.Id);

            model.Description = HTMLProcess(model.Description, model.ImagePath);
            DbFactory.Default.Update(model);
        }
        /// <summary>
        /// 调整排序
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sequence"></param>
        public void UpdateSequence(long id, int sequence)
        {
            var flag = DbFactory.Default.Set<GiftInfo>().Set(n => n.Sequence, sequence).Where(p => p.Id == id).Succeed();
            if (!flag) throw new MallException("未找到id为" + id + "的礼品");
        }
        /// <summary>
        /// 调整排序
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sequence"></param>
        public void ChangeStatus(long id, bool status)
        {
            var gift = DbFactory.Default.Get<GiftInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (gift == null)
                throw new MallException("未找到id为" + id + "的礼品");
            if (status)
            {
                gift.SalesStatus = GiftInfo.GiftSalesStatus.Normal;
                //已过期礼品增加一个月兑换截止时间
                if (gift.EndDate.Date < DateTime.Now.Date)
                {
                    gift.EndDate = DateTime.Now.AddMonths(1);
                }
            }
            else
            {
                gift.SalesStatus = GiftInfo.GiftSalesStatus.OffShelves;
            }
            DbFactory.Default.Update(gift);
        }
        /// <summary>
        /// 获取礼品
        /// </summary>
        /// <param name="id"></param>
        public GiftInfo GetById(long id)
        {
            GiftInfo result = null;
            result = DbFactory.Default.Get<GiftInfo>().Where(d => d.Id == id).FirstOrDefault();
            if (result != null)
            {
                if (result.NeedGrade != 0)
                {
                    var grade = DbFactory.Default.Get<MemberGradeInfo>().Where(d => d.Id == result.NeedGrade).FirstOrDefault();
                    if (grade != null)
                    {
                        result.GradeIntegral = grade.Integral;
                        result.NeedGradeName = grade.GradeName;
                    }
                    else
                    {
                        result.GradeIntegral = -1;
                        result.NeedGradeName = "等级已删除";
                    }
                }
                else
                {
                    result.GradeIntegral = 0;
                    result.NeedGradeName = "不限等级";
                }
            }
            return result;
        }
        /// <summary>
        /// 获取礼品
        /// <para>无追踪实体，UpdateGift前调用</para>
        /// </summary>
        /// <param name="id"></param>
        public GiftInfo GetByIdAsNoTracking(long id)
        {
            var result = DbFactory.Default.Get<GiftInfo>().Where(d => d.Id == id).FirstOrDefault();
            return result;
        }
        /// <summary>
        /// 查询礼品
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<GiftModel> GetGifts(GiftQuery query)
        {


            var db = DbFactory.Default.Get<GiftInfo>()
                .LeftJoin<MemberGradeInfo>((gi, mgi) => gi.NeedGrade == mgi.Id)
                .Select(d => new
                {
                    Id = d.Id,
                    GiftName = d.GiftName,
                    NeedIntegral = d.NeedIntegral,
                    LimtQuantity = d.LimtQuantity,
                    StockQuantity = d.StockQuantity,
                    EndDate = d.EndDate,
                    NeedGrade = d.NeedGrade,
                    VirtualSales = d.VirtualSales,
                    RealSales = d.RealSales,
                    SalesStatus = d.SalesStatus,
                    ImagePath = d.ImagePath,
                    Sequence = d.Sequence,
                    GiftValue = d.GiftValue,
                    AddDate = d.AddDate
                })
                .Select<MemberGradeInfo>(d => new
                {
                    GradeIntegral = d.Integral.ExIfNull(0),
                    NeedGradeName = d.GradeName.ExIfNull("不限等级")
                });

            if (!string.IsNullOrWhiteSpace(query.skey))
            {
                db.Where(d => d.GiftName.Contains(query.skey));
            }

            if (query.status != null)
            {
                DateTime CurDay = DateTime.Now;
                DateTime CurAddOneDay = CurDay.AddDays(1).Date;
                switch (query.status)
                {
                    case GiftInfo.GiftSalesStatus.HasExpired:
                        db.Where(d => d.SalesStatus == GiftInfo.GiftSalesStatus.Normal && d.EndDate < CurDay);
                        break;
                    case GiftInfo.GiftSalesStatus.Normal:
                        db.Where(d => d.SalesStatus == GiftInfo.GiftSalesStatus.Normal && d.EndDate >= CurDay);
                        break;
                    default:
                        db.Where(d => d.SalesStatus == query.status);
                        break;
                }
            }

            if (query.isShowAll != true)
            {
                db.Where(d => d.SalesStatus != GiftInfo.GiftSalesStatus.IsDelete);
            }

            //排序
            switch (query.Sort.ToLower())
            {
                case "salesnumber":
                    if (query.IsAsc) db.OrderBy(d => d.RealSales).OrderByDescending(d => d.Id);
                    else db.OrderByDescending(d => d.RealSales).OrderByDescending(d => d.Id);
                    break;
                case "realsalesnumber":
                    if (query.IsAsc) db.OrderBy(d => d.RealSales).OrderByDescending(d => d.Id);
                    else db.OrderByDescending(d => d.RealSales).OrderByDescending(d => d.Id);
                    break;
                case "adddate":
                    if (query.IsAsc) db.OrderBy(d => d.AddDate);
                    else db.OrderByDescending(d => d.AddDate);
                    break;
                case "enddate":
                    if (query.IsAsc) db.OrderBy(d => d.EndDate);
                    else db.OrderByDescending(d => d.EndDate);
                    break;
                case "needintegral":
                    if (query.IsAsc) db.OrderBy(d => d.NeedIntegral);
                    else db.OrderByDescending(d => d.NeedIntegral);
                    break;
                case "showlimtquantity":
                    if (query.IsAsc) db.OrderBy(d => d.LimtQuantity);
                    else db.OrderByDescending(d => d.LimtQuantity);
                    break;
                case "stockquantity":
                    if (query.IsAsc) db.OrderBy(d => d.StockQuantity);
                    else db.OrderByDescending(d => d.StockQuantity);
                    break;
                case "realsales":
                    if (query.IsAsc) db.OrderBy(d => d.RealSales);
                    else db.OrderByDescending(d => d.RealSales);
                    break;
                default:
                    db.OrderBy(d => d.Sequence).OrderByDescending(d => d.Id);
                    break;
            }

            var result = db.ToPagedList<GiftModel>(query.PageNo, query.PageSize);
            return new QueryPageModel<GiftModel>
            {
                Models = result,
                Total = result.TotalRecordCount
            };
        }

        #region 广告配置
        /// <summary>
        /// 获取广告配置
        /// </summary>
        /// <param name="adtype">活动类型</param>
        /// <param name="adplatform">显示平台</param>
        /// <returns></returns>
        public IntegralMallAdInfo GetAdInfo(IntegralMallAdInfo.AdActivityType adtype, IntegralMallAdInfo.AdShowPlatform adplatform)
        {
            int sadtype = adtype.GetHashCode();
            int sadplatform = adplatform.GetHashCode();
            int sstatus = IntegralMallAdInfo.AdShowStatus.Show.GetHashCode();
            IntegralMallAdInfo result = DbFactory.Default.Get<IntegralMallAdInfo>().Where(d => d.ActivityType == sadtype && d.ShowPlatform == sadplatform && d.ShowStatus == sstatus).FirstOrDefault();
            if (result != null)
            {
                var nowdate = DateTime.Now;
                var isExist = DbFactory.Default.Get<WeiActivityInfoInfo>().Where(d => d.Id == result.ActivityId && d.EndTime > nowdate && d.BeginTime <= nowdate).Exist();
                if (!isExist)
                {
                    return null;
                }
            }
            return result;
        }
        /// <summary>
        /// 更新广告信息
        /// </summary>
        /// <param name="ActivityType">活动类型</param>
        /// <param name="ActivityId">活动编号</param>
        /// <param name="Cover">广告图片</param>
        /// <param name="ShowStatus">显示状态</param>
        /// <param name="ShowPlatform">显示平台</param>
        /// <returns></returns>
        public IntegralMallAdInfo UpdateAdInfo(IntegralMallAdInfo.AdActivityType ActivityType, long ActivityId, string Cover, IntegralMallAdInfo.AdShowStatus? ShowStatus, IntegralMallAdInfo.AdShowPlatform? ShowPlatform)
        {
            int adtype = ActivityType.GetHashCode();
            var data = DbFactory.Default.Get<IntegralMallAdInfo>().Where(d => d.ActivityType == adtype).FirstOrDefault();
            if (data == null)
            {
                data = new IntegralMallAdInfo();
                data.ShowAdStatus = IntegralMallAdInfo.AdShowStatus.Show;
            }
            else
            {
                if (data.ActivityId == ActivityId)
                {
                    if (data.ShowAdStatus == IntegralMallAdInfo.AdShowStatus.Hide)
                    {
                        data.ShowAdStatus = IntegralMallAdInfo.AdShowStatus.Show;
                    }
                    else
                    {
                        data.ShowAdStatus = IntegralMallAdInfo.AdShowStatus.Hide;
                    }
                }
                else
                {
                    data.ShowAdStatus = IntegralMallAdInfo.AdShowStatus.Show;
                }
            }
            data.ActivityType = adtype;
            data.ActivityId = ActivityId;
            data.Cover = Cover;
            data.ShowAdPlatform = ShowPlatform;
            DbFactory.Default.Save(data);
            return data;
        }
        #endregion


        /// <summary>
        /// 转移外站图片，去除script脚本
        /// </summary>
        /// <param name="content">html内容</param>
        /// <param name="id"></param>
        /// <returns></returns>
        private string HTMLProcess(string content, string path)
        {
            var details = Path.Combine(path, "Details").Replace("\\", "/");
            var rename = Path.Combine(path, "Temp").Replace("\\", "/");
            var urlTemp = Core.Helper.IOHelper.GetMapPath(rename);
            try
            {

                //TD需要修改
                string imageRealtivePath = details;
                content = Core.Helper.HtmlContentHelper.TransferToLocalImage(content, "/", imageRealtivePath, Core.MallIO.GetImagePath(imageRealtivePath) + "/");
                content = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(content);
            }
            catch
            {

            }
            return content;
        }
    }
}
