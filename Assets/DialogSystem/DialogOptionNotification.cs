using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

namespace DialogSystem
{

    public enum DialogNotificationTarget
    {
        Player,
        Npc,
        World,
        Other
    }

    [System.Serializable]
    public class DialogOptionNotification: IDialogNotification
    {
        [SerializeField, HideInInspector]
        private DialogNotificationType type = DialogNotificationType.DialogCompleted;
        public DialogNotificationType Type
        {
            get { return type; }
            set { type = value; }
        }

        [SerializeField, HideInInspector]
        private string paramValue = "";
        public string Value
        {
            get { return paramValue; }
            set { paramValue = value; }
        }

        [SerializeField, HideInInspector]
        private DialogNotificationTarget target = DialogNotificationTarget.Player;
        public DialogNotificationTarget Target
        {
            get { return target; }
            set { target = value; }
        }
        

        [SerializeField, HideInInspector]
        private string targetName = "";
        public string TargetName
        {
            get { return targetName; }
            set { targetName = value; }
        }

        public void Notify(Dialog parent, IConversationRelevance npc, IConversationRelevance player, IConversationRelevance worldContext)
        {
            Debug.Log("TODO notifier string/ID to other function?");
            if (type == DialogNotificationType.DialogCompleted)
            {
                Value = parent.ID.ToString();
            }
            switch (target)
            {
                case DialogNotificationTarget.Player:
                    player.OnDialogNotification(player, npc, this);
                    break;
                case DialogNotificationTarget.Npc:
                    npc.OnDialogNotification(player, npc, this);
                    break;
                case DialogNotificationTarget.World:
                    worldContext.OnDialogNotification(player, npc, this);
                    break;
                case DialogNotificationTarget.Other:
                default:
                    GameObject go = GameObject.Find(targetName);
                    if (go == null) { Debug.Log("Dialog-NotificationTarget not found: "+targetName); return; }
                    IDialogNotificationReceiver rec = go.GetComponent(typeof(IDialogNotificationReceiver)) as IDialogNotificationReceiver;
                    if (rec == null) { Debug.Log("Dialog-NotificationTarget does not implement " + rec.GetType().Name); }
                    rec.OnDialogNotification(player, npc, this);
                    break;
            }
        }

        public string GetShortIdentifier()
        {
            return string.Format("{0}{1}", target.ToString()[0], type.ToString()[0]);
        }

        public Color GetColor()
        {
            return Color.Lerp(Color.gray, new Color((float)target*0.6f, (float)type*0.6f, (float)Value.GetHashCode()*0.6f, 1f), 0.5f);
        }
    }
}
