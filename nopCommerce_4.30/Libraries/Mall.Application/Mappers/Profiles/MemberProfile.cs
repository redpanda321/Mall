using AutoMapper;
using Mall.CommonModel;
using Mall.Entities;

namespace Mall.Application.Mappers.Profiles
{
    public class MemberProfile:Profile
	{
	public MemberProfile()
		{
			

            //CreateMap<MemberGrade, MemberGrade>();
            //CreateMap<MemberGrade, Mall.Model.MemberGrade>();

            CreateMap<Entities.MemberInfo, DTO.Members>();
			CreateMap<DTO.Members, Entities.MemberInfo>();
            CreateMap<QueryPageModel<Mall.Entities.MemberInfo>, QueryPageModel<Mall.DTO.Members>>();


            //CreateMap<Mall.Model.LabelInfo, Mall.DTO.Labels>();
            //CreateMap<Mall.DTO.Labels, Mall.Model.LabelInfo>();


            //  CreateMap<Model.MemberConsumeStatisticsInfo, DTO.MemberConsumeStatistics>();
            //  CreateMap<DTO.MemberConsumeStatistics, Model.MemberConsumeStatisticsInfo>();

            CreateMap<Mall.Entities.MemberInfo, Mall.DTO.MemberPurchasingPower>();
            CreateMap<Entities.MemberOpenIdInfo, DTO.MemberOpenId>();
			CreateMap<DTO.MemberOpenId, Entities.MemberOpenIdInfo>();

			CreateMap<Entities.ChargeDetailInfo, DTO.ChargeDetail>();
			CreateMap<DTO.ChargeDetail, Entities.ChargeDetailInfo>();

            CreateMap<SendMessageRecordInfo, DTO.SendMessageRecord>();
            CreateMap<DTO.SendMessageRecord, SendMessageRecordInfo>();
		}
	}
}
