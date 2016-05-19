using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogSystem.Localization
{
    [Serializable]
    public class LocalizedString
    {
        [SerializeField, HideInInspector] public string Description;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        [SerializeField, HideInInspector] List<LanguageString> _strings = new List<LanguageString>();

        public static readonly LocalizedString Empty = new LocalizedString("");

        public LocalizedString(string description)
        {
            Description = description;
        }

        public List<LanguageString> Strings
        {
            get { return _strings; }
        }

        public delegate bool LocalizedStringDelegate(DialogLanguage lang, out string text);

        public bool GetString(DialogLanguage lang, out string text)
        {
            for (var i = 0; i < Strings.Count; i++)
            {
                if (Strings[i].language != lang) continue;
                text = Strings[i].Text;
                return true;
            }
            text = string.Empty;
            return false;
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