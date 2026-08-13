using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the persistent gameplay UI including objectives, dialogue,
/// timer countdown, pause menu, win/lose conditions, end-game audio,
/// and UI visibility. This script persists across scenes.
/// </summary>
public class GameplayUI : MonoBehaviour
{
    private static GameplayUI instance;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI objectivesText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameUI;  // The main HUD canvas — hidden while paused

    [Header("Timer Settings")]
    [SerializeField] private float startTime = 480f;
    [SerializeField] private string wave2SceneName;
    [SerializeField] private string finalWaveSceneName;

    [Header("End Game Audio")]
    [SerializeField] private AudioClip winMusic;
    [SerializeField] private AudioClip loseMusic;

    private int currentWaveIndex = 0;

    // Required catches per wave
    private int[] thievesPerWave = { 2, 1, 2 };
    private int[] hackersPerWave = { 0, 1, 2 };

    private float currentTime;
    private bool quizCompleted = false;
    private int thiefCaught = 0;
    private int hackerCaught = 0;
    private bool isPaused = false;
    private bool isGameOver = false;  // Prevents WinGame/LoseGame firing every frame

    /// <summary>
    /// Ensures only one instance of GameplayUI persists across scenes.
    /// </summary>
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    /// <summary>
    /// Initializes timer and UI state.
    /// </summary>
    void Start()
    {
        currentTime = startTime;
        if (losePanel != null) losePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        UpdateObjectives();
        dialogueText.text = "Refresh your knowledge of crime prevention,\n\nHead towards the Exhibition area and interact with a Kiosk to complete a Quiz";
    }

    /// <summary>
    /// Handles Escape key for pause toggle, then runs the timer countdown.
    /// </summary>
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                ContinueGame();
            else
                OpenPauseMenu();
        }

        // Once the game has ended stop all timer logic.
        // Without isGameOver, LoseGame/WinGame fire every frame because
        // Time.deltaTime is 0 when frozen so currentTime never changes.
        if (isGameOver) return;

        if (!isPaused)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                UpdateObjectives();
            }
            else
            {
                LoseGame();
            }
        }
    }

    // ---------------- Quiz ----------------

    /// <summary>
    /// Marks the quiz as completed and updates dialogue.
    /// </summary>
    public void CompleteQuiz()
    {
        quizCompleted = true;
        UpdateObjectives();
        dialogueText.text = "Please head to the Gallery using the Lift in the Exhibition area.";
    }

    // ---------------- Catch Counters ----------------

    /// <summary>
    /// Increments thief caught count and updates objectives.
    /// </summary>
    public void CatchThief()
    {
        thiefCaught++;
        if (thiefCaught > 5) thiefCaught = 5;
        UpdateObjectives();
    }

    /// <summary>
    /// Increments hacker caught count and updates objectives.
    /// </summary>
    public void CatchHacker()
    {
        hackerCaught++;
        if (hackerCaught > 3) hackerCaught = 3;
        UpdateObjectives();
    }

    // ---------------- Timer ----------------

    /// <summary>
    /// Adds to or subtracts from the remaining time, clamped at zero.
    /// </summary>
    public void AdjustTimer(float amount)
    {
        currentTime += amount;
        if (currentTime < 0f)
            currentTime = 0f;
        UpdateObjectives();
    }

    // ---------------- Objectives ----------------

    /// <summary>
    /// Updates objectives text including timer, quiz, thief, and hacker progress.
    /// Also checks win conditions per wave.
    /// </summary>
    void UpdateObjectives()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        string timeFormatted = string.Format("{0}:{1:00}", minutes, seconds);

        string quizProgress = quizCompleted ? "1/1" : "0/1";

        objectivesText.text =
            "Objectives\n" +
            "Time Left: " + timeFormatted + "\n" +
            "1. Complete Quiz " + quizProgress + "\n" +
            "2. Catch the People Involved:\n" +
            "   Thief: " + thiefCaught + "/5\n" +
            "   Hacker: " + hackerCaught + "/3";

        if (quizCompleted)
        {
            int requiredThieves = 0;
            int requiredHackers = 0;
            for (int i = 0; i <= currentWaveIndex; i++)
            {
                requiredThieves += thievesPerWave[i];
                requiredHackers += hackersPerWave[i];
            }

            if (thiefCaught >= requiredThieves && hackerCaught >= requiredHackers)
            {
                if (currentWaveIndex < 2)
                    TriggerNextWave();
                else
                    WinGame();
            }
        }
    }

    // ---------------- Wave Transition ----------------

    /// <summary>
    /// Loads the next wave scene and advances the wave index.
    /// </summary>
    void TriggerNextWave()
    {
        if (currentWaveIndex == 0)
            SceneManager.LoadScene(wave2SceneName);
        else if (currentWaveIndex == 1)
            SceneManager.LoadScene(finalWaveSceneName);

        currentWaveIndex++;
    }

    /// <summary>
    /// Updates the dialogue text shown to the player.
    /// </summary>
    public void SetDialogue(string message)
    {
        if (dialogueText != null)
            dialogueText.text = message;
    }

    // ---------------- End Game ----------------

    /// <summary>
    /// Shows the win panel and plays win music.
    /// isGameOver guard ensures this only ever runs once.
    /// </summary>
    void WinGame()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (winPanel != null) winPanel.SetActive(true);
        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayEndGameMusic(winMusic);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Shows the lose panel and plays lose music.
    /// isGameOver guard ensures this only ever runs once.
    /// </summary>
    void LoseGame()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (losePanel != null) losePanel.SetActive(true);
        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayEndGameMusic(loseMusic);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Updates dialogue when entering specific scenes.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GalleryScene")
        {
            dialogueText.text = "The Culprits of the Heist are in this Gallery. Find them and catch them using the Key E!";
        }
    }

    // ---------------- Pause Menu ----------------

    /// <summary>
    /// Opens the pause menu, hides the HUD, disables player input,
    /// unlocks the cursor, and freezes the game.
    /// </summary>
    public void OpenPauseMenu()
    {
        if (isGameOver) return;

        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);

        // Hide the main HUD canvas so it doesn't block button clicks
        Transform gameUITransform = transform.Find("GameUI");
        if (gameUITransform != null) gameUITransform.gameObject.SetActive(false);

        // Disable PlayerInput so mouse clicks reach the UI buttons
        // instead of being consumed by the player controller
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerInput pi = player.GetComponent<PlayerInput>();
            if (pi != null) pi.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Closes the pause menu, restores the HUD, re-enables player input,
    /// re-locks the cursor, and resumes the game.
    /// </summary>
    public void ContinueGame()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);

        // Restore the main HUD canvas
        Transform gameUITransform = transform.Find("GameUI");
        if (gameUITransform != null) gameUITransform.gameObject.SetActive(true);

        // Re-enable PlayerInput so the player can move and look again
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerInput pi = player.GetComponent<PlayerInput>();
            if (pi != null) pi.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Exits the game. Stops Play Mode in Editor, quits application in build.
    /// </summary>
    public void ExitGame()
    {
        Time.timeScale = 1f;

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    // ---------------- UI Visibility ----------------

    /// <summary>
    /// Shows the full GameplayUI object (objectives and dialogue).
    /// </summary>
    public void ShowUI() => gameObject.SetActive(true);

    /// <summary>
    /// Hides the full GameplayUI object (objectives and dialogue).
    /// </summary>
    public void HideUI() => gameObject.SetActive(false);
}