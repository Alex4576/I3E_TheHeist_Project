using UnityEngine;

/// <summary>
/// Controls a museum door that opens when the player enters the trigger zone
/// and closes when the player exits. Plays a single sound effect for both actions.
/// </summary>
public class MuseumDoorTrigger : MonoBehaviour
{
    /// <summary>
    /// Animator controlling the door animations.
    /// </summary>
    public Animator anim;

    /// <summary>
    /// Sound clip played when the door opens or closes.
    /// Assigned in the Inspector.
    /// </summary>
    [SerializeField] private AudioClip doorClip;

    /// <summary>
    /// AudioSource component attached to the door.
    /// Used to play the sound effect.
    /// </summary>
    private AudioSource audioSource;

    /// <summary>
    /// Tracks whether the door is currently open.
    /// Prevents duplicate sound playback.
    /// </summary>
    private bool isOpen = false;

    /// <summary>
    /// Initialize references to Animator and AudioSource.
    /// </summary>
    void Start()
    {
        audioSource = GetComponent<AudioSource>(); // Door must have an AudioSource
    }

    /// <summary>
    /// Called when the player enters the trigger zone.
    /// Opens the door and plays the sound once.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            anim.SetBool("Near", true);            // Trigger door open animation
            audioSource.PlayOneShot(doorClip);     // Play sound
            isOpen = true;                         // Mark door as open
        }
    }

    /// <summary>
    /// Called when the player exits the trigger zone.
    /// Closes the door and plays the sound once.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isOpen)
        {
            anim.SetBool("Near", false);           // Trigger door close animation
            audioSource.PlayOneShot(doorClip);     // Play sound
            isOpen = false;                        // Mark door as closed
        }
    }
}
