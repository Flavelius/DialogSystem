using DialogSystem.Internal;

namespace DialogSystem.Actions
{
    public abstract class DialogOptionAction : DialogAttribute
    {
        public abstract void Execute(Dialog activeDialog, IDialogRelevantPlayer player, IDialogRelevantNpc npc, IDialogRelevantWorld worldInfo);     
    }
}