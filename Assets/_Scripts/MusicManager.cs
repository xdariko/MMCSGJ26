using UnityEngine;

[DisallowMultipleComponent]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private enum MusicState
    {
        None,
        Menu,
        Game,
        Boss
    }

    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;

    [Header("Menu Music")]
    [Tooltip("Музыка для меню, дерева прокачки, выбора уровня.")]
    [SerializeField] private AudioClip menuTrack;

    [Header("Game Music")]
    [Tooltip("Обычные игровые треки. Каждый новый забег будет брать следующий трек из этого списка.")]
    [SerializeField] private AudioClip[] gameTracks;

    [Tooltip("Если true, выбранный игровой трек будет повторяться до конца забега.")]
    [SerializeField] private bool loopCurrentGameTrack = true;

    [Header("Boss Music")]
    [Tooltip("Треки боссов. Будут включаться по очереди в том порядке, в котором ты их укажешь.")]
    [SerializeField] private AudioClip[] bossTracks;

    [Header("Settings")]
    [SerializeField] private bool dontDestroyOnLoad = false;

    [Tooltip("Плавное переключение между обычными треками. Смерть всё равно останавливает музыку резко.")]
    [SerializeField] private float fadeDuration = 0.45f;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [Header("Preload")]
    [Tooltip("Лучше включить. Unity заранее подготовит аудиоклипы, чтобы не было рывка при старте трека.")]
    [SerializeField] private bool preloadAudioClips = true;

    private MusicState currentState = MusicState.None;

    private int nextGameTrackIndex;
    private int currentRunGameTrackIndex = -1;
    private int bossTrackIndex;

    private float fadeTimer;
    private bool isFadingOut;
    private AudioClip pendingClip;
    private MusicState pendingState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        G.music = this;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        EnsureAudioSource();

        if (preloadAudioClips)
            PreloadAllClips();
    }

    private void Update()
    {
        UpdateFade();
        UpdateTrackLoop();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (G.music == this)
            G.music = null;
    }

    public void PlayMenuMusic()
    {
        PlayMenuMusic(true);
    }

    public void PlayMenuMusic(bool allowFade)
    {
        if (menuTrack == null)
            return;

        PlayMusic(menuTrack, MusicState.Menu, allowFade);
    }

    public void PlayNextGameMusicForRun()
    {
        PlayNextGameMusicForRun(true);
    }

    public void PlayNextGameMusicForRun(bool allowFade)
    {
        AudioClip clip = GetNextGameTrackForNewRun();

        if (clip == null)
            return;

        PlayMusic(clip, MusicState.Game, allowFade);
    }

    public void PlayGameMusic()
    {
        PlayGameMusic(true);
    }

    public void PlayGameMusic(bool allowFade)
    {
        AudioClip clip = GetCurrentRunGameTrack();

        if (clip == null)
            clip = GetNextGameTrackForNewRun();

        if (clip == null)
            return;

        PlayMusic(clip, MusicState.Game, allowFade);
    }

    public void PlayNextBossMusic()
    {
        AudioClip clip = GetNextBossTrack();

        if (clip == null)
        {
            PlayGameMusic();
            return;
        }

        PlayMusic(clip, MusicState.Boss, true);
    }

    public void StopMusicImmediate()
    {
        EnsureAudioSource();

        isFadingOut = false;
        pendingClip = null;
        pendingState = MusicState.None;
        currentState = MusicState.None;

        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = volume;
    }

    public void StopMusicWithFade()
    {
        EnsureAudioSource();

        if (!musicSource.isPlaying)
        {
            StopMusicImmediate();
            return;
        }

        pendingClip = null;
        pendingState = MusicState.None;
        isFadingOut = true;
        fadeTimer = 0f;
    }

    public void RefreshMusicByState()
    {
        if (G.ui != null)
        {
            if (G.ui.IsDeathPanelOpen())
            {
                StopMusicImmediate();
                return;
            }

            if (G.ui.IsFinalPanelOpen() || G.ui.IsSkillTreePanelOpen())
            {
                PlayMenuMusic();
                return;
            }
        }

        if (G.IsPlayerDead)
        {
            StopMusicImmediate();
            return;
        }

        PlayGameMusic();
    }

    private void PlayMusic(AudioClip clip, MusicState state, bool allowFade)
    {
        if (clip == null)
            return;

        EnsureAudioSource();

        if (musicSource.clip == clip && musicSource.isPlaying && currentState == state)
            return;

        if (preloadAudioClips)
            clip.LoadAudioData();

        if (!allowFade || fadeDuration <= 0f || !musicSource.isPlaying)
        {
            SetClipAndPlay(clip, state);
            return;
        }

        pendingClip = clip;
        pendingState = state;
        isFadingOut = true;
        fadeTimer = 0f;
    }

    private void SetClipAndPlay(AudioClip clip, MusicState state)
    {
        EnsureAudioSource();

        currentState = state;
        isFadingOut = false;
        pendingClip = null;
        pendingState = MusicState.None;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = false;
        musicSource.volume = volume;
        musicSource.Play();
    }

    private void UpdateFade()
    {
        if (!isFadingOut)
            return;

        EnsureAudioSource();

        fadeTimer += Time.unscaledDeltaTime;

        float t = fadeDuration <= 0f
            ? 1f
            : Mathf.Clamp01(fadeTimer / fadeDuration);

        musicSource.volume = Mathf.Lerp(volume, 0f, t);

        if (t < 1f)
            return;

        isFadingOut = false;

        if (pendingClip == null)
        {
            StopMusicImmediate();
            return;
        }

        SetClipAndPlay(pendingClip, pendingState);
    }

    private void UpdateTrackLoop()
    {
        if (musicSource == null)
            return;

        if (isFadingOut)
            return;

        if (musicSource.clip == null)
            return;

        if (musicSource.isPlaying)
            return;

        if (currentState == MusicState.Game)
        {
            if (loopCurrentGameTrack)
                musicSource.Play();

            return;
        }

        if (currentState == MusicState.Boss)
        {
            musicSource.Play();
            return;
        }

        if (currentState == MusicState.Menu)
            musicSource.Play();
    }

    private void PreloadAllClips()
    {
        PreloadClip(menuTrack);

        if (gameTracks != null)
        {
            foreach (AudioClip clip in gameTracks)
                PreloadClip(clip);
        }

        if (bossTracks != null)
        {
            foreach (AudioClip clip in bossTracks)
                PreloadClip(clip);
        }
    }

    private void PreloadClip(AudioClip clip)
    {
        if (clip == null)
            return;

        clip.LoadAudioData();
    }

    private AudioClip GetNextGameTrackForNewRun()
    {
        if (gameTracks == null || gameTracks.Length == 0)
            return null;

        for (int i = 0; i < gameTracks.Length; i++)
        {
            int index = nextGameTrackIndex;

            nextGameTrackIndex++;

            if (nextGameTrackIndex >= gameTracks.Length)
                nextGameTrackIndex = 0;

            if (gameTracks[index] == null)
                continue;

            currentRunGameTrackIndex = index;
            return gameTracks[index];
        }

        currentRunGameTrackIndex = -1;
        return null;
    }

    private AudioClip GetCurrentRunGameTrack()
    {
        if (gameTracks == null || gameTracks.Length == 0)
            return null;

        if (currentRunGameTrackIndex < 0 || currentRunGameTrackIndex >= gameTracks.Length)
            return null;

        return gameTracks[currentRunGameTrackIndex];
    }

    private AudioClip GetNextBossTrack()
    {
        if (bossTracks == null || bossTracks.Length == 0)
            return null;

        for (int i = 0; i < bossTracks.Length; i++)
        {
            int index = bossTrackIndex;

            bossTrackIndex++;

            if (bossTrackIndex >= bossTracks.Length)
                bossTrackIndex = 0;

            if (bossTracks[index] != null)
                return bossTracks[index];
        }

        return null;
    }

    private void EnsureAudioSource()
    {
        if (musicSource != null)
            return;

        musicSource = GetComponent<AudioSource>();

        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = false;
        musicSource.volume = volume;
    }
}