using UnityEngine;
using System.Collections;

public interface IDialogRequirement
{
    DialogRequirementType Type { get; }
    int IntValue { get; }
    string StringValue { get; }
    float FloatValue { get; }
}
