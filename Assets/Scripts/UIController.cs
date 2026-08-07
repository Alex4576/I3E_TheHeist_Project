using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    public static string actionText;
    public static string commandText;
    public static bool uiActive;

    [SerializeField] GameObject actionBox;
    [SerializeField] GameObject commandBox;
    [SerializeField] GameObject interactCross;

    [SerializeField] float interactDistance = 5f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        CheckInteraction();

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
        }
    }
}
