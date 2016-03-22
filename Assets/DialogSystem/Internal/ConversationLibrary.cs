using System;
using System.Collections.Generic;
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
        public List<DialogCollection> EditorGetCollections()
        {
            return _collections;
        } 
#endif
    }
}