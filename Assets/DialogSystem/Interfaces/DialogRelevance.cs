namespace DialogSystem
{
    public interface IDialogRelevantPlayer
    {
        int IntValue { get; }
        void OnDialogCompleted(int id);
    }

    public interface IDialogRelevantNPC
    {

    }

    public interface IDialogRelevantWorld
    {

    }
}