using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the persistent gameplay UI including objectives, dialogue,
/// timer countdown, pause menu, lose condition, and UI visibility.
/// This script persists across scenes.
/// </summary>
public class GameplayUI : MonoBehaviour
{
    private static GameplayUI instance;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI objectivesText; 
    /// <summary>Text element displaying objectives and timer.</summary>

    [SerializeField] private TextMeshProUGUI dialogueText;   
    /// <summary>Text element displaying dialogue instructions.</summary>

    [SerializeField] private GameObject losePanel;           
    /// <summary>Panel shown when the player loses (time runs out).</summary>

    [SerializeField] private GameObject pausePanel;          
    /// <summary>Panel shown when the game is paused.</summary>

    [Header("Timer Settings")]
    [SerializeField] private float startTime = 480f;         
    /// <summary>Starting time in seconds (default 8 minutes).</summary>

    private float currentTime;                               /// <summary>Current countdown time.</summary>
    private bool quizCompleted = false;                      /// <summary>Tracks whether the quiz is completed.</summary>
    private int thiefCaught = 0;                             /// <summary>Number of thieves caught.</summary>
    private int hackerCaught = 0;                            /// <summary>Number of hackers caught.</summary>
    private bool isPaused = false;                           /// <summary>Tracks whether the game is paused.</summary>

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
    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

    /// <summary>
    /// Unsubscribes from scene load events.
    /// </summary>
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    /// <summary>
    /// Initializes timer and UI state.
    /// </summary>
    void Start()
    {
        currentTime = startTime;
        if (losePanel != null) losePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        UpdateObjectives();
        dialogueText.text = "Refresh your knowledge of crime prevention,\n\nHead towards the exhibition area and interact with a Kiosk to complete a Quiz";
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
        dialogueText.text = "Please head towards the Gallery using the Lift in this room.";
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
    /// Updates dialogue when entering specific scenes.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "galleryscene")
        {
            dialogueText.text = "The Culprits of the Heist are in this Gallery. Find them and catch them using the Key E!";
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
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    // ---------------- UI Visibility Methods ----------------

    /// <summary>
    /// Shows the UI (objectives and dialogue).
    /// </summary>
    public void ShowUI() => gameObject.SetActive(true);

    /// <summary>
    /// Hides the UI (objectives and dialogue).
    /// </summary>
    public void HideUI() => gameObject.SetActive(false);
}
