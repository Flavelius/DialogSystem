using DialogSystem;
using DialogSystem.Localization;
using System.Collections.Generic;
using UnityEngine;

namespace DialogSystem
{
    public class ConversationEngine : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private DialogCollection savedDialogs;
        /// <summary>
        /// The saved collection of dialogs to use for requests
        /// </summary>
        public DialogCollection SavedDialogs
        {
            get { return savedDialogs; }
            set { savedDialogs = value; }
        }

        /// <summary>
        /// What to do, when a dialog has no -, or doesn't contain text in the requested language
        /// </summary>
        public LocalizationFallback fallback = LocalizationFallback.DebugOutput;

        /// <summary>
        /// What language to resort to, when the supplied one yields no results, if <see cref="fallback"/> is set to language
        /// </summary>
        public Language fallbackLanguage = Language.EN_Default;

        /// <summary>
        /// if no dialogoptions are available (requirements not met), inject <see cref="EndConversationFallback"/>
        /// </summary>
        public bool UseEndConversationfallback = false;

        /// <summary>
        /// The default end conversation fallback text (if <see cref="UseEndConversationfallback"/> is set)
        /// </summary>
        [SerializeField]
        public LocalizedString EndConversationFallback = new LocalizedString("End Conversation");

        /// <summary>
        /// all loaded conversations are stored here
        /// </summary>
        private List<Dialog> conversations = new List<Dialog>();

        void Start()
        {
            if (savedDialogs == null)
            {
                Debug.LogWarning("Dialogs-file not specified, no dialogs will be available");
                return;
            }
            LoadDialogs(savedDialogs);
        }

        /// <summary>
        /// Loads all saved dialogs from the specified save file
        /// </summary>
        /// <returns>returns true if loading was successful</returns>
        public bool LoadDialogs(DialogCollection collection)
        {
            if (collection == null) { return false; }
            conversations = collection.dialogs;
            return true;
        }

        /// <summary>
        /// Requests a list of topics
        /// </summary>
        /// <param name="npc">Required, reference to the topics owning npc</param>
        /// <param name="player">Required, reference to the conversing player</param>
        /// <param name="worldContext">Not required, but could be, depending on the settings of certain dialogs</param>
        /// <param name="language">The language the conversing player should receive an answer in</param>
        /// <returns></returns>
        public Conversation GetAvailableTopics(IDialogRelevantNPC npc, IDialogRelevantPlayer player, IDialogRelevantWorld worldContext, Language language)
        {
            List<Dialog> availableTopics = new List<Dialog>();
            for (int i = 0; i < conversations.Count; i++)
            {
                if (CheckAvailability(conversations[i], npc, player, worldContext))
                {
                    availableTopics.Add(conversations[i]);
                }
            }
            if (availableTopics.Count == 1)
            {
                string title = conversations[0].Title.GetString(language, fallback, fallbackLanguage);
                string text = conversations[0].GetText().GetString(language, fallback, fallbackLanguage);
                return Conversation.Construct(availableTopics[0].ID, title, text, availableTopics[0].Tag, Conversation.ConversationType.Single, GetAvailableAnswers(availableTopics[0], npc, player, worldContext, language));
            }
            else if (availableTopics.Count > 1)
            {
                Conversation c = Conversation.Construct(-1, "", "", "", Conversation.ConversationType.TopicList, new List<Conversation.Answer>());
                foreach (Dialog d in availableTopics)
                {
                    string title = d.Title.GetString(language, fallback, fallbackLanguage);
                    Conversation.Answer ca = new Conversation.Answer(d.ID, title, d.Tag);
                    c.Answers.Add(ca);
                }
                return c;
            }
            return null;
        }

        /// <summary>
        /// Retrieves the dialog following the supplied answer from a previous conversation
        /// </summary>
        /// <param name="npc">Required, reference to the topics owning npc</param>
        /// <param name="player">Required, reference to the conversing player</param>
        /// <param name="worldContext">Not required, but could be, depending on the settings of certain dialogs</param>
        /// <param name="previous">Conversation the answer is based on</param>
        /// <param name="answerIndex">The index of the answer of the answered dialog, or dialogID if answer came from topicList</param>
        /// <param name="language">The language the conversing player should receive an answer in</param>
        /// <returns></returns>
        public Conversation Answer(IDialogRelevantNPC npc, IDialogRelevantPlayer player, IDialogRelevantWorld worldContext, Conversation previous, Conversation.Answer answer, Language language)
        {
            if (previous == null) { return null; }
            Dialog activeDialog = null;
            if (previous.ID == -1)
            {
                activeDialog = GetDialog(answer.Index);
                if (activeDialog == null || !CheckAvailability(activeDialog, npc, player, worldContext))
                {
                    Debug.LogWarning("Selection from topicList invalid");
                    return null;
                }
                else
                {
                    string title = activeDialog.Title.GetString(language, fallback, fallbackLanguage);
                    string text = activeDialog.GetText().GetString(language, fallback, fallbackLanguage);
                    return Conversation.Construct(activeDialog.ID, title, text, activeDialog.Tag, Conversation.ConversationType.Single, GetAvailableAnswers(activeDialog, npc, player, worldContext, language));
                }
            }
            else
            {
                activeDialog = GetDialog(previous.ID);
            }
            if (activeDialog == null) { return null; }
            if (answer.Index >= 0 && answer.Index < activeDialog.Options.Count)
            {
                DialogOption chosenOption = activeDialog.Options[answer.Index];
                for (int i = 0; i < chosenOption.Actions.Count; i++)
                {
                    chosenOption.Actions[i].Execute(activeDialog, player, npc, worldContext);
                }
                if (chosenOption.NextDialog != null)
                {
                    if (CheckAvailability(chosenOption.NextDialog, npc, player, worldContext))
                    {
                        string title = chosenOption.NextDialog.Title.GetString(language, fallback, fallbackLanguage);
                        string text = chosenOption.NextDialog.GetText().GetString(language, fallback, fallbackLanguage);
                        return Conversation.Construct(chosenOption.NextDialog.ID, title, text, chosenOption.NextDialog.Tag, Conversation.ConversationType.Single, GetAvailableAnswers(chosenOption.NextDialog, npc, player, worldContext, language));
                    }
                }
            }
            else
            {
                if (answer.Index == -1) { return null; } //close dialog
                Debug.LogWarning("AnswerIndex out of bounds");
            }
            return null;
        }

        private Dialog GetDialog(int id)
        {
            for (int i = 0; i < conversations.Count; i++)
            {
                Dialog d = FindDialog(conversations[i], id);
                if (d != null)
                {
                    return d;
                }
            }
            return null;
        }

        private Dialog FindDialog(Dialog current, int id)
        {
            if (current.ID == id)
            {
                return current;
            }
            for (int i = 0; i < current.Options.Count; i++)
            {
                if (current.Options[i].NextDialog != null)
                {
                    if (!current.Options[i].IsRedirection)
                    {
                        Dialog d = FindDialog(current.Options[i].NextDialog, id);
                        if (d != null)
                        {
                            return d;
                        }
                    }
                }
            }
            return null;
        }

        private bool CheckAvailability(Dialog d, IDialogRelevantNPC npc, IDialogRelevantPlayer player, IDialogRelevantWorld worldContext)
        {
            if (d.RequirementMode == DialogRequirementMode.And)
            {
                for (int i = 0; i < d.Requirements.Count; i++)
                {
                    if (!d.Requirements[i].Evaluate(player, npc, worldContext))
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (d.RequirementMode == DialogRequirementMode.Or)
            {
                for (int i = 0; i < d.Requirements.Count; i++)
                {
                    if (d.Requirements[i].Evaluate(player, npc, worldContext))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private List<Conversation.Answer> GetAvailableAnswers(Dialog d, IDialogRelevantNPC npc, IDialogRelevantPlayer player, IDialogRelevantWorld worldContext, Language language)
        {
            List<Conversation.Answer> answers = new List<Conversation.Answer>();
            for (int i = 0; i < d.Options.Count; i++)
            {
                if (d.Options[i].NextDialog == null)
                {
                    string text = d.Options[i].Text.GetString(language, fallback, fallbackLanguage);
                    answers.Add(new Conversation.Answer(i, text, d.Options[i].Tag));
                }
                else if (CheckAvailability(d.Options[i].NextDialog, npc, player, worldContext))
                {
                    string text = d.Options[i].Text.GetString(language, fallback, fallbackLanguage);
                    answers.Add(new Conversation.Answer(i, text, d.Options[i].Tag));
                }
            }
            if (answers.Count == 0 && UseEndConversationfallback)
            {
                answers.Add(new Conversation.Answer(-1, EndConversationFallback.GetString(language, fallback, fallbackLanguage), ""));
            }
            return answers;
        }

    }

    /// <summary>
    /// Holds the data used to display a requested dialog
    /// </summary>
    public class Conversation
    {
        public enum ConversationType { Single, TopicList }

        private Conversation(int id, string title, string text, string tag, ConversationType type, List<Answer> answers)
        {
            ID = id;
            Title = title;
            Text = text;
            Tag = tag;
            Answers = answers;
            Type = type;
        }
        public static Conversation Construct(int id, string title, string text, string tag, ConversationType type, List<Answer> answers)
        {
            return new Conversation(id, title, text, tag, type, answers);
        }

        /// <summary>
        /// used by the conversation engine to identify a dialog
        /// </summary>
        public readonly int ID;

        public readonly string Title;
        public readonly string Text;

        /// <summary>
        /// User defined dialog tag (if <see cref="Type"/> is Single
        /// </summary>
        public readonly string Tag;

        /// <summary>
        /// available answers, if <see cref="Type"/> is Single, else available dialogs
        /// </summary>
        public readonly List<Answer> Answers;

        /// <summary>
        /// The type of dialog; if TopicList, it means more than one dialog is available and this conversation lists them in <see cref="Answers"/>
        /// </summary>
        public readonly ConversationType Type;

        public class Answer
        {
            public Answer(int index, string text, string tag)
            {
                Index = index;
                Text = text;
                Tag = tag;
            }
            public readonly int Index;
            public readonly string Text;
            public readonly string Tag;
        }
    }
}