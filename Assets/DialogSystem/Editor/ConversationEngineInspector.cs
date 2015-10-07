using UnityEngine;
using UnityEditor;
using System.Collections;
using DialogSystem;

[CustomEditor(typeof(ConversationEngine))]
public class ConversationEngineInspector : Editor
{

    private LocalizedStringEditor activeStringEditor;

    public override void OnInspectorGUI()
    {
        ConversationEngine engine = target as ConversationEngine;
        engine.SavedDialogs = EditorGUILayout.ObjectField(new GUIContent("Dialogs"), engine.SavedDialogs, typeof(DialogCollection), false) as DialogCollection;
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
            if (GUILayout.Button("Edit text", EditorStyles.miniButton))
            {
                activeStringEditor = new LocalizedStringEditor(engine.EndConversationFallback, "Fallback option text", false);
            }
        }
        GUILayout.EndHorizontal();
        if (activeStringEditor != null)
        {
            if (activeStringEditor.DrawGUI() == false)
            {
                activeStringEditor.EndEdit();
                activeStringEditor = null;
            }
        }
        GUILayout.Space(10);
        GUI.enabled = engine.SavedDialogs != null;
        if (GUILayout.Button("Edit Dialogs"))
        {
            DialogEditor.OpenEdit(engine.SavedDialogs);
        }
        GUILayout.Space(10);
    }
}
