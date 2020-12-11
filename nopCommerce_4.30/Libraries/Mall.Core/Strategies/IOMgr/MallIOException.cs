using System;
namespace Mall.Core
{
	public class MallIOException : MallException
	{
		public MallIOException()
		{
		}
		public MallIOException(string message) : base(message)
		{
		}
        public MallIOException(string message, Exception inner)
            : base(message, inner)
		{
		}
	}
}
