using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Application
{
    public class WeiActivityApplication:BaseApplicaion<IWeiActivityInfoService>
    {
        public static long AddActivitySubmit(WeiActivityModel model)
        {
            var weiInfo = new WeiActivityInfoInfo();
            if (model.Id > 0)
            {
                weiInfo.Id = model.Id;
            }
            weiInfo.ActivityTitle = model.activityTitle;
            weiInfo.ActivityType = (int)model.activityType;
            weiInfo.ActivityDetails = model.activityDetails;
            weiInfo.ActivityUrl = TransferImage(model.activityUrl);
            weiInfo.BeginTime = model.beginTime;
            weiInfo.EndTime = model.endTime;
            weiInfo.ParticipationType = (int)model.participationType;
            weiInfo.ParticipationCount = model.participationCount;
            weiInfo.ConsumePoint = model.consumePoint;
            weiInfo.CodeUrl = model.codeUrl;

            var awardInfo = model.awards.Select(item => new WeiActivityAwardInfo
            {
                ActivityId = item.activityId,
                AwardCount = item.awardCount,
                AwardLevel = item.awardLevel,
                AwardType = (int)item.awardType,
                BonusId = item.bonusId,
                CouponId = item.couponId,
                Integral = item.integral,
                Proportion = item.proportion
            }).ToList();
            return Service.AddActivity(weiInfo, awardInfo);
        }

        private static string TransferImage(string sourceFile)
        {
            if (!string.IsNullOrWhiteSpace(sourceFile) && !sourceFile.Contains("/Storage/Plat/"))
            {
                string newDir = "/Storage/Plat/WeiActivity/";

                string ext = sourceFile.Substring(sourceFile.LastIndexOf('.'));//得到扩展名
                string newName = "WeiActivity_" + System.DateTime.Now.ToString("yyyyMMddHHmmssffffff") + ext;//得到新的文件名

                if (!string.IsNullOrWhiteSpace(sourceFile))
                {
                    if (sourceFile.Contains("/temp/"))
                    {
                        string logoname = sourceFile.Substring(sourceFile.LastIndexOf('/') + 1);
                        string oldlogo = sourceFile.Substring(sourceFile.LastIndexOf("/temp"));
                        string newLogo = newDir + newName;
                        Core.MallIO.CopyFile(oldlogo, newLogo, true);
                        sourceFile = newLogo;
                        return sourceFile;//返回新的文件路径
                    }
                    else if (sourceFile.Contains("/Storage/"))
                    {
                        sourceFile = sourceFile.Substring(sourceFile.LastIndexOf("/Storage"));
                    }
                }
            }
            else if (sourceFile.Contains("/Storage/"))
            {
                sourceFile = sourceFile.Substring(sourceFile.LastIndexOf("/Storage"));
            }

            return sourceFile;
        }



        public static QueryPageModel<WeiActivityModel> Get(WeiActivityQuery query)
        {
            QueryPageModel<Entities.WeiActivityInfoInfo> weiInfo = Service.Get(query);
            var datas = weiInfo.Models.ToList().Select(m => new WeiActivityModel()
            {
                Id = m.Id,
                activityTitle = m.ActivityTitle,
                activityType = (WeiActivityType)m.ActivityType,
                activityDetails = m.ActivityDetails,
                activityUrl = MallIO.GetImagePath(m.ActivityUrl),
                beginTime = m.BeginTime,
                endTime = m.EndTime,
                participationType = (WeiParticipateType)m.ParticipationType,
                participationCount = Convert.ToInt32(m.ParticipationCount),
                consumePoint = m.ConsumePoint,
                codeUrl = m.CodeUrl,
                addDate = m.AddDate,
                totalNumber = GetWinNumber(m.Id, "ALL"),
                winNumber = GetWinNumber(m.Id, "True")
            }).ToList();

            QueryPageModel<WeiActivityModel> t = new QueryPageModel<WeiActivityModel>();
            t.Models = datas;
            t.Total = weiInfo.Total;
            return t;

        }


        public static void DeleteActivity(long id)
        {
            Service.DeleteActivity(id);
        }

        public static WeiActivityModel GetActivityModel(long id, string url = "")
        {
            var model = Service.GetActivityModel(id, url);

            var viewModel = new WeiActivityModel
            {
                Id = model.Id,
                activityTitle = model.ActivityTitle,
                activityType = (WeiActivityType)model.ActivityType,
                activityDetails = model.ActivityDetails,
                activityUrl = MallIO.GetImagePath(model.ActivityUrl),
                beginTime = model.BeginTime,
                endTime = model.EndTime,
                participationType = (WeiParticipateType)model.ParticipationType,
                participationCount = Convert.ToInt32(model.ParticipationCount),
                consumePoint = model.ConsumePoint,
                codeUrl = model.CodeUrl,
                addDate = model.AddDate
            };
            var awards = Service.GetActivityAwards(model.Id);
            viewModel.awards = awards.Select(item => new WeiActivityAwardModel
            {
                Id = item.Id,
                activityId = item.ActivityId,
                awardCount = item.AwardCount,
                awardLevel = item.AwardLevel,
                awardType = (WeiActivityAwardType)item.AwardType,
                bonusId = item.BonusId,
                couponId = item.CouponId,
                integral = item.Integral,
                proportion = item.Proportion
            }).ToList();
            return viewModel;
        }

        public static WeiActivityModel GetActivityModelByBigWheel(long id)
        {
            Entities.WeiActivityInfoInfo model = Service.GetActivityModel(id);

            WeiActivityModel viewModel = new WeiActivityModel();
            viewModel.Id = model.Id;
            viewModel.activityTitle = model.ActivityTitle;
            viewModel.activityType = (WeiActivityType)model.ActivityType;
            viewModel.activityDetails = model.ActivityDetails;
            viewModel.activityUrl = model.ActivityUrl;
            viewModel.beginTime = model.BeginTime;
            viewModel.endTime = model.EndTime;
            viewModel.participationType = (WeiParticipateType)model.ParticipationType;
            viewModel.participationCount = Convert.ToInt32(model.ParticipationCount);
            viewModel.consumePoint = model.ConsumePoint;
            viewModel.codeUrl = model.CodeUrl;
            viewModel.addDate = model.AddDate;

            List<WeiActivityAwardModel> listAwardModel = new List<WeiActivityAwardModel>();
            var items = Service.GetActivityAwards(model.Id);
            int awardNum = 0;//获取奖等序号
            int falg = 9 - items.Count();
            for (int i = 0; i < 9; i++)//创建9宫格实体
            {
                //放空值 ，未中奖
                if (i % 2 == 0)
                {
                    //最高奖等6 ，4以内奖等能均匀分布
                    if (items.Count() <= 4)
                    {
                        //获取奖项
                        WeiActivityAwardModel awardModel = new WeiActivityAwardModel();
                        listAwardModel.Add(awardModel);
                    }
                    else //大于4组奖项  未中奖显示不能均匀分布 
                    {
                        if (falg > 0)
                        {
                            //获取奖项
                            WeiActivityAwardModel awardModel = new WeiActivityAwardModel();
                            listAwardModel.Add(awardModel);
                            falg--;

                        }
                        else
                        {
                            //获取奖项
                            WeiActivityAwardModel awardModel = new WeiActivityAwardModel();
                            awardModel.Id = items[awardNum].Id;
                            awardModel.activityId = items[awardNum].ActivityId;
                            awardModel.awardCount = items[awardNum].AwardCount;
                            awardModel.awardLevel = items[awardNum].AwardLevel;
                            awardModel.awardType = (WeiActivityAwardType)items[awardNum].AwardType;
                            awardModel.bonusId =  items[awardNum].BonusId;
                            awardModel.couponId = items[awardNum].CouponId;
                            awardModel.integral = items[awardNum].Integral;
                            awardModel.couponId =  items[awardNum].CouponId;
                            awardModel.integral =  items[awardNum].Integral;
                            if (awardModel.couponId != 0)
                            {
                                awardModel.couponName = CouponApplication.GetCouponInfo(long.Parse(items[awardNum].CouponId.ToString())).CouponName;
                            }
                            awardModel.proportion = items[awardNum].Proportion;
                            listAwardModel.Add(awardModel);
                            awardNum++;
                        }
                    }
                }
                else
                {
                    //奖等未取完
                    if (items.Count() > awardNum)
                    {
                        //获取奖项
                        WeiActivityAwardModel awardModel = new WeiActivityAwardModel();
                        awardModel.Id = items[awardNum].Id;
                        awardModel.activityId = items[awardNum].ActivityId;
                        awardModel.awardCount = items[awardNum].AwardCount;
                        awardModel.awardLevel = items[awardNum].AwardLevel;
                        awardModel.awardType = (WeiActivityAwardType)items[awardNum].AwardType;
                        awardModel.bonusId = items[awardNum].BonusId;
                        awardModel.couponId = items[awardNum].CouponId;
                        awardModel.integral = items[awardNum].Integral;
                        if (awardModel.couponId != 0)
                        {
                            awardModel.couponName = CouponApplication.GetCouponInfo(long.Parse(items[awardNum].CouponId.ToString())).CouponName;
                        }

                        awardModel.proportion = items[awardNum].Proportion;
                        listAwardModel.Add(awardModel);
                        awardNum++;
                    }
                    else
                    {
                        //获取奖项
                        WeiActivityAwardModel awardModel = new WeiActivityAwardModel();
                        listAwardModel.Add(awardModel);
                    }

                }
            }
            viewModel.awards = listAwardModel;
            return viewModel;
        }
        public static Entities.WeiActivityInfoInfo GetActivityInfo(long id)
        {
            return Service.GetActivityModel(id);
        }
        public static List<WeiActivityWinInfoInfo> GetActivityWin(long activity)
        {
            return Service.GetActivityWin(activity);
        }
        public static List<WeiActivityAwardInfo> GetActivityAwards(long activity) {
            return Service.GetActivityAwards(activity);
        }

        private static IWeiActivityWinService _iWeiActivityWinService =  EngineContext.Current.Resolve<IWeiActivityWinService>();
        private static IWeiActivityInfoService _iActivityInfoService =  EngineContext.Current.Resolve<IWeiActivityInfoService>();

        public static void WinnerSubmit(WeiActivityWinModel winModel)
        {
            Entities.WeiActivityInfoInfo model = _iActivityInfoService.GetActivityModel(winModel.activityId);
            //添加中奖信息记录
            Entities.WeiActivityWinInfoInfo activityWinInfo = new Entities.WeiActivityWinInfoInfo();
            activityWinInfo.ActivityId = winModel.activityId;
            activityWinInfo.AddDate = DateTime.Now;
            activityWinInfo.UserId = winModel.userId;
            activityWinInfo.IsWin = winModel.isWin;
            //消耗积分
            if (winModel.integrals > 0)
            {
                var factoryService = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create;
                Mall.Entities.MemberIntegralRecordInfo info = new Mall.Entities.MemberIntegralRecordInfo();
                info.UserName = MemberApplication.GetMember(activityWinInfo.UserId).UserName;
                info.MemberId = activityWinInfo.UserId;
                info.RecordDate = DateTime.Now;
                info.ReMark = ((WeiActivityType)model.ActivityType).ToDescription() + "消耗";
                info.TypeId = Entities.MemberIntegralInfo.IntegralType.WeiActivity;
                var memberIntegral = factoryService.Create(Mall.Entities.MemberIntegralInfo.IntegralType.WeiActivity, -winModel.integrals);
                MemberIntegralApplication.AddMemberIntegral(info, memberIntegral);
            }
            if (activityWinInfo.IsWin)
            {
                if (winModel.awardType == WeiActivityAwardType.Integral)//积分
                {
                    activityWinInfo.AwardName = winModel.awardName + " 积分";
                    //新增积分记录
                    var factoryService = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create;
                    Entities.MemberIntegralRecordInfo info = new Entities.MemberIntegralRecordInfo();
                    info.UserName = MemberApplication.GetMember(activityWinInfo.UserId).UserName;
                    info.MemberId = activityWinInfo.UserId;
                    info.RecordDate = DateTime.Now;
                    info.ReMark = ((WeiActivityType)model.ActivityType).ToDescription() + "中奖";
                    info.TypeId = Mall.Entities.MemberIntegralInfo.IntegralType.WeiActivity;
                    var memberIntegral = factoryService.Create(Entities.MemberIntegralInfo.IntegralType.WeiActivity, Convert.ToInt32(winModel.awardName));//winModel.integrals为消耗积分 此处不是用户总积分
                    MemberIntegralApplication.AddMemberIntegral(info, memberIntegral);
                }
                else if (winModel.awardType == WeiActivityAwardType.Bonus)//红包
                {
                    activityWinInfo.AwardName = winModel.awardName;
                    BonusApplication.SetShareByUserId(winModel.bonusId, false, winModel.userId);//添加红包记录
                    BonusApplication.IncrReceiveCount(winModel.bonusId);
                }
                else
                {
                    activityWinInfo.AwardName = winModel.awardName;
                    //添加优惠券记录
                    Entities.CouponRecordInfo recordInfo = new Entities.CouponRecordInfo();
                    recordInfo.UserId = winModel.userId;
                    recordInfo.UserName = MemberApplication.GetMember(winModel.userId).UserName;
                    recordInfo.ShopId = CouponApplication.GetCouponInfo(winModel.couponId).ShopId;
                    recordInfo.CouponId = winModel.couponId;
                    CouponApplication.AddCouponRecord(recordInfo);
                }
            }
            else
            {
                activityWinInfo.AwardName = winModel.awardName;
            }
            activityWinInfo.AwardId = winModel.awardId;
            _iWeiActivityWinService.AddWinner(activityWinInfo);
        }

        public static QueryPageModel<WeiActivityWinModel> GetActivityWin(string text, long id, int pageIndex, int pageSize)
        {

            QueryPageModel<WeiActivityWinModel> weiInfo = _iWeiActivityWinService.Get(text, id, pageIndex, pageSize);
            return weiInfo;
        }

        public static IEnumerable<Entities.BonusInfo> GetBonusByType(Mall.Entities.BonusInfo.BonusType bonusType)
        {
            return BonusApplication.GetBonusByType(bonusType);
        }

        public static string GetBonusSurplus(long bonusId)
        {
            return BonusApplication.GetBonusSurplus(bonusId);
        }

        public static int GetWinModel(WeiActivityModel model)
        {
            List<Entities.WeiActivityWinInfoInfo> listWinModel = _iWeiActivityWinService.GetWinInfo(model.userId);
            listWinModel = listWinModel.Where(p => p.ActivityId == model.Id).ToList();
            if (model.participationType == WeiParticipateType.DayCount)
            {
                return listWinModel.Where(p => p.AddDate.Date == DateTime.Now.Date).Count();
            }
            else if (model.participationType == WeiParticipateType.CommonCount)
            {
                return listWinModel.Count();
            }
            else
            {
                return -1;
            }

        }
        public static bool GetWinNumberByAward(long id, long awardId)
        {
            bool flag = false;
            var item = GetActivityInfo(id);

            var awardItem = GetActivityAwards(id).Where(p => p.Id == awardId).ToList();
            int num = GetActivityWin(id).Where(p => p.AwardId == awardId).Count();
            foreach (var i in awardItem)
            {
                if (num < i.AwardCount)
                {
                    flag = true;
                }
            }

            return flag;
        }

        public static int GetCouPonMax(long userId, long activityId, long awardId)
        {
            var item = _iWeiActivityWinService.GetWinInfo(userId);
            return item.Where(p => p.ActivityId == activityId && p.AwardId == awardId).Count();
        }


        public static string GetWinNumber(long activityId, string text)
        {
            return _iWeiActivityWinService.GetWinNumber(activityId, text);
        }
    }
}
