using UnityEngine;
using System.Collections.Generic;

public class DialogTester : MonoBehaviour
{

    public ConversationEngine conversationEngine;

    public Localization.Language language;

    private Conversation c;
    void OnGUI()
    {
        if (c == null)
        {
            if (GUILayout.Button("Get Dialog"))
            {
                c = conversationEngine.GetAvailableTopics(npc, player, world, language);
            }
        }
        else
        {
            if (c.Type == Conversation.ConversationType.Single)
            {
                GUILayout.Label(c.Title);
                GUILayout.Label(c.Text);
            }
            for (int i = 0; i < c.Answers.Count; i++)
            {
                if (GUILayout.Button(c.Answers[i].Text))
                {
                    c = conversationEngine.Answer(npc, player, world, c.ID, c.Answers[i].Index, language);
                    break;
                }
            }
        }
    }

    [SerializeField]
    private TestDialogEntity npc = new TestDialogEntity("Npc");
    [SerializeField]
    private TestDialogEntity player = new TestDialogEntity("Player");
    [SerializeField]
    private TestDialogEntity world = new TestDialogEntity("World");

    [System.Serializable]
    public class TestDialogEntity : IConversationRelevance
    {

        public TestDialogEntity(string name)
        {
            this.name = name;
        }

        [SerializeField]
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
