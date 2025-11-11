using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WezzelNL.Gemini;
using WezzelNL.Util;

public class IntegrationExample : MonoBehaviour
{
    //Example on how to use the gemini api
    [field: SerializeField]public GeminiLiveConfiguration Configuration { get; private set; } = GeminiLiveConfiguration.Default;
    [field: SerializeField]public string ApiKey { get; private set; }
    [field: SerializeField]public StreamedAudioSource StreamingSource { get; private set; }
    public GeminiLiveSession Session { get; private set; }
    public Queue<string> TextResponseQueue { get; } = new();
    
    
    void OnEnable()
    {
        Session = new GeminiLiveSession(GeminiAccessToken.Create(ApiKey), Configuration);
        
        //Session.AddListener<GeminiSessionInteractionEvent>(HandleInteractionAsync); You can use this instead of AddPlayback for more manual control or text-only generation
        
        Session.AddListener<GeminiTranscribeEvent>(HandleTranscriptionAsync);
        Session.AddListener<GeminiServerClosedSessionEvent>(HandleServerClosure);
        Session.AddListener<GeminiTurnEndEvent>(HandleTurnEnd);
        Session.AddListener<GeminiGenerationCompleteEvent>(HandleGenerationComplete);
        
        //Extension methods for easily connecting the session to an audio source
        Session.AddPlayback(StreamingSource); 
        Session.AddOnPlaybackFinishedHandler(StreamingSource, OnAudioFinishPlaying);
        
        AsyncDispatcher.DispatchNonBlocking(Session.StartSessionAsync, destroyCancellationToken);
    }

    void  OnDisable()
    {
        //Session.RemoveListener<GeminiSessionInteractionEvent>(HandleInteractionAsync); You can use this instead of RemovePlayback for more manual control
        
        Session.RemoveListener<GeminiTranscribeEvent>(HandleTranscriptionAsync);
        Session.RemoveListener<GeminiServerClosedSessionEvent>(HandleServerClosure);
        Session.RemoveListener<GeminiTurnEndEvent>(HandleTurnEnd);
        Session.RemoveListener<GeminiGenerationCompleteEvent>(HandleGenerationComplete);
        
        Session.RemovePlayback(StreamingSource);
        Session.RemoveOnPlaybackFinishedHandler(StreamingSource, OnAudioFinishPlaying);
        
        AsyncDispatcher.DispatchNonBlocking(Session.EndSessionAsync, destroyCancellationToken);
    }
    
    //This will make it so you see the errors the server gives (if you get them at all)
    //Client errors will be thrown as exceptions, you can get those by setting Session.ExceptionHandler = YourErrorHandler;
    //The default exception handler is Debug.LogException
    private Task HandleServerClosure(GeminiServerClosedSessionEvent evt, CancellationToken ct)
    {
        Debug.LogError($"Server closed session with error '{evt.CloseStatus}', reason: {evt.ClosureReason}");
        return Task.CompletedTask;
    }
    
    //Example on how to use manual event handling instead of the AddPlayback method
    // 
    // private FloatDataStream stream;
    // private Task HandleInteractionAsync(GeminiSessionInteractionEvent evt, CancellationToken ct)
    // {
    //     foreach (var part in evt.Interaction.Parts)
    //     {
    //         switch (part)
    //         {
    //             case GeminiAudioInteractionPart geminiAudio:
    //             {
    //                 if (!StreamingSource.IsConnected)
    //                 {
    //                     stream = StreamingSource.Connect(geminiAudio.SampleRate);
    //                 }
    //                 stream.Write(geminiAudio.Samples);
    //             } break;
    //             case GeminiTextInteractionPart geminiText:
    //             {
    //                 TextResponseQueue.Enqueue(geminiText.Text);
    //             } break;
    //         }
    //     }
    //     return Task.CompletedTask;
    // }
    
    private Task HandleTranscriptionAsync(GeminiTranscribeEvent evt, CancellationToken ct)
    {
        TextResponseQueue.Enqueue(evt.Text);
        return Task.CompletedTask;
    }
    
    private Task HandleTurnEnd(GeminiTurnEndEvent evt, CancellationToken ct)
    {
        Debug.Log("Turn end!");
        TextResponseQueue.Enqueue("\n\n");
        return Task.CompletedTask;
    }
    
    //This function is invoked everytime when the Session sends a TurnEnd signal and the streamedAudioSource finishes its buffer.
    //Everytime the turn end signal is sent, all data from the current response has been sent to the audio source.
    //This means that when that fragment finishes playing, the response finishes playing.
    private void OnAudioFinishPlaying(GeminiLiveSession session, StreamedAudioSource source)
    {
        Debug.Log($"Audio on gameobject {source.gameObject.name} finished playing!");
    }

    private Task HandleGenerationComplete(GeminiGenerationCompleteEvent evt, CancellationToken ct)
    {
        Debug.Log("Generation Complete");
        return Task.CompletedTask;
    }
    
    public void Prompt(string message)
    {
        AsyncDispatcher.DispatchNonBlocking(ct => Session.PromptAsync(message, ct), destroyCancellationToken);
    }
}