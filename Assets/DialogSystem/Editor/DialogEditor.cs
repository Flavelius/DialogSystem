using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DialogSystem.Actions;
using DialogSystem.Localization;
using DialogSystem.Requirements;
using UnityEditor;
using UnityEngine;

namespace DialogSystem
{
    public class DialogEditor : EditorWindow
    {
        const string TxtNotSetMsg = "Text not set";
        const float IndentWidth = 50;
        const float LeftColumnWidth = 200;
        const float NodeHeight = 30;
        const float NodeWidth = 180;
        const float RightColumnoverlayWidth = 200;
        readonly Color _nodeColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        readonly List<string> _usableActionNames = new List<string>();

        readonly List<Type> _usableActionTypes = new List<Type>();
        readonly List<string> _usableRequirementNames = new List<string>();
        readonly List<Type> _usableRequirementTypes = new List<Type>();

        Dialog _activeDialog;
        DialogOption _activeOptionNode;

        LocalizedStringEditor _activeStringEditor;
        Dialog _activeSubDialog;
        GUIStyle _buttonStyle;
        float _countedHeight;

        float _countedWidth;
        Vector2 _dialogsScroll = Vector2.zero;

        GUIStyle _headerStyle;
        Color _inspectorColor;
        GUIStyle _lblStyle;

        Vector2 _mainViewScrollPos;
        float _previousState;

        Vector2 _scrollbar;
        Vector2 _scrollbar2;
        public DialogCollection SourceCollection;

        int GetUniqueId()
        {
            return SourceCollection.GetUniqueId();
        }

        void CollectUsableRequirementTypes()
        {
            _usableRequirementTypes.Clear();
            _usableRequirementNames.Clear();
            var types = new List<Type>
                (
                Assembly.GetAssembly(typeof (DialogRequirement)).GetTypes().Where(i => i.IsSubclassOf(typeof (DialogRequirement)) && i.IsPublic && i.IsClass && !i.IsAbstract)
                );
            _usableRequirementTypes.Add(null);
            _usableRequirementNames.Add("Add");
            foreach (var t in types)
            {
                _usableRequirementTypes.Add(t);
                var rns = t.GetCustomAttributes(typeof (ReadableNameAttribute), false) as ReadableNameAttribute[];
                if (rns == null) continue;
                if (rns.Length == 0)
                {
                    _usableRequirementNames.Add(t.Name);
                }
                else
                {
                    _usableRequirementNames.Add(rns[0].Name);
                }
            }
        }

        void CollectUsableActionTypes()
        {
            _usableActionTypes.Clear();
            _usableActionNames.Clear();
            var types = new List<Type>
                (
                Assembly.GetAssembly(typeof (DialogOptionAction))
                    .GetTypes()
                    .Where(i => i.IsSubclassOf(typeof (DialogOptionAction)) && i.IsPublic && i.IsClass && !i.IsAbstract)
                );
            _usableActionTypes.Add(null);
            _usableActionNames.Add("Add");
            foreach (var t in types)
            {
                _usableActionTypes.Add(t);
                var rns = t.GetCustomAttributes(typeof (ReadableNameAttribute), false) as ReadableNameAttribute[];
                if (rns == null) continue;
                if (rns.Length > 0)
                {
                    _usableActionNames.Add(rns[0].Name);
                }
                else
                {
                    _usableActionNames.Add(t.Name);
                }
            }
        }

        public void Cleanup()
        {
            if (SourceCollection != null)
            {
                EditorUtility.SetDirty(SourceCollection);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(SourceCollection));
            }
            if (_activeStringEditor != null)
            {
                _activeStringEditor.EndEdit();
                _activeStringEditor = null;
            }
        }

        public void Initialize()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(GUI.skin.GetStyle("TL SelectionBarPreview"))
                {
                    stretchWidth = true,
                    fontStyle = FontStyle.Bold,
                    fontSize = 14,
                    normal = {textColor = new Color(0.7f, 0.6f, 0.6f)}
                };
            }
            _lblStyle = new GUIStyle(GUI.skin.GetStyle("MeTransitionHead")) {stretchWidth = true};
            _inspectorColor = Color.Lerp(Color.gray, Color.white, 0.5f);
            _buttonStyle = GUI.skin.GetStyle("PreButton");
            CollectUsableRequirementTypes();
            CollectUsableActionTypes();
        }

        void OnEnable()
        {
            CollectUsableActionTypes();
            CollectUsableRequirementTypes();
        }

        void InspectOptionNode(DialogOption o)
        {
            _activeSubDialog = null;
            _activeOptionNode = o;
        }

        void InspectDialogNode(Dialog d)
        {
            _activeOptionNode = null;
            _activeSubDialog = d;
        }

        void CloseSubInspector()
        {
            _activeSubDialog = null;
            _activeOptionNode = null;
            _activeStringEditor = null;
        }

        void OnGUI()
        {
            if (SourceCollection == null)
            {
                return;
            }
            if (_activeStringEditor != null)
            {
                GUI.enabled = false;
            }
            else
            {
                GUI.enabled = _activeOptionNode == null && _activeSubDialog == null;
            }
            var left = new Rect(0, 0, LeftColumnWidth, position.height);
            var prev = GUI.color;
            GUI.color = _inspectorColor;
            GUI.BeginGroup(left, EditorStyles.textField);
            GUI.color = prev;
            DisplayDialogTools(left);
            GUI.EndGroup();
            DisplayDialogEditor(new Rect(LeftColumnWidth, 0, position.width - LeftColumnWidth, position.height));
            if (_activeOptionNode != null)
            {
                GUI.enabled = true & _activeStringEditor == null;
                DisplayOptionNodeInspector(new Rect(position.width - RightColumnoverlayWidth, 0, RightColumnoverlayWidth, position.height), _activeOptionNode);
            }
            else if (_activeSubDialog != null)
            {
                GUI.enabled = true & _activeStringEditor == null;
                DisplayDialogNodeInspector(new Rect(position.width - RightColumnoverlayWidth, 0, RightColumnoverlayWidth, position.height), _activeSubDialog);
            }
            if (_activeStringEditor != null)
            {
                GUI.enabled = true;
                if (_activeStringEditor.DrawGui()) return;
                _activeStringEditor.EndEdit();
                _activeStringEditor = null;
            }
        }

        void Update()
        {
            if (SourceCollection == null)
            {
                Cleanup();
                Close();
            }
        }

        void OnDestroy()
        {
            Cleanup();
        }

        public static void OpenEdit(DialogCollection collection)
        {
            var window = GetWindow<DialogEditor>("Dialog Editor");
            window.Close();
            window = GetWindow<DialogEditor>("Dialog Editor");
            window.minSize = new Vector2(600, 400);
            window.Load(collection);
        }

        public void Load(DialogCollection collection)
        {
            Initialize();
            SourceCollection = collection;
        }

        void DeleteCollectionDialog(Dialog d)
        {
            if (SourceCollection.Dialogs.Remove(d))
            {
                DeleteDialog(d);
            }
        }

        void DeleteDialog(Dialog d)
        {
            DestroyImmediate(d, true);
            DirtyAsset();
        }

        void DirtyAsset()
        {
            EditorUtility.SetDirty(SourceCollection);
        }

        void AddToAsset(ScriptableObject so)
        {
            so.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            AssetDatabase.AddObjectToAsset(so, SourceCollection);
            DirtyAsset();
        }

        void DeleteCleanupDialog(Dialog toDelete)
        {
            DeleteRecurseDialog(toDelete, _activeDialog, false);
            CleanupUnreferencedAssets();
        }

        void DeleteRecurseDialog(Dialog toDelete, Dialog currentItemToCheck, bool releaseIfNotLoop)
        {
            if (currentItemToCheck == null)
            {
                return;
            }
            for (var i = currentItemToCheck.Options.Count; i-- > 0;)
            {
                if (currentItemToCheck.Options[i].NextDialog == null)
                {
                    continue;
                }

                if (currentItemToCheck.Options[i].NextDialog == toDelete | releaseIfNotLoop)
                {
                    if (!currentItemToCheck.Options[i].IsRedirection)
                    {
                        DeleteRecurseDialog(toDelete, currentItemToCheck.Options[i].NextDialog, true);
                        //clean
                        for (var r = currentItemToCheck.Options[i].NextDialog.Requirements.Count; r-- > 0;)
                        {
                            DestroyImmediate(currentItemToCheck.Options[i].NextDialog.Requirements[r], true);
                        }
                        DeleteDialog(currentItemToCheck.Options[i].NextDialog);
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

        void CleanupUnreferencedAssets()
        {
            var path = AssetDatabase.GetAssetPath(SourceCollection);
            var objs = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var o in objs)
            {
                var d = o as Dialog;
                if (d != null)
                {
                    if (!IsReferencedInChains(d))
                    {
                        Debug.Log("Removing unreferenced dialog: " + d.ID);
                        DestroyImmediate(d, true);
                    }
                }
                else
                {
                    var tr = o as DialogOptionAction;
                    if (tr != null)
                    {
                        if (!IsReferencedInChains(tr))
                        {
                            Debug.Log("Removing unreferenced dialog action: " + tr.CachedName);
                            DestroyImmediate(tr, true);
                        }
                    }
                    else
                    {
                        var br = o as DialogRequirement;
                        if (br != null)
                        {
                            if (!IsReferencedInChains(br))
                            {
                                Debug.Log("Removing unreferenced dialog requirement: " + br.CachedName);
                                DestroyImmediate(br, true);
                            }
                        }
                    }
                }
            }
        }

        bool IsReferencedInChains(Dialog d)
        {
            foreach (var dl in SourceCollection.Dialogs)
            {
                var chain = new List<Dialog>();
                chain = GetAllDialogsInChain(chain, dl);
                foreach (var chainDialog in chain)
                {
                    if (chainDialog == d)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        bool IsReferencedInChains(DialogOptionAction tr)
        {
            foreach (var dl in SourceCollection.Dialogs)
            {
                var chain = new List<Dialog>();
                chain = GetAllDialogsInChain(chain, dl);
                foreach (var chainDialog in chain)
                {
                    foreach (var dop in chainDialog.Options)
                    {
                        if (dop.Actions.Contains(tr))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        bool IsReferencedInChains(DialogRequirement br)
        {
            foreach (var dl in SourceCollection.Dialogs)
            {
                var chain = new List<Dialog>();
                chain = GetAllDialogsInChain(chain, dl);
                foreach (var chainDialog in chain)
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
            GUILayout.BeginVertical(EditorStyles.textArea, GUILayout.Width(r.width - 8));
            if (GUILayout.Button("New Dialog"))
            {
                var d = CreateInstance<Dialog>();
                d.ID = GetUniqueId();
                AddToAsset(d);
                SourceCollection.Dialogs.Add(d);
                _activeDialog = d;
            }
            GUILayout.EndVertical();
            var itemRect = new Rect(0, 0, r.width - 38, 17);
            var deleteItemRect = new Rect(itemRect.width, 0, 18, 17);
            var scrollContainerRect = new Rect(2, 30, r.width - 5, r.height - 32);
            var contentScrollRect = new Rect(0, 0, r.width - 20, Mathf.Max(SourceCollection.Dialogs.Count*itemRect.height, scrollContainerRect.height));
            _dialogsScroll = GUI.BeginScrollView(scrollContainerRect, _dialogsScroll, contentScrollRect, false, true);
            for (var i = 0; i < SourceCollection.Dialogs.Count; i++)
            {
                itemRect.y = i*(itemRect.height + 2);
                deleteItemRect.y = itemRect.y;
                if (itemRect.y < _dialogsScroll.y - itemRect.height)
                {
                    continue;
                }
                if (itemRect.y > _dialogsScroll.y + scrollContainerRect.height)
                {
                    break;
                }
                if (SourceCollection.Dialogs[i] == _activeDialog)
                {
                    GUI.color = Color.gray;
                }
                var dTitle = SourceCollection.Dialogs[i].Title.Description;
                if (string.IsNullOrEmpty(dTitle))
                {
                    dTitle = TxtNotSetMsg;
                }
                if (GUI.Button(itemRect, dTitle, "ButtonLeft"))
                {
                    if (_activeDialog == SourceCollection.Dialogs[i])
                    {
                        _activeDialog = null;
                    }
                    else
                    {
                        _activeDialog = SourceCollection.Dialogs[i];
                    }
                }
                GUI.color = Color.white;
                if (GUI.Button(deleteItemRect, "x", "ButtonRight"))
                {
                    DeleteCleanupDialog(SourceCollection.Dialogs[i]);
                    if (SourceCollection.Dialogs[i] == _activeDialog)
                    {
                        CloseSubInspector();
                        _activeDialog = null;
                    }
                    DeleteCollectionDialog(SourceCollection.Dialogs[i]);
                }
            }
            GUI.EndScrollView();
        }

        void DisplayDialogEditor(Rect r)
        {
            if (_activeDialog == null)
            {
                return;
            }
            GUI.color = _nodeColor;
            GUILayout.BeginArea(new Rect(r.x, r.y, r.width, r.height));
            if (_countedHeight < r.height)
            {
                _countedHeight = r.height - 15;
            }
            if (_countedWidth < r.width)
            {
                _countedWidth = r.width - 15;
            }
            float inspectorInset = 0;
            if (_activeOptionNode != null || _activeSubDialog != null)
            {
                inspectorInset = RightColumnoverlayWidth;
                if (_countedWidth > r.width)
                {
                    _mainViewScrollPos = new Vector2(_mainViewScrollPos.x + inspectorInset, _mainViewScrollPos.y);
                }
            }
            _mainViewScrollPos = GUI.BeginScrollView(new Rect(0, 0, r.width - inspectorInset, r.height), _mainViewScrollPos, new Rect(0, 0, _countedWidth, _countedHeight));
            if (!Mathf.Approximately(_previousState, _countedWidth + _countedHeight))
            {
                Repaint();
                _previousState = _countedWidth + _countedHeight;
            }
            _countedHeight = 0;
            _countedWidth = 0;
            RecurseDialogs(new Rect(5, r.y + 5, NodeWidth, NodeHeight), 0, 0, _activeDialog);
            GUI.EndScrollView();
            GUILayout.EndArea();
            GUI.color = Color.white;
        }

        int RecurseDialogs(Rect parent, int length, int depth, Dialog d)
        {
            var r = new Rect(parent.x + depth*NodeWidth, parent.y + length*NodeHeight, NodeWidth, NodeHeight);
            DisplaySingleDialogNode(r, d);
            var branchDepth = 1;
            var lastOptionDepth = 0;
            for (var i = 0; i < d.Options.Count; i++)
            {
                lastOptionDepth = branchDepth;
                var move = DisplayDialogOption(new Rect(r.x + NodeWidth + IndentWidth, r.y + NodeHeight*lastOptionDepth, NodeWidth + IndentWidth, NodeHeight),
                    d.Options[i]);
                if (d.Options[i].NextDialog != null)
                {
                    if (d.Options[i].IsRedirection)
                    {
                        DisplayDialogLoop(new Rect(r.x + NodeWidth + IndentWidth*2, r.y + NodeHeight*lastOptionDepth, NodeWidth + IndentWidth, NodeHeight), d.Options[i]);
                        branchDepth += 1;
                    }
                    else
                    {
                        branchDepth += RecurseDialogs(new Rect(parent.x + IndentWidth*2, parent.y, NodeWidth, NodeHeight), length + branchDepth, depth + 1,
                            d.Options[i].NextDialog);
                    }
                }
                else
                {
                    branchDepth += 1;
                    if (d.Options[i].Actions.Count > 0)
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
                            var dI = d.Options[i];
                            if (dI.NextDialog != null && !dI.IsRedirection)
                            {
                                DeleteCleanupDialog(dI.NextDialog);
                            }
                            for (var t = dI.Actions.Count; t-- > 0;)
                            {
                                DestroyImmediate(dI.Actions[t], true);
                            }
                            d.Options.Remove(dI);
                            DestroyImmediate(dI, true);
                        }
                    }
                    else if (i + move >= 0 & i + move < d.Options.Count)
                    {
                        var dO = d.Options[i];
                        d.Options[i] = d.Options[i + move];
                        d.Options[i + move] = dO;
                    }
                }
            }
            if (DrawOptionRegion(new Rect(r.x + NodeWidth + IndentWidth, r.y + NodeHeight, NodeWidth + IndentWidth, NodeHeight), lastOptionDepth))
            {
                var dO = CreateInstance<DialogOption>();
                AddToAsset(dO);
                d.Options.Add(dO);
            }
            branchDepth++;
            return branchDepth;
        }

        int DisplayDialogOption(Rect r, DialogOption option)
        {
            var prev = GUI.color;
            GUI.color = _nodeColor;
            var gs = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 8,
                border = new RectOffset(2, 2, 2, 2)
            };
            var rect = new Rect(r.x - r.width + IndentWidth + 1, r.y + r.height*0.6f, r.width, r.height*0.4f);
            if (rect.y + rect.height > _countedHeight)
            {
                _countedHeight = rect.y + rect.height;
            }
            if (rect.x + rect.width > _countedWidth)
            {
                _countedWidth = rect.x + rect.width;
            }
            var ret = 0;
            var tTitle = option.Text.Description;
            if (string.IsNullOrEmpty(tTitle))
            {
                tTitle = TxtNotSetMsg;
            }
            if (GUI.Button(rect, tTitle, gs))
            {
                InspectOptionNode(option);
            }
            gs.alignment = TextAnchor.MiddleCenter;
            if (GUI.Button(new Rect(rect.x - 18, rect.y - 5, 17, 17), "˄", gs))
            {
                ret = -1;
            }
            if (GUI.Button(new Rect(rect.x - 34, rect.y - 5, 17, 17), "˅", gs))
            {
                ret = 1;
            }
            if (GUI.Button(new Rect(rect.x - 50, rect.y - 5, 17, 17), "x", gs))
            {
                ret = -10;
            }
            DrawInlineDialogOptionActions(new Rect(rect.x + IndentWidth, rect.y + rect.height, rect.width, rect.height + 10), option);
            if (option.NextDialog != null)
            {
                var prevEnabled = GUI.enabled;
                GUI.enabled = !option.IgnoreRequirements & prevEnabled;
                DrawInlineDialogRequirements(new Rect(rect.x + IndentWidth, rect.y - (rect.height + 2), rect.width, rect.height + 10), option);
                GUI.enabled = prevEnabled;
                if (GUI.Button(new Rect(rect.x + rect.width - 17, r.y + 5, 17, 13), "x", gs))
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
            var prev = GUI.color;
            var gs = new GUIStyle(GUI.skin.button)
            {
                fontSize = 8,
                alignment = TextAnchor.MiddleCenter,
                contentOffset = new Vector2(-1, 0),
                clipping = TextClipping.Overflow,
                border = new RectOffset(1, 1, 1, 1)
            };
            for (var i = d.NextDialog.Requirements.Count; i-- > 0;)
            {
                var req = d.NextDialog.Requirements[i];
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

        void DrawInlineDialogOptionActions(Rect r, DialogOption d)
        {
            GUILayout.BeginArea(r);
            GUILayout.BeginHorizontal();
            var prev = GUI.color;
            GUI.color = Color.white;
            var gs = new GUIStyle(GUI.skin.button)
            {
                fontSize = 8,
                alignment = TextAnchor.MiddleCenter,
                contentOffset = new Vector2(-1, 0),
                clipping = TextClipping.Overflow,
                border = new RectOffset(1, 1, 1, 1)
            };
            for (var i = d.Actions.Count; i-- > 0;)
            {
                var dot = d.Actions[i];
                GUI.color = dot.GetColor();
                GUILayout.Box(new GUIContent(dot.GetShortIdentifier(), dot.GetToolTip()), gs, GUILayout.Width(19), GUILayout.Height(15));
            }
            GUI.color = prev;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        bool DrawOptionRegion(Rect r, int depth)
        {
            var ret = false;
            var prev = GUI.color;
            GUI.color = _nodeColor;
            if (depth > 0)
            {
                GUI.Box(new Rect(r.x - r.width + IndentWidth - 2, r.y, 5, r.height*depth), "", EditorStyles.textArea);
            }
            if (GUI.Button(new Rect(r.x - r.width + IndentWidth + 2, r.y, 20, 19), "+", EditorStyles.miniButton))
            {
                ret = true;
            }
            if (r.height + 17 > _countedHeight)
            {
                _countedHeight = r.height + 17;
            }
            GUI.color = prev;
            return ret;
        }

        void DisplayOptionNodeInspector(Rect r, DialogOption dOption)
        {
            var prev = GUI.color;
            GUI.color = _inspectorColor;
            GUILayout.BeginArea(r, EditorStyles.textArea);
            GUI.color = prev;
            GUILayout.Space(5);
            GUILayout.Label("Text", _headerStyle);
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            var tText = dOption.Text.Description;
            if (string.IsNullOrEmpty(tText))
            {
                tText = TxtNotSetMsg;
            }
            GUILayout.Label(tText, _lblStyle, GUILayout.MaxWidth(154));
            if (GUILayout.Button("Edit", _buttonStyle, GUILayout.Width(40)))
            {
                _activeStringEditor = new LocalizedStringEditor(dOption.Text, "Option text");
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.Label(new GUIContent("Tag", "User data"), _headerStyle);
            GUILayout.Space(5);
            dOption.Tag = GUILayout.TextField(dOption.Tag);
            GUILayout.Space(10);
            GUILayout.Label("Connection", _headerStyle);
            GUILayout.Space(5);
            if (GUILayout.Button("New Sub-Dialog", _buttonStyle))
            {
                var d = CreateInstance<Dialog>();
                d.ID = GetUniqueId();
                AddToAsset(d);
                dOption.NextDialog = d;
                dOption.IsRedirection = false;
                CloseSubInspector();
            }
            GUILayout.Label(new GUIContent("Or link to existing: ", "Use with care, can result in infinite loops!"));
            _scrollbar = GUILayout.BeginScrollView(_scrollbar);
            var ddialogs = new List<Dialog>();
            ddialogs = GetAllDialogsInChain(ddialogs, _activeDialog);
            for (var i = 0; i < ddialogs.Count; i++)
            {
                if (ddialogs[i].Options.Contains(dOption))
                {
                    continue;
                }
                var dTitle = ddialogs[i].Title.Description;
                if (string.IsNullOrEmpty(dTitle))
                {
                    dTitle = TxtNotSetMsg;
                }
                if (GUILayout.Button(dTitle, _buttonStyle))
                {
                    dOption.NextDialog = ddialogs[i];
                    dOption.IsRedirection = true;
                    CloseSubInspector();
                    break;
                }
            }
            GUILayout.EndScrollView();
            dOption.IgnoreRequirements = GUILayout.Toggle(dOption.IgnoreRequirements, "Ignore Requirements");
            GUILayout.Label("Actions", _headerStyle);
            GUILayout.BeginHorizontal();
            var prevEnabled = GUI.enabled;
            if (dOption.Actions.Count >= 6)
            {
                GUI.enabled = false;
            }
            var index = EditorGUILayout.Popup(0, _usableActionNames.ToArray());
            if (index > 0)
            {
                var tr = CreateInstance(_usableActionTypes[index]) as DialogOptionAction;
                AddToAsset(tr);
                dOption.Actions.Add(tr);
            }
            GUI.enabled = prevEnabled;
            if (GUILayout.Button("Remove all", _buttonStyle))
            {
                for (var i = dOption.Actions.Count; i-- > 0;)
                {
                    DestroyImmediate(dOption.Actions[i], true);
                    dOption.Actions.RemoveAt(i);
                }
            }
            GUILayout.EndHorizontal();
            _scrollbar2 = GUILayout.BeginScrollView(_scrollbar2);
            for (var i = dOption.Actions.Count; i-- > 0;)
            {
                if (dOption.Actions[i] == null)
                {
                    dOption.Actions.RemoveAt(i);
                    continue;
                }
                if (!InlineDisplayOptionActionEditor(dOption.Actions[i]))
                {
                    DestroyImmediate(dOption.Actions[i], true);
                    dOption.Actions.RemoveAt(i);
                }
            }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close", _buttonStyle))
            {
                //RemoveDuplicateNotifications(dOption.Actions);
                CloseSubInspector();
            }
            GUILayout.EndArea();
        }

        bool InlineDisplayOptionActionEditor(DialogOptionAction tr)
        {
            var ret = true;
            GUILayout.BeginVertical(EditorStyles.textArea);
            GUILayout.BeginHorizontal();
            GUILayout.Label(tr.CachedName, EditorStyles.helpBox);
            if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(16)))
            {
                ret = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            if (!tr.DrawComplexGui())
            {
                var so = new SerializedObject(tr);
                var sp = so.GetIterator();
                sp.NextVisible(true);
                while (sp.NextVisible(true))
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

        void RemoveDuplicateActions(List<DialogOptionAction> sourceList)
        {
            var cleanList = new List<DialogOptionAction>();
            for (var i = 0; i < sourceList.Count; i++)
            {
                var found = false;
                for (var cl = 0; cl < cleanList.Count; cl++)
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
            var prev = GUI.color;
            GUI.color = _inspectorColor;
            GUILayout.BeginArea(r, EditorStyles.textArea);
            GUI.color = prev;
            GUILayout.Space(5);
            GUILayout.Label("Title", _headerStyle);
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            var dTitle = d.Title.Description;
            if (string.IsNullOrEmpty(dTitle))
            {
                dTitle = TxtNotSetMsg;
            }
            GUILayout.Label(dTitle, _lblStyle, GUILayout.MaxWidth(154));
            if (GUILayout.Button("Edit", _buttonStyle, GUILayout.Width(40)))
            {
                _activeStringEditor = new LocalizedStringEditor(d.Title, "Dialog title");
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.Label("Text" + (d.Texts.Count > 1 ? " (random)" : ""), _headerStyle);
            GUILayout.Space(5);
            for (var i = 0; i < d.Texts.Count; i++)
            {
                var dtextsT = d.Texts[i].Description;
                if (string.IsNullOrEmpty(dtextsT))
                {
                    dtextsT = TxtNotSetMsg;
                }
                GUILayout.BeginHorizontal();
                GUILayout.Label(dtextsT, _lblStyle, GUILayout.MaxWidth(134));
                if (GUILayout.Button("Edit", _buttonStyle, GUILayout.Width(40)))
                {
                    _activeStringEditor = new LocalizedStringEditor(d.Texts[i], "Dialog text");
                }
                if (GUILayout.Button("x", _buttonStyle, GUILayout.Width(20)))
                {
                    d.Texts.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            if (d.Texts.Count < 3)
            {
                if (GUILayout.Button("Add Variation", _buttonStyle))
                {
                    d.Texts.Add(new LocalizedString(""));
                }
                GUILayout.Space(5);
            }
            GUILayout.Label(new GUIContent("Tag", "User data"), _headerStyle);
            GUILayout.Space(5);
            d.Tag = GUILayout.TextField(d.Tag);
            GUILayout.Space(10);
            GUILayout.Label("Requirements", _headerStyle);
            if (d.Requirements.Count > 1)
            {
                d.RequirementMode = (DialogRequirementMode) EditorGUILayout.EnumPopup("Mode:", d.RequirementMode);
            }
            GUILayout.BeginHorizontal();
            var prevEnabled = GUI.enabled;
            if (d.Requirements.Count >= 6)
            {
                GUI.enabled = false;
            }
            var index = EditorGUILayout.Popup(0, _usableRequirementNames.ToArray());
            if (index > 0)
            {
                var bs = CreateInstance(_usableRequirementTypes[index]) as DialogRequirement;
                AddToAsset(bs);
                d.Requirements.Add(bs);
            }
            GUI.enabled = prevEnabled;
            if (GUILayout.Button("Remove all", _buttonStyle))
            {
                for (var i = d.Requirements.Count; i-- > 0;)
                {
                    DestroyImmediate(d.Requirements[i], true);
                    d.Requirements.RemoveAt(i);
                }
            }
            GUILayout.EndHorizontal();
            _scrollbar = GUILayout.BeginScrollView(_scrollbar, false, true);
            for (var i = d.Requirements.Count; i-- > 0;)
            {
                if (d.Requirements[i] == null)
                {
                    d.Requirements.RemoveAt(i);
                    continue;
                }
                if (!DrawInlineRequirement(d.Requirements[i]))
                {
                    DestroyImmediate(d.Requirements[i], true);
                    d.Requirements.RemoveAt(i);
                }
            }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close", _buttonStyle))
            {
                //RemoveDuplicateRequirements(d.Requirements);
                CloseSubInspector();
            }
            GUILayout.EndArea();
        }

        void RemoveDuplicateRequirements(List<DialogRequirement> sourceList)
        {
            var cleanList = new List<DialogRequirement>();
            for (var i = 0; i < sourceList.Count; i++)
            {
                var found = false;
                for (var cl = 0; cl < cleanList.Count; cl++)
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

        bool DrawInlineRequirement(DialogRequirement dr)
        {
            var ret = true;
            GUILayout.BeginVertical(EditorStyles.textArea);
            GUILayout.BeginHorizontal();
            GUILayout.Label(dr.CachedName, EditorStyles.helpBox);
            if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(16)))
            {
                ret = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            if (!dr.DrawComplexGui())
            {
                var so = new SerializedObject(dr);
                var sp = so.GetIterator();
                sp.NextVisible(true);
                while (sp.NextVisible(true))
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
            var prev = GUI.color;
            GUI.color = Color.yellow;
            var ndTitle = dOption.NextDialog.Title.Description;
            if (string.IsNullOrEmpty(ndTitle))
            {
                ndTitle = TxtNotSetMsg;
            }
            ndTitle = string.Format("({0}) {1}", dOption.NextDialog.ID, ndTitle);
            if (GUI.Button(new Rect(r.x, r.y + 5, r.width, r.height - 5), "Loop: " + ndTitle))
            {
                InspectOptionNode(dOption);
            }
            GUI.color = prev;
            if (r.x + r.width > _countedWidth)
            {
                _countedWidth = r.x + r.width;
            }
            if (r.y + r.height > _countedHeight)
            {
                _countedHeight = r.y + r.height;
            }
        }

        void DisplaySingleDialogNode(Rect r, Dialog d)
        {
            var rect = new Rect(r.x, r.y + 5, r.width - 20, r.height - 5);
            if (rect.y + rect.height > _countedHeight)
            {
                _countedHeight = rect.y + rect.height;
            }
            if (rect.x + rect.width > _countedWidth)
            {
                _countedWidth = rect.x + rect.width;
            }
            var dTitle = d.Title.Description;
            if (string.IsNullOrEmpty(dTitle))
            {
                dTitle = TxtNotSetMsg;
            }
            dTitle = string.Format("({0}) {1}", d.ID, dTitle);
            if (GUI.Button(rect, dTitle))
            {
                InspectDialogNode(d);
            }
        }

        static List<Dialog> GetAllDialogsInChain(List<Dialog> dl, Dialog d)
        {
            if (!dl.Contains(d))
            {
                dl.Add(d);
            }
            for (var i = 0; i < d.Options.Count; i++)
            {
                if (d.Options[i].NextDialog == null) continue;
                if (!d.Options[i].IsRedirection)
                {
                    GetAllDialogsInChain(dl, d.Options[i].NextDialog);
                }
            }
            return dl;
        }
    }
}