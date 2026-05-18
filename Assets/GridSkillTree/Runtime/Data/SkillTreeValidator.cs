using System.Collections.Generic;
using UnityEngine;

namespace GridSkillTree
{
    public static class SkillTreeValidator
    {
        public static SkillTreeValidationResult Validate(SkillTreeData treeData)
        {
            SkillTreeValidationResult result = new();

            if (treeData == null)
            {
                result.AddError("Tree Data is null.");
                return result;
            }

            ValidateNodes(treeData, result);
            ValidateConnections(treeData, result);

            return result;
        }

        private static void ValidateNodes(SkillTreeData treeData, SkillTreeValidationResult result)
        {
            HashSet<string> ids = new();
            Dictionary<Vector2Int, string> occupiedCells = new();

            foreach (SkillNodeData node in treeData.nodes)
            {
                if (node == null)
                {
                    result.AddError("Tree contains a null node.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(node.id))
                {
                    result.AddError($"Node at {node.gridPosition} has an empty id.");
                    continue;
                }

                if (!ids.Add(node.id))
                    result.AddError($"Duplicate node id: {node.id}");

                if (occupiedCells.TryGetValue(node.gridPosition, out string existingNodeId))
                {
                    result.AddError(
                        $"Cell {node.gridPosition} is occupied by both '{existingNodeId}' and '{node.id}'."
                    );
                }
                else
                {
                    occupiedCells.Add(node.gridPosition, node.id);
                }

                if (node.maxLevel < 1)
                    result.AddError($"Node '{node.id}' has maxLevel < 1.");

                if (node.costs == null)
                {
                    result.AddWarning($"Node '{node.id}' has null costs list.");
                }
                else
                {
                    foreach (SkillCost cost in node.costs)
                    {
                        if (cost == null)
                        {
                            result.AddWarning($"Node '{node.id}' has null cost entry.");
                            continue;
                        }

                        if (cost.currency == CurrencyType.None)
                            result.AddWarning($"Node '{node.id}' has cost with CurrencyType.None.");

                        if (cost.baseAmount < 0)
                            result.AddError($"Node '{node.id}' has cost baseAmount < 0.");
                    }
                }

                if (node.effectType == SkillEffectType.CurrencyDropPercent && node.currencyDropType == CurrencyType.None)
                    result.AddWarning($"Node '{node.id}' has CurrencyDropPercent effect with CurrencyType.None.");

                if (node.effectType == SkillEffectType.PassiveCurrency)
                {
                    if (node.passiveCurrencyType == CurrencyType.None)
                        result.AddWarning($"Node '{node.id}' has PassiveCurrency effect with CurrencyType.None.");

                    if (node.passiveCurrencyIntervalSeconds <= 0f)
                        result.AddError($"Node '{node.id}' has PassiveCurrency interval <= 0.");
                }

                if (node.previousNodeIds == null)
                    result.AddWarning($"Node '{node.id}' has null previousNodeIds list.");
            }
        }

        private static void ValidateConnections(SkillTreeData treeData, SkillTreeValidationResult result)
        {
            HashSet<string> existingIds = new();

            foreach (SkillNodeData node in treeData.nodes)
            {
                if (node != null && !string.IsNullOrWhiteSpace(node.id))
                    existingIds.Add(node.id);
            }

            foreach (SkillNodeData node in treeData.nodes)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.id))
                    continue;

                if (node.previousNodeIds == null)
                    continue;

                HashSet<string> localPreviousIds = new();

                foreach (string previousNodeId in node.previousNodeIds)
                {
                    if (string.IsNullOrWhiteSpace(previousNodeId))
                    {
                        result.AddError($"Node '{node.id}' has an empty previous node id.");
                        continue;
                    }

                    if (previousNodeId == node.id)
                    {
                        result.AddError($"Node '{node.id}' references itself.");
                        continue;
                    }

                    if (!existingIds.Contains(previousNodeId))
                    {
                        result.AddError($"Node '{node.id}' references missing node '{previousNodeId}'.");
                        continue;
                    }

                    if (!localPreviousIds.Add(previousNodeId))
                    {
                        result.AddWarning($"Node '{node.id}' has duplicate previous node reference '{previousNodeId}'.");
                    }
                }
            }
        }
    }
}
