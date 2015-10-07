using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using DialogSystem;

[CustomEditor(typeof(DialogCollection))]
public class DialogCollectionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Edit"))
        {
            DialogEditor.OpenEdit((target as DialogCollection));
        }
    }
}
