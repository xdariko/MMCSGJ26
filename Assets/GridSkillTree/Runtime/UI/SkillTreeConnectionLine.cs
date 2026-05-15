using UnityEngine;
using UnityEngine.UI;

namespace GridSkillTree
{
    public class SkillTreeConnectionLine : MonoBehaviour
    {
        [SerializeField] private Image image;

        public void SetPoints(Vector2 from, Vector2 to, float thickness = 6f)
        {
            RectTransform rect = GetComponent<RectTransform>();

            Vector2 direction = to - from;
            float distance = direction.magnitude;

            rect.anchoredPosition = from + direction * 0.5f;
            rect.sizeDelta = new Vector2(distance, thickness);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rect.localRotation = Quaternion.Euler(0f, 0f, angle);

            if (image != null)
                image.raycastTarget = false;
        }
    }
}