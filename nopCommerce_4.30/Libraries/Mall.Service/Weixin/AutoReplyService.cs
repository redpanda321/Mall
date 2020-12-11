using Mall.CommonModel;
using Mall.Core;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;

namespace Mall.Service
{
    public class AutoReplyService : ServiceBase, IAutoReplyService
    {
        /// <summary>
        /// 添加/修改规则
        /// </summary>
        /// <param name="autoReplyInfo"></param>
        public void ModAutoReply(AutoReplyInfo autoReplyInfo)
        {
            var m = DbFactory.Default.Get<AutoReplyInfo>().Where(a => a.Id == autoReplyInfo.Id).FirstOrDefault();
            if (m != null)
            {
                if (m.ReplyType == ReplyType.Keyword || m.ReplyType == ReplyType.Msg)
                {
                    m.RuleName = autoReplyInfo.RuleName;
                    m.Keyword = autoReplyInfo.Keyword;
                    m.MatchType = autoReplyInfo.MatchType;
                }
                m.TextReply = autoReplyInfo.TextReply;
                m.IsOpen = autoReplyInfo.IsOpen;
                m.ReplyType = autoReplyInfo.ReplyType;
                m.ReplyContentType = autoReplyInfo.ReplyContentType;
                m.MediaId = autoReplyInfo.MediaId;
                DbFactory.Default.Update(m);
            }
            else
            {
                DbFactory.Default.Add(autoReplyInfo);
            }
        }
        /// <summary>
        /// 删除规则
        /// </summary>
        /// <param name="autoReplyInfo"></param>
        public void DeleteAutoReply(AutoReplyInfo autoReplyInfo)
        {
            var m = DbFactory.Default.Del<AutoReplyInfo>().Where(a => a.Id == autoReplyInfo.Id).Succeed();
            if (!m)
            {
                throw new MallException("错误:规则不存在！");
            }
        }
        /// <summary>
        /// 根据关键词和消息类型获取自动回复信息
        /// </summary>
        /// <param name="type">消息类型</param>
        /// <param name="keyWord">关键字</param>
        /// <param name="isList">是否列表</param>
        /// <param name="isReply">是否自动回复</param>
        /// <returns></returns>
        public AutoReplyInfo GetAutoReplyByKey(ReplyType type, string keyWord = "", bool isList = true, bool isReply = false, bool isFollow = false)
        {
            var sql = DbFactory.Default.Get<AutoReplyInfo>().Where(d => d.ReplyType == type);
            if (!string.IsNullOrWhiteSpace(keyWord))
            {
                sql.Where(d => d.Keyword == keyWord);
            }
            if (isReply && !isFollow)
            {
                sql.Where(d => d.MatchType == MatchType.Full);
            }
            if (!isList)
            {
                sql.Where(d => d.IsOpen == 0);
            }
            var result = sql.FirstOrDefault();//优先完全匹配查询
            if (result == null && type == ReplyType.Keyword)
            {
                //完全匹配查询无数据，再根据模糊匹配查询数据1234con123
                result = DbFactory.Default.Get<AutoReplyInfo>().Where(d => d.Keyword.Contains(keyWord) && d.MatchType == MatchType.Like && d.IsOpen == 0).OrderByDescending(d => d.Id).FirstOrDefault();//如果完全关键字结果为null,在进行模糊匹配，返回最新一条
                if (result == null)
                {
                    //关键字回复内容为空，取消息自动回复内容
                    result = this.GetAutoReplyMsg();
                }
            }

            return result;
        }
        private AutoReplyInfo GetAutoReplyMsg()
        {
            var data = DbFactory.Default.Get<AutoReplyInfo>().Where(d => d.ReplyType == ReplyType.Msg && d.IsOpen == 0).FirstOrDefault();
            return data;
        }
        public AutoReplyInfo GetAutoReplyById(int Id)
        {
            return DbFactory.Default.Get<AutoReplyInfo>().Where(d => d.Id == Id).FirstOrDefault();
        }
        /// <summary>
        /// 获取关键字回复列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public QueryPageModel<AutoReplyInfo> GetPage(int pageIndex, int pageSize, ReplyType type)
        {
            QueryPageModel<AutoReplyInfo> result = new QueryPageModel<AutoReplyInfo>();

            var sql = DbFactory.Default.Get<AutoReplyInfo>().Where(d => d.ReplyType == type);
            var rets = sql.OrderByDescending(o => o.Keyword).ToPagedList(pageIndex, pageSize);
            result.Models = rets;
            result.Total = rets.TotalRecordCount;
            return result;
        }
    }
}
