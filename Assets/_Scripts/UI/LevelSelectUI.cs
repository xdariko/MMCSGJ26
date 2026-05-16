using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button[] levelButtons;
    [SerializeField] private Button playButton;

    [Header("Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite lockedSprite;

    private Image[] levelImages;

    private void Start()
    {
        levelImages = new Image[levelButtons.Length];

        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelImages[i] = levelButtons[i].GetComponent<Image>();

            int index = i;
            levelButtons[i].onClick.AddListener(() => SelectLevel(index));
        }

        playButton.onClick.AddListener(OnPlayClicked);
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayClicked);
    }

    private void SelectLevel(int index)
    {
        if (index > LevelProgress.UnlockedLevel) return;

        LevelProgress.SelectedLevel = index;
        Refresh();
    }

    private void Refresh()
    {
        if (levelButtons == null || levelImages == null) return;

        for (int i = 0; i < levelButtons.Length; i++)
        {
            bool unlocked = i <= LevelProgress.UnlockedLevel;
            bool selected = i == LevelProgress.SelectedLevel;

            levelButtons[i].interactable = unlocked;

            if (levelImages[i] != null)
            {
                if (!unlocked)
                    levelImages[i].sprite = lockedSprite;
                else if (selected)
                    levelImages[i].sprite = selectedSprite;
                else
                    levelImages[i].sprite = normalSprite;
            }
        }

        bool canPlay = LevelProgress.SelectedLevel <= LevelProgress.UnlockedLevel
                    && LevelProgress.SelectedLevel < levelButtons.Length;

        playButton.interactable = canPlay;
    }

    private void OnPlayClicked()
    {
        if (G.main != null)
            G.main.StartNewRun();
    }
}
