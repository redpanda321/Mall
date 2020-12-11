using Mall.Core;
using Mall.IServices;
using Mall.Entities;
using NetRube.Data;
using System.Collections.Generic;

namespace Mall.Service
{
    public class MobileHomeTopicService : ServiceBase, IMobileHomeTopicService
    {
        /// <summary>
        /// 最大专题数
        /// </summary>
        const int MAX_HOMETOPIC_COUNT = 10;

        public List<MobileHomeTopicInfo> GetMobileHomeTopicInfos(PlatformType platformType, long shopId = 0)
        {
            return DbFactory.Default.Get<MobileHomeTopicInfo>().Where(item => item.ShopId == shopId && item.Platform == platformType).ToList();
        }

        public MobileHomeTopicInfo GetMobileHomeTopic(long id, long shopId = 0)
        {
            return DbFactory.Default.Get<MobileHomeTopicInfo>().Where(item => item.Id == id && item.ShopId == shopId).FirstOrDefault();
        }

        public void AddMobileHomeTopic(long topicId, long shopId, PlatformType platformType, string frontCoverImage = null)
        {
            var existTopicInfo = DbFactory.Default.Get<MobileHomeTopicInfo>().Where(item => item.TopicId == topicId && item.ShopId == shopId && item.Platform == platformType).Count();
            if (existTopicInfo > 0)
                throw new Mall.Core.MallException("已经添加过相同的专题");

            var allCount = DbFactory.Default.Get<MobileHomeTopicInfo>().Where(item => item.ShopId == shopId && item.Platform == platformType).Count();
            if (allCount >= MAX_HOMETOPIC_COUNT)
                throw new Mall.Core.MallException(string.Format("最多只能添加{0}个专题", MAX_HOMETOPIC_COUNT));

            var mobileHomeTopicInfo = new MobileHomeTopicInfo()
            {
                Platform = platformType,
                ShopId = shopId,
                TopicId = topicId,
            };
            DbFactory.Default.Add(mobileHomeTopicInfo);
        }

        public void SetSequence(long id, int sequence, long shopId = 0)
        {
            var mobileHomeTopicInfo = GetMobileHomeTopic(id, shopId);
            DbFactory.Default.Set<MobileHomeTopicInfo>().Set(n => n.Sequence, sequence).Where(p => p.Id == mobileHomeTopicInfo.Id).Succeed();
        }

        public void Delete(long id)
        {
            DbFactory.Default.Del<MobileHomeTopicInfo>(n => n.Id == id);
        }




    }
}
