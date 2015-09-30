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
        GUI.BeginGroup(Left, EditorStyles.textField);
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
    }

    void OnSelectionChange()
    {
        if (!Selection.activeGameObject) { return; }
        IConversationRelevance rel = Selection.activeGameObject.GetComponent(typeof(IConversationRelevance)) as IConversationRelevance;
        if (rel != null && rel.Type != DialogRequirementTarget.Player && isValidName(rel.Name))
        {
            npcFilter = rel.Name;
            newNpcName = rel.Name;
        }
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

    string newNpcName = "";
    string npcFilter = "";
    bool showCreateNewNpc = false;
    void DisplayDialogTools(Rect r)
    {
        GUILayout.Space(5);
        GUILayout.BeginVertical(EditorStyles.textArea,GUILayout.Width(r.width-8));
        if (GUILayout.Button((showCreateNewNpc?"Cancel":"New Dialog")))
        {
            showCreateNewNpc = !showCreateNewNpc;
        }
        if (showCreateNewNpc)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Npc: ", GUILayout.Width(30));
            newNpcName = GUILayout.TextField(newNpcName, GUILayout.Width(95));
            if (GUILayout.Button("Create") & isValidName(newNpcName))
            {
                Dialog d = new Dialog(true);
                d.ID = ReserveDialogID();
                d.Tag = GenerateUniqueTag();
                d.Npc = newNpcName;
                npcFilter = newNpcName;
                dialogCollection.Add(d);
                activeDialog = d;
                showCreateNewNpc = false;
            }
            GUILayout.EndHorizontal();
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
            npcFilter = "";
            newNpcName = "";
            showCreateNewNpc = false;
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
        GUILayout.BeginHorizontal(GUILayout.Width(r.width-8));
        GUILayout.Label("Filter by Npc:", GUILayout.Width(80));
        EditorGUI.BeginChangeCheck();
        npcFilter = GUILayout.TextField(npcFilter);
        if (EditorGUI.EndChangeCheck())
        {
            while (npcFilter.StartsWith(" "))
            {
                npcFilter = npcFilter.Remove(0, 1);
            }
            newNpcName = npcFilter;
            CloseSubInspector();
            activeDialog = null;
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginScrollView(dialogsScroll, false, true, GUILayout.Width(r.width-5), GUILayout.Height(r.height-(showCreateNewNpc?116:95)));
        GUILayout.BeginVertical();
        for (int i = dialogCollection.Count; i-- > 0; )
        {
            if (isValidName(npcFilter) && !dialogCollection[i].Npc.Equals(npcFilter, StringComparison.OrdinalIgnoreCase)) { continue; }
            if (dialogCollection[i] == activeDialog)
            {
                GUI.color = Color.gray;
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(dialogCollection[i].Title != null ? dialogCollection[i].Title.Description : "No Title", dialogCollection[i].Npc), EditorStyles.miniButtonLeft))
            {
                if (activeDialog == dialogCollection[i]) { activeDialog = null; npcFilter = ""; newNpcName = ""; }
                else
                {
                    activeDialog = dialogCollection[i];
                    npcFilter = activeDialog.Npc;
                    newNpcName = npcFilter;
                }
            }
            if (GUILayout.Button("x", EditorStyles.miniButtonRight, GUILayout.Width(18)))
            {
                DeleteCleanupDialog(dialogCollection[i]);
                if (dialogCollection[i] == activeDialog) { CloseSubInspector(); activeDialog = null; }
                dialogCollection.RemoveAt(i);
                break;
            }
            GUILayout.EndHorizontal();
            GUI.color = Color.white;
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
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
                d.Options.Add(new DialogOption("Not Set"));
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
    void DisplayOptionNodeInspector(Rect r, DialogOption dOption)
    {
        GUILayout.BeginArea(r, EditorStyles.textArea);
        GUILayout.Space(5);
        GUILayout.Label("Title: ");
        GUILayout.Label(dOption.Text != null ? dOption.Text.Description : "Not Set");
        if (dOption.Text == null)
        {
            if (GUILayout.Button("Add"))
            {
                dOption.Text = new Localization.LocalizedString();
            }
        }
        else
        {
            if (GUILayout.Button("Edit"))
            {
                Localization.LocalizedStringEditor.OpenEdit(dOption.Text);
            }
        }
        GUILayout.Space(10);
        if (GUILayout.Button("New Sub-Dialog"))
        {
            int id = ReserveDialogID();
            Dialog d = new Dialog(true);
            d.ID = id;
            d.Tag = GenerateUniqueTag();
            d.Npc = activeDialog.Npc;
            dOption.NextDialog = d;
            dOption.IsRedirection = false;
            CloseSubInspector();
        }
        GUILayout.BeginVertical(GUILayout.Width(rightColumnoverlayWidth));
        GUILayout.Label("Go to existing: ", GUILayout.Width(rightColumnoverlayWidth));
        scrollbar = GUILayout.BeginScrollView(scrollbar, false, true, GUILayout.Width(rightColumnoverlayWidth-5));
        List<Dialog> ddialogs = new List<Dialog>();
        ddialogs = GetAllDialogsInChain(ddialogs, activeDialog);
        for (int i = 0; i < ddialogs.Count; i++)
        {
            if (ddialogs[i].Options.Contains(dOption)) { continue; }
            if (GUILayout.Button(ddialogs[i].Title != null ? ddialogs[i].Title.Description : "No Title"))
            {
                dOption.NextDialog = ddialogs[i];
                dOption.IsRedirection = true;
                CloseSubInspector();
                break;
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.Space(10);
        GUILayout.Label("Notifications:");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add"))
        {
            dOption.Notifications.Add(new DialogOptionNotification());
        }
        if (GUILayout.Button("Remove all"))
        {
            dOption.Notifications.Clear();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginScrollView(scrollbar, GUILayout.Width(rightColumnoverlayWidth - 5));
        GUILayout.BeginVertical();
        for (int i = dOption.Notifications.Count; i-- > 0; )
        {
            if (!InlineDisplayNotificationEditor(dOption.Notifications[i]))
            {
                dOption.Notifications.RemoveAt(i);
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        if (GUILayout.Button("Close"))
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
        GUILayout.BeginArea(r, EditorStyles.textArea);
        GUILayout.Space(5);
        GUILayout.Label("Title: ");
        GUILayout.Label(d.Title!=null?d.Title.Description:"Not Set");
        if (d.Title == null)
        {
            if (GUILayout.Button("Add"))
            {
                d.Title = new Localization.LocalizedString();
            }
        }
        else
        {
            if (GUILayout.Button("Edit"))
            {
                Localization.LocalizedStringEditor.OpenEdit(d.Title);
            }
        }
        GUILayout.Space(10);
        Color prev = GUI.color;
        GUI.color = HasUniqueTag(d) ? Color.green : Color.yellow;
        GUILayout.Label(new GUIContent("Tag: ", "Should be unique if not intended duplicate for special reasons"), GUILayout.Width(30));
        EditorGUI.BeginChangeCheck();
        d.Tag = GUILayout.TextField(d.Tag, GUILayout.Width(r.width-43));
        if (EditorGUI.EndChangeCheck())
        {
            if (!isValidName(d.Tag))
            {
                while (!HasUniqueTag(d))
                {
                    d.Tag = GenerateUniqueTag();
                }
            }
        }
        GUILayout.Space(10);
        GUI.color = prev;
        GUILayout.BeginVertical();
        GUILayout.Label("Text: ");
        GUILayout.Label(d.Text != null ? d.Text.Description : "Not Set");
        if (d.Text == null)
        {
            if (GUILayout.Button("Add"))
            {
                d.Text = new Localization.LocalizedString();
            }
        }
        else
        {
            if (GUILayout.Button("Edit"))
            {
                Localization.LocalizedStringEditor.OpenEdit(d.Text);
            }
        }
        GUILayout.Space(10);
        GUILayout.Label("Requirements: ");
        GUILayout.BeginHorizontal();
        bool prevEnabled = GUI.enabled;
        if (d.Requirements.Count >= 6) { GUI.enabled = false; }
        if (GUILayout.Button("Add"))
        {
            d.Requirements.Add(new DialogRequirement());
        }
        GUI.enabled = prevEnabled;
        if (GUILayout.Button("Remove all"))
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
        GUILayout.EndVertical();

        if (GUILayout.Button("Close"))
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

    private bool HasUniqueTag(Dialog d)
    {
        for (int i = 0; i < dialogCollection.Count; i++)
        {
            List<Dialog> subDialogs = new List<Dialog>();
            subDialogs = GetAllDialogsInChain(subDialogs, dialogCollection[i]);
            for (int s = 0; s < subDialogs.Count; s++)
            {
                if (subDialogs[s] != d && subDialogs[s].Tag.Equals(d.Tag, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
        }
        return true;
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

    private string GenerateUniqueTag()
    {
        return Guid.NewGuid().ToString().GetHashCode().ToString("x");
    }
}
