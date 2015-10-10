using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace DialogSystem.Localization
{
    [System.Serializable]
    public class LocalizedString
    {
        public LocalizedString(string description)
        {
            Description = description;
        }

        [SerializeField, HideInInspector]
        public string Description;

        [SerializeField, HideInInspector]
        private List<LanguageString> strings = new List<LanguageString>();
        public List<LanguageString> Strings
        {
            get
            {
                return strings;
            }
        }

        public string GetString(Language lang, LocalizationFallback fallback, Language fallbackLanguage = Language.EN_Default)
        {
            for (int i = 0; i < Strings.Count; i++)
            {
                if (Strings[i].language == lang)
                {
                    return Strings[i].Text;
                }
            }
            switch (fallback)
            {
                case LocalizationFallback.Language:
                    for (int i = 0; i < Strings.Count; i++)
                    {
                        if (Strings[i].language == fallbackLanguage)
                        {
                            return Strings[i].Text;
                        }
                    }
                    goto default;
                case LocalizationFallback.EmptyString:
                    return "";
                case LocalizationFallback.DebugOutput:
                default:
                    return string.Format("LocalizedString not found: {0}", Description);
            }
        }

        [System.Serializable]
        public class LanguageString
        {
            public LanguageString(Language lang = Language.EN_Default)
            {
                language = lang;
                Text = "Not Set";
            }
            public Language language;
            public string Text;
            public LanguageString(Language lang, string txt)
            {
                language = lang;
                Text = txt;
            }
        }
    }
}