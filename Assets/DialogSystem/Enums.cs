namespace DialogSystem
{
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
