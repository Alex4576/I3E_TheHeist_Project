using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuizKiosk : MonoBehaviour
{
    [Header("Camera")]
    public Camera quizCamera;
    private Camera mainCamera;

    [Header("Quiz UI")]
    public GameObject quizUI;
    public GameObject homePanel;
    public GameObject completePanel;
    public GameObject wrongPanel;

    [Header("Questions")]
    public GameObject[] questionPanels;

    // A = 0, B = 1, C = 2, D = 3
    public int[] correctAnswers;

    [Header("Answer Colours")]
    public Color correctColor;

    [Header("Interaction")]
    public UIController uiController;

    private int currentQuestion = 0;

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
    }

    public void StartQuiz()
    {
        uiController.HideInteractionUI();

        mainCamera.enabled = false;
        quizCamera.enabled = true;

        if (quizUI != null)
        {
            quizUI.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartQuestions()
    {
        homePanel.SetActive(false);

        currentQuestion = 0;

        ShowQuestion(currentQuestion);
    }

    void SetupQuestions()
    {
        for (int i = 0; i < questionPanels.Length; i++)
        {
            int questionIndex = i;

            questionPanels[i].SetActive(false);

            Button[] answerButtons = GetAnswerButtons(questionPanels[i]);

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

    void ShowQuestion(int questionIndex)
    {
        for (int i = 0; i < questionPanels.Length; i++)
        {
            questionPanels[i].SetActive(i == questionIndex);
        }
    }

    void CheckAnswer(int questionIndex, int answerIndex)
    {
        Button[] answerButtons =
            GetAnswerButtons(questionPanels[questionIndex]);

        if (answerIndex == correctAnswers[questionIndex])
        {
            Button correctButton = answerButtons[answerIndex];

            correctButton.image.color = correctColor;

            // Disable all answers after getting it correct
            foreach (Button button in answerButtons)
            {
                if (button != null)
                {
                    button.interactable = false;
                }
            }

            // Show Next button
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

    void CompleteQuiz()
    {
        if (completePanel != null)
        {
            completePanel.SetActive(true);
        }

        Debug.Log("QUIZ COMPLETE");
    }

    Button[] GetAnswerButtons(GameObject panel)
    {
        Button[] buttons = new Button[4];

        buttons[0] = FindButton(panel, "AnswerAButton");
        buttons[1] = FindButton(panel, "AnswerBButton");
        buttons[2] = FindButton(panel, "AnswerCButton");
        buttons[3] = FindButton(panel, "AnswerDButton");

        return buttons;
    }

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

    public void CloseWrongPanel()
    {
        wrongPanel.SetActive(false);
    }
}