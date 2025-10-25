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
                if(Socket.State == WebSocketState.Open) socketIsOpen = true;
                await ConfigureSessionAsync(ct);
                SessionActive = true;
                SessionState = SessionState.SettingUp;
                await EventHandlerInvokeAsync(new GeminiSessionStartEvent(), ct);
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
            await CloseSocketAsync(ct);
            SessionActive = false;
            SessionState = SessionState.NotConnected;
            await EventHandlerInvokeAsync(new GeminiSessionEndEvent(), ct);
        }

        private bool socketIsOpen = false;
        public async Task CloseSocketAsync(CancellationToken ct)
        {
            if (!socketIsOpen) return;
            socketIsOpen = false;
            if (Socket is not null)
            {
                if (Socket.State == WebSocketState.Open)
                {
                    try { await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client Disconnect", ct); }
                    catch (Exception) { try { Socket.Dispose(); } catch(Exception) { /* ignored */ } }
                }
                else try { Socket.Dispose(); } catch(Exception) { /* ignored */ }
            }
            Socket = null;
        }

        public async Task PromptAsync(string prompt, CancellationToken ct)
        {
            if (!SessionActive) throw new GeminiLiveException("Cannot prompt while no session is active!");
            var sanitized = GeminiPromptSanitizer.FullSanitize(prompt);
            if (string.IsNullOrWhiteSpace(sanitized)) return;
            await EventHandlerInvokeAsync(new GeminiPromptedEvent(prompt, sanitized), ct);
            await SocketSendJsonAsync($@"
{{
    ""realtimeInput"": {{
        ""text"": ""{sanitized}""
    }}
}}
", ct);
        }
        
        public delegate Task GeminiEventHandler<T>(T evt, CancellationToken ct) where T : GeminiLiveEvent;
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
        
        private async Task EventHandlerInvokeAsync<T>(T evt, CancellationToken ct) where T : GeminiLiveEvent
        {
            //In case the user is listening to all events
            foreach (var baseEventHandler in GeminiEventHandlers<GeminiLiveEvent>.EventHandlers)
            {
                try { await baseEventHandler(evt, ct); }
                catch (Exception e) { Debug.LogException(e); }
            }
            
            foreach (var handler in GeminiEventHandlers<T>.EventHandlers)
            {
                try { await handler(evt, ct); }
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
                    await EventHandlerInvokeAsync(new GeminiServerClosedSessionEvent(Socket.CloseStatus, Socket.CloseStatusDescription), ct);
                    await EndSessionAsync(ct);
                    return;
                }

            
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                while (!result.EndOfMessage && !ct.IsCancellationRequested)
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

                if(!ct.IsCancellationRequested) await HandleIncomingJsonPacketAsync(message, ct);
            }
        }

        private void HandleSocketReceiveException(WebSocketException socketException, ExceptionDispatchInfo capture) => ExceptionHandler(new GeminiLiveListeningException(socketException, capture));
        
        private async Task HandleIncomingJsonPacketAsync(string json, CancellationToken ct)
        {
            await EventHandlerInvokeAsync(new GeminiJsonPacketReceived(json), ct);
            if (JToken.Parse(json) is JObject packet)
            {
                if (packet.TryGetValue("serverContent", out var serverContentRaw) && serverContentRaw is JObject serverContent) await HandleServerContentAsync(serverContent, ct);
                if (packet.TryGetValue("setupComplete", out _))
                {
                    SessionState = SessionState.Ready;
                    await EventHandlerInvokeAsync(new GeminiSessionReadyEvent(), ct);
                }
                if(packet.TryGetValue("usageMetadata", out var usageMetadataRaw) && usageMetadataRaw is JObject usageMetadata) await HandleUsageMetadataAsync(usageMetadata, ct);
            } else ExceptionHandler(new GeminiLiveException($"Received invalid json: '{json}'"));
        }

        private async Task HandleUsageMetadataAsync(JObject usageMetadata, CancellationToken ct)
        {
            var promptTokenCount = 0;
            var responseTokenCount = 0;
            var totalTokenCount = 0;
            var promptTokenDetails = new List<GeminiTokenDetail>();
            var responseTokenDetails = new List<GeminiTokenDetail>();
            if (usageMetadata.TryGetValue("promptTokenCount", out var promptTokenCountRaw) && promptTokenCountRaw is JValue ptcValue && ptcValue.Value is long ptc) promptTokenCount = checked((int)ptc);
            if (usageMetadata.TryGetValue("responseTokenCount", out var responseTokenCountRaw) && responseTokenCountRaw is JValue rtcValue && rtcValue.Value is long rtc) responseTokenCount = checked((int)rtc);
            if (usageMetadata.TryGetValue("totalTokenCount", out var totalTokenCountRaw) && totalTokenCountRaw is JValue ttcValue && ttcValue.Value is long ttc) totalTokenCount = checked((int)ttc);
            if (usageMetadata.TryGetValue("promptTokensDetails", out var promptTokenDetailsRaw) && promptTokenDetailsRaw is JArray promptTokenDetailsJson)
            {
                foreach (var promptTokenDetailRaw in promptTokenDetailsJson)
                {
                    if (promptTokenDetailRaw is JObject promptTokenDetail) promptTokenDetails.Add(ParseTokenDetail(promptTokenDetail));
                }
            }
            if (usageMetadata.TryGetValue("responseTokensDetails", out var responseTokenDetailsRaw) && responseTokenDetailsRaw is JArray responseTokenDetailsJson)
            {
                foreach (var responseTokenDetailRaw in responseTokenDetailsJson)
                {
                    if (responseTokenDetailRaw is JObject responseTokenDetail) responseTokenDetails.Add(ParseTokenDetail(responseTokenDetail));
                }
            }

            await EventHandlerInvokeAsync(new GeminiReceiveUsageMetricsEvent(promptTokenCount, responseTokenCount, totalTokenCount, promptTokenDetails, responseTokenDetails), ct);
        }

        private GeminiTokenDetail ParseTokenDetail(JObject tc)
        {
            var modality = GeminiModality.Text;
            var tokenCount = 0;
            if (tc.TryGetValue("modality", out var modalityRaw) && modalityRaw is JValue modalityJson && modalityJson.Value is string modalityString && Enum.TryParse<GeminiModality>(modalityString, out var parsedModalilty)) modality = parsedModalilty;
            if (tc.TryGetValue("tokenCount", out var tokenCountRaw) && tokenCountRaw is JValue tokenCountJson && tokenCountJson.Value is long parsedTokenCount) tokenCount = checked((int)parsedTokenCount);
            return new GeminiTokenDetail(modality, tokenCount);
        }

        private async Task HandleServerContentAsync(JObject serverContent, CancellationToken ct)
        {
            if (serverContent.TryGetValue("modelTurn", out var modelTurnRaw) && modelTurnRaw is JObject modelTurn) await HandleModelTurnAsync(modelTurn, ct);
            if (serverContent.TryGetValue("outputTranscription", out var modelOutputTranscriptionRaw) && modelOutputTranscriptionRaw is JObject modelOutputTranscription) await HandleTranscriptionAsync(GeminiTransciption.Output, modelOutputTranscription, ct);
            if (serverContent.TryGetValue("inputTranscription", out var modelInputTranscriptionRaw) && modelInputTranscriptionRaw is JObject modelInputTranscription) await HandleTranscriptionAsync(GeminiTransciption.Input, modelInputTranscription, ct);
            if (serverContent.TryGetValue("generationComplete", out var generationCompleteRaw) && generationCompleteRaw is JValue generationComplete && generationComplete.Value is bool gc && gc) await EventHandlerInvokeAsync(new GeminiGenerationCompleteEvent(), ct);
            if (serverContent.TryGetValue("turnComplete", out var turnCompleteRaw) && turnCompleteRaw is JValue turnComplete && turnComplete.Value is bool tc && tc) await EventHandlerInvokeAsync(new GeminiTurnEndEvent(), ct);
        }

        private async Task HandleTranscriptionAsync(GeminiTransciption transcriptionType, JObject transcription, CancellationToken ct)
        {
            if (transcription.TryGetValue("text", out var textRaw) && textRaw is JValue jsonText && jsonText.Value is string text)
            {
                await EventHandlerInvokeAsync(new GeminiTranscribeEvent(transcriptionType, text), ct);
            }
        }

        private async Task HandleModelTurnAsync(JObject modelTurn, CancellationToken ct)
        {
            if (modelTurn.TryGetValue("parts", out var partsRaw) && partsRaw is JArray parts)
            {
                List<GeminiInteractionPart> interactionParts = new List<GeminiInteractionPart>(); 
                foreach (var partRaw in parts)
                {
                    if(partRaw is not JObject part) continue;
                    interactionParts.Add(GeminiInteractionPartParser.ParsePart(part));
                }
                await EventHandlerInvokeAsync(new GeminiSessionInteractionEvent(new GeminiInteraction(InteractionRole.Model, interactionParts)), ct);
            }
        }
    }
}