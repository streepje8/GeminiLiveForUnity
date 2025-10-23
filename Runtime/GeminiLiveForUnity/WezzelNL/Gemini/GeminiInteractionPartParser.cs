using System;
using Newtonsoft.Json.Linq;

namespace WezzelNL.Gemini
{
    public static class GeminiInteractionPartParser
    {
        public static GeminiInteractionPart ParsePart(JObject part)
        {
            if (part.TryGetValue("inlineData", out var inlineDataRaw) && inlineDataRaw is JObject inlineData)
            {
                if(inlineData.TryGetValue("mimeType", out var mimeTypeRaw) && mimeTypeRaw is JValue jsonMimeType
                   && inlineData.TryGetValue("data", out var dataRaw)) 
                { 
                        var mimeType = ((string)jsonMimeType.Value) ?? "";
                        var mimeTypeData = mimeType.Split(";");
                        if(mimeTypeData.Length < 1) return new GeminiInvalidInteractionPart();
                        switch (mimeTypeData[0].ToLowerInvariant().Trim())
                        {
                            case "audio/pcm":
                            {
                                if(mimeTypeData.Length < 2) return new GeminiInvalidInteractionPart(); //expect a sample rate
                                var sampleRateRaw = mimeTypeData[1].Split('=')[1];
                                if(string.IsNullOrEmpty(sampleRateRaw) || !int.TryParse(sampleRateRaw, out var sampleRate) || dataRaw is not JValue { Value: string data }) return new GeminiInvalidInteractionPart();
                                return ParseAudio(sampleRate, data);
                            }
                        }
                }
            }

            if (part.TryGetValue("text", out var textRaw) && textRaw is JValue jsonText && jsonText.Value is string text) return new GeminiTextInteractionPart(text);
            return new GeminiInvalidInteractionPart();
        }

        private static GeminiAudioInteractionPart ParseAudio(int sampleRate, string data)
        {
            var pcmBytes = Convert.FromBase64String(data);
            var shortBuffer = new short[pcmBytes.Length / 2];
            Buffer.BlockCopy(pcmBytes, 0, shortBuffer, 0, pcmBytes.Length);
            var sampleBuffer = new float[shortBuffer.Length];
            var shortSpan = shortBuffer.AsSpan();
            for (int i = 0; i < shortSpan.Length; i++)
            {
                sampleBuffer[i] = shortSpan[i] / 32768f; // Normalize to [-1, 1]
            }
            return new GeminiAudioInteractionPart(sampleRate, sampleBuffer);
        }
    }
}