using System;
using System.Threading;

using Newtonsoft.Json;

using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;

namespace Mall.API.Hubs
{

    public class PrintHub : Hub
    {
        public static List<CurrentConnectUser> ConnectedUsers = new List<CurrentConnectUser>();
        private static string _connectionId = string.Empty;
        public void GetLatestItem(string userKey)
        {
            Core.Log.Info("GetLatestItem方法调用"+userKey);
            _connectionId = Context.ConnectionId;

            var deleteUser = ConnectedUsers.Find(p => p.UserKey == userKey);
            if (deleteUser != null)
                ConnectedUsers.Remove(deleteUser);

            ConnectedUsers.Add(new CurrentConnectUser()
            {
                ConnectionId = _connectionId,
                UserKey = userKey
            });
        }
        public static string ConnectionId
        {
            get { return _connectionId; }
        }
    }
    public class CurrentConnectUser
    {
        /// <summary>
        /// 当前的连接ID
        /// </summary>
        public string ConnectionId { get; set; }
        /// <summary>
        /// 商家/门店APP用户登录后唯一标识
        /// </summary>
        public string UserKey { get; set; }
        /// <summary>
        /// 商家/门店是否开启自动打印
        /// </summary>
        public bool IsAutoPrint { get; set; }
        /// <summary>
        /// 商家/门店设置打印张数
        /// </summary>
        public int PrintCount { get; set; }

        public Timer BroadcastLoop { get; set; }
    }
}
