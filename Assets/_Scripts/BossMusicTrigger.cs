using UnityEngine;

public class BossMusicTrigger : MonoBehaviour
{
    [Header("Behaviour")]
    [SerializeField] private bool playOnStart = true;

    [Tooltip("Если true, после уничтожения босса музыка вернётся к обычной игровой.")]
    [SerializeField] private bool returnToGameMusicOnDestroy = true;

    private bool played;

    private void Start()
    {
        if (playOnStart)
            PlayBossMusic();
    }

    public void PlayBossMusic()
    {
        if (played)
            return;

        played = true;

        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayNextBossMusic();
    }

    private void OnDestroy()
    {
        if (!returnToGameMusicOnDestroy)
            return;

        if (G.IsPlayerDead)
            return;

        if (G.ui != null)
        {
            if (G.ui.IsDeathPanelOpen())
                return;

            if (G.ui.IsSkillTreePanelOpen() || G.ui.IsFinalPanelOpen())
                return;
        }

        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayGameMusic();
    }
}