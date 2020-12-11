using Mall.Core.Helper;
using Microsoft.AspNetCore.Mvc;

namespace Mall.Web.Framework
{
    [ApiExceptionFilter]
    public abstract class HiAPIController<TUser> : Controller
    {
        #region 字段
        private TUser _user;
        #endregion

        #region 属性
        /// <summary>
        /// 当前登录的用户
        /// </summary>
        public TUser CurrentUser
        {
            get
            {
                if (object.Equals(_user, default(TUser)))
                    _user = GetUser();

                return _user;
            }
        }

        /// <summary>
        /// 当前用户Id
        /// </summary>
        public long CurrentUserId
        {
            get
            {
                string userkey = "";
                userkey = WebHelper.GetQueryString("userkey");
                if (string.IsNullOrWhiteSpace(userkey))
                {
                    userkey = WebHelper.GetFormString("userkey");
                }
                return DecryptUserKey(userkey);
            }
        }
        #endregion

        #region 虚拟方法
        /// <summary>
        /// 解析userKey
        /// </summary>
        /// <param name="userKey"></param>
        /// <returns></returns>
        protected virtual long DecryptUserKey(string userKey)
        {
            return UserCookieEncryptHelper.Decrypt(userKey, CookieKeysCollection.USERROLE_USER);
        }
        #endregion

        #region 抽像方法
        /// <summary>
        /// 根据用户id获取用户信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        protected abstract TUser GetUser();
        #endregion

        #region 公共方法

        /// <summary>
        /// 通用JSON成功返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
  //     protected JsonResult<Result<T>> JsonResult<T>(T data = default(T), string msg = "", int code = 0)
  //      {
     //       return Json(SuccessResult(data, msg, code));
       // }

        /// <summary>
        /// 通用返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="success"></param>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected Result<T> ApiResult<T>(bool success, string msg = "", T data = default(T), int code = 0)
        {
            return new Result<T>
            {
                success = success,
                msg = msg,
                data = data,
                code = code
            };
        }

        /// <summary>
        /// 成功返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected Result<T> SuccessResult<T>(T data = default(T), string msg = "", int code = 0)
        {
            return ApiResult<T>(true, msg, data, code);
        }

        /// <summary>
        /// 失败返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected Result<T> ErrorResult<T>(string msg, T data = default(T), int code = 0)
        {
            return ApiResult<T>(false, msg, data, code);
        }

        public class Result<TData>
        {
            #region 字段
            private bool _success = false;
            private string _msg = string.Empty;
            private int _code = 0;
            private TData _data = default(TData);
            #endregion

            #region 构造函数

            #endregion

            #region 属性
            public bool success
            {
                get { return _success; }
                set { _success = value; }
            }

            public string msg
            {
                get { return _msg; }
                set { _msg = value; }
            }

            /// <summary>
            /// 状态码
            /// </summary>
            public int code
            {
                get { return _code; }
                set { _code = value; }
            }

            public TData data
            {
                get { return _data; }
                set { _data = value; }
            }

            #endregion

            #region 重写方法
            //public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object value)
            //{
            //    if (!_members.ContainsKey(binder.Name))
            //    {
            //        _members.Add(binder.Name, value);
            //        return true;
            //    }

            //    return base.TrySetMember(binder, value);
            //}

            //public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
            //{
            //    if (_members.ContainsKey(binder.Name))
            //    {
            //        result = _members[binder.Name];
            //        return true;
            //    }

            //    return base.TryGetMember(binder, out result);
            //}

            //public override IEnumerable<string> GetDynamicMemberNames()
            //{
            //    return base.GetDynamicMemberNames().Concat(_members.Keys);
            //}
            #endregion
        }

        #endregion
    }
}
