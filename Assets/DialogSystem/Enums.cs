
namespace DialogSystem
{
    public enum DialogRequirementTarget
    {
        Npc = 4,
        Player = 7,
        World = 10,
    }

    public enum DialogRequirementMode
    {
        And,
        Or
    }

    public enum DialogNotificationType
    {
        DialogCompleted,
        Other
    }

    public enum Language
    {
        EN_Default,
        DE
    }

    public enum LocalizationFallback
    {
        Language,
        DebugOutput,
        EmptyString
    }
}
