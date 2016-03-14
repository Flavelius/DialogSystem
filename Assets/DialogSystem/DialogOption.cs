using System.Collections.Generic;
using DialogSystem.Actions;
using DialogSystem.Localization;
using UnityEngine;

namespace DialogSystem
{
    public class DialogOption : ScriptableObject
    {
        [SerializeField, HideInInspector] List<DialogOptionAction> _actions = new List<DialogOptionAction>();

        [SerializeField, HideInInspector] bool _ignoreRequirements;

        [SerializeField, HideInInspector] bool _isRedirection;

        [SerializeField, HideInInspector] Dialog _nextDialog;

        [SerializeField, HideInInspector] string _tag = "";

        [SerializeField, HideInInspector] public LocalizedString Text = new LocalizedString("");

        public string Tag
        {
            get { return _tag ?? ""; }
            set { _tag = value; }
        }

        public Dialog NextDialog
        {
            get { return _nextDialog; }
            set { _nextDialog = value; }
        }

        public bool IsRedirection
        {
            get { return _isRedirection; }
            set { _isRedirection = value; }
        }

        public bool IgnoreRequirements
        {
            get { return _ignoreRequirements; }
            set { _ignoreRequirements = value; }
        }

        public List<DialogOptionAction> Actions
        {
            get { return _actions; }
            set { _actions = value; }
        }
    }
}