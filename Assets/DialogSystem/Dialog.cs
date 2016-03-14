using System.Collections.Generic;
using DialogSystem.Localization;
using DialogSystem.Requirements;
using UnityEngine;

namespace DialogSystem
{
    public class Dialog : ScriptableObject
    {
        [SerializeField] int _id;

        [SerializeField, HideInInspector] List<DialogOption> _options = new List<DialogOption>();

        [SerializeField, HideInInspector] DialogRequirementMode _requirementMode = DialogRequirementMode.And;


        [SerializeField, HideInInspector] List<DialogRequirement> _requirements = new List<DialogRequirement>();

        [SerializeField, HideInInspector] string _tag = "";

        [SerializeField, HideInInspector] public List<LocalizedString> Texts = new List<LocalizedString>
        {
            new LocalizedString("")
        };

        [SerializeField, HideInInspector] public LocalizedString Title = new LocalizedString("");

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Tag
        {
            get { return _tag ?? ""; }
            set { _tag = value; }
        }

        public List<DialogOption> Options
        {
            get { return _options; }
            set { _options = value; }
        }

        public DialogRequirementMode RequirementMode
        {
            get { return _requirementMode; }
            set { _requirementMode = value; }
        }

        public List<DialogRequirement> Requirements
        {
            get { return _requirements; }
            set { _requirements = value; }
        }

        public LocalizedString GetText()
        {
            return Texts.Count == 0 ? LocalizedString.Empty : Texts[Random.Range(0, Texts.Count - 1)];
        }
    }
}