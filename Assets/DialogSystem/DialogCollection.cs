using System.Collections.Generic;
using DialogSystem.Internal;
using UnityEngine;

namespace DialogSystem
{
    [CreateAssetMenu]
    public class DialogCollection: ScriptableObject
    {
        [HideInInspector] public List<Dialog> Dialogs = new List<Dialog>();

        public List<int> GetUsedIds()
        {
            var usedIDs = new List<int>();
            foreach (var dialog in Dialogs)
            {
                RetrieveIDs(dialog, usedIDs);
            }
            return usedIDs;
        }

        static void RetrieveIDs(Dialog current, List<int> idList)
        {
            idList.Add(current.ID);
            foreach (var dialogOption in current.Options)
            {
                if (dialogOption.IsRedirection) continue;
                if (dialogOption.NextDialog == null) continue;
                RetrieveIDs(dialogOption.NextDialog, idList);
            }
        }
    }
}
