using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MusicManager.cs
/// Plays a looping background track and persists across scene loads.
/// Automatically crossfades to a different track when a scene with a
/// matching entry in sceneTracks is loaded (e.g. switching to chase music).
/// Also handles end game music via PlayEndGameMusic().
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [System.Serializable]
    public class SceneTrack
    {
        public string sceneName;
        public AudioClip clip;
    }

    [Header("Default Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;
    [SerializeField] private bool playOnStart = true;

    [Header("Per-Scene Music")]
    [SerializeField] private SceneTrack[] sceneTracks;
    [SerializeField] private float fadeDuration = 1.5f;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.volume = volume;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (playOnStart)
            Play();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioClip targetClip = backgroundMusic;

        foreach (SceneTrack track in sceneTracks)
        {
            if (track.sceneName == scene.name && track.clip != null)
            {
                targetClip = track.clip;
                break;
            }
        }

        ChangeTrack(targetClip);
    }

    // =====================================================
    // PLAYBACK CONTROL
    // =====================================================

    public void Play()
    {
        if (audioSource.clip == null) return;
        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (fadeCoroutine == null)
            audioSource.volume = volume;
    }

    /// <summary>
    /// Crossfades to a new clip. Does nothing if the clip is already playing.
    /// </summary>
    public void ChangeTrack(AudioClip newClip)
    {
        if (newClip == null || audioSource.clip == newClip)
            return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToClip(newClip));
    }

    /// <summary>
    /// Uses unscaledDeltaTime so fades still complete even when the
    /// game is paused (Time.timeScale = 0).
    /// </summary>
    private IEnumerator FadeToClip(AudioClip newClip)
    {
        float halfDuration = fadeDuration / 2f;

        // Fade out
        float startVolume = audioSource.volume;
        float t = 0f;
        while (t < halfDuration)
        {
            t += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / halfDuration);
            yield return null;
        }
        audioSource.volume = 0f;

        // Swap clip
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in
        t = 0f;
        while (t < halfDuration)
        {
            t += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, t / halfDuration);
            yield return null;
        }
        audioSource.volume = volume;

        fadeCoroutine = null;
    }

    /// <summary>
    /// Plays a one-shot end game clip immediately, stopping any
    /// in-progress fade first. Safe to call while Time.timeScale is 0
    /// because audio playback ignores timeScale entirely.
    /// </summary>
    public void PlayEndGameMusic(AudioClip clip)
    {
        if (clip == null) return;

        // Kill any running fade coroutine so it can't interfere
        StopAllCoroutines();
        fadeCoroutine = null;

        // Restore volume in case a fade left it at zero,
        // then swap to the end game clip and play immediately
        audioSource.volume = volume;
        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = clip;
        audioSource.Play();
    }
}