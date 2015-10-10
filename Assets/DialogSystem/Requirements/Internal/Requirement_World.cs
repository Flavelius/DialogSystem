using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DialogSystem.Requirements.Internal;

namespace DialogSystem.Requirements
{
    public abstract class Requirement_World : BaseRequirement
    {
        public override DialogTargetSpecifier Target
        {
            get { return DialogTargetSpecifier.World; }
        }

        public override sealed bool Evaluate(IDialogRelevantPlayer player, IDialogRelevantNPC npc, IDialogRelevantWorldInfo worldInfo)
        {
            return Evaluate(worldInfo);
        }

        protected abstract bool Evaluate(IDialogRelevantWorldInfo worldInfo);
    }
}
