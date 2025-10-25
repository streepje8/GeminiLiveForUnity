using System;

namespace WezzelNL.Gemini
{
    [Serializable]
    public struct GeminiTokenDetail
    {
        public GeminiModality Modality { get; }
        public int TokenCount { get; }

        public GeminiTokenDetail(GeminiModality modality, int tokenCount)
        {
            Modality = modality;
            TokenCount = tokenCount;
        }
    }
}