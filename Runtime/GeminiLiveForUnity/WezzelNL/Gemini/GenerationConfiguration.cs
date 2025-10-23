using System;
using System.Globalization;
using UnityEngine;

namespace WezzelNL.Gemini
{
    /// <summary>
    /// Meer informatie op https://ai.google.dev/api/generate-content#v1beta.GenerationConfig
    /// </summary>
    [Serializable]
    public struct GenerationConfiguration
    { 
        [field: SerializeField]public int MaxOutputTokens { get; set; }
        [field: SerializeField]public float Temperature { get; set; }
        [field: SerializeField]public float TopP { get; set; }
        [field: SerializeField]public int TopK { get; set; }
        [field: SerializeField]public GeminiModality ResponseGeminiModality { get; set; }
        [field: SerializeField]public GeminiSpeechConfiguration SpeechConfiguration { get; set; }
        [field: SerializeField]public GeminiThinkingConfiguration ThinkingConfiguration { get; set; }

        public string ToJson()
        {
            return $@"{{
            ""maxOutputTokens"": {MaxOutputTokens.ToString(CultureInfo.InvariantCulture)},
            ""temperature"": {Temperature.ToString(CultureInfo.InvariantCulture)},
            ""topP"": {TopP.ToString(CultureInfo.InvariantCulture)},
            ""topK"": {TopK.ToString(CultureInfo.InvariantCulture)},
            ""thinkingConfig"": {ThinkingConfiguration.ToJson()},
            ""responseModalities"": [{$"\"{ResponseGeminiModality.ToString().ToUpperInvariant()}\""}]
            {GetSpeechConfig()}
        }}";
        }

        private string GetSpeechConfig()
        {
            if (ResponseGeminiModality != GeminiModality.Audio) return string.Empty;
            return $@",""speechConfig"": {SpeechConfiguration.ToJson()}";
        }
    }
}