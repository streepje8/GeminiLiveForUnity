using System.Net.WebSockets;
using System.Runtime.ExceptionServices;

namespace WezzelNL.Gemini
{
    public class GeminiLiveListeningException : GeminiLiveException
    {
        private ExceptionDispatchInfo Capture { get; }
        public GeminiLiveListeningException(WebSocketException exception, ExceptionDispatchInfo capture) : base($"An exception occured while listening on the websocket: '{exception.Message}'. Call .ReThrow() on this exception for more info")
        {
            Capture = capture;
        }

        public void ReThrow() => Capture.Throw();
    }
}