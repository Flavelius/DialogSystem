
namespace DialogSystem
{
    public interface IDialogNotificationReceiver
    {
        void OnDialogNotification(IDialogRelevantPlayer player, IDialogRelevantNPC npc, IDialogNotification notification);
    }

    public interface IDialogNotification
    {
        DialogNotificationType Type { get; }
        string Value { get; }
    }
}