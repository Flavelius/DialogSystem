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

    public class DialogOptionNotification: IDialogNotification
    {
        [SerializeField, HideInInspector]
        private DialogNotificationType type = DialogNotificationType.DialogCompleted;
        [DataMember]
        public DialogNotificationType Type
        {
            get { return type; }
            set { type = value; }
        }

        [SerializeField, HideInInspector]
        private string paramValue = "";
        [DataMember(Name = "value")]
        public string Value
        {
            get { return paramValue; }
            set { paramValue = value; }
        }

        [SerializeField, HideInInspector]
        private DialogNotificationTarget target = DialogNotificationTarget.Player;
        [DataMember]
        public DialogNotificationTarget Target
        {
            get { return target; }
            set { target = value; }
        }
        

        [SerializeField, HideInInspector]
        private string targetName = "";
        [DataMember(Name = "target")]
        public string TargetName
        {
            get { return targetName; }
            set { targetName = value; }
        }

        public void Notify(Dialog parent, IConversationRelevance npc, IConversationRelevance player, IConversationRelevance worldInfo)
        {
            Debug.Log("TODO notifier string/ID to other function?");
            switch (target)
            {
                case DialogNotificationTarget.Player:
                    player.OnDialogNotification(player, npc, this);
                    break;
                case DialogNotificationTarget.Npc:
                    npc.OnDialogNotification(player, npc, this);
                    break;
                case DialogNotificationTarget.World:
                    worldInfo.OnDialogNotification(player, npc, this);
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
    }
}
