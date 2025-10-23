using System;

namespace WezzelNL.Gemini
{
    public class GeminiLiveException : Exception
    {
        public override string Message { get; }

        public GeminiLiveException(string message)
        {
            Message = message;
        }
    }
}