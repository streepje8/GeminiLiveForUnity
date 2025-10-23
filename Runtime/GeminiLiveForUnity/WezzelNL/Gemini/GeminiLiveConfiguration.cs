using System;
using UnityEngine;

namespace WezzelNL.Gemini
{

    [Serializable]
    public class GeminiLiveConfiguration
    {
        [field: SerializeField]public GeminiBiDirectionalModel Model { get; set; }
        [field: SerializeField]public GenerationConfiguration GenerationConfiguration { get; set; }
        [field: SerializeField]public bool InputTranscription { get; set; }
        [field: SerializeField]public bool OutputTranscription { get; set; }
        [field: SerializeField]public string SystemInstruction { get; set; }
        [field: SerializeField]public bool SlidingContextWindow { get; set; }
    
        public static readonly GeminiLiveConfiguration Default = new GeminiLiveConfiguration()
        {
            Model = GeminiBiDirectionalModel.Flash20Experimental,
            GenerationConfiguration = new GenerationConfiguration()
            {
                MaxOutputTokens = 65536,
                Temperature = 1,
                TopP = 0.95f,
                TopK = 40,
                ResponseGeminiModality = GeminiModality.Audio,
                SpeechConfiguration = new GeminiSpeechConfiguration()
                {
                    SpeechLanguage = GeminiLanguage.EnglishUnitedStates,
                    VoiceConfiguration = new GeminiVoiceConfiguration()
                    {
                        Voice = GeminiVoice.Zephyr
                    }
                },
                ThinkingConfiguration = new GeminiThinkingConfiguration()
                {
                    IncludeThoughts = false,
                    ThinkingBudget = 0
                }
            },
            InputTranscription = false,
            OutputTranscription = true,
            SlidingContextWindow = true,
            SystemInstruction = "You are an ai chatbot integrated in unity. Introduce yourself as gemini integrated in unity and then tell users to change the system prompt in the Gemini Live configuration object.",
        };

        public string ToJson() => $@"
{{
    ""setup"": {{
        ""model"": ""{GeminiEnumUtility.EnumToModelString(Model)}"",
        ""generationConfig"": {GenerationConfiguration.ToJson()},
        {GetSlidingContextWindow()}
        {GetTranscriptions()}
        ""systemInstruction"": {{
            ""parts"": [
                {{
                    ""text"": ""{GeminiPromptSanitizer.JsonSanitize(SystemInstruction)}""
                }}
            ]
        }}
    }}
}}
";

        private string GetSlidingContextWindow()
        {
            return SlidingContextWindow ? @"""contextWindowCompression"": { ""slidingWindow"": {} }," : string.Empty;
        }

        private string GetTranscriptions()
        {
            var res = string.Empty;
            if (InputTranscription) res += "\"input_audio_transcription\": {},\n";
            if (OutputTranscription) res += "\"output_audio_transcription\": {},\n";
            return res;
        }
    }
}