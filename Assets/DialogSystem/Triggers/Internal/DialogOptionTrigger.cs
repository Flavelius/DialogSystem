using UnityEngine;
using System.Collections;
using System;

namespace DialogSystem.Triggers
{
    public abstract class DialogOptionTrigger: ScriptableObject
    {
        public abstract void Execute(Dialog activeDialog, IDialogRelevantPlayer player, IDialogRelevantNPC npc, IDialogRelevantWorldInfo worldInfo);

        [NonSerialized]
        private string cachedName = "";
        public virtual string CachedName { get { return cachedName; } }

        public virtual Color GetColor() { return Color.white; }

        public virtual string GetToolTip() { return this.GetType().Name; }

        public virtual string GetShortIdentifier() { return this.GetType().Name[0].ToString(); }

        void OnEnable()
        {
            ReadableNameAttribute[] rns = GetType().GetCustomAttributes(typeof(ReadableNameAttribute), false) as ReadableNameAttribute[];
            if (rns.Length > 0)
            {
                cachedName = rns[0].Name;
            }
            else
            {
                cachedName = GetType().Name;
            }
        }
    }
}