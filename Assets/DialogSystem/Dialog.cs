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
        private List<BaseRequirement> requirements = new List<BaseRequirement>();
        public List<BaseRequirement> Requirements
        {
            get { return requirements; }
            set { requirements = value; }
        }

        public bool MeetsRequirements(IDialogRelevantPlayer player, IDialogRelevantNPC npc, IDialogRelevantWorldInfo worldInfo)
        {
            if (requirementMode == DialogRequirementMode.And)
            {
                for (int i = 0; i < requirements.Count; i++)
                {
                    if (!requirements[i].Evaluate(player, npc, worldInfo))
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
                    if (requirements[i].Evaluate(player, npc, worldInfo))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
