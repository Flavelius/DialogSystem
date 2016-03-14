using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DialogSystem.Localization
{
    public class LocalizedStringEditor
    {
        readonly string _targetIdentifier;
        readonly bool _needsDescription;
        Vector2 _scrollPos;
        LocalizedString _target;

        public LocalizedStringEditor(LocalizedString str, string identifier, bool needsDescription = true)
        {
            _targetIdentifier = identifier;
            _target = str;
            _needsDescription = needsDescription;
        }

        public void EndEdit()
        {
            _target = null;
        }

        public bool DrawGui()
        {
            var ret = true;
            if (_target == null)
            {
                return false;
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).GetStyle("GroupBox"));
            GUILayout.Space(5);
            GUILayout.Label("Editing: " + _targetIdentifier, EditorStyles.helpBox);
            if (_needsDescription)
            {
                GUILayout.Label("Description:", EditorStyles.helpBox);
                _target.Description = EditorGUILayout.TextField(_target.Description);
            }
            GUILayout.Label("Localized strings:", EditorStyles.helpBox);
            if (GUILayout.Button("Add"))
            {
                var currentLangIndex = 0;
                var maxIndex = Enum.GetNames(typeof (DialogLanguage)).Length;
                while (currentLangIndex < maxIndex)
                {
                    if (!LanguageEntryExists(_target.Strings, (DialogLanguage) currentLangIndex))
                    {
                        break;
                    }
                    currentLangIndex++;
                }
                if (currentLangIndex < maxIndex)
                {
                    _target.Strings.Add(new LocalizedString.LanguageString((DialogLanguage) currentLangIndex, ""));
                }
            }
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.MinHeight(150));
            var removeIndex = -1;
            for (var i = 0; i < _target.Strings.Count; i++)
            {
                GUILayout.BeginVertical(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).GetStyle("GroupBox"));
                GUILayout.BeginHorizontal();
                var l = (DialogLanguage) EditorGUILayout.EnumPopup(_target.Strings[i].language);
                if (l != _target.Strings[i].language)
                {
                    if (!LanguageEntryExists(_target.Strings, l))
                    {
                        var ls = _target.Strings[i];
                        ls.language = l;
                        _target.Strings[i] = ls;
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove"))
                {
                    removeIndex = i;
                }
                GUILayout.EndHorizontal();
                var els = _target.Strings[i];
                els.Text = GUILayout.TextArea(els.Text, GUILayout.MinWidth(200), GUILayout.MinHeight(80));
                _target.Strings[i] = els;
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                if (removeIndex >= 0)
                {
                    _target.Strings.RemoveAt(removeIndex);
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

        bool LanguageEntryExists(List<LocalizedString.LanguageString> ls, DialogLanguage l)
        {
            for (var i = 0; i < ls.Count; i++)
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