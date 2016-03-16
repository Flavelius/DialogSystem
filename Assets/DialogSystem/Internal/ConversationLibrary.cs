using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DialogSystem.Internal
{
    [CreateAssetMenu]
    public class ConversationLibrary : ScriptableObject
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        [SerializeField, HideInInspector] List<DialogCollection> _collections = new List<DialogCollection>();

        public DialogCollection GetDialogCollection(string collectionName)
        {
            for (var i = 0; i < _collections.Count; i++)
            {
                if (_collections[i].name.Equals(collectionName, StringComparison.OrdinalIgnoreCase))
                {
                    return _collections[i];
                }
            }
            return null;
        }

#if UNITY_EDITOR

        [SerializeField, HideInInspector] int _lastUsedDialogID;

        public List<DialogCollection> EditorGetCollections()
        {
            return _collections;
        } 

        public bool EditorAllowOverrideOldIDs = false;

        public int EditorGetNewId(DialogCollection collection)
        {
            var usedIDs = new HashSet<int>();
            for (var i = _collections.Count; i-- > 0;)
            {
                //var objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(_collections[i]));
                //foreach (var o in objs)
                //{
                //    var d = o as Dialog;
                //    if (d != null)
                //    {
                //        usedIDs.Add(d.ID);
                //    }
                //}
                var ids = _collections[i].GetUsedIds();
                foreach (var id in ids)
                {
                    usedIDs.Add(id);
                }
            }
            var freeId = EditorAllowOverrideOldIDs ? 0 : _lastUsedDialogID;
            while (usedIDs.Contains(freeId))
            {
                freeId++;
            }
            return freeId;
        }
#endif
    }
}