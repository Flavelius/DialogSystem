using DialogSystem;
using DialogSystem.Requirements.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using DialogSystem.Triggers;

public class DialogEditor : EditorWindow
{
    public DialogCollection sourceCollection;
    List<Type> usableRequirementTypes = new List<Type>();
    List<string> usableRequirementNames = new List<string>();

    List<Type> usableTriggerTypes = new List<Type>();
    List<string> usableTriggerNames = new List<string>();

    private GUIStyle headerStyle;
    private Color inspectorColor;
    private GUIStyle buttonStyle;
    private GUIStyle lblStyle;

    private const string txtNotSetMsg = "Text not set";

    private int GetUniqueID(Dialog d)
    {
        int iid = d.GetInstanceID();
        return Mathf.Abs(iid);
    }

    private void CollectUsableRequirementTypes()
    {
        usableRequirementTypes.Clear();
        usableRequirementNames.Clear();
        List<Type> types = new List<Type>
            (
            Assembly.GetAssembly(typeof(BaseRequirement)).GetTypes().Where(i => i.IsSubclassOf(typeof(BaseRequirement)) && i.IsPublic && i.IsClass && !i.IsAbstract)
            );
        usableRequirementTypes.Add(null);
        usableRequirementNames.Add("Add");
        foreach (Type t in types)
        {
            usableRequirementTypes.Add(t);
            ReadableNameAttribute[] rns = t.GetCustomAttributes(typeof(ReadableNameAttribute), false) as ReadableNameAttribute[];
            if (rns.Length > 0)
            {
                usableRequirementNames.Add(rns[0].Name);
            }
            else
            {
                usableRequirementNames.Add(t.Name);
            }
        }
    }

    private void CollectUsableTriggerTypes()
    {
        usableTriggerTypes.Clear();
        usableTriggerNames.Clear();
        List<Type> types = new List<Type>
            (
            Assembly.GetAssembly(typeof(DialogOptionTrigger)).GetTypes().Where(i => i.IsSubclassOf(typeof(DialogOptionTrigger)) && i.IsPublic && i.IsClass && !i.IsAbstract)
            );
        usableTriggerTypes.Add(null);
        usableTriggerNames.Add("Add");
        foreach (Type t in types)
        {
            usableTriggerTypes.Add(t);
            ReadableNameAttribute[] rns = t.GetCustomAttributes(typeof(ReadableNameAttribute), false) as ReadableNameAttribute[];
            if (rns.Length > 0)
            {
                usableTriggerNames.Add(rns[0].Name);
            }
            else
            {
                usableTriggerNames.Add(t.Name);
            }
        }
    }

    public void Cleanup()
    {
        if (sourceCollection != null)
        {
            EditorUtility.SetDirty(sourceCollection);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(sourceCollection));
        }
        if (activeStringEditor != null) 
        {
            activeStringEditor.EndEdit();
            activeStringEditor = null;
        }
    }

    void OnEnable()
    {
        CollectUsableTriggerTypes();
        CollectUsableRequirementTypes();
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
        activeStringEditor = null;
    }
    private float leftColumnWidth = 200;
    private float rightColumnoverlayWidth = 200;
    private Color nodeColor = new Color(0.75f,0.75f,0.75f,1f);
    Vector2 dialogsScroll = Vector2.zero;

    private LocalizedStringEditor activeStringEditor;

    void OnGUI()
    {
        if (sourceCollection == null) { return; }
        if (activeStringEditor != null) { GUI.enabled = false; }
        else { GUI.enabled = (activeOptionNode == null && activeSubDialog == null); }
        Rect Left = new Rect(0, 0, leftColumnWidth, position.height);
        Color prev = GUI.color;
        GUI.color = inspectorColor;
        GUI.BeginGroup(Left, EditorStyles.textField);
        GUI.color = prev;
        DisplayDialogTools(Left);
        GUI.EndGroup();
        DisplayDialogEditor(new Rect(leftColumnWidth, 0, position.width-leftColumnWidth, position.height));
        if (activeOptionNode != null)
        {
            GUI.enabled = true & activeStringEditor == null;
            DisplayOptionNodeInspector(new Rect(position.width - rightColumnoverlayWidth, 0, rightColumnoverlayWidth, position.height), activeOptionNode);
        }
        else if (activeSubDialog != null)
        {
            GUI.enabled = true & activeStringEditor == null;
            DisplayDialogNodeInspector(new Rect(position.width - rightColumnoverlayWidth, 0, rightColumnoverlayWidth, position.height), activeSubDialog);
        }
        if (activeStringEditor != null)
        {
            GUI.enabled = true;
            if (activeStringEditor.DrawGUI() == false)
            {
                activeStringEditor.EndEdit();
                activeStringEditor = null;
            }
        }
    }

    public void Initialize()
    {
        if (headerStyle == null) 
        {
            headerStyle = new GUIStyle(GUI.skin.GetStyle("TL SelectionBarPreview"));
            headerStyle.stretchWidth = true; 
            headerStyle.fontStyle = FontStyle.Bold; 
            headerStyle.fontSize = 14;
            headerStyle.normal.textColor = new Color(0.7f, 0.6f, 0.6f);
        }
        lblStyle = new GUIStyle(GUI.skin.GetStyle("flow overlay header upper left"));
        lblStyle.stretchWidth = true;
        inspectorColor = Color.Lerp(Color.gray, Color.white, 0.5f);
        buttonStyle = GUI.skin.GetStyle("PreButton");
        CollectUsableRequirementTypes();
        CollectUsableTriggerTypes();
    }

    void Update()
    {
        if (sourceCollection == null)
        {
            Cleanup();
            Close();
            return;
        }
    }

    void OnDestroy()
    {
        Cleanup();
    }

    bool isValidName(string name)
    {
        return !string.IsNullOrEmpty(name) && !name.StartsWith(" ");
    }

    public static void OpenEdit(DialogCollection collection)
    {
        DialogEditor window = EditorWindow.GetWindow<DialogEditor>("Dialog Editor");
        window.Close();
        window = EditorWindow.GetWindow<DialogEditor>("Dialog Editor");
        window.minSize = new Vector2(600, 400);
        window.Load(collection);
    }

    public void Load(DialogCollection collection)
    {
        Initialize();
        sourceCollection = collection;
    }

    private void DeleteDialog(Dialog d)
    {
        if (sourceCollection.dialogs.Remove(d))
        {
            DestroyImmediate(d, true);
            DirtyAsset();
        }
    }

    private void DirtyAsset()
    {
        EditorUtility.SetDirty(sourceCollection);
    }

    private void AddToAsset(ScriptableObject so)
    {
        so.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
        AssetDatabase.AddObjectToAsset(so, sourceCollection);
        DirtyAsset();
    }

    private void DeleteCleanupDialog(Dialog toDelete)
    {
        DeleteRecurseDialog(toDelete, activeDialog, false);
        CleanupUnreferencedAssets();
    }

    private void DeleteRecurseDialog(Dialog toDelete, Dialog currentItemToCheck, bool releaseIfNotLoop)
    {
        if (currentItemToCheck == null) { return; }
        for (int i = currentItemToCheck.Options.Count; i-- > 0; )
        {
            if (currentItemToCheck.Options[i].NextDialog == null) { continue; }

            if (currentItemToCheck.Options[i].NextDialog == toDelete | releaseIfNotLoop)
            {
                if (!currentItemToCheck.Options[i].IsRedirection)
                {
                    DeleteRecurseDialog(toDelete, currentItemToCheck.Options[i].NextDialog, true);
                    //clean
                    for (int r = currentItemToCheck.Options[i].NextDialog.Requirements.Count; r-- > 0; )
                    {
                        DestroyImmediate(currentItemToCheck.Options[i].NextDialog.Requirements[r], true);
                    }
                    DestroyImmediate(currentItemToCheck.Options[i].NextDialog, true);
                    //
                }
                currentItemToCheck.Options[i].NextDialog = null;
            }
            else
            {
                DeleteRecurseDialog(toDelete, currentItemToCheck.Options[i].NextDialog, false);
            }
        }
    }

    private void CleanupUnreferencedAssets()
    {
        string path = AssetDatabase.GetAssetPath(sourceCollection);
        object[] objs = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (object o in objs)
        {
            Dialog d = o as Dialog;
            if (d != null)
            {
                if (!IsReferencedInChains(d))
                {
                    Debug.Log("Unreferenced Dialog: " + d.ID);
                }
            }
            else
            {
                DialogOptionTrigger tr = o as DialogOptionTrigger;
                if (tr != null)
                {
                    if (!IsReferencedInChains(tr))
                    {
                        Debug.Log("Unreferenced trigger: " + tr.CachedName);
                    }
                }
                else
                {
                    BaseRequirement br = o as BaseRequirement;
                    if (br != null)
                    {
                        if (!IsReferencedInChains(br))
                        {
                            Debug.Log("Unreferenced Requirement: " + br.CachedName);
                        }
                    }
                }
            }
        }
    }

    private bool IsReferencedInChains(Dialog d)
    {
        foreach (Dialog dl in sourceCollection.dialogs)
        {
            List<Dialog> chain = new List<Dialog>();
            chain = GetAllDialogsInChain(chain, dl);
            foreach (Dialog chainDialog in chain)
            {
                if (chainDialog == d)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsReferencedInChains(DialogOptionTrigger tr)
    {
        foreach (Dialog dl in sourceCollection.dialogs)
        {
            List<Dialog> chain = new List<Dialog>();
            chain = GetAllDialogsInChain(chain, dl);
            foreach (Dialog chainDialog in chain)
            {
                foreach (DialogOption dop in chainDialog.Options)
                {
                    if (dop.Triggers.Contains(tr))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool IsReferencedInChains(BaseRequirement br)
    {
        foreach (Dialog dl in sourceCollection.dialogs)
        {
            List<Dialog> chain = new List<Dialog>();
            chain = GetAllDialogsInChain(chain, dl);
            foreach (Dialog chainDialog in chain)
            {
                if (chainDialog.Requirements.Contains(br))
                {
                    return true;
                }
            }
        }
        return false;
    }

    void DisplayDialogTools(Rect r)
    {
        GUILayout.Space(5);
        GUILayout.BeginVertical(EditorStyles.textArea,GUILayout.Width(r.width-8));
        if (GUILayout.Button("New Dialog"))
        {
            Dialog d = ScriptableObject.CreateInstance<Dialog>();
            AddToAsset(d);
            d.ID = GetUniqueID(d);
            sourceCollection.dialogs.Add(d);
            activeDialog = d;
        }
        GUILayout.EndVertical();
        Rect itemRect = new Rect(0, 0, r.width - 38, 17);
        Rect deleteItemRect = new Rect(itemRect.width, 0, 18, 17);
        Rect scrollContainerRect = new Rect(2, 30, r.width - 5, r.height - 32);
        Rect contentScrollRect = new Rect(0, 0, r.width - 20, Mathf.Max(sourceCollection.dialogs.Count * itemRect.height, scrollContainerRect.height));
        dialogsScroll = GUI.BeginScrollView(scrollContainerRect, dialogsScroll, contentScrollRect, false, true);
        for (int i = 0; i < sourceCollection.dialogs.Count; i++)
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
            if (sourceCollection.dialogs[i] == activeDialog)
            {
                GUI.color = Color.gray;
            }
            string dTitle = sourceCollection.dialogs[i].Title.Description;
            if (dTitle == null || dTitle.Length == 0)
            {
                dTitle = txtNotSetMsg;
            }
            if (GUI.Button(itemRect, dTitle, "ButtonLeft"))
            {
                if (activeDialog == sourceCollection.dialogs[i]) { activeDialog = null; }
                else
                {
                    activeDialog = sourceCollection.dialogs[i];
                }
            }
            GUI.color = Color.white;
            if (GUI.Button(deleteItemRect, "x", "ButtonRight"))
            {
                DeleteCleanupDialog(sourceCollection.dialogs[i]);
                if (sourceCollection.dialogs[i] == activeDialog) { CloseSubInspector(); activeDialog = null; }
                DeleteDialog(sourceCollection.dialogs[i]);
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
            Repaint();
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
            lastOptionDepth = branchDepth;
            int move = DisplayDialogOption(new Rect(r.x + nodeWidth + indentWidth, r.y + nodeHeight * lastOptionDepth, nodeWidth + indentWidth, nodeHeight), d.Options[i]);
            if (d.Options[i].NextDialog != null)
            {
                if (d.Options[i].IsRedirection)
                { 
                    DisplayDialogLoop(new Rect(r.x + nodeWidth + indentWidth * 2, r.y + nodeHeight * lastOptionDepth, nodeWidth + indentWidth, nodeHeight), d.Options[i]);
                    branchDepth += 1;
                }
                else
                {
                    branchDepth += RecurseDialogs(new Rect(parent.x + indentWidth * 2, parent.y, nodeWidth, nodeHeight), length + branchDepth, depth + 1, d.Options[i].NextDialog);
                }
            }
            else
            {
                branchDepth += 1;
                if (d.Options[i].Triggers.Count > 0)
                {
                    branchDepth += 1;
                }
            }
            if (move != 0)
            {
                if (move == -10)
                {
                    if (d.Options.Count > 0)
                    {
                        DialogOption dI = d.Options[i];
                        if (dI.NextDialog != null && !dI.IsRedirection)
                        {
                            DeleteCleanupDialog(dI.NextDialog);
                        }
                        for (int t = dI.Triggers.Count; t-- > 0; )
                        {
                            DestroyImmediate(dI.Triggers[t], true);
                        }
                        d.Options.Remove(dI);
                        DestroyImmediate(dI, true);
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
        if (DrawOptionRegion(new Rect(r.x + nodeWidth + indentWidth, r.y + nodeHeight, nodeWidth + indentWidth, nodeHeight), lastOptionDepth))
        {
            DialogOption dO = ScriptableObject.CreateInstance<DialogOption>();
            AddToAsset(dO);
            d.Options.Add(dO);
        }
        branchDepth++;
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
        string tTitle = option.Text.Description;
        if (tTitle == null || tTitle.Length == 0)
        {
            tTitle = txtNotSetMsg;
        }
        if (GUI.Button(title, tTitle, gs))
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
        DrawInlineDialogOptionTriggers(new Rect(title.x + indentWidth, title.y + title.height, title.width, title.height + 10), option);
        if (option.NextDialog != null)
        {
            DrawInlineDialogRequirements(new Rect(title.x+indentWidth, title.y - (title.height+2), title.width, title.height+10), option);
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

    void DrawInlineDialogRequirements(Rect r, DialogOption d)
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
        for (int i = d.NextDialog.Requirements.Count; i-- > 0; )
        {
            BaseRequirement req = d.NextDialog.Requirements[i];
            if (req != null)
            {
                GUI.color = req.GetColor();
                GUILayout.Box(new GUIContent(req.GetShortIdentifier(), req.GetToolTip()), gs, GUILayout.Width(19), GUILayout.Height(15));
            }
        }
        GUI.color = prev;
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void DrawInlineDialogOptionTriggers(Rect r, DialogOption d)
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
        for (int i = d.Triggers.Count; i-- > 0; )
        {
            DialogOptionTrigger dot = d.Triggers[i];
            GUI.color = dot.GetColor();
            GUILayout.Box(new GUIContent(dot.GetShortIdentifier(), dot.GetToolTip()), gs, GUILayout.Width(19), GUILayout.Height(15));
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
        if (depth > 0)
        {
            GUI.Box(new Rect(r.x - r.width + indentWidth - 2, r.y, 5, r.height * depth), "", EditorStyles.textArea);
        }
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
        string tText = dOption.Text.Description;
        if (tText == null || tText.Length == 0)
        {
            tText = txtNotSetMsg;
        }
        GUILayout.Label(tText, lblStyle);
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Edit", buttonStyle))
        {
            activeStringEditor = new LocalizedStringEditor(dOption.Text, "Option text");
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
            Dialog d = ScriptableObject.CreateInstance<Dialog>();
            AddToAsset(d);
            d.ID = GetUniqueID(d);
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
            string dTitle = ddialogs[i].Title.Description;
            if (dTitle == null || dTitle.Length == 0)
            {
                dTitle = txtNotSetMsg;
            }
            if (GUILayout.Button(dTitle, buttonStyle))
            {
                dOption.NextDialog = ddialogs[i];
                dOption.IsRedirection = true;
                CloseSubInspector();
                break;
            }
        }
        GUILayout.EndScrollView();
        GUILayout.Space(10);
        GUILayout.Label("Triggers", headerStyle);
        GUILayout.BeginHorizontal();
        bool prevEnabled = GUI.enabled;
        if (dOption.Triggers.Count >= 6) { GUI.enabled = false; }
        int index = EditorGUILayout.Popup(0, usableTriggerNames.ToArray());
        if (index > 0)
        {
            DialogOptionTrigger tr = ScriptableObject.CreateInstance(usableTriggerTypes[index]) as DialogOptionTrigger;
            AddToAsset(tr);
            dOption.Triggers.Add(tr);
        }
        GUI.enabled = prevEnabled;
        if (GUILayout.Button("Remove all", buttonStyle))
        {
            for (int i = dOption.Triggers.Count; i-- > 0; )
            {
                DestroyImmediate(dOption.Triggers[i], true);
                dOption.Triggers.RemoveAt(i);
            }
        }
        GUILayout.EndHorizontal();
        scrollbar2 = GUILayout.BeginScrollView(scrollbar2);
        for (int i = dOption.Triggers.Count; i-- > 0; )
        {
            if (dOption.Triggers[i] == null) { dOption.Triggers.RemoveAt(i); continue; }
            if (!InlineDisplayTriggerEditor(dOption.Triggers[i]))
            {
                DestroyImmediate(dOption.Triggers[i], true);
                dOption.Triggers.RemoveAt(i);
                continue;
            }
        }
        GUILayout.EndScrollView();
        if (GUILayout.Button("Close", buttonStyle))
        {
            //RemoveDuplicateNotifications(dOption.Triggers);
            CloseSubInspector();
        }
        GUILayout.EndArea();
    }

    bool InlineDisplayTriggerEditor(DialogOptionTrigger tr)
    {
        bool ret = true;
        GUILayout.BeginVertical(EditorStyles.textArea);
        GUILayout.Label(tr.CachedName, EditorStyles.helpBox);
        if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(16))) { ret = false; }
        SerializedObject so = new SerializedObject(tr);
        SerializedProperty sp = so.GetIterator();
        sp.NextVisible(true);
        if (sp != null)
        {
            while (sp.NextVisible(false))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(sp.name, GUILayout.ExpandWidth(true));
                EditorGUILayout.PropertyField(sp, GUIContent.none);
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndVertical();
        return ret;
    }

    void RemoveDuplicateTriggers(List<DialogOptionTrigger> sourceList)
    {
        List<DialogOptionTrigger> cleanList = new List<DialogOptionTrigger>();
        for (int i = 0; i < sourceList.Count; i++)
        {
            bool found = false;
            for (int cl = 0; cl < cleanList.Count; cl++)
            {
                if (cleanList[cl].GetType() == sourceList[i].GetType())
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

    void DisplayDialogNodeInspector(Rect r, Dialog d)
    {
        Color prev = GUI.color;
        GUI.color = inspectorColor;
        GUILayout.BeginArea(r, EditorStyles.textArea);
        GUI.color = prev;
        GUILayout.Space(5);
        GUILayout.Label("Title", headerStyle);
        string dTitle = d.Title.Description;
        if (dTitle == null || dTitle.Length == 0)
        {
            dTitle = txtNotSetMsg;
        }
        GUILayout.Label(dTitle, lblStyle);
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Edit", buttonStyle))
        {
            activeStringEditor = new LocalizedStringEditor(d.Title, "Dialog title");
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.Label("Text", headerStyle);
        string dText = d.Text.Description;
        if (dText == null || dText.Length == 0)
        {
            dText = txtNotSetMsg;
        }
        GUILayout.Label(dText, lblStyle);
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Edit", buttonStyle))
        {
            activeStringEditor = new LocalizedStringEditor(d.Text, "Dialog text");
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
            d.RequirementMode = (DialogRequirementMode)EditorGUILayout.EnumPopup("Mode:",d.RequirementMode);
        }
        GUILayout.BeginHorizontal();
        bool prevEnabled = GUI.enabled;
        if (d.Requirements.Count >= 6) { GUI.enabled = false; }
        int index = EditorGUILayout.Popup(0, usableRequirementNames.ToArray());
        if (index > 0)
        {
            BaseRequirement bs = ScriptableObject.CreateInstance(usableRequirementTypes[index]) as BaseRequirement;
            AddToAsset(bs);
            d.Requirements.Add(bs);
        }
        GUI.enabled = prevEnabled;
        if (GUILayout.Button("Remove all", buttonStyle))
        {
            for (int i = d.Requirements.Count; i-- > 0; )
            {
                DestroyImmediate(d.Requirements[i], true);
                d.Requirements.RemoveAt(i);
            }
        }
        GUILayout.EndHorizontal();
        scrollbar = GUILayout.BeginScrollView(scrollbar, false, true);
        for (int i = d.Requirements.Count; i-- > 0; )
        {
            if (d.Requirements[i] == null) { d.Requirements.RemoveAt(i); continue; }
            if (!DrawInlineRequirement(d.Requirements[i])) {
                DestroyImmediate(d.Requirements[i], true);
                d.Requirements.RemoveAt(i); 
                continue;
            }
        }
        GUILayout.EndScrollView();
        if (GUILayout.Button("Close", buttonStyle))
        {
            //RemoveDuplicateRequirements(d.Requirements);
            CloseSubInspector();
        }
        GUILayout.EndArea();
    }

    void RemoveDuplicateRequirements(List<BaseRequirement> sourceList)
    {
        List<BaseRequirement> cleanList = new List<BaseRequirement>();
        for (int i = 0; i < sourceList.Count; i++)
        {
            bool found = false;
            for (int cl = 0; cl < cleanList.Count; cl++)
            {
                if (cleanList[cl].GetType() == sourceList[i].GetType())
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

    bool DrawInlineRequirement(BaseRequirement dr)
    {
        bool ret = true;
        GUILayout.BeginVertical(EditorStyles.textArea);
        GUILayout.Label(string.Format("({0}) {1}", dr.Target, dr.CachedName), EditorStyles.helpBox);
        if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(16))) { ret = false; }
        SerializedObject so = new SerializedObject(dr);
        SerializedProperty sp = so.GetIterator();
        sp.NextVisible(true);
        if (sp != null)
        {
            while (sp.NextVisible(false))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(sp.name, GUILayout.ExpandWidth(true));
                EditorGUILayout.PropertyField(sp, GUIContent.none);
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndVertical();
        return ret;
    }

    void DisplayDialogLoop(Rect r, DialogOption dOption)
    {
        Color prev = GUI.color;
        GUI.color = Color.yellow;
        string ndTitle = dOption.NextDialog.Title.Description;
        if (ndTitle == null || ndTitle.Length == 0)
        {
            ndTitle = txtNotSetMsg;
        }
        ndTitle = string.Format("({0}) {1}", dOption.NextDialog.ID, ndTitle);
        if (GUI.Button(new Rect(r.x, r.y+5, r.width, r.height-5), "Loop: " + ndTitle))
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
        string dTitle = d.Title.Description;
        if (dTitle == null || dTitle.Length == 0)
        {
            dTitle = txtNotSetMsg;
        }
        dTitle = string.Format("({0}) {1}", d.ID, dTitle);
        if (GUI.Button(title, dTitle))
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

}
