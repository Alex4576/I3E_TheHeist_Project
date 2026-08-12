/*
* Author: Sheryn Batrisyia
* Date: 12/08/2026
* Description: Controls the crime prevention quiz kiosk interaction and quiz flow.
* The script manages camera switching, quiz UI panels, answer checking,
* wrong answer feedback, question progression, quiz completion,
* and unlocking the lift after the player completes the quiz.
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Controls the full quiz kiosk interaction, including entering quiz mode,
/// managing quiz panels, checking answers, showing feedback, progressing
/// through questions, and unlocking the lift upon quiz completion.
/// </summary>
public class QuizKiosk : MonoBehaviour
{
    [Header("Camera")]

    /// <summary>
    /// Fixed camera used while the player is interacting with the quiz kiosk.
    /// </summary>
    public Camera quizCamera;

    // Stores the player's main gameplay camera.
    private Camera mainCamera;


    [Header("Quiz UI")]

    /// <summary>
    /// Parent GameObject containing all quiz-related UI elements.
    /// </summary>
    public GameObject quizUI;

    /// <summary>
    /// Homepage displayed when the player first enters quiz mode.
    /// </summary>
    public GameObject homePanel;

    /// <summary>
    /// Panel displayed after the player completes all quiz questions.
    /// </summary>
    public GameObject completePanel;

    /// <summary>
    /// Reusable panel displayed when the player selects an incorrect answer.
    /// </summary>
    public GameObject wrongPanel;


    [Header("Questions")]

    /// <summary>
    /// Array containing each question panel in the order
    /// they should be displayed.
    /// </summary>
    public GameObject[] questionPanels;

    /// <summary>
    /// Stores the index of the correct answer for each question.
    /// Answer indexes follow: A = 0, B = 1, C = 2, D = 3.
    /// </summary>
    public int[] correctAnswers;


    [Header("Answer Colours")]

    /// <summary>
    /// Colour used to highlight the correct answer.
    /// </summary>
    public Color correctColor;


    [Header("Interaction")]

    /// <summary>
    /// Reference to the UIController used to hide and restore
    /// the player's normal interaction UI.
    /// </summary>
    public UIController uiController;


    [Header("Lift")]

    /// <summary>
    /// Reference to the lift that is unlocked after
    /// the player completes the quiz.
    /// </summary>
    public LiftDoor liftDoor;


    [Header("Gameplay UI")]
    /// <summary>
    /// Reference to the persistent GameplayUI HUD that updates objectives and dialogue.
    /// </summary>
    public GameplayUI gameplayUI;



    // Stores the index of the question currently being displayed.
    private int currentQuestion = 0;


    /// <summary>
    /// Initializes the quiz cameras and UI panels, then prepares
    /// the question buttons and their interactions.
    /// </summary>
    void Start()
    {
        mainCamera = Camera.main;

        quizCamera.enabled = false;

        if (quizUI != null)
        {
            quizUI.SetActive(false);
        }

        if (homePanel != null)
        {
            homePanel.SetActive(true);
        }

        if (completePanel != null)
        {
            completePanel.SetActive(false);
        }

        if (wrongPanel != null)
        {
            wrongPanel.SetActive(false);
        }

        SetupQuestions();

        if (gameplayUI == null)
        {
            gameplayUI = FindObjectOfType<GameplayUI>();
        }

    }


    /// <summary>
    /// Allows the player to exit quiz mode using the Escape key,
    /// but only while they are still on the quiz homepage.
    /// </summary>
    void Update()
    {
        // ESC can only exit while the player is on the quiz homepage.
        if (quizUI.activeSelf && homePanel.activeSelf)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ExitQuiz();
            }
        }
    }


    /// <summary>
    /// Starts quiz mode by hiding the normal interaction UI,
    /// switching to the fixed quiz camera, displaying the quiz UI,
    /// and unlocking the mouse cursor.
    /// </summary>
    public void StartQuiz()
    {
        uiController.HideInteractionUI();

        mainCamera.enabled = false;
        quizCamera.enabled = true;

        if (quizUI != null) quizUI.SetActive(true);
        if (gameplayUI != null) gameplayUI.HideUI(); // Hide UI during quiz

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    /// <summary>
    /// Hides the quiz homepage and begins the quiz
    /// from the first question.
    /// </summary>
    public void StartQuestions()
    {
        homePanel.SetActive(false);

        currentQuestion = 0;

        ShowQuestion(currentQuestion);
    }


    /// <summary>
    /// Prepares all question panels by assigning answer button
    /// listeners and Next button listeners automatically.
    /// </summary>
    void SetupQuestions()
    {
        for (int i = 0; i < questionPanels.Length; i++)
        {
            int questionIndex = i;

            questionPanels[i].SetActive(false);

            Button[] answerButtons = GetAnswerButtons(questionPanels[i]);

            // Assign each answer button to the answer checking method.
            for (int j = 0; j < answerButtons.Length; j++)
            {
                int answerIndex = j;

                if (answerButtons[j] != null)
                {
                    answerButtons[j].onClick.AddListener(
                        () => CheckAnswer(questionIndex, answerIndex)
                    );
                }
            }

            Button nextButton =
                FindButton(questionPanels[i], "NextButton");

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(false);

                nextButton.onClick.AddListener(
                    () => NextQuestion(questionIndex)
                );
            }
        }
    }


    /// <summary>
    /// Displays the requested question panel and hides
    /// all other question panels.
    /// </summary>
    /// <param name="questionIndex">
    /// Index of the question panel to display.
    /// </param>
    void ShowQuestion(int questionIndex)
    {
        for (int i = 0; i < questionPanels.Length; i++)
        {
            questionPanels[i].SetActive(i == questionIndex);
        }
    }


    /// <summary>
    /// Checks whether the selected answer is correct.
    /// Correct answers are highlighted and the Next button appears.
    /// Incorrect answers display the reusable wrong answer panel.
    /// </summary>
    /// <param name="questionIndex">
    /// Index of the current question.
    /// </param>
    /// <param name="answerIndex">
    /// Index of the selected answer.
    /// </param>
    void CheckAnswer(int questionIndex, int answerIndex)
    {
        Button[] answerButtons =
            GetAnswerButtons(questionPanels[questionIndex]);

        if (answerIndex == correctAnswers[questionIndex])
        {
            Button correctButton = answerButtons[answerIndex];

            correctButton.image.color = correctColor;

            // Disable all answers after the correct answer is selected.
            foreach (Button button in answerButtons)
            {
                if (button != null)
                {
                    button.interactable = false;
                }
            }

            // Display the Next button after a correct answer.
            Button nextButton =
                FindButton(questionPanels[questionIndex], "NextButton");

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(true);
            }
        }
        else
        {
            wrongPanel.SetActive(true);
        }
    }


    /// <summary>
    /// Hides the current question and moves the player
    /// to the next question. If all questions are complete,
    /// the quiz completion sequence begins.
    /// </summary>
    /// <param name="questionIndex">
    /// Index of the question being exited.
    /// </param>
    void NextQuestion(int questionIndex)
    {
        questionPanels[questionIndex].SetActive(false);

        currentQuestion++;

        if (currentQuestion < questionPanels.Length)
        {
            ShowQuestion(currentQuestion);
        }
        else
        {
            CompleteQuiz();
        }
    }


    /// <summary>
    /// Displays the quiz completion panel and unlocks
    /// the lift so the player can proceed to Level 2.
    /// </summary>
    void CompleteQuiz()
    {
        if (completePanel != null)
        {
            completePanel.SetActive(true);
        }

        if (liftDoor != null)
        {
            liftDoor.UnlockLift();
        }

        if (gameplayUI != null)
        {
            gameplayUI.CompleteQuiz();
        }

        Debug.Log("QUIZ COMPLETE - LIFT UNLOCKED");
    }


    /// <summary>
    /// Finds and returns the four answer buttons inside
    /// the supplied question panel.
    /// </summary>
    /// <param name="panel">
    /// Question panel containing the answer buttons.
    /// </param>
    /// <returns>
    /// Array containing the A, B, C, and D answer buttons.
    /// </returns>
    Button[] GetAnswerButtons(GameObject panel)
    {
        Button[] buttons = new Button[4];

        buttons[0] = FindButton(panel, "AnswerAButton");
        buttons[1] = FindButton(panel, "AnswerBButton");
        buttons[2] = FindButton(panel, "AnswerCButton");
        buttons[3] = FindButton(panel, "AnswerDButton");

        return buttons;
    }


    /// <summary>
    /// Searches the supplied GameObject and its children
    /// for a Button with the specified GameObject name.
    /// </summary>
    /// <param name="parent">
    /// Parent GameObject containing the button.
    /// </param>
    /// <param name="buttonName">
    /// Name of the button to search for.
    /// </param>
    /// <returns>
    /// Matching Button component if found; otherwise null.
    /// </returns>
    Button FindButton(GameObject parent, string buttonName)
    {
        Button[] buttons =
            parent.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (button.gameObject.name == buttonName)
            {
                return button;
            }
        }

        Debug.LogWarning(
            buttonName + " could not be found inside " + parent.name
        );

        return null;
    }


    /// <summary>
    /// Closes the wrong answer panel so the player can
    /// return to the current question and retry.
    /// </summary>
    public void CloseWrongPanel()
    {
        wrongPanel.SetActive(false);
    }


    /// <summary>
    /// Exits quiz mode from the homepage and returns the player
    /// to the normal gameplay camera and interaction UI.
    /// </summary>
    public void ExitQuiz()
    {
        quizCamera.enabled = false;
        mainCamera.enabled = true;

        quizUI.SetActive(false);

        uiController.ShowInteractionUI();
        if (gameplayUI != null) gameplayUI.ShowUI(); // restore GameplayUI

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    /// <summary>
    /// Exits the quiz after completion and returns the player
    /// to the normal gameplay camera and interaction UI.
    /// </summary>
    public void ExitCompletedQuiz()
    {
        quizCamera.enabled = false;
        mainCamera.enabled = true;

        quizUI.SetActive(false);

        uiController.ShowInteractionUI();
        if (gameplayUI != null) gameplayUI.ShowUI(); // Show UI again

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}