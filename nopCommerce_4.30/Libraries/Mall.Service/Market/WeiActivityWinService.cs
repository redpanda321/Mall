using Mall.CommonModel;
using Mall.DTO;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System.Collections.Generic;

namespace Mall.Service
{
    public class WeiActivityWinService : ServiceBase, IWeiActivityWinService
    {
        public QueryPageModel<WeiActivityWinModel> Get(string text, long id, int pageIndex, int pageSize)
        {

            var query = DbFactory.Default.Get<WeiActivityWinInfoInfo>().LeftJoin<WeiActivityAwardInfo>((wwi, wai) => wwi.AwardId == wai.Id).LeftJoin<MemberInfo>((wwi, mi) => wwi.UserId == mi.Id);

            if (text == "1")
            {
                query.Where(p => p.IsWin == true);
            }
            else if (text == "0")
            {
                query.Where(p => p.IsWin == false);
            }
            query.Where(p => p.ActivityId == id);

            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            query.Select(p => new { userId = p.UserId, activityId = p.ActivityId, awardId = p.AwardId, isWin = p.IsWin, awardName = p.AwardName, addDate = p.AddDate })
                .Select<MemberInfo>(p=>new { userName = p.UserName }).Select<WeiActivityAwardInfo>(p => p.AwardLevel);
            var datas = query.OrderByDescending(o => o.AddDate).ToPagedList<WeiActivityWinModel>(pageIndex, pageSize);
            QueryPageModel<WeiActivityWinModel> pageModel = new QueryPageModel<WeiActivityWinModel>()
            {
                Models = datas,
                Total = datas.TotalRecordCount
            };
            return pageModel;
        }



        public void AddWinner(Entities.WeiActivityWinInfoInfo info)
        {
            var awardinfo = DbFactory.Default.Get<Entities.WeiActivityAwardInfo>().Where(t => t.Id == info.AwardId).FirstOrDefault();
            if (awardinfo != null && awardinfo.BonusId > 0)
            {
                var bonu = DbFactory.Default.Get<Entities.BonusInfo>().Where(t => t.Id == awardinfo.BonusId).FirstOrDefault();
                if (bonu != null)
                    info.AwardName = bonu.Name;
            }
            DbFactory.Default.Add(info);
        }

        public string GetWinNumber(long id, string text)
        {
            var query = DbFactory.Default.Get<Entities.WeiActivityWinInfoInfo>().Where(p => p.ActivityId == id);
            if (text == "True")
            {
                query.Where(p => p.IsWin == true);
            }
            else if (text == "False")
            {
                query.Where(p => p.IsWin == false);
            }
            return query.GroupBy(x => x.UserId).Count().ToString();
        }

        public List<Entities.WeiActivityWinInfoInfo> GetWinInfo(long userId)
        {
            var query = DbFactory.Default.Get<Entities.WeiActivityWinInfoInfo>().Where(p => p.UserId == userId);
            return query.ToList();
        }
    }
}
