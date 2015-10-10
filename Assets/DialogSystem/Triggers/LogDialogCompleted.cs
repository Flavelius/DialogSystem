using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DialogSystem.Triggers
{
    [ReadableName("Log player dialog completed")]
    public class LogDialogCompleted: DialogOptionTrigger
    {
        public override UnityEngine.Color GetColor()
        {
            return Color.cyan;
        }

        public override void Execute(Dialog activeDialog, IDialogRelevantPlayer player, IDialogRelevantNPC npc, IDialogRelevantWorldInfo worldInfo)
        {
            player.OnDialogCompleted(activeDialog.ID);
        }
    }
}
