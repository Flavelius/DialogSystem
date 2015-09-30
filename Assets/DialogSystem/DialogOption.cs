using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Runtime.Serialization;
using System;
using Localization;

namespace DialogSystem
{

    [DataContract(Name = "DialogOption")]
    public class DialogOption
    {
        public DialogOption():this("Not Set") { }

        public DialogOption(string title)
        {
            text = new LocalizedString(title);
        }

        [SerializeField, HideInInspector]
        private LocalizedString text;
        [DataMember]
        public LocalizedString Text
        {
            get { return text; }
            set { text = value; }
        }

        public string GetText(Language language, LocalizationFallback fallback, Language fallbackLanguage)
        {
            if (text == null)
            {
                return "Not Set";
            }
            return text.GetString(language, fallback, fallbackLanguage);
        }

        [SerializeField, HideInInspector]
        private Dialog nextDialog;
        [DataMember]
        public Dialog NextDialog
        {
            get { return nextDialog; }
            set { nextDialog = value; }
        }

        [SerializeField, HideInInspector]
        private bool isRedirection = false;
        [DataMember]
        public bool IsRedirection
        {
            get { return isRedirection; }
            set { isRedirection = value; }
        }


        [SerializeField, HideInInspector]
        private List<DialogOptionNotification> notifications = new List<DialogOptionNotification>();
        [DataMember]
        public List<DialogOptionNotification> Notifications
        {
            get { return notifications; }
            set { notifications = value; }
        }
    }
}
