namespace DialogSystem.Requirements
{
    public abstract class DialogRequirement : DialogAttribute
    {
        public abstract bool Evaluate(IDialogRelevantPlayer player, IDialogRelevantNpc npc, IDialogRelevantWorld worldInfo);
    }
}