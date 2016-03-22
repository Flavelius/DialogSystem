using System;
using System.Collections.Generic;
using DialogSystem.Internal;
using UnityEditor;
using UnityEngine;

namespace DialogSystem
{
    [CustomEditor(typeof (ConversationLibrary))]
    public class ConversationLibraryEditor : Editor
    {
        List<DialogCollection> _collections = new List<DialogCollection>();
        Vector2 _collectionScroll;
        string _newCollectionName = string.Empty;
        bool _showAddInterface;
        string _filter = "";

        public override void OnInspectorGUI()
        {
            GUI.enabled = !DialogEditor.IsOpen;
            EditorGUILayout.HelpBox("Conversation Library", MessageType.None);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Filter:");
            var prevColor = GUI.backgroundColor;
            GUI.backgroundColor = !string.IsNullOrEmpty(_filter) ? Color.green : prevColor;
            _filter = EditorGUILayout.TextField(_filter);
            GUI.backgroundColor = prevColor;
            GUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(string.Format("{0} Collections", _collections.Count));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(_showAddInterface ? "Cancel" : "Add"))
            {
                _showAddInterface = !_showAddInterface;
            }
            EditorGUILayout.EndHorizontal();
            if (_showAddInterface)
            {
                prevColor = GUI.backgroundColor;
                GUI.backgroundColor = NameExists(_newCollectionName) ? Color.red : Color.white;
                _newCollectionName = EditorGUILayout.TextField("Unique name:", _newCollectionName);
                GUI.backgroundColor = prevColor;
                var prevEnabled = GUI.enabled;
                GUI.enabled = !string.IsNullOrEmpty(_newCollectionName) && !NameExists(_newCollectionName);
                if (GUILayout.Button("Add"))
                {
                    var newCollection = CreateInstance<DialogCollection>();
                    if (Add(newCollection, _newCollectionName))
                    {
                        _showAddInterface = false;
                        ReloadAsset();
                    }
                }
                GUI.enabled = prevEnabled;
            }
            ListCollections();
        }

        void ListCollections()
        {
            _collectionScroll = EditorGUILayout.BeginScrollView(_collectionScroll);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            for (var i = 0; i < _collections.Count; i++)
            {
                if (!string.IsNullOrEmpty(_filter) && _collections[i].name.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) <0)
                {
                    continue;
                }
                var delete = false;
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label(_collections[i].name, GUILayout.Width(100));
                if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.Width(32)))
                {
                    DialogEditor.OpenEdit(_collections[i]);
                }
                GUILayout.Label(string.Format("{0} Dialogs", _collections[i].Dialogs.Count));    
                if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(24)))
                {
                    delete = true;
                }
                EditorGUILayout.EndHorizontal();
                if (delete)
                {
                    RemoveAndDelete(_collections[i]);
                    ReloadAsset();
                    break;
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        void OnEnable()
        {
            _collections = (target as ConversationLibrary).EditorGetCollections();
            for (var i = _collections.Count; i-- > 0;)
            {
                if (_collections[i] == null)
                {
                    _collections.RemoveAt(i);
                }
            }
        }

        bool NameExists(string collectionName)
        {
            for (var i = 0; i < _collections.Count; i++)
            {
                if (_collections[i].name.Equals(collectionName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        bool Add(DialogCollection collection, string collectionName)
        {
            if (collection == null) return false;
            if (_collections.Contains(collection)) return false;
            if (string.IsNullOrEmpty(collectionName)) return false;
            if (NameExists(collectionName)) return false;
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(target))) return false;
            collection.name = collectionName;
            _collections.Add(collection);
            AssetDatabase.AddObjectToAsset(collection, target);
            EditorUtility.SetDirty(target);
            return true;
        }

        void RemoveAndDelete(DialogCollection collection)
        {
            if (collection == null) return;
            if (!_collections.Remove(collection)) return;
            DestroyImmediate(collection, true);
            EditorUtility.SetDirty(target);
        }

        void ReloadAsset()
        {
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
        }
    }
}