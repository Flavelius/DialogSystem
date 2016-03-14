using System.Reflection;
using DialogSystem.Localization;
using UnityEditor;
using UnityEngine;

namespace DialogSystem
{
    [CustomEditor(typeof (ConversationEngine))]
    public class ConversationEngineInspector : Editor
    {
        LocalizedStringEditor _activeStringEditor;

        public override void OnInspectorGUI()
        {
            var engine = target as ConversationEngine;
            var dialogs = serializedObject.FindProperty("_savedDialogs");
            if (dialogs == null)
            {
                EditorGUILayout.HelpBox("Property not found!", MessageType.Error);
                return;
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(dialogs, new GUIContent("Dialogs"), false);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            if (dialogs.objectReferenceValue != null && GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
            {
                DialogEditor.OpenEdit(dialogs.objectReferenceValue as DialogCollection);
            }
            EditorGUILayout.EndHorizontal();
            engine.Fallback = (LocalizationFallback) EditorGUILayout.EnumPopup("Fallback:", engine.Fallback);
            if (engine.Fallback == LocalizationFallback.Language)
            {
                engine.FallbackLanguage = (DialogLanguage) EditorGUILayout.EnumPopup("Fallback language:", engine.FallbackLanguage);
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label(
                new GUIContent("EndConversation fallback",
                    "if no dialogoptions are available because of requirements for example, inject a default one, to end the conversation"), GUILayout.Width(150));
            engine.UseEndConversationfallback = EditorGUILayout.Toggle(engine.UseEndConversationfallback, GUILayout.Width(15));
            if (engine.UseEndConversationfallback)
            {
                var fInfo = typeof (ConversationEngine).GetField("_endConversationFallback", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fInfo == null)
                {
                    EditorGUILayout.HelpBox("Property not found", MessageType.Error);
                }
                else
                {
                    var fallbackString = fInfo.GetValue(engine) as LocalizedString;
                    if (GUILayout.Button("Edit text", EditorStyles.miniButton))
                    {
                        _activeStringEditor = new LocalizedStringEditor(fallbackString, "Fallback option text", false);
                    }
                }
            }
            GUILayout.EndHorizontal();
            if (_activeStringEditor == null || _activeStringEditor.DrawGui()) return;
            _activeStringEditor.EndEdit();
            _activeStringEditor = null;
        }
    }
}