using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace DialogSystem.Internal
{
    public class DialogIdAllocator: ScriptableObject
    {
        [SerializeField, HideInInspector] int _lastUsedDialogId;

        //if id's are saved somewhere and represent progress, resetting can lead to unexpected results
        [ContextMenu("Reset ID search to start from 0 (use with care!)")]
        // ReSharper disable once UnusedMember.Local
        void ResetCounter()
        {
            _lastUsedDialogId = 0;
        }

        public int GetNewId(DialogCollection collection)
        {
            var collections = new List<DialogCollection>();
            var foundAssets = AssetDatabase.FindAssets("t:" + typeof(DialogCollection).Name);
            for (var i = 0; i < foundAssets.Length; i++)
            {
                var lib = AssetDatabase.LoadAssetAtPath<DialogCollection>(AssetDatabase.GUIDToAssetPath(foundAssets[i]));
                if (lib != null)
                {
                    collections.Add(lib);
                }
            }
            var usedIDs = new HashSet<int>();
            foreach (var library in collections)
            {
                var ids = library.GetUsedIds();
                foreach (var id in ids)
                {
                    usedIDs.Add(id);
                }
            } 
            var freeId = _lastUsedDialogId;
            while (usedIDs.Contains(freeId))
            {
                freeId++;
            }
            _lastUsedDialogId = freeId;
            return freeId;
        }
    }
}
