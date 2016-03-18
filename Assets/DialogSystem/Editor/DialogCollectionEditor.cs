using UnityEngine;
using UnityEditor;

namespace DialogSystem
{
    [CustomEditor(typeof (DialogCollection))]
    public class DialogCollectionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Edit", GUILayout.ExpandWidth(false)))
            {
                DialogEditor.OpenEdit(target as DialogCollection);
            }
        }
    }
}