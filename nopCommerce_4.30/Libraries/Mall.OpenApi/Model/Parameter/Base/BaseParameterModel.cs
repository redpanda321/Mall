using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hishop.Open.Api;
using Mall.Core;

using Mall.Web.Framework;

namespace Mall.OpenApi.Model.Parameter
{
    /// <summary>
    /// 基础传入参数
    /// </summary>
    public class BaseParameterModel
    {
        public string app_key { get; set;}
        public string timestamp{get; set;}
        public string sign{get; set;}

        /// <summary>
        /// 检测参数完整性与合法性
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckParameter()
        {
            bool result = false;
            if(string.IsNullOrWhiteSpace(app_key))
            {
                throw new MallApiException(Hishop.Open.Api.OpenApiErrorCode.Missing_App_Key,"app_key");
            }
            if (string.IsNullOrWhiteSpace(timestamp))
            {
                throw new MallApiException(Hishop.Open.Api.OpenApiErrorCode.Missing_Timestamp, "timestamp");
            }
            if (!OpenApiSign.CheckTimeStamp(timestamp))
            {
                throw new MallApiException(Hishop.Open.Api.OpenApiErrorCode.Invalid_Timestamp, "timestamp");
            }
            if (string.IsNullOrWhiteSpace(sign))
            {
                throw new MallApiException(Hishop.Open.Api.OpenApiErrorCode.Missing_Signature, "sign");
            }
            return result;
        }
        /// <summary>
        /// 值初始
        /// </summary>
        public virtual void ValueInit()
        {

        }
        /// <summary>
        /// 值初始检测
        /// </summary>
        public virtual void InitCheck()
        {
            ValueInit();
            CheckParameter();
        }
    }
}
