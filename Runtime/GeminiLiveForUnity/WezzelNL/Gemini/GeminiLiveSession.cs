using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace WezzelNL.Gemini
{
    public enum SessionState
    {
        NotConnected,
        SettingUp,
        Ready
    }
    public class GeminiLiveSession
    {
        //Public API
        public ClientWebSocket Socket { get; private set; }
        public bool SessionActive { get; private set; }
        public SessionState SessionState { get; private set; } = SessionState.NotConnected;
        public GeminiLiveConfiguration Configuration { get; private set; }
        public delegate void ExceptionHandlerFunction(GeminiLiveException exception);
        public ExceptionHandlerFunction ExceptionHandler { get; set; } = Debug.LogException;

        private GeminiAccessToken AccessToken { get; }
        private static readonly string WebsocketUri = "wss://generativelanguage.googleapis.com/ws/google.ai.generativelanguage.v1beta.GenerativeService.BidiGenerateContent";

        public GeminiLiveSession(GeminiAccessToken accessToken, GeminiLiveConfiguration? configuration = null)
        {
            Configuration = configuration ?? GeminiLiveConfiguration.Default;
            AccessToken = accessToken;
        }

        public async Task StartSessionAsync(CancellationToken ct)
        {
            if(SessionActive) await EndSessionAsync(ct);
            try
            {
                Socket = new ClientWebSocket()
                {
                    Options = { }
                };
                await Socket.ConnectAsync(new Uri($"{WebsocketUri}?key={AccessToken.AccessToken}"), ct);
                await ConfigureSessionAsync(ct);
                SessionActive = true;
                SessionState = SessionState.SettingUp;
                await EventHandlerInvokeAsync(new GeminiSessionStartEvent());
                await ListenToSocketAsync(ct);
            }
            catch (Exception e)
            {
                await EndSessionAsync(ct);
                throw new GeminiLiveConnectionException(e, ExceptionDispatchInfo.Capture(e));
            }
        }

        private async Task ConfigureSessionAsync(CancellationToken ct)
        {
            var configJson = Configuration.ToJson();
            await SocketSendJsonAsync(configJson, ct);
        }

        public async Task EndSessionAsync(CancellationToken ct)
        {
            if (!SessionActive) return;
            if (Socket is not null)
            {
                if(Socket.State == WebSocketState.Open) await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client Disconnect", ct);
                Socket.Dispose();
            }
            Socket = null;
            SessionActive = false;
            SessionState = SessionState.NotConnected;
            await EventHandlerInvokeAsync(new GeminiSessionEndEvent());
        }

        public async Task PromptAsync(string prompt, CancellationToken ct)
        {
            if (!SessionActive) throw new GeminiLiveException("Cannot prompt while no session is active!");
            var sanitized = GeminiPromptSanitizer.FullSanitize(prompt);
            if (string.IsNullOrWhiteSpace(sanitized)) return;
            await EventHandlerInvokeAsync(new GeminiPromptedEvent(prompt, sanitized));
            await SocketSendJsonAsync($@"
{{
    ""realtimeInput"": {{
        ""text"": ""{sanitized}""
    }}
}}
", ct);
        }
        
        public delegate Task GeminiEventHandler<T>(T evt) where T : GeminiLiveEvent;
        internal static class GeminiEventHandlers<T> where T : GeminiLiveEvent
        {
            public static readonly List<GeminiEventHandler<T>> EventHandlers = new List<GeminiEventHandler<T>>();
        }
        public void AddListener<T>(GeminiEventHandler<T> handler) where T : GeminiLiveEvent
        {
            GeminiEventHandlers<T>.EventHandlers.Add(handler); 
        }
        
        public void RemoveListener<T>(GeminiEventHandler<T> handler) where T : GeminiLiveEvent
        {
            GeminiEventHandlers<T>.EventHandlers.Remove(handler); 
        }
        
        private async Task EventHandlerInvokeAsync<T>(T evt) where T : GeminiLiveEvent
        {
            //In case the user is listening to all events
            foreach (var baseEventHandler in GeminiEventHandlers<GeminiLiveEvent>.EventHandlers)
            {
                try { await baseEventHandler(evt); }
                catch (Exception e) { Debug.LogException(e); }
            }
            
            foreach (var handler in GeminiEventHandlers<T>.EventHandlers)
            {
                try { await handler(evt); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }
        
        private async Task SocketSendJsonAsync(string json, CancellationToken ct)
        {
            json = json.Trim();
            var encoder = Encoding.UTF8.GetEncoder();
            var charBufferSize = 4096;
            var byteBufferSize = Encoding.UTF8.GetMaxByteCount(charBufferSize);
            var charBuffer = new char[charBufferSize];
            var byteBuffer = new byte[byteBufferSize];
            var totalChars = json.Length;
            var charOffset = 0;

            while (charOffset < totalChars)
            {
                var charsToProcess = Math.Min(charBufferSize, totalChars - charOffset);
                json.CopyTo(charOffset, charBuffer, 0, charsToProcess);

                var flush = (charOffset + charsToProcess) >= totalChars;
                encoder.Convert(charBuffer,0, charsToProcess, byteBuffer, 0, byteBuffer.Length, flush,out var charsUsed, out var bytesUsed, out var completed);
                charOffset += charsUsed;
                var endOfMessage = (charOffset >= totalChars) && completed;
                await Socket.SendAsync(new ArraySegment<byte>(byteBuffer, 0, bytesUsed), WebSocketMessageType.Text, endOfMessage, ct);
            }
        }
    
        private async Task ListenToSocketAsync(CancellationToken ct)
        {
            var buffer = new byte[4096];
            var segment = new ArraySegment<byte>(buffer);
            while (Socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                WebSocketReceiveResult result;
                try { result = await Socket.ReceiveAsync(segment, ct); }
                catch (OperationCanceledException) { return; }
                catch (WebSocketException ex)
                {
                    HandleSocketReceiveException(ex, ExceptionDispatchInfo.Capture(ex));
                    continue;
                }
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await EventHandlerInvokeAsync(new GeminiServerClosedSessionEvent(Socket.CloseStatus, Socket.CloseStatusDescription));
                    await EndSessionAsync(ct);
                    return;
                }

            
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                while (!result.EndOfMessage)
                {
                    try { result = await Socket.ReceiveAsync(segment, ct); }
                    catch (OperationCanceledException) { return; }
                    catch (WebSocketException ex)
                    {
                        HandleSocketReceiveException(ex, ExceptionDispatchInfo.Capture(ex));
                        continue;
                    }
                    message += Encoding.UTF8.GetString(buffer, 0, result.Count);
                }

                await HandleIncomingJsonPacketAsync(message);
            }
        }

        private void HandleSocketReceiveException(WebSocketException socketException, ExceptionDispatchInfo capture) => ExceptionHandler(new GeminiLiveListeningException(socketException, capture));
        
        private async Task HandleIncomingJsonPacketAsync(string json)
        {
            await EventHandlerInvokeAsync(new GeminiJsonPacketReceived(json));
            if (JToken.Parse(json) is JObject packet)
            {
                if (packet.TryGetValue("serverContent", out var serverContentRaw) && serverContentRaw is JObject serverContent) await HandleServerContentAsync(serverContent);
                if (packet.TryGetValue("setupComplete", out _))
                {
                    SessionState = SessionState.Ready;
                    await EventHandlerInvokeAsync<GeminiSessionReadyEvent>(new GeminiSessionReadyEvent());
                }
            } else ExceptionHandler(new GeminiLiveException($"Received invalid json: '{json}'"));
        }

        private async Task HandleServerContentAsync(JObject serverContent)
        {
            if (serverContent.TryGetValue("modelTurn", out var modelTurnRaw) && modelTurnRaw is JObject modelTurn) await HandleModelTurnAsync(modelTurn);
            if (serverContent.TryGetValue("outputTranscription", out var modelOutputTranscriptionRaw) && modelOutputTranscriptionRaw is JObject modelOutputTranscription) await HandleTranscriptionAsync(GeminiTransciption.Output, modelOutputTranscription);
            if (serverContent.TryGetValue("inputTranscription", out var modelInputTranscriptionRaw) && modelInputTranscriptionRaw is JObject modelInputTranscription) await HandleTranscriptionAsync(GeminiTransciption.Input, modelInputTranscription);
        }

        private async Task HandleTranscriptionAsync(GeminiTransciption transcriptionType, JObject transcription)
        {
            if (transcription.TryGetValue("text", out var textRaw) && textRaw is JValue jsonText && jsonText.Value is string text)
            {
                await EventHandlerInvokeAsync(new GeminiTranscribeEvent(transcriptionType, text));
            }
        }

        private async Task HandleModelTurnAsync(JObject modelTurn)
        {
            if (modelTurn.TryGetValue("parts", out var partsRaw) && partsRaw is JArray parts)
            {
                List<GeminiInteractionPart> interactionParts = new List<GeminiInteractionPart>(); 
                foreach (var partRaw in parts)
                {
                    if(partRaw is not JObject part) continue;
                    interactionParts.Add(GeminiInteractionPartParser.ParsePart(part));
                }
                await EventHandlerInvokeAsync(new GeminiSessionInteractionEvent(new GeminiInteraction(InteractionRole.Model, interactionParts)));
            }
        }
    }
}