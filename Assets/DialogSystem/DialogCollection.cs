using System.Collections.Generic;
using UnityEngine;

namespace DialogSystem
{
    [CreateAssetMenu]
    public class DialogCollection: ScriptableObject
    {
        public List<Dialog> Dialogs = new List<Dialog>();

        public int GetUniqueId()
        {
            var usedIDs = new List<int>();
            foreach (var dialog in Dialogs)
            {
                RetrieveIDs(dialog, usedIDs);
            }
            var newId = 0;
            while (usedIDs.Contains(newId))
            {
                newId++;
            }
            return newId;
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
