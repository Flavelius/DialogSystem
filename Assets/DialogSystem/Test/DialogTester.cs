using System;
using UnityEngine;

namespace DialogSystem.Test
{
    public class DialogTester : MonoBehaviour
    {
        Conversation _activeConversation;
        public ConversationEngine DialogEngine;

        public DialogLanguage Language;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        [SerializeField] TestDialogNpc _npc = new TestDialogNpc();

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        [SerializeField] TestDialogPlayer _player = new TestDialogPlayer();

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        [SerializeField] TestDialogWorld _world = new TestDialogWorld();

        void OnGUI()
        {
            if (_activeConversation == null)
            {
                if (GUILayout.Button("Get Dialog"))
                {
                    _activeConversation = DialogEngine.GetAvailableTopics(_npc, _player, _world, Language);
                }
            }
            else
            {
                if (_activeConversation.Type == ConversationType.Single)
                {
                    GUILayout.Label(_activeConversation.Title);
                    GUILayout.Label(_activeConversation.Text);
                }
                for (var i = 0; i < _activeConversation.Answers.Count; i++)
                {
                    if (!GUILayout.Button(_activeConversation.Answers[i].Text)) continue;
                    _activeConversation = DialogEngine.Answer(_npc, _player, _world, _activeConversation, _activeConversation.Answers[i], Language);
                    break;
                }
            }
        }

        [Serializable]
        public class TestDialogNpc : IDialogRelevantNpc
        {
            [SerializeField] string _name = "Npc";
            public float FloatValue;

            public int IntValue;

            public string StringValue;

            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }
        }

        [Serializable]
        public class TestDialogPlayer : IDialogRelevantPlayer
        {
            [SerializeField] string _name = "Player";
            public float FloatValue;

            public int IntValue;

            public string StringValue;

            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }

            int IDialogRelevantPlayer.GetIntValue()
            {
                return IntValue;
            }

            void IDialogRelevantPlayer.OnDialogCompleted(int id)
            {
                Debug.Log(Name+": Dialog completed: " + id);
            }
        }

        [Serializable]
        public class TestDialogWorld : IDialogRelevantWorld
        {
            [SerializeField] string _name = "World";
            public float FloatValue;

            public int IntValue;

            public string StringValue;

            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }
        }
    }
}