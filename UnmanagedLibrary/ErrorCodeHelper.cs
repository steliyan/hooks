using System;
using System.ComponentModel;

namespace UnmanagedLibrary
{
    public class ErrorCodeHelper
    {
        public static string GetMessage(uint errorCode)
        {
            var message = new Win32Exception((int)errorCode).Message;
            return message;
        }
    }
}
