using Mall.IServices;
using Mall.DTO.QueryModel;
using System;
using Mall.CommonModel;
using Mall.DTO;
using Mall.Entities;
using NetRube.Data;
using System.Linq;

namespace Mall.Service
{
    public class MemberInviteService : ServiceBase, IMemberInviteService
    {
        public void SetInviteRule(InviteRuleInfo model)
        {
            var m = DbFactory.Default.Get<InviteRuleInfo>().Where(a => a.Id == model.Id).FirstOrDefault();
            if (m == null)
            {
                DbFactory.Default.Add(model);
            }
            else
            {
                m.InviteIntegral = model.InviteIntegral;
                m.RegIntegral = model.RegIntegral;
                m.ShareDesc = model.ShareDesc;
                m.ShareIcon = model.ShareIcon;
                m.ShareRule = model.ShareRule;
                m.ShareTitle = model.ShareTitle;
                DbFactory.Default.Update(m);
            }
            if (m != null)
            {
                //转移图片
                m.ShareIcon = MoveImages(model.ShareIcon);
                DbFactory.Default.Update(m);
            }
            else
            {
                model.ShareIcon = MoveImages(model.ShareIcon);
                DbFactory.Default.Update(model);
            }
        }

        string MoveImages(string image)
        {
            if (string.IsNullOrWhiteSpace(image))
            {
                return "";
            }

            var ext = image.Substring(image.LastIndexOf("."));
            //转移图片
            string relativeDir = "/Storage/Plat/MemberInvite/";
            string fileName = "Invite_Icon" + ext;
            if (image.Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                string temp = image.Substring(image.LastIndexOf("/temp"));
                Core.MallIO.CopyFile(temp, relativeDir + fileName, true);
                return relativeDir + fileName;
            }  //目标地址
            else
            {
                var fname = image.Substring(image.LastIndexOf("/") + 1);
                return relativeDir + fname;
            }
        }

        public InviteRuleInfo GetInviteRule()
        {
            var inviteRule = DbFactory.Default.Get<InviteRuleInfo>().FirstOrDefault();
            if (inviteRule == null)
            {
                InviteRuleInfo ruleInfo = new InviteRuleInfo();
                ruleInfo.InviteIntegral = 0;
                ruleInfo.RegIntegral = 0;
                ruleInfo.ShareDesc = "分享描述";
                ruleInfo.ShareRule = "分享规则";
                ruleInfo.ShareTitle = "分享标题";
                return ruleInfo;
            }
            inviteRule.ShareIcon = Core.MallIO.GetRomoteImagePath(inviteRule.ShareIcon);
            return inviteRule;
        }


        public bool HasInviteIntegralRecord(long RegId)
        {
            return DbFactory.Default.Get<InviteRecordInfo>().Where(a => a.RegUserId == RegId).Exist();
        }

        public QueryPageModel<InviteRecordInfo> GetInviteList(InviteRecordQuery query)
        {
            var datasql = DbFactory.Default.Get<InviteRecordInfo>();
            if (!string.IsNullOrEmpty(query.userName))
            {
                datasql.Where(d => d.UserName.Contains(query.userName));
            }
            var model = datasql.OrderByDescending(a => a.Id).ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<InviteRecordInfo>
            {
                Models = model,
                Total = model.TotalRecordCount
            };
        }

        public UserInviteModel GetMemberInviteInfo(long userId)
        {
            var data = DbFactory.Default.Get<InviteRecordInfo>()
                .Where(a => a.UserId == userId)
                .Select(p => new { Item1 = p.ExCount(false), Item2 = p.InviteIntegral.ExSum() })
                .FirstOrDefault<SimpItem<int, int>>();
            return new UserInviteModel
            {
                InvitePersonCount = data.Item1,
                InviteIntergralCount = data.Item2,
            };
        }


        public void AddInviteRecord(InviteRecordInfo info)
        {
            DbFactory.Default.Add(info);
        }

        public void AddInviteIntegel(MemberInfo RegMember, MemberInfo InviteMember,bool hasEmailOrPhone = false)
        {
            var InviteRule = GetInviteRule();
            if (InviteRule == null)
            {
                return;
            }

            if (!HasInviteIntegralRecord(RegMember.Id)) //没有过邀请得分，加积分
            {
                var factoryService = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create;
                var integralService = ServiceProvider.Instance<IMemberIntegralService>.Create;
                var info = new MemberIntegralRecordInfo();
                info.UserName = RegMember.UserName;
                info.MemberId = RegMember.Id;
                info.RecordDate = DateTime.Now;
                info.ReMark = "被邀请注册";
                info.TypeId = MemberIntegralInfo.IntegralType.Others;
                var memberIntegral = factoryService.Create(MemberIntegralInfo.IntegralType.Others, InviteRule.RegIntegral);
                integralService.AddMemberIntegral(info, memberIntegral);

                if (hasEmailOrPhone)
                {
                    var info2 = new MemberIntegralRecordInfo();
                    info2.UserName = InviteMember.UserName;
                    info2.MemberId = InviteMember.Id;
                    info2.RecordDate = DateTime.Now;
                    info2.ReMark = "邀请会员";
                    info2.TypeId = MemberIntegralInfo.IntegralType.InvitationMemberRegiste;
                    var memberIntegral2 = factoryService.Create(MemberIntegralInfo.IntegralType.InvitationMemberRegiste);
                    integralService.AddMemberIntegral(info2, memberIntegral2);

                    var inviteInfo = new InviteRecordInfo();
                    inviteInfo.RegIntegral = InviteRule.RegIntegral;
                    inviteInfo.InviteIntegral = InviteRule.InviteIntegral;
                    inviteInfo.RecordTime = DateTime.Now;
                    inviteInfo.RegTime = RegMember.CreateDate;
                    inviteInfo.RegUserId = RegMember.Id;
                    inviteInfo.RegName = RegMember.UserName;
                    inviteInfo.UserId = InviteMember.Id;
                    inviteInfo.UserName = InviteMember.UserName;
                    AddInviteRecord(inviteInfo);
                }
            }
        }
    }
}

