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
    private bool initialized;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnEnable()
    {
        EnsureInitialized();
        Refresh();
    }

    private void OnDestroy()
    {
        if (levelButtons != null)
        {
            for (int i = 0; i < levelButtons.Length; i++)
            {
                if (levelButtons[i] != null)
                    levelButtons[i].onClick.RemoveAllListeners();
            }
        }

        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayClicked);
    }

    private void EnsureInitialized()
    {
        if (initialized && levelImages != null && levelButtons != null && levelImages.Length == levelButtons.Length)
            return;

        if (levelButtons == null)
        {
            levelImages = new Image[0];
            initialized = true;
            return;
        }

        levelImages = new Image[levelButtons.Length];

        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == null)
                continue;

            levelImages[i] = levelButtons[i].GetComponent<Image>();

            int index = i;
            levelButtons[i].onClick.RemoveAllListeners();
            levelButtons[i].onClick.AddListener(() => SelectLevel(index));
        }

        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OnPlayClicked);
            playButton.onClick.AddListener(OnPlayClicked);
        }

        initialized = true;
    }

    private void SelectLevel(int index)
    {
        if (index > LevelProgress.UnlockedLevel)
            return;

        LevelProgress.SelectedLevel = index;
        Refresh();
    }

    private void Refresh()
    {
        EnsureInitialized();

        if (levelButtons == null || levelImages == null)
            return;

        int levelCount = G.levelDatabase != null && G.levelDatabase.levels != null
            ? G.levelDatabase.levels.Length
            : levelButtons.Length;

        if (LevelProgress.SelectedLevel < 0)
            LevelProgress.SelectedLevel = 0;

        if (LevelProgress.SelectedLevel >= levelCount)
            LevelProgress.SelectedLevel = Mathf.Max(0, levelCount - 1);

        LevelProgress.UnlockedLevel = Mathf.Clamp(LevelProgress.UnlockedLevel, 0, Mathf.Max(0, levelCount - 1));

        for (int i = 0; i < levelButtons.Length; i++)
        {
            bool exists = i < levelCount;
            bool unlocked = exists && i <= LevelProgress.UnlockedLevel;
            bool selected = exists && i == LevelProgress.SelectedLevel;

            if (levelButtons[i] != null)
                levelButtons[i].interactable = unlocked;

            if (i < levelImages.Length && levelImages[i] != null)
            {
                if (!unlocked)
                    levelImages[i].sprite = lockedSprite;
                else if (selected)
                    levelImages[i].sprite = selectedSprite;
                else
                    levelImages[i].sprite = normalSprite;
            }
        }

        bool canPlay = levelCount > 0
                    && LevelProgress.SelectedLevel <= LevelProgress.UnlockedLevel
                    && LevelProgress.SelectedLevel < levelCount;

        if (playButton != null)
            playButton.interactable = canPlay;
    }

    private void OnPlayClicked()
    {
        if (G.main != null)
            G.main.StartNewRun();
    }
}


