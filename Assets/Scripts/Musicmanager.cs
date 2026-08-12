using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MusicManager.cs
/// Plays a looping background track and persists across scene loads.
/// Automatically crossfades to a different track when a scene with a
/// matching entry in sceneTracks is loaded (e.g. switching to chase music).
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [System.Serializable]
    public class SceneTrack
    {
        public string sceneName;   // Must match the exact Scene name in Build Settings
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
        // Singleton — prevent duplicate MusicManagers across scene loads
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
        audioSource.spatialBlend = 0f; // 2D sound — same volume no matter where the player is
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
        AudioClip targetClip = backgroundMusic; // fallback to default if no match found

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
    /// Crossfades to a new clip. Safe to call even if newClip is already playing
    /// (it'll just do nothing) or if it's the same clip as the default.
    /// </summary>
    public void ChangeTrack(AudioClip newClip)
    {
        if (newClip == null || audioSource.clip == newClip)
            return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToClip(newClip));
    }

    private IEnumerator FadeToClip(AudioClip newClip)
    {
        float halfDuration = fadeDuration / 2f;

        // Fade out current track
        float startVolume = audioSource.volume;
        float t = 0f;
        while (t < halfDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / halfDuration);
            yield return null;
        }
        audioSource.volume = 0f;

        // Swap clip
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in new track
        t = 0f;
        while (t < halfDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, t / halfDuration);
            yield return null;
        }
        audioSource.volume = volume;

        fadeCoroutine = null;
    }
}