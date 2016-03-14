namespace DialogSystem
{
    public interface IDialogRelevantPlayer
    {
        int GetIntValue();
        void OnDialogCompleted(int id);
    }

    public interface IDialogRelevantNpc
    {

    }

    public interface IDialogRelevantWorld
    {

    }
}