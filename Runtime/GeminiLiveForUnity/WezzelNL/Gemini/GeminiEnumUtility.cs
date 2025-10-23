using System;

namespace WezzelNL.Gemini
{
    public static class GeminiEnumUtility
    {
        public static string EnumToModelString(GeminiBiDirectionalModel model)
        {
            return model switch
            {
                GeminiBiDirectionalModel.Flash20Experimental => "models/gemini-2.0-flash-exp",
                GeminiBiDirectionalModel.Flash20Live => "models/gemini-2.0-flash-live-001",
                GeminiBiDirectionalModel.Flash25Preview => "models/gemini-live-2.5-flash-preview",
                GeminiBiDirectionalModel.Flash25LivePreview => "models/gemini-2.5-flash-live-preview",
                GeminiBiDirectionalModel.Flash25NativeAudioLatest => "models/gemini-2.5-flash-native-audio-latest",
                GeminiBiDirectionalModel.Flash25NativeAudio092025 => "models/gemini-2.5-flash-native-audio-preview-09-2025",
                _ => throw new GeminiLiveException($"Unsupported model type: '{model.ToString()}'")
            };
        }

        public static string EnumToLanguageCode(GeminiLanguage language)
        {
            return language switch
            {
                GeminiLanguage.GermanGermany => "de-DE",
                GeminiLanguage.EnglishAustralia => "en-AU",
                GeminiLanguage.EnglishGreatBritain => "en-GB",
                GeminiLanguage.EnglishIndia => "en-IN",
                GeminiLanguage.EnglishUnitedStates => "en-US",
                GeminiLanguage.SpanishUnitedStates => "es-US",
                GeminiLanguage.FrenchFrance => "fr-FR",
                GeminiLanguage.HindiIndia => "hi-IN",
                GeminiLanguage.PortugueseBrazil => "pt-BR",
                GeminiLanguage.SpanishSpain => "es-ES",
                GeminiLanguage.FrenchCanada => "fr-CA",
                GeminiLanguage.IndonesianIndonesia => "id-ID",
                GeminiLanguage.ItalianItaly => "it-IT",
                GeminiLanguage.JapaneseJapan => "ja-JP",
                GeminiLanguage.TurkishTurkey => "tr-TR",
                GeminiLanguage.VietnameseVietnam => "vi-VN",
                GeminiLanguage.BengaliIndia => "bn-IN",
                GeminiLanguage.GujaratiIndia => "gu-IN",
                GeminiLanguage.KannadaIndia => "kn-IN",
                GeminiLanguage.MalayalamIndia => "ml-IN",
                GeminiLanguage.MarathiIndia => "mr-IN",
                GeminiLanguage.TamilIndia => "ta-IN",
                GeminiLanguage.TeluguIndia => "te-IN",
                GeminiLanguage.DutchNetherlands => "nl-NL",
                GeminiLanguage.KoreanSouthKorea => "ko-KR",
                GeminiLanguage.ChineseChinaMandarin => "cmn-CN",
                GeminiLanguage.PolishPoland => "pl-PL",
                GeminiLanguage.RussianRussia => "ru-RU",
                GeminiLanguage.ThaiThailand => "th-TH",
                _ => throw new ArgumentOutOfRangeException(nameof(language), language, "Unsupported language enum value")
            };
        }


        public static string EnumToVoiceName(GeminiVoice voice)
        {
            return voice switch
            {
                GeminiVoice.Zephyr => "Zephyr",
                GeminiVoice.Puck => "Puck",
                GeminiVoice.Charon => "Charon",
                GeminiVoice.Kore => "Kore",
                GeminiVoice.Fenrir => "Fenrir",
                GeminiVoice.Leda => "Leda",
                GeminiVoice.Orus => "Orus",
                GeminiVoice.Aoede => "Aoede",
                GeminiVoice.Callirrhoe => "Callirrhoe",
                GeminiVoice.Autonoe => "Autonoe",
                GeminiVoice.Enceladus => "Enceladus",
                GeminiVoice.Iapetus => "Iapetus",
                GeminiVoice.Umbriel => "Umbriel",
                GeminiVoice.Algieba => "Algieba",
                GeminiVoice.Despina => "Despina",
                GeminiVoice.Erinome => "Erinome",
                GeminiVoice.Algenib => "Algenib",
                GeminiVoice.Rasalgethi => "Rasalgethi",
                GeminiVoice.Laomedeia => "Laomedeia",
                GeminiVoice.Achernar => "Achernar",
                GeminiVoice.Alnilam => "Alnilam",
                GeminiVoice.Schedar => "Schedar",
                GeminiVoice.Gacrux => "Gacrux",
                GeminiVoice.Pulcherrima => "Pulcherrima",
                GeminiVoice.Achird => "Achird",
                GeminiVoice.Zubenelgenubi => "Zubenelgenubi",
                GeminiVoice.Vindemiatrix => "Vindemiatrix",
                GeminiVoice.Sadachbia => "Sadachbia",
                GeminiVoice.Sadaltager => "Sadaltager",
                GeminiVoice.Sulafat => "Sulafat",
                _ => throw new ArgumentOutOfRangeException(nameof(voice), voice, null)
            };
        }

    }
}