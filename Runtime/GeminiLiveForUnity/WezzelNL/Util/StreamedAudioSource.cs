using System;
using UnityEngine;
using UnityEngine.Events;

namespace WezzelNL.Util
{
    [RequireComponent(typeof(AudioSource))]
    public class StreamedAudioSource : MonoBehaviour
    {
        [field: SerializeField] public AudioSource Source { get; private set; }
        [field: SerializeField] public int BufferSizeSeconds { get; private set; } = 240;
        [field: SerializeField]public UnityEvent OnAudioFragmentFinishPlaying { get; private set; }
        public bool IsConnected { get; private set; } = false;
        public FloatDataStream Stream { get; private set; }
        public int SamplesRemaining { get; private set; } = 0;
        private int lastSample = 0;
        
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
            Source.loop = true;
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

        private void Update()
        {
            if (!IsConnected) return;
            var deltaSamples = 0;
            if (lastSample > Source.timeSamples) //Audio clip looped
            {
                var diffA = Source.clip.samples - lastSample;
                var diffB = Source.timeSamples;
                deltaSamples = diffA + diffB;
            }
            else deltaSamples = Source.timeSamples - lastSample;
            lastSample = Source.timeSamples;
            if (SamplesRemaining > 0)
            {
                SamplesRemaining -= deltaSamples;
                if (SamplesRemaining < 1)
                {
                    OnBufferFinishPlaying();
                    SamplesRemaining = 0;
                }
            }
        }

        private void OnBufferFinishPlaying() => OnAudioFragmentFinishPlaying?.Invoke();

        private void OnAudioRead(float[] data)
        {
            if (!IsConnected) return;
            Stream.Read(data);
            SamplesRemaining += Stream.ReadBufferDelta();
        }
    }
}