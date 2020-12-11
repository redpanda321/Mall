using Mall.Entities;
using Mall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Application
{
    public class TopicApplication:BaseApplicaion<ITopicService>
    {
        public static TopicInfo GetTopic(long id)
        {
            return Service.GetTopicInfo(id);
        }

        public static List<TopicInfo> GetTopics(List<long> topics)
        {
            return Service.GetTopics(topics);
        }

        public static TopicModuleInfo GetTopicModule(long moduleId) {
            return Service.GetTopicModule(moduleId);
        }
        public static List<TopicModuleInfo> GetModules(long topic) {
            return Service.GetModules(topic);
        }

        public static List<ModuleProductInfo> GetModuleProducts(IEnumerable<long> modules)
        {
            return Service.GetModuleProducts(modules.ToList());
        }

        public static List<ModuleProductInfo> GetModuleProducts(long moduleId)
        {
            return Service.GetModuleProducts(new List<long> { moduleId });
        }


    }
}
