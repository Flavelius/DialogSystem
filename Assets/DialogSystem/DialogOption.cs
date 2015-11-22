using UnityEngine;
using System.Collections.Generic;
using System;
using DialogSystem.Actions;
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
        private bool ignoreRequirements = false;
        public bool IgnoreRequirements
        {
            get { return ignoreRequirements; }
            set { ignoreRequirements = value; }
        }

        [SerializeField, HideInInspector]
        private List<DialogOptionAction> actions = new List<DialogOptionAction>();
        public List<DialogOptionAction> Actions
        {
            get { return actions; }
            set { actions = value; }
        }
    }
}
