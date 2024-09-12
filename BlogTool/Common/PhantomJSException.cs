using System;
using System.Runtime.Serialization;

namespace BlogTool.Common
{
    public class PhantomJSException : Exception
    {
        public int ErrorCode { get; private set; }

        public PhantomJSException(int errCode, string message)
            : base($"PhantomJS exit code {errCode}: {message}")
        {
            ErrorCode = errCode;
        }
    }
}