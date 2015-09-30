using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Runtime.Serialization;
using Localization;

namespace DialogSystem 
{
    [DataContract(Name="Dialog", IsReference=true)]
    public class Dialog
    {
        public Dialog():this(false) { }

        public Dialog(bool isEditor)
        {
            title = new LocalizedString("Not Set");
            text = new LocalizedString("Not Set");
            if (isEditor)
            {
                options.Add(new DialogOption("End Conversation"));
            }
        }

        [SerializeField, HideInInspector]
        private int id;
        [DataMember]
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        [SerializeField, HideInInspector]
        private LocalizedString title;
        [DataMember]
        public LocalizedString Title
        {
            get { return title; }
            set { title = value; }
        }

        public string GetTitle(Language language, LocalizationFallback fallback, Language fallbackLanguage = Language.EN_Default)
        {
            if (title == null)
            {
                return "No Title set";
            }
            return title.GetString(language, fallback, fallbackLanguage);
        }

        [SerializeField, HideInInspector]
        private LocalizedString text;
        [DataMember]
        public LocalizedString Text
        {
            get { return text; }
            set { text = value; }
        }

        public string GetText(Language language, LocalizationFallback fallback, Language fallbackLanguage = Language.EN_Default)
        {
            if (text == null)
            {
                return "No Text set";
            }
            return text.GetString(language, fallback, fallbackLanguage);
        }

        [SerializeField, HideInInspector]
        private string tag = "";
        [DataMember]
        public string Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        [SerializeField, HideInInspector]
        private string npc;
        [DataMember]
        public string Npc
        {
            get { return npc; }
            set { npc = value; }
        }

        [SerializeField, HideInInspector]
        private List<DialogOption> options = new List<DialogOption>();
        [DataMember]
        public List<DialogOption> Options
        {
            get { return options; }
            set { options = value; }
        }

        [SerializeField, HideInInspector]
        private List<DialogRequirement> requirements = new List<DialogRequirement>();
        [DataMember]
        public List<DialogRequirement> Requirements
        {
            get { return requirements; }
            set { requirements = value; }
        }

        internal bool MeetsRequirements(IConversationRelevance target)
        {
            for (int i = 0; i < requirements.Count; i++)
            {
                if (requirements[i].Target == target.Type)
                {
                    if (!target.ValidateDialogRequirement(requirements[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
