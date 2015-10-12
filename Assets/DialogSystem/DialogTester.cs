using UnityEngine;
using System.Collections.Generic;
using DialogSystem;

namespace DialogSystem
{
    public class DialogTester : MonoBehaviour
    {
        public ConversationEngine conversationEngine;

        public Language language;

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
                        c = conversationEngine.Answer(npc, player, world, c, c.Answers[i], language);
                        break;
                    }
                }
            }
        }

        [SerializeField]
        private TestDialogNPC npc = new TestDialogNPC();
        [SerializeField]
        private TestDialogPlayer player = new TestDialogPlayer();
        [SerializeField]
        private TestDialogWorld world = new TestDialogWorld();

        [System.Serializable]
        public class TestDialogNPC : IDialogRelevantNPC
        {
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
        }

        [System.Serializable]
        public class TestDialogPlayer : IDialogRelevantPlayer
        {
            [SerializeField]
            private string name = "Player";
            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            public int intValue;
            public int IntValue { get { return intValue; } }
            public string stringValue;
            public float floatValue;

            public void OnDialogCompleted(int id)
            {
                Debug.Log("Dialog completed: "+id);
            }
        }

        [System.Serializable]
        public class TestDialogWorld : IDialogRelevantWorld
        {
            [SerializeField]
            private string name = "World";
            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            public int intValue;
            public string stringValue;
            public float floatValue;
        }

    }
}