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
        public abstract DialogRequirementTarget Target { get; }

        public abstract bool Evaluate(IDialogRelevantPlayer player, IDialogRelevantNPC npc, IDialogRelevantWorldInfo worldInfo);

        public virtual Color GetColor() { return Color.white; }

        public virtual string GetToolTip() { return this.GetType().Name; }

        public virtual string GetShortIdentifier() { return this.GetType().Name[0].ToString(); }
    }
}
