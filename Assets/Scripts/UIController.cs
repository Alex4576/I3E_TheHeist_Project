/*
* Author: Sheryn Batrisyia
* Date: 12/08/2026
* Description: Controls the interaction UI displayed to the player.
* The script uses raycasting to detect interactable objects such as
* the lift button and quiz kiosk, and displays the appropriate action
* and command prompts. It also manages the interactive and
* non-interactive crosshair states.
*/

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the player's interaction UI, including action prompts,
/// command prompts, crosshair states, and raycast detection for
/// interactable objects.
/// </summary>
public class UIController : MonoBehaviour
{
    /// <summary>
    /// Text displayed in the action box to identify the
    /// object or interaction currently targeted.
    /// </summary>
    public static string actionText;

    /// <summary>
    /// Text displayed in the command box to tell the player
    /// which input or action can be performed.
    /// </summary>
    public static string commandText;

    /// <summary>
    /// Determines whether the interaction prompt is currently active.
    /// </summary>
    public static bool uiActive;

    /// <summary>
    /// Determines whether player interaction UI is currently enabled.
    /// </summary>
    public static bool interactionEnabled = true;


    [SerializeField] GameObject actionBox;
    [SerializeField] GameObject commandBox;
    [SerializeField] GameObject interactCross;
    [SerializeField] GameObject nonInteractCross;

    [SerializeField] bool useOwnRaycast = true;
    [SerializeField] float interactDistance = 5f;

    private Camera mainCamera;


    /// <summary>
    /// Finds the main camera and initializes the interaction UI
    /// to its default state when the scene begins.
    /// </summary>
    void Start()
    {
        mainCamera = Camera.main;

        interactionEnabled = true;
        uiActive = false;

        nonInteractCross.SetActive(true);
        interactCross.SetActive(false);
        actionBox.SetActive(false);
        commandBox.SetActive(false);
    }


    /// <summary>
    /// Checks for interactable objects when raycasting is enabled
    /// and updates the visibility and text of the interaction UI.
    /// </summary>
    void Update()
    {
        if (interactionEnabled && useOwnRaycast)
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


    /// <summary>
    /// Casts a ray from the player's camera to detect supported
    /// interactable objects, including the lift and quiz kiosk.
    /// Displays the appropriate prompt and handles the E key input.
    /// </summary>
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
            // Check whether the player is looking at the lift.
            LiftDoor liftDoor =
                hit.collider.GetComponentInParent<LiftDoor>();

            if (liftDoor != null)
            {
                uiActive = true;

                // Display the locked message if the quiz
                // has not yet unlocked the lift.
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

            // Check whether the player is looking at the quiz kiosk.
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


    /// <summary>
    /// Disables the interaction system and hides all interaction
    /// UI elements, including both crosshairs.
    /// </summary>
    public void HideInteractionUI()
    {
        interactionEnabled = false;
        uiActive = false;

        actionBox.SetActive(false);
        commandBox.SetActive(false);
        interactCross.SetActive(false);
        nonInteractCross.SetActive(false);
    }


    /// <summary>
    /// Re-enables the interaction system and restores the
    /// non-interactive crosshair to its default state.
    /// </summary>
    public void ShowInteractionUI()
    {
        interactionEnabled = true;
        uiActive = false;

        nonInteractCross.SetActive(true);
        interactCross.SetActive(false);
        actionBox.SetActive(false);
        commandBox.SetActive(false);
    }


    /// <summary>
    /// Displays an interaction prompt using the supplied action
    /// and command text and switches to the interactive crosshair.
    /// </summary>
    /// <param name="action">
    /// Name or description of the targeted interaction.
    /// </param>
    /// <param name="command">
    /// Command or key input displayed to the player.
    /// </param>
    public void ShowPrompt(string action, string command)
    {
        actionText = action;
        commandText = command;
        uiActive = true;

        nonInteractCross.SetActive(false);

        actionBox.SetActive(true);
        commandBox.SetActive(true);
        interactCross.SetActive(true);

        actionBox.GetComponent<TMPro.TMP_Text>().text = actionText;
        commandBox.GetComponent<TMPro.TMP_Text>().text = commandText;
    }


    /// <summary>
    /// Clears the current interaction prompt and restores the
    /// non-interactive crosshair.
    /// </summary>
    public void ClearPrompt()
    {
        uiActive = false;

        actionBox.SetActive(false);
        commandBox.SetActive(false);
        interactCross.SetActive(false);

        nonInteractCross.SetActive(true);
    }
}