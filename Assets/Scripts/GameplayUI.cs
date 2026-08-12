using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the persistent gameplay HUD including timer, objectives, dialogue text,
/// pause menu, and lose condition. This script persists across scenes.
/// </summary>
public class GameplayUI : MonoBehaviour
{
    private static GameplayUI instance;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI objectivesText;  /// <summary>Displays objectives and timer.</summary>
    [SerializeField] private TextMeshProUGUI dialogueText;    /// <summary>Displays dialogue instructions.</summary>
    [SerializeField] private GameObject losePanel;            /// <summary>Panel shown when time runs out.</summary>
    [SerializeField] private GameObject pausePanel;           /// <summary>Panel shown when game is paused.</summary>

    [Header("Timer Settings")]
    [SerializeField] private float startTime = 480f;          /// <summary>Starting time in seconds (8 minutes).</summary>
    private float currentTime;

    private bool quizCompleted = false;
    private int thiefCaught = 0;
    private int hackerCaught = 0;
    private bool isPaused = false;

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

    /// <summary>
    /// Subscribes to scene load events.
    /// </summary>
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Unsubscribes from scene load events.
    /// </summary>
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Initializes timer and UI state.
    /// </summary>
    void Start()
    {
        currentTime = startTime;
        if (losePanel != null) losePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        UpdateObjectives();
        dialogueText.text = "Player, please head to exhibit to complete quiz.";
    }

    /// <summary>
    /// Updates timer countdown each frame and checks for lose condition.
    /// </summary>
    void Update()
    {
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

    /// <summary>
    /// Marks the quiz as completed, updates objectives, and changes dialogue text.
    /// </summary>
    public void CompleteQuiz()
    {
        quizCompleted = true;
        UpdateObjectives();
        dialogueText.text = "Please head towards the gallery using the lift in this room.";
    }

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

    /// <summary>
    /// Updates objectives text including timer, quiz, thief, and hacker progress.
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
    }

    /// <summary>
    /// Shows the lose panel and freezes the game when time runs out.
    /// </summary>
    void LoseGame()
    {
        if (losePanel != null) losePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Updates dialogue when entering specific scenes (e.g., Gallery).
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GalleryScene") // replace with your actual scene name
        {
            dialogueText.text = "The culprits of the heist are in this room. Find them and catch them!";
        }
    }

    // ---------------- Pause Menu Methods ----------------

    /// <summary>
    /// Opens the pause menu and freezes the game.
    /// </summary>
    public void OpenPauseMenu()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Closes the pause menu and resumes the game.
    /// </summary>
    public void ContinueGame()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Exits the game. Stops Play Mode in Editor, quits application in build.
    /// </summary>
    public void ExitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // stops Play Mode in Editor
    #else
        Application.Quit(); // closes the built game
    #endif
    }
}
