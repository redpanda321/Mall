using System;

namespace Mall.Core
{
    /// <summary>
    /// Mall 异常
    /// </summary>
    public class MallException : ApplicationException
    {
        public MallException() {
            Log.Info(this.Message, this);
        }

        public MallException(string message) : base(message) {
            Log.Info(message, this);
        }

        public MallException(string message, Exception inner) : base(message, inner) {
            Log.Info(message, inner);
        }

    }
}
