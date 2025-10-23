using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using WezzelNL.Gemini;
using WezzelNL.Util;

public class IntegrationExample : MonoBehaviour
{
    //Example on how to use the gemini api
    [field: SerializeField]public GeminiLiveConfiguration Configuration { get; private set; } = GeminiLiveConfiguration.Default;
    [field: SerializeField]public string ApiKey { get; private set; }
    public GeminiLiveSession Session { get; private set; }
    
    void OnEnable()
    {
        Session = new GeminiLiveSession(GeminiAccessToken.Create(ApiKey), Configuration);
        Session.AddListener<GeminiSessionInteractionEvent>(HandleInteractionAsync);
        Session.AddListener<GeminiTranscribeEvent>(HandleTranscriptionAsync);
        Session.AddListener<GeminiServerClosedSessionEvent>(HandleServerClosure);
        AsyncDispatcher.SetMainThread(); 
        AsyncDispatcher.DispatchNonBlocking(Session.StartSessionAsync, destroyCancellationToken);
    }
    private void OnDisable()
    {
        Session.RemoveListener<GeminiSessionInteractionEvent>(HandleInteractionAsync);
        Session.RemoveListener<GeminiTranscribeEvent>(HandleTranscriptionAsync);
        Session.RemoveListener<GeminiServerClosedSessionEvent>(HandleServerClosure);
        AsyncDispatcher.DispatchNonBlocking(Session.EndSessionAsync, destroyCancellationToken);
    }
    
    //This will make it so you see the errors the server gives (if you get them at all)
    //Client errors will be thrown as exceptions, you can get those by setting Session.ExceptionHandler = YourErrorHandler;
    //The default exception handler is Debug.LogException
    private Task HandleServerClosure(GeminiServerClosedSessionEvent evt)
    {
        Debug.LogError($"Server closed session with error '{evt.CloseStatus}', reason: {evt.ClosureReason}");
        return Task.CompletedTask;
    }
    
    //Example on how to stream audio to an audio source
    [field: SerializeField]public StreamedAudioSource StreamingSource { get; private set; }
    public Queue<string> TextResponseQueue { get; } = new();
    private FloatDataStream stream;
    private Task HandleInteractionAsync(GeminiSessionInteractionEvent evt)
    {
        foreach (var part in evt.Interaction.Parts)
        {
            switch (part)
            {
                case GeminiAudioInteractionPart geminiAudio:
                {
                    if(!StreamingSource.IsConnected) stream = StreamingSource.Connect(geminiAudio.SampleRate);
                    stream.Write(geminiAudio.Samples);
                } break;
                case GeminiTextInteractionPart geminiText:
                {
                    TextResponseQueue.Enqueue(geminiText.Text);
                } break;
            }
        }
        return Task.CompletedTask;
    }

    
    private Task HandleTranscriptionAsync(GeminiTranscribeEvent evt)
    {
        TextResponseQueue.Enqueue(evt.Text);
        return Task.CompletedTask;
    }
    
    public void Prompt(string message)
    {
        AsyncDispatcher.DispatchNonBlocking(ct => Session.PromptAsync(message, ct), destroyCancellationToken);
    }
}