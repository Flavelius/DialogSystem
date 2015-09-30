using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Runtime.Serialization;

namespace Localization
{

    public enum Language
    {
        EN_Default,
        DE
    }

    public enum LocalizationFallback
    {
        Language,
        DebugOutput,
        EmptyString
    }

    [System.Serializable]
    [DataContract(Name = "LocalizedString")]
    public class LocalizedString
    {

        public LocalizedString() { }

        public LocalizedString(string description)
        {
            this.description = description;
        }

        [DataMember(Name = "Description")]
        private string description = "";
        public string Description { get { return description!= ""?description:"No Description"; } set { description = value; } }

        [DataMember(Name="Strings")]
        public List<LanguageString> Strings = new List<LanguageString>();

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
        [DataContract(Name="LanguageString")]
        public class LanguageString
        {
            [DataMember(Name="Language")]
            public Language language = Language.EN_Default;
            [DataMember(Name="Text")]
            public string Text = "";
            public LanguageString() { }
            public LanguageString(Language lang, string txt)
            {
                language = lang;
                Text = txt;
            }
        }
    }

    public class LocalizedStringEditor : EditorWindow
    {

        private static LocalizedString target;
        private static bool cancelPending = false;

        public static void OpenEdit(LocalizedString toEdit)
        {
            target = toEdit;
            EditorWindow.GetWindow<LocalizedStringEditor>(true, "Edit");
            cancelPending = false;
        }

        public static void CancelEdit()
        {
            cancelPending = true;
        }

        void OnDestroy()
        {
            target = null;
            cancelPending = false;
        }

        void Update()
        {
            if (cancelPending) { Close(); }
        }

        private LocalizedString.LanguageString editLanguageString;

        private Vector2 scrollPos;
        void OnGUI()
        {
            EditorGUILayout.HelpBox(string.Format("Editing LocalizedString: {0} - {1} Entries", target.Description, target.Strings.Count), MessageType.Info);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Description:");
            target.Description = EditorGUILayout.TextField(target.Description);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Add"))
            {
                int currentLangIndex = 0;
                int maxIndex = System.Enum.GetNames(typeof(Language)).Length;
                while (currentLangIndex < maxIndex)
                {
                    if (!LanguageEntryExists((Language)currentLangIndex))
                    {
                        break;
                    }
                    currentLangIndex++;
                }
                if (currentLangIndex < maxIndex)
                {
                    target.Strings.Add(new LocalizedString.LanguageString((Language)currentLangIndex, ""));
                }
            }
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            int removeIndex = -1;
            for (int i = 0; i < target.Strings.Count; i++)
            {
                GUILayout.BeginVertical(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).GetStyle("GroupBox"));
                GUILayout.BeginHorizontal();
                Language l = (Language)EditorGUILayout.EnumPopup(target.Strings[i].language);
                if (l != target.Strings[i].language)
                {
                    if (!LanguageEntryExists(l))
                    {
                        target.Strings[i].language = l;
                    }
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Edit"))
                {
                    editLanguageString = target.Strings[i];
                }
                if (GUILayout.Button("Remove"))
                {
                    removeIndex = i;
                }
                GUILayout.EndHorizontal();
                if (editLanguageString == target.Strings[i])
                {
                    editLanguageString.Text = GUILayout.TextArea(editLanguageString.Text, GUILayout.MinWidth(200), GUILayout.MinHeight(80));
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Close"))
                    {
                        editLanguageString = null;
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                if (removeIndex >= 0)
                {
                    target.Strings.RemoveAt(removeIndex);
                    break;
                }
            }
            GUILayout.EndScrollView();
        }

        private bool LanguageEntryExists(Language l)
        {
            for (int i = 0; i < target.Strings.Count; i++)
            {
                if (target.Strings[i].language == l)
                {
                    return true;
                }
            }
            return false;
        }

    }
}