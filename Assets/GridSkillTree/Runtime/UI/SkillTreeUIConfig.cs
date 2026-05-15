using UnityEngine;

namespace GridSkillTree
{
    [CreateAssetMenu(menuName = "Grid Skill Tree/UI Config")]
    public class SkillTreeUIConfig : ScriptableObject
    {
        [Header("Prefabs")]
        public SkillNodeButton nodeButtonPrefab;
        public SkillTreeConnectionLine connectionLinePrefab;

        [Header("Connections")]
        public float connectionThickness = 6f;

        [Header("Layout")]
        public Vector2 cellSize = new Vector2(120f, 120f);
        public Vector2 spacing = new Vector2(30f, 30f);

        [Header("State Colors")]
        public Color lockedColor = Color.gray;
        public Color availableColor = Color.white;
        public Color purchasedColor = Color.green;
        public Color maxedColor = Color.yellow;
    }
}