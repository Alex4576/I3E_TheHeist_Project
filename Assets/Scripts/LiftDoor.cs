using UnityEngine;
using System.Collections;

public class LiftDoor : MonoBehaviour
{
    public Transform leftDoor;
    public Transform rightDoor;

    public float openSpeed = 2f;

    private Vector3 leftClosedPosition;
    private Vector3 rightClosedPosition;

    private Vector3 leftOpenPosition;
    private Vector3 rightOpenPosition;

    private bool isOpen = false;
    private bool isClosing = false;

    private bool isLocked = true;
    public bool showLockedMessage = false;

    [Header("Intro Lift")]
    public bool isIntroLift = false;
    public float introOpenDelay = 1f;

    private bool introClosed = false;

    void Start()
    {
        leftClosedPosition = leftDoor.localPosition;
        rightClosedPosition = rightDoor.localPosition;

        leftOpenPosition = new Vector3(
            leftClosedPosition.x,
            leftClosedPosition.y,
            -50f
        );

        rightOpenPosition = new Vector3(
            rightClosedPosition.x,
            rightClosedPosition.y,
            50f
        );

        // Only automatically open if this is the intro lift
        if (isIntroLift)
        {
            StartCoroutine(OpenIntroLift());
        }
    }

    void Update()
    {
        if (isOpen)
        {
            leftDoor.localPosition = Vector3.MoveTowards(
                leftDoor.localPosition,
                leftOpenPosition,
                openSpeed * Time.deltaTime
            );

            rightDoor.localPosition = Vector3.MoveTowards(
                rightDoor.localPosition,
                rightOpenPosition,
                openSpeed * Time.deltaTime
            );
        }

        if (isClosing)
        {
            leftDoor.localPosition = Vector3.MoveTowards(
                leftDoor.localPosition,
                leftClosedPosition,
                openSpeed * Time.deltaTime
            );

            rightDoor.localPosition = Vector3.MoveTowards(
                rightDoor.localPosition,
                rightClosedPosition,
                openSpeed * Time.deltaTime
            );
        }
    }

    IEnumerator OpenIntroLift()
    {
        yield return new WaitForSeconds(introOpenDelay);

        isOpen = true;
    }

    public void CloseIntroLift()
    {
        if (!isIntroLift)
            return;

        isOpen = false;
        isClosing = true;
    }

    public void OpenDoor()
    {
        if (isLocked)
        {
            showLockedMessage = true;
        }
        else
        {
            isOpen = true;
        }
    }

    public void UnlockLift()
    {
        isLocked = false;
        showLockedMessage = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isIntroLift || introClosed)
            return;

        if (other.CompareTag("Player"))
        {
            introClosed = true;
            CloseIntroLift();
        }
    }
}