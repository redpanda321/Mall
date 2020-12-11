using Mall.CommonModel;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System.Collections.Generic;

namespace Mall.Service
{
    public class SensitiveWordService : ServiceBase, ISensitiveWordService
    {
        public QueryPageModel<SensitiveWordInfo> GetSensitiveWords(SensitiveWordQuery query)
        {
            var Infos = DbFactory.Default.Get<SensitiveWordInfo>();
            if (!string.IsNullOrWhiteSpace(query.SensitiveWord))
            {
                Infos.Where(item => item.SensitiveWord.Contains(query.SensitiveWord));
            }
            if (!string.IsNullOrWhiteSpace(query.CategoryName))
            {
                Infos.Where(item => item.CategoryName.Contains(query.CategoryName));
            }
            var rets = Infos.OrderByDescending(o => o.Id).ToPagedList(query.PageNo, query.PageSize);

            QueryPageModel<SensitiveWordInfo> pageModel = new QueryPageModel<SensitiveWordInfo>() { Models = rets, Total = rets.TotalRecordCount };
            return pageModel;
        }

        public List<string> GetCategories()
        {
            var categories = DbFactory.Default.Get<SensitiveWordInfo>().Select(item => item.CategoryName).Distinct().ToList<string>();
            return categories;
        }

        public SensitiveWordInfo GetSensitiveWord(int id)
        {
            return DbFactory.Default.Get<SensitiveWordInfo>().Where(p => p.Id == id).FirstOrDefault();
        }

        public void AddSensitiveWord(SensitiveWordInfo model)
        {
            DbFactory.Default.Add(model);
        }

        public void UpdateSensitiveWord(SensitiveWordInfo model)
        {
            SensitiveWordInfo item = GetSensitiveWord(model.Id);
            item.SensitiveWord = model.SensitiveWord;
            item.CategoryName = model.CategoryName;
            DbFactory.Default.Update(item);
        }

        public void DeleteSensitiveWord(int id)
        {
            DbFactory.Default.Del<SensitiveWordInfo>().Where(p => p.Id == id).Succeed();
        }

        public void BatchDeleteSensitiveWord(int[] ids)
        {
            DbFactory.Default.Del<SensitiveWordInfo>().Where(item => item.Id.ExIn(ids)).Succeed();
        }

        public bool ExistSensitiveWord(string word)
        {
            return DbFactory.Default.Get<SensitiveWordInfo>().Where(item => item.SensitiveWord == word.Trim()).Exist();
            //if (data.Count() > 0)
            //    return true;
            //else
            //    return false;
        }
    }
}
