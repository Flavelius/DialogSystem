using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogSystem.Localization
{
    [Serializable]
    public class LocalizedString
    {
        [SerializeField, HideInInspector] public string Description;

        [SerializeField, HideInInspector] List<LanguageString> strings = new List<LanguageString>();

        public LocalizedString(string description)
        {
            Description = description;
        }

        public List<LanguageString> Strings
        {
            get { return strings; }
        }

        public string GetString(DialogLanguage lang, LocalizationFallback fallback, DialogLanguage fallbackLanguage = DialogLanguage.EN_Default)
        {
            for (var i = 0; i < Strings.Count; i++)
            {
                if (Strings[i].language == lang)
                {
                    return Strings[i].Text;
                }
            }
            switch (fallback)
            {
                case LocalizationFallback.Language:
                    for (var i = 0; i < Strings.Count; i++)
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

        [Serializable]
        public class LanguageString
        {
            public DialogLanguage language;
            public string Text;

            public LanguageString(DialogLanguage lang = DialogLanguage.EN_Default)
            {
                language = lang;
                Text = "Not Set";
            }

            public LanguageString(DialogLanguage lang, string txt)
            {
                language = lang;
                Text = txt;
            }
        }
    }
}