using System;
using UnityEngine;

namespace WezzelNL.Gemini
{
    [Serializable]
    public struct GeminiVoiceConfiguration
    {
        [field: SerializeField]public GeminiVoice Voice { get; set; }
        public string ToJson()
        {
            return $@"{{
                ""prebuilt_voice_config"": {{
                    ""voice_name"": ""{GeminiEnumUtility.EnumToVoiceName(Voice)}""
                }}
            }}";
        }
    }
}