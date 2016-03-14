using UnityEngine;

namespace DialogSystem.Requirements
{
    [ReadableName("Player Int Value")]
    public class RequireIntValue: DialogRequirement
    {
        public int IntValue;

        public override bool Evaluate(IDialogRelevantPlayer player, IDialogRelevantNpc npc, IDialogRelevantWorld worldInfo)
        {
            return player != null && IntValue == player.GetIntValue();
        }

        public override Color GetColor()
        {
            return Color.green; 
        }
    }
}
