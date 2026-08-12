using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// StartPage.cs
/// Controls the Start Scene UI flow:
/// - Shows Main Menu, Story/Goals, and Instructions panels
/// - Handles navigation between panels
/// - Loads the gameplay scene when "Continue" is pressed
/// - Exits the game when "Exit" is pressed
/// </summary>
public class StartPage : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;     // Panel with Start, Instructions, Exit buttons
    [SerializeField] private GameObject storyPanel;        // Panel with story, goals, tasks, Continue button
    [SerializeField] private GameObject instructionsPanel; // Panel with controls/tutorial, Back button

    /// <summary>
    /// Initialize the Start Scene by showing the Main Menu.
    /// Ensures the game runs at normal speed.
    /// </summary>
    void Start()
    {
        ShowMainMenu();
        Time.timeScale = 1f;
    }

    // ============================
    // MENU NAVIGATION
    // ============================

    /// <summary>
    /// Displays the Main Menu panel and hides all others.
    /// </summary>
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        storyPanel.SetActive(false);
        instructionsPanel.SetActive(false);
    }

    /// <summary>
    /// Displays the Story/Goals panel and hides others.
    /// </summary>
    public void ShowStory()
    {
        mainMenuPanel.SetActive(false);
        storyPanel.SetActive(true);
        instructionsPanel.SetActive(false);
    }

    /// <summary>
    /// Displays the Instructions panel and hides others.
    /// </summary>
    public void ShowInstructions()
    {
        mainMenuPanel.SetActive(false);
        storyPanel.SetActive(false);
        instructionsPanel.SetActive(true);
    }

    /// <summary>
    /// Returns to the Main Menu panel from Story or Instructions.
    /// </summary>
    public void BackToMenu()
    {
        ShowMainMenu();
    }

    // ============================
    // GAME FLOW
    // ============================

    /// <summary>
    /// Loads the main gameplay scene when the player presses Continue.
    /// Replace "GameScene" with your actual gameplay scene name.
    /// </summary>
    public void ContinueGame()
    {
        SceneManager.LoadScene("GameScene"); 
    }

    /// <summary>
    /// Exits the game. Works in both Editor and Build.
    /// </summary>
    public void ExitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stops Play Mode in editor
    #else
        Application.Quit(); // Quits the game in build
    #endif
    }
}
