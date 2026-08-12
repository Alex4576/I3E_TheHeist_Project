using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu.cs
/// Controls the Start Scene UI flow:
/// - Shows Main Menu
/// - Shows Story panel
/// - Loads gameplay scene
/// - Exits game
/// </summary>
public class Menu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel; // Play/Exit buttons
    [SerializeField] private GameObject storyPanel;    // Story + Continue button

    /// <summary>
    /// Initialize the Start Scene by showing the Main Menu.
    /// Ensures the game runs at normal speed.
    /// </summary>
    void Start()
    {
        ShowMainMenu();
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Displays the Main Menu panel and hides the Story panel.
    /// </summary>
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        storyPanel.SetActive(false);
    }

    /// <summary>
    /// Displays the Story panel when Play is pressed.
    /// </summary>
    public void ShowStory()
    {
        mainMenuPanel.SetActive(false);
        storyPanel.SetActive(true);
    }

    /// <summary>
    /// Loads the first scene when Continue is pressed.
    /// </summary>
    public void ContinueGame()
    {
        SceneManager.LoadScene("MuseumScene");
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
