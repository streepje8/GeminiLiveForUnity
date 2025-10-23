namespace WezzelNL.Gemini
{
    public class GeminiAudioInteractionPart : GeminiInteractionPart
    {
        public int SampleRate { get; }
        public float[] Samples { get; }

        public GeminiAudioInteractionPart(int sampleRate, float[] samples)
        {
            SampleRate = sampleRate;
            Samples = samples;
        }
    }
}