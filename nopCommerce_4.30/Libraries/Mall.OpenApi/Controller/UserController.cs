using Hishop.Open.Api;
using Mall.OpenApi.Model.DTO;
using Mall.OpenApi.Model.Parameter;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mall.OpenApi
{
    /// <summary>
    /// 交易控制器
    /// </summary>
    [Route("OpenApi")]
    public class UserController : OpenAPIController
    {
        private UserHelper userHelper;

        public UserController()
        {
            userHelper = new UserHelper();
        }

        /// <summary>
        /// 获取当前商家的订单列表
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public object GetUsers([FromQuery]GetUsersUserParameterModel para)
        {
            #region 参数初始
            if (para == null)
            {
                para = new GetUsersUserParameterModel();
            }
            para.InitCheck();
            CheckShopAuthority(para.app_key);
            #endregion

            var data = userHelper.GetUsers(para.start_time, para.end_time, para.page_no.Value, para.page_size.Value);
            List<user_list_model> datalist = new List<user_list_model>();
            if (data.Total > 0)
            {
                datalist = data.Models.ToList();
            }
            var result = new { users_get_response = new { total_results = data.Total, users = datalist } };
            return  new  JsonResult(result);
        }
        /// <summary>
        /// 获取当前商家的订单列表
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public object GetUser([FromQuery]GetUserUserParameterModel para)
        {
            #region 参数初始
            if (para == null)
            {
                para = new GetUserUserParameterModel();
            }
            para.InitCheck();
            CheckShopAuthority(para.app_key);
            #endregion

            var data = userHelper.GetUser(para.user_name);
            var result = new { user_get_response = new { user = data } };
            return  new JsonResult(result);
        }
        public object AddUser(AddUserUserParameterModel para)
        {
            #region 参数初始
            if (para == null)
            {
                para = new AddUserUserParameterModel();
            }
            para.InitCheck();
            CheckShopAuthority(para.app_key);
            #endregion
            var data = userHelper.GetUser(para.user_name);
            if (data != null)
            {
                throw new MallApiException(OpenApiErrorCode.Parameter_Error, "user_name");
            }
            string _pass = para.password;
            if (string.IsNullOrWhiteSpace(_pass))
            {
                _pass = GenerateNewPassword();
                para.password = _pass;
            }
            if (!para.created.HasValue)
            {
                para.created = DateTime.Now;
            }
            var uid = userHelper.AddUser(para.user_name, para.password, para.created.Value,
                para.real_name, para.mobile, para.email, para.sex, para.birthday,
                para.state, para.city, para.district, para.town, para.address);
            var result = new { user_add_response = new { uid = uid, password = _pass, created = para.created.Value.ToString("yyyy-MM-dd HH:mm:ss") } };
            return   new  JsonResult(result);

        }

        #region 私有
        /// <summary>
        /// 检测商家权限
        /// </summary>
        /// <param name="appkey"></param>
        private void CheckShopAuthority(string app_key)
        {
            var _shoph = new ShopHelper(app_key);
            if (!_shoph.IsSelf)  //仅限官方自营店使用用户接口
            {
                throw new MallApiException(OpenApiErrorCode.Invalid_App_Key, "appkey");
            }
        }

        private string GenerateNewPassword()
        {
            string result = "opu";
            Random rnd = new Random();
            string[] seeds = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            int seedlen = seeds.Length;
            result += seeds[rnd.Next(0, seedlen)];
            result += seeds[rnd.Next(0, seedlen)];
            result += seeds[rnd.Next(0, seedlen)];
            return result;
        }
        #endregion
    }
}
