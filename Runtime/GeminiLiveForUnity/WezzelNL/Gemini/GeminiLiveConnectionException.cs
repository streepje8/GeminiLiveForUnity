using System;
using System.Runtime.ExceptionServices;

namespace WezzelNL.Gemini
{
    public class GeminiLiveConnectionException : GeminiLiveException
    {
        private ExceptionDispatchInfo FailureCause { get; }
        public GeminiLiveConnectionException(Exception exception, ExceptionDispatchInfo failureCause) : 
            base($"Failed to connect and start gemini live session. Reason: An exception occured while connecting. Exception: {exception.Message}. For more information call .ReThrow() on this exception")
        {
            FailureCause = failureCause;
        }

        public void ReThrow() => FailureCause.Throw();
    }
}