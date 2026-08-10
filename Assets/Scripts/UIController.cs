using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    public static string actionText;
    public static string commandText;
    public static bool uiActive;
    public static bool interactionEnabled = true;

    [SerializeField] GameObject actionBox;
    [SerializeField] GameObject commandBox;
    [SerializeField] GameObject interactCross;
    [SerializeField] GameObject nonInteractCross;

    [SerializeField] float interactDistance = 5f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (interactionEnabled)
        {
            CheckInteraction();
        }

        if (uiActive == true)
        {
            actionBox.SetActive(true);
            commandBox.SetActive(true);
            interactCross.SetActive(true);

            actionBox.GetComponent<TMPro.TMP_Text>().text = actionText;
            commandBox.GetComponent<TMPro.TMP_Text>().text = commandText;
        }
        else
        {
            actionBox.SetActive(false);
            commandBox.SetActive(false);
            interactCross.SetActive(false);
        }
    }

    void CheckInteraction()
    {
        uiActive = false;

        Ray ray = new Ray(
            mainCamera.transform.position,
            mainCamera.transform.forward
        );

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            LiftDoor liftDoor =
                hit.collider.GetComponentInParent<LiftDoor>();

            if (liftDoor != null)
            {
                uiActive = true;

                if (liftDoor.showLockedMessage)
                {
                    actionText = "Lift Locked";
                    commandText = "Complete the Crime Prevention Quiz to unlock Level 2";
                }
                else
                {
                    actionText = "Lift Button";
                    commandText = "[E] Interact";

                    if (Keyboard.current.eKey.wasPressedThisFrame)
                    {
                        liftDoor.OpenDoor();
                    }
                }
            }

            QuizKiosk quizKiosk =
                hit.collider.GetComponentInParent<QuizKiosk>();

            if (quizKiosk != null)
            {
                actionText = "Crime Prevention Quiz";
                commandText = "[E] Interact";
                uiActive = true;

                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    quizKiosk.StartQuiz();
                }
            }
        }
    }

    public void HideInteractionUI()
    {
        interactionEnabled = false;
        uiActive = false;

        actionBox.SetActive(false);
        commandBox.SetActive(false);
        interactCross.SetActive(false);
        nonInteractCross.SetActive(false);
    }

    public void ShowInteractionUI()
    {
        interactionEnabled = true;
        uiActive = false;

        nonInteractCross.SetActive(true);
        interactCross.SetActive(false);
        actionBox.SetActive(false);
        commandBox.SetActive(false);
    }
}
