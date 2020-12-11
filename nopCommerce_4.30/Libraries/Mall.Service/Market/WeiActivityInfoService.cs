using Mall.CommonModel;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Mall.Service
{
    public class ActivityInfoService : ServiceBase, IWeiActivityInfoService
    {
        public QueryPageModel<WeiActivityInfoInfo> Get(WeiActivityQuery query)
        {
            var db = DbFactory.Default.Get<WeiActivityInfoInfo>();

            if (!string.IsNullOrWhiteSpace(query.Name))
            {
                db.Where(p => p.ActivityTitle.Contains(query.Name));
            }
          
            if (query.IsIntegralActivity.HasValue)
            {
                if (query.IsIntegralActivity.Value)
                    db.Where(d => d.ConsumePoint > 0);
                else
                    db.Where(d => d.ConsumePoint == 0);
            }
            if (!query.IsShowAll)
                db.Where(d => d.BeginTime < DateTime.Now && d.EndTime > DateTime.Now);//取有效期内活动
            db.Where(p => p.ActivityType == (int)query.Type);
            db.Select();
            switch (query.Sort.ToLower()) {
                //case "totalnumber": break;
                //case "winnumber":
                    //var winNumber = DbFactory.Default.Get<WeiActivityWinInfoInfo>()
                    //    .Where(p => p.IsWin == true)
                    //    .GroupBy(p => p.UserId)
                    //    .Having<WeiActivityInfoInfo>((w, a) => w.ActivityId == a.Id)
                    //    .Select(p => p.ExCount(false)).Continue();
                    //db.Select(p => new { winNumber = winNumber.ExResolve<long>() });
                    //if (query.IsAsc) db.OrderBy(p => "winNumber");
                    //else db.OrderByDescending(p => "winNumber");
                    //break;
                default:
                    db.OrderByDescending(o => o.AddDate);
                    break;
            }

            var data = db.ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<WeiActivityInfoInfo>()
            {
                Models = data,
                Total = data.TotalRecordCount
            };
        }
        public WeiActivityInfoInfo GetActivityModel(long id, string url = "")
        {
            WeiActivityInfoInfo weiInfo = DbFactory.Default.Get<WeiActivityInfoInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(url))
            {
                Mall.Core.MallIO.DeleteFile(weiInfo.CodeUrl);
                weiInfo.CodeUrl = GenerateQR(url + weiInfo.Id);
                DbFactory.Default.Update(weiInfo);
            }
            return weiInfo;
        }

        public List<WeiActivityAwardInfo> GetActivityAwards(long activity) {
            return DbFactory.Default.Get<WeiActivityAwardInfo>().Where(p => p.ActivityId == activity).ToList();
        }
        public List<WeiActivityWinInfoInfo> GetActivityWin(long activity) {
            return DbFactory.Default.Get<WeiActivityWinInfoInfo>().Where(p => p.ActivityId == activity).ToList();
        }
        public long AddActivity(WeiActivityInfoInfo model , List<WeiActivityAwardInfo> awardInfos)
        {
            DbFactory.Default.InTransaction(() =>
            {
                if (model.Id <= 0)
                {
                    model.AddDate = DateTime.Now;
                    DbFactory.Default.Add(model);
                    awardInfos.ForEach(p => p.ActivityId = model.Id);
                    DbFactory.Default.Add<WeiActivityAwardInfo>(awardInfos);
                    model.CodeUrl = GenerateQR(model.CodeUrl + model.Id);
                    DbFactory.Default.Update(model);
                }
                else
                {
                    var info = DbFactory.Default.Get<WeiActivityInfoInfo>(a => a.Id == model.Id).FirstOrDefault();
                    info.ParticipationCount = model.ParticipationCount;
                    info.ParticipationType = model.ParticipationType;
                    info.ActivityDetails = model.ActivityDetails;
                    info.ActivityTitle = model.ActivityTitle;
                    info.ActivityType = model.ActivityType;
                    info.ActivityUrl = model.ActivityUrl;
                    info.ConsumePoint = model.ConsumePoint;
                    info.EndTime = model.EndTime;
                    DbFactory.Default.Update(info);
                }
            });

            return model.Id;
        }
        /// <summary>
        /// 生成二维码
        /// </summary>
        private string GenerateQR(string path)
        {
            Bitmap bi = Mall.Core.Helper.QRCodeHelper.Create(path);
            string fileName = Guid.NewGuid().ToString() + ".jpg";
            string fileFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage", "Plat", "WeiActivity");
            string fileFullPath = Path.Combine(fileFolderPath, fileName);
            if (!Directory.Exists(fileFolderPath))
            {
                Directory.CreateDirectory(fileFolderPath);
            }
            bi.Save(fileFullPath);

            return "/Storage/Plat/WeiActivity/" + fileName;
        }
        public void DeleteActivity(long id)
        {
            //删除主表信息
            var ociobj = DbFactory.Default.Get<WeiActivityInfoInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (ociobj != null)
            {
                DbFactory.Default.InTransaction(() =>
                {
                    //删除相关信息
                    var procoms = DbFactory.Default.Del<WeiActivityAwardInfo>(d => ociobj.Id == d.ActivityId);

                    var procom = DbFactory.Default.Del<WeiActivityWinInfoInfo>(d => ociobj.Id == d.ActivityId);

                    //删除微信活动主表
                    DbFactory.Default.Delete(ociobj);
                });
            }
        }


    }
}
