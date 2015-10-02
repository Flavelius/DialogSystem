using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Runtime.Serialization;
using System.IO;
using DialogSystem;

public class DialogEditor : EditorWindow
{

    private static DialogEditor window;
    private static ConversationEngine source;

    private GUIStyle headerStyle;
    private Color inspectorColor;
    private GUIStyle buttonStyle;
    private GUIStyle lblStyle;

    private List<Dialog> dialogCollection = new List<Dialog>();

    private HashSet<int> reservedDialogIDs = new HashSet<int>();
    private int ReserveDialogID()
    {
        int newID = 0;
        while (reservedDialogIDs.Contains(newID))
        {
            newID += 1;
        }
        reservedDialogIDs.Add(newID);
        return newID;
    }
    private void ReleaseDialogID(int id)
    {
        reservedDialogIDs.Remove(id);
    }

    private void LoadFixReservedIDs(List<Dialog> allDialogs)
    {
        bool needsSaving = false;
        for (int i = 0; i < allDialogs.Count; i++)
        {
            List<Dialog> subDialogs = new List<Dialog>();
            subDialogs = GetAllDialogsInChain(subDialogs, allDialogs[i]);
            for (int s = 0; s < subDialogs.Count; s++)
            {
                if (!reservedDialogIDs.Contains(subDialogs[s].ID))
                {
                    reservedDialogIDs.Add(subDialogs[s].ID);
                }
                else
                {
                    subDialogs[s].ID = ReserveDialogID();
                    needsSaving = true;
                }
            }
        }
        if (needsSaving)
        {
            Debug.LogWarning("duplicate IDs fixed, Save Dialog file!");
        }
    }

    public static void ShowWindow(ConversationEngine instance)
    {
        source = instance;
        window = EditorWindow.GetWindow<DialogEditor>("Dialog Editor");
        window.minSize = new Vector2(600, 400);
        window.Start();
    }

    public static void Cleanup()
    {
        source = null;
        window = null;
        Localization.LocalizedStringEditor.CancelEdit();
    }

    private Dialog activeDialog;
    private DialogOption activeOptionNode;
    private Dialog activeSubDialog;
    private void InspectOptionNode(DialogOption o)
    {
        activeSubDialog = null;
        activeOptionNode = o;
    }
    private void InspectDialogNode(Dialog d)
    {
        activeOptionNode = null;
        activeSubDialog = d;
    }
    private void CloseSubInspector()
    {
        activeSubDialog = null;
        activeOptionNode = null;
        Localization.LocalizedStringEditor.CancelEdit();
    }
    private float leftColumnWidth = 200;
    private float rightColumnoverlayWidth = 200;
    private Color nodeColor = new Color(0.75f,0.75f,0.75f,1f);
    Vector2 dialogsScroll = Vector2.zero;

    void OnGUI()
    {
        if (window == null || source == null) {
            return;
        }
        GUI.enabled = (activeOptionNode == null && activeSubDialog == null);
        Rect Left = new Rect(0, 0, leftColumnWidth, window.position.height);
        Color prev = GUI.color;
        GUI.color = inspectorColor;
        GUI.BeginGroup(Left, EditorStyles.textField);
        GUI.color = prev;
        DisplayDialogTools(Left);
        GUI.EndGroup();
        DisplayDialogEditor(new Rect(leftColumnWidth, 0, window.position.width-leftColumnWidth, window.position.height));

        if (activeOptionNode != null)
        {
            GUI.enabled = true;
            DisplayOptionNodeInspector(new Rect(window.position.width - rightColumnoverlayWidth, 0, rightColumnoverlayWidth, window.position.height), activeOptionNode);
        }
        else if (activeSubDialog != null)
        {
            GUI.enabled = true;
            DisplayDialogNodeInspector(new Rect(window.position.width - rightColumnoverlayWidth, 0, rightColumnoverlayWidth, window.position.height), activeSubDialog);
        }
    }

    void Initialize()
    {
        if (headerStyle == null) 
        {
            headerStyle = new GUIStyle(GUI.skin.GetStyle("flow shader node 0")); 
            headerStyle.stretchWidth = true; 
            headerStyle.fontStyle = FontStyle.Bold; 
            headerStyle.fontSize = 14;
            headerStyle.normal.textColor = new Color(0.7f, 0.6f, 0.6f);
        }
        lblStyle = new GUIStyle(GUI.skin.GetStyle("flow overlay header upper left"));
        lblStyle.stretchWidth = true;
        inspectorColor = Color.Lerp(Color.gray, Color.white, 0.5f);
        buttonStyle = GUI.skin.GetStyle("PreButton");
    }

    void Update()
    {
        if (window == null || source == null)
        {
            Cleanup();
            Close();
        }
    }

    void OnDestroy()
    {
        Cleanup();
    }

    public void Start()
    {
        if (source != null && source.SavedDialogs != null)
        {
            LoadDialogs(new StringReader(source.SavedDialogs.text));
        }
        Initialize();
    }

    bool isValidName(string name)
    {
        return !string.IsNullOrEmpty(name) && !name.StartsWith(" ");
    }

    private void LoadDialogs(StringReader reader)
    {
        try
        {
            DataContractSerializer deserializer = new DataContractSerializer(typeof(List<Dialog>));
            using (XmlReader stream = XmlReader.Create(reader))
            {
                List<Dialog> lst = deserializer.ReadObject(stream) as List<Dialog>;
                if (lst != null)
                {
                    reservedDialogIDs.Clear();
                    dialogCollection = lst;
                    LoadFixReservedIDs(dialogCollection);
                }
                else { Debug.LogWarning("Error loading dialogs"); }
            }
        }
        catch (SerializationException)
        {
            Debug.LogWarning("Error loading dialogs");
        }
        catch (XmlException)
        {
            Debug.LogWarning("Error reading saved dialogs, format problem");
        }
    }
    void DisplayDialogTools(Rect r)
    {
        GUILayout.Space(5);
        GUILayout.BeginVertical(EditorStyles.textArea,GUILayout.Width(r.width-8));
        if (GUILayout.Button("New Dialog"))
        {
            Dialog d = new Dialog(true);
            d.ID = ReserveDialogID();
            dialogCollection.Add(d);
            activeDialog = d;
        }
        bool prevEnabled = GUI.enabled;
        if (dialogCollection.Count == 0)
        {
            GUI.enabled = false;
        }
        if (GUILayout.Button("Save"))
        {
            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(List<Dialog>));
                string path = EditorUtility.SaveFilePanel("Save Dialogs", "Assets/DialogSystem/Dialogs", "Dialogs", "xml");
                XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
                using (XmlWriter stream = XmlWriter.Create(path, settings))
                {
                    serializer.WriteObject(stream, dialogCollection);
                }
                if (path.StartsWith(Application.dataPath))
                {
                    string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                    source.SavedDialogs = AssetDatabase.LoadAssetAtPath(relativePath, typeof(TextAsset)) as TextAsset;
                }
                else
                {
                    Debug.Log("File not saved in Game folder, will not be available during runtime!");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
        GUI.enabled = prevEnabled;
        if (GUILayout.Button("Load"))
        {
            try
            {
                string path = EditorUtility.OpenFilePanel("Load Dialogs", "Assets/DialogSystem/Dialogs", "xml");
                if (string.IsNullOrEmpty(path) || !System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(path))) { return; }
                using (StreamReader sr = new StreamReader(path))
                {
                    using (StringReader reader = new StringReader(sr.ReadToEnd()))
                    {
                        LoadDialogs(reader);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
        GUILayout.EndVertical();
        Rect itemRect = new Rect(0, 0, r.width - 38, 17);
        Rect deleteItemRect = new Rect(itemRect.width, 0, 18, 17);
        Rect scrollContainerRect = new Rect(2, 74, r.width - 5, r.height - 76);
        Rect contentScrollRect = new Rect(0, 0, r.width - 20, Mathf.Max(dialogCollection.Count * itemRect.height, scrollContainerRect.height));
        dialogsScroll = GUI.BeginScrollView(scrollContainerRect, dialogsScroll, contentScrollRect, false, true);
        for (int i = 0; i < dialogCollection.Count; i++)
        {
            itemRect.y = i * (itemRect.height+2);
            deleteItemRect.y = itemRect.y;
            if (itemRect.y < dialogsScroll.y - itemRect.height)
            {
                continue;
            }
            if (itemRect.y > dialogsScroll.y + scrollContainerRect.height)
            {
                break;
            }
            if (dialogCollection[i] == activeDialog)
            {
                GUI.color = Color.gray;
            }
            if (GUI.Button(itemRect, dialogCollection[i].Title != null ? dialogCollection[i].Title.Description : "No Title", "ButtonLeft"))
            {
                if (activeDialog == dialogCollection[i]) { activeDialog = null; }
                else
                {
                    activeDialog = dialogCollection[i];
                }
            }
            GUI.color = Color.white;
            if (GUI.Button(deleteItemRect, "x", "ButtonRight"))
            {
                DeleteCleanupDialog(dialogCollection[i]);
                if (dialogCollection[i] == activeDialog) { CloseSubInspector(); activeDialog = null; }
                dialogCollection.Remove(dialogCollection[i]);
            }
        }
        GUI.EndScrollView();
    }

    Vector2 mainViewScrollPos;
    float nodeWidth = 180;
    float nodeHeight = 30;
    float indentWidth = 50;
    void DisplayDialogEditor(Rect r)
    {
        if (activeDialog == null) { return; }
        GUI.color = nodeColor;
        GUILayout.BeginArea(new Rect(r.x, r.y, r.width, r.height));
        if (countedHeight < r.height) { countedHeight = r.height-15; }
        if (countedWidth < r.width) { countedWidth = r.width-15; }
        float inspectorInset = 0;
        if (activeOptionNode != null || activeSubDialog != null)
        {
            inspectorInset = rightColumnoverlayWidth;
            if (countedWidth > r.width)
            {
                mainViewScrollPos = new Vector2(mainViewScrollPos.x + inspectorInset, mainViewScrollPos.y);
            }
        }
        mainViewScrollPos = GUI.BeginScrollView(new Rect(0, 0, r.width-inspectorInset, r.height), mainViewScrollPos, new Rect(0, 0, countedWidth, countedHeight));
        if (previousState != countedWidth + countedHeight)
        {
            window.Repaint();
            previousState = countedWidth + countedHeight;
        }
        countedHeight = 0;
        countedWidth = 0;
        RecurseDialogs(new Rect(5, r.y+5,nodeWidth, nodeHeight) , 0, 0, activeDialog);
        GUI.EndScrollView();
        GUILayout.EndArea();
        GUI.color = Color.white;
    }

    float countedWidth;
    float countedHeight;
    float previousState;
    int RecurseDialogs(Rect parent, int length, int depth, Dialog d)
    {
        Rect r = new Rect(parent.x + (depth * nodeWidth), parent.y + (length * nodeHeight), nodeWidth, nodeHeight);
        DisplaySingleDialogNode(r, d, length, depth);
        int branchDepth = 1;
        int lastOptionDepth = 0;
        for (int i = 0; i < d.Options.Count;i++ )
        {
            int move = 0;
            if (d.Options[i].NextDialog != null)
            {
                if (d.Options[i].IsRedirection)
                {
                    lastOptionDepth = branchDepth;
                    move = DisplayDialogOption(new Rect(r.x + nodeWidth + indentWidth, r.y + nodeHeight * lastOptionDepth, nodeWidth + indentWidth, nodeHeight), d.Options[i]);
                    if (d.Options[i].NextDialog == null) { continue; }
                    DisplayDialogLoop(new Rect(r.x + nodeWidth + indentWidth * 2, r.y + nodeHeight * lastOptionDepth, nodeWidth + indentWidth, nodeHeight), d.Options[i]);
                    branchDepth += 1;
                }
                else
                {
                    lastOptionDepth = branchDepth;
                    move = DisplayDialogOption(new Rect(r.x + nodeWidth + indentWidth, r.y + nodeHeight * lastOptionDepth, nodeWidth + indentWidth, nodeHeight), d.Options[i]);
                    if (d.Options[i].NextDialog == null) { continue; }
                    branchDepth += RecurseDialogs(new Rect(parent.x + indentWidth * 2, parent.y, nodeWidth, nodeHeight), length + branchDepth, depth + 1, d.Options[i].NextDialog);
                }
            }
            else
            {
                lastOptionDepth = branchDepth;
                move = DisplayDialogOption(new Rect(r.x + nodeWidth + indentWidth, r.y + nodeHeight * lastOptionDepth, nodeWidth + indentWidth, nodeHeight), d.Options[i]);
                branchDepth += 1;
            }
            if (move != 0)
            {
                if (move == -10)
                {
                    if (d.Options.Count > 1)
                    {
                        if (d.Options[i].NextDialog != null && !d.Options[i].IsRedirection)
                        {
                            DeleteCleanupDialog(d.Options[i].NextDialog);
                        }
                        d.Options.RemoveAt(i);
                    }
                }
                else if (i + move >= 0 & i + move < d.Options.Count)
                {
                    DialogOption dO = d.Options[i];
                    d.Options[i] = d.Options[i + move];
                    d.Options[i + move] = dO;
                }
            }
        }
        if (lastOptionDepth > 0)
        {
            if (DrawOptionRegion(new Rect(r.x + nodeWidth + indentWidth, r.y + nodeHeight, nodeWidth + indentWidth, nodeHeight), lastOptionDepth))
            {
                DialogOption dO = new DialogOption("Not Set");
                d.Options.Add(dO);
            }
        }
        return branchDepth;
    }

    int DisplayDialogOption(Rect r, DialogOption option)
    {
        Color prev = GUI.color;
        GUI.color = nodeColor;
        GUIStyle gs = new GUIStyle(GUI.skin.button);
        gs.alignment = TextAnchor.MiddleLeft;
        gs.fontSize = 8;
        gs.border = new RectOffset(2, 2, 2, 2);
        Rect title = new Rect(r.x - r.width + indentWidth+1, r.y + r.height * 0.6f, r.width, r.height * 0.4f);
        if (title.y + title.height > countedHeight) { countedHeight = title.y + title.height; }
        if (title.x + title.width > countedWidth) { countedWidth = title.x + title.width; }
        int ret = 0;
        if (GUI.Button(title, option.Text != null ? option.Text.Description : "Not Set", gs))
        {
            InspectOptionNode(option);
        }
        gs.alignment = TextAnchor.MiddleCenter;
        if (GUI.Button(new Rect(title.x - 18, title.y - 5, 17, 17), "˄", gs))
        {
            ret = -1;
        }
        if (GUI.Button(new Rect(title.x - 34, title.y - 5, 17, 17), "˅", gs))
        {
            ret = 1;
        }
        if (GUI.Button(new Rect(title.x - 50, title.y - 5, 17, 17), "x", gs))
        {
            ret = -10;
        }
        if (option.NextDialog != null)
        {
            DrawInlineDialogConstraints(new Rect(title.x+indentWidth, title.y - (title.height+2), title.width, title.height+10), option);
            if (GUI.Button(new Rect(title.x + title.width - 17, r.y+5, 17, 13), "x", gs))
            {
                if (!option.IsRedirection)
                {
                    DeleteCleanupDialog(option.NextDialog);
                }
                option.NextDialog = null;
                option.IsRedirection = false;
            }
        }
        GUI.color = prev;
        return ret;
    }

    void DrawInlineDialogConstraints(Rect r, DialogOption d)
    {
        GUILayout.BeginArea(r);
        GUILayout.BeginHorizontal();
        Color prev = GUI.color;
        GUI.color = Color.white;
        GUIStyle gs = new GUIStyle(GUI.skin.button);
        gs.fontSize = 8;
        gs.alignment = TextAnchor.MiddleCenter;
        gs.contentOffset = new Vector2(-1, 0);
        gs.clipping = TextClipping.Overflow;
        gs.border = new RectOffset(1, 1, 1, 1);
        if (d.NextDialog != null)
        {
            for (int i = d.NextDialog.Requirements.Count; i-- > 0; )
            {
                DialogRequirement req = d.NextDialog.Requirements[i];
                GUI.color = req.GetColor();
                string tooltip = string.Format("Target: {0}, Type: {1}, IntValue({2}), StringValue('{3}'), FloatValue({4})", req.Target, req.Type, req.IntValue, req.StringValue, req.FloatValue);
                GUILayout.Box(new GUIContent(req.ShortIdentifier, tooltip), gs, GUILayout.Width(19), GUILayout.Height(15));
            }
        }
        GUI.color = prev;
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    bool DrawOptionRegion(Rect r, int depth)
    {
        bool ret = false;
        Color prev = GUI.color;
        GUI.color = nodeColor;
        GUI.Box(new Rect(r.x - r.width+indentWidth-2 , r.y, 5, r.height*depth), "", EditorStyles.textArea);
        if (GUI.Button(new Rect(r.x-r.width+indentWidth+2, r.y, 20, 19), "+", EditorStyles.miniButton)) {
            ret = true;
        }
        if (r.height + 17 > countedHeight) { countedHeight = r.height + 17; }
        GUI.color = prev;
        return ret;
    }

    Vector2 scrollbar;
    Vector2 scrollbar2;
    void DisplayOptionNodeInspector(Rect r, DialogOption dOption)
    {
        Color prev = GUI.color;
        GUI.color = inspectorColor;
        GUILayout.BeginArea(r, EditorStyles.textArea);
        GUI.color = prev;
        GUILayout.Space(5);
        GUILayout.Label("Text", headerStyle);
        GUILayout.Label(dOption.Text != null ? dOption.Text.Description : "Not Set", lblStyle);
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (dOption.Text == null)
        {
            if (GUILayout.Button("Add", buttonStyle))
            {
                dOption.Text = new Localization.LocalizedString();
            }
        }
        else
        {
            if (GUILayout.Button("Edit", buttonStyle))
            {
                Localization.LocalizedStringEditor.OpenEdit(dOption.Text);
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.Label(new GUIContent("Tag", "User data"), headerStyle);
        GUILayout.Space(5);
        dOption.Tag = GUILayout.TextField(dOption.Tag);
        GUILayout.Space(10);
        GUILayout.Label("Connection", headerStyle);
        GUILayout.Space(5);
        if (GUILayout.Button("New Sub-Dialog", buttonStyle))
        {
            int id = ReserveDialogID();
            Dialog d = new Dialog(true);
            d.ID = id;
            dOption.NextDialog = d;
            dOption.IsRedirection = false;
            CloseSubInspector();
        }
        GUILayout.Label(new GUIContent("Or link to existing: ", "Use with care, can result in infinite loops!"));
        scrollbar = GUILayout.BeginScrollView(scrollbar);
        List<Dialog> ddialogs = new List<Dialog>();
        ddialogs = GetAllDialogsInChain(ddialogs, activeDialog);
        for (int i = 0; i < ddialogs.Count; i++)
        {
            if (ddialogs[i].Options.Contains(dOption)) { continue; }
            if (GUILayout.Button(ddialogs[i].Title != null ? ddialogs[i].Title.Description : "No Title", buttonStyle))
            {
                dOption.NextDialog = ddialogs[i];
                dOption.IsRedirection = true;
                CloseSubInspector();
                break;
            }
        }
        GUILayout.EndScrollView();
        GUILayout.Space(10);
        GUILayout.Label("Notifications", headerStyle);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add", buttonStyle))
        {
            dOption.Notifications.Add(new DialogOptionNotification());
        }
        if (GUILayout.Button("Remove all", buttonStyle))
        {
            dOption.Notifications.Clear();
        }
        GUILayout.EndHorizontal();
        scrollbar2 = GUILayout.BeginScrollView(scrollbar2);
        for (int i = dOption.Notifications.Count; i-- > 0; )
        {
            if (!InlineDisplayNotificationEditor(dOption.Notifications[i]))
            {
                dOption.Notifications.RemoveAt(i);
            }
        }
        GUILayout.EndScrollView();
        if (GUILayout.Button("Close", buttonStyle))
        {
            CloseSubInspector();
        }
        GUILayout.EndArea();
    }

    bool InlineDisplayNotificationEditor(DialogOptionNotification notification)
    {
        bool ret = true;
        GUILayout.BeginVertical(EditorStyles.textArea);
        if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(17)))
        {
            ret = false;
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("Type: ", GUILayout.Width(70));
        notification.Type = (DialogNotificationType)EditorGUILayout.EnumPopup(notification.Type);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Target: ", GUILayout.Width(70));
        notification.Target = (DialogNotificationTarget)EditorGUILayout.EnumPopup(notification.Target);
        GUILayout.EndHorizontal();

        if (notification.Target == DialogNotificationTarget.Other)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Name: ", "Exact name of target GameObject"), GUILayout.Width(50));
            notification.TargetName = EditorGUILayout.TextField(notification.TargetName);
            GUILayout.EndHorizontal();
        }

        if (notification.Type == DialogNotificationType.Other)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Value: ", GUILayout.Width(50));
            notification.Value = GUILayout.TextField(notification.Value);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        return ret;
    }

    void DisplayDialogNodeInspector(Rect r, Dialog d)
    {
        Color prev = GUI.color;
        GUI.color = inspectorColor;
        GUILayout.BeginArea(r, EditorStyles.textArea);
        GUI.color = prev;
        GUILayout.Space(5);
        GUILayout.Label("Title", headerStyle);
        GUILayout.Label(d.Title!=null?d.Title.Description:"Not Set", lblStyle);
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (d.Title == null)
        {
            if (GUILayout.Button("Add", buttonStyle))
            {
                d.Title = new Localization.LocalizedString();
            }
        }
        else
        {
            if (GUILayout.Button("Edit", buttonStyle))
            {
                Localization.LocalizedStringEditor.OpenEdit(d.Title);
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.Label("Text", headerStyle);
        GUILayout.Label(d.Text != null ? d.Text.Description : "Not Set", lblStyle);
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (d.Text == null)
        {
            if (GUILayout.Button("Add", buttonStyle))
            {
                d.Text = new Localization.LocalizedString();
            }
        }
        else
        {
            if (GUILayout.Button("Edit", buttonStyle))
            {
                Localization.LocalizedStringEditor.OpenEdit(d.Text);
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.Label(new GUIContent("Tag", "User data"), headerStyle);
        GUILayout.Space(5);
        d.Tag = GUILayout.TextField(d.Tag);
        GUILayout.Space(10);
        GUILayout.Label("Requirements", headerStyle);
        if (d.Requirements.Count > 1)
        {
            d.RequirementMode = (Dialog.DialogRequirementMode)EditorGUILayout.EnumPopup("Mode:",d.RequirementMode);
        }
        GUILayout.BeginHorizontal();
        bool prevEnabled = GUI.enabled;
        if (d.Requirements.Count >= 6) { GUI.enabled = false; }
        if (GUILayout.Button("Add", buttonStyle))
        {
            d.Requirements.Add(new DialogRequirement());
        }
        GUI.enabled = prevEnabled;
        if (GUILayout.Button("Remove all", buttonStyle))
        {
            d.Requirements.Clear();
        }
        GUILayout.EndHorizontal();
        scrollbar = GUILayout.BeginScrollView(scrollbar, false, true);
        for (int i = d.Requirements.Count; i-- > 0; )
        {
            if (!DrawInlineRequirement(d.Requirements[i]) | d.Requirements[i].Type == DialogRequirementType.None) {
                d.Requirements.RemoveAt(i); 
                continue;
            }
        }
        GUILayout.EndScrollView();
        if (GUILayout.Button("Close", buttonStyle))
        {
            RemoveDuplicateRequirements(d.Requirements);
            CloseSubInspector();
        }
        GUILayout.EndArea();
    }

    void RemoveDuplicateRequirements(List<DialogRequirement> sourceList)
    {
        List<DialogRequirement> cleanList = new List<DialogRequirement>();
        for (int i = 0; i < sourceList.Count; i++)
        {
            bool found = false;
            for (int cl = 0; cl < cleanList.Count; cl++)
            {
                if (cleanList[cl].Type == sourceList[i].Type & cleanList[cl].Target == sourceList[i].Target)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                cleanList.Add(sourceList[i]);
            }
        }
        sourceList.Clear();
        sourceList.AddRange(cleanList);
    }

    bool DrawInlineRequirement(DialogRequirement dr)
    {
        bool ret = true;
        GUILayout.BeginVertical(EditorStyles.textArea);
        if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(16))) { ret = false; }
        GUILayout.BeginHorizontal();
        GUILayout.Label("Type: ", GUILayout.Width(50));
        dr.Type = (DialogRequirementType)EditorGUILayout.EnumPopup(dr.Type);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Target: ", GUILayout.Width(50));
        dr.Target = (DialogRequirementTarget)EditorGUILayout.EnumPopup(dr.Target);
        GUILayout.EndHorizontal();
        if (dr.Type != DialogRequirementType.LifeTime)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Value: ", GUILayout.Width(60));
            dr.IntValue = EditorGUILayout.IntField(dr.IntValue);
            GUILayout.EndHorizontal();
        }
        if (dr.Type == DialogRequirementType.LifeTime | dr.Type == DialogRequirementType.PastState)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Timespan: ", GUILayout.Width(60));
            dr.FloatValue = EditorGUILayout.FloatField(dr.FloatValue);
            GUILayout.EndHorizontal();
        }
        if (dr.Type == DialogRequirementType.EventLog)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Text:", GUILayout.Width(60));
            dr.StringValue = EditorGUILayout.TextField(dr.StringValue);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        return ret;
    }

    void DisplayDialogLoop(Rect r, DialogOption dOption)
    {
        Color prev = GUI.color;
        GUI.color = Color.yellow;
        if (GUI.Button(new Rect(r.x, r.y+5, r.width, r.height-5), "Loop: " + dOption.NextDialog.Title))
        {
            InspectOptionNode(dOption);
        }
        GUI.color = prev;
        if (r.x + r.width > countedWidth) { countedWidth = r.x + r.width; }
        if (r.y + r.height > countedHeight) { countedHeight = r.y + r.height; }
    }

    void DisplaySingleDialogNode(Rect r, Dialog d, int depth, int indent)
    {
        Rect title = new Rect(r.x, r.y+5, r.width-20, r.height-5);
        if (title.y + title.height > countedHeight) { countedHeight = title.y + title.height; }
        if (title.x + title.width > countedWidth) { countedWidth = title.x + title.width; }
        if (GUI.Button(title, d.Title != null ? d.Title.Description : "No Title"))
        {
            InspectDialogNode(d);
        }
    }

    private List<Dialog> GetAllDialogsInChain(List<Dialog> dl, Dialog d)
    {
        if (!dl.Contains(d))
        {
            dl.Add(d);
        }
        for (int i = 0; i < d.Options.Count; i++)
        {
            if (d.Options[i].NextDialog != null)
            {
                if (!d.Options[i].IsRedirection)
                {
                    GetAllDialogsInChain(dl, d.Options[i].NextDialog);
                }
            }
        }
        return dl;
    }

    private void DeleteCleanupDialog(Dialog toDelete)
    {
        DeleteRecurseDialog(toDelete, activeDialog, false);
        ReleaseDialogID(toDelete.ID);
    }

    private void DeleteRecurseDialog(Dialog toDelete, Dialog currentItemToCheck, bool releaseIfNotLoop)
    {
        for (int i = currentItemToCheck.Options.Count; i-- > 0; )
        {
            if (currentItemToCheck.Options[i].NextDialog == null) { continue; }

            if (currentItemToCheck.Options[i].NextDialog == toDelete | releaseIfNotLoop)
            {

                if (!currentItemToCheck.Options[i].IsRedirection)
                {
                    DeleteRecurseDialog(toDelete, currentItemToCheck.Options[i].NextDialog, true);
                }

                currentItemToCheck.Options[i].NextDialog = null;
            }
            else
            {
                DeleteRecurseDialog(toDelete, currentItemToCheck.Options[i].NextDialog, false);
            }

        }
    }
}
