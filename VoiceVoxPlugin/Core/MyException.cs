using System;

namespace VoiceVoxPlugin.Core
{
    public class MyException : Exception
    {
        public MyException(string message) : base(message)
        {
        }
    }
}