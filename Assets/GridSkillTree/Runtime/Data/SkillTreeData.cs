using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GridSkillTree
{
    [CreateAssetMenu(menuName = "Grid Skill Tree/Skill Tree Data")]
    public class SkillTreeData : ScriptableObject
    {
        public string treeId = "main_tree";
        public List<SkillNodeData> nodes = new();

        public SkillNodeData GetNodeById(string id)
        {
            return nodes.FirstOrDefault(node => node.id == id);
        }

        public bool ContainsNode(string id)
        {
            return nodes.Any(node => node.id == id);
        }
    }
}