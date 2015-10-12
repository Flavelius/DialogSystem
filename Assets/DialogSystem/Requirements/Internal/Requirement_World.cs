using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DialogSystem.Requirements.Internal;

namespace DialogSystem.Requirements
{
    public abstract class Requirement_World : BaseRequirement
    {
        public override sealed bool Evaluate(IDialogRelevantPlayer player, IDialogRelevantNPC npc, IDialogRelevantWorld worldInfo)
        {
            return Evaluate(worldInfo);
        }

        protected abstract bool Evaluate(IDialogRelevantWorld worldInfo);
    }
}
