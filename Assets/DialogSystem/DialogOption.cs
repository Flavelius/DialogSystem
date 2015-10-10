using UnityEngine;
using System.Collections.Generic;
using System;
using DialogSystem.Triggers;
using DialogSystem.Localization;

namespace DialogSystem
{
    public class DialogOption: ScriptableObject
    {
        [SerializeField, HideInInspector]
        public LocalizedString Text = new LocalizedString("");

        [SerializeField, HideInInspector]
        private string tag = "";
        public string Tag
        {
            get { return tag??""; }
            set { tag = value; }
        }

        [SerializeField, HideInInspector]
        private Dialog nextDialog;
        public Dialog NextDialog
        {
            get { return nextDialog; }
            set { nextDialog = value; }
        }

        [SerializeField, HideInInspector]
        private bool isRedirection = false;
        public bool IsRedirection
        {
            get { return isRedirection; }
            set { isRedirection = value; }
        }


        [SerializeField, HideInInspector]
        private List<DialogOptionTrigger> triggers = new List<DialogOptionTrigger>();
        public List<DialogOptionTrigger> Triggers
        {
            get { return triggers; }
            set { triggers = value; }
        }
    }
}
