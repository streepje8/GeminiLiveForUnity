using System;
using UnityEngine;

namespace WezzelNL.Gemini
{
    [Serializable]
    public struct GeminiThinkingConfiguration
    {
        [field: SerializeField]public bool IncludeThoughts { get; set; }
        [field: SerializeField]public int ThinkingBudget { get; set; }
        public string ToJson()
        {
            return $@"{{
                ""includeThoughts"": ""{IncludeThoughts}"",
                ""thinkingBudget"": {ThinkingBudget}
            }}";
        }
    }
}