using Mall.API.Model.ParamsModel;
using Mall.Application;
using Mall.DTO.QueryModel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Web.Http;

namespace Mall.API
{
    public class ShopMessageController : BaseShopLoginedApiController
    {
        /// <summary>
        /// 获取未读消息数
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet("GetMessages")]
        public object GetMessages(
            int pageNo = 1, /*页码*/
            int pageSize = 10/*每页显示数据量*/)
        {
            CheckUserLogin();
            long shopid = CurrentUser.ShopId;
            AppMessageQuery query = new AppMessageQuery();
            query.ShopId = shopid;
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            query.StartTime = DateTime.Now.AddDays(-30).Date;
            var data = AppMessageApplication.GetMessages(query);
            return new { success = true, rows = data.Models, total = data.Total };
        }
        /// <summary>
        /// 消息状态改已读
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("PostReadMessage")]
        public object PostReadMessage(ShopAppReadMessageModel model)
        {
            CheckUserLogin();
            AppMessageApplication.ReadMessage(model.id);
            return new { success=true };
        }
    }
}
