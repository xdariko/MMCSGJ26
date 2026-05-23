using System.Collections.Generic;
using UnityEngine;

namespace GridSkillTree
{
    public class SkillTreeUIBuilder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SkillTreeRuntime runtime;
        [SerializeField] private SkillTreeUIConfig config;
        [SerializeField] private RectTransform nodesRoot;
        [SerializeField] private RectTransform linesRoot;
        [SerializeField] private SkillTooltip tooltip;

        private readonly List<SkillNodeButton> spawnedButtons = new();
        private readonly List<ConnectionView> spawnedLines = new();

        private readonly Dictionary<string, RectTransform> nodeRectsById = new();
        private readonly Dictionary<string, SkillNodeButton> buttonsById = new();
        private readonly Dictionary<string, SkillNodeData> nodesById = new();

        private class ConnectionView
        {
            public SkillTreeConnectionLine line;
            public string fromNodeId;
            public string toNodeId;
        }

        private void Start()
        {
            Build();

            if (runtime != null)
                runtime.OnTreeChanged += Refresh;

            CurrencyManager.OnCurrencyChanged += HandleCurrencyChanged;
        }

        private void OnDestroy()
        {
            if (runtime != null)
                runtime.OnTreeChanged -= Refresh;

            CurrencyManager.OnCurrencyChanged -= HandleCurrencyChanged;
        }

        private void HandleCurrencyChanged(CurrencyType type, int total)
        {
            Refresh();
        }

        public void Build()
        {
            Clear();

            if (runtime == null || runtime.TreeData == null || config == null || config.nodeButtonPrefab == null)
            {
                Debug.LogError("SkillTreeUIBuilder is missing references.");
                return;
            }

            SkillTreeValidationResult validation = SkillTreeValidator.Validate(runtime.TreeData);

            if (!validation.IsValid)
            {
                Debug.LogError(validation.GetReport());
                return;
            }

            if (validation.warnings.Count > 0)
            {
                Debug.LogWarning(validation.GetReport());
            }

            CacheNodes();
            BuildNodes();
            BuildConnections();
            Refresh();
        }

        private void CacheNodes()
        {
            nodesById.Clear();

            foreach (SkillNodeData node in runtime.TreeData.nodes)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.id))
                    continue;

                nodesById[node.id] = node;
            }
        }

        private void BuildNodes()
        {
            foreach (SkillNodeData node in runtime.TreeData.nodes)
            {
                if (node == null)
                    continue;

                SkillNodeButton button = Instantiate(config.nodeButtonPrefab, nodesRoot);

                RectTransform rect = button.GetComponent<RectTransform>();
                rect.anchoredPosition = GridToUIPosition(node.gridPosition);

                button.Init(node, runtime, config, tooltip);

                spawnedButtons.Add(button);
                buttonsById[node.id] = button;
                nodeRectsById[node.id] = rect;
            }
        }

        private void BuildConnections()
        {
            if (config.connectionLinePrefab == null || linesRoot == null)
                return;

            foreach (SkillNodeData node in runtime.TreeData.nodes)
            {
                if (node == null)
                    continue;

                if (!nodeRectsById.TryGetValue(node.id, out RectTransform currentRect))
                    continue;

                foreach (string previousNodeId in node.previousNodeIds)
                {
                    if (!nodeRectsById.TryGetValue(previousNodeId, out RectTransform previousRect))
                    {
                        Debug.LogWarning($"Connection skipped. Previous node not found: {previousNodeId}");
                        continue;
                    }

                    SkillTreeConnectionLine line = Instantiate(config.connectionLinePrefab, linesRoot);

                    Vector2 from = previousRect.anchoredPosition;
                    Vector2 to = currentRect.anchoredPosition;

                    line.SetPoints(from, to, config.connectionThickness);

                    spawnedLines.Add(new ConnectionView
                    {
                        line = line,
                        fromNodeId = previousNodeId,
                        toNodeId = node.id
                    });
                }
            }
        }

        public void Refresh()
        {
            if (tooltip != null)
                tooltip.Hide();

            RefreshNodesVisibility();
            RefreshLinesVisibility();
        }

        private void RefreshNodesVisibility()
        {
            foreach (SkillNodeButton button in spawnedButtons)
            {
                if (button == null || button.Node == null)
                    continue;

                bool visible = runtime.ShouldShowNode(button.Node);

                button.gameObject.SetActive(visible);

                if (visible)
                    button.Refresh();
            }
        }

        private void RefreshLinesVisibility()
        {
            foreach (ConnectionView connection in spawnedLines)
            {
                if (connection == null || connection.line == null)
                    continue;

                bool fromVisible = IsNodeVisible(connection.fromNodeId);
                bool toVisible = IsNodeVisible(connection.toNodeId);

                connection.line.gameObject.SetActive(fromVisible && toVisible);
            }
        }

        private bool IsNodeVisible(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
                return false;

            if (!nodesById.TryGetValue(nodeId, out SkillNodeData node))
                return false;

            return runtime.ShouldShowNode(node);
        }

        private void Clear()
        {
            if (nodesRoot != null)
            {
                for (int i = nodesRoot.childCount - 1; i >= 0; i--)
                {
                    GameObject go = nodesRoot.GetChild(i).gameObject;

                    if (go.scene.isLoaded)
                        Destroy(go);
                }
            }

            if (linesRoot != null)
            {
                for (int i = linesRoot.childCount - 1; i >= 0; i--)
                {
                    GameObject go = linesRoot.GetChild(i).gameObject;

                    if (go.scene.isLoaded)
                        Destroy(go);
                }
            }

            spawnedButtons.Clear();
            spawnedLines.Clear();

            nodeRectsById.Clear();
            buttonsById.Clear();
            nodesById.Clear();
        }

        private Vector2 GridToUIPosition(Vector2Int gridPosition)
        {
            float x = gridPosition.x * (config.cellSize.x + config.spacing.x);
            float y = gridPosition.y * (config.cellSize.y + config.spacing.y);

            return new Vector2(x, y);
        }
    }
}