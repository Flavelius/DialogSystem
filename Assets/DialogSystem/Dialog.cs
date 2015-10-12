using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using DialogSystem.Localization;
using DialogSystem.Requirements;
using DialogSystem.Requirements.Internal;

namespace DialogSystem
{
    public class Dialog: ScriptableObject
    {

        [SerializeField]
        private int id;
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        [SerializeField, HideInInspector]
        public LocalizedString Title = new LocalizedString("");

        [SerializeField, HideInInspector]
        public List<LocalizedString> Texts = new List<LocalizedString>()
        {
            new LocalizedString("")
        };

        public LocalizedString GetText()
        {
            return Texts[UnityEngine.Random.Range(0, Texts.Count - 1)];
        }

        [SerializeField, HideInInspector]
        private string tag = "";
        public string Tag
        {
            get { return tag??""; }
            set { tag = value; }
        }

        [SerializeField, HideInInspector]
        private List<DialogOption> options = new List<DialogOption>();
        public List<DialogOption> Options
        {
            get { return options; }
            set { options = value; }
        }

        [SerializeField, HideInInspector]
        private DialogRequirementMode requirementMode = DialogRequirementMode.And;
        public DialogRequirementMode RequirementMode
        {
            get { return requirementMode; }
            set { requirementMode = value; }
        }


        [SerializeField, HideInInspector]
        private List<BaseRequirement> requirements = new List<BaseRequirement>();
        public List<BaseRequirement> Requirements
        {
            get { return requirements; }
            set { requirements = value; }
        }
    }
}
