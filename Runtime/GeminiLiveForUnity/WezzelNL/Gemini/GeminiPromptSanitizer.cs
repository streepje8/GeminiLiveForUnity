using System;
using System.Collections.Generic;

namespace WezzelNL.Gemini
{
    public static class GeminiPromptSanitizer
    {
        public delegate (bool, string) CustomGeminiSanitizer(string unsanitized);
        private static List<CustomGeminiSanitizer> CustomSanitizers { get; } = new List<CustomGeminiSanitizer>();
        public static void AddCustomSanitizer(CustomGeminiSanitizer sanitizer) => CustomSanitizers.Add(sanitizer);
        public static void RemoveCustomSanitizer(CustomGeminiSanitizer sanitizer) => CustomSanitizers.Remove(sanitizer);
        
        public static string JsonSanitize(string prompt)
        {
            var promptCharacters = prompt.AsSpan();
            var changed = false;
            List<char> newChars = null;
            for (var i = 0; i < promptCharacters.Length; i++)
            {
                switch (promptCharacters[i])
                {
                    case '\\':
                    case '\"':
                    case '/':
                    case '\'':
                    case '\b':
                    case '\f':
                    case '\n':
                    case '\r':
                    case '\t':
                        if (!changed)
                        {
                            newChars = new();
                            if(i > 0)newChars.AddRange(promptCharacters[..(i-1)].ToArray());
                            changed = true;
                        }
                        newChars.AddRange(GetEscaped(promptCharacters[i]));
                        break;
                    default:
                        if(changed)newChars.Add(promptCharacters[i]);
                        break;
                }
            }
            return changed ? new string(newChars.ToArray()) : prompt;
        }

        private static string GetEscaped(char c)
        {
            return c switch
            {
                '\\' => "\\\\",
                '\"' => "\\\"",
                '/' => "\\/",
                '\b' => "\\b",
                '\f' => "\\f",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                '\'' => "\\'",
                _ => c.ToString()
            };
        }

        public static string FullSanitize(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt)) return string.Empty;
            foreach (var sanitizer in CustomSanitizers)
            {
                bool success;
                (success, prompt) = sanitizer(prompt);
                if (!success) return string.Empty;
            }
            return prompt;
        }
    }
}