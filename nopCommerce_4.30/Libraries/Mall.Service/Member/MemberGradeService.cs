using Mall.Core;
using Mall.IServices;
using System.Collections.Generic;
using System.Linq;
using Mall.Entities;
using NetRube.Data;

namespace Mall.Service
{
    public class MemberGradeService : ServiceBase, IMemberGradeService
    {
        public void AddMemberGrade(MemberGradeInfo model)
        {
            if (model.Integral < 0)
                throw new MallException("积分不能为负数");

            if (DbFactory.Default.Get<MemberGradeInfo>().Where(a => a.Integral == model.Integral).Exist())
                throw new MallException("等级之间的积分不能相同");

            if (DbFactory.Default.Get<MemberGradeInfo>().Where(a => a.GradeName.ToLower() == model.GradeName.ToLower()).Exist())
                throw new MallException("已存在相同名称的等级");

            DbFactory.Default.Add(model);
        }

        public void UpdateMemberGrade(MemberGradeInfo model)
        {
            if (DbFactory.Default.Get<MemberGradeInfo>().Where(a => a.Integral == model.Integral && a.Id != model.Id).Exist())
                throw new MallException("等级之间的积分不能相同");

            if (DbFactory.Default.Get<MemberGradeInfo>().Where(a => a.Id != model.Id && a.GradeName.ToLower() == model.GradeName.ToLower()).Exist())
                throw new MallException("已存在相同名称的等级");
          
            DbFactory.Default.Update(model);
        }

        public void DeleteMemberGrade(long id)
        {
            if (DbFactory.Default.Get<GiftInfo>().Where(d => d.NeedGrade == id).Exist())
                throw new MallException("有礼品兑换与等级关联，不可删除！");
            
            DbFactory.Default.Delete<MemberGradeInfo>(id);
        }


        public MemberGradeInfo GetMemberGrade(long id)
        {
            return DbFactory.Default.Get<MemberGradeInfo>().Where(p => p.Id == id).FirstOrDefault();
        }

        public List<MemberGradeInfo> GetMemberGradeList()
        {
            List<MemberGradeInfo> result = DbFactory.Default.Get<MemberGradeInfo>().ToList();
            //补充积分礼品信息
            var gifts = DbFactory.Default.Get<GiftInfo>().GroupBy(d => d.NeedGrade).Select(d => new { cnt = d.ExCount(true), grid = d.NeedGrade }).ToList<dynamic>();
            foreach (var item in result)
            {
                if (gifts.Where(d => d.grid == item.Id && d.cnt > 0).Count() > 0)
                {
                    item.IsNoDelete = true;
                }
            }
            return result;
        }
    }
}
