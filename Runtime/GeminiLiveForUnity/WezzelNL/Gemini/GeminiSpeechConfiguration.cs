using System;
using UnityEngine;

namespace WezzelNL.Gemini
{
    [Serializable]
    public struct GeminiSpeechConfiguration
    {
        [field: SerializeField]public GeminiVoiceConfiguration VoiceConfiguration { get; set; }
        [field: SerializeField]public GeminiLanguage SpeechLanguage { get; set; }
        public string ToJson()
        {
            return $@"{{
                ""voiceConfig"": {VoiceConfiguration.ToJson()},
                ""languageCode"": ""{GeminiEnumUtility.EnumToLanguageCode(SpeechLanguage)}""
            }}";
        }
    }
}