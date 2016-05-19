using UnityEngine;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace DialogSystem
{
    [CustomEditor(typeof (DialogCollection))]
    public class DialogCollectionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var collection = target as DialogCollection;
            if (!collection) return;
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox(string.Format("{0} Conversations", collection.Dialogs.Count), MessageType.None);
            if (GUILayout.Button("Edit", GUILayout.ExpandWidth(false)))
            {
                DialogEditor.OpenEdit(target as DialogCollection);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}