using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using Localization;

namespace DialogSystem
{
    public class LocalizedStringEditor
    {
        public LocalizedStringEditor(LocalizedString str, string identifier, bool needsDescription = true)
        {
            ident = identifier;
            target = str;
            this.needsDescription = needsDescription;
        }

        public void EndEdit()
        {
            target = null;
        }

        string ident = "";
        bool needsDescription;
        LocalizedString target;
        private Vector2 scrollPos;
        public bool DrawGUI()
        {
            bool ret = true;
            if (target == null) { return false; }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).GetStyle("GroupBox"));
            GUILayout.Space(5);
            GUILayout.Label("Editing: " + ident, EditorStyles.helpBox);
            if (needsDescription)
            {
                GUILayout.Label("Description:", EditorStyles.helpBox);
                target.Description = EditorGUILayout.TextField(target.Description);
            }
            GUILayout.Label("Localized strings:", EditorStyles.helpBox);
            if (GUILayout.Button("Add"))
            {
                int currentLangIndex = 0;
                int maxIndex = System.Enum.GetNames(typeof(Language)).Length;
                while (currentLangIndex < maxIndex)
                {
                    if (!LanguageEntryExists(target.Strings, (Language)currentLangIndex))
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
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.MinHeight(150));
            int removeIndex = -1;
            for (int i = 0; i < target.Strings.Count; i++)
            {
                GUILayout.BeginVertical(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).GetStyle("GroupBox"));
                GUILayout.BeginHorizontal();
                Language l = (Language)EditorGUILayout.EnumPopup(target.Strings[i].language);
                if (l != target.Strings[i].language)
                {
                    if (!LanguageEntryExists(target.Strings, l))
                    {
                        LocalizedString.LanguageString ls = target.Strings[i];
                        ls.language = l;
                        target.Strings[i] = ls;
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove"))
                {
                    removeIndex = i;
                }
                GUILayout.EndHorizontal();
                LocalizedString.LanguageString els = target.Strings[i];
                els.Text = GUILayout.TextArea(els.Text, GUILayout.MinWidth(200), GUILayout.MinHeight(80));
                target.Strings[i] = els;
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                if (removeIndex >= 0)
                {
                    target.Strings.RemoveAt(removeIndex);
                    break;
                }
            }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close"))
            {
                ret = false;
            }

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return ret;
        }

        private bool LanguageEntryExists(List<LocalizedString.LanguageString> ls, Language l)
        {
            for (int i = 0; i < ls.Count; i++)
            {
                if (ls[i].language == l)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
