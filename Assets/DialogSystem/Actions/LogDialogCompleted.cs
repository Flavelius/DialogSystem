using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DialogSystem.Actions
{
    [ReadableName("Log player dialog completed")]
    public class LogDialogCompleted: DialogOptionAction
    {
        public override UnityEngine.Color GetColor()
        {
            return Color.cyan;
        }

        public override void Execute(Dialog activeDialog, IDialogRelevantPlayer player, IDialogRelevantNPC npc, IDialogRelevantWorld worldInfo)
        {
            player.OnDialogCompleted(activeDialog.ID);
        }
    }
}
