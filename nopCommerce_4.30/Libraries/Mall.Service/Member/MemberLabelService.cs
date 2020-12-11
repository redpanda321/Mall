using System.Linq;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.CommonModel;
using NetRube.Data;

namespace Mall.Service
{
    public class MemberLabelService : ServiceBase, IMemberLabelService
    {

        public void AddLabel(LabelInfo model)
        {
            //Context.LabelInfo.Add(model);
            //Context.SaveChanges();
            DbFactory.Default.Add(model);
        }

        public void DeleteLabel(LabelInfo model)
        {
            //var label = Context.LabelInfo.FirstOrDefault(e => e.Id == model.Id);
            //Context.LabelInfo.Remove(label);
            //Context.SaveChanges();
            DbFactory.Default.Del<LabelInfo>(e => e.Id == model.Id);
        }

        public void UpdateLabel(LabelInfo model)
        {
            //var label = Context.LabelInfo.FirstOrDefault(e => e.Id == model.Id);
            //label.LabelName = model.LabelName;
            //Context.SaveChanges();
            DbFactory.Default.Set<LabelInfo>().Set(n => n.LabelName, model.LabelName).Where(e => e.Id == model.Id).Succeed();
        }

        public QueryPageModel<LabelInfo> GetMemberLabelList(LabelQuery model)
        {
            //var result = Context.LabelInfo.AsQueryable();
            var result = DbFactory.Default.Get<LabelInfo>();
            if (!string.IsNullOrWhiteSpace(model.LabelName))
            {
                result.Where(item => item.LabelName.Contains(model.LabelName));
            }
            if (model.LabelIds != null)
            {
                //result = result.Where(item => model.LabelIds.Contains(item.Id));
                result.Where(item => item.Id.ExIn(model.LabelIds));
            }
            //int total = 0;
            if (model.PageNo > 0 && model.PageSize > 0)
            {
                //result = result.GetPage(d => d.OrderByDescending(o => o.Id), out total, model.PageNo, model.PageSize);
                var models = result.OrderByDescending(o => o.Id).ToPagedList(model.PageNo, model.PageSize);
                QueryPageModel<LabelInfo> pageModel = new QueryPageModel<LabelInfo> { Models = models, Total = models.TotalRecordCount };
                return pageModel;
            }
            else
            {
                QueryPageModel<LabelInfo> pageModel = new QueryPageModel<LabelInfo> { Models = result.ToList(), Total = result.Count() };
                return pageModel;
            }
        }


        public LabelInfo GetLabel(long id)
        {
            //return Context.LabelInfo.FirstOrDefault(item => item.Id == id);
            return DbFactory.Default.Get<LabelInfo>().Where(item => item.Id == id).FirstOrDefault();
        }

        public bool CheckNameIsExist(string name)
        {
            //return Context.LabelInfo.Any(e => e.LabelName == name);
            return DbFactory.Default.Get<LabelInfo>().Where(e => e.LabelName == name).Exist();
        }
    }
}
