using UnityEngine;

namespace DialogSystem.Requirements
{
    public class Requirement_IntValue: Requirement_Player
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
