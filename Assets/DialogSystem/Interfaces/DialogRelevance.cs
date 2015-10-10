namespace DialogSystem
{
    public interface IDialogRelevantPlayer : IDialogNotificationReceiver
    {
        int IntValue { get; }
    }

    public interface IDialogRelevantNPC : IDialogNotificationReceiver
    {

    }

    public interface IDialogRelevantWorldInfo : IDialogNotificationReceiver
    {

    }
}