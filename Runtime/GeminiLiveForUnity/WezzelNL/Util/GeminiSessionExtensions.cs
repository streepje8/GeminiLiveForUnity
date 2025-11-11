using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WezzelNL.Gemini;

namespace WezzelNL.Util
{
    internal class PlaybackFinishedHandler
    {
        private GeminiLiveSession Session { get; }
        private StreamedAudioSource Source { get; }
        private Action<GeminiLiveSession, StreamedAudioSource> OnFinishedHandler { get; }

        public PlaybackFinishedHandler(GeminiLiveSession session, StreamedAudioSource source, Action<GeminiLiveSession, StreamedAudioSource> onFinishedHandler)
        {
            Session = session;
            Source = source;
            OnFinishedHandler = onFinishedHandler;
        }

        private bool turnEnded = false;
        public Task OnTurnEnd(GeminiTurnEndEvent evt, CancellationToken ct)
        {
            turnEnded = true;
            return Task.CompletedTask;
        }
        
        public void OnFragmentFinish()
        {
            if (!turnEnded) return;
            turnEnded = false;
            OnFinishedHandler(Session, Source);
        }
    }
    
    public static class GeminiSessionExtensions
    {
        private static readonly Dictionary<StreamedAudioSource, GeminiLiveSession.GeminiEventHandler<GeminiSessionInteractionEvent>> PlaybackHandlers = new();
        private static readonly Dictionary<Action<GeminiLiveSession, StreamedAudioSource>, PlaybackFinishedHandler> PlaybackFinishedHandlers = new();
        public static void AddOnPlaybackFinishedHandler(this GeminiLiveSession session, StreamedAudioSource source, Action<GeminiLiveSession, StreamedAudioSource> onFinishedHandler)
        {
            session.RemoveOnPlaybackFinishedHandler(source, onFinishedHandler);
            var handler = new PlaybackFinishedHandler(session, source, onFinishedHandler);
            session.AddListener<GeminiTurnEndEvent>(handler.OnTurnEnd);
            source.OnAudioFragmentFinishPlaying.AddListener(handler.OnFragmentFinish);
            PlaybackFinishedHandlers[onFinishedHandler] = handler;
        }
        
        public static void RemoveOnPlaybackFinishedHandler(this GeminiLiveSession session, StreamedAudioSource source, Action<GeminiLiveSession, StreamedAudioSource> onFinishedHandler)
        {
            if (PlaybackFinishedHandlers.Remove(onFinishedHandler, out var oldHandler))
            {
                session.RemoveListener<GeminiTurnEndEvent>(oldHandler.OnTurnEnd);
                source.OnAudioFragmentFinishPlaying.RemoveListener(oldHandler.OnFragmentFinish);
            }
        }
        
        public static void AddPlayback(this GeminiLiveSession session, StreamedAudioSource source)
        {
            session.RemovePlayback(source);
            var handler = CreatePlaybackHandler(source);
            session.AddListener(handler);
            PlaybackHandlers[source] = handler;
        }

        public static void RemovePlayback(this GeminiLiveSession session, StreamedAudioSource source)
        {
            if (PlaybackHandlers.Remove(source, out var oldHandler)) session.RemoveListener(oldHandler);
        }
        
        private static GeminiLiveSession.GeminiEventHandler<GeminiSessionInteractionEvent> CreatePlaybackHandler(StreamedAudioSource source)
        {
            FloatDataStream stream = null;
            return (evt, ct) =>
            {
                foreach (var part in evt.Interaction.Parts)
                {
                    if (part is not GeminiAudioInteractionPart geminiAudio) continue;
                    if (!source.IsConnected) stream = source.Connect(geminiAudio.SampleRate);
                    stream?.Write(geminiAudio.Samples);
                }
                return Task.CompletedTask;
            };
        }
    }
}