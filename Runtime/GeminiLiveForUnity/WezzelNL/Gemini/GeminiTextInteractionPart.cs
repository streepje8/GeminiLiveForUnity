namespace WezzelNL.Gemini
{
    public class GeminiTextInteractionPart : GeminiInteractionPart
    {
        public string Text { get; }

        public GeminiTextInteractionPart(string text)
        {
            Text = text;
        }
    }
}