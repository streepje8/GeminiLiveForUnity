using System;
using System.Collections.Generic;

namespace WezzelNL.Gemini
{
    [Serializable]
    public enum InteractionRole
    {
        System,
        User,
        Model
    }
    
    [Serializable]
    public class GeminiInteraction
    {
        public InteractionRole Role { get; }
        public IEnumerable<GeminiInteractionPart> Parts { get; }

        public GeminiInteraction(InteractionRole role, IEnumerable<GeminiInteractionPart> parts)
        {
            Role = role;
            Parts = parts;
        }
    }
}