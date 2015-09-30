using UnityEngine;
using System.Collections.Generic;

public class DialogTester : MonoBehaviour, IConversationRelevance
{
    public string Name
    {
        get { return "Player"; }
    }

    public string npcName = "TestNPC";

    public Localization.Language language;

    public int intValue;
    public string stringValue;
    public float floatValue;

    public int npcInt;
    public string npcString;
    public float npcFloat;

    public int worldInt;
    public string worldString;
    public float worldFloat;

    private Conversation c;
    void OnGUI()
    {
        if (c == null)
        {
            if (GUILayout.Button("Get Dialog"))
            {
                List<Conversation> cons = ConversationEngine.Instance.GetAvailableTopics(npc, this, world, language);
                if (cons.Count > 0)
                {
                    c = cons[0];
                }
            }
        }
        else
        {
            GUILayout.Label(c.Title);
            GUILayout.Label(c.Text);
            for (int i = 0; i < c.Answers.Count; i++)
            {
                if (GUILayout.Button(c.Answers[i].Text))
                {
                    c = ConversationEngine.Instance.Answer(npc, this, world, c.ID, c.Answers[i].Index, language);
                    break;
                }
            }
        }
    }

    public bool ValidateDialogRequirement(IDialogRequirement req)
    {
        switch (req.Type)
        {
            case DialogRequirementType.State:
            case DialogRequirementType.PastState:
            case DialogRequirementType.Flag:
                return intValue == req.IntValue;
            case DialogRequirementType.EventLog:
                return stringValue == req.StringValue;
            case DialogRequirementType.LifeTime:
                return floatValue > req.FloatValue;
            default:
                return true;
        }
    }

    void Update()
    {
        npc.Name = npcName;
        npc.intValue = npcInt;
        npc.stringValue = npcString;
        npc.floatValue = npcFloat;

        world.intValue = worldInt;
        world.stringValue = worldString;
        world.floatValue = worldFloat;
    }

    private TestNpc npc = new TestNpc();
    private TestWorld world = new TestWorld();

    public DialogRequirementTarget Type
    {
        get { return DialogRequirementTarget.Player; }
    }

    public void OnDialogNotification(IConversationRelevance player, IConversationRelevance npc, IDialogNotification notification)
    {
        Debug.Log(Name + " received notification: " + notification.Type + " with value: " + notification.Value);
    }

    private class TestWorld : IConversationRelevance
    {

        public string Name
        {
            get { return "World"; }
        }

        public int intValue;
        public string stringValue;
        public float floatValue;

        public bool ValidateDialogRequirement(IDialogRequirement req)
        {
            switch (req.Type)
            {
                case DialogRequirementType.State:
                case DialogRequirementType.PastState:
                case DialogRequirementType.Flag:
                    return intValue == req.IntValue;
                case DialogRequirementType.EventLog:
                    return stringValue == req.StringValue;
                case DialogRequirementType.LifeTime:
                    return floatValue > req.FloatValue;
                default:
                    return true;
            }
        }

        public void OnDialogNotification(IConversationRelevance player, IConversationRelevance npc, IDialogNotification notification)
        {
            Debug.Log(Name+" received notification: " + notification.Type + " with value: " + notification.Value);
        }

        public DialogRequirementTarget Type
        {
            get { return DialogRequirementTarget.World; }
        }
    }

    private class TestNpc : IConversationRelevance
    {

        private string name = "Npc";
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int intValue;
        public string stringValue;
        public float floatValue;

        public bool ValidateDialogRequirement(IDialogRequirement req)
        {
            switch (req.Type)
            {
                case DialogRequirementType.State:
                case DialogRequirementType.PastState:
                case DialogRequirementType.Flag:
                    return intValue == req.IntValue;
                case DialogRequirementType.EventLog:
                    return stringValue == req.StringValue;
                case DialogRequirementType.LifeTime:
                    return floatValue > req.FloatValue;
                default:
                    return true;
            }
        }

        public void OnDialogNotification(IConversationRelevance player, IConversationRelevance npc, IDialogNotification notification)
        {
            Debug.Log(Name + " received notification: " + notification.Type + " with value: " + notification.Value);
        }


        public DialogRequirementTarget Type
        {
            get { return DialogRequirementTarget.Npc; }
        }
    }

}
