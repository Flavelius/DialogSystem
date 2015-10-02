using DialogSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using UnityEngine;
using Localization;

public class ConversationEngine : MonoBehaviour
{

    [SerializeField, HideInInspector]
    private TextAsset savedDialogs;
    public TextAsset SavedDialogs
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

    void Awake()
    {
        Initialize();
    }

    private void Initialize()
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
    public bool LoadDialogs(TextAsset asset)
    {
        try
        {
            DataContractSerializer deserializer = new DataContractSerializer(typeof(List<Dialog>));
            StringReader reader = new StringReader(asset.text);
            using (XmlReader stream = XmlReader.Create(reader))
            {
                List<Dialog> lst = deserializer.ReadObject(stream) as List<Dialog>;
                if (lst != null)
                {
                    //PrepareConversations(lst);
                    conversations = lst;
                    return true;
                }
                else 
                {
                    Debug.LogWarning("Error loading dialogs");
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
            return false;
        }
    }

    /// <summary>
    /// Requests a list of topics
    /// </summary>
    /// <param name="npc">Required, reference to the topics owning npc</param>
    /// <param name="player">Required, reference to the conversing player</param>
    /// <param name="worldInfo">Not required, but could be, depending on the settings of certain dialogs</param>
    /// <param name="language">The language the conversing player should receive an answer in</param>
    /// <returns></returns>
    public Conversation GetAvailableTopics(IConversationRelevance npc, IConversationRelevance player, IConversationRelevance worldInfo, Language language)
    {
        //List<Conversation> availableTopics = new List<Conversation>();
        List<Dialog> availableTopics = new List<Dialog>();
        for (int i = 0; i < conversations.Count; i++)
        {
            if (CheckAvailability(conversations[i], npc, player, worldInfo))
            {
                //string title = conversations[i].GetTitle(language, fallback, fallbackLanguage);
                //string text = conversations[i].GetText(language, fallback, fallbackLanguage);
                //availableTopics.Add(new Conversation(conversations[i].ID, title, text, conversations[i].Tag, GetAvailableAnswers(conversations[i], npc, player, worldInfo, language)));
                availableTopics.Add(conversations[i]);
            }
        }
        if (availableTopics.Count == 1)
        {
            string title = conversations[0].GetTitle(language, fallback, fallbackLanguage);
            string text = conversations[0].GetText(language, fallback, fallbackLanguage);
            return new Conversation(availableTopics[0].ID, title, text, availableTopics[0].Tag, Conversation.ConversationType.Single, GetAvailableAnswers(availableTopics[0], npc, player, worldInfo, language));
        }
        else if (availableTopics.Count > 1)
        {
            Conversation c = new Conversation(-1, "", "", "", Conversation.ConversationType.TopicList, new List<Conversation.Answer>());
            foreach (Dialog d in availableTopics)
            {
                string title = d.GetTitle(language, fallback, fallbackLanguage);
                Conversation.Answer ca = new Conversation.Answer(d.ID, title, d.Tag);
                c.Answers.Add(ca);
            }
            return c;
        }
        return null;
        //return availableTopics;
    }

    /// <summary>
    /// Retrieves the topic following the supplied answer from a previous topic
    /// </summary>
    /// <param name="npc">Required, reference to the topics owning npc</param>
    /// <param name="player">Required, reference to the conversing player</param>
    /// <param name="worldInfo">Not required, but could be, depending on the settings of certain dialogs</param>
    /// <param name="dialogID">The id, of the dialog that is answered, or -1 if answer came from topicList</param>
    /// <param name="answerIndex">The index of the answer of the answered dialog, or dialogID if answer came from topicList</param>
    /// <param name="language">The language the conversing player should receive an answer in</param>
    /// <returns></returns>
    public Conversation Answer(IConversationRelevance npc, IConversationRelevance player, IConversationRelevance worldInfo, int dialogID, int answerIndex, Language language)
    {
        Dialog activeDialog = null;
        if (dialogID == -1)
        {
            activeDialog = GetDialog(npc, answerIndex);
            if (activeDialog == null || !CheckAvailability(activeDialog, npc, player, worldInfo))
            {
                Debug.LogWarning("Selection from topicList invalid");
                return null;
            }
            else
            {
                string title = activeDialog.GetTitle(language, fallback, fallbackLanguage);
                string text = activeDialog.GetText(language, fallback, fallbackLanguage);
                return new Conversation(activeDialog.ID, title, text, activeDialog.Tag, Conversation.ConversationType.Single, GetAvailableAnswers(activeDialog, npc, player, worldInfo, language));
            }
        }
        else
        {
            activeDialog = GetDialog(npc, dialogID);
        }
        if (activeDialog == null) { return null; }
        if (answerIndex >= 0 && answerIndex < activeDialog.Options.Count)
        {
            DialogOption chosenOption = activeDialog.Options[answerIndex];
            for (int i = 0; i < chosenOption.Notifications.Count; i++)
            {
                chosenOption.Notifications[i].Notify(activeDialog, npc, player, worldInfo);
            }
            if (chosenOption.NextDialog != null)
            {
                if (CheckAvailability(chosenOption.NextDialog, npc, player, worldInfo))
                {
                    string title = chosenOption.NextDialog.GetTitle(language, fallback, fallbackLanguage);
                    string text = chosenOption.NextDialog.GetText(language, fallback, fallbackLanguage);
                    return new Conversation(chosenOption.NextDialog.ID, title, text, chosenOption.NextDialog.Tag, Conversation.ConversationType.Single, GetAvailableAnswers(chosenOption.NextDialog, npc, player, worldInfo, language));
                }
            }
        }
        else
        {
            if (answerIndex == -1) { return null; } //close dialog
            Debug.LogWarning("AnswerIndex out of bounds");
        }
        return null;
    }

    private Dialog GetDialog(IConversationRelevance npc, int id)
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
                    Dialog d =FindDialog(current.Options[i].NextDialog, id);
                    if (d != null)
                    {
                        return d;
                    }
                }
            }
        }
        return null;
    }

    private bool CheckAvailability(Dialog d, IConversationRelevance npc, IConversationRelevance player, IConversationRelevance worldInfo)
    {
        if (!d.MeetsRequirements(npc)) { return false; }
        if (!d.MeetsRequirements(player)) { return false; }
        if (worldInfo != null)
        {
            if (!d.MeetsRequirements(worldInfo)) { return false; }
        }
        return true;
    }

    private List<Conversation.Answer> GetAvailableAnswers(Dialog d, IConversationRelevance npc, IConversationRelevance player, IConversationRelevance worldInfo, Language language)
    {
        List<Conversation.Answer> answers = new List<Conversation.Answer>();
        for (int i = 0; i < d.Options.Count; i++)
        {
            if (d.Options[i].NextDialog == null)
            {
                string text = d.Options[i].GetText(language, fallback, fallbackLanguage);
                answers.Add(new Conversation.Answer(i, text, d.Options[i].Tag));
            } 
            else if (CheckAvailability(d.Options[i].NextDialog, npc, player, worldInfo))
            {
                string text = d.Options[i].GetText(language, fallback, fallbackLanguage);
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

public class Conversation
{
    public enum ConversationType { Single, TopicList }
    public Conversation(int id, string title, string text, string tag, ConversationType type, List<Answer> answers)
    {
        ID = id;
        Title = title;
        Text = text;
        Tag = tag;
        Answers = answers;
        Type = type;
    }
    public readonly int ID;
    public readonly string Npc;
    public readonly string Title;
    public readonly string Text;
    public readonly string Tag;
    public readonly List<Answer> Answers;
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
