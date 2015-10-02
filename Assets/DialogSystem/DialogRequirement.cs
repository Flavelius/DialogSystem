using System.Runtime.Serialization;
using UnityEngine;

[DataContract]
public class DialogRequirement: IDialogRequirement
{

    [SerializeField, HideInInspector]
    private DialogRequirementType type = DialogRequirementType.State;
    [DataMember]
    public DialogRequirementType Type
    {
        get { return type; }
        set { type = value; }
    }

    [SerializeField, HideInInspector]
    private DialogRequirementTarget target = DialogRequirementTarget.Npc;
    [DataMember]
    public DialogRequirementTarget Target
    {
        get { return target; }
        set { target = value; }
    }

    [SerializeField, HideInInspector]
    private int intValue = 0;
    [DataMember]
    public int IntValue
    {
        get { return intValue; }
        set { intValue = value; }
    }

    [SerializeField, HideInInspector]
    private float floatValue = 0;
    [DataMember]
    public float FloatValue
    {
        get { return floatValue; }
        set { floatValue = value; }
    }
    

    [SerializeField, HideInInspector]
    private string stringValue = "";
    [DataMember]
    public string StringValue
    {
        get { return stringValue; }
        set { stringValue = value; }
    }

    public string ShortIdentifier
    {
        get
        {
            return string.Format("{0}{1}", target.ToString()[0], type.ToString()[0]);
        }
    }

    public Color GetColor()
    {
        return new Color((int)target * 0.1f, (int)type * 0.1f, (int)type * 0.1f, 1f);
    }

}

