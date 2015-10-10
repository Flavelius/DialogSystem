using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DialogSystem.Requirements.Internal;

namespace DialogSystem.Requirements
{
    public abstract class Requirement_Player : BaseRequirement
    {
        public override DialogRequirementTarget Target
        {
            get { return DialogRequirementTarget.Player; }
        }

        public override sealed bool Evaluate(IDialogRelevantPlayer player, IDialogRelevantNPC npc, IDialogRelevantWorldInfo worldInfo)
        {
            return Evaluate(player);
        }

        protected abstract bool Evaluate(IDialogRelevantPlayer player);
    }
}
