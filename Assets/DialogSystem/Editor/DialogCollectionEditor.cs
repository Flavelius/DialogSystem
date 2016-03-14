using UnityEditor;
using UnityEngine;

namespace DialogSystem
{
    [CustomEditor(typeof (DialogCollection))]
    public class DialogCollectionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Edit"))
            {
                DialogEditor.OpenEdit(target as DialogCollection);
            }
        }
    }
}