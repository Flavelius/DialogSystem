using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DialogSystem.Requirements.Internal
{
    /// <summary>
    /// Dont inherit from this, use Requirement_x (player, npc, world) instead
    /// </summary>
    public abstract class BaseRequirement : ScriptableObject
    {
        public abstract DialogTargetSpecifier Target { get; }

        public abstract bool Evaluate(IDialogRelevantPlayer player, IDialogRelevantNPC npc, IDialogRelevantWorldInfo worldInfo);

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
