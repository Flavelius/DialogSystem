using UnityEngine;
using System.Collections;

public enum DialogRequirementType
{
    None = 4,
    State = 5,
    PastState = 6,
    Flag = 7,
    LifeTime = 8,
    EventLog = 9,
    DialogLog = 10,
    TaskLog = 11
}

public interface IDialogRequirement
{
    DialogRequirementType Type { get; }
    int IntValue { get; }
    string StringValue { get; }
    float FloatValue { get; }
}

public enum DialogRequirementTarget
{
    Npc = 4,
    Player = 7,
    World = 10,
}

public enum DialogNotificationType
{
    DialogCompleted,
    Other
}

public interface IDialogNotificationReceiver
{
    void OnDialogNotification(IConversationRelevance player, IConversationRelevance npc, IDialogNotification notification);
}

public interface IConversationRelevance : IDialogNotificationReceiver
{
    string Name { get; }
    bool ValidateDialogRequirement(IDialogRequirement req);
    DialogRequirementTarget Type { get; }
}

public interface IDialogNotification
{
    DialogNotificationType Type { get; }
    string Value { get; }
}
