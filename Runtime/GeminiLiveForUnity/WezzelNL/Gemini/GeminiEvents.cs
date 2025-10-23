using System;
using System.Net.WebSockets;

namespace WezzelNL.Gemini
{
    public abstract class GeminiLiveEvent { }
    public class GeminiSessionStartEvent : GeminiLiveEvent {}
    public class GeminiSessionReadyEvent : GeminiLiveEvent {}
    public class GeminiSessionEndEvent : GeminiLiveEvent {}
    public class GeminiSessionInteractionEvent : GeminiLiveEvent
    {
        public GeminiInteraction Interaction { get; }

        public GeminiSessionInteractionEvent(GeminiInteraction interaction)
        {
            Interaction = interaction;
        }
    }

    [Serializable]
    public enum GeminiTransciption
    {
        Output,
        Input
    }
    public class GeminiTranscribeEvent : GeminiLiveEvent
    {
        public GeminiTransciption Type { get; }
        public string Text { get; }

        public GeminiTranscribeEvent(GeminiTransciption type, string text)
        {
            Type = type;
            Text = text;
        }
    }

    public class GeminiServerClosedSessionEvent : GeminiLiveEvent
    {
        public WebSocketCloseStatus? CloseStatus { get; }
        public string ClosureReason { get; }

        public GeminiServerClosedSessionEvent(WebSocketCloseStatus? closeStatus, string closureReason)
        {
            CloseStatus = closeStatus;
            ClosureReason = closureReason;
        }
    }

    public class GeminiJsonPacketReceived : GeminiLiveEvent
    {
        public string Json { get; }

        public GeminiJsonPacketReceived(string json)
        {
            Json = json;
        }
    }
    
    public class GeminiPromptedEvent : GeminiLiveEvent
    {
        public string PromptRaw { get; }
        public string PromptSanitized { get; }

        public GeminiPromptedEvent(string promptRaw, string promptSanitized)
        {
            PromptRaw = promptRaw;
            PromptSanitized = promptSanitized;
        }
    }
}