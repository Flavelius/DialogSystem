using System;
using System.Collections.Generic;
using DialogSystem.Internal;
using DialogSystem.Localization;
using UnityEngine;

namespace DialogSystem
{
    public class ConversationEngine : MonoBehaviour
    {
        /// <summary>
        /// Format of the DebugOutput fallback if a dialog text is not available
        /// </summary>
        const string DebugStringFormat = "No Text: {0}";

        /// <summary>
        ///     All loaded conversations are stored here
        /// </summary>
        [NonSerialized] List<Dialog> _conversations = new List<Dialog>();

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        [SerializeField, HideInInspector] LocalizedString _endConversationFallback = new LocalizedString("End Conversation");

        /// <summary>
        ///     What to do, when a dialog has no -, or doesn't contain text in the requested language
        /// </summary>
        public LocalizationFallback Fallback = LocalizationFallback.DebugOutput;

        /// <summary>
        ///     What language to resort to, when the supplied one yields no results, if <see cref="Fallback" /> is set to language
        /// </summary>
        public DialogLanguage FallbackLanguage = DialogLanguage.EN_Default;

#pragma warning disable 649
        [SerializeField, HideInInspector] DialogCollection _savedDialogs;
#pragma warning restore 649

        /// <summary>
        ///     if no dialogoptions are available (requirements not met), inject <see cref="_endConversationFallback" />
        /// </summary>
        public bool UseEndConversationfallback;

        void Start()
        {
            if (_savedDialogs == null)
            {
                Debug.LogWarning(string.Format("Dialog collection not set for {0}, no dialogs will be available initially", gameObject.name));
                return;
            }
            LoadDialogs(_savedDialogs);
        }

        /// <summary>
        ///     Loads all saved dialogs from the specified collection
        /// </summary>
        /// <returns>returns true if loading was successful</returns>
        public bool LoadDialogs(DialogCollection collection)
        {
            if (collection == null || collection.Dialogs.Count == 0)
            {
                return false;
            }
            _conversations = collection.Dialogs;
            return true;
        }

        /// <summary>
        ///     Requests a list of topics
        /// </summary>
        /// <param name="npc">Required, reference to the topics owning npc</param>
        /// <param name="player">Required, reference to the conversing player</param>
        /// <param name="worldContext">Not required, but could be, depending on the settings of certain dialogs</param>
        /// <param name="language">The language the conversing player should receive an answer in</param>
        /// <returns></returns>
        public Conversation GetAvailableTopics(IDialogRelevantNpc npc, IDialogRelevantPlayer player, IDialogRelevantWorld worldContext, DialogLanguage language)
        {
            var availableTopics = new List<Dialog>();
            for (var i = 0; i < _conversations.Count; i++)
            {
                if (CheckAvailability(_conversations[i], npc, player, worldContext))
                {
                    availableTopics.Add(_conversations[i]);
                }
            }
            if (availableTopics.Count == 1) //return directly
            {
                var title = GetDialogStringOrFallback(language, _conversations[0].Title.GetString, _conversations[0].ID);
                var text = GetDialogStringOrFallback(language, _conversations[0].GetText().GetString, _conversations[0].ID);
                return new Conversation(availableTopics[0].ID, title, text, availableTopics[0].Tag, ConversationType.Single,
                    GetAvailableAnswers(availableTopics[0], npc, player, worldContext, language));
            }
            if (availableTopics.Count > 1) //create list
            {
                var answers = new List<Conversation.Answer>();
                for (var i = 0; i < availableTopics.Count; i++)
                {
                    var title = GetDialogStringOrFallback(language, availableTopics[i].Title.GetString, availableTopics[i].ID);
                    var ca = new Conversation.Answer(availableTopics[i].ID, title, availableTopics[i].Tag);
                    answers.Add(ca);
                }
                return new Conversation(-1, "", "", "", ConversationType.TopicList, answers);
            }
            return null;
        }

        /// <summary>
        ///     Retrieves the dialog following the supplied answer from a previous conversation
        /// </summary>
        /// <param name="npc">Required, reference to the topics owning npc</param>
        /// <param name="player">Required, reference to the conversing player</param>
        /// <param name="worldContext">Not required, but could be, depending on the settings of certain dialogs</param>
        /// <param name="previous">Conversation the answer is based on</param>
        /// <param name="answer">Answer of the previous dialog</param>
        /// <param name="language">The language the conversing player should receive an answer in</param>
        /// <returns></returns>
        public Conversation Answer(IDialogRelevantNpc npc, IDialogRelevantPlayer player, IDialogRelevantWorld worldContext, Conversation previous,
            Conversation.Answer answer, DialogLanguage language)
        {
            if (previous == null)
            {
                return null;
            }
            Dialog activeDialog;
            if (previous.ID == -1) //from list return selected
            {
                activeDialog = GetDialog(answer.Index);
                if (activeDialog == null || !CheckAvailability(activeDialog, npc, player, worldContext))
                {
                    Debug.LogWarning("Selection from topicList invalid");
                    return null;
                }
                var title = GetDialogStringOrFallback(language, activeDialog.Title.GetString, activeDialog.ID);
                var text = GetDialogStringOrFallback(language, activeDialog.GetText().GetString, activeDialog.ID);
                return new Conversation(activeDialog.ID, title, text, activeDialog.Tag, ConversationType.Single,
                    GetAvailableAnswers(activeDialog, npc, player, worldContext, language));
            }
            activeDialog = GetDialog(previous.ID);
            if (activeDialog == null)
            {
                return null;
            }
            if (answer.Index >= 0 && answer.Index < activeDialog.Options.Count)
            {
                var chosenOption = activeDialog.Options[answer.Index];
                for (var i = 0; i < chosenOption.Actions.Count; i++)
                {
                    chosenOption.Actions[i].Execute(activeDialog, player, npc, worldContext);
                }
                if (chosenOption.NextDialog == null) return null;
                if (chosenOption.IgnoreRequirements || CheckAvailability(chosenOption.NextDialog, npc, player, worldContext))
                {
                    var title = GetDialogStringOrFallback(language, chosenOption.NextDialog.Title.GetString, chosenOption.NextDialog.ID);
                    var text = GetDialogStringOrFallback(language, chosenOption.NextDialog.GetText().GetString, chosenOption.NextDialog.ID);
                    return new Conversation(chosenOption.NextDialog.ID, title, text, chosenOption.NextDialog.Tag, ConversationType.Single,
                        GetAvailableAnswers(chosenOption.NextDialog, npc, player, worldContext, language));
                }
            }
            else
            {
                if (answer.Index < 0) //dialog end
                {
                    return null;
                } 
                Debug.LogWarning("AnswerIndex out of bounds");
            }
            return null;
        }

        Dialog GetDialog(int id)
        {
            for (var i = 0; i < _conversations.Count; i++)
            {
                var d = FindDialog(_conversations[i], id);
                if (d != null)
                {
                    return d;
                }
            }
            return null;
        }

        static Dialog FindDialog(Dialog current, int id)
        {
            if (current.ID == id)
            {
                return current;
            }
            for (var i = 0; i < current.Options.Count; i++)
            {
                if (current.Options[i].NextDialog == null) continue;
                if (current.Options[i].IsRedirection) continue;
                var d = FindDialog(current.Options[i].NextDialog, id);
                if (d != null)
                {
                    return d;
                }
            }
            return null;
        }

        static bool CheckAvailability(Dialog d, IDialogRelevantNpc npc, IDialogRelevantPlayer player, IDialogRelevantWorld worldContext)
        {
            if (d.Requirements.Count == 0) return true;
            switch (d.RequirementMode)
            {
                case DialogRequirementMode.And:
                    for (var i = 0; i < d.Requirements.Count; i++)
                    {
                        if (!d.Requirements[i].Evaluate(player, npc, worldContext))
                        {
                            return false;
                        }
                    }
                    return true;
                case DialogRequirementMode.Or:
                    for (var i = 0; i < d.Requirements.Count; i++)
                    {
                        if (d.Requirements[i].Evaluate(player, npc, worldContext))
                        {
                            return true;
                        }
                    }
                    break;
            }
            return false;
        }

        List<Conversation.Answer> GetAvailableAnswers(Dialog d, IDialogRelevantNpc npc, IDialogRelevantPlayer player, IDialogRelevantWorld worldContext, DialogLanguage language)
        {
            var answers = new List<Conversation.Answer>();
            for (var i = 0; i < d.Options.Count; i++)
            {
                if (d.Options[i].NextDialog == null)
                {
                    var text = GetDialogStringOrFallback(language, d.Options[i].Text.GetString, d.ID);
                    answers.Add(new Conversation.Answer(i, text, d.Options[i].Tag));
                }
                else if (CheckAvailability(d.Options[i].NextDialog, npc, player, worldContext))
                {
                    var text = GetDialogStringOrFallback(language, d.Options[i].Text.GetString, d.ID);
                    answers.Add(new Conversation.Answer(i, text, d.Options[i].Tag));
                }
            }
            if (answers.Count == 0 && UseEndConversationfallback)
            {
                var txt = GetDialogStringOrFallback(language, _endConversationFallback.GetString, -1);
                answers.Add(new Conversation.Answer(-1, txt, ""));
            }
            return answers;
        }

        string GetDialogStringOrFallback(DialogLanguage lang, LocalizedString.LocalizedStringDelegate getString, int referenceID)
        {
            string txt;
            if (getString(lang, out txt))
            {
                return txt;
            }
            switch (Fallback)
            {
                case LocalizationFallback.Language:
                    if (getString(FallbackLanguage, out txt))
                    {
                        return txt;
                    }
                    goto default;
                    // ReSharper disable once RedundantCaseLabel
                case LocalizationFallback.DebugOutput:
                default:
                    return string.Format(DebugStringFormat, referenceID);
                case LocalizationFallback.EmptyString:
                    return string.Empty;
            }
        }
    }

}