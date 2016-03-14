using DialogSystem.Internal;
using UnityEngine;

namespace DialogSystem.Actions
{
    [ReadableName("player Log dialog completed")]
    public class LogDialogCompleted: DialogOptionAction
    {
        public override Color GetColor()
        {
            return Color.cyan;
        }

        public override void Execute(Dialog activeDialog, IDialogRelevantPlayer player, IDialogRelevantNpc npc, IDialogRelevantWorld worldInfo)
        {
            player.OnDialogCompleted(activeDialog.ID);
        }
    }
}
