using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Localization;

namespace DialogSystem
{
    public class Dialog: ScriptableObject
    {

        public enum DialogRequirementMode
        {
            And,
            Or
        }

        [SerializeField, HideInInspector]
        private int id;
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        [SerializeField, HideInInspector]
        public LocalizedString Title = new LocalizedString("");

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
        private List<DialogRequirement> requirements = new List<DialogRequirement>();
        public List<DialogRequirement> Requirements
        {
            get { return requirements; }
            set { requirements = value; }
        }

        public bool MeetsRequirements(IConversationRelevance target)
        {
            if (requirementMode == DialogRequirementMode.And)
            {
                for (int i = 0; i < requirements.Count; i++)
                {
                    if (requirements[i].Target != target.Type)
                    {
                        continue;
                    }
                    if (!target.ValidateDialogRequirement(requirements[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (requirementMode == DialogRequirementMode.Or)
            {
                for (int i = 0; i < requirements.Count; i++)
                {
                    if (requirements[i].Target != target.Type)
                    {
                        continue;
                    }
                    if (target.ValidateDialogRequirement(requirements[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
