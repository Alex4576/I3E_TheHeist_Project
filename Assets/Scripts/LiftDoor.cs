using UnityEngine;

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
    private bool isLocked = true;
    public bool showLockedMessage = false;

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
    }

    public void OpenDoor()
    {
        if (isLocked)
        {
            showLockedMessage = true;
        }
    }
}