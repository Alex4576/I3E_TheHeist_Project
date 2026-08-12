/*
* Author: Sheryn Batrisyia
* Date: 12/08/2026
* Description: Controls the opening and closing behaviour of the lift doors.
* The script supports both the quiz room lift, which is unlocked
* through player interaction, and the gallery intro lift, which
* opens automatically when the scene begins.
*/

using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the movement, interaction, locking, audio, and
/// intro sequence behaviour of the lift doors.
/// </summary>
public class LiftDoor : MonoBehaviour
{
    /// <summary>
    /// Transform of the left lift door.
    /// </summary>
    public Transform leftDoor;

    /// <summary>
    /// Transform of the right lift door.
    /// </summary>
    public Transform rightDoor;


    [Header("Audio")]

    /// <summary>
    /// Audio source played when the player presses the lift button.
    /// </summary>
    public AudioSource buttonAudio;

    /// <summary>
    /// Audio source played when the lift doors begin opening.
    /// </summary>
    public AudioSource doorOpenAudio;


    /// <summary>
    /// Speed at which the lift doors open and close.
    /// </summary>
    public float openSpeed = 2f;


    // Stores the original closed positions of both lift doors.
    private Vector3 leftClosedPosition;
    private Vector3 rightClosedPosition;

    // Stores the target open positions of both lift doors.
    private Vector3 leftOpenPosition;
    private Vector3 rightOpenPosition;

    // Tracks the current movement state of the lift doors.
    private bool isOpen = false;
    private bool isClosing = false;

    // Determines whether the lift can currently be opened.
    private bool isLocked = true;

    /// <summary>
    /// Indicates whether the locked lift message should be shown.
    /// </summary>
    public bool showLockedMessage = false;


    [Header("Intro Lift")]

    /// <summary>
    /// Determines whether this lift should behave as the
    /// automatic intro lift in the gallery scene.
    /// </summary>
    public bool isIntroLift = false;

    /// <summary>
    /// Delay before the intro lift doors automatically open.
    /// </summary>
    public float introOpenDelay = 1f;

    // Prevents the intro lift from closing more than once.
    private bool introClosed = false;


    /// <summary>
    /// Stores the initial door positions, calculates their open
    /// positions, and starts the automatic intro lift sequence
    /// when required.
    /// </summary>
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

        // Only automatically open if this is the intro lift.
        if (isIntroLift)
        {
            StartCoroutine(OpenIntroLift());
        }
    }


    /// <summary>
    /// Moves the lift doors towards their open or closed positions
    /// depending on the current door state.
    /// </summary>
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

        // Stop the closing movement once both doors reach
        // their original closed positions.
        if (leftDoor.localPosition == leftClosedPosition &&
            rightDoor.localPosition == rightClosedPosition)
        {
            isClosing = false;
        }
    }


    /// <summary>
    /// Waits for the specified intro delay before automatically
    /// opening the gallery intro lift.
    /// </summary>
    IEnumerator OpenIntroLift()
    {
        yield return new WaitForSeconds(introOpenDelay);

        if (doorOpenAudio != null)
        {
            doorOpenAudio.Play();
        }

        isOpen = true;
    }


    /// <summary>
    /// Closes the intro lift after the player has exited.
    /// This method only affects lifts marked as intro lifts.
    /// </summary>
    public void CloseIntroLift()
    {
        if (!isIntroLift)
            return;

        isOpen = false;
        isClosing = true;
    }


    /// <summary>
    /// Attempts to open the lift doors.
    /// If the lift is locked, the locked message is activated.
    /// Otherwise, the button and door audio are played before
    /// opening the doors.
    /// </summary>
    public void OpenDoor()
    {
        if (isLocked)
        {
            showLockedMessage = true;
        }
        else
        {
            if (buttonAudio != null)
            {
                buttonAudio.Play();
            }

            if (doorOpenAudio != null)
            {
                doorOpenAudio.Play();
            }

            isOpen = true;
        }
    }


    /// <summary>
    /// Unlocks the lift and removes the locked message,
    /// allowing the player to open the lift.
    /// </summary>
    public void UnlockLift()
    {
        isLocked = false;
        showLockedMessage = false;
    }


    /// <summary>
    /// Detects when the player enters the intro lift trigger.
    /// Once triggered, the lift doors close and cannot be
    /// triggered again.
    /// </summary>
    /// <param name="other">
    /// Collider of the GameObject that entered the trigger.
    /// </param>
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