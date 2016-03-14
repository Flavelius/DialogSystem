
namespace DialogSystem
{
    public enum DialogTargetSpecifier
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

    public enum DialogLanguage
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

    public enum ConversationType
    {
        Single,
        TopicList
    }
}
