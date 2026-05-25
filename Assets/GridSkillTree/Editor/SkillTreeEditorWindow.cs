using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GridSkillTree.Editor
{
    public class SkillTreeEditorWindow : EditorWindow
    {
        private SkillTreeData treeData;

        private Vector2 gridScroll;
        private Vector2 inspectorScroll;

        private SkillNodeData selectedNode;
        private Vector2Int selectedCell;

        private bool isConnecting;
        private SkillNodeData connectionStartNode;

        private const float CellSize = 80f;
        private const float NodeSize = 56f;
        private const int GridHalfSize = 12;
        private const float InspectorWidth = 340f;
        private const float InspectorInnerWidth = 305f;
        private const float DescriptionMinHeight = 80f;
        private const float DescriptionMaxHeight = 180f;

        private readonly Dictionary<Vector2Int, SkillNodeData> nodesByPosition = new();

        [MenuItem("Window/Grid Skill Tree/Editor")]
        public static void Open()
        {
            SkillTreeEditorWindow window = GetWindow<SkillTreeEditorWindow>();
            window.titleContent = new GUIContent("Skill Tree Editor");
            window.minSize = new Vector2(900f, 600f);
            window.Show();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (treeData == null)
            {
                DrawEmptyState();
                return;
            }

            RebuildLookup();

            EditorGUILayout.BeginHorizontal();

            DrawGridPanel();
            DrawInspectorPanel();

            EditorGUILayout.EndHorizontal();

            HandleKeyboard();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUI.BeginChangeCheck();

            treeData = (SkillTreeData)EditorGUILayout.ObjectField(
                treeData,
                typeof(SkillTreeData),
                false,
                GUILayout.Width(320f)
            );

            if (EditorGUI.EndChangeCheck())
            {
                selectedNode = null;
                isConnecting = false;
                connectionStartNode = null;
            }

            GUILayout.Space(8f);

            if (GUILayout.Button("Create New Tree", EditorStyles.toolbarButton, GUILayout.Width(120f)))
            {
                CreateNewTreeAsset();
            }

            using (new EditorGUI.DisabledScope(treeData == null))
            {
                if (GUILayout.Button("Validate", EditorStyles.toolbarButton, GUILayout.Width(80f)))
                {
                    ValidateTree();
                }

                if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(60f)))
                {
                    SaveTree();
                }
            }

            GUILayout.FlexibleSpace();

            if (isConnecting && connectionStartNode != null)
            {
                GUILayout.Label($"Connecting from: {connectionStartNode.id}", EditorStyles.boldLabel);

                if (GUILayout.Button("Cancel Connect", EditorStyles.toolbarButton, GUILayout.Width(110f)))
                {
                    CancelConnection();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawEmptyState()
        {
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            GUILayout.Label("Select or create a SkillTreeData asset.", EditorStyles.centeredGreyMiniLabel);

            GUILayout.Space(8f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Create Skill Tree", GUILayout.Width(180f), GUILayout.Height(32f)))
            {
                CreateNewTreeAsset();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
        }

        private void DrawGridPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            Rect toolbarRect = EditorGUILayout.GetControlRect(false, 24f);
            GUI.Box(toolbarRect, GUIContent.none, EditorStyles.toolbar);

            GUI.Label(new Rect(toolbarRect.x + 8f, toolbarRect.y + 4f, 500f, 20f),
                "Grid: double click empty cell to create node. Right click node for actions.");

            Rect gridRect = GUILayoutUtility.GetRect(
                10f,
                100000f,
                10f,
                100000f,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true)
            );

            GUI.Box(gridRect, GUIContent.none);

            Rect contentRect = new Rect(
                0f,
                0f,
                GridHalfSize * 2 * CellSize + CellSize,
                GridHalfSize * 2 * CellSize + CellSize
            );

            gridScroll = GUI.BeginScrollView(gridRect, gridScroll, contentRect);

            DrawGridBackground(contentRect);
            DrawConnections();
            DrawNodes();
            HandleGridInput(contentRect);

            GUI.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void DrawGridBackground(Rect contentRect)
        {
            Handles.BeginGUI();

            Color oldColor = Handles.color;

            for (int x = -GridHalfSize; x <= GridHalfSize; x++)
            {
                Vector2 from = GridToEditorPosition(new Vector2Int(x, -GridHalfSize));
                Vector2 to = GridToEditorPosition(new Vector2Int(x, GridHalfSize));

                Handles.color = x == 0 ? new Color(1f, 1f, 1f, 0.35f) : new Color(1f, 1f, 1f, 0.08f);
                Handles.DrawLine(from, to + new Vector2(0f, CellSize));
            }

            for (int y = -GridHalfSize; y <= GridHalfSize; y++)
            {
                Vector2 from = GridToEditorPosition(new Vector2Int(-GridHalfSize, y));
                Vector2 to = GridToEditorPosition(new Vector2Int(GridHalfSize, y));

                Handles.color = y == 0 ? new Color(1f, 1f, 1f, 0.35f) : new Color(1f, 1f, 1f, 0.08f);
                Handles.DrawLine(from, to + new Vector2(CellSize, 0f));
            }

            Handles.color = oldColor;
            Handles.EndGUI();
        }

        private void DrawConnections()
        {
            Handles.BeginGUI();

            Color oldColor = Handles.color;
            Handles.color = new Color(0.7f, 0.9f, 1f, 0.85f);

            foreach (SkillNodeData node in treeData.nodes)
            {
                if (node.previousNodeIds == null)
                    continue;

                Vector2 currentCenter = GridToEditorPosition(node.gridPosition) + Vector2.one * CellSize * 0.5f;

                foreach (string previousId in node.previousNodeIds)
                {
                    SkillNodeData previousNode = treeData.GetNodeById(previousId);

                    if (previousNode == null)
                        continue;

                    Vector2 previousCenter = GridToEditorPosition(previousNode.gridPosition) + Vector2.one * CellSize * 0.5f;

                    Handles.DrawAAPolyLine(4f, previousCenter, currentCenter);
                }
            }

            Handles.color = oldColor;
            Handles.EndGUI();
        }

        private void DrawNodes()
        {
            foreach (SkillNodeData node in treeData.nodes)
            {
                Rect nodeRect = GetNodeRect(node);

                bool isSelected = selectedNode == node;
                bool isConnectionStart = isConnecting && connectionStartNode == node;

                Color oldColor = GUI.color;

                if (isConnectionStart)
                    GUI.color = new Color(0.4f, 1f, 1f, 1f);
                else if (isSelected)
                    GUI.color = new Color(1f, 0.9f, 0.35f, 1f);
                else
                    GUI.color = Color.white;

                GUI.Box(nodeRect, GUIContent.none, EditorStyles.helpBox);

                GUI.color = oldColor;

                Rect iconRect = new Rect(nodeRect.x + 12f, nodeRect.y + 6f, 32f, 32f);

                if (node.icon != null)
                {
                    GUI.DrawTexture(iconRect, node.icon.texture, ScaleMode.ScaleToFit, true);
                }
                else
                {
                    GUI.Box(iconRect, "?", EditorStyles.centeredGreyMiniLabel);
                }

                Rect titleRect = new Rect(nodeRect.x + 4f, nodeRect.y + 40f, nodeRect.width - 8f, 14f);
                GUI.Label(titleRect, string.IsNullOrWhiteSpace(node.title) ? node.id : node.title, EditorStyles.centeredGreyMiniLabel);

                Rect levelRect = new Rect(nodeRect.x + 4f, nodeRect.y + 2f, nodeRect.width - 8f, 14f);
                GUI.Label(levelRect, $"Lv {node.maxLevel}", EditorStyles.miniLabel);
            }
        }

        private void HandleGridInput(Rect contentRect)
        {
            Event e = Event.current;

            if (e.type != EventType.MouseDown)
                return;

            if (e.button != 0 && e.button != 1)
                return;

            Vector2 mousePosition = e.mousePosition;

            Vector2Int cell = EditorPositionToGrid(mousePosition);
            selectedCell = cell;

            SkillNodeData clickedNode = GetNodeAtCell(cell);

            if (e.button == 0)
            {
                if (clickedNode != null)
                {
                    if (isConnecting)
                    {
                        FinishConnection(clickedNode);
                    }
                    else
                    {
                        selectedNode = clickedNode;
                    }

                    e.Use();
                    Repaint();
                    return;
                }

                selectedNode = null;

                if (e.clickCount == 2)
                {
                    CreateNodeAt(cell);
                    e.Use();
                    Repaint();
                    return;
                }

                e.Use();
                Repaint();
            }

            if (e.button == 1)
            {
                if (clickedNode != null)
                {
                    selectedNode = clickedNode;
                    ShowNodeContextMenu(clickedNode);
                }
                else
                {
                    ShowCellContextMenu(cell);
                }

                e.Use();
                Repaint();
            }
        }

        private void DrawInspectorPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(InspectorWidth), GUILayout.ExpandHeight(true));

            GUILayout.Label("Inspector", EditorStyles.boldLabel);

            inspectorScroll = EditorGUILayout.BeginScrollView(inspectorScroll);

            if (selectedNode == null)
            {
                DrawEmptyCellInspector();
            }
            else
            {
                DrawNodeInspector(selectedNode);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void DrawEmptyCellInspector()
        {
            EditorGUILayout.HelpBox($"Selected cell: {selectedCell}", MessageType.Info);

            if (GUILayout.Button("Create Node Here", GUILayout.Height(28f)))
            {
                CreateNodeAt(selectedCell);
            }

            GUILayout.Space(12f);

            DrawTreeInfo();
        }

        private void DrawTreeInfo()
        {
            if (treeData == null)
                return;

            GUILayout.Label("Tree Info", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            string newTreeId = EditorGUILayout.TextField("Tree Id", treeData.treeId);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(treeData, "Change Tree Id");
                treeData.treeId = newTreeId;
                MarkDirty();
            }

            EditorGUILayout.LabelField("Nodes", treeData.nodes.Count.ToString());
        }

        private string DrawWrappedDescriptionField(string value)
        {
            value ??= string.Empty;

            GUIStyle wrappedTextAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true,
                stretchWidth = false
            };

            GUIContent content = new GUIContent(value);

            float height = wrappedTextAreaStyle.CalcHeight(content, InspectorInnerWidth);
            height = Mathf.Clamp(height + 8f, DescriptionMinHeight, DescriptionMaxHeight);

            return EditorGUILayout.TextArea(
                value,
                wrappedTextAreaStyle,
                GUILayout.Width(InspectorInnerWidth),
                GUILayout.Height(height)
            );
        }

        private void DrawNodeInspector(SkillNodeData node)
        {
            EditorGUILayout.HelpBox($"Selected node: {node.id}", MessageType.Info);

            EditorGUI.BeginChangeCheck();

            string id = EditorGUILayout.TextField("Id", node.id);
            string title = EditorGUILayout.TextField("Title", node.title);

            GUILayout.Label("Description");
            string description = DrawWrappedDescriptionField(node.description);

            Vector2Int gridPosition = EditorGUILayout.Vector2IntField("Grid Position", node.gridPosition);

            Sprite icon = (Sprite)EditorGUILayout.ObjectField("Icon", node.icon, typeof(Sprite), false);

            GUILayout.Space(8f);
            GUILayout.Label("Levels", EditorStyles.boldLabel);

            int maxLevel = EditorGUILayout.IntField("Max Level", node.maxLevel);

            GUILayout.Space(8f);
            GUILayout.Label("Cost (Currencies)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Add one or more currency costs. Empty list means the node is free.", MessageType.None);

            if (node.costs == null)
                node.costs = new List<SkillCost>();

            for (int i = 0; i < node.costs.Count; i++)
            {
                SkillCost c = node.costs[i];
                if (c == null) { node.costs.RemoveAt(i); i--; continue; }

                EditorGUILayout.BeginHorizontal();
                c.currency = (CurrencyType)EditorGUILayout.EnumPopup(c.currency, GUILayout.Width(100f));
                c.baseAmount = Mathf.Max(0, EditorGUILayout.IntField(c.baseAmount, GUILayout.Width(60f)));
                c.formula = (CostFormulaType)EditorGUILayout.EnumPopup(c.formula);
                if (GUILayout.Button("X", GUILayout.Width(24f)))
                {
                    Undo.RecordObject(treeData, "Remove Skill Cost");
                    node.costs.RemoveAt(i);
                    MarkDirty();
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add Currency Cost"))
            {
                Undo.RecordObject(treeData, "Add Skill Cost");
                node.costs.Add(new SkillCost());
                MarkDirty();
            }

            GUILayout.Space(8f);
            GUILayout.Label("Effect", EditorStyles.boldLabel);

            SkillEffectType effectType = (SkillEffectType)EditorGUILayout.EnumPopup("Effect Type", node.effectType);
            float baseValue = EditorGUILayout.FloatField("Base Value", node.baseValue);
            GrowthFormulaType growthFormula = (GrowthFormulaType)EditorGUILayout.EnumPopup("Growth Formula", node.growthFormula);

            CurrencyType unlockCurrencyType = node.unlockCurrencyType;
            if (effectType == SkillEffectType.UnlockCurrency)
                unlockCurrencyType = (CurrencyType)EditorGUILayout.EnumPopup("Unlock Currency", node.unlockCurrencyType);

            CurrencyType currencyDropType = node.currencyDropType;
            if (effectType == SkillEffectType.CurrencyDropPercent)
            {
                currencyDropType = (CurrencyType)EditorGUILayout.EnumPopup("Drop Currency", node.currencyDropType);
                EditorGUILayout.HelpBox("Base Value/Growth are treated as percent: 25 = +25% currency from every enemy drop of this currency.", MessageType.None);
            }

            CurrencyType passiveCurrencyType = node.passiveCurrencyType;
            float passiveCurrencyIntervalSeconds = node.passiveCurrencyIntervalSeconds;
            if (effectType == SkillEffectType.PassiveCurrency)
            {
                passiveCurrencyType = (CurrencyType)EditorGUILayout.EnumPopup("Passive Currency", node.passiveCurrencyType);
                passiveCurrencyIntervalSeconds = EditorGUILayout.FloatField("Interval Seconds", node.passiveCurrencyIntervalSeconds);
                EditorGUILayout.HelpBox("Base Value/Growth are treated as amount: 1 = +1 selected currency every interval while player is alive.", MessageType.None);
            }

            if (effectType == SkillEffectType.InvincibilityDuration)
            {
                EditorGUILayout.HelpBox("Base Value/Growth are treated as seconds added to damage invincibility duration.", MessageType.None);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(treeData, "Edit Skill Node");

                node.id = id;
                node.title = title;
                node.description = description;
                node.gridPosition = gridPosition;
                node.icon = icon;
                node.maxLevel = Mathf.Max(1, maxLevel);
                node.effectType = effectType;
                node.baseValue = baseValue;
                node.growthFormula = growthFormula;
                node.unlockCurrencyType = unlockCurrencyType;
                node.currencyDropType = currencyDropType;
                node.passiveCurrencyType = passiveCurrencyType;
                node.passiveCurrencyIntervalSeconds = Mathf.Max(0.1f, passiveCurrencyIntervalSeconds);

                MarkDirty();
                RebuildLookup();
            }

            GUILayout.Space(12f);
            DrawConnectionsInspector(node);

            GUILayout.Space(12f);
            DrawNodeActions(node);
        }

        private void DrawConnectionsInspector(SkillNodeData node)
        {
            GUILayout.Label("Previous Nodes", EditorStyles.boldLabel);

            if (node.previousNodeIds == null)
                node.previousNodeIds = new List<string>();

            for (int i = 0; i < node.previousNodeIds.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                SkillNodeData previousNode = treeData.GetNodeById(node.previousNodeIds[i]);

                string label = previousNode == null
                    ? $"Missing: {node.previousNodeIds[i]}"
                    : $"{previousNode.id}";

                EditorGUILayout.LabelField(label);

                if (GUILayout.Button("Select", GUILayout.Width(58f)))
                {
                    if (previousNode != null)
                        selectedNode = previousNode;
                }

                if (GUILayout.Button("X", GUILayout.Width(24f)))
                {
                    Undo.RecordObject(treeData, "Remove Connection");
                    node.previousNodeIds.RemoveAt(i);
                    MarkDirty();
                    Repaint();
                    return;
                }

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(4f);

            if (GUILayout.Button("Start Connect From This Node"))
            {
                StartConnection(node);
            }

            if (isConnecting && connectionStartNode != null && connectionStartNode != node)
            {
                if (GUILayout.Button($"Connect {connectionStartNode.id} → {node.id}"))
                {
                    FinishConnection(node);
                }
            }
        }

        private void DrawNodeActions(SkillNodeData node)
        {
            GUILayout.Label("Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Duplicate"))
            {
                DuplicateNode(node);
            }

            if (GUILayout.Button("Delete"))
            {
                DeleteNode(node);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ShowNodeContextMenu(SkillNodeData node)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Start Connection From This"), false, () => StartConnection(node));

            if (isConnecting && connectionStartNode != null && connectionStartNode != node)
            {
                menu.AddItem(new GUIContent($"Connect {connectionStartNode.id} → {node.id}"), false, () => FinishConnection(node));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Connect Here"));
            }

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Duplicate"), false, () => DuplicateNode(node));
            menu.AddItem(new GUIContent("Delete"), false, () => DeleteNode(node));

            menu.ShowAsContext();
        }

        private void ShowCellContextMenu(Vector2Int cell)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent($"Create Node at {cell}"), false, () => CreateNodeAt(cell));

            if (isConnecting)
                menu.AddItem(new GUIContent("Cancel Connection"), false, CancelConnection);

            menu.ShowAsContext();
        }

        private void CreateNodeAt(Vector2Int cell)
        {
            if (treeData == null)
                return;

            if (GetNodeAtCell(cell) != null)
            {
                Debug.LogWarning($"Cell {cell} is already occupied.", treeData);
                return;
            }

            Undo.RecordObject(treeData, "Create Skill Node");

            SkillNodeData node = new SkillNodeData
            {
                id = GenerateUniqueNodeId(),
                title = "New Skill",
                description = "Skill description.",
                gridPosition = cell,
                maxLevel = 1,
                costs = new List<SkillCost> { new SkillCost() },
                effectType = SkillEffectType.None,
                baseValue = 1f,
                growthFormula = GrowthFormulaType.Constant,
                passiveCurrencyType = CurrencyType.Basic,
                passiveCurrencyIntervalSeconds = 3f,
                previousNodeIds = new List<string>()
            };

            treeData.nodes.Add(node);
            selectedNode = node;

            MarkDirty();
            RebuildLookup();
        }

        private void DuplicateNode(SkillNodeData source)
        {
            if (source == null)
                return;

            Undo.RecordObject(treeData, "Duplicate Skill Node");

            Vector2Int newPosition = FindFreeNearbyCell(source.gridPosition);

            SkillNodeData copy = new SkillNodeData
            {
                id = GenerateUniqueNodeId(source.id),
                title = source.title + " Copy",
                description = source.description,
                gridPosition = newPosition,
                icon = source.icon,
                previousNodeIds = new List<string>(source.previousNodeIds),
                maxLevel = source.maxLevel,
                costs = CloneCosts(source.costs),
                effectType = source.effectType,
                baseValue = source.baseValue,
                growthFormula = source.growthFormula,
                unlockCurrencyType = source.unlockCurrencyType,
                currencyDropType = source.currencyDropType,
                passiveCurrencyType = source.passiveCurrencyType,
                passiveCurrencyIntervalSeconds = source.passiveCurrencyIntervalSeconds
            };

            treeData.nodes.Add(copy);
            selectedNode = copy;

            MarkDirty();
            RebuildLookup();
            Repaint();
        }

        private static List<SkillCost> CloneCosts(List<SkillCost> src)
        {
            List<SkillCost> result = new();
            if (src == null) return result;
            foreach (SkillCost c in src)
            {
                if (c == null) continue;
                result.Add(new SkillCost
                {
                    currency = c.currency,
                    baseAmount = c.baseAmount,
                    formula = c.formula
                });
            }
            return result;
        }

        private void DeleteNode(SkillNodeData node)
        {
            if (node == null)
                return;

            bool confirm = EditorUtility.DisplayDialog(
                "Delete Skill Node",
                $"Delete node '{node.id}'?\nAll connections to this node will also be removed.",
                "Delete",
                "Cancel"
            );

            if (!confirm)
                return;

            Undo.RecordObject(treeData, "Delete Skill Node");

            treeData.nodes.Remove(node);

            foreach (SkillNodeData otherNode in treeData.nodes)
            {
                otherNode.previousNodeIds?.Remove(node.id);
            }

            if (selectedNode == node)
                selectedNode = null;

            if (connectionStartNode == node)
                CancelConnection();

            MarkDirty();
            RebuildLookup();
            Repaint();
        }

        private void StartConnection(SkillNodeData fromNode)
        {
            if (fromNode == null)
                return;

            isConnecting = true;
            connectionStartNode = fromNode;
            selectedNode = fromNode;
            Repaint();
        }

        private void FinishConnection(SkillNodeData toNode)
        {
            if (!isConnecting || connectionStartNode == null || toNode == null)
                return;

            if (connectionStartNode == toNode)
            {
                Debug.LogWarning("Cannot connect node to itself.", treeData);
                return;
            }

            if (toNode.previousNodeIds == null)
                toNode.previousNodeIds = new List<string>();

            if (toNode.previousNodeIds.Contains(connectionStartNode.id))
            {
                Debug.LogWarning($"Connection already exists: {connectionStartNode.id} → {toNode.id}", treeData);
                CancelConnection();
                return;
            }

            Undo.RecordObject(treeData, "Create Skill Connection");

            toNode.previousNodeIds.Add(connectionStartNode.id);

            selectedNode = toNode;
            CancelConnection();

            MarkDirty();
            Repaint();
        }

        private void CancelConnection()
        {
            isConnecting = false;
            connectionStartNode = null;
            Repaint();
        }

        private void HandleKeyboard()
        {
            Event e = Event.current;

            if (e.type != EventType.KeyDown)
                return;

            if (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace)
            {
                if (selectedNode != null)
                {
                    DeleteNode(selectedNode);
                    e.Use();
                }
            }

            if (e.keyCode == KeyCode.Escape)
            {
                if (isConnecting)
                {
                    CancelConnection();
                    e.Use();
                }
            }
        }

        private Rect GetNodeRect(SkillNodeData node)
        {
            Vector2 cellPosition = GridToEditorPosition(node.gridPosition);
            Vector2 nodePosition = cellPosition + Vector2.one * ((CellSize - NodeSize) * 0.5f);

            return new Rect(nodePosition.x, nodePosition.y, NodeSize, NodeSize);
        }

        private Vector2 GridToEditorPosition(Vector2Int gridPosition)
        {
            float origin = GridHalfSize * CellSize;

            float x = origin + gridPosition.x * CellSize;
            float y = origin - gridPosition.y * CellSize;

            return new Vector2(x, y);
        }

        private Vector2Int EditorPositionToGrid(Vector2 editorPosition)
        {
            float origin = GridHalfSize * CellSize;

            int x = Mathf.FloorToInt((editorPosition.x - origin) / CellSize);
            int y = -Mathf.FloorToInt((editorPosition.y - origin) / CellSize);

            return new Vector2Int(x, y);
        }

        private SkillNodeData GetNodeAtCell(Vector2Int cell)
        {
            nodesByPosition.TryGetValue(cell, out SkillNodeData node);
            return node;
        }

        private void RebuildLookup()
        {
            nodesByPosition.Clear();

            if (treeData == null)
                return;

            foreach (SkillNodeData node in treeData.nodes)
            {
                if (node == null)
                    continue;

                nodesByPosition[node.gridPosition] = node;
            }
        }

        private string GenerateUniqueNodeId(string baseId = "skill")
        {
            if (string.IsNullOrWhiteSpace(baseId))
                baseId = "skill";

            string cleanBaseId = baseId
                .ToLowerInvariant()
                .Replace(" ", "_")
                .Replace("-", "_");

            string id = cleanBaseId;
            int index = 1;

            while (treeData.ContainsNode(id))
            {
                id = $"{cleanBaseId}_{index}";
                index++;
            }

            return id;
        }

        private Vector2Int FindFreeNearbyCell(Vector2Int start)
        {
            Vector2Int[] directions =
            {
                Vector2Int.right,
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.down,
                new Vector2Int(1, 1),
                new Vector2Int(-1, 1),
                new Vector2Int(1, -1),
                new Vector2Int(-1, -1)
            };

            for (int radius = 1; radius < 20; radius++)
            {
                foreach (Vector2Int direction in directions)
                {
                    Vector2Int candidate = start + direction * radius;

                    if (GetNodeAtCell(candidate) == null)
                        return candidate;
                }
            }

            return start + Vector2Int.right;
        }

        private void ValidateTree()
        {
            SkillTreeValidationResult result = SkillTreeValidator.Validate(treeData);

            if (result.IsValid)
                Debug.Log(result.GetReport(), treeData);
            else
                Debug.LogError(result.GetReport(), treeData);
        }

        private void SaveTree()
        {
            MarkDirty();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Skill tree saved.", treeData);
        }

        private void MarkDirty()
        {
            if (treeData == null)
                return;

            EditorUtility.SetDirty(treeData);
        }

        private void CreateNewTreeAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Skill Tree",
                "NewSkillTree",
                "asset",
                "Choose location for the new skill tree asset."
            );

            if (string.IsNullOrEmpty(path))
                return;

            SkillTreeData newTree = CreateInstance<SkillTreeData>();
            newTree.treeId = "new_tree";
            newTree.nodes = new List<SkillNodeData>();

            AssetDatabase.CreateAsset(newTree, path);
            AssetDatabase.SaveAssets();

            treeData = newTree;
            selectedNode = null;
            isConnecting = false;
            connectionStartNode = null;

            EditorGUIUtility.PingObject(treeData);
        }
    }
}
