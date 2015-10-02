using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ConversationEngine))]
public class ConversationEngineInspector : Editor
{
    public override void OnInspectorGUI()
    {
        ConversationEngine engine = target as ConversationEngine;
        engine.SavedDialogs = EditorGUILayout.ObjectField(new GUIContent("Saved Dialogs"), engine.SavedDialogs, typeof(TextAsset), false) as TextAsset;
        engine.fallback = (Localization.LocalizationFallback)EditorGUILayout.EnumPopup("Fallback:", engine.fallback);
        if (engine.fallback == Localization.LocalizationFallback.Language)
        {
            engine.fallbackLanguage = (Localization.Language)EditorGUILayout.EnumPopup("Fallback language:", engine.fallbackLanguage);
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("EndConversation fallback", "if no dialogoptions are available because of requirements for example, inject a default one, to end the conversation"), GUILayout.Width(150));
        engine.UseEndConversationfallback = EditorGUILayout.Toggle(engine.UseEndConversationfallback, GUILayout.Width(15));
        if (engine.UseEndConversationfallback)
        {
            if (engine.EndConversationFallback == null)
            {
                engine.EndConversationFallback = new Localization.LocalizedString("End Conversation");
            }
            if (GUILayout.Button("Edit text", EditorStyles.miniButton))
            {
                Localization.LocalizedStringEditor.OpenEdit(engine.EndConversationFallback);
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button("Edit Dialogs"))
        {
            DialogEditor.ShowWindow(engine);
        }
        GUILayout.Space(10);
    }
}
