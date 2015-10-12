using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace DialogSystem.Internal
{
    public class DialogIDTracker : ScriptableObject
    {
        List<DialogCollection> dialogCollections = new List<DialogCollection>();

        private void RegisterCollection(DialogCollection collection)
        {
            if (dialogCollections.Contains(collection)) { return; }
            dialogCollections.Add(collection);
        }

        [SerializeField]
        public int nextIDSearchStart;

        public int GetNewID(DialogCollection collection)
        {
            RegisterCollection(collection);
            HashSet<int> usedIDs = new HashSet<int>();
            for (int i = dialogCollections.Count; i-- > 0; )
            {
                if (dialogCollections[i] == null) { dialogCollections.RemoveAt(i); continue; }
                object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(dialogCollections[i]));
                foreach (object o in objs)
                {
                    Dialog d = o as Dialog;
                    if (d != null)
                    {
                        usedIDs.Add(d.ID);
                    }
                }
            }
            int freeID = nextIDSearchStart;
            while (usedIDs.Contains(freeID))
            {
                freeID++;
            }
            return freeID;
        }
    }
}
