using UnityEngine;

namespace WezzelNL.Util
{
    [RequireComponent(typeof(AudioSource))]
    public class StreamedAudioSource : MonoBehaviour
    {
        [field: SerializeField]public AudioSource Source { get; private set; }
        [field: SerializeField]public int BufferSizeSeconds { get; private set; } = 240;
        public bool IsConnected { get; private set; } = false;
        public FloatDataStream Stream { get; private set; }
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if(!Source) Source = GetComponent<AudioSource>();
        }
        #endif

        
        public FloatDataStream Connect(int sampleRate)
        {
            AudioClip streamingClip = AudioClip.Create(
                name: "Streaming_AudioClip",
                lengthSamples: sampleRate * 10,
                channels: 1,
                frequency: sampleRate,
                stream: true,
                pcmreadercallback: OnAudioRead,
                pcmsetpositioncallback: null
            );
            Source.clip = streamingClip;
            Source.Play();
            IsConnected = true;
            Stream = new FloatDataStream(sampleRate * BufferSizeSeconds);
            return Stream;
        }

        public void Disconnect()
        {
            Source.Stop();
            Source.clip = null;
            Stream = null;
            IsConnected = false;
        }

        private void OnAudioRead(float[] data)
        {
            if (!IsConnected) return;
            Stream.Read(data);
        }
    }
}