using UnityEngine;

namespace DialogSystem.Requirements
{
    [ReadableName("Player Int Value")]
    public class RequireIntValue: Requirement_Player
    {
        public int intValue;

        protected override bool Evaluate(IDialogRelevantPlayer player)
        {
            return intValue == player.IntValue;
        }

        public override Color GetColor()
        {
            return Color.green; 
        }
    }
}
