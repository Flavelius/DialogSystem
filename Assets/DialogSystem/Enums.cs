
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
