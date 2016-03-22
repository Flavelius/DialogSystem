using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DialogSystem.Internal
{
    public class DialogIdAllocator: ScriptableObject
    {
        [SerializeField, HideInInspector] int _lastUsedDialogId;

        [ContextMenu("Reset ID search to start from 0 (use with care!)")]
        void ResetCounter()
        {
            _lastUsedDialogId = 0;
        }

        public int GetNewId(DialogCollection collection)
        {
            var libraries = new List<ConversationLibrary>();
            var allLibraries = AssetDatabase.FindAssets("t:" + typeof(ConversationLibrary).Name);
            for (var i = 0; i < allLibraries.Length; i++)
            {
                var lib = AssetDatabase.LoadAssetAtPath<ConversationLibrary>(allLibraries[i]);
                if (lib != null)
                {
                    libraries.Add(lib);
                }
            }
            var usedIDs = new HashSet<int>();
            foreach (var library in libraries)
            {
                for (var i = library.EditorGetCollections().Count; i-- > 0;)
                {
                    var ids = library.EditorGetCollections()[i].GetUsedIds();
                    foreach (var id in ids)
                    {
                        usedIDs.Add(id);
                    }
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
