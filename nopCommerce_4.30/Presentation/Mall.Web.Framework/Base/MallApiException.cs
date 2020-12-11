using Mall.Core;
using System;

namespace Mall.Web.Framework
{
    public class MallApiException : MallException
    {
        private Enum _ErrorCode { get; set; }
        public Enum ErrorCode
        {
            get
            {
                return _ErrorCode;
            }
        }

        private string _Message { get; set; }
        public override string Message
        {
            get
            {
                string result = base.Message;
                if (!string.IsNullOrWhiteSpace(_Message))
                {
                    result = _Message;
                }
                else
                {
                    _ErrorCode = ApiErrorCode.System_Error;
                }
                return result;
            }
        }

        /// <summary>
        /// Api统一异常
        /// </summary>
        public MallApiException() { }
        /// <summary>
        /// Api统一异常
        /// </summary>
        public MallApiException(Enum errorcode, string message) : base(errorcode.ToString()+":"+message)
        {
            this._ErrorCode = errorcode;
            this._Message = message;
        }

        /// <summary>
        /// Api统一异常
        /// </summary>
        public MallApiException(string message) : base(message) { }

        /// <summary>
        /// Api统一异常
        /// </summary>
        public MallApiException(string message, Exception inner) : base(message, inner) { }
    }
}
