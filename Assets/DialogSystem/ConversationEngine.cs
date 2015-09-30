using DialogSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using UnityEngine;
using Localization;

public class ConversationEngine : MonoBehaviour, IConversationEngine
{

    [SerializeField, HideInInspector]
    private TextAsset savedDialogs;
    public TextAsset SavedDialogs
    {
        get { return savedDialogs; }
        set { savedDialogs = value; }
    }

    public LocalizationFallback fallback = LocalizationFallback.DebugOutput;
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

    private static IConversationEngine instance;
    private static IConversationEngine dummyInstance;
    public static IConversationEngine Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.Log("Missing ConversationEngine instance");
                if (dummyInstance == null) { dummyInstance = new ConversationEngineDummy(); }
                return dummyInstance;
            }
            return instance;
        }
    }

    private Dictionary<string, List<Dialog>> conversations = new Dictionary<string, List<Dialog>>();

    void Awake()
    {
        Initialize();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void Initialize()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        if (savedDialogs == null)
        {
            Debug.LogWarning("Dialogs-file not specified, no dialogs will be available");
            return;
        }
        LoadDialogs(savedDialogs);
    }

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
                    PrepareConversations(lst);
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

    List<Conversation> IConversationEngine.GetAvailableTopics(IConversationRelevance npc, IConversationRelevance player, IConversationRelevance worldInfo, Language language)
    {
        List<Dialog> npcTopics;
        List<Conversation> availableTopics = new List<Conversation>();
        if (conversations.TryGetValue(npc.Name, out npcTopics))
        {
            for (int i = 0; i < npcTopics.Count; i++)
            {
                if (CheckAvailability(npcTopics[i], npc, player, worldInfo))
                {
                    string title = npcTopics[i].GetTitle(language, fallback, fallbackLanguage);
                    string text = npcTopics[i].GetText(language, fallback, fallbackLanguage);
                    availableTopics.Add(new Conversation(npcTopics[i].ID, npcTopics[i].Npc, title, text, GetAvailableAnswers(npcTopics[i], npc, player, worldInfo, language)));
                }
            }

        }
        return availableTopics;
    }

    Conversation IConversationEngine.Answer(IConversationRelevance npc, IConversationRelevance player, IConversationRelevance worldInfo, int dialogID, int answerIndex, Language language)
    {
        Dialog activeDialog = GetDialog(npc, dialogID);
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
                    return new Conversation(chosenOption.NextDialog.ID, chosenOption.NextDialog.Npc, title, text, GetAvailableAnswers(chosenOption.NextDialog, npc, player, worldInfo, language));
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

    private void PrepareConversations(List<Dialog> dialogTrees)
    {
        List<string> npcs = new List<string>();
        for (int i = 0; i < dialogTrees.Count; i++)
        {
            if (!npcs.Contains(dialogTrees[i].Npc))
            {
                npcs.Add(dialogTrees[i].Npc);
            }
        }
        for (int i = 0; i < npcs.Count; i++)
        {
            conversations.Add(npcs[i], GetAllTopics(dialogTrees, npcs[i]));
        }
    }

    private List<Dialog> GetAllTopics(List<Dialog> dialogCollection, string npc)
    {
        List<Dialog> dialogs = new List<Dialog>();
        for (int i = 0; i < dialogCollection.Count; i++)
        {
            if (dialogCollection[i].Npc.Equals(npc, StringComparison.OrdinalIgnoreCase))
            {
                dialogs.Add(dialogCollection[i]);
            }
        }
        return dialogs;
    }

    private Dialog GetDialog(IConversationRelevance npc, int id)
    {
        List<Dialog> topics = new List<Dialog>();
        if (conversations.TryGetValue(npc.Name, out topics))
        {
            for (int i = 0; i < topics.Count; i++)
            {
                Dialog d = FindDialog(topics[i], id);
                if (d != null)
                {
                    return d;
                }
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
        if (!d.MeetsRequirements(worldInfo)) { return false; }
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
                answers.Add(new Conversation.Answer(i, text));
            } 
            else if (CheckAvailability(d.Options[i].NextDialog, npc, player, worldInfo))
            {
                string text = d.Options[i].GetText(language, fallback, fallbackLanguage);
                answers.Add(new Conversation.Answer(i, text));
            }
        }
        if (answers.Count == 0 && UseEndConversationfallback)
        {
            answers.Add(new Conversation.Answer(-1, EndConversationFallback.GetString(language, fallback, fallbackLanguage)));
        }
        return answers;
    }

    private class ConversationEngineDummy:IConversationEngine
    {

        public List<Conversation> GetAvailableTopics(IConversationRelevance npc, IConversationRelevance player, IConversationRelevance worldInfo, Language language = Language.EN_Default)
        {
            return new List<Conversation>();
        }

        public Conversation Answer(IConversationRelevance npc, IConversationRelevance player, IConversationRelevance worldInfo, int dialogID, int answerIndex, Language language = Language.EN_Default)
        {
            return null;
        }

        public bool LoadDialogs(TextAsset asset)
        {
            return false;
        }
    }

}

public interface IConversationEngine
{
    List<Conversation> GetAvailableTopics(IConversationRelevance npc, IConversationRelevance player, IConversationRelevance worldInfo, Language language);
    Conversation Answer(IConversationRelevance npc, IConversationRelevance player, IConversationRelevance worldInfo, int dialogID, int answerIndex, Language language);
    bool LoadDialogs(TextAsset asset);
}

public class Conversation
{
    public Conversation(int id, string npc, string title, string text, List<Answer> answers)
    {
        ID = id;
        Title = title;
        Text = text;
        Answers = answers;
    }
    public readonly int ID;
    public readonly string Npc;
    public readonly string Title;
    public readonly string Text;
    public readonly List<Answer> Answers;

    public class Answer
    {
        public Answer(int index, string text)
        {
            Index = index;
            Text = text;
        }
        public readonly int Index;
        public readonly string Text;
    }
}
